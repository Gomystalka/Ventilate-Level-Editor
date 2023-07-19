using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tom.LevelEditor.Runtime.Utility;

namespace Tom.LevelEditor.Runtime.PathMeshEditor
{
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
        public Material unplacedMaterial;

        public Color quadColor = Color.red;
        public DrawMode drawMode;

        public List<Quad> currentlyDrawnQuads = new List<Quad>();
        public int QuadCount => currentlyDrawnQuads.Count;

        [HideInInspector] public bool showMeshBuildingControls;

#if !UNITY_EDITOR
    private void Awake()
    {
        Debug.LogWarning("The Path Mesh editor was left within the scene! This object MUST be removed from the scene before building the application. The object was automatically removed.");
        Destroy(gameObject);
    }
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView.duringSceneGui -= DrawQuadsSceneGUI;
                UnityEditor.SceneView.duringSceneGui += DrawQuadsSceneGUI;
            }
#endif

            if (Application.isPlaying)
                Destroy(gameObject);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.duringSceneGui -= DrawQuadsSceneGUI;
#endif
        }

        public Quad GeneratePresetQuad(Quad source = null, Quad.Direction direction = Quad.Direction.None)
        {
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

#if UNITY_EDITOR
        private void DrawQuadsSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (!unplacedMaterial) return;

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
                if (!showMeshBuildingControls) continue;

                UnityEditor.Handles.color = Color.red;
                Vector3 normal = Vector3.zero;
                UnityEditor.Handles.color = Color.blue;
                for (Quad.Direction direction = Quad.Direction.Up; direction <= Quad.Direction.Right; ++direction)
                {
                    if (direction > Quad.Direction.Down)
                        UnityEditor.Handles.color = Color.red;

                    int index = ((int)direction) - 1; //Subtract 1 because of Direction.None
                                                      //if (quad.DirectionLocks[index]) continue;

                    normal = quad.GetEdgeNormal(direction);
                    Quaternion lookRotation = normal.magnitude > Mathf.Epsilon ? Quaternion.LookRotation(normal) : Quaternion.identity;

                    if (UnityEditor.Handles.Button(quad.GetEdge(direction),
                        lookRotation, 0.05f, 0.05f, UnityEditor.Handles.DotHandleCap))
                    {
                        Quad newQuad = GeneratePresetQuad(quad, direction);
                        //quad.DirectionLocks[index] = true;

                        //Quad.Direction invertedDirection = Quad.GetInvertedDirection(direction);                  
                        //newQuad.DirectionLocks[((byte)invertedDirection) - 1] = true;
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
                    DrawDeleteQuadControl(ref contentRect);

                    GUI.DragWindow();
                }, "Path Tools");

                UnityEditor.Handles.EndGUI();
            }
        }
#else
    private void DrawQuadsSceneGUI(object _) {}
#endif

        private bool _isLocalSpace;

        private void DrawVertexSpaceControls(ref Rect contentRect)
        {
            if (GUI.Button(new Rect(1, 1, 24f, 24f), _isLocalSpace ? "L" : "W"))
            {
                _isLocalSpace = !_isLocalSpace;
                SetVertexSpace();
            }
        }

        private void DrawConnectVerticesControl(ref Rect contentRect)
        {
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

        private void DrawDeleteQuadControl(ref Rect contentRect)
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.count != 1) return;
            if (UnityEditor.Selection.objects[0] is GameObject go)
            {
                Vertex v = go.GetComponent<Vertex>();
                if (!v || v.Owner == null) return;

                contentRect.y += 20f;
                contentRect.height = 20f;

                if (GUI.Button(contentRect, $"Delete Quad {v.Owner.QuadIndex}"))
                {
                    TryDeleteQuad(v.Owner.QuadIndex);
                    //v.Disconnect();
                    UnityEditor.Selection.activeObject = null;
                }
            }
