using System.Collections.Generic;
using UnityEngine;

public class PlayerCoinInventory : MonoBehaviour
{
    [Header("플레이어 - 기존 구매 기능용")]
    [SerializeField] private NewPlayer player;

    [SerializeField, Min(0)]
    private int minimumRemainingHealth = 1;

    [Header("새 런의 초기 코인")]
    [Tooltip(
        "런 데이터가 아직 없을 때만 사용합니다. " +
        "현재 보유량은 PlayerRunData에 있습니다."
    )]
    [SerializeField]
    private List<CoinInventoryEntry> coins =
        new List<CoinInventoryEntry>();

    private PlayerRunData Run
    {
        get
        {
            PlayerRunData state =
                PlayerRunData.GetOrCreate();

            state.InitializeCoinsIfNeeded(coins);

            return state;
        }
    }

    public IReadOnlyList<CoinInventoryEntry> Coins =>
        Run.Coins;

    private void Awake()
    {
        if (player == null)
            player = GetComponent<NewPlayer>();

        PlayerRunData.GetOrCreate()
            .InitializeCoinsIfNeeded(coins);
    }

    /// <summary>
    /// 기존 구매 메서드 유지.
    ///
    /// 배치 시 체력을 지불하는 현재 베팅 UI에서
    /// 이 메서드까지 함께 호출하지 않는다.
    /// </summary>
    public bool PurchaseCoin(BetCoinData coin)
    {
        if (coin == null || player == null)
            return false;

        if (GetCoinCount(coin) == int.MaxValue)
            return false;

        if (!player.TryPayHealthCost(
                coin.HealthCost,
                minimumRemainingHealth))
        {
            return false;
        }

        AddCoin(coin, 1);

        return true;
    }

    public void AddCoin(
        BetCoinData coin,
        int amount = 1
    )
    {
        if (coin == null || amount <= 0)
            return;

        PlayerRunData state = Run;

        CoinInventoryEntry entry =
            state.MutableCoins.Find(
                item => item != null &&
                        item.Coin == coin
            );

        if (entry == null)
        {
            state.MutableCoins.Add(
                new CoinInventoryEntry(coin, amount)
            );
        }
        else
        {
            if (amount > int.MaxValue - entry.Count)
            {
                Debug.LogError(
                    "코인 보유 개수 범위를 초과했습니다.",
                    this
                );

                return;
            }

            entry.Add(amount);
        }

        state.NotifyChanged();
    }

    public bool TryConsumeCoin(
        BetCoinData coin,
        int amount = 1
    )
    {
        if (coin == null || amount <= 0)
            return false;

        PlayerRunData state = Run;

        CoinInventoryEntry entry =
            state.MutableCoins.Find(
                item => item != null &&
                        item.Coin == coin
            );

        if (entry == null || entry.Count < amount)
            return false;

        entry.Add(-amount);

        state.NotifyChanged();

        return true;
    }

    public int GetCoinCount(BetCoinData coin)
    {
        if (coin == null)
            return 0;

        foreach (CoinInventoryEntry entry in Coins)
        {
            if (entry != null &&
                entry.Coin == coin)
            {
                return entry.Count;
            }
        }

        return 0;
    }

    public void SetCoins(
        IEnumerable<CoinInventoryEntry> entries
    )
    {
        PlayerRunData.GetOrCreate()
            .SetCoins(entries);
    }
}