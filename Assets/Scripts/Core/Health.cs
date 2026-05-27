using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class OneShotAudio : MonoBehaviour
{
    private static GameObject _audioPrefab;

    public static void Play(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        
        if (_audioPrefab == null)
        {
            _audioPrefab = new GameObject("OneShotAudio_Template");
            _audioPrefab.AddComponent<AudioSource>();
            _audioPrefab.AddComponent<OneShotAudio>(); 
            _audioPrefab.SetActive(false); 
            DontDestroyOnLoad(_audioPrefab);
        }

        GameObject audioObj = ObjectPool.Instance.Get(_audioPrefab, position, Quaternion.identity);
        
        AudioSource source = audioObj.GetComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0.5f; 
        source.volume = volume * SFXManager.GetVolume();
        
        if (!source.enabled) source.enabled = true;
        source.Play();

        if(ObjectPool.Instance != null)
            ObjectPool.Instance.Return(audioObj, clip.length + 0.1f);
    }
}

public class Health : MonoBehaviour, IDamageable
{
    [Header("Animation")]
    public Animator animator; 

    [Header("Stats")]
    public int maxHealth = 5;
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    [Header("Identity")]
    public bool isPlayer = false; 
    public bool isEnemy = false;  

    [Header("Death Behavior")]
    [Tooltip("If true, the object is Destroyed. If false, it is Deactivated (hidden).")]
    public bool destroyOnDeath = false; 
    [Tooltip("If true, the object is Deactivated automatically on death. UNCHECK THIS for the Boss so the BossController can play the death animation.")]
    public bool deactivateOnDeath = true; 

    [Header("Invincibility")]
    public float invincibilityDuration = 0.2f; 
    private bool _isInvincible = false;
    private bool _isDashInvincible = false; 
    private bool _isWallJumpInvincible = false;
    private bool _isCutsceneInvincible = false; 

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; 
    public Color hurtColor = new Color(1f, 0.5f, 0.5f, 0.8f); 
    public GameObject hitParticlePrefab;
    public float hitParticleLifetime = 1.0f;
    private Color _originalColor;

    [Header("Death Effects")]
    public ParticleSystem deathParticlePrefab;

    [Header("Sound Effects")]
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    public AudioSource audioSource;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("Debug / Cheats")]
    public bool godMode = false; 

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) _originalColor = spriteRenderer.color;
        if (animator == null) animator = GetComponent<Animator>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

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
    public void SetCutsceneInvincibility(bool state) { _isCutsceneInvincible = state; }

    public void TakeDamage(int damage)
    {
        if (godMode) return; 
        if (_currentHealth <= 0) return;
        if (_isInvincible || _isDashInvincible || _isWallJumpInvincible || _isCutsceneInvincible) return;

        _currentHealth -= damage;
        
        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(_currentHealth);
        }

        if (isPlayer && CameraShakeManager.Instance != null)
        {
             CameraShakeManager.Instance.Shake(8f, 0.3f); 
        }

        if (gameObject.activeInHierarchy) StartCoroutine(FlashRoutine());
        
        if (hitParticlePrefab != null)
        {
            GameObject effect = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            if (hitParticleLifetime > 0) Destroy(effect, hitParticleLifetime);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayHurtSound();
            if (animator != null) animator.SetTrigger("Hit");
            if (invincibilityDuration > 0) StartCoroutine(InvincibilityRoutine());
        }
    }

    void PlayHurtSound()
    {
        if (hurtSounds != null && hurtSounds.Length > 0 && audioSource != null)
        {
            AudioClip randomClip = hurtSounds[Random.Range(0, hurtSounds.Length)];
            if (randomClip != null) SFXManager.PlaySound(audioSource, randomClip, soundVolume);
        }
    }

    void PlayDeathSound()
    {
        if (deathSound != null) OneShotAudio.Play(deathSound, transform.position, soundVolume);
    }

    void Die()
    {
        PlayDeathSound();

        if (isPlayer)
        {
            if (animator != null) animator.SetBool("IsDead", true);
            if (CameraShakeManager.Instance != null) CameraShakeManager.Instance.Shake(10f, 0.5f); 

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

            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
            else StartCoroutine(ReloadSceneDelay());
            return; 
        }
        
        if (isEnemy)
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.AddChronoEnergy(TimeManager.Instance.refillAmountOnKill);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemyKill();
            }

            if (deathParticlePrefab != null)
            {
                Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            }
        }

        if (destroyOnDeath) Destroy(gameObject);
        else if (deactivateOnDeath) gameObject.SetActive(false);
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

    public float GetHealthPercent()
    {
        if (maxHealth == 0) return 0;
        return (float)_currentHealth / maxHealth;
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
}