using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages player health, damage, invincibility frames and death
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum player health")]
    [SerializeField] private int maxHealth = 6;
    
    [Tooltip("Duration of invincibility after taking damage (in seconds)")]
    [SerializeField] private float invincibilityDuration = 1f;
    
    [Tooltip("Flash rate during invincibility (flashes per second)")]
    [SerializeField] private float flashRate = 10f;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    // Private variables
    private int currentHealth;
    private bool isInvincible = false;
    private int isDamagedHash;
    private int dieHash;
    
    // Public properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    // Events
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnPlayerDeath;

    private void Awake()
    {
        // Get components if not assigned in inspector
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        
        // Cache animation parameter hashes
        isDamagedHash = Animator.StringToHash("IsDamaged");
        dieHash = Animator.StringToHash("Die");
    }

    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Notify UI of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Method to take damage from enemies or hazards
    /// </summary>
    /// <param name="damageAmount">Amount of damage to take</param>
    public void TakeDamage(int damageAmount)
    {
        // Ignore damage during invincibility frames
        if (isInvincible)
            return;
            
        // Apply damage
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        
        // Notify listeners about health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        
        // Otherwise apply damage reaction
        animator.SetTrigger(isDamagedHash);
        StartCoroutine(InvincibilityFrames());
    }

    /// <summary>
    /// Method to heal the player
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    public void Heal(int healAmount)
    {
        // Apply healing, capped at max health
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        // Notify listeners about health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Increase max health (used for upgrades)
    /// </summary>
    /// <param name="amount">Amount to increase max health by</param>
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // Optional: heal when max health increases
        
        // Notify listeners about health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void Die()
    {
        // Trigger death animation
        animator.SetTrigger(dieHash);
        
        // Disable player controller
        GetComponent<PlayerController>().enabled = false;
        
        // Disable collisions
        GetComponent<Collider2D>().enabled = false;
        
        // Notify game manager
        OnPlayerDeath?.Invoke();
    }

    /// <summary>
    /// Coroutine for invincibility frames after taking damage
    /// </summary>
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Flash the sprite during invincibility
        float endTime = Time.time + invincibilityDuration;
        bool visible = false;
        
        while (Time.time < endTime)
        {
            // Toggle visibility
            visible = !visible;
            spriteRenderer.enabled = visible;
            
            // Wait based on flash rate
            yield return new WaitForSeconds(1f / flashRate / 2f);
        }
        
        // Ensure sprite is visible after invincibility ends
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
} 