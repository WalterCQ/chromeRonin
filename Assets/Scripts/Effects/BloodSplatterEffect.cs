using UnityEngine;

/// <summary>
/// A smooth blood splatter effect controller that creates polished particle effects on enemy death.
/// Attach this to a prefab or use it to dynamically spawn realistic blood effects.
/// Enhanced with Stretched Billboard rendering, velocity damping, and realistic physics.
/// </summary>
public class BloodSplatterEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [Tooltip("Main blood particle system")]
    public ParticleSystem mainBloodParticles;
    
    [Tooltip("Secondary mist/spray particles for smoother look")]
    public ParticleSystem bloodMistParticles;
    
    [Tooltip("Blood droplet trails for added realism")]
    public ParticleSystem bloodDropletParticles;

    [Header("Effect Configuration")]
    [Tooltip("Duration before effect auto-destroys")]
    public float effectDuration = 2f;
    
    [Tooltip("Scale of the overall effect")]
    public float effectScale = 1f;
    
    [Tooltip("Direction override for blood spray (-1 = auto from attack)")]
    public Vector2 sprayDirection = Vector2.zero;

    [Header("Particle Physics")]
    [Tooltip("Min and max start speed for blood particles")]
    public Vector2 speedRange = new Vector2(5f, 10f);
    
    [Tooltip("Min and max start size for blood particles")]
    public Vector2 sizeRange = new Vector2(0.1f, 0.5f);
    
    [Tooltip("Gravity modifier for realistic blood arcs (higher = falls faster)")]
    public float gravityModifier = 3.5f;
    
    [Tooltip("Velocity dampen value (0.1-0.2) to simulate air resistance")]
    [Range(0.05f, 0.3f)]
    public float velocityDampen = 0.15f;
    
    [Tooltip("Length scale for stretched billboard effect (creates directional streaks)")]
    public float lengthScale = 2.0f;

    [Header("Colors")]
    [Tooltip("Fresh blood color (#B20000)")]
    public Color primaryBloodColor = new Color(0.698f, 0f, 0f, 1f);
    
    [Tooltip("Oxidized/dried blood color (#4D0505)")]
    public Color secondaryBloodColor = new Color(0.302f, 0.02f, 0.02f, 1f);
    
    public Color mistColor = new Color(0.5f, 0.0f, 0.0f, 0.4f);

    private void Start()
    {
        // Auto-destroy after effect duration
        Destroy(gameObject, effectDuration);
        
        // Apply scale
        transform.localScale = Vector3.one * effectScale;
        
        // Configure and play all particle systems
        ConfigureParticleSystems();
        PlayAllEffects();
    }

    private void ConfigureParticleSystems()
    {
        if (mainBloodParticles != null)
        {
            ConfigureMainBlood();
        }
        
        if (bloodMistParticles != null)
        {
            ConfigureMist();
        }
        
        if (bloodDropletParticles != null)
        {
            ConfigureDroplets();
        }
    }

    private void ConfigureMainBlood()
    {
        var main = mainBloodParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(primaryBloodColor, secondaryBloodColor);
        
        // Random start speed for variety (technical spec: 5-10)
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedRange.x, speedRange.y);
        
        // Random start size for variety (technical spec: 0.1-0.5)
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        
        // Gravity modifier for realistic blood arcs (technical spec: 1.5-3.0)
        main.gravityModifier = gravityModifier;
        
        // Configure renderer for stretched billboard effect (directional streaks)
        var renderer = mainBloodParticles.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = lengthScale;
            renderer.velocityScale = 0f; // Use lengthScale only
        }
        
        // Configure Limit Velocity over Lifetime for air resistance simulation
        var limitVelocity = mainBloodParticles.limitVelocityOverLifetime;
        limitVelocity.enabled = true;
        limitVelocity.dampen = velocityDampen;
        limitVelocity.separateAxes = false;
        limitVelocity.limit = new ParticleSystem.MinMaxCurve(15f); // Maximum velocity limit
        
        // Apply spray direction if specified
        if (sprayDirection != Vector2.zero)
        {
            var shape = mainBloodParticles.shape;
            float angle = Mathf.Atan2(sprayDirection.y, sprayDirection.x) * Mathf.Rad2Deg;
            shape.rotation = new Vector3(-90f, angle, 0f);
        }
        
        // Configure color over lifetime - from bright red (#B20000) to dark brownish-red (#4D0505)
        var colorOverLifetime = mainBloodParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(primaryBloodColor, 0f),
                new GradientColorKey(secondaryBloodColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        
        // Size over lifetime for more organic feel
        var sizeOverLifetime = mainBloodParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.1f, 1f);
        sizeCurve.AddKey(0.5f, 0.8f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }

    private void ConfigureMist()
    {
        var main = bloodMistParticles.main;
        main.startColor = mistColor;
        
        var colorOverLifetime = bloodMistParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(mistColor, 0f),
                new GradientColorKey(mistColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.5f, 0.15f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private void ConfigureDroplets()
    {
        var main = bloodDropletParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(primaryBloodColor, secondaryBloodColor);
        main.gravityModifier = 1.5f;
        
        var colorOverLifetime = bloodDropletParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(primaryBloodColor, 0f),
                new GradientColorKey(secondaryBloodColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.7f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private void PlayAllEffects()
    {
        if (mainBloodParticles != null) mainBloodParticles.Play();
        if (bloodMistParticles != null) bloodMistParticles.Play();
        if (bloodDropletParticles != null) bloodDropletParticles.Play();
    }

    /// <summary>
    /// Static factory method to spawn a blood effect at a position with optional direction.
    /// </summary>
    public static BloodSplatterEffect SpawnAt(Vector3 position, Vector2 direction, GameObject prefab)
    {
        if (prefab == null) return null;
        
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        BloodSplatterEffect effect = instance.GetComponent<BloodSplatterEffect>();
        
        if (effect != null && direction != Vector2.zero)
        {
            effect.sprayDirection = direction;
        }
        
        return effect;
    }
}
