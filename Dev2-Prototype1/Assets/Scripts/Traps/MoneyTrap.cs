using System.Collections;
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

    Coroutine payCoroutine;
    bool isPayingOut;

    private void Awake()
    {
        GetCurrenyManager();
    }

    private void OnEnable()
    {
        StartPayingPlayer();
    }

    private void OnDisable()
    {
        StopPayingPlayer();
    }

    void GetCurrenyManager()
    {
        if(currencyManager != null)
        {
            return;
        }

        if(gamemanager.instance != null)
        {
            currencyManager = gamemanager.instance.currencyManager;
        }

    }

    void StartPayingPlayer()
    {
        isPayingOut = true;

        if(payCoroutine != null)
        {
            StopCoroutine(payCoroutine);
        }

        payCoroutine = StartCoroutine(PayPlayerLoop());
    }

    void StopPayingPlayer()
    {
        isPayingOut = false;

        if(payCoroutine == null)
        {
            return;
        }

        StopCoroutine(payCoroutine);
        payCoroutine = null;
    }

    IEnumerator PayPlayerLoop()
    {
        while (isPayingOut)
        {
            yield return new WaitForSeconds(GetCurrentPayInterval());

            if (isPayingOut)
            {
                PayPlayer();
            }
        }
    }

    void PayPlayer()
    {
        if(currencyManager == null)
        {
            Debug.LogWarning("[Money Trap] Currency manager is null", this);
            return;
        }

        currencyManager.AddCurrency(GetCurrentAmountPerPay());
    }

    public int GetCurrentAmountPerPay()
    {
        return Mathf.RoundToInt(amountPerPayout * amountPerPayoutMult);
    }

    public float GetCurrentPayInterval()
    {
        float currentPayInterval = payoutInterval * payoutIntervalMult;

        return Mathf.Max(minSecondsBetweenPayout, currentPayInterval);
    }

    public void SetAmountPerPayMult(float _NewAmountPerPayMult)
    {
        amountPerPayoutMult = _NewAmountPerPayMult;
    }

    public void SetPayoutIntervalMult(float _NewPayIntervalMult)
    {
        payoutIntervalMult = Mathf.Max(0.1f, _NewPayIntervalMult);
    }

}
