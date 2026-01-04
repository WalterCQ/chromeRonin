<<<<<<< HEAD
using UnityEngine;
using System.Collections;
using Cinemachine; // NEW

public class PlayerCombat : MonoBehaviour
{
    [Header("Aiming Settings")]
    public Transform weaponPivot; 
    public Transform attackPoint; 
    public float attackRange = 0.5f;
    public LayerMask enemyLayers; 

    [Header("Parry Settings")]
    public GameObject parryShield; 
    public float parryDuration = 0.5f;
    public float parryCooldown = 0.5f;
    private float _nextParryTime = 0f;
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    
        if (!weaponPivot) weaponPivot = transform; 
        if (!attackPoint) attackPoint = transform; 
        if (parryShield) parryShield.SetActive(false);
    }

    void Update()
    {
        HandleAiming();

        if (Input.GetMouseButtonDown(0)) 
        {
            PerformAttack();
        }

        if (Input.GetMouseButtonDown(1) && Time.time >= _nextParryTime)
        {
            StartCoroutine(PerformParry());
        }
    }

    void HandleAiming()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused) return;

        Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDirection = (mousePos - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        if (transform.localScale.x < 0) angle = 180 - angle;
        
        weaponPivot.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void PerformAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.TakeDamage(1);
            }
        }
    }

    IEnumerator PerformParry()
    {
        if (parryShield) parryShield.SetActive(true);
        yield return new WaitForSeconds(parryDuration);
        if (parryShield) parryShield.SetActive(false);

        _nextParryTime = Time.time + parryCooldown;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint) 
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
=======
using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement moveScript;
    public Animator animator;

    [Header("Visuals")]
    public Transform modelTransform; 

    [Header("Attack Feel")]
    public float maxLungeDistance = 5f; 
    public float lungeDuration = 0.2f;
    public float hitStopDuration = 0.05f; 
    public float attackCooldown = 0.4f; 
    private float _nextAttackTime = 0f;

    [Header("Air Combo Settings")]
    public float weakLungeDistance = 2f; 
    public float airLungeDecay = 0.7f; 
    
    private bool _hasUsedPowerLunge = false;
    private int _airComboCount = 0;

    [Header("Aiming Settings")]
    public Transform weaponPivot; 
    public Transform attackPoint; 
    public float attackRange = 0.5f;
    public LayerMask enemyLayers; 
    public LayerMask obstacleLayers; 

    [Header("Parry Settings")]
    public GameObject parryShield; 
    public float parryDuration = 0.5f;
    public float parryCooldown = 0.5f;
    private float _nextParryTime = 0f;
    
    private Camera _cam;

    private bool _wasPaused = false;
    private float _inputBlockTime = 0f;

    void Start()
    {
        _cam = Camera.main;
        if (moveScript == null) moveScript = GetComponent<PlayerMovement>();
        
        if (modelTransform == null)
        {
            Transform visualsChild = transform.Find("Visuals");
            if (visualsChild != null) modelTransform = visualsChild;
        }

        if (!weaponPivot) weaponPivot = transform; 
        if (!attackPoint) attackPoint = transform; 
        if (parryShield) parryShield.SetActive(false);
    }

    void Update()
    {
        // 1. Pause Logic
        if (Time.timeScale == 0)
        {
            _wasPaused = true;
            return;
        }

        if (_wasPaused)
        {
            _inputBlockTime = Time.unscaledTime + 0.15f;
            _wasPaused = false;
        }

        if (Time.unscaledTime < _inputBlockTime) return;

        // --- NEW CODE START ---
        // 2. Cutscene / Input Lock Check
        // If the movement script is locked (by camera preview), STOP all combat.
        if (moveScript != null && moveScript.IsInputLocked) return; 
        // --- NEW CODE END ---

        if (moveScript.IsClimbing) return; 

        if (moveScript.IsGrounded() && !moveScript.IsLunging)
        {
            _hasUsedPowerLunge = false;
            _airComboCount = 0;
        }

        HandleAiming();

        if (Input.GetKeyDown(GameKeys.Attack) && Time.time >= _nextAttackTime && (moveScript == null || !moveScript.IsLunging)) 
        {
            StartCoroutine(PerformAttackLunge());
            _nextAttackTime = Time.time + attackCooldown;
        }

        if (Input.GetKeyDown(GameKeys.Parry) && Time.time >= _nextParryTime)
        {
            StartCoroutine(PerformParry());
        }
    }

    void HandleAiming()
    {
        if (Time.timeScale == 0) return; 

        Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDirection = (mousePos - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        if (transform.localScale.x < 0) angle = 180 - angle;
        
        weaponPivot.localRotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator PerformAttackLunge()
    {
        Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 startPos = moveScript.RB.position;
        Vector2 rawDirection = (Vector2)mousePos - startPos;

        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;

        if (modelTransform != null)
        {
            if (transform.localScale.x > 0) modelTransform.rotation = Quaternion.Euler(0, 0, angle);
            else modelTransform.rotation = Quaternion.Euler(0, 0, angle - 180);
        }

        weaponPivot.localRotation = Quaternion.identity;

        float limitForThisAttack;

        if (moveScript.IsGrounded())
        {
            limitForThisAttack = maxLungeDistance;
            if (rawDirection.y > 0.1f)
            {
                _hasUsedPowerLunge = true;
                _airComboCount = 1; 
            }
        }
        else
        {
            if (!_hasUsedPowerLunge)
            {
                limitForThisAttack = maxLungeDistance;
                _hasUsedPowerLunge = true;
                _airComboCount = 1;
            }
            else
            {
                float decayFactor = Mathf.Pow(airLungeDecay, _airComboCount);
                limitForThisAttack = weakLungeDistance * decayFactor;
                _airComboCount++;
            }
        }

        Vector2 clampedOffset = Vector2.ClampMagnitude(rawDirection, limitForThisAttack);
        Vector2 targetPos = startPos + clampedOffset;

        if (moveScript != null)
        {
            moveScript.CheckDirectionToFace(clampedOffset.x > 0);
            moveScript.IsLunging = true;
            moveScript.RB.velocity = Vector2.zero; 
            float originalGravity = moveScript.Data.gravityScale;
            moveScript.SetGravityScale(0);
        
            if (animator != null) animator.SetTrigger("AttackTrigger"); 

            float elapsedTime = 0f;
            bool hasHitEnemy = false; 

            while (elapsedTime < lungeDuration)
            {
                if (Time.timeScale == 0) 
                {
                    yield return null;
                    continue; 
                }

                float t = elapsedTime / lungeDuration;
                moveScript.RB.MovePosition(Vector2.Lerp(startPos, targetPos, t));

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

                if (hitEnemies.Length > 0)
                {
                    Collider2D closestEnemy = GetClosestEnemy(hitEnemies);
                    Vector2 dirToEnemy = closestEnemy.transform.position - attackPoint.position;
                    float distToEnemy = dirToEnemy.magnitude;

                    RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, dirToEnemy, distToEnemy, obstacleLayers);

                    if (hit.collider == null)
                    {
                        IDamageable damageable = closestEnemy.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(1);
                            Health targetHealth = closestEnemy.GetComponent<Health>();
                            if (targetHealth != null && targetHealth.isEnemy)
                            {
                                if (TimeManager.Instance != null) TimeManager.Instance.TriggerHitStop(hitStopDuration);
                            }
                            hasHitEnemy = true;
                            break; 
                        }
                    }
                }
                elapsedTime += Time.deltaTime;
                yield return null; 
            }

            if (!hasHitEnemy) moveScript.RB.MovePosition(targetPos);
            if (modelTransform != null) modelTransform.localRotation = Quaternion.identity;
            
            moveScript.SetGravityScale(originalGravity);
            moveScript.IsLunging = false; 
        }
    }

    Collider2D GetClosestEnemy(Collider2D[] enemies)
    {
        Collider2D bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPos = attackPoint.position;
        foreach(Collider2D potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    IEnumerator PerformParry()
    {
        if (animator != null) animator.SetBool("isParrying", true);
        if (parryShield) parryShield.SetActive(true);
        yield return new WaitForSeconds(parryDuration);
        if (parryShield) parryShield.SetActive(false);
        if (animator != null) animator.SetBool("isParrying", false);
        _nextParryTime = Time.time + parryCooldown;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
>>>>>>> upstream/dev
}