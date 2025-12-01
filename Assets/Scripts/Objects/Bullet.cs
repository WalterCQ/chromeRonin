using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f;
    [Tooltip("Sets the scale of the bullet (0.3 is good for precise parries).")]
    public float bulletSize = 0.3f;

    [Header("Collision Settings")]
    [Tooltip("Select all layers that should destroy the bullet (e.g., Ground, Wall, Barrier)")]
    public LayerMask solidLayers; 
    
    [Header("Game Modes")]
    [Tooltip("If true, bullets hurt enemies too!")]
    public bool friendlyFire = false; 

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(bulletSize, bulletSize, 1f);
    }

    void Start()
    {
        if (rb) rb.velocity = transform.right * speed; 
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if ((solidLayers.value & (1 << hitInfo.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
            return;
        }

        IDamageable target = hitInfo.GetComponent<IDamageable>();
        if (target != null)
        {
            if (friendlyFire)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (hitInfo.CompareTag("Player"))
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}