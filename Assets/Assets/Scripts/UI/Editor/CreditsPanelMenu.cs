using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectNTH.UI.Editor
{
    public static class CreditsPanelMenu
    {
        public const string PrefabPath = "Assets/Assets/Prefabs/Credits Panel.prefab";
        public const string DataPath = "Assets/Assets/UI/Sketchfab Credits.asset";
        private static readonly Color Ink = new Color(0.88f, 0.92f, 0.93f, 1f);

        [MenuItem("Tools/Project NTH/Create Credits Panel")]
        private static void Create()
        {
            foreach (var existing in Object.FindObjectsOfType<CreditsPanelUI>(true))
                if (existing.gameObject.scene == SceneManager.GetActiveScene())
                {
                    Selection.activeGameObject = existing.gameObject;
                    EditorGUIUtility.PingObject(existing);
                    return;
                }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) { Debug.LogError("Credits Panel prefab could not be found."); return; }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            Undo.RegisterCreatedObjectUndo(instance, "Create credits panel");
            Selection.activeGameObject = instance;
        }

        [MenuItem("Tools/Project NTH/Create Credits Panel", true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

        // Used to author the supplied prefab; does not run on import or overwrite customized prefabs.
        public static GameObject BuildPrefabContents(CreditsList data, TMP_FontAsset font)
        {
            var root = Rect("Credits Panel", null);
            var canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.gameObject.AddComponent<GraphicRaycaster>();
            var controller = root.gameObject.AddComponent<CreditsPanelUI>();
            var group = root.gameObject.AddComponent<CanvasGroup>();
            // An invisible full-screen raycast target stops clicks reaching the menu behind the panel.
            var blocker = Rect("Input Blocker", root);
            Fill(blocker);
            blocker.gameObject.AddComponent<Image>().color = Color.clear;
            var safe = Rect("Safe Area", root);
            Fill(safe);
            var bounds = Rect("Frame", safe);
            bounds.anchorMin = new Vector2(0.1f, 0.1f);
            bounds.anchorMax = new Vector2(0.9f, 0.89f);
            bounds.offsetMin = bounds.offsetMax = Vector2.zero;
            var panel = Rect("Panel", bounds);
            panel.anchorMin = new Vector2(0.1f, 0f);
            panel.anchorMax = Vector2.one;
            panel.offsetMin = panel.offsetMax = Vector2.zero;
            panel.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 0.88f);

            var back = Text("Back", bounds, "Back", font, 72f);
            Top(back.rectTransform, 0f, 0f, 152f, 80f);
            back.color = Color.white;
            back.raycastTarget = true;
            var button = back.gameObject.AddComponent<Button>();
            button.targetGraphic = back;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 0.07f, 0.05f, 1f);
            colors.highlightedColor = new Color(1f, 0.25f, 0.22f);
            colors.selectedColor = new Color(1f, 0.13f, 0.1f);
            colors.pressedColor = new Color(0.65f, 0.04f, 0.03f, 1f);
            button.colors = colors;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };

            var title = Text("Title", panel, "Credits", font, 104f);
            Top(title.rectTransform, 64f, 28f, 700f, 104f);
            var headings = Rect("Column Headings", panel);
            headings.anchorMin = new Vector2(0f, 1f);
            headings.anchorMax = Vector2.one;
            headings.pivot = new Vector2(0f, 1f);
            headings.offsetMin = new Vector2(64f, -242f);
            headings.offsetMax = new Vector2(-58f, -164f);
            var model = Text("Model Heading", headings, "Model name", font, 72f);
            var creator = Text("Creator Heading", headings, "Creator", font, 72f);
            model.enableWordWrapping = creator.enableWordWrapping = false;
            Fill(model.rectTransform);
            Fill(creator.rectTransform);
            model.rectTransform.anchorMax = new Vector2(0.63f, 1f);
            creator.rectTransform.anchorMin = new Vector2(0.67f, 0f);

            var scrollRect = Rect("Scroll View", panel);
            Fill(scrollRect);
            scrollRect.offsetMin = new Vector2(64f, 28f);
            scrollRect.offsetMax = new Vector2(-28f, -246f);
            var scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 48f;
            var viewport = Rect("Viewport", scrollRect);
            Fill(viewport);
            viewport.offsetMax = new Vector2(-30f, 0f);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0f, 1f);
            content.sizeDelta = Vector2.zero;
            scroll.viewport = viewport;
            scroll.content = content;
            var track = Rect("Scrollbar", scrollRect);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = Vector2.one;
            track.pivot = new Vector2(1f, 1f);
            track.sizeDelta = new Vector2(18f, 0f);
            track.anchoredPosition = Vector2.zero;
            track.gameObject.AddComponent<Image>().color = new Color(Ink.r, Ink.g, Ink.b, 0.05f);
            var sliding = Rect("Sliding Area", track);
            Fill(sliding);
            sliding.offsetMin = new Vector2(6f, 0f);
            sliding.offsetMax = new Vector2(-6f, 0f);
            var handle = Rect("Handle", sliding);
            Fill(handle);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(Ink.r, Ink.g, Ink.b, 0.52f);
            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle;
            bar.targetGraphic = handleImage;
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            var template = Rect("Row Template", root);
            template.anchorMin = new Vector2(0f, 1f);
            template.anchorMax = Vector2.one;
            template.pivot = new Vector2(0f, 1f);
            template.sizeDelta = new Vector2(0f, 48f);
            Fill(Text("Model", template, "Model name", font, 52f).rectTransform);
            Fill(Text("Creator", template, "Creator name", font, 52f).rectTransform);
            template.gameObject.SetActive(false);
            var empty = Text("Empty Message", viewport, "No credits added yet.", font, 52f);
            Fill(empty.rectTransform);
            empty.gameObject.SetActive(false);

            var settings = new SerializedObject(controller);
            settings.FindProperty("credits").objectReferenceValue = data;
            settings.FindProperty("view").objectReferenceValue = group;
            settings.FindProperty("safeArea").objectReferenceValue = safe;
            settings.FindProperty("scroll").objectReferenceValue = scroll;
            settings.FindProperty("rowTemplate").objectReferenceValue = template;
            settings.FindProperty("modelHeading").objectReferenceValue = model.rectTransform;
            settings.FindProperty("creatorHeading").objectReferenceValue = creator.rectTransform;
            settings.FindProperty("emptyMessage").objectReferenceValue = empty;
            settings.FindProperty("backButton").objectReferenceValue = button;
            settings.ApplyModifiedPropertiesWithoutUndo();
            return root.gameObject;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = 5;
            if (parent != null) rect.SetParent(parent, false);
            return rect;
        }
        private static TMP_Text Text(string name, Transform parent, string value, TMP_FontAsset font, float size)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.text = value;
            text.color = Ink;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.richText = false;
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.margin = Vector4.zero;
            return text;
        }
        private static void Fill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Top(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
