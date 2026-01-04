<<<<<<< HEAD
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

    void Start()
    {
        if (deathScreenPanel) deathScreenPanel.SetActive(false);
    }

    public void PlayDeathSequence(Action onComplete)
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathScreenPanel) deathScreenPanel.SetActive(true);

        StartCoroutine(SequenceRoutine(onComplete));
    }

    IEnumerator SequenceRoutine(Action onComplete)
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        
        onComplete?.Invoke();
    }
=======
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

    void Start()
    {
        if (deathScreenPanel) deathScreenPanel.SetActive(false);
    }

    public void PlayDeathSequence(Action onComplete)
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathScreenPanel) deathScreenPanel.SetActive(true);

        StartCoroutine(SequenceRoutine(onComplete));
    }

    IEnumerator SequenceRoutine(Action onComplete)
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        
        onComplete?.Invoke();
    }
>>>>>>> upstream/dev
}