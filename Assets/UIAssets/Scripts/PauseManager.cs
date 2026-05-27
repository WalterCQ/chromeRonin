using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject pauseMenuPanel;
    public GameObject hudPanel;
    public GameObject keySettingsPanel;  

    void Update()
    {
        // NEW: If the player is currently typing a new key, 
        // prevent the Escape key from closing the menu immediately.
        if (RebindText.isRebinding) return;
    }

    public void SetPauseState(bool isPaused)
    {
        if (pauseMenuPanel) 
        {
            pauseMenuPanel.SetActive(isPaused);
            
            CanvasGroup group = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;            
                group.blocksRaycasts = true; 
                group.interactable = true;   
            }
        }
        if (hudPanel) hudPanel.SetActive(!isPaused); 

        // Safety: If we unpause the game, make sure we aren't left in a "rebinding" state
        if (!isPaused)
        {
            ForceResetAllRebinds();
        }
    }

    public void ResumeButton()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.TogglePause();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); 
    }

    public void OpenKeySettings()
    {
        pauseMenuPanel.SetActive(false); 
        keySettingsPanel.SetActive(true);  
    }

    public void CloseKeySettings()
    {
        // NEW: Cleanup rebinding state so buttons aren't locked next time
        ForceResetAllRebinds();

        keySettingsPanel.SetActive(false); 
        pauseMenuPanel.SetActive(true);  
    }

    private void ForceResetAllRebinds()
    {
        if (keySettingsPanel != null)
        {
            RebindText[] rebinds = keySettingsPanel.GetComponentsInChildren<RebindText>();
            foreach (RebindText rebind in rebinds)
            {
                rebind.StopRebinding();
            }
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}