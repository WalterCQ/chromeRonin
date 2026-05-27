using UnityEngine;
using System.Collections;
using System;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject deathScreenPanel; 
    
    [Header("Audio")]
    public AudioSource audioSource; 
    public AudioClip deathSound;    

    [Header("Settings")]
    public float restartDelay = 0.5f; 
    
    // PlayerPrefs key for SFX volume (must match VolumeSettings)
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const float DEFAULT_VOLUME = 0.75f;

    void Start()
    {
        if (deathScreenPanel) deathScreenPanel.SetActive(false);
    }
    
    private float GetSFXVolume()
    {
        if (SFXManager.Instance != null)
        {
            return SFXManager.GetVolume();
        }
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }

    public void PlayDeathSequence(Action onComplete)
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, GetSFXVolume());
        }

        if (deathScreenPanel) deathScreenPanel.SetActive(true);

        StartCoroutine(SequenceRoutine(onComplete));
    }

    IEnumerator SequenceRoutine(Action onComplete)
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        
        onComplete?.Invoke();
    }
}