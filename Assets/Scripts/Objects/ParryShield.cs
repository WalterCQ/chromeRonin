using UnityEngine;

public class ParryShield : MonoBehaviour
{
    [Header("Settings")]
    public GameObject reflectedBulletPrefab; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);

            // 2. Spawn Reflected Bullet
            GameObject reflected = Instantiate(reflectedBulletPrefab, other.transform.position, Quaternion.identity);
            ReflectedBullet script = reflected.GetComponent<ReflectedBullet>();

            if (script != null)
            {
                // 3. Aim towards Mouse Cursor
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;

                Vector2 aimDir = (mousePos - transform.position).normalized;
                
                script.Launch(aimDir);
            }
        }
    }
}