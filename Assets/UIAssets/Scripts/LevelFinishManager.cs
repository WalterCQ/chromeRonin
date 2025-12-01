using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class LevelFinishManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject finishPanel;      
    public TextMeshProUGUI timeText;    
    public TextMeshProUGUI deathText;   
    public TextMeshProUGUI gradeText;   

    [Header("Grading Criteria (For Rank A)")]
    public float targetTime = 60f;      // Finish under 60 seconds for good rank
    public int maxDeaths = 3;           // Less than 3 deaths for good rank

    void Start()
    {
        finishPanel.SetActive(false); // Hide it at start
    }
    
    // Call this function when player hits the Exit Door
    public void LevelComplete(float finalTime, int deathCount)
    {
        finishPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game

        // 1. Display Stats
        timeText.text = "TIME: " + finalTime.ToString("F2") + "s";
        deathText.text = "DEATHS: " + deathCount;

        // 2. Calculate Rank Logic
        string rank = "C"; // Default
        
        if (finalTime <= targetTime && deathCount == 0) 
            rank = "S"; // Perfect Run
        else if (finalTime <= targetTime && deathCount <= maxDeaths) 
            rank = "A"; // Great Run
        else if (finalTime <= targetTime * 1.5f) 
            rank = "B"; // Okay Run

        gradeText.text = "RANK: " + rank;
    }

    // we use it after completing each level
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        // Loads the next scene in the Build Settings list
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}