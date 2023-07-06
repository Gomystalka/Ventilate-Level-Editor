using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelEditorSerializationUtility
{
    public static List<Vector3> EnumerateVertexPositionsFromSerializedQuadArray(ref SerializedQuadData[] serializedQuads, bool includeDuplicates = false)
    {
        List<Vector3> vertexPositions = new List<Vector3>(serializedQuads.Length * 4);
        for (int q = 0; q < serializedQuads.Length; ++q)
        {
            for (int v = 0; v < 4; ++v)
            {
                SerializedVertexData vertex = serializedQuads[q].vertexData[v];
                if (!vertexPositions.Contains(vertex.position) || includeDuplicates)
                    vertexPositions.Add(vertex.position);
            }
        }
        return vertexPositions;
    }

    public static List<Vertex> CreateVertexSceneObjectsFromSerializedQuadData(ref SerializedQuadData[] serializedQuads, bool includeDuplicates = false)
    {
        List<Vertex> sceneVertices = new List<Vertex>(serializedQuads.Length * 4);
        for (int q = 0; q < serializedQuads.Length; ++q)
        {
            for (int v = 0; v < 4; ++v)
            {
                SerializedVertexData vertexData = serializedQuads[q].vertexData[v];
                Vertex sceneVertex = CreateSceneVertexFromSerializedVertexData(ref vertexData);
            }
        }
        return sceneVertices;
    }

    public static Vertex CreateSceneVertexFromSerializedVertexData(ref SerializedVertexData serializedVertexData) { 
        //GameObject vertexGameObject = new GameObject($"Vertex{serializedVertexData.index}:{serializedVertexData.connections[0].quadIndex}");
        //Vertex vertex = vertexGameObject.AddComponent<Vertex>()
        //    .Setup(null, (byte)serializedVertexData.index);
        //vertex.CreateConnection()

        return null;
    }
}
