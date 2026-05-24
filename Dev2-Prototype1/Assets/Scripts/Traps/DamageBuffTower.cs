using System.Collections.Generic;
using UnityEngine;

public class DamageBuffTower : MonoBehaviour
{
    [Header("----- Settings -----")]
    [SerializeField] SphereCollider rangeTrigger;
    [SerializeField] float buffRange = 8f;

    [Range(0f, 500f)][SerializeField] float damageBuffPercent = 25f;

    [SerializeField] LayerMask towerLayerMask = ~0;

    [SerializeField] float cleanupCheckRate = 0.25f;
    float beamCleanupTimer;

    [Header("----- Beam Visuals -----")]
    [SerializeField] Transform beamStartPos;
    [SerializeField] Material beamMat;
    [SerializeField] float beamWidth = 0.05f;
    [SerializeField] Color beamColor = Color.cyan;

    [Header("----- Debug -----")]
    [SerializeField] bool showDebugLogs = true;

    Dictionary<IBuffableTower, float> buffedTowers = new Dictionary<IBuffableTower, float>();

    Dictionary<IBuffableTower, GameObject> buffBeams = new Dictionary<IBuffableTower, GameObject>();

    private void Awake()
    {
        SetupRangeTrigger();
    }

    private void Update()
    {
        beamCleanupTimer += Time.deltaTime;

        if(beamCleanupTimer >= cleanupCheckRate)
        {
            beamCleanupTimer = 0f;

            CleanupRemovedBuffedTowers();
        }
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

        MonoBehaviour tower = buffableTower as MonoBehaviour;

        float appliedPercent = damageBuffPercent;

        buffedTowers.Add(buffableTower, appliedPercent);
        buffableTower.AddDamageBuff(appliedPercent);

        GameObject beamObj = CreateBeamToTower(tower);

        if(beamObj != null)
        {
            buffBeams[buffableTower] = beamObj;
        }

        //DebugBuff("Added damage buff to: " + _Other.name + " | Buff percent: +" + appliedPercent + "%");
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

        RemoveBeam(buffableTower);

        //DebugBuff("Removed damage buff: " + _Other.name + " | Buff percent -" + appliedPercent + "%");
    }

    GameObject CreateBeamToTower(MonoBehaviour _Tower)
    {
        if(_Tower == null)
        {
            return null;
        }

        GameObject beamObj = new GameObject("DamageBuffBeam " + _Tower.name);
        beamObj.transform.SetParent(transform);

        LineRenderer lineRend = beamObj.AddComponent<LineRenderer>();

        lineRend.useWorldSpace = true;
        lineRend.positionCount = 2;

        lineRend.startWidth = beamWidth;
        lineRend.endWidth = beamWidth;

        lineRend.startColor = beamColor;
        lineRend.endColor = beamColor;

        if(beamMat != null)
        {
            lineRend.material = beamMat;
        }

        lineRend.SetPosition(0, GetBeamStartPos());
        lineRend.SetPosition(1, _Tower.transform.position);

        return beamObj;
    }

    Vector3 GetBeamStartPos()
    {
        if(beamStartPos != null)
        {
            return beamStartPos.position;
        }

        return transform.position;
    }

    void RemoveBeam(IBuffableTower _Tower)
    {
        if(_Tower == null)
        {
            return;
        }

        if (!buffBeams.ContainsKey(_Tower))
        {
            return;
        }

        GameObject beamObj = buffBeams[_Tower];

        if(beamObj != null)
        {
            Destroy(beamObj);
        }

        buffBeams.Remove(_Tower);
    }

    void RemoveAllBuffs()
    {
        foreach(KeyValuePair<IBuffableTower, float> currBuffedTower in buffedTowers)
        {
            if (IsBuffableTowerStillValid(currBuffedTower.Key))
            {
                currBuffedTower.Key.RemoveDamageBuff(currBuffedTower.Value);
            }

            RemoveBeam(currBuffedTower.Key);
        }

        buffedTowers.Clear();
        buffBeams.Clear();
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
            RemoveBeam(towersToRemove[i]);
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
