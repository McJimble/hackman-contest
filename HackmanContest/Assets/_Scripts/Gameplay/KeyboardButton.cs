using UnityEngine;
using System.Collections;
using TMPro;

public class KeyboardButton : MonoBehaviour, IDamageReceiver
{
    [Header("Components")]
    [Tooltip("Hangman controller to send events and listen to. If not set, it" +
        "will try to find one with FindObjectOfType.")]
    [SerializeField] private HangmanController hangmanController;
    [SerializeField] private TextMeshPro displayText;

    [Header("Properties")]
    [SerializeField] private char associatedLetter;
    [SerializeField] private float onClickedZOffset = 0.5f;
    [SerializeField] private Color onEnabledColor;
    [SerializeField] private Color onDisabledColor;
    [SerializeField] private Color onWrongColor;
    [SerializeField] private Color onRightColor;

    private Vector3 startZPosition;
    private Vector3 clickedZPosition;
    private bool isClicked = false;

    private MeshRenderer rend;

    // ---- IDamageReceiver Implementation ---- 

    public bool ReceiveDamage(float damage, object sender = null)
    {
        //Debug.LogFormat("Player Shot '{0}'", associatedLetter);
        if (!hangmanController.GuessIsReady) return false;

        return hangmanController.GuessLetter(associatedLetter);
    }

    private void Awake()
    {
        hangmanController = (hangmanController) ? hangmanController : FindObjectOfType<HangmanController>();
        rend = GetComponent<MeshRenderer>();

        hangmanController.onWordStarted += ReturnToNormalState;
        hangmanController.onAnswerWrong += SetToClickedWrongState;
        hangmanController.onAnswerRight += SetToClickedRightState;
        hangmanController.onWordFailed += WrongUntilNextWord;
        hangmanController.onWordGuessed += RightUntilNextWord;

        displayText.text = associatedLetter.ToString();

        startZPosition = transform.localPosition;
        clickedZPosition = startZPosition + new Vector3(0, 0, onClickedZOffset);
    }

    private void Update()
    {
        if (hangmanController.GuessIsReady && !isClicked)
        {
            SetClickVisuals(onEnabledColor, startZPosition);
        }
    }

    private void RightUntilNextWord(string word, string lettersGuessed)
    {
        SetClickVisuals(onRightColor, clickedZPosition);
    }

    private void WrongUntilNextWord(string word, string lettersGuessed)
    {
        SetClickVisuals(onWrongColor, clickedZPosition);
    }

    private void ReturnToNormalState(string word)
    {
        isClicked = false;
        StopAllCoroutines();
        StartCoroutine(LerpToLocalPosition(startZPosition, 0.5f));
    }

    private void SetToClickedWrongState(char letter)
    {
        if (isClicked) return;

        if ((char.ToLower(letter) == char.ToLower(associatedLetter) && !isClicked))
        {
            isClicked = true;
            SetClickVisuals(onWrongColor, clickedZPosition);
        }
        else
            SetClickVisuals(onDisabledColor, startZPosition);
    }

    private void SetToClickedRightState(char letter, int[] indices)
    {
        if (isClicked) return;

        if (char.ToLower(letter) == char.ToLower(associatedLetter))
        {
            isClicked = true;
            SetClickVisuals(onRightColor, clickedZPosition);
        }
        else
            SetClickVisuals(onDisabledColor, startZPosition);
    }

    private void SetClickVisuals(Color setCol, Vector3 lerpPosition)
    {
        StopAllCoroutines();
        StartCoroutine(LerpToLocalPosition(lerpPosition, 0.4f));

        rend.material.color = setCol;
    }

    private IEnumerator LerpToLocalPosition(Vector3 toPosition, float t)
    {
        while (transform.localPosition != toPosition)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, toPosition, t);
            yield return null;
        }
    }
}
