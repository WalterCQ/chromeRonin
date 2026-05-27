using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Overclock Settings")]
    public float slowdownFactor = 0.25f;
    public float slowdownLength = 2f; 
    
    [Header("Chrono-Bar Resources")]
    public float maxChronoEnergy = 100f;
    public float currentChronoEnergy;
    public float drainRate = 20f; 
    public float refillAmountOnKill = 30f;
    
    [Header("Kill Refill Modifier")]
    [Tooltip("Multiplier for refill amount on kill (0.5 = 50% of refillAmountOnKill)")]
    [Range(0.1f, 2f)]
    public float killRefillMultiplier = 1f;  // Adjust this to reduce refill amount per kill
    
    [Header("Gradual Refill Settings")]
    [Tooltip("Speed at which energy refills after a kill (per second)")]
    public float refillSpeed = 15f;  // Energy per second during gradual refill
    
    private float _pendingRefillAmount = 0f;  // Amount of energy waiting to be gradually added
    private bool _isOverclockActive = false;
    private bool _isHitStopActive = false; 
    private float _fixedDeltaTime; 

    [Header("Auto-Overclock Settings")]
    public float autoSlowdownDuration = 1.5f;
    public bool isAutoSlowEnabled = true; // NEW: Toggleable preference
    private float _autoSlowTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); 
        
        _fixedDeltaTime = Time.fixedDeltaTime;
        ResetEnergy();

        // Load preference (Default to enabled)
        isAutoSlowEnabled = PlayerPrefs.GetInt("AutoSlowPref", 1) == 1;

        GameKeys.LoadKeys(); 
    }

    public void SetAutoSlowPreference(bool enabled)
    {
        isAutoSlowEnabled = enabled; // This line updates the LIVE gameplay behavior
        PlayerPrefs.SetInt("AutoSlowPref", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void TriggerAutoSlow()
    {
        // Check both energy AND player preference
        if (!isAutoSlowEnabled || _autoSlowTimer > 0 || currentChronoEnergy <= 5f) return;
        
        _autoSlowTimer = autoSlowdownDuration;
        currentChronoEnergy -= 10f; 
    }
    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGamePaused || GameManager.Instance.isGameOver))
        {
            return;
        }

        HandleInput();
        HandleGradualRefill();
        HandleTimeScale();
    }

    public void ResetEnergy()
    {
        currentChronoEnergy = maxChronoEnergy;
        _pendingRefillAmount = 0f;
        _isOverclockActive = false;
        _isHitStopActive = false;
    }

    void HandleGradualRefill()
    {
        // Gradually add pending refill energy
        if (_pendingRefillAmount > 0 && currentChronoEnergy < maxChronoEnergy)
        {
            float refillThisFrame = refillSpeed * Time.unscaledDeltaTime;
            refillThisFrame = Mathf.Min(refillThisFrame, _pendingRefillAmount);
            
            currentChronoEnergy += refillThisFrame;
            _pendingRefillAmount -= refillThisFrame;
            
            // Clamp to max
            if (currentChronoEnergy > maxChronoEnergy)
            {
                currentChronoEnergy = maxChronoEnergy;
                _pendingRefillAmount = 0f;
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKey(GameKeys.Overclock))
        {
            if (currentChronoEnergy > 0)
            {
                _isOverclockActive = true;
                currentChronoEnergy -= drainRate * Time.unscaledDeltaTime;
            }
            else
            {
               _isOverclockActive = false;
            }
        }
        else
        {
            _isOverclockActive = false;
        }

        currentChronoEnergy = Mathf.Clamp(currentChronoEnergy, 0, maxChronoEnergy);
    }
    // Update HandleTimeScale to account for the auto-timer
    void HandleTimeScale()
    {
        if (_isHitStopActive)
        {
            Time.timeScale = 0f;
        }
        else if (_isOverclockActive || _autoSlowTimer > 0) // Check the auto timer here
        {
            Time.timeScale = slowdownFactor;
            if (_autoSlowTimer > 0) _autoSlowTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            Time.timeScale = 1f;
        }

        Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
    }

    public void TriggerHitStop(float duration)
    {
        if (_isHitStopActive) return; 
        StartCoroutine(HitStopRoutine(duration));
    }

    IEnumerator HitStopRoutine(float duration)
    {
        _isHitStopActive = true;
        yield return new WaitForSecondsRealtime(duration);
        _isHitStopActive = false;
    }

    public void AddChronoEnergy(float amount)
    {
        // Apply kill refill multiplier to reduce the amount
        float adjustedAmount = amount * killRefillMultiplier;
        // Add to pending refill for gradual increase instead of instant
        _pendingRefillAmount += adjustedAmount;
    }
    
    /// <summary>
    /// Adds energy instantly without gradual refill animation
    /// </summary>
    public void AddChronoEnergyInstant(float amount)
    {
        currentChronoEnergy += amount;
        if (currentChronoEnergy > maxChronoEnergy) currentChronoEnergy = maxChronoEnergy;
    }
}