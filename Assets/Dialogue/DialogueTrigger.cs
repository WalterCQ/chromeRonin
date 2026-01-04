using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueData; 
    public bool triggerOnce = true; 
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            if (triggerOnce && hasTriggered) return;
            
            if (SmartIslandController.Instance != null)
            {
                SmartIslandController.Instance.PlayDialogue(dialogueData);
                hasTriggered = true;
            }
        }
        
    }
}