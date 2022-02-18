using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour, IDamageReceiver
{
    [SerializeField] AudioClip onHitSound;
    [SerializeField] private float maxHealth;
    [Space]
    [Tooltip("If false, no blinking occurs when damage received.")]
    [SerializeField] private bool enableHitBlinking = true;
    [SerializeField] private Renderer blinkMesh;
    [SerializeField] private float hitBlinkDuration = 0.05f; 
    [SerializeField] private float hitBlinkIntensity = 7.5f;
    [Space]
    [SerializeField] private GameObject[] onHitEffectPrefabs;
    [SerializeField] private GameObject[] onDeathEffectPrefabs;

    [Header("UI")]
    [Tooltip("Health bar visual to change. Must be instanced in the world already, preferably with this object as a parent!")]
    [SerializeField] private HealthBarUI healthBar;

    private float currentHealth;
    private float blinkTimer = 0f;

    // Params for both: params: [float] new value of health after damage received, [float] amount of damage received, [object] object that sent the damage.
    public event Action<float, float, object> OnDamageReceived; // Called when health is successfully reduced.
    public event Action<float, float, object> OnDeath;          // Called when IsAlive is first set to false.

    public bool CanTakeDamage { get; protected set; }


    public float Health { get => currentHealth;}
    public float MaxHealth { get => maxHealth; }
    public bool IsAlive { get => (Health >= 0f); }

    public bool ReceiveDamage(float damage, object sender = null)
    {
        if (CanTakeDamage)
        {
            currentHealth -= damage;
            OnDamageReceived?.Invoke(currentHealth, damage, sender);

            blinkTimer = hitBlinkDuration;

            if (currentHealth <= 0f)
            {
                OnDeath?.Invoke(currentHealth, damage, sender);
            }

            return true;
        }

        return false;
    }

    public void ShowHealthbar(bool enable)
    {
        if (healthBar)
        {
            healthBar.gameObject.SetActive(enable);
        }
    }

    private void Awake()
    {
        blinkMesh = (blinkMesh) ? blinkMesh : GetComponentInChildren<MeshRenderer>();

        currentHealth = maxHealth;
        CanTakeDamage = true;

        OnDeath += SpawnDeathPrefabs;
        OnDamageReceived += SpawnHitPrefabs;

        if (healthBar)
        {
            OnDeath += HealthbarEffectOnDeath;
            OnDamageReceived += HealthbarEffectOnHit;
        }
    }
    private void Update()
    {
        if (enableHitBlinking && blinkTimer > 0f)
        {
            blinkTimer -= Time.deltaTime;
            float lerpT = Mathf.Clamp01(blinkTimer / hitBlinkDuration);
            float intensity = Mathf.Max(1, lerpT * hitBlinkIntensity);
            blinkMesh.material.color = Color.white * intensity;
        }
    }

    private void HealthbarEffectOnHit(float health, float damage, object sender)
    {
        healthBar.SetHealthBarPercentage(currentHealth / maxHealth);
    }

    private void HealthbarEffectOnDeath(float health, float damage, object sender)
    {
        healthBar.gameObject.SetActive(false);
    }

    private void SpawnDeathPrefabs(float health, float damage, object sender)
    {
        foreach (var effect in onDeathEffectPrefabs)
        {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation, transform.parent);

            Destroy(newEffect, 5f);
        }
    }

    private void SpawnHitPrefabs(float health, float damage, object sender)
    {
        foreach (var effect in onHitEffectPrefabs)
        {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation, transform.parent);

            Destroy(newEffect, 5f);
        }
    }
}
