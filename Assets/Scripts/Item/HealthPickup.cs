using UnityEngine;

/// <summary>
/// Health pickup item that restores player health
/// </summary>
public class HealthPickup : ItemBase
{
    [Header("Health Settings")]
    [Tooltip("Amount of health to restore")]
    [SerializeField] private int healAmount = 1;
    
    [Tooltip("Whether this is a max health upgrade")]
    [SerializeField] private bool isMaxHealthUpgrade = false;
    
    /// <summary>
    /// Apply health restoration effect
    /// </summary>
    protected override void ApplyItemEffect()
    {
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                if (isMaxHealthUpgrade)
                {
                    // Increase max health
                    playerHealth.IncreaseMaxHealth(healAmount);
                }
                else
                {
                    // Heal player
                    playerHealth.Heal(healAmount);
                }
            }
        }
    }
} 