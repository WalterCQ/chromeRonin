using UnityEngine;

/// <summary>
/// Simple component to force particle system physics settings at runtime.
/// Attach this to any particle system prefab to make blood fall realistically.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class BloodParticlePhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    [Tooltip("Gravity strength - higher values make blood fall faster")]
    public float gravityModifier = 4f;
    
    [Tooltip("Simulation space - World makes particles fall correctly")]
    public ParticleSystemSimulationSpace simulationSpace = ParticleSystemSimulationSpace.World;
    
    [Header("Lifetime")]
    [Tooltip("How long particles live before disappearing")]
    public float lifetime = 1.5f;
    
    [Tooltip("Auto-destroy this GameObject after this many seconds")]
    public float destroyAfter = 3f;

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        
        // Stop all particle systems first to allow modifying duration
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        // Apply to main particle system
        ApplySettingsToSystem(ps);
        
        // Also apply to all child particle systems
        ParticleSystem[] childSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (var childPs in childSystems)
        {
            childPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ApplySettingsToSystem(childPs);
        }
        
        // Restart particle systems
        ps.Play(true);
        
        // Force destroy after set time
        Invoke(nameof(ForceDestroy), destroyAfter);
    }
    
    private void ApplySettingsToSystem(ParticleSystem targetPs)
    {
        if (targetPs == null) return;
        
        var main = targetPs.main;
        
        // Force world space so gravity works correctly
        main.simulationSpace = simulationSpace;
        
        // Apply gravity
        main.gravityModifier = gravityModifier;
        
        // Set lifetime
        main.startLifetime = lifetime;
        
        // Disable looping so particles stop spawning
        main.loop = false;
        
        // Set duration to match lifetime
        main.duration = lifetime;
        
        // Set stop action to destroy when finished
        main.stopAction = ParticleSystemStopAction.Destroy;
    }
    
    private void ForceDestroy()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        CancelInvoke();
    }
}
