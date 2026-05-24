using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TurretAI : MonoBehaviour, IDamage, IBuffableTower
{
    [Header("----- Buff Settings -----")]
    [SerializeField] bool showBuffDebugLog = false;

    float totalDamageBuffPercent;

    [SerializeField] int maxHP = 3;
    int HP;
    [SerializeField] int cost;

    [SerializeField] Renderer model; // Needed to flash model red when damaged
    [SerializeField] NavMeshAgent agent;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform gunPivot;
    [SerializeField] int gunRotateSpeed;
    [SerializeField] int targetFaceSpeed;
    [SerializeField] int FOV;
    [SerializeField] bool ignoreTriggerColForTarget = true;

    [Header("----- Audio Settings -----")]
    [SerializeField] AudioSource audioSrc;
    [SerializeField] AudioClip shootSFX;
    [SerializeField]
    [Range(0f, 1f)] float  shootSFXVol;

    Color colorOrig;

    float shootTimer;
    float angleToEnemy;
    float stoppingDistOrig;

    List<Transform> enemiesInRange = new List<Transform>();

    Transform enemyPos;

    Vector3 enemyDir; // enemy pos - Turret pos
    Vector3 startingPos;

    int HPOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(model != null)
        {
            colorOrig = model.material.color;
        }

        HPOrigin = maxHP;
        HP = maxHP;

        startingPos = transform.position;

        if(agent != null)
        {
            stoppingDistOrig = agent.stoppingDistance;
        }

    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        CleanupEnemyList();
        AcquireTarget();
        
        if(enemyPos == null)
        {
            return;
        }

        Vector3 targetPos = GetTargetPoint();
        enemyDir = targetPos - transform.position;

        rotateToTarget();
        gunRotate();

        if(canSeeEnemy() && shootTimer >= shootRate)
        {
            shoot();
        }

    }

    private void OnDisable()
    {
        ClearDamageBuffs();
    }

    void AcquireTarget()
    {
        if(enemyPos != null && enemyPos.gameObject.activeInHierarchy)
        {
            return;
        }

        enemyPos = null;

        for(int i = 0; i < enemiesInRange.Count; i++)
        {
            Transform target = enemiesInRange[i];

            if(target != null && target.gameObject.activeInHierarchy)
            {
                enemyPos = target;
                break;
            }
        }
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

    void CleanupEnemyList()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
    }

    Transform GetEnemyTransform(Collider _EnemyCol)
    {
        if(_EnemyCol == null)
        {
            return null;
        }

        if (_EnemyCol.CompareTag("Enemy"))
        {
            return _EnemyCol.transform;
        }

        Transform currTransform = _EnemyCol.transform.parent;

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
        if(other == null)
        {
            return;
        }

        if(ignoreTriggerColForTarget && other.isTrigger)
        {
            //Debug.Log("Turret ignored trigger collider entering range: " + other.name);
            return;
        }

        Transform enemy = GetEnemyTransform(other);

        if(enemy == null)
        {
            //DebugBuff("Collider entered range but no Enemy root was found: " + other.name);
            return;
        }

        //Debug.Log($"Turret trigger entered by enemy: {enemy.name}");

        if (!enemiesInRange.Contains(enemy))
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
        if(other == null)
        {
            return;
        }

        if(ignoreTriggerColForTarget && other.isTrigger)
        {
            //Debug.Log("Turret ignored trigger collider exiting range: " + other.name);
            return;
        }

        Transform enemy = GetEnemyTransform(other);

        if(enemy == null)
        {
            return;
        }

        //Debug.Log($"Turret trigger exited by enemy: {enemy.name}");

        enemiesInRange.Remove(enemy);

        if(enemyPos == enemy)
        {
            enemyPos = null;
            enemyDir = Vector3.zero;
            AcquireTarget();
        }
    }

    Vector3 GetTargetPoint()
    {
        if(enemyPos == null)
        {
            return transform.position;
        }

        Collider[] enemyCols = enemyPos.GetComponentsInChildren<Collider>();

        for(int i = 0; i < enemyCols.Length; i++)
        {
            if (enemyCols[i] != null && !enemyCols[i].isTrigger)
            {
                return enemyCols[i].bounds.center;
            }
        }

        return enemyPos.position;
    }

    void shoot()
    {
        shootTimer = 0;

        PlayTurretSFX(shootSFX, shootSFXVol);

        if(bullet != null)
        {
            GameObject spawnedBullet = Instantiate(bullet, shootPos.position, gunPivot.rotation);

            //Debug.DrawRay(shootPos.position, gunPivot.forward * 100f, Color.yellow, 1f);

            damage bulletDamage = spawnedBullet.GetComponent<damage>();

            if(bulletDamage == null)
            {
                bulletDamage = spawnedBullet.GetComponentInChildren<damage>();
            }

            if(bulletDamage != null)
            {
                bulletDamage.SetDamMult(GetDamMult());

                //DebugBuff("Spawned bullet with damage multiplier of: " + GetDamMult());
            }
            //else
            //{
            //    DebugBuff("Spawned bullet has no damage script");
            //}

        }
    }

    void gunRotate()
    {
        if(enemyPos == null || gunPivot == null)
        {
            return;
        }

        Vector3 targetPos = GetTargetPoint();
        Vector3 gunDir = targetPos - gunPivot.position;

        //Debug.DrawRay(gunPivot.position, gunDir, Color.red);

        if(gunDir == Vector3.zero)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(gunDir);
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation,rot, gunRotateSpeed * Time.deltaTime);
    }

    void rotateToTarget()
    {
        Vector3 xEnemyDir = new Vector3(enemyDir.x, 0, enemyDir.z);

        if(xEnemyDir == Vector3.zero)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(xEnemyDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
    }

    public void ResetTurretState()
    {
        HP = HPOrigin;
        shootTimer = 0f;
        angleToEnemy = 0f;
        enemyPos = null;
        enemyDir = Vector3.zero;
        enemiesInRange.Clear();
        ClearDamageBuffs();

        if(model != null)
        {
            model.material.color = colorOrig;
        }

        if(agent != null)
        {
            agent.enabled = true;
            agent.ResetPath();
            agent.stoppingDistance = stoppingDistOrig;
            agent.velocity = Vector3.zero;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP <= 0)
        {
            PooledTurret pooledTurret = GetComponent<PooledTurret>();

            if(pooledTurret != null)
            {
                pooledTurret.RemoveFromManager();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void AddDamageBuff(float _Percent)
    {
        totalDamageBuffPercent += _Percent;
        totalDamageBuffPercent = Mathf.Max(0f, totalDamageBuffPercent);

        //DebugBuff("Added damage buff: " + _Percent + "% | Total Damage Buff: " +  totalDamageBuffPercent + "% | Multiplier: " + GetDamMult());
    }

    public void RemoveDamageBuff(float _Percent)
    {
        totalDamageBuffPercent -= _Percent;
        totalDamageBuffPercent = Mathf.Max(0f, totalDamageBuffPercent);

        //DebugBuff("Removed damage buff: -" + _Percent + "% | Total damage Buff: " + totalDamageBuffPercent + "% | Multiplier: " + GetDamMult());
    }

    void ClearDamageBuffs()
    {
        totalDamageBuffPercent = 0f;
    }

    float GetDamMult()
    {
        return 1f + (totalDamageBuffPercent / 100f);
    }

    void PlayTurretSFX(AudioClip _AudioClip, float _Vol)
    {
        if(_AudioClip == null)
        {
            return;
        }

        if(audioSrc == null)
        {
            //Debug.LogWarning("Turret tried to play audio but it has no src");
            return;
        }

        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(audioSrc, _Vol, SoundCategory.Trap, true);
        }
        else
        {
            audioSrc.PlayOneShot(_AudioClip, Mathf.Clamp01(_Vol));
        }

    }

    void DebugBuff(string _MSG)
    {
        if (showBuffDebugLog)
        {
            //Debug.Log("Turret Buff: " + gameObject.name + _MSG, gameObject);
        }
    }

    public int getCost()
    {
        return cost;
    }
}
