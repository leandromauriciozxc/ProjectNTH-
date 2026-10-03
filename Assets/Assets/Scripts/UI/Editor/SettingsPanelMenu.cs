using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectNTH.UI.Editor
{
    public static class SettingsPanelMenu
    {
        public const string PrefabPath = "Assets/Assets/Prefabs/Settings Panel.prefab";
        private static readonly Color Ink = new Color(0.88f, 0.92f, 0.93f);
        private static readonly Color Surface = new Color(0.16f, 0.16f, 0.16f, 0.96f);

        [MenuItem("Tools/Project NTH/Create Settings Panel")]
        private static void Create()
        {
            foreach (var existing in Object.FindObjectsOfType<SettingsPanelUI>(true))
                if (existing.gameObject.scene == SceneManager.GetActiveScene())
                { Selection.activeGameObject = existing.gameObject; return; }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) { Debug.LogError("Settings Panel prefab is missing."); return; }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            Undo.RegisterCreatedObjectUndo(instance, "Create settings panel");
            Selection.activeGameObject = instance;
        }

        [MenuItem("Tools/Project NTH/Create Settings Panel", true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

        // Authoring only: the shipped prefab remains editable and is never rebuilt on import.
        public static GameObject BuildPrefabContents(TMP_FontAsset font, SO_SensivitySettings defaults)
        {
            var root = Rect("Settings Panel", null);
            var canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
            var scaler = root.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root.gameObject.AddComponent<GraphicRaycaster>();
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = group.blocksRaycasts = false;
            var ui = root.gameObject.AddComponent<SettingsPanelUI>();
            Image("Input Blocker", root, new Color(0, 0, 0, 0.3f));
            var safe = Rect("Safe Area", root); Fill(safe);
            var panel = Image("Panel", safe, Surface).rectTransform;
            panel.anchorMin = new Vector2(0.12f, 0.09f);
            panel.anchorMax = new Vector2(0.9f, 0.91f);
            var title = Text("Title", panel, "Settings", font, 96);
            Top(title.rectTransform, 64, 24, 600, 110);
            var back = Button("Back", safe, "Back", font, true);
            var br = (RectTransform)back.transform;
            br.anchorMin = br.anchorMax = new Vector2(0.02f, 0.91f);
            br.pivot = new Vector2(0, 1); br.sizeDelta = new Vector2(180, 80);
            var tabRow = Rect("Tabs", panel); Band(tabRow, 150, 72);
            tabRow.offsetMin = new Vector2(64, tabRow.offsetMin.y);
            tabRow.offsetMax = new Vector2(-64, tabRow.offsetMax.y);
            var tabs = new Button[3];
            var pages = new GameObject[3];
            string[] names = { "Audio", "Display", "Controls" };
            for (int i = 0; i < 3; i++)
            {
                tabs[i] = Button(names[i], tabRow, names[i], font);
                var tr = (RectTransform)tabs[i].transform; Fill(tr);
                tr.anchorMin = new Vector2(i / 3f, 0); tr.anchorMax = new Vector2((i + 1) / 3f, 1);
                tr.offsetMin = new Vector2(0, 0); tr.offsetMax = new Vector2(-16, 0);
                var line = Image("Active", tr, Ink).rectTransform;
                line.anchorMax = new Vector2(1, 0); line.sizeDelta = new Vector2(0, 3);
                line.gameObject.SetActive(i == 0);
                var page = Rect(names[i] + " Page", panel); Fill(page);
                page.offsetMin = new Vector2(64, 72); page.offsetMax = new Vector2(-64, -268);
                page.gameObject.AddComponent<CanvasGroup>();
                pages[i] = page.gameObject;
                page.gameObject.SetActive(i == 0);
            }
            var sliders = new Slider[4]; var values = new TMP_Text[4];
            string[] volumes = { "Master", "Music", "Sound effects", "Dialogue" };
            for (int i = 0; i < 4; i++)
                sliders[i] = SliderRow(pages[0].transform, volumes[i], i, font, out values[i]);
            var audioNote = Text("Audio Help", pages[0].transform, "Sound effects includes footsteps, doors and ambience.", font, 36);
            Band(audioNote.rectTransform, 432, 90);
            var sensitivity = SliderRow(pages[2].transform, "Mouse sensitivity", 0, font, out var sensitivityText);
            var invertRow = Row(pages[2].transform, "Invert Y", 1, font);
            var invert = Button("Invert", invertRow, "Off", font);
            Control((RectTransform)invert.transform);
            var controlNote = Text("Controls Help", pages[2].transform, "Invert Y reverses vertical mouse movement.", font, 36);
            Band(controlNote.rectTransform, 228, 90);
            var prev = new Button[4]; var next = new Button[4]; var display = new TMP_Text[4];
            string[] displayNames = { "Graphics quality", "Screen mode", "Resolution", "Frame rate limit" };
            for (int i = 0; i < 4; i++)
            {
                var row = Row(pages[1].transform, displayNames[i], i, font);
                var box = Rect("Control", row); Control(box);
                prev[i] = Button("Previous", box, "<", font);
                var pr = (RectTransform)prev[i].transform; Fill(pr); pr.anchorMax = new Vector2(0, 1); pr.sizeDelta = new Vector2(64, 0);
                next[i] = Button("Next", box, ">", font);
                var nr = (RectTransform)next[i].transform; Fill(nr); nr.anchorMin = new Vector2(1, 0); nr.pivot = new Vector2(1, 0.5f); nr.sizeDelta = new Vector2(64, 0);
                display[i] = Text("Value", box, "", font, 44);
                Fill(display[i].rectTransform); display[i].rectTransform.offsetMin = new Vector2(70, 0); display[i].rectTransform.offsetMax = new Vector2(-70, 0);
                display[i].alignment = TextAlignmentOptions.Center;
                display[i].enableAutoSizing = true; display[i].fontSizeMin = 30; display[i].fontSizeMax = 44;
            }
            var apply = Button("Apply Display", pages[1].transform, "Apply display", font);
            Band((RectTransform)apply.transform, 440, 76);
            var confirm = Image("Display Confirmation", root, new Color(0.08f, 0.08f, 0.08f, 0.98f)).gameObject;
            var prompt = Text("Message", confirm.transform, "Keep these display settings?\nReverting in 15 seconds.", font, 56);
            Fill(prompt.rectTransform); prompt.rectTransform.anchorMin = new Vector2(0.15f, 0.48f); prompt.rectTransform.anchorMax = new Vector2(0.85f, 0.7f);
            prompt.alignment = TextAlignmentOptions.Center;
            var keep = Button("Keep", confirm.transform, "Keep changes", font);
            var revert = Button("Revert", confirm.transform, "Revert", font, true);
            Fill((RectTransform)keep.transform); ((RectTransform)keep.transform).anchorMin = new Vector2(0.2f, 0.3f); ((RectTransform)keep.transform).anchorMax = new Vector2(0.48f, 0.4f);
            Fill((RectTransform)revert.transform); ((RectTransform)revert.transform).anchorMin = new Vector2(0.52f, 0.3f); ((RectTransform)revert.transform).anchorMax = new Vector2(0.8f, 0.4f);
            confirm.SetActive(false);
            var so = new SerializedObject(ui);
            Set(so, "view", group); Set(so, "safeArea", safe); Set(so, "backButton", back); Set(so, "sensitivityDefaults", defaults);
            SetArray(so, "tabs", tabs); SetArray(so, "pages", pages); SetArray(so, "volumeSliders", sliders); SetArray(so, "volumeValues", values);
            Set(so, "sensitivitySlider", sensitivity); Set(so, "sensitivityValue", sensitivityText); Set(so, "invertButton", invert); Set(so, "invertValue", invert.GetComponentInChildren<TMP_Text>());
            SetArray(so, "previousButtons", prev); SetArray(so, "nextButtons", next); SetArray(so, "displayValues", display);
            Set(so, "applyDisplayButton", apply); Set(so, "displayConfirmation", confirm); Set(so, "confirmationMessage", prompt);
            Set(so, "keepDisplayButton", keep); Set(so, "revertDisplayButton", revert);
            so.ApplyModifiedPropertiesWithoutUndo();
            return root.gameObject;
        }
        private static void Set(SerializedObject so, string name, Object value) => so.FindProperty(name).objectReferenceValue = value;
        private static void SetArray<T>(SerializedObject so, string name, T[] values) where T : Object
        {
            var property = so.FindProperty(name); property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        private static RectTransform Row(Transform page, string label, int index, TMP_FontAsset font)
        {
            var row = Rect(label, page); Band(row, index * 102, 76);
            var text = Text("Label", row, label, font, 44); Fill(text.rectTransform);
            text.rectTransform.anchorMax = new Vector2(0.43f, 1);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            return row;
        }
        private static Slider SliderRow(Transform page, string label, int index, TMP_FontAsset font, out TMP_Text value)
        {
            var row = Row(page, label, index, font);
            var box = Rect("Slider", row); Control(box); box.offsetMax = new Vector2(-120, 0);
            var hit = Image("Hit Area", box, Color.clear);
            var track = Image("Track", box, new Color(0.38f, 0.4f, 0.41f)).rectTransform;
            track.anchorMin = new Vector2(0, 0.5f); track.anchorMax = new Vector2(1, 0.5f); track.sizeDelta = new Vector2(0, 6);
            var fill = Image("Fill", track, Ink).rectTransform;
            var handleArea = Rect("Handle Area", box); Fill(handleArea);
            handleArea.anchorMin = new Vector2(0, 0.5f); handleArea.anchorMax = new Vector2(1, 0.5f);
            handleArea.offsetMin = new Vector2(12, -22); handleArea.offsetMax = new Vector2(-12, 22);
            var handle = Image("Handle", handleArea, Ink);
            handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, 0.5f);
            handle.rectTransform.sizeDelta = new Vector2(22, 0);
            var slider = box.gameObject.AddComponent<Slider>();
            slider.fillRect = fill; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            slider.minValue = 0; slider.maxValue = 1; Style(slider);
            value = Text("Value", row, "100%", font, 42); Fill(value.rectTransform);
            value.rectTransform.anchorMin = new Vector2(1, 0); value.rectTransform.pivot = new Vector2(1, 0.5f); value.rectTransform.sizeDelta = new Vector2(110, 0);
            value.alignment = TextAlignmentOptions.MidlineRight;
            return slider;
        }
        private static void Control(RectTransform rect) { Fill(rect); rect.anchorMin = new Vector2(0.44f, 0); }
        private static Button Button(string name, Transform parent, string label, TMP_FontAsset font, bool red = false)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>(); image.color = red ? Color.clear : new Color(0.27f, 0.28f, 0.29f);
            var text = Text("Label", rect, label, font, 48); Fill(text.rectTransform); text.alignment = TextAlignmentOptions.Center;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = red ? (Graphic)text : image;
            if (red) text.color = Color.white;
            Style(button, red);
            return button;
        }
        private static void Style(Selectable target, bool red = false)
        {
            var colors = target.colors;
            colors.normalColor = red ? new Color(1, 0.12f, 0.09f) : Color.white;
            colors.highlightedColor = red ? new Color(1, 0.45f, 0.4f) : new Color(1.5f, 1.5f, 1.5f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = red ? new Color(0.7f, 0.1f, 0.07f) : new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f); colors.fadeDuration = 0.12f;
            target.colors = colors;
            target.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Automatic };
        }
        private static TMP_Text Text(string name, Transform parent, string value, TMP_FontAsset font, float size)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = size; text.text = value; text.color = Ink;
            text.richText = false; text.raycastTarget = false; text.enableWordWrapping = true;
            return text;
        }
        private static Image Image(string name, Transform parent, Color color)
        {
            var rect = Rect(name, parent); Fill(rect);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; return image;
        }
        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = 5; if (parent != null) rect.SetParent(parent, false); return rect;
        }
        private static void Fill(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        private static void Top(RectTransform r, float x, float y, float width, float height)
        { r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(width, height); }
        private static void Band(RectTransform r, float y, float height)
        { r.anchorMin = new Vector2(0, 1); r.anchorMax = Vector2.one; r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(0, -y); r.sizeDelta = new Vector2(0, height); }
    }
}
