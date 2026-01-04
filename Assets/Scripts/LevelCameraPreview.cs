using UnityEngine;
using System.Collections;
using Cinemachine;

public class LevelCameraPreview : MonoBehaviour
{
    [Header("--- Motion Settings ---")]
    public float smoothTime = 0.5f;     
    public float stayDuration = 1.5f;   
    public float zOffset = -10f;

    [Header("--- Zoom Settings ---")]
    [Tooltip("Size when looking at a switch/exit (Zoomed In)")]
    public float focusSize = 5f; 
    [Tooltip("Size when moving between objects (Zoomed Out)")]
    public float travelSize = 8f; 

    [Header("--- Input Skip Settings ---")]
    public int inputsToSkip = 3; 
    private int _currentInputCount = 0;
    private bool _skipTriggered = false;

    [Header("--- Required References ---")]
    public Transform playerTransform; 
    public Collider2D mapCollider; 

    // Internal Variables
    private Vector3 _currentVelocity; 
    private float _currentZoomVelocity; 
    private Camera _cam;
    private MonoBehaviour _brain;
    private PlayerMovement _playerScript;

    // Bounds variables
    private float _minX, _maxX, _minY, _maxY;
    private float _camHalfHeight, _camHalfWidth;

    void Start()
    {
        _cam = Camera.main;
        _brain = _cam.GetComponent("CinemachineBrain") as MonoBehaviour;
        
        if (playerTransform != null)
             _playerScript = playerTransform.GetComponent<PlayerMovement>();

        _currentVelocity = Vector3.zero;
        _currentZoomVelocity = 0f;

        CalculateCameraBounds();

        GameObject[] switches = GameObject.FindGameObjectsWithTag("Switch");

        GameObject exitTarget = null;

        exitTarget = GameObject.FindGameObjectWithTag("Exit");

        if (exitTarget == null)
        {
            DoorController[] allDoors = FindObjectsOfType<DoorController>();
            foreach (var door in allDoors)
            {
                if (door.isExitDoor && door.exitTriggerObject != null)
                {
                    exitTarget = door.exitTriggerObject;
                    break; 
                }
            }
        }
        StartCoroutine(PlayLevelSequence(switches, exitTarget));
    }

    IEnumerator PlayLevelSequence(GameObject[] switches, GameObject exit)
    {
        InitializeCamera();
        TogglePlayerControl(false);
        _currentInputCount = 0;
        _skipTriggered = false;

        if (switches != null)
        {
            foreach (var switchObj in switches)
            {
                if (_skipTriggered) break;
                
                yield return MoveCameraTo(switchObj.transform.position);
                yield return WaitWithSkipCheck(stayDuration);
            }
        }

        if (exit != null && !_skipTriggered)
        {
            yield return MoveCameraTo(exit.transform.position);
            yield return WaitWithSkipCheck(stayDuration);
        }

        if (playerTransform != null)
            yield return MoveCameraTo(playerTransform.position);

        if (_brain != null) _brain.enabled = true;
        TogglePlayerControl(true);
        this.enabled = false;
    }

