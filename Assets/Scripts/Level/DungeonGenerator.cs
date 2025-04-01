using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates procedural dungeons with connected rooms
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [Tooltip("Minimum number of rooms to generate")]
    [SerializeField] private int minRooms = 10;
    
    [Tooltip("Maximum number of rooms to generate")]
    [SerializeField] private int maxRooms = 15;
    
    [Tooltip("Distance between room centers")]
    [SerializeField] private float roomDistance = 20f;
    
    [Tooltip("Chance to branch paths (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float branchChance = 0.3f;
    
    [Header("Room Distribution")]
    [Tooltip("Proportion of normal rooms")]
    [Range(0f, 1f)]
    [SerializeField] private float normalRoomChance = 0.7f;
    
    [Tooltip("Proportion of elite rooms")]
    [Range(0f, 1f)]
    [SerializeField] private float eliteRoomChance = 0.15f;
    
    [Tooltip("Proportion of shop rooms")]
    [Range(0f, 1f)]
    [SerializeField] private float shopRoomChance = 0.1f;
    
    [Tooltip("Proportion of treasure rooms")]
    [Range(0f, 1f)]
    [SerializeField] private float treasureRoomChance = 0.05f;
    
    [Tooltip("Chance to generate a secret room")]
    [Range(0f, 1f)]
    [SerializeField] private float secretRoomChance = 0.2f;
    
    [Header("Room Prefabs")]
    [Tooltip("Starting room prefab")]
    [SerializeField] private GameObject startRoomPrefab;
    
    [Tooltip("Normal room prefab variants")]
    [SerializeField] private List<GameObject> normalRoomPrefabs = new List<GameObject>();
    
    [Tooltip("Elite room prefab variants")]
    [SerializeField] private List<GameObject> eliteRoomPrefabs = new List<GameObject>();
    
    [Tooltip("Boss room prefab variants")]
    [SerializeField] private List<GameObject> bossRoomPrefabs = new List<GameObject>();
    
    [Tooltip("Shop room prefab variants")]
    [SerializeField] private List<GameObject> shopRoomPrefabs = new List<GameObject>();
    
    [Tooltip("Treasure room prefab variants")]
    [SerializeField] private List<GameObject> treasureRoomPrefabs = new List<GameObject>();
    
    [Tooltip("Secret room prefab variants")]
    [SerializeField] private List<GameObject> secretRoomPrefabs = new List<GameObject>();
    
    // Private variables
    private List<RoomNode> roomNodes = new List<RoomNode>();
    private List<GameObject> generatedRooms = new List<GameObject>();
    private GameObject startRoom;
    private GameObject bossRoom;
    
    // Events
    public event Action OnDungeonGenerated;

    /// <summary>
    /// Structure to represent a room in the generation grid
    /// </summary>
    private class RoomNode
    {
        public Vector2Int GridPosition { get; private set; }
        public RoomType Type { get; set; }
        public bool IsVisited { get; set; }
        public GameObject RoomPrefab { get; set; }
        public GameObject SpawnedRoom { get; set; }
        public Dictionary<Direction, RoomNode> Connections { get; private set; }

        public RoomNode(Vector2Int position, RoomType type = RoomType.Normal)
        {
            GridPosition = position;
            Type = type;
            IsVisited = false;
            Connections = new Dictionary<Direction, RoomNode>();
        }

        public void AddConnection(Direction direction, RoomNode otherRoom)
        {
            if (!Connections.ContainsKey(direction))
            {
                Connections.Add(direction, otherRoom);
                
                // Add reverse connection to other room
                Direction oppositeDirection = GetOppositeDirection(direction);
                if (!otherRoom.Connections.ContainsKey(oppositeDirection))
                {
                    otherRoom.Connections.Add(oppositeDirection, this);
                }
            }
        }

        private Direction GetOppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                default: return Direction.North;
            }
        }
    }

    /// <summary>
    /// Generate a new dungeon
    /// </summary>
    public void GenerateDungeon()
    {
        // Clear any existing dungeon
        ClearDungeon();
        
        // Create the layout
        CreateDungeonLayout();
        
        // Assign room types
        AssignRoomTypes();
        
        // Spawn room prefabs
        SpawnRooms();
        
        // Connect rooms
        ConnectRooms();
        
        // Populate rooms with enemies, obstacles, etc.
        PopulateRooms();
        
        // Notify listeners
        OnDungeonGenerated?.Invoke();
    }

    /// <summary>
    /// Clear existing dungeon
    /// </summary>
    private void ClearDungeon()
    {
        // Destroy all spawned rooms
        foreach (GameObject room in generatedRooms)
        {
            if (room != null)
            {
                Destroy(room);
            }
        }
        
        // Clear lists
        roomNodes.Clear();
        generatedRooms.Clear();
        startRoom = null;
        bossRoom = null;
    }

    /// <summary>
    /// Create the basic layout of connected room nodes
    /// </summary>
    private void CreateDungeonLayout()
    {
        // Determine number of rooms for this dungeon
        int numRooms = UnityEngine.Random.Range(minRooms, maxRooms + 1);
        
        // Create start room at center
        RoomNode startNode = new RoomNode(Vector2Int.zero, RoomType.Start);
        roomNodes.Add(startNode);
        
        // Use a queue for breadth-first generation
        Queue<RoomNode> roomQueue = new Queue<RoomNode>();
        roomQueue.Enqueue(startNode);
        
        // Dictionary to track grid positions that are already occupied
        Dictionary<Vector2Int, RoomNode> positionMap = new Dictionary<Vector2Int, RoomNode>();
        positionMap.Add(startNode.GridPosition, startNode);
        
        // Create rooms
        while (roomNodes.Count < numRooms && roomQueue.Count > 0)
        {
            RoomNode currentRoom = roomQueue.Dequeue();
            
            // Limit connections from this room
            int maxConnections = (currentRoom == startNode) ? 4 : UnityEngine.Random.Range(1, 4);
            
            // Try to create connections in different directions
            List<Direction> availableDirections = GetRandomDirections();
            
            foreach (Direction dir in availableDirections)
            {
                // Stop if we've reached the max number of rooms
                if (roomNodes.Count >= numRooms)
                    break;
                    
                // Stop if this room has too many connections
                if (currentRoom.Connections.Count >= maxConnections)
                    break;
                    
                // Calculate position of potential new room
                Vector2Int newPos = GetPositionInDirection(currentRoom.GridPosition, dir);
                
                // Check if position is already occupied
                if (positionMap.ContainsKey(newPos))
                    continue;
                    
                // Create new room
                RoomNode newRoom = new RoomNode(newPos);
                roomNodes.Add(newRoom);
                positionMap.Add(newPos, newRoom);
                
                // Connect rooms
                currentRoom.AddConnection(dir, newRoom);
                
                // Add to queue for further expansion
                roomQueue.Enqueue(newRoom);
                
                // Sometimes branch from the new room immediately (creates more interconnected layouts)
                if (UnityEngine.Random.value < branchChance)
                {
                    roomQueue.Enqueue(newRoom);
                }
            }
        }
        
        // Ensure we have at least one dead end for the boss room
        EnsureDeadEnds();
    }

    /// <summary>
    /// Make sure we have at least one dead end for the boss room
    /// </summary>
    private void EnsureDeadEnds()
    {
        List<RoomNode> deadEnds = FindDeadEnds();
        
        if (deadEnds.Count == 0)
        {
            // No dead ends found, create one by removing a connection
            // Find a room with more than one connection
            foreach (RoomNode room in roomNodes)
            {
                if (room.Type != RoomType.Start && room.Connections.Count > 1)
                {
                    // Get a random connection to remove
                    List<Direction> directions = new List<Direction>(room.Connections.Keys);
                    Direction dirToRemove = directions[UnityEngine.Random.Range(0, directions.Count)];
                    
                    // Get the room on the other side
                    RoomNode otherRoom = room.Connections[dirToRemove];
                    
                    // Remove connections between rooms
                    room.Connections.Remove(dirToRemove);
                    
                    // Find and remove reverse connection
                    foreach (var kvp in new Dictionary<Direction, RoomNode>(otherRoom.Connections))
                    {
                        if (kvp.Value == room)
                        {
                            otherRoom.Connections.Remove(kvp.Key);
                            break;
                        }
                    }
                    
                    // If we've created a dead end, we're done
                    if (otherRoom.Connections.Count == 0)
                    {
                        roomNodes.Remove(otherRoom);
                    }
                    else if (room.Connections.Count <= 1)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Find all dead end rooms (rooms with only one connection)
    /// </summary>
    private List<RoomNode> FindDeadEnds()
    {
        List<RoomNode> deadEnds = new List<RoomNode>();
        
        foreach (RoomNode room in roomNodes)
        {
            if (room.Type != RoomType.Start && room.Connections.Count == 1)
            {
                deadEnds.Add(room);
            }
        }
        
        return deadEnds;
    }

    /// <summary>
    /// Assign types to rooms based on their position in the layout
    /// </summary>
    private void AssignRoomTypes()
    {
        // Find dead ends (rooms with only one connection)
        List<RoomNode> deadEnds = FindDeadEnds();
        
        if (deadEnds.Count == 0)
        {
            Debug.LogError("No dead ends found for boss room!");
            return;
        }
        
        // Pick a random dead end for boss room
        int bossIndex = UnityEngine.Random.Range(0, deadEnds.Count);
        deadEnds[bossIndex].Type = RoomType.Boss;
        
        // Remove boss room from dead ends list
        RoomNode bossNode = deadEnds[bossIndex];
        deadEnds.RemoveAt(bossIndex);
        
        // Assign start room
        RoomNode startNode = roomNodes.Find(room => room.Type == RoomType.Start);
        
        // Calculate number of special rooms based on total rooms
        int totalRooms = roomNodes.Count - 2; // Excluding start and boss
        
        // Calculate distribution based on configured chances
        float totalSpecialChance = eliteRoomChance + shopRoomChance + treasureRoomChance;
        
        // Adjust for possibility that chances don't sum to 1
        float adjustedNormalChance = normalRoomChance;
        if (totalSpecialChance + normalRoomChance != 1.0f)
        {
            adjustedNormalChance = 1.0f - totalSpecialChance;
            Debug.LogWarning($"Room chances don't sum to 1. Adjusting normal room chance to {adjustedNormalChance}");
        }
        
        // Calculate number of each room type
        int eliteRooms = Mathf.RoundToInt(totalRooms * (eliteRoomChance / (totalSpecialChance + adjustedNormalChance)));
        int shopRooms = Mathf.RoundToInt(totalRooms * (shopRoomChance / (totalSpecialChance + adjustedNormalChance)));
        int treasureRooms = Mathf.RoundToInt(totalRooms * (treasureRoomChance / (totalSpecialChance + adjustedNormalChance)));
        int normalRooms = totalRooms - (eliteRooms + shopRooms + treasureRooms);
        
        // List of rooms to assign (excluding start and boss)
        List<RoomNode> roomsToAssign = new List<RoomNode>();
        foreach (RoomNode room in roomNodes)
        {
            if (room.Type == RoomType.Normal) // Only unassigned rooms (excludes start and boss)
            {
                roomsToAssign.Add(room);
            }
        }
        
        // Shuffle the list
        ShuffleList(roomsToAssign);
        
        // Assign room types
        int index = 0;
        
        // Elite rooms
        for (int i = 0; i < eliteRooms && index < roomsToAssign.Count; i++)
        {
            roomsToAssign[index].Type = RoomType.Elite;
            index++;
        }
        
        // Shop rooms - prefer rooms that aren't dead ends
        int shopsAssigned = 0;
        foreach (RoomNode room in roomsToAssign)
        {
            if (shopsAssigned >= shopRooms || index >= roomsToAssign.Count)
                break;
                
            if (room.Type == RoomType.Normal && room.Connections.Count > 1)
            {
                room.Type = RoomType.Shop;
                shopsAssigned++;
                index++;
            }
        }
        
        // If we couldn't place all shops in non-dead ends, place remaining in any available rooms
        while (shopsAssigned < shopRooms && index < roomsToAssign.Count)
        {
            if (roomsToAssign[index].Type == RoomType.Normal)
            {
                roomsToAssign[index].Type = RoomType.Shop;
                shopsAssigned++;
            }
            index++;
        }
        
        // Treasure rooms - prefer dead ends
        int treasuresAssigned = 0;
        foreach (RoomNode room in deadEnds)
        {
            if (treasuresAssigned >= treasureRooms)
                break;
                
            if (room.Type == RoomType.Normal)
            {
                room.Type = RoomType.Treasure;
                treasuresAssigned++;
            }
        }
        
        // If we couldn't place all treasures in dead ends, place remaining in any available rooms
        foreach (RoomNode room in roomsToAssign)
        {
            if (treasuresAssigned >= treasureRooms)
                break;
                
            if (room.Type == RoomType.Normal)
            {
                room.Type = RoomType.Treasure;
                treasuresAssigned++;
            }
        }
        
        // Remaining rooms are normal rooms
        
        // Try to add a secret room if chance allows
        if (UnityEngine.Random.value < secretRoomChance)
        {
            // Find a suitable location for a secret room
            // In a real implementation, you might want to find a wall between two rooms
            // and place the secret room adjacent to it
            
            // For now, just pick a normal room and change it to secret
            List<RoomNode> normalRoomList = roomNodes.FindAll(room => room.Type == RoomType.Normal);
            if (normalRoomList.Count > 0)
            {
                int secretIndex = UnityEngine.Random.Range(0, normalRoomList.Count);
                normalRoomList[secretIndex].Type = RoomType.Secret;
            }
        }
    }

    /// <summary>
    /// Get available directions from a room (directions that don't have connections)
    /// </summary>
    private List<Direction> GetAvailableDirections(RoomNode room)
    {
        List<Direction> available = new List<Direction>
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };
        
        foreach (Direction dir in room.Connections.Keys)
        {
            available.Remove(dir);
        }
        
        return available;
    }

    /// <summary>
    /// Spawn actual room prefabs based on the generated layout
    /// </summary>
    private void SpawnRooms()
    {
        foreach (RoomNode node in roomNodes)
        {
            // Select appropriate prefab based on room type
            GameObject prefab = GetRoomPrefab(node.Type);
            node.RoomPrefab = prefab;
            
            if (prefab != null)
            {
                // Calculate world position from grid position
                Vector3 worldPos = new Vector3(
                    node.GridPosition.x * roomDistance,
                    0,
                    node.GridPosition.y * roomDistance
                );
                
                // Spawn room
                GameObject spawnedRoom = Instantiate(prefab, worldPos, Quaternion.identity);
                spawnedRoom.transform.parent = transform;
                node.SpawnedRoom = spawnedRoom;
                generatedRooms.Add(spawnedRoom);
                
                // Store references to special rooms
                if (node.Type == RoomType.Start)
                {
                    startRoom = spawnedRoom;
                }
                else if (node.Type == RoomType.Boss)
                {
                    bossRoom = spawnedRoom;
                }
                
                // Only activate the start room initially
                if (node.Type != RoomType.Start)
                {
                    Room roomComponent = spawnedRoom.GetComponent<Room>();
                    if (roomComponent != null)
                    {
                        roomComponent.DeactivateRoom();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Connect the spawned rooms based on the layout
    /// </summary>
    private void ConnectRooms()
    {
        foreach (RoomNode node in roomNodes)
        {
            if (node.SpawnedRoom != null)
            {
                Room roomComponent = node.SpawnedRoom.GetComponent<Room>();
                
                if (roomComponent != null)
                {
                    // Set door connections based on node connections
                    foreach (Direction dir in node.Connections.Keys)
                    {
                        roomComponent.SetDoorConnection(dir, true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Populate rooms with enemies, obstacles, etc.
    /// </summary>
    private void PopulateRooms()
    {
        // Ideally handled by a separate system that spawns enemies based on room type
        // Each room could handle its own population via the Room component
        
        // Example: Use another manager class to populate rooms
        // RoomPopulator.Instance.PopulateRooms(generatedRooms);
    }

    /// <summary>
    /// Get a random room prefab based on the room type
    /// </summary>
    private GameObject GetRoomPrefab(RoomType type)
    {
        switch (type)
        {
            case RoomType.Start:
                return startRoomPrefab;
                
            case RoomType.Normal:
                return GetRandomPrefab(normalRoomPrefabs);
                
            case RoomType.Elite:
                return GetRandomPrefab(eliteRoomPrefabs);
                
            case RoomType.Boss:
                return GetRandomPrefab(bossRoomPrefabs);
                
            case RoomType.Shop:
                return GetRandomPrefab(shopRoomPrefabs);
                
            case RoomType.Treasure:
                return GetRandomPrefab(treasureRoomPrefabs);
                
            case RoomType.Secret:
                return GetRandomPrefab(secretRoomPrefabs);
                
            default:
                return null;
        }
    }

    /// <summary>
    /// Get a random prefab from a list
    /// </summary>
    private GameObject GetRandomPrefab(List<GameObject> prefabs)
    {
        if (prefabs == null || prefabs.Count == 0)
            return null;
            
        return prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
    }

    /// <summary>
    /// Get a grid position in a specific direction from another position
    /// </summary>
    private Vector2Int GetPositionInDirection(Vector2Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector2Int(position.x, position.y + 1);
                
            case Direction.East:
                return new Vector2Int(position.x + 1, position.y);
                
            case Direction.South:
                return new Vector2Int(position.x, position.y - 1);
                
            case Direction.West:
                return new Vector2Int(position.x - 1, position.y);
                
            default:
                return position;
        }
    }

    /// <summary>
    /// Get a list of directions in random order
    /// </summary>
    private List<Direction> GetRandomDirections()
    {
        List<Direction> directions = new List<Direction>
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };
        
        // Fisher-Yates shuffle
        for (int i = 0; i < directions.Count - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, directions.Count);
            Direction temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
        
        return directions;
    }

    /// <summary>
    /// Shuffle a list using Fisher-Yates algorithm
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
} 