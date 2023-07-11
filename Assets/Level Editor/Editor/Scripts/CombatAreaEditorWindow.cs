using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CombatAreaEditorWindow : ILevelEditorWindow
{
    private const string kCombatZoneEditorGameObjectName = "CombatAreaEditor";

    public string Title => "Combat Zone Editor";

    public Texture2D Icon { get; set; }

    public byte Order => 2;

    public LevelEditorWindow OwnerWindow { get; set; }

    private static CombatAreaCreator _combatAreaCreatorSceneReference;

    private Texture _vertexIconTexture;

    [SerializeField] private bool _showDebugOptions;

    public GUIContent GetTitleContent()
    {
        return GUIContent.none;
    }

    public void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("The Combat Area Editor is only available outside of play mode!", MessageType.Warning, true);
            return;
        }

        if (!_combatAreaCreatorSceneReference)
        {
            EditorGUILayout.HelpBox("The Combat Area Editor is not present in the scene!", MessageType.Warning, true);
            return;
        }

        bool sourcePointPresent = _combatAreaCreatorSceneReference.VertexCount != 0;
        EditorGUI.BeginDisabledGroup(sourcePointPresent);
        if (GUILayout.Button("Create Source Point")) {
            GameObject vertex = _combatAreaCreatorSceneReference.CreateNewPosition(_combatAreaCreatorSceneReference.transform.position);
            if(!_vertexIconTexture)
                _vertexIconTexture = LevelEditorUtility.LoadResourceForWindowAtPath<Texture>(OwnerWindow, "Sprites", "VertexIcon.png");
            CombatAreaCreator.vertexIconTexture = _vertexIconTexture;

            EditorGUIUtility.SetIconForObject(vertex, _vertexIconTexture as Texture2D);
            LevelEditorMessageSystem.Push("Source Vertex Generated", 2f, LevelEditorMessageSystem.MessageType.Info);
        }
        EditorGUI.EndDisabledGroup();
        if (sourcePointPresent)
        {
            if (GUILayout.Button("Clear All Vertices")) {
                bool response = EditorUtility.DisplayDialog("Vertex Clear", "Are you sure that you'd like to remove all vertices in the current combat zone?\n" +
                    "This action cannot be undone!\n" +
                    $"Vertex Count in Scene: {_combatAreaCreatorSceneReference.VertexCount}", "Yes", "No");
                if (response)
                {
                    _combatAreaCreatorSceneReference.Clear();
                    LevelEditorMessageSystem.Push("All Vertices Cleared", 2f, LevelEditorMessageSystem.MessageType.Warning);
                }
            }
        }

        float panelWidth = OwnerWindow.Size.x / 2f;
        GUILayout.BeginHorizontal("Combat Bounds", GUI.skin.window);
        GUILayout.BeginVertical("Settings", GUI.skin.window, GUILayout.ExpandWidth(false), GUILayout.Width(panelWidth));
        _combatAreaCreatorSceneReference.lineSegmentLength = EditorGUILayout.FloatField("Line Segment Length", _combatAreaCreatorSceneReference.lineSegmentLength);
        _combatAreaCreatorSceneReference.useDirectionVectorForNormal = GUILayout.Toggle(_combatAreaCreatorSceneReference.useDirectionVectorForNormal, "Use Direction Vector for Normal");
        _showDebugOptions = GUILayout.Toggle(_showDebugOptions, "Show Debug Options");
        if (_showDebugOptions)
        {
            LevelEditorUtility.IndentedFieldLayout(1, () => {
                GUILayout.BeginVertical();
                _combatAreaCreatorSceneReference.showRendererBounds = GUILayout.Toggle(_combatAreaCreatorSceneReference.showRendererBounds, "Show Renderer Bounds");
                _combatAreaCreatorSceneReference.showMidpointAtLineSegmentDuringEditMode = GUILayout.Toggle(_combatAreaCreatorSceneReference.showMidpointAtLineSegmentDuringEditMode, "Show Mid-Point At Line Segment During Edit Mode");
                GUILayout.EndVertical();
            });
        }
            //GUILayout
        GUILayout.EndVertical();
        GUILayout.BeginVertical("Vertex Manipulation", GUI.skin.window, GUILayout.ExpandWidth(false), GUILayout.Width(panelWidth));

        if (_combatAreaCreatorSceneReference.EditMode == CombatAreaEditMode.None)
        {
            if (GUILayout.Button("Add Vertex"))
                _combatAreaCreatorSceneReference.EditMode = CombatAreaEditMode.VertexAdd;

            EditorGUI.BeginDisabledGroup(_combatAreaCreatorSceneReference.VertexCount <= 2);
            if (!_combatAreaCreatorSceneReference.IsLoopConnected)
            {
                if (GUILayout.Button("Finalise Area"))
                    _combatAreaCreatorSceneReference.ConnectLoop();
            }
            else
                if (GUILayout.Button("Disconnect Area Loop"))
                    _combatAreaCreatorSceneReference.DisconnectLoop();

            EditorGUI.EndDisabledGroup();
        }
        else
            if (GUILayout.Button("Exit Vertex Edit Mode"))
            _combatAreaCreatorSceneReference.EditMode = CombatAreaEditMode.None;
        GUILayout.Label($"Status: {GetAreaStatusString()}");
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private string GetAreaStatusString() {
        if (_combatAreaCreatorSceneReference.IsLoopConnected)
            return "Finalised";
        else if (_combatAreaCreatorSceneReference.EditMode == CombatAreaEditMode.VertexAdd)
            return "Adding Vertices";
        else
            return "Building Area";
    }

    public void OnDestroy()
    {
        OnWindowClosed();
    }

    public void OnDisable()
    {
    }

    public void OnEnable()
    {
        _vertexIconTexture = LevelEditorUtility.LoadResourceForWindowAtPath<Texture>(OwnerWindow, "Sprites", "VertexIcon.png");
        CombatAreaCreator.vertexIconTexture = _vertexIconTexture;
        OnWindowOpened();
    }

    public void OnWindowOpened()
    {
        //if (!_combatAreaCreatorSceneReference)
        //    CreateCombatAreaEditor();

        SetCombatAreaCreatorVisibility(true);

        LevelEditorMessageSystem.Push("Opened Combat Area Editor", 1f, LevelEditorMessageSystem.MessageType.Info);
    }

    public void OnWindowClosed()
    {
        SetCombatAreaCreatorVisibility(false);
    }

    private void CreateCombatAreaEditor()
    {
        //if (!_combatAreaCreatorSceneReference)
        //{

        //    if (_combatAreaCreatorSceneReference = Object.FindObjectOfType<CombatAreaCreator>(true)) //Assign find result to the static reference variable.
        //        return; //No need to create a new one if one already exists!
        //}
        //else
        //    return;//No need to create a new one if one already exists!

        GameObject combatZoneEditorPrefab 
            = LevelEditorUtility.LoadResourceForWindowAtPath<GameObject>(OwnerWindow, "Prefabs", kCombatZoneEditorGameObjectName + ".prefab");

        _combatAreaCreatorSceneReference = Object.Instantiate(combatZoneEditorPrefab).GetComponentInChildren<CombatAreaCreator>(true);
        _combatAreaCreatorSceneReference.transform.root.position = Vector3.zero;
        _combatAreaCreatorSceneReference.transform.root.gameObject.name = kCombatZoneEditorGameObjectName; //lol
        if (_combatAreaCreatorSceneReference)
            LevelEditorMessageSystem.Push("Combat Area Creator scene object created successfully!", 2f, LevelEditorMessageSystem.MessageType.Info);
        else
            LevelEditorMessageSystem.Push("Failed to create new Combat Area Creator in the scene!", 2f, LevelEditorMessageSystem.MessageType.Error);
    }

    private void SetCombatAreaCreatorVisibility(bool visible)
    {
        if (_combatAreaCreatorSceneReference)
            _combatAreaCreatorSceneReference.transform.root.gameObject.SetActive(visible);
    }

    private void SetCombatAreaCreatorVisibilityInHierarchy(bool visible)
    {
        if (_combatAreaCreatorSceneReference)
            _combatAreaCreatorSceneReference.transform.root.gameObject.hideFlags = visible ? HideFlags.None : HideFlags.HideInHierarchy;
    }
}
