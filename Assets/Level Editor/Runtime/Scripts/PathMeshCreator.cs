using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathMeshCreator : MonoBehaviour
{
    //TO-DO
    // -Persistence (After play mode, editor close, etc.)
    // -Mesh generation - DONE
    //  -Generated mesh controls (Save to File) - DONE

    [Header("Settings")]
    public float quadSize = 2f;
    
    [Header("References")]
    public Material placedMaterial;
    public Material unplacedMaterial;

    public Color quadColor = Color.red;
    public DrawMode drawMode;

    public List<Quad> currentlyDrawnQuads = new List<Quad>(); //Somehow serialize this...

    private void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView.duringSceneGui -= DrawQuadsSceneGUI;
        UnityEditor.SceneView.duringSceneGui += DrawQuadsSceneGUI;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView.duringSceneGui -= DrawQuadsSceneGUI;
#endif
    }

    public Quad GeneratePresetQuad(Quad source = null, Quad.Direction direction = Quad.Direction.None) {
        Quad quad = null;

        if (source != null)
        { 
            quad = Quad.CreateConnectedQuad(ref source, direction, quadSize, currentlyDrawnQuads.Count);
            quad.GenerateSelfConnections();
            quad.SetParent(transform);

            source.ConnectQuad(ref quad, direction);
        }
        else
        {
            quad = Quad.CreateQuad(transform.position, quadSize, currentlyDrawnQuads.Count);
            quad.GenerateSelfConnections();
            quad.SetParent(transform);
        }

        currentlyDrawnQuads.Add(quad);

        return quad;
}

    private Rect _editWindowRect;

    private void DrawQuadsSceneGUI(UnityEditor.SceneView sceneView)
    {
#if UNITY_EDITOR
        if (!placedMaterial || !unplacedMaterial) return;

        for (int q = 0; q < currentlyDrawnQuads.Count; ++q)
        {
            Quad quad = currentlyDrawnQuads[q];
            if (drawMode == DrawMode.Quad)
            {
                GL.Begin(GL.QUADS);
                unplacedMaterial.SetPass(0);
                GL.Color(quadColor);
                quad.GLDraw();
                GL.End();
            }
            else
            {
                GL.Begin(GL.TRIANGLES);
                unplacedMaterial.SetPass(0);
                GL.Color(quadColor);
                quad.GLDrawTriangles();
                GL.End();
            }

            UnityEditor.Handles.color = Color.red;
            Vector3 normal = Vector3.zero;
            UnityEditor.Handles.color = Color.blue;
            for (Quad.Direction direction = Quad.Direction.Up; direction <= Quad.Direction.Right; ++direction)
            {
                if(direction > Quad.Direction.Down)
                    UnityEditor.Handles.color = Color.red;

                int index = ((int)direction) - 1; //Subtract 1 because of Direction.None
                if (quad.DirectionLocks[index]) continue;

                normal = quad.GetEdgeNormal(direction);
                if (UnityEditor.Handles.Button(quad.GetEdge(direction),
                    Quaternion.LookRotation(normal), 0.05f, 0.05f, UnityEditor.Handles.DotHandleCap)) {
                    Quad newQuad = GeneratePresetQuad(quad, direction);
                    quad.DirectionLocks[index] = true;

                    Quad.Direction invertedDirection = Quad.GetInvertedDirection(direction);                  
                    newQuad.DirectionLocks[((byte)invertedDirection) - 1] = true;
                }
            }
        }


        if (Vertex.CurrentlySelectedVertex != null)
        {
            UnityEditor.Handles.BeginGUI();
            string vertexName = Vertex.CurrentlySelectedVertex.name;

            _editWindowRect = GUI.Window(0, _editWindowRect, (id) =>
            {
                Rect contentRect = new Rect(_editWindowRect);
                contentRect.x = 0;
                contentRect.y = 0;

                DrawVertexSpaceControls(ref contentRect);

                GUIStyle centerLabel = new GUIStyle(GUI.skin.label);
                centerLabel.alignment = TextAnchor.MiddleCenter;
                GUI.Label(contentRect, vertexName, centerLabel);

                DrawConnectVerticesControl(ref contentRect);
                DrawDisconnectVerticesControl(ref contentRect);

                GUI.DragWindow();
            }, "Path Tools");

            UnityEditor.Handles.EndGUI();
        }
#endif
    }

    private bool _isLocalSpace;

    private void DrawVertexSpaceControls(ref Rect contentRect)
    {
        if (GUI.Button(new Rect(1, 1, 24f, 24f), _isLocalSpace ? "L" : "W"))
        {
            _isLocalSpace = !_isLocalSpace;
            SetVertexSpace();
        }
    } 

    private void DrawConnectVerticesControl(ref Rect contentRect) {
#if UNITY_EDITOR
        CheckForConnectableVerticesInSelection(out Vertex vertex1, out Vertex vertex2);
        if (!vertex1 || !vertex2) return;

        contentRect.y += UnityEditor.EditorGUIUtility.singleLineHeight + 40f;
        contentRect.height = 20f;

        if (GUI.Button(contentRect, $"Connect {vertex1.name} to {vertex2.name}"))
        {
            UnityEditor.Selection.activeObject = null;
            vertex1.Merge(vertex2);
        }
#endif
    }

    private void DrawDisconnectVerticesControl(ref Rect contentRect)
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.count != 1) return;

        if (UnityEditor.Selection.objects[0] is GameObject go)
        {
            Vertex v = go.GetComponent<Vertex>();
            if (!v || v.Connections.Count <= 1) return;

            contentRect.y += UnityEditor.EditorGUIUtility.singleLineHeight + 40f;
            contentRect.height = 20f;

            if (GUI.Button(contentRect, $"Disconnect {v.name}"))
            {
                v.Disconnect();
                UnityEditor.Selection.activeObject = null;
            }
        }
