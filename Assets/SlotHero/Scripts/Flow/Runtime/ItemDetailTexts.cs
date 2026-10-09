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
    /// **아이템마다의 효과 설명과 플레이버 텍스트는 아직 없다.**
    /// 문양, 코인, 유물 기획서 소관인데 그 문서와 저장소 에셋이 오지 않았다.
    /// 지금은 목록에 있는 것만 적는다. 문양은 태그 둘, 코인과 유물은 종류다.
    /// </summary>
    public static class ItemDetailTexts
    {
        /// <summary>그 식별자의 오버레이 내용. 목록에 없으면 비어 있는 내용을 돌려준다.</summary>
        public static ItemDetailSpec Describe(string id, RunCatalogConfig config, CurrentBuildVisualConfig tagNames)
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
                string body = "태그  " + GetTagName(symbol.FirstTag, tagNames) + " · " + GetTagName(symbol.SecondTag, tagNames);
                return new ItemDetailSpec(symbol.DisplayName, value, body, string.Empty);
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
                    return new ItemDetailSpec(config.Relics[i].DisplayName, GetRarityName(config.Relics[i].Rarity), "유물", string.Empty);
                }
            }

            return new ItemDetailSpec();
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

        /// <summary>현재 빌드 화면과 같은 태그 이름. 설정이 없으면 영어 이름이다.</summary>
        private static string GetTagName(SymbolTagType tag, CurrentBuildVisualConfig tagNames)
        {
            if (tagNames == null)
            {
                return tag.ToString();
            }

            string name = tagNames.GetTagVisual(tag).DisplayName;
            return string.IsNullOrEmpty(name) ? tag.ToString() : name;
        }
    }
}
