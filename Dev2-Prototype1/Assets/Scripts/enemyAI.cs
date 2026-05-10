using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    enum TargetPriority
    {
        LOWHEALTH,
        CLOSEST,
        ORDER
    };
    [Header("Sound")]
    [Range(0f, 1f)]
    [SerializeField] float enemyShootVol = 0.5f;

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
    [SerializeField] TargetPriority targetPriority;

    [SerializeField] int currencyDrop;

    Color colorOrig;

    
    Nexus currTarget;

    float shootTimer;
    float afkTimer;
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
            //agent.ResetPath();
        }

        HP = maxHP;
        changeTarget();
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        if(agent.velocity.magnitude < 1)
            afkTimer += Time.deltaTime;
        else
            afkTimer = 0;


        //playerDir = gamemanager.instance.player.transform.position - transform.position; // Vile
        if (currTarget == null && NexusManager.nexusManagerInstance.nexusCount > 0)
        {
            StartCoroutine(CheckTarget());
            return;
        }
        if (nexusInRange && canSeeNexus())
        {

        }
        else if (playerInRange && canSeePlayer())
        {

        }
        else if (!agent.isOnNavMesh)
        {
            ResetAgentToMesh();
        }
        //else if (!agent.pathPending && afkTimer > 5)
        else if (currTarget != null && afkTimer > 5)
        {
            //Debug.Log("Before reset: " + agent.pathStatus);
            agent.ResetPath();
            if (!agent.SetDestination(currTarget.transform.position))
            {
                Debug.Log("RUH ROH RAGGY I CAN'T FIND A NEXUS");
            }
            //Debug.Log("After Reset: " + agent.pathStatus);
            shootTimer = 0;
        }
        else
        {
            if (currTarget != null && !agent.SetDestination(currTarget.transform.position))
            {
                Debug.Log("RUH ROH RAGGY I CAN'T FIND A NEXUS but in else");
            }
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

        if(bullet == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.enemyShootSound, enemyShootVol, SoundCategory.Enemy);
        }

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
    //the compare tag has to be there or the guys won't shoot at the nexus
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
        //currTargetNexus = -1;
        currTarget = null;
        if(model != null)
        {
            model.material.color = colorOrig;
        }

        if(agent != null)
        {
            agent.enabled = true;
            //agent.ResetPath();
            //agent.stoppingDistance = stoppingDistOrig;
            agent.velocity = Vector3.zero;
            agent.Warp(transform.position);
        }
    }
    bool canSeeNexus()
    {
        if (currTarget == null) return false;
        nexusDir = currTarget.transform.position - transform.position;
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

                agent.SetDestination(currTarget.transform.position);

                return true;
            }
        }
        return false;
    }
    void changeTarget()
    {
        nexusInRange = false;
        
        if(targetPriority == TargetPriority.LOWHEALTH)
        {
            currTarget = LowHealthSearch();
        }
        if (targetPriority == TargetPriority.CLOSEST)
        {
            currTarget = DistanceSearch();
        }
        if (targetPriority == TargetPriority.ORDER)
        {
            currTarget = OrderSearch();
        }
        if(currTarget == null)
        {
            Debug.Log("No Path Found");
        }
        else
        {
            agent.SetDestination(currTarget.transform.position);
        }
    }
    IEnumerator CheckTarget()
    {
        changeTarget();
        yield return new WaitForSeconds(0.5f);
    }

    public void ResetAgentToMesh()
    {
        NavMeshHit hit;
        // Search within a small radius (typically 1-2x agent height)
        float searchRadius = 2.0f;

        if (NavMesh.SamplePosition(agent.transform.position, out hit, searchRadius, NavMesh.AllAreas))
        {
            // Warp the agent to the actual sampled position on the mesh
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning("No NavMesh found near agent position!");
        }
    }
    Nexus LowHealthSearch()
    {
        Nexus Temp = null;
        float lowestHP = Mathf.Infinity;
        for(int i = 0; i < NexusManager.nexusManagerInstance.nexusList.Count; ++i)
        {
            if (NexusManager.nexusManagerInstance.nexusList[i] != null &&
                NexusManager.nexusManagerInstance.nexusList[i].GetCurrHP() < lowestHP)
            {
                Temp = NexusManager.nexusManagerInstance.nexusList[i];
                lowestHP = NexusManager.nexusManagerInstance.nexusList[i].GetCurrHP();
            }
        }

        return Temp;
    }
    Nexus DistanceSearch()
    {
        Nexus Temp = null;
        float shortestDistance = Mathf.Infinity;
        for (int i = 0; i < NexusManager.nexusManagerInstance.nexusList.Count; ++i)
        {
            if(NexusManager.nexusManagerInstance.nexusList[i] != null)
            {
                float distance = Vector3.Distance(transform.position, NexusManager.nexusManagerInstance.nexusList[i].transform.position);
                if (distance < shortestDistance)
                {
                    Temp = NexusManager.nexusManagerInstance.nexusList[i];
                    shortestDistance = distance;
                }
            }
        }

        return Temp;
    }
    Nexus OrderSearch()
    {
        Nexus Temp = null;
        int iterator = 0;
        Temp = NexusManager.nexusManagerInstance.nexusList[iterator];
        while(Temp == null && NexusManager.nexusManagerInstance.nexusCount > 0 && iterator < NexusManager.nexusManagerInstance.nexusList.Count)
        {
            iterator++;
            Temp = NexusManager.nexusManagerInstance.nexusList[iterator];
        }

        return Temp;
    }
}
