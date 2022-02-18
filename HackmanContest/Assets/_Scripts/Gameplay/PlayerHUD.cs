using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private CharacterMaster trackedPlayer;
    [Space]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Properties")]
    [SerializeField] private Color gainPointsColor;

    private void Start()
    {
        trackedPlayer.PlayerHealth.OnDamageReceived += UpdateHealthValue;
        GameManager.Instance.onScoreChanged += UpdateScoreValue;

        scoreText.text = GameManager.Instance.Score.ToString();
        UpdateHealthValue(trackedPlayer.PlayerHealth.Health, 0, null);
    }

    private void Update()
    {
        
    }

    private void UpdateHealthValue(float health, float damage, object sender)
    {
        healthText.text = health + " / " + trackedPlayer.PlayerHealth.MaxHealth;
    }

    private void UpdateScoreValue(int score)
    {
        scoreText.text = score.ToString();
        StopAllCoroutines();
        StartCoroutine(TempColorShiftRoutine(gainPointsColor, scoreText, 0.95f));
    }

    /// <summary>
    /// Coroutine that sets a TMP text color to the new color, then lerps it back to the original color by a specified amount.
    /// </summary>
    /// <param name="newColor">Color to temporarily set it to</param>
    /// <param name="shiftText">Image to change the color of</param>
    /// <param name="lerpT">Lerp value, preferrably Time.deltaTime</param>
    private IEnumerator TempColorShiftRoutine(Color newColor, TextMeshProUGUI shiftText, float lerpT)
    {
        Color originalColor = shiftText.color;
        shiftText.color = newColor;

        while (!ColorAlmostEquals(shiftText.color, originalColor))
        {
            shiftText.color = Color.Lerp(shiftText.color, originalColor, lerpT * Time.deltaTime);
            yield return null;
        }
    }

    // Color.Equals doesn't care about really close values that result from lerping, so this.
    public static bool ColorAlmostEquals(Color a, Color b)
    {
        return (Mathf.Abs(a.a - b.a) < Mathf.Epsilon && Mathf.Abs(a.g - b.g) < Mathf.Epsilon && Mathf.Abs(a.b - b.b) < Mathf.Epsilon);
    }
}
