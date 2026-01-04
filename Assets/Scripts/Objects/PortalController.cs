using UnityEngine;
using UnityEngine.Events;

public class PortalController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("需要激活的开关数量才能让传送门消失")]
    public int switchesRequired = 3;
    
    [Header("References")]
    [Tooltip("要消失的传送门对象，如果为空则使用当前对象")]
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
        
        // 如果没有指定传送门对象，则使用当前对象
        if (portalToDisable == null)
            portalToDisable = gameObject;
    }

    /// <summary>
    /// 当开关被激活时调用此方法
    /// </summary>
    public void AddActiveSwitch()
    {
        currentActiveSwitches++;
        CheckPortalState();
    }

    /// <summary>
    /// 当开关被关闭时调用此方法
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

        // 传送门消失
        if (portalToDisable != null)
            portalToDisable.SetActive(false);

        onPortalDisabled?.Invoke();

        // 摄像机看向传送门位置
        if (lookAtPortalOnDisable && _camPreview != null)
        {
            StartCoroutine(_camPreview.LookAtTarget(transform.position, cameraStayDuration));
        }
    }

    private void EnablePortal()
    {
        if (!isDisabled) return;
        isDisabled = false;

        // 传送门恢复
        if (portalToDisable != null)
            portalToDisable.SetActive(true);

        onPortalEnabled?.Invoke();
    }
}
