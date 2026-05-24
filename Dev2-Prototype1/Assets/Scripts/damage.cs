using UnityEngine;
using System.Collections;
using System.Collections.Generic; // gives us acces to Ienumerator

public class damage : MonoBehaviour
{
    [Header("----- Slow Settings -----")]
    [SerializeField] bool applySlow;
    [Range(0f, 100f)]
    [SerializeField] float slowPercent = 50f;
    [SerializeField] float slowDuration = 3f;

    [Header("----- Explo Settings -----")]
    [SerializeField] bool explodeOnImpact;
    [SerializeField] float exploRadius = 3f;
    [SerializeField] int exploDam = 10;
    [SerializeField] LayerMask exploDamMask;
    [SerializeField] ParticleSystem exploVFX;

    [Header("Collision Settings")]
    [SerializeField] bool ignoreTriggerCollision = true;

    [Header("----- Feedback -----")]
    [SerializeField] float exploDelay = 0.25f;
    [SerializeField] AudioSource audioSrc;
    [SerializeField] AudioClip impactSFX;
    [SerializeField] AudioClip exploSFX;
    [SerializeField]
    [Range(0f, 1f)] float impactSFXVol = 1f;
    [SerializeField]
    [Range(0f, 1f)] float exploSFXVol = 1f;

    [Header("----- Shake Settings -----")]
    [SerializeField] bool shakeCamOnExplo = true;
    [SerializeField] float screenShakeRad = 8f;
    [SerializeField] float screenShakeDur = 0.2f;
    [SerializeField] float screenShakeMag = 2f;
    [SerializeField] float screenShakeFreq = 20f;

    [Header("Debug")]
    //[SerializeField] public bool showDebugLogs;

    HashSet<IDamage> targetsToDam = new HashSet<IDamage>();

