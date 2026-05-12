using System.Collections.Generic;
using UnityEngine;

public class DamageBuffTower : MonoBehaviour
{
    [Header("----- Settings -----")]
    [SerializeField] SphereCollider rangeTrigger;
    [SerializeField] float buffRange = 8f;

    [Range(0f, 500f)][SerializeField] float damageBuffPercent = 25f;

    [SerializeField] LayerMask towerLayerMask = ~0;

    [Header("----- Debug -----")]
    [SerializeField] bool showDebugLogs = true;

    Dictionary<IBuffableTower, float> buffedTowers = new Dictionary<IBuffableTower, float>();

    private void Awake()
    {
        SetupRangeTrigger();
    }

    private void Start()
    {
        ApplyBuffToTowersAlreadyInRange();
    }

    private void OnValidate()
    {
        SetupRangeTrigger();
    }

    private void OnDisable()
    {
        RemoveAllBuffs();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryToBuff(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TryRemoveBuff(other);
    }

    void SetupRangeTrigger()
    {
        if(rangeTrigger == null)
        {
            return;
        }

        rangeTrigger.isTrigger = true;
        rangeTrigger.radius = buffRange;
    }

    void ApplyBuffToTowersAlreadyInRange()
    {
        Collider[] towerHits = Physics.OverlapSphere(transform.position, buffRange, towerLayerMask, QueryTriggerInteraction.Ignore);

        for(int i = 0; i < towerHits.Length; i++)
        {
            TryToBuff(towerHits[i]);
        }
    }

    void TryToBuff(Collider _Other)
    {
        CleanupRemovedBuffedTowers();

        if (_Other.isTrigger)
        {
            return;
        }

        IBuffableTower buffableTower = _Other.GetComponentInParent<IBuffableTower>();

        if(buffableTower == null)
        {
            return;
        }

        if (buffedTowers.ContainsKey(buffableTower))
        {
            return;
        }

        float appliedPercent = damageBuffPercent;

        buffedTowers.Add(buffableTower, appliedPercent);
        buffableTower.AddDamageBuff(appliedPercent);

        DebugBuff("Added damage buff to: " + _Other.name + " | Buff percent: +" + appliedPercent + "%");
    }

    void TryRemoveBuff(Collider _Other)
    {
        CleanupRemovedBuffedTowers();

        if(_Other.isTrigger)
        {
            return;
        }

        IBuffableTower buffableTower = _Other.GetComponentInParent<IBuffableTower>();

        if(buffableTower == null)
        {
            return;
        }

        if (!buffedTowers.ContainsKey(buffableTower))
        {
            return;
        }

        float appliedPercent = buffedTowers[buffableTower];

        buffedTowers.Remove(buffableTower);
        buffableTower.RemoveDamageBuff(appliedPercent);

        DebugBuff("Removed damage buff: " + _Other.name + " | Buff percent -" + appliedPercent + "%");
    }

    void RemoveAllBuffs()
    {
        foreach(KeyValuePair<IBuffableTower, float> currBuffedTower in buffedTowers)
        {
            if (!IsBuffableTowerStillValid(currBuffedTower.Key))
            {
                continue;
            }

            currBuffedTower.Key.RemoveDamageBuff(currBuffedTower.Value);
        }

        buffedTowers.Clear();
    }

    bool IsBuffableTowerStillValid(IBuffableTower _Tower)
    {
        if(_Tower == null)
        {
            return false;
        }

        MonoBehaviour towerMono = _Tower as MonoBehaviour;

        if(towerMono == null)
        {
            return false;
        }

        if (!towerMono.gameObject.activeInHierarchy)
        {
            return false;
        }

        return true;
    }

    void CleanupRemovedBuffedTowers()
    {
        List<IBuffableTower> towersToRemove = new List<IBuffableTower>();

        foreach(KeyValuePair<IBuffableTower, float> currBuffedTower in buffedTowers)
        {
            if (!IsBuffableTowerStillValid(currBuffedTower.Key))
            {
                towersToRemove.Add(currBuffedTower.Key);
            }
        }

        for (int i = 0; i < towersToRemove.Count; i++)
        {
            buffedTowers.Remove(towersToRemove[i]);
        }
    }

    void DebugBuff(string _MSG)
    {
        if (showDebugLogs)
        {
            Debug.Log("Damage buff tower: " + gameObject.name + _MSG, gameObject);
        }
    }
}
