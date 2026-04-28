using UnityEngine;

public class PooledEnemy : PooledObject
{
    WaveManager owningWaveManager;

    bool removedFromWave;
    EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    public override void Init(ObjectPoolManager _PoolManager)
    {
        base.Init(_PoolManager);
    }
    
    public void OnSpawned(WaveManager _WaveManager)
    {
        owningWaveManager = _WaveManager;
        removedFromWave = false;
        ResetState();
    }

    public override void OnSpawned()
    {
        removedFromWave = false;
        ResetState();
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

        ReturnToPool();
    }

    public override void OnReturnedToPool()
    {
        ResetState();
        owningPoolManager = null;
        removedFromWave = false;
    }

    void ResetState()
    {
        if(enemyAI != null)
        {
            enemyAI.ResetEnemyState();
        }
    }

}