    enum damageType { bullet, stationary, DOT }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] float damageMult = 1f;
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDestroyTime;
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] float hitEffectDestroyDelay = 0.25f;

    bool isDamaging;
    bool hasHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == damageType.bullet)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
            Destroy(gameObject, bulletDestroyTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(type == damageType.bullet && hasHit)
        {
            return;
        }

        //DebugDam("OnTriggerEnter touched: " + other.name + " | Layer: " + LayerMask.LayerToName(other.gameObject.layer) + " | Tage: " + other.tag + " | isTrigger: " + other.isTrigger);

        if (other.isTrigger && ignoreTriggerCollision) // A trigger can enter another trigger so need this to not do anything with that or something
        {
            //DebugDam("Ignored cause: " + other.name + " is a trigger collider and/or Ignore Trigger Colliders is ON");
            return;
        }

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if(applySlow && type == damageType.bullet)
        {
            ISlowable slowable = other.GetComponentInParent<ISlowable>();

            if(slowable != null)
            {
                //DebugDam("Applying slow to " + other.name + " | Slow Percent: " + slowPercent + " | Duration: " + slowDuration);
                slowable.ApplySlow(slowPercent, slowDuration);
            }
        }

        if (dmg != null && type != damageType.DOT && !explodeOnImpact)
        {
            int finalDamage = GetFinalDam();

            //DebugDam("Damaging " + other.name + " for " + finalDamage + " | Base Damage: " + damageAmount + " | Multiplier: " + damageMult);

            dmg.takeDamage(finalDamage);
        }

        if (type == damageType.bullet)
        {
            hasHit = true;

            PlayDamSFX(impactSFX, impactSFXVol);

            if (explodeOnImpact)
            {
                StartCoroutine(ExplodeAfterDelay(true));
            }
            else
            {
                SpawnHitEffect(transform.position, transform.rotation);

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(type != damageType.DOT)
        {
            return;
        }

        //DebugDam("OnTriggerStay touching: " + other.name + " | Layer: " + LayerMask.LayerToName(other.gameObject.layer) + " | Tage: " + other.tag + " | isTrigger: " + other.isTrigger);

        if (other.isTrigger && ignoreTriggerCollision)
        {
            //DebugDam("Ignored cuase: " + other.name + " is a trigger collider and/or Ignore Trigger Colliders is ON");
            return;
        }

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if(dmg == null)
        {
            //DebugDam("No IDamage found on " + other.name + " or it's parents");
            return;
        }

        if(type == damageType.DOT && !targetsToDam.Contains(dmg))
        {
            //DebugDam("Starting DOT damage on " + other.name + " for " + damageAmount);
            StartCoroutine(damageOther(dmg));
        }
    }

    void Explode()
    {
        int finalExploDam = GetFinalExploDam();

        QueryTriggerInteraction triggerMode;

        if (ignoreTriggerCollision)
        {
            triggerMode = QueryTriggerInteraction.Ignore;
        }
        else
        {
            triggerMode = QueryTriggerInteraction.Collide;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, exploRadius, exploDamMask, triggerMode);

        HashSet<IDamage> damTargets = new HashSet<IDamage>();

        for(int i = 0; i < hits.Length; i++)
        {
            IDamage dmg = hits[i].GetComponentInParent<IDamage>();

            if(dmg == null)
            {
                continue;
            }

            if (damTargets.Contains(dmg))
            {
                continue;
            }

            damTargets.Add(dmg);

            //DebugDam("Explosion damaged " + hits[i].name + " for " + finalExploDam);

            dmg.takeDamage(finalExploDam);
        }
    }

    IEnumerator ExplodeAfterDelay(bool _ShouldDamage)
    {
        DeactivateProjectilePostImpact();

        if(exploDelay > 0)
        {
            yield return new WaitForSeconds(exploDelay);
        }

        if (_ShouldDamage)
        {
            Explode();
        }

        if(exploVFX != null)
        {
            ParticleSystem spawnedExplo = Instantiate(exploVFX, transform.position, Quaternion.identity);

            ParticleSystem.MainModule mainParticle = spawnedExplo.main;

            Destroy(spawnedExplo.gameObject, mainParticle.duration + mainParticle.startLifetime.constantMax);     
        }

        PlayDamSFX(exploSFX, exploSFXVol);

        TryShakeCam();

        float destroyDelay = 0.05f;

        if(exploSFX != null)
        {
            destroyDelay = exploSFX.length + 0.05f;
        }

        Destroy(gameObject, destroyDelay);
    }

    void DeactivateProjectilePostImpact()
    {
        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider[] cols = GetComponentsInChildren<Collider>();

        for(int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for(int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }

    void PlayDamSFX(AudioClip _AudioClip, float _Vol)
    {
        if(_AudioClip == null)
        {
            return;
        }

        if(audioSrc == null)
        {
            //DebugDam("Tried to play dam SFX but AudioSource is missing.");
            return;
        }

        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(audioSrc, _AudioClip, _Vol, SoundCategory.Trap, true);
        }
        else
        {
            audioSrc.PlayOneShot(_AudioClip, Mathf.Clamp01(_Vol));
        }

    }

    void TryShakeCam()
    {
        if (!shakeCamOnExplo)
        {
            return;
        }

        Camera mainCam = Camera.main;

        if(mainCam == null)
        {
            //DebugDam("No Main Camera found for screenshake");
            return;
        }

        float dist = Vector3.Distance(transform.position, mainCam.transform.position);

        if(dist > screenShakeRad)
        {
            return;
        }

        cameraController camController = mainCam.GetComponent<cameraController>();

        if(camController == null)
        {
            camController = mainCam.GetComponentInParent<cameraController>();   
        }

        if(camController == null)
        {
            //DebugDam("Main Camera has no cameraController for screen shake");
            return;
        }

        float distPercent = 1f - (dist / screenShakeRad);
        float finalMag = screenShakeMag * Mathf.Clamp01(distPercent);

        camController.Shake(screenShakeDur, finalMag, screenShakeFreq);
    }

    IEnumerator damageOther(IDamage d)
    {
        targetsToDam.Add(d);
        int finalDamage = GetFinalDam();
        //DebugDam("DOT damage applied for " + finalDamage + " | Base Damage: " + damageAmount + " | Multiplier: " + damageMult);
        d.takeDamage(finalDamage);
        yield return new WaitForSeconds(damageRate);
        targetsToDam.Remove(d);
    }

    public void SetDamMult(float _Mult)
    {
        damageMult = Mathf.Max(0f, _Mult);

        //DebugDam("Damage mult set to: " + damageMult);
    }

    int GetFinalDam()
    {
        return Mathf.Max(0, Mathf.RoundToInt(damageAmount * damageMult));
    }

    int GetFinalExploDam()
    {
        return Mathf.Max(0, Mathf.RoundToInt(exploDam * damageMult));
    }

    void DebugDam(string _MSG)
    {
        //if (showDebugLogs)
        //{
        //    Debug.Log("[Damage Script: " + gameObject.name + "] "+  _MSG, gameObject);
        //}
    }

    void SpawnHitEffect(Vector3 _HitPos, Quaternion _HitRot)
    {
        if(hitEffect == null)
        {
            return;
        }

        ParticleSystem spawnedHitEffect = Instantiate(hitEffect, _HitPos, _HitRot);

        spawnedHitEffect.Play();

        ParticleSystem.MainModule main = spawnedHitEffect.main;

        float destoryTime = main.duration + main.startLifetime.constantMax + hitEffectDestroyDelay;

        Destroy(spawnedHitEffect.gameObject, destoryTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (!explodeOnImpact)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, exploRadius);
    }

}
