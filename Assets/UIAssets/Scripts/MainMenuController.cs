using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainButtonsPanel; 
    public GameObject keySettingsPanel;  

    void Start()
    {
        GameKeys.LoadKeys();

        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (keySettingsPanel != null) keySettingsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenKeySettings()
    {
        mainButtonsPanel.SetActive(false); // Hide Main Buttons
        keySettingsPanel.SetActive(true);  // Show Rebind Icons
    }

    public void CloseKeySettings()
    {
        keySettingsPanel.SetActive(false); // Hide Rebind Icons
        mainButtonsPanel.SetActive(true);  // Show Main Buttons
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Triggered"); 
        Application.Quit();
    }
}