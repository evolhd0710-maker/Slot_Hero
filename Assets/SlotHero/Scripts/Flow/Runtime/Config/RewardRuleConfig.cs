using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Map;
using SlotHero.Sanctum;

namespace SlotHero.Flow
{
    /// <summary>
    /// 방마다 어떤 보상이 나오는지.
    ///
    /// **보상 기획서가 아직 없다.** 인게임 화면 기획서 10장은 화면 배치만 정하고
    /// 무엇이 얼마나 나오는지는 다른 문서로 넘겼다.
    /// 그 문서가 오면 여기 값을 갈아 끼운다.
    ///
    /// 골드는 방 종류마다 범위를 두고 런 시드로 뽑는다.
    /// 같은 방이면 늘 같은 값이 나온다.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardRuleConfig", menuName = "Slot Hero/흐름/보상 규칙")]
    public class RewardRuleConfig : ScriptableObject
    {
        [Tooltip("방 종류마다의 보상 규칙. 적어 두지 않은 방은 보상이 없다.")]
        public List<RewardRule> Rules = new List<RewardRule>
        {
            new RewardRule(RoomType.Normal, 10, 15, 3, true, false, false),
            new RewardRule(RoomType.Elite, 25, 35, 3, false, true, true),
            new RewardRule(RoomType.Event, 0, 0, 0, false, false, false),
            new RewardRule(RoomType.Boss, 50, 70, 4, false, true, true),
        };

        [Tooltip("이벤트가 연 전투(35 도전자, 38 매복)를 이기면 어느 방의 카드 규칙으로 보상을 고르게 할지. " +
            "이벤트 기획서는 \"보상 선택(문양, 유물 등)\" 이라고만 적어 일반 방과 같게 두었다. 골드는 전투가 정한 것만 받는다.")]
        public RoomType EventCombatCardsLike = RoomType.Normal;

        [Tooltip("보상 카드의 코인과 유물을 뽑을 때 등급부터 굴리는 비중. 스테이지 범위마다 한 줄이다. " +
            "비어 있거나 그 스테이지를 덮는 줄이 없으면 등급을 따지지 않고 고르게 뽑는다. " +
            "문양이 섞인 카드 묶음에는 쓰지 않는다. 기획서에 수치가 없어 비워 둔다(2026년 10월 9일 원재).")]
        public List<StageRarityWeights> RarityByStage = new List<StageRarityWeights>();

        /// <summary>그 스테이지의 보상 희귀도 분포. 없으면 null 이고 그때는 고르게 뽑는다.</summary>
        public StageRarityWeights GetRarityWeights(int stage)
        {
            return StageRarityWeights.Find(RarityByStage, stage);
        }

        /// <summary>그 방의 규칙. 없으면 보상이 없는 규칙을 돌려준다.</summary>
        public RewardRule GetRule(RoomType roomType)
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].RoomType == roomType)
                {
                    return Rules[i];
                }
            }

            return new RewardRule(roomType, 0, 0, 0, false, false, false);
        }

        /// <summary>그 방에서 받는 기본 보상 골드. 시드에서 뽑는다.</summary>
        public int GetBaseGold(RoomType roomType, MapRandom random)
        {
            RewardRule rule = GetRule(roomType);

            if (rule.MaxGold <= rule.MinGold)
            {
                return rule.MinGold;
            }

            return random.Range(rule.MinGold, rule.MaxGold + 1);
        }

        /// <summary>그 방에서 고를 카드 장수.</summary>
        public int GetCardCount(RoomType roomType)
        {
            return GetRule(roomType).CardCount;
        }

        /// <summary>그 방에 문양이 나오는지.</summary>
        public bool GivesSymbols(RoomType roomType)
        {
            return GetRule(roomType).GivesSymbols;
        }

        /// <summary>그 방에 코인이 나오는지.</summary>
        public bool GivesCoins(RoomType roomType)
        {
            return GetRule(roomType).GivesCoins;
        }

        /// <summary>그 방에 유물이 나오는지.</summary>
        public bool GivesRelics(RoomType roomType)
        {
            return GetRule(roomType).GivesRelics;
        }

        /// <summary>값이 앞뒤가 맞는지. 최솟값이 최댓값보다 클 수 없다.</summary>
        public bool IsConsistent()
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].MinGold < 0 || Rules[i].MaxGold < Rules[i].MinGold)
                {
                    return false;
                }

                if (Rules[i].CardCount < 0)
                {
                    return false;
                }

                // 카드를 주는데 무엇을 줄지 정하지 않았으면 빈 화면이 뜬다.
                bool gives = Rules[i].GivesSymbols || Rules[i].GivesCoins || Rules[i].GivesRelics;
                if (Rules[i].CardCount > 0 && !gives)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>방 하나의 보상 규칙.</summary>
    [Serializable]
    public struct RewardRule
    {
        public RoomType RoomType;

        [Tooltip("기본 보상 골드의 최솟값.")]
        public int MinGold;

        [Tooltip("기본 보상 골드의 최댓값.")]
        public int MaxGold;

        [Tooltip("고를 카드 장수. 0 이면 카드가 나오지 않는다.")]
        public int CardCount;

        public bool GivesSymbols;
        public bool GivesCoins;
        public bool GivesRelics;

        public RewardRule(
            RoomType roomType, int minGold, int maxGold, int cardCount,
            bool givesSymbols, bool givesCoins, bool givesRelics)
        {
            RoomType = roomType;
            MinGold = minGold;
            MaxGold = maxGold;
            CardCount = cardCount;
            GivesSymbols = givesSymbols;
            GivesCoins = givesCoins;
            GivesRelics = givesRelics;
        }
    }
}
