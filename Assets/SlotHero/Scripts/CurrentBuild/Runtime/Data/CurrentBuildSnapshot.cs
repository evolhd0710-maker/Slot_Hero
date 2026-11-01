using System;
using System.Collections.Generic;
using SlotHero.Combat;

namespace SlotHero.CurrentBuild
{
    /// <summary>
    /// 칸 하나에 놓을 것.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 유물, 코인, 문양 칸이 모두 이 자료를 쓴다.
    ///
    /// 무엇을 가지고 있는지는 유물, 코인, 문양 기획서 소관이라
    /// 화면에 그릴 식별자와 이름, 개수만 받는다.
    /// 그림은 식별자로 <see cref="UI.IBuildIconSource"/>에서 찾아온다.
    /// </summary>
    [Serializable]
    public struct BuildEntry
    {
        /// <summary>원본 데이터의 식별자.</summary>
        public string Id;

        /// <summary>화면에 띄우는 이름. 이름 순 정렬에 쓴다.</summary>
        public string DisplayName;

        /// <summary>가지고 있는 개수. 문양만 1보다 커질 수 있다.</summary>
        public int Count;

        /// <summary>
        /// 문양이 가진 첫 태그. 태그 순 정렬의 기준이 된다.
        /// 유물과 코인은 쓰지 않는다.
        /// </summary>
        public SymbolTagType FirstTag;

        /// <summary>
        /// 문양이 가진 둘째 태그.
        /// 문양 하나는 서로 다른 태그를 둘 가진다.
        /// </summary>
        public SymbolTagType SecondTag;

        /// <summary>칸에 놓을 것이 들어 있는지.</summary>
        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(Id); }
        }

        /// <summary>그 태그를 가지고 있는지. 두 자리 중 하나라도 맞으면 그렇다.</summary>
        public bool HasTag(SymbolTagType tag)
        {
            return FirstTag == tag || SecondTag == tag;
        }

        /// <summary>두 태그가 서로 다른지. 문양은 늘 달라야 한다.</summary>
        public bool HasTwoDifferentTags
        {
            get { return FirstTag != SecondTag; }
        }

        public static BuildEntry Item(string id, string displayName)
        {
            BuildEntry entry = new BuildEntry();
            entry.Id = id;
            entry.DisplayName = displayName;
            entry.Count = 1;
            return entry;
        }

        /// <summary>문양 하나. 태그는 서로 다른 둘을 받는다.</summary>
        public static BuildEntry Symbol(
            string id, string displayName, int count, SymbolTagType firstTag, SymbolTagType secondTag)
        {
            BuildEntry entry = new BuildEntry();
            entry.Id = id;
            entry.DisplayName = displayName;
            entry.Count = count;
            entry.FirstTag = firstTag;
            entry.SecondTag = secondTag;
            return entry;
        }
    }

    /// <summary>
    /// 현재 빌드 화면에 한 번에 그릴 것 전부.
    /// 유물, 코인, 문양을 담는다.
    ///
    /// 무엇을 가지고 있는지 모으는 일은 아이템 쪽이 맡고
    /// 이 화면은 받은 것을 그리기만 한다.
    ///
    /// 태그 통계는 따로 받지 않고 문양에서 바로 센다.
    /// 문양과 통계가 어긋날 자리를 없애려는 것이다.
    /// </summary>
    [Serializable]
    public class CurrentBuildSnapshot
    {
        /// <summary>1열에 놓을 유물. 획득한 차례대로 담는다.</summary>
        public List<BuildEntry> Relics = new List<BuildEntry>();

        /// <summary>2열에 놓을 코인.</summary>
        public List<BuildEntry> Coins = new List<BuildEntry>();

        /// <summary>3열부터 놓을 문양. 가진 종류만 담는다.</summary>
        public List<BuildEntry> Symbols = new List<BuildEntry>();

        /// <summary>
        /// 코인을 최대 몇 개까지 가질 수 있는지. 현재 빌드 화면이 코인 칸을 이만큼 놓는다.
        /// 음수면 정하지 않은 것이라 설정의 칸 수(`CurrentBuildLayoutConfig.CoinSlotCount`)를 쓴다. 0 이면 칸이 없다.
        /// 소지 한도는 조건에 따라 바뀔 수 있다(2026년 10월 9일 원재).
        /// </summary>
        public int CoinCapacity = -1;

        /// <summary>유물을 최대 몇 개까지 가질 수 있는지. 음수면 설정의 칸 수를 쓴다.</summary>
        public int RelicCapacity = -1;

        /// <summary>
        /// 가진 문양이 모두 몇 장인지. 태그 막대의 기준이 된다.
        /// 종류마다의 개수를 모두 더한 값이다.
        /// </summary>
        public int TotalSymbolCount
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < Symbols.Count; i++)
                {
                    sum += Symbols[i].Count;
                }

                return sum;
            }
        }

        /// <summary>
        /// 그 태그를 가진 문양이 몇 장인지.
        /// 문양 하나가 태그를 둘 가지므로 한 장이 두 태그에 함께 센다.
        /// </summary>
        public int GetTagCount(SymbolTagType tag)
        {
            int sum = 0;

            for (int i = 0; i < Symbols.Count; i++)
            {
                if (Symbols[i].IsValid && Symbols[i].HasTag(tag))
                {
                    sum += Symbols[i].Count;
                }
            }

            return sum;
        }

        /// <summary>
        /// 태그 막대의 길이 비율. 그 태그를 가진 문양이 전체 문양 중 얼마인지다.
        /// 화면 예시로 보면 문양 18장에 토성 6장이면 막대가 300 중 100이 된다.
        ///
        /// 문양 하나가 태그를 둘 가지므로 아홉 막대의 합은 2까지 갈 수 있다.
        /// 막대 하나는 1을 넘지 않는다.
        /// </summary>
        public float GetTagRatio(SymbolTagType tag)
        {
            int total = TotalSymbolCount;
            if (total <= 0)
            {
                return 0f;
            }

            float ratio = (float)GetTagCount(tag) / total;
            if (ratio < 0f)
            {
                return 0f;
            }

            return ratio > 1f ? 1f : ratio;
        }

        /// <summary>
        /// 태그 9종의 장수를 태양계 차례로 만든다.
        /// 문양에서 바로 세므로 따로 채워 넣을 것이 없다.
        /// </summary>
        public List<BuildTagStat> GetTagStats()
        {
            IReadOnlyList<SymbolTagType> all = SymbolTags.InOrder;
            List<BuildTagStat> stats = new List<BuildTagStat>(all.Count);

            for (int i = 0; i < all.Count; i++)
            {
                stats.Add(new BuildTagStat(all[i], GetTagCount(all[i])));
            }

            return stats;
        }

        /// <summary>문양마다 태그 둘이 서로 다른지. 하나라도 같으면 자료가 잘못된 것이다.</summary>
        public bool AllSymbolsHaveTwoDifferentTags()
        {
            for (int i = 0; i < Symbols.Count; i++)
            {
                if (Symbols[i].IsValid && !Symbols[i].HasTwoDifferentTags)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
