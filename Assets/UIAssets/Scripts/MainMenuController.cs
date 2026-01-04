<<<<<<< HEAD
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
=======
using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainButtonsPanel; 
    public GameObject keySettingsPanel;
    public GameObject levelSelectPanel; 

    void Start()
    {
        GameKeys.LoadKeys();

        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (keySettingsPanel != null) keySettingsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (keySettingsPanel != null && keySettingsPanel.activeSelf)
            {
                CloseKeySettings();
            }
            else if (levelSelectPanel != null && levelSelectPanel.activeSelf)
            {
                CloseLevelSelect();
            }
        }
    }


    public void ResumeGame()
    {
        FindObjectOfType<GameManager>()?.ContinueGame();
    }

    public void NewGame()
    {
        FindObjectOfType<GameManager>()?.NewGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Triggered"); 
        Application.Quit();
    }


    public void OpenKeySettings()
    {
        mainButtonsPanel.SetActive(false);
        keySettingsPanel.SetActive(true);
    }

    public void CloseKeySettings()
    {
        keySettingsPanel.SetActive(false);
        mainButtonsPanel.SetActive(true);
    }


    public void OpenLevelSelect()
    {
        mainButtonsPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false);
        mainButtonsPanel.SetActive(true);
    }
>>>>>>> upstream/dev
}