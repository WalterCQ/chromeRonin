using UnityEngine;

public class ReflexTrigger : MonoBehaviour
{
    public LayerMask dangerLayers;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if the feature is enabled in TimeManager
        if (TimeManager.Instance == null || !TimeManager.Instance.isAutoSlowEnabled) 
            return;

        // 2. Check if the object entering our bubble is dangerous
        if (((1 << other.gameObject.layer) & dangerLayers) != 0)
        {
            // Trigger the slow motion!
            TimeManager.Instance.TriggerAutoSlow();
            
            if (CameraShakeManager.Instance != null)
                CameraShakeManager.Instance.Shake(5f, 0.2f);
        }
    }
}