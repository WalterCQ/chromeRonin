using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 使平台可以左右移动，支持设置移动范围和速度
/// 适用于 OneWay 平台或其他需要移动的平台
/// 使用速度传递方式，不会影响玩家的移动操作
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动速度")]
    public float moveSpeed = 2f;
    
    [Tooltip("向左移动的距离（相对于起始位置）")]
    public float leftDistance = 3f;
    
    [Tooltip("向右移动的距离（相对于起始位置）")]
    public float rightDistance = 3f;
    
    [Header("行为设置")]
    [Tooltip("是否在到达边界时暂停")]
    public bool pauseAtEdge = false;
    
    [Tooltip("到达边界时暂停的时间（秒）")]
    public float pauseDuration = 0.5f;
    
    [Tooltip("是否从右边开始移动")]
    public bool startMovingRight = true;
    
    [Header("可视化")]
    [Tooltip("在编辑器中显示移动范围")]
    public bool showGizmos = true;
    
    [Tooltip("Gizmo 颜色")]
    public Color gizmoColor = Color.yellow;

    private Vector3 _startPosition;
    private Vector3 _lastPosition;
    private float _leftBoundary;
    private float _rightBoundary;
    private int _direction; // 1 = 右, -1 = 左
    private float _pauseTimer;
    private bool _isPaused;
    
    // 存储站在平台上的对象
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
        // 记录移动前的位置
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
            // 移动平台
            Vector3 newPosition = transform.position;
            newPosition.x += _direction * moveSpeed * Time.fixedDeltaTime;

            // 检查边界
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
            
            // 计算平台速度
            _platformVelocity = (newPosition - oldPosition);
        }
        
        // 移动站在平台上的乘客
        MovePassengers();
        
        _lastPosition = transform.position;
    }

    private void MovePassengers()
    {
        // 清理已经不存在的乘客
        _passengers.RemoveWhere(p => p == null);
        
        foreach (Transform passenger in _passengers)
        {
            if (passenger != null)
            {
                // 直接移动乘客位置，跟随平台
                Rigidbody2D rb = passenger.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 使用 MovePosition 来移动带有 Rigidbody 的物体
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
        // 检查碰撞是否来自上方（玩家站在平台上）
        if (IsPassengerOnTop(collision))
        {
            _passengers.Add(collision.transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 持续检查，确保玩家在平台上
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
    /// 检查碰撞物体是否在平台上方
    /// </summary>
    private bool IsPassengerOnTop(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 如果接触点的法线指向上方，说明物体在平台上面
            if (contact.normal.y < -0.5f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 在编辑器中可视化移动范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 basePos = Application.isPlaying ? _startPosition : transform.position;
        
        Gizmos.color = gizmoColor;
        
        // 绘制左边界
        Vector3 leftPos = basePos;
        leftPos.x -= leftDistance;
        Gizmos.DrawWireSphere(leftPos, 0.3f);
        
        // 绘制右边界
        Vector3 rightPos = basePos;
        rightPos.x += rightDistance;
        Gizmos.DrawWireSphere(rightPos, 0.3f);
        
        // 绘制连接线
        Gizmos.DrawLine(leftPos, rightPos);
        
        // 绘制起始位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(basePos, 0.2f);
    }

    /// <summary>
    /// 设置新的移动范围（运行时调用）
    /// </summary>
    public void SetMoveRange(float left, float right)
    {
        leftDistance = left;
        rightDistance = right;
        _leftBoundary = _startPosition.x - leftDistance;
        _rightBoundary = _startPosition.x + rightDistance;
    }

    /// <summary>
    /// 设置移动速度（运行时调用）
    /// </summary>
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 暂停/恢复平台移动
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
