using System;
using System.Collections;
using UnityEngine;

public class Nexus : MonoBehaviour, IDamage
{
   [Range(1,500)] [SerializeField] int HP;
    [SerializeField] Renderer model;
    int HPOrig;
    Color colorOrig;

    public event Action<int> OnNexusHPChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        colorOrig = model.material.color;
        GetComponent<Collider>().enabled = true;
        OnNexusHPChanged?.Invoke(HP);
        NexusManager.nexusManagerInstance.OnNexusSpawn(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetMaxHP()
    {
        return HPOrig;
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        OnNexusHPChanged?.Invoke(HP);

        if (HP <= 0)
        {
            NexusManager.nexusManagerInstance.OnNexusDeath();
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject);
            
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
}
