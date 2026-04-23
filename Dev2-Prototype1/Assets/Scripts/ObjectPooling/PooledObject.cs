using UnityEngine;

public class PooledObject : MonoBehaviour
{
    protected ObjectPoolManager owningPoolManager;

    public virtual void Init(ObjectPoolManager _PoolManager)
    {
        owningPoolManager = _PoolManager;
    }

    public virtual void OnSpawned()
    {

    }

    public virtual void OnReturnedToPool()
    {

    }

    public virtual void ReturnToPool()
    {
        if(owningPoolManager == null)
        {
            Debug.LogWarning($"{name} has no owning pool manager");
            gameObject.SetActive(false);
            return;
        }

        owningPoolManager.ReturnToPool(this);
    }

}
