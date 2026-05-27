using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Moving platform with configurable range and speed.
/// Works with one-way platforms. Uses velocity transfer without affecting player controls.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;
    
    [Tooltip("Distance to move left (relative to start)")]
    public float leftDistance = 3f;
    
    [Tooltip("Distance to move right (relative to start)")]
    public float rightDistance = 3f;
    
    [Header("Behavior Settings")]
    [Tooltip("Pause when reaching boundaries")]
    public bool pauseAtEdge = false;
    
    [Tooltip("Pause duration at boundaries (seconds)")]
    public float pauseDuration = 0.5f;
    
    [Tooltip("Start moving to the right")]
    public bool startMovingRight = true;
    
    [Header("Visualization")]
    [Tooltip("Show movement range in editor")]
    public bool showGizmos = true;
    
    [Tooltip("Gizmo Color")]
    public Color gizmoColor = Color.yellow;

    private Vector3 _startPosition;
    private Vector3 _lastPosition;
    private float _leftBoundary;
    private float _rightBoundary;
    private int _direction; // 1 = right, -1 = left
    private float _pauseTimer;
    private bool _isPaused;
    
    // Stores objects standing on the platform
    private HashSet<Transform> _passengers = new HashSet<Transform>();
    private Vector3 _platformVelocity;

    private void Start()
    {
        _startPosition = transform.position;
        _lastPosition = transform.position;
        _leftBoundary = _startPosition.x - leftDistance;
        _rightBoundary = _startPosition.x + rightDistance;
        _direction = startMovingRight ? 1 : -1;
        _pauseTimer = 0f;
        _isPaused = false;
    }

    private void FixedUpdate()
    {
        // Record position before movement
        Vector3 oldPosition = transform.position;
        
        if (_isPaused)
        {
            _pauseTimer -= Time.fixedDeltaTime;
            if (_pauseTimer <= 0f)
            {
                _isPaused = false;
            }
            _platformVelocity = Vector3.zero;
        }
        else
        {
            // Move platform
            Vector3 newPosition = transform.position;
            newPosition.x += _direction * moveSpeed * Time.fixedDeltaTime;

            // Check boundaries
            if (newPosition.x >= _rightBoundary)
            {
                newPosition.x = _rightBoundary;
                _direction = -1;
                if (pauseAtEdge)
                {
                    _isPaused = true;
                    _pauseTimer = pauseDuration;
                }
            }
            else if (newPosition.x <= _leftBoundary)
            {
                newPosition.x = _leftBoundary;
                _direction = 1;
                if (pauseAtEdge)
                {
                    _isPaused = true;
                    _pauseTimer = pauseDuration;
                }
            }

            transform.position = newPosition;
            
            // Calculate platform velocity
            _platformVelocity = (newPosition - oldPosition);
        }
        
        // Move passengers on the platform
        MovePassengers();
        
        _lastPosition = transform.position;
    }

    private void MovePassengers()
    {
        // Remove destroyed passengers
        _passengers.RemoveWhere(p => p == null);
        
        foreach (Transform passenger in _passengers)
        {
            if (passenger != null)
            {
                // Move passenger position to follow platform
                Rigidbody2D rb = passenger.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Use MovePosition for Rigidbody objects
                    rb.position += (Vector2)_platformVelocity;
                }
                else
                {
                    passenger.position += _platformVelocity;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collision is from above (player on platform)
        if (IsPassengerOnTop(collision))
        {
            _passengers.Add(collision.transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Continuous check to ensure player stays on platform
        if (IsPassengerOnTop(collision))
        {
            if (!_passengers.Contains(collision.transform))
            {
                _passengers.Add(collision.transform);
            }
        }
        else
        {
            _passengers.Remove(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _passengers.Remove(collision.transform);
    }

    /// <summary>
    /// Checks if the colliding object is on top of the platform
    /// </summary>
    private bool IsPassengerOnTop(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If contact normal points upward, object is on top
            if (contact.normal.y < -0.5f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Visualizes movement range in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 basePos = Application.isPlaying ? _startPosition : transform.position;
        
        Gizmos.color = gizmoColor;
        
        // Draw left boundary
        Vector3 leftPos = basePos;
        leftPos.x -= leftDistance;
        Gizmos.DrawWireSphere(leftPos, 0.3f);
        
        // Draw right boundary
        Vector3 rightPos = basePos;
        rightPos.x += rightDistance;
        Gizmos.DrawWireSphere(rightPos, 0.3f);
        
        // Draw connecting line
        Gizmos.DrawLine(leftPos, rightPos);
        
        // Draw start position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(basePos, 0.2f);
    }

    /// <summary>
    /// Sets a new movement range at runtime
    /// </summary>
    public void SetMoveRange(float left, float right)
    {
        leftDistance = left;
        rightDistance = right;
        _leftBoundary = _startPosition.x - leftDistance;
        _rightBoundary = _startPosition.x + rightDistance;
    }

    /// <summary>
    /// Sets movement speed at runtime
    /// </summary>
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// Pause/Resume platform movement
    /// </summary>
    public void SetPaused(bool paused)
    {
        _isPaused = paused;
        if (!paused)
        {
            _pauseTimer = 0f;
        }
    }
}
