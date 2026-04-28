using NUnit.Framework.Interfaces;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] WeaponData gun;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        if (pik != null)
        {
            gun.bulletsLeft = gun.magazineSize;
            pik.getWeaponData(gun);
            Destroy(gameObject);
        }
    }
}
