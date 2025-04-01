using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the minimap, showing the dungeon layout
/// </summary>
public class MinimapSystem : MonoBehaviour
{
    [Header("Minimap Settings")]
    [Tooltip("Minimap camera")]
    [SerializeField] private Camera minimapCamera;
    
    [Tooltip("Player icon on minimap")]
    [SerializeField] private Transform playerIcon;
    
    [Tooltip("Room icon prefab")]
    [SerializeField] private GameObject roomIconPrefab;
    
    [Tooltip("Room connection prefab")]
    [SerializeField] private GameObject connectionPrefab;
    
    [Tooltip("Current room indicator")]
    [SerializeField] private GameObject currentRoomIndicator;
    
    [Header("Icon Colors")]
    [Tooltip("Normal room color")]
    [SerializeField] private Color normalRoomColor = Color.gray;
    
    [Tooltip("Elite room color")]
    [SerializeField] private Color eliteRoomColor = Color.red;
    
    [Tooltip("Boss room color")]
    [SerializeField] private Color bossRoomColor = new Color(1f, 0f, 0f, 1f);
    
    [Tooltip("Shop room color")]
    [SerializeField] private Color shopRoomColor = Color.blue;
    
    [Tooltip("Treasure room color")]
    [SerializeField] private Color treasureRoomColor = Color.yellow;
    
    [Tooltip("Secret room color")]
    [SerializeField] private Color secretRoomColor = Color.magenta;
    
    [Tooltip("Starting room color")]
    [SerializeField] private Color startRoomColor = Color.green;
    
