using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 스테이지 범위. 이벤트와 아이템이 어느 스테이지에서 나오는지, 희귀도 분포가 어느 스테이지에 쓰이는지 적는다.
    /// **0 이하는 따지지 않는다.** 처음 값 0 은 첫 스테이지부터, 끝 값 0 은 마지막 스테이지까지다.
    /// 칸이 없던 예전 자료가 0 으로 읽혀도 모든 스테이지에 나오게 하려는 것이다.
    /// 2026년 10월 9일 외부 검토가 스테이지별 등장 조건을 권했고 원재가 틀만 만들기로 정했다. 수치는 기획서가 오면 넣는다.
    /// </summary>
    public static class StageRange
    {
        /// <summary>그 스테이지가 범위 안인지.</summary>
        public static bool Covers(int minStage, int maxStage, int stage)
        {
            return (minStage <= 0 || stage >= minStage) && (maxStage <= 0 || stage <= maxStage);
        }
    }

    /// <summary>
    /// 스테이지 범위 하나의 희귀도 분포. 보상과 행상이 코인과 유물을 뽑을 때 등급부터 이 비중대로 굴린다.
    /// 문양은 등급이 없어 쓰지 않는다.
    /// 목록이 비었거나 그 스테이지를 덮는 줄이 없으면 등급을 따지지 않고 고르게 뽑는다. 지금 기본값이 그렇다.
    /// </summary>
    [Serializable]
    public class StageRarityWeights
    {
        [Tooltip("이 분포를 쓰기 시작하는 스테이지. 0 이하면 첫 스테이지부터.")]
        public int MinStage;

        [Tooltip("이 분포를 마지막으로 쓰는 스테이지. 0 이하면 끝까지.")]
        public int MaxStage;

        [Tooltip("일반, 고급, 희귀, 특급, 전설 차례의 비중. 모자란 칸과 0 이하는 그 등급이 나오지 않는다.")]
        public List<int> Weights = new List<int>();

        /// <summary>그 스테이지에 쓰는 분포인지.</summary>
        public bool Covers(int stage)
        {
            return StageRange.Covers(MinStage, MaxStage, stage);
        }

        /// <summary>그 등급의 비중. 칸이 없으면 0 이다.</summary>
        public int WeightOf(ItemRarity rarity)
        {
            int index = (int)rarity;
            return Weights != null && index >= 0 && index < Weights.Count && Weights[index] > 0 ? Weights[index] : 0;
        }

        /// <summary>
        /// 그 스테이지에 쓸 분포. 위에서부터 처음 덮는 줄이다. 없으면 null 이고, 그때는 고르게 뽑는다.
        /// 비중이 하나도 없는 줄은 건너뛴다.
        /// </summary>
        public static StageRarityWeights Find(List<StageRarityWeights> table, int stage)
        {
            if (table == null)
            {
                return null;
            }

            for (int i = 0; i < table.Count; i++)
            {
                StageRarityWeights row = table[i];
                if (row == null || !row.Covers(stage))
                {
                    continue;
                }

                for (int r = 0; r <= (int)ItemRarity.Legend; r++)
                {
                    if (row.WeightOf((ItemRarity)r) > 0)
                    {
                        return row;
                    }
                }
            }

            return null;
        }
    }
}
