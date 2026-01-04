<<<<<<< HEAD
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PlayerData Data;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public Collider2D MainCollider { get; private set; } 
    private Health _health; // Reference to Health for I-Frames
    #endregion

    #region STATE PARAMETERS
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsClimbing { get; private set; } 

    //Timers
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;
    private int _jumpsLeft; 

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    //Wall Slide Internal
    private float _wallStickTimer; 

    //Dash
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;
    
    //Climbing Internal
    private bool _canClimb;
    private float _originalGravity; 
    
    // One Way Platform
    private GameObject _currentOneWayPlatform;
    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    [Header("Climbing Settings")]
    public float climbSpeed = 5f;
    [Tooltip("Layer Mask for objects that can be climbed (Must have IsTrigger = True).")]
    public LayerMask climbableLayer;
    [Tooltip("Layer Mask for solid objects that should stop vertical movement (Ground/Walls).")]
    public LayerMask obstacleLayer; 

    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    
    #region LAYERS & TAGS
    [Header("GroundCheck Layers")]
    [SerializeField] private LayerMask _groundLayer;
    
    [Header("WallJump Layers")]
    [Tooltip("Layer Mask for layers to wallJump from (")]
    [SerializeField] private LayerMask wallJumpLayer; // Layer for wall jump detection

    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        MainCollider = GetComponent<Collider2D>(); 
        _health = GetComponent<Health>(); // Get Health Component
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        _jumpsLeft = Data.jumpAmount; 
    }

    private void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0 && !IsClimbing) 
            CheckDirectionToFace(_moveInput.x > 0);

        // JUMP (Updated to use GameKeys)
        if (Input.GetKeyDown(GameKeys.Jump))
        {
            if (IsClimbing)
            {
                StopClimbing();
                RB.velocity = Vector2.up * 5f; 
            }
            else
            {
                OnJumpInput();
            }
        }

        // JUMP CUT (Updated to use GameKeys)
        if (Input.GetKeyUp(GameKeys.Jump))
        {
            OnJumpUpInput();
        }

        // DASH (Updated to use GameKeys)
        if (Input.GetKeyDown(GameKeys.Dash))
        {
            OnDashInput();
        }
        
        // CLIMB
        if (_canClimb && Mathf.Abs(_moveInput.y) > 0.1f && !IsClimbing)
        {
            StartClimbing();
        }
        
        // ONE WAY PLATFORM DROP (S / Down Arrow)
        if (_moveInput.y < -0.1f && _currentOneWayPlatform != null)
        {
            StartCoroutine(DisableCollision());
        }
        
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping && !IsClimbing)
        {
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
            {
                LastOnGroundTime = Data.coyoteTime;
                _jumpsLeft = Data.jumpAmount; 
            }

            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, wallJumpLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, wallJumpLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, wallJumpLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, wallJumpLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        if (IsClimbing) return; 

        if (IsJumping && RB.velocity.y < 0)
        {
            IsJumping = false;
            _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;
            _isJumpFalling = false;
        }

        if (!IsDashing)
        {
            // PRIORITY 1: Wall Jump 
            if (CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                
                _jumpsLeft = Data.jumpAmount - 1; 

                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                WallJump(_lastWallJumpDir);
            }
            // PRIORITY 2: Normal/Double Jump
            else if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();
            }
        }
        #endregion

        #region DASH CHECKS
        if (IsClimbing) return; 

        if (CanDash() && LastPressedDashTime > 0)
        {
            Sleep(Data.dashSleepTime);

            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;

            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region SLIDE CHECKS
        bool isOnLeftWall = LastOnWallLeftTime > 0;
        bool isOnRightWall = LastOnWallRightTime > 0;
        
        bool isPushingLeft = isOnLeftWall && _moveInput.x < 0;
        bool isPushingRight = isOnRightWall && _moveInput.x > 0;
        bool isPushingAgainstWall = isPushingLeft || isPushingRight;

        bool isPullingFromLeft = isOnLeftWall && _moveInput.x > 0;
        bool isPullingFromRight = isOnRightWall && _moveInput.x < 0;
        bool isPullingAway = isPullingFromLeft || isPullingFromRight;

        if (CanSlide())
        {
            if (IsSliding)
            {
                if (isPullingAway) IsSliding = false;
            }
            else
            {
                if (isPushingAgainstWall)
                {
                    IsSliding = true;
                    _wallStickTimer = Data.wallStickTime; 
                }
            }
        }
        else
        {
            IsSliding = false;
        }
        #endregion

        #region GRAVITY
        if (IsClimbing)
        {
            SetGravityScale(0); 
        }
        else if (!_isDashAttacking)
        {
            if (IsSliding)
            {
                SetGravityScale(0);
            }
            else if (RB.velocity.y < 0 && _moveInput.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (_isJumpCut)
            {
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
            {
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.velocity.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else
            {
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            SetGravityScale(0);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (IsClimbing)
        {
            float xVel = _moveInput.x * climbSpeed;
            float yVel = _moveInput.y * climbSpeed;

            if (MainCollider != null)
            {
                float skinWidth = 0.05f;
                if (yVel != 0)
                {
                    RaycastHit2D hitY = Physics2D.BoxCast(transform.position, MainCollider.bounds.size * 0.9f, 0, new Vector2(0, Mathf.Sign(yVel)), skinWidth + Mathf.Abs(yVel * Time.fixedDeltaTime), obstacleLayer);
                    if (hitY) yVel = 0;
                }
                if (xVel != 0)
                {
                    RaycastHit2D hitX = Physics2D.BoxCast(transform.position, MainCollider.bounds.size * 0.9f, 0, new Vector2(Mathf.Sign(xVel), 0), skinWidth + Mathf.Abs(xVel * Time.fixedDeltaTime), obstacleLayer);
                    if (hitX) xVel = 0;
                }
            }

            RB.velocity = new Vector2(xVel, yVel);
            return; 
        }

        if (!IsDashing)
        {
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        if (IsSliding)
            Slide();
    }

    #region CLIMBING METHODS
    void StartClimbing()
    {
        IsClimbing = true;
        IsJumping = false;
        IsWallJumping = false;
        _isJumpCut = false; 
        
        RB.isKinematic = true; 
        RB.velocity = Vector2.zero;
    }

    public void StopClimbing()
    {
        IsClimbing = false;
        RB.isKinematic = false; 
        SetGravityScale(Data.gravityScale);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & climbableLayer) != 0)
        {
            _canClimb = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & climbableLayer) != 0)
        {
            _canClimb = false;
            if (IsClimbing) StopClimbing();
        }
    }
    #endregion

    #region INPUT CALLBACKS
    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput()
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    //MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        if (LastOnGroundTime > 0)
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        }
        else
        {
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                accelRate = Data.runAccelAmount * Data.accelInAir;
            }
            else
            {
                accelRate = 0; 
            }
        }
        #endregion

        #region Add Bonus Jump Apex Acceleration
        if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Conserve Momentum
        if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }
        #endregion

        float speedDif = targetSpeed - RB.velocity.x;
        float movement = speedDif * accelRate;

        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        bool isAirJump = LastOnGroundTime <= 0;

        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        
        _jumpsLeft--; 

        #region Perform Jump
        float force = Data.jumpForce;

        if (isAirJump)
        {
            force *= Data.doubleJumpForceMult;
        }
        
        if (RB.velocity.y < 0 || RB.velocity.y > 0)
            RB.velocity = new Vector2(RB.velocity.x, 0);

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region Perform Wall Jump
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir;

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0)
            force.y -= RB.velocity.y;

        RB.AddForce(force, ForceMode2D.Impulse);
        #endregion

        // Trigger Wall Jump I-Frames
        StartCoroutine(WallJumpInvincibility());
    }

    private IEnumerator WallJumpInvincibility()
    {
        if (_health != null) _health.SetWallJumpInvincibility(true);
        yield return new WaitForSeconds(0.2f); 
        if (_health != null) _health.SetWallJumpInvincibility(false);
    }
    #endregion

    #region DASH METHODS
    private IEnumerator StartDash(Vector2 dir)
    {
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);
        
        if(_health != null) _health.SetDashInvincibility(true);

        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }

        startTime = Time.time;

        _isDashAttacking = false;

        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;
        
        if(_health != null) _health.SetDashInvincibility(false);

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        IsDashing = false;
    }

    private IEnumerator RefillDash(int amount)
    {
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
    {
        if (_wallStickTimer > 0)
        {
            _wallStickTimer -= Time.fixedDeltaTime;
            RB.velocity = Vector2.zero;
            return;
        }

        float targetSpeed = -Data.slideSpeed; 
        
        float speedDif = targetSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    
    // ONE WAY PLATFORM LOGIC
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            _currentOneWayPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == _currentOneWayPlatform)
        {
            _currentOneWayPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        Collider2D platformCollider = _currentOneWayPlatform.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(MainCollider, platformCollider);
        yield return new WaitForSeconds(0.5f); // Pass through time
        Physics2D.IgnoreCollision(MainCollider, platformCollider, false);
    }
    #endregion


    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 || _jumpsLeft > 0;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        }
        if (_frontWallCheckPoint != null && _backWallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
            Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
        }
    }
    #endregion
