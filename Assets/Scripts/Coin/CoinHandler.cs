using System;
using UnityEngine;


[DefaultExecutionOrder(-100)]
public class CoinHandler : MonoBehaviour
{
    public static CoinHandler Instance { get; private set; }

    public int TotalCoins { get; private set; }
    public static event Action<int> OnCoinCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Optionally: DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Broadcast current value so any late UI can sync immediately
        OnCoinCountChanged?.Invoke(TotalCoins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        TotalCoins += amount;
        OnCoinCountChanged?.Invoke(TotalCoins);
    }

    public void ResetCoins()
    {
        TotalCoins = 0;
        OnCoinCountChanged?.Invoke(TotalCoins);
    }
}