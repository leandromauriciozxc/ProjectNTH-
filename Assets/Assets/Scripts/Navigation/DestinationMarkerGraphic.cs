using UnityEngine;
using UnityEngine.UI;

namespace ProjectNTH.Navigation
{
    /// <summary>A small outlined diamond; needs no texture or font glyph.</summary>
    [AddComponentMenu("")]
    public sealed class DestinationMarkerGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2 center = rect.center;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            Ring(mesh, center, radius, radius * 0.62f, new Color(0f, 0f, 0f, color.a * 0.8f));
            Ring(mesh, center, radius * 0.88f, radius * 0.74f, color);
            Ring(mesh, center, radius * 0.18f, 0f, color);
        }

        private static void Ring(VertexHelper mesh, Vector2 center, float outer, float inner, Color32 tint)
        {
            for (int edge = 0; edge < 4; edge++)
            {
                Vector2 a = Direction(edge);
                Vector2 b = Direction((edge + 1) % 4);
                int first = mesh.currentVertCount;
                mesh.AddVert(center + a * outer, tint, Vector2.zero);
                mesh.AddVert(center + b * outer, tint, Vector2.zero);
                mesh.AddVert(center + b * inner, tint, Vector2.zero);
                mesh.AddVert(center + a * inner, tint, Vector2.zero);
                mesh.AddTriangle(first, first + 1, first + 2);
                mesh.AddTriangle(first + 2, first + 3, first);
            }
        }

        private static Vector2 Direction(int corner)
        {
            switch (corner)
            {
                case 0: return Vector2.up;
                case 1: return Vector2.right;
                case 2: return Vector2.down;
                default: return Vector2.left;
            }
        }
    }
}
