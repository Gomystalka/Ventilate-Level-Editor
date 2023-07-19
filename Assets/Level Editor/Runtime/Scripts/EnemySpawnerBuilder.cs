using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tom.LevelEditor.Runtime.CombatAreaEditor
{
    public class EnemySpawnerBuilder : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isPlaying)
                Destroy(this);
        }

        private void OnDrawGizmos()
        {
            Color c = Color.red;
            Gizmos.color = c;
            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            c.a = 0.2f;
            Gizmos.color = c;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = matrix;
        }
    }
}