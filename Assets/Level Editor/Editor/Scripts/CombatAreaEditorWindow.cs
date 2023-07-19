using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Tom.LevelEditor.Runtime.CombatAreaEditor;
using Tom.LevelEditor.Runtime.Utility;
using Tom.LevelEditor.Editor.Utility;

namespace Tom.LevelEditor.Editor.EditorWindows
{
    public class CombatAreaEditorWindow : ILevelEditorWindow
    {
        private const string kCombatZoneEditorGameObjectName = "CombatAreaEditor";

        public string Title => "Combat Zone Editor";

        public Texture2D Icon { get; set; }

        public byte Order => 2;

        public LevelEditorWindow OwnerWindow { get; set; }

        private Texture _vertexIconTexture;

        [SerializeField] private bool _showDebugOptions;

        public GUIContent GetTitleContent()
        {
            return GUIContent.none;
        }

        public void OnGUI()
        {
            CheckForSelectionChange();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("The Combat Area Editor is only available outside of play mode!", MessageType.Warning, true);
                return;
            }

            EditorStyles.boldLabel.richText = true;
            TextAnchor lastAlignment = EditorStyles.boldLabel.alignment;
            EditorStyles.boldLabel.alignment = TextAnchor.MiddleCenter;
            if (!CombatAreaCreator.CurrentCombatAreaCreator)
            {

                GUILayout.Label("<size=21>No Combat Area Creator Selected.</size>\n" +
                                "<size=16>Please select one in the scene or create one below!</size>", EditorStyles.boldLabel);

                GUI.skin.button.richText = true;
                float buttonWidth = 200f;
                GUILayout.BeginHorizontal();
                GUILayout.Space((OwnerWindow.Size.x / 2f) - (buttonWidth / 2f));
                if (GUILayout.Button("<size=21><b>Create New\nCombat Area</b></size>", GUILayout.ExpandWidth(false), GUILayout.Width(buttonWidth), GUILayout.Height(80f)))
                    CreateCombatAreaEditor();
                GUILayout.EndHorizontal();
                GUI.skin.button.richText = false;

                EditorStyles.boldLabel.richText = false;
                EditorStyles.boldLabel.alignment = lastAlignment;
                return;
            }
            GUILayout.Label($"<size=24>{CombatAreaCreator.CurrentCombatAreaCreator.transform.root.gameObject.name}</size>", EditorStyles.boldLabel);

            EditorStyles.boldLabel.richText = false;
            EditorStyles.boldLabel.alignment = lastAlignment;

            bool sourcePointPresent = CombatAreaCreator.CurrentCombatAreaCreator.VertexCount != 0;
            EditorGUI.BeginDisabledGroup(sourcePointPresent);
            if (GUILayout.Button("Create Source Point"))
            {
                GameObject vertex = CombatAreaCreator.CurrentCombatAreaCreator.CreateNewPosition(CombatAreaCreator.CurrentCombatAreaCreator.transform.position);
                if (!_vertexIconTexture)
                    _vertexIconTexture = LevelEditorUtility.LoadResourceForWindowAtPath<Texture>(OwnerWindow, "Sprites", "VertexIcon.png");
                CombatAreaCreator.vertexIconTexture = _vertexIconTexture;

                LevelEditorUtility.SetIconForObject(vertex, _vertexIconTexture as Texture2D);
                LevelEditorMessageSystem.Push("Source Vertex Generated", 2f, LevelEditorMessageSystem.MessageType.Info);
            }
            EditorGUI.EndDisabledGroup();
            if (sourcePointPresent)
            {
                if (GUILayout.Button("Clear All Vertices"))
                {
                    bool response = EditorUtility.DisplayDialog("Vertex Clear", "Are you sure that you'd like to remove all vertices in the current combat zone?\n" +
                        "This action cannot be undone!\n" +
                        $"Vertex Count in Scene: {CombatAreaCreator.CurrentCombatAreaCreator.VertexCount}", "Yes", "No");
                    if (response)
                    {
                        CombatAreaCreator.CurrentCombatAreaCreator.Clear();
                        LevelEditorMessageSystem.Push("All Vertices Cleared", 2f, LevelEditorMessageSystem.MessageType.Warning);
                    }
                }
            }

