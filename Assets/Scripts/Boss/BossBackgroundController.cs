using UnityEngine;
using UnityEngine.Events;

public class BossBackgroundController : MonoBehaviour
{
    [Header("--- Settings ---")]
    [Tooltip("Background start Y position when boss is at full health")]
    public float startY = 0f;

    [Tooltip("Background end Y position when boss dies (should be less than startY)")]
    public float endY = -50f;

    [Tooltip("Constant movement speed (units per second) - elevator style")]
    public float moveSpeed = 5f;

    [Header("--- Events ---")]
    [Tooltip("Triggered when background reaches the top (startY) after boss dies")]
    public UnityEvent onReachedTop;

    [Header("--- Current State (Read Only) ---")]
    [SerializeField] private float _currentHealthPercent = 1f;
    [SerializeField] private bool _hasReachedTop = false;
    private float _targetY;

    void Start()
    {
        _targetY = startY;
        Vector3 pos = transform.position;
        pos.y = startY;
        transform.position = pos;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        // MoveTowards: constant speed movement like an elevator
        pos.y = Mathf.MoveTowards(pos.y, _targetY, moveSpeed * Time.deltaTime);
        transform.position = pos;

        // Check if background reached top after boss died (health = 0)
        if (!_hasReachedTop && _currentHealthPercent <= 0f && Mathf.Abs(pos.y - startY) < 0.1f)
        {
            _hasReachedTop = true;
            Debug.Log("[BossBackground] Reached top! Triggering victory sequence.");
            onReachedTop?.Invoke();
        }
    }

    /// <summary>
    /// Called by boss script to set current health percentage
    /// </summary>
    /// <param name="percent">0~1, 1=full health, 0=dead</param>
    public void SetBossHealthPercent(float percent)
    {
        _currentHealthPercent = Mathf.Clamp01(percent);
        _targetY = Mathf.Lerp(endY, startY, _currentHealthPercent);
    }

    [ContextMenu("Test Damage (50%)")]
    void TestDamage()
    {
        SetBossHealthPercent(0.5f);
    }

    [ContextMenu("Test Reset (100%)")]
    void TestReset()
    {
        SetBossHealthPercent(1f);
    }
}
