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
    private void Start()
    {
        StartCoroutine(StartDisappearing());
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
                        gamemanager.instance.playerScript.GetCurrentWeapon().StartReload();
                    }
                    break;
                case EssenceType.UPGRADEMATERIAL:
                    break;
                case EssenceType.MONEY:
                    gamemanager.instance.currencyManager.AddCurrency(Amount);
                    break;
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
