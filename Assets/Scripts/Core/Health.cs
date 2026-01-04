<<<<<<< HEAD
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class Health : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 5;
    private int _currentHealth;

    [Header("Identity")]
    public bool isPlayer = false; 
    public bool isEnemy = false;  

    [Header("Death Behavior")]
    public bool destroyOnDeath = false; 

    [Header("Invincibility")]
    public float invincibilityDuration = 0.2f; 
    private bool _isInvincible = false;
    private bool _isDashInvincible = false; 
    private bool _isWallJumpInvincible = false;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; 
    public Color hurtColor = new Color(1f, 0.5f, 0.5f, 0.8f); 
    private Color _originalColor;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) _originalColor = spriteRenderer.color;
        
        if (!isPlayer) _currentHealth = maxHealth;
    }

    public void SetHealth(int amount)
    {
        _currentHealth = amount;
    }

    public void SetDashInvincibility(bool state) { _isDashInvincible = state; }
    public void SetWallJumpInvincibility(bool state) { _isWallJumpInvincible = state; }

    public void TakeDamage(int damage)
    {
        if (_isInvincible || _isDashInvincible || _isWallJumpInvincible) return;

        _currentHealth -= damage;
        
        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(_currentHealth);
            CameraShakeManager.Instance.Shake(10f);  
        }

        Debug.Log($"{gameObject.name} took {damage} dmg. HP: {_currentHealth}/{maxHealth}");

        if (gameObject.activeInHierarchy) StartCoroutine(FlashRoutine());
        
        if (_currentHealth <= 0)
        {
            Die();
        }
        else if (invincibilityDuration > 0)
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    void Die()
    {
        if (isPlayer)
        {
            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
            else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            
            gameObject.SetActive(false); 
            return; 
        }
        
        if (isEnemy)
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.AddChronoEnergy(TimeManager.Instance.refillAmountOnKill);

            CameraShakeManager.Instance.Shake(2f);   
        }

        if (destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        _isInvincible = false;
    }

    IEnumerator FlashRoutine()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = _originalColor;
        }
    }
=======
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class Health : MonoBehaviour, IDamageable
{
    [Header("Animation")]
    public Animator animator; 

    [Header("Stats")]
    public int maxHealth = 5;
    private int _currentHealth;

    [Header("Identity")]
    public bool isPlayer = false; 
    public bool isEnemy = false;  

    [Header("Death Behavior")]
    public bool destroyOnDeath = false; 

    [Header("Invincibility")]
    public float invincibilityDuration = 0.2f; 
    private bool _isInvincible = false;
    private bool _isDashInvincible = false; 
    private bool _isWallJumpInvincible = false;
    
    private bool _isCutsceneInvincible = false; 

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; 
    public Color hurtColor = new Color(1f, 0.5f, 0.5f, 0.8f); 
    private Color _originalColor;

    [Header("Debug / Cheats")]
    public bool godMode = false; 

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) _originalColor = spriteRenderer.color;
        
        if (animator == null) animator = GetComponent<Animator>();

        if (!isPlayer) _currentHealth = maxHealth;
    }

    void Update()
    {
        if (isPlayer && Input.GetKeyDown(KeyCode.C))
        {
            godMode = !godMode;
            Debug.Log($"God Mode Toggled: {godMode}");
        }
    }

    public void SetHealth(int amount)
    {
        _currentHealth = amount;
    }

    public void SetDashInvincibility(bool state) { _isDashInvincible = state; }
    public void SetWallJumpInvincibility(bool state) { _isWallJumpInvincible = state; }

    // --- NEW: Public Setter for Camera Script ---
    public void SetCutsceneInvincibility(bool state) { _isCutsceneInvincible = state; }
    // --------------------------------------------

    public void TakeDamage(int damage)
    {
        if (godMode) return; 

        if (_currentHealth <= 0) return;
        
        // --- UPDATED CHECK ---
        if (_isInvincible || _isDashInvincible || _isWallJumpInvincible || _isCutsceneInvincible) return;
        // ---------------------

        _currentHealth -= damage;
        
        Debug.Log($"{gameObject.name} took {damage} dmg. HP: {_currentHealth}/{maxHealth}");

        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(_currentHealth);
        }

        if (isPlayer && CameraShakeManager.Instance != null)
        {
             CameraShakeManager.Instance.Shake(8f, 0.3f); 
        }

        if (gameObject.activeInHierarchy) StartCoroutine(FlashRoutine());
        
        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hit");

            if (invincibilityDuration > 0)
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }
    }

    void Die()
    {
        if (isPlayer && animator != null) 
        {
            animator.SetBool("IsDead", true);
        }

        if (isPlayer && CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.Shake(10f, 0.5f); 
        }

        if (isPlayer)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb) 
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; 
            }
            
            var movement = GetComponent<PlayerMovement>();
            if (movement) movement.enabled = false;
            
            var combat = GetComponent<PlayerCombat>();
            if (combat) combat.enabled = false;

            if (GameManager.Instance != null) 
            {
                GameManager.Instance.TriggerGameOver();
            }
            else 
            {
                StartCoroutine(ReloadSceneDelay());
            }
            return; 
        }
        
        if (isEnemy)
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.AddChronoEnergy(TimeManager.Instance.refillAmountOnKill);
        }

        if (destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    IEnumerator ReloadSceneDelay()
    {
        yield return new WaitForSeconds(1.5f); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        _isInvincible = false;
    }

    IEnumerator FlashRoutine()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = _originalColor;
        }
    }
>>>>>>> upstream/dev
}