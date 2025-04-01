using UnityEngine;

/// <summary>
/// Base class for all item types in the game
/// </summary>
public abstract class ItemBase : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Unique ID for the item")]
    [SerializeField] private string itemID;
    
    [Tooltip("Display name of the item")]
    [SerializeField] private string itemName;
    
    [Tooltip("Item description")]
    [SerializeField] private string description;
    
    [Tooltip("Item icon")]
    [SerializeField] private Sprite icon;
    
    [Tooltip("Item rarity")]
    [SerializeField] private ItemRarity rarity = ItemRarity.Common;
    
    [Header("Pickup Settings")]
    [Tooltip("Auto pickup when player gets near")]
    [SerializeField] private bool autoPickup = true;
    
    [Tooltip("Pickup range for auto pickup")]
    [SerializeField] private float pickupRange = 1.5f;
    
    [Tooltip("Pickup effect prefab")]
    [SerializeField] private GameObject pickupEffectPrefab;
    
    [Tooltip("Sound to play on pickup")]
    [SerializeField] private AudioClip pickupSound;
    
    // Properties
    public string ItemID => itemID;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemRarity Rarity => rarity;
    public bool AutoPickup => autoPickup;
    
    // References
    protected Transform player;
    
    protected virtual void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    protected virtual void Update()
    {
        // Check for auto pickup
        if (autoPickup && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= pickupRange)
            {
                OnPickup();
            }
        }
    }
    
    /// <summary>
    /// Called when item is picked up
    /// </summary>
    public virtual void OnPickup()
    {
        // Spawn pickup effect
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play pickup sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Apply item effect
        ApplyItemEffect();
        
        // Destroy the item GameObject
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Apply the item's effect when picked up
    /// </summary>
    protected abstract void ApplyItemEffect();
    
    /// <summary>
    /// Get color based on item rarity
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
                
            case ItemRarity.Uncommon:
                return Color.green;
                
            case ItemRarity.Rare:
                return Color.blue;
                
            case ItemRarity.Epic:
                return new Color(0.5f, 0f, 0.5f); // Purple
                
            case ItemRarity.Legendary:
                return new Color(1f, 0.5f, 0f); // Orange
                
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Draw pickup range gizmo
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}

/// <summary>
/// Item rarity levels
/// </summary>
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
} 