using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Action Sounds")]
    public AudioClip runSound;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip attackSound;
    public AudioClip slideSound;
    public AudioClip shieldSound;
    public AudioClip climbSound;

    // PlayerPrefs key for SFX volume (must match VolumeSettings)
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const float DEFAULT_VOLUME = 0.75f;

    private float lastDashTime; 

    public void PlayRun()    => PlaySound(runSound);
    public void PlayJump()   => PlaySound(jumpSound);
    
    public void PlayDash()
    {
        if (Time.time - lastDashTime > 0.5f)
        {
            PlaySound(dashSound);
            lastDashTime = Time.time;
        }
    }

    public void PlayAttack() => PlaySound(attackSound);
    public void PlaySlide()  => PlaySound(slideSound);
    public void PlayShield() => PlaySound(shieldSound, 0.1f);
    public void PlayClimb()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            PlaySound(climbSound);
        }
    }

    private float GetSFXVolume()
    {
        if (SFXManager.Instance != null)
        {
            return SFXManager.GetVolume();
        }
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }

    private void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, GetSFXVolume() * volumeMultiplier);
        }
    }
}