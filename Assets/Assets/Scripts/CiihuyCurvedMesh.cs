using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways] // Biar jalan di Editor tanpa Play
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CiihuyCurvedMesh : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Transform> points = new List<Transform>();

    [Header("Mesh Settings")]
    [Range(3, 64)] public int sides = 4;
    public float radius = 0.5f;
    public Material material;
    public bool invertNormals = false;

    [Header("Curve Settings")]
    [Range(2, 100)] public int segmentsPerCurve = 10;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Vector3[] lastPointPositions; // Untuk deteksi perubahan posisi

    void OnValidate()
    {
        InitComponents();
        GenerateMesh();
    }

    void Awake()
    {
        InitComponents();
        GenerateMesh();
    }

    void Update()
    {
        if (!Application.isPlaying) // Biar jalan di Editor
        {
            if (PointsMoved())
            {
                GenerateMesh();
            }
        }
    }

    void InitComponents()
    {
        if (!meshFilter) meshFilter = GetComponent<MeshFilter>();
        if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
    }

    bool PointsMoved()
    {
        if (points == null || points.Count == 0) return false;

        if (lastPointPositions == null || lastPointPositions.Length != points.Count)
            lastPointPositions = new Vector3[points.Count];

        bool moved = false;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null) continue;

            Vector3 pos = points[i].position;
            if (pos != lastPointPositions[i])
            {
                moved = true;
                lastPointPositions[i] = pos;
            }
        }
        return moved;
    }

    void GenerateMesh()
    {
        if (points == null || points.Count < 2) return;

        List<Vector3> splinePoints = GetCatmullRomPoints(points, segmentsPerCurve);
        BuildMesh(splinePoints);

        if (material) meshRenderer.sharedMaterial = material;
    }

    List<Vector3> GetCatmullRomPoints(List<Transform> inputPoints, int segments)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < inputPoints.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? inputPoints[i].position : inputPoints[i - 1].position;
            Vector3 p1 = inputPoints[i].position;
            Vector3 p2 = inputPoints[i + 1].position;
            Vector3 p3 = (i + 2 < inputPoints.Count) ? inputPoints[i + 2].position : p2;

            for (int j = 0; j < segments; j++)
            {
                float t = j / (float)segments;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                result.Add(transform.InverseTransformPoint(point));
            }
        }

        result.Add(transform.InverseTransformPoint(inputPoints[inputPoints.Count - 1].position));
        return result;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * (t * t) +
            (-p0 + 3 * p1 - 3 * p2 + p3) * (t * t * t)
        );
    }

    void BuildMesh(List<Vector3> path)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float totalLength = 0f;
        for (int i = 1; i < path.Count; i++)
            totalLength += Vector3.Distance(path[i - 1], path[i]);

        float currentLength = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 forward = (i < path.Count - 1) ? (path[i + 1] - path[i]).normalized : (path[i] - path[i - 1]).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right);

            for (int s = 0; s < sides; s++)
            {
                float angle = (s / (float)sides) * Mathf.PI * 2;
                Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * radius;
                vertices.Add(path[i] + offset);

                uvs.Add(new Vector2(s / (float)sides, currentLength / totalLength));
            }

            if (i < path.Count - 1)
                currentLength += Vector3.Distance(path[i], path[i + 1]);
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            for (int s = 0; s < sides; s++)
            {
                int curr = i * sides + s;
                int next = i * sides + (s + 1) % sides;
                int currNextRow = (i + 1) * sides + s;
                int nextNextRow = (i + 1) * sides + (s + 1) % sides;

                if (!invertNormals)
                {
                    triangles.Add(curr);
                    triangles.Add(currNextRow);
                    triangles.Add(nextNextRow);

                    triangles.Add(curr);
                    triangles.Add(nextNextRow);
                    triangles.Add(next);
                }
                else
                {
                    triangles.Add(curr);
                    triangles.Add(nextNextRow);
                    triangles.Add(currNextRow);

                    triangles.Add(curr);
                    triangles.Add(next);
                    triangles.Add(nextNextRow);
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }
}