using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelEditorAssetWindow : ILevelEditorWindow
{
    private const float kFullAssetPreviewUniformSize = 120f;
    private const float kFullAssetPreviewLabelFontSize = 16f;

    public string Title => "Level Asset Editor";
    public Texture2D Icon { get; set; }
    public LevelEditorWindow OwnerWindow { get; set; }
    public byte Order => 0;

    private Vector2 _currentScrollPosition;
    private GUIStyle _assetTitleLabelStyle;
    private GUIStyle _noAssetDataLoadedStyle;
    private float _scale = 1f;
    private int _selectedCategoryIndex;
    private bool _draggingPrefab;
    private Texture2D _noAssetDataLabelTexture;
    private Dictionary<string, Object> _pathToAssetMap = new Dictionary<string, Object>();
    private string _currentlyHoveredAssetPath;
    private bool _isAssetDataElementHoverLocked;

    private static LevelEditorAssetData _loadedAssetData;
    private static Dictionary<string, List<AssetData>> _assetCategoryMap;
    private static string[] _availableCategories = new string[0];

    public LevelEditorAssetWindow() {
    }

    public void OnGUI()
    {
        if (!_noAssetDataLabelTexture)
            _noAssetDataLabelTexture = OwnerWindow.LoadTexture("NoAssetDataLabel.png");

        GUIStyle windowStyle = new GUIStyle(EditorStyles.helpBox);

        GUILayout.BeginVertical(windowStyle, GUILayout.ExpandHeight(false), GUILayout.Height(10f));
        _selectedCategoryIndex = GUILayout.Toolbar(_selectedCategoryIndex, _availableCategories, GUILayout.Width(320f), GUILayout.Height(18f));
        GUILayout.EndVertical();

        _currentScrollPosition = GUILayout.BeginScrollView(_currentScrollPosition, windowStyle);

        if (_loadedAssetData)
        {
            GUILayout.BeginHorizontal();

            List<AssetData> currentCategoryList = _assetCategoryMap[_availableCategories[_selectedCategoryIndex]];
            float size = kFullAssetPreviewUniformSize * _scale;
            int elementsPerRow = Mathf.FloorToInt((OwnerWindow.Size.x - (size / 4f)) / size);
            int currentElement = 0;

            for (int i = 0; i < currentCategoryList.Count; i++)
            {
                DrawLayoutAssetPreview(currentCategoryList[i], size, size);

                if (++currentElement % elementsPerRow == 0 && currentElement != 1)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();
        }
        else
            DrawNoAssetDataLabel();

        GUILayout.EndScrollView();

        //Bottom Controls
        GUILayout.BeginHorizontal(windowStyle);
        GUILayout.Label("View Scale: ", GUILayout.ExpandWidth(false));
        _scale = GUILayout.HorizontalSlider(_scale, 0.25f, 1f, GUILayout.Width(140f));
        GUILayout.Label($"x{System.Math.Round(_scale, 2)}", GUILayout.ExpandWidth(true), GUILayout.Width(64f));
        GUILayout.Label("Asset Data: ", GUILayout.ExpandWidth(true), GUILayout.Width(72f));

        EditorGUI.BeginDisabledGroup(true); //Always hide it.
        EditorGUILayout.ObjectField(_loadedAssetData, typeof(LevelEditorAssetData), false, GUILayout.ExpandWidth(true));
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!_loadedAssetData);
        if (GUILayout.Button("Unload", GUILayout.Width(64f)))
            UnloadCurrentLevelEditorAssetData();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        //Update Drag and Drop behaviour.
        UpdateDragAndDropBehaviour();
    }

    private void DrawLayoutAssetPreview(AssetData assetData, float width, float height)
    {
        if (_assetTitleLabelStyle == null)
            _assetTitleLabelStyle = new GUIStyle(GUI.skin.box);
        _assetTitleLabelStyle.alignment = TextAnchor.LowerCenter;
        _assetTitleLabelStyle.fontSize = (int)(kFullAssetPreviewLabelFontSize * _scale);
        _assetTitleLabelStyle.wordWrap = false;
        _assetTitleLabelStyle.clipping = TextClipping.Clip;

        Object assetObject = null;
        if (!_pathToAssetMap.ContainsKey(assetData.assetPath))
        {
            var loadedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetData.assetPath);
            if (loadedAsset != null)
            {
                _pathToAssetMap.Add(assetData.assetPath, loadedAsset);
                assetObject = loadedAsset;
            }
        }
        else
            assetObject = _pathToAssetMap[assetData.assetPath];

        if (!assetObject) {
            //Draw error message if the asset is not valid.
            return;
        }

        Texture2D assetPreviewTex = AssetPreview.GetAssetPreview(assetObject);
        GUIContent elementGuiContent = new GUIContent("", assetPreviewTex, assetData.assetPath);
        Rect assetRect = GUILayoutUtility.GetRect(elementGuiContent, EditorStyles.helpBox,
            GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        if (LevelEditorUtility.CheckIfRectIsHoveredAndPassesCondition(assetRect, !_isAssetDataElementHoverLocked))
        {
            GUI.Box(new Rect(assetRect.x, assetRect.y, assetRect.width + 2, assetRect.height + 2), "", EditorStyles.helpBox);
            _currentlyHoveredAssetPath = assetData.assetPath;
        }

        GUI.Box(assetRect, elementGuiContent, EditorStyles.helpBox);
        GUI.Label(assetRect, assetData.AssetName, _assetTitleLabelStyle);
    }

    private void DrawNoAssetDataLabel()
    {
        //GUILayout.Label("No Asset Data loaded. Drag and Drop a new Asset Data object into this window.");
        if (_noAssetDataLoadedStyle == null)
            _noAssetDataLoadedStyle = new GUIStyle(EditorStyles.label);
        _noAssetDataLoadedStyle.fontStyle = FontStyle.Bold;
        _noAssetDataLoadedStyle.fontSize = 20;
        _noAssetDataLoadedStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
        _noAssetDataLoadedStyle.alignment = TextAnchor.UpperCenter;
        _noAssetDataLoadedStyle.contentOffset = new Vector2(0, 32f);
        _noAssetDataLoadedStyle.richText = true;

        Vector2 noAssetDataLabelSize = new Vector2(300, 300);
        GUILayout.BeginHorizontal();
        GUILayout.Space((OwnerWindow.Size.x / 2) - (noAssetDataLabelSize.x / 2f));
        GUILayout.Box(_noAssetDataLabelTexture, GUILayout.Width(noAssetDataLabelSize.x), GUILayout.Height(noAssetDataLabelSize.y));

        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Label(lastRect, "No Asset Data loaded.\n\n" +
            "<size=12>Drag and Drop a new Asset Data object\ninto this window.</size>", _noAssetDataLoadedStyle);

        GUILayout.EndHorizontal();
        _noAssetDataLoadedStyle.richText = false;
    }

    private void UpdateDragAndDropBehaviour()
    {
        switch (Event.current.type) {
            case EventType.MouseDown: //Begin the drag operation.
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.StartDrag($"{GetHashCode()}");

                Selection.activeGameObject = null;
                _draggingPrefab = true;

                if (!string.IsNullOrEmpty(_currentlyHoveredAssetPath))
                {
                    _isAssetDataElementHoverLocked = true;
                    GameObject loadedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_currentlyHoveredAssetPath);
                    if (loadedAsset)
                        DragAndDrop.objectReferences = new Object[] { loadedAsset };
                }
                Event.current.Use();
                break;
            case EventType.DragUpdated: //Update drag operation.
                DragAndDrop.visualMode = GetDragAndDropMode();
                Event.current.Use();
                break;
            case EventType.DragPerform: //Handle new LevelEditorAssetData asset being dragged into the editor window.
                if (!_draggingPrefab && DragAndDrop.objectReferences[0] != null)
                {
                    if (DragAndDrop.objectReferences[0].GetType() == typeof(LevelEditorAssetData))
                    {
                        DragAndDrop.AcceptDrag();
                        LoadLevelEditorAssetData(DragAndDrop.objectReferences[0] as LevelEditorAssetData);
                    }
                }
                Event.current.Use();
                break;
            case EventType.DragExited: //Reset drag state.
                _draggingPrefab = false;
                _isAssetDataElementHoverLocked = false; //This causes a bug but it's small.
                Event.current.Use();
                break;
        }
    }

    private void LoadLevelEditorAssetData(LevelEditorAssetData levelEditorAssetData)
    {
        if (!levelEditorAssetData)
        {
            Debug.LogError("Failed to load Editor Asset Data!", OwnerWindow);
            return;
        }

        if (levelEditorAssetData.Validate())
        {
            _pathToAssetMap.Clear();
            _loadedAssetData = levelEditorAssetData;
            SortCurrentAssetDataByCategory();
        }
        else
        {
            Debug.LogError("Failed to load Editor Asset Data! Asset Data created is only valid for the project it was created with!", OwnerWindow);
            return;
        }
    }

    private void UnloadCurrentLevelEditorAssetData()
    {
        _loadedAssetData = null;
        _pathToAssetMap.Clear();
        if (_assetCategoryMap != null)
            _assetCategoryMap.Clear();

        if(_availableCategories != null)
            System.Array.Clear(_availableCategories, 0, _availableCategories.Length);
        _availableCategories = new string[0];
    }

    private void SortCurrentAssetDataByCategory() {
        _assetCategoryMap = new Dictionary<string, List<AssetData>>();
        List<string> tempCategories = new List<string>();

        foreach (AssetData data in _loadedAssetData.assetData) {
            if (_assetCategoryMap.ContainsKey(data.assetCategory))
                _assetCategoryMap[data.assetCategory].Add(data);
            else
            {
                List<AssetData> assetList = new List<AssetData>();
                assetList.Add(data);
                _assetCategoryMap.Add(data.assetCategory, assetList);

                tempCategories.Add(data.assetCategory);
            }
        }

        _availableCategories = tempCategories.ToArray();
    }

    private DragAndDropVisualMode GetDragAndDropMode()
    {
        if (_draggingPrefab) return DragAndDropVisualMode.Move;
        if (DragAndDrop.objectReferences.Length == 0) return DragAndDropVisualMode.Rejected;

        return DragAndDrop.objectReferences[0].GetType() == typeof(LevelEditorAssetData) ?
            DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
    }

    public void OnEnable() {
    }

    public void OnDisable() { 
        
    }

    public void OnDestroy() { 
    
    }

    public GUIContent GetTitleContent()
    {
        return GUIContent.none;
    }

    public void OnWindowOpened()
    {
        Debug.Log("Opened Level Asset Window");
    }

    public void OnWindowClosed() {
        Debug.Log("");   
    }
}
