using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투의 규칙과 수치.
    ///
    /// 틀은 슬롯머신 작동 방식 1.1.a 와 전투 UI 및 연출 초안을 따른다.
    /// **수치는 거의 다 임시다.** 2026년 10월 25일 기준 적 데이터, 코인 목록, 중복 배수표, 전투 골드를 정한 기획서가 없어
    /// 임시값으로 두기로 했다. 1.1.a 도 "코인의 수치 및 체력의 수치는 차후 밸런스를 위해서 변경해야합니다" 라고 적었다.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatRuleConfig", menuName = "Slot Hero/전투/규칙")]
    public class CombatRuleConfig : ScriptableObject
    {
        [Header("슬롯")]
        [Tooltip("릴 수. 전투 UI 초안의 \"문양(6개)\" 다. 흐름의 최소 덱 장수(6)와 맞춘다.")]
        [Min(1)]
        public int ReelCount = 6;

        [Header("코인")]
        [Tooltip("매 턴 코인 풀에서 꺼내 보여 주는 코인 수. 1.1.a 의 \"매턴 코인은 코인풀에서 랜덤으로 3개가 제공된다\".")]
        [Min(1)]
        public int CoinsOffered = 3;

        [Tooltip("코인을 쓰고 남아야 하는 체력. 소모값 때문에 스스로 쓰러지지 않게 한다. " +
                 "1.1.a 는 \"해당 값 이상의 체력을 소유해야만 사용 가능\" 이라 적었는데, 그대로면 체력 10 으로 소모 10 코인을 써 0 이 된다. " +
                 "저장소 원본의 `PlayerCoinInventory.minimumRemainingHealth` 도 1 이었다.")]
        [Min(0)]
        public int MinimumHealthAfterCoin = 1;

        [Tooltip("코인마다 소모값과 가중치. 목록에 없는 코인은 아래 기본값을 쓴다. **임시다.** " +
                 "금화는 1.1.a 예시의 첫 코인(소모 10 · 가중치 5), 은화는 둘째 코인(소모 20 · 가중치 15)을 옮겼다.")]
        public List<CoinRule> Coins = new List<CoinRule>
        {
            new CoinRule("coin_gold", 10, 5, "건 태그의 배수에 5 를 더한다."),
            new CoinRule("coin_silver", 20, 15, "건 태그의 배수에 15 를 더한다."),
        };

        [Tooltip("목록에 없는 코인의 소모값. 임시다.")]
        [Min(0)]
        public int DefaultCoinCost = 10;

        [Tooltip("목록에 없는 코인의 가중치. 임시다.")]
        [Min(0)]
        public int DefaultCoinWeight = 5;

        [Header("중복 배수")]
        [Tooltip("같은 태그 문양이 몇 개일 때 배수가 얼마인지. 적지 않은 개수는 개수 그대로다. **임시다.** " +
                 "1.1.a 예시가 1개 ×1, 2개 ×2 라 그 꼴로 이어 두었다.")]
        public List<DuplicateMultiplier> DuplicateMultipliers = new List<DuplicateMultiplier>
        {
            new DuplicateMultiplier(1, 1),
            new DuplicateMultiplier(2, 2),
            new DuplicateMultiplier(3, 3),
            new DuplicateMultiplier(4, 4),
            new DuplicateMultiplier(5, 5),
            new DuplicateMultiplier(6, 6),
        };

        [Header("적")]
        [Tooltip("방 종류마다 첫 스테이지의 적. **임시다.** 이름은 Jackpot 아트웍의 \"투기장 검투사\" 를 빌렸다.")]
        public List<EnemyRule> Enemies = new List<EnemyRule>
        {
            new EnemyRule(CombatEnemyKind.Normal, "투기장 검투사", "콜로세움에서 싸우는 검투사.", 40, 6),
            new EnemyRule(CombatEnemyKind.Elite, "투기장 챔피언", "검투사들 가운데 살아남은 자.", 80, 9),
            new EnemyRule(CombatEnemyKind.Boss, "콜로세움의 주인", "콜로세움을 지배하는 자.", 160, 12),
        };

        [Tooltip("스테이지가 하나 오를 때마다 적 체력에 곱하는 비율의 증가분. 0.5 면 2스테이지 1.5배, 3스테이지 2배다. 임시다.")]
        [Min(0f)]
        public float EnemyHealthGrowthPerStage = 0.5f;

        [Tooltip("스테이지가 하나 오를 때마다 적 공격력에 더하는 값. 임시다.")]
        [Min(0)]
        public int EnemyAttackGrowthPerStage = 2;

        [Header("보상")]
        [Tooltip("이 피해만큼 넘칠 때마다 오버킬 골드 1. 0 이면 오버킬 골드를 주지 않는다. 임시다. " +
                 "보상 화면에 오버킬 줄이 있고 얼마가 되는지는 전투가 정하기로 되어 있다.")]
        [Min(0)]
        public int OverkillDamagePerGold = 10;

        /// <summary>그 코인의 규칙. 목록에 없으면 기본값으로 만든다.</summary>
        public CoinRule GetCoinRule(string coinId)
        {
            for (int i = 0; i < Coins.Count; i++)
            {
                if (Coins[i].Id == coinId)
                {
                    return Coins[i];
                }
            }

            return new CoinRule(coinId, DefaultCoinCost, DefaultCoinWeight,
                "건 태그의 배수에 " + DefaultCoinWeight + " 를 더한다.");
        }

        /// <summary>
        /// 같은 태그 문양 수에 맞는 배수. 0 개면 0 이다.
        /// 표에 정확히 있으면 그 값, 없으면 개수 그대로다.
        /// </summary>
        public int GetDuplicateMultiplier(int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            for (int i = 0; i < DuplicateMultipliers.Count; i++)
            {
                if (DuplicateMultipliers[i].Count == count)
                {
                    return DuplicateMultipliers[i].Multiplier;
                }
            }

            return count;
        }

        /// <summary>그 종류와 스테이지의 적을 새로 만든다. 스테이지는 1부터다.</summary>
        public CombatEnemy MakeEnemy(CombatEnemyKind kind, int stage)
        {
            EnemyRule rule = new EnemyRule(kind, "적", string.Empty, 30, 5);
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].Kind == kind)
                {
                    rule = Enemies[i];
                    break;
                }
            }

            int above = stage > 1 ? stage - 1 : 0;
            int health = Mathf.RoundToInt(rule.MaxHealth * (1f + EnemyHealthGrowthPerStage * above));
            int attack = rule.Attack + EnemyAttackGrowthPerStage * above;

            return new CombatEnemy(rule.Name, rule.Description, health, attack);
        }

        /// <summary>넘친 피해로 받는 오버킬 골드.</summary>
        public int GetOverkillGold(int overkill)
        {
            if (OverkillDamagePerGold <= 0 || overkill <= 0)
            {
                return 0;
            }

            return overkill / OverkillDamagePerGold;
        }
    }

    /// <summary>코인 하나의 소모값과 가중치.</summary>
    [Serializable]
    public struct CoinRule
    {
        [Tooltip("물건 목록의 코인 식별자.")]
        public string Id;

        [Tooltip("쓸 때 내는 체력.")]
        public int HealthCost;

        [Tooltip("중복 배수에 더하는 값.")]
        public int Weight;

        [Tooltip("코인 정보 팝업에 적는 능력 설명.")]
        public string Description;

        public CoinRule(string id, int healthCost, int weight, string description)
        {
            Id = id;
            HealthCost = healthCost;
            Weight = weight;
            Description = description;
        }
    }

    /// <summary>같은 태그 문양 수와 그 배수.</summary>
    [Serializable]
    public struct DuplicateMultiplier
    {
        public int Count;
        public int Multiplier;

        public DuplicateMultiplier(int count, int multiplier)
        {
            Count = count;
            Multiplier = multiplier;
        }
    }

    /// <summary>방 종류 하나의 첫 스테이지 적.</summary>
    [Serializable]
    public struct EnemyRule
    {
        public CombatEnemyKind Kind;
        public string Name;
        public string Description;
        public int MaxHealth;
        public int Attack;

        public EnemyRule(CombatEnemyKind kind, string name, string description, int maxHealth, int attack)
        {
            Kind = kind;
            Name = name;
            Description = description;
            MaxHealth = maxHealth;
            Attack = attack;
        }
    }
}
