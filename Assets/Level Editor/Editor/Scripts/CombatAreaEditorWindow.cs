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
    [SerializeField] private bool _useDirectionBetweenTwoPointsForNormal = true;

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

        if (GUILayout.Button("Create Source Point")) {
            GameObject vertex = _combatAreaCreatorSceneReference.CreateNewPosition(_combatAreaCreatorSceneReference.transform.position);
            if(!_vertexIconTexture)
                _vertexIconTexture = LevelEditorUtility.LoadResourceForWindowAtPath<Texture>(OwnerWindow, "Sprites", "VertexIcon.png");
            CombatAreaCreator.vertexIconTexture = _vertexIconTexture;

            EditorGUIUtility.SetIconForObject(vertex, _vertexIconTexture as Texture2D);
        }
        GUILayout.BeginVertical("Settings", GUI.skin.window);
        _useDirectionBetweenTwoPointsForNormal = GUILayout.Toggle(_useDirectionBetweenTwoPointsForNormal, "Use Direction Vector for Normal");
        GUILayout.EndVertical();
    }

    public void OnDestroy()
    {
    }

    public void OnDisable()
    {
    }

    public void OnEnable()
    {
        _vertexIconTexture = LevelEditorUtility.LoadResourceForWindowAtPath<Texture>(OwnerWindow, "Sprites", "VertexIcon.png");
        CombatAreaCreator.vertexIconTexture = _vertexIconTexture;
    }

    public void OnWindowOpened()
    {
        if (!_combatAreaCreatorSceneReference)
            CreateOrFindCombatAreaCreatorInScene();

        SetCombatAreaCreatorVisibility(true);

        LevelEditorMessageSystem.Push("Opened Combat Area Editor", 1f, LevelEditorMessageSystem.MessageType.Info);
    }

    public void OnWindowClosed()
    {
        SetCombatAreaCreatorVisibility(false);
    }

    private void CreateOrFindCombatAreaCreatorInScene()
    {
        if (!_combatAreaCreatorSceneReference)
        {

            if (_combatAreaCreatorSceneReference = Object.FindObjectOfType<CombatAreaCreator>(true)) //Assign find result to the static reference variable.
                return; //No need to create a new one if one already exists!
        }
        else
            return;//No need to create a new one if one already exists!

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
