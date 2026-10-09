using System;
using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상 진열 전체.
    /// 성소 기획서 v0.2 / 05 행상 구성 의 문양 4, 코인 2, 유물 3 자리를 그대로 담는다.
    ///
    /// 08 화면 구성 의 배치는 좌측 돗자리에 문양과 코인, 우측 양탄자에 유물이므로
    /// 자리 목록도 그 세 덩어리로 나눠 둔다.
    /// </summary>
    [Serializable]
    public class MerchantStock
    {
        /// <summary>문양 자리. 돗자리 상단에 2 × 2로 놓는다.</summary>
        public List<MerchantSlot> SymbolSlots = new List<MerchantSlot>();

        /// <summary>코인 자리. 돗자리 하단에 가로로 놓는다.</summary>
        public List<MerchantSlot> CoinSlots = new List<MerchantSlot>();

        /// <summary>유물 자리. 육각형의 가로 넓은 공간에 놓는다.</summary>
        public List<MerchantSlot> RelicSlots = new List<MerchantSlot>();

        /// <summary>자리 수를 맞춰 비어 있는 진열을 만든다.</summary>
        public static MerchantStock CreateEmpty(int symbolCount, int coinCount, int relicCount)
        {
            MerchantStock stock = new MerchantStock();
            Fill(stock.SymbolSlots, symbolCount);
            Fill(stock.CoinSlots, coinCount);
            Fill(stock.RelicSlots, relicCount);
            return stock;
        }

        /// <summary>진열된 모든 자리를 문양, 코인, 유물 순서로 훑는다.</summary>
        public IEnumerable<MerchantSlot> AllSlots()
        {
            for (int i = 0; i < SymbolSlots.Count; i++)
            {
                yield return SymbolSlots[i];
            }

            for (int i = 0; i < CoinSlots.Count; i++)
            {
                yield return CoinSlots[i];
            }

            for (int i = 0; i < RelicSlots.Count; i++)
            {
                yield return RelicSlots[i];
            }
        }

        /// <summary>종류에 맞는 자리 목록을 돌려준다.</summary>
        public List<MerchantSlot> GetSlots(SanctumItemKind kind)
        {
            switch (kind)
            {
                case SanctumItemKind.Symbol:
                    return SymbolSlots;
                case SanctumItemKind.Coin:
                    return CoinSlots;
                case SanctumItemKind.Relic:
                    return RelicSlots;
                default:
                    return null;
            }
        }

        /// <summary>불러온 뒤 자리 목록이 비어 있지 않도록 손본다.</summary>
        public void EnsureSlots(int symbolCount, int coinCount, int relicCount)
        {
            EnsureCount(SymbolSlots, symbolCount);
            EnsureCount(CoinSlots, coinCount);
            EnsureCount(RelicSlots, relicCount);
        }

        private static void Fill(List<MerchantSlot> slots, int count)
        {
            slots.Clear();
            for (int i = 0; i < count; i++)
            {
                slots.Add(new MerchantSlot());
            }
        }

        private static void EnsureCount(List<MerchantSlot> slots, int count)
        {
            if (slots == null)
            {
                return;
            }

            while (slots.Count < count)
            {
                slots.Add(new MerchantSlot());
            }

            while (slots.Count > count)
            {
                slots.RemoveAt(slots.Count - 1);
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = new MerchantSlot();
                }
            }
        }
    }
}
