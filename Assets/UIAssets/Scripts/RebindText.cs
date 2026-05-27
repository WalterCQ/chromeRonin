using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class RebindText : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuration")]
    public string actionName;

    [Header("Visual Feedback")]
    public Color waitingColor = Color.yellow;
    public Color normalColor = Color.white;

    public static bool isRebinding = false;

    private TextMeshProUGUI keyText;
    private bool isWaiting = false;

    void Start()
    {
        keyText = GetComponent<TextMeshProUGUI>();
        UpdateVisual();
    }

    void OnEnable()
    {
        UpdateVisual();
    }

    void OnDisable()
    {
        StopRebinding();
    }

    // 🔹 REQUIRED by PauseManager — DO NOT REMOVE
    public void StopRebinding()
    {
        StopAllCoroutines();
        isWaiting = false;
        isRebinding = false;
        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Block UI interaction while rebinding
        if (isRebinding || isWaiting)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            StartCoroutine(WaitForKey());
        }
    }

    IEnumerator WaitForKey()
    {
        isWaiting = true;
        isRebinding = true;

        keyText.text = "...";
        keyText.color = waitingColor;

        // IMPORTANT: wait until the click that opened this UI is released
        while (Input.GetMouseButton(0))
            yield return null;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FinishRebind();
                yield break;
            }

            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (!Input.GetKeyDown(key))
                    continue;

                AssignKey(key);
                FinishRebind();
                yield break;
            }

            yield return null;
        }
    }

    void AssignKey(KeyCode key)
    {
        switch (actionName)
        {
            case "Jump":
                GameKeys.Jump = key;
                break;
            case "Attack":
                GameKeys.Attack = key;
                break;
            case "Parry":
                GameKeys.Parry = key;
                break;
            case "Overclock":
                GameKeys.Overclock = key;
                break;
            case "Dash":
                GameKeys.Dash = key;
                break;
        }

        GameKeys.SaveKeys();
    }

    void FinishRebind()
    {
        StopAllCoroutines();
        StartCoroutine(FinishRebindSafely());
    }

    IEnumerator FinishRebindSafely()
    {
        isWaiting = false;
        UpdateVisual();

        // Prevent UI click re-trigger by waiting for mouse release
        while (Input.GetMouseButton(0))
            yield return null;

        yield return null; // extra safety frame

        isRebinding = false;
    }

    void UpdateVisual()
    {
        if (keyText == null)
            return;

        switch (actionName)
        {
            case "Jump":
                keyText.text = GameKeys.Jump.ToString();
                break;
            case "Attack":
                keyText.text = GameKeys.Attack.ToString();
                break;
            case "Parry":
                keyText.text = GameKeys.Parry.ToString();
                break;
            case "Overclock":
                keyText.text = GameKeys.Overclock.ToString();
                break;
            case "Dash":
                keyText.text = GameKeys.Dash.ToString();
                break;
        }

        keyText.color = normalColor;
    }
}
