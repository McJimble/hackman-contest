using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Saves API information as an encrypted string in an asset file so as to avoid exposing
/// private API keys, urls, etc. [ Hopefully :) ]
/// 
/// Would probably be better as a special editor window, but that's not
/// a priority for a project like this. Just attach to an object and use it.
/// </summary>
public class APIInfoFormatter : MonoBehaviour
{
    [Header("File Info")]
    [SerializeField] private string fileName;

    [Header("API Info To Save")]
    [SerializeField] private ExternalAPI apiInfo;

    [Header("Generated Asset")]
    [Tooltip("Reference to a base64 encoded version of an assets info." +
        " This is solely you can find it assets and use however you like")]
    [SerializeField] private EncodedAsset generatedAsset;

    public string FileName { get => fileName; }
    public EncodedAsset GeneratedAsset { get => generatedAsset; set => generatedAsset = value; }

    public static string API_INFO_FILETYPE = ".dat";
    public static string API_INFO_FOLDER = "/API";

    /// <summary>
    /// Saves this object's API data to a file at the specified path
    /// </summary>
    public void SaveAPIData()
    {
        string path = GetPathFileName(fileName);
        SavingUtils.SaveObjectDataJSON(path, apiInfo);
    }

    // Just for testing that my serialization works; updates info shown in the editor.
    public void LoadAPIData()
    {
        string path = GetPathFileName(fileName);
        SavingUtils.LoadObjectDataJSON(path, apiInfo);
    }

    /// <summary>
    /// Generates a string representation of a
    /// file location of an API file based on the main name provided.
    /// </summary>
    /// <param name="fileName">Name of file, without a file type</param>
    public static string GetPathFileName(string fileName)
    {
        return Path.Combine(Application.persistentDataPath + API_INFO_FOLDER, fileName + API_INFO_FILETYPE);
    }

    /// <summary>
    /// Generates a string representation of a
    /// file location of an API file based on the main name provided.
    /// Returns the directory rooted at /Resources/ APIs were manually placed.
    /// </summary>
    /// <param name="fileName">Name of file, without a file type</param>
    public static string GetResourcesFileName(string fileName)
    {
        return Path.Combine(API_INFO_FOLDER, fileName + API_INFO_FILETYPE);
    }
}
