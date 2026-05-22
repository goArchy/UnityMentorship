using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a floor mesh from level grid data, omitting cells marked as holes ('*').
/// </summary>
public static class GroundMeshBuilder
{
    public static Mesh Build(
        string[] levelData,
        float cellSize,
        float offsetX,
        float offsetZ,
        float groundMargin,
        float y = 0f)
    {
        int rows = levelData.Length;
        int cols = levelData[0].Length;
        int marginCells = Mathf.Max(1, Mathf.CeilToInt(groundMargin / cellSize));

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        int minCol = -marginCells;
        int maxCol = cols - 1 + marginCells;
        int minRow = -marginCells;
        int maxRow = rows - 1 + marginCells;

        float half = cellSize * 0.5f;

        for (int row = minRow; row <= maxRow; row++)
        {
            for (int col = minCol; col <= maxCol; col++)
            {
                if (IsHoleCell(levelData, rows, cols, row, col))
                    continue;

                float cx = offsetX + col * cellSize;
                float cz = offsetZ - row * cellSize;

                AddQuad(vertices, triangles, normals, uvs, cx, y, cz, half);
            }
        }

        var mesh = new Mesh { name = "GroundFloor" };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();
        return mesh;
    }

    static bool IsHoleCell(string[] levelData, int rows, int cols, int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return false;

        string rowData = levelData[row];
        if (col >= rowData.Length)
            return false;

        return rowData[col] == '*';
    }

    static void AddQuad(
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector3> normals,
        List<Vector2> uvs,
        float cx,
        float y,
        float cz,
        float half)
    {
        int i = vertices.Count;

        vertices.Add(new Vector3(cx - half, y, cz - half));
        vertices.Add(new Vector3(cx + half, y, cz - half));
        vertices.Add(new Vector3(cx + half, y, cz + half));
        vertices.Add(new Vector3(cx - half, y, cz + half));

        for (int v = 0; v < 4; v++)
        {
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(vertices[i + v].x, vertices[i + v].z));
        }

        triangles.Add(i);
        triangles.Add(i + 2);
        triangles.Add(i + 1);
        triangles.Add(i);
        triangles.Add(i + 3);
        triangles.Add(i + 2);
    }
}
