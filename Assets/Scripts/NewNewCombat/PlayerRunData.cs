using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRunData : MonoBehaviour
{
    public static PlayerRunData Instance { get; private set; }

    [Header("현재 런 체력")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    [Header("현재 런 보유량")]
    [SerializeField]
    private List<SymbolDeckEntry> currentSymbols =
        new List<SymbolDeckEntry>();

    [SerializeField]
    private List<CoinInventoryEntry> currentCoins =
        new List<CoinInventoryEntry>();

    // 0 HP / 빈 덱 / 코인 0개도 유효한 저장 상태다.
    [SerializeField] private bool healthInitialized;
    [SerializeField] private bool deckInitialized;
    [SerializeField] private bool coinsInitialized;

    private ReadOnlyCollection<SymbolDeckEntry> symbolView;
    private ReadOnlyCollection<CoinInventoryEntry> coinView;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public bool HasHealthData => healthInitialized;
    public bool HasDeckData => deckInitialized;
    public bool HasCoinData => coinsInitialized;

    public IReadOnlyList<SymbolDeckEntry> Symbols =>
        symbolView ?? (symbolView = currentSymbols.AsReadOnly());

    public IReadOnlyList<CoinInventoryEntry> Coins =>
        coinView ?? (coinView = currentCoins.AsReadOnly());

    // 실제 목록 변경은 인벤토리/덱 스크립트에서만 수행한다.
    internal List<SymbolDeckEntry> MutableSymbols => currentSymbols;
    internal List<CoinInventoryEntry> MutableCoins => currentCoins;

    public event Action Changed;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticReference()
    {
        Instance = null;
    }

    public static PlayerRunData GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject root = new GameObject("PlayerRunData");

        return root.AddComponent<PlayerRunData>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // UI나 전투 오브젝트를 보존하지 않고
        // 이 데이터 루트만 보존한다.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeHealthIfNeeded(int initialMaxHealth)
    {
        if (healthInitialized)
            return;

        StoreHealth(initialMaxHealth, initialMaxHealth);
    }

    public void StoreHealth(int health, int maximumHealth)
    {
        maximumHealth = Mathf.Max(1, maximumHealth);
        health = Mathf.Clamp(health, 0, maximumHealth);

        if (healthInitialized &&
            currentHealth == health &&
            maxHealth == maximumHealth)
        {
            return;
        }

        currentHealth = health;
        maxHealth = maximumHealth;
        healthInitialized = true;

        NotifyChanged();
    }

    public void InitializeDeckIfNeeded(
        IEnumerable<SymbolDeckEntry> entries
    )
    {
        if (!deckInitialized)
            SetSymbols(entries);
    }

    public void InitializeCoinsIfNeeded(
        IEnumerable<CoinInventoryEntry> entries
    )
    {
        if (!coinsInitialized)
            SetCoins(entries);
    }

    public void SetSymbols(IEnumerable<SymbolDeckEntry> entries)
    {
        // 자기 자신의 목록이 전달되어도 안전하도록 먼저 복사한다.
        List<SymbolDeckEntry> copy = new List<SymbolDeckEntry>();

        if (entries != null)
        {
            foreach (SymbolDeckEntry entry in entries)
            {
                if (entry == null ||
                    entry.Symbol == null ||
                    entry.Count <= 0)
                {
                    continue;
                }

                SymbolDeckEntry existing =
                    copy.Find(item => item.Symbol == entry.Symbol);

                if (existing == null)
                {
                    copy.Add(
                        new SymbolDeckEntry(
                            entry.Symbol,
                            entry.Count
                        )
                    );
                }
                else
                {
                    // 같은 SO가 여러 항목에 있으면 보유량을 합친다.
                    int combined = (int)Math.Min(
                        int.MaxValue,
                        (long)existing.Count + entry.Count
                    );

                    existing.SetCount(combined);
                }
            }
        }

        currentSymbols.Clear();
        currentSymbols.AddRange(copy);

        deckInitialized = true;

        NotifyChanged();
    }

    public void SetCoins(IEnumerable<CoinInventoryEntry> entries)
    {
        List<CoinInventoryEntry> copy =
            new List<CoinInventoryEntry>();

        if (entries != null)
        {
            foreach (CoinInventoryEntry entry in entries)
            {
                if (entry == null ||
                    entry.Coin == null ||
                    entry.Count <= 0)
                {
                    continue;
                }

                CoinInventoryEntry existing =
                    copy.Find(item => item.Coin == entry.Coin);

                if (existing == null)
                {
                    copy.Add(
                        new CoinInventoryEntry(
                            entry.Coin,
                            entry.Count
                        )
                    );
                }
                else
                {
                    int amount = (int)Math.Min(
                        entry.Count,
                        (long)int.MaxValue - existing.Count
                    );

                    existing.Add(amount);
                }
            }
        }

        currentCoins.Clear();
        currentCoins.AddRange(copy);

        coinsInitialized = true;

        NotifyChanged();
    }

    /// <summary>
    /// 새 게임을 시작할 때만 호출한다.
    /// 맵 복귀나 다음 전투 진입 시에는 호출하지 않는다.
    /// </summary>
    public void ClearRun()
    {
        currentHealth = 0;
        maxHealth = 0;

        currentSymbols.Clear();
        currentCoins.Clear();

        healthInitialized = false;
        deckInitialized = false;
        coinsInitialized = false;

        NotifyChanged();
    }

    internal void NotifyChanged()
    {
        Changed?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}