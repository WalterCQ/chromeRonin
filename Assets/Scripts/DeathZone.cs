using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Health>().TakeDamage(9999);
        }
        else if (collision.CompareTag("Enemy"))
        {
             Destroy(collision.gameObject);
        }
    }
}