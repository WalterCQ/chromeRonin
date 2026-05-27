using UnityEngine;
using UnityEngine.Events;

public class PortalController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Number of switches required to deactivate the portal")]
    public int switchesRequired = 3;
    
    [Header("References")]
    [Tooltip("Portal object to disable, uses current object if empty")]
    public GameObject portalToDisable;
    
    [Header("Camera Preview")]
    public bool lookAtPortalOnDisable = true;
    public float cameraStayDuration = 1.5f;
    
    [Header("Events")]
    public UnityEvent onPortalDisabled;
    public UnityEvent onPortalEnabled;
    
    [Header("Debug Info")]
    [SerializeField] private int currentActiveSwitches = 0;
    [SerializeField] private bool isDisabled = false;

    private LevelCameraPreview _camPreview;

    void Start()
    {
        _camPreview = FindObjectOfType<LevelCameraPreview>();
        
        // If no portal object is specified, use current object
        if (portalToDisable == null)
            portalToDisable = gameObject;
    }

    /// <summary>
    /// Called when a switch is activated
    /// </summary>
    public void AddActiveSwitch()
    {
        currentActiveSwitches++;
        CheckPortalState();
    }

    /// <summary>
    /// Called when a switch is deactivated
    /// </summary>
    public void RemoveActiveSwitch()
    {
        currentActiveSwitches--;
        CheckPortalState();
    }

    private void CheckPortalState()
    {
        if (currentActiveSwitches >= switchesRequired)
        {
            DisablePortal();
        }
        else
        {
            EnablePortal();
        }
    }

    private void DisablePortal()
    {
        if (isDisabled) return;
        isDisabled = true;

        // Disable the portal
        if (portalToDisable != null)
            portalToDisable.SetActive(false);

        onPortalDisabled?.Invoke();

        // Camera looks at portal position
        if (lookAtPortalOnDisable && _camPreview != null)
        {
            StartCoroutine(_camPreview.LookAtTarget(transform.position, cameraStayDuration));
        }
    }

    private void EnablePortal()
    {
        if (!isDisabled) return;
        isDisabled = false;

        // Re-enable the portal
        if (portalToDisable != null)
            portalToDisable.SetActive(true);

        onPortalEnabled?.Invoke();
    }
}
