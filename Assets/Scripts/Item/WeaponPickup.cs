using UnityEngine;

/// <summary>
/// Weapon pickup that equips a new weapon for the player
/// </summary>
public class WeaponPickup : ItemBase
{
    [Header("Weapon Settings")]
    [Tooltip("Weapon data")]
    [SerializeField] private Weapon weaponData;
    
    [Tooltip("Weapon model to display")]
    [SerializeField] private GameObject weaponModel;
    
    [Tooltip("Rotation speed for display")]
    [SerializeField] private float rotationSpeed = 45f;
    
    /// <summary>
    /// Rotate weapon for display
    /// </summary>
    protected override void Update()
    {
        base.Update();
        
        // Rotate the weapon model
        if (weaponModel != null)
        {
            weaponModel.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    /// <summary>
    /// Equip the weapon on pickup
    /// </summary>
    protected override void ApplyItemEffect()
    {
        if (player != null && weaponData != null)
        {
            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            
            if (playerCombat != null)
            {
                // Equip the weapon
                playerCombat.EquipWeapon(weaponData);
                
                // Show pickup message
                // You could implement a notification system to display what weapon was picked up
                Debug.Log($"Picked up {weaponData.weaponName}");
            }
        }
    }
    
    /// <summary>
    /// Set the weapon data and update the model
    /// </summary>
    /// <param name="weapon">Weapon data to set</param>
    public void SetWeapon(Weapon weapon)
    {
        weaponData = weapon;
        
        // Update any display properties based on the weapon
        // This could include changing the model, color, etc.
    }
} 