using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SixReelSlotMachine : MonoBehaviour
{
    private const int ReelCount = 6;

    [Header("플레이어 문양 덱")]
    [SerializeField]
    private PlayerSymbolDeck playerSymbolDeck;

    [Header("6개 릴")]
    [SerializeField]
    private Image[] reelImages =
        new Image[ReelCount];

    [Header("스핀 연출")]
    [SerializeField, Min(0.01f)]
    private float symbolChangeInterval = 0.05f;

    [SerializeField, Min(0f)]
    private float firstStopDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float reelStopInterval = 0.15f;

    [Header("버튼")]
    [SerializeField]
    private Button spinButton;

    private readonly List<SymbolData> resultSymbols =
        new List<SymbolData>();

    private bool isSpinning;

    public bool IsSpinning =>
        isSpinning;

    public IReadOnlyList<SymbolData> ResultSymbols =>
        resultSymbols;

    public event Action<IReadOnlyList<SymbolData>>
        OnSpinCompleted;

    private void Awake()
    {
        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(Spin);
            spinButton.onClick.AddListener(Spin);
        }

        ValidateReferences();
    }

    private void OnDestroy()
    {
        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(Spin);
        }
    }

    private void ValidateReferences()
    {
        if (reelImages == null ||
            reelImages.Length != ReelCount)
        {
            Debug.LogError(
                $"릴 이미지는 정확히 {ReelCount}개 필요합니다.",
                this
            );
        }
    }

    public void Spin()
    {
        if (isSpinning)
            return;

        if (playerSymbolDeck == null)
        {
            Debug.LogError(
                "PlayerSymbolDeck이 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (playerSymbolDeck.TotalSymbolCount < ReelCount)
        {
            Debug.LogError(
                $"슬롯을 돌리려면 문양이 최소 {ReelCount}개 필요합니다. " +
                $"현재 문양 수: {playerSymbolDeck.TotalSymbolCount}",
                this
            );

            return;
        }

        if (reelImages == null ||
            reelImages.Length != ReelCount)
        {
            Debug.LogError(
                $"릴 이미지가 정확히 {ReelCount}개 필요합니다.",
                this
            );

            return;
        }

        StartCoroutine(
            SpinRoutine()
        );
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        if (spinButton != null)
            spinButton.interactable = false;

        resultSymbols.Clear();

        // 플레이어의 현재 덱을 복사해서
        // 이번 스핀 전용 추첨 풀을 만든다.
        List<SymbolData> drawPool =
            playerSymbolDeck.CreateDrawPool();

        // ===== 실제 결과 비복원추출 =====

        for (int i = 0; i < ReelCount; i++)
        {
            if (drawPool.Count == 0)
            {
                Debug.LogError(
                    "문양 추첨 풀이 부족합니다.",
                    this
                );

                break;
            }

            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    drawPool.Count
                );

            SymbolData selectedSymbol =
                drawPool[randomIndex];

            resultSymbols.Add(
                selectedSymbol
            );

            // 핵심:
            // 뽑힌 문양 1개를 이번 추첨 풀에서 제거한다.
            drawPool.RemoveAt(
                randomIndex
            );
        }

        if (resultSymbols.Count != ReelCount)
        {
            isSpinning = false;

            if (spinButton != null)
                spinButton.interactable = true;

            yield break;
        }

        bool[] stopped =
            new bool[ReelCount];

        int stoppedCount = 0;

        float startTime =
            Time.unscaledTime;

        float nextChangeTime =
            Time.unscaledTime;

        while (stoppedCount < ReelCount)
        {
            float elapsedTime =
                Time.unscaledTime -
                startTime;

            // 돌아가는 동안 보여주는 이미지는
            // 단순 연출이므로 실제 추첨 결과와 무관하다.
            if (Time.unscaledTime >=
                nextChangeTime)
            {
                nextChangeTime =
                    Time.unscaledTime +
                    symbolChangeInterval;

                for (int i = 0; i < ReelCount; i++)
                {
                    if (stopped[i])
                        continue;

                    SymbolData preview =
                        playerSymbolDeck
                            .GetRandomPreviewSymbol();

                    SetReelImage(
                        i,
                        preview
                    );
                }
            }

            // 왼쪽 릴부터 순차 정지
            for (int i = 0; i < ReelCount; i++)
            {
                if (stopped[i])
                    continue;

                float stopTime =
                    firstStopDelay +
                    reelStopInterval * i;

                if (elapsedTime < stopTime)
                    continue;

                stopped[i] = true;

                stoppedCount++;

                SetReelImage(
                    i,
                    resultSymbols[i]
                );
            }

            yield return null;
        }

        isSpinning = false;

        if (spinButton != null)
            spinButton.interactable = true;

        OnSpinCompleted?.Invoke(
            resultSymbols
        );

        DebugSpinResult();
    }

    private void SetReelImage(
        int reelIndex,
        SymbolData symbol
    )
    {
        if (reelIndex < 0 ||
            reelIndex >= reelImages.Length)
        {
            return;
        }

        Image reelImage =
            reelImages[reelIndex];

        if (reelImage == null)
            return;

        if (symbol == null)
        {
            reelImage.sprite = null;
            reelImage.enabled = false;
            return;
        }

        reelImage.enabled = true;
        reelImage.sprite = symbol.Icon;
        reelImage.preserveAspect = true;
    }

    private void DebugSpinResult()
    {
        string message =
            "슬롯 비복원추출 결과: ";

        for (int i = 0;
             i < resultSymbols.Count;
             i++)
        {
            SymbolData symbol =
                resultSymbols[i];

            if (symbol == null)
            {
                message += "[없음]";
            }
            else
            {
                message +=
                    $"[{symbol.DisplayName}]";
            }

            if (i <
                resultSymbols.Count - 1)
            {
                message += " ";
            }
        }

        Debug.Log(
            message,
            this
        );
    }
}