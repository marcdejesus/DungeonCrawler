using System.Collections;
using UnityEngine;

/// <summary>
/// Controls enemy AI behavior with different states and movement patterns
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Distance at which enemy can see player")]
    [SerializeField] private float detectionRange = 5f;
    
    [Tooltip("Distance at which enemy will attack")]
    [SerializeField] private float attackRange = 1.2f;
    
    [Tooltip("Layers that block enemy vision")]
    [SerializeField] private LayerMask obstacleLayers;
    
    [Header("Movement Settings")]
    [Tooltip("Enemy movement speed")]
    [SerializeField] public float moveSpeed = 2.5f;
    
    [Tooltip("Radius of wandering circle")]
    [SerializeField] private float wanderRadius = 3f;
    
    [Tooltip("Time between changing wander direction")]
    [SerializeField] private float wanderChangeTime = 2f;
    
    [Header("Attack Settings")]
    [Tooltip("Damage dealt by attack")]
    [SerializeField] public int attackDamage = 1;
    
    [Tooltip("Cooldown between attacks")]
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // Private variables
    private Transform player;
    private Vector2 startPosition;
    private Vector2 moveDirection;
    private EnemyState currentState;
    private float lastAttackTime;
    private float nextWanderChangeTime;
    
    // Animation parameter hashes
    private int moveXHash;
    private int moveYHash;
    private int isMovingHash;
    private int attackHash;

    /// <summary>
    /// Enemy AI states
    /// </summary>
    private enum EnemyState
    {
        Idle,
        Wander,
        Chase,
        Attack,
        Return
    }

    private void Awake()
    {
        // Get components if not assigned in inspector
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Cache animation parameter hashes
        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        isMovingHash = Animator.StringToHash("IsMoving");
        attackHash = Animator.StringToHash("Attack");
    }

    private void Start()
    {
        // Get player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Store starting position for return behavior
        startPosition = transform.position;
        
        // Set initial state
        ChangeState(EnemyState.Idle);
        
        // Set initial wander direction change time
        nextWanderChangeTime = Time.time + wanderChangeTime;
    }

    private void Update()
    {
        if (player == null)
            return;
            
        // Check for state transitions
        UpdateState();
        
        // Update behavior based on current state
        switch (currentState)
        {
            case EnemyState.Idle:
                // Just hanging out
                HandleIdleState();
                break;
                
            case EnemyState.Wander:
                // Randomly moving around
                HandleWanderState();
                break;
                
            case EnemyState.Chase:
                // Following the player
                HandleChaseState();
                break;
                
            case EnemyState.Attack:
                // Attacking the player
                HandleAttackState();
                break;
                
            case EnemyState.Return:
                // Returning to starting position
                HandleReturnState();
                break;
        }
        
        // Update animations
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        if (currentState != EnemyState.Attack)
        {
            Move();
        }
        else
        {
            // Stop movement during attack
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// Determines whether to change states based on conditions
    /// </summary>
    private void UpdateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is visible
        bool canSeePlayer = CanSeePlayer();
        
        switch (currentState)
        {
            case EnemyState.Idle:
                // Transition to wander after a while
                if (Time.time > nextWanderChangeTime)
                {
                    ChangeState(EnemyState.Wander);
                }
                // If player is detected, chase
                else if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
                
            case EnemyState.Wander:
                // If wandered too far, return to start
                if (Vector2.Distance(transform.position, startPosition) > wanderRadius * 1.5f)
                {
                    ChangeState(EnemyState.Return);
                }
                // If player is detected, chase
                else if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
                
            case EnemyState.Chase:
                // If player is in attack range, attack
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                // If player gets too far or not visible, return to wandering
                else if (distanceToPlayer > detectionRange * 1.5f || !canSeePlayer)
                {
                    ChangeState(EnemyState.Return);
                }
                break;
                
            case EnemyState.Attack:
                // If player moves out of attack range, chase
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
                
            case EnemyState.Return:
                // If close to start position, go idle
                if (Vector2.Distance(transform.position, startPosition) < 0.5f)
                {
                    ChangeState(EnemyState.Idle);
                }
                // If player is detected on the way back, chase
                else if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
        }
    }

    /// <summary>
    /// Change to a new state
    /// </summary>
    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
        
        // Reset state-specific variables
        switch (newState)
        {
            case EnemyState.Idle:
                moveDirection = Vector2.zero;
                nextWanderChangeTime = Time.time + Random.Range(1f, 3f);
                break;
                
            case EnemyState.Wander:
                ChooseRandomWanderDirection();
                nextWanderChangeTime = Time.time + wanderChangeTime;
                break;
                
            case EnemyState.Attack:
                // Face the player when attacking
                FaceTarget(player.position);
                break;
        }
    }

    /// <summary>
    /// Handle behavior in idle state
    /// </summary>
    private void HandleIdleState()
    {
        // Just wait - no movement
        moveDirection = Vector2.zero;
    }

    /// <summary>
    /// Handle behavior in wander state
    /// </summary>
    private void HandleWanderState()
    {
        // Check if it's time to change direction
        if (Time.time >= nextWanderChangeTime)
        {
            ChooseRandomWanderDirection();
            nextWanderChangeTime = Time.time + wanderChangeTime;
        }
        
        // Avoid wandering too far from start position
        if (Vector2.Distance(transform.position, startPosition) > wanderRadius)
        {
            // Start moving back toward the center
            moveDirection = (startPosition - (Vector2)transform.position).normalized;
        }
    }

    /// <summary>
    /// Handle behavior in chase state
    /// </summary>
    private void HandleChaseState()
    {
        if (player != null)
        {
            // Move toward player
            moveDirection = (player.position - transform.position).normalized;
            
            // Face the direction of movement
            FaceTarget(player.position);
        }
    }

    /// <summary>
    /// Handle behavior in attack state
    /// </summary>
    private void HandleAttackState()
    {
        if (player != null)
        {
            // Face the player
            FaceTarget(player.position);
            
            // Attack if cooldown has passed
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Handle behavior in return state
    /// </summary>
    private void HandleReturnState()
    {
        // Move toward starting position
        moveDirection = (startPosition - (Vector2)transform.position).normalized;
        
        // Face the direction of movement
        FaceDirection(moveDirection);
    }

    /// <summary>
    /// Execute an attack
    /// </summary>
    private void Attack()
    {
        // Trigger attack animation
        animator.SetTrigger(attackHash);
        
        // Check if player is still in range and apply damage
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            // Get player health component and apply damage
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// Applies movement to the rigidbody
    /// </summary>
    private void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    /// <summary>
    /// Choose a random direction for wandering
    /// </summary>
    private void ChooseRandomWanderDirection()
    {
        // Random angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Convert to direction vector
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        
        // Face the direction of movement
        FaceDirection(moveDirection);
    }

    /// <summary>
    /// Face a specific target position
    /// </summary>
    private void FaceTarget(Vector3 target)
    {
        Vector2 direction = ((Vector2)target - (Vector2)transform.position).normalized;
        FaceDirection(direction);
    }

    /// <summary>
    /// Face in a specific direction
    /// </summary>
    private void FaceDirection(Vector2 direction)
    {
        // Flip sprite based on horizontal direction
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    /// <summary>
    /// Check if player is visible (not blocked by obstacles)
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null)
            return false;
            
        // Direction to player
        Vector2 directionToPlayer = player.position - transform.position;
        
        // Distance to player
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Check line of sight
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer.normalized,
            distanceToPlayer,
            obstacleLayers
        );
        
        // No obstacles hit means player is visible
        return hit.collider == null;
    }

    /// <summary>
    /// Updates animator parameters based on current state
    /// </summary>
    private void UpdateAnimations()
    {
        bool isMoving = moveDirection.sqrMagnitude > 0;
        
        animator.SetFloat(moveXHash, moveDirection.x);
        animator.SetFloat(moveYHash, moveDirection.y);
        animator.SetBool(isMovingHash, isMoving);
    }

    /// <summary>
    /// Draw gizmos for visualization in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw wander radius (from start position in Play mode)
        Gizmos.color = Color.blue;
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
} 