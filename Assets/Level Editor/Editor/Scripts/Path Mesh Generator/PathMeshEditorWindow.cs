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
        if (_pathMeshCreatorSceneReference.currentlyDrawnQuads.Count == 0)
        {
            if (GUILayout.Button("Create Source Quad"))
            {
                if(_pathMeshCreatorSceneReference.currentlyDrawnQuads.Count == 0) //Fail safe.
                    _pathMeshCreatorSceneReference.GeneratePresetQuad();
            }
        }
        EditorGUI.BeginDisabledGroup(_pathMeshCreatorSceneReference && _pathMeshCreatorSceneReference.currentlyDrawnQuads.Count == 0);
        if (GUILayout.Button("Clear All Quads")) {
            if(EditorUtility.DisplayDialog("Clear All Quads", "Are you sure that you'd like to remove all quads currently in the scene?" +
                "\nThis action cannot be undone!", "Yes", "No"))
                _pathMeshCreatorSceneReference.DestroyAllQuads();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();

        if (_pathMeshCreatorSceneReference.currentlyDrawnQuads.Count > 0) {
            if (GUILayout.Button("Generate Mesh"))
                PathMeshCreationWizard.Init(_pathMeshCreatorSceneReference);
        }

        GUILayout.Space(100f);
        if (GUILayout.Button("Serialize Current Data (WIP)"))
            _pathMeshCreatorSceneReference.SerializeCurrentQuadData();
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

        _pathMeshCreatorSceneReference.RemoveStaleVerticesInScene();
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
