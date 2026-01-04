<<<<<<< HEAD
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    private PlayerMovement _playerLogic; 

    [Header("Movement Settings")]
    public float moveSpeed = 3f; 
    public float lookAheadDistance = 4f; 
    public float lookAheadSpeed = 3f; 
    public float maxVertOffset = 3f;

    [Header("Level Boundaries")]
    public bool enableBounds = true;
    public Vector2 minBounds = new Vector2(-10, -10); // Bottom-Left Limit
    public Vector2 maxBounds = new Vector2(100, 20);  // Top-Right Limit

    private Vector3 targetPoint;
    private float lookOffset;
    private bool isFalling;

    void Start()
    {
        targetPoint = transform.position;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                target = playerObj.transform;
                _playerLogic = playerObj.GetComponent<PlayerMovement>();
                targetPoint = new Vector3(target.position.x, target.position.y, -10f);
            }
            else return; 
        }

        if (_playerLogic == null && target != null)
        {
            _playerLogic = target.GetComponent<PlayerMovement>();
        }

        bool isGrounded = _playerLogic != null && _playerLogic.LastOnGroundTime > 0;

        if (isGrounded)
        {
            targetPoint.y = target.position.y;
        }

        if (transform.position.y - target.position.y > maxVertOffset)
        {
            isFalling = true;
        }

        if (isFalling)
        {
            targetPoint.y = target.position.y;
            if (isGrounded) isFalling = false;
        }

        float xVelocity = (_playerLogic != null) ? _playerLogic.RB.velocity.x : 0f;

        if (xVelocity > 0.1f)
            lookOffset = Mathf.Lerp(lookOffset, lookAheadDistance, lookAheadSpeed * Time.deltaTime);
        else if (xVelocity < -0.1f)
            lookOffset = Mathf.Lerp(lookOffset, -lookAheadDistance, lookAheadSpeed * Time.deltaTime);

        targetPoint.x = target.position.x + lookOffset;
        targetPoint.z = -10f; 

        if (enableBounds)
        {
            targetPoint.x = Mathf.Clamp(targetPoint.x, minBounds.x, maxBounds.x);
            targetPoint.y = Mathf.Clamp(targetPoint.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
    }
    void OnDrawGizmos()
    {
        if (enableBounds)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
            Gizmos.DrawWireCube(center, size);
        }
    }
=======
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    private PlayerMovement _playerLogic; 

    [Header("Movement Settings")]
    public float moveSpeed = 3f; 
    public float lookAheadDistance = 4f; 
    public float lookAheadSpeed = 3f; 
    public float maxVertOffset = 3f;

    [Header("Level Boundaries")]
    public bool enableBounds = true;
    public Vector2 minBounds = new Vector2(-10, -10);
    public Vector2 maxBounds = new Vector2(100, 20);

    private Vector3 targetPoint;
    private float lookOffset;
    private bool isFalling;

    void Start()
    {
        targetPoint = transform.position;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                target = playerObj.transform;
                _playerLogic = playerObj.GetComponent<PlayerMovement>();
                targetPoint = new Vector3(target.position.x, target.position.y, -10f);
            }
            else return; 
        }

        if (_playerLogic == null && target != null)
        {
            _playerLogic = target.GetComponent<PlayerMovement>();
        }

        bool isGrounded = _playerLogic != null && _playerLogic.LastOnGroundTime > 0;

        if (isGrounded)
        {
            targetPoint.y = target.position.y;
        }

        if (transform.position.y - target.position.y > maxVertOffset)
        {
            isFalling = true;
        }

        if (isFalling)
        {
            targetPoint.y = target.position.y;
            if (isGrounded) isFalling = false;
        }

        float xVelocity = (_playerLogic != null) ? _playerLogic.RB.velocity.x : 0f;

        if (xVelocity > 0.1f)
            lookOffset = Mathf.Lerp(lookOffset, lookAheadDistance, lookAheadSpeed * Time.deltaTime);
        else if (xVelocity < -0.1f)
            lookOffset = Mathf.Lerp(lookOffset, -lookAheadDistance, lookAheadSpeed * Time.deltaTime);

        targetPoint.x = target.position.x + lookOffset;
        targetPoint.z = -10f; 

        if (enableBounds)
        {
            targetPoint.x = Mathf.Clamp(targetPoint.x, minBounds.x, maxBounds.x);
            targetPoint.y = Mathf.Clamp(targetPoint.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
    }
    void OnDrawGizmos()
    {
        if (enableBounds)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
            Gizmos.DrawWireCube(center, size);
        }
    }
>>>>>>> upstream/dev
}