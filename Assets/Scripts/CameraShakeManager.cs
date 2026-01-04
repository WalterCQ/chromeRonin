<<<<<<< HEAD
using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }
    private CinemachineImpulseSource _impulseSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ensure this object persists if it's a manager, 
            // or just put it on the player/GameManager
        }
        
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_impulseSource == null)
        {
            _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }
        
        // Default settings for the "Kick"
        // You can tweak these in the Inspector on the ImpulseSource component too
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_AttackTime = 0.1f;
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = 0.1f;
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = 0.5f;
    }

    public void Shake(float intensity)
    {
        // Vector3.one * intensity gives the direction of the shake.
        // You can change this to Vector3.down for a landing thud, etc.
        _impulseSource.GenerateImpulse(Vector3.one * intensity);
    }
=======
using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    private CinemachineVirtualCamera _currentVCam;
    private CinemachineBasicMultiChannelPerlin _noiseComponent;
    
    private Coroutine _shakeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Shake(float intensity, float duration)
    {
        if (_currentVCam == null || _noiseComponent == null || !_currentVCam.gameObject.activeInHierarchy)
        {
            FindCurrentCamera();
        }

        if (_noiseComponent != null)
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);

            _noiseComponent.m_AmplitudeGain = 0f; 

            if (_currentVCam != null)
            {
                _currentVCam.transform.rotation = Quaternion.identity; 
            }

            _shakeCoroutine = StartCoroutine(ProcessShake(intensity, duration));
        }
    }

    IEnumerator ProcessShake(float intensity, float duration)
    {
        _noiseComponent.m_AmplitudeGain = intensity;

        yield return new WaitForSecondsRealtime(duration);

        _noiseComponent.m_AmplitudeGain = 0f;
        
        if (_currentVCam != null)
        {
            _currentVCam.transform.rotation = Quaternion.identity;
        }
    }

    void FindCurrentCamera()
    {
        if (Camera.main != null)
        {
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null && brain.ActiveVirtualCamera != null)
            {
                _currentVCam = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            }
        }

        if (_currentVCam == null)
        {
            _currentVCam = FindObjectOfType<CinemachineVirtualCamera>();
        }

        if (_currentVCam != null)
        {
            _noiseComponent = _currentVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }
>>>>>>> upstream/dev
}