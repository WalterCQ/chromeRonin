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
}