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
                    
                    break;
                case EssenceType.PLAYERAMMO:
                    
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
