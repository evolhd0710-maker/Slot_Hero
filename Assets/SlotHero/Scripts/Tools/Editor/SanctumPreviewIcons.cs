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
    /// 문양 그림 서른여섯 장이 저장소에서 오면 이 파일은 지운다.
    /// </summary>
    public class SanctumPreviewIcons : IMerchantIconSource
    {
        /// 임시 상품 식별자와 지금 가진 그림 이름의 짝.
        /// 없는 것은 비워 두면 그 자리는 그림 없이 가격만 나온다.
        private static readonly string[] Names =
        {
            "symbol_skull", "문양_해골",
            "symbol_cloud", "문양_구름",
            "symbol_sword", "문양_검",
            "symbol_clock", "문양_시계",
            "relic_book", "relic_book",
            "relic_wing", "relic_wing",
            "relic_pickaxe", "relic_pick",
        };

        /// 짝이 없는 임시 상품에 돌려 쓰는 그림. 종류마다 다르다.
        private static readonly string[] SymbolFallback =
        {
            "문양_해골", "문양_구름", "문양_검", "문양_시계",
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

            for (int i = 0; i + 1 < Names.Length; i += 2)
            {
                if (Names[i] == item.Id)
                {
                    return ImportArt.LoadItemArt(Names[i + 1]);
                }
            }

            // 짝이 없는 임시 상품은 같은 종류의 그림을 돌려 쓴다.
            // 빈 칸보다는 자리와 크기를 눈으로 보는 편이 낫다.
            string[] pool = SymbolFallback;
            if (item.Id.StartsWith("coin_")) pool = CoinFallback;
            else if (item.Id.StartsWith("relic_")) pool = RelicFallback;

            // 글자 코드를 더해 고른다. `GetHashCode` 는 값이 몰려 같은 그림만 나왔다.
            int sum = 0;
            for (int i = 0; i < item.Id.Length; i++)
            {
                sum += item.Id[i] * (i + 1);
            }

            return ImportArt.LoadItemArt(pool[Mathf.Abs(sum) % pool.Length]);
        }
    }
}
