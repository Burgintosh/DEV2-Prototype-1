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

    [Header("----- Audio -----")]
    [SerializeField] AudioSource moneyAudioSource;
    [SerializeField] AudioClip moneyPayoutAudioClip;
    [SerializeField] float moneyPayoutVol = 1f;
    [SerializeField] SoundCategory moneySoundCategory = SoundCategory.Trap;

    [Header("----- VFX Feedback -----")]
    [SerializeField] ParticleSystem moneyPayoutVFX;
    [SerializeField] Transform moneyPayoutVFXPos;

    [Header("----- Debug -----")]
    //[SerializeField] bool showDebugLogs = false;

    Coroutine payCoroutine;
    bool isPayingOut;

    private void Awake()
    {
        GetCurrenyManager();
    }

    private void OnEnable()
    {
        WaveManager.OnFirstWaveStart += TryStartPayingPlayer;

        if (WaveManager.HasFirstWaveStarted)
        {
            TryStartPayingPlayer();
        }
        //else if (showDebugLogs)
        //{
        //    Debug.Log("[MoneyTrap] Waiting for first wave to start", this);
        //}

    }

    private void OnDisable()
    {
        WaveManager.OnFirstWaveStart -= TryStartPayingPlayer;

        StopPayingPlayer();
    }

    void GetCurrenyManager()
    {
        if(currencyManager != null)
        {
            //if (showDebugLogs)
            //{
            //    Debug.Log("[MoneyTrap] CurrencyManager was already assigned", this);
            //}

            return;
        }

        if(gamemanager.instance != null)
        {
            currencyManager = gamemanager.instance.currencyManager;
        }

        //if (showDebugLogs)
        //{
        //    Debug.Log("[MoneyTrap] CurrencyManager Found!");
        //}

    }

    void TryStartPayingPlayer()
    {
        if (isPayingOut)
        {
            return;
        }

        StartPayingPlayer();
    }

    void StartPayingPlayer()
    {
        isPayingOut = true;

        if(payCoroutine != null)
        {
            StopCoroutine(payCoroutine);
        }

        payCoroutine = StartCoroutine(PayPlayerLoop());

        //if (showDebugLogs)
        //{
        //    Debug.Log("[MoneyTrap] Started paying player. At interval: " + GetCurrentPayInterval(), this);
        //}

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

        //if (showDebugLogs)
        //{
        //    Debug.Log("[MoneyTrap] Stopping playing the player", this);
        //}

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
            //Debug.LogWarning("[Money Trap] Currency manager is null", this);
            return;
        }

        int amountToPay = GetCurrentAmountPerPay();

        //if (showDebugLogs)
        //{
        //    Debug.Log("[MoneyTrap] Paying player: " + amountToPay, this);
        //}

        currencyManager.AddCurrency(amountToPay);

        PlayPayoutFeedback();

        //if (showDebugLogs)
        //{
        //    Debug.Log("[MoneyTrap] Player currency after payout: " + currencyManager.GetCurrentCurrency(), this);
        //}
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

    void PlayPayoutFeedback()
    {
        PlayPayoutSFX();
        PlayPayoutVFX();
    }

    void PlayPayoutSFX()
    {
        if(moneyAudioSource == null)
        {
            //Debug.LogWarning("[MoneyTrap] Money AudioSource is missing", this);
            return;
        }

        if(moneyPayoutAudioClip == null)
        {
            //Debug.LogWarning("[MoneyTrap] Money payout AudioClip is missing", this);
            return;
        }

        if(SoundManager.Instance == null)
        {
            //Debug.LogWarning("[Money Trap] SoundManager instance is missing.", this);
            return;
        }

        SoundManager.Instance.PlayWithRandomPitch(moneyAudioSource, moneyPayoutAudioClip, moneyPayoutVol, moneySoundCategory, true);
    }

    void PlayPayoutVFX()
    {
        if(moneyPayoutVFX == null)
        {
            return;
        }

        if(moneyPayoutVFXPos != null)
        {
            moneyPayoutVFX.transform.position = moneyPayoutVFXPos.position;
            moneyPayoutVFX.transform.rotation = moneyPayoutVFXPos.rotation;
        }

        moneyPayoutVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        moneyPayoutVFX.Play(true);

        //Debug.Log("[MoneyTrap] Playing payout VFX at: " + moneyPayoutVFX.transform.position, this);
    }
}
