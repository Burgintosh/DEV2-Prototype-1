using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;
    public AudioSource audioSource;
    //public GameObject gunModel;

    //public string weaponName;
    //public int shootDamage;
    //public int shootDist;
    //public float shootRate;

    [SerializeField] LayerMask ignoreLayer;

    //public float reloadTimer;
    //public int magazineSize;
    

    [SerializeField] private GameObject muzzleEffect; // Eventually move to WeaponData. Requires a bit of work though.
    private Animator animator; 
    //public ParticleSystem hitEffect;
    //public AudioSource shootSound;
    //public AudioSource reloadSound;
    //public AudioSource shootEmptySound;

    public event Action<int> OnAmmoChange;

    void Awake()
    {
        data.bulletsLeft = data.magazineSize;
        OnAmmoChange?.Invoke(data.bulletsLeft);
        animator = GetComponent<Animator>();
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
            data.isReloading = false; // cancel reload, primarily for shotgun.

        // New Logic
        if (data.shootClip != null)
            PlayGunSound(data.shootClip);

        if (muzzleEffect != null)
            muzzleEffect.GetComponent<ParticleSystem>().Play();
            //Instantiate(muzzleEffect, transform.position, transform.rotation);

        if(animator != null)
        {
            animator.SetTrigger("RECOIL");
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

    public IEnumerator Reload()
    {
        if (data.isReloading)
            yield break;

        data.isReloading = true;

        if(!data.isSingleShellReload) // Normal Reloading
        {
            if (data.reloadClip != null)
                PlayGunSound(data.reloadClip);

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
        }
        else // Shotgun reload
        {
            while(data.bulletsLeft < data.magazineSize)
            {
                if (!data.isReloading)
                    yield break;

                if (data.reloadClip != null)
                    PlayGunSound(data.reloadClip);

                yield return new WaitForSeconds(data.shellReloadTime);

                data.bulletsLeft++;
                OnAmmoChange?.Invoke(data.bulletsLeft);
            }
        }

        data.isReloading = false;

        /* More old logic
        if (weaponName == "Bennelli")
            SoundManager.Instance.reloadSoundBennelli.Play();
        else
            SoundManager.Instance.reloadSound.Play();
        // TODO Play animation n sound
        */
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
}
