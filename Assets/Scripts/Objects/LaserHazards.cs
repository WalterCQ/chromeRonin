using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(BoxCollider2D))]
public class LaserHazard : MonoBehaviour
{
    [Header("Laser Settings")]
    public int damageAmount = 999;
    public float laserLength = 10f;
    public float laserWidth = 0.3f;
    public LayerMask hitLayers; // What stops the laser (Walls, Ground, Player, Enemy)
    public LayerMask obstacleLayer; // For Movement Turnaround

    [Header("Movement Settings")]
    public bool isMoving = false;
    public float moveSpeed = 2f;
    public float wallCheckDist = 0.5f;

    [Header("Visuals")]
    public bool isAlwaysActive = true;

    private LineRenderer _lineRenderer;
    private BoxCollider2D _collider;
    private Rigidbody2D _rb;
    private bool _movingRight = true;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        
        if (isMoving)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.isKinematic = true; 
        }
        
        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (!isAlwaysActive)
        {
            _lineRenderer.enabled = false;
            _collider.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;
        _collider.enabled = true;

        UpdateLaser();
        
        if (isMoving)
        {
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        Vector2 velocity = new Vector2((_movingRight ? 1 : -1) * moveSpeed, 0);
        transform.Translate(velocity * Time.deltaTime);

        Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, wallCheckDist, obstacleLayer);

        if (hit.collider != null)
        {
            Flip();
        }
    }

    void Flip()
    {
        _movingRight = !_movingRight;
    }

    void UpdateLaser()
    {
        // 0. Sync Widths
        _lineRenderer.startWidth = laserWidth;
        _lineRenderer.endWidth = laserWidth;

        // 1. Start Point
        Vector3 startPos = transform.position;
        _lineRenderer.SetPosition(0, startPos);

        // 2. Raycast
        Vector2 dir = -transform.up; 
        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, laserLength, hitLayers);
        
        Vector3 endPos;
        float dist;

        if (hit.collider != null)
        {
            endPos = hit.point;
            dist = hit.distance;

            // --- DAMAGE LOGIC ---
            // Check for ANY damageable component (Player, Enemy, etc.)
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damageAmount);
            }
        }
        else
        {
            endPos = startPos + (Vector3)(dir * laserLength);
            dist = laserLength;
        }

        _lineRenderer.SetPosition(1, endPos);

        _collider.size = new Vector2(laserWidth, dist);
        _collider.offset = new Vector2(0, -dist / 2f); 
    }
}