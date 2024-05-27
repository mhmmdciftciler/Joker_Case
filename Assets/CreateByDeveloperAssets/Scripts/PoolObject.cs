using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    private PoolManager PoolManager;
    public string PoolName { private set; get; }
    [SerializeField] private float returnTime = 3;
    float timer;
    private void OnEnable()
    {
        timer = returnTime;
    }
    public void Init(string poolName, PoolManager poolManager)
    {
        PoolManager = poolManager;
        PoolName = poolName;
    }
    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            ReturnPool();
        }
    }
    public void ReturnPool()
    {
        PoolManager.ReturnObjectToPool(PoolName, this);
    }
}
