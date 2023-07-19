using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tom.LevelEditor.PathMeshEditor;

namespace Tom.LevelEditor.Utility
{
    public static class LevelEditorMeshUtility
    {
        public static Mesh GenerateMeshFromQuadData(ref List<Quad> drawnQuads)
        { //This will be slow.
            List<Vector2> uv = new List<Vector2>(drawnQuads.Count * 4);
            List<Vector3> vertices = EnumerateVertexPositionsFromQuadList(ref drawnQuads, false, (vertex) =>
            {
                uv.Add(new Vector2(vertex.Position.x, vertex.Position.z));
            });
            List<int> triangles = new List<int>(drawnQuads.Count * 8);
            Mesh mesh = new Mesh();

            //There's probably a better way to do this but this works perfectly fine.
            for (int q = 0; q < drawnQuads.Count; ++q)
            {
                Quad quad = drawnQuads[q];

                int v0 = vertices.IndexOf(quad.Vertices[0].Position);
                int v1 = vertices.IndexOf(quad.Vertices[1].Position);
                int v2 = vertices.IndexOf(quad.Vertices[2].Position);
                int v3 = vertices.IndexOf(quad.Vertices[3].Position);

                //Why in the flying FUCK am I creating 2 arrays PER iteration??? AM I OKAY?!
                //triangles.AddRange(new[] { v0, v1, v2}); //0 1 2
                //triangles.AddRange(new[] { v2, v3, v0}); //2, 3, 0

                triangles.Add(v0);
                triangles.Add(v1);
                triangles.Add(v2);
                triangles.Add(v2);
                triangles.Add(v3);
                triangles.Add(v0);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
            //mesh.uv = 
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

#if UNITY_EDITOR
        public static string SaveMeshToFile(Mesh mesh, bool optimiseMesh, UnityEditor.ModelImporterMeshCompression compressionLevel = UnityEditor.ModelImporterMeshCompression.Off, string nameOverride = null)
        {
            if (optimiseMesh)
            {
                UnityEditor.MeshUtility.SetMeshCompression(mesh, compressionLevel);
                UnityEditor.MeshUtility.Optimize(mesh);
            }

            if (string.IsNullOrEmpty(nameOverride))
                nameOverride = mesh.name;

            string filePath = UnityEditor.EditorUtility.SaveFilePanel("Save new mesh...", "Assets/", nameOverride, "asset");

            //Create asset uses the relative path instead of full path... Who knew? I fucking didn't.
            UnityEditor.AssetDatabase.CreateAsset(mesh, UnityEditor.FileUtil.GetProjectRelativePath(filePath));
            UnityEditor.AssetDatabase.SaveAssets();

            return filePath;
        }
#endif

        public static List<Vector3> EnumerateVertexPositionsFromQuadList(ref List<Quad> drawnQuads, bool includeDuplicates = false, System.Action<Vertex> enumerationCallback = null)
        {
            List<Vector3> vertexPositions = new List<Vector3>(drawnQuads.Count * 4);
            for (int q = 0; q < drawnQuads.Count; ++q)
            {
                for (int v = 0; v < 4; ++v)
                {
                    Vertex vertex = drawnQuads[q].Vertices[v];
                    if (!vertexPositions.Contains(vertex.Position) || includeDuplicates)
                    {
                        vertexPositions.Add(vertex.Position);
                        enumerationCallback?.Invoke(vertex);
                    }
                }
            }
            return vertexPositions;
        }
    }
}
