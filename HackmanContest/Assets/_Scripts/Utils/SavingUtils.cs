using UnityEngine;
using System.Text;
using System.IO;

public static class SavingUtils
{
    private static Encoding utf8 = Encoding.UTF8;

    /// <summary>
    /// Saves an object ot the specified path in an encrypted JSON format.
    /// </summary>
    /// <param name="path">Path to save file to</param>
    /// <param name="objToSave">Object to be saved. All serializable fields will be saved</param>
    public static void SaveObjectDataJSON(string path, object objToSave)
    {
        // If directories in the path don't exist, create them.
        string directoryName = Path.GetDirectoryName(path);
        if (directoryName.Length > 0)
        {
            Directory.CreateDirectory(directoryName);
        }

        Debug.Log("Saving To: " + path);

        using (FileStream stream = File.Open(path, FileMode.Create))
        {
            if (stream.CanWrite)
            {
                string json = JsonUtility.ToJson(objToSave, true);
                byte[] objByteArray = utf8.GetBytes(json);

                stream.Write(objByteArray, 0, objByteArray.Length);
            }
            else
                Debug.LogError("Error writing to filestream for " + path);
        }
    }

    /// <summary>
    /// Retrieves the raw bytes from a file, and returns its string
    /// representation. Good for doing custom parsing of the raw data.
    /// </summary>
    /// <param name="path">Path to load the file from.</param>
    /// <returns>String represenation of entire file's data.</returns>
    public static string LoadObjectDataRaw(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("The specified file was not found: " + path);
            return null;
        }

        Debug.Log("Loading from: " + path);

        using (FileStream stream = File.Open(path, FileMode.Open))
        {
            if (stream.CanRead)
            {
                byte[] objBuffer = new byte[stream.Length];
                stream.Read(objBuffer, 0, objBuffer.Length);

                return utf8.GetString(objBuffer);
            }
            else
                Debug.LogError("Error reading from filestream for " + path);
        }

        return null;
    }

    /// <summary>
    /// Loads bytes from a file a the specified path, and writes into
    /// the object given. Can use any serializable type.
    /// </summary>
    public static void LoadObjectDataJSON(string path, object objToWrite)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("The specified file was not found: " + path);
            return;
        }

        Debug.Log("Loading from: " + path);

        using (FileStream stream = File.Open(path, FileMode.Open))
        {
            if (stream.CanRead)
            {
                byte[] objBuffer = new byte[stream.Length];
                stream.Read(objBuffer, 0, objBuffer.Length);

                string json = utf8.GetString(objBuffer);
                JsonUtility.FromJsonOverwrite(json, objToWrite);
            }
            else
                Debug.LogError("Error reading from filestream for " + path);
        }
    }

    // I know this is technically not secure and someone could still easily decrypt it
    // using this method, but I needed it to be quick and dirty and avoid going overkill
    // with cryptography libraries or something.
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = utf8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return utf8.GetString(base64EncodedBytes);
    }
}
