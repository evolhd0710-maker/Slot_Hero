using System;
using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 희귀도 분포대로 하나를 고른다. 등급부터 비중대로 굴린다.
    /// 후보에 없는 등급은 비중에서 빠진다. 후보의 어느 등급에도 비중이 없으면 등급을 따지지 않는다.
    /// 난수는 부르는 쪽 것을 넘겨받는다(range 는 0 이상 그 값 미만의 정수). 같은 시드면 같은 결과다.
    ///
    /// 등급 안에서 무엇을 꺼낼지는 쓰는 쪽에 따라 둘이다.
    ///   - `PickIndex` 그 등급 안에서 고르게 뽑는다. 보상 카드와 행상 코인이 쓴다
    ///   - `PickFirstIndex` 그 등급에서 후보 차례가 가장 앞선 것을 꺼낸다. 행상 유물이 순서 테이블을 지키려고 쓴다
    /// </summary>
    public static class RarityPicker
    {
        /// <summary>등급을 굴린 뒤 그 등급 안에서 고르게 뽑은 후보의 자리. 후보가 없으면 -1 이다.</summary>
        public static int PickIndex<T>(IList<T> candidates, Func<T, ItemRarity> rarityOf, StageRarityWeights weights, Func<int, int> range)
        {
            if (candidates == null || candidates.Count == 0 || rarityOf == null || range == null)
            {
                return -1;
            }

            if (weights == null)
            {
                return range(candidates.Count);
            }

            int[] counts = CountRarities(candidates, rarityOf);
            int picked = PickRarity(counts, weights, range);
            if (picked < 0)
            {
                return range(candidates.Count);
            }

            int nth = range(counts[picked]);
            for (int i = 0; i < candidates.Count; i++)
            {
                if ((int)rarityOf(candidates[i]) != picked)
                {
                    continue;
                }

                if (nth == 0)
                {
                    return i;
                }

                nth--;
            }

            return -1;
        }

        /// <summary>
        /// 등급만 굴리고 그 등급에서 후보 차례가 가장 앞선 것의 자리. 후보가 없으면 -1 이다.
        /// 분포가 없거나 후보의 어느 등급에도 비중이 없으면 맨 앞 후보다.
        /// 행상 유물이 쓴다. 같은 등급 안에서는 순서 테이블의 차례를 그대로 지킨다.
        /// 예전에는 등급 안에서 다시 무작위로 골라, 모두 일반 등급이어도 테이블 첫 유물과 다른 것이 나왔다. 2026년 10월 9일 외부 검토.
        /// </summary>
        public static int PickFirstIndex<T>(IList<T> candidates, Func<T, ItemRarity> rarityOf, StageRarityWeights weights, Func<int, int> range)
        {
            if (candidates == null || candidates.Count == 0 || rarityOf == null || range == null)
            {
                return -1;
            }

            if (weights == null)
            {
                return 0;
            }

            int picked = PickRarity(CountRarities(candidates, rarityOf), weights, range);
            if (picked < 0)
            {
                return 0;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                if ((int)rarityOf(candidates[i]) == picked)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>등급마다 후보가 몇 개인지.</summary>
        private static int[] CountRarities<T>(IList<T> candidates, Func<T, ItemRarity> rarityOf)
        {
            int[] counts = new int[(int)ItemRarity.Legend + 1];
            for (int i = 0; i < candidates.Count; i++)
            {
                int r = (int)rarityOf(candidates[i]);
                if (r >= 0 && r < counts.Length)
                {
                    counts[r]++;
                }
            }

            return counts;
        }

        /// <summary>후보가 있는 등급 가운데 비중대로 굴린 등급. 후보가 있는 어느 등급에도 비중이 없으면 -1 이고 난수를 쓰지 않는다.</summary>
        private static int PickRarity(int[] counts, StageRarityWeights weights, Func<int, int> range)
        {
            int total = 0;
            for (int r = 0; r < counts.Length; r++)
            {
                if (counts[r] > 0)
                {
                    total += weights.WeightOf((ItemRarity)r);
                }
            }

            if (total <= 0)
            {
                return -1;
            }

            int roll = range(total);
            for (int r = 0; r < counts.Length; r++)
            {
                if (counts[r] == 0)
                {
                    continue;
                }

                roll -= weights.WeightOf((ItemRarity)r);
                if (roll < 0)
                {
                    return r;
                }
            }

            return -1;
        }
    }
}
