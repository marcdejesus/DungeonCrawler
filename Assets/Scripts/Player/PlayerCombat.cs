using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player attacks, weapons, and combat interactions
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Base attack speed (attacks per second)")]
    [SerializeField] private float baseAttackRate = 1f;
    
    [Tooltip("Base attack damage")]
    [SerializeField] private int baseAttackDamage = 1;
    
    [Tooltip("Attack range")]
    [SerializeField] private float attackRange = 1.5f;
    
    [Header("References")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask enemyLayers;
    
    // Private variables
    private float nextAttackTime = 0f;
    private int attackHash;
    private Weapon currentWeapon;
    private Camera mainCamera;
    
    // Events
    public event Action<Weapon> OnWeaponChanged;

    private void Awake()
    {
        // Get components if not assigned in inspector
        if (animator == null) animator = GetComponent<Animator>();
        if (attackPoint == null) attackPoint = transform;
        
        mainCamera = Camera.main;
        
        // Cache animation parameter hashes
        attackHash = Animator.StringToHash("Attack");
    }

    private void Update()
    {
        RotateAttackPointTowardsMouse();
        ProcessAttackInput();
        ProcessWeaponSwitch();
    }

    /// <summary>
    /// Rotate the attack point towards the mouse cursor
    /// </summary>
    private void RotateAttackPointTowardsMouse()
    {
        // Get mouse position in world space
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        
        // Calculate direction to mouse
        Vector3 direction = mousePos - transform.position;
        
        // Calculate angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Apply rotation to attack point
        attackPoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Processes mouse input for attacks
    /// </summary>
    private void ProcessAttackInput()
    {
        if (Time.time >= nextAttackTime && Input.GetMouseButton(0))
        {
            Attack();
            
            // Set cooldown based on attack rate
            float attackCooldown = 1f / (baseAttackRate * (currentWeapon != null ? currentWeapon.attackSpeedMultiplier : 1f));
            nextAttackTime = Time.time + attackCooldown;
        }
    }
    
    /// <summary>
    /// Process inputs for weapon switching (1-4 keys)
    /// </summary>
    private void ProcessWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            // Switch to weapon slot 1
            // Implementation depends on inventory system
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            // Switch to weapon slot 2
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            // Switch to weapon slot 3
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            // Switch to weapon slot 4
        }
    }

    /// <summary>
    /// Execute attack action
    /// </summary>
    private void Attack()
    {
        // Trigger attack animation
        animator.SetTrigger(attackHash);
        
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, 
            attackRange * (currentWeapon != null ? currentWeapon.rangeMultiplier : 1f), 
            enemyLayers);
        
        // Calculate final damage
        int damage = Mathf.RoundToInt(baseAttackDamage * (currentWeapon != null ? currentWeapon.damageMultiplier : 1f));
        
        // Apply damage to enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            // Get enemy health component and apply damage
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            // Apply weapon-specific effects if available
            if (currentWeapon != null && currentWeapon.hasSpecialEffect)
            {
                ApplyWeaponSpecialEffect(enemy);
            }
        }
    }

    /// <summary>
    /// Apply special effects from weapons (to be implemented per weapon)
    /// </summary>
    private void ApplyWeaponSpecialEffect(Collider2D enemy)
    {
        // This would be implemented based on weapon type
        // For example: poison, slow, stun, etc.
        if (currentWeapon.weaponType == WeaponType.Sword)
        {
            // Sword-specific effects
        }
        else if (currentWeapon.weaponType == WeaponType.Bow)
        {
            // Bow-specific effects
        }
        // And so on...
    }

    /// <summary>
    /// Equip a new weapon
    /// </summary>
    /// <param name="weapon">Weapon to equip</param>
    public void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        
        // Update visuals, could be done by changing sprites or enabling/disabling weapon game objects
        // ... implementation
        
        // Notify listeners about weapon change
        OnWeaponChanged?.Invoke(currentWeapon);
    }

    /// <summary>
    /// Draw attack range gizmos in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
            
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

/// <summary>
/// Enum for weapon types
/// </summary>
public enum WeaponType
{
    Sword,
    Bow,
    Staff,
    Dagger,
    Axe,
    Mace
}

/// <summary>
/// Class representing a weapon with its properties
/// </summary>
[System.Serializable]
public class Weapon
{
    public string weaponName;
    public WeaponType weaponType;
    public Sprite weaponSprite;
    
    // Stat multipliers
    public float damageMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;
    public float rangeMultiplier = 1f;
    
    // Special effects
    public bool hasSpecialEffect = false;
    public string effectDescription;
    
    // Additional properties could include:
    // - Projectile prefab for ranged weapons
    // - Area of effect size
    // - Effect duration
    // - etc.
} 