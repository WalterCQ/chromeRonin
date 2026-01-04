using UnityEngine;

/// <summary>
/// Attach this to any AudioSource that plays SFX to automatically respect the SFX volume setting.
/// Updates volume on start and whenever the SFX volume changes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SFXAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;
    private float _baseVolume;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _baseVolume = _audioSource.volume;
    }

    private void OnEnable()
    {
        // Subscribe to volume changes
        AudioManager.OnSFXVolumeChanged += OnSFXVolumeChanged;
        
        // Apply current volume
        ApplyVolume();
    }

    private void OnDisable()
    {
        AudioManager.OnSFXVolumeChanged -= OnSFXVolumeChanged;
    }

    private void Start()
    {
        ApplyVolume();
    }

    private void OnSFXVolumeChanged(float volume)
    {
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        float sfxVolume = AudioManager.Instance != null 
            ? AudioManager.Instance.GetSFXVolume() 
            : PlayerPrefs.GetFloat("SFXVolume", 1f);
            
        _audioSource.volume = _baseVolume * sfxVolume;
    }

    /// <summary>
    /// Play a one-shot clip with SFX volume applied
    /// </summary>
    public void PlayOneShotWithVolume(AudioClip clip)
    {
        if (clip == null) return;
        
        float sfxVolume = AudioManager.Instance != null 
            ? AudioManager.Instance.GetSFXVolume() 
            : PlayerPrefs.GetFloat("SFXVolume", 1f);
            
        _audioSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Play a one-shot clip with SFX volume and additional volume multiplier
    /// </summary>
    public void PlayOneShotWithVolume(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null) return;
        
        float sfxVolume = AudioManager.Instance != null 
            ? AudioManager.Instance.GetSFXVolume() 
            : PlayerPrefs.GetFloat("SFXVolume", 1f);
            
        _audioSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }
}
