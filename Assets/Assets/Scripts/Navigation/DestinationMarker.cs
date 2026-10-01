using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectNTH.Navigation
{
    /// <summary>A world destination drawn on the HUD, including through walls.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10000)]
    [AddComponentMenu("Project NTH/Navigation/Destination Marker")]
    public sealed class DestinationMarker : MonoBehaviour
    {
        [Header("Destination")]
        [SerializeField] private string areaName = "Destination";
        [Tooltip("Show this objective now. Can also be controlled with Show, Hide or SetVisible from an event.")]
        [SerializeField] private bool markerVisible = true;
        [Tooltip("Marker height/offset in world metres, independent of the object's scale.")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Camera and distance")]
        [Tooltip("Optional. When empty, follows the active camera tagged MainCamera, including Cinemachine camera changes.")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private bool showDistance = true;
        [Tooltip("Optional player transform for measuring distance. Uses the viewing camera when empty.")]
        [SerializeField] private Transform distanceOrigin;
        [Tooltip("0 shows the marker at any distance. Walls never hide it.")]
        [Min(0f), SerializeField] private float maximumDistance;

        [Header("Appearance")]
        [Tooltip("Optional replacement for the default diamond icon.")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color markerColor = new Color(0.65f, 1f, 0.79f, 1f);
        [Min(12f), SerializeField] private float iconSize = 36f;
        [Tooltip("Overlay UI order. The default keeps the marker below ordinary HUDs and transition fades.")]
        [SerializeField] private int sortingOrder = -100;
        [Tooltip("Optional child containing your future world VFX. Show/Hide controls this child too. Its own material determines whether the VFX is visible through walls.")]
        [SerializeField] private GameObject vfxRoot;

        private GameObject overlay;
        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform markerRect;
        private DestinationMarkerGraphic diamond;
        private Image customIcon;
        private TMP_Text nameText;
        private TMP_Text distanceText;
        private int lastDistance = -1;

        public Vector3 AnchorPosition => transform.position + worldOffset;
        public bool IsMarkerVisible => markerRect != null && markerRect.gameObject.activeInHierarchy;

        public void Show() => SetVisible(true);
        public void Hide() => SetVisible(false);

        public void SetVisible(bool visible)
        {
            markerVisible = visible;
            if (isActiveAndEnabled) RefreshMarker();
        }

        public void SetAreaName(string value)
        {
            areaName = value ?? string.Empty;
        }

        private void OnEnable()
        {
            CreateUI();
            RefreshMarker();
        }

        private void LateUpdate() => RefreshMarker();

        private void RefreshMarker()
        {
            SetVfxVisible(markerVisible);
            if (canvas == null) return;

            Camera view = playerCamera != null ? playerCamera : Camera.main;
            if (!markerVisible || view == null || !view.isActiveAndEnabled || view.targetTexture != null)
            {
                SetHudVisible(false);
                return;
            }

            float distance = Vector3.Distance(distanceOrigin != null ? distanceOrigin.position : view.transform.position,
                transform.position);
            Vector3 screenPoint = view.WorldToScreenPoint(AnchorPosition);
            bool inView = screenPoint.z > view.nearClipPlane && view.pixelRect.Contains(new Vector2(screenPoint.x, screenPoint.y));
            // No raycast: the destination remains visible through walls. Off-screen points are hidden, never clamped to an edge.
            if (!inView || (maximumDistance > 0f && distance > maximumDistance))
            {
                SetHudVisible(false);
                return;
            }

            canvas.targetDisplay = view.targetDisplay;
            canvas.sortingOrder = Mathf.Clamp(sortingOrder, short.MinValue, short.MaxValue);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
            markerRect.anchoredPosition = localPoint;
            float size = Mathf.Max(12f, iconSize);
            diamond.rectTransform.sizeDelta = customIcon.rectTransform.sizeDelta = Vector2.one * size;
            diamond.color = customIcon.color = markerColor;
            diamond.gameObject.SetActive(icon == null);
            customIcon.gameObject.SetActive(icon != null);
            customIcon.sprite = icon;

            nameText.text = areaName;
            nameText.color = markerColor;
            bool hasName = !string.IsNullOrWhiteSpace(areaName);
            nameText.gameObject.SetActive(hasName);
            nameText.rectTransform.anchoredPosition = new Vector2(0f, -size * 0.5f - 17f);
            distanceText.rectTransform.anchoredPosition = new Vector2(0f, -size * 0.5f - (hasName ? 40f : 17f));
            distanceText.gameObject.SetActive(showDistance);
            int metres = Mathf.RoundToInt(distance);
            if (lastDistance != metres)
            {
                distanceText.SetText("{0} m", metres);
                lastDistance = metres;
            }
            SetHudVisible(true);
        }

        private void SetHudVisible(bool visible)
        {
            if (markerRect != null && markerRect.gameObject.activeSelf != visible)
                markerRect.gameObject.SetActive(visible);
        }

        private void SetVfxVisible(bool visible)
        {
            if (vfxRoot != null && vfxRoot != gameObject && vfxRoot.transform.IsChildOf(transform)
                && vfxRoot.activeSelf != visible) vfxRoot.SetActive(visible);
        }

        private void CreateUI()
        {
            if (overlay != null) return;
            overlay = new GameObject("Destination UI - " + name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            // A separate root avoids inheriting world-space rotation/scale. Keep its lifetime in the destination's scene.
            SceneManager.MoveGameObjectToScene(overlay, gameObject.scene);
            canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvasRect = (RectTransform)overlay.transform;
            CanvasScaler scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup group = overlay.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            markerRect = NewRect("Marker", canvasRect, new Vector2(240f, 100f));
            diamond = NewRect("Diamond", markerRect, Vector2.one * iconSize).gameObject.AddComponent<DestinationMarkerGraphic>();
            diamond.raycastTarget = false;
            customIcon = NewRect("Custom icon", markerRect, Vector2.one * iconSize).gameObject.AddComponent<Image>();
            customIcon.preserveAspect = true;
            customIcon.raycastTarget = false;
            nameText = CreateText("Area name", 20f, markerColor);
            distanceText = CreateText("Distance", 16f, new Color(0.92f, 0.96f, 0.93f, 1f));
            markerRect.gameObject.SetActive(false);
        }

        private TMP_Text CreateText(string objectName, float fontSize, Color color)
        {
            var text = NewRect(objectName, markerRect, new Vector2(240f, 26f)).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.richText = false;
            text.raycastTarget = false;
            text.color = color;
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private static RectTransform NewRect(string objectName, Transform parent, Vector2 size)
        {
            var rect = (RectTransform)new GameObject(objectName, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            return rect;
        }

        private void OnDisable()
        {
            SetVfxVisible(false);
            if (overlay != null)
            {
                overlay.SetActive(false);
                Destroy(overlay);
            }
            overlay = null;
            canvas = null;
            markerRect = null;
            lastDistance = -1;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = markerColor;
            Gizmos.DrawWireSphere(AnchorPosition, 0.2f);
            Gizmos.DrawLine(transform.position, AnchorPosition);
        }
    }
}
