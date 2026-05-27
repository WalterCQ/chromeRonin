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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GameKeys.LoadKeys();

        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (keySettingsPanel != null) keySettingsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false); 
    }

    void Update()
    {
        // NEW: Check if we are currently rebinding a key
        // If we are, ignore the Escape key so it only cancels the rebind
        if (RebindText.isRebinding) return;

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

    public void OpenKeySettings()
    {
        mainButtonsPanel.SetActive(false);
        keySettingsPanel.SetActive(true);
    }

    public void CloseKeySettings()
    {
        if (keySettingsPanel != null)
        {
            // Reset any active listeners as a safety measure
            RebindText[] rebinds = keySettingsPanel.GetComponentsInChildren<RebindText>();
            foreach (RebindText rebind in rebinds)
            {
                rebind.StopRebinding();
            }
            keySettingsPanel.SetActive(false);
        }

        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
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

    public void ResumeGame() { FindObjectOfType<GameManager>()?.ContinueGame(); }
    public void NewGame() { FindObjectOfType<GameManager>()?.NewGame(); }
    public void QuitGame() { Application.Quit(); }
}