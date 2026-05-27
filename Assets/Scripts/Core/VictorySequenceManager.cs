using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the victory sequence after boss is defeated:
/// 1. Play victory video
/// 2. Show developer credits
/// 3. Wait for user click to return to main menu
/// </summary>
public class VictorySequenceManager : MonoBehaviour
{
    [Header("--- Video Settings ---")]
    [Tooltip("VideoPlayer component for playing victory video (optional)")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("RawImage to display the video")]
    public RawImage videoDisplay;
    
    [Tooltip("Video clip to play (optional - if null, skip to credits)")]
    public VideoClip victoryVideoClip;

    [Header("--- Credits Settings ---")]
    [Tooltip("Credits panel GameObject")]
    public GameObject creditsPanel;
    
    [Tooltip("Text showing 'Click anywhere to continue' hint")]
    public GameObject clickToContinueHint;

    [Header("--- UI to Hide ---")]
    [Tooltip("Game UI elements to hide during victory sequence (drag in HUD, health bar, etc)")]
    public GameObject[] gameUIToHide;

    [Header("--- Audio ---")]
    [Tooltip("Music to play during video and credits (uses MusicManager)")]
    public AudioClip victoryMusic;

    [Header("--- Scene Transition ---")]
    [Tooltip("Scene to load after credits (default: MainMenu)")]
    public string menuSceneName = "MainMenu";

    private bool _isPlayingVideo = false;
    private bool _waitingForClick = false;
    private bool _sequenceActive = false;
    private bool _videoFinished = false;

    void Start()
    {
        // Hide UI elements at start
        if (videoDisplay != null) videoDisplay.gameObject.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (clickToContinueHint != null) clickToContinueHint.SetActive(false);
    }

    void HideGameUI()
    {
        if (gameUIToHide == null) return;
        foreach (var ui in gameUIToHide)
        {
            if (ui != null) ui.SetActive(false);
        }
    }

    void Update()
    {
        if (!_sequenceActive) return;

        // During video: click to skip
        if (_isPlayingVideo && Input.GetMouseButtonDown(0))
        {
            SkipVideo();
        }
        
        // During credits waiting: click to return to menu
        if (_waitingForClick && Input.GetMouseButtonDown(0))
        {
            ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Called when boss dies
    /// </summary>
    public void StartVictorySequence()
    {
        if (_sequenceActive) return;
        
        _sequenceActive = true;
        Debug.Log("[Victory] Starting victory sequence!");

        // --- NEW: LOCK PLAYER INPUT ---
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetInputLock(true); // Disables movement and triggers cutscene invincibility
            }
        }
        // ------------------------------
        
        // Mark game as completed
        PlayerPrefs.SetInt("GameCompleted", 1);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel(5);
        }
        PlayerPrefs.Save();

        // Switch to victory music
        if (victoryMusic != null && MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(victoryMusic);
        }

        // IMPORTANT: Keep time running so video plays!
        Time.timeScale = 1f;

        // Hide game UI (health bar, HUD, etc)
        HideGameUI();

        StartCoroutine(VictorySequenceRoutine());
    }

    IEnumerator VictorySequenceRoutine()
    {
        // --- PHASE 1: VIDEO ---
        if (victoryVideoClip != null && videoPlayer != null)
        {
            yield return StartCoroutine(PlayVideoRoutine());
        }

        // --- PHASE 2: CREDITS ---
        yield return StartCoroutine(ShowCreditsRoutine());

        // --- PHASE 3: WAIT FOR USER CLICK ---
        // (ReturnToMainMenu is called from Update when user clicks)
    }

    IEnumerator PlayVideoRoutine()
    {
        _isPlayingVideo = true;
        _videoFinished = false;
        
        Debug.Log("[Victory] Starting video playback...");
        
        // Setup video display
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            
            // Create render texture if needed
            if (videoPlayer.targetTexture == null)
            {
                videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
            }
            videoDisplay.texture = videoPlayer.targetTexture;
        }

        // Setup video player
        videoPlayer.clip = victoryVideoClip;
        videoPlayer.playOnAwake = false;
        
        // Register callback for video end
        videoPlayer.loopPointReached += OnVideoFinished;
        
        // IMPORTANT: Prepare the video first, then wait until it's ready
        videoPlayer.Prepare();
        
        Debug.Log("[Victory] Preparing video...");
        
        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        Debug.Log("[Victory] Video prepared, starting playback...");
        
        // Now play
        videoPlayer.Play();
        
        // Wait one frame to ensure playback started
        yield return null;
        
        Debug.Log("[Victory] Video is playing: " + videoPlayer.isPlaying);

        // Wait for video to finish
        while (!_videoFinished && _isPlayingVideo)
        {
            yield return null;
        }

        CleanupVideo();
        _isPlayingVideo = false;
        
        Debug.Log("[Victory] Video finished.");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        _videoFinished = true;
        vp.loopPointReached -= OnVideoFinished;
    }

    void SkipVideo()
    {
        Debug.Log("[Victory] Skipping video...");
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
        _videoFinished = true;
        CleanupVideo();
        _isPlayingVideo = false;
    }

    void CleanupVideo()
    {
        if (videoDisplay != null) videoDisplay.gameObject.SetActive(false);
    }

    IEnumerator ShowCreditsRoutine()
    {
        Debug.Log("[Victory] Showing credits...");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }

        // Wait a moment before showing the click hint
        yield return new WaitForSeconds(2f);

        // Show click to continue hint
        if (clickToContinueHint != null)
        {
            clickToContinueHint.SetActive(true);
        }

        _waitingForClick = true;
        
        Debug.Log("[Victory] Waiting for user click to continue...");
    }

    void ReturnToMainMenu()
    {
        Debug.Log("[Victory] Returning to main menu...");
        
        _waitingForClick = false;
        _sequenceActive = false;

        // Cleanup
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        if (clickToContinueHint != null)
        {
            clickToContinueHint.SetActive(false);
        }

        // Optional: Do not stop music here if you want it to continue into the menu 
        // OR stop it if the menu has its own setter. 
        // For consistency with Level exit, we can stop or fade.
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // Editor test methods
    [ContextMenu("Test Victory Sequence")]
    void TestVictorySequence()
    {
        StartVictorySequence();
    }
}
