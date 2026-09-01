using System.Collections.Generic;
using UnityEngine;

public class RunSelectionData : MonoBehaviour
{
    public static RunSelectionData Instance { get; private set; }

    [Header("선택 결과")]
    [SerializeField] private StartingDeckData selectedDeck;
    [SerializeField] private StartingRelicSetData selectedStartingRelicSet;

    [Header("현재 런 데이터")]
    [SerializeField] private List<Symbol> currentDeck = new List<Symbol>();
    [SerializeField] private List<RelicData> currentRelics = new List<RelicData>();

    public StartingDeckData SelectedDeck => selectedDeck;
    public StartingRelicSetData SelectedStartingRelicSet => selectedStartingRelicSet;

    public IReadOnlyList<Symbol> CurrentDeck => currentDeck;
    public IReadOnlyList<RelicData> CurrentRelics => currentRelics;

    public bool HasDeckSelection =>
        selectedDeck != null &&
        currentDeck.Count > 0;

    public bool HasRelicSelection =>
        selectedStartingRelicSet != null &&
        currentRelics.Count > 0;

    public bool IsReady =>
        HasDeckSelection &&
        HasRelicSelection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 시작 덱 조합을 선택하고 해당 조합의 모든 심볼을 현재 덱에 적용한다.
    /// </summary>
    public void SelectDeck(StartingDeckData deck)
    {
        selectedDeck = deck;
        currentDeck.Clear();

        if (deck == null)
        {
            Debug.LogWarning("선택한 시작 덱이 없습니다.", this);
            return;
        }

        if (deck.Symbols == null)
        {
            Debug.LogWarning(
                $"{deck.name}에 시작 심볼 목록이 없습니다.",
                deck
            );

            return;
        }

        foreach (Symbol symbol in deck.Symbols)
        {
            if (symbol != null)
                currentDeck.Add(symbol);
        }

        Debug.Log(
            $"시작 덱 선택: {deck.DeckName}, 심볼 수: {currentDeck.Count}",
            this
        );
    }

    /// <summary>
    /// 시작 유물 조합을 선택하고 해당 조합의 모든 유물을 현재 런에 적용한다.
    /// </summary>
    public void SelectStartingRelicSet(StartingRelicSetData relicSet)
    {
        selectedStartingRelicSet = relicSet;
        currentRelics.Clear();

        if (relicSet == null)
        {
            Debug.LogWarning("선택한 시작 유물 조합이 없습니다.", this);
            return;
        }

        if (relicSet.Relics == null)
        {
            Debug.LogWarning(
                $"{relicSet.name}에 시작 유물 목록이 없습니다.",
                relicSet
            );

            return;
        }

        foreach (RelicData relic in relicSet.Relics)
        {
            if (relic == null)
                continue;

            // 중복을 허용하지 않는 유물이 이미 들어 있다면 추가하지 않는다.
            if (!relic.AllowDuplicates && currentRelics.Contains(relic))
                continue;

            currentRelics.Add(relic);
        }

        Debug.Log(
            $"시작 유물 조합 선택: {relicSet.SetName}, 유물 수: {currentRelics.Count}",
            this
        );
    }

    /// <summary>
    /// 현재 덱 전체를 전달받은 심볼 목록으로 교체한다.
    /// </summary>
    public void SetCurrentDeck(IEnumerable<Symbol> symbols)
    {
        currentDeck.Clear();

        if (symbols == null)
            return;

        foreach (Symbol symbol in symbols)
        {
            if (symbol != null)
                currentDeck.Add(symbol);
        }
    }

    /// <summary>
    /// 현재 덱에 심볼 하나를 추가한다.
    /// </summary>
    public void AddSymbol(Symbol symbol)
    {
        if (symbol == null)
            return;

        currentDeck.Add(symbol);
    }

    /// <summary>
    /// 현재 덱에서 지정한 심볼 하나를 제거한다.
    /// </summary>
    public bool RemoveSymbol(Symbol symbol)
    {
        if (symbol == null)
            return false;

        return currentDeck.Remove(symbol);
    }

    /// <summary>
    /// 현재 유물 전체를 전달받은 유물 목록으로 교체한다.
    /// </summary>
    public void SetCurrentRelics(IEnumerable<RelicData> relics)
    {
        currentRelics.Clear();

        if (relics == null)
            return;

        foreach (RelicData relic in relics)
        {
            if (relic == null)
                continue;

            if (!relic.AllowDuplicates && currentRelics.Contains(relic))
                continue;

            currentRelics.Add(relic);
        }
    }

    /// <summary>
    /// 현재 런에 유물 하나를 추가한다.
    /// </summary>
    public void AddRelic(RelicData relic)
    {
        if (relic == null)
            return;

        if (!relic.AllowDuplicates && currentRelics.Contains(relic))
            return;

        currentRelics.Add(relic);
    }

    /// <summary>
    /// 현재 런에서 지정한 유물 하나를 제거한다.
    /// </summary>
    public bool RemoveRelic(RelicData relic)
    {
        if (relic == null)
            return false;

        return currentRelics.Remove(relic);
    }

    /// <summary>
    /// 현재 런의 선택 결과와 보유 목록을 전부 초기화한다.
    /// </summary>
    public void ClearRun()
    {
        selectedDeck = null;
        selectedStartingRelicSet = null;

        currentDeck.Clear();
        currentRelics.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}