            float panelWidth = OwnerWindow.Size.x / 2f;
            GUILayout.BeginHorizontal("Combat Bounds", GUI.skin.window);
            GUILayout.BeginVertical("Settings", GUI.skin.window, GUILayout.ExpandWidth(false), GUILayout.Width(panelWidth));
            CombatAreaCreator.CurrentCombatAreaCreator.lineSegmentLength = EditorGUILayout.FloatField("Line Segment Length", CombatAreaCreator.CurrentCombatAreaCreator.lineSegmentLength);
            CombatAreaCreator.CurrentCombatAreaCreator.useDirectionVectorForNormal = GUILayout.Toggle(CombatAreaCreator.CurrentCombatAreaCreator.useDirectionVectorForNormal, "Use Direction Vector for Normal");

            EditorGUI.BeginChangeCheck();
            CombatAreaCreator.CurrentCombatAreaCreator.showCombatAreaDetails = GUILayout.Toggle(CombatAreaCreator.CurrentCombatAreaCreator.showCombatAreaDetails, "Show Combat Area Details");
            if (EditorGUI.EndChangeCheck())
                CombatAreaCreator.CurrentCombatAreaCreator.SetDetailsVisiblity(CombatAreaCreator.CurrentCombatAreaCreator.showCombatAreaDetails);

            CombatAreaCreator.CurrentCombatAreaCreator.combatAreaHeightOffset = EditorGUILayout.FloatField("Combat Area Height", CombatAreaCreator.CurrentCombatAreaCreator.combatAreaHeightOffset);

            _showDebugOptions = GUILayout.Toggle(_showDebugOptions, "Show Debug Options");
            if (_showDebugOptions)
            {
                LevelEditorUtility.IndentedFieldLayout(1, () =>
                {
                    GUILayout.BeginVertical();
                    CombatAreaCreator.CurrentCombatAreaCreator.showRendererBounds = GUILayout.Toggle(CombatAreaCreator.CurrentCombatAreaCreator.showRendererBounds, "Show Renderer Bounds");
                    CombatAreaCreator.CurrentCombatAreaCreator.showMidpointAtLineSegmentDuringEditMode = GUILayout.Toggle(CombatAreaCreator.CurrentCombatAreaCreator.showMidpointAtLineSegmentDuringEditMode, "Show Mid-Point At Line Segment During Edit Mode");
                    GUILayout.EndVertical();
                });
            }
            //GUILayout
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Area Manipulation", GUI.skin.window, GUILayout.ExpandWidth(false), GUILayout.Width(panelWidth));

            if (CombatAreaCreator.CurrentCombatAreaCreator.EditMode == CombatAreaEditMode.None)
            {
                if (GUILayout.Button("Add Vertex Mode"))
                    CombatAreaCreator.CurrentCombatAreaCreator.EditMode = CombatAreaEditMode.VertexAdd;

                if (GUILayout.Button("Add Area Trigger"))
                    CombatAreaCreator.CurrentCombatAreaCreator.EditMode = CombatAreaEditMode.AreaTriggerAdd;

                if (GUILayout.Button("Add Enemy Spawner"))
                    CombatAreaCreator.CurrentCombatAreaCreator.EditMode = CombatAreaEditMode.SpawnerAdd;

                EditorGUI.BeginDisabledGroup(CombatAreaCreator.CurrentCombatAreaCreator.VertexCount <= 2);
                if (!CombatAreaCreator.CurrentCombatAreaCreator.IsLoopConnected)
                {
                    if (GUILayout.Button("Finalise Area"))
                        CombatAreaCreator.CurrentCombatAreaCreator.ConnectLoop();
                }
                else
                    if (GUILayout.Button("Disconnect Area Loop"))
                    CombatAreaCreator.CurrentCombatAreaCreator.DisconnectLoop();

                EditorGUI.EndDisabledGroup();
            }
            else
                if (GUILayout.Button("Exit Vertex Edit Mode"))
                CombatAreaCreator.CurrentCombatAreaCreator.EditMode = CombatAreaEditMode.None;
            GUILayout.Label($"Status: {GetAreaStatusString()}");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private string GetAreaStatusString()
        {
            if (CombatAreaCreator.CurrentCombatAreaCreator.IsLoopConnected)
                return "Finalised";
            else if (CombatAreaCreator.CurrentCombatAreaCreator.EditMode == CombatAreaEditMode.VertexAdd)
                return "Adding Vertices";
            else if (CombatAreaCreator.CurrentCombatAreaCreator.EditMode == CombatAreaEditMode.AreaTriggerAdd)
                return "Adding Area Triggers";
            else if (CombatAreaCreator.CurrentCombatAreaCreator.EditMode == CombatAreaEditMode.SpawnerAdd)
                return "Adding Enemy Spawners";
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

            //Selection.selectionChanged -= OnSelectionChanged;
            //Selection.selectionChanged += OnSelectionChanged;
        }

