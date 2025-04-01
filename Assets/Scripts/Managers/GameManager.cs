using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for game state, progression, and core systems
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Number of levels before boss level")]
    [SerializeField] private int levelsPerRun = 5;
    
    [Tooltip("Time to wait before restarting after death")]
    [SerializeField] private float gameOverDelay = 2f;
    
    [Header("Player Settings")]
    [Tooltip("Player prefab to spawn")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Difficulty Settings")]
    [Tooltip("Difficulty multiplier")]
    [Range(0.5f, 2f)]
    [SerializeField] private float difficultyMultiplier = 1f;
    
    [Tooltip("Difficulty increase per level")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float difficultyIncreasePerLevel = 0.1f;
    
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    // Game state
    private GameState currentState = GameState.MainMenu;
    private int currentLevel = 1;
    private int playerCoins = 0;
    private int playerMaxHealth = 6;
    private int enemiesDefeated = 0;
    private int roomsCleared = 0;
    private float runTime = 0f;
    private bool isPaused = false;
    
    // References
    private GameObject player;
    private DungeonGenerator dungeonGenerator;
    private PlayerHealth playerHealthComponent;
    
    // Events
    public event Action OnGameStateChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnCoinsChanged;
    public event Action OnPlayerDeath;
    public event Action OnBossDefeated;
    
    // Properties
    public GameState CurrentState => currentState;
    public int CurrentLevel => currentLevel;
    public float DifficultyMultiplier => difficultyMultiplier + (currentLevel - 1) * difficultyIncreasePerLevel;
    public bool IsPaused => isPaused;
    public int PlayerCoins => playerCoins;
    public GameObject Player => player;

    /// <summary>
    /// Game states
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

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
        
        // Find dungeon generator
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
    }

    private void Start()
    {
        // Start in main menu
        SetGameState(GameState.MainMenu);
    }

    private void Update()
    {
        // Track run time while playing
        if (currentState == GameState.Playing && !isPaused)
        {
            runTime += Time.deltaTime;
        }
        
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape) && currentState == GameState.Playing)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartNewGame()
    {
        // Reset game variables
        currentLevel = 1;
        playerCoins = 0;
        playerMaxHealth = 6;
        enemiesDefeated = 0;
        roomsCleared = 0;
        runTime = 0f;
        
        // Start first level
        StartLevel();
    }

    /// <summary>
    /// Start current level
    /// </summary>
    private void StartLevel()
    {
        // Generate dungeon
        if (dungeonGenerator == null)
        {
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        }
        
        if (dungeonGenerator != null)
        {
            dungeonGenerator.GenerateDungeon();
        }
        else
        {
            Debug.LogError("Dungeon Generator not found!");
            return;
        }
        
        // Spawn player in start room
        SpawnPlayer();
        
        // Set game state to playing
        SetGameState(GameState.Playing);
        
        // Notify listeners of level change
        OnLevelChanged?.Invoke(currentLevel);
    }

    /// <summary>
    /// Spawn player at start position
    /// </summary>
    private void SpawnPlayer()
    {
        // Find spawn point (could be a specific marker in the start room)
        GameObject startRoom = GameObject.FindGameObjectWithTag("StartRoom");
        Transform spawnPoint = startRoom?.transform;
        
        if (spawnPoint == null)
        {
            // Fallback to center of scene
            spawnPoint = transform;
        }
        
        // Instantiate player
        player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        
        // Get player components
        playerHealthComponent = player.GetComponent<PlayerHealth>();
        
        // Subscribe to player events
        if (playerHealthComponent != null)
        {
            playerHealthComponent.OnPlayerDeath += HandlePlayerDeath;
            
            // Set max health (for continuing games)
            playerHealthComponent.IncreaseMaxHealth(playerMaxHealth - 6); // 6 is the default
        }
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void HandlePlayerDeath()
    {
        // Set game over state
        SetGameState(GameState.GameOver);
        
        // Notify listeners
        OnPlayerDeath?.Invoke();
        
        // Schedule restart after delay
        StartCoroutine(GameOverCoroutine());
    }

    /// <summary>
    /// Coroutine for game over sequence
    /// </summary>
    private IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(gameOverDelay);
        
        // Return to main menu
        SetGameState(GameState.MainMenu);
        
        // Reload scene if needed
        // SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Complete current level and proceed to next
    /// </summary>
    public void CompleteLevel()
    {
        currentLevel++;
        
        // Check if we've completed the run
        if (currentLevel > levelsPerRun)
        {
            // Victory!
            SetGameState(GameState.Victory);
            
            // Notify listeners
            OnBossDefeated?.Invoke();
        }
        else
        {
            // Proceed to next level
            StartLevel();
        }
    }

    /// <summary>
    /// Add coins to player total
    /// </summary>
    /// <param name="amount">Amount to add</param>
    public void AddCoins(int amount)
    {
        playerCoins += amount;
        
        // Notify listeners
        OnCoinsChanged?.Invoke(playerCoins);
    }

    /// <summary>
    /// Spend coins from player total
    /// </summary>
    /// <param name="amount">Amount to spend</param>
    /// <returns>Whether the purchase was successful</returns>
    public bool SpendCoins(int amount)
    {
        if (playerCoins >= amount)
        {
            playerCoins -= amount;
            
            // Notify listeners
            OnCoinsChanged?.Invoke(playerCoins);
            
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Increase player max health
    /// </summary>
    /// <param name="amount">Amount to increase</param>
    public void IncreasePlayerMaxHealth(int amount)
    {
        playerMaxHealth += amount;
        
        // Apply to player if active
        if (playerHealthComponent != null)
        {
            playerHealthComponent.IncreaseMaxHealth(amount);
        }
    }

    /// <summary>
    /// Record an enemy defeat
    /// </summary>
    public void RecordEnemyDefeated()
    {
        enemiesDefeated++;
    }

    /// <summary>
    /// Record a room cleared
    /// </summary>
    public void RecordRoomCleared()
    {
        roomsCleared++;
    }

    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                Time.timeScale = 0f;
                SetGameState(GameState.Paused);
            }
            else
            {
                Time.timeScale = 1f;
                SetGameState(GameState.Playing);
            }
        }
    }

    /// <summary>
    /// Set the game state
    /// </summary>
    /// <param name="newState">New game state</param>
    private void SetGameState(GameState newState)
    {
        currentState = newState;
        
        // Notify listeners
        OnGameStateChanged?.Invoke();
    }

    /// <summary>
    /// Get total run statistics
    /// </summary>
    /// <returns>Run statistics as a formatted string</returns>
    public string GetRunStatistics()
    {
        TimeSpan time = TimeSpan.FromSeconds(runTime);
        string timeStr = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        
        return $"Level Reached: {currentLevel}\n" +
               $"Enemies Defeated: {enemiesDefeated}\n" +
               $"Rooms Cleared: {roomsCleared}\n" +
               $"Time: {timeStr}\n" +
               $"Coins Collected: {playerCoins}";
    }
} 