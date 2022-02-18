using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(APIInfoFormatter))]
public class APIInfoFormatterEditor : Editor
{
    [SerializeField] private EncodedAsset generatedAsset;

    private APIInfoFormatter targetFormatter;

    public const string APIAssetPath = "Assets/API";

    public void OnEnable()
    {
        targetFormatter = target as APIInfoFormatter;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Save API info"))
        {
            targetFormatter.SaveAPIData();
        }

        if (GUILayout.Button("Load API info"))
        {
            targetFormatter.LoadAPIData();
        }

        GUILayout.Space(15);

        if (GUILayout.Button("Generate Asset from file"))
        {
            GenerateAPIAsset();
        }

        base.DrawDefaultInspector();
    }

    /// <summary>
    /// Creates a scriptable object instance that can be used elsewhere and decrypted
    /// at runtime to access API info.
    /// </summary>
    /// <param name="fileName">Name of file to search for an generate encoded version of</param>
    private void GenerateAPIAsset()
    {
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(APIAssetPath + "/" + targetFormatter.FileName + "Encoded API.asset");
        string rawData = SavingUtils.LoadObjectDataRaw(APIInfoFormatter.GetPathFileName(targetFormatter.FileName));
        rawData = SavingUtils.Base64Encode(rawData);

        if (!AssetDatabase.IsValidFolder(APIAssetPath))
        {
            int parentChildBreakpoint = APIAssetPath.LastIndexOf("/");
            string parent = APIAssetPath.Substring(0, parentChildBreakpoint);
            string child = APIAssetPath.Substring(parentChildBreakpoint, APIAssetPath.Length);

            AssetDatabase.CreateFolder(parent, child);
        }

        Debug.Log(assetPathAndName);

        EncodedAsset encodedData = ScriptableObject.CreateInstance<EncodedAsset>();
        encodedData.EncryptedData = rawData;
        targetFormatter.GeneratedAsset = encodedData;

        Undo.RegisterCreatedObjectUndo(encodedData, "Create Encoded Data Asset");

        AssetDatabase.CreateAsset(encodedData, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