#endif
    }

    private void SetVertexSpace()
    {
        foreach (Quad q in currentlyDrawnQuads)
            q.SetVertexSpace(_isLocalSpace);
    }

    private void CheckForConnectableVerticesInSelection(out Vertex vertex1, out Vertex vertex2) {
#if UNITY_EDITOR
        vertex1 = null; 
        vertex2 = null;
        if (UnityEditor.Selection.count != 2) return; 

        //This "Should" preserve selection order.
        if (UnityEditor.Selection.objects[0] is GameObject go)
            vertex1 = go.GetComponent<Vertex>();
        if (UnityEditor.Selection.objects[1] is GameObject go2)
            vertex2 = go2.GetComponent<Vertex>();

        //vertex1 = UnityEditor.Selection.gameObjects[0].GetComponent<Vertex>();
        //vertex2 = UnityEditor.Selection.gameObjects[1].GetComponent<Vertex>();
#endif
    }


    public void DestroyAllQuads() {
        foreach (Quad quad in currentlyDrawnQuads)
            quad.Destroy();

        currentlyDrawnQuads.Clear();
        currentlyDrawnQuads.Capacity = 0;
    }

    public void ResetWindowState() {
#if UNITY_EDITOR
        _editWindowRect = new Rect(400, 200, 200, 80);
#endif
    }

    public void RemoveStaleVerticesInScene() {
        Vertex[] sceneVertices = FindObjectsOfType<Vertex>(true);

        for (int v = 0; v < sceneVertices.Length; ++v) {
            Vertex vertex = sceneVertices[v];
            //A vertex is stale if there are no connections to/from it and its owner is null.
            if ((vertex.Connections == null || vertex.Connections.Count == 0) && vertex.Owner == null)
            {
                if (Application.isPlaying)
                    Destroy(vertex.gameObject);
                else
                    DestroyImmediate(vertex.gameObject);
            }
        }

        System.Array.Clear(sceneVertices, 0, sceneVertices.Length);
    }

    public void SerializeCurrentQuadData() {
        SerializedPathCreatorData data = new SerializedPathCreatorData();
        data.IsInLocalView = _isLocalSpace;
        data.DrawMode = drawMode;
        data.UnplacedMaterialPath = UnityEditor.AssetDatabase.GetAssetPath(unplacedMaterial);
        data.QuadSizeUnits = quadSize;
        data.QuadColor = quadColor;
        data.QuadData = new SerializedQuadData[currentlyDrawnQuads.Count];



        for (int q = 0; q < currentlyDrawnQuads.Count; ++q) {
            Quad quad = currentlyDrawnQuads[q];
            data.QuadData[q] = SerializedQuadData.GenerateDataFromQuad(ref quad);
            //currentlyDrawnQuads[q] = quad;
        }

        //Compile list of all vertices
        //Compile list of Quad data. Vertices linked by indices
        //Save connections by indices
    }

    public void DeserializeQuadData() { 
        
    }
}

public enum DrawMode {
    Quad,
    Triangle
}

[System.Serializable]
public struct SerializedPathCreatorData {
    public bool IsInLocalView { get; set; }
    public DrawMode DrawMode { get; set; }
    public string UnplacedMaterialPath { get; set; }
    public float QuadSizeUnits { get; set; }
    public Color QuadColor { get; set; }
    public SerializedQuadData[] QuadData { get; set; }
}
[System.Serializable]
public struct SerializedVertexData {
    public Vector3 Position { get; set; }
    public int Index { get; set; }
    public int UniqueIndex { get; set; }
    public SerializedConnectionData[] Connections { get; set; }
}
[System.Serializable]
public struct SerializedQuadData {
    public int QuadIndex { get; set; }
    public SerializedVertexData[] VertexData { get; set; }

    public static SerializedQuadData GenerateDataFromQuad(ref Quad quad) {
        SerializedQuadData data = new SerializedQuadData();
        data.QuadIndex = quad.QuadIndex;
        data.VertexData = new SerializedVertexData[4];
        //Implement IndexOf Vertex
        //data.VertexData = 

        return default;
    }
}
[System.Serializable]
public struct SerializedConnectionData {
    public int QuadIndex { get; set; }
    public int VertexIndex { get; set; }
}