=======
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PlayerData Data;

    [Header("Animation")]
    public Animator animator;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public Collider2D MainCollider { get; private set; }
    private Health _health;
    #endregion

    #region STATE PARAMETERS
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsClimbing { get; private set; }
    
    // --- NEW: Lock State for Cutscenes ---
    public bool IsInputLocked { get; private set; }
    // -------------------------------------

    private Collider2D _currentLadder;
    
    public bool IsLunging { get; set; } 

    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    private bool _isJumpCut;
    private bool _isJumpFalling;
    private int _jumpsLeft;

    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    private float _wallStickTimer;

    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;

    private bool _canClimb;
    
    private GameObject _currentOneWayPlatform;
    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;
    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    [Header("Climbing Settings")]
    public float climbSpeed = 5f;
    [Tooltip("Layer Mask for objects that can be climbed (Must have IsTrigger = True).")]
    public LayerMask climbableLayer;
    [Tooltip("Layer Mask for solid objects that should stop vertical movement (Ground/Walls).")]
    public LayerMask obstacleLayer;

    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS
    [Header("Layers")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask wallJumpLayer;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        MainCollider = GetComponent<Collider2D>();
        _health = GetComponent<Health>();
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        _jumpsLeft = Data.jumpAmount;
    }

    private void OnDisable()
    {
        SetGravityScale(Data.gravityScale);
    }

    private void Update()
    {
        // --- NEW: Stop inputs if locked ---
        if (IsInputLocked)
        {
            RB.velocity = Vector2.zero; // Stop sliding immediately
            if (animator != null) animator.SetBool("isMoving", false); // Stop running anim
            return; 
        }
        // ----------------------------------

        float dt = Time.deltaTime;

        #region TIMERS
        LastOnGroundTime -= dt;
        LastOnWallTime -= dt;
        LastOnWallRightTime -= dt;
        LastOnWallLeftTime -= dt;
        LastPressedJumpTime -= dt;
        LastPressedDashTime -= dt;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0 && !IsClimbing)
            CheckDirectionToFace(_moveInput.x > 0);

        if (Input.GetKeyDown(GameKeys.Jump))
        {
            if (IsClimbing)
            {
                StopClimbing();
                RB.velocity = Vector2.up * 5f;
            }
            else
            {
                OnJumpInput();
            }
        }

        if (Input.GetKeyUp(GameKeys.Jump)) OnJumpUpInput();

        if (Input.GetKeyDown(GameKeys.Dash)) OnDashInput();

        if (_canClimb && Mathf.Abs(_moveInput.y) > 0.1f && !IsClimbing)
            StartClimbing();

        if (_moveInput.y < -0.1f && _currentOneWayPlatform != null)
            StartCoroutine(DisableCollision());
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping && !IsClimbing)
        {
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
            {
                LastOnGroundTime = Data.coyoteTime;
                _jumpsLeft = Data.jumpAmount;
            }

            bool frontWall = CheckWall(_frontWallCheckPoint);
            bool backWall = CheckWall(_backWallCheckPoint);

            bool rightWall = (frontWall && IsFacingRight) || (backWall && !IsFacingRight);
            bool leftWall = (frontWall && !IsFacingRight) || (backWall && IsFacingRight);

            if (rightWall && !IsWallJumping) LastOnWallRightTime = Data.coyoteTime;
            if (leftWall && !IsWallJumping) LastOnWallLeftTime = Data.coyoteTime;

            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        if (!IsClimbing)
        {
            if (IsJumping && RB.velocity.y < 0)
            {
                IsJumping = false;
                _isJumpFalling = true;
            }

            if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
                IsWallJumping = false;

            if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
            {
                _isJumpCut = false;
                _isJumpFalling = false;
            }

            if (!IsDashing)
            {
                if (CanWallJump() && LastPressedJumpTime > 0)
                {
                    IsWallJumping = true;
                    IsJumping = false;
                    _isJumpCut = false;
                    _isJumpFalling = false;
                    _jumpsLeft = Data.jumpAmount - 1;
                    _wallJumpStartTime = Time.time;
                    _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                    WallJump(_lastWallJumpDir);
                }
                else if (CanJump() && LastPressedJumpTime > 0)
                {
                    IsJumping = true;
                    IsWallJumping = false;
                    _isJumpCut = false;
                    _isJumpFalling = false;
                    Jump();
                }
            }
        }
        #endregion

        #region DASH CHECKS
        if (!IsClimbing && CanDash() && LastPressedDashTime > 0)
        {
            Sleep(Data.dashSleepTime);
            _lastDashDir = (_moveInput != Vector2.zero) ? _moveInput : (IsFacingRight ? Vector2.right : Vector2.left);
            
            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region SLIDE CHECKS
        if (CanSlide())
        {
             bool isPushingAgainst = (LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0);
             bool isPullingAway = (LastOnWallLeftTime > 0 && _moveInput.x > 0) || (LastOnWallRightTime > 0 && _moveInput.x < 0);

            if (IsSliding)
            {
                if (isPullingAway) IsSliding = false;
            }
            else if (isPushingAgainst)
            {
                IsSliding = true;
                _wallStickTimer = Data.wallStickTime;
            }
        }
        else
        {
            IsSliding = false;
        }
        #endregion

        #region GRAVITY (OPTIMIZED)
        float gravityMult = 1f;
        float yVelLimit = float.MinValue;

        if (IsClimbing || IsSliding || _isDashAttacking)
        {
            gravityMult = 0;
        }
        else if (RB.velocity.y < 0 && _moveInput.y < 0)
        {
            gravityMult = Data.fastFallGravityMult;
            yVelLimit = -Data.maxFastFallSpeed;
        }
        else if (_isJumpCut)
        {
            gravityMult = Data.jumpCutGravityMult;
            yVelLimit = -Data.maxFallSpeed;
        }
        else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
        {
            gravityMult = Data.jumpHangGravityMult;
        }
        else if (RB.velocity.y < 0)
        {
            gravityMult = Data.fallGravityMult;
            yVelLimit = -Data.maxFallSpeed;
        }

        SetGravityScale(Data.gravityScale * gravityMult);

        if (yVelLimit != float.MinValue)
        {
            RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, yVelLimit));
        }
        #endregion

        #region ANIMATION
        if (animator != null)
        {
            animator.SetBool("isMoving", Mathf.Abs(_moveInput.x) > 0.01f && LastOnGroundTime > 0);
            animator.SetBool("isJumping", IsJumping || LastOnGroundTime <= 0);
            animator.SetBool("isGrounded", LastOnGroundTime > 0);
            animator.SetBool("isDashing", IsDashing);
            animator.SetBool("isWallSliding", IsSliding);
            animator.SetBool("isClimbing", IsClimbing);

            animator.SetFloat("VerticalSpeed", RB.velocity.y);

            if (IsClimbing)
            {
                animator.speed = Mathf.Abs(_moveInput.y) > 0.01f ? 1f : 0f;
            }
            else
            {
                animator.speed = 1f;
            }
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (IsLunging) return;

        // --- NEW: Safety check for physics ---
        if (IsInputLocked) return; 
        // -------------------------------------

        if (IsClimbing)
        {
            float xVel = _moveInput.x * climbSpeed;
            float yVel = _moveInput.y * climbSpeed;

            if (MainCollider != null)
            {
                Vector2 origin = MainCollider.bounds.center;
                float skinWidth = 0.05f;

                if (yVel != 0)
                {
                    RaycastHit2D hitY = Physics2D.BoxCast(origin, MainCollider.bounds.size * 0.9f, 0, new Vector2(0, Mathf.Sign(yVel)), skinWidth + Mathf.Abs(yVel * Time.fixedDeltaTime), obstacleLayer);
                    if (hitY) yVel = 0;
                }
                if (xVel != 0)
                {
                    RaycastHit2D hitX = Physics2D.BoxCast(origin, MainCollider.bounds.size * 0.9f, 0, new Vector2(Mathf.Sign(xVel), 0), skinWidth + Mathf.Abs(xVel * Time.fixedDeltaTime), obstacleLayer);
                    if (hitX) xVel = 0;
                }
            }

            RB.velocity = new Vector2(xVel, yVel);
            return;
        }

        if (!IsDashing)
        {
            if (IsWallJumping) Run(Data.wallJumpRunLerp);
            else Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        if (IsSliding) Slide();
    }

    #region CLIMBING METHODS
    void StartClimbing()
    {
        IsClimbing = true;
        IsJumping = false;
        IsWallJumping = false;
        _isJumpCut = false;
        
        RB.isKinematic = true;
        RB.velocity = Vector2.zero;
        
        if (_currentLadder != null)
        {
            float ladderCenterX = _currentLadder.bounds.center.x;
            transform.position = new Vector3(ladderCenterX, transform.position.y, transform.position.z);
        }
    }

    public void StopClimbing()
    {
        IsClimbing = false;
        RB.isKinematic = false;
        SetGravityScale(Data.gravityScale);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & climbableLayer) != 0)
        {
            _canClimb = true;
            _currentLadder = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & climbableLayer) != 0)
        {
            _canClimb = false;
            _currentLadder = null;
            if (IsClimbing) StopClimbing();
        }
    }
    #endregion

    #region INPUT CALLBACKS
    public void OnJumpInput() => LastPressedJumpTime = Data.jumpInputBufferTime;
    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut()) _isJumpCut = true;
    }
    public void OnDashInput() => LastPressedDashTime = Data.dashInputBufferTime;
    #endregion

    #region GENERAL METHODS
    public void SetInputLock(bool isLocked)
    {
        IsInputLocked = isLocked;

        if (isLocked)
        {
            RB.velocity = Vector2.zero;
            _moveInput = Vector2.zero;
            
            if (animator != null) 
            {
                animator.SetBool("isMoving", false);
                animator.SetBool("isJumping", false);
                animator.SetBool("isDashing", false);
                animator.SetBool("isWallSliding", false);
                animator.SetBool("isClimbing", false);
            }

            if (_health != null) _health.SetCutsceneInvincibility(true);
        }
        else
        {
            if (_health != null) _health.SetCutsceneInvincibility(false);
        }
    }

    public void SetGravityScale(float scale) => RB.gravityScale = scale;

    private void Sleep(float duration) => StartCoroutine(nameof(PerformSleep), duration);

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    private bool CheckWall(Transform point)
    {
        return point != null && Physics2D.OverlapBox(point.position, _wallCheckSize, 0, wallJumpLayer);
    }
    #endregion

    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

        float accelRate;

        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : 0;

        if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }

        if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - RB.velocity.x;
        float movement = speedDif * accelRate;

        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        IsFacingRight = !IsFacingRight;
    }

    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight) Turn();
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        bool isAirJump = LastOnGroundTime <= 0;
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        _jumpsLeft--;

        float force = Data.jumpForce;
        if (isAirJump)
        {
            force *= Data.doubleJumpForceMult;
            
            if(animator != null) animator.SetTrigger("DoubleJump");
        }

        RB.velocity = new Vector2(RB.velocity.x, 0);
        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void WallJump(int dir)
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        Vector2 force = new Vector2(Data.wallJumpForce.x * dir, Data.wallJumpForce.y);

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0)
            force.y -= RB.velocity.y;

        RB.AddForce(force, ForceMode2D.Impulse);
        StartCoroutine(WallJumpInvincibility());
    }

    private IEnumerator WallJumpInvincibility()
    {
        if (_health != null) _health.SetWallJumpInvincibility(true);
        yield return new WaitForSeconds(0.2f);
        if (_health != null) _health.SetWallJumpInvincibility(false);
    }
    #endregion

    #region DASH METHODS
    private IEnumerator StartDash(Vector2 dir)
    {
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;
        float startTime = Time.time;
        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);
        if (_health != null) _health.SetDashInvincibility(true);

        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }

        startTime = Time.time;
        _isDashAttacking = false;

        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;
        if (_health != null) _health.SetDashInvincibility(false);

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        IsDashing = false;
    }

    private IEnumerator RefillDash(int amount)
    {
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region SLIDE & PLATFORM METHODS
    private void Slide()
    {
        if (_wallStickTimer > 0)
        {
            _wallStickTimer -= Time.fixedDeltaTime;
            RB.velocity = Vector2.zero;
            return;
        }

        float targetSpeed = -Data.slideSpeed;
        float speedDif = targetSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
            _currentOneWayPlatform = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == _currentOneWayPlatform)
            _currentOneWayPlatform = null;
    }

    private IEnumerator DisableCollision()
    {
        Collider2D platformCollider = _currentOneWayPlatform.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(MainCollider, platformCollider);

        yield return new WaitUntil(() => MainCollider.bounds.max.y < platformCollider.bounds.min.y);
        
        Physics2D.IgnoreCollision(MainCollider, platformCollider, false);
    }
    #endregion

    #region CHECKS
    public bool IsGrounded() => LastOnGroundTime > 0;

    private bool CanJump() => LastOnGroundTime > 0 || _jumpsLeft > 0;

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && 
               (!IsWallJumping || (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut() => IsJumping && RB.velocity.y > 0;
    private bool CanWallJumpCut() => IsWallJumping && RB.velocity.y > 0;

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
            StartCoroutine(nameof(RefillDash), 1);
        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        return LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0;
    }
    #endregion

    #region EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        }
        if (_frontWallCheckPoint != null && _backWallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
            Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
        }
    }
    #endregion
>>>>>>> upstream/dev
}