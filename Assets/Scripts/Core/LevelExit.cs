using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [Header("Transition")]
    public string targetSceneName;
    public int targetSpawnID; // 0, 1, 2... matches LevelEntrance ID

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadLevel(targetSceneName, targetSpawnID);
            }
            else
            {
                Debug.LogError("No Game Manager found to load level!");
            }
        }
    }
}