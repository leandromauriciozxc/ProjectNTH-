using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectNTH.Elevators.Editor
{
    /// <summary>Runs only when explicitly selected from Tools; never edits scenes on import.</summary>
    public static class ElevatorSetupMenu
    {
        private const string MenuPath = "Tools/Project NTH/Set Up Selected Elevator";

        [MenuItem(MenuPath)]
        private static void SetUp()
        {
            GameObject selected = Selection.activeGameObject;
            ElevatorDoor legacy = selected == null ? null : selected.GetComponentInParent<ElevatorDoor>();
            if (EditorApplication.isPlayingOrWillChangePlaymode || legacy == null
                || EditorUtility.IsPersistent(legacy) || !legacy.gameObject.scene.IsValid())
            {
                Debug.LogWarning("In Edit Mode, select the scene's Elevator object with its existing ElevatorDoor component.");
                return;
            }
            if (legacy.GetComponent<ElevatorController>() != null)
            {
                Selection.activeGameObject = legacy.gameObject;
                Debug.Log("This elevator is already set up. Edit its existing controller and Elevator Controls children.", legacy);
                return;
            }

            var oldSettings = new SerializedObject(legacy);
            var leftObject = oldSettings.FindProperty("m_leftDoor").objectReferenceValue as GameObject;
            var rightObject = oldSettings.FindProperty("m_rightDoor").objectReferenceValue as GameObject;
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (leftObject == null || rightObject == null || leftObject == rightObject || font == null)
            {
                Debug.LogError("Assign two different doors on ElevatorDoor and a default TMP font before setup.", legacy);
                return;
            }

            int layer = LayerMask.NameToLayer("Interactable");
            if (layer < 0)
            {
                Debug.LogError("The scene needs an Interactable layer included in the player's interaction raycast.", legacy);
                return;
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set up elevator and world-space indicator");
            try
            {
                Transform root = legacy.transform;
                Transform left = leftObject.transform;
                Transform right = rightObject.transform;
                float distance = Mathf.Max(0.01f, oldSettings.FindProperty("m_openDistanceModifier").floatValue);
                float duration = Mathf.Max(0.01f, oldSettings.FindProperty("m_durationModifier").floatValue);
                var controller = Undo.AddComponent<ElevatorController>(root.gameObject);
                var settings = new SerializedObject(controller);
                SetReference(settings, "leftDoor", left);
                SetReference(settings, "rightDoor", right);
                settings.FindProperty("leftOpenOffset").vector3Value = ParentVector(left, -root.right * distance);
                settings.FindProperty("rightOpenOffset").vector3Value = ParentVector(right, root.right * distance);
                settings.FindProperty("doorDuration").floatValue = duration;
                settings.ApplyModifiedProperties();

                GameObject controls = Create("Elevator Controls", root);
                Vector3 center = (left.position + right.position) * 0.5f;
                CreateIndicator("Cabin Indicator", controls.transform, controller, font,
                    center + root.up * 1.53f - root.forward * 0.12f, root.rotation);
                CreateIndicator("Hall Indicator", controls.transform, controller, font,
                    center + root.up * 1.53f + root.forward * 0.19f,
                    root.rotation * Quaternion.Euler(0f, 180f, 0f));

                float jambDistance = Mathf.Abs(Vector3.Dot(right.position - left.position, root.right)) * 0.5f + 0.55f;
                Vector3 jamb = center + root.right * jambDistance + root.up * 0.3f;
                RectTransform inside = CreateCanvas("Cabin Buttons", controls.transform,
                    jamb - root.forward * 0.13f, root.rotation, new Vector2(420f, 330f));
                CreateButton(inside, controller, font, layer, 0, ElevatorFloorButton.ButtonAction.SelectFloor, new Vector2(0f, 110f));
                CreateButton(inside, controller, font, layer, 1, ElevatorFloorButton.ButtonAction.SelectFloor, Vector2.zero);
                CreateButton(inside, controller, font, layer, 0, ElevatorFloorButton.ButtonAction.OpenDoors, new Vector2(-110f, -110f));
                CreateButton(inside, controller, font, layer, 0, ElevatorFloorButton.ButtonAction.CloseDoors, new Vector2(110f, -110f));
                RectTransform outside = CreateCanvas("Hall Open Button", controls.transform,
                    jamb + root.forward * 0.19f, root.rotation * Quaternion.Euler(0f, 180f, 0f),
                    new Vector2(200f, 90f));
                CreateButton(outside, controller, font, layer, 0, ElevatorFloorButton.ButtonAction.OpenDoors, Vector2.zero);

                settings.Update();
                SetReference(settings, "effectsSource", CreateAudio("Door and Arrival Audio", controls.transform));
                SetReference(settings, "travelSource", CreateAudio("Travel Audio", controls.transform));
                settings.ApplyModifiedProperties();

                Undo.RecordObject(legacy, "Use the new elevator controller");
                legacy.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(legacy);
                EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
                Selection.activeGameObject = root.gameObject;
                Undo.CollapseUndoOperations(group);
                Debug.Log("Elevator setup complete: two example floors, inside/outside indicators and E buttons. "
                    + "Check panel placement, assign arrival events and optional audio, then save the scene. "
                    + "The previous ElevatorDoor component is disabled. One Undo restores the previous setup.", controller);
            }
            catch (Exception exception)
            {
                Undo.RevertAllDownToGroup(group);
                Debug.LogException(exception);
            }
        }

        private static Vector3 ParentVector(Transform door, Vector3 worldOffset)
        {
            return door.parent == null ? worldOffset : door.parent.InverseTransformVector(worldOffset);
        }

        private static GameObject Create(string name, Transform parent, params Type[] components)
        {
            var item = new GameObject(name, components);
            item.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(item, "Create elevator controls");
            return item;
        }

        private static RectTransform CreateCanvas(string name, Transform parent, Vector3 position,
            Quaternion rotation, Vector2 size)
        {
            GameObject item = Create(name, parent, typeof(RectTransform), typeof(Canvas));
            var rect = (RectTransform)item.transform;
            rect.position = position;
            rect.rotation = rotation;
            rect.localScale = Vector3.one * 0.001f;
            rect.sizeDelta = size;
            item.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            // Interaction uses the existing Physics raycast, so no UI raycaster/EventSystem is needed.
            return rect;
        }

        private static void CreateIndicator(string name, Transform parent, ElevatorController controller,
            TMP_FontAsset font, Vector3 position, Quaternion rotation)
        {
            RectTransform canvas = CreateCanvas(name, parent, position, rotation, new Vector2(560f, 230f));
            var background = canvas.gameObject.AddComponent<Image>();
            background.color = new Color(0.015f, 0.022f, 0.025f, 1f);
            background.raycastTarget = false;
            TMP_Text number = CreateText("Floor", canvas, font, new Vector2(-55f, 0f), new Vector2(400f, 200f), 130f, "1");
            TMP_Text arrow = CreateText("Direction", canvas, font, new Vector2(190f, 0f), new Vector2(145f, 140f), 38f, "");
            var indicator = canvas.gameObject.AddComponent<ElevatorFloorIndicator>();
            var settings = new SerializedObject(indicator);
            SetReference(settings, "elevator", controller);
            SetReference(settings, "floorText", number);
            SetReference(settings, "directionText", arrow);
            settings.ApplyModifiedProperties();
        }

        private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font,
            Vector2 position, Vector2 size, float fontSize, string text)
        {
            GameObject item = Create(name, parent, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)item.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, -1f);
            var label = item.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.richText = false;
            label.color = new Color(0.65f, 1f, 0.79f, 1f);
            label.text = text;
            return label;
        }

        private static void CreateButton(RectTransform parent, ElevatorController controller,
            TMP_FontAsset font, int layer, int index, ElevatorFloorButton.ButtonAction action, Vector2 position)
        {
            bool isFloor = action == ElevatorFloorButton.ButtonAction.SelectFloor;
            bool opens = action == ElevatorFloorButton.ButtonAction.OpenDoors;
            GameObject item = Create(isFloor ? "Floor " + (index + 1) : opens ? "Open Doors" : "Close Doors", parent,
                typeof(RectTransform), typeof(Image), typeof(BoxCollider), typeof(Interactable), typeof(ElevatorFloorButton));
            item.layer = layer;
            var rect = (RectTransform)item.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200f, 90f);
            var background = item.GetComponent<Image>();
            background.color = new Color(0.12f, 0.14f, 0.16f, 1f);
            background.raycastTarget = false;
            var collider = item.GetComponent<BoxCollider>();
            collider.size = new Vector3(200f, 90f, 20f);
            collider.center = new Vector3(0f, 0f, -10f);
            TMP_Text label = CreateText("Label", rect, font, Vector2.zero, rect.sizeDelta, 48f,
                isFloor ? controller.GetFloorLabel(index) : opens ? "OPEN" : "CLOSE");

            var button = item.GetComponent<ElevatorFloorButton>();
            var settings = new SerializedObject(button);
            SetReference(settings, "elevator", controller);
            SetReference(settings, "label", label);
            SetReference(settings, "background", background);
            settings.FindProperty("floorIndex").intValue = index;
            settings.FindProperty("action").enumValueIndex = (int)action;
            settings.ApplyModifiedProperties();

            var interaction = new SerializedObject(item.GetComponent<Interactable>());
            interaction.FindProperty("promptText").stringValue = isFloor ? "[E] Floor " + (index + 1)
                : opens ? "[E] Open doors" : "[E] Close doors";
            SerializedProperty calls = interaction.FindProperty("onInteract.m_PersistentCalls.m_Calls");
            calls.arraySize = 1;
            SerializedProperty call = calls.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = button;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = typeof(ElevatorFloorButton).AssemblyQualifiedName;
            call.FindPropertyRelative("m_MethodName").stringValue = nameof(ElevatorFloorButton.Press);
            call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void, no argument.
            call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // RuntimeOnly.
            interaction.ApplyModifiedProperties();
        }

        private static AudioSource CreateAudio(string name, Transform parent)
        {
            var source = Create(name, parent, typeof(AudioSource)).GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 1f;
            source.maxDistance = 12f;
            return source;
        }

        private static void SetReference(SerializedObject settings, string name, UnityEngine.Object value)
        {
            settings.FindProperty(name).objectReferenceValue = value;
        }
    }
}
