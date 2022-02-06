using UnityEngine;
using System.Net;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Holds generic information needed to retrieve information
/// from an external API, excluding custom query parameters and the like.
/// </summary>
[System.Serializable]
public class ExternalAPI
{
    [SerializeField] private string apiKey;
    [SerializeField] private string apiURL;

    public string ApiKey { get => apiKey; }
    public string ApiURL { get => apiURL; }

    public static ExternalAPI CreateFromEncodedAsset(EncodedAsset asset)
    {
        string decryptedJson = SavingHelpers.Base64Decode(asset.EncryptedData);

        return JsonUtility.FromJson<ExternalAPI>(decryptedJson);
    }

    public static async Task<string> AsyncJSONResponseHttp(string urlWithQueries)
    {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urlWithQueries);
        HttpWebResponse response = (HttpWebResponse) await req.GetResponseAsync();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        string jsonResponse = reader.ReadToEnd();
        reader.Close();
        response.Close();

        return jsonResponse;
    }
}