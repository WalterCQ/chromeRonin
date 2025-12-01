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
}