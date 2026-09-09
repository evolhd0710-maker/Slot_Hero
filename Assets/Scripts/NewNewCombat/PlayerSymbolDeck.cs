using System.Collections.Generic;
using UnityEngine;

public class PlayerSymbolDeck : MonoBehaviour
{
    [Header("현재 문양 덱")]
    [SerializeField]
    private List<SymbolDeckEntry> symbols =
        new List<SymbolDeckEntry>();

    public IReadOnlyList<SymbolDeckEntry> Symbols =>
        symbols;

    public int TotalSymbolCount
    {
        get
        {
            int total = 0;

            foreach (SymbolDeckEntry entry in symbols)
            {
                if (entry == null)
                    continue;

                if (entry.Symbol == null)
                    continue;

                if (entry.Count <= 0)
                    continue;

                total += entry.Count;
            }

            return total;
        }
    }

    public void AddSymbol(
        SymbolData symbol,
        int amount = 1
    )
    {
        if (symbol == null || amount <= 0)
            return;

        SymbolDeckEntry entry =
            FindEntry(symbol);

        if (entry != null)
        {
            entry.AddCount(amount);
            return;
        }

        symbols.Add(
            new SymbolDeckEntry(
                symbol,
                amount
            )
        );
    }

    public void RemoveSymbol(
        SymbolData symbol,
        int amount = 1
    )
    {
        if (symbol == null || amount <= 0)
            return;

        SymbolDeckEntry entry =
            FindEntry(symbol);

        if (entry == null)
            return;

        entry.AddCount(-amount);

        if (entry.Count <= 0)
            symbols.Remove(entry);
    }

    public int GetSymbolCount(
        SymbolData symbol
    )
    {
        SymbolDeckEntry entry =
            FindEntry(symbol);

        if (entry == null)
            return 0;

        return entry.Count;
    }

    public bool HasSymbol(
        SymbolData symbol
    )
    {
        return GetSymbolCount(symbol) > 0;
    }

    private SymbolDeckEntry FindEntry(
        SymbolData symbol
    )
    {
        foreach (SymbolDeckEntry entry in symbols)
        {
            if (entry == null)
                continue;

            if (entry.Symbol == symbol)
                return entry;
        }

        return null;
    }

    /// <summary>
    /// 현재 덱의 실제 문양 개수만큼
    /// 임시 추첨 풀을 만든다.
    ///
    /// 예:
    /// 수은 x3
    /// 물고기 x2
    ///
    /// 결과:
    /// [수은, 수은, 수은, 물고기, 물고기]
    /// </summary>
    public List<SymbolData> CreateDrawPool()
    {
        List<SymbolData> drawPool =
            new List<SymbolData>();

        foreach (SymbolDeckEntry entry in symbols)
        {
            if (entry == null)
                continue;

            if (entry.Symbol == null)
                continue;

            if (entry.Count <= 0)
                continue;

            for (int i = 0; i < entry.Count; i++)
            {
                drawPool.Add(
                    entry.Symbol
                );
            }
        }

        return drawPool;
    }

    /// <summary>
    /// 슬롯 회전 중 보여줄 임시 이미지용.
    /// 실제 결과 추첨에는 사용하지 않는다.
    /// </summary>
    public SymbolData GetRandomPreviewSymbol()
    {
        List<SymbolData> pool =
            CreateDrawPool();

        if (pool.Count == 0)
            return null;

        int randomIndex =
            Random.Range(
                0,
                pool.Count
            );

        return pool[randomIndex];
    }
}