using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    private bool hasTriggeredStart = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggeredStart) return;

        if (collision.gameObject.CompareTag("GamePlatform"))
        {
            if (transform.position.y > collision.transform.position.y)
            {
                TriggerTheFloor();
            }
        }
    }

    void TriggerTheFloor()
    {
        StartingFloor floor = FindObjectOfType<StartingFloor>();

        if (floor != null)
        {
            floor.ActivateFall();
            hasTriggeredStart = true;
            FallingPlatform.hasGameStarted = true;
        }
    }
}