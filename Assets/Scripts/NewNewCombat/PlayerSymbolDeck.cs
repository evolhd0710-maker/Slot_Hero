using System.Collections.Generic;
using UnityEngine;

public class PlayerSymbolDeck : MonoBehaviour
{
    [Header("새 런의 초기 문양 덱")]
    [Tooltip(
        "런 데이터가 아직 없을 때만 사용합니다. " +
        "현재 보유량은 PlayerRunData에 있습니다."
    )]
    [SerializeField]
    private List<SymbolDeckEntry> symbols =
        new List<SymbolDeckEntry>();

    private PlayerRunData Run
    {
        get
        {
            PlayerRunData state =
                PlayerRunData.GetOrCreate();

            state.InitializeDeckIfNeeded(symbols);

            return state;
        }
    }

    public IReadOnlyList<SymbolDeckEntry> Symbols =>
        Run.Symbols;

    public int TotalSymbolCount
    {
        get
        {
            int total = 0;

            foreach (SymbolDeckEntry entry in Symbols)
            {
                if (entry == null ||
                    entry.Symbol == null ||
                    entry.Count <= 0)
                {
                    continue;
                }

                total += entry.Count;
            }

            return total;
        }
    }

    private void Awake()
    {
        PlayerRunData.GetOrCreate()
            .InitializeDeckIfNeeded(symbols);
    }

    public void AddSymbol(
        SymbolData symbol,
        int amount = 1
    )
    {
        if (symbol == null || amount <= 0)
            return;

        PlayerRunData state = Run;

        SymbolDeckEntry entry =
            state.MutableSymbols.Find(
                item => item != null &&
                        item.Symbol == symbol
            );

        if (entry == null)
        {
            state.MutableSymbols.Add(
                new SymbolDeckEntry(symbol, amount)
            );
        }
        else
        {
            if (amount > int.MaxValue - entry.Count)
            {
                Debug.LogError(
                    "문양 보유 개수 범위를 초과했습니다.",
                    this
                );

                return;
            }

            entry.AddCount(amount);
        }

        state.NotifyChanged();
    }

    public void RemoveSymbol(
        SymbolData symbol,
        int amount = 1
    )
    {
        if (symbol == null || amount <= 0)
            return;

        PlayerRunData state = Run;

        SymbolDeckEntry entry =
            state.MutableSymbols.Find(
                item => item != null &&
                        item.Symbol == symbol
            );

        if (entry == null)
            return;

        entry.SetCount(
            Mathf.Max(0, entry.Count - amount)
        );

        if (entry.Count == 0)
            state.MutableSymbols.Remove(entry);

        state.NotifyChanged();
    }

    public int GetSymbolCount(SymbolData symbol)
    {
        if (symbol == null)
            return 0;

        foreach (SymbolDeckEntry entry in Symbols)
        {
            if (entry != null &&
                entry.Symbol == symbol)
            {
                return entry.Count;
            }
        }

        return 0;
    }

    public bool HasSymbol(SymbolData symbol)
    {
        return GetSymbolCount(symbol) > 0;
    }

    public void SetDeck(
        IEnumerable<SymbolDeckEntry> entries
    )
    {
        PlayerRunData.GetOrCreate()
            .SetSymbols(entries);
    }

    /// <summary>
    /// 비복원추출에 사용할 임시 복사본을 만든다.
    /// 이 목록에서 문양을 제거해도 실제 보유량은 바뀌지 않는다.
    /// </summary>
    public List<SymbolData> CreateDrawPool()
    {
        List<SymbolData> pool =
            new List<SymbolData>();

        foreach (SymbolDeckEntry entry in Symbols)
        {
            if (entry == null ||
                entry.Symbol == null ||
                entry.Count <= 0)
            {
                continue;
            }

            for (int i = 0; i < entry.Count; i++)
                pool.Add(entry.Symbol);
        }

        return pool;
    }

    /// <summary>
    /// 회전 연출용 문양 선택.
    /// 실제 결과의 비복원추출 규칙과는 별개다.
    /// </summary>
    public SymbolData GetRandomPreviewSymbol()
    {
        int total = TotalSymbolCount;

        if (total <= 0)
            return null;

        int roll = Random.Range(0, total);

        foreach (SymbolDeckEntry entry in Symbols)
        {
            if (entry == null ||
                entry.Symbol == null ||
                entry.Count <= 0)
            {
                continue;
            }

            if (roll < entry.Count)
                return entry.Symbol;

            roll -= entry.Count;
        }

        return null;
    }
}