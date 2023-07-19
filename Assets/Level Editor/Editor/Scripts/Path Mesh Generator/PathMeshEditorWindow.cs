using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Tom.LevelEditor.PathMeshEditor;
using Tom.LevelEditor.Utility;

public class PathMeshEditorWindow : ILevelEditorWindow
{
    public const string kDefaultCurrentSessionFileName = "CurrentMeshData";
    public const string kDefaultCurrentSessionFileExtension = ".poi";

    private const string kPathMeshEditorGameObjectName = "PathMeshEditor";

    public string Title => "Path Mesh Editor";

    public Texture2D Icon { get; set; }

    public byte Order => 1;

    public LevelEditorWindow OwnerWindow { get; set; }

    private static PathMeshCreator _pathMeshCreatorSceneReference;

    public bool isAutosaveEnabled = true;
    public bool showVertexIndices = false;
    public bool saveOnWindowClose = true;
    public bool showDebugOptions = false;
    public bool showMeshBuildingControls = true;

    public bool pathCreatorVisibility = false;

    public float autosaveIntervalSeconds = 60f;

    [SerializeField] private double _nextAutosaveTime;

    public GUIContent GetTitleContent()
    {
        return GUIContent.none;
    }

    public void OnGUI()
    {
        if (EditorApplication.isPlaying) {
            EditorGUILayout.HelpBox("The Path Mesh Editor is only available outside of play mode!", MessageType.Warning, true);
            return;
        }

        if (!_pathMeshCreatorSceneReference)
        {
            EditorGUILayout.HelpBox("The Path Mesh Creator is not present in the scene!", MessageType.Warning, true);
            return;
        }

        EditorGUI.BeginDisabledGroup(_pathMeshCreatorSceneReference.QuadCount != 0);
        {
            if (GUILayout.Button("Create Source Quad") && _pathMeshCreatorSceneReference.QuadCount == 0)
            {
                _pathMeshCreatorSceneReference.GeneratePresetQuad();
                LevelEditorMessageSystem.Push("Source quad created.", 1f, LevelEditorMessageSystem.MessageType.Info);
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(_pathMeshCreatorSceneReference.QuadCount == 0);
        if (GUILayout.Button("Clear All Quads") && _pathMeshCreatorSceneReference.QuadCount != 0)
        {
            if (EditorUtility.DisplayDialog("Clear All Quads", "Are you sure that you'd like to remove all quads currently in the scene?" +
                "\nThis action cannot be undone!", "Yes", "No"))
            {
                _pathMeshCreatorSceneReference.DestroyAllQuads();
                LevelEditorMessageSystem.Push("All quads cleared.", 1f, LevelEditorMessageSystem.MessageType.Warning);
            }
        }
        EditorGUI.EndDisabledGroup();

        if (_pathMeshCreatorSceneReference.QuadCount > 0)
        {
            if (GUILayout.Button("Generate Mesh"))
                PathMeshCreationWizard.Init(_pathMeshCreatorSceneReference);
        }

        GUILayout.BeginHorizontal("Serialization", GUI.skin.window);

        float panelWidth = OwnerWindow.Size.x / 2f;
        GUILayout.BeginVertical("Controls", GUI.skin.window, GUILayout.ExpandWidth(false), GUILayout.Width(panelWidth));

        EditorGUI.BeginDisabledGroup(_pathMeshCreatorSceneReference.QuadCount == 0);
        if (GUILayout.Button("Save Current Session"))
        {
            //string serializedJsonString = _pathMeshCreatorSceneReference.SerializeCurrentQuadData();
            PerformSave();
            LevelEditorMessageSystem.Push("Save Complete.", 2f, LevelEditorMessageSystem.MessageType.Info);
            _nextAutosaveTime = EditorApplication.timeSinceStartup + autosaveIntervalSeconds;
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Load Last Session")) {
            if (_pathMeshCreatorSceneReference.QuadCount == 0)
                PerformLoad();
            else
            {
                if (EditorUtility.DisplayDialog("Session Load", "Are you sure you'd like to load the last session?\n(This will clear all your current progress.)", "Yes", "No"))
                    PerformLoad();
                else
                    LevelEditorMessageSystem.Push("Load Aborted.", 1f, LevelEditorMessageSystem.MessageType.Warning);

            }
            _nextAutosaveTime = EditorApplication.timeSinceStartup + autosaveIntervalSeconds;
        }

        if (GUILayout.Button("Load From File")) {
            if (_pathMeshCreatorSceneReference.QuadCount == 0)
                PerformLoadFromFile();
            else
            {
                if (EditorUtility.DisplayDialog("Session Load", "Are you sure you'd like to load the last session?\n(This will clear all your current progress.)", "Yes", "No"))
                    PerformLoadFromFile();
                else
                    LevelEditorMessageSystem.Push("Load Aborted.", 1f, LevelEditorMessageSystem.MessageType.Warning);

            }
            _nextAutosaveTime = EditorApplication.timeSinceStartup + autosaveIntervalSeconds;
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Settings", GUI.skin.window, GUILayout.ExpandWidth(false), GUILayout.Width(panelWidth));
        if (isAutosaveEnabled = GUILayout.Toggle(isAutosaveEnabled, "Enable Autosave"))
        {
            autosaveIntervalSeconds = EditorGUILayout.FloatField("Autosave Interval", autosaveIntervalSeconds);
            if (autosaveIntervalSeconds <= 2) //Just so that you can't autosave every frame. That'd be bad...
                autosaveIntervalSeconds = 2;
        }
        saveOnWindowClose = GUILayout.Toggle(saveOnWindowClose, "Save on Window Close");
        showMeshBuildingControls = GUILayout.Toggle(showMeshBuildingControls, "Show Mesh Creation Controls");
        _pathMeshCreatorSceneReference.showMeshBuildingControls = showMeshBuildingControls;

        if (showDebugOptions = GUILayout.Toggle(showDebugOptions, "Show Debug Options"))
        {
            EditorGUI.BeginChangeCheck();

            LevelEditorUtility.IndentedFieldLayout(1, () => {
                showVertexIndices = GUILayout.Toggle(showVertexIndices, "Show Vertex Indices");
            });
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Quad q in _pathMeshCreatorSceneReference.currentlyDrawnQuads)
                {
                    if (q == null) continue;
                    foreach (Vertex v in q.Vertices)
                    {
                        if (v == null) continue;
                        v.ShowVertexIndices = showVertexIndices;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            LevelEditorUtility.IndentedFieldLayout(1, () => {
                pathCreatorVisibility = GUILayout.Toggle(pathCreatorVisibility, "Show Path Creator Object in Hierarchy");
            });
            if (EditorGUI.EndChangeCheck())
                SetPathMeshCreatorObjectVIsibilityInHierarchy(pathCreatorVisibility);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void PerformSave() {
        string savePath 
            = LevelEditorUtility.GetLevelEditorResourcePath(OwnerWindow, "Path Cache", kDefaultCurrentSessionFileName + kDefaultCurrentSessionFileExtension);
        if (string.IsNullOrEmpty(savePath)) return;
        string content = _pathMeshCreatorSceneReference.SerializeCurrentQuadData();

        System.IO.File.WriteAllText(savePath, content);
    }

    private void PerformLoad() {
        string loadPath 
            = LevelEditorUtility.GetLevelEditorResourcePath(OwnerWindow, "Path Cache", kDefaultCurrentSessionFileName + kDefaultCurrentSessionFileExtension);
        if (string.IsNullOrEmpty(loadPath)) return;

        if (System.IO.File.Exists(loadPath))
        {
            string content = System.IO.File.ReadAllText(loadPath);

            _pathMeshCreatorSceneReference.DeserializeQuadData(content);
            if (_pathMeshCreatorSceneReference.currentlyDrawnQuads.Count != 0)
                LevelEditorMessageSystem.Push("Saved session loaded.", 2f, LevelEditorMessageSystem.MessageType.Info);
            else
                LevelEditorMessageSystem.Push("Failed to load saved session!", 2f, LevelEditorMessageSystem.MessageType.Error);
        }
        else
            LevelEditorMessageSystem.Push("No saved session data was found!", 2f, LevelEditorMessageSystem.MessageType.Warning);
    }

    private void PerformLoadFromFile() {
        string loadPath = EditorUtility.OpenFilePanel("Open path file...", "Assets/", kDefaultCurrentSessionFileExtension.Replace(".", ""));
        if (string.IsNullOrEmpty(loadPath) || !System.IO.File.Exists(loadPath)) {
            LevelEditorMessageSystem.Push($"Failed to load from file! Invalid path. [{loadPath}]", 2f, LevelEditorMessageSystem.MessageType.Error);
            return;
        }
        TryLoadFromPath(loadPath);
    }

    public void TryLoadFromPath(string loadPath) {
        string content = System.IO.File.ReadAllText(loadPath);

        _pathMeshCreatorSceneReference.DeserializeQuadData(content);
        if (_pathMeshCreatorSceneReference.currentlyDrawnQuads.Count != 0)
            LevelEditorMessageSystem.Push("Saved session loaded.", 2f, LevelEditorMessageSystem.MessageType.Info);
        else
            LevelEditorMessageSystem.Push("Failed to load saved session!", 2f, LevelEditorMessageSystem.MessageType.Error);
    }

    public void OnDestroy()
    {
        SetPathMeshCreatorObjectVisibility(false);
        OnWindowClosed();
    }

    public void OnDisable()
    {
        SetPathMeshCreatorObjectVisibility(false);
        OnWindowClosed();
    }

    public void OnEnable()
    {
        OnWindowOpened();
    }

    public void OnWindowOpened()
    {
        LevelEditorMessageSystem.Push("Opened Path Mesh Editor", 1f, LevelEditorMessageSystem.MessageType.Info);
            
        //SetPathMeshCreatorObjectVisibility(true);
        
        if(!_pathMeshCreatorSceneReference)
            CreateOrFindPathMeshCreatorInScene();

        SetPathMeshCreatorObjectVisibility(true);

        _pathMeshCreatorSceneReference.RemoveStaleVerticesInScene();
        _pathMeshCreatorSceneReference.ResetWindowState();

        EditorApplication.update -= UpdateAutosave;
        EditorApplication.update += UpdateAutosave;

        if(!EditorApplication.isPlaying)
            PerformLoad();
    }

    public void OnWindowClosed()
    {
        SetPathMeshCreatorObjectVisibility(false);
        if (saveOnWindowClose && _pathMeshCreatorSceneReference)
        {
            if (_pathMeshCreatorSceneReference.QuadCount != 0 && !EditorApplication.isPlaying)
            {
                PerformSave();
                LevelEditorMessageSystem.Push("Window Close Save Completed.", 1f, LevelEditorMessageSystem.MessageType.Info);
            }
        }
        EditorApplication.update -= UpdateAutosave;
    }

    private void CreateOrFindPathMeshCreatorInScene() {
        if (!_pathMeshCreatorSceneReference)
        {

            if (_pathMeshCreatorSceneReference = Object.FindObjectOfType<PathMeshCreator>(true)) //Assign find result to the static reference variable.
                return; //No need to create a new one if one already exists!
        }
        else
            return;//No need to create a new one if one already exists!

        PathMeshCreator meshCreatorObjectPrefab 
            = LevelEditorUtility.LoadResourceForWindowAtPath<PathMeshCreator>(OwnerWindow, "Prefabs", kPathMeshEditorGameObjectName + ".prefab");
        _pathMeshCreatorSceneReference = Object.Instantiate(meshCreatorObjectPrefab, Vector3.zero, Quaternion.identity);
        _pathMeshCreatorSceneReference.gameObject.name = kPathMeshEditorGameObjectName;
        _pathMeshCreatorSceneReference.gameObject.hideFlags = HideFlags.HideInHierarchy;
    }

    private void SetPathMeshCreatorObjectVisibility(bool visible) {
        if (_pathMeshCreatorSceneReference)
            _pathMeshCreatorSceneReference.gameObject.SetActive(visible);
    }

    private void SetPathMeshCreatorObjectVIsibilityInHierarchy(bool visible) {
        if (_pathMeshCreatorSceneReference) {
            _pathMeshCreatorSceneReference.gameObject.hideFlags = visible ? HideFlags.None : HideFlags.HideInHierarchy;
        }
    }

    private void UpdateAutosave() {
        if (!isAutosaveEnabled || !_pathMeshCreatorSceneReference || _pathMeshCreatorSceneReference.QuadCount == 0 || EditorApplication.isPlaying) return;

        if (EditorApplication.timeSinceStartup > _nextAutosaveTime) {
            _nextAutosaveTime = EditorApplication.timeSinceStartup + autosaveIntervalSeconds;
            PerformSave();
            LevelEditorMessageSystem.Push("Autosave Complete.", 2f, LevelEditorMessageSystem.MessageType.Info);
        }
    }
}
