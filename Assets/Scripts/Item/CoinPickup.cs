using UnityEngine;

/// <summary>
/// Coin pickup that adds to player's currency
/// </summary>
public class CoinPickup : ItemBase
{
    [Header("Coin Settings")]
    [Tooltip("Value of this coin")]
    [SerializeField] private int coinValue = 1;
    
    [Tooltip("Spin animation speed")]
    [SerializeField] private float spinSpeed = 90f;
    
    [Tooltip("Hover animation height")]
    [SerializeField] private float hoverHeight = 0.1f;
    
    [Tooltip("Hover animation speed")]
    [SerializeField] private float hoverSpeed = 2f;
    
    // Private variables
    private Vector3 startPosition;
    
    /// <summary>
    /// Store starting position for hover effect
    /// </summary>
    protected override void Start()
    {
        base.Start();
        
        // Store start position for hover effect
        startPosition = transform.position;
    }
    
    /// <summary>
    /// Apply animations
    /// </summary>
    protected override void Update()
    {
        base.Update();
        
        // Apply spin animation
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
        
        // Apply hover animation
        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
    }
    
    /// <summary>
    /// Add coins to player's currency
    /// </summary>
    protected override void ApplyItemEffect()
    {
        // Add coins to game manager
        GameManager gameManager = GameManager.Instance;
        
        if (gameManager != null)
        {
            gameManager.AddCoins(coinValue);
        }
    }
} 