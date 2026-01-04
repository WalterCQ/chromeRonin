using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton Audio Manager that persists across all scenes and manages volume settings.
/// Controls both Music and SFX volumes globally.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    // PlayerPrefs keys
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // Events for volume changes
    public delegate void VolumeChangedHandler(float volume);
    public static event VolumeChangedHandler OnMusicVolumeChanged;
    public static event VolumeChangedHandler OnSFXVolumeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        // Load saved settings, default to 1 (full volume) if not set
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        // Apply the loaded settings
        ApplyMusicVolume();
        ApplySFXVolume();
    }

    /// <summary>
    /// Set music volume (0 to 1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        // Note: PlayerPrefs.Save() is deferred to OnDisable/OnApplicationQuit for performance
        ApplyMusicVolume();
    }

    /// <summary>
    /// Set SFX volume (0 to 1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        // Note: PlayerPrefs.Save() is deferred to OnDisable/OnApplicationQuit for performance
        ApplySFXVolume();
    }

    /// <summary>
    /// Save settings to disk (call when closing settings panel or quitting)
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Get current music volume
    /// </summary>
    public float GetMusicVolume()
    {
        return musicVolume;
    }

    /// <summary>
    /// Get current SFX volume
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    private void ApplyMusicVolume()
    {
        // Update MusicManager if available
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMasterVolume(musicVolume);
        }

        // Fire event for any other listeners
        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    private void ApplySFXVolume()
    {
        // Fire event for any SFX listeners
        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    /// <summary>
    /// Get the effective SFX volume for playing sounds
    /// </summary>
    public static float GetEffectiveSFXVolume()
    {
        if (Instance != null)
        {
            return Instance.sfxVolume;
        }
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// Get the effective Music volume
    /// </summary>
    public static float GetEffectiveMusicVolume()
    {
        if (Instance != null)
        {
            return Instance.musicVolume;
        }
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
    }
}
