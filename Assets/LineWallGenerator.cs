using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LineRenderer))]
public class LineWallGenerator : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Transform _optionalParent;
    [SerializeField] private float _wallHeight = 1f;
    [SerializeField] private Vector3 _wallPositionOffset;
    [SerializeField] private float _colliderHeight = 10f;
    [SerializeField] private Vector3 _colliderCenterOffset;
    [SerializeField] private float _wallThickness = 1f;
    [SerializeField] private float _cornerThickness = 1f;
    [SerializeField] private bool _instantiateInCenterOfLineSegment = true;

    private void OnEnable()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void GenerateWalls() {
        if(!_lineRenderer)
            _lineRenderer = GetComponent<LineRenderer>();

        //if (_lineRenderer.positionCount % 2 != 0) { //Wait, there's no reason for this restriction to be in place...
        //    Debug.LogError("Failed to generate walls. The number of vertices must be even!");
        //    return;
        //}
        if (_lineRenderer.GetPosition(0) != _lineRenderer.GetPosition(_lineRenderer.positionCount - 1))
        {
            Debug.LogError("Failed to generate walls. A loop must be completed in order to generate the zone!");
            return;
        }
        for (int w = 0; w < _lineRenderer.positionCount - 1; ++w) {
            Vector3 currentVertex = _lineRenderer.GetPosition(w) + _wallPositionOffset;
            Vector3 nextVertex = _lineRenderer.GetPosition(w + 1) + _wallPositionOffset;
            Vector3 scale = new Vector3(_cornerThickness, _wallHeight / 2f, _cornerThickness);
            Vector3 direction = nextVertex - currentVertex;
            Vector3 midPoint = (currentVertex + nextVertex) / 2f;

            GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            corner.transform.localScale = scale;
            corner.transform.position = currentVertex;
            corner.name = "Corner";

            scale.x = _wallThickness;
            scale.y = _wallHeight;
            scale.z = direction.magnitude; //Length of the wall.

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetPositionAndRotation(midPoint, 
                Quaternion.LookRotation(direction));
            wall.transform.localScale = scale;
            wall.name = "Wall";

            corner.transform.SetParent(_optionalParent ? _optionalParent : transform);
            wall.transform.SetParent(_optionalParent ? _optionalParent : transform);

            BoxCollider wallCollider = wall.GetComponent<BoxCollider>();
            CapsuleCollider cornerCollider = corner.GetComponent<CapsuleCollider>();

            wallCollider.center = _colliderCenterOffset;
            wallCollider.size = new Vector3(wallCollider.size.x, _colliderHeight, wallCollider.size.z);

            cornerCollider.center = _colliderCenterOffset * 2f;
            cornerCollider.height = _colliderHeight * 2f;

            if (!_instantiateInCenterOfLineSegment)
            {
                corner.transform.position += Vector3.up * (_wallHeight / 2f);
                wall.transform.position += Vector3.up * (_wallHeight / 2f);
            }

#if UNITY_EDITOR //This makes the Undos happen in the Unitysssss. Yes.
            Undo.RegisterCreatedObjectUndo(corner, "Created Line Wall Corner");
            Undo.RegisterCreatedObjectUndo(wall, "Created Line Wall");
#endif
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LineWallGenerator))]
public class LineWallGeneratorInspector : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
            ((LineWallGenerator)target).GenerateWalls();
    }
}
#endif
