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
}