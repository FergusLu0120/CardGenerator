using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dataHexa : MonoBehaviour
{
    public float[] cornerDistances = new float[6]; // Distances from the center to each corner

    void Start()
    {
        Mesh hexagonMesh = CreateHexagonMesh();
        GetComponent<MeshFilter>().mesh = hexagonMesh;
    }

    Mesh CreateHexagonMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[7]; // Increased to 7 to close the hexagon
        int[] triangles = new int[18]; // 6 vertices, 3 triangles per vertex
        Vector2[] uv = new Vector2[7]; // Increased to 7 to match the number of vertices

        // Generating vertices using the cornerDistances array
        for (int i = 0; i < 6; i++)
        {
            float angleRad = Mathf.PI / 3 * i;
            vertices[i] = new Vector3(Mathf.Cos(angleRad) * cornerDistances[i], 0f, Mathf.Sin(angleRad) * cornerDistances[i]);
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        // Adding center vertex
        vertices[6] = Vector3.zero;
        uv[6] = new Vector2(0.5f, 0.5f);

        // Generating triangles
        for (int i = 0; i < 6; i++)
        {
            triangles[i * 3] = 6; // Center vertex index
            triangles[i * 3 + 1] = i;
            triangles[i * 3 + 2] = (i + 1) % 6;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();

        return mesh;
    }
}
