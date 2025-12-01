using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{

    [Header("Chrono Bar")]
    public Image chronoBarFill;

    [Header("Coins")]
    public TextMeshProUGUI coinText;

    void Update()
    {
        UpdateChrono();
        UpdateCoins();
    }

    void UpdateChrono()
    {
        if (TimeManager.Instance != null && chronoBarFill != null)
        {
            float energyPercent = TimeManager.Instance.currentChronoEnergy / TimeManager.Instance.maxChronoEnergy;
            chronoBarFill.fillAmount = energyPercent;

            if (energyPercent > 0f) chronoBarFill.color = Color.cyan;
            else chronoBarFill.color = Color.white;
        }
    }

    void UpdateCoins()
    {
        if (GameManager.Instance != null && coinText != null)
        {
            coinText.text = GameManager.Instance.coins.ToString("000");
        }
    }
}