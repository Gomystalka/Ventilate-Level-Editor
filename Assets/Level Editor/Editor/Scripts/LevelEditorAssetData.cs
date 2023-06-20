using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Level Editor Asset Data", menuName = "Tom's Level Editor/Level Editor Asset Data")]
public class LevelEditorAssetData : ScriptableObject
{
    public AssetData this[int index]
        => assetData[index];

    public AssetData[] assetData;
    [SerializeField] private System.Guid _projectOwnerGuid = System.Guid.Empty;
    [HideInInspector][SerializeField] private string _projectOwnerGuidString = string.Empty;

    //TO-DO
    //Extract Categories - DONE
    //Create category map - DONE

    public int Length => assetData.Length;

    public bool Validate()
        => _projectOwnerGuid == PlayerSettings.productGUID;

    private void AssignCurrentProjectGuid() {
        _projectOwnerGuid = PlayerSettings.productGUID;
        _projectOwnerGuidString = _projectOwnerGuid.ToString();
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(_projectOwnerGuidString))
            if (!System.Guid.TryParse(_projectOwnerGuidString, out _projectOwnerGuid))
                Debug.LogError("Failed to parse Owner GUID!", this);
    }

    [CustomEditor(typeof(LevelEditorAssetData))]
    public class LevelEditorAssetDataInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            LevelEditorAssetData data = (LevelEditorAssetData)target;
            if (data._projectOwnerGuid == System.Guid.Empty)
            {
                if (GUILayout.Button("Set Current Project as Owner"))
                {
                    data.AssignCurrentProjectGuid();
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssetIfDirty(data);
                }
            }
            else
                EditorGUILayout.HelpBox($"Owner GUID: {data._projectOwnerGuid}", MessageType.Info, true);
        }
    }
}

[System.Serializable]
public class AssetData {
    public string assetPath;
    public string assetCategory = "General";
    [System.NonSerialized] public bool isPathEditingLocked = true;
    [System.NonSerialized] public bool import = true;

    public string AssetName {
        get {
            if (string.IsNullOrEmpty(assetPath)) return string.Empty;
            if (assetPath.IndexOf('/') == -1) return string.Empty;

            return assetPath.Substring(assetPath.LastIndexOf('/') + 1);
        }
    }

    public override int GetHashCode()
        => assetPath.GetHashCode(); //Only test asset path when checking for collisions.

    public override bool Equals(object obj)
        => obj.GetHashCode() == GetHashCode();  

    public static bool operator ==(AssetData a1, AssetData a2)
        => a1.GetHashCode() == a2.GetHashCode();

    public static bool operator !=(AssetData a1, AssetData a2)
        => a1.GetHashCode() != a2.GetHashCode();
}