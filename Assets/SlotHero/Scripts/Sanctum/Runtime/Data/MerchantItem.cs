using System;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상이 다루는 상품 하나.
    /// 성소 기획서 v0.2 / 02 요약 의 "행상에서 판매하는 아이템은 문양, 코인, 유물 기획서에서 다룬다"에 따라
    /// 상품의 실제 정의는 담지 않고, 진열과 가격 산정에 필요한 값만 옮겨 담는다.
    ///
    /// 아이콘 같은 표시 자원은 여기에 넣지 않는다.
    /// 저장 시스템 기획서의 런 데이터에 그대로 실리려면 순수 값만 있어야 하기 때문이다.
    /// 표시 자원은 <see cref="IMerchantCatalog.GetIcon"/>으로 Id를 주고 받아 온다.
    /// </summary>
    [Serializable]
    public struct MerchantItem
    {
        /// <summary>문양, 코인, 유물 기획서가 정한 식별자. 저장과 복원의 기준이다.</summary>
        public string Id;

        /// <summary>화면에 띄우는 이름.</summary>
        public string DisplayName;

        /// <summary>상품 종류.</summary>
        public SanctumItemKind Kind;

        /// <summary>등급. 코인과 유물에만 쓴다. 07 가격 의 코인 기준가, 유물 기준가 표.</summary>
        public ItemRarity Rarity;

        /// <summary>
        /// 문양이 가진 태그 가치의 합. 문양에만 쓴다.
        /// 07 가격 의 "문양의 가격은 20 + (태그 가치 합) × 5".
        /// 태그마다의 가치는 문양 기획서가 정하므로 여기서는 합만 받는다.
        /// </summary>
        public int TagValueSum;

        /// <summary>상품이 들어 있는지. Id가 비어 있으면 빈 자리로 본다.</summary>
        public bool IsValid
        {
            get { return Kind != SanctumItemKind.None && !string.IsNullOrEmpty(Id); }
        }

        public static MerchantItem Symbol(string id, string displayName, int tagValueSum)
        {
            MerchantItem item = new MerchantItem();
            item.Id = id;
            item.DisplayName = displayName;
            item.Kind = SanctumItemKind.Symbol;
            item.TagValueSum = tagValueSum;
            return item;
        }

        public static MerchantItem Coin(string id, string displayName, ItemRarity rarity)
        {
            MerchantItem item = new MerchantItem();
            item.Id = id;
            item.DisplayName = displayName;
            item.Kind = SanctumItemKind.Coin;
            item.Rarity = rarity;
            return item;
        }

        public static MerchantItem Relic(string id, string displayName, ItemRarity rarity)
        {
            MerchantItem item = new MerchantItem();
            item.Id = id;
            item.DisplayName = displayName;
            item.Kind = SanctumItemKind.Relic;
            item.Rarity = rarity;
            return item;
        }
    }
}
