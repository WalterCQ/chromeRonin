using UnityEngine;

public class TreadmillSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] platformPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 1.5f;
    
    [Header("Boss Control")]
    public bool isBossDead = false;

    private float _timer;

    void Update()
    {
        if (isBossDead) return;

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            SpawnPlatform();
            _timer = spawnInterval;
        }
    }

    void SpawnPlatform()
    {
        if (platformPrefabs == null || platformPrefabs.Length == 0) return;
        
        int index = Random.Range(0, platformPrefabs.Length);
        if (platformPrefabs[index] == null) return;
        
        float randomX = Random.Range(-5f, 5f);
        Vector3 finalPos = new Vector3(randomX, spawnPoint.position.y, 0);

        Instantiate(platformPrefabs[index], finalPos, Quaternion.identity);
    }
}