using UnityEngine;

[CreateAssetMenu]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int shootDamage;
    public int shootDist;
    public float shootRate;

    public int magazineSize;
    public float reloadTime;

    public AudioClip shootClip;
    public AudioClip reloadClip;
    public AudioClip emptyClip;

    //public GameObject muzzleEffect; // Not worth the effort rn, Definitely worth coming back to eventually.
    public ParticleSystem hitEffect;

    public bool isSingleShellReload; // true = reload one bullet/shell at a time. Turns on shellReloadTime.
    public float shellReloadTime = 0.5f;
    public int pelletCount = 1;      // shotgun = 6–12. 1 for single bullet
    public float spreadAngle = 0f;   // degrees. 0 for normal (no spread)
}