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
    [Tooltip("If true, this enemy will NOT turn around at cliffs. Ideal for Drones.")]
    public bool isFlyingEnemy = false; 

    [Header("Collision Settings")]
    [Tooltip("Select all layers that should make the enemy flip (e.g., Ground, Wall, Barrier).")]
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

    private Transform _player;
    private Rigidbody2D _rb;
    private bool _movingRight = true;
    private float _nextFireTime;
    private bool _isShooting = false;
    private Collider2D _myCollider;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _myCollider = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) _player = playerObj.transform;

        if (!edgeCheck) edgeCheck = transform;
        if (!firePoint) firePoint = transform;

        if (animator == null) animator = GetComponent<Animator>();

        if (laserSight) laserSight.enabled = false;
    }

    void Update()
    {
        bool canSeePlayer = CheckLineOfSight();
        float distToPlayer = _player ? Vector2.Distance(transform.position, _player.position) : 999f;

        if (canSeePlayer && distToPlayer <= attackRange)
        {
            if (distToPlayer <= stopDistance)
            {
                StopMoving();
                FacePlayer();
                HandleShooting(); 
            }
            else
            {
                if (laserSight) laserSight.enabled = false; 
                Patrol(); 
            }
        }
        else
        {
            _isShooting = false;
            if (laserSight) laserSight.enabled = false;
            Patrol();
        }
    }

    void Patrol()
    {
        _rb.velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, _rb.velocity.y);

        if (!isFlyingEnemy)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            if (!groundInfo.collider)
            {
                Flip();
            }
        }
    }

    void StopMoving()
    {
        _rb.velocity = new Vector2(0, _rb.velocity.y);
    }

    void FacePlayer()
    {
        if (!_player) return;

        if (_player.position.x > transform.position.x && !_movingRight)
        {
            Flip();
        }
        else if (_player.position.x < transform.position.x && _movingRight)
        {
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((obstacleLayers.value & (1 << collision.gameObject.layer)) > 0)
        {
            Flip();
        }
    }

    bool CheckLineOfSight()
    {
        if (!_player) return false;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > attackRange) return false;

        Vector2 dir = (_player.position - firePoint.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dir, dist, sightObstacles);

        if (hit.collider != null && hit.collider.transform != _player)
        {
            return false;
        }
        return true;
    }

    void HandleShooting()
    {
        _isShooting = true;

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
        if (animator) animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(chargeUpTime);
        Shoot();
    }

    void Shoot()
    {
        if (!bulletPrefab || !firePoint || !_player) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol && _myCollider) Physics2D.IgnoreCollision(bulletCol, _myCollider);

        Vector2 dir = (_player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (edgeCheck && !isFlyingEnemy)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * groundCheckDistance);
        }
    }
}