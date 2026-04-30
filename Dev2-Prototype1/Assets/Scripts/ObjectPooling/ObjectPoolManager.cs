using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoolConfig
{
    public GameObject prefab;
    public int initPoolSize = 10;
    public bool canExpand = true;
}

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] List<PoolConfig> poolConfigs = new List<PoolConfig>();
    [SerializeField] bool showDebugLogs = true;

    Dictionary<GameObject, Queue<PooledObject>> pools = new Dictionary<GameObject, Queue<PooledObject>>();
    Dictionary<GameObject, PoolConfig> configLookup = new Dictionary<GameObject, PoolConfig>();
    Dictionary<PooledObject, GameObject> instanceToPrefab = new Dictionary<PooledObject, GameObject>();

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
            if(currConfig.prefab == null)
            {
                LogWarning("Pool config is missing an enemy prefab");
                continue;
            }

            if (configLookup.ContainsKey(currConfig.prefab))
            {
                LogWarning("Duplicate pool config found for the current prefab");
                continue;
            }

            if(currConfig.initPoolSize < 1)
            {
                LogWarning("The pool size is less than 1 silly fix it");
                continue;
            }

            configLookup.Add(currConfig.prefab, currConfig);
            pools.Add(currConfig.prefab, new Queue<PooledObject>());

            for(int i = 0; i < currConfig.initPoolSize; i++)
            {
                CreateAndStoreInstance(currConfig.prefab);
            }

        }
    }

    void CreateAndStoreInstance(GameObject _Prefab)
    {
        GameObject obj = Instantiate(_Prefab, transform);
        obj.SetActive(false);

        PooledObject pooledObject = obj.GetComponent<PooledObject>();

        if(pooledObject == null)
        {
            pooledObject = obj.AddComponent<PooledObject>();
        }

        pooledObject.Init(this);

        pools[_Prefab].Enqueue(pooledObject);
        instanceToPrefab[pooledObject] = _Prefab;

    }

    public bool HasPoolForPrefab(GameObject _Prefab)
    {
        return _Prefab != null && pools.ContainsKey(_Prefab);
    }

    public PooledObject GetFromPool(GameObject _Prefab, Vector3 _Pos, Quaternion _Rot)
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

        PooledObject pooledObject = pools[_Prefab].Dequeue();
        pooledObject.transform.position = _Pos;
        pooledObject.transform.rotation = _Rot;
        pooledObject.gameObject.SetActive(true);
        return pooledObject;
    }

    public void ReturnToPool(PooledObject _PooledObject)
    {
        if(_PooledObject == null)
        {
            LogWarning("Tried to return null object to pool");
            return;
        }

        if (!instanceToPrefab.ContainsKey(_PooledObject))
        {
            LogWarning("Returned object is not tracked by this pool");
            _PooledObject.gameObject.SetActive(false);
            return;
        }

        GameObject prefab = instanceToPrefab[_PooledObject];
        _PooledObject.gameObject.SetActive(false);
        _PooledObject.transform.SetParent(transform);
        pools[prefab].Enqueue(_PooledObject);

    }

    void LogWarning(string _Msg)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning($"[ObjectPoolManager] {_Msg}", this);
        }
    }

}