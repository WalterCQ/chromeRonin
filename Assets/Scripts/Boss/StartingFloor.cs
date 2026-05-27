using UnityEngine;

public class StartingFloor : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 5f;    // Same speed as other platforms
    public float destroyY = -15f;   // Destroy height

    private bool _isFalling = false; // Switch: default is off

    void Update()
    {
        // Only execute falling logic when switch is on
        if (_isFalling)
        {
            // 1. Fall
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            // 2. Destroy
            if (transform.position.y < destroyY)
            {
                Destroy(gameObject);
            }
        }
    }

    // Public method for external objects (player) to call
    public void ActivateFall()
    {
        _isFalling = true;
    }
}