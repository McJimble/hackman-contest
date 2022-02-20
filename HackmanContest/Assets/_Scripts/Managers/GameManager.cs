using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main gameplay manager for the game. Obviously not good for expanding
/// the scope of the game further in its current state, (as it controls things like UI that
/// it really shouldn't be) but it's sufficient for now
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; protected set; }
    public static Camera MainCamera { get; set; }   // Helps avoid Camera.main calls when reference to main camera is needed.

    [Header("UI")]
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject gameOverHUD;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Stats")]
    [SerializeField] private int wordGuessedPointsAward = 500;
    [SerializeField] private float roundParTime = 30f;
    [SerializeField] private float damageAmountOnWordFailed = 25f;

    private float currentRoundTime = 0f;
    private bool isRoundActive = false;
    private bool awardPoints = true;

    private CharacterMaster player;
    private HangmanController hangmanController;

    public int Score { get; protected set; }
    public int WordGuessedPointsAward { get => wordGuessedPointsAward; }
    public float CurrentRoundTime { get => currentRoundTime; }
    public float CurrentRoundTimeBonusFactor
    {
        get
        {
            if (currentRoundTime <= 0f) return 0f;
            float initialMultiplier = Mathf.Clamp(roundParTime / currentRoundTime, 1f, 5f);
            return Mathf.Round(initialMultiplier * 100f) / 100f;
        }
    }

    public int CalculateCurrentRoundAward
    {
        get
        {
            return (int)(CurrentRoundTimeBonusFactor * wordGuessedPointsAward);
        }
    }

    // Calls when score value gets updated (params: [int] new value of score)
    public event Action<int> onScoreChanged;

    private void Awake()
    {
        // Singleton setup.
        if (Instance)
            Destroy(this);
        Instance = this;
    }

    private void Start()
    {
        player = FindObjectOfType<CharacterMaster>();
        hangmanController = (hangmanController) ? hangmanController : FindObjectOfType<HangmanController>();

        hangmanController.onWordStarted += StartNewRound;
        hangmanController.onWordFailed += WordFailedPenalty;
        hangmanController.onWordGuessed += WordGuessedAward;

        // Quick and dirty, should change this?
        player.PlayerHealth.OnDeath += EnableGameOver;

        gameOverHUD.SetActive(false);
        playerHUD.SetActive(true);

        Score = 0;

        MainCamera = Camera.main;
    }

    private void Update()
    {
        if (isRoundActive)
        {
            currentRoundTime += Time.deltaTime;
        }
    }

    private void StartNewRound(string word)
    {
        if (awardPoints)
            AwardPoints(CalculateCurrentRoundAward);

        awardPoints = false;

        currentRoundTime = 0f;
        isRoundActive = true;
    }

    private void WordFailedPenalty(string word, string guessedLetters)
    {
        DamagePlayer(damageAmountOnWordFailed);
    }

    private void WordGuessedAward(string word, string guessedLetters)
    {
        isRoundActive = false;
        awardPoints = true;
    }

    public void DamagePlayer(float damageAmount)
    {
        player.PlayerHealth.ReceiveDamage(damageAmount, hangmanController);
    }

    public void AwardPoints(int amount)
    {
        Score += amount;
        onScoreChanged?.Invoke(Score);
    }

    private void EnableGameOver(float health, float damage, object sender)
    {
        playerHUD.SetActive(false);
        Invoke("GameOverUIActive", 4f);
    }

    private void GameOverUIActive()
    {
        gameOverScoreText.text = "Game Over!\n You Scored " + Score + " points\nPlay again?";
        gameOverHUD.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // TODO: Change the following two functions after deadline. Super lazy way of reloading/quitting
    // and it just uses random buttons' OnClick UnityEvents. 

    public void ReloadLevel()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
