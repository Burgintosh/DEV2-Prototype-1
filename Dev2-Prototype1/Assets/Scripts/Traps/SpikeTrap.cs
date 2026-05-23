using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpikeTrap : MonoBehaviour
{

    [Header("----- Refs -----")]
    [SerializeField] Transform spikeVisualRoot;
    [SerializeField] Transform spikeDownPos;
    [SerializeField] Transform spikeUpPos;
    [SerializeField] Collider detectTrigger;
    [SerializeField] Collider damageTrigger;

    [Header("----- Trap Settings -----")]
    [SerializeField] int damage = 1;
    [SerializeField] float attackDelay = 0.5f;
    [SerializeField] float cooldown = 1.5f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float upHoldBuffer = 0.15f;
    [SerializeField] LayerMask damageMask;

    [Header("----- VFX -----")]
    [SerializeField] GameObject hitVFX;
    [SerializeField] Transform hitVFXSpawnPos;
    [SerializeField] float hitVFXDestroyDelay = 2f;

    [Header("----- Audio -----")]
    [SerializeField] AudioSource spikeAudioSource;
    [SerializeField] AudioClip activateAudio;
    [SerializeField] AudioClip retractAudio;
    [SerializeField][Range(0f, 1f)] float spikeAudioVol = 1f;
    [SerializeField] SoundCategory spikeSoundCategory = SoundCategory.Trap;

    bool isAttacking;
    bool isCoolingDown;

    List<Collider> targetsInRange = new List<Collider>();

    private void Awake()
    {   
        if(spikeAudioSource == null)
        {
            spikeAudioSource = GetComponent<AudioSource>();
        }

        if(spikeAudioSource != null)
        {
            spikeAudioSource.playOnAwake = false;
            spikeAudioSource.loop = false;
        }

        //Debug.Log("[SpikeTrap] Awake ran", this);

        if(spikeVisualRoot == null)
        {
            //Debug.LogError("[SpikeTrap] spikeVisualRoot is NOT assigned", this);
            return;
        }

        if(spikeDownPos == null)
        {
            //Debug.LogError("[SpikeTrap] spikeDownPos is NOT assigned", this);
            return;
        }

        //Debug.Log($"[SpikeTrap] Before snap | spikeVisualRoot.localPosition = {spikeVisualRoot.localPosition}", this);
        //Debug.Log($"[SpikeTrap] Target down pos | spikeDownPos.localPosition = {spikeDownPos.localPosition}", this);

        spikeVisualRoot.localPosition = spikeDownPos.localPosition;

        //Debug.Log($"[SpikeTrap] After snap | spikeVisualRoot.localPosition = {spikeVisualRoot.localPosition}", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg == null)
        {
            dmg = other.GetComponentInParent<IDamage>();
        }

        if(dmg == null)
        {
            return;
        }

        if (!targetsInRange.Contains(other))
        {
            targetsInRange.Add(other);
        }

        if(!isAttacking && !isCoolingDown)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetsInRange.Contains(other))
        {
            targetsInRange.Remove(other);
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);

        CleanupTargets();

        if (targetsInRange.Count == 0)
        {
            isAttacking = false;
            yield break;
        }

        PlayTrapSound(activateAudio);

        yield return StartCoroutine(MoveSpikes(spikeUpPos.localPosition));

        bool hitEnemy = DealBurstDamage();

        if (hitEnemy)
        {
            SpawnHitVFX();
        }

        yield return new WaitForSeconds(upHoldBuffer);

        PlayTrapSound(retractAudio);

        yield return StartCoroutine(MoveSpikes(spikeDownPos.localPosition));

        isAttacking = false;
        isCoolingDown = true;

        yield return new WaitForSeconds(cooldown);

        isCoolingDown = false;

        CleanupTargets();

        if(targetsInRange.Count > 0)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator MoveSpikes(Vector3 _TargetPos)
    {
        if(spikeVisualRoot == null)
        {
            yield break;
        }

        while(Vector3.Distance(spikeVisualRoot.localPosition, _TargetPos) > 0.01f)
        {
            spikeVisualRoot.localPosition = Vector3.MoveTowards(spikeVisualRoot.localPosition, _TargetPos, moveSpeed * Time.deltaTime);

            yield return null;
        }

        spikeVisualRoot.localPosition = _TargetPos;
    }

    void PlayTrapSound(AudioClip _TrapSound)
    {
        if(_TrapSound == null)
        {
            return;
        }

        if(spikeAudioSource == null)
        {
            Debug.LogWarning("[SpikeTrap] Attempted to play spike sound but AudioSource is missing");
            return;
        }

        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(spikeAudioSource, _TrapSound, spikeAudioVol, spikeSoundCategory, true);
        }
        else
        {
            Debug.LogWarning("[SpikeTrap] SoundManager is missing");
            spikeAudioSource.PlayOneShot(_TrapSound, Mathf.Clamp01(spikeAudioVol));
        }
    }

    void SpawnHitVFX()
    {
        if(hitVFX == null)
        {
            return;
        }

        if(hitVFXSpawnPos == null)
        {
            Debug.LogWarning("Hit spawn pos is missing", this);
            return;
        }

        GameObject spawnedHitVFX = Instantiate(hitVFX, hitVFXSpawnPos.position, hitVFXSpawnPos.rotation);

        Destroy(spawnedHitVFX, hitVFXDestroyDelay);
    }

    bool DealBurstDamage()
    {
        if(damageTrigger == null)
        {
            //Debug.LogWarning("[SpikeTrap] Damage trigger is not assigned", this);
            return false;
        }

        bool dealtDam = false;

        Bounds damBounds = damageTrigger.bounds;

        Collider[] allHits = Physics.OverlapBox(damBounds.center, damBounds.extents, damageTrigger.transform.rotation, damageMask, QueryTriggerInteraction.Ignore);

        //Debug.Log($"[SpikeTrap] Burst hit count: {allHits.Length}", this);

        HashSet<IDamage> damTargets = new HashSet<IDamage>();

        for(int i = 0; i < allHits.Length; i++)
        {
            //Debug.Log($"[SpikeTrap] Hit collider: {allHits[i].name} | Layer: {LayerMask.LayerToName(allHits[i].gameObject.layer)}", this);

            IDamage dmg = allHits[i].GetComponent<IDamage>();

            if(dmg == null)
            {
                dmg = allHits[i].GetComponentInParent<IDamage>();
            }

            if(dmg == null)
            {
                //Debug.Log("[SpikeTrap] No IDamage found on hit collider or parent", allHits[i]);
                continue;
            }

            if (damTargets.Contains(dmg))
            {
                continue;
            }

            damTargets.Add(dmg);
            //Debug.Log("[SpikeTrap] Damaging Target");
            dmg.takeDamage(damage);

            dealtDam = true;
        }

        return dealtDam;
    }

    void CleanupTargets()
    {
        for(int i = targetsInRange.Count - 1; i >= 0; i--)
        {
            if(targetsInRange[i] == null)
            {
                targetsInRange.RemoveAt(i);
                continue;
            }

            IDamage dmg = targetsInRange[i].GetComponent<IDamage>();

            if(dmg == null)
            {
                dmg = targetsInRange[i].GetComponentInParent<IDamage>();
            }

            if(dmg == null)
            {
                targetsInRange.RemoveAt(i);
            }
        }
    }
}
