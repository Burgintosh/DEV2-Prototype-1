using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public int shootDamage;
    public int shootDist;
    public float shootRate;

    [SerializeField] LayerMask ignoreLayer;

    public float reloadTimer;
    public int magazineSize;
    public int bulletsLeft;
    public bool isReloading;

    public GameObject muzzleEffect;
    public ParticleSystem hitEffect;
    //public AudioSource shootSound;
    //public AudioSource reloadSound;
    //public AudioSource shootEmptySound;

    public event Action<int> OnAmmoChange;

    void Awake()
    {
        bulletsLeft = magazineSize;
        OnAmmoChange?.Invoke(bulletsLeft);
    }

    public void FireWeapon()
    {
        AudioSource shootSound = null;
        muzzleEffect.GetComponent<ParticleSystem>().Play();
        if (weaponName == "M1911")
            shootSound = SoundManager.Instance.shootingSound1911;
        else if (weaponName == "Bennelli")
            shootSound = SoundManager.Instance.shootingSoundBennelli;
        else if (weaponName == "M4")
            shootSound = SoundManager.Instance.shootingSoundM4;

        if(shootSound != null)
        {
            SoundManager.Instance.PlayWithRandomPitch(shootSound);
        }

        //shootSound.Play();

        bulletsLeft--;
        OnAmmoChange?.Invoke(bulletsLeft);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            //Debug.Log(hit.collider.name);
            
            Instantiate(hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if(dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }
    public void GunClick()
    {
        SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.shootingSoundEmpty, false);
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        if(weaponName == "Bennelli")
            SoundManager.Instance.reloadSoundBennelli.Play();
        else
            SoundManager.Instance.reloadSound.Play();
        // TODO Play animation n sound
        yield return new WaitForSeconds(reloadTimer);
        if (bulletsLeft < magazineSize)
            bulletsLeft = magazineSize;
        else if (bulletsLeft == magazineSize) 
            bulletsLeft = magazineSize + 1;
        OnAmmoChange?.Invoke(bulletsLeft);
        isReloading = false;
    }

    public bool canReload()
    {
        return bulletsLeft <= magazineSize;
    }
    public bool canShoot()
    {
        return bulletsLeft > 0;
    }
}
