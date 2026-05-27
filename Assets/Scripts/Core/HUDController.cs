using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    // ==================== CHRONO BAR ====================
    [Header("Chrono Bar References")]
    public Image chronoBarFill;
    public Image chronoBarBackground;
    public Image chronoBarGlow;
    public Image chronoBarOverlay;         // Optional: scanline/pattern overlay
    public RectTransform chronoBarContainer;
    
    [Header("Chrono Bar Colors")]
    public Gradient chronoGradient;        // Use gradient for smooth color transitions
    public Color activeOverclockColor = new Color(0.6f, 0.2f, 1f, 1f);
    public Color depletedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    
    [Header("Chrono Bar Animation")]
    public float fillLerpSpeed = 10f;
    public float colorLerpSpeed = 8f;
    public float lowEnergyThreshold = 0.3f;
    public float criticalEnergyThreshold = 0.15f;
    public float fillSnapThreshold = 0.002f;  // Threshold to snap to target and stop lerping
    
    [Header("Chrono Bar Easing")]
    [Tooltip("Duration of the smooth easing animation (EaseOutQuart)")]
    public float easeDuration = 0.35f;                         // Optimal duration for responsive feel
    
    [Header("Chrono Pulse Effect")]
    public float pulseSpeed = 5f;
    public float pulseIntensity = 0.25f;
    public float criticalPulseSpeed = 10f;
    public float scaleBounceMagnitude = 0.02f;
    
    [Header("Chrono Shake Effect")]
    public float shakeIntensity = 3f;
    public float shakeDuration = 0.25f;
    
    [Header("Chrono Flash Effect")]
    public float flashDuration = 0.15f;
    public Color flashColor = new Color(1f, 1f, 1f, 0.8f);
    
    [Header("Chrono Glow Effect")]
    public float glowPulseSpeed = 3f;
    public float maxGlowAlpha = 0.6f;
    
    [Header("Chrono Segmented Look (Optional)")]
    public bool useSegmentedBar = false;
    public int segmentCount = 10;
    public float segmentGap = 0.01f;

    // ==================== HEALTH HEARTS ====================
    [Header("Health Hearts")]
    public Transform heartsContainer;       // Parent object for heart icons
    public GameObject heartPrefab;          // Prefab with Image component
    public int maxHearts = 5;
    
    [Header("Heart Sprites")]
    public Sprite fullHeartSprite;          // The heart.png sprite
    
    [Header("Heart Colors")]
    public Color fullHeartColor = Color.white;                       // Normal heart color
    public Color healColor = new Color(0.2f, 1f, 0.4f, 1f);          // Green flash for healing
    
    [Header("Heart Animation")]
    public float heartBeatSpeed = 2f;
    public float heartBeatScale = 1.15f;
    public float lowHealthBeatSpeed = 4f;   // Faster beat when low health
    public float heartPopScale = 1.4f;      // Scale when taking damage/healing
    public float heartAnimDuration = 0.3f;
    
    [Header("Heart Damage Animation (Pop, Darken, Fade)")]
    public float heartDamageScale = 1.8f;         // How big the heart grows when damaged
    public float heartDamageDuration = 0.5f;      // Duration of the damage animation
    public Color heartDarkenColor = new Color(0.2f, 0f, 0f, 1f);  // Dark red color
    
    [Header("Heart Shake")]
    public float heartShakeIntensity = 8f;
    public float heartShakeDuration = 0.2f;
    
    // ==================== COINS ====================
    [Header("Coins")]
    public TextMeshProUGUI coinText;
    public Image coinIcon;
    public float coinPopScale = 1.3f;
    public float coinAnimDuration = 0.2f;

    // ==================== PRIVATE STATE ====================
    // Chrono bar state
    private float _displayedFillAmount;
    private float _previousEnergy;
    private Color _currentBarColor;
    private Vector3 _originalBarPosition;
    private Vector3 _originalBarScale;
    private float _shakeTimer;
    private float _flashTimer;
    private bool _isBarShaking;
    private RectTransform _barRectTransform;
    
    // Easing animation state
    private float _easeStartValue;
    private float _easeTargetValue;
    private float _easeElapsed;
    private bool _isEasing;
    
    // Hearts state
    private List<Image> _heartImages = new List<Image>();
    private List<RectTransform> _heartRects = new List<RectTransform>();
    private int _previousHealth = -1;
    private int _currentDisplayedHealth;
    private Coroutine _heartAnimCoroutine;
    
    // Coins state
    private int _previousCoins = -1;
    private RectTransform _coinIconRect;
    private Vector3 _originalCoinScale;

    void Start()
    {
        InitializeChronoBar();
        InitializeHearts();
        InitializeCoins();
        
        // Set up default gradient if not assigned
        if (chronoGradient == null || chronoGradient.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }

        // EVENT SUBSCRIPTION (Observer Pattern)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerHealthChanged += HandleHealthChanged;
            GameManager.Instance.OnCoinChanged += HandleCoinChanged;
            
            // Init values
            HandleHealthChanged(GameManager.Instance.playerCurrentHealth);
            HandleCoinChanged(GameManager.Instance.coins);
        }
    }

    void OnDestroy()
    {
        // EVENT UNSUBSCRIPTION
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerHealthChanged -= HandleHealthChanged;
            GameManager.Instance.OnCoinChanged -= HandleCoinChanged;
        }
    }
    
    void SetupDefaultGradient()
    {
        chronoGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[4];
        colorKeys[0] = new GradientColorKey(new Color(1f, 0f, 0.2f), 0f);      // Red at 0%
        colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.3f);    // Orange at 30%
        colorKeys[2] = new GradientColorKey(new Color(1f, 0.9f, 0f), 0.6f);    // Yellow at 60%
        colorKeys[3] = new GradientColorKey(new Color(0f, 1f, 1f), 1f);        // Cyan at 100%
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);
        
        chronoGradient.SetKeys(colorKeys, alphaKeys);
    }

    void InitializeChronoBar()
    {
        if (chronoBarFill != null)
        {
            _barRectTransform = chronoBarContainer != null 
                ? chronoBarContainer 
                : chronoBarFill.GetComponent<RectTransform>();
                
            if (_barRectTransform != null)
            {
                _originalBarPosition = _barRectTransform.localPosition;
                _originalBarScale = _barRectTransform.localScale;
            }
            
            _displayedFillAmount = chronoBarFill.fillAmount;
            _currentBarColor = chronoGradient != null ? chronoGradient.Evaluate(1f) : Color.cyan;
        }
        
        if (TimeManager.Instance != null)
            _previousEnergy = TimeManager.Instance.currentChronoEnergy;
    }
    
    void InitializeHearts()
    {
        if (heartsContainer == null || heartPrefab == null)
            return;
            
        // Clear existing hearts
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        _heartImages.Clear();
        _heartRects.Clear();
        
        // Create heart icons
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            heart.name = $"Heart_{i}";
            
            Image heartImg = heart.GetComponent<Image>();
            RectTransform heartRect = heart.GetComponent<RectTransform>();
            
            if (heartImg != null)
            {
                _heartImages.Add(heartImg);
                if (fullHeartSprite != null)
                    heartImg.sprite = fullHeartSprite;
                heartImg.color = fullHeartColor;
            }
            
            if (heartRect != null)
            {
                _heartRects.Add(heartRect);
            }
        }
    }
    
    void InitializeCoins()
    {
        if (coinIcon != null)
        {
            _coinIconRect = coinIcon.GetComponent<RectTransform>();
            if (_coinIconRect != null)
                _originalCoinScale = _coinIconRect.localScale;
        }
    }

    void Update()
    {
        UpdateChronoBar();
    }

    // ==================== EVENT HANDLERS ====================

    void HandleHealthChanged(int currentHealth)
    {
        if (currentHealth < 0) currentHealth = maxHearts;
        
        // Detect damage
        if (currentHealth != _previousHealth && _previousHealth != -1)
        {
            bool tookDamage = currentHealth < _previousHealth;
            int changedIndex = tookDamage ? currentHealth : currentHealth - 1;
            
            UpdateHeartVisuals(currentHealth, true);
            
            if (changedIndex >= 0 && changedIndex < _heartImages.Count)
            {
                if (_heartAnimCoroutine != null)
                    StopCoroutine(_heartAnimCoroutine);
                _heartAnimCoroutine = StartCoroutine(AnimateHeart(changedIndex, tookDamage));
            }
            
            if (tookDamage) StartCoroutine(ShakeHearts());
        }
        else
        {
            // Initial set
            UpdateHeartVisuals(currentHealth, false);
        }

        _previousHealth = currentHealth;
        _currentDisplayedHealth = currentHealth;
        
        // Heartbeat animation when low health
        if (currentHealth > 0 && currentHealth <= 2)
        {
        }
    }

    void HandleCoinChanged(int newAmount)
    {
        if (coinText != null) coinText.text = newAmount.ToString("000");
        
        if (newAmount > _previousCoins && _previousCoins != -1)
        {
            StartCoroutine(AnimateCoinPickup());
        }
        _previousCoins = newAmount;
    }

    // ==================== CHRONO BAR UPDATE ====================
    void UpdateChronoBar()
    {
        // ... (Keep existing ChronoBar logic as is)
        if (TimeManager.Instance == null || chronoBarFill == null)
            return;
            
        float currentEnergy = TimeManager.Instance.currentChronoEnergy;
        float maxEnergy = TimeManager.Instance.maxChronoEnergy;
        float energyPercent = currentEnergy / maxEnergy;
        
        if (currentEnergy > _previousEnergy + 0.5f) TriggerBarFlash();
        if (_previousEnergy > 0 && currentEnergy <= 0) TriggerBarShake();
            
        _previousEnergy = currentEnergy;
        
        bool isOverclockActive = Time.timeScale < 1f && Time.timeScale > 0f && !GameManager.Instance.isGamePaused;
        float targetFill = energyPercent;
        
        if (Mathf.Abs(_easeTargetValue - targetFill) > fillSnapThreshold)
        {
            _easeStartValue = _displayedFillAmount;
            _easeTargetValue = targetFill;
            _easeElapsed = 0f;
            _isEasing = true;
        }
        
        if (_isEasing)
        {
            _easeElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_easeElapsed / easeDuration);
            float easedT = EaseOutQuart(t);
            _displayedFillAmount = Mathf.LerpUnclamped(_easeStartValue, _easeTargetValue, easedT);
            _displayedFillAmount = Mathf.Clamp01(_displayedFillAmount);
            if (t >= 1f) { _isEasing = false; _displayedFillAmount = _easeTargetValue; }
        }
        
        if (useSegmentedBar && segmentCount > 0)
        {
            float segmentedValue = Mathf.Floor(_displayedFillAmount * segmentCount) / segmentCount;
            chronoBarFill.fillAmount = segmentedValue;
        }
        else
        {
            chronoBarFill.fillAmount = _displayedFillAmount;
        }
        
        Color targetColor = GetChronoTargetColor(energyPercent, isOverclockActive);
        _currentBarColor = Color.Lerp(_currentBarColor, targetColor, Time.unscaledDeltaTime * colorLerpSpeed);
        
        Color finalColor = _currentBarColor;
        float scaleMultiplier = 1f;
        
        if (energyPercent <= lowEnergyThreshold && energyPercent > 0)
        {
            float speed = energyPercent <= criticalEnergyThreshold ? criticalPulseSpeed : pulseSpeed;
            float pulse = Mathf.Sin(Time.unscaledTime * speed) * 0.5f + 0.5f;
            finalColor = Color.Lerp(_currentBarColor, _currentBarColor * 1.5f, pulse * pulseIntensity);
            finalColor.a = 1f;
            if (energyPercent <= criticalEnergyThreshold) scaleMultiplier = 1f + pulse * scaleBounceMagnitude;
        }
        
        if (isOverclockActive)
        {
            float activePulse = Mathf.Sin(Time.unscaledTime * 8f) * 0.5f + 0.5f;
            finalColor = Color.Lerp(finalColor, activeOverclockColor, 0.3f + activePulse * 0.2f);
        }
        
        if (_flashTimer > 0)
        {
            _flashTimer -= Time.unscaledDeltaTime;
            float flashLerp = _flashTimer / flashDuration;
            finalColor = Color.Lerp(finalColor, flashColor, flashLerp * 0.6f);
        }
        
        chronoBarFill.color = finalColor;
        
        if (_barRectTransform != null && !_isBarShaking)
            _barRectTransform.localScale = _originalBarScale * scaleMultiplier;
            
        UpdateBarShake();
        UpdateChronoGlow(energyPercent, isOverclockActive);

        // Low Health Heartbeat Logic (moved from UpdateHearts)
        if (_currentDisplayedHealth > 0 && _currentDisplayedHealth <= 2)
        {
            AnimateHeartbeat(_currentDisplayedHealth);
        }
    }
    
    // ... (Helper methods remain unchanged)
    Color GetChronoTargetColor(float energyPercent, bool isOverclockActive)
    {
        if (energyPercent <= 0)
            return depletedColor;
            
        Color baseColor = chronoGradient != null 
            ? chronoGradient.Evaluate(energyPercent) 
            : Color.Lerp(Color.red, Color.cyan, energyPercent);
            
        if (isOverclockActive)
            return Color.Lerp(baseColor, activeOverclockColor, 0.5f);
            
        return baseColor;
    }
    
    // ==================== EASING FUNCTION ====================
    /// <summary>
    /// EaseOutQuart - Best easing function for energy bars.
    /// Fast response → Smooth deceleration. Formula: 1 - (1 - t)^4
    /// </summary>
    float EaseOutQuart(float t)
    {
        return 1f - Mathf.Pow(1f - t, 4f);
    }
    
    void TriggerBarFlash() => _flashTimer = flashDuration;
    
    void TriggerBarShake()
    {
        _isBarShaking = true;
        _shakeTimer = shakeDuration;
    }
    
    void UpdateBarShake()
    {
        if (!_isBarShaking || _barRectTransform == null)
            return;
            
        _shakeTimer -= Time.unscaledDeltaTime;
        
        if (_shakeTimer <= 0)
        {
            _isBarShaking = false;
            _barRectTransform.localPosition = _originalBarPosition;
            _barRectTransform.localScale = _originalBarScale;
            return;
        }
        
        float progress = _shakeTimer / shakeDuration;
        float intensity = shakeIntensity * progress;
        
        Vector3 offset = new Vector3(
            Random.Range(-intensity, intensity),
            Random.Range(-intensity, intensity),
            0
        );
        
        _barRectTransform.localPosition = _originalBarPosition + offset;
    }
    
    void UpdateChronoGlow(float energyPercent, bool isOverclockActive)
    {
        if (chronoBarGlow == null)
            return;
            
        float glowAlpha = 0f;
        Color glowColor = _currentBarColor;
        
        if (isOverclockActive)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * glowPulseSpeed * 3f) * 0.5f + 0.5f;
            glowAlpha = maxGlowAlpha * (0.5f + pulse * 0.5f);
            glowColor = activeOverclockColor;
        }
        else if (energyPercent <= criticalEnergyThreshold && energyPercent > 0)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * criticalPulseSpeed) * 0.5f + 0.5f;
            glowAlpha = maxGlowAlpha * pulse;
            glowColor = Color.red;
        }
        else if (energyPercent <= lowEnergyThreshold && energyPercent > 0)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f;
            glowAlpha = maxGlowAlpha * 0.4f * pulse;
        }
        else if (energyPercent > 0.9f)
        {
            glowAlpha = maxGlowAlpha * 0.15f;
        }
        
        glowColor.a = glowAlpha;
        chronoBarGlow.color = glowColor;
    }

    // ==================== HEARTS UPDATE ====================
    
    void UpdateHeartVisuals(int health, bool animated)
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            bool isFull = i < health;
            
            if (fullHeartSprite != null)
            {
                _heartImages[i].sprite = fullHeartSprite;
            }
            
            // Full heart = visible, lost heart = completely invisible
            _heartImages[i].color = isFull ? fullHeartColor : Color.clear;
        }
    }
    
    void AnimateHeartbeat(int currentHealth)
    {
        float speed = currentHealth == 1 ? lowHealthBeatSpeed * 1.5f : lowHealthBeatSpeed;
        float beat = Mathf.Sin(Time.unscaledTime * speed) * 0.5f + 0.5f;
        float scale = 1f + beat * (heartBeatScale - 1f);
        
        // Animate only the remaining full hearts
        for (int i = 0; i < currentHealth && i < _heartRects.Count; i++)
        {
            _heartRects[i].localScale = Vector3.one * scale;
        }
    }
    
    IEnumerator AnimateHeart(int index, bool isDamage)
    {
        if (index < 0 || index >= _heartRects.Count)
            yield break;
            
        RectTransform rect = _heartRects[index];
        Image img = _heartImages[index];
        
        if (isDamage)
        {
            // DAMAGE ANIMATION: Scale up, darken, and fade out
            float elapsed = 0f;
            Color startColor = fullHeartColor;
            Vector3 startScale = rect.localScale;
            
            while (elapsed < heartDamageDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / heartDamageDuration;
                
                // Ease out for smoother animation
                float easedT = 1f - Mathf.Pow(1f - t, 2f);
                
                // Scale: grow bigger over time
                float currentScale = Mathf.Lerp(1f, heartDamageScale, easedT);
                rect.localScale = Vector3.one * currentScale;
                
                // Color: darken and fade out
                Color targetColor = heartDarkenColor;
                targetColor.a = Mathf.Lerp(1f, 0f, easedT);  // Fade to transparent
                img.color = Color.Lerp(startColor, targetColor, easedT);
                
                yield return null;
            }
            
            // Final state: reset scale, heart is now invisible
            rect.localScale = Vector3.one;
            img.color = Color.clear;  // Completely transparent - heart disappeared
        }
        else
        {
            // HEAL ANIMATION: Pop and flash green
            Color originalColor = fullHeartColor;
            Color flashCol = healColor;
            
            float elapsed = 0f;
            
            while (elapsed < heartAnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / heartAnimDuration;
                
                // Scale: pop out then back
                float scaleT = t < 0.3f 
                    ? Mathf.Lerp(1f, heartPopScale, t / 0.3f) 
                    : Mathf.Lerp(heartPopScale, 1f, (t - 0.3f) / 0.7f);
                rect.localScale = Vector3.one * scaleT;
                
                // Color flash
                float colorT = t < 0.2f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.2f) / 0.8f);
                img.color = Color.Lerp(originalColor, flashCol, colorT);
                
                yield return null;
            }
            
            rect.localScale = Vector3.one;
            img.color = originalColor;
        }
    }
    
    IEnumerator ShakeHearts()
    {
        List<Vector3> originalPositions = new List<Vector3>();
        foreach (var rect in _heartRects)
        {
            originalPositions.Add(rect.localPosition);
        }
        
        float elapsed = 0f;
        while (elapsed < heartShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float intensity = heartShakeIntensity * (1f - elapsed / heartShakeDuration);
            
            for (int i = 0; i < _heartRects.Count; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity),
                    0
                );
                _heartRects[i].localPosition = originalPositions[i] + offset;
            }
            
            yield return null;
        }
        
        // Reset positions
        for (int i = 0; i < _heartRects.Count; i++)
        {
            _heartRects[i].localPosition = originalPositions[i];
        }
    }

    // ==================== COINS UPDATE ====================
    void UpdateCoins()
    {
        if (GameManager.Instance == null || coinText == null)
            return;
            
        int currentCoins = GameManager.Instance.coins;
        coinText.text = currentCoins.ToString("000");
        
        // Animate on coin pickup
        if (currentCoins > _previousCoins && _previousCoins >= 0)
        {
            StartCoroutine(AnimateCoinPickup());
        }
        
        _previousCoins = currentCoins;
    }
    
    IEnumerator AnimateCoinPickup()
    {
        if (_coinIconRect == null)
            yield break;
            
        float elapsed = 0f;
        while (elapsed < coinAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / coinAnimDuration;
            
            float scale = t < 0.3f 
                ? Mathf.Lerp(1f, coinPopScale, t / 0.3f) 
                : Mathf.Lerp(coinPopScale, 1f, (t - 0.3f) / 0.7f);
                
            _coinIconRect.localScale = _originalCoinScale * scale;
            yield return null;
        }
        
        _coinIconRect.localScale = _originalCoinScale;
    }
    
    // ==================== PUBLIC METHODS ====================
    public void RefreshHearts()
    {
        InitializeHearts();
    }
    
    public void ForceUpdateHealth(int health)
    {
        _previousHealth = -1;
        UpdateHeartVisuals(health, false);
    }
}