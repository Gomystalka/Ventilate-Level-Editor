using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer), typeof(BoxCollider))]
public class CombatAreaCreator : MonoBehaviour
{
    //TO-DO
    // -Add support for multiple combat areas.
    // -Make the direction toggle actually work.
    //   -Reorderable list of all CombatAreaCreators in the scene perhaps?
    // -Add support for creating vertices between existing vertices.
    // -Add finalise button which connects the last point to the source point, thus finishing the combat area.
    // -Add Enemy display UI in scene view above the area.
    // -Add point overlap detection? (Maybe)
    // -Make all object names unique.

    public static Texture vertexIconTexture;

    private LineRenderer _combatAreaLineRenderer;
    private BoxCollider _collider;
    public Transform SourceTransform { get; set; }

    /*[HideInInspector] */[SerializeField] private List<Transform> _vertices;

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

    private void SetPositions() {
        DetectDeletedVertices();

        for (int p = 0; p < _vertices.Count; ++p)
            _combatAreaLineRenderer.SetPosition(p, _vertices[p].transform.position);
    }

    private void SetColliderBounds() {
        Vector3 size = _combatAreaLineRenderer.bounds.size;
        size.y = 0.1f;
        _collider.size = size;
        _collider.center = _combatAreaLineRenderer.bounds.center;
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView currentScene = UnityEditor.SceneView.currentDrawingSceneView;
        if (currentScene)
            currentScene.Repaint();
#endif

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_combatAreaLineRenderer.bounds.center, _combatAreaLineRenderer.bounds.size);
    }

#if UNITY_EDITOR
    private void DrawHandles(UnityEditor.SceneView sceneView) {
        if (_vertices.Count == 0) return;
        Vector3 normal = Vector3.zero;
        if (_vertices.Count >= 2)
            normal = (_vertices[_vertices.Count - 1].position - _vertices[_vertices.Count - 2].position).normalized;
        else
            normal = SourceTransform.forward;

        Quaternion lookRotation = normal.magnitude > Mathf.Epsilon ? Quaternion.LookRotation(normal) : Quaternion.identity;
        UnityEditor.Handles.color = Color.blue;

        Vector3 frontVertexPosition = _vertices[_vertices.Count - 1].position;

        if (UnityEditor.Handles.Button(frontVertexPosition, lookRotation, 0.5f, 0.5f, UnityEditor.Handles.ArrowHandleCap))
        {
            GameObject vertex = CreateNewPosition(frontVertexPosition + normal);
            if(vertexIconTexture)
                UnityEditor.EditorGUIUtility.SetIconForObject(vertex, vertexIconTexture as Texture2D);
        }
        OnHoverInScene();
    }

    int closestVertexindex = -1;
    float closestDistance = float.MaxValue;

    private void OnHoverInScene() {
        Ray worldRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out RaycastHit hit, Mathf.Infinity, 1 << 5, QueryTriggerInteraction.Collide)) {
            if (hit.transform == transform) {
                Vector3 hitPosition2D = hit.point;
                hitPosition2D.y = 0f; //Perform the test on a 2D plane.

                for(int v = 1; v < _vertices.Count; ++v) {
                    Vector3 current = _vertices[v].position;
                    Vector3 previous = _vertices[v - 1].position;
                    current.y = 0f;
                    previous.y = 0f;

                    //if (Physics.CheckBox(previous, Vector3.one * 0.1f))
                    //{
                    //    closestVertexindex = v - 1;
                    //    break;
                    //}
                    closestVertexindex = 0;
                    closestDistance = float.MaxValue;

                    float dist = Vector3.Distance(previous, hitPosition2D);
                    if (dist < closestDistance)
                    {
                        //if (closestVertexindex == v + 1)
                        //    break;

                        closestDistance = dist;
                        closestVertexindex = v - 1;
                    }
                }

                if (closestVertexindex != -1)
                {
                    Vector3 current = _vertices[closestVertexindex].position;
                    current.y = 0f;
                    Vector3 next = _vertices[closestVertexindex + 1].position;
                    next.y = 0f;

                    Vector3 mainPosition = Vector3.Lerp(current, next, (current - hitPosition2D).magnitude / (next - current).magnitude);
                    UnityEditor.Handles.color = Color.cyan;
                    UnityEditor.Handles.DrawWireCube(mainPosition, Vector3.one * 0.5f);
                }
            }
        }
    }
#endif
}
