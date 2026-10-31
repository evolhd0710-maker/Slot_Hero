using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Combat
{
    /// <summary>
    /// 유물 효과의 종류. `유물 정보시트.xlsx` 의 조건(유물조건)과 능력(유물 능력)을 한 묶음씩 옮긴 것이다.
    ///
    /// 시트는 조건 Enum 과 능력 Enum 을 따로 두고 번호로 잇는 범용 꼴인데,
    /// 지금 유물 열 개가 쓰는 조합은 아래 열 가지뿐이라 조합 하나를 종류 하나로 두었다.
    /// 유물이 늘어 새 조합이 나오면 종류를 하나 더한다.
    /// </summary>
    public enum RelicEffectKind
    {
        /// <summary>항시발동 · 태그 가치에 더한다. 제국 군기, 일식.</summary>
        TagValueAdd,

        /// <summary>항시발동 · 코인 소모값에 더한다. 흑요석 단검.</summary>
        CoinCostAdd,

        /// <summary>공격력 계산 시 · 코인 가치(가중치)에 더한다. 흑요석 단검.</summary>
        CoinWeightAdd,

        /// <summary>
        /// 공격력 계산 시 · 유물 고유 수치만큼 코인 가치에 더한다. 고유 수치는 시작값에서
        /// 릴이 멈출 때 같은 태그 문양이 `Threshold` 개 이상이면 `Amount` 씩 영구히 오른다. 12개의 금화.
        /// </summary>
        CoinWeightFromCounter,

        /// <summary>`Threshold` 번째 턴 회전 종료 시 · 가장 많은 태그의 문양을 `Amount` 장 이번 전투 덱에 더한다. 망치와 끌.</summary>
        AddTopTagSymbols,

        /// <summary>적이 때릴 피해가 `Threshold` 이상이면 `Amount` 만큼 줄인다. 철제 방패.</summary>
        ReduceDamageTaken,

        /// <summary>블랙홀(소용돌이) 칸을 고르면 가장 많은 태그로 공격한다. 사건의 지평선.</summary>
        VortexAttacksTopTag,

        /// <summary>그 태그를 가진 문양을 쓰면 가치를 한 번 더 더한다. 태그마다 따로 발동한다. 별의 저편.</summary>
        ReuseSymbolsWithTag,

        /// <summary>왼쪽에서 `Threshold` 번째 문양을 쓰면 가치를 한 번 더 더한다. 속임수 주사위.</summary>
        ReuseReelAt,

        /// <summary>체력이 최대의 `Threshold` 퍼센트 이하면 공격력에 `Multiplier` 를 곱한다. 곰 가죽 망토.</summary>
        DamageMultiplierAtLowHealth
    }

    /// <summary>유물 효과 하나.</summary>
    [Serializable]
    public struct RelicEffect
    {
        public RelicEffectKind Kind;

        [Tooltip("조건이나 대상이 되는 태그. 태그를 쓰지 않는 효과는 비운다.")]
        public List<SymbolTagType> Tags;

        [Tooltip("더하거나 빼는 값. 문양을 더하는 효과면 장수다.")]
        public int Amount;

        [Tooltip("조건 수치. 몇 번째 턴, 몇 개 이상, 몇 이상의 피해, 몇 퍼센트 이하, 왼쪽에서 몇 번째.")]
        public int Threshold;

        [Tooltip("곱하는 값.")]
        public float Multiplier;

        public RelicEffect(RelicEffectKind kind, int amount, int threshold, float multiplier, params SymbolTagType[] tags)
        {
            Kind = kind;
            Amount = amount;
            Threshold = threshold;
            Multiplier = multiplier;
            Tags = new List<SymbolTagType>(tags);
        }

        /// <summary>그 태그가 대상인지.</summary>
        public bool Targets(SymbolTagType tag)
        {
            return Tags != null && Tags.Contains(tag);
        }
    }

    /// <summary>유물 하나. 시트의 `유물 목록` 한 줄과 그 유물의 조건 · 능력이다.</summary>
    [Serializable]
    public class RelicDefinition
    {
        [Tooltip("물건 목록의 식별자. `relic_` 뒤에 시트의 Num 이다.")]
        public string Id;

        [Tooltip("시트의 Num. 유물 데이터는 12 로 시작한다.")]
        public int Number;

        public string DisplayName;
        public string EnglishName;

        [Tooltip("시트의 희귀도. 1 일반, 2 고급, 3 희귀, 4 특급, 5 전설.")]
        [Range(1, 5)]
        public int Rarity = 1;

        [Tooltip("인게임 소개문. {0} 은 유물 고유 수치로 바뀐다(12개의 금화).")]
        [TextArea]
        public string Description;

        [TextArea]
        public string Flavor;

        [Tooltip("유물 고유 수치의 시작값. 시트의 `RelicValue` 를 `Set` 하는 능력이다. 쓰지 않으면 0.")]
        public int CounterStart;

        public List<RelicEffect> Effects = new List<RelicEffect>();

        /// <summary>쌓인 횟수로 유물 고유 수치를 낸다. 저장에는 쌓인 횟수만 남는다(`OwnedRelic.Stack`).</summary>
        public int GetCounter(int stack)
        {
            return CounterStart + stack;
        }

        /// <summary>소개문. 고유 수치 자리를 지금 값으로 채운다.</summary>
        public string GetDescription(int stack)
        {
            if (string.IsNullOrEmpty(Description) || !Description.Contains("{0}"))
            {
                return Description ?? string.Empty;
            }

            int counter = GetCounter(stack);
            return Description.Replace("{0}", counter >= 0 ? "+" + counter : counter.ToString());
        }

        /// <summary>그 종류의 효과가 있는지.</summary>
        public bool Has(RelicEffectKind kind)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                if (Effects[i].Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 유물 목록과 효과. `유물 정보시트.xlsx`(유물 목록 · 유물조건 · 유물 능력)를 옮긴 것이다.
    ///
    /// **코드가 정본이다.** `설정 에셋 한 번에 만들기` 가 이미 있는 에셋도 이 목록으로 갈아 끼우고,
    /// 물건 목록(`RunCatalogConfig.Relics`)의 이름과 희귀도도 여기에 맞춘다.
    /// 그림(`Relicimage_120NN.jpg`)은 아직 오지 않아 칸에 이름을 적는다.
    ///
    /// 2026년 10월 25일에 정한 해석.
    ///   - 망치와 끌로 더한 문양은 이번 전투 덱에만 들어간다
    ///   - 문양 재사용(별의 저편, 속임수 주사위)은 그 문양의 가치를 한 번 더 더한다. 중복 배수는 그대로다
    ///   - 12개의 금화는 릴이 멈출 때마다(처음 회전과 재굴림 뒤) 같은 태그 문양이 3개 이상이면 +1, 런 내내 쌓인다
    /// </summary>
    [CreateAssetMenu(fileName = "RelicConfig", menuName = "Slot Hero/전투/유물")]
    public class RelicConfig : ScriptableObject
    {
        public List<RelicDefinition> Relics = SheetRelics();

        /// <summary>그 식별자의 유물. 없으면 null 이다.</summary>
        public RelicDefinition Find(string id)
        {
            for (int i = 0; i < Relics.Count; i++)
            {
                if (Relics[i] != null && Relics[i].Id == id)
                {
                    return Relics[i];
                }
            }

            return null;
        }

        /// <summary>시트의 유물 열 개.</summary>
        public static List<RelicDefinition> SheetRelics()
        {
            List<RelicDefinition> list = new List<RelicDefinition>();

            list.Add(Make(12001, "제국 군기", "Aquila", 1,
                "화성 태그 가치 +5",
                "제국의 건국자는 마르스의 아들이니.", 0,
                new RelicEffect(RelicEffectKind.TagValueAdd, 5, 0, 1f, SymbolTagType.Mars)));

            list.Add(Make(12002, "흑요석 단검", "Obsidian Knife", 3,
                "코인 소모값 +10, 코인 가치 +3",
                "눈에는 눈, 이에는 이, 희생에는 희생", 0,
                new RelicEffect(RelicEffectKind.CoinCostAdd, 10, 0, 1f),
                new RelicEffect(RelicEffectKind.CoinWeightAdd, 3, 0, 1f)));

            list.Add(Make(12003, "12개의 금화", "Twelve Coin", 4,
                "코인 가치 {0}, 동일 태그를 지닌 문양이 3개 등장할때마다 영구히 +1",
                "영웅의 위대한 열두과업을 기념하는 주화", -5,
                new RelicEffect(RelicEffectKind.CoinWeightFromCounter, 1, 3, 1f)));

            list.Add(Make(12004, "망치와 끌", "Hammer and Chisel", 3,
                "첫 턴 최다 태그 문양 3개 추가",
                "모든 말들을 큰 회석에 새기라", 0,
                new RelicEffect(RelicEffectKind.AddTopTagSymbols, 3, 1, 1f)));

            list.Add(Make(12005, "철제 방패", "Iron Shield", 2,
                "10 이상의 데미지를 받을 때, 해당 데미지에 -5",
                string.Empty, 0,
                new RelicEffect(RelicEffectKind.ReduceDamageTaken, 5, 10, 1f)));

            list.Add(Make(12006, "사건의 지평선", "Event Horizon", 4,
                "블랙홀이 사용되었을 때, 가장 많은 태그로 공격",
                string.Empty, 0,
                new RelicEffect(RelicEffectKind.VortexAttacksTopTag, 0, 0, 1f)));

            list.Add(Make(12007, "별의 저편", "Beyond Stars", 5,
                "천왕성, 해왕성, 명왕성 태그가 2회 사용",
                string.Empty, 0,
                new RelicEffect(RelicEffectKind.ReuseSymbolsWithTag, 1, 0, 1f, SymbolTagType.Uranus),
                new RelicEffect(RelicEffectKind.ReuseSymbolsWithTag, 1, 0, 1f, SymbolTagType.Neptune),
                new RelicEffect(RelicEffectKind.ReuseSymbolsWithTag, 1, 0, 1f, SymbolTagType.Pluto)));

            list.Add(Make(12008, "일식", "Eclips", 3,
                "내행성의 가치 -3, 외행성의 가치 +3",
                string.Empty, 0,
                new RelicEffect(RelicEffectKind.TagValueAdd, -3, 0, 1f,
                    SymbolTagType.Mercury, SymbolTagType.Venus, SymbolTagType.Earth),
                new RelicEffect(RelicEffectKind.TagValueAdd, 3, 0, 1f,
                    SymbolTagType.Mars, SymbolTagType.Jupiter, SymbolTagType.Saturn,
                    SymbolTagType.Uranus, SymbolTagType.Neptune, SymbolTagType.Pluto)));

            list.Add(Make(12009, "곰 가죽 망토", "Berserk", 3,
                "체력 50% 이하일 때 모든 태그의 데미지 1.2배",
                string.Empty, 0,
                new RelicEffect(RelicEffectKind.DamageMultiplierAtLowHealth, 0, 50, 1.2f)));

            list.Add(Make(12010, "속임수 주사위", "Loaded dice", 3,
                "가장 좌측의 문양 1회 재사용",
                string.Empty, 0,
                new RelicEffect(RelicEffectKind.ReuseReelAt, 1, 1, 1f)));

            return list;
        }

        /// <summary>시트 Num 으로 물건 목록 식별자를 만든다.</summary>
        public static string IdFor(int number)
        {
            return "relic_" + number;
        }

        private static RelicDefinition Make(
            int number, string name, string englishName, int rarity, string description, string flavor,
            int counterStart, params RelicEffect[] effects)
        {
            RelicDefinition relic = new RelicDefinition();
            relic.Id = IdFor(number);
            relic.Number = number;
            relic.DisplayName = name;
            relic.EnglishName = englishName;
            relic.Rarity = rarity;
            relic.Description = description;
            relic.Flavor = flavor;
            relic.CounterStart = counterStart;
            relic.Effects.AddRange(effects);
            return relic;
        }
    }
}
