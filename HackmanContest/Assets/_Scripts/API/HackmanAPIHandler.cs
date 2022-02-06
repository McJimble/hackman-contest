using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Loads information from the Hackman API, in this case using the url and
/// API key found from a save file.
/// 
/// Could probably expand to have a generic API info handler in the future,
/// but that's not a priority right now.
/// </summary>
public class HackmanAPIHandler : MonoBehaviour
{
    [System.Serializable]
    public class HangmanWord
    {
        [SerializeField] private string word;

        public override string ToString()
        {
            return word;
        }
    }

    public const float API_CHECK_TIME = 6.0f;
    public const string API_FILENAME = "HackMan";

    [SerializeField] private EncodedAsset hackManAPIEncoded;

    private ExternalAPI hackManAPIInfo;
    private float apiCheckTimer = API_CHECK_TIME;

    public bool CanAPICheck { get => (apiCheckTimer < 0f); }

    private System.Action<HangmanWord> testAsyncRequest;

    private void Awake()
    {
        hackManAPIInfo = ExternalAPI.CreateFromEncodedAsset(hackManAPIEncoded);

        Debug.Log(hackManAPIInfo.ApiURL);

        testAsyncRequest += WordReceived;
    }

    private void Update()
    {
        //apiCheckTimer = (CanAPICheck) ? (API_CHECK_TIME) : (apiCheckTimer - Time.deltaTime);
        apiCheckTimer -= Time.deltaTime;

        if (CanAPICheck)
        {
            GetWord(testAsyncRequest);
            apiCheckTimer = API_CHECK_TIME;
        }
    }

    /// <summary>
    /// Gets a hangman word from HackMan API. If a length is specified, it will
    /// use that as a query parameter and return of that length.
    /// </summary>
    /// 
    /// <param name="onWordReceivedCallback">
    /// Action to invoke once asyncrhonous loading of word from http request is complete
    /// returns null if request for failed. Use this to store the word in other scripts
    /// </param>
    /// 
    /// <param name="length">
    /// Length of word to retrieve. If not specified, a random word is given
    /// with no regard to its length
    /// </param>
    public async void GetWord(System.Action<HangmanWord> onWordReceivedCallback, int length = 0)
    {
        // Formatting used for web request based on whether we want the length parameter or not.
        string formatQueries = (length > 0) ? string.Format(hackManAPIInfo.ApiURL + "?key={0}&length={1}", hackManAPIInfo.ApiKey, length) :
            string.Format(hackManAPIInfo.ApiURL + "?key={0}", hackManAPIInfo.ApiKey);

        // Get Json from http request, convert to readable, then return word parameter from the response.
        HangmanWord word = new HangmanWord();
        Task<string> reqTask = ExternalAPI.AsyncJSONResponseHttp(formatQueries);
        string json = await reqTask;

        JsonUtility.FromJsonOverwrite(json, word);

        onWordReceivedCallback?.Invoke(word);
    }

    private void WordReceived(HangmanWord word)
    {
        Debug.Log(word.ToString());
    }
}
