using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a single dungeon room with enemies, obstacles, and doors
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("Type of room")]
    [SerializeField] private RoomType roomType = RoomType.Normal;
    
    [Tooltip("Is this room currently active")]
    [SerializeField] private bool isActive = false;
    
    [Header("Doors")]
    [Tooltip("North door GameObject")]
    [SerializeField] private GameObject northDoor;
    
    [Tooltip("East door GameObject")]
    [SerializeField] private GameObject eastDoor;
    
    [Tooltip("South door GameObject")]
    [SerializeField] private GameObject southDoor;
    
    [Tooltip("West door GameObject")]
    [SerializeField] private GameObject westDoor;
    
    [Header("Door Connections")]
    [Tooltip("North door connection point")]
    [SerializeField] private Transform northConnection;
    
    [Tooltip("East door connection point")]
    [SerializeField] private Transform eastConnection;
    
    [Tooltip("South door connection point")]
    [SerializeField] private Transform southConnection;
    
    [Tooltip("West door connection point")]
    [SerializeField] private Transform westConnection;
    
    [Header("Spawn Settings")]
    [Tooltip("Possible enemy spawn positions")]
    [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();
    
    [Tooltip("Possible obstacle spawn positions")]
    [SerializeField] private List<Transform> obstacleSpawnPoints = new List<Transform>();
    
    [Tooltip("Possible pickup spawn positions")]
    [SerializeField] private List<Transform> pickupSpawnPoints = new List<Transform>();
    
    [Header("Content")]
    [Tooltip("Enemies in this room")]
    private List<GameObject> enemies = new List<GameObject>();
    
    [Tooltip("Objects/obstacles in this room")]
    private List<GameObject> obstacles = new List<GameObject>();
    
    [Tooltip("Pickups in this room")]
    private List<GameObject> pickups = new List<GameObject>();
    
    // Door connection status
    private bool hasNorthConnection = false;
    private bool hasEastConnection = false;
    private bool hasSouthConnection = false;
    private bool hasWestConnection = false;
    
    // Room state
    private bool isCleared = false;
    private bool doorsLocked = false;
    
    // Events
    public event Action OnRoomActivated;
    public event Action OnRoomCleared;
    
    // Properties
    public RoomType RoomType => roomType;
    public bool IsActive => isActive;
    public bool IsCleared => isCleared;
    public bool DoorsLocked => doorsLocked;
    public Transform NorthConnection => northConnection;
    public Transform EastConnection => eastConnection;
    public Transform SouthConnection => southConnection;
    public Transform WestConnection => westConnection;
    public bool HasNorthConnection => hasNorthConnection;
    public bool HasEastConnection => hasEastConnection;
    public bool HasSouthConnection => hasSouthConnection;
    public bool HasWestConnection => hasWestConnection;

    private void Start()
    {
        // Initialize room
        if (isActive)
        {
            ActivateRoom();
        }
        else
        {
            DeactivateRoom();
        }
    }

    /// <summary>
    /// Set up doors based on connections
    /// </summary>
    public void SetupDoors()
    {
        // Enable/disable doors based on connections
        if (northDoor != null) northDoor.SetActive(!hasNorthConnection);
        if (eastDoor != null) eastDoor.SetActive(!hasEastConnection);
        if (southDoor != null) southDoor.SetActive(!hasSouthConnection);
        if (westDoor != null) westDoor.SetActive(!hasWestConnection);
    }

    /// <summary>
    /// Set a door connection
    /// </summary>
    /// <param name="direction">Direction of the connection</param>
    /// <param name="hasConnection">Whether there's a connection</param>
    public void SetDoorConnection(Direction direction, bool hasConnection)
    {
        switch (direction)
        {
            case Direction.North:
                hasNorthConnection = hasConnection;
                break;
            case Direction.East:
                hasEastConnection = hasConnection;
                break;
            case Direction.South:
                hasSouthConnection = hasConnection;
                break;
            case Direction.West:
                hasWestConnection = hasConnection;
                break;
        }
        
        // Update doors
        SetupDoors();
    }

    /// <summary>
    /// Activate this room (when player enters)
    /// </summary>
    public void ActivateRoom()
    {
        isActive = true;
        
        // Activate all contents
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        
        // If room isn't cleared and has enemies, lock doors
        if (!isCleared && enemies.Count > 0 && !doorsLocked)
        {
            LockDoors();
        }
        else if (isCleared && doorsLocked)
        {
            // Make sure doors are unlocked if room is already cleared
            UnlockDoors();
        }
        
        // Notify listeners
        OnRoomActivated?.Invoke();
    }

    /// <summary>
    /// Deactivate this room (when player leaves area)
    /// </summary>
    public void DeactivateRoom()
    {
        isActive = false;
        
        // Option: Deactivate contents for performance
        foreach (Transform child in transform)
        {
            // Don't deactivate the room itself
            if (child.GetComponent<Room>() == null)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Lock all doors (when combat starts)
    /// </summary>
    public void LockDoors()
    {
        doorsLocked = true;
        
        // Implementation depends on your door system
        // Could be changing sprites, adding colliders, etc.
        
        // Example: change door tag or layer to block passage
        if (northDoor != null && hasNorthConnection) northDoor.SetActive(true);
        if (eastDoor != null && hasEastConnection) eastDoor.SetActive(true);
        if (southDoor != null && hasSouthConnection) southDoor.SetActive(true);
        if (westDoor != null && hasWestConnection) westDoor.SetActive(true);
    }

    /// <summary>
    /// Unlock all doors (when room is cleared)
    /// </summary>
    public void UnlockDoors()
    {
        doorsLocked = false;
        
        // Implementation depends on your door system
        // Revert whatever was done in LockDoors
        
        // Example: restore normal door state
        if (northDoor != null && hasNorthConnection) northDoor.SetActive(false);
        if (eastDoor != null && hasEastConnection) eastDoor.SetActive(false);
        if (southDoor != null && hasSouthConnection) southDoor.SetActive(false);
        if (westDoor != null && hasWestConnection) westDoor.SetActive(false);
    }

    /// <summary>
    /// Add enemy to the room
    /// </summary>
    /// <param name="enemy">Enemy GameObject</param>
    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
        
        // Subscribe to enemy death
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDeath += OnEnemyDefeated;
        }
    }

    /// <summary>
    /// Called when an enemy is defeated
    /// </summary>
    private void OnEnemyDefeated()
    {
        // Remove dead enemies from list (find and remove first null entry)
        enemies.RemoveAll(enemy => enemy == null);
        
        // Check if all enemies are defeated
        if (enemies.Count == 0)
        {
            RoomCleared();
        }
    }

    /// <summary>
    /// Mark room as cleared
    /// </summary>
    private void RoomCleared()
    {
        isCleared = true;
        
        // Unlock doors
        UnlockDoors();
        
        // Spawn rewards if appropriate
        if (roomType == RoomType.Normal || roomType == RoomType.Elite)
        {
            SpawnRewards();
        }
        
        // Notify listeners
        OnRoomCleared?.Invoke();
    }

    /// <summary>
    /// Spawn rewards when room is cleared
    /// </summary>
    private void SpawnRewards()
    {
        // Implementation depends on your reward system
        // Could spawn health, gold, items, etc.
        
        // Example: Spawn pickup at center of room
        Vector3 center = transform.position;
        
        // Use LevelManager to spawn appropriate reward
        // LevelManager.Instance.SpawnReward(center, roomType);
    }

    /// <summary>
    /// Get a random spawn point for enemies
    /// </summary>
    public Transform GetRandomEnemySpawnPoint()
    {
        if (enemySpawnPoints.Count == 0)
            return transform; // Fallback to room center
            
        return enemySpawnPoints[UnityEngine.Random.Range(0, enemySpawnPoints.Count)];
    }

    /// <summary>
    /// Get a random spawn point for obstacles
    /// </summary>
    public Transform GetRandomObstacleSpawnPoint()
    {
        if (obstacleSpawnPoints.Count == 0)
            return transform; // Fallback to room center
            
        return obstacleSpawnPoints[UnityEngine.Random.Range(0, obstacleSpawnPoints.Count)];
    }

    /// <summary>
    /// Get a random spawn point for pickups
    /// </summary>
    public Transform GetRandomPickupSpawnPoint()
    {
        if (pickupSpawnPoints.Count == 0)
            return transform; // Fallback to room center
            
        return pickupSpawnPoints[UnityEngine.Random.Range(0, pickupSpawnPoints.Count)];
    }
}

/// <summary>
/// Types of rooms in the dungeon
/// </summary>
public enum RoomType
{
    Start,
    Normal,
    Elite,
    Boss,
    Shop,
    Treasure,
    Secret
}

/// <summary>
/// Direction enum for room connections
/// </summary>
public enum Direction
{
    North,
    East,
    South,
    West
} 