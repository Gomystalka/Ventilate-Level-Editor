using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tom.LevelEditor.Runtime.Utility
{
    public static class LevelEditorRuntimeUtility
    {
        public static void SetIconForObject(Object obj, Texture2D icon)
        {
#if UNITY_2021_2_OR_NEWER
            EditorGUIUtility.SetIconForObject(obj, icon);
#else
            //In older Unity versions, this method is not exposed so we need reflection to access it.
            System.Reflection.MethodInfo iconMethod = typeof(EditorGUIUtility).GetMethod("SetIconForObject",
                System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                null, new System.Type[] { typeof(Object), typeof(Texture2D)}, null);

            iconMethod?.Invoke(null, new object[] { obj, icon});      
#endif
        }
    }
}
