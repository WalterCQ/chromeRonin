<<<<<<< HEAD
using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections;

public class RebindButton : MonoBehaviour
{
    [Header("Configuration")]
    public string actionName; 
    public TextMeshProUGUI keyDisplay; 

    // Global flag
    public static bool isRebinding = false; 

    private Button myButton;
    private bool isWaiting = false;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(StartRebind);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (actionName == "Jump") keyDisplay.text = GameKeys.Jump.ToString();
        else if (actionName == "Attack") keyDisplay.text = GameKeys.Attack.ToString();
        else if (actionName == "Parry") keyDisplay.text = GameKeys.Parry.ToString();
        else if (actionName == "Overclock") keyDisplay.text = GameKeys.Overclock.ToString();
        else if (actionName == "Dash") keyDisplay.text = GameKeys.Dash.ToString();
    }

    void StartRebind()
    {
        if (!isWaiting) StartCoroutine(WaitForKey());
    }

    IEnumerator WaitForKey()
    {
        isWaiting = true;
        isRebinding = true; 
        keyDisplay.text = "..."; 

        yield return null; 

        while (!Input.anyKeyDown) yield return null;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopRebindingProcess();
            yield break; 
        }

        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
            {
                if (actionName == "Jump") GameKeys.Jump = kcode;
                else if (actionName == "Attack") GameKeys.Attack = kcode;
                else if (actionName == "Parry") GameKeys.Parry = kcode;
                else if (actionName == "Overclock") GameKeys.Overclock = kcode;
                else if (actionName == "Dash") GameKeys.Dash = kcode;

                GameKeys.SaveKeys();
                StopRebindingProcess();
                break;
            }
        }
    }

    void StopRebindingProcess()
    {
        isWaiting = false;
        UpdateVisual();
        StartCoroutine(UnlockInputDelay());
    }

    IEnumerator UnlockInputDelay()
    {
        yield return null;
        isRebinding = false; 
    }
=======
using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections;

public class RebindButton : MonoBehaviour
{
    [Header("Configuration")]
    public string actionName; 
    public TextMeshProUGUI keyDisplay; 

    public static bool isRebinding = false; 

    private Button myButton;
    private bool isWaiting = false;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(StartRebind);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (actionName == "Jump") keyDisplay.text = GameKeys.Jump.ToString();
        else if (actionName == "Attack") keyDisplay.text = GameKeys.Attack.ToString();
        else if (actionName == "Parry") keyDisplay.text = GameKeys.Parry.ToString();
        else if (actionName == "Overclock") keyDisplay.text = GameKeys.Overclock.ToString();
        else if (actionName == "Dash") keyDisplay.text = GameKeys.Dash.ToString();
    }

    void StartRebind()
    {
        if (!isWaiting) StartCoroutine(WaitForKey());
    }

    IEnumerator WaitForKey()
    {
        isWaiting = true;
        isRebinding = true; 
        keyDisplay.text = "..."; 

        yield return null; 

        while (!Input.anyKeyDown) yield return null;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopRebindingProcess();
            yield break; 
        }

        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
            {
                if (actionName == "Jump") GameKeys.Jump = kcode;
                else if (actionName == "Attack") GameKeys.Attack = kcode;
                else if (actionName == "Parry") GameKeys.Parry = kcode;
                else if (actionName == "Overclock") GameKeys.Overclock = kcode;
                else if (actionName == "Dash") GameKeys.Dash = kcode;

                GameKeys.SaveKeys();
                StopRebindingProcess();
                break;
            }
        }
    }

    void StopRebindingProcess()
    {
        isWaiting = false;
        UpdateVisual();
        StartCoroutine(UnlockInputDelay());
    }

    IEnumerator UnlockInputDelay()
    {
        yield return null;
        isRebinding = false; 
    }
>>>>>>> upstream/dev
}