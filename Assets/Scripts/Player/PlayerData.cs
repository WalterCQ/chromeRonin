using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] 
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength; 
    [HideInInspector] public float gravityScale; 
    
    [Space(5)]
    public float fallGravityMult = 1.5f;
    public float maxFallSpeed = 25f; 
    [Space(5)]
    public float fastFallGravityMult = 2f; 
    public float maxFastFallSpeed = 30f; 
    
    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed = 10f; 
    public float runAcceleration = 5f; 
    [HideInInspector] public float runAccelAmount; 
    public float runDecceleration = 5f; 
    [HideInInspector] public float runDeccelAmount; 
    [Space(5)]
    [Range(0f, 1)] public float accelInAir = 0.65f; 
    [Range(0f, 1)] public float deccelInAir = 0.65f;
    [Space(5)]
    public bool doConserveMomentum = true;

    [Space(20)]

    [Header("Jump")]
    public int jumpAmount = 2;
    public float jumpHeight = 3.5f; 
    public float jumpTimeToApex = 0.35f; 
    [Range(0f, 1)] public float doubleJumpForceMult = 0.8f;
    [HideInInspector] public float jumpForce; 

    [Header("Both Jumps")]
    public float jumpCutGravityMult = 2f; 
    [Range(0f, 1)] public float jumpHangGravityMult = 0.5f; 
    public float jumpHangTimeThreshold = 0.1f; 
    [Space(0.5f)]
    public float jumpHangAccelerationMult = 1.1f; 
    public float jumpHangMaxSpeedMult = 1.3f;             

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(10f, 20f); 
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp = 0.5f; 
    [Range(0f, 1.5f)] public float wallJumpTime = 0.4f; 
    public bool doTurnOnWallJump = true; 

    [Space(20)]

    [Header("Slide")]
    public float slideSpeed = 10f;
    public float slideAccel = 5f;
    public float wallStickTime = 0.25f;

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime = 0.1f; 
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime = 0.1f; 
    
    [Header("Dash")]
    public int dashAmount = 1;
    public float dashSpeed = 20f;
    public float dashSleepTime = 0.05f;
    public float dashAttackTime = 0.15f;
    public float dashEndTime = 0.15f;
    public Vector2 dashEndSpeed = new Vector2(15f, 15f);
    [Range(0f, 1f)] public float dashEndRunLerp = 0.5f;
    public float dashRefillTime = 0.1f;
    [Range(0.01f, 0.5f)] public float dashInputBufferTime = 0.1f;

    private void OnValidate()
    {
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / Physics2D.gravity.y;

        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}	