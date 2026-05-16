using UnityEngine;

public class SniperBullet : MonoBehaviour
{
    [Header("----- Sniper Bullet Settings -----")]
    [SerializeField] float bulletSpeed = 60f;
    [SerializeField] float hitDist = 0.6f;
    [SerializeField] float selfDestructTime = 5f;
    [SerializeField] ParticleSystem hitVFX;

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
            Instantiate(hitVFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void DebugSniperBullet(string _MSG)
    {
        if (showDebugLog)
        {
            Debug.Log("SniperBullet: " + gameObject.name + " " + _MSG, gameObject);
        }
    }
}
