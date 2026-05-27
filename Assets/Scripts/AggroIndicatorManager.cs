using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AggroIndicatorManager : MonoBehaviour
{
    public static AggroIndicatorManager Instance { get; private set; }

    [Header("Visual Settings")]
    public GameObject indicatorPrefab; 
    public float verticalOffset = 1.5f; 
    public float destroyTime = 1.0f;    

    [Header("Audio Settings")]
    public AudioClip aggroSound;        
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource _source;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _source = GetComponent<AudioSource>();
    }

    // USE THIS FOR: Boss/Enemies (Follows them)
    public void ShowAggro(Transform enemyTransform)
    {
        PlaySound();

        if (indicatorPrefab != null)
        {
            Vector3 spawnPos = enemyTransform.position + Vector3.up * verticalOffset;
            GameObject indicator = Instantiate(indicatorPrefab, spawnPos, Quaternion.identity);
            indicator.transform.SetParent(enemyTransform);
            Destroy(indicator, destroyTime);
        }
    }

    // USE THIS FOR: Lasers / Traps (Static world position)
    public void ShowAggroAtPosition(Vector3 worldPosition)
    {
        PlaySound();

        if (indicatorPrefab != null)
        {
            // Spawn exactly where requested (no offset, no parent)
            GameObject indicator = Instantiate(indicatorPrefab, worldPosition, Quaternion.identity);
            Destroy(indicator, destroyTime);
        }
    }

    private void PlaySound()
    {
        if (aggroSound != null && _source != null)
        {
            _source.pitch = Random.Range(0.9f, 1.1f); 
            _source.PlayOneShot(aggroSound, volume);
        }
    }
}