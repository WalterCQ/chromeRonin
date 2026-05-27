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

    [Header("Kill Rewards")]
    public int killsToHeal = 3;
    private int _currentKillCount = 0;

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

    // Cached component references for performance
    private PauseManager _cachedPauseManager;
    private GameOverManager _cachedGameOverManager;

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

        // Physics safety: prevents tunnelling through walls on frame rate drops or Alt+Tab
        Time.maximumDeltaTime = 0.1f; 
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

        if (RebindText.isRebinding) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            HandlePauseInput();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isGamePaused && SceneManager.GetActiveScene().name != mainMenuSceneName)
        {
            TogglePause();
        }
    }

    void HandlePauseInput()
    {
        if (_cachedPauseManager != null && _cachedPauseManager.keySettingsPanel.activeSelf)
        {
            _cachedPauseManager.CloseKeySettings();
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;

        if (_cachedPauseManager != null)
            _cachedPauseManager.SetPauseState(isGamePaused);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        _cachedPauseManager = FindObjectOfType<PauseManager>();
        _cachedGameOverManager = FindObjectOfType<GameOverManager>();

        if (scene.name == mainMenuSceneName)
            return;

        if (!shouldPreserveStats)
        {
            if (TimeManager.Instance != null) TimeManager.Instance.ResetEnergy();
            playerCurrentHealth = -1; 
            _currentKillCount = 0; // Reset kill count on fresh level
        }

        SpawnAndRestorePlayer();

        if (!isResumingFromSave)
        {
            PlayerPrefs.SetString("SavedLevel", scene.name);
            PlayerPrefs.SetInt("SavedSpawnID", spawnID);
            PlayerPrefs.SetInt("SavedHealth", playerCurrentHealth);
            PlayerPrefs.Save();
        }

        isResumingFromSave = false;
        shouldPreserveStats = false; 
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

                CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();

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

    public void RegisterEnemyKill()
    {
        _currentKillCount++;
        
        if (_currentKillCount >= killsToHeal)
        {
            _currentKillCount = 0;
            HealPlayer(1);
        }
    }

    public void HealPlayer(int amount)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health != null && health.CurrentHealth < health.maxHealth)
            {
                int newHealth = Mathf.Min(health.CurrentHealth + amount, health.maxHealth);
                health.SetHealth(newHealth);
                UpdatePlayerHealth(newHealth);
                Debug.Log("Regenerated 1 HP from Kills!");
            }
        }
    }

    public void LoadLevel(string sceneName, int targetID, bool resetStats = true)
    {
        spawnID = targetID;
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
        shouldPreserveStats = false; 

        spawnID = PlayerPrefs.GetInt("SavedSpawnID");
        SceneManager.LoadScene(PlayerPrefs.GetString("SavedLevel"));
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        coins = 0;
        spawnID = 0;
        shouldPreserveStats = false; 
        SceneManager.LoadScene(1);
    }

    public event System.Action<int> OnPlayerHealthChanged;
    public event System.Action<int> OnCoinChanged;

    public void UpdatePlayerHealth(int newHealth)
    {
        playerCurrentHealth = newHealth;
        OnPlayerHealthChanged?.Invoke(playerCurrentHealth);
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        OnCoinChanged?.Invoke(coins);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (_cachedGameOverManager != null)
            _cachedGameOverManager.PlayDeathSequence(OnDeathSequenceFinished);
        else
            StartCoroutine(FallbackRestartRoutine());
    }

    void OnDeathSequenceFinished()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        coins = 0;
        shouldPreserveStats = false; 
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
}