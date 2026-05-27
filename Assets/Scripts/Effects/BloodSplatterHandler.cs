using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles particle collisions to spawn permanent blood decals on surfaces.
/// Attach this to the blood particle system GameObject.
/// Requires Collision module to be enabled with "Send Collision Messages" checked.
/// </summary>
public class BloodSplatterHandler : MonoBehaviour 
{
    [Header("Decal Settings")]
    [Tooltip("Array of blood stain sprite prefabs for variety")]
    public GameObject[] splatterPrefabs;
    
    [Tooltip("Parent transform for spawned decals (auto-created if null)")]
    public Transform decalContainer;
    
    [Tooltip("Maximum number of decals allowed (0 = unlimited, set limit for performance)")]
    public int maxDecals = 100;
    
    [Tooltip("Scale range for random decal sizing")]
    public Vector2 scaleRange = new Vector2(0.1f, 0.4f);
    
    [Tooltip("Lifetime of decals in seconds (0 = permanent)")]
    public float decalLifetime = 3f;
    
    [Header("Spawn Settings")]
    [Tooltip("Chance to spawn a decal on each collision (0-1)")]
    [Range(0f, 1f)]
    public float spawnChance = 0.7f;
    
    [Tooltip("Small offset from surface to prevent z-fighting")]
    public float surfaceOffset = 0.01f;
    
    [Header("Sorting")]
    [Tooltip("Sorting layer name for spawned decals")]
    public string sortingLayerName = "Default";
    
    [Tooltip("Order in sorting layer")]
    public int sortingOrder = -10;
    
    private ParticleSystem bloodParticleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private Queue<GameObject> spawnedDecals = new Queue<GameObject>();

    void Start()
    {
        bloodParticleSystem = GetComponent<ParticleSystem>();
        
        if (bloodParticleSystem == null)
        {
            Debug.LogError("BloodSplatterHandler: No ParticleSystem found on this GameObject!");
            enabled = false;
            return;
        }
        
        if (decalContainer == null)
        {
            // Create a container for organization
            GameObject container = new GameObject("BloodDecalContainer");
            decalContainer = container.transform;
            // Don't destroy with scene load for persistent blood
            // DontDestroyOnLoad(container); // Uncomment if you want decals to persist
        }
        
        // Validate collision module is set up
        var collision = bloodParticleSystem.collision;
        if (!collision.enabled)
        {
            Debug.LogWarning("BloodSplatterHandler: Particle System Collision module is not enabled! " +
                           "Enable it and check 'Send Collision Messages' for decals to work.");
        }
    }

    void OnParticleCollision(GameObject other) 
    {
        if (splatterPrefabs == null || splatterPrefabs.Length == 0)
        {
            Debug.LogWarning("BloodSplatterHandler: No splatter prefabs assigned!");
            return;
        }
        
        int numCollisionEvents = bloodParticleSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++) 
        {
            // Random chance to skip spawning (for performance and variety)
            if (Random.value > spawnChance) continue;
            
            SpawnDecal(collisionEvents[i]);
        }
    }
    
    private void SpawnDecal(ParticleCollisionEvent collisionEvent)
    {
        Vector3 pos = collisionEvent.intersection;
        Vector3 normal = collisionEvent.normal;
        
        // Add small offset to prevent z-fighting
        pos += normal * surfaceOffset;
        
        // Randomly select a splatter sprite
        GameObject prefab = splatterPrefabs[Random.Range(0, splatterPrefabs.Length)];
        if (prefab == null) return;
        
        // Instantiate and rotate to align with the surface normal
        GameObject splatter = Instantiate(prefab, pos, Quaternion.identity, decalContainer);
        
        // For 2D, align the sprite to the surface
        // Calculate rotation to face the camera while aligning to surface
        if (Mathf.Abs(normal.z) > 0.9f)
        {
            // Surface is roughly parallel to XY plane (floor/ceiling in 2D)
            splatter.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }
        else
        {
            // Surface is a wall - align sprite "up" with the surface normal
            splatter.transform.up = normal;
            splatter.transform.Rotate(0, 0, Random.Range(0f, 360f)); // Random rotation for variety
        }
        
        // Set a random scale for realism
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        splatter.transform.localScale = new Vector3(scale, scale, 1);
        
        // Configure sorting layer
        SpriteRenderer sr = splatter.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
        
        // Handle max decals limit (remove oldest decals when limit reached)
        if (maxDecals > 0)
        {
            spawnedDecals.Enqueue(splatter);
            while (spawnedDecals.Count > maxDecals)
            {
                GameObject oldDecal = spawnedDecals.Dequeue();
                if (oldDecal != null) Destroy(oldDecal);
            }
        }
        
        // Handle decal lifetime (0 = permanent)
        if (decalLifetime > 0)
        {
            Destroy(splatter, decalLifetime);
        }
    }
    
    /// <summary>
    /// Clears all spawned blood decals from the scene.
    /// </summary>
    public void ClearAllDecals()
    {
        while (spawnedDecals.Count > 0)
        {
            GameObject decal = spawnedDecals.Dequeue();
            if (decal != null) Destroy(decal);
        }
        
        // Also clear any remaining children in container
        if (decalContainer != null)
        {
            foreach (Transform child in decalContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
