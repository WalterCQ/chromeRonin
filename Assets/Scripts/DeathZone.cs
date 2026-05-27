using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // "Player" is a built-in tag, so this is safe.
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(9999);
            }
        }
        else
        {
            // Check if the object has a Health component and is marked as an enemy.
            // This is safer than using CompareTag("Enemy") because the tag might not be defined in Unity.
            Health health = collision.GetComponent<Health>();
            if (health != null && health.isEnemy)
            {
                Destroy(collision.gameObject);
            }
        }
    }
}