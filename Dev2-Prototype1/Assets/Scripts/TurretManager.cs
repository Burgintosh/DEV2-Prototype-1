using System.Collections.Generic;
using UnityEngine;

public class TurretManager : MonoBehaviour
{
    //[SerializeField] KeyCode testBuildKey = KeyCode.T;
    [SerializeField] Transform testBuildPoint;

    [SerializeField] ObjectPoolManager poolManager;
    [SerializeField] GameObject turretPrefab;
    [SerializeField] bool showDebugLogs = true;

    List<PooledTurret> activeTurrets = new List<PooledTurret>();

    // Was For Testing Purposes
    //void Update()
    //{
    //    if (Input.GetKeyDown(testBuildKey))
    //    {
    //        if(testBuildPoint != null)
    //        {
    //            BuildTurret(testBuildPoint.position, testBuildPoint.rotation);
    //        }
    //        else
    //        {
    //            LogWarning("Test build point is not assigned");
    //        }

    //    }
    //}

    private void Start()
    {
        ValidateSetup();
    }

    public PooledTurret BuildTurret(Vector3 _Pos, Quaternion _Rot)
    {
        if(poolManager == null)
        {
            LogWarning("Pool manager is not assigned");
            return null;
        }

        if(turretPrefab == null)
        {
            LogWarning("Turret prefab is not assigned");
            return null;
        }

        PooledObject pooledObj = poolManager.GetFromPool(turretPrefab, _Pos, _Rot);

        if(pooledObj == null)
        {
            LogWarning("Failed to get turret from pool");
            return null;
        }

        PooledTurret turret = pooledObj as PooledTurret;

        if(turret == null)
        {
            LogWarning($"Spawned pooled object from {turretPrefab.name} is not a PooledTurret");
            pooledObj.ReturnToPool();
            return null;
        }

        // Placing this in BuildPlacementController
        //if (!gamemanager.instance.currencyManager.canBuy(turretPrefab.GetComponent<TurretAI>().getCost()))
        //{
        //    return null;
        //}

        //gamemanager.instance.currencyManager.SpendCurrency(turretPrefab.GetComponent<TurretAI>().getCost());

        turret.OnSpawned(this);

        if(!activeTurrets.Contains(turret))
        {
            activeTurrets.Add(turret);
        }

        return turret;
    }

    public void NotifyTurretRemoved(PooledTurret _Turret)
    {
        RemoveTurret(_Turret);
    }

    public void RemoveTurret(PooledTurret _Turret)
    {
        if(_Turret == null)
        {
            LogWarning("Attempting to remove a null turret");
            return;
        }

        activeTurrets.Remove(_Turret);
        _Turret.ReturnToPool();
    }

    public IReadOnlyList<PooledTurret> GetActiveTurrets()
    {
        return activeTurrets;
    }

    void ValidateSetup()
    {
        if(poolManager == null)
        {
            LogWarning("Pool Manager is not assigned");
        }

        if(turretPrefab == null)
        {
            LogWarning("Turret prefab is not assigned");
        }

        if(poolManager != null && turretPrefab != null && !poolManager.HasPoolForPrefab(turretPrefab))
        {
            LogWarning("No pool exists for the turret prefab");
        }
    }

    void LogWarning(string _Msg)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning($"[TurretManager] {_Msg}", this);
        }
    }


    public int GetTurretCost()
    {
        if (turretPrefab != null)
        {
            TurretAI turretAIScript = turretPrefab.GetComponentInChildren<TurretAI>();

            if (turretAIScript != null)
            {
                return turretAIScript.getCost();
            }
            else
            {
                Debug.LogWarning("TurretAI script could not be found on the prefab or its children.");
            }
        }
        return 0;
    }
}
