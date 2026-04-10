using UnityEngine;

public class PooledEnemy : MonoBehaviour
{
    ObjectPoolManager owningPoolManager;
    WaveManager owningWaveManager;

    bool removedFromWave;

    public void Init(ObjectPoolManager _PoolManager)
    {
        owningPoolManager = _PoolManager;
    }
    
    public void OnSpawned(WaveManager _WaveManager)
    {
        owningWaveManager = _WaveManager;
        removedFromWave = false;
        //ResetState();
    }

    public void RemoveFromWave()
    {
        if (removedFromWave)
        {
            return;
        }

        removedFromWave = true;

        if(owningWaveManager != null)
        {
            owningWaveManager.NotifyEnemyRemoved(this);
        }

        // ReturnToPool();
    }

    public void ReturnToPool()
    {
        if(owningPoolManager == null)
        {
            Debug.Log("Prefab had no owning pool manager");
            gameObject.SetActive(false);
            return;
        }

        // ResetState();
        owningPoolManager.ReturnToPool(this);
    }

    protected virtual void ResetState()
    {

    }

}
