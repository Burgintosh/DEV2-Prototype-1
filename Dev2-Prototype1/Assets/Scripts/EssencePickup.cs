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
                    gamemanager.instance.playerScript.GetCurrentWeapon().StartReload();
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
}
