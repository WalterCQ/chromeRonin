using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Object Pool System (Zero-Setup).
/// Automatically manages object pools without manual editor configuration (pre-warming is optional).
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private static ObjectPool _instance;
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ObjectPool>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GlobalObjectPool");
                    _instance = go.AddComponent<ObjectPool>();
                }
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int initialSize = 5;
    }

    [Header("Pre-warm Pool Config (Optional)")]
    public List<PoolConfig> prewarmPools = new List<PoolConfig>();

    // Key: Prefab Name (clean), Value: Queue of objects
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    // Key: Instance ID, Value: Original Prefab Name (for returning without needing the prefab reference)
    private Dictionary<int, string> _instanceSourceMap = new Dictionary<int, string>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var config in prewarmPools)
        {
            if (config.prefab != null)
            {
                Prewarm(config.prefab, config.initialSize);
            }
        }
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if(prefab == null) return;
        string key = prefab.name;

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            CreateNewObject(prefab, key);
        }
    }

    private GameObject CreateNewObject(GameObject prefab, string key)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = key; // Keep name clean
        obj.SetActive(false);
        _pools[key].Enqueue(obj);
        return obj;
    }

    public GameObject Get(GameObject prefab)
    {
        return Get(prefab, Vector3.zero, Quaternion.identity);
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) 
        {
            Debug.LogError("[ObjectPool] Tried to spawn null prefab!");
            return null;
        }

        string key = prefab.name;

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        GameObject obj;

        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }
        else
        {
            // Pool empty, create new with correct transform immediately
            // This ensures OnEnable runs with the correct rotation if the prefab is active
            obj = Instantiate(prefab, position, rotation, transform);
            obj.name = key;
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        // Reset parent to pool
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        // Strip "(Clone)" just in case, though we try to prevent it
        string key = obj.name.Replace("(Clone)", "").Trim();

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        _pools[key].Enqueue(obj);
    }

    public void Return(GameObject obj, float delay)
    {
        if (obj == null) return;
        StartCoroutine(ReturnDelayed(obj, delay));
    }

    private IEnumerator ReturnDelayed(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(obj);
    }
}