    [Tooltip("Cleared room color modifier")]
    [SerializeField] private Color clearedRoomModifier = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("Scale Settings")]
    [Tooltip("Scale of the minimap")]
    [SerializeField] private float minimapScale = 3f;
    
    [Tooltip("Distance between room icons")]
    [SerializeField] private float roomIconDistance = 10f;
    
    // Private variables
    private Dictionary<Room, GameObject> roomIcons = new Dictionary<Room, GameObject>();
    private Dictionary<(Room, Room), GameObject> connections = new Dictionary<(Room, Room), GameObject>();
    private Transform player;
    private Room currentRoom;
    
    // Singleton instance
    public static MinimapSystem Instance { get; private set; }

    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Set minimap camera orthographic size based on scale
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = minimapScale;
        }
        
        // Subscribe to dungeon generation event
        DungeonGenerator dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        
        if (dungeonGenerator != null)
        {
            dungeonGenerator.OnDungeonGenerated += GenerateMinimap;
        }
    }

    private void Update()
    {
        // Update player icon position
        UpdatePlayerIcon();
        
        // Update current room indicator
        UpdateCurrentRoom();
    }

    /// <summary>
    /// Update player icon position on minimap
    /// </summary>
    private void UpdatePlayerIcon()
    {
        if (player != null && playerIcon != null)
        {
            // Scale player position to minimap
            Vector3 scaledPos = new Vector3(
                player.position.x / roomIconDistance,
                playerIcon.position.y, // Keep Y the same (height on minimap)
                player.position.z / roomIconDistance
            );
            
            playerIcon.position = scaledPos;
            
            // Rotate player icon to match player rotation
            playerIcon.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
    }

    /// <summary>
    /// Update current room indicator
    /// </summary>
    private void UpdateCurrentRoom()
    {
        if (player == null)
            return;
            
        // Find all rooms
        Room[] rooms = FindObjectsOfType<Room>();
        
        // Find which room contains the player
        foreach (Room room in rooms)
        {
            // Simple check - is player within room bounds?
            // Could be improved with actual collider check
            Collider roomCollider = room.GetComponent<Collider>();
            
            if (roomCollider != null && roomCollider.bounds.Contains(player.position))
            {
                if (room != currentRoom)
                {
                    // Room changed
                    currentRoom = room;
                    
                    // Update current room indicator
                    if (currentRoomIndicator != null && roomIcons.ContainsKey(room))
                    {
                        currentRoomIndicator.transform.position = roomIcons[room].transform.position;
                        currentRoomIndicator.SetActive(true);
                    }
                    
                    // Mark room as visited on minimap
                    MarkRoomAsVisited(room);
                }
                
                // Room found, no need to check others
                return;
            }
        }
    }

    /// <summary>
    /// Mark a room as visited on the minimap
    /// </summary>
    private void MarkRoomAsVisited(Room room)
    {
        if (roomIcons.ContainsKey(room))
        {
            // Get icon renderer and set it to fully visible
            Renderer iconRenderer = roomIcons[room].GetComponent<Renderer>();
            
            if (iconRenderer != null)
            {
                // Make sure the room icon is visible
                Color iconColor = iconRenderer.material.color;
                iconColor.a = 1f;
                iconRenderer.material.color = iconColor;
            }
            
            // Also show connected rooms, but slightly faded
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                // Check if there's a connection in this direction
                if (room.HasNorthConnection && dir == Direction.North ||
                    room.HasEastConnection && dir == Direction.East ||
                    room.HasSouthConnection && dir == Direction.South ||
                    room.HasWestConnection && dir == Direction.West)
                {
                    // Find the connected room
                    Vector3 connectedPos = room.transform.position;
                    
                    switch (dir)
                    {
                        case Direction.North:
                            connectedPos.z += roomIconDistance;
                            break;
                        case Direction.East:
                            connectedPos.x += roomIconDistance;
                            break;
                        case Direction.South:
                            connectedPos.z -= roomIconDistance;
                            break;
                        case Direction.West:
                            connectedPos.x -= roomIconDistance;
                            break;
                    }
                    
                    // Find room at this position
                    Room connectedRoom = FindRoomAtPosition(connectedPos);
                    
                    if (connectedRoom != null && roomIcons.ContainsKey(connectedRoom))
                    {
                        // Show the connected room icon (semi-transparent)
                        Renderer connectedRenderer = roomIcons[connectedRoom].GetComponent<Renderer>();
                        
                        if (connectedRenderer != null)
                        {
                            Color connectedColor = connectedRenderer.material.color;
                            connectedColor.a = 0.5f;
                            connectedRenderer.material.color = connectedColor;
                        }
                        
                        // Show the connection
                        if (connections.ContainsKey((room, connectedRoom)))
                        {
                            connections[(room, connectedRoom)].SetActive(true);
                        }
                        else if (connections.ContainsKey((connectedRoom, room)))
                        {
                            connections[(connectedRoom, room)].SetActive(true);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Find room at specific position
    /// </summary>
    private Room FindRoomAtPosition(Vector3 position, float tolerance = 1f)
    {
        // Find all rooms
        Room[] rooms = FindObjectsOfType<Room>();
        
        foreach (Room room in rooms)
        {
            // Check if positions are approximately equal
            if (Vector3.Distance(room.transform.position, position) < tolerance)
            {
                return room;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Generate the minimap from the dungeon
    /// </summary>
    private void GenerateMinimap()
    {
        // Clear existing minimap
        ClearMinimap();
        
        // Find all rooms
        Room[] rooms = FindObjectsOfType<Room>();
        
        // Create icons for each room
        foreach (Room room in rooms)
        {
            // Create icon
            Vector3 iconPos = new Vector3(
                room.transform.position.x / roomIconDistance,
                0.1f, // Slightly above the minimap plane
                room.transform.position.z / roomIconDistance
            );
            
            GameObject icon = Instantiate(roomIconPrefab, iconPos, Quaternion.Euler(90f, 0f, 0f), transform);
            
            // Set color based on room type
            Renderer iconRenderer = icon.GetComponent<Renderer>();
            
            if (iconRenderer != null)
            {
                Color roomColor = GetRoomColor(room.RoomType);
                
                // Apply cleared modifier if room is cleared
                if (room.IsCleared)
                {
                    roomColor *= clearedRoomModifier;
                }
                
                // Hide room until discovered (except start room)
                if (room.RoomType != RoomType.Start)
                {
                    roomColor.a = 0f;
                }
                
                iconRenderer.material.color = roomColor;
            }
            
            // Store reference
            roomIcons.Add(room, icon);
        }
        
        // Create connections between rooms
        CreateConnections(rooms);
        
        // Make sure the player icon is on top
        if (playerIcon != null)
        {
            playerIcon.SetAsLastSibling();
        }
        
        // Set initial current room
        if (currentRoomIndicator != null)
        {
            currentRoomIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Create connections between rooms on the minimap
    /// </summary>
    private void CreateConnections(Room[] rooms)
    {
        foreach (Room room in rooms)
        {
            // Check each direction for connections
            if (room.HasNorthConnection)
            {
                CreateConnection(room, Direction.North);
            }
            
            if (room.HasEastConnection)
            {
                CreateConnection(room, Direction.East);
            }
            
            // No need to check South and West
            // They will be covered when we handle the connected rooms' North and East connections
        }
    }

    /// <summary>
    /// Create a connection from a room in a specific direction
    /// </summary>
    private void CreateConnection(Room room, Direction direction)
    {
        if (!roomIcons.ContainsKey(room))
            return;
            
        // Calculate position of connected room
        Vector3 connectedPos = room.transform.position;
        float connectionLength = roomIconDistance;
        Quaternion connectionRotation = Quaternion.identity;
        
        switch (direction)
        {
            case Direction.North:
                connectedPos.z += roomIconDistance;
                connectionRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
                
            case Direction.East:
                connectedPos.x += roomIconDistance;
                connectionRotation = Quaternion.Euler(90f, 90f, 0f);
                break;
                
            case Direction.South:
                connectedPos.z -= roomIconDistance;
                connectionRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
                
            case Direction.West:
                connectedPos.x -= roomIconDistance;
                connectionRotation = Quaternion.Euler(90f, 90f, 0f);
                break;
        }
        
        // Find the connected room
        Room connectedRoom = FindRoomAtPosition(connectedPos);
        
        if (connectedRoom != null && roomIcons.ContainsKey(connectedRoom))
        {
            // Check if connection already exists
            if (connections.ContainsKey((room, connectedRoom)) || connections.ContainsKey((connectedRoom, room)))
                return;
                
            // Create connection object
            Vector3 midpoint = (roomIcons[room].transform.position + roomIcons[connectedRoom].transform.position) / 2f;
            GameObject connection = Instantiate(connectionPrefab, midpoint, connectionRotation, transform);
            
            // Scale connection to fit between rooms
            connection.transform.localScale = new Vector3(0.1f, Vector3.Distance(roomIcons[room].transform.position, roomIcons[connectedRoom].transform.position) / 2f, 1f);
            
            // Hide initially except for start room connections
            connection.SetActive(room.RoomType == RoomType.Start || connectedRoom.RoomType == RoomType.Start);
            
            // Store reference
            connections.Add((room, connectedRoom), connection);
        }
    }

    /// <summary>
    /// Get color based on room type
    /// </summary>
    private Color GetRoomColor(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Start:
                return startRoomColor;
                
            case RoomType.Normal:
                return normalRoomColor;
                
            case RoomType.Elite:
                return eliteRoomColor;
                
            case RoomType.Boss:
                return bossRoomColor;
                
            case RoomType.Shop:
                return shopRoomColor;
                
            case RoomType.Treasure:
                return treasureRoomColor;
                
            case RoomType.Secret:
                return secretRoomColor;
                
            default:
                return normalRoomColor;
        }
    }

    /// <summary>
    /// Clear the minimap
    /// </summary>
    private void ClearMinimap()
    {
        // Destroy all room icons
        foreach (GameObject icon in roomIcons.Values)
        {
            Destroy(icon);
        }
        
        // Destroy all connections
        foreach (GameObject connection in connections.Values)
        {
            Destroy(connection);
        }
        
        // Clear dictionaries
        roomIcons.Clear();
        connections.Clear();
        
        // Reset current room
        currentRoom = null;
        
        // Hide current room indicator
        if (currentRoomIndicator != null)
        {
            currentRoomIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Mark a room as cleared on the minimap
    /// </summary>
    public void MarkRoomCleared(Room room)
    {
        if (roomIcons.ContainsKey(room))
        {
            Renderer iconRenderer = roomIcons[room].GetComponent<Renderer>();
            
            if (iconRenderer != null)
            {
                // Apply cleared modifier to color
                Color roomColor = iconRenderer.material.color;
                roomColor *= clearedRoomModifier;
                iconRenderer.material.color = roomColor;
            }
        }
    }
} 