using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("0 = Moves with Player (Static), 1 = Moves with Camera (Far away)")]
    [Range(0, 1)] 
    public float parallaxFactor;

    private Transform cam;
    private Vector3 startPos;
    private Vector3 startCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
        startCamPos = cam.position;
    }

    void LateUpdate()
    {
        // 1. Calculate how far the camera has moved from the start
        Vector3 camDifference = cam.position - startCamPos;

        // 2. Apply the parallax factor to that distance
        // If factor is 1, we move exactly with the camera (Sky)
        // If factor is 0, we don't move at all (Ground)
        float moveX = camDifference.x * parallaxFactor;
        float moveY = camDifference.y * parallaxFactor; // Optional: Remove this line if you only want horizontal parallax

        // 3. Update position
        transform.position = new Vector3(startPos.x + moveX, startPos.y + moveY, transform.position.z);
    }
}