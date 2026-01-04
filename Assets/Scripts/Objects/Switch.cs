<<<<<<< HEAD
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public bool isOneTimeUse = false;
    public bool isActivated = false;
    
    [Header("Visuals")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;

    [Header("Events")]
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    public void TakeDamage(int damage)
    {
        // Toggle the switch
        if (isOneTimeUse && isActivated) return;

        Toggle();
    }

    public void Toggle()
    {
        isActivated = !isActivated;

        if (isActivated)
        {
            onActivate.Invoke();
        }
        else
        {
            onDeactivate.Invoke();
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (_sr == null) return;

        if (isActivated)
        {
            if (activeSprite) _sr.sprite = activeSprite;
            _sr.color = activeColor;
        }
        else
        {
            if (inactiveSprite) _sr.sprite = inactiveSprite;
            _sr.color = inactiveColor;
        }
    }
=======
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public bool isOneTimeUse = false;
    public bool isActivated = false;
    
    [Header("Visuals")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    [Header("Events")]
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    public void TakeDamage(int damage)
    {
        if (isOneTimeUse && isActivated) return;

        Toggle();
    }

    public void Toggle()
    {
        isActivated = !isActivated;

        if (isActivated)
        {
            onActivate.Invoke();
        }
        else
        {
            onDeactivate.Invoke();
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (_sr == null) return;

        if (isActivated)
        {
            if (activeSprite) _sr.sprite = activeSprite;
        }
        else
        {
            if (inactiveSprite) _sr.sprite = inactiveSprite;
        }
    }
>>>>>>> upstream/dev
}