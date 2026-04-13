using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TurretAI : MonoBehaviour, IDamage
{
    [SerializeField] int maxHP = 3;
    int HP;
    [SerializeField] int Cost;

    [SerializeField] Renderer model; // Needed to flash model red when damaged
    [SerializeField] NavMeshAgent agent;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform gunPivot;
    [SerializeField] int gunRotateSpeed;
    [SerializeField] int targetFaceSpeed;
    [SerializeField] int FOV;


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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Turret trigger entered by: {other.name}");

            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
            }

            if(enemyPos == null)
            {
                enemyPos = other.transform;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Turret trigger exited by: {other.name}");

            enemiesInRange.Remove(other.transform);

            if(enemyPos == other.transform)
            {
                enemyPos = null;
                enemyDir = Vector3.zero;
                AcquireTarget();
            }
        }
    }

    Vector3 GetTargetPoint()
    {
        if(enemyPos == null)
        {
            return transform.position;
        }

        Collider enemyCol = enemyPos.GetComponentInChildren<Collider>();

        if(enemyCol != null)
        {
            return enemyCol.bounds.center;
        }

        return enemyPos.position;
    }

    void shoot()
    {
        shootTimer = 0;
        if(bullet != null)
        {
            Instantiate(bullet, shootPos.position, gunPivot.rotation);
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

        Debug.DrawRay(gunPivot.position, gunDir, Color.red);

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
}
