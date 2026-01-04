<<<<<<< HEAD
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement AI")]
    public float moveSpeed = 2f;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;
    public Transform edgeCheck;

    [Header("Collision Settings")]
    [Tooltip("Enemy will flip when hitting ANYTHING, except layers selected here.")]
    public LayerMask ignoreCollisionLayers;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackCooldown = 1.0f;
    public int damageToPlayer = 1;

    private float _nextAttackTime = 0f;
    private bool _movingRight = true;
    private Rigidbody2D _rb;
    private LayerMask _playerLayerMask;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (!edgeCheck) edgeCheck = transform;
        if (!attackPoint) attackPoint = transform;
        _playerLayerMask = LayerMask.GetMask("Player");
    }

    void FixedUpdate()
    {
        Patrol();
        HandleAttack();
    }

    void Patrol()
    {
        _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);

        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        if (!groundInfo.collider) Flip();
    }

    void HandleAttack()
    {
        if (Time.time < _nextAttackTime) return;

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, _playerLayerMask);
        if (hitPlayer)
        {
            IDamageable playerHealth = hitPlayer.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                _nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // LOGIC: If the object we hit is NOT in the "Ignore" list, then we flip.
        
        // Bitwise Check: "Is the layer of the thing we hit inside our Ignore Mask?"
        if (((1 << collision.gameObject.layer) & ignoreCollisionLayers) == 0)
        {
            // It was NOT in the ignore list (so it's a wall, ground, or obstacle)
            Flip();
        }
    }

    void Flip()
    {
        _movingRight = !_movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnDrawGizmos()
    {
        if (edgeCheck) { Gizmos.color = Color.red; Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * groundCheckDistance); }
        if (attackPoint) { Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(attackPoint.position, attackRange); }
    }
=======
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement AI")]
    public float moveSpeed = 2f;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;
    public Transform edgeCheck;

    [Header("Collision Settings")]
    [Tooltip("Enemy will flip when hitting ANYTHING, except layers selected here.")]
    public LayerMask ignoreCollisionLayers;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackCooldown = 1.0f;
    public int damageToPlayer = 1;

    private float _nextAttackTime = 0f;
    private bool _movingRight = true;
    private Rigidbody2D _rb;
    private LayerMask _playerLayerMask;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (!edgeCheck) edgeCheck = transform;
        if (!attackPoint) attackPoint = transform;
        _playerLayerMask = LayerMask.GetMask("Player");
    }

    void FixedUpdate()
    {
        Patrol();
        HandleAttack();
    }

    void Patrol()
    {
        _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);

        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        if (!groundInfo.collider) Flip();
    }

    void HandleAttack()
    {
        if (Time.time < _nextAttackTime) return;

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, _playerLayerMask);
        if (hitPlayer)
        {
            IDamageable playerHealth = hitPlayer.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                _nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // LOGIC: If the object we hit is NOT in the "Ignore" list, then we flip.
        
        // Bitwise Check: "Is the layer of the thing we hit inside our Ignore Mask?"
        if (((1 << collision.gameObject.layer) & ignoreCollisionLayers) == 0)
        {
            // It was NOT in the ignore list (so it's a wall, ground, or obstacle)
            Flip();
        }
    }

    void Flip()
    {
        _movingRight = !_movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnDrawGizmos()
    {
        if (edgeCheck) { Gizmos.color = Color.red; Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * groundCheckDistance); }
        if (attackPoint) { Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(attackPoint.position, attackRange); }
    }
>>>>>>> upstream/dev
}