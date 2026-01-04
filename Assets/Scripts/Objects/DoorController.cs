<<<<<<< HEAD
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpen = false;
    public float moveSpeed = 2f;
    public Vector3 openOffset = new Vector3(0, 3, 0);

    private Vector3 _closedPos;
    private Vector3 _openPos;

    void Start()
    {
        _closedPos = transform.position;
        _openPos = _closedPos + openOffset;
    }

    void Update()
    {
        Vector3 target = isOpen ? _openPos : _closedPos;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    // Call these from the Switch's UnityEvent
    public void Open()
    {
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
    }

    public void Toggle()
    {
        isOpen = !isOpen;
    }
=======
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Type")]
    [Tooltip("Check this if this door leads to the Next Level.")]
    public bool isExitDoor = false; 

    [Header("Settings")]
    public int switchesRequired = 1;
    
    [Header("Camera Preview")]
    public bool lookAtDoorOnOpen = true;
    public float cameraStayDuration = 1.5f;
    
    [Header("Events")]
    public UnityEvent onDoorOpen;
    public UnityEvent onDoorClose;

    [Header("Movement Settings")]
    public bool useSlidingMovement = false; 
    public float moveSpeed = 2f;
    public Vector3 openOffset = new Vector3(0, 3, 0);

    [Header("References")]
    public Animator animator;
    
    [Tooltip("REQUIRED if isExitDoor is TRUE. The object that loads the next level.")]
    public GameObject exitTriggerObject;

    [Tooltip("REQUIRED if isExitDoor is FALSE. The physical barrier.")]
    public Collider2D doorCollider; 

    [Header("Debug Info")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private int currentActiveSwitches = 0;

    private Vector3 _closedPos;
    private Vector3 _openPos;
    private LevelCameraPreview _camPreview; 

    void Start()
    {
        _closedPos = transform.position;
        _openPos = _closedPos + openOffset;
        _camPreview = FindObjectOfType<LevelCameraPreview>();

        // --- TYPE 1: EXIT DOOR LOGIC ---
        if (isExitDoor)
        {
            // Ensure Exit Trigger is hidden at start
            if (exitTriggerObject != null) exitTriggerObject.SetActive(false);
            // Ensure Exit Doors don't block the player (per your preference)
            if (doorCollider != null) doorCollider.enabled = false; 
        }
        // --- TYPE 2: BARRIER DOOR LOGIC ---
        else
        {
            // Ensure Barrier is SOLID at start
            if (doorCollider != null) doorCollider.enabled = true;
        }
    }

    void Update()
    {
        if (useSlidingMovement)
        {
            Vector3 target = isOpen ? _openPos : _closedPos;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        }
    }

    public void AddActiveSwitch()
    {
        currentActiveSwitches++;
        CheckDoorState();
    }

    public void RemoveActiveSwitch()
    {
        currentActiveSwitches--;
        CheckDoorState();
    }

    private void CheckDoorState()
    {
        if (currentActiveSwitches >= switchesRequired) Open();
        else Close();
    }

    private void Open()
    {
        if (isOpen) return; 
        isOpen = true;

        if (animator != null) animator.SetBool("isOpen", true);

        // --- EXIT DOOR: Enable the Trigger ---
        if (isExitDoor)
        {
            if (exitTriggerObject != null) exitTriggerObject.SetActive(true);
        }
        // --- BARRIER DOOR: Handle Physics ---
        else
        {
            StopAllCoroutines();
            StartCoroutine(DisableColliderRoutine());
        }

        onDoorOpen?.Invoke();
        if (lookAtDoorOnOpen && _camPreview != null)
        {
             StartCoroutine(_camPreview.LookAtTarget(transform.position, cameraStayDuration));
        }
    }

    private void Close()
    {
        if (!isOpen) return; 
        isOpen = false;

        if (animator != null) animator.SetBool("isOpen", false);

        // --- EXIT DOOR: Disable Trigger ---
        if (isExitDoor)
        {
            if (exitTriggerObject != null) exitTriggerObject.SetActive(false);
        }
        // --- BARRIER DOOR: Re-enable Physics ---
        else
        {
            StopAllCoroutines();
            if (doorCollider != null) doorCollider.enabled = true;
        }
        
        onDoorClose?.Invoke();
    }

    private IEnumerator DisableColliderRoutine()
    {
        if (useSlidingMovement) yield break;

        yield return new WaitForSeconds(0.5f);
        
        if (doorCollider != null) doorCollider.enabled = false;
    }
>>>>>>> upstream/dev
}