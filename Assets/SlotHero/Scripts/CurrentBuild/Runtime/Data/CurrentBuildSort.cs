using System.Collections.Generic;

namespace SlotHero.CurrentBuild
{
    /// <summary>
    /// 문양과 태그를 늘어놓는 차례를 정한다.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의
    /// "개수 순, 태그 순, 이름 순 등의 정렬을 지원한다"를 따른다.
    ///
    /// 어느 차례든 같은 값이 겹치면 이름으로 마저 가른다.
    /// 그래야 같은 빌드를 다시 열어도 늘 같은 자리에 놓인다.
    /// </summary>
    public static class CurrentBuildSort
    {
        /// <summary>문양을 고른 차례대로 늘어놓는다.</summary>
        public static void SortSymbols(List<BuildEntry> symbols, BuildSortOrder order)
        {
            if (symbols == null)
            {
                return;
            }

            symbols.Sort(delegate (BuildEntry left, BuildEntry right)
            {
                return CompareEntries(left, right, order);
            });
        }

        /// <summary>태그 통계를 고른 차례대로 늘어놓는다.</summary>
        public static void SortTagStats(List<BuildTagStat> stats, BuildSortOrder order)
        {
            if (stats == null)
            {
                return;
            }

            stats.Sort(delegate (BuildTagStat left, BuildTagStat right)
            {
                // 이름 순은 태그에 쓸 이름이 따로 없으므로 태그 차례로 본다.
                if (order == BuildSortOrder.Count)
                {
                    int byCount = right.Count.CompareTo(left.Count);
                    if (byCount != 0)
                    {
                        return byCount;
                    }
                }

                return ((int)left.Tag).CompareTo((int)right.Tag);
            });
        }

        private static int CompareEntries(BuildEntry left, BuildEntry right, BuildSortOrder order)
        {
            switch (order)
            {
                case BuildSortOrder.Count:
                {
                    int byCount = right.Count.CompareTo(left.Count);
                    if (byCount != 0)
                    {
                        return byCount;
                    }

                    break;
                }

                case BuildSortOrder.Tag:
                {
                    // 문양 하나가 태그를 둘 가지므로 첫 태그로 먼저 가르고
                    // 그것이 같으면 둘째 태그로 마저 가른다.
                    int byFirst = ((int)left.FirstTag).CompareTo((int)right.FirstTag);
                    if (byFirst != 0)
                    {
                        return byFirst;
                    }

                    int bySecond = ((int)left.SecondTag).CompareTo((int)right.SecondTag);
                    if (bySecond != 0)
                    {
                        return bySecond;
                    }

                    int byCount = right.Count.CompareTo(left.Count);
                    if (byCount != 0)
                    {
                        return byCount;
                    }

                    break;
                }
            }

            return CompareName(left, right);
        }

        private static int CompareName(BuildEntry left, BuildEntry right)
        {
            string leftName = left.DisplayName ?? string.Empty;
            string rightName = right.DisplayName ?? string.Empty;

            int byName = string.CompareOrdinal(leftName, rightName);
            if (byName != 0)
            {
                return byName;
            }

            return string.CompareOrdinal(left.Id ?? string.Empty, right.Id ?? string.Empty);
        }
    }
}
