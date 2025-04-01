using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls boss enemy behavior with multiple attack patterns and phases
/// </summary>
public class BossEnemy : MonoBehaviour
{
    [Header("Boss Settings")]
    [Tooltip("Total number of phases")]
    [SerializeField] private int totalPhases = 3;
    
    [Tooltip("Health percentage thresholds to change phases (0-1)")]
    [SerializeField] private List<float> phaseChangeThresholds = new List<float> { 0.7f, 0.3f };
    
    [Tooltip("Invincibility period between phases")]
    [SerializeField] private float phaseTransitionTime = 2f;
    
    [Tooltip("Speed increase per phase")]
    [SerializeField] private float speedIncreasePerPhase = 0.5f;
    
    [Tooltip("Damage increase per phase")]
    [SerializeField] private int damageIncreasePerPhase = 1;
    
    [Header("Special Attack Settings")]
    [Tooltip("Interval between special attacks")]
    [SerializeField] private float specialAttackCooldown = 5f;
    
    [Tooltip("Attack range for special attacks")]
    [SerializeField] private float specialAttackRange = 5f;
    
    [Tooltip("Duration of special attack animation/effect")]
    [SerializeField] private float specialAttackDuration = 1.5f;
    
    [Tooltip("Warning effect before special attack")]
    [SerializeField] private GameObject specialAttackWarning;
    
    [Tooltip("Special attack effect")]
    [SerializeField] private GameObject specialAttackEffect;
    
    [Tooltip("Projectile prefab for ranged attacks")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Header("References")]
    [SerializeField] private EnemyHealth healthComponent;
    [SerializeField] private EnemyAI aiComponent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;
    
    // Private variables
    private int currentPhase = 1;
    private bool isInPhaseTransition = false;
    private float lastSpecialAttackTime = 0f;
    private Transform player;
    private int specialAttackHash;
    private int phaseTransitionHash;
    
    // Events
    public event Action<int> OnPhaseChanged;

    private void Awake()
    {
        // Get components if not assigned in inspector
        if (healthComponent == null) healthComponent = GetComponent<EnemyHealth>();
        if (aiComponent == null) aiComponent = GetComponent<EnemyAI>();
        if (animator == null) animator = GetComponent<Animator>();
        if (attackPoint == null) attackPoint = transform;
        
        // Cache animation parameter hashes
        specialAttackHash = Animator.StringToHash("SpecialAttack");
        phaseTransitionHash = Animator.StringToHash("PhaseTransition");
    }

    private void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Subscribe to health changes to detect phase transitions
        healthComponent.OnHealthChanged += CheckPhaseTransition;
        
        // Hide warning and effect objects initially
        if (specialAttackWarning != null)
            specialAttackWarning.SetActive(false);
        if (specialAttackEffect != null)
            specialAttackEffect.SetActive(false);
            
