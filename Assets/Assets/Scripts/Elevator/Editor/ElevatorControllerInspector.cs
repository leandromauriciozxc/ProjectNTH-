using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectNTH.Elevators.Editor
{
    internal static class ElevatorInspectorFields
    {
        internal static void FloorPopup(SerializedProperty property, SerializedProperty floors, string label)
        {
            int count = floors == null ? 0 : floors.arraySize;
            if (count == 0)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
                EditorGUILayout.HelpBox("Add real floors to the controller first.", MessageType.Info);
                return;
            }

            bool invalid = property.intValue < 0 || property.intValue >= count;
            var options = new string[count + (invalid ? 1 : 0)];
            for (int i = 0; i < count; i++)
            {
                string name = floors.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue;
                options[i] = (string.IsNullOrWhiteSpace(name) ? (i + 1).ToString() : name) + " (index " + i + ")";
            }
            if (invalid) options[count] = "Missing floor (index " + property.intValue + ")";
            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup(label, invalid ? count : property.intValue, options);
            if (EditorGUI.EndChangeCheck() && selected < count) property.intValue = selected;
        }

        internal static bool IsDoorSymbol(string value)
        {
            return value != null && (value.Trim() == ">|<" || value.Trim() == "<|>");
        }
    }

    [CustomEditor(typeof(ElevatorController)), CanEditMultipleObjects]
    public sealed class ElevatorControllerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                EditorGUILayout.HelpBox("Select one elevator to choose floors by name or repair its floor list.", MessageType.Info);
                return;
            }
            serializedObject.Update();
            SerializedProperty floors = serializedObject.FindProperty("floors");
            SerializedProperty property = serializedObject.GetIterator();
            bool enter = true;
            while (property.NextVisible(enter))
            {
                enter = false;
                if (property.name == "startingFloor")
                    ElevatorInspectorFields.FloorPopup(property, floors, "Starting Floor");
                else if (property.name == "startOpen")
                    EditorGUILayout.PropertyField(property, new GUIContent("Open On Start", property.tooltip));
                else
                {
                    using (new EditorGUI.DisabledScope(property.name == "m_Script"))
                        EditorGUILayout.PropertyField(property, true);
                }
            }
            serializedObject.ApplyModifiedProperties();

            bool hasDoorEntries = false;
            for (int i = 0; i < floors.arraySize; i++)
                hasDoorEntries |= ElevatorInspectorFields.IsDoorSymbol(floors.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue);
            if (!hasDoorEntries) return;
            EditorGUILayout.HelpBox("The floor list contains door-control symbols. They count as travel stops. Remove them and remap button indices with the repair below.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Remove Door Entries and Remap Buttons")) RepairFloorList((ElevatorController)target);
            }
        }

        private static void RepairFloorList(ElevatorController controller)
        {
            if (!controller.gameObject.scene.IsValid() || EditorUtility.IsPersistent(controller))
            {
                Debug.LogWarning("Run this repair on the elevator in the scene, not a prefab asset.", controller);
                return;
            }
            var settings = new SerializedObject(controller);
            SerializedProperty floors = settings.FindProperty("floors");
            var removed = new List<int>();
            var map = new int[floors.arraySize];
            int next = 0;
            for (int i = 0; i < floors.arraySize; i++)
            {
                SerializedProperty floor = floors.GetArrayElementAtIndex(i);
                if (ElevatorInspectorFields.IsDoorSymbol(floor.FindPropertyRelative("label").stringValue))
                {
                    if (floor.FindPropertyRelative("onArrival.m_PersistentCalls.m_Calls").arraySize != 0)
                    {
                        Debug.LogWarning("A door-symbol entry has arrival events. Move those events to the intended real floor before repairing.", controller);
                        return;
                    }
                    removed.Add(i);
                    map[i] = -1;
                }
                else map[i] = next++;
            }
            if (removed.Count == 0) return;
            int start = settings.FindProperty("startingFloor").intValue;
            if (next == 0 || start < 0 || start >= map.Length || map[start] < 0)
            {
                Debug.LogWarning("Choose a real Starting Floor before repairing the floor list.", controller);
                return;
            }

            var buttons = new List<ElevatorFloorButton>();
            foreach (var button in Resources.FindObjectsOfTypeAll<ElevatorFloorButton>())
            {
                if (button.Elevator != controller || !button.gameObject.scene.IsValid()) continue;
                if (button.Action == ElevatorFloorButton.ButtonAction.OpenDoors || button.Action == ElevatorFloorButton.ButtonAction.CloseDoors) continue;
                if (button.FloorIndex < 0 || button.FloorIndex >= map.Length || map[button.FloorIndex] < 0)
                {
                    Debug.LogWarning("A floor/call button points to a missing floor or a door-symbol entry. Assign it a real floor first: " + button.name, button);
                    return;
                }
                buttons.Add(button);
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Repair elevator floor list");
            try
            {
                Undo.RecordObject(controller, "Remove door entries");
                for (int i = removed.Count - 1; i >= 0; i--) floors.DeleteArrayElementAtIndex(removed[i]);
                settings.FindProperty("startingFloor").intValue = map[start];
                settings.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                foreach (var button in buttons)
                {
                    var buttonSettings = new SerializedObject(button);
                    Undo.RecordObject(button, "Remap elevator floor");
                    buttonSettings.FindProperty("floorIndex").intValue = map[button.FloorIndex];
                    buttonSettings.ApplyModifiedProperties();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(button);
                    EditorSceneManager.MarkSceneDirty(button.gameObject.scene);
                }
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                Undo.CollapseUndoOperations(group);
                Debug.Log("Removed door symbols from the floor list and remapped floor/call buttons and Starting Floor. Door controls and panel layout were preserved. Save the scene when ready.", controller);
            }
            catch (Exception exception)
            {
                Undo.RevertAllDownToGroup(group);
                Debug.LogException(exception);
            }
        }
    }

}