#endif
        }

        private void TryDeleteQuad(int quadIndex)
        { //It's easier working with indices 
            for (int q = currentlyDrawnQuads.Count - 1; q >= 0; --q)
            {
                if (quadIndex == q)
                {
                    currentlyDrawnQuads[q].Destroy(true);
                    currentlyDrawnQuads.RemoveAt(q);
                }
            }

            for (int q = 0; q < currentlyDrawnQuads.Count; ++q) //Re-set indices as they will no longer be valid
                currentlyDrawnQuads[q].QuadIndex = q;
        }

        private void SetVertexSpace()
        {
            foreach (Quad q in currentlyDrawnQuads)
                q.SetVertexSpace(_isLocalSpace);
        }

        private void CheckForConnectableVerticesInSelection(out Vertex vertex1, out Vertex vertex2)
        {
            vertex1 = null;
            vertex2 = null;
#if UNITY_EDITOR
            if (UnityEditor.Selection.count != 2) return;

            //This "Should" preserve selection order.
            if (UnityEditor.Selection.objects[0] is GameObject go)
                vertex1 = go.GetComponent<Vertex>();
            if (UnityEditor.Selection.objects[1] is GameObject go2)
                vertex2 = go2.GetComponent<Vertex>();
#endif
        }


        public void DestroyAllQuads()
        {
            foreach (Quad quad in currentlyDrawnQuads)
                quad.Destroy(false);

            currentlyDrawnQuads.Clear();
            currentlyDrawnQuads.Capacity = 0;
        }

        public void ResetWindowState()
        {
#if UNITY_EDITOR
            _editWindowRect = new Rect(400, 300, 200, 100);
#endif
        }

        public void RemoveStaleVerticesInScene()
        {
            Vertex[] sceneVertices = FindObjectsOfType<Vertex>(true);

            for (int v = 0; v < sceneVertices.Length; ++v)
            {
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

        public string SerializeCurrentQuadData()
        {
            SerializedPathCreatorData data = new SerializedPathCreatorData();
            data.isInLocalView = _isLocalSpace;
            data.drawMode = drawMode;
#if UNITY_EDITOR
            data.unplacedMaterialPath = UnityEditor.AssetDatabase.GetAssetPath(unplacedMaterial);
#endif
            data.quadSizeUnits = quadSize;
            data.quadColor = quadColor;
            data.vertexData = LevelEditorSerializationUtility.CreateVertexDataFromQuadList(ref currentlyDrawnQuads).ToArray();
            data.quadData = new SerializedQuadData[currentlyDrawnQuads.Count];

            for (int q = 0; q < data.quadData.Length; ++q)
            {
                data.quadData[q].vertexIndices = new int[4];
                for (int v = 0; v < 4; ++v)
                    data.quadData[q].vertexIndices[v] = currentlyDrawnQuads[q].Vertices[v].UniqueVertexIndex; //UniqueVertexIndex will be correct due to the CreateVertexDataFromQuadList call.
            }

            return JsonUtility.ToJson(data, true);
        }

        public void DeserializeQuadData(string jsonString)
        {
            SerializedPathCreatorData data = JsonUtility.FromJson<SerializedPathCreatorData>(jsonString);
            if (!data.IsValid)
            {
                Debug.LogError($"Failed to deserialize quad data!");
                return;
            }

            _isLocalSpace = data.isInLocalView;
            drawMode = data.drawMode;
#if UNITY_EDITOR
            unplacedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(data.unplacedMaterialPath);
            if (!unplacedMaterial)
                Debug.LogError($"[{typeof(PathMeshCreator)}]: Failed to load Unplaced Material resource! Check if the path is correct and if the material is not corrupted!", this);
#endif
            quadSize = data.quadSizeUnits;
            quadColor = data.quadColor;

            foreach (Quad q in currentlyDrawnQuads)
                q.Destroy(false);
            currentlyDrawnQuads.Clear();

            currentlyDrawnQuads = LevelEditorSerializationUtility.CreateQuadsFromPathData(ref data, transform);
        }
    }

    public enum DrawMode
    {
        Quad,
        Triangle
    }
}