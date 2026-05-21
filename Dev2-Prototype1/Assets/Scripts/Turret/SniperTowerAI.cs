using System.Collections.Generic;
using UnityEngine;

public class SniperTowerAI : MonoBehaviour, IBuffableTower
{
    [Header("----- Debug Settings -----")]
    [SerializeField] bool showBuffDebugLog = true;
    [SerializeField] bool showDebugLog;

    [Header("----- Gen Settings -----")]
    [SerializeField] int cost = 200;

    [Header("----- Refs -----")]
    [SerializeField] Renderer model;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform pivotPos;

    [Header("----- Sniper Settings -----")]
    [SerializeField] int damAmount = 20;
    [SerializeField] float shootRate = 4f;
    [SerializeField] int gunRotSpeed = 8;
    [SerializeField] int faceRotSpeed = 8;
    [SerializeField] int turretFOV = 360;
    [SerializeField] LayerMask LOSMask = ~0;
    [SerializeField] bool ignoreTriggerCollidersForTargeting = true;

    float totalDamBuffPercent;

    Color colorOrig;

    float shootTimer;
    float angleToEnemy;

    List<Transform> enemiesInRange = new List<Transform>();

    Transform enemyPos;
    Vector3 enemyDir;

    void Awake()
    {
        if(model != null)
        {
            colorOrig = model.material.color;
        }
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        CleanEnemyList();
        GetTarget();

        if(enemyPos == null)
        {
            return;
        }

        Vector3 targetPos = GetTargetPos();
        enemyDir = targetPos - transform.position;

        rotToTarget();
        rotGun();

        if(CanSeeEnemy() && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    private void OnDisable()
    {
        ClearDamBuff();
    }

    void CleanEnemyList()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            Transform enemy = enemiesInRange[i];

            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                enemiesInRange.RemoveAt(i);
            }
        }
    }

    bool CanSeeEnemy()
    {
        if(enemyPos == null)
        {
            return false;
        }

        Vector3 targetPos = GetTargetPos();

        Vector3 flatDirToEnemy = targetPos - transform.position;
        flatDirToEnemy.y = 0f;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;

        if (flatDirToEnemy == Vector3.zero || flatForward == Vector3.zero)
        {
            return true;
        }

        angleToEnemy = Vector3.Angle(flatDirToEnemy, transform.forward);

        if(angleToEnemy > turretFOV)
        {
            return false;
        }

        Vector3 startPos;

        if(shootPos != null)
        {
            startPos = shootPos.position;
        }
        else
        {
            startPos = transform.position;
        }

        Vector3 rayDir = targetPos - startPos;

        float rayDist = rayDir.magnitude;

        if(rayDist <= 0f)
        {
            return false;
        }

        if(Physics.Raycast(startPos,rayDir.normalized, out RaycastHit hit, rayDist, LOSMask, QueryTriggerInteraction.Ignore))
        {
            if(hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                return true;
            }

            Transform hitEnemy = GetEnemyTransform(hit.collider);

            if(hitEnemy == enemyPos)
            {
                return true;
            }

            DebugSniper("LOS is blocked by: " + hit.collider.name);

            return false;
        }

        return true;
    }
    
    Transform GetEnemyTransform(Collider _ColToFind)
    {
        if (_ColToFind.CompareTag("Enemy"))
        {
            return _ColToFind.transform;
        }

        Transform currTransform = _ColToFind.transform.parent;

        while(currTransform != null)
        {
            if (currTransform.CompareTag("Enemy"))
            {
                return currTransform;
            }

            currTransform = currTransform.parent;
        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(ignoreTriggerCollidersForTargeting && other.isTrigger)
        {
            DebugSniper("Ignored trigger collider entering range: " + other.name);
            return;
        }

        Transform enemy = GetEnemyTransform(other);

        if(enemy == null)
        {
            return;
        }

        DebugSniper("Sniper rang col entered by enemy: " + enemy.name);

        if(!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }

        if(enemyPos == null)
        {
            enemyPos = enemy;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(ignoreTriggerCollidersForTargeting && other.isTrigger)
        {
            DebugSniper("Ignored trigger col exiting range: " + other.name);
            return;
        }

        Transform enemy = GetEnemyTransform(other);

        if(enemy == null)
        {
            return;
        }

        DebugSniper("Sniper col exited by enemy: " + enemy.name);

        enemiesInRange.Remove(enemy);

        if(enemyPos == enemy)
        {
            enemyPos = null;
            enemyDir = Vector3.zero;
            GetTarget();
        }
    }

    void GetTarget()
    {
        if(enemyPos != null && enemyPos.gameObject.activeInHierarchy)
        {
            return;
        }

        enemyPos = null;

        for(int i = 0; i < enemiesInRange.Count; i++)
        {
            Transform currTarget = enemiesInRange[i];

            if(currTarget != null && currTarget.gameObject.activeInHierarchy)
            {
                enemyPos = currTarget;
                break;
            }
        }
    }

    void shoot()
    {
        shootTimer = 0f;

        if(bullet == null || shootPos == null)
        {
            DebugSniper("Missing bullet or shootPos");
            return;
        }

        Quaternion spawnRot = transform.rotation;

        if (pivotPos != null)
        {
            spawnRot = pivotPos.rotation;
        }

        GameObject spawnedBullet = Instantiate(bullet, shootPos.position, spawnRot);

        SniperBullet sniperBullet = spawnedBullet.GetComponent<SniperBullet>();

        if(sniperBullet == null)
        {
            sniperBullet = spawnedBullet.GetComponentInChildren<SniperBullet>();
        }

        if(sniperBullet != null)
        {
            sniperBullet.Init(enemyPos, damAmount, GetDamMult());
            DebugSniper("Fired sniper projectile at " + enemyPos.name);
        }
        else
        {
            Debug.LogWarning("Sniper bullet prefab is missing SniperBullet script", spawnedBullet);
        }
    }

    void rotGun()
    {
        if(enemyPos == null || pivotPos == null)
        {
            return;
        }

        Vector3 targetPos = GetTargetPos();
        Vector3 gunDir = targetPos - pivotPos.position;

        Debug.DrawRay(pivotPos.position, gunDir, Color.red);

        if(gunDir == Vector3.zero)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(gunDir);
        pivotPos.rotation = Quaternion.Lerp(pivotPos.rotation, rot, gunRotSpeed * Time.deltaTime);
    }

    Vector3 GetTargetPos()
    {
        if (enemyPos == null)
        {
            return transform.position;
        }

        Collider[] enemyCols = enemyPos.GetComponentsInChildren<Collider>();

        for (int i = 0; i < enemyCols.Length; i++)
        {
            if (enemyCols[i] != null && !enemyCols[i].isTrigger)
            {
                return enemyCols[i].bounds.center;
            }
        }

        return enemyPos.position;
    }

    void rotToTarget()
    {
        Vector3 xEnemyDir = new Vector3(enemyDir.x, 0f, enemyDir.z);

        if(xEnemyDir == Vector3.zero)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(xEnemyDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceRotSpeed); 
    }

    public void ResetTurretState()
    {
        shootTimer = 0f;
        angleToEnemy = 0f;
        enemyPos = null;
        enemyDir = Vector3.zero;
        enemiesInRange.Clear();
        ClearDamBuff();

        if(model != null)
        {
            model.material.color = colorOrig;
        }
    }

    void ClearDamBuff()
    {
        totalDamBuffPercent = 0f;
    }

    float GetDamMult()
    {
        return 1f + (totalDamBuffPercent / 100f);
    }

    public int getCost()
    {
        return cost;
    }

    public void AddDamageBuff(float _Percent)
    {
        totalDamBuffPercent += _Percent;
        totalDamBuffPercent = Mathf.Max(0f, totalDamBuffPercent);

        DebugBuff("Added damage buff +" + _Percent + "% | Total Damage Buff" + totalDamBuffPercent + "% | Mult: " + GetDamMult());
    }

    public void RemoveDamageBuff(float _Percent)
    {
        totalDamBuffPercent -= _Percent;
        totalDamBuffPercent = Mathf.Max(0f, totalDamBuffPercent);

        DebugBuff("Removed damage buff -" + _Percent + "% | Total Damage Buff" + totalDamBuffPercent + "% | Mult: " + GetDamMult());
    }

    void DebugBuff(string _MSG)
    {
        if (showBuffDebugLog)
        {
            Debug.Log("SniperBuffLog: " + gameObject.name + " " + _MSG, gameObject);
        }
    }

    void DebugSniper(string _MSG)
    {
        if (showDebugLog)
        {
            Debug.Log("SniperTowerLog: " + gameObject.name + " " + _MSG, gameObject);
        }
    }
}
