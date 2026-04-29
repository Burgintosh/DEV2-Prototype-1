using System;
using System.Collections;
using UnityEngine;

public class Nexus : MonoBehaviour, IDamage
{
   [Range(1,500)] [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] GameObject HPObject;
    [SerializeField] Renderer HPmodel;
    int HPOrig;
    Color colorOrig;
    Color HPcolorOrig;

    Vector3 HPScale;
    public event Action<int> OnNexusHPChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        colorOrig = model.material.color;
        HPcolorOrig = HPmodel.material.color;
        OnNexusHPChanged?.Invoke(HP);
        HPScale = HPObject.transform.localScale;

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
            Destroy(gameObject);
            
        }
        else
        {
            StartCoroutine(flashRed());
            HPObject.transform.localScale = new Vector3(HPScale.x, (float)HP / HPOrig, HPScale.z);
            HPObject.transform.localPosition = new Vector3(0, -(1-HPObject.transform.localScale.y) / 2, 0);
        }
    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        HPmodel.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
        HPmodel.material.color = HPcolorOrig;
    }
}
