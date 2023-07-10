using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathMeshCreationWizard : ScriptableWizard
{
    [Header("Settings")]
    [HideInInspector] [SerializeField] private string _meshName = "New Mesh";
    [SerializeField] private string _sceneObjectName = "Scene:New Mesh";
    [SerializeField] private Material _material;
    [SerializeField] private bool _saveToFile;
    [SerializeField] private bool _createCollider;
    [HideInInspector] [SerializeField] private bool _isConvex;
    [HideInInspector] [SerializeField] private bool _isTrigger;

    [Header("Transform")]
    [HideInInspector] [SerializeField] private Vector3 _positionOffset;
    [HideInInspector] [SerializeField] private Vector3 _rotation;
    [HideInInspector] [SerializeField] private Vector3 _scale = Vector3.one;

    private static PathMeshCreator _pathMeshCreator;

    internal static void Init(PathMeshCreator creator) {
        _pathMeshCreator = creator;
        DisplayWizard<PathMeshCreationWizard>("Path Mesh Generator", "Generate", "Close");
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
        if (_createCollider)
        {
            LevelEditorUtility.IndentedFieldLayout(1, () => {
                _isConvex = GUILayout.Toggle(_isConvex, "Is Convex");
            });

            EditorGUI.BeginDisabledGroup(!_isConvex);
            LevelEditorUtility.IndentedFieldLayout(1, () => {
                _isTrigger = GUILayout.Toggle(_isTrigger, "Is Trigger");
            });
            EditorGUI.EndDisabledGroup();

            if (!_isConvex)
                _isTrigger = false;
        }
        GUILayout.BeginVertical(EditorStyles.helpBox);
        _positionOffset = EditorGUILayout.Vector3Field("Position Offset", _positionOffset);
        _rotation = EditorGUILayout.Vector3Field("Rotation", _rotation);
        _scale = EditorGUILayout.Vector3Field("Scale", _scale);
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        return changeResult;
    }

    private void OnWizardCreate()
    {
        if (!_pathMeshCreator) return;
        if (string.IsNullOrEmpty(_sceneObjectName))
            _sceneObjectName = "New Mesh";

        GameObject meshObjectInScene = new GameObject(_sceneObjectName);
        meshObjectInScene.transform.position = _pathMeshCreator.transform.position + _positionOffset;
        meshObjectInScene.transform.rotation = Quaternion.Euler(_rotation);
        meshObjectInScene.transform.localScale = _scale;

        Mesh mesh = LevelEditorMeshUtility.GenerateMeshFromQuadData(ref _pathMeshCreator.currentlyDrawnQuads);
        mesh.name = _saveToFile ?  _meshName : _sceneObjectName;
        MeshFilter filter = meshObjectInScene.AddComponent<MeshFilter>();
        MeshRenderer renderer = meshObjectInScene.AddComponent<MeshRenderer>();
        GeneratedPathMesh pathMesh = meshObjectInScene.AddComponent<GeneratedPathMesh>();
        if (_createCollider) {
            MeshCollider collider = meshObjectInScene.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = _isConvex;
            collider.isTrigger = _isTrigger;
        }

        renderer.material = _material;
        filter.sharedMesh = mesh;

        string meshEditorDataPath;
        if (_saveToFile)
        {
            string path = LevelEditorMeshUtility.SaveMeshToFile(mesh, false, ModelImporterMeshCompression.Off, _meshName) + PathMeshEditorWindow.kDefaultCurrentSessionFileExtension;
            meshEditorDataPath = path;
            System.IO.File.WriteAllText(path, _pathMeshCreator.SerializeCurrentQuadData());
        }
        else
        {
            meshEditorDataPath = this.GetScriptableObjectScriptPath();
            meshEditorDataPath = meshEditorDataPath.Substring(0, meshEditorDataPath.IndexOf("/Editor/") + 8);
            meshEditorDataPath = System.IO.Path.Combine(meshEditorDataPath, "Path Cache", "Unsaved", $"{_meshName}{PathMeshEditorWindow.kDefaultCurrentSessionFileExtension}");
            System.IO.File.WriteAllText(meshEditorDataPath, _pathMeshCreator.SerializeCurrentQuadData());
        }

        pathMesh.meshCachePath = meshEditorDataPath;
        _pathMeshCreator = null;
        LevelEditorMessageSystem.Push($"Mesh Generation Complete.", 2f, LevelEditorMessageSystem.MessageType.Info);
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
