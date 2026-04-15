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

    public event Action<int> OnAmmoChange;

    void Awake()
    {
        bulletsLeft = magazineSize;
        OnAmmoChange?.Invoke(bulletsLeft);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FireWeapon()
    {
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

    public IEnumerator Reload()
    {
        isReloading = true;
        // TODO Play animation n sound
        yield return new WaitForSeconds(.5f);
        if (bulletsLeft < magazineSize)
            bulletsLeft = magazineSize;
        else if (bulletsLeft == magazineSize) 
            bulletsLeft = magazineSize + 1; // Changing this to bulletsLeft++ increases it by 1 for each frame it's reloading which is really funny.
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
