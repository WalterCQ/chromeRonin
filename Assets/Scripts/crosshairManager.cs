using UnityEngine;

public class CrosshairManager : MonoBehaviour
{
    [Header("References")]
    public PlayerCombat playerCombat;
    public SpriteRenderer spriteRenderer;

    [Header("Settings")]
    public LayerMask enemyLayer; 
    public LayerMask obstacleLayer;
    
    [Header("Colors")]
    public Color normalColor = Color.cyan; 
    public Color validTargetColor = Color.red;    
    public Color outOfRangeColor = Color.gray;    
    public Color blockedColor = Color.gray;

    private Vector3 _baseScale; 

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCombat == null) playerCombat = FindObjectOfType<PlayerCombat>();

        _baseScale = transform.localScale;
        spriteRenderer.color = normalColor;
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; 
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        
        transform.position = new Vector3(worldPos.x, worldPos.y, 0);

        UpdateCrosshairColor(transform.position);
    }

    void UpdateCrosshairColor(Vector3 currentPos)
    {
        if (playerCombat == null) return;

        Collider2D enemyHit = Physics2D.OverlapPoint(currentPos, enemyLayer);

        if (enemyHit != null)
        {
            Vector2 dirToEnemy = enemyHit.transform.position - playerCombat.transform.position;
            float distToPlayer = dirToEnemy.magnitude;

            if (distToPlayer <= playerCombat.maxLungeDistance)
            {
                RaycastHit2D wallHit = Physics2D.Raycast(playerCombat.transform.position, dirToEnemy, distToPlayer, obstacleLayer);

                if (wallHit.collider == null)
                {
                    spriteRenderer.color = validTargetColor;
                    transform.localScale = _baseScale * 1.2f; 
                }
                else
                {
                    spriteRenderer.color = blockedColor;
                    transform.localScale = _baseScale; 
                }
            }
            else
            {
                spriteRenderer.color = outOfRangeColor;
                transform.localScale = _baseScale; 
            }
        }
        else
        {
            spriteRenderer.color = normalColor;
            transform.localScale = _baseScale;
        }
    }
}