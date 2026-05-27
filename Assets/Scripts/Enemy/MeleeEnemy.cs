using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class MeleeEnemy : MonoBehaviour
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

    [Header("Smart AI Settings")]
    public float chaseRange = 5f;
    public float loseAggroTime = 2f;
    public LayerMask sightObstacles;

    private Transform _player;
    private bool _isChasing = false;
    private bool _wasChasing = false;
    private float _lastSawPlayerTime;

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

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) _player = p.transform;
    }

    void FixedUpdate()
    {
        // AI & Aggro Logic
        if (_player != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, _player.position);
            bool canSee = CanSeePlayer();

            if (distToPlayer < chaseRange && canSee)
            {
                _isChasing = true;
                _lastSawPlayerTime = Time.time;
            }
            else if (_isChasing && Time.time > _lastSawPlayerTime + loseAggroTime)
            {
                _isChasing = false;
            }
        }

        // Indicator
        if (_isChasing && !_wasChasing)
        {
            if (AggroIndicatorManager.Instance != null)
                AggroIndicatorManager.Instance.ShowAggro(transform);
        }
        _wasChasing = _isChasing;

        if (_isChasing) ChasePlayer();
        else Patrol();
        
        HandleAttack();
    }

    void Patrol()
    {
        // FIX 1: Check Edge BEFORE moving
        // If we move first, we spend 1 frame in the air before flipping.
        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        
        if (!groundInfo.collider)
        {
            Flip();
            // Important: Don't return! Move in the NEW direction immediately so we don't freeze.
        }

        _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);
    }

    void ChasePlayer()
    {
        if (_player == null) return;

        // 1. Face Player
        float xDiff = _player.position.x - transform.position.x;
        if (xDiff > 0 && !_movingRight) Flip();
        else if (xDiff < 0 && _movingRight) Flip();

        // 2. Wall Check
        Vector2 direction = _movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallCheck = Physics2D.Raycast(edgeCheck.position, direction, 0.5f, sightObstacles);
        if (wallCheck.collider != null) { _rb.velocity = new Vector2(0, _rb.velocity.y); return; }

        // 3. Cliff Check
        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        if (groundInfo.collider)
        {
            _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
    }

    bool CanSeePlayer()
    {
        if (_player == null) return false;
        Vector2 dir = (_player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, _player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, sightObstacles);
        if (hit.collider != null && hit.collider.transform != _player) return false;
        return true;
    }

    void HandleAttack()
    {
        if (Time.time < _nextAttackTime) return;
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, _playerLayerMask);
        if (hitPlayer)
        {
            IDamageable playerHealth = hitPlayer.GetComponent<IDamageable>();
            if (playerHealth == null) playerHealth = hitPlayer.GetComponentInParent<IDamageable>(); // Check parent
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                _nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isChasing) return; 

        // Check if layer is NOT ignored
        if (((1 << collision.gameObject.layer) & ignoreCollisionLayers) == 0)
        {
            // FIX 2: Check Contact Normal
            // Hitting the floor creates a collision with Normal.y = 1.
            // Hitting a wall creates a collision with Normal.y = 0.
            // We ONLY want to flip if we hit a wall.
            foreach(ContactPoint2D contact in collision.contacts)
            {
                // If the normal is horizontal (wall), then flip.
                if (Mathf.Abs(contact.normal.y) < 0.5f)
                {
                    Flip();
                    return; // Stop checking contacts
                }
            }
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
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}