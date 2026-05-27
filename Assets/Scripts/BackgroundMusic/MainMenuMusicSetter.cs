using UnityEngine;

/// <summary>
/// Main Menu Music Setter - plays different music based on player progress
/// </summary>
public class MainMenuMusicSetter : MonoBehaviour
{
    [Header("Main Menu Music")]
    [Tooltip("Default main menu music (plays before completion)")]
    public AudioClip defaultMenuMusic;
    
    [Tooltip("Main menu music after game completion")]
    public AudioClip completedGameMusic;
    
    [Header("Advanced Settings (Optional)")]
    [Tooltip("Whether to play different music based on levels completed")]
    public bool usePerLevelMusic = false;
    
    [Tooltip("Music for each level (index 0 = after level 1, index 1 = after level 2...)")]
    public AudioClip[] levelCompletedMusic;
    
    // Total number of levels (based on your game settings)
    private const int TOTAL_LEVELS = 5;

    private void Start()
    {
        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("MusicManager.Instance is null!");
            return;
        }

        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        AudioClip musicToPlay = SelectMusicByProgress(levelReached);
        
        if (musicToPlay != null)
        {
            MusicManager.Instance.PlayMusic(musicToPlay);
        }
    }

    /// <summary>
    /// Select music to play based on player progress
    /// </summary>
    private AudioClip SelectMusicByProgress(int levelReached)
    {
        // If per-level music is enabled
        if (usePerLevelMusic && levelCompletedMusic != null && levelCompletedMusic.Length > 0)
        {
            // levelReached = 1 means no levels completed, use default music
            // levelReached = 2 means level 1 completed, use levelCompletedMusic[0]
            // levelReached = 3 means level 2 completed, use levelCompletedMusic[1]
            // and so on...
            
            int musicIndex = levelReached - 2; // levelReached 2 corresponds to index 0
            
            if (musicIndex >= 0 && musicIndex < levelCompletedMusic.Length)
            {
                return levelCompletedMusic[musicIndex];
            }
        }
        
        // Simple mode: only differentiate between completed and not completed
        bool hasCompletedGame = levelReached > TOTAL_LEVELS;
        
        if (hasCompletedGame && completedGameMusic != null)
        {
            return completedGameMusic;
        }
        
        return defaultMenuMusic;
    }
}
