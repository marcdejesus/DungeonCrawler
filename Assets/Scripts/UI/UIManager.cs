using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI elements and displays in the game
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Main menu panel")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Tooltip("HUD panel")]
    [SerializeField] private GameObject hudPanel;
    
    [Tooltip("Pause menu panel")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Tooltip("Game over panel")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Tooltip("Victory panel")]
    [SerializeField] private GameObject victoryPanel;
    
    [Header("HUD Elements")]
    [Tooltip("Health bar")]
    [SerializeField] private Slider healthBar;
    
    [Tooltip("Health text")]
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Tooltip("Coin counter text")]
    [SerializeField] private TextMeshProUGUI coinText;
    
    [Tooltip("Level text")]
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Tooltip("Mini-map")]
    [SerializeField] private GameObject miniMap;
    
    [Header("Status Effects")]
    [Tooltip("Status effect container")]
    [SerializeField] private Transform statusEffectContainer;
    
    [Tooltip("Status effect icon prefab")]
    [SerializeField] private GameObject statusEffectIconPrefab;
    
    [Header("Game Over Elements")]
    [Tooltip("Game over stats text")]
    [SerializeField] private TextMeshProUGUI gameOverStatsText;
    
    [Header("Victory Elements")]
    [Tooltip("Victory stats text")]
    [SerializeField] private TextMeshProUGUI victoryStatsText;
    
    [Header("Animation Settings")]
    [Tooltip("Fade duration for UI transitions")]
    [SerializeField] private float fadeTime = 0.5f;
    
    [Tooltip("Canvass group for fading")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    // Singleton pattern
    public static UIManager Instance { get; private set; }
    
    // References
    private GameManager gameManager;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find GameManager
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        // Subscribe to events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += UpdateUI;
            gameManager.OnCoinsChanged += UpdateCoins;
            gameManager.OnLevelChanged += UpdateLevel;
            gameManager.OnPlayerDeath += ShowGameOver;
            gameManager.OnBossDefeated += ShowVictory;
        }
        
        // Initialize UI
        UpdateUI();
    }

    /// <summary>
    /// Update UI based on current game state
    /// </summary>
    private void UpdateUI()
    {
        if (gameManager == null)
            return;
            
        // Hide all panels first
        HideAllPanels();
        
        // Show appropriate panel based on game state
        switch (gameManager.CurrentState)
        {
            case GameManager.GameState.MainMenu:
                ShowMainMenu();
                break;
                
            case GameManager.GameState.Playing:
                ShowHUD();
                break;
                
            case GameManager.GameState.Paused:
                ShowPauseMenu();
                break;
                
            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
                
            case GameManager.GameState.Victory:
                ShowVictory();
                break;
        }
    }

    /// <summary>
    /// Hide all UI panels
    /// </summary>
    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Show main menu
    /// </summary>
    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Show HUD during gameplay
    /// </summary>
    private void ShowHUD()
    {
        hudPanel.SetActive(true);
        StartCoroutine(FadeIn());
        
        // Find and connect to player health if necessary
        ConnectToPlayer();
    }

    /// <summary>
    /// Show pause menu
    /// </summary>
    private void ShowPauseMenu()
    {
        hudPanel.SetActive(true);
        pauseMenuPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    private void ShowGameOver()
    {
        // Update stats text
        if (gameManager != null && gameOverStatsText != null)
        {
            gameOverStatsText.text = gameManager.GetRunStatistics();
        }
        
        gameOverPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Show victory screen
    /// </summary>
    private void ShowVictory()
    {
        // Update stats text
        if (gameManager != null && victoryStatsText != null)
        {
            victoryStatsText.text = gameManager.GetRunStatistics();
        }
        
        victoryPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Connect to player for health updates
    /// </summary>
    private void ConnectToPlayer()
    {
        // Only connect if not already connected
        if (playerHealth != null)
            return;
            
        if (gameManager != null && gameManager.Player != null)
        {
            playerHealth = gameManager.Player.GetComponent<PlayerHealth>();
            
            // Subscribe to health changes
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                
                // Initial health update
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
    }

    /// <summary>
    /// Update health display
    /// </summary>
    /// <param name="currentHealth">Current health</param>
    /// <param name="maxHealth">Maximum health</param>
    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            // Update slider
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        if (healthText != null)
        {
            // Update text
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    /// <summary>
    /// Update coin counter
    /// </summary>
    /// <param name="coins">Current coin count</param>
    private void UpdateCoins(int coins)
    {
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }

    /// <summary>
    /// Update level display
    /// </summary>
    /// <param name="level">Current level</param>
    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }

    /// <summary>
    /// Add status effect icon
    /// </summary>
    /// <param name="icon">Icon sprite</param>
    /// <param name="duration">Effect duration</param>
    /// <returns>Created icon GameObject</returns>
    public GameObject AddStatusEffect(Sprite icon, float duration)
    {
        if (statusEffectContainer == null || statusEffectIconPrefab == null)
            return null;
            
        // Instantiate icon
        GameObject effectIcon = Instantiate(statusEffectIconPrefab, statusEffectContainer);
        
        // Set icon sprite
        Image iconImage = effectIcon.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
        
        // Set up duration
        if (duration > 0)
        {
            StartCoroutine(RemoveStatusEffectAfterDuration(effectIcon, duration));
        }
        
        return effectIcon;
    }

    /// <summary>
    /// Remove status effect after duration
    /// </summary>
    private IEnumerator RemoveStatusEffectAfterDuration(GameObject effectIcon, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (effectIcon != null)
        {
            Destroy(effectIcon);
        }
    }

    /// <summary>
    /// Remove status effect icon
    /// </summary>
    /// <param name="effectIcon">Icon to remove</param>
    public void RemoveStatusEffect(GameObject effectIcon)
    {
        if (effectIcon != null)
        {
            Destroy(effectIcon);
        }
    }

    /// <summary>
    /// Start a new game (button callback)
    /// </summary>
    public void OnStartGameButton()
    {
        if (gameManager != null)
        {
            StartCoroutine(FadeOutAndStartGame());
        }
    }

    /// <summary>
    /// Resume game (button callback)
    /// </summary>
    public void OnResumeButton()
    {
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    /// <summary>
    /// Quit to main menu (button callback)
    /// </summary>
    public void OnQuitToMenuButton()
    {
        StartCoroutine(FadeOutAndReturnToMenu());
    }

    /// <summary>
    /// Quit application (button callback)
    /// </summary>
    public void OnQuitGameButton()
    {
        StartCoroutine(FadeOutAndQuit());
    }

    /// <summary>
    /// Fade in effect
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null)
            yield break;
            
        canvasGroup.alpha = 0f;
        
        float time = 0f;
        while (time < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeTime);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Fade out effect
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
            yield break;
            
        canvasGroup.alpha = 1f;
        
        float time = 0f;
        while (time < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeTime);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Fade out and start game
    /// </summary>
    private IEnumerator FadeOutAndStartGame()
    {
        yield return StartCoroutine(FadeOut());
        
        if (gameManager != null)
        {
            gameManager.StartNewGame();
        }
    }

    /// <summary>
    /// Fade out and return to menu
    /// </summary>
    private IEnumerator FadeOutAndReturnToMenu()
    {
        yield return StartCoroutine(FadeOut());
        
        // Ensure time scale is reset
        Time.timeScale = 1f;
        
        if (gameManager != null)
        {
            gameManager.SetGameState(GameManager.GameState.MainMenu);
        }
    }

    /// <summary>
    /// Fade out and quit game
    /// </summary>
    private IEnumerator FadeOutAndQuit()
    {
        yield return StartCoroutine(FadeOut());
        
        // Quit application
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= UpdateUI;
            gameManager.OnCoinsChanged -= UpdateCoins;
            gameManager.OnLevelChanged -= UpdateLevel;
            gameManager.OnPlayerDeath -= ShowGameOver;
            gameManager.OnBossDefeated -= ShowVictory;
        }
        
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealth;
        }
    }
} 