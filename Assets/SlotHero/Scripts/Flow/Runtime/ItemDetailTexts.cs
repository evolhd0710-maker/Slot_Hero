using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.Popup;
using SlotHero.Sanctum;

namespace SlotHero.Flow
{
    /// <summary>
    /// 물건 식별자로 아이템 상세 오버레이에 적을 것을 만든다.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 아이템 상세 팝업 구성이다.
    ///
    /// 이름은 좌상단, 가치 값은 우상단, 내용은 그 아래, 플레이버 텍스트는 하단이다.
    /// 기획서가 "문양은 현재 가치를, 유물과 코인은 희귀도를" 우상단에 적게 했다.
    ///
    /// 유물은 `유물 정보시트.xlsx` 의 소개문과 플레이버 텍스트를 적는다(`RelicConfig`). 2026년 10월 25일.
    /// 문양과 코인의 효과 설명은 아직 없다. 문양은 태그 둘, 코인은 종류만 적는다.
    /// </summary>
    public static class ItemDetailTexts
    {
        /// <summary>그 식별자의 오버레이 내용. 목록에 없으면 비어 있는 내용을 돌려준다.</summary>
        public static ItemDetailSpec Describe(string id, RunCatalogConfig config, CurrentBuildVisualConfig tagNames)
        {
            return Describe(id, config, tagNames, null, 0);
        }

        /// <summary>
        /// 유물이면 소개문과 플레이버 텍스트까지 적는다.
        /// 쌓인 횟수는 12개의 금화처럼 고유 수치를 쓰는 유물이 지금 값을 적는 데 쓴다.
        /// </summary>
        public static ItemDetailSpec Describe(
            string id, RunCatalogConfig config, CurrentBuildVisualConfig tagNames, RelicConfig relics, int relicStack)
        {
            if (config == null || string.IsNullOrEmpty(id))
            {
                return new ItemDetailSpec();
            }

            for (int i = 0; i < config.Symbols.Count; i++)
            {
                SymbolDefinition symbol = config.Symbols[i];
                if (symbol.Id != id)
                {
                    continue;
                }

                string value = "가치 " + config.GetTagValueSum(symbol);
                ItemDetailSpec spec = new ItemDetailSpec(symbol.DisplayName, value, DescribeTags(symbol, tagNames), string.Empty);

                // 내용이 태그 둘뿐이라 내용 글자 전체에 태그 테두리를 두른다(2026년 10월 25일 원재).
                if (tagNames != null)
                {
                    spec.BodyOutlineColor = tagNames.TagOutlineColor;
                    spec.BodyOutlineWidth = tagNames.TagOutlineWidth;
                }

                return spec;
            }

            for (int i = 0; i < config.Coins.Count; i++)
            {
                if (config.Coins[i].Id == id)
                {
                    return new ItemDetailSpec(config.Coins[i].DisplayName, GetRarityName(config.Coins[i].Rarity), "코인", string.Empty);
                }
            }

            for (int i = 0; i < config.Relics.Count; i++)
            {
                if (config.Relics[i].Id == id)
                {
                    RelicDefinition relic = relics != null ? relics.Find(id) : null;
                    string body = relic != null ? relic.GetDescription(relicStack) : "유물";
                    string flavor = relic != null ? relic.Flavor : string.Empty;
                    return new ItemDetailSpec(config.Relics[i].DisplayName, GetRarityName(config.Relics[i].Rarity), body, flavor);
                }
            }

            return new ItemDetailSpec();
        }

        /// <summary>
        /// 문양의 태그 둘. "태그" 라는 머리말 없이 둘만 적고, 태그마다 제 색으로 굵고 크게 적고 흰 테두리를 두른다(2026년 10월 25일 원재).
        /// 크기 배율은 `CurrentBuildVisualConfig.DetailTagSizePercent` 다. 설정이 없으면 이름만 적는다.
        /// </summary>
        public static string DescribeTags(SymbolDefinition symbol, CurrentBuildVisualConfig tagNames)
        {
            string tags = SymbolTags.GetColoredName(symbol.FirstTag, tagNames) + " · " + SymbolTags.GetColoredName(symbol.SecondTag, tagNames);
            if (tagNames == null)
            {
                return tags;
            }

            string size = tagNames.DetailTagSizePercent.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
            return "<b><size=" + size + "%>" + tags + "</size></b>";
        }

        /// <summary>등급 이름. 성소 기획서 07 가격 의 표 이름이다.</summary>
        public static string GetRarityName(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return "일반";
                case ItemRarity.Uncommon:
                    return "고급";
                case ItemRarity.Rare:
                    return "희귀";
                case ItemRarity.Epic:
                    return "특급";
                case ItemRarity.Legend:
                    return "전설";
            }

            return string.Empty;
        }
    }
}
