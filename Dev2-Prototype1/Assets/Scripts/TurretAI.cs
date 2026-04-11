using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TurretAI : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] int Cost;

    [SerializeField] Renderer model; // Needed to flash model red when damaged
    [SerializeField] NavMeshAgent agent;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform gunPivot;
    [SerializeField] int gunRotateSpeed;
    [SerializeField] int targetFaceSpeed;


    Color colorOrig;
    

    float shootTimer;
    float angleToPlayer;
    float stoppingDistOrig;

    bool enemyInRange;
    Transform enemyPos;

    Vector3 enemyDir; // enemy pos - Turret pos
    Vector3 startingPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;

    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        enemyDir = enemyPos.position - transform.position;
        if (enemyInRange)
        {
           // rotateToTarget();
            gunRotate();
            if(shootTimer>= shootRate)
            {
                shoot();
            }
        }
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
    }
    bool canSeeEnemy()
    {
        return true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EnemyAI>() != null)
        {
            enemyInRange = true;
            enemyPos = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<EnemyAI>() != null)
        {
            enemyInRange = false;
            enemyPos = null;
        }
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
        Quaternion rot = Quaternion.LookRotation(enemyDir);
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rot, Time.deltaTime * gunRotateSpeed);
    }
    void rotateToTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(enemyDir.x, enemyDir.y, enemyDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
    }
}
