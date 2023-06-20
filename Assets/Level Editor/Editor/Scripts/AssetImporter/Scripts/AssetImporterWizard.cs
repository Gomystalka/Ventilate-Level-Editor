using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetImporterWizard : ScriptableWizard
{
    private const string kGeneralCategory = "General";
    private const float kFixedTopFieldsHeight = 150f;
    public LevelEditorAssetData assetData;

    private List<AssetData> _assetDataListCopy;

    private bool _assetDataLoaded = false;

    [MenuItem("Tom's Level Editor/Asset Importer")]
    static void Init() {
        DisplayWizard<AssetImporterWizard>("Asset Importer", "Import");
    }

    private void OnEnable()
    {
        if(assetData)
            OnAssetDataLoaded();
    }

    protected override bool DrawWizardGUI()
    {
        bool drawResult = base.DrawWizardGUI();
        if (_assetDataListCopy == null) return drawResult;
        createButtonName = _assetDataListCopy.Count == 0 ? "" : $"Import into {assetData.name}";
        HandleDragAndDropBehaviour();

        if (_assetDataListCopy.Count == 0) {
            EditorGUILayout.HelpBox("There is no existing Asset Data present within the specified LevelEditorAssetData.\n" +
                "Drag and Drop any prefab GameObject into this window to begin importing assets.", MessageType.Warning, true);
            GUILayout.Box("", EditorStyles.helpBox, GUILayout.ExpandHeight(true), GUILayout.Height(position.size.y - kFixedTopFieldsHeight));
            return drawResult;
        }
        GUILayout.Label("Assets to Import");
        GUILayout.BeginVertical(EditorStyles.helpBox);
        for (int i = 0; i < _assetDataListCopy.Count; i++)
            _assetDataListCopy[i] = DrawElement(_assetDataListCopy[i]);
        GUILayout.EndVertical();
        return drawResult;
    }

    private AssetData DrawElement(AssetData assetDataElement) {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        //GUILayout.Label(AssetDatabase.LoadAssetAtPath(assetDataElement.assetPath));
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Name: ");
        EditorGUI.BeginDisabledGroup(true);
        GUILayout.TextField(assetDataElement.AssetName, GUILayout.MaxWidth(400f), GUILayout.MinWidth(100f));
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Path: ");
        EditorGUI.BeginDisabledGroup(assetDataElement.isPathEditingLocked);
        float buttonWidth = EditorGUIUtility.singleLineHeight * 2f;
        assetDataElement.assetPath = GUILayout.TextField(assetDataElement.assetPath, GUILayout.MaxWidth(400f - buttonWidth), GUILayout.MinWidth(100f));
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button(assetDataElement.isPathEditingLocked ? EditorGUIUtility.IconContent("LockIcon-On") :
            EditorGUIUtility.IconContent("LockIcon"), GUILayout.MaxWidth(buttonWidth - 3), GUILayout.Height(buttonWidth / 2f), GUILayout.ExpandWidth(false)))
            assetDataElement.isPathEditingLocked = !assetDataElement.isPathEditingLocked;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Category: ");
        assetDataElement.assetCategory = GUILayout.TextField(assetDataElement.assetCategory, GUILayout.MaxWidth(400f), GUILayout.MinWidth(100f));
        GUILayout.EndHorizontal();
        assetDataElement.import = GUILayout.Toggle(assetDataElement.import, "Import");
        GUILayout.EndVertical();

        if (!string.IsNullOrEmpty(assetDataElement.assetPath))
        {
            Object asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetDataElement.assetPath);
            if (asset)
            {
                GUILayout.Box(AssetPreview.GetAssetPreview(asset), EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false), GUILayout.Width(72), GUILayout.Height(72));
            }
            else
                EditorGUILayout.HelpBox("Invalid Path", MessageType.Error);
        }
        GUILayout.EndHorizontal();

        return assetDataElement;
    }

    private void OnWizardCreate() //Import button
    {
        if (!_assetDataLoaded || _assetDataListCopy == null || _assetDataListCopy.Count == 0) return;
        System.Array.Clear(assetData.assetData, 0, assetData.assetData.Length); //Clear existing array.
        List<AssetData> importableAssetData = new List<AssetData>();
        foreach (AssetData data in _assetDataListCopy) //Build new list with only importable assets.
            if (data.import)
                importableAssetData.Add(data);

        assetData.assetData = importableAssetData.ToArray(); //Assign the list to the existing asset data.
        EditorUtility.SetDirty(assetData); //Mark asset for saving.
        AssetDatabase.SaveAssetIfDirty(assetData); //Force save the asset.

        assetData = null; //Remove reference
    }

    private void OnWizardUpdate()
    {
        if (assetData)
        {
            if (!_assetDataLoaded)
            {
                OnAssetDataLoaded();
                _assetDataLoaded = true;
            }
            helpString = "Please drag and drop any asset you'd like to import.";
        }
        else
        {
            _assetDataLoaded = false;
            createButtonName = "";
            helpString = "No asset data detected.";
        }
    }

    private void HandleDragAndDropBehaviour() {
        if (Event.current.type == EventType.DragPerform) {
            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
                Object draggedObject = DragAndDrop.objectReferences[i];

                if (draggedObject != null && draggedObject is GameObject)
                    AddAssetIfIsUnique(DragAndDrop.paths[i]);
            }
            DragAndDrop.AcceptDrag();
        }
        if (Event.current.type == EventType.DragUpdated) {

            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
            {
                Object draggedObject = DragAndDrop.objectReferences[i];
                if (draggedObject == null || !(draggedObject is GameObject)) { 
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    Debug.Log($"Rejected: {draggedObject} | {draggedObject.GetType()} | {draggedObject.GetType().BaseType}");
                    break;
                }
            }
            if(DragAndDrop.visualMode != DragAndDropVisualMode.Rejected)
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }
    }

    private void AddAssetIfIsUnique(string assetPath) {
        AssetData data = new AssetData() {
            assetPath = assetPath,
            assetCategory = kGeneralCategory,
            import = true,
            isPathEditingLocked = true
        };
        if (_assetDataListCopy != null && !_assetDataListCopy.Contains(data))
            _assetDataListCopy.Add(data);
    }

    private void OnAssetDataLoaded() {
        _assetDataListCopy = new List<AssetData>(assetData.assetData);
    }
}
