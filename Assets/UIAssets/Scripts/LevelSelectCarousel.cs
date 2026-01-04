<<<<<<< HEAD
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectCarousel : MonoBehaviour
{
    [System.Serializable]
    public class LevelItem
    {
        public string sceneName; 
        public Sprite previewImage;
        public Sprite levelButtonImage;
    }

    [Header("Level Data")]
    public LevelItem[] levels; 

    [Header("UI References")]
    public Image displayImage;
    public Image levelButtonDisplay;
    public Button playButton;    
    public Button leftArrow;         
    public Button rightArrow;        

    private int currentIndex = 0;

    void OnEnable()
    {
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        int targetIndex = levelReached - 1;

        if (targetIndex >= levels.Length) targetIndex = levels.Length - 1;
        currentIndex = targetIndex;

        UpdateUI();
    }

    public void NextLevel()
    {
        if (currentIndex < levels.Length - 1)
        {
            currentIndex++;
            UpdateUI();
        }
    }

    public void PrevLevel()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    public void PlayCurrentLevel()
    {
        SceneManager.LoadScene(levels[currentIndex].sceneName);
    }

    private void UpdateUI()
    {
        displayImage.sprite = levels[currentIndex].previewImage;
        
        if (levelButtonDisplay != null && levels[currentIndex].levelButtonImage != null)
            levelButtonDisplay.sprite = levels[currentIndex].levelButtonImage;

        leftArrow.gameObject.SetActive(currentIndex > 0);
        rightArrow.gameObject.SetActive(currentIndex < levels.Length - 1);

        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        int levelNum = currentIndex + 1; 

        if (levelNum > levelReached)
        {
            playButton.interactable = false;
            displayImage.color = Color.gray;
            if (levelButtonDisplay != null) levelButtonDisplay.color = Color.gray;
        }
        else
        {
            playButton.interactable = true;
            displayImage.color = Color.white;
            if (levelButtonDisplay != null) levelButtonDisplay.color = Color.white;
        }
    }
=======
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectCarousel : MonoBehaviour
{
    [System.Serializable]
    public class LevelItem
    {
        public string sceneName; 
        public Sprite previewImage;
        public Sprite levelButtonImage;
    }

    [Header("Level Data")]
    public LevelItem[] levels; 

    [Header("UI References")]
    public Image displayImage;
    public Image levelButtonDisplay;
    public Button playButton;    
    public Button leftArrow;         
    public Button rightArrow;        

    private int currentIndex = 0;

    void OnEnable()
    {
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        int targetIndex = levelReached - 1;

        if (targetIndex >= levels.Length) targetIndex = levels.Length - 1;
        currentIndex = targetIndex;

        UpdateUI();
    }

    public void NextLevel()
    {
        if (currentIndex < levels.Length - 1)
        {
            currentIndex++;
            UpdateUI();
        }
    }

    public void PrevLevel()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    public void PlayCurrentLevel()
    {
        SceneManager.LoadScene(levels[currentIndex].sceneName);
    }

    private void UpdateUI()
    {
        displayImage.sprite = levels[currentIndex].previewImage;
        
        if (levelButtonDisplay != null && levels[currentIndex].levelButtonImage != null)
            levelButtonDisplay.sprite = levels[currentIndex].levelButtonImage;

        leftArrow.gameObject.SetActive(currentIndex > 0);
        rightArrow.gameObject.SetActive(currentIndex < levels.Length - 1);

        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        int levelNum = currentIndex + 1; 

        if (levelNum > levelReached)
        {
            playButton.interactable = false;
            displayImage.color = Color.gray;
            if (levelButtonDisplay != null) levelButtonDisplay.color = Color.gray;
        }
        else
        {
            playButton.interactable = true;
            displayImage.color = Color.white;
            if (levelButtonDisplay != null) levelButtonDisplay.color = Color.white;
        }
    }
>>>>>>> upstream/dev
}