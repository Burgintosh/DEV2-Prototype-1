using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;
    public AudioSource audioSource;

    [SerializeField] LayerMask ignoreLayer;
    

    [SerializeField] private GameObject muzzleEffect; // Eventually move to WeaponData. Requires a bit of work though.
    private Animator animator; 
    //public ParticleSystem hitEffect;


    public event Action<int> OnAmmoChange;

    void Awake()
    {
        data.bulletsLeft = data.magazineSize;
        OnAmmoChange?.Invoke(data.bulletsLeft);
        animator = GetComponent<Animator>();
        data.canShootShotgun = true;
    }

    public void FireWeapon()
    {
        /* Old Logic, delete after new stuff works
        AudioSource shootSound = null;
        muzzleEffect.GetComponent<ParticleSystem>().Play();
        if (data.weaponName == "M1911")
            shootSound = SoundManager.Instance.shootingSound1911;
        else if (weaponName == "Bennelli")
            shootSound = SoundManager.Instance.shootingSoundBennelli;
        else if (weaponName == "M4")
            shootSound = SoundManager.Instance.shootingSoundM4;

        if(shootSound != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(shootSound);
        }
        */

        //shootSound.Play();
        if (data.isReloading)
            StopReload(); // cancel reload for shotgun.

        // New Logic
        if (data.shootClip != null)
            PlayGunSound(data.shootClip);

        if (muzzleEffect != null)
            muzzleEffect.GetComponent<ParticleSystem>().Play();
            //Instantiate(muzzleEffect, transform.position, transform.rotation);

        if(animator != null)
        {
            animator.SetTrigger("RECOIL");

            if (data.isSingleShellReload)
                data.canShootShotgun = false; // Animator controls when this is true. If no animator, revert to normal firerate var. This is mostly a proof of concept
        }

        

        data.bulletsLeft--;
        OnAmmoChange?.Invoke(data.bulletsLeft);

        for(int i = 0; i < data.pelletCount; i++)
        {
            Vector3 direction = GetSpreadDirection();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, direction, out hit, data.shootDist, ~ignoreLayer))
            {
                //Debug.Log(hit.collider.name);

                Instantiate(data.hitEffect, hit.point, Quaternion.identity);

                IDamage dmg = hit.collider.GetComponent<IDamage>();
                if (dmg != null)
                    dmg.takeDamage(data.shootDamage);
            }
        }

        
    }
    Vector3 GetSpreadDirection()
    {
        Vector3 forward = Camera.main.transform.forward;

        float spreadX = UnityEngine.Random.Range(-data.spreadAngle, data.spreadAngle);
        float spreadY = UnityEngine.Random.Range(-data.spreadAngle, data.spreadAngle);
        Quaternion rotation = Quaternion.Euler(spreadY, spreadX, 0);

        return rotation * forward;
    }

    public void GunClick()
    {
        PlayGunSound(data.emptyClip);
    }

    public void StartReload()
    {
        if (data.isReloading) return;
        if (data.bulletsLeft >= data.magazineSize) return;

        data.isReloading = true;
        //data.wantsToReload = true;

        if (data.isSingleShellReload)
        {
            animator.SetTrigger("RELOAD");
        }
        else
        {
            StartCoroutine(MagReload());
        }
    }
    private IEnumerator MagReload() // This is just for mag-fed guns now
    {


        if (animator != null)
        {

            animator.SetTrigger("RELOAD");
        }

        yield return new WaitForSeconds(data.reloadTime);

        if (data.bulletsLeft <= 0)
            data.bulletsLeft = data.magazineSize;
        else
            data.bulletsLeft = data.magazineSize + 1;

        OnAmmoChange?.Invoke(data.bulletsLeft);
        
        data.isReloading = false;
    }
    public void BeginShotgunReloadLoop()
    {
        if (!data.isSingleShellReload) return;

        animator.SetTrigger("RELOADLOOP");
        //animator.SetBool("Reloading", true);
    }
    public void InsertShell()
    {
        if (!data.isSingleShellReload) return;

        data.bulletsLeft++;
        OnAmmoChange?.Invoke(data.bulletsLeft);

        if (data.bulletsLeft >= data.magazineSize || !data.isReloading)
        {
            StopReload();
        }

        animator.SetTrigger("RELOADLOOP");
    }
    public void StopReload()
    {
        if (!data.isReloading) return;

        data.isReloading = false;

        //animator.SetBool("Reloading", false);
        animator.SetTrigger("RELOADEND");
    }


    public bool canReload()
    {
        return data.bulletsLeft <= data.magazineSize;
    }
    public bool canShoot()
    {
        return data.bulletsLeft > 0;
    }

    private void PlayGunSound(AudioClip clip)
    {
        if (clip != null) {
            SoundManager.Instance.PlayWithRandomPitch(audioSource, clip);
        }
    }
    public void ReadyToFire()
    {
        data.canShootShotgun = true; // Only matters for Shotgun
    }
   

}

