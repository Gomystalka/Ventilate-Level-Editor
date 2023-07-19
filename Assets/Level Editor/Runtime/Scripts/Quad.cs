using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tom.LevelEditor.Runtime.PathMeshEditor
{
    public class Quad
    {
        //TO-DO
        //-Properly lock directional buttons (NSY)
        //-Save to mesh - DONE
        //  -Every vertex has unique ID. Connected vertices share IDs. Creating new Vertex will assign a new ID. Vector3 Position to index map? - DONE
        //  -Quads into triangles - DONE
        //  -Draw in order of creation. - DONE
        public Vertex[] Vertices { get; private set; } = new Vertex[4];
        //public Quad[] VertexOwners { get; private set; } = new Quad[4];

        public Vector3 Up => (Vertices[1].Position + Vertices[2].Position) / 2f;
        public Vector3 Down => (Vertices[0].Position + Vertices[3].Position) / 2f;
        public Vector3 Left => (Vertices[0].Position + Vertices[1].Position) / 2f;
        public Vector3 Right => (Vertices[2].Position + Vertices[3].Position) / 2f;
        public Vector3 Position => Vertices[0].Position;

        public Transform Parent { get; set; }

        //public bool[] DirectionLocks = new bool[4];

        public int QuadIndex { get; set; } = 0;

        public bool IsSelected //lol
        {
            //get => Vertex.CurrentlySelectedVertex == Vertices[0] ||
            //Vertex.CurrentlySelectedVertex == Vertices[1] ||
            //Vertex.CurrentlySelectedVertex == Vertices[2] ||
            //Vertex.CurrentlySelectedVertex == Vertices[3];
#if UNITY_EDITOR
            get => UnityEditor.Selection.Contains(Vertices[0].gameObject) ||
            UnityEditor.Selection.Contains(Vertices[1].gameObject) ||
            UnityEditor.Selection.Contains(Vertices[2].gameObject) ||
            UnityEditor.Selection.Contains(Vertices[3].gameObject);
#else
        get => false;
#endif
        }

        //Used for deserialization so I can keep the QuadIndex property's setter private.
        public static Quad CreateEmptyIndexedQuad(int quadIndex)
        {
            return new Quad()
            {
                QuadIndex = quadIndex,
                Vertices = new Vertex[4]
            };
        }

        public static Quad CreateQuad(Vector3 startCornerPosition, float quadSizeUnits = 1f, int quadIndex = 0)
        {
            Quad newQuad = new Quad();
            newQuad.QuadIndex = quadIndex;

            GameObject vertex1 = new GameObject($"Vertex0:{quadIndex}");
            //vertex1.transform.position = new Vector3(startCornerPosition.x, startCornerPosition.y, startCornerPosition.z + quadSizeUnits);
            vertex1.transform.position = new Vector3(startCornerPosition.x, startCornerPosition.y, startCornerPosition.z); //v4
            vertex1.transform.localScale = Vector3.one * quadIndex;

            GameObject vertex2 = new GameObject($"Vertex1:{quadIndex}");
            //vertex2.transform.position = new Vector3(startCornerPosition.x + quadSizeUnits, startCornerPosition.y, startCornerPosition.z + quadSizeUnits);
            vertex2.transform.position = new Vector3(startCornerPosition.x, startCornerPosition.y, startCornerPosition.z + quadSizeUnits); //v1
            vertex2.transform.localScale = Vector3.one * quadIndex;

            GameObject vertex3 = new GameObject($"Vertex2:{quadIndex}");
            //vertex3.transform.position = new Vector3(startCornerPosition.x + quadSizeUnits, startCornerPosition.y, startCornerPosition.z);
            vertex3.transform.position = new Vector3(startCornerPosition.x + quadSizeUnits, startCornerPosition.y, startCornerPosition.z + quadSizeUnits); //v2
            vertex3.transform.localScale = Vector3.one * quadIndex;

            GameObject vertex4 = new GameObject($"Vertex3:{quadIndex}");
            //vertex4.transform.position = new Vector3(startCornerPosition.x, startCornerPosition.y, startCornerPosition.z);
            vertex4.transform.position = new Vector3(startCornerPosition.x + quadSizeUnits, startCornerPosition.y, startCornerPosition.z); //v3
            vertex4.transform.localScale = Vector3.one * quadIndex;

            newQuad.Vertices[0] = vertex1.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 0);
            newQuad.Vertices[1] = vertex2.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 1);
            newQuad.Vertices[2] = vertex3.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 2);
            newQuad.Vertices[3] = vertex4.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 3);

