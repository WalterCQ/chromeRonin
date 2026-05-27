using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource _audioSource;

    [Header("Fade Settings")]
    [Tooltip("Fade in duration (seconds)")]
    public float fadeInDuration = 1.5f;
    [Tooltip("Fade out duration (seconds)")]
    public float fadeOutDuration = 1.0f;
    [Tooltip("Target volume")]
    [Range(0f, 1f)]
    public float targetVolume = 1.0f;

    private Coroutine _fadeCoroutine;
    
    // PlayerPrefs key for music volume (must match VolumeSettings)
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const float DEFAULT_VOLUME = 0.75f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            _audioSource = GetComponent<AudioSource>();
            
            // Load saved volume from PlayerPrefs
            targetVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
            _audioSource.volume = targetVolume;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip musicToPlay)
    {
        if (_audioSource.clip == musicToPlay && _audioSource.isPlaying)
        {
            return; 
        }

        // Stop current fade coroutine
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(CrossFadeMusic(musicToPlay));
    }

    /// <summary>
    /// Cross-fade to new music
    /// </summary>
    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // If currently playing music, fade out first
        if (_audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut());
        }

        // Switch to new music and fade in
        _audioSource.clip = newClip;
        _audioSource.volume = 0f;
        _audioSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Fade in effect
    /// </summary>
    private IEnumerator FadeIn()
    {
        float currentTime = 0f;
        float startVolume = 0f;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeInDuration);
            yield return null;
        }

        _audioSource.volume = targetVolume;
    }

    /// <summary>
    /// Fade out effect
    /// </summary>
    private IEnumerator FadeOut()
    {
        float currentTime = 0f;
        float startVolume = _audioSource.volume;

        while (currentTime < fadeOutDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeOutDuration);
            yield return null;
        }

        _audioSource.volume = 0f;
        _audioSource.Stop();
    }

    /// <summary>
    /// Stop music (with fade out effect)
    /// </summary>
    public void StopMusic()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        yield return StartCoroutine(FadeOut());
        _audioSource.clip = null;
    }

    /// <summary>
    /// Pause music (with fade out effect)
    /// </summary>
    public void PauseMusic()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeOutAndPause());
    }

    private IEnumerator FadeOutAndPause()
    {
        yield return StartCoroutine(FadeOut());
        _audioSource.Pause();
    }

    /// <summary>
    /// Resume music (with fade in effect)
    /// </summary>
    public void ResumeMusic()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _audioSource.UnPause();
        _fadeCoroutine = StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Set volume (takes effect immediately)
    /// </summary>
    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        _audioSource.volume = targetVolume;
    }
}