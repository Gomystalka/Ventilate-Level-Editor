using UnityEngine;
using UnityEditor;

public static class LevelEditorUtility
{
    public static string GetScriptableObjectScriptPath(this ScriptableObject scriptableObject)
        => AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(scriptableObject));

    public static Texture2D LoadTexture(this ScriptableObject scriptableObjectOwner, string textureFileName) {
        string path = scriptableObjectOwner.GetScriptableObjectScriptPath();
        System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);

        string spriteDir = fileInfo.Directory.ToString();

        spriteDir = spriteDir.Replace('\\', '/'); //Make sure all slashes are forward slashes for the next step.
        spriteDir = spriteDir.Substring(0, spriteDir.LastIndexOf('/') + 1); //Get the directory before the script folder.
        return AssetDatabase.LoadAssetAtPath<Texture2D>(System.IO.Path.Combine(spriteDir, "sprites", textureFileName));
    }

    public static bool CheckIfRectIsHoveredAndPassesCondition(Rect rect, bool additionalCondition) {
        if (Event.current.type == EventType.Repaint)
            return rect.Contains(Event.current.mousePosition) && additionalCondition;

        return false;
    }
}
