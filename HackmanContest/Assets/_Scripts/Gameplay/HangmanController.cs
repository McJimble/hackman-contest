using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(HackmanAPIHandler))]
public class HangmanController : MonoBehaviour
{
    [SerializeField] private int maxWrongGuesses = 7;
    [SerializeField] private float guessStandbyTime;
    [SerializeField] private float nextWordCooldownTime;

    private HackmanAPIHandler wordHandler;

    private List<char> guessedLetters = new List<char>();
    private string currentWord;
    private string queuedWord;

    public event Action<char> onAnswerWrong;            // A letter in the current word has been wrongly guessed (params: [char] letter guessed)
    public event Action<char, int[]> onAnswerRight;     // A letter in the current word has been correctly guessed (params: [char] letter guessed, [int[]] indeces of correct letter)
    public event Action<string> onWordStarted;          // New word has been received and is now up to be guessed (params: [string] the newly started word)
    public event Action<string, string> onWordGuessed;  // Current word was successfully guessed (params: [string] the word that was correctly guessed, [string] what letters were guessed)
    public event Action<string, string> onWordFailed;   // Word was failed to be guessed. Hangman round was "lost" (params: [string] what the correct word was, [string] what letters were guessed)

    public int CorrectGuessesRemaining { get; private set; }
    public int WrongGuessesRemaining { get; private set; }
    public float StandbyTimeRemaining { get; private set; }
    public float NextWordCooldownTime { get => nextWordCooldownTime; }
    public bool GuessIsReady { get => StandbyTimeRemaining < 0f && !string.IsNullOrEmpty(currentWord); }

    private void Awake()
    {
        wordHandler = GetComponent<HackmanAPIHandler>();
        WrongGuessesRemaining = maxWrongGuesses;
        CorrectGuessesRemaining = 0;
        StandbyTimeRemaining = nextWordCooldownTime;
    }

    private void Update()
    {
        StandbyTimeRemaining -= Time.deltaTime;

        if (string.IsNullOrEmpty(queuedWord) && wordHandler.CanAPICheck)
        {
            wordHandler.GetWord(WordReceivedCallback);
        }

        // Do not use try and use queued word if we are currently on standby.
        if (StandbyTimeRemaining > 0f)
        {
            return;
        }

        // A word is queued up and there is no current word, so start a round with queued word
        if (!string.IsNullOrEmpty(queuedWord) && string.IsNullOrEmpty(currentWord))
        {
            currentWord = new string(queuedWord);
            queuedWord = null;
            CorrectGuessesRemaining = currentWord.Length;

            onWordStarted?.Invoke(currentWord);
        }
    }

    private void WordReceivedCallback(string newWord)
    {
        queuedWord = new string(newWord);
        Debug.Log(newWord);
    }

    private void ResetHangmanState()
    {
        CorrectGuessesRemaining = 0;
        WrongGuessesRemaining = maxWrongGuesses;
        StandbyTimeRemaining = nextWordCooldownTime;

        currentWord = null;
        guessedLetters.Clear();
    }

    private void UpdateWrongGuessesRemaining()
    {
        --WrongGuessesRemaining;
        StandbyTimeRemaining = guessStandbyTime;

        if (WrongGuessesRemaining <= 0)
        {
            onWordFailed?.Invoke(currentWord, guessedLetters.ToString());
            ResetHangmanState();
        }
    }

    private void UpdateCorrectGuessesRemaining(int[] indeces)
    {
        CorrectGuessesRemaining -= indeces.Length;
        StandbyTimeRemaining = guessStandbyTime;

        if (CorrectGuessesRemaining <= 0)
        {
            onWordGuessed?.Invoke(currentWord, guessedLetters.ToString());
            ResetHangmanState();
        }
    }

    /// <summary>
    /// Guess a letter on this controller. Returns whether the guess was SUCCESSFULLY
    /// parsed by the controller, not whether it was correct.
    /// </summary>
    /// <param name="letter">char to guess</param>
    /// <returns>Whether the controller successfully parsed the guess and was therefore affected by it in some way.</returns>
    public bool GuessLetter(char letter)
    {
        letter = char.ToLower(letter);

        // If no word queued, failed to guess.
        if (currentWord == null) return false;

        // If letter already guessed, failed.
        if (guessedLetters.Contains(letter)) return false;
        guessedLetters.Add(letter);

        // Check current word for letter given and store indeces where it was found.
        List<int> correctIndeces = new List<int>();
        for (int i = 0; i < currentWord.Length; i++)
        {
            if (currentWord[i] == letter)
                correctIndeces.Add(i);
        }

        // Guess was correct if any indeces were found.
        if (correctIndeces.Count <= 0)
        {
            onAnswerWrong?.Invoke(letter);
            UpdateWrongGuessesRemaining();
        }
        else
        {
            int[] toSend = correctIndeces.ToArray();
            onAnswerRight?.Invoke(letter, toSend);
            UpdateCorrectGuessesRemaining(toSend);
        }

        Debug.LogFormat("Correct: {0}, Wrong Left: {1}", CorrectGuessesRemaining, WrongGuessesRemaining);
        Debug.Log(CurrentStateToString('_'));

        return true;
    }

    public string CurrentStateToString(char fillerChar = ' ')
    {
        if (currentWord == null) return null;

        char[] ret = new char[currentWord.Length];
        for (int i = 0; i < ret.Length; ++i)
        {
            ret[i] = (guessedLetters.Contains(currentWord[i])) ? currentWord[i] : fillerChar;
        }

        return new string(ret);
    }
}
