using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelEditorSerializationUtility
{
    //public static List<Vector3> EnumerateVertexPositionsFromSerializedQuadArray(ref SerializedQuadData[] serializedQuads, bool includeDuplicates = false)
    //{
    //    List<Vector3> vertexPositions = new List<Vector3>(serializedQuads.Length * 4);
    //    for (int q = 0; q < serializedQuads.Length; ++q)
    //    {
    //        for (int v = 0; v < 4; ++v)
    //        {
    //            SerializedVertexData vertex = serializedQuads[q].vertexData[v];
    //            if (!vertexPositions.Contains(vertex.position) || includeDuplicates)
    //                vertexPositions.Add(vertex.position);
    //        }
    //    }
    //    return vertexPositions;
    //}

    ////This method needs to create the quads associated with the vertices before 
    //public static (List<Vertex>, List<Quad>) CreateSceneQuadsFromSerializedQuadData(ref SerializedQuadData[] serializedQuads, bool includeDuplicates = false)
    //{
    //    //List<Vertex> sceneVertices = new List<Vertex>(serializedQuads.Length * 4);
    //    //Quad[] quads = new Quad[serializedQuads.Length];

    //    //for (int q = 0; q < serializedQuads.Length; ++q)
    //    //{
    //    //    quads[q] = Quad.CreateEmptyIndexedQuad(q);

    //    //    for (int v = 0; v < 4; ++v)
    //    //    {
    //    //        SerializedVertexData vertexData = serializedQuads[q].vertexData[v];
    //    //        Vertex sceneVertex = CreateSceneVertexFromSerializedVertexData(ref vertexData);
    //    //    }
    //    //}
    //    //return sceneVertices;

    //    return (null, null);
    //}

    //public static List<Quad> CreateQuadsFromSerializedQuadData(ref SerializedQuadData[] serializedQuads) {
    //    //List<Quad> quads = new List<Quad>(serializedQuads.Length);
    //    ////List<Vertex> vertices = CreateSceneQuadsFromSerializedQuadData(ref serializedQuads);

    //    //for (int v = 0; v < vertices.Count; ++v) {//Create connections for vertices. (This can only be done AFTER the quads are present.)
    //    //    Vertex vertex = vertices[v];
    //    //} 

    //    //for (int q = 0; q < serializedQuads.Length; ++q) { //Populate quads with scene vertices
    //    //    Quad quad = quads[q];
    //    //    quad.Vertices[0] = vertices[serializedQuads[q].vertexData[0].uniqueIndex];
    //    //    quad.Vertices[1] = vertices[serializedQuads[q].vertexData[1].uniqueIndex];
    //    //    quad.Vertices[2] = vertices[serializedQuads[q].vertexData[2].uniqueIndex];
    //    //    quad.Vertices[3] = vertices[serializedQuads[q].vertexData[3].uniqueIndex];
    //    //}
    //    return null;
    //}

    /// <summary>
    /// This method is used to create a serializable data type from the present quads in the scene. <i>This scene also sets the <b>UniqueVertexIndex</b> property of the vertices to aid in the serialization process.</i>
    /// </summary>
    /// <param name="quads">A reference to the list of draw quads in the scene.</param>
    /// <param name="includeDuplicates">Determines whether duplicate vertices may be included.</param>
    /// <returns>A <b>List</b> of <b>SerializedVertexData</b> created from all vertices in the scene.</returns>
    public static List<SerializedVertexData> CreateVertexDataFromQuadList(ref List<Quad> quads, bool includeDuplicates = false)
    {
        List<SerializedVertexData> vertices = new List<SerializedVertexData>(quads.Count * 4);
        for (int q = 0; q < quads.Count; ++q)
        {
            for (int v = 0; v < 4; ++v)
            {
                int uniqueVertexIndex = vertices.Count;

                SerializedVertexData vertex = CreateSerializedVertexDataFromVertex(quads[q].Vertices[v], uniqueVertexIndex);
                if (!vertices.Contains(vertex) || includeDuplicates)
                {
                    quads[q].Vertices[v].UniqueVertexIndex = uniqueVertexIndex;
                    vertices.Add(vertex);
                }
            }
        }
        return vertices;
    }

    //private bool VertexDataExists(ref List<SerializedVertexData> data) { 
        
    //}

    private static SerializedVertexData CreateSerializedVertexDataFromVertex(Vertex vertex, int index) {
        if (vertex == null) return default;
        SerializedVertexData data = new SerializedVertexData();
        data.uniqueIndex = index;
        data.position = vertex.Position;
        data.quadOwnerIndex = vertex.Owner.QuadIndex;
        data.index = vertex.VertexIndex;
        data.connections = SerializedConnectionData.GenerateSerializableConnectionData(vertex.Connections);
        return data;
    }

    private static Vertex CreateSceneVertexFromSerializedVertexData(ref SerializedVertexData serializedVertexData, Quad ownerQuad) {
        GameObject vertexGameObject = new GameObject($"Vertex{serializedVertexData.index}:{serializedVertexData.quadOwnerIndex}");
        vertexGameObject.transform.position = serializedVertexData.position;
        Vertex vertex = vertexGameObject.AddComponent<Vertex>()
            .Setup(ownerQuad, (byte)serializedVertexData.index);
        vertex.UniqueVertexIndex = serializedVertexData.uniqueIndex;
        //vertex.CreateConnection()

        return vertex;
    }

    public static List<Quad> CreateQuadsFromPathData(ref SerializedPathCreatorData data, Transform quadParent, bool worldPositionStays = true) //This needs optimisation...
    {
        List<Quad> quads = new List<Quad>(data.quadData.Length);
        List<Vertex> sceneVertices = new List<Vertex>(data.vertexData.Length);

        if (data.vertexData.Length < 3)
            return quads;

        if (data.quadData.Length == 0) return quads;

        for (int q = 0; q < data.quadData.Length; ++q) //Create Quads
            quads.Add(Quad.CreateEmptyIndexedQuad(q));

        for (int v = 0; v < data.vertexData.Length; ++v) { //Create the scene vertices and its connections
            Vertex vertex = CreateSceneVertexFromSerializedVertexData(ref data.vertexData[v], quads[data.vertexData[v].quadOwnerIndex]);
            CreateConnectionsForVertexFromSerializedData(ref vertex, ref data.vertexData[v], ref quads);
            sceneVertices.Add(vertex);
        }

        for (int q = 0; q < quads.Count; ++q)
        { //Assign the vertices to the quads.
            for (int v = 0; v < 4; ++v)
                quads[q].Vertices[v] = sceneVertices[data.quadData[q].vertexIndices[v]];

            quads[q].SetParent(quadParent, worldPositionStays);
        }

        sceneVertices.Clear();
        sceneVertices.Capacity = 0;

        return quads;
    }

    private static void CreateConnectionsForVertexFromSerializedData(ref Vertex vertex, ref SerializedVertexData data, ref List<Quad> quads) {
        for (int c = 0; c < data.connections.Length; ++c)
            vertex.CreateConnection(quads[data.connections[c].quadIndex], (byte)data.connections[c].vertexIndex);
    }
}


