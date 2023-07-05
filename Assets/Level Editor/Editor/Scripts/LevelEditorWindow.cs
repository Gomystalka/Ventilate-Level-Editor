using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class LevelEditorWindow : EditorWindow
{
    //TO-DO
    //Asset importer for the Asset Data Scriptable Object with category sorting. - DONE
    //Unload button for asset data. - DONE
    //Path mesh generator. - DONE SOMEHOW?! 天才か？！？
    //Combat zone creator. (Maji Yabe)
    //Idea -> ILevelEditorPage interface for handling other parts of the editor Path mesh gen, Combat zone creator, etc. - DONE

    private static LevelEditorWindow _editorWindowInstance;

    public Vector2 Position => position.position;
    public Vector2 Size => position.size;

    private static List<ILevelEditorWindow> _editorWindows = new List<ILevelEditorWindow>();
    private static int _currentEditorWindowIndex = 0;

    private static string[] _availableEditors = new string[0];

    [MenuItem("Tom's Level Editor/Editor")]
    static void Init() {
        _editorWindowInstance = GetWindow<LevelEditorWindow>();
        _editorWindowInstance.titleContent = new GUIContent("Level Editor", EditorGUIUtility.IconContent("Terrain Icon").image);
        _editorWindows.Clear();
        _editorWindowInstance.FindAllEditorWindows();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        int lastEditorWindowIndex = _currentEditorWindowIndex;
        _currentEditorWindowIndex = GUILayout.Toolbar(_currentEditorWindowIndex, _availableEditors);
        if (EditorGUI.EndChangeCheck())
        {
            _editorWindows[lastEditorWindowIndex].OnWindowClosed();
            _editorWindows[_currentEditorWindowIndex].OnWindowOpened();
        }

        _editorWindows[_currentEditorWindowIndex].OnGUI();

        wantsMouseMove = true;
        if (Event.current.type == EventType.MouseMove) //This is required for the GUI to be always updated.
            Repaint();
    }

    private void OnEnable()
    {
        if (_editorWindows.Count == 0)
            FindAllEditorWindows();

        foreach (ILevelEditorWindow editorWindow in _editorWindows)
            editorWindow.OnEnable();
    }

    private void OnDisable()
    {
        foreach (ILevelEditorWindow editorWindow in _editorWindows)
            editorWindow.OnDisable();
    }

    private void OnDestroy()
    {
        foreach (ILevelEditorWindow editorWindow in _editorWindows)
            editorWindow.OnDestroy();

        _editorWindowInstance = null;
    }

    private void FindAllEditorWindows() {
        System.Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        List<string> availableEditorsTemp = new List<string>();

        foreach (System.Type type in types) {
            bool isEditorWindowType = typeof(ILevelEditorWindow).IsAssignableFrom(type) && !type.IsInterface;
            if (isEditorWindowType)
            {
                var editorWindowInstance = (ILevelEditorWindow)System.Activator.CreateInstance(type);
                editorWindowInstance.OwnerWindow = this;
                _editorWindows.Add(editorWindowInstance);
            }
        }

        _editorWindows.Sort((x, y) =>
        {
            if (x.Order < y.Order) return -1;
            if (x.Order > y.Order) return 1;
            return 0;
        });

        foreach (ILevelEditorWindow window in _editorWindows) //This is the safest approach as the list is guaranteed to be sorted here.
            availableEditorsTemp.Add(window.Title);

        _availableEditors = availableEditorsTemp.ToArray();
    }
}
