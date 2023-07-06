using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathMeshCreationWizard : ScriptableWizard
{
    [HideInInspector] [SerializeField] private string _meshName = "New Mesh";
    [SerializeField] private string _sceneObjectName = "Scene:New Mesh";
    [SerializeField] private Material _material;
    [SerializeField] private bool _saveToFile;

    private static PathMeshCreator _pathMeshCreator;

    internal static void Init(PathMeshCreator creator) {
        _pathMeshCreator = creator;
        PathMeshCreationWizard w = DisplayWizard<PathMeshCreationWizard>("Path Mesh Generator", "Generate", "Close");
    }

    protected override bool DrawWizardGUI()
    {
        if (!_pathMeshCreator)
            return false;

        GUILayout.BeginVertical(EditorStyles.helpBox);
        bool changeResult = base.DrawWizardGUI();

        if (_saveToFile) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Mesh File Name");
            _meshName = GUILayout.TextField(_meshName);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        return changeResult;
    }

    private void OnWizardCreate()
    {
        if (!_pathMeshCreator) return;
        if (string.IsNullOrEmpty(_sceneObjectName))
            _sceneObjectName = "New Mesh";

        GameObject meshObjectInScene = new GameObject(_sceneObjectName);
        meshObjectInScene.transform.position = _pathMeshCreator.transform.position;
        meshObjectInScene.transform.rotation = _pathMeshCreator.transform.rotation;

        Mesh mesh = LevelEditorMeshUtility.GenerateMeshFromQuadData(ref _pathMeshCreator.currentlyDrawnQuads);
        MeshFilter filter = meshObjectInScene.AddComponent<MeshFilter>();
        MeshRenderer renderer = meshObjectInScene.AddComponent<MeshRenderer>();
        //GeneratedPathMesh pathMesh = meshObjectInScene.AddComponent<GeneratedPathMesh>();

        renderer.material = _material;
        filter.sharedMesh = mesh;

        if (_saveToFile)
            LevelEditorMeshUtility.SaveMeshToFile(mesh, false, ModelImporterMeshCompression.Off, _meshName);

        _pathMeshCreator = null;
    }

    private void OnWizardOtherButton()
    {
        Close();
        _pathMeshCreator = null;
    }

    private void OnWizardUpdate()
    {
        if (_pathMeshCreator)
        {
            createButtonName = string.IsNullOrEmpty(_meshName) ? "" : "Create";
            helpString = "Use this to generate and save your path mesh.\nIf the Material is null, a default Material will be used.";
            errorString = "";
        }
        else
        {
            createButtonName = "";
            errorString = "No Path Mesh data loaded! This window should only be opened via the Path Mesh Editor Window!";
            helpString = "";
        }
    }

    private void OnDestroy()
    {
        //_pathMeshCreator = null;
    }
}
