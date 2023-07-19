using UnityEngine;

namespace Tom.LevelEditor.Editor.EditorWindows
{
    public interface ILevelEditorWindow
    {
        string Title { get; }
        Texture2D Icon { get; set; }
        byte Order { get; }
        LevelEditorWindow OwnerWindow { get; set; }
        void OnGUI();
        void OnEnable();
        void OnDisable();
        void OnDestroy();
        void OnWindowOpened();
        void OnWindowClosed();
        GUIContent GetTitleContent();
    }
}
