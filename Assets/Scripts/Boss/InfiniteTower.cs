using UnityEngine;
using UnityEngine.SceneManagement;

public class InfiniteTower : MonoBehaviour
{
    [Header("--- Core Bindings ---")]
    [Tooltip("Drag in the player (optional, will auto-find)")]
    public Transform player;

    [Tooltip("Drag in Room Chunk Prefabs (Chunk A, Chunk B...)")]
    public GameObject[] roomPrefabs;

    [Tooltip("Drag in the roof Prefab for when boss dies")]
    public GameObject roofPrefab;

    [Header("--- Spawn Settings ---")]
    [Tooltip("Height of each chunk (Unity units)")]
    public float chunkHeight = 20f;

    [Tooltip("How far ahead of the player to maintain chunks (in Unity units)")]
    public float spawnAheadDistance = 60f;

    [Header("--- Position Settings ---")]
    [Tooltip("Starting Y height for first platform (default: 0)")]
    public float startHeight = 0f;

    [Tooltip("X position of the tower")]
    public float towerX = 0f;
    
    [Header("--- Debug / Tests ---")]
    [Tooltip("Any prefab containing this string will NOT spawn (Case Sensitive)")]
    public string excludeChunkName = "Chunk_B";

    [Header("--- Game State (Don't modify) ---")]
    public bool isBossDead = false;

    private float _nextSpawnY;
    private bool _isSpawning = false;
    private bool _hasInitialized = false;
    private int _lastSpawnedIndex = -1;

    [Header("--- Camera Lock Settings ---")]
    [Tooltip("Lock player to camera view (prevents speedrunning off-screen)")]
    public bool lockPlayerToCamera = true;

    [Tooltip("Max Y offset above camera center (Unity units)")]
    public float maxYAboveCamera = 4f;

    void Awake()
    {
        _hasInitialized = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _hasInitialized = false;
        CancelInvoke("Initialize");
        Invoke("Initialize", 0.1f);
    }

    void Start()
    {
        Invoke("Initialize", 0.05f);
    }
    
    void Initialize()
    {
        if (_hasInitialized) return;
        _hasInitialized = true;
        
        if (roomPrefabs == null || roomPrefabs.Length == 0) return;
        
        int validCount = 0;
        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            if (roomPrefabs[i] != null) validCount++;
        }
        if (validCount == 0) return;
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        isBossDead = false;
        _nextSpawnY = startHeight;
        _isSpawning = false;
        _lastSpawnedIndex = -1;

        // Spawn initial 3 chunks
        SpawnInitialChunk();
        SpawnInitialChunk();
        SpawnInitialChunk();
    }

    void Update()
    {
        if (isBossDead) return;

        if (!_isSpawning)
        {
            if (FallingPlatform.hasGameStarted)
            {
                _isSpawning = true;
            }
            return;
        }

        // Spawn chunks dynamically based on player position
        if (player != null)
        {
            // Keep spawning until we have enough chunks ahead of the player
            // Also check isBossDead inside loop to stop mid-frame if needed
            while (player.position.y + spawnAheadDistance > _nextSpawnY && !isBossDead)
            {
                SpawnContinuousChunk();
            }
        }
    }

    int GetUniqueRandomIndex()
    {
        if (roomPrefabs.Length == 0) return -1;
        if (roomPrefabs.Length == 1) return 0;

        int maxAttempts = 50;
        int attempts = 0;
        int newIndex = -1;

        bool isValid = false;
        while (!isValid && attempts < maxAttempts)
        {
            attempts++;
            newIndex = Random.Range(0, roomPrefabs.Length);
            
            // Check duplicates
            if (newIndex == _lastSpawnedIndex) continue;

            // Check exclusions
            if (roomPrefabs[newIndex] != null && 
                !string.IsNullOrEmpty(excludeChunkName) && 
                roomPrefabs[newIndex].name.Contains(excludeChunkName))
            {
                continue;
            }

            isValid = true;
        }

        if (isValid)
        {
            _lastSpawnedIndex = newIndex;
            return newIndex;
        }
        
        return -1;
    }

    void SpawnInitialChunk()
    {
        int index = GetUniqueRandomIndex();
        if (index == -1 || roomPrefabs[index] == null) return;
        
        Vector3 spawnPos = new Vector3(towerX, _nextSpawnY, 0);
        GameObject chunk = Instantiate(roomPrefabs[index], spawnPos, Quaternion.identity);
        
        SetTagRecursive(chunk);
        
        if (chunk.GetComponent<FallingPlatform>() == null)
        {
            FallingPlatform fp = chunk.AddComponent<FallingPlatform>();
            fp.fallSpeed = 10f;
            fp.destroyY = -30f;
        }

        // Measure the actual height of this chunk
        Renderer[] renderers = chunk.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            float actualHeight = bounds.size.y;
            _nextSpawnY += actualHeight;
        }
        else
        {
            _nextSpawnY += chunkHeight; // Fallback to default
        }
    }
    
    void SpawnContinuousChunk()
    {
        // Double-check: don't spawn if boss is dead (roof already spawned)
        if (isBossDead) return;
        
        int index = GetUniqueRandomIndex();
        if (index == -1 || roomPrefabs[index] == null) return;
        
        Vector3 spawnPos = new Vector3(towerX, _nextSpawnY, 0);
        GameObject chunk = Instantiate(roomPrefabs[index], spawnPos, Quaternion.identity);
        
        SetTagRecursive(chunk);
        
        if (chunk.GetComponent<FallingPlatform>() == null)
        {
            FallingPlatform fp = chunk.AddComponent<FallingPlatform>();
            fp.fallSpeed = 10f;
            fp.destroyY = -30f;
        }

        // Measure the actual height of this chunk
        Renderer[] renderers = chunk.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            float actualHeight = bounds.size.y;
            _nextSpawnY += actualHeight;
        }
        else
        {
            _nextSpawnY += chunkHeight; // Fallback to default
        }
    }
    
    void SetTagRecursive(GameObject obj)
    {
        if (obj.GetComponent<Collider2D>() != null)
        {
            obj.tag = "GamePlatform";
        }
        
        foreach (Transform child in obj.transform)
        {
            SetTagRecursive(child.gameObject);
        }
    }

    public void SpawnRoof()
    {
        // First, destroy all existing chunks/platforms
        GameObject[] allPlatforms = GameObject.FindGameObjectsWithTag("GamePlatform");
        foreach (GameObject platform in allPlatforms)
        {
            Destroy(platform);
        }
        
        // Spawn roof at player's position + offset above
        if (roofPrefab != null && player != null)
        {
            // Spawn roof slightly above player so they land on it
            float roofSpawnY = player.position.y - 5f;
            Vector3 spawnPos = new Vector3(towerX, roofSpawnY, 0);
            Instantiate(roofPrefab, spawnPos, Quaternion.identity);
        }
        
        this.enabled = false;
    }
}