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
}