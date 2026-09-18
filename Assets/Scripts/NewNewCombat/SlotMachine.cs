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

    [Header("6개 릴 이미지")]
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


    private readonly List<SymbolData> resultSymbols =
        new List<SymbolData>();

    private readonly bool[] lockedReels =
        new bool[ReelCount];

    private bool isSpinning;


    public bool IsSpinning =>
        isSpinning;

    public IReadOnlyList<SymbolData> ResultSymbols =>
        resultSymbols;


    public event Action<IReadOnlyList<SymbolData>>
        OnSpinCompleted;


    // =========================================================
    // 상태
    // =========================================================

    public bool CanSpinAll
    {
        get
        {
            return
                !isSpinning &&
                ValidateBasicSpinSettings();
        }
    }


    public bool CanSpinUnlockedReels
    {
        get
        {
            if (isSpinning)
                return false;

            if (!ValidateBasicSpinSettings())
                return false;

            if (resultSymbols.Count != ReelCount)
                return false;

            if (AreAllReelsLocked)
                return false;

            return true;
        }
    }


    public int LockedReelCount
    {
        get
        {
            int count = 0;

            for (int i = 0; i < ReelCount; i++)
            {
                if (lockedReels[i])
                    count++;
            }

            return count;
        }
    }


    public bool AreAllReelsLocked
    {
        get
        {
            for (int i = 0; i < ReelCount; i++)
            {
                if (!lockedReels[i])
                    return false;
            }

            return true;
        }
    }


    // =========================================================
    // 첫 번째 스핀
    // =========================================================

    public bool TrySpinAll()
    {
        if (!CanSpinAll)
        {
            Debug.LogWarning(
                "현재 전체 슬롯을 돌릴 수 없습니다.",
                this
            );

            return false;
        }

        // 새 라운드의 첫 스핀이므로
        // 이전 HOLD 상태는 전부 제거
        ClearAllLocks();

        StartCoroutine(
            SpinRoutine(false)
        );

        return true;
    }


    // =========================================================
    // 두 번째 스핀
    // =========================================================

    public bool TrySpinUnlockedReels()
    {
        if (!CanSpinUnlockedReels)
        {
            Debug.LogWarning(
                "현재 HOLD되지 않은 릴을 다시 돌릴 수 없습니다.",
                this
            );

            return false;
        }

        StartCoroutine(
            SpinRoutine(true)
        );

        return true;
    }


    // =========================================================
    // 스핀
    // =========================================================

    private IEnumerator SpinRoutine(
        bool rerollUnlockedOnly
    )
    {
        isSpinning = true;


        List<int> spinningIndices =
            new List<int>();

        List<SymbolData> finalResults =
            new List<SymbolData>(
                ReelCount
            );


        // 결과 리스트 크기를 6으로 고정
        for (int i = 0; i < ReelCount; i++)
        {
            if (rerollUnlockedOnly &&
                resultSymbols.Count == ReelCount)
            {
                finalResults.Add(
                    resultSymbols[i]
                );
            }
            else
            {
                finalResults.Add(
                    null
                );
            }
        }


        // =====================================================
        // 실제로 돌릴 릴 결정
        // =====================================================

        for (int i = 0; i < ReelCount; i++)
        {
            if (rerollUnlockedOnly &&
                lockedReels[i])
            {
                continue;
            }

            spinningIndices.Add(i);
        }


        // =====================================================
        // 비복원추출 풀 생성
        // =====================================================

        List<SymbolData> drawPool =
            playerSymbolDeck.CreateDrawPool();


        if (rerollUnlockedOnly)
        {
            /*
             * 고정된 릴은 이미 이번 최종 결과에 포함되어 있으므로
             * 뽑기 풀에서도 해당 문양을 1개씩 제거한다.
             *
             * 예:
             * 수은 x1이 HOLD되어 있다면
             * 다른 릴에서 수은 x1이 또 뽑히면 안 된다.
             */

            for (int i = 0; i < ReelCount; i++)
            {
                if (!lockedReels[i])
                    continue;

                SymbolData lockedSymbol =
                    finalResults[i];

                if (lockedSymbol == null)
                    continue;

                bool removed =
                    drawPool.Remove(
                        lockedSymbol
                    );

                if (!removed)
                {
                    Debug.LogError(
                        $"HOLD된 문양 {lockedSymbol.DisplayName}을 " +
                        $"DrawPool에서 제거하지 못했습니다.",
                        this
                    );

                    FinishFailedSpin();

                    yield break;
                }
            }
        }


        if (drawPool.Count <
            spinningIndices.Count)
        {
            Debug.LogError(
                $"재추첨할 문양 수가 부족합니다. " +
                $"필요: {spinningIndices.Count}, " +
                $"현재 풀: {drawPool.Count}",
                this
            );

            FinishFailedSpin();

            yield break;
        }


        // =====================================================
        // 최종 결과 미리 결정
        // =====================================================

        foreach (int reelIndex in spinningIndices)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    drawPool.Count
                );

            SymbolData selectedSymbol =
                drawPool[randomIndex];

            finalResults[reelIndex] =
                selectedSymbol;

            drawPool.RemoveAt(
                randomIndex
            );
        }


        // =====================================================
        // 슬롯 회전 연출
        // =====================================================

        bool[] stopped =
            new bool[ReelCount];

        int stoppedCount =
            0;

        float elapsed =
            0f;


        while (stoppedCount <
               spinningIndices.Count)
        {
            for (int order = 0;
                 order < spinningIndices.Count;
                 order++)
            {
                int reelIndex =
                    spinningIndices[order];

                if (stopped[reelIndex])
                    continue;


                float stopTime =
                    firstStopDelay +
                    reelStopInterval *
                    order;


                if (elapsed >= stopTime)
                {
                    SetReelSymbol(
                        reelIndex,
                        finalResults[reelIndex]
                    );

                    stopped[reelIndex] =
                        true;

                    stoppedCount++;

                    continue;
                }


                // 회전 중 미리보기
                SymbolData previewSymbol =
                    playerSymbolDeck
                        .GetRandomPreviewSymbol();

                if (previewSymbol != null)
                {
                    SetReelSymbol(
                        reelIndex,
                        previewSymbol
                    );
                }
            }


            yield return new WaitForSeconds(
                symbolChangeInterval
            );


            elapsed +=
                symbolChangeInterval;
        }


        // =====================================================
        // 최종 결과 저장
        // =====================================================

        resultSymbols.Clear();

        resultSymbols.AddRange(
            finalResults
        );


        isSpinning =
            false;


        Debug.Log(
            rerollUnlockedOnly
                ? "두 번째 슬롯 완료"
                : "첫 번째 슬롯 완료",
            this
        );


        OnSpinCompleted?.Invoke(
            resultSymbols
        );
    }


    // =========================================================
    // HOLD
    // =========================================================

    public bool ToggleReelLock(
        int reelIndex
    )
    {
        if (!IsValidReelIndex(
                reelIndex))
        {
            return false;
        }

        lockedReels[reelIndex] =
            !lockedReels[reelIndex];

        return
            lockedReels[reelIndex];
    }


    public bool IsReelLocked(
        int reelIndex
    )
    {
        if (!IsValidReelIndex(
                reelIndex))
        {
            return false;
        }

        return
            lockedReels[reelIndex];
    }


    public void SetReelLock(
        int reelIndex,
        bool locked
    )
    {
        if (!IsValidReelIndex(
                reelIndex))
        {
            return;
        }

        lockedReels[reelIndex] =
            locked;
    }


    public void ClearAllLocks()
    {
        for (int i = 0;
             i < ReelCount;
             i++)
        {
            lockedReels[i] =
                false;
        }
    }


    // =========================================================
    // UI
    // =========================================================

    private void SetReelSymbol(
        int reelIndex,
        SymbolData symbol
    )
    {
        if (!IsValidReelIndex(
                reelIndex))
        {
            return;
        }

        Image reelImage =
            reelImages[reelIndex];

        if (reelImage == null)
            return;


        if (symbol == null)
        {
            reelImage.sprite =
                null;

            reelImage.enabled =
                false;

            return;
        }


        reelImage.sprite =
            symbol.Icon;

        reelImage.enabled =
            symbol.Icon != null;

        reelImage.preserveAspect =
            true;
    }


    // =========================================================
    // 검사
    // =========================================================

    private bool ValidateBasicSpinSettings()
    {
        if (playerSymbolDeck == null)
        {
            Debug.LogError(
                "PlayerSymbolDeck이 연결되지 않았습니다.",
                this
            );

            return false;
        }


        if (playerSymbolDeck.TotalSymbolCount <
            ReelCount)
        {
            Debug.LogError(
                $"문양 덱에 최소 {ReelCount}개의 문양이 필요합니다.",
                this
            );

            return false;
        }


        if (reelImages == null ||
            reelImages.Length != ReelCount)
        {
            Debug.LogError(
                "Reel Images는 정확히 6개여야 합니다.",
                this
            );

            return false;
        }


        for (int i = 0;
             i < ReelCount;
             i++)
        {
            if (reelImages[i] == null)
            {
                Debug.LogError(
                    $"Reel Images[{i}]가 비어 있습니다.",
                    this
                );

                return false;
            }
        }


        return true;
    }


    private bool IsValidReelIndex(
        int index
    )
    {
        return
            index >= 0 &&
            index < ReelCount;
    }


    private void FinishFailedSpin()
    {
        isSpinning =
            false;
    }
}