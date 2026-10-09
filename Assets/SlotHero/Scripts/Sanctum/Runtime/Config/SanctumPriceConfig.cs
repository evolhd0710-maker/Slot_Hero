using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상의 모든 가격 규칙.
    /// 기본값은 성소 기획서 v0.2 / 07 가격 의 네 표를 그대로 옮긴 것이다.
    ///
    /// 13 가격 · 공통 규칙 의 "가격은 별도의 데이터로 관리하여 수정하기 편하게 한다"에 따라
    /// 수치를 코드에 박지 않고 이 설정 에셋에 모아 둔다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SanctumPriceConfig",
        menuName = "Slot Hero/성소/가격 설정",
        order = 1)]
    public class SanctumPriceConfig : ScriptableObject
    {
        [Header("문양 기준가")]
        [Tooltip("문양은 등급이 없어 태그 가치 합으로 값을 매긴다. 가격은 기본가 + 태그 가치 합 × 계수다. 기본가 20.")]
        [Min(0)]
        public int SymbolBasePrice = 20;

        [Tooltip("태그 가치 합에 곱하는 계수. 기본 5.")]
        [Min(0)]
        public int SymbolTagValueMultiplier = 5;

        [Header("코인 기준가")]
        public List<RarityPrice> CoinPrices = new List<RarityPrice>
        {
            new RarityPrice(ItemRarity.Common, 15),
            new RarityPrice(ItemRarity.Uncommon, 25),
            new RarityPrice(ItemRarity.Rare, 35),
            new RarityPrice(ItemRarity.Epic, 45),
            new RarityPrice(ItemRarity.Legend, 60),
        };

        [Header("유물 기준가")]
        public List<RarityPrice> RelicPrices = new List<RarityPrice>
        {
            new RarityPrice(ItemRarity.Common, 30),
            new RarityPrice(ItemRarity.Uncommon, 45),
            new RarityPrice(ItemRarity.Rare, 60),
            new RarityPrice(ItemRarity.Epic, 75),
            new RarityPrice(ItemRarity.Legend, 100),
        };

        [Header("문양 변경")]
        [Tooltip("처음 쓸 때의 가격. 기본 10.")]
        [Min(0)]
        public int SymbolChangeStartPrice = 10;

        [Tooltip("가격이 오르는 단위. 기본 10.")]
        [Min(0)]
        public int SymbolChangeStep = 10;

        [Tooltip("오르는 방식. 기획서 예시 10 > 30 > 60 > 100 > 150 은 증가 폭이 커지는 쪽이다.")]
        public PriceGrowth SymbolChangeGrowth = PriceGrowth.Triangular;

        [Header("유물 새로고침")]
        [Tooltip("처음 쓸 때의 가격. 기본 20.")]
        [Min(0)]
        public int RelicRefreshStartPrice = 20;

        [Tooltip("가격이 오르는 단위. 기본 10.")]
        [Min(0)]
        public int RelicRefreshStep = 10;

        [Tooltip("오르는 방식. 기획서 예시 20 > 30 > 40 > 50 > 60 은 증가 폭이 늘 같은 쪽이다.")]
        public PriceGrowth RelicRefreshGrowth = PriceGrowth.Linear;

        /// <summary>상품 하나의 가격. 종류에 맞는 기준가 규칙을 골라 쓴다.</summary>
        public int GetItemPrice(MerchantItem item)
        {
            switch (item.Kind)
            {
                case SanctumItemKind.Symbol:
                    return GetSymbolPrice(item.TagValueSum);
                case SanctumItemKind.Coin:
                    return GetRarityPrice(CoinPrices, item.Rarity);
                case SanctumItemKind.Relic:
                    return GetRarityPrice(RelicPrices, item.Rarity);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 문양 하나의 가격.
        /// 기획서 예시로 검산하면 수은 2 → 30, 연꽃 4 → 40, 까마귀 7 → 55, 밤하늘 9 → 65 가 나온다.
        /// </summary>
        public int GetSymbolPrice(int tagValueSum)
        {
            if (tagValueSum < 0)
            {
                tagValueSum = 0;
            }

            return SymbolBasePrice + tagValueSum * SymbolTagValueMultiplier;
        }

        /// <summary>이번 성소에서 문양 변경을 usedCount 번 쓴 뒤의 다음 가격.</summary>
        public int GetSymbolChangePrice(int usedCount)
        {
            return GetServicePrice(usedCount, SymbolChangeStartPrice, SymbolChangeStep, SymbolChangeGrowth);
        }

        /// <summary>이번 성소에서 유물 새로고침을 usedCount 번 쓴 뒤의 다음 가격.</summary>
        public int GetRelicRefreshPrice(int usedCount)
        {
            return GetServicePrice(usedCount, RelicRefreshStartPrice, RelicRefreshStep, RelicRefreshGrowth);
        }

        /// <summary>등급표에서 가격을 찾는다. 표에 없는 등급은 0으로 본다.</summary>
        public static int GetRarityPrice(List<RarityPrice> table, ItemRarity rarity)
        {
            if (table == null)
            {
                return 0;
            }

            for (int i = 0; i < table.Count; i++)
            {
                if (table[i].Rarity == rarity)
                {
                    return table[i].Price;
                }
            }

            return 0;
        }

        /// <summary>
        /// 쓸 때마다 오르는 기능의 가격.
        /// usedCount 는 이미 쓴 횟수라 이번이 usedCount + 1 번째 사용이 된다.
        /// 증가 폭이 커지는 쪽은 2번째부터 단위 × 2, 3번째부터 단위 × 3 씩 더 붙는다.
        /// </summary>
        private static int GetServicePrice(int usedCount, int startPrice, int step, PriceGrowth growth)
        {
            if (usedCount < 0)
            {
                usedCount = 0;
            }

            if (growth == PriceGrowth.Linear)
            {
                return startPrice + step * usedCount;
            }

            int useIndex = usedCount + 1;
            int triangular = useIndex * (useIndex + 1) / 2 - 1;
            return startPrice + step * triangular;
        }
    }
}
