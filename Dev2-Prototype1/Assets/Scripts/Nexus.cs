using System;
using System.Collections;
using UnityEngine;

public class Nexus : MonoBehaviour, IDamage
{
   [Range(1,1000)] [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] GameObject HPObject;
    [SerializeField] Renderer HPmodel;
    [SerializeField] AudioSource nexusDeathSFX;
    [Range(0, 1f)][SerializeField] float volume;
    [SerializeField] GameObject deathEffectPrefab;
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
        if(HP <= 0)
        {
            HP = 0;
            return;
        }
        HP -= amount;
        // OnNexusHPChanged?.Invoke(NexusManager.nexusManagerInstance.currNexusHealth);
        gamemanager.instance.UpdateNexusHPBar2();

        if (HP <= 0)
        {
            NexusManager.nexusManagerInstance.OnNexusDeath(this);
            if(SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.enemyShootSound,nexusDeathSFX.clip, volume, SoundCategory.Master, true);
            }
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
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
    } public int GetCurrHP()
    {
        return HP;
    }
}
