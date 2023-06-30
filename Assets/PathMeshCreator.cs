using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathMeshCreator : MonoBehaviour
{
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
    //    //quad = Quad.CreateQuad(startPo, quadSize, currentlyDrawnQuads.Count);
    //}
    //quad.SetParent(transform);

    //if (direction != Quad.Direction.None && source != null)
    //    source.ConnectQuad(ref quad, direction);

    //if(currentlyDrawnQuads.Count != 0  
    //currentlyDrawnQuads.Add(quad);
}

    private Rect _editWindowRect;
    HashSet<Vertex> vertexSet = new HashSet<Vertex>();

    private void DrawQuadsSceneGUI(UnityEditor.SceneView sceneView)
    {
#if UNITY_EDITOR
        if (!placedMaterial || !unplacedMaterial) return;

        //GL.PushMatrix();
        vertexSet.Clear();

        for (int q = 0; q < currentlyDrawnQuads.Count; ++q)
        {
            Quad quad = currentlyDrawnQuads[q];

            for (int v = 0; v < 4; ++v) {

                Vertex vertex = quad.Vertices[v];
                if (!vertexSet.Contains(vertex)) {
                    vertex.UniqueVertexIndex = q * 4 + v;
                    vertexSet.Add(vertex);
                }
            }

            if (drawMode == DrawMode.Quad)
            {
                GL.Begin(GL.QUADS);
                placedMaterial.SetPass(0);
                GL.Color(quadColor);
                quad.GLDraw();
                GL.End();
            }
            else
            {
                GL.Begin(GL.TRIANGLES);
                placedMaterial.SetPass(0);
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

            //if(quad.DirectionLocks[])
            //if (UnityEditor.Handles.Button(quad.Right,
            //    Quaternion.LookRotation(normal), 1f, 1f, UnityEditor.Handles.ArrowHandleCap))
            //{
            //    GeneratePresetQuad(quad, Quad.Direction.Right);
            //}

            //normal = quad.GetEdgeNormal(Quad.Direction.Left);
            //if (UnityEditor.Handles.Button(quad.Left,
            //    Quaternion.LookRotation(normal), 1f, 1f, UnityEditor.Handles.ArrowHandleCap))
            //{
            //    GeneratePresetQuad(quad, Quad.Direction.Left);
            //}

            //UnityEditor.Handles.color = Color.blue;
            //normal = quad.GetEdgeNormal(Quad.Direction.Up);
            //if (UnityEditor.Handles.Button(quad.Up,
            //    Quaternion.LookRotation(normal), 1f, 1f, UnityEditor.Handles.ArrowHandleCap))
            //{
            //    GeneratePresetQuad(quad, Quad.Direction.Up);
            //}

            //normal = quad.GetEdgeNormal(Quad.Direction.Down);
            //if (UnityEditor.Handles.Button(quad.Down,
            //    Quaternion.LookRotation(normal), 1f, 1f, UnityEditor.Handles.ArrowHandleCap))
            //{
            //    GeneratePresetQuad(quad, Quad.Direction.Down);
            //}
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
            //GUI.DragWindow()

            //GUI.Button(new Rect(100, 100, 200, 50), "Test");

            UnityEditor.Handles.EndGUI();
        }
#endif
        //GL.PopMatrix();

        //GL.PushMatrix();
        //GL.Begin(GL.QUADS);
        //mat.SetPass(0);
        //GL.Color(Color.red);
        //GL.Vertex(v1.position);
        //GL.Vertex(v2.position);
        //GL.Vertex(v3.position);
        //GL.Vertex(v4.position);

        //GL.Vertex(v2.position);
        //GL.Vertex(v1.position);
        //GL.Vertex(v7.position);
        //GL.Vertex(v8.position);
        //GL.End();
        //GL.PopMatrix();
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
}

public enum DrawMode {
    Quad,
    Triangle
}