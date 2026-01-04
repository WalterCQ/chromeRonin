<<<<<<< HEAD
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
=======
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Transition")]
    public string targetSceneName;
    public int targetSpawnID;

    [Header("Level Completion")]
    [Tooltip("Check this if this exit leads to a NEW level (not just another room in the same level)")]
    public bool completesLevel = false;
    [Tooltip("The level number that gets completed (e.g., 1 for Level 1). Only used if completesLevel is true.")]
    public int levelToComplete = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                if (completesLevel)
                {
                    GameManager.Instance.CompleteLevel(levelToComplete);
                }
                GameManager.Instance.LoadLevel(targetSceneName, targetSpawnID, completesLevel);
            }
            else
            {
                Debug.LogError("No Game Manager found to load level!");
            }
        }
    }
>>>>>>> upstream/dev
}