using UnityEngine;

public class MoneyTrap : MonoBehaviour
{
    [Header("----- Money Gen Settings -----")]
    [SerializeField] int amountPerPayout = 5;
    [SerializeField] float payoutInterval = 5f;
    [Range(0.1f, 30)][SerializeField] float minSecondsBetweenPayout = 0.1f;

    [Header("----- Upgrade Settings -----")]
    [SerializeField] float amountPerPayoutMult = 1f;
    [SerializeField] float payoutIntervalMult = 1f;

    [Header("----- Refs -----")]
    [SerializeField] CurrencyManager currencyManager;



}
