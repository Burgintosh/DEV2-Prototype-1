using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{

    [SerializeField] int maxHP = 3;
    int HP;

    [SerializeField] Renderer model; // Needed to flash model red when damaged
    [SerializeField] NavMeshAgent agent;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform gunPivot;
    [SerializeField] int gunRotateSpeed;
    [SerializeField] int targetFaceSpeed;
    [SerializeField] int FOV;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int currencyDrop;

    Color colorOrig;

    float shootTimer;
    float angleToPlayer;
    float angleToNexus;
    float stoppingDistOrig;

    bool playerInRange;
    bool nexusInRange;

    Vector3 playerDir; // player pos - enemy pos
    Vector3 nexusDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        startingPos = transform.position;

        if(agent != null)
        {
            stoppingDistOrig = agent.stoppingDistance;
        }

        HP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        //playerDir = gamemanager.instance.player.transform.position - transform.position; // Vile

        if (nexusInRange && canSeeNexus())
        {

        }
        else if (playerInRange && canSeePlayer())
        {

        }
        else
        {
            if(gamemanager.instance.Nexus == null)
            {
                //Debug.Log("Nexus is null or not found");
                return;
            }

            agent.SetDestination(gamemanager.instance.Nexus.transform.position);
        }
    }

    bool canSeePlayer() // 
    {
        playerDir = gamemanager.instance.player.transform.position - transform.position; // Still vile
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);


        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= FOV && !nexusInRange)
            {
                rotateToTarget();
                gunRotate();
                // maybe add a short wait time
                if (shootTimer >= shootRate)
                    shoot();

                agent.SetDestination(gamemanager.instance.player.transform.position);

                return true;
            }
        }
        return false;
    }

    void shoot()
    {
        shootTimer = 0;
        if (bullet != null)
            SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.enemyShootSound);
            Instantiate(bullet, shootPos.position, gunPivot.rotation);
    }

    void gunRotate()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rot, Time.deltaTime * gunRotateSpeed); // Lerp() is rotation over time, parameters(current rot, destination rot, time to rotate)
    }

    void rotateToTarget() // Can pass a gameObject in here but just using player for now so keeping it simple
    {
        //Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.rotation.y, playerDir.z));
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
        if (other.CompareTag("Nexus"))
        {
            nexusInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
        if (other.CompareTag("Nexus"))
        {
            nexusInRange = false;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP <= 0)
        {
            gamemanager.instance.currencyManager.AddCurrency(currencyDrop);
            PooledEnemy pooledEnemy = GetComponent<PooledEnemy>();

            if(pooledEnemy != null)
            {
                pooledEnemy.RemoveFromWave();
            }
            else
            {
                gameObject.SetActive(false);
            }

        }
        else
        {
            StartCoroutine(flashRed()); // Object must still exist for coroutine to finish. Have to put this in an else
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    public void ResetEnemyState()
    {
        HP = maxHP;
        shootTimer = 0f;
        angleToPlayer = 0f;
        playerInRange = false;
        playerDir = Vector3.zero;
        angleToNexus = 0f;
        nexusInRange = false;
        nexusDir = Vector3.zero;

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
    bool canSeeNexus()
    {
        if (gamemanager.instance.Nexus == null) return false;
        nexusDir = gamemanager.instance.Nexus.transform.position - transform.position;
        angleToNexus = Vector3.Angle(nexusDir, transform.forward);


        Debug.DrawRay(transform.position, nexusDir);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, nexusDir, out hit))
        {
            if (hit.collider.CompareTag("Nexus") && angleToNexus <= FOV && nexusInRange)
            {
                Quaternion rotBody = Quaternion.LookRotation(new Vector3(nexusDir.x, 0, nexusDir.z));
                transform.rotation = Quaternion.Lerp(transform.rotation, rotBody, Time.deltaTime * targetFaceSpeed);
                Quaternion rotGun = Quaternion.LookRotation(nexusDir);
                gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rotGun, Time.deltaTime * gunRotateSpeed);
                // maybe add a short wait time
                if (shootTimer >= shootRate)
                    shoot();

                agent.SetDestination(gamemanager.instance.Nexus.transform.position);

                return true;
            }
        }
        return false;
    }
}
