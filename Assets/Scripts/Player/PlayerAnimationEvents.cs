<<<<<<< HEAD
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

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            // Apply SFX volume multiplier
            float sfxVolume = AudioManager.Instance != null 
                ? AudioManager.Instance.GetSFXVolume() 
                : PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, volume * sfxVolume);
        }
    }
=======
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

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, volume);
        }
    }
>>>>>>> upstream/dev
}