using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] int startingCurrency = 100;

    private int currentCurrency;

    public event Action<int> OnCurrencyChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCurrency = startingCurrency;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public int GetCurrentCurrency()
    {
        return currentCurrency;
    }
    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public bool TrySpendCurrency(int amount)
    {
        if (currentCurrency < amount)
        {
            return false;
        }
        else
        {
            currentCurrency -= amount;
            OnCurrencyChanged?.Invoke(currentCurrency);
            return true;
        }

    }
}
