using NUnit.Framework.Interfaces;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] WeaponData gun;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        playerController pc = gamemanager.instance.playerScript;

        if (pc.HasWeapon(gun))
        {
            pc.RefillWeapon(gun);
            Destroy(gameObject);
            return;
        }


        if (pik != null)
        {
            gun.bulletsLeft = gun.magazineSize;
            pik.getWeaponData(gun);
            Destroy(gameObject);
        }
    }
}
