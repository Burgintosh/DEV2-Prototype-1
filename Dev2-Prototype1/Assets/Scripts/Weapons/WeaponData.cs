using UnityEngine;

[CreateAssetMenu]
public class WeaponData : ScriptableObject
{
    public GameObject prefab; // Very important to assign.
    public string weaponName;
    public int shootDamage;
    public int shootDist;
    public float shootRate;

    public int magazineSize;
    public float reloadTime;
    public int bulletsLeft;
    public bool isReloading = false;


    public AudioClip shootClip;
    public AudioClip emptyClip;

    //public GameObject muzzleEffect; // Not worth the effort rn, Definitely worth coming back to eventually.
    public ParticleSystem hitEffect;

    [Header("----- Volume Settings -----")]
    [Range(0f, 1f)] public float shootVol = 0.5f;
    [Range(0f, 1f)] public float reloadVol = 0.5f;
    [Range(0f, 1f)] public float emptyClickVol = 0.5f;

    public bool isSingleShellReload; // true = reload one bullet/shell at a time. Turns on shellReloadTime.
    public float shellReloadTime = 0.5f;
    public int pelletCount = 1;      // shotgun = 6–12. 1 for single bullet
    public float spreadAngle = 0f;   // degrees. 0 for normal (no spread)
    public bool canShootShotgun = true;
}