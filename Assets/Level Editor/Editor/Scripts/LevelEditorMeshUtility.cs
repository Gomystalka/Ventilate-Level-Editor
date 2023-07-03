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

        for(int q = 0; q < drawnQuads.Count; ++q) {
            for (int v = 0; v < 4; ++v) {
                Vertex vertex = drawnQuads[q].Vertices[v];
                if (!vertices.Contains(vertex.Position))
                {
                    //usedVertices.Add(vertex); //This can probably just be a contains check on the vertices list tbh
                    vertices.Add(vertex.Position);
                    uv.Add(new Vector2(vertex.Position.x, vertex.Position.z));
                }
                //triangles.Add(drawnQuads[q].Vertices[v]); //0, 1, 2 | 2, 3, 0
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        //mesh.uv = 
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        
        return mesh;
    }
}
