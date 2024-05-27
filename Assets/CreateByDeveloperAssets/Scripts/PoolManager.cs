using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private Dictionary<string, Queue<PoolObject>> poolDictionary = new Dictionary<string, Queue<PoolObject>>();

    public void CreatePool(PoolObject prefab, int poolSize)
    {
        string poolKey = prefab.name;

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary[poolKey] = new Queue<PoolObject>();

            for (int i = 0; i < poolSize; i++)
            {
                PoolObject newObject = Instantiate(prefab);
                newObject.Init(prefab.name, this);
                newObject.gameObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }

    public PoolObject GetObjectFromPool(PoolObject prefab)
    {
        string poolKey = prefab.name;

        if (poolDictionary.ContainsKey(poolKey))
        {
            if (poolDictionary[poolKey].Count > 0)
            {
                PoolObject objectToReuse = poolDictionary[poolKey].Dequeue();
                objectToReuse.gameObject.SetActive(true);
                return objectToReuse;
            }
            else
            {
                PoolObject newObject = Instantiate(prefab);
                newObject.Init(prefab.name, this);
                return newObject;
            }
        }
        else
        {
            Debug.LogWarning("Pool for prefab " + poolKey + " doesn't exist. Creating a new one.");
            CreatePool(prefab, 1);
            return GetObjectFromPool(prefab);
        }
    }

    public void ReturnObjectToPool(string poolKey, PoolObject objectToReturn)
    {
        if (poolDictionary.ContainsKey(poolKey))
        {
            objectToReturn.gameObject.SetActive(false);
            poolDictionary[poolKey].Enqueue(objectToReturn);
        }
        else
        {
            Debug.LogWarning("Pool for prefab " + poolKey + " doesn't exist. Destroying the object.");
            Destroy(objectToReturn);
        }
    }
}