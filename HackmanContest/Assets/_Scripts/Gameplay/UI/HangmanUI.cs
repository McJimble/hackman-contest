using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

/// <summary>
/// Listens to a HangmanController and controls a special user interface for one.
/// </summary>
public class HangmanUI : MonoBehaviour
{
    public const string FAIL_SCREEN_INIT_TEXT = "Too bad!\nThe word was:\n\n";
    public const string CORRECT_SCREEN_INIT_TEXT = "You got the word in:\n";

    public static readonly string[] RandomPraiseWords = new string[] { "Nice", "Good Job", "Awesome", "Wow", "Congrats" };

    [SerializeField] private AudioClip scoreUpClip;
    [SerializeField] private AudioClip wrongAnswerClip;
    [SerializeField] private AudioClip rightAnswerClip;

    [Header("Components")]
    [SerializeField] private HangmanController controller;
    [SerializeField] private ScrollingRawImage scrollingBG;
    [SerializeField] private RawImage colorBG;
    [SerializeField] private Color onWrongGuessColor;
    [SerializeField] private float hueShiftPerSecond;
    [Space]
    [SerializeField] private RawImage hangmanStateImage;
    [SerializeField] private RawImage[] poleImages;
    [SerializeField] private RawImage onRoundCompleteBackground;
    [SerializeField] private Texture[] wrongAnswerImages;
    [Space]
    [SerializeField] private TextMeshProUGUI wordDisplayText;
    [SerializeField] private TextMeshProUGUI failedDisplayText;
    [SerializeField] private TextMeshProUGUI correctDisplayText;
    [SerializeField] private TextMeshProUGUI correctScoreDisplayText;

    private int hangmanStateIndex = -1;
    private Vector3 hangmanImageOriginalPos;
    private Rigidbody hangmanImageRB;

    private float targetHue;
    private Coroutine lerpColorRoutine;

    private StringBuilder sb = new StringBuilder();

    private void Awake()
    {
        controller = (controller) ? controller : FindObjectOfType<HangmanController>();
        hangmanStateImage.transform.gameObject.SetActive(false);
        failedDisplayText.transform.gameObject.SetActive(false);
        onRoundCompleteBackground.transform.gameObject.SetActive(false);
        correctDisplayText.transform.gameObject.SetActive(false);
        correctScoreDisplayText.transform.gameObject.SetActive(false);

        correctDisplayText.text = "";
        correctScoreDisplayText.text = "0";

        hangmanImageRB = hangmanStateImage.GetComponent<Rigidbody>();
        hangmanImageOriginalPos = hangmanImageRB.transform.position;

        controller.onAnswerRight += GuessRightEffect;
        controller.onAnswerWrong += GuessWrongEffect;
        controller.onWordStarted += WordStartSetup;
        controller.onWordFailed += DisplayFail;
        controller.onWordGuessed += WordCorrectEffect;

        hangmanStateImage.texture = null;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (lerpColorRoutine == null)
        {
            float h, s, v;
            Color.RGBToHSV(colorBG.color, out h, out s, out v);

            h += hueShiftPerSecond * Time.deltaTime;
            colorBG.color = Color.HSVToRGB(h, s, v);
        }

    }

    private void WordCorrectEffect(string word, string guesses)
    {
        SoundManager.Instance.PlaySound(rightAnswerClip);

        GameObject imgCopy = Instantiate(hangmanStateImage.transform.gameObject, hangmanStateImage.transform.position, 
            hangmanStateImage.transform.rotation, hangmanStateImage.transform.parent);

        imgCopy.transform.localScale = imgCopy.transform.localScale;
        imgCopy.GetComponent<HingeJoint>().breakForce = 0.001f;
        imgCopy.GetComponent<Rigidbody>().AddRelativeForce(Random.Range(-100, 100), 0, 0, ForceMode.Impulse);
        Destroy(imgCopy, 4.0f);

        wordDisplayText.transform.gameObject.SetActive(false);
        hangmanStateImage.transform.gameObject.SetActive(false);

        StartCoroutine(ScoreDisplayTextRoutine(controller.NextWordCooldownTime));
    }

    private void WordStartSetup(string newWord)
    {
        wordDisplayText.text = controller.CurrentStateToString('_');
        wordDisplayText.transform.gameObject.SetActive(true);

        hangmanStateImage.transform.gameObject.SetActive(false);
        failedDisplayText.transform.gameObject.SetActive(false);
        onRoundCompleteBackground.transform.gameObject.SetActive(false);
        correctDisplayText.transform.gameObject.SetActive(false);
        correctScoreDisplayText.transform.gameObject.SetActive(false);

        hangmanStateIndex = -1;
    }

    private void DisplayFail(string word, string guessedLetters)
    {
        SoundManager.Instance.PlaySound(wrongAnswerClip);

        failedDisplayText.transform.gameObject.SetActive(true);
        onRoundCompleteBackground.transform.gameObject.SetActive(true);
        wordDisplayText.transform.gameObject.SetActive(false);

        hangmanImageRB.AddRelativeForce(Random.Range(-100, 100), 0, 0);

        failedDisplayText.text = FAIL_SCREEN_INIT_TEXT + word;
    }

