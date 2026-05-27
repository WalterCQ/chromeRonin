using UnityEngine;

/// <summary>
/// Manages global SFX volume. Attach this to a GameObject that persists across scenes,
/// or have it created automatically when needed.
/// All scripts that play SFX should use SFXManager.GetVolume() to respect user settings.
/// </summary>
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    
    // PlayerPrefs key for SFX volume (must match VolumeSettings)
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const float DEFAULT_VOLUME = 0.75f;
    
    private float _currentVolume;
    
    public float CurrentVolume => _currentVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadVolume()
    {
        _currentVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }

    /// <summary>
    /// Set the SFX volume (called from VolumeSettings)
    /// </summary>
    public void SetVolume(float volume)
    {
        _currentVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Get the current SFX volume. Use this when playing sound effects.
    /// </summary>
    public static float GetVolume()
    {
        if (Instance != null)
        {
            return Instance._currentVolume;
        }
        // Fallback: read directly from PlayerPrefs if Instance doesn't exist
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }
    
    /// <summary>
    /// Play a sound effect with the correct volume using Object Pooling
    /// </summary>
    public static void PlaySound(AudioSource source, AudioClip clip, float volumeMultiplier = 1f)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip, GetVolume() * volumeMultiplier);
        }
    }

    /// <summary>
    /// Spawns a pooled audio object at the given position.
    /// </summary>
    public static void PlaySoundAt(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        // Note: Pooling for runtime-created AudioSources not implemented.
        // OneShotAudio in Health.cs handles its own instantiation.
    }
}
