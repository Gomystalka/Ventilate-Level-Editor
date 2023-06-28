using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public static Vertex CurrentlySelectedVertex { get; set; } = null;
    public Quad Owner { get; private set; }

    private List<VertexConnection> _connections = new List<VertexConnection>();
    public List<VertexConnection> Connections => _connections;

    public byte VertexIndex { get; private set; } = 0;

    public Vector3 Position => transform.position;

    private void OnDrawGizmos()
    {
        CheckForVertexSelection();

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }

    public Vertex Setup(Quad ownerQuad, byte vertexIndex)
    {
        Owner = ownerQuad;
        VertexIndex = vertexIndex;

        return this;
    }

    public void SetParent(Transform parent, bool worldPositionStays = true)
        => transform.SetParent(parent, worldPositionStays);

    public void Merge(Vertex vertexToMerge)
    {
        foreach (VertexConnection connection in _connections)
        {
            vertexToMerge.CreateConnection(connection.quad, connection.vertexIndex);
            connection.quad.Vertices[connection.vertexIndex] = vertexToMerge;
        }

        Owner.Vertices[VertexIndex] = vertexToMerge;
        vertexToMerge.CreateConnection(Owner, VertexIndex);

        if (Application.isPlaying)
            Destroy(gameObject);
        else
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#endif
        }
    }

    public void Disconnect() {
        if (Connections.Count == 0) return;
        foreach (VertexConnection connection in _connections)
        {
            GameObject vertex = new GameObject($"Vertex{connection.vertexIndex}:{connection.quad.QuadIndex}");
            vertex.transform.position = Position;

            Vertex vertexObject = vertex.AddComponent<Vertex>()
                .Setup(connection.quad, connection.vertexIndex);
            vertexObject.SetParent(connection.quad.Parent);
            connection.quad.Vertices[connection.vertexIndex] = vertexObject;
            //Create new vertices - DONE
            //Assign new vertices to source quads - DONE
            //Translate slightly to avoid overlap
            //Clear connections - DONE
            //Destroy Vertex - DONE
        }
        _connections.Clear();
        if (Application.isPlaying)
            Destroy(gameObject);
        else
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#endif
        }
    }

    public void CreateConnection(Quad connectionSourceQuad, byte connectionSourceVertexIndex) {
        VertexConnection connection = new VertexConnection() { 
            quad = connectionSourceQuad,
            vertexIndex = connectionSourceVertexIndex
        };

        if(!_connections.Contains(connection)) //This'll correctly identify duplicates due to overloaded equal operators.
            _connections.Add(connection);
    }

    private GameObject _lastSelectedObject = null;

    private void CheckForVertexSelection()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject != _lastSelectedObject)
        {
            _lastSelectedObject = UnityEditor.Selection.activeGameObject;
            if (_lastSelectedObject == gameObject)
            {
                CurrentlySelectedVertex = this;
            }

            if (_lastSelectedObject == null)
            {
                CurrentlySelectedVertex = null;
                UnityEditor.SceneView.RepaintAll();
            }
        }
#endif
    }
}

public struct VertexConnection {
    public Quad quad;
    public byte vertexIndex;

    public override bool Equals(object obj)
    {
        if (obj is VertexConnection connection)
            return this == connection;
        else
            return false;
    }

    public override int GetHashCode()
        => quad.GetHashCode() ^ vertexIndex.GetHashCode();

    public static bool operator ==(VertexConnection c1, VertexConnection c2)
        => c1.quad == c2.quad && c1.vertexIndex == c2.vertexIndex;

    public static bool operator !=(VertexConnection c1, VertexConnection c2)
        => c1.quad != c2.quad || c1.vertexIndex != c2.vertexIndex;

    public override string ToString()
        => $"Quad:{quad.QuadIndex} | Vertex: {vertexIndex}";
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Vertex))]
public class VertexInspector : UnityEditor.Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Vertex v = (Vertex)target;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (VertexConnection connection in v.Connections)
            sb.AppendLine(connection.ToString());

        UnityEditor.EditorGUILayout.HelpBox($"Connections:{System.Environment.NewLine}{sb}", UnityEditor.MessageType.None, true);

        sb.Length = 0;
        sb.Capacity = 0;
    }
}
#endif
