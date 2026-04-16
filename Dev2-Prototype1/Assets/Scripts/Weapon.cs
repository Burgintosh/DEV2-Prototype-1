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
    public AudioSource shootSound;
    public AudioSource reloadSound;
    public AudioSource shootEmptySound;

    public event Action<int> OnAmmoChange;

    void Awake()
    {
        bulletsLeft = magazineSize;
        OnAmmoChange?.Invoke(bulletsLeft);
    }

    public void FireWeapon()
    {
        muzzleEffect.GetComponent<ParticleSystem>().Play();
        if (weaponName == "M1911")
            SoundManager.Instance.shootingSound1911.Play();
        else if (weaponName == "Bennelli")
            SoundManager.Instance.shootingSoundBennelli.Play();
        else if (weaponName == "M4")
            SoundManager.Instance.shootingSoundM4.Play();


        //shootSound.Play();

        bulletsLeft--;
        OnAmmoChange?.Invoke(bulletsLeft);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if(dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }
    public void GunClick()
    {
        SoundManager.Instance.shootingSoundEmpty.Play();
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        SoundManager.Instance.reloadSound.Play();
        // TODO Play animation n sound
        yield return new WaitForSeconds(.5f);
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
