using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoolConfig
{
    public GameObject enemyPrefab;
    public int initPoolSize = 10;
    public bool canExpand = true;
}

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] List<PoolConfig> poolConfigs = new List<PoolConfig>();
    [SerializeField] bool showDebugLogs = true;

    Dictionary<GameObject, Queue<PooledEnemy>> pools = new Dictionary<GameObject, Queue<PooledEnemy>>();
    Dictionary<GameObject, PoolConfig> configLookup = new Dictionary<GameObject, PoolConfig>();
    Dictionary<PooledEnemy, GameObject> instanceToPrefab = new Dictionary<PooledEnemy, GameObject>();

    private void Awake()
    {
        InitializePools();
    }

    void InitializePools()
    {
        pools.Clear();
        configLookup.Clear();
        instanceToPrefab.Clear();

        foreach(PoolConfig currConfig in poolConfigs)
        {
            if(currConfig.enemyPrefab == null)
            {
                LogWarning("Pool config is missing an enemy prefab");
                continue;
            }

            if (configLookup.ContainsKey(currConfig.enemyPrefab))
            {
                LogWarning("Duplicate pool config found for the current prefab");
                continue;
            }

            if(currConfig.initPoolSize < 1)
            {
                LogWarning("The pool size is less than 1 silly fix it");
                continue;
            }

            configLookup.Add(currConfig.enemyPrefab, currConfig);
            pools.Add(currConfig.enemyPrefab, new Queue<PooledEnemy>());

            for(int i = 0; i < currConfig.initPoolSize; i++)
            {
                CreateAndStoreInstance(currConfig.enemyPrefab);
            }

        }
    }

    void CreateAndStoreInstance(GameObject _Prefab)
    {
        GameObject obj = Instantiate(_Prefab, transform);
        obj.SetActive(false);

        PooledEnemy pooledEnemy = obj.GetComponent<PooledEnemy>();

        if(pooledEnemy == null)
        {
            pooledEnemy = obj.AddComponent<PooledEnemy>();
        }

        pooledEnemy.Init(this);

        pools[_Prefab].Enqueue(pooledEnemy);
        instanceToPrefab[pooledEnemy] = _Prefab;

    }

    public bool HasPoolForPrefab(GameObject _Prefab)
    {
        return _Prefab != null && pools.ContainsKey(_Prefab);
    }

    public PooledEnemy GetFromPool(GameObject _Prefab, Vector3 _Pos, Quaternion _Rot)
    {
        if(_Prefab == null)
        {
            LogWarning("Tried to get from a pool with null prefab");
            return null;
        }

        if (!pools.ContainsKey(_Prefab))
        {
            LogWarning("No pool exists for the current prefab");
            return null;
        }

        if (pools[_Prefab].Count == 0)
        {
            PoolConfig config = configLookup[_Prefab];

            if(config != null && config.canExpand)
            {
                LogWarning("Pool expaned for prefab");
                CreateAndStoreInstance(_Prefab);
            }
            else
            {
                LogWarning("Pool is empty and cannot expand");
                return null;
            }
        }

        PooledEnemy enemy = pools[_Prefab].Dequeue();
        enemy.transform.position = _Pos;
        enemy.transform.rotation = _Rot;
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void ReturnToPool(PooledEnemy _Enemy)
    {
        if(_Enemy == null)
        {
            LogWarning("Tried to return null enemy to pool");
            return;
        }

        if (!instanceToPrefab.ContainsKey(_Enemy))
        {
            LogWarning("Returned enemy is not tracked by this pool");
            _Enemy.gameObject.SetActive(false);
            return;
        }

        GameObject prefab = instanceToPrefab[_Enemy];
        _Enemy.gameObject.SetActive(false);
        _Enemy.transform.SetParent(transform);
        pools[prefab].Enqueue(_Enemy);

    }

    void LogWarning(string _Msg)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning($"[ObjectPoolManager] {_Msg}", this);
        }
    }

}