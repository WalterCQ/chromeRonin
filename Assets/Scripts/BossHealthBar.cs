using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;
    public GameObject visualContainer; // To hide/show the bar

    [Header("Settings")]
    public string bossName = "The Ascended";
    public bool showOnStart = false;

    private Health _bossHealth;

    // Call this from BossChaseController when the fight starts
    public void Initialize(Health bossHealth)
    {
        _bossHealth = bossHealth;
        
        if (bossNameText != null)
            bossNameText.text = bossName;

        if (visualContainer != null)
            visualContainer.SetActive(true);
    }

    void Start()
    {
        if (!showOnStart && visualContainer != null)
            visualContainer.SetActive(false);
    }

    void Update()
    {
        if (_bossHealth == null) return;

        // Smoothly update the slider using your Health.cs 'GetHealthPercent'
        healthSlider.value = Mathf.Lerp(healthSlider.value, _bossHealth.GetHealthPercent(), Time.deltaTime * 5f);

        // Optional: Hide bar if boss dies
        if (_bossHealth.GetHealthPercent() <= 0 && visualContainer.activeSelf)
        {
            // Delay hiding or fade out could go here
            visualContainer.SetActive(false);
        }
    }
}