    IEnumerator MoveCameraTo(Vector3 rawTargetPos)
    {
        if (_skipTriggered) yield break;

        // Reset velocity slightly to prevent overshooting from previous move
        _currentVelocity = Vector3.zero;

        // We loop until we are close to position AND close to the correct zoom
        // This ensures we don't stop while stuck at a wall zoomed out
        bool arrived = false;

        while (!arrived)
        {
            CheckForSkipInput();
            if (_skipTriggered) break;

            // --- 1. Calculate Bounds & Target ---
            CalculateCameraBounds();
            // IMPORTANT: Calculate finalTarget inside the loop. 
            // As we zoom in/out, the valid clamp position changes!
            Vector3 finalTarget = ClampPos(rawTargetPos);
            finalTarget.z = zOffset;

            float distToTarget = Vector3.Distance(_cam.transform.position, finalTarget);

            // --- 2. Motion ---
            _cam.transform.position = Vector3.SmoothDamp(
                _cam.transform.position, 
                finalTarget, 
                ref _currentVelocity, 
                smoothTime
            );

            // --- 3. Elastic Zoom ---
            // If far, Zoom Out (Travel). If close, Zoom In (Focus).
            float targetZoom = (distToTarget > 3.0f) ? travelSize : focusSize;
            
            _cam.orthographicSize = Mathf.SmoothDamp(
                _cam.orthographicSize, 
                targetZoom, 
                ref _currentZoomVelocity, 
                smoothTime
            );

            // --- 4. Safety Clamp ---
            // Re-clamp current position in case zoom-out pushed borders into us
            Vector3 clampedCurrent = ClampPos(_cam.transform.position);
            _cam.transform.position = new Vector3(clampedCurrent.x, clampedCurrent.y, zOffset);

            // --- 5. Arrival Check ---
            // We are arrived if Position is close AND Zoom is close to Focus
            // This prevents "Offset" bug where camera stops at wall but stays zoomed out
            bool posArrived = distToTarget < 0.05f;
            bool zoomArrived = Mathf.Abs(_cam.orthographicSize - focusSize) < 0.1f;

            // If we are at the position but Zoom is still big, we force it to keep running
            // so the zoom finishes shrinking, which relaxes the bounds, allowing us to center perfectly.
            if (posArrived && zoomArrived) 
            {
                arrived = true;
            }
            // Special Case: If targetZoom is TravelSize (we are far), we only care about position
            else if (targetZoom == travelSize && posArrived)
            {
                // We hit the wall (limit) but are still zoomed out.
                // Force switch to Focus Size to "drill in" to the target
                // This logic naturally happens as distToTarget shrinks, 
                // but if we hit a wall, distToTarget hits 0.
            }

            yield return null; 
        }

        // Final snap to ensure perfection
        if (!_skipTriggered)
        {
            _cam.orthographicSize = focusSize;
            CalculateCameraBounds();
            _cam.transform.position = ClampPos(rawTargetPos);
            Vector3 p = _cam.transform.position;
            p.z = zOffset;
            _cam.transform.position = p;
        }
    }

    // --- Helpers ---

    public IEnumerator LookAtTarget(Vector3 targetPosition, float stayTime)
    {
        InitializeCamera();
        TogglePlayerControl(false);
        _currentInputCount = 0; 
        _skipTriggered = false;

        yield return MoveCameraTo(targetPosition);
        yield return WaitWithSkipCheck(stayTime);
        
        if (playerTransform != null)
            yield return MoveCameraTo(playerTransform.position);

        if (_brain != null) _brain.enabled = true;
        TogglePlayerControl(true);
    }

    IEnumerator WaitWithSkipCheck(float time)
    {
        float timer = 0;
        while (timer < time && !_skipTriggered)
        {
            CheckForSkipInput();
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void CheckForSkipInput()
    {
        if (_skipTriggered) return;
        if (Input.anyKeyDown) 
        {
            _currentInputCount++;
            if (_currentInputCount >= inputsToSkip) _skipTriggered = true;
        }
    }

    void InitializeCamera()
    {
        if (_cam == null) 
        {
            _cam = Camera.main;
            _brain = _cam.GetComponent("CinemachineBrain") as MonoBehaviour;
            CalculateCameraBounds();
        }
        if (_brain != null) _brain.enabled = false;
    }

    void TogglePlayerControl(bool canMove)
    {
        if (_playerScript == null) return;
        _playerScript.SetInputLock(!canMove); 
    }

    void CalculateCameraBounds()
    {
        if (mapCollider == null) return;
        _camHalfHeight = _cam.orthographicSize;
        _camHalfWidth = _camHalfHeight * _cam.aspect;
        Bounds b = mapCollider.bounds;
        _minX = b.min.x + _camHalfWidth;
        _maxX = b.max.x - _camHalfWidth;
        _minY = b.min.y + _camHalfHeight;
        _maxY = b.max.y - _camHalfHeight;
    }

    Vector3 ClampPos(Vector3 target)
    {
        if (mapCollider == null) return target;
        // Logic: If min > max (zoom is larger than map), center the camera
        float x = (_minX > _maxX) ? mapCollider.bounds.center.x : Mathf.Clamp(target.x, _minX, _maxX);
        float y = (_minY > _maxY) ? mapCollider.bounds.center.y : Mathf.Clamp(target.y, _minY, _maxY);
        return new Vector3(x, y, target.z);
    }
}