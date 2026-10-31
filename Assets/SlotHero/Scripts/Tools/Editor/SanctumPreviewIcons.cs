using SlotHero.Sanctum;
using SlotHero.Sanctum.UI;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 확인용 씬에서만 쓰는 상품 그림 창구.
    ///
    /// 진짜 게임에서는 `RunCatalog` 가 이 창구를 겸한다.
    /// 확인용 씬은 `SampleMerchantCatalog` 의 임시 상품을 쓰는데
    /// 그 상품에는 그림이 없어 이름으로 있는 그림을 골라 물린다.
    ///
    /// 문양은 게임과 같은 문양 그림(`Symbols`)을 쓴다. 예전에는 확인용으로만 쓰던 옛 그림 넷
    /// (`문양_검`, `문양_구름`, `문양_시계`, `문양_해골`)을 썼는데 2026년 10월 22일 원재가 지우라고 해 바꿨다.
    /// </summary>
    public class SanctumPreviewIcons : IMerchantIconSource
    {
        /// 임시 문양 식별자와 문양 그림 이름의 짝. 07 가격 예시 넷은 같은 문양을, 나머지는 닮은 문양을 쓴다.
        private static readonly string[] SymbolNames =
        {
            "symbol_mercury", "Quicksilver",
            "symbol_lotus", "Lotus",
            "symbol_crow", "Raven",
            "symbol_nightsky", "NightSky",
            "symbol_sword", "Scythe",
            "symbol_cloud", "DarkCloud",
            "symbol_clock", "Sunrise",
            "symbol_skull", "Grave",
        };

        /// 임시 유물 식별자와 지금 가진 그림 이름의 짝.
        /// 없는 것은 비워 두면 그 자리는 그림 없이 가격만 나온다.
        private static readonly string[] Names =
        {
            "relic_book", "relic_book",
            "relic_wing", "relic_wing",
            "relic_pickaxe", "relic_pick",
        };

        /// 짝이 없는 임시 문양에 돌려 쓰는 그림.
        private static readonly string[] SymbolFallback =
        {
            "Whale", "Volcano", "Coffin", "Golem",
        };

        private static readonly string[] CoinFallback =
        {
            "coin_silver", "coin_gold",
        };

        private static readonly string[] RelicFallback =
        {
            "relic_book", "relic_wing", "relic_pick",
        };

        public Sprite GetIcon(MerchantItem item)
        {
            // MerchantItem 은 저장에 그대로 실리는 구조체라 null 인지 묻지 못한다.
            // 빈 자리는 식별자가 비어 있다.
            if (string.IsNullOrEmpty(item.Id))
            {
                return null;
            }

            for (int i = 0; i + 1 < SymbolNames.Length; i += 2)
            {
                if (SymbolNames[i] == item.Id)
                {
                    return ImportArt.LoadSymbol(SymbolNames[i + 1]);
                }
            }

            for (int i = 0; i + 1 < Names.Length; i += 2)
            {
                if (Names[i] == item.Id)
                {
                    return ImportArt.LoadItemArt(Names[i + 1]);
                }
            }

            // 짝이 없는 임시 상품은 같은 종류의 그림을 돌려 쓴다.
            // 빈 칸보다는 자리와 크기를 눈으로 보는 편이 낫다.
            // 글자 코드를 더해 고른다. `GetHashCode` 는 값이 몰려 같은 그림만 나왔다.
            int sum = 0;
            for (int i = 0; i < item.Id.Length; i++)
            {
                sum += item.Id[i] * (i + 1);
            }

            if (item.Id.StartsWith("coin_"))
            {
                return ImportArt.LoadItemArt(CoinFallback[Mathf.Abs(sum) % CoinFallback.Length]);
            }

            if (item.Id.StartsWith("relic_"))
            {
                return ImportArt.LoadItemArt(RelicFallback[Mathf.Abs(sum) % RelicFallback.Length]);
            }

            return ImportArt.LoadSymbol(SymbolFallback[Mathf.Abs(sum) % SymbolFallback.Length]);
        }

        /// <summary>확인용 상품은 태그 자료가 없어 테두리를 얹지 않는다. 게임에서는 `RunCatalog` 가 태그로 찾는다.</summary>
        public bool TryGetTagCorners(MerchantItem item, out Sprite topRight, out Sprite bottomLeft)
        {
            topRight = null;
            bottomLeft = null;
            return false;
        }

        /// <summary>확인용 상품은 태그 자료가 없어 기호도 얹지 않는다.</summary>
        public bool TryGetTagGlyphs(MerchantItem item, out Sprite topRight, out Sprite bottomLeft)
        {
            topRight = null;
            bottomLeft = null;
            return false;
        }

        /// <summary>확인용 상품은 기호를 얹지 않는다.</summary>
        public float CornerGlyphSizeRatio
        {
            get { return 0f; }
        }

        /// <summary>확인용 상품은 기호를 얹지 않는다.</summary>
        public float CornerGlyphFromCorner
        {
            get { return 0f; }
        }

        /// <summary>확인용 상품은 테두리를 얹지 않아 들일 일이 없다.</summary>
        public float SymbolFrameInset
        {
            get { return 0f; }
        }
    }
}