    private void GuessWrongEffect(char letter)
    {
        lerpColorRoutine = StartCoroutine(TempColorShiftRoutine(onWrongGuessColor, colorBG, 0.8f));

        hangmanStateImage.transform.gameObject.SetActive(true);
        hangmanStateIndex = Mathf.Min(hangmanStateIndex + 1, wrongAnswerImages.Length - 1);
        hangmanStateImage.texture = wrongAnswerImages[hangmanStateIndex];

        int forceSign = (hangmanStateIndex % 2 == 0) ? 1 : -1;
        Vector3 onWrongForce = new Vector3(333 * forceSign, 0, 0);
        hangmanImageRB.AddRelativeForce(onWrongForce, ForceMode.Impulse);
    }

    private void GuessRightEffect(char letter, int[] indeces)
    {
        // Slightly more performant method of updating the string than getting state again.
        char[] manipArr = wordDisplayText.text.ToCharArray();
        foreach (var index in indeces)
            manipArr[index] = letter;

        wordDisplayText.text = new string(manipArr);
    }
    
    /// <summary>
    /// Enables score display text, updating it every frame at a certain speed to show points earned.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScoreDisplayTextRoutine(float effectTime)
    {
        float roundTimeAtStart = GameManager.Instance.CurrentRoundTime;
        float roundBonus = GameManager.Instance.CurrentRoundTimeBonusFactor;
        int totalPointAward = GameManager.Instance.CalculateCurrentRoundAward;

        // Caching wait times for performance; yield returning a new WaitForSeconds is an underrated fps killer
        float scoreAddWaitTime = 0.3f * effectTime;
        WaitForSeconds topTextWaitTime = new WaitForSeconds(0.2f * effectTime);
        WaitForSeconds bottomTextWaitTime = new WaitForSeconds(0.3f * effectTime);
        WaitForSeconds finalWaitTime = new WaitForSeconds(0.2f * effectTime);

        onRoundCompleteBackground.transform.gameObject.SetActive(true);
        correctDisplayText.transform.gameObject.SetActive(true);
        correctScoreDisplayText.transform.gameObject.SetActive(true);

        // Set top text, wait for fraction of effect time.
        sb.Clear();
        sb.Append(RandomPraiseWords[Random.Range(0, RandomPraiseWords.Length - 1)]).Append("!\n");
        sb.Append(CORRECT_SCREEN_INIT_TEXT);
        sb.Append(roundTimeAtStart.ToString("0.00")).Append("s\n\n");
        correctDisplayText.text = sb.ToString();
        
        yield return topTextWaitTime;

        // Show First half correct display text
        sb.Append("Time Bonus: x").AppendFormat(roundBonus.ToString("0.00")).AppendLine();
        sb.AppendFormat("{0:n0}", GameManager.Instance.WordGuessedPointsAward).Append(" X ").Append(roundBonus.ToString("0.00")).Append(" =\n");
        correctDisplayText.text = sb.ToString();
        sb.Clear();

        yield return bottomTextWaitTime;

        // Every frame, add up towards total point award.
        int currentDisplay = 0;
        int countUntilSound = 0;
        while (currentDisplay < totalPointAward)
        {
            currentDisplay += (int)((Time.deltaTime / scoreAddWaitTime) * totalPointAward);
            currentDisplay = Mathf.Min(currentDisplay, totalPointAward);
            Debug.LogFormat("Current: {0}, Total: {1}", currentDisplay, totalPointAward);
            correctScoreDisplayText.text = currentDisplay.ToString();
            ++countUntilSound;
            if (countUntilSound % 10 == 0)
                SoundManager.Instance.PlaySound(scoreUpClip);
            yield return null;
        }

        yield return finalWaitTime;

        // Effect done, disable stuff
        correctScoreDisplayText.text = "0";
        correctDisplayText.transform.gameObject.SetActive(false);
        correctScoreDisplayText.transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine that sets a raw image's color to the new color, then lerps it a specified amount.
    /// </summary>
    /// <param name="newColor">Color to temporarily set it to</param>
    /// <param name="shiftImage">Image to change the color of</param>
    /// <param name="lerpT">Lerp value, preferrably Time.deltaTime</param>
    private IEnumerator TempColorShiftRoutine(Color newColor, RawImage shiftImage, float lerpT)
    {
        Color originalColor = shiftImage.color;
        shiftImage.color = newColor;

        while (ColorAlmostEquals(shiftImage.color, originalColor))
        {
            shiftImage.color = Color.Lerp(shiftImage.color, originalColor, lerpT * Time.deltaTime);

            yield return null;
        }

        lerpColorRoutine = null;
    }

    // Color.Equals doesn't care about really close values that result from lerping, so this.
    public static bool ColorAlmostEquals(Color a, Color b)
    {
        return (Mathf.Abs(a.a - b.a) < Mathf.Epsilon && Mathf.Abs(a.g - b.g) < Mathf.Epsilon && Mathf.Abs(a.b - b.b) < Mathf.Epsilon);
    }
}
