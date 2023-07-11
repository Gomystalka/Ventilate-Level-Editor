using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer), typeof(BoxCollider))]
public class CombatAreaCreator : MonoBehaviour
{
    //TO-DO
    // -Add support for multiple combat areas. -Create scene object via editor window and use focused one.
    // -Make the direction toggle actually work. - DONE
    //   -Reorderable list of all CombatAreaCreators in the scene perhaps?
    // -Add support for creating vertices between existing vertices. - DONE
    // -Add finalise button which connects the last point to the source point, thus finishing the combat area. -DONE + Extra disconnect button =3
    // -Add Enemy display UI in scene view above the area. -WIP
    // -Add line segment overlap detection? (Maybe)
    // -Make all object names unique.

    public static Texture vertexIconTexture;

    private LineRenderer _combatAreaLineRenderer;
    private BoxCollider _collider;
    public Transform SourceTransform { get; set; }
    public CombatAreaEditMode EditMode { get; set; } = CombatAreaEditMode.None;

    /*[HideInInspector] */
    [HideInInspector][SerializeField] private List<Transform> _vertices;

    [HideInInspector] public bool useDirectionVectorForNormal = true;
    [HideInInspector] public bool showRendererBounds = true;
    [HideInInspector] public bool showMidpointAtLineSegmentDuringEditMode = false;
    [HideInInspector] public float lineSegmentLength = 1f;

    [Header("References")]
    [SerializeField] private Canvas _detailCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI _title;

    public int VertexCount => _vertices.Count;

    public bool IsLoopConnected => //A loop is connected if both the first and last vertices are the same.
        _vertices.Count >= 2 && _vertices[0] == _vertices[_vertices.Count - 1];

    [ExecuteInEditMode]
    private void OnEnable()
    {
        _combatAreaLineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<BoxCollider>();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneView.duringSceneGui -= DrawHandles;
            UnityEditor.SceneView.duringSceneGui += DrawHandles;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView.duringSceneGui -= DrawHandles;
#endif
    }

    [ExecuteInEditMode]
    void Update()
    {
        if (_vertices.Count > 0)
            SourceTransform = _vertices[0]; //The source will always be at index 0;

        if (!SourceTransform)
            _combatAreaLineRenderer.positionCount = 0;

        SetPositions();
        SetColliderBounds();
    }

    public GameObject CreateNewPosition(Vector3 position) {
        GameObject vertex = new GameObject($"Area:Vertex:{_vertices.Count}");
        vertex.transform.position = position;
        vertex.transform.SetParent(transform);

        if (_vertices.Count == 0)
            SourceTransform = vertex.transform;

        _vertices.Add(vertex.transform);
        _combatAreaLineRenderer.positionCount = _vertices.Count;

        return vertex;
    }

    public GameObject InsertVertex(int insertIndex, Vector3 position) {
        GameObject vertex = new GameObject($"Area:Vertex:{insertIndex}");
        vertex.transform.position = position;
        vertex.transform.SetParent(transform);

        _vertices.Insert(insertIndex, vertex.transform);
        _combatAreaLineRenderer.positionCount = _vertices.Count;

        return vertex;
    }

    private void SetPositions() {
        DetectDeletedVertices();

        for (int p = 0; p < _vertices.Count; ++p)
            _combatAreaLineRenderer.SetPosition(p, _vertices[p].transform.position);
    }

    private void SetColliderBounds() {
        Vector3 size = _combatAreaLineRenderer.bounds.size;
        size.y = 0.1f;
        _collider.size = size;
        _collider.center = transform.InverseTransformPoint(_combatAreaLineRenderer.bounds.center);
    }

    private void DetectDeletedVertices() {
        for (int v = _vertices.Count - 1; v >= 0; --v)
        {
            Transform vertex = _vertices[v];
            if (!vertex)
            {
                _vertices.Remove(vertex);
                _combatAreaLineRenderer.positionCount = _vertices.Count;
            }
        }
    }

    public void Clear() {
        for (int v = 0; v < _vertices.Count; ++v)
        {
            if (Application.isPlaying)
                Destroy(_vertices[v].gameObject);
            else
                DestroyImmediate(_vertices[v].gameObject);
        }
        _vertices.Clear();
        _combatAreaLineRenderer.positionCount = 0;
        _combatAreaLineRenderer.SetPositions(new Vector3[0]);
    }

    public void ConnectLoop() {
        if (VertexCount < 3) return; //At least 3 Points required
        _vertices.Add(_vertices[0]);

        _combatAreaLineRenderer.positionCount = _vertices.Count;
    }
    public void DisconnectLoop() {
        if (!IsLoopConnected) return;
        _vertices.RemoveAt(_vertices.Count - 1);

        _combatAreaLineRenderer.positionCount = _vertices.Count;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView currentScene = UnityEditor.SceneView.currentDrawingSceneView;
        if (currentScene)
            currentScene.Repaint();
#endif
        if (SourceTransform)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(SourceTransform.position, 0.25f);
        }