#if UNITY_EDITOR
            //Texture vertexIcon = UnityEditor.EditorGUIUtility.IconContent("blendKey").image;
            //UnityEditor.EditorGUIUtility.SetIconForObject(vertex1, vertexIcon as Texture2D);
            //UnityEditor.EditorGUIUtility.SetIconForObject(vertex2, vertexIcon as Texture2D);
            //UnityEditor.EditorGUIUtility.SetIconForObject(vertex3, vertexIcon as Texture2D);
            //UnityEditor.EditorGUIUtility.SetIconForObject(vertex4, vertexIcon as Texture2D);
#endif

            return newQuad;
        }

        //Finish this. Needs to create a quad in the specified direction based on the source quad's vertices.
        public static Quad CreateConnectedQuad(ref Quad sourceQuad, Direction connectionDirection, float quadSizeUnits = 1f, int quadIndex = 0)
        {
            Quad newQuad = new Quad();
            newQuad.QuadIndex = quadIndex;
            GameObject vertex0 = new GameObject($"Vertex0:{quadIndex}");
            GameObject vertex1 = new GameObject($"Vertex1:{quadIndex}");
            GameObject vertex2 = new GameObject($"Vertex2:{quadIndex}");
            GameObject vertex3 = new GameObject($"Vertex3:{quadIndex}");

            Vector3 normal = sourceQuad.GetEdgeNormal(connectionDirection) * quadSizeUnits;
            switch (connectionDirection)
            {
                case Direction.Up:
                    //Set up bottom vertices.
                    vertex0.transform.position = sourceQuad.Vertices[1].Position;
                    vertex3.transform.position = sourceQuad.Vertices[2].Position;

                    //Translate bottom vertices along edge normal by [quadSizeUnits] units.
                    vertex1.transform.position = vertex0.transform.position + normal;
                    vertex2.transform.position = vertex3.transform.position + normal;
                    break;
                case Direction.Down:
                    vertex1.transform.position = sourceQuad.Vertices[0].Position;
                    vertex2.transform.position = sourceQuad.Vertices[3].Position;

                    vertex0.transform.position = vertex1.transform.position + normal;
                    vertex3.transform.position = vertex2.transform.position + normal;
                    break;
                case Direction.Left:
                    vertex2.transform.position = sourceQuad.Vertices[1].Position;
                    vertex3.transform.position = sourceQuad.Vertices[0].Position;

                    vertex1.transform.position = vertex2.transform.position + normal;
                    vertex0.transform.position = vertex3.transform.position + normal;
                    break;
                case Direction.Right:
                    vertex1.transform.position = sourceQuad.Vertices[2].Position;
                    vertex0.transform.position = sourceQuad.Vertices[3].Position;

                    vertex2.transform.position = vertex1.transform.position + normal;
                    vertex3.transform.position = vertex0.transform.position + normal;
                    break;
                default:
                    break;
            }

            newQuad.Vertices[0] = vertex0.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 0);
            newQuad.Vertices[1] = vertex1.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 1);
            newQuad.Vertices[2] = vertex2.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 2);
            newQuad.Vertices[3] = vertex3.AddComponent<Vertex>()
                .Setup(ownerQuad: newQuad, vertexIndex: 3);

            return newQuad;
        }

        public void SetParent(Transform parent, bool worldPositionStays = true)
        {
            Parent = parent;
            Vertices[0].SetParent(parent, worldPositionStays);
            Vertices[1].SetParent(parent, worldPositionStays);
            Vertices[2].SetParent(parent, worldPositionStays);
            Vertices[3].SetParent(parent, worldPositionStays);
        }

        public void Destroy(bool checkForConnections)
        { //Removes the vertices from the scene
            for (int v = 0; v < Vertices.Length; ++v)
            {
                Vertex vertex = Vertices[v];
                if (!vertex) continue; //If a vertex has 0 or 1 connections, it's not connected to anything so it can be safely removed.

                if (checkForConnections && vertex.Connections.Count > 1) continue; //This part needs an ownership check
                if (Application.isPlaying)
                    Object.Destroy(vertex.gameObject);
                else
                {
#if UNITY_EDITOR
                    //UnityEditor.Undo.DestroyObjectImmediate(vertex.gameObject); //Add Undo later.
                    Object.DestroyImmediate(vertex.gameObject);
#endif
                }
            }
            //Delete first, then break connections for any vertices that are still alive.

            for (int v = 0; v < Vertices.Length; ++v)
            {
                Vertex vertex = Vertices[v];
                if (!vertex) continue;

                vertex.BreakConnectionWithQuad(this);
                Vertices[v] = null;
            }
        }

        public void GLDraw()
        {

            for (int i = 0; i < 4; ++i)
            {
                if (!Vertices[i])
                {
                    //Debug.Log($"Vertex: {i} is NULL");
                    continue;
                }
                if (IsSelected)
                    GL.Color(Color.yellow);
                GL.Vertex(Vertices[i].Position);
            }
            //GL.Vertex(Vertices[0].Position);
            //GL.Vertex(Vertices[1].Position);
            //GL.Vertex(Vertices[2].Position);
            //GL.Vertex(Vertices[3].Position);
        }

        public void GLDrawTriangles()
        {
            if (IsSelected)
                GL.Color(Color.yellow);

            //GL.Vertex(0, 1, 2);
            //GL.Vertex(2, 3, 0);
            GL.Vertex(Vertices[0].Position);
            GL.Vertex(Vertices[1].Position);
            GL.Vertex(Vertices[2].Position);
            GL.Vertex(Vertices[2].Position);
            GL.Vertex(Vertices[3].Position);
            GL.Vertex(Vertices[0].Position);
        }

        public Vector3 GetEdgeNormal(Direction direction)
        {
            Vector3 dir = Vector3.zero;
            switch (direction)
            {
                case Direction.Up:
                    dir = Vertices[2].Position - Vertices[1].Position;
                    break;
                case Direction.Down:
                    dir = Vertices[0].Position - Vertices[3].Position;
                    break;
                case Direction.Left:
                    dir = Vertices[1].Position - Vertices[0].Position;
                    break;
                case Direction.Right:
                    dir = Vertices[3].Position - Vertices[2].Position;
                    break;
            }
            dir.Set(-dir.z, 0f, dir.x);
            return dir.normalized;
        }

        public Vector3 GetEdge(Direction edgeDirection)
        {
            switch (edgeDirection)
            {
                case Direction.Up:
                    return Up;
                case Direction.Down:
                    return Down;
                case Direction.Left:
                    return Left;
                case Direction.Right:
                    return Right;
                default:
                    return Position;
            }
        }

        public static Direction GetInvertedDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default:
                    return Direction.None;
            }
        }

        public void ConnectQuad(ref Quad quad, Direction connectionSourceDirection)
        { //Example: If direction is UP, the connected quad will be connected from its bottom vertices to the top.   
            switch (connectionSourceDirection)
            {
                case Direction.Up:
                    quad.DestroyVertices(0, 3);
                    //quad.SetVertexOwnership(this, 2, 3);
                    quad.Vertices[0] = Vertices[1]; //Connect bottom right to top right
                    quad.Vertices[3] = Vertices[2]; //Connect bottom left to top left

                    Vertices[2].CreateConnection(this, 2);
                    Vertices[2].CreateConnection(quad, 3);

                    Vertices[1].CreateConnection(this, 1);
                    Vertices[1].CreateConnection(quad, 0);
                    break;
                case Direction.Down:
                    quad.DestroyVertices(1, 2);
                    //quad.SetVertexOwnership(this, 1, 0);
                    quad.Vertices[1] = Vertices[0];
                    quad.Vertices[2] = Vertices[3];

                    Vertices[3].CreateConnection(this, 3);
                    Vertices[3].CreateConnection(quad, 2);

                    Vertices[0].CreateConnection(this, 0);
                    Vertices[0].CreateConnection(quad, 1);
                    break;
                case Direction.Left:
                    quad.DestroyVertices(2, 3);
                    //quad.SetVertexOwnership(this, 1, 2);
                    quad.Vertices[2] = Vertices[1];
                    quad.Vertices[3] = Vertices[0];

                    Vertices[0].CreateConnection(this, 0);
                    Vertices[0].CreateConnection(quad, 3);

                    Vertices[1].CreateConnection(this, 1);
                    Vertices[1].CreateConnection(quad, 2);
                    break;
                case Direction.Right:
                    quad.DestroyVertices(0, 1);
                    //quad.SetVertexOwnership(this, 0, 3);
                    quad.Vertices[0] = Vertices[3];
                    quad.Vertices[1] = Vertices[2];

                    Vertices[2].CreateConnection(this, 2);
                    Vertices[2].CreateConnection(quad, 1);

                    Vertices[3].CreateConnection(this, 3);
                    Vertices[3].CreateConnection(quad, 0);
                    break;
            }
        }

        //private void SetVertexOwnership(Quad quadOwner, params int[] vertexIndices) {
        //    for (int i = 0; i < vertexIndices.Length; i++) {
        //        if (vertexIndices[i] < 4 && vertexIndices[i] >= 0)
        //            VertexOwners[vertexIndices[i]] = quadOwner;
        //    }
        //}

        // -----------CORRECT-----------
        //           1--------2
        //           |        |
        //           |        |
        //           |        |
        //           0--------3
        //1--------2 1--------2 1--------2
        //|        | |        | |        |
        //|        | |        | |        |
        //|        | |        | |        |
        //0--------3 0--------3 0--------3
        //           1--------2
        //           |        |
        //           |        |
        //           |        |
        //           0--------3

        //   ---------INCORRECT-----------
        //           0--------1
        //           |        |
        //           |        |
        //           |        |
        //           3--------2
        //0--------1 0--------1 0--------1
        //|        | |        | |        |
        //|        | |        | |        |
        //|        | |        | |        |
        //3--------2 3--------2 3--------2
        //           0--------1
        //           |        |
        //           |        |
        //           |        |
        //           3--------2

        public void GenerateSelfConnections()
        {
            for (byte vertexIndex = 0; vertexIndex < 4; ++vertexIndex)
                Vertices[vertexIndex].CreateConnection(this, vertexIndex);
        }

        public void DestroyVertices(params int[] vertexIndices)
        { //Implement ownership check

            for (int i = 0; i < vertexIndices.Length; i++)
            {
                if (Application.isPlaying)
                    Object.Destroy(Vertices[vertexIndices[i]].gameObject);
                else
                    Object.DestroyImmediate(Vertices[vertexIndices[i]].gameObject);
            }
        }

        public void SetVertexSpace(bool isLocalSpace)
        {
            Quaternion rotation = Quaternion.LookRotation(GetEdgeNormal(Direction.Up));
            if (!isLocalSpace)
                rotation = Quaternion.identity;

            foreach (Vertex v in Vertices)
                v.transform.rotation = rotation;
        }

        public enum Direction : byte
        {
            None,
            Up,
            Down,
            Left,
            Right
        }
    }
}