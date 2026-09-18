using System.Text;
using TMPro;
using UnityEngine;

public class MapPlayerStatusUI : MonoBehaviour
{
    [Header("지도 화면 표시 - 필요한 항목만 연결")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text symbolDeckText;

    private PlayerRunData runData;

    private readonly StringBuilder builder =
        new StringBuilder();

    private void OnEnable()
    {
        runData = PlayerRunData.GetOrCreate();

        runData.Changed -= Refresh;
        runData.Changed += Refresh;

        // 맵 Canvas가 다시 켜졌을 때
        // 반드시 최신 값으로 표시한다.
        Refresh();
    }

    private void OnDisable()
    {
        if (runData != null)
            runData.Changed -= Refresh;
    }

    public void Refresh()
    {
        if (runData == null)
            return;

        // 체력
        if (healthText != null)
        {
            healthText.text = runData.HasHealthData
                ? $"{runData.CurrentHealth} / {runData.MaxHealth}"
                : "- / -";
        }

        // 코인 보유량
        if (coinText != null)
        {
            builder.Clear();

            foreach (CoinInventoryEntry entry in runData.Coins)
            {
                if (entry == null ||
                    entry.Coin == null ||
                    entry.Count <= 0)
                {
                    continue;
                }

                builder.AppendLine(
                    $"{entry.Coin.DisplayName} x{entry.Count}"
                );
            }

            coinText.text = builder.Length > 0
                ? builder.ToString().TrimEnd()
                : "보유 코인 없음";
        }

        // 문양 덱
        if (symbolDeckText != null)
        {
            builder.Clear();

            foreach (SymbolDeckEntry entry in runData.Symbols)
            {
                if (entry == null ||
                    entry.Symbol == null ||
                    entry.Count <= 0)
                {
                    continue;
                }

                builder.AppendLine(
                    $"{entry.Symbol.DisplayName} x{entry.Count}"
                );
            }

            symbolDeckText.text = builder.Length > 0
                ? builder.ToString().TrimEnd()
                : "보유 문양 없음";
        }
    }
}