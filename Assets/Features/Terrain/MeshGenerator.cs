using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public int xSize = 20, zSize = 20;
    public float maxHeight = 5f;

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    private Mesh mesh;

    public Gradient gradient;

    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    private void Start()
    {
        mesh = new Mesh();

        CalculateVertices();
        CalculateTriangles();

        UpdateMesh();
    }

    private void CalculateVertices()
    {
        vertices = new Vector3[xSize * zSize];
        colors = new Color[xSize * zSize];

        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                ComputeVertex(x, z);
            }
        }
    }

    private void ComputeVertex(int x, int z)
    {
        float noise = Mathf.PerlinNoise(x * .1f, z * .1f);

        vertices[x * zSize + z] = new Vector3(x, noise * maxHeight, z);
        colors[x * zSize + z] = gradient.Evaluate(noise);
    }

    private void CalculateTriangles()
    {
        triangles = new int[(xSize - 1) * (zSize - 1) * 6]; // A quad (2 triangles) for each 4 vertices

        for(int x = 0; x < xSize - 1; x++)
        {
            for (int z = 0; z < zSize - 1; z++)
            {
                int triangleIndex = (x * (zSize - 1) + z) * 6;
                // Down left triangle
                triangles[triangleIndex] = x * zSize + z;
                triangles[triangleIndex + 1] = x * zSize + (z + 1);
                triangles[triangleIndex + 2] = (x + 1) * zSize + (z + 1);
                // Up right triangle
                triangles[triangleIndex + 3] = x * zSize + z;
                triangles[triangleIndex + 4] = (x + 1) * zSize + (z + 1);
                triangles[triangleIndex + 5] = (x + 1) * zSize + z;
            }
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
