using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSystem : MonoBehaviour
{
    [Header("--- Core Settings ---")]
    public CanvasGroup uiElement; 
    public bool isOneTime = true;

    [Header("--- Dynamic Binding (Optional) ---")]
    [Tooltip("Leave EMPTY for static tutorials like WASD or Wall Jumping. \nOtherwise enter: Jump, Dash, Attack, Overclock, or Parry")]
    public string actionName; 
    
    [Tooltip("The TextMeshPro object for instructions")]
    public TextMeshProUGUI tutorialText;
    
    [Tooltip("Text template. Use {KEY} for dynamic binds. \nIgnored if Action Name is empty.")]
    public string textTemplate = "Press {KEY} to Dash";

    [Header("--- Dynamic Icons (Optional) ---")]
    [Tooltip("Only used if Action Name is filled. Maps KeyCodes to specific icon objects.")]
    public List<KeyToImageMap> keyIconMaps;

    [System.Serializable]
    public class KeyToImageMap
    {
        public KeyCode key;
        public GameObject iconObject;
    }

    [Header("--- Animation Effects ---")]
    public float fadeDuration = 0.4f;
    public bool enableFloating = true;
    public float floatSpeed = 2.0f;
    public float floatRange = 0.1f;

    private bool isPlayerInZone = false;
    private bool hasCompleted = false;
    private Vector3 originalPos;
    private Coroutine currentFadeRoutine;
    private KeyCode _requiredKey = KeyCode.None;

    void Start()
    {
        if (uiElement != null)
        {
            uiElement.alpha = 0; 
            originalPos = uiElement.transform.localPosition;
            uiElement.gameObject.SetActive(true); 
        }
        RefreshTutorial();
    }

    public void RefreshTutorial()
    {
        if (string.IsNullOrEmpty(actionName)) return;

        UpdateRequiredKey();

        if (tutorialText != null)
        {
            string keyString = _requiredKey.ToString();
            if (keyString.Contains("Alpha")) keyString = keyString.Replace("Alpha", "");
            tutorialText.text = textTemplate.Replace("{KEY}", keyString);
        }

        if (keyIconMaps != null)
        {
            foreach (var map in keyIconMaps)
            {
                if (map.iconObject != null)
                {
                    bool isMatch = (map.key == _requiredKey);
                    map.iconObject.SetActive(isMatch);
                }
            }
        }
    }

    private void UpdateRequiredKey()
    {
        switch (actionName.ToLower())
        {
            case "jump": _requiredKey = GameKeys.Jump; break;
            case "dash": _requiredKey = GameKeys.Dash; break;
            case "attack": _requiredKey = GameKeys.Attack; break;
            case "overclock": _requiredKey = GameKeys.Overclock; break;
            case "parry": _requiredKey = GameKeys.Parry; break;
            default: _requiredKey = KeyCode.None; break; 
        }
    }

    void Update()
    {
        if (hasCompleted || uiElement == null) return;

        if (enableFloating && uiElement.alpha > 0)
        {
            float newY = originalPos.y + Mathf.Sin(Time.time * floatSpeed) * floatRange;
            uiElement.transform.localPosition = new Vector3(originalPos.x, newY, originalPos.z);
        }

        if (isPlayerInZone)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                if (Input.anyKeyDown || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
                    CompleteTutorial();
            }
            else if (_requiredKey != KeyCode.None && Input.GetKeyDown(_requiredKey))
            {
                CompleteTutorial();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only block entry if it's a one-time tutorial that's already finished
        if (isOneTime && hasCompleted) return;

        if (other.CompareTag("Player"))
        {
            RefreshTutorial(); 
            isPlayerInZone = true;
            FadeUI(1.0f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            
            // If it's repeatable, we reset hasCompleted so it can fade in again next time
            if (!isOneTime)
            {
                hasCompleted = false;
            }

            if (!hasCompleted) FadeUI(0.0f);
        }
    }

    void CompleteTutorial()
    {
        hasCompleted = true;
        // FadeUI(0.0f);
        
        if (isOneTime) 
        {
            Destroy(gameObject, fadeDuration + 0.1f);
        }
    }

    void FadeUI(float targetAlpha)
    {
        if (!gameObject.activeInHierarchy) return;
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = uiElement.alpha;
        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            uiElement.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        uiElement.alpha = targetAlpha;
    }

    void OnEnable()
    {
        GameKeys.OnSettingsApplied.AddListener(RefreshTutorial);
        RefreshTutorial();
    }

    void OnDisable()
    {
        GameKeys.OnSettingsApplied.RemoveListener(RefreshTutorial);
    }
}