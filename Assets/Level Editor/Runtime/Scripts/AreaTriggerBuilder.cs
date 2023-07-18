using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTriggerBuilder : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Matrix4x4 matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = matrix;
    }
}
