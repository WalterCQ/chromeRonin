<<<<<<< HEAD
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Pickup, Switch, Talk }
    public InteractionType type;

    [Header("Settings")]
    [Tooltip("If true, triggers immediately on touch (like Coins). If false, requires pressing E (like Chests).")]
    public bool autoInteract = false; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only run this logic if autoInteract is TRUE
        if (autoInteract && other.CompareTag("Player"))
        {
            OnInteract();
        }
    }

    // This is called by PlayerInteraction.cs when pressing E
    // OR called by OnTriggerEnter2D above if autoInteract is true
    public void OnInteract()
    {
        switch (type)
        {
            case InteractionType.Pickup:
                Debug.Log("Picked up item: " + gameObject.name);
                if (GameManager.Instance != null) 
                {
                    GameManager.Instance.AddCoin(1);
                }
                Destroy(gameObject);
                break;
            case InteractionType.Switch:
                Debug.Log("Switch activated!");
                // Add switch logic (e.g., OpenDoor())
                break;
            case InteractionType.Talk:
                Debug.Log("Hello Traveler!");
                // Add dialogue logic
                break;
        }
    }
=======
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Pickup, Switch, Talk }
    public InteractionType type;

    [Header("Settings")]
    [Tooltip("If true, triggers immediately on touch (like Coins). If false, requires pressing E (like Chests).")]
    public bool autoInteract = false; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (autoInteract && other.CompareTag("Player"))
        {
            OnInteract();
        }
    }

    public void OnInteract()
    {
        switch (type)
        {
            case InteractionType.Pickup:
                Debug.Log("Picked up item: " + gameObject.name);
                if (GameManager.Instance != null) 
                {
                    GameManager.Instance.AddCoin(1);
                }
                Destroy(gameObject);
                break;
            case InteractionType.Switch:
                Debug.Log("Switch activated!");
                break;
            case InteractionType.Talk:
                Debug.Log("Hello Traveler!");
                break;
        }
    }
>>>>>>> upstream/dev
}