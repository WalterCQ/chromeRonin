using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for controlling volume settings with sliders.
/// Attach this to a panel containing volume sliders.
/// Works with both Pause Menu and Main Menu.
/// </summary>
public class VolumeSettings : MonoBehaviour
{
    [Header("Sliders")]
    [Tooltip("Slider for controlling background music volume")]
    public Slider musicSlider;
    [Tooltip("Slider for controlling sound effects volume")]
    public Slider sfxSlider;

    [Header("Optional Labels")]
    [Tooltip("Optional: Text to show music volume percentage")]
    public TMPro.TMP_Text musicVolumeText;
    [Tooltip("Optional: Text to show SFX volume percentage")]
    public TMPro.TMP_Text sfxVolumeText;

    [Header("Fill Colors")]
    [Tooltip("Color when volume is greater than 0")]
    public Color activeFillColor = new Color(1f, 0.85f, 0.2f, 1f); // Yellowish
    [Tooltip("Color when volume is 0")]
    public Color inactiveFillColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Gray

    // References to fill images
    private Image musicFillImage;
    private Image sfxFillImage;

    private void OnEnable()
    {
        // Load current volume values when panel becomes active
        InitializeSliders();
    }

    private void Start()
    {
        // Get fill image references
        if (musicSlider != null)
        {
            musicFillImage = musicSlider.fillRect?.GetComponent<Image>();
        }
        if (sfxSlider != null)
        {
            sfxFillImage = sfxSlider.fillRect?.GetComponent<Image>();
        }

        InitializeSliders();
        
        // Add listeners
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void InitializeSliders()
    {
        // Get fill image references if not already set
        if (musicFillImage == null && musicSlider != null)
        {
            musicFillImage = musicSlider.fillRect?.GetComponent<Image>();
        }
        if (sfxFillImage == null && sfxSlider != null)
        {
            sfxFillImage = sfxSlider.fillRect?.GetComponent<Image>();
        }

        // Get current volume from AudioManager or PlayerPrefs
        float currentMusicVolume = AudioManager.Instance != null 
            ? AudioManager.Instance.GetMusicVolume() 
            : PlayerPrefs.GetFloat("MusicVolume", 1f);
            
        float currentSFXVolume = AudioManager.Instance != null 
            ? AudioManager.Instance.GetSFXVolume() 
            : PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Set slider values without triggering callbacks
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(currentMusicVolume);
            UpdateMusicVolumeText(currentMusicVolume);
            UpdateSliderFillColor(musicFillImage, currentMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(currentSFXVolume);
            UpdateSFXVolumeText(currentSFXVolume);
            UpdateSliderFillColor(sfxFillImage, currentSFXVolume);
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else
        {
            // Fallback: Save directly to PlayerPrefs (no Save() call - deferred)
            PlayerPrefs.SetFloat("MusicVolume", value);
            
            // Update MusicManager directly if available
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.SetMasterVolume(value);
            }
        }

        UpdateMusicVolumeText(value);
        UpdateSliderFillColor(musicFillImage, value);
    }

    private void OnSFXSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        else
        {
            // Fallback: Save directly to PlayerPrefs (no Save() call - deferred)
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        UpdateSFXVolumeText(value);
        UpdateSliderFillColor(sfxFillImage, value);
    }

    private void UpdateSliderFillColor(Image fillImage, float value)
    {
        if (fillImage != null)
        {
            fillImage.color = value > 0f ? activeFillColor : inactiveFillColor;
        }
    }

    private void UpdateMusicVolumeText(float value)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    private void UpdateSFXVolumeText(float value)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    private void OnDisable()
    {
        // Save PlayerPrefs when panel closes (deferred save for performance)
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        }
    }
}
