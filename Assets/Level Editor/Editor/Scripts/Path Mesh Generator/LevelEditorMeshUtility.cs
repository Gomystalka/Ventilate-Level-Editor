using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelEditorMeshUtility
{
    public static Mesh GenerateMeshFromQuadData(ref List<Quad> drawnQuads) { //This will be slow.
        List<Vector3> vertices = new List<Vector3>(drawnQuads.Count * 4);
        List<Vector2> uv = new List<Vector2>(vertices.Capacity);
        List<int> triangles = new List<int>(drawnQuads.Count * 8);
        Mesh mesh = new Mesh();
        //HashSet<Vertex> usedVertices = new HashSet<Vertex>();

        for (int q = 0; q < drawnQuads.Count; ++q) {
            Quad quad = drawnQuads[q];

            for (int v = 0; v < 4; ++v) {
                Vertex vertex = quad.Vertices[v];
                if (!vertices.Contains(vertex.Position))
                {
                    //usedVertices.Add(vertex); //This can probably just be a contains check on the vertices list tbh
                    vertices.Add(vertex.Position);
                    uv.Add(new Vector2(vertex.Position.x, vertex.Position.z));
                }
            }
        }
        //There's probably a better way to do this but this works perfectly fine.
        for (int q = 0; q < drawnQuads.Count; ++q) {
            Quad quad = drawnQuads[q];

            int v0 = vertices.IndexOf(quad.Vertices[0].Position);
            int v1 = vertices.IndexOf(quad.Vertices[1].Position);
            int v2 = vertices.IndexOf(quad.Vertices[2].Position);
            int v3 = vertices.IndexOf(quad.Vertices[3].Position);

            triangles.AddRange(new[] { v0, v1, v2}); //0 1 2
            triangles.AddRange(new[] { v2, v3, v0}); //2, 3, 0
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        //mesh.uv = 
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        
        return mesh;
    }

    public static void SaveMeshToFile(Mesh mesh, bool optimiseMesh, string nameOverride = null) {
        if (string.IsNullOrEmpty(nameOverride))
            nameOverride = mesh.name;

        string filePath = UnityEditor.EditorUtility.SaveFilePanel("Save new mesh...", "Assets/", nameOverride, "asset");
        Debug.Log($"Path: {filePath}");
    }
}
