#define AMOGUS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tom.LevelEditor.Runtime.CombatAreaEditor
{
    public class AreaTriggerBuilder : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isPlaying)
                Destroy(this);
        }
        private void OnDrawGizmos()
        {
            Color c = Color.yellow;
            Gizmos.color = c;
            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            c.a = 0.2f;
            Gizmos.color = c;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = matrix;
        }

        public void CreateCollider()
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
}

#if UNITY_EDITOR
namespace Tom.LevelEditor.Editor
{
    [CustomEditor(typeof(Runtime.CombatAreaEditor.AreaTriggerBuilder))]
    public class AreaTriggerBuilderInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Runtime.CombatAreaEditor.AreaTriggerBuilder atb = (Runtime.CombatAreaEditor.AreaTriggerBuilder)target;

            if (GUILayout.Button("Create Collider"))
                atb.CreateCollider();
        }
    }
}
#endif