        public void OnWindowOpened()
        {
            SetCombatAreaCreatorVisibility(true);

            if (CombatAreaCreator.CurrentCombatAreaCreator)
                CombatAreaCreator.CurrentCombatAreaCreator.SetDetailsVisiblity(CombatAreaCreator.CurrentCombatAreaCreator.showCombatAreaDetails);
            LevelEditorMessageSystem.Push("Opened Combat Area Editor", 1f, LevelEditorMessageSystem.MessageType.Info);
        }

        public void OnWindowClosed()
        {
            SetCombatAreaCreatorVisibility(false);
            //Selection.selectionChanged -= OnSelectionChanged;

            if (CombatAreaCreator.CurrentCombatAreaCreator)
            {
                CombatAreaCreator.CurrentCombatAreaCreator.EditMode = CombatAreaEditMode.None;
                CombatAreaCreator.CurrentCombatAreaCreator.SetDetailsVisiblity(CombatAreaCreator.CurrentCombatAreaCreator.showCombatAreaDetails);
            }
        }

        private GameObject _lastSelection = null;

        private void CheckForSelectionChange()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (!selectedObject)
            {
                CombatAreaCreator.CurrentCombatAreaCreator = null;
                return;
            }

            if (selectedObject != _lastSelection)
            {
                _lastSelection = selectedObject;
                CombatAreaCreator combatAreaCreator = selectedObject.GetComponentInChildren<CombatAreaCreator>();
                if (combatAreaCreator)
                    CombatAreaCreator.CurrentCombatAreaCreator = combatAreaCreator;
            }
        }

        private void CreateCombatAreaEditor()
        {
            //if (!CombatAreaCreator.CurrentCombatAreaCreator)
            //{

            //    if (CombatAreaCreator.CurrentCombatAreaCreator = Object.FindObjectOfType<CombatAreaCreator>(true)) //Assign find result to the static reference variable.
            //        return; //No need to create a new one if one already exists!
            //}
            //else
            //    return;//No need to create a new one if one already exists!

            GameObject combatZoneEditorPrefab
                = LevelEditorUtility.LoadResourceForWindowAtPath<GameObject>(OwnerWindow, "Prefabs", kCombatZoneEditorGameObjectName + ".prefab");

            CombatAreaCreator.CurrentCombatAreaCreator = Object.Instantiate(combatZoneEditorPrefab).GetComponentInChildren<CombatAreaCreator>(true);
            CombatAreaCreator.CurrentCombatAreaCreator.transform.root.position = Vector3.zero;
            CombatAreaCreator.CurrentCombatAreaCreator.transform.root.gameObject.name = kCombatZoneEditorGameObjectName; //lol
            if (CombatAreaCreator.CurrentCombatAreaCreator)
            {
                LevelEditorMessageSystem.Push("Combat Area Creator scene object created successfully!", 2f, LevelEditorMessageSystem.MessageType.Info);
                Selection.activeGameObject = CombatAreaCreator.CurrentCombatAreaCreator.transform.root.gameObject;
            }
            else
                LevelEditorMessageSystem.Push("Failed to create new Combat Area Creator in the scene!", 2f, LevelEditorMessageSystem.MessageType.Error);
        }

        private void SetCombatAreaCreatorVisibility(bool visible)
        {
            if (CombatAreaCreator.CurrentCombatAreaCreator)
                CombatAreaCreator.CurrentCombatAreaCreator.transform.root.gameObject.SetActive(visible);
        }

        private void SetCombatAreaCreatorVisibilityInHierarchy(bool visible)
        {
            if (CombatAreaCreator.CurrentCombatAreaCreator)
                CombatAreaCreator.CurrentCombatAreaCreator.transform.root.gameObject.hideFlags = visible ? HideFlags.None : HideFlags.HideInHierarchy;
        }
    }
}