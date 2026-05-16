using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GatlingTowerAI : MonoBehaviour, IBuffableTower
{
    [Header("----- Refs -----")]
    [SerializeField] Renderer model;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform pivotPos;

    [Header("----- General Settings -----")]
    [SerializeField] int gunRotSpeed = 8;
    [SerializeField] int faceRotSpeed = 8;
    [SerializeField] int FOV = 360;

    [Header("----- Gatling Settings -----")]
    [SerializeField] float baseShootRate = 0.12f;
    [SerializeField] float rampedShootRate = 0.05f;
    [SerializeField] float timeTilRampUp = 3f;
    [SerializeField] float keepRampUpSwitchDist = 2f;

    [Header("----- Debug -----")]
    [SerializeField] bool showBuffDebugLog = true;
    [SerializeField] bool showDebugLog = true;

    Color colorOrig;

    float totalDamageBuffPercent;

    float shootTimer;
    float rampUpTimer;
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

        if (!IsValidTarget(enemyPos))
        {
            ChangeTarget(FindNextTarget());
        }

        if(enemyPos == null)
        {
            ResetGatlingRamp();
            return;
        }

        Vector3 targetPos = GetTargetPos(enemyPos);
        enemyDir = targetPos - transform.position;

        RotToTarget();
        RotGun();

        if (canSeeEnemy())
        {
            rampUpTimer += Time.deltaTime;

            float currShootRate = GetCurrentShootRate();

            if(shootTimer >= currShootRate)
            {
                shoot();
            }
        }
        else
        {
            rampUpTimer = 0f;
        }
    }

    private void OnDisable()
    {
        ClearDamBuff();
        ResetGatlingRamp();
        enemiesInRange.Clear();
        enemyPos = null;
        enemyDir = Vector3.zero;
    }

    bool IsValidTarget(Transform _Target)
    {
        if(_Target == null)
        {
            return false;
        }

        if (!_Target.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (!enemiesInRange.Contains(_Target))
        {
            return false;
        }

        return true;
    }

    float GetCurrentShootRate()
    {
        if(rampUpTimer >= timeTilRampUp)
        {
            return rampedShootRate;
        }

        return baseShootRate;
    }

    bool canSeeEnemy()
    {
        if(enemyPos == null)
        {
            return false;
        }

        angleToEnemy = Vector3.Angle(enemyDir, transform.forward);

        if(angleToEnemy <= FOV)
        {
            return true;
        }

        return false;
    }

    void CleanEnemyList()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            DebugGatling("Trigger entered by: " +  other.name);

            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
            }

            if(enemyPos == null)
            {
                ChangeTarget(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            DebugGatling("Trigger exited by: " + other.name);

            enemiesInRange.Remove(other.transform);

            if(enemyPos == other.transform)
            {
                ChangeTarget(FindNextTarget());
            }
        }
    }

    Transform FindNextTarget()
    {
        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            Transform target = enemiesInRange[i];

            if(target != null && target.gameObject.activeInHierarchy)
            {
                return target;
            }
        }

        return null;
    }

    void ChangeTarget(Transform _NewTarget)
    {
        if(enemyPos == _NewTarget)
        {
            return;
        }

        Transform prevTarget = enemyPos;

        bool keepRamp = ShouldKeepRamp(prevTarget, _NewTarget);

        enemyPos = _NewTarget;
        enemyDir = Vector3.zero;

        if (!keepRamp)
        {
            ResetGatlingRamp();
        }

        DebugBuff("Changed target. Keeping ramp: " + keepRamp);
    }

    void ResetGatlingRamp()
    {
        rampUpTimer = 0f;
        shootTimer = 0f;
    }

    bool ShouldKeepRamp(Transform _PrevTarget, Transform _NewTarget)
    {
        if(_PrevTarget == null || _NewTarget == null)
        {
            return false;
        }

        if(keepRampUpSwitchDist <= 0f)
        {
            return false;
        }

        float distBetweenTargets = Vector3.Distance(_PrevTarget.position, _NewTarget.position);

        return distBetweenTargets <= keepRampUpSwitchDist;
    }

    void shoot()
    {
        shootTimer = 0f;

        if(bullet == null || shootPos == null)
        {
            DebugGatling("Missing bullet or shootPos");
            return;
        }

        Quaternion bulletRot;

        if(pivotPos != null)
        {
            bulletRot = pivotPos.rotation;
        }
        else
        {
            bulletRot = transform.rotation;
        }

        GameObject spawnedBullet = Instantiate(bullet, shootPos.position, bulletRot);

        damage bulletDam = spawnedBullet.GetComponent<damage>();

        if(bulletDam == null)
        {
            bulletDam = spawnedBullet.GetComponentInChildren<damage>();
        }

        if(bulletDam != null)
        {
            bulletDam.SetDamMult(GetDamMult());
            DebugBuff("Just spawned Gatling Bullet with mult: " + GetDamMult());
        }
        else
        {
            DebugBuff("Spawned Gatling bullet has no damage script");
        }
    }

    void RotGun()
    {
        if(enemyPos == null || pivotPos == null)
        {
            return;
        }

        Vector3 targetPos = GetTargetPos(enemyPos);

        Vector3 gunDir = targetPos - pivotPos.position;

        Debug.DrawRay(pivotPos.position, gunDir, Color.red);

        if(gunDir == Vector3.zero)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(gunDir);
        pivotPos.rotation = Quaternion.Lerp(pivotPos.rotation, rot, gunRotSpeed * Time.deltaTime);
    }

    void RotToTarget()
    {
        Vector3 xEnemyDir = new Vector3(enemyDir.x, 0f, enemyDir.z);

        if(xEnemyDir == Vector3.zero)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(xEnemyDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceRotSpeed * Time.deltaTime);
    }

    Vector3 GetTargetPos(Transform _Target)
    {
        if(_Target == null)
        {
            return transform.position;
        }

        Collider enemyCol = _Target.GetComponentInChildren<Collider>();

        if(enemyCol != null)
        {
            return enemyCol.bounds.center;
        }

        return _Target.position;
    }

    public void ResetTurretState()
    {
        shootTimer = 0f;
        rampUpTimer = 0f;
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

    public void AddDamageBuff(float _Percent)
    {
        totalDamageBuffPercent += _Percent;
        totalDamageBuffPercent = Mathf.Max(0f, totalDamageBuffPercent);

        DebugBuff("Removed damage buff: " + _Percent + "% | Total Damage Buff: " + totalDamageBuffPercent + "% | Multiplier: " + GetDamMult());
    }

    public void RemoveDamageBuff(float _Percent)
    {
        totalDamageBuffPercent -= _Percent;
        totalDamageBuffPercent = Mathf.Max(0f, totalDamageBuffPercent);

        DebugBuff("Removed damage buff: -" + _Percent + "% | Total Damage Buff: " + totalDamageBuffPercent + "% | Multiplier: " + GetDamMult());
    }

    void ClearDamBuff()
    {
        totalDamageBuffPercent = 0f;
    }

    float GetDamMult()
    {
        return 1f + (totalDamageBuffPercent / 100f);
    }

    void DebugBuff(string _MSG)
    {
        if (showBuffDebugLog)
        {
            Debug.Log("Gatling Buff: " + gameObject.name + " " + _MSG, gameObject);
        }
    }

    void DebugGatling(string _MSG)
    {
        if (showDebugLog)
        {
            Debug.Log("GatlingTowerAI: " + gameObject.name + " " + _MSG, gameObject);
        }
    }

    public int getCost()
    {
        return getCost();
    }
}