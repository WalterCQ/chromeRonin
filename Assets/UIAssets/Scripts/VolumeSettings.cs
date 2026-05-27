using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using SlimUI.ModernMenu;
using TMPro;

public class VolumeSettings : MonoBehaviour
{
    [Header("Audio Mixer")]
    [Tooltip("Optional: Assign an AudioMixer to control volume via mixer groups")]
    public AudioMixer audioMixer;
    
    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    
    [Header("Slider Fill Images (For Color Change)")]
    [Tooltip("Assign the Fill image from the Music Slider")]
    public Image musicSliderFill;
    [Tooltip("Assign the Fill image from the SFX Slider")]
    public Image sfxSliderFill;
    
    [Header("Slider Colors")]
    public Color activeColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellowish color
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray when volume is 0
    
    [Header("Audio Sources")]
    [Tooltip("Assign all your background music AudioSources here")]
    public AudioSource[] backgroundMusicSources;
    [Tooltip("Assign all your SFX AudioSources here")]
    public AudioSource[] sfxSources;
    
    [Header("Volume Percentage Text (Right Side)")]
    public TMP_Text musicVolumeText;
    public TMP_Text sfxVolumeText;
    
    // PlayerPrefs keys for saving volume settings
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    
    // Default volume values
    private const float DEFAULT_VOLUME = 0.75f;

    private void Start()
    {
        // Load saved volume settings or use defaults
        float savedMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
        
        // Initialize sliders with saved values
        if (musicSlider != null)
        {
            musicSlider.value = savedMusicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        
        // Apply the saved volumes
        ApplyMusicVolume(savedMusicVolume);
        ApplySFXVolume(savedSFXVolume);
        
        // Update volume text displays and slider colors
        UpdateMusicVolumeText(savedMusicVolume);
        UpdateSFXVolumeText(savedSFXVolume);
        UpdateSliderFillColor(musicSliderFill, savedMusicVolume);
        UpdateSliderFillColor(sfxSliderFill, savedSFXVolume);
    }
    
    private void OnEnable()
    {
        // Refresh slider values when the settings panel is opened
        float savedMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
        
        if (musicSlider != null)
            musicSlider.value = savedMusicVolume;
        if (sfxSlider != null)
            sfxSlider.value = savedSFXVolume;
            
        UpdateMusicVolumeText(savedMusicVolume);
        UpdateSFXVolumeText(savedSFXVolume);
        UpdateSliderFillColor(musicSliderFill, savedMusicVolume);
        UpdateSliderFillColor(sfxSliderFill, savedSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        ApplyMusicVolume(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
        UpdateMusicVolumeText(volume);
        UpdateSliderFillColor(musicSliderFill, volume);
    }

    public void SetSFXVolume(float volume)
    {
        ApplySFXVolume(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
        UpdateSFXVolumeText(volume);
        UpdateSliderFillColor(sfxSliderFill, volume);
    }

    private void ApplyMusicVolume(float volume)
    {
        // Method 1: Using AudioMixer (Recommended)
        if (audioMixer != null)
        {
            // Convert linear slider value (0-1) to logarithmic decibels (-80 to 0)
            float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat("MusicVolume", dbValue);
        }
        
        // Method 2: Update MusicManager singleton if it exists
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(volume);
        }
        
        // Method 3: Direct AudioSource control for multiple music sources
        if (backgroundMusicSources != null && backgroundMusicSources.Length > 0)
        {
            foreach (AudioSource music in backgroundMusicSources)
            {
                if (music != null)
                    music.volume = volume;
            }
        }
        
        // Method 4: Find all CheckMusicVolume components and update them
        CheckMusicVolume[] musicVolumeComponents = FindObjectsOfType<CheckMusicVolume>(true);
        foreach (CheckMusicVolume mvc in musicVolumeComponents)
        {
            if (mvc != null)
            {
                AudioSource audioSrc = mvc.GetComponent<AudioSource>();
                if (audioSrc != null)
                    audioSrc.volume = volume;
            }
        }
    }

    private void ApplySFXVolume(float volume)
    {
        // Method 1: Using AudioMixer (Recommended)
        if (audioMixer != null)
        {
            // Convert linear slider value (0-1) to logarithmic decibels (-80 to 0)
            float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat("SFXVolume", dbValue);
        }
        
        // Method 2: Update SFXManager singleton if it exists
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.SetVolume(volume);
        }
        
        // Method 3: Direct AudioSource control for multiple SFX sources
        if (sfxSources != null && sfxSources.Length > 0)
        {
            foreach (AudioSource sfx in sfxSources)
            {
                if (sfx != null)
                    sfx.volume = volume;
            }
        }
        
        // Method 4: Find all CheckSFXVolume components and update them
        CheckSFXVolume[] sfxVolumeComponents = FindObjectsOfType<CheckSFXVolume>(true);
        foreach (CheckSFXVolume svc in sfxVolumeComponents)
        {
            if (svc != null)
            {
                AudioSource audioSrc = svc.GetComponent<AudioSource>();
                if (audioSrc != null)
                    audioSrc.volume = volume;
            }
        }
    }

    private void UpdateSliderFillColor(Image fillImage, float volume)
    {
        if (fillImage != null)
        {
            // Yellow when volume > 0, gray when volume is 0
            fillImage.color = volume > 0 ? activeColor : inactiveColor;
        }
    }

    private void UpdateMusicVolumeText(float volume)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    private void UpdateSFXVolumeText(float volume)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    // Call this method to play a test sound effect when adjusting SFX volume
    public void PlayTestSFX()
    {
        if (sfxSources != null && sfxSources.Length > 0 && sfxSources[0] != null)
        {
            sfxSources[0].PlayOneShot(sfxSources[0].clip);
        }
    }

    // Static method to get saved music volume (useful for other scripts)
    public static float GetSavedMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
    }

    // Static method to get saved SFX volume (useful for other scripts)
    public static float GetSavedSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }
}
