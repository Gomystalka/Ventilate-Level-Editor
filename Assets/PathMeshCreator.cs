using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathMeshCreator : MonoBehaviour
{
    public Material placedMaterial;
    public Material unplacedMaterial;

    public Color quadColor = Color.red;

    public List<Quad> currentlyDrawnQuads = new List<Quad>(); //Somehow serialize this...

    private void OnDrawGizmos()
    {
        if (!placedMaterial || !unplacedMaterial) return;

        //GL.PushMatrix();
        GL.Begin(GL.QUADS);
        placedMaterial.SetPass(0);
        GL.Color(quadColor);
        foreach (Quad quad in currentlyDrawnQuads)
        {
            quad.GLDraw();
#if UNITY_EDITOR
            //UnityEditor.Handles.ArrowHandleCap(0)
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(quad.Right, 0.1f);
            Gizmos.DrawSphere(quad.Left, 0.1f);
            Gizmos.DrawSphere(quad.Up, 0.1f);
            Gizmos.DrawSphere(quad.Down, 0.1f);

            //if(Event.current.type == EventType.Repaint)
            Gizmos.DrawRay(quad.Up, quad.GetEdgeNormal(Quad.Direction.Up));
            Gizmos.DrawRay(quad.Down, quad.GetEdgeNormal(Quad.Direction.Down));
            Gizmos.DrawRay(quad.Left, quad.GetEdgeNormal(Quad.Direction.Left));
            Gizmos.DrawRay(quad.Right, quad.GetEdgeNormal(Quad.Direction.Right));
 
            //Debug.Log(Vector3.SignedAngle(quad.Right, quad.Vertices[0].position, Vector3.forward));
            
            //UnityEditor.Handles.ArrowHandleCap(0, quad.Up, Quaternion.LookRotation(quad.UpperEdgeNormal), 1f, EventType.Repaint);
#endif
        }
        GL.End();
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

    public void DestroyAllQuads() {
        foreach (Quad quad in currentlyDrawnQuads)
            quad.Destroy();

        currentlyDrawnQuads.Clear();
    }
}

public class Quad {
    public Transform[] Vertices { get; private set; } = new Transform[4];

    public Vector3 Up => (Vertices[0].position + Vertices[1].position) / 2f;
    public Vector3 Down => (Vertices[2].position + Vertices[3].position) / 2f;
    public Vector3 Right => (Vertices[1].position + Vertices[2].position) / 2f;
    public Vector3 Left => (Vertices[0].position + Vertices[3].position) / 2f;

    //public Vector3 UpperEdgeNormal => Vector3.

    public static Quad CreateQuad(Vector3 startCornerPosition, float quadSizeUnits = 1f, int quadIndex = 0) {
        Quad newQuad = new Quad();
        
        GameObject vertex1 = new GameObject($"Vertex1:{quadIndex}");
        vertex1.transform.position = startCornerPosition;

        GameObject vertex2 = new GameObject($"Vertex2:{quadIndex}");
        vertex2.transform.position = new Vector3(startCornerPosition.x + quadSizeUnits, startCornerPosition.y, startCornerPosition.z);

        GameObject vertex3 = new GameObject($"Vertex3:{quadIndex}");
        vertex3.transform.position = new Vector3(startCornerPosition.x + quadSizeUnits, startCornerPosition.y, startCornerPosition.z - quadSizeUnits);

        GameObject vertex4 = new GameObject($"Vertex4:{quadIndex}");
        vertex4.transform.position = new Vector3(startCornerPosition.x, startCornerPosition.y, startCornerPosition.z - quadSizeUnits);

        newQuad.Vertices[0] = vertex1.transform;
        newQuad.Vertices[1] = vertex2.transform;
        newQuad.Vertices[2] = vertex3.transform;
        newQuad.Vertices[3] = vertex4.transform;

#if UNITY_EDITOR
        Texture vertexIcon = UnityEditor.EditorGUIUtility.IconContent("blendKey").image;
        UnityEditor.EditorGUIUtility.SetIconForObject(vertex1, vertexIcon as Texture2D);
        UnityEditor.EditorGUIUtility.SetIconForObject(vertex2, vertexIcon as Texture2D);
        UnityEditor.EditorGUIUtility.SetIconForObject(vertex3, vertexIcon as Texture2D);
        UnityEditor.EditorGUIUtility.SetIconForObject(vertex4, vertexIcon as Texture2D);
#endif

        return newQuad;
    }

    public void SetParent(Transform parent, bool worldPositionStays = true) {
        Vertices[0].SetParent(parent, worldPositionStays);
        Vertices[1].SetParent(parent, worldPositionStays);
        Vertices[2].SetParent(parent, worldPositionStays);
        Vertices[3].SetParent(parent, worldPositionStays);
    }

    public void Destroy() { //Removes the vertices from the scene
        for(int v = 0; v < Vertices.Length; ++v)
        {
            Transform vertex = Vertices[v];
            if (Application.isPlaying)
                Object.Destroy(vertex.gameObject);
            else
            {
#if UNITY_EDITOR
                //UnityEditor.Undo.DestroyObjectImmediate(vertex.gameObject); //Add Undo later.
                Object.DestroyImmediate(vertex.gameObject);
#endif
            }
            Vertices[v] = null;
        }
    }

    public void GLDraw() {
        GL.Vertex(Vertices[0].position);
        GL.Vertex(Vertices[1].position);
        GL.Vertex(Vertices[2].position);
        GL.Vertex(Vertices[3].position);
    }

    public Vector3 GetEdgeNormal(Direction direction) {
        Vector3 dir = Vector3.zero;
        switch (direction) {
            case Direction.Up:
                dir = Vertices[1].position - Vertices[0].position;
                break;
            case Direction.Down:
                dir = Vertices[3].position - Vertices[2].position;
                break;
            case Direction.Left:
                dir = Vertices[0].position - Vertices[3].position;
                break;
            case Direction.Right:
                dir = Vertices[2].position - Vertices[1].position;
                break;
        }
        dir.Set(-dir.z, 0f, dir.x);
        return dir.normalized;
    }

    public enum Direction : byte
    {
        Up,
        Down,
        Left,
        Right
    }
}
