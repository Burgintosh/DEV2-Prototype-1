using UnityEngine;
using System.Collections;
using System.Collections.Generic; // gives us acces to Ienumerator

public class damage : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] bool ignoreTriggerCollision = true;

    [Header("Debug")]
    [SerializeField] public bool showDebugLogs;

    HashSet<IDamage> targetsToDam = new HashSet<IDamage>();

    enum damageType { bullet, stationary, DOT }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDestroyTime;
    [SerializeField] ParticleSystem hitEffect;

    bool isDamaging;

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
        DebugDam("OnTriggerEnter touched: " + other.name + " | Layer: " + LayerMask.LayerToName(other.gameObject.layer) + " | Tage: " + other.tag + " | isTrigger: " + other.isTrigger);

        if (other.isTrigger && ignoreTriggerCollision) // A trigger can enter another trigger so need this to not do anything with that or something
        {
            DebugDam("Ignored cause: " + other.name + " is a trigger collider and/or Ignore Trigger Colliders is ON");
            return;
        }

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if (dmg != null && type != damageType.DOT)
        {
            DebugDam("Damaging " + other.name + " for " + damageAmount);
            dmg.takeDamage(damageAmount);
        }

        if (type == damageType.bullet)
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity); // Quaternion identity means (0,0,0)
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(type != damageType.DOT)
        {
            return;
        }

        DebugDam("OnTriggerStay touching: " + other.name + " | Layer: " + LayerMask.LayerToName(other.gameObject.layer) + " | Tage: " + other.tag + " | isTrigger: " + other.isTrigger);

        if (other.isTrigger && ignoreTriggerCollision)
        {
            DebugDam("Ignored cuase: " + other.name + " is a trigger collider and/or Ignore Trigger Colliders is ON");
            return;
        }

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if(dmg == null)
        {
            DebugDam("No IDamage found on " + other.name + " or it's parents");
            return;
        }

        if(type == damageType.DOT && !targetsToDam.Contains(dmg))
        {
            DebugDam("Starting DOT damage on " + other.name + " for " + damageAmount);
            StartCoroutine(damageOther(dmg));
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        targetsToDam.Add(d);
        DebugDam("DOT damage applied for " + damageAmount);
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        targetsToDam.Remove(d);
    }

    void DebugDam(string _MSG)
    {
        if (showDebugLogs)
        {
            Debug.Log("[Damage Script: " + gameObject.name + "] "+  _MSG, gameObject);
        }
    }

}
