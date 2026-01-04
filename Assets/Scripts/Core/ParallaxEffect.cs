<<<<<<< HEAD
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
=======
using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("1.0 = Moves with Camera (Skybox), 0.0 = Static World Object")]
    public float parallaxFactor; 
    
    [Tooltip("Check this if you want the background to stay fixed vertically.")]
    public bool lockY = false; 

    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        
        lastCamPos = cam.position; 
    }

    void LateUpdate()
    {
        if (Vector3.Distance(cam.position, lastCamPos) > 5f)
        {
            lastCamPos = cam.position;
            return;
        }

        Vector3 deltaMovement = cam.position - lastCamPos;

        float moveX = deltaMovement.x * parallaxFactor;
        float moveY = deltaMovement.y * parallaxFactor;

        transform.position += new Vector3(moveX, lockY ? 0 : moveY, 0);

        lastCamPos = cam.position;
    }
>>>>>>> upstream/dev
}