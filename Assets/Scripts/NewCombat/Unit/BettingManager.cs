using System;
using System.Collections.Generic;
using UnityEngine;

public class BettingManager : MonoBehaviour
{
    [Header("코인 인벤토리")]
    [SerializeField]
    private PlayerCoinInventory coinInventory;


    [Header("현재 태그 예측 베팅")]
    [SerializeField]
    private List<TagBetRecord> tagBets =
        new List<TagBetRecord>();


    [Header("현재 중복 예측 베팅")]
    [SerializeField]
    private List<DuplicateBetRecord> duplicateBets =
        new List<DuplicateBetRecord>();


    public IReadOnlyList<TagBetRecord> TagBets =>
        tagBets;

    public IReadOnlyList<DuplicateBetRecord> DuplicateBets =>
        duplicateBets;


    public event Action OnBetsChanged;
    public event Action OnBetsConsumed;
    public event Action OnBetsCancelled;


    // =========================================================
    // 태그 베팅
    // =========================================================

    public bool PlaceTagBet(
        SymbolTagType tagType,
        BetCoinData coin
    )
    {
        if (coin == null)
        {
            Debug.LogError(
                "PlaceTagBet: CoinData가 null입니다.",
                this
            );

            return false;
        }


        if (coinInventory == null)
        {
            Debug.LogError(
                "PlayerCoinInventory가 연결되지 않았습니다.",
                this
            );

            return false;
        }


        if (!coinInventory.TryConsumeCoin(
                coin,
                1))
        {
            Debug.LogWarning(
                $"{coin.DisplayName} 코인이 없습니다.",
                this
            );

            return false;
        }


        TagBetRecord bet =
            new TagBetRecord(
                tagType,
                coin
            );


        tagBets.Add(
            bet
        );


        Debug.Log(
            $"[태그 베팅 등록] " +
            $"태그={tagType} | " +
            $"코인={coin.DisplayName} | " +
            $"가중치=+{coin.Weight}",
            this
        );


        OnBetsChanged?.Invoke();


        return true;
    }


    // =========================================================
    // 중복 베팅
    // =========================================================

    public bool PlaceDuplicateBet(
        int requiredCount,
        BetCoinData coin
    )
    {
        if (coin == null)
        {
            Debug.LogError(
                "PlaceDuplicateBet: CoinData가 null입니다.",
                this
            );

            return false;
        }


        if (requiredCount < 1 ||
            requiredCount > 6)
        {
            Debug.LogError(
                $"잘못된 중복 베팅 값: {requiredCount}",
                this
            );

            return false;
        }


        if (coinInventory == null)
        {
            Debug.LogError(
                "PlayerCoinInventory가 연결되지 않았습니다.",
                this
            );

            return false;
        }


        if (!coinInventory.TryConsumeCoin(
                coin,
                1))
        {
            Debug.LogWarning(
                $"{coin.DisplayName} 코인이 없습니다.",
                this
            );

            return false;
        }


        DuplicateBetRecord bet =
            new DuplicateBetRecord(
                requiredCount,
                coin
            );


        duplicateBets.Add(
            bet
        );


        Debug.Log(
            $"[중복 베팅 등록] " +
            $"정확히 {requiredCount}중복 | " +
            $"코인={coin.DisplayName} | " +
            $"가중치=+{coin.Weight}",
            this
        );


        Debug.Log(
            $"현재 중복 베팅 개수 = {duplicateBets.Count}",
            this
        );


        OnBetsChanged?.Invoke();


        return true;
    }


    // =========================================================
    // 태그 베팅 가중치
    // =========================================================

    public int GetTagBetWeight(
        SymbolTagType tagType
    )
    {
        int totalWeight = 0;


        for (int i = 0;
             i < tagBets.Count;
             i++)
        {
            TagBetRecord bet =
                tagBets[i];


            if (bet == null)
                continue;


            if (bet.coin == null)
                continue;


            if (bet.tagType != tagType)
                continue;


            totalWeight +=
                bet.coin.Weight;
        }


        Debug.Log(
            $"[태그 가중치 조회] " +
            $"{tagType} → +{totalWeight}",
            this
        );


        return totalWeight;
    }


