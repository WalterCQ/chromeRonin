using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class LightningStrike : MonoBehaviour
{
    [Header("Combat Settings")]
    public int damageAmount = 1;
    [Tooltip("The size of the hitbox. Y should match your sprite's length.")]
    public Vector2 hitBoxSize = new Vector2(1.5f, 18f); 
    public LayerMask targetLayer; 

    [Header("Timing")]
    [Tooltip("How long the hitbox stays active.")]
    public float damageDuration = 0.2f;
    [Tooltip("Time before the object is destroyed.")]
    public float autoDestroyTime = 1.0f;

    private SpriteRenderer _sr;
    private Animator _anim;
    private BoxCollider2D _col;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _col = GetComponent<BoxCollider2D>();

        // Disable hitbox initially to prevent accidental damage on spawn frame
        if (_col) _col.enabled = false;
    }

    void Start()
    {
        StartCoroutine(StrikeRoutine());
    }

    IEnumerator StrikeRoutine()
    {
        // NO WARNINGS HERE. The Boss already did the warning.
        // We strike immediately.

        _sr.enabled = true;
        if (_anim) _anim.enabled = true; // Play animation immediately
        if (_col) _col.enabled = true;   // Enable hitbox
        
        // Deal instant damage
        PerformDamageCheck();

        yield return new WaitForSeconds(damageDuration);

        if (_col) _col.enabled = false;

        yield return new WaitForSeconds(autoDestroyTime - damageDuration);
        Destroy(gameObject);
    }

    void PerformDamageCheck()
    {
        // Calculate the center point for the box
        // Assuming the object is rotated 180 to face down, "Down" in local space is actually "Up" in world space?
        // To be safe, we use -transform.up which respects the object's rotation.
        Vector2 center = (Vector2)transform.position + ((Vector2)(-transform.up) * (hitBoxSize.y / 2f));
        
        // Debug draw
        Debug.DrawRay(transform.position, -transform.up * hitBoxSize.y, Color.red, 1.0f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitBoxSize, transform.eulerAngles.z, targetLayer);

        foreach (Collider2D hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damageAmount);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Visualize hitbox relative to rotation
        Vector2 direction = -transform.up; 
        Vector2 center = (Vector2)transform.position + (direction * (hitBoxSize.y / 2f));
        
        // Save current matrix to draw rotated gizmo
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
        Gizmos.matrix = Matrix4x4.identity; // Reset
    }
}