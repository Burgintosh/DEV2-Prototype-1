using System.Collections;
using UnityEngine;
public class EssencePickup : MonoBehaviour
{
    enum EssenceType
    {
        PLAYERHEALTH,
        PLAYERAMMO,
        MONEY,
        UPGRADEMATERIAL
    };
    [SerializeField] int Amount;
    [SerializeField] EssenceType Type;
    [SerializeField] ParticleSystem PickupEffect;
    [SerializeField] bool temporary;
    private void Start()
    {
        if (temporary)
        {
            StartCoroutine(StartDisappearing());
        } 
    }
    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();

        if (pik != null)
        {
            switch (Type)
            {
                case EssenceType.PLAYERHEALTH:
                    gamemanager.instance.playerScript.HealPlayer(Amount);
                    break;
                case EssenceType.PLAYERAMMO:
                    if (gamemanager.instance.playerScript.GetCurrentWeapon().isActiveAndEnabled)
                    {
                        Debug.Log("Weapon is active, jkust won't reload");
                        gamemanager.instance.playerScript.RefillAllGuns();
                        //gamemanager.instance.playerScript.GetCurrentWeapon().data.bulletsLeft = gamemanager.instance.playerScript.GetCurrentWeapon().data.magazineSize;
                    }
                    break;
                case EssenceType.UPGRADEMATERIAL:
                    break;
                case EssenceType.MONEY:
                    gamemanager.instance.currencyManager.AddCurrency(Amount);
                    break;
            }
            if(PickupEffect != null)
            {
                Instantiate(PickupEffect, gamemanager.instance.player.transform.position,Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
    IEnumerator StartDisappearing()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