        // Initialize last special attack time
        lastSpecialAttackTime = Time.time + UnityEngine.Random.Range(1f, 3f);
    }

    private void Update()
    {
        if (player == null || isInPhaseTransition)
            return;
            
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if it's time for a special attack
        if (Time.time >= lastSpecialAttackTime + specialAttackCooldown && 
            distanceToPlayer <= specialAttackRange)
        {
            // Choose a special attack based on current phase
            ChooseSpecialAttack();
            
            // Reset timer
            lastSpecialAttackTime = Time.time;
        }
    }

    /// <summary>
    /// Choose and execute a special attack based on current phase
    /// </summary>
    private void ChooseSpecialAttack()
    {
        // Temporarily disable normal AI during special attack
        if (aiComponent != null)
            aiComponent.enabled = false;
            
        // Different attacks for different phases
        switch (currentPhase)
        {
            case 1:
                StartCoroutine(ExecuteChargeAttack());
                break;
                
            case 2:
                StartCoroutine(ExecuteProjectileBarrage());
                break;
                
            case 3:
                StartCoroutine(ExecuteAreaOfEffectAttack());
                break;
                
            default:
                // Random choice for any additional phases
                int attackChoice = UnityEngine.Random.Range(0, 3);
                
                if (attackChoice == 0)
                    StartCoroutine(ExecuteChargeAttack());
                else if (attackChoice == 1)
                    StartCoroutine(ExecuteProjectileBarrage());
                else
                    StartCoroutine(ExecuteAreaOfEffectAttack());
                break;
        }
    }

    /// <summary>
    /// Special attack 1: Charge toward player
    /// </summary>
    private IEnumerator ExecuteChargeAttack()
    {
        // Set animation trigger
        animator.SetTrigger(specialAttackHash);
        
        // Direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Show warning
        ShowWarningEffect(0.5f);
        
        // Wait for animation/warning
        yield return new WaitForSeconds(0.5f);
        
        // Charge movement
        float chargeSpeed = aiComponent.moveSpeed * 3f;
        float chargeTime = 0.7f;
        float endTime = Time.time + chargeTime;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        while (Time.time < endTime)
        {
            rb.velocity = directionToPlayer * chargeSpeed;
            yield return null;
        }
        
        // Stop after charge
        rb.velocity = Vector2.zero;
        
        // Damage nearby players
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, 1.5f, LayerMask.GetMask("Player"));
        foreach (Collider2D hitPlayer in hitPlayers)
        {
            PlayerHealth playerHealth = hitPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(2 + damageIncreasePerPhase * (currentPhase - 1));
            }
        }
        
        // Cooldown/recovery period
        yield return new WaitForSeconds(1f);
        
        // Re-enable AI
        if (aiComponent != null)
            aiComponent.enabled = true;
    }

    /// <summary>
    /// Special attack 2: Fire multiple projectiles
    /// </summary>
    private IEnumerator ExecuteProjectileBarrage()
    {
        // Set animation trigger
        animator.SetTrigger(specialAttackHash);
        
        // Show warning
        ShowWarningEffect(1f);
        
        // Wait for animation/warning
        yield return new WaitForSeconds(1f);
        
        // Number of projectiles based on phase
        int projectileCount = 4 + (currentPhase - 1) * 2;
        
        // Fire projectiles in a pattern
        for (int i = 0; i < projectileCount; i++)
        {
            // Calculate angle (spread in a cone toward player)
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float spreadAngle = 15f; // Degrees
            float angle = (i - (projectileCount - 1) / 2f) * spreadAngle;
            
            Vector3 projectileDirection = Quaternion.Euler(0, 0, angle) * directionToPlayer;
            
            // Spawn projectile
            if (projectilePrefab != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
                
                // Set up projectile
                Projectile projectileComponent = projectile.GetComponent<Projectile>();
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(projectileDirection, 
                                                  10f, 
                                                  1 + damageIncreasePerPhase * (currentPhase - 1), 
                                                  gameObject.layer);
                }
            }
            
            // Small delay between each projectile
            yield return new WaitForSeconds(0.1f);
        }
        
        // Cooldown/recovery period
        yield return new WaitForSeconds(0.5f);
        
        // Re-enable AI
        if (aiComponent != null)
            aiComponent.enabled = true;
    }

    /// <summary>
    /// Special attack 3: Area of effect attack around the boss
    /// </summary>
    private IEnumerator ExecuteAreaOfEffectAttack()
    {
        // Set animation trigger
        animator.SetTrigger(specialAttackHash);
        
        // Show warning
        ShowWarningEffect(1.5f);
        
        // Wait for animation/warning
        yield return new WaitForSeconds(1.5f);
        
        // Show attack effect
        if (specialAttackEffect != null)
        {
            specialAttackEffect.SetActive(true);
            specialAttackEffect.transform.localScale = Vector3.one * specialAttackRange;
        }
        
        // Deal damage to players in range
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, specialAttackRange, LayerMask.GetMask("Player"));
        foreach (Collider2D hitPlayer in hitPlayers)
        {
            PlayerHealth playerHealth = hitPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(3 + damageIncreasePerPhase * (currentPhase - 1));
            }
        }
        
        // Hide attack effect after a short time
        yield return new WaitForSeconds(0.5f);
        if (specialAttackEffect != null)
        {
            specialAttackEffect.SetActive(false);
        }
        
        // Cooldown/recovery period
        yield return new WaitForSeconds(1f);
        
        // Re-enable AI
        if (aiComponent != null)
            aiComponent.enabled = true;
    }

    /// <summary>
    /// Show warning effect before special attack
    /// </summary>
    private void ShowWarningEffect(float duration)
    {
        if (specialAttackWarning != null)
        {
            StartCoroutine(ShowWarningCoroutine(duration));
        }
    }

    /// <summary>
    /// Coroutine to show and hide warning effect
    /// </summary>
    private IEnumerator ShowWarningCoroutine(float duration)
    {
        specialAttackWarning.SetActive(true);
        yield return new WaitForSeconds(duration);
        specialAttackWarning.SetActive(false);
    }

    /// <summary>
    /// Check if boss should transition to next phase based on health
    /// </summary>
    private void CheckPhaseTransition(int currentHealth)
    {
        if (isInPhaseTransition)
            return;
            
        // Calculate health percentage
        float healthPercent = (float)currentHealth / healthComponent.maxHealth;
        
        // Check thresholds for phase changes
        for (int i = 0; i < phaseChangeThresholds.Count; i++)
        {
            // If health is below threshold and we're still in an earlier phase
            if (healthPercent <= phaseChangeThresholds[i] && currentPhase == i + 1)
            {
                StartCoroutine(TransitionToNextPhase());
                break;
            }
        }
    }

    /// <summary>
    /// Transition to the next phase with effects and behavior changes
    /// </summary>
    private IEnumerator TransitionToNextPhase()
    {
        isInPhaseTransition = true;
        
        // Temporarily make invincible
        bool wasInvincible = healthComponent.isInvincible;
        healthComponent.SetInvincible(true);
        
        // Disable AI during transition
        if (aiComponent != null)
            aiComponent.enabled = false;
        
        // Trigger phase transition animation
        animator.SetTrigger(phaseTransitionHash);
        
        // Play effects, screenshake, etc.
        // ...
        
        // Wait during phase transition
        yield return new WaitForSeconds(phaseTransitionTime);
        
        // Increase phase
        currentPhase++;
        
        // Apply phase-specific stat changes
        if (aiComponent != null)
        {
            aiComponent.moveSpeed += speedIncreasePerPhase;
            aiComponent.attackDamage += damageIncreasePerPhase;
        }
        
        // Notify listeners
        OnPhaseChanged?.Invoke(currentPhase);
        
        // Re-enable AI
        if (aiComponent != null)
            aiComponent.enabled = true;
        
        // Reset invincibility to previous state
        healthComponent.SetInvincible(wasInvincible);
        
        isInPhaseTransition = false;
    }

    /// <summary>
    /// Draw gizmos for visualization in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw special attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, specialAttackRange);
    }
}

/// <summary>
/// Projectile behavior for boss ranged attacks
/// </summary>
public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private int damage;
    private LayerMask ignoreLayer;
    private float lifetime = 5f;

    /// <summary>
    /// Initialize projectile properties
    /// </summary>
    public void Initialize(Vector3 dir, float spd, int dmg, int sourceLayer)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        ignoreLayer = 1 << sourceLayer; // Convert layer index to layermask
        
        // Set rotation to match direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move in specified direction
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collisions with source layer
        if (((1 << other.gameObject.layer) & ignoreLayer) != 0)
            return;
            
        // Deal damage to player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
        
        // Destroy projectile on hit
        Destroy(gameObject);
    }
} 