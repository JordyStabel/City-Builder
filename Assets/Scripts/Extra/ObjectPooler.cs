//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

    public static ObjectPooler Instance { get; protected set; }

    // Create a dictionary of queues (list of all pools)
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // Create list of pools
    public List<Pool> pools;

    private void Awake()
    {
        // Setting the instance equal to this current one (with check)
        if (Instance != null)
            Debug.LogError("There shouldn't be two objectPoolers.");
        else
            Instance = this;
    }

    void Start () {

        // List of all the available pools
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // For each pool a queue full of objects gets created
        foreach (Pool pool in pools)
        {
            // Create new pool 'folder'
            GameObject poolHolder = new GameObject(pool.tag + "_pool");
            poolHolder.transform.SetParent(this.transform, true);

            // Create new pool
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Each pool will get filled, all objects get disabled and finally all objects get added to the queue
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject gameObject = Instantiate(pool.prefab);
                gameObject.SetActive(false);
                gameObject.name = pool.tag + "_" + i;
                gameObject.transform.SetParent(poolHolder.transform, true);
                objectPool.Enqueue(gameObject);
            }

            // Add the new pool to the pooldictionary
            poolDictionary.Add(pool.tag, objectPool);
        }
	}

    public void AddToPool(string poolTag, int quantity)
    {

    }

    /// <summary>
    /// 'Spawn' an object from the given object pool, on the given position and rotation.
    /// </summary>
    /// <param name="tag">The name of the object to pool</param>
    /// <param name="position">The position the pool object will get.</param>
    /// <param name="rotation">The rotation the pool object will get.</param>
    /// <returns>GameObject from the pool</returns>
    public GameObject SpawnFromPool(string tag, Vector2 position, Quaternion rotation)
    {
        // Check if tag actually excists.
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag: " + tag + " doesn't excist.");
            return null;
        }

        // Pick the right object from the pooldictionary
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Activate the object and set its properties
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Grab the interface from the pooled object
        IPooledObject pooledObject = objectToSpawn.GetComponent<IPooledObject>();

        // Run the 'start' function IF it has the interface
        if (pooledObject != null)
            pooledObject.OnObjectSpawn();

        // Add the object back to the queue, so it can get re-used later
        poolDictionary[tag].Enqueue(objectToSpawn);

        // Return the object pooled from the pool
        return objectToSpawn;
    }

    /// <summary>
    /// Disable a pooled gameobject.
    /// </summary>
    /// <param name="gameObject">Pooled GameObject to disable.</param>
    public void DespawnFromPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
