using System;
using UnityEngine;

[Serializable]
public class CoinInventoryEntry
{
    [SerializeField] private BetCoinData coin;
    [SerializeField, Min(0)] private int count;

    public BetCoinData Coin => coin;
    public int Count => count;

    public CoinInventoryEntry(
        BetCoinData coin,
        int count
    )
    {
        this.coin = coin;
        this.count = Mathf.Max(0, count);
    }

    public void Add(int amount)
    {
        count = Mathf.Max(
            0,
            count + amount
        );
    }
}