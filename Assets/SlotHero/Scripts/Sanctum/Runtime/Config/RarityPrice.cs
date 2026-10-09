using System;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 등급 하나의 기준가.
    /// 성소 기획서 v0.2 / 07 가격 의 코인 기준가 표와 유물 기준가 표의 한 줄에 해당한다.
    /// </summary>
    [Serializable]
    public struct RarityPrice
    {
        public ItemRarity Rarity;
        public int Price;

        public RarityPrice(ItemRarity rarity, int price)
        {
            Rarity = rarity;
            Price = price;
        }
    }
}
