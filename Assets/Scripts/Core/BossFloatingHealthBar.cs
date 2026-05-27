using UnityEngine;
using UnityEngine.UI;

public class BossFloatingHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider slider;
    public Health bossHealth;
    public GameObject canvasObject;
    public Image fillImage; 

    [Header("Name Tag")]
    public Text bossNameText;
    public string bossName = "Shinobi Killer";

    [Header("Color Transition")]
    [Tooltip("Define the color over health. Right side is 100%, Left side is 0%.")]
    public Gradient healthGradient; // <-- Replaces the single color
    public Color hitFlashColor = Color.white;

    [Header("Juice Settings")]
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.2f;
    public float scalePunch = 1.2f;

    // Internal State
    private float _lastHealthPercent;
    private float _shakeTimer;
    private float _flashTimer;
    private Vector3 _originalPos;
    private Vector3 _originalScale;
    private RectTransform _rectTransform;

    void Start()
    {
        if (slider == null) slider = GetComponent<Slider>();
        
        if (fillImage == null && slider != null)
            fillImage = slider.fillRect.GetComponent<Image>();

        // Set a default "Red -> Purple" gradient if one isn't set in Inspector
        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
        {
            healthGradient = new Gradient();
            // 100% = Red, 50% = Purple, 0% = Dark Purple
            GradientColorKey[] keys = new GradientColorKey[3];
            keys[0] = new GradientColorKey(new Color(0.2f, 0f, 0.2f), 0.0f); // 0% (Near Death)
            keys[1] = new GradientColorKey(new Color(0.5f, 0f, 0.5f), 0.5f); // 50% (Phase 2 Start)
            keys[2] = new GradientColorKey(Color.red, 1.0f);                 // 100% (Full Health)
            
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphas[1] = new GradientAlphaKey(1.0f, 1.0f);
            
            healthGradient.SetKeys(keys, alphas);
        }

        _rectTransform = GetComponent<RectTransform>();
        _originalPos = _rectTransform.localPosition;
        _originalScale = _rectTransform.localScale;

        if (bossHealth != null) 
            _lastHealthPercent = bossHealth.GetHealthPercent();

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
    }

    void Update()
    {
        if (bossHealth == null) return;

        float currentPercent = bossHealth.GetHealthPercent();

        // 1. Detect Damage
        if (currentPercent < _lastHealthPercent)
        {
            OnTakeDamage();
        }
        _lastHealthPercent = currentPercent;

        // 2. Smooth Slider
        slider.value = Mathf.Lerp(slider.value, currentPercent, Time.deltaTime * 10f);

        // 3. Shake Effect
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            randomOffset.z = 0;
            _rectTransform.localPosition = _originalPos + randomOffset;
        }
        else
        {
            _rectTransform.localPosition = _originalPos;
        }

        // 4. Color Logic (Gradient vs Flash)
        if (fillImage)
        {
            if (_flashTimer > 0)
            {
                _flashTimer -= Time.deltaTime;
                fillImage.color = hitFlashColor; // Flash White
            }
            else
            {
                // This line does the magic: Evaluate color based on health %
                fillImage.color = healthGradient.Evaluate(currentPercent);
            }
        }

        // 5. Scale Recovery
        _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, _originalScale, Time.deltaTime * 10f);

        // 6. Hide if dead
        if (currentPercent <= 0)
        {
            if (canvasObject != null) canvasObject.SetActive(false);
        }
        else
        {
            if (canvasObject != null && !canvasObject.activeSelf) 
                canvasObject.SetActive(true);
        }
    }

    void OnTakeDamage()
    {
        _shakeTimer = shakeDuration;
        _flashTimer = 0.1f;
        _rectTransform.localScale = _originalScale * scalePunch;
    }
}