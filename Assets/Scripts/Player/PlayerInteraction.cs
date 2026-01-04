<<<<<<< HEAD
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRadius = 1f;
    public LayerMask interactableLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteraction();
        }
    }
    void CheckInteraction()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);
        if (hit)
        {
            Interactable item = hit.GetComponent<Interactable>();
            if (item != null) item.OnInteract();
        }
    }
=======
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRadius = 1f;
    public LayerMask interactableLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteraction();
        }
    }
    void CheckInteraction()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);
        if (hit)
        {
            Interactable item = hit.GetComponent<Interactable>();
            if (item != null) item.OnInteract();
        }
    }
>>>>>>> upstream/dev
}