<<<<<<< HEAD
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGamePaused = false;
    public bool isGameOver = false;

    [Header("Resources")]
    public GameObject playerPrefab;
    public int coins = 0;

    [Header("Persistence")]
    public int playerCurrentHealth;
    public int spawnID = 0;

    [Header("Settings")]
    public float deathRestartDelay = 2f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandlePauseInput();
        }
    }
    void HandlePauseInput()
    {
        if (RebindButton.isRebinding) return;

        PauseManager pm = FindObjectOfType<PauseManager>();
        if (pm != null && pm.keySettingsPanel.activeSelf)
        {
            pm.CloseKeySettings();
            return;
        }
        TogglePause();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null && playerPrefab != null)
        {
            player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                if (playerCurrentHealth <= 0) playerCurrentHealth = playerHealth.maxHealth;
                
                playerHealth.SetHealth(playerCurrentHealth);
            }

            LevelEntrance[] entrances = FindObjectsOfType<LevelEntrance>();
            foreach (LevelEntrance entrance in entrances)
            {
                if (entrance.entranceID == spawnID)
                {
                    player.transform.position = entrance.transform.position;
                    
                    CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
                    if (vcam != null)
                    {
                        vcam.Follow = player.transform;
                        vcam.OnTargetObjectWarped(player.transform, player.transform.position - vcam.transform.position); 
                    }
                    break;
                }
            }
        }
    }

    public void LoadLevel(string sceneName, int targetID)
    {
        spawnID = targetID;
        SceneManager.LoadScene(sceneName);
    }

    public void UpdatePlayerHealth(int newHealth)
    {
        playerCurrentHealth = newHealth;
    }

    public void AddCoin(int amount)
    {
        coins += amount;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        GameOverManager deathScreen = FindObjectOfType<GameOverManager>();
        
        if (deathScreen != null)
        {
            deathScreen.PlayDeathSequence(OnDeathSequenceFinished); 
        }
        else
        {
            Debug.LogWarning("GameOverManager not found! Using fallback restart.");
            StartCoroutine(FallbackRestartRoutine());
        }
    }

    void OnDeathSequenceFinished()
    {
        isGameOver = false; 
        Time.timeScale = 1f;

        coins = 0;
        
        if (TimeManager.Instance != null) 
        {
            TimeManager.Instance.ResetEnergy();
        }

        if (playerPrefab != null)
        {
            Health h = playerPrefab.GetComponent<Health>();
            if (h != null) playerCurrentHealth = h.maxHealth;
            else playerCurrentHealth = 5; 
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public IEnumerator FallbackRestartRoutine()
    {
        yield return new WaitForSeconds(deathRestartDelay);
        OnDeathSequenceFinished();
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;

        PauseManager pm = FindObjectOfType<PauseManager>();
        if (pm != null)
        {
            pm.SetPauseState(isGamePaused);
        }
    }
=======
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGamePaused = false;
    public bool isGameOver = false;

    [Header("Resources")]
    public GameObject playerPrefab;
    public int coins = 0;

    [Header("Persistence")]
    public int playerCurrentHealth;
    public int spawnID = 0;

    [Header("Settings")]
    public float deathRestartDelay = 2f;
    [Tooltip("Name of the Scene for the Main Menu")]
    public string mainMenuSceneName = "MainMenu";

    // Internal flag to track if we should heal the player or keep damage
    private bool shouldPreserveStats = false; 
    private bool isResumingFromSave = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            HandlePauseInput();
    }

    void HandlePauseInput()
    {
        PauseManager pm = FindObjectOfType<PauseManager>();
        if (pm != null && pm.keySettingsPanel.activeSelf)
        {
            pm.CloseKeySettings();
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;

        PauseManager pm = FindObjectOfType<PauseManager>();
        if (pm != null)
            pm.SetPauseState(isGamePaused);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        if (scene.name == mainMenuSceneName)
            return;

        // --- STATS LOGIC ---
        if (!shouldPreserveStats)
        {
            // CASE A: We are resetting (Level Transition, Resume, New Game)
            // 1. Reset Chrono Bar
            if (TimeManager.Instance != null) TimeManager.Instance.ResetEnergy();
            
            // 2. Set HP flag to -1 (Means "Give me Max Health" in Spawn routine)
            playerCurrentHealth = -1; 
        }
        else
        {
            // CASE B: Room Transition
            // We do nothing. We keep the 'playerCurrentHealth' value we have in memory.
            // We do not reset TimeManager.
        }

        SpawnAndRestorePlayer();

        // Checkpointing
        if (!isResumingFromSave)
        {
            PlayerPrefs.SetString("SavedLevel", scene.name);
            PlayerPrefs.SetInt("SavedSpawnID", spawnID);
            // We always save the health we currently have (whether it was just maxed or preserved)
            PlayerPrefs.SetInt("SavedHealth", playerCurrentHealth);
            PlayerPrefs.Save();
        }

        isResumingFromSave = false;
        shouldPreserveStats = false; // Always reset flag to "Fresh" for safety
    }

    void SpawnAndRestorePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null && playerPrefab != null)
            player = Instantiate(playerPrefab);

        if (player == null)
            return;

        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            // If flag is -1, it means we want a Fresh Start (Max HP)
            if (playerCurrentHealth == -1) 
            {
                playerCurrentHealth = health.maxHealth;
            }

            health.SetHealth(playerCurrentHealth);
        }

        LevelEntrance[] entrances = FindObjectsOfType<LevelEntrance>();
        foreach (LevelEntrance entrance in entrances)
        {
            if (entrance.entranceID == spawnID)
            {
                player.transform.position = entrance.transform.position;

                CinemachineVirtualCamera vcam =
                    FindObjectOfType<CinemachineVirtualCamera>();

                if (vcam != null)
                {
                    vcam.Follow = player.transform;
                    vcam.OnTargetObjectWarped(
                        player.transform,
                        player.transform.position - vcam.transform.position
                    );
                }
                break;
            }
        }
    }

    // --- UPDATED LOAD LEVEL ---
    // Added 'resetStats' parameter. Defaults to TRUE (Fresh Start)
    public void LoadLevel(string sceneName, int targetID, bool resetStats = true)
    {
        spawnID = targetID;
        // If we are resetting stats (Level Change), we do NOT preserve.
        // If we are NOT resetting stats (Room Change), we DO preserve.
        shouldPreserveStats = !resetStats; 
        
        SceneManager.LoadScene(sceneName);
    }

    public void ContinueGame()
    {
        if (!PlayerPrefs.HasKey("SavedLevel"))
        {
            NewGame();
            return;
        }

        Time.timeScale = 1f;
        isResumingFromSave = true;
        shouldPreserveStats = false; // RESUME = FRESH START (Max HP)

        spawnID = PlayerPrefs.GetInt("SavedSpawnID");
        // Note: We ignore "SavedHealth" from PlayerPrefs to ensure fair restart
        
        SceneManager.LoadScene(PlayerPrefs.GetString("SavedLevel"));
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();

        coins = 0;
        spawnID = 0;
        shouldPreserveStats = false; // FRESH START

        SceneManager.LoadScene(1);
    }

    public void UpdatePlayerHealth(int newHealth)
    {
        playerCurrentHealth = newHealth;
    }

    public void AddCoin(int amount)
    {
        coins += amount;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        GameOverManager deathScreen = FindObjectOfType<GameOverManager>();
        if (deathScreen != null)
            deathScreen.PlayDeathSequence(OnDeathSequenceFinished);
        else
            StartCoroutine(FallbackRestartRoutine());
    }

    void OnDeathSequenceFinished()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        coins = 0;
        shouldPreserveStats = false; // Death = Full Restart = Max HP

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator FallbackRestartRoutine()
    {
        yield return new WaitForSeconds(deathRestartDelay);
        OnDeathSequenceFinished();
    }

    public void CompleteLevel(int levelIndex)
    {
        int currentLevelReached = PlayerPrefs.GetInt("levelReached", 1);

        if (levelIndex + 1 > currentLevelReached)
        {
            PlayerPrefs.SetInt("levelReached", levelIndex + 1);
            PlayerPrefs.Save();
        }
    }
>>>>>>> upstream/dev
}