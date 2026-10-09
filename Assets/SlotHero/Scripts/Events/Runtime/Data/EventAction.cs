using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 선택지가 하는 작업.
    /// 이벤트 기획서 초안 1장 선택지 종류 의
    /// "획득, 지불, 손실, 교환, 변경, 전투 시작" 여섯 갈래를 펼친 것이다.
    /// </summary>
    public enum EventActionKind
    {
        /// <summary>아무것도 하지 않는다. 진행과 종료가 쓴다.</summary>
        None = 0,

        /// <summary>문양을 받는다. 대상이 있으면 그것, 없으면 무작위.</summary>
        GainSymbol = 1,

        /// <summary>문양을 잃는다.</summary>
        LoseSymbol = 2,

        /// <summary>가진 문양 하나를 다른 무작위 문양으로 바꾼다.</summary>
        ChangeSymbol = 3,

        /// <summary>가진 문양을 모두 무작위 문양으로 바꾼다. 장수는 그대로다.</summary>
        ChangeAllSymbols = 4,

        /// <summary>유물을 받는다.</summary>
        GainRelic = 5,

        /// <summary>가진 유물 하나를 다른 유물로 바꾼다.</summary>
        ChangeRelic = 6,

        /// <summary>코인을 받는다.</summary>
        GainCoin = 7,

        /// <summary>코인을 잃는다.</summary>
        LoseCoin = 8,

        /// <summary>코인을 같은 등급의 다른 코인으로 바꾼다.</summary>
        ChangeCoin = 9,

        /// <summary>코인을 한 단계 낮은 등급으로 떨어뜨린다.</summary>
        DowngradeCoin = 10,

        /// <summary>코인을 한 단계 높은 등급으로 올린다.</summary>
        UpgradeCoin = 19,

        /// <summary>코인을 팔아 골드를 받는다.</summary>
        SellCoin = 11,

        /// <summary>골드를 받는다.</summary>
        GainGold = 12,

        /// <summary>골드를 잃는다. 모자라면 가진 만큼만 잃는다.</summary>
        LoseGold = 13,

        /// <summary>체력을 회복한다.</summary>
        Heal = 14,

        /// <summary>체력을 잃는다.</summary>
        LoseHealth = 15,

        /// <summary>최대 체력이 영구히 는다.</summary>
        GainMaxHealth = 16,

        /// <summary>최대 체력이 영구히 준다.</summary>
        LoseMaxHealth = 17,

        /// <summary>전투를 시작한다.</summary>
        StartCombat = 18,

        /// <summary>
        /// 두 태그를 모두 가진 문양 하나를 받는다. 02 힘을 새기는 것.
        /// 한 태그는 기억해 둔 것, 다른 태그는 선택지가 지목한 것이다.
        /// 문양은 태그를 둘씩 가지고 모든 짝에 문양이 하나씩 있어 하나로 정해진다.
        /// </summary>
        GainSymbolByTags = 20,

        /// <summary>
        /// 기억해 둔 문양의 태그 하나를 바꾼다. 04 닮은 모습.
        /// 기억 두 개를 쓴다. 바꿀 문양과 버릴 태그다. 새 태그는 선택지가 지목한 것이다.
        /// 남긴 태그와 새 태그를 가진 문양으로 바뀐다.
        /// </summary>
        ReshapeSymbol = 21,

        /// <summary>
        /// 지목한 태그를 가진 문양을 모두 그 태그가 없는 무작위 문양으로 바꾼다. 05 정화.
        /// 장수는 그대로다.
        /// </summary>
        PurgeTag = 22,

        /// <summary>
        /// 코인 소지 한도를 `Amount` 만큼 바꾼다. 음수면 줄인다. 0 아래로는 내려가지 않는다.
        /// 한도는 특정 조건에서 바뀔 수 있다(2026년 10월 9일 원재). 어느 이벤트가 쓰는지는 아직 정해지지 않았다.
        /// 줄어 넘치면 흐름이 넘친 만큼 바로 버리게 한다.
        /// </summary>
        ChangeCoinCapacity = 23,

        /// <summary>유물 소지 한도를 `Amount` 만큼 바꾼다. 규칙은 `ChangeCoinCapacity` 와 같다.</summary>
        ChangeRelicCapacity = 24,
    }

    /// <summary>
    /// 작업 하나의 내용.
    ///
    /// 수치는 두 갈래로 적는다.
    /// <see cref="Amount"/> 는 골드처럼 그대로 쓰는 값이고,
    /// <see cref="MaxHealthRatio"/> 는 최대 체력의 몇 퍼센트인지다.
    /// 기획서가 체력을 거의 비율로 적어 두었기 때문이다.
    /// </summary>
    [Serializable]
    public class EventAction
    {
        /// <summary>
        /// 체력이 모자라 못 하는 선택지에 적는 글. 4장 공통 규칙 의 "체력 손실로 현재 체력이 1 미만이 되는 선택지는 비활성".
        /// 각본을 굴리는 쪽이 이 글로 체력 때문에만 막힌 선택지를 가려 강제 손실로 다시 연다(`EventRunner`).
        /// </summary>
        public const string NotEnoughHealth = "체력 부족";

        /// <summary>
        /// 최대 체력이 1 아래로 내려가 못 하는 선택지에 적는 글.
        /// 최대 체력 손실은 지금 체력을 1 아래로 떨어뜨리지 않으므로 강제 손실로 다시 열지 않는다.
        /// </summary>
        public const string NotEnoughMaxHealth = "최대 체력 부족";

        public EventActionKind Kind = EventActionKind.None;

        /// <summary>그대로 쓰는 값. 골드와 개수에 쓴다.</summary>
        public int Amount;

        /// <summary>최대 체력에 곱할 비율. 0.15 면 15퍼센트다.</summary>
        public float MaxHealthRatio;

        /// <summary>몇 개에 적용할지. 유물 둘을 한꺼번에 받을 때처럼 쓴다.</summary>
        public int Count = 1;

        /// <summary>
        /// 작업을 하기 전에 치르는 골드.
        /// 기획서의 "10G를 지불하고 교환한다" 같은 줄이 이것이다.
        /// 모자라면 그 선택지가 비활성이 된다.
        /// </summary>
        public int GoldCost;

        /// <summary>
        /// 작업과 함께 받는 골드.
        /// 기획서의 "체력을 잃고 00G를 획득한다" 같은 줄이 이것이다.
        /// </summary>
        public int GoldGain;

        /// <summary>
        /// 작업과 함께 영구히 깎는 최대 체력의 비율. 0.10 이면 10퍼센트다.
        /// 12 잠든 보물 의 "유물 A와 B를 모두 획득한다. 최대 체력이 00% 영구 감소한다" 가 이것이다.
        /// 기획서가 한 선택지에 둘을 함께 적어 두어 사이 화면 없이 한 번에 한다.
        /// </summary>
        public float MaxHealthCostRatio;

        /// <summary>
        /// 선택지가 지목한 대상을 쓴다.
        /// 켜 두면 화면에 들어갈 때 채워 넣은 그 문양이나 코인이 대상이 된다.
        /// </summary>
        public bool UseChoiceTarget;

        /// <summary>전투 보상을 두 배로 준다. 3장 도전 이벤트 공통 규칙.</summary>
        public bool DoubleCombatReward;

        /// <summary>전투에 부정 효과를 하나 걸고 시작한다.</summary>
        public bool WithCombatPenalty;

        /// <summary>
        /// 작업에 넘길 기억 이름들. 앞 화면에서 고른 것을 쓰는 작업이 적는다.
        /// 적은 차례대로 <see cref="EventTarget.Recalled"/> 에 들어간다.
        /// </summary>
        public List<string> RecallKeys = new List<string>();

        /// <summary>
        /// 대상을 따로 지목하지 않을 때 고를 보유 물건의 가장 낮은 등급. -1 이면 따지지 않는다.
        /// 등급은 0 일반 ~ 4 전설이다. 25 미치광이 대장장이 의 "고급 이상 보유 코인 중 무작위 1개" 가 1 로 쓴다.
        /// 그런 물건이 없으면 그 선택지는 비활성이다.
        /// </summary>
        public int TargetMinRarity = -1;

        /// <summary>
        /// 받을 물건의 등급을 정하는 비중. 잃는 물건의 등급에서 몇 단계 위인지가 차례다(0 같은 등급, 1 한 단계 위 …).
        /// 13 유물 교환 의 "같은 희귀도 50%, 한 단계 높은 희귀도 40%, 두 단계 높은 희귀도 10%" 가 50, 40, 10 이다.
        /// 비어 있으면 등급을 따지지 않고 남은 것에서 고르게 뽑는다.
        /// </summary>
        public List<int> RarityStepWeights = new List<int>();

        public EventAction()
        {
        }

        public EventAction(EventActionKind kind)
        {
            Kind = kind;
        }

        /// <summary>대상을 따로 지목하지 않을 때 그 등급 이상의 보유 물건에서 고르게 한다.</summary>
        public EventAction TargetingRarityAtLeast(int rarity)
        {
            TargetMinRarity = rarity;
            return this;
        }

        /// <summary>받을 물건의 등급을 잃는 물건보다 몇 단계 위로 할지 비중을 정한다. 0 단계부터 적는다.</summary>
        public EventAction StepUpRarity(params int[] weights)
        {
            RarityStepWeights.Clear();

            if (weights != null)
            {
                RarityStepWeights.AddRange(weights);
            }

            return this;
        }

        /// <summary>아무것도 하지 않는 작업.</summary>
        public static EventAction None()
        {
            return new EventAction();
        }

        /// <summary>골드를 주고받는 작업.</summary>
        public static EventAction Gold(EventActionKind kind, int amount)
        {
            EventAction action = new EventAction(kind);
            action.Amount = amount;
            return action;
        }

        /// <summary>최대 체력의 몇 퍼센트로 재는 작업.</summary>
        public static EventAction ByMaxHealth(EventActionKind kind, float ratio)
        {
            EventAction action = new EventAction(kind);
            action.MaxHealthRatio = ratio;
            return action;
        }

        /// <summary>선택지가 지목한 대상에 거는 작업.</summary>
        public static EventAction OnTarget(EventActionKind kind)
        {
            EventAction action = new EventAction(kind);
            action.UseChoiceTarget = true;
            return action;
        }

        /// <summary>전투를 시작하는 작업.</summary>
        public static EventAction Combat(bool doubleReward, bool withPenalty)
        {
            EventAction action = new EventAction(EventActionKind.StartCombat);
            action.DoubleCombatReward = doubleReward;
            action.WithCombatPenalty = withPenalty;
            return action;
        }

        /// <summary>
        /// 이 작업이 실제로 다루는 체력 수. 최대 체력의 비율이면 반올림하고 최소 1 이다.
        /// 비율이 없으면 <see cref="Amount"/> 를 그대로 쓴다.
        /// 선택지 칸에 적는 숫자와 실제로 깎이는 숫자가 같아야 하므로 둘 다 이것을 쓴다.
        /// </summary>
        public int HealthAmount(int maxHealth)
        {
            return MaxHealthRatio > 0f ? ByRatio(maxHealth, MaxHealthRatio) : Amount;
        }

        /// <summary>함께 깎는 최대 체력 수. 없으면 0 이다.</summary>
        public int MaxHealthCostAmount(int maxHealth)
        {
            return MaxHealthCostRatio > 0f ? ByRatio(maxHealth, MaxHealthCostRatio) : 0;
        }

        /// <summary>최대 체력에 비율을 곱해 반올림한다. 최소 1 이다. 최대 체력이 작을 때 0 이 되지 않게 한다.</summary>
        public static int ByRatio(int maxHealth, float ratio)
        {
            float raw = maxHealth * ratio;
            int value = (int)Math.Round(raw, MidpointRounding.AwayFromZero);
            return value < 1 ? 1 : value;
        }

        /// <summary>이 작업이 전투를 여는지. 전투는 화면을 넘기는 때가 다르다.</summary>
        public bool OpensCombat
        {
            get { return Kind == EventActionKind.StartCombat; }
        }

        /// <summary>값을 치르게 한다. 줄줄이 부를 수 있게 자기를 돌려준다.</summary>
        public EventAction Costing(int gold)
        {
            GoldCost = gold;
            return this;
        }

        /// <summary>골드를 함께 받게 한다.</summary>
        public EventAction Giving(int gold)
        {
            GoldGain = gold;
            return this;
        }

        /// <summary>최대 체력을 함께 영구히 깎게 한다.</summary>
        public EventAction CostingMaxHealth(float ratio)
        {
            MaxHealthCostRatio = ratio;
            return this;
        }

        /// <summary>몇 개에 걸지 정한다.</summary>
        public EventAction Times(int count)
        {
            Count = count;
            return this;
        }

        /// <summary>앞 화면에서 기억해 둔 것을 작업에 넘기게 한다.</summary>
        public EventAction Recalling(params string[] keys)
        {
            if (keys != null)
            {
                RecallKeys.AddRange(keys);
            }

            return this;
        }
    }
}
