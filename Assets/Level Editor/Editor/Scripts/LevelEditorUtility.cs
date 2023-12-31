using UnityEngine;
using UnityEditor;

using Tom.LevelEditor.Editor.EditorWindows;

namespace Tom.LevelEditor.Editor.Utility
{
    public static class LevelEditorUtility
    {
        public const float kIndentSpacePixels = 20f;

        public static string GetScriptableObjectScriptPath(this ScriptableObject scriptableObject)
            => AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(scriptableObject));

        public static Texture2D LoadTexture(this ScriptableObject scriptableObjectOwner, string textureFileName)
        {
            string path = scriptableObjectOwner.GetScriptableObjectScriptPath();
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);

            string spriteDir = fileInfo.Directory.ToString();

            spriteDir = spriteDir.Replace('\\', '/'); //Make sure all slashes are forward slashes for the next step.
            spriteDir = spriteDir.Substring(0, spriteDir.LastIndexOf('/') + 1); //Get the directory before the script folder.
            spriteDir = FileUtil.GetProjectRelativePath(spriteDir);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(System.IO.Path.Combine(spriteDir, "sprites", textureFileName));
        }

        public static bool CheckIfRectIsHoveredAndPassesCondition(Rect rect, bool additionalCondition)
        {
            if (Event.current.type == EventType.Repaint)
                return rect.Contains(Event.current.mousePosition) && additionalCondition;

            return false;
        }

        public static string GetLevelEditorResourcePath(LevelEditorWindow sourceWindow, params string[] additionalPaths)
        {
            string loadPath = sourceWindow.GetScriptableObjectScriptPath();
            if (string.IsNullOrEmpty(loadPath)) return "";
            loadPath = loadPath.Substring(0, loadPath.IndexOf("/Editor/") + 8);
            return loadPath + "/" + System.IO.Path.Combine(additionalPaths);
        }

        public static T LoadResourceForWindowAtPath<T>(LevelEditorWindow sourceWindow, params string[] additionalPaths) where T : Object
            => AssetDatabase.LoadAssetAtPath<T>(GetLevelEditorResourcePath(sourceWindow, additionalPaths));

        public static void IndentedFieldLayout(int indentLevel, System.Action drawAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentLevel * kIndentSpacePixels);
            drawAction?.Invoke();
            GUILayout.EndHorizontal();
        }

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