using UnityEngine;

public class SniperBullet : MonoBehaviour
{
    [Header("----- Sniper Bullet Settings -----")]
    [SerializeField] float bulletSpeed = 60f;
    [SerializeField] float hitDist = 0.6f;
    [SerializeField] float selfDestructTime = 5f;
    [SerializeField] ParticleSystem hitVFX;

    [Header("----- Audio Settings -----")]
    [SerializeField] AudioSource audioSrc;
    [SerializeField] AudioClip hitSFX;
    [SerializeField] [Range(0f, 1f)] float hitSFXVol;

    [Header("----- Debug Settings -----")]
    [SerializeField] bool showDebugLog;

    Transform target;
    IDamage targetDam;

    int damAmount;
    float damMult = 1f;
    float lifeTimer;

    public void Init(Transform _Target, int _BaseDam, float _DamMult)
    {
        target = _Target;
        damAmount = _BaseDam;
        damMult = _DamMult;

        if(target != null)
        {
            targetDam = target.GetComponentInParent<IDamage>();
        }
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if(lifeTimer > selfDestructTime)
        {
            Destroy(gameObject);
            return;
        }

        if(target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardTargetNextPos();
    }

    void MoveTowardTargetNextPos()
    {
        Vector3 targetPos = GetTargetPos();
        Vector3 nextTargetPos = GetTargetNextPos(targetPos);

        Vector3 moveDir = nextTargetPos - transform.position;

        if(moveDir == Vector3.zero)
        {
            HitTheTarget();
            return;
        }

        transform.rotation = Quaternion.LookRotation(moveDir);
        transform.position = Vector3.MoveTowards(transform.position, nextTargetPos, bulletSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, targetPos) <= hitDist)
        {
            HitTheTarget();
        }
    }

    Vector3 GetTargetNextPos(Vector3 _TargetPos)
    {
        float dist = Vector3.Distance(transform.position, _TargetPos);
        float timeToTarget = dist / bulletSpeed;

        Vector3 targetVel = GetTargetVel();

        return _TargetPos + targetVel * timeToTarget;
    }

    Vector3 GetTargetVel()
    {
        EnemyAI enemyAI = target.GetComponentInParent<EnemyAI>();

        if(enemyAI == null)
        {
            return Vector3.zero;
        }

        return enemyAI.GetEnemyVelocity();
    }

    Vector3 GetTargetPos()
    {
        Collider enemyCol = target.GetComponentInChildren<Collider>();

        if(enemyCol != null)
        {
            return enemyCol.bounds.center;
        }

        return target.position;
    }

    void HitTheTarget()
    {
        if(targetDam != null)
        {
            int finalDam = Mathf.Max(0, Mathf.RoundToInt(damAmount * damMult));
            targetDam.takeDamage(finalDam);

            DebugSniperBullet("Hit for " + finalDam);
        }

        if(hitVFX != null)
        {
            ParticleSystem spawnedHitVFX = Instantiate(hitVFX, transform.position, Quaternion.identity);

            ParticleSystem.MainModule mainParticle = spawnedHitVFX.main;

            Destroy(spawnedHitVFX.gameObject, mainParticle.duration + mainParticle.startLifetime.constantMax);
        }

        PlayImpactSFX();

        if(hitSFX != null && audioSrc != null)
        {
            StopProjectileAfterImpact();
            Destroy(gameObject, hitSFX.length + 0.05f);
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    void PlayImpactSFX()
    {
        if(hitSFX == null)
        {
            return;
        }

        if(audioSrc == null)
        {
            DebugSniperBullet("Attempted to play sniper bullet impact SFX but AudioSource was missing");
            return;
        }

        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(audioSrc, hitSFX, hitSFXVol, SoundCategory.Trap, true);
        }
        else
        {
            audioSrc.PlayOneShot(hitSFX, Mathf.Clamp01(hitSFXVol));
        }
    }

    void StopProjectileAfterImpact()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for(int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }

    void DebugSniperBullet(string _MSG)
    {
        if (showDebugLog)
        {
            Debug.Log("SniperBullet: " + gameObject.name + " " + _MSG, gameObject);
        }
    }
}
