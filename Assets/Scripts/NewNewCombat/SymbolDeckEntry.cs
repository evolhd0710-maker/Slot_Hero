using System;
using UnityEngine;

[Serializable]
public class SymbolDeckEntry
{
    [SerializeField]
    private SymbolData symbol;

    [SerializeField, Min(0)]
    private int count = 1;

    public SymbolData Symbol => symbol;
    public int Count => count;

    public SymbolDeckEntry(
        SymbolData symbol,
        int count
    )
    {
        this.symbol = symbol;
        this.count = Mathf.Max(0, count);
    }

    public void SetCount(int value)
    {
        count = Mathf.Max(0, value);
    }

    public void AddCount(int amount)
    {
        count = Mathf.Max(
            0,
            count + amount
        );
    }
}