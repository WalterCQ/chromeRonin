<<<<<<< HEAD
using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource _audioSource;

    [Header("Fade Settings")]
    [Tooltip("渐入时间（秒）")]
    public float fadeInDuration = 1.5f;
    [Tooltip("渐出时间（秒）")]
    public float fadeOutDuration = 1.0f;
    [Tooltip("目标音量")]
    [Range(0f, 1f)]
    public float targetVolume = 1.0f;

    // Master volume multiplier (set by AudioManager)
    private float _masterVolume = 1.0f;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            _audioSource = GetComponent<AudioSource>();
            
            // Load saved music volume on startup
            _masterVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
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

        // 停止当前的渐变协程
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(CrossFadeMusic(musicToPlay));
    }

    /// <summary>
    /// 交叉渐变到新音乐
    /// </summary>
    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // 如果当前正在播放音乐，先渐出
        if (_audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut());
        }

        // 切换到新音乐并渐入
        _audioSource.clip = newClip;
        _audioSource.volume = 0f;
        _audioSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 渐入效果
    /// </summary>
    private IEnumerator FadeIn()
    {
        float currentTime = 0f;
        float startVolume = 0f;
        float effectiveTargetVolume = targetVolume * _masterVolume;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, effectiveTargetVolume, currentTime / fadeInDuration);
            yield return null;
        }

        _audioSource.volume = effectiveTargetVolume;
    }

    /// <summary>
    /// 渐出效果
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
    /// 停止音乐（带渐出效果）
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
    /// 暂停音乐（带渐出效果）
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
    /// 恢复音乐（带渐入效果）
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
    /// 设置音量（立即生效）
    /// </summary>
    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        _audioSource.volume = targetVolume * _masterVolume;
    }

    /// <summary>
    /// Set master volume (called by AudioManager/VolumeSettings)
    /// This scales the music volume without affecting the target volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        // Apply immediately to current playback
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.volume = targetVolume * _masterVolume;
        }
    }

    /// <summary>
    /// Get current master volume
    /// </summary>
    public float GetMasterVolume()
    {
        return _masterVolume;
    }
=======
using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource _audioSource;

    [Header("Fade Settings")]
    [Tooltip("渐入时间（秒）")]
    public float fadeInDuration = 1.5f;
    [Tooltip("渐出时间（秒）")]
    public float fadeOutDuration = 1.0f;
    [Tooltip("目标音量")]
    [Range(0f, 1f)]
    public float targetVolume = 1.0f;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            _audioSource = GetComponent<AudioSource>();
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

        // 停止当前的渐变协程
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(CrossFadeMusic(musicToPlay));
    }

    /// <summary>
    /// 交叉渐变到新音乐
    /// </summary>
    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // 如果当前正在播放音乐，先渐出
        if (_audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut());
        }

        // 切换到新音乐并渐入
        _audioSource.clip = newClip;
        _audioSource.volume = 0f;
        _audioSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 渐入效果
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
    /// 渐出效果
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
    /// 停止音乐（带渐出效果）
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
    /// 暂停音乐（带渐出效果）
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
    /// 恢复音乐（带渐入效果）
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
    /// 设置音量（立即生效）
    /// </summary>
    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        _audioSource.volume = targetVolume;
    }
>>>>>>> upstream/dev
}