        if (!showRendererBounds) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_combatAreaLineRenderer.bounds.center, _combatAreaLineRenderer.bounds.size);
    }

#if UNITY_EDITOR
    private void DrawHandles(UnityEditor.SceneView sceneView) {
        if (sceneView.camera && _detailCanvas) {
            _detailCanvas.transform.position = _combatAreaLineRenderer.bounds.center + Vector3.up * 2f;
            _detailCanvas.transform.LookAt(sceneView.camera.transform);
            _detailCanvas.transform.Rotate(0f, 180f, 0f);
        }
        
        if (_vertices.Count == 0) return;
        Vector3 normal = Vector3.zero;
        if (_vertices.Count >= 2 && useDirectionVectorForNormal)
            normal = (_vertices[_vertices.Count - 1].position - _vertices[_vertices.Count - 2].position).normalized;
        else
            normal = _vertices[_vertices.Count - 1].forward;

        Quaternion lookRotation = normal.magnitude > Mathf.Epsilon ? Quaternion.LookRotation(normal) : Quaternion.identity;
        UnityEditor.Handles.color = Color.blue;

        Vector3 frontVertexPosition = _vertices[_vertices.Count - 1].position;

        if (!IsLoopConnected)
        {
            if (UnityEditor.Handles.Button(frontVertexPosition, lookRotation, 0.5f, 0.5f, UnityEditor.Handles.ArrowHandleCap))
            {
                GameObject vertex = CreateNewPosition(frontVertexPosition + (normal * lineSegmentLength));
                if (vertexIconTexture)
                    UnityEditor.EditorGUIUtility.SetIconForObject(vertex, vertexIconTexture as Texture2D);
            }
        }

        OnHoverInScene();
    }

    [System.NonSerialized] private int closestMidPointIndex = -1;
    [System.NonSerialized] private float closestDistance = float.MaxValue;
    [System.NonSerialized] private GUIStyle _labelStyle;

    private void OnHoverInScene() {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(UnityEditor.EditorStyles.boldLabel);
            _labelStyle.richText = true;
        }

        Ray worldRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out RaycastHit hit, Mathf.Infinity, 1 << 5, QueryTriggerInteraction.Collide)) {
            if (hit.transform == transform) {
                switch (EditMode) {
                    case CombatAreaEditMode.VertexAdd:
                        OnVertexAddMode(ref hit);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void OnVertexAddMode(ref RaycastHit hit) {
        if (VertexCount < 2) return;
        
        Vector3 hitPosition2D = hit.point;
        hitPosition2D.y = 0f; //Perform the test on a 2D plane.

        for (int v = 1; v < _vertices.Count; ++v)
        {
            Vector3 current = _vertices[v].position;
            Vector3 previous = _vertices[v - 1].position;
            current.y = 0f;
            previous.y = 0f;

            Vector3 midPoint = (previous + current) / 2f;
            float dist = Vector3.Distance(midPoint, hitPosition2D);

            if (showMidpointAtLineSegmentDuringEditMode)
            {
                midPoint.y = _vertices[v].position.y;
                UnityEditor.Handles.color = Color.magenta;
                UnityEditor.Handles.SphereHandleCap(0, midPoint, Quaternion.identity, 0.25f, EventType.Repaint);
            }

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestMidPointIndex = v - 1;
            }
        }

        if (closestMidPointIndex != -1)
        {
            Vector3 current = _vertices[closestMidPointIndex].position;
            Vector3 next = _vertices[closestMidPointIndex + 1].position;
            float yDifference = current.y - next.y;
            current.y = 0f;
            next.y = 0f;

            float currentTime = (current - hitPosition2D).magnitude;
            float maxTime = (next - current).magnitude;

            Vector3 mainPosition = Vector3.Lerp(current, next, currentTime / maxTime);
            UnityEditor.Handles.Label((mainPosition + Vector3.up) + (Vector3.right * 0.75f),
                "<color=cyan><b><size=20>Add Vertex</size></b></color>\n" +
                $"<color=yellow><b><size=18>          {closestMidPointIndex} : {closestMidPointIndex + 1}</size></b></color>"
                , _labelStyle);
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireCube(mainPosition, Vector3.one * 0.5f);

            mainPosition.y = transform.position.y + yDifference;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GameObject insertedVertex = InsertVertex(closestMidPointIndex + 1, mainPosition);
                if (vertexIconTexture)
                    UnityEditor.EditorGUIUtility.SetIconForObject(insertedVertex, vertexIconTexture as Texture2D);
                Event.current.Use();
            }
        }

        closestDistance = float.MaxValue;
    }

    //private void TryInterceptDeleteAction() {
    //    if (UnityEditor.Selection.count == 0) return;
    //    if (IsLoopConnected && Event.current.keyCode == KeyCode.Delete)
    //    {
    //        Event.current.Use();
    //        Debug.LogWarning("You shouldn't delete vertices after the area has been finalised. Disconnect the loop and then try again.");
    //    }
    //}
#endif
}

public enum CombatAreaEditMode { 
    None,
    VertexAdd
}
