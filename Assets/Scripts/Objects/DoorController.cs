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
}