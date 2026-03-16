using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private int playerId = 1;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int hitsPerLife = 5;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    [Header("Hit Cooldown")]
    [SerializeField] private float hitCooldown = 0.4f;

    [Header("Fall Settings")]
    [SerializeField] private float fallCooldown = 1.5f;

    [Header("FX")]
    [SerializeField] private GameObject loseLifeEffect;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private Transform fxPoint;
    [SerializeField] private float loseLifeEffectDestroyTime = 2f;
    [SerializeField] private float deathEffectDestroyTime = 3f;

    private int currentLives;
    private int currentHits;
    private float lastHitTime = -999f;
    private float lastFallTime = -999f;
    private bool isDead = false;

    private RagdollController ragdollController;

    public int PlayerId => playerId;
    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public int CurrentHits => currentHits;
    public bool IsDead => isDead;

    public event Action<int, int> OnLivesChanged;
    public event Action<PlayerHealth> OnPlayerDied;

    private void Awake()
    {
        currentLives = maxLives;
        currentHits = 0;
        ragdollController = GetComponent<RagdollController>();
    }

    private void Start()
    {
        NotifyLivesChanged();
    }

    public void ReceiveHit()
    {
        if (currentLives <= 0 || isDead) return;

        if (Time.time < lastHitTime + hitCooldown)
            return;

        lastHitTime = Time.time;

        currentHits++;
        Debug.Log($"{gameObject.name} recibió golpe. Hits: {currentHits}/{hitsPerLife}");

        if (currentHits >= hitsPerLife)
        {
            currentHits = 0;
            LoseLife(false);
        }
    }

    public void FallOutOfRing()
    {
        if (currentLives <= 0 || isDead) return;

        if (Time.time < lastFallTime + fallCooldown)
            return;

        lastFallTime = Time.time;

        Debug.Log($"{gameObject.name} cayó fuera del ring.");
        currentHits = 0;
        LoseLife(true);
    }

    private void LoseLife(bool shouldRespawn)
    {
        currentLives--;

        if (currentLives < 0)
            currentLives = 0;

        Debug.Log($"{gameObject.name} perdió una vida. Vidas restantes: {currentLives}");

        NotifyLivesChanged();

        if (currentLives <= 0)
        {
            Die();
            return;
        }

        SpawnLoseLifeEffect();

        if (shouldRespawn)
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        if (ragdollController != null)
        {
            ragdollController.MakeActiveRagdoll();
        }

        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in bodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No tiene respawnPoint asignado.");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        SpawnDeathEffect();

        Debug.Log($"{gameObject.name} fue eliminado.");

        if (ragdollController != null)
        {
            ragdollController.MakeRagdoll();
        }

        OnPlayerDied?.Invoke(this);

        gameObject.SetActive(false);
    }

    private void SpawnLoseLifeEffect()
    {
        if (loseLifeEffect == null) return;

        Vector3 spawnPosition = GetFXPosition();
        GameObject fx = Instantiate(loseLifeEffect, spawnPosition, Quaternion.identity);
        Destroy(fx, loseLifeEffectDestroyTime);
    }

    private void SpawnDeathEffect()
    {
        if (deathEffect == null) return;

        Vector3 spawnPosition = GetFXPosition();
        GameObject fx = Instantiate(deathEffect, spawnPosition, Quaternion.identity);
        Destroy(fx, deathEffectDestroyTime);
    }

    private Vector3 GetFXPosition()
    {
        if (fxPoint != null)
            return fxPoint.position;

        return transform.position + Vector3.up * 1f;
    }

    private void NotifyLivesChanged()
    {
        OnLivesChanged?.Invoke(currentLives, maxLives);
    }
}