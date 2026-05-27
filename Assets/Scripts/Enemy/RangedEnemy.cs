using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RangedEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;
    public Transform edgeCheck;
    public bool isFlyingEnemy = false; 

    [Header("Collision Settings")]
    public LayerMask obstacleLayers;

    [Header("Combat Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float attackRange = 8f;
    public float fireRate = 2f;
    public LayerMask sightObstacles;
    public float stopDistance = 6f; 

    [Header("Visuals")]
    public LineRenderer laserSight; 
    public Animator animator;       
    public float chargeUpTime = 0.5f; 

    [Header("Smart AI Settings")]
    public float chaseRange = 10f;
    public float loseAggroTime = 2f;

    private Transform _player;
    private Rigidbody2D _rb;
    private bool _movingRight = true;
    private float _nextFireTime;
    private Collider2D _myCollider;

    private bool _isChasing = false;
    private bool _wasChasing = false;
    private float _lastSawPlayerTime;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _myCollider = GetComponent<Collider2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) _player = playerObj.transform;
        
        // Use transform as fallback, but this usually causes the bug if a child isn't assigned
        if (!edgeCheck) edgeCheck = transform;
        
        if (!firePoint) firePoint = transform;
        if (animator == null) animator = GetComponent<Animator>();
        if (laserSight) laserSight.enabled = false;
    }

    void Update()
    {
        bool canSeePlayer = CheckLineOfSight();
        float distToPlayer = _player ? Vector2.Distance(transform.position, _player.position) : 999f;

        // State Switching
        if (distToPlayer < chaseRange && canSeePlayer)
        {
            _isChasing = true;
            _lastSawPlayerTime = Time.time;
        }
        else if (_isChasing && Time.time > _lastSawPlayerTime + loseAggroTime)
        {
            _isChasing = false;
        }

        // Indicator
        if (_isChasing && !_wasChasing)
        {
            if (AggroIndicatorManager.Instance != null)
                AggroIndicatorManager.Instance.ShowAggro(transform);
        }
        _wasChasing = _isChasing;

        // AI Logic
        if (_isChasing && _player != null)
        {
            if (distToPlayer <= stopDistance && canSeePlayer)
            {
                StopMoving();
                FacePlayer();
                HandleShooting();
            }
            else
            {
                if (laserSight) laserSight.enabled = false;
                ChasePlayer();
            }
        }
        else
        {
            if (laserSight) laserSight.enabled = false;
            Patrol();
        }
    }

    void Patrol()
    {
        // FIX: Remove the "Center Raycast" which was breaking the transition.
        // Instead, just ensure we aren't literally falling (y velocity ~ 0)
        
        bool isGrounded = Mathf.Abs(_rb.velocity.y) < 0.1f;

        if (!isFlyingEnemy && isGrounded)
        {
            // Check the Edge Detector
            RaycastHit2D edgeInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            
            // If Edge Detector sees Nothing -> We are at a cliff. Flip.
            if (!edgeInfo.collider)
            {
                Flip();
            }
        }
        
        // Apply velocity
        _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);
    }

    void ChasePlayer()
    {
        FacePlayer();

        // 1. Check for Walls
        Vector2 direction = _movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallCheck = Physics2D.Raycast(edgeCheck.position, direction, 0.5f, sightObstacles);
        if (wallCheck.collider != null) 
        { 
            StopMoving();
            _isChasing = false; // Give up
            return; 
        }

        // 2. Check for Cliffs
        if (!isFlyingEnemy)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            if (!groundInfo.collider)
            {
                StopMoving();
                _isChasing = false; // Give up
                return;
            }
        }

        // 3. Move
        _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);
    }

    void StopMoving()
    {
        _rb.velocity = new Vector2(0, _rb.velocity.y);
    }

    void FacePlayer()
    {
        if (!_player) return;
        if (_player.position.x > transform.position.x && !_movingRight) Flip();
        else if (_player.position.x < transform.position.x && _movingRight) Flip();
    }

    void Flip()
    {
        _movingRight = !_movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isChasing) return; // Don't flip on collision while chasing
        
        // Flip if we hit an obstacle layer
        if ((obstacleLayers.value & (1 << collision.gameObject.layer)) > 0)
        {
            Flip();
        }
    }

    bool CheckLineOfSight()
    {
        if (!_player) return false;
        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > attackRange && dist > chaseRange) return false;

        Vector2 dir = (_player.position - firePoint.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dir, dist, sightObstacles);

        if (hit.collider != null && hit.collider.transform != _player) return false;
        return true;
    }

    void HandleShooting()
    {
        if (laserSight)
        {
            laserSight.enabled = true;
            laserSight.SetPosition(0, firePoint.position);
            laserSight.SetPosition(1, _player.position);
        }

        if (Time.time > _nextFireTime)
        {
            _nextFireTime = Time.time + fireRate;
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence()
    {
        // Check if the "Shoot" parameter exists before triggering
        if (animator && HasAnimatorParameter(animator, "Shoot"))
        {
            animator.SetTrigger("Shoot");
        }
        yield return new WaitForSeconds(chargeUpTime);
        Shoot();
    }

    private bool HasAnimatorParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    void Shoot()
    {
        if (!bulletPrefab || !firePoint || !_player) return;
        
        // Calculate rotation BEFORE spawning
        Vector2 dir = (_player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Spawn with correct rotation
        GameObject bullet = ObjectPool.Instance.Get(bulletPrefab, firePoint.position, rotation);
        
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol && _myCollider) Physics2D.IgnoreCollision(bulletCol, _myCollider);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, chaseRange);
        if (edgeCheck) { Gizmos.color = Color.green; Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * groundCheckDistance); }
    }
}