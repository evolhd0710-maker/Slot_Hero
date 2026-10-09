using System;
using System.Collections.Generic;

namespace SlotHero.Reward
{
    /// <summary>
    /// 방을 깨고 받는 보상 한 벌.
    /// 인게임 화면 기획서 v0.2 / 10 보상 화면 의 "보상 박스" 안에 들어가는 것들이다.
    ///
    /// 기본 보상 골드, 오버킬 보상 골드, 그리고 고를 카드 목록이다.
    /// 카드 하나를 고르면 그것만 받고 화면이 닫힌다.
    /// 건너뛰기를 누르면 카드는 받지 않고 나간다.
    ///
    /// 골드는 고르는 것이 아니라 방을 깬 순간 정해진 것이라 카드와 달리 늘 받는다.
    /// </summary>
    [Serializable]
    public class RewardOffer
    {
        /// <summary>방을 깨서 받는 골드.</summary>
        public int BaseGold;

        /// <summary>오버킬로 더 받는 골드. 0 이면 그 줄을 비워 둔다.</summary>
        public int OverkillGold;

        /// <summary>고를 수 있는 카드.</summary>
        public List<RewardCard> Cards = new List<RewardCard>();

        /// <summary>받는 골드를 다 더한 값.</summary>
        public int TotalGold
        {
            get { return BaseGold + OverkillGold; }
        }

        /// <summary>
        /// 오버킬 줄을 보여 줄지.
        /// 기획서가 "오버킬 골드가 없을 경우 해당 줄은 비워둔다"로 정했다.
        /// </summary>
        public bool HasOverkill
        {
            get { return OverkillGold > 0; }
        }

        /// <summary>고를 카드가 있는지.</summary>
        public bool HasCards
        {
            get { return Cards.Count > 0; }
        }

        /// <summary>받을 것이 하나도 없는지. 골드도 카드도 없으면 화면을 열 까닭이 없다.</summary>
        public bool IsEmpty
        {
            get { return !HasCards && TotalGold <= 0; }
        }

        /// <summary>골드만 주는 보상.</summary>
        public static RewardOffer Gold(int baseGold, int overkillGold)
        {
            RewardOffer offer = new RewardOffer();
            offer.BaseGold = baseGold;
            offer.OverkillGold = overkillGold;
            return offer;
        }

        /// <summary>카드를 넣는다.</summary>
        public RewardOffer Add(RewardCard card)
        {
            if (card != null && card.IsValid)
            {
                Cards.Add(card);
            }

            return this;
        }

        /// <summary>식별자로 카드를 찾는다. 없으면 null 이다.</summary>
        public RewardCard Find(string cardId)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].Id == cardId)
                {
                    return Cards[i];
                }
            }

            return null;
        }

        /// <summary>값이 앞뒤가 맞는지.</summary>
        public bool IsConsistent()
        {
            if (BaseGold < 0 || OverkillGold < 0)
            {
                return false;
            }

            for (int i = 0; i < Cards.Count; i++)
            {
                if (!Cards[i].IsValid)
                {
                    return false;
                }

                // 같은 카드가 두 번 나오면 고른 것을 가릴 수 없다.
                for (int j = i + 1; j < Cards.Count; j++)
                {
                    if (Cards[i].Id == Cards[j].Id)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
