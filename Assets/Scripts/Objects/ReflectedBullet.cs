<<<<<<< HEAD
using UnityEngine;

public class ReflectedBullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 20f; 
    public int damage = 100; 
    public float bulletSize = 0.3f; 

    [Header("Collision Settings")]
    public LayerMask solidLayers; 
    
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(bulletSize, bulletSize, 1f);
    }

    void Start()
    {
        Destroy(gameObject, 3f); 

        // Ignore Player Collision
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D myCollider = GetComponent<Collider2D>();
            if (playerCollider != null && myCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, myCollider);
            }
        }
    }

    // Called by ParryShield.cs
    public void Launch(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.velocity = direction.normalized * speed;
        
        // Rotate visual to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player")) return;

        // Wall/Ground Check
        if ((solidLayers.value & (1 << hitInfo.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
            return;
        }

        // Damage Check (Enemies, Switches, Breakables)
        IDamageable target = hitInfo.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
=======
using UnityEngine;

public class ReflectedBullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 20f; 
    public int damage = 100; 
    public float bulletSize = 0.3f; 

    [Header("Collision Settings")]
    public LayerMask solidLayers; 
    
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(bulletSize, bulletSize, 1f);
    }

    void Start()
    {
        Destroy(gameObject, 3f); 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D myCollider = GetComponent<Collider2D>();
            if (playerCollider != null && myCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, myCollider);
            }
        }
    }

    public void Launch(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.velocity = direction.normalized * speed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player")) return;

        if ((solidLayers.value & (1 << hitInfo.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
            return;
        }

        IDamageable target = hitInfo.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
>>>>>>> upstream/dev
}