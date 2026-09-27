using UnityEngine;

/// <summary>
/// Exposes the glass renderer's sorting order in the normal Inspector.
/// Use for foreground glass that should blend over the scene's mesh decals.
/// This is a fixed drawing priority, so check views from both sides of the glass.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Rendering/Glass Draw Order")]
public sealed class GlassDrawOrder : MonoBehaviour
{
    [SerializeField]
    [Tooltip("101 draws after Driven Decals orders 0–100 on the same Sorting Layer. " +
        "This also places the glass over decals physically in front of it. " +
        "Set to 0 to restore the usual default order.")]
    private int sortingOrder = 101;

    private void OnEnable()
    {
        ApplySortingOrder();
    }

    private void OnValidate()
    {
        ApplySortingOrder();
    }

    private void ApplySortingOrder()
    {
        sortingOrder = Mathf.Clamp(sortingOrder, short.MinValue, short.MaxValue);
        GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
    }
}
