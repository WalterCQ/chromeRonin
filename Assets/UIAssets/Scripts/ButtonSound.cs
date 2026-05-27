using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Settings")]
    public AudioSource audioSource; 
    public AudioClip hoverSound;    
    public AudioClip clickSound;    
    
    // PlayerPrefs key for SFX volume (must match VolumeSettings)
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const float DEFAULT_VOLUME = 0.75f;
    
    private float GetSFXVolume()
    {
        if (SFXManager.Instance != null)
        {
            return SFXManager.GetVolume();
        }
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, GetSFXVolume());
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, GetSFXVolume());
        }
    }
}