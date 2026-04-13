using UnityEngine;

public class PooledTurret : PooledObject
{
    TurretAI turretAI;
    TurretManager owningTurretManager;

    private void Awake()
    {
        turretAI = GetComponent<TurretAI>();
    }

    public override void Init(ObjectPoolManager _PoolManager)
    {
        base.Init(_PoolManager);
    }

    public override void OnSpawned()
    {
        ResetState();
    }

    public void OnSpawned(TurretManager _TurretManager)
    {
        owningTurretManager = _TurretManager;
        ResetState();
    }

    public void RemoveFromManager()
    {
        if(owningTurretManager != null)
        {
            owningTurretManager.NotifyTurretRemoved(this);
        }
        else
        {
            ReturnToPool();
        }
    }

    public override void OnReturnedToPool()
    {
        ResetState();
        owningTurretManager = null;
    }

    void ResetState()
    {
        if(turretAI != null)
        {
            turretAI.ResetTurretState();
        }
    }

}
