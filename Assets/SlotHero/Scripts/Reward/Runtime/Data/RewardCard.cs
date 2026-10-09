using System;
using SlotHero.Combat;

namespace SlotHero.Reward
{
    /// <summary>보상 카드에 담기는 것이 무엇인지.</summary>
    public enum RewardKind
    {
        /// <summary>정해지지 않은 상태.</summary>
        None = 0,

        /// <summary>문양. 태그 둘을 함께 적는다.</summary>
        Symbol = 1,

        /// <summary>코인.</summary>
        Coin = 2,

        /// <summary>유물.</summary>
        Relic = 3,
    }

    /// <summary>
    /// 보상 카드 하나.
    /// 인게임 화면 기획서 v0.2 / 10 보상 화면 의 "보상 카드"다.
    ///
    /// 기획서가 "해당 아이템의 주요 정보를 표시한다"로 정했다.
    /// 와이어프레임을 보면 그림과 이름, 그리고 문양이면 태그 둘이 들어간다.
    /// </summary>
    [Serializable]
    public class RewardCard
    {
        /// <summary>물건 식별자. 목록에서 그림과 이름을 찾는 열쇠다.</summary>
        public string Id = string.Empty;

        /// <summary>화면에 적는 이름.</summary>
        public string DisplayName = string.Empty;

        /// <summary>무엇인지.</summary>
        public RewardKind Kind = RewardKind.None;

        /// <summary>문양일 때의 첫 태그.</summary>
        public SymbolTagType FirstTag;

        /// <summary>문양일 때의 둘째 태그.</summary>
        public SymbolTagType SecondTag;

        /// <summary>쓸 수 있는 카드인지.</summary>
        public bool IsValid
        {
            get { return Kind != RewardKind.None && !string.IsNullOrEmpty(Id); }
        }

        /// <summary>태그 줄을 적어야 하는지. 문양만 적는다.</summary>
        public bool HasTags
        {
            get { return Kind == RewardKind.Symbol; }
        }

        /// <summary>문양 카드.</summary>
        public static RewardCard Symbol(
            string id, string displayName, SymbolTagType firstTag, SymbolTagType secondTag)
        {
            RewardCard card = new RewardCard();
            card.Id = id;
            card.DisplayName = displayName;
            card.Kind = RewardKind.Symbol;
            card.FirstTag = firstTag;
            card.SecondTag = secondTag;
            return card;
        }

        /// <summary>코인 카드.</summary>
        public static RewardCard Coin(string id, string displayName)
        {
            RewardCard card = new RewardCard();
            card.Id = id;
            card.DisplayName = displayName;
            card.Kind = RewardKind.Coin;
            return card;
        }

        /// <summary>유물 카드.</summary>
        public static RewardCard Relic(string id, string displayName)
        {
            RewardCard card = new RewardCard();
            card.Id = id;
            card.DisplayName = displayName;
            card.Kind = RewardKind.Relic;
            return card;
        }
    }
}
