using System;
using UnityEngine;

/// <summary>
/// Manages enemy health, damage handling, and death
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum enemy health")]
    [SerializeField] public int maxHealth = 3;
    
    [Tooltip("Does this enemy flash on hit?")]
    [SerializeField] private bool flashOnHit = true;
    
    [Tooltip("Flash duration in seconds")]
    [SerializeField] private float flashDuration = 0.1f;
    
    [Header("Death Settings")]
    [Tooltip("Destroy enemy on death after delay")]
    [SerializeField] private bool destroyOnDeath = true;
    
    [Tooltip("Delay before destroying enemy GameObject")]
    [SerializeField] private float destroyDelay = 1.5f;
    
    [Tooltip("Minimum items dropped on death")]
    [SerializeField] private int minDrops = 0;
    
    [Tooltip("Maximum items dropped on death")]
    [SerializeField] private int maxDrops = 2;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    // Private variables
    private int currentHealth;
    private Material originalMaterial;
    private Material flashMaterial;
    private int dieHash;
    
    // Public properties
    public bool isInvincible { get; private set; } = false;
    
    // Events
    public event Action<int> OnHealthChanged;
    public event Action OnEnemyDeath;

    private void Awake()
    {
        // Get components if not assigned in inspector
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        
        // Cache animation parameter hashes
        dieHash = Animator.StringToHash("Die");
        
        // Cache the original material
        originalMaterial = spriteRenderer.material;
        
        // Create a material for flash effect
        // Note: This assumes you have a white flash material in Resources
        // You'd need to create this material with a shader that replaces color with white
        flashMaterial = Resources.Load<Material>("Materials/WhiteFlash");
    }

    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Apply damage to the enemy
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply</param>
    public void TakeDamage(int damageAmount)
    {
        // Ignore damage when invincible
        if (isInvincible)
            return;
            
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        
        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth);
        
        // Show hit feedback
        if (flashOnHit && flashMaterial != null)
        {
            FlashSprite();
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Set invincibility state
    /// </summary>
    /// <param name="invincible">Whether to make the enemy invincible</param>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    /// <summary>
    /// Flash the sprite white briefly to indicate damage
    /// </summary>
    private void FlashSprite()
    {
        spriteRenderer.material = flashMaterial;
        Invoke(nameof(ResetMaterial), flashDuration);
    }

    /// <summary>
    /// Reset material after flash effect
    /// </summary>
    private void ResetMaterial()
    {
        spriteRenderer.material = originalMaterial;
    }

    /// <summary>
    /// Handle enemy death
    /// </summary>
    private void Die()
    {
        // Trigger death animation if available
        if (animator != null)
        {
            animator.SetTrigger(dieHash);
        }
        
        // Disable components
        GetComponent<Collider2D>().enabled = false;
        
        // Disable any AI or movement scripts
        // This would vary based on your implementation
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Don't disable this script
            if (script != this && script.GetType().Name != "EnemyHealth")
            {
                script.enabled = false;
            }
        }
        
        // Spawn drops
        DropItems();
        
        // Notify listeners
        OnEnemyDeath?.Invoke();
        
        // Destroy GameObject after delay
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    /// <summary>
    /// Drop items upon death
    /// </summary>
    private void DropItems()
    {
        // Determine number of drops
        int dropCount = UnityEngine.Random.Range(minDrops, maxDrops + 1);
        
        // This would integrate with your item system
        // Example implementation:
        // for (int i = 0; i < dropCount; i++)
        // {
        //     ItemManager.Instance.DropRandomItem(transform.position);
        // }
    }
}