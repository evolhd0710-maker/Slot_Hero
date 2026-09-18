using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class MapPlayerRunInitializer : MonoBehaviour
{
    [Header("시작 플레이어 데이터")]
    [Tooltip(
        "새 런의 초기 최대 체력에 사용합니다. " +
        "전투의 NewPlayer에도 같은 CharacterData를 연결하세요."
    )]
    [SerializeField]
    private CharacterData initialPlayerData;

    [Header("시작 문양 덱")]
    [Tooltip(
        "새 런에 사용할 문양과 보유 개수입니다. " +
        "이미 덱이 초기화된 런에서는 다시 적용하지 않습니다."
    )]
    [SerializeField]
    private List<SymbolDeckEntry> initialSymbols =
        new List<SymbolDeckEntry>();

    [Header("시작 코인")]
    [Tooltip(
        "새 런에 사용할 코인과 보유 개수입니다. " +
        "이미 코인이 초기화된 런에서는 다시 적용하지 않습니다."
    )]
    [SerializeField]
    private List<CoinInventoryEntry> initialCoins =
        new List<CoinInventoryEntry>();

    private void Awake()
    {
        InitializeRunIfNeeded();
    }

    /// <summary>
    /// 첫 맵 진입 시 필요한 런 데이터를 준비한다.
    /// 기존 런의 체력, 문양, 코인은 덮어쓰지 않는다.
    /// </summary>
    public void InitializeRunIfNeeded()
    {
        PlayerRunData runData =
            PlayerRunData.GetOrCreate();

        bool needsHealth = !runData.HasHealthData;
        bool needsDeck = !runData.HasDeckData;
        bool needsCoins = !runData.HasCoinData;

        // 이미 준비된 런이라면 아무것도 변경하지 않는다.
        if (!needsHealth && !needsDeck && !needsCoins)
            return;

        // 실제 데이터를 적용하기 전에 필요한 설정을 검사한다.
        if (needsHealth)
        {
            if (initialPlayerData == null)
            {
                Debug.LogError(
                    "MapPlayerRunInitializer에 " +
                    "Initial Player Data가 연결되지 않았습니다.",
                    this
                );

                return;
            }

            if (initialPlayerData.maxHealth <= 0)
            {
                Debug.LogError(
                    "시작 플레이어의 최대 체력은 1 이상이어야 합니다.",
                    initialPlayerData
                );

                return;
            }
        }

        // 현재 슬롯은 6개를 비복원추출하므로
        // 새 런의 시작 덱에는 최소 6장이 필요하다.
        if (needsDeck && GetInitialSymbolCount() < 6)
        {
            Debug.LogError(
                "시작 문양 덱에는 유효한 문양이 최소 6장 필요합니다. " +
                "Initial Symbols의 Symbol과 Count를 확인하세요.",
                this
            );

            return;
        }

        // 새 런의 체력만 최초 1회 설정한다.
        if (needsHealth)
        {
            runData.InitializeHealthIfNeeded(
                initialPlayerData.maxHealth
            );
        }

        // 새 런의 문양 덱만 최초 1회 설정한다.
        if (needsDeck)
        {
            runData.InitializeDeckIfNeeded(
                initialSymbols
            );
        }

        // 코인이 0개인 초기 구성도 허용한다.
        if (needsCoins)
        {
            runData.InitializeCoinsIfNeeded(
                initialCoins
            );
        }

        Debug.Log(
            $"맵에서 런 데이터 준비 완료 | " +
            $"체력: {runData.CurrentHealth}/{runData.MaxHealth}",
            this
        );
    }

    private long GetInitialSymbolCount()
    {
        long total = 0;

        if (initialSymbols == null)
            return total;

        foreach (SymbolDeckEntry entry in initialSymbols)
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