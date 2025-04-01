using System.Collections;
using UnityEngine;

/// <summary>
/// Controls player movement, rotation, and input handling
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Player movement speed")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Tooltip("Player dodge roll speed multiplier")]
    [SerializeField] private float dodgeSpeed = 2.5f;
    
    [Tooltip("Dodge roll cooldown in seconds")]
    [SerializeField] private float dodgeCooldown = 0.8f;
    
    [Tooltip("Duration of dodge roll in seconds")]
    [SerializeField] private float dodgeDuration = 0.3f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Private variables
    private Vector2 moveDirection;
    private Vector2 lastMoveDirection;
    private Vector2 aimDirection;
    private bool canDodge = true;
    private bool isDodging = false;
    private Camera mainCamera;

    // Animation parameter hashes for performance
    private int moveXHash;
    private int moveYHash;
    private int isMovingHash;
    private int isDodgingHash;

    private void Awake()
    {
        // Get components if not assigned in inspector
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        mainCamera = Camera.main;
        
        // Cache animation parameter hashes
        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        isMovingHash = Animator.StringToHash("IsMoving");
        isDodgingHash = Animator.StringToHash("IsDodging");
    }

    private void Update()
    {
        // Process input if not dodging
        if (!isDodging)
        {
            ProcessMovementInput();
            ProcessAiming();
            ProcessDodgeInput();
        }
        
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        Move();
    }

    /// <summary>
    /// Processes WASD/Arrow key input for movement
    /// </summary>
    private void ProcessMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        moveDirection = new Vector2(moveX, moveY).normalized;
        
        // Store last move direction when actively moving
        if (moveDirection.sqrMagnitude > 0)
        {
            lastMoveDirection = moveDirection;
        }
    }

    /// <summary>
    /// Processes mouse position for aiming
    /// </summary>
    private void ProcessAiming()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;
        
        aimDirection = (worldPosition - transform.position).normalized;
        
        // Flip sprite based on aim direction
        if (aimDirection.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (aimDirection.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    /// <summary>
    /// Processes spacebar input for dodge roll
    /// </summary>
    private void ProcessDodgeInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canDodge)
        {
            StartCoroutine(DodgeRoll());
        }
    }

    /// <summary>
    /// Applies movement to the rigidbody
    /// </summary>
    private void Move()
    {
        if (isDodging)
        {
            // During dodge, movement is handled by the DodgeRoll coroutine
            return;
        }
        
        rb.velocity = moveDirection * moveSpeed;
    }

    /// <summary>
    /// Updates animator parameters based on current state
    /// </summary>
    private void UpdateAnimations()
    {
        bool isMoving = moveDirection.sqrMagnitude > 0;
        
        animator.SetFloat(moveXHash, lastMoveDirection.x);
        animator.SetFloat(moveYHash, lastMoveDirection.y);
        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isDodgingHash, isDodging);
    }

    /// <summary>
    /// Coroutine for dodge roll ability
    /// </summary>
    private IEnumerator DodgeRoll()
    {
        // Start dodge
        isDodging = true;
        canDodge = false;
        
        // Use last move direction if not currently moving
        Vector2 dodgeDirection = moveDirection.sqrMagnitude > 0 ? moveDirection : lastMoveDirection;
        
        // Apply dodge movement
        float endTime = Time.time + dodgeDuration;
        
        while (Time.time < endTime)
        {
            rb.velocity = dodgeDirection * moveSpeed * dodgeSpeed;
            yield return null;
        }
        
        // End dodge
        isDodging = false;
        rb.velocity = Vector2.zero;
        
        // Apply cooldown
        yield return new WaitForSeconds(dodgeCooldown - dodgeDuration);
        
        canDodge = true;
    }
} 