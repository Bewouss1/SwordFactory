using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Système de pooling d'objets pour optimiser les performances
/// Réutilise les GameObjects au lieu de les détruire et recréer constamment
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size = 10;
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    public static ObjectPool Instance { get; private set; }

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePools();
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary[pool.tag] = objectPool;
        }
    }

    /// <summary>
    /// Récupère un objet du pool ou en crée un nouveau si le pool est vide
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: Pool with tag '{tag}' doesn't exist!");
            return null;
        }

        GameObject obj;

        if (poolDictionary[tag].Count > 0)
        {
            obj = poolDictionary[tag].Dequeue();
        }
        else
        {
            // Pool vide : créer un nouvel objet
            var poolConfig = pools.Find(p => p.tag == tag);
            if (poolConfig != null)
            {
                obj = Instantiate(poolConfig.prefab, transform);
                Debug.Log($"ObjectPool: Pool '{tag}' exhausted, created new object");
            }
            else
            {
                return null;
            }
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    /// <summary>
    /// Retourne un objet au pool
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: Pool with tag '{tag}' doesn't exist! Destroying object.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }
}
