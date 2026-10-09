using System.Globalization;
using SlotHero.CurrentBuild;

namespace SlotHero.Reward
{
    /// <summary>
    /// 보상 화면에 적는 글을 만든다.
    ///
    /// 태그 이름은 현재 빌드 화면의 표시 설정에서 가져온다.
    /// 이름을 두 곳에 두지 않으려는 것이다. 런 종료 결과 화면도 같은 방식이다.
    /// </summary>
    public static class RewardText
    {
        /// <summary>골드 액수. 아이콘은 글이 아니라 그림으로 따로 붙는다.</summary>
        public static string Gold(int amount)
        {
            return amount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 카드 아래에 적는 태그 줄.
        /// 문양이 아니면 빈 글이다.
        /// </summary>
        public static string Tags(RewardCard card, CurrentBuildVisualConfig visual)
        {
            if (card == null || !card.HasTags)
            {
                return string.Empty;
            }

            string first = GetTagName(card.FirstTag, visual);
            string second = GetTagName(card.SecondTag, visual);

            return first + " " + second;
        }

        private static string GetTagName(
            SlotHero.Combat.SymbolTagType tag, CurrentBuildVisualConfig visual)
        {
            if (visual == null)
            {
                return tag.ToString();
            }

            BuildTagVisual found = visual.GetTagVisual(tag);
            return string.IsNullOrEmpty(found.DisplayName) ? tag.ToString() : found.DisplayName;
        }
    }
}
