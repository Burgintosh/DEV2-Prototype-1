using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage, ISlowable
{
    enum TargetPriority
    {
        LOWHEALTH,
        CLOSEST,
        FURTHEST,
        ORDER
    };
    [System.Serializable]
    public class ItemDrop
    {
        public GameObject itemDrop;
        [Range(0f, 1.0f)] public float Odds;
    }
    [Header("Sound")]
    [Range(0f, 1f)]
    [SerializeField] float enemyShootVol = 0.5f;
    [SerializeField] AudioSource enemyShootSound;

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
    [SerializeField] List<ItemDrop> DropTables = new List<ItemDrop>();

    Color colorOrig;

    Nexus currTarget;

    float shootTimer;
    float afkTimer;
    float angleToPlayer;
    float angleToNexus;
    float stoppingDistOrig;

    [Header("----- Slow Settings -----")]
    //[SerializeField] bool showSlowLogs = true;

    [SerializeField] Color slowColor = new Color(0.45f, 0.85f, 1f, 1f);
    [SerializeField] ParticleSystem slowHitVFX;
    [SerializeField] float slowVFXDestroyBuffer = 0.5f;

    float origAgentSpeed;
    Coroutine slowCoroutine;
    Coroutine flashCoroutine;

    bool isSlowed;

    bool playerInRange;
    bool nexusInRange;
    bool currentlyRetargeting;

    Vector3 playerDir; // player pos - enemy pos
    Vector3 nexusDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(model != null)
        {
            colorOrig = model.material.color;
        }

        startingPos = transform.position;

        if(agent != null)
        {
            stoppingDistOrig = agent.stoppingDistance;
            origAgentSpeed = agent.speed;
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
        //if (agent.pathPending)
        //{
        //    Debug.Log(agent.hasPath);
        //    Debug.Log(agent.pathPending);
        //    Debug.Log(agent.remainingDistance);
        //    Debug.Log(agent.isStopped);
        //    Debug.Log(agent.pathStatus);
        //}
        if (currTarget == null && !currentlyRetargeting)
        {
            StartCoroutine(CheckTarget());
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
        //else if (currTarget != null && afkTimer > 5)
        //{
        //    //Debug.Log("Before reset: " + agent.pathStatus);
        //    agent.ResetPath();
        //    if (!agent.SetDestination(currTarget.transform.position))
        //    {
        //        Debug.Log("RUH ROH RAGGY I CAN'T FIND A NEXUS");
        //    }
        //    //Debug.Log("After Reset: " + agent.pathStatus);
        //    shootTimer = 0;
        //}
        //else
        {
            if (currTarget != null && !agent.SetDestination(currTarget.transform.position))
            {
               // Debug.Log("RUH ROH RAGGY I CAN'T FIND A NEXUS but in else");
            }
        }
    }

    bool canSeePlayer() // 
    {
        playerDir = gamemanager.instance.player.transform.position - transform.position; // Still vile
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);


        //Debug.DrawRay(transform.position, playerDir);

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
            SoundManager.Instance.PlayWithRandomPitch(enemyShootSound, enemyShootVol, SoundCategory.Enemy, true);
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
            gamemanager.instance.currScore += maxHP * 10;
            PooledEnemy pooledEnemy = GetComponent<PooledEnemy>();
            if(DropTables.Count > 0)
            {
                for (int i = 0; i < DropTables.Count; i++)
                {
                    float chance = Random.Range(0, 101);
                    float toBeat = 100 - 100 * DropTables[i].Odds;
                    //Debug.Log(chance + "toBeat: " + toBeat);
                    if ( chance >= toBeat)
                    { 
                        Vector3 DropOffset = Random.insideUnitSphere * 2;
                        DropOffset.y = 0f;
                        Instantiate(DropTables[i].itemDrop, transform.position + DropOffset, Quaternion.identity);
                    }
                }
            }
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
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(flashRed());
        }
    }

    public void ApplySlow(float _SlowPercent, float _SlowDuration)
    {
        if(agent == null)
        {
            return;
        }

        _SlowPercent = Mathf.Clamp(_SlowPercent, 0f, 100f);
        _SlowDuration = Mathf.Max(0f, _SlowDuration);

        if(origAgentSpeed <= 0f)
        {
            origAgentSpeed = agent.speed;
        }

        if(flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        isSlowed = true;
        SetModelColor(slowColor);
        SpawnSlowVFX();

        if(slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(SlowTimer(_SlowPercent, _SlowDuration));
    }

    IEnumerator SlowTimer(float _SlowPercent, float _SlowDuration)
    {
        float speedMult = 1f - (_SlowPercent / 100f);
        agent.speed = origAgentSpeed * speedMult;

        //if (showSlowLogs)
        //{
        //    Debug.Log(gameObject.name + " slowed by " + _SlowPercent + "% for " + _SlowDuration + " seconds.", gameObject);
        //}

        yield return new WaitForSeconds(_SlowDuration);

        agent.speed = origAgentSpeed;
        isSlowed = false;
        slowCoroutine = null;
        SetModelColor(colorOrig);

        //if (showSlowLogs)
        //{
        //    Debug.Log(gameObject.name + " slow ended.", gameObject);
        //}
    }

    void ResetSlowStatus()
    {
        if(slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }

        if(flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        isSlowed = false;

        if(agent != null && origAgentSpeed > 0f)
        {
            agent.speed = origAgentSpeed;
        }

        SetModelColor(colorOrig);
    }

    void SetModelColor(Color _Color)
    {
        if(model == null)
        {
            return;
        }

        model.material.color = _Color;
    }

    void SpawnSlowVFX()
    {
        if(slowHitVFX == null)
        {
            return;
        }

        ParticleSystem spawnedVFX = Instantiate(slowHitVFX, transform.position, Quaternion.identity);

        ParticleSystem.MainModule mainParticle = spawnedVFX.main;

        Destroy(spawnedVFX.gameObject, mainParticle.duration + mainParticle.startLifetime.constantMax + slowVFXDestroyBuffer);
    }

    IEnumerator flashRed()
    {
        SetModelColor(Color.red);
        yield return new WaitForSeconds(0.1f);

        if (isSlowed)
        {
            SetModelColor(slowColor);
        }
        else
        {
            SetModelColor(colorOrig);
        }

        flashCoroutine = null;
    }

    public void ResetEnemyState()
    {
        ResetSlowStatus();

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


        //Debug.DrawRay(transform.position, nexusDir);

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
        if(NexusManager.nexusManagerInstance.nexusCount == 1)
        {
             currTarget = NexusManager.nexusManagerInstance.nexusList[0];
        }
        else
        {
            if (targetPriority == TargetPriority.LOWHEALTH)
            {
                currTarget = LowHealthSearch();
            }
            if (targetPriority == TargetPriority.CLOSEST)
            {
                currTarget = ClosestSearch();
            }
            if (targetPriority == TargetPriority.FURTHEST)
            {
                currTarget = FurthestSearch();
            }
            if (targetPriority == TargetPriority.ORDER)
            {
                currTarget = OrderSearch();
            }
        }
        if(currTarget == null)
        {
            //Debug.Log("No Path Found");
            return;
        }
    }
    IEnumerator CheckTarget()
    {
        currentlyRetargeting = true;
        yield return null;
        changeTarget();
        if (agent.isOnNavMesh && currTarget != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (currTarget != null)
            {
                agent.SetDestination(currTarget.transform.position);
            }
        }
        currentlyRetargeting = false;
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
        //else
        //{
        //    Debug.LogWarning("No NavMesh found near agent position!");
        //}
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
    Nexus ClosestSearch()
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
    Nexus FurthestSearch()
    {
        Nexus Temp = null;
        float furthestDistance = 0;
        for (int i = 0; i < NexusManager.nexusManagerInstance.nexusList.Count; ++i)
        {
            if (NexusManager.nexusManagerInstance.nexusList[i] != null)
            {
                float distance = Vector3.Distance(transform.position, NexusManager.nexusManagerInstance.nexusList[i].transform.position);
                if (distance > furthestDistance)
                {
                    Temp = NexusManager.nexusManagerInstance.nexusList[i];
                    furthestDistance = distance;
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

    public Vector3 GetEnemyVelocity()
    {
        if(agent == null)
        {
            return Vector3.zero;
        }

        return agent.velocity;
    }

}
