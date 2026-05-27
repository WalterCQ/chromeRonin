using UnityEngine;

public class ScreenWrapTeleport : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Teleport destination height (Y coordinate)")]
    public float topSpawnY = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it's the player
        if (collision.CompareTag("Player"))
        {
            // Get player's current position
            Vector3 currentPos = collision.transform.position;

            // Keep X and Z, only change Y to destination height
            collision.transform.position = new Vector3(currentPos.x, topSpawnY, currentPos.z);
            
            // Reset velocity to prevent clipping through floor at high falling speed
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Preserve horizontal velocity, reset vertical velocity
                rb.velocity = new Vector2(rb.velocity.x, 0); 
            }
        }
    }
}