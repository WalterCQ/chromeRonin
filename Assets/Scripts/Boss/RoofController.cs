using UnityEngine;

/// <summary>
/// Attach this to the roof prefab. When the roof falls to a target Y position,
/// all chunks will stop moving.
/// </summary>
public class RoofController : MonoBehaviour
{
    [Header("--- Settings ---")]
    [Tooltip("The Y position at which all platforms should freeze")]
    public float freezeAtY = 10f;
    
    [Tooltip("Fall speed (same as platforms, or set differently)")]
    public float fallSpeed = 5f;
    
    [Header("--- State (Read Only) ---")]
    [SerializeField] private bool hasFrozen = false;

    void Update()
    {
        // Don't move if game hasn't started or already frozen
        if (!FallingPlatform.hasGameStarted || FallingPlatform.isFrozen) return;
        
        // Fall like other platforms
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        
        // Check if we reached the freeze position
        if (!hasFrozen && transform.position.y <= freezeAtY)
        {
            hasFrozen = true;
            FallingPlatform.FreezeAllPlatforms();
            Debug.Log("[ROOF] Reached target Y. All platforms frozen!");
        }
    }
}
