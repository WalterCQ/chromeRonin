<<<<<<< HEAD
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))] 
public class PistolEnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float attackRange = 10f;
    public float fireRate = 3f;
    public LayerMask sightObstacles;

    [Header("Visuals")]
    public LineRenderer laserSight; 

    private Transform player;
    private float nextFireTime;
    private Collider2D _myCollider;
    private Coroutine _aimCoroutine;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        _myCollider = GetComponent<Collider2D>(); 

        if (!laserSight) laserSight = GetComponent<LineRenderer>();
        if (laserSight) laserSight.enabled = false;
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool hasSight = HasLineOfSight();
        
        // Logic: 
        // 1. If in range AND visible: Check if we can shoot.
        // 2. If shooting is in progress (aiming) but sight is lost: STOP aiming.
        
        if (dist < attackRange && hasSight)
        {
            if (Time.time > nextFireTime && _aimCoroutine == null)
            {
                _aimCoroutine = StartCoroutine(ShootSequence());
            }
        }
        else
        {
            // If player goes out of range or hides behind a wall
            if (_aimCoroutine != null)
            {
                StopCoroutine(_aimCoroutine);
                _aimCoroutine = null;
                if (laserSight) laserSight.enabled = false;
            }
        }
    }

    bool HasLineOfSight()
    {
        Vector2 direction = (player.position - firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, distance, sightObstacles);

        // If we hit something (that isn't the player/null), vision is blocked
        if (hit.collider != null)
        {
            return false;
        }
        return true;
    }

    IEnumerator ShootSequence()
    {
        if (laserSight) laserSight.enabled = true;
        float aimDuration = 1.0f;
        float timer = 0;

        while (timer < aimDuration && player != null)
        {
            if (laserSight)
            {
                laserSight.SetPosition(0, firePoint.position);

                // Calculate direction to player
                Vector2 dir = (player.position - firePoint.position).normalized;
                float distToPlayer = Vector2.Distance(firePoint.position, player.position);

                // Cast ray to find WHERE the laser should stop
                RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dir, distToPlayer, sightObstacles);

                if (hit.collider != null)
                {
                    laserSight.SetPosition(1, hit.point);
                }
                else
                {
                    laserSight.SetPosition(1, player.position);
                }
            }

            if (!HasLineOfSight())
            {
                laserSight.enabled = false;
                _aimCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (laserSight) laserSight.enabled = false;
        
        if(HasLineOfSight())
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        
        _aimCoroutine = null;
    }

    void Shoot()
    {
        if (bulletPrefab && firePoint && player)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            if (bulletCollider != null && _myCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, _myCollider);
            }

            Vector2 dir = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
=======
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class TurretAI : MonoBehaviour
{
    [Header("Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform headPivot; 
    public float attackRange = 10f;
    public float fireRate = 3f;
    public LayerMask sightObstacles;

    [Header("Movement")]
    public float viewAngle = 90f; 
    public float rotationSpeed = 200f; 

    [Header("Art Fixes")]
    [Tooltip("Rotates the sprite only. Leave at 0 usually.")]
    public float rotationOffset = 0f;

    [Header("Visuals")]
    public LineRenderer laserSight;
    public bool showGizmos = true;

    private Transform player;
    private float nextFireTime;
    private Collider2D _myCollider;
    private Coroutine _aimCoroutine;
    
    private float defaultRotationZ; 
    private float initialX;
    private float initialY;
    
    private const float CONE_OFFSET = 180f;

    void Start()
    {
        if (headPivot != null)
        {
            defaultRotationZ = headPivot.eulerAngles.z;
            initialX = headPivot.eulerAngles.x;
            initialY = headPivot.eulerAngles.y;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        _myCollider = GetComponent<Collider2D>();

        if (!laserSight) laserSight = GetComponent<LineRenderer>();
        if (laserSight) laserSight.enabled = false;
    }

    void Update()
    {
        if (!player || !headPivot) return;

        float dist = Vector2.Distance(transform.position, player.position);
        
        bool inCone = IsPlayerInCone(); 

        if (dist < attackRange && inCone)
        {
            RotateHeadTowardsPlayer(); 
            
            if (HasLineOfSight())
            {
                if (Time.time > nextFireTime && _aimCoroutine == null)
                {
                    _aimCoroutine = StartCoroutine(ShootSequence());
                }
            }
            else
            {
                StopShooting(); 
            }
        }
        else
        {
            ReturnToIdle(); 
            StopShooting();
        }
    }

    void RotateHeadTowardsPlayer()
    {
        Vector3 dir = player.position - headPivot.position;
        float angleToPlayer = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float centerAngle = defaultRotationZ + CONE_OFFSET;
        
        float angleDifference = Mathf.DeltaAngle(centerAngle, angleToPlayer);
        float clampedDifference = Mathf.Clamp(angleDifference, -viewAngle / 2f, viewAngle / 2f);
        
        float finalAngleZ = centerAngle + clampedDifference;

        Quaternion targetRotation = Quaternion.Euler(initialX, initialY, finalAngleZ + rotationOffset);

        headPivot.rotation = Quaternion.RotateTowards(
            headPivot.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }

    void ReturnToIdle()
    {
        float idleAngle = defaultRotationZ + CONE_OFFSET + rotationOffset;

        Quaternion idleRotation = Quaternion.Euler(initialX, initialY, idleAngle);
        
        headPivot.rotation = Quaternion.RotateTowards(
            headPivot.rotation, 
            idleRotation, 
            rotationSpeed * Time.deltaTime
        );
    }

    bool IsPlayerInCone()
    {
        Vector3 dir = player.position - headPivot.position;
        float angleToPlayer = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        float centerAngle = defaultRotationZ + CONE_OFFSET;
        
        float angleDifference = Mathf.DeltaAngle(centerAngle, angleToPlayer);
        return Mathf.Abs(angleDifference) <= viewAngle / 2f;
    }

    void StopShooting()
    {
        if (_aimCoroutine != null)
        {
            StopCoroutine(_aimCoroutine);
            _aimCoroutine = null;
            if (laserSight) laserSight.enabled = false;
        }
    }

    bool HasLineOfSight()
    {
        if (firePoint == null) return false;  
        Vector2 direction = (player.position - firePoint.position).normalized;
        float distance = Vector2.Distance(firePoint.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, distance, sightObstacles);
        if (hit.collider != null) return false; 
        return true;
    }

    IEnumerator ShootSequence()
    {
        if (laserSight) laserSight.enabled = true;
        float aimDuration = 1.0f; 
        float timer = 0;

        while (timer < aimDuration && player != null)
        {
            if (laserSight)
            {
                laserSight.SetPosition(0, firePoint.position);
                Vector2 dir = (player.position - firePoint.position).normalized;
                float distToPlayer = Vector2.Distance(firePoint.position, player.position);
                RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dir, distToPlayer, sightObstacles);
                if (hit.collider != null) laserSight.SetPosition(1, hit.point);
                else laserSight.SetPosition(1, player.position);
            }

            if (!HasLineOfSight() || !IsPlayerInCone())
            {
                laserSight.enabled = false;
                _aimCoroutine = null;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (laserSight) laserSight.enabled = false;

        Shoot();
        nextFireTime = Time.time + fireRate;
        _aimCoroutine = null;
    }

    void Shoot()
    {
        if (bulletPrefab && firePoint && player)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            if (bulletCollider != null && _myCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, _myCollider);
            }
            Vector2 dir = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || headPivot == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        float lineLen = 5f;

        float baseAngle = Application.isPlaying ? defaultRotationZ : headPivot.eulerAngles.z;
        
        float drawAngle = baseAngle + CONE_OFFSET;

        Vector3 leftDir = Quaternion.AngleAxis(drawAngle - viewAngle / 2f, Vector3.forward) * Vector3.right;
        Gizmos.DrawLine(headPivot.position, headPivot.position + leftDir * lineLen);

        Vector3 rightDir = Quaternion.AngleAxis(drawAngle + viewAngle / 2f, Vector3.forward) * Vector3.right;
        Gizmos.DrawLine(headPivot.position, headPivot.position + rightDir * lineLen);
        
        Gizmos.DrawLine(headPivot.position + leftDir * lineLen, headPivot.position + rightDir * lineLen);
    }
>>>>>>> upstream/dev
}