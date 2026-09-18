using System;
using UnityEngine;

[Serializable]
public class DuplicateBetRecord
{
    [SerializeField]
    private int requiredCount;

    [SerializeField]
    private BetCoinData coin;


    public int RequiredCount =>
        requiredCount;

    public BetCoinData Coin =>
        coin;


    public DuplicateBetRecord(
        int requiredCount,
        BetCoinData coin
    )
    {
        this.requiredCount =
            requiredCount;

        this.coin =
            coin;
    }
}