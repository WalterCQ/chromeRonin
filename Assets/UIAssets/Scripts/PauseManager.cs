using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject pauseMenuPanel;
    public GameObject hudPanel;
    public GameObject keySettingsPanel;  

    public void SetPauseState(bool isPaused)
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(isPaused);
        
        // Hide HUD while paused for a cleaner look
        if (hudPanel) hudPanel.SetActive(!isPaused); 
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
        pauseMenuPanel.SetActive(false); // Hide Main Buttons
        keySettingsPanel.SetActive(true);  // Show Rebind Icons
    }

    public void CloseKeySettings()
    {
        keySettingsPanel.SetActive(false); // Hide Rebind Icons
        pauseMenuPanel.SetActive(true);  // Show Main Buttons
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}