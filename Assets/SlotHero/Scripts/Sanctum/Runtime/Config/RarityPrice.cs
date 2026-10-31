using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 등급 하나의 기준가.
    /// 성소 기획서 v0.2 / 07 가격 의 코인 기준가 표와 유물 기준가 표의 한 줄에 해당한다.
    /// </summary>
    [Serializable]
    public struct RarityPrice
    {
        [FormerlySerializedAs("Rarity")]
        [SerializeField] private ItemRarity _rarity;
        public ItemRarity Rarity { get { return _rarity; } set { _rarity = value; } }

        [FormerlySerializedAs("Price")]
        [SerializeField] private int _price;
        public int Price { get { return _price; } set { _price = value; } }

        public RarityPrice(ItemRarity rarity, int price)
        {
            _rarity = rarity;
            _price = price;
        }
    }
}
