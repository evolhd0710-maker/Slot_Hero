using System;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상 진열의 한 자리.
    /// 성소 기획서 v0.2 / 05 행상 구성 의 문양 4자리, 코인 2자리, 유물 3자리가 모두 이 자료를 쓴다.
    ///
    /// 가격은 자리에 박아 둔다. 진열을 만들 때 한 번 계산해 두면
    /// 08 화면 구성 의 "물건의 가격은 물건의 하단에 골드 아이콘과 함께 배치한다"를
    /// 매 프레임 다시 계산하지 않고 그릴 수 있다.
    /// </summary>
    [Serializable]
    public class MerchantSlot
    {
        /// <summary>이 자리에 놓인 상품.</summary>
        public MerchantItem Item;

        /// <summary>진열할 때 계산해 둔 가격.</summary>
        public int Price;

        /// <summary>이미 팔렸는지.</summary>
        public bool Sold;

        /// <summary>상품이 놓여 있고 아직 팔리지 않아 살 수 있는 자리인지.</summary>
        public bool IsBuyable
        {
            get { return Item.IsValid && !Sold; }
        }

        /// <summary>상품이 놓여 있는지. 후보가 모자라 비워 둔 자리는 false.</summary>
        public bool HasItem
        {
            get { return Item.IsValid; }
        }

        /// <summary>자리에 상품을 올린다.</summary>
        public void Set(MerchantItem item, int price)
        {
            Item = item;
            Price = price;
            Sold = false;
        }

        /// <summary>자리를 비운다.</summary>
        public void Clear()
        {
            Item = new MerchantItem();
            Price = 0;
            Sold = false;
        }
    }
}
