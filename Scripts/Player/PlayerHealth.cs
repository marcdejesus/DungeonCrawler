using System;

public class PlayerHealth
{
    // Public properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    // Events
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnPlayerDeath;

    private int currentHealth;
    private int maxHealth;

    public PlayerHealth(int initialHealth, int maxHealth)
    {
        this.currentHealth = initialHealth;
        this.maxHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            OnPlayerDeath?.Invoke();
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
} 