using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    public GameObject cam;
    public float parallaxEffect;

    private float length;
    private float startpos;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        float temp = (cam.transform.position.x * (1 - parallaxEffect));

        if (temp > startpos + length)
        {
            startpos += length;
        }
        else if (temp < startpos - length)
        {
            startpos -= length;
        }
    }
}