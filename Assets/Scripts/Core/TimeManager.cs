using UnityEngine;

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

    [Header("Input")]
    public KeyCode overclockKey = KeyCode.L;

    private bool _isOverclockActive = false;
    private float _fixedDeltaTime; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        _fixedDeltaTime = Time.fixedDeltaTime;
        ResetEnergy();
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGamePaused || GameManager.Instance.isGameOver))
        {
            return;
        }

        HandleInput();
        HandleTimeScale();
    }

    public void ResetEnergy()
    {
        currentChronoEnergy = maxChronoEnergy;
        _isOverclockActive = false;
    }

    void HandleInput()
    {
        if (Input.GetKey(overclockKey))
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

    void HandleTimeScale()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused) return;

        if (_isOverclockActive)
        {
            Time.timeScale = slowdownFactor;
        }
        else
        {
            Time.timeScale = 1f;
        }

        Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
    }

    public void AddChronoEnergy(float amount)
    {
        currentChronoEnergy += amount;
        if (currentChronoEnergy > maxChronoEnergy) currentChronoEnergy = maxChronoEnergy;
    }
}