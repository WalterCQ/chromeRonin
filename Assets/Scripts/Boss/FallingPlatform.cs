using UnityEngine;
using UnityEngine.SceneManagement;

public class FallingPlatform : MonoBehaviour
{
    [Tooltip("Fall speed")]
    public float fallSpeed = 5f;
    
    [Tooltip("Y position to destroy at")]
    public float destroyY = -15f;

    public static bool hasGameStarted = false;
    
    /// <summary>
    /// When true, all platforms stop moving (used when roof reaches target Y)
    /// </summary>
    public static bool isFrozen = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasGameStarted = false;
        isFrozen = false;
    }

    void Update()
    {
        // Stop if game hasn't started OR if platforms are frozen
        if (!hasGameStarted || isFrozen) return;

        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    public static void ResetGameState()
    {
        hasGameStarted = false;
        isFrozen = false;
    }

    /// <summary>
    /// Call this to freeze all platforms in place
    /// </summary>
    public static void FreezeAllPlatforms()
    {
        isFrozen = true;
    }
}