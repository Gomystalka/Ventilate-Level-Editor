using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathMeshEditorWindow : ILevelEditorWindow
{
    private const string kPathMeshEditorGameObjectName = "SceneObject:PathMeshEditor";

    public string Title => "Path Mesh Editor";

    public Texture2D Icon { get; set; }

    public byte Order => 1;

    public LevelEditorWindow OwnerWindow { get; set; }

    private static PathMeshCreator _pathMeshCreatorSceneReference;

    public GUIContent GetTitleContent()
    {
        return GUIContent.none;
    }

    public void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(!_pathMeshCreatorSceneReference);
        if (GUILayout.Button("Create Quad (WIP)"))
        {
            _pathMeshCreatorSceneReference.GeneratePresetQuad();
        }
        EditorGUI.BeginDisabledGroup(_pathMeshCreatorSceneReference && _pathMeshCreatorSceneReference.currentlyDrawnQuads.Count == 0);
        if (GUILayout.Button("Clear Quads (WIP)")) {
            _pathMeshCreatorSceneReference.DestroyAllQuads();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
    }

    public void OnDestroy()
    {
        if (_pathMeshCreatorSceneReference)
            _pathMeshCreatorSceneReference.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        if (_pathMeshCreatorSceneReference)
            _pathMeshCreatorSceneReference.gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        OnWindowOpened();
    }

    public void OnWindowOpened()
    {
        Debug.Log("Opened Path Mesh Editor");
        if (_pathMeshCreatorSceneReference)
            SetPathMeshCreatorObjectVisibility(true);
        else
            CreateOrFindPathMeshCreatorInScene();

        _pathMeshCreatorSceneReference.ResetWindowState();
    }

    public void OnWindowClosed()
    {
        SetPathMeshCreatorObjectVisibility(false);
    }

    private void CreateOrFindPathMeshCreatorInScene() {
        if (!_pathMeshCreatorSceneReference)
        {

            if (_pathMeshCreatorSceneReference = Object.FindObjectOfType<PathMeshCreator>(true)) //Assign find result to the static reference variable.
                return; //No need to create a new one if one already exists!
        }
        else
            return;//No need to create a new one if one already exists!

        GameObject meshCreatorObject = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags(kPathMeshEditorGameObjectName,
            HideFlags.None, typeof(PathMeshCreator));

        _pathMeshCreatorSceneReference = meshCreatorObject.GetComponent<PathMeshCreator>();
    }

    private void SetPathMeshCreatorObjectVisibility(bool visible) {
        if (_pathMeshCreatorSceneReference)
            _pathMeshCreatorSceneReference.gameObject.SetActive(visible);
    }
}
