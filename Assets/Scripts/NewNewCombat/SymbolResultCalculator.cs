using System.Collections.Generic;
using UnityEngine;

public class SymbolResultCalculator : MonoBehaviour
{
    [Header("중복 기본 배수")]
    [SerializeField]
    private List<DuplicateMultiplierRule> duplicateRules =
        new List<DuplicateMultiplierRule>()
        {
            new DuplicateMultiplierRule(1, 1),
            new DuplicateMultiplierRule(2, 2),
            new DuplicateMultiplierRule(3, 4),
            new DuplicateMultiplierRule(4, 6),
            new DuplicateMultiplierRule(5, 10)
        };


    private class TagRuntimeData
    {
        public SymbolTagData tag;

        public int count;

        public int valueSum;

        public List<SymbolContribution> contributions =
            new List<SymbolContribution>();
    }


    public SymbolCalculationResult Calculate(
        IReadOnlyList<SymbolData> symbols,
        BettingManager bettingManager
    )
    {
        SymbolCalculationResult result =
            new SymbolCalculationResult();


        if (symbols == null)
            return result;


        if (bettingManager == null)
        {
            Debug.LogError(
                "SymbolResultCalculator.Calculate에 " +
                "BettingManager가 전달되지 않았습니다.",
                this
            );
        }
        else
        {
            Debug.Log(
                $"[계산 시작] 현재 베팅 개수 = " +
                $"{bettingManager.GetTotalBetCount()}",
                this
            );
        }


        Dictionary<SymbolTagType, TagRuntimeData>
            tagDataMap =
                new Dictionary<
                    SymbolTagType,
                    TagRuntimeData
                >();


        // =====================================================
        // 1. 슬롯 결과 태그별 분류
        // =====================================================

        for (int reelIndex = 0;
             reelIndex < symbols.Count;
             reelIndex++)
        {
            SymbolData symbol =
                symbols[reelIndex];


            if (symbol == null)
                continue;


            AddSymbolToTag(
                tagDataMap,
                symbol.FirstTag,
                symbol,
                reelIndex
            );


            if (symbol.SecondTag != null &&
                symbol.SecondTag != symbol.FirstTag)
            {
                AddSymbolToTag(
                    tagDataMap,
                    symbol.SecondTag,
                    symbol,
                    reelIndex
                );
            }
        }


        // =====================================================
        // 2. 태그별 결과 생성
        // =====================================================

        foreach (
            KeyValuePair<
                SymbolTagType,
                TagRuntimeData
            > pair in tagDataMap)
        {
            TagRuntimeData data =
                pair.Value;


            if (data == null ||
                data.tag == null)
            {
                continue;
            }


            int tagBetWeight =
                0;

            int duplicateBetWeight =
                0;


            if (bettingManager != null)
            {
                tagBetWeight =
                    bettingManager
                        .GetTagBetWeight(
                            data.tag.TagType
                        );


                duplicateBetWeight =
                    bettingManager
                        .GetDuplicateBetWeight(
                            data.count
                        );
            }


            int baseMultiplier =
                GetBaseMultiplier(
                    data.count
                );


            int finalMultiplier =
                baseMultiplier +
                duplicateBetWeight;


            int leftValue =
                data.valueSum +
                tagBetWeight;


            int finalScore =
                leftValue *
                finalMultiplier;


            // =================================================
            // 디버그
            // =================================================

            Debug.Log(
                $"[태그 계산] " +
                $"{data.tag.DisplayName} / " +
                $"중복 {data.count} / " +
                $"문양 합 {data.valueSum} / " +
                $"태그 예측 +{tagBetWeight} / " +
                $"기본 배수 {baseMultiplier} / " +
                $"중복 예측 +{duplicateBetWeight} / " +
                $"최종 = ({data.valueSum} + {tagBetWeight}) " +
                $"X ({baseMultiplier} + {duplicateBetWeight}) " +
                $"= {finalScore}",
                this
            );


            TagCalculationResult tagResult =
                new TagCalculationResult();


            tagResult.tagType =
                data.tag.TagType;


            tagResult.tagName =
                data.tag.DisplayName;


            tagResult.duplicateCount =
                data.count;


            tagResult.symbolValueSum =
                data.valueSum;


            tagResult.tagBetWeight =
                tagBetWeight;


            tagResult.baseMultiplier =
                baseMultiplier;


            tagResult.duplicateBetWeight =
                duplicateBetWeight;


            tagResult.finalMultiplier =
                finalMultiplier;


            tagResult.finalScore =
                finalScore;


            foreach (
                SymbolContribution contribution
                in data.contributions)
            {
                tagResult.contributions.Add(
                    contribution
                );
            }


            // 릴 왼쪽부터
            tagResult.contributions.Sort(
                CompareContributions
            );


            result.tagResults.Add(
                tagResult
            );


            result.totalDamage +=
                finalScore;
        }


        // =====================================================
        // 3. 계산 순서
        //
        // 중첩 적은 순
        // → 행성 enum 순서
        // =====================================================

        result.tagResults.Sort(
            CompareTagResults
        );


        Debug.Log(
            $"[전체 계산 완료] 최종 피해 = " +
            $"{result.totalDamage}",
            this
        );


        return result;
    }


    private void AddSymbolToTag(
        Dictionary<
            SymbolTagType,
            TagRuntimeData
        > map,
        SymbolTagData tag,
        SymbolData symbol,
        int reelIndex
    )
    {
        if (tag == null ||
            symbol == null)
        {
            return;
        }


        if (!map.TryGetValue(
                tag.TagType,
                out TagRuntimeData data))
        {
            data =
                new TagRuntimeData();


            data.tag =
                tag;


            map.Add(
                tag.TagType,
                data
            );
        }


        data.count++;


        data.valueSum +=
            symbol.Value;


        data.contributions.Add(
            new SymbolContribution(
                reelIndex,
                symbol,
                symbol.Value
            )
        );
    }


    private int GetBaseMultiplier(
        int duplicateCount
    )
    {
        if (duplicateCount <= 0)
            return 0;


        int fallbackCount =
            0;

        int fallbackMultiplier =
            0;


        foreach (
            DuplicateMultiplierRule rule
            in duplicateRules)
        {
            if (rule == null)
                continue;


            if (rule.count ==
                duplicateCount)
            {
                return rule.multiplier;
            }


            if (rule.count <
                    duplicateCount &&
                rule.count >
                    fallbackCount)
            {
                fallbackCount =
                    rule.count;

                fallbackMultiplier =
                    rule.multiplier;
            }
        }


        return fallbackMultiplier;
    }


    private int CompareContributions(
        SymbolContribution a,
        SymbolContribution b
    )
    {
        return
            a.reelIndex.CompareTo(
                b.reelIndex
            );
    }


    private int CompareTagResults(
        TagCalculationResult a,
        TagCalculationResult b
    )
    {
        int duplicateCompare =
            a.duplicateCount.CompareTo(
                b.duplicateCount
            );


        if (duplicateCompare != 0)
            return duplicateCompare;


        return
            a.tagType.CompareTo(
                b.tagType
            );
    }
}