    // =========================================================
    // 중복 베팅 가중치
    //
    // 중요:
    //
    // 1중복 베팅 → 실제 1중복만 적용
    // 2중복 베팅 → 실제 2중복만 적용
    // ...
    //
    // >= 사용하지 않음.
    // =========================================================

    public int GetDuplicateBetWeight(
        int actualDuplicateCount
    )
    {
        int totalWeight = 0;


        Debug.Log(
            $"[중복 가중치 조회 시작] " +
            $"실제 중복={actualDuplicateCount} | " +
            $"등록된 중복 베팅={duplicateBets.Count}개",
            this
        );


        for (int i = 0;
             i < duplicateBets.Count;
             i++)
        {
            DuplicateBetRecord bet =
                duplicateBets[i];


            if (bet == null)
            {
                Debug.Log(
                    $"Bet[{i}] = null",
                    this
                );

                continue;
            }


            if (bet.Coin == null)
            {
                Debug.Log(
                    $"Bet[{i}] Coin = null",
                    this
                );

                continue;
            }


            Debug.Log(
                $"Bet[{i}] 검사 | " +
                $"예측={bet.RequiredCount}중복 | " +
                $"실제={actualDuplicateCount}중복 | " +
                $"Coin={bet.Coin.DisplayName} | " +
                $"Weight=+{bet.Coin.Weight}",
                this
            );


            // =================================================
            // 정확히 같은 중복 수일 때만 적용
            // =================================================

            if (bet.RequiredCount !=
                actualDuplicateCount)
            {
                Debug.Log(
                    $"→ 불일치, 적용 안 함",
                    this
                );

                continue;
            }


            totalWeight +=
                bet.Coin.Weight;


            Debug.Log(
                $"→ 일치, +{bet.Coin.Weight} 적용",
                this
            );
        }


        Debug.Log(
            $"[중복 가중치 조회 결과] " +
            $"실제 {actualDuplicateCount}중복 → +{totalWeight}",
            this
        );


        return totalWeight;
    }


    // =========================================================
    // 베팅 존재 여부
    // =========================================================

    public bool HasAnyBet()
    {
        return
            tagBets.Count > 0 ||
            duplicateBets.Count > 0;
    }


    public int GetTotalBetCount()
    {
        return
            tagBets.Count +
            duplicateBets.Count;
    }


    // =========================================================
    // 베팅 결과 처리 완료
    // =========================================================

    public void ConsumeCurrentBets()
    {
        Debug.Log(
            $"[베팅 소모] " +
            $"태그={tagBets.Count}개 | " +
            $"중복={duplicateBets.Count}개",
            this
        );


        tagBets.Clear();
        duplicateBets.Clear();


        OnBetsConsumed?.Invoke();
        OnBetsChanged?.Invoke();
    }


    // =========================================================
    // 베팅 취소
    // =========================================================

    public void CancelAllBets()
    {
        if (coinInventory != null)
        {
            // =================================================
            // 태그 코인 반환
            // =================================================

            foreach (TagBetRecord bet in tagBets)
            {
                if (bet == null ||
                    bet.coin == null)
                {
                    continue;
                }


                coinInventory.AddCoin(
                    bet.coin,
                    1
                );
            }


            // =================================================
            // 중복 코인 반환
            // =================================================

            foreach (
                DuplicateBetRecord bet
                in duplicateBets)
            {
                if (bet == null ||
                    bet.Coin == null)
                {
                    continue;
                }


                coinInventory.AddCoin(
                    bet.Coin,
                    1
                );
            }
        }


        tagBets.Clear();
        duplicateBets.Clear();


        Debug.Log(
            "[모든 베팅 취소]",
            this
        );


        OnBetsCancelled?.Invoke();
        OnBetsChanged?.Invoke();
    }
}