[System.Serializable]
public struct SerializedPathCreatorData
{
    public bool isInLocalView;
    public DrawMode drawMode;
    public string unplacedMaterialPath;
    public float quadSizeUnits;
    public Color quadColor;
    public SerializedVertexData[] vertexData;
    public SerializedQuadData[] quadData;

    public bool IsValid => quadData != null && quadData.Length != 0 && vertexData != null && vertexData.Length != 0;
}
[System.Serializable]
public struct SerializedVertexData
{
    public Vector3 position;
    public int index;
    public int uniqueIndex;
    public int quadOwnerIndex;
    public SerializedConnectionData[] connections;

    public static bool operator !=(SerializedVertexData data0, SerializedVertexData data1)
        => !(data0 == data1);

    public static bool operator ==(SerializedVertexData data0, SerializedVertexData data1)
        => Mathf.Approximately(data0.position.x, data1.position.x) &&
            Mathf.Approximately(data0.position.y, data1.position.y) &&
            Mathf.Approximately(data0.position.z, data1.position.z);

    public override bool Equals(object obj)
    {
        if (obj is SerializedVertexData data)
            return data == this;
        return false;
    }

    public override int GetHashCode()
        => base.GetHashCode();
}

[System.Serializable]
public struct SerializedConnectionData
{
    public int quadIndex;
    public int vertexIndex;

    public static SerializedConnectionData[] GenerateSerializableConnectionData(List<VertexConnection> connections)
    {
        SerializedConnectionData[] connectionData = new SerializedConnectionData[connections.Count];
        for (int c = 0; c < connections.Count; ++c)
        {
            connectionData[c].quadIndex = connections[c].quad.QuadIndex;
            connectionData[c].vertexIndex = connections[c].vertexIndex;
        }
        return connectionData;
    }
}
[System.Serializable]
public struct SerializedQuadData
{
    public int[] vertexIndices;
}
