using System;
using System.Collections.Generic;
using SlotHero.Combat;

namespace SlotHero.CurrentBuild
{
    /// <summary>
    /// 태그 하나의 장수.
    /// 태그 막대 한 줄을 그리는 데 쓴다.
    /// </summary>
    [Serializable]
    public struct BuildTagStat
    {
        public SymbolTagType Tag;

        /// <summary>그 태그를 가진 문양이 몇 장인지.</summary>
        public int Count;

        public BuildTagStat(SymbolTagType tag, int count)
        {
            Tag = tag;
            Count = count;
        }
    }

    /// <summary>
    /// 태그 9종을 다루는 데 쓰는 값.
    ///
    /// 태그 자체는 전투 쪽의 <see cref="SymbolTagType"/> 을 쓴다.
    /// 문양 자료가 그것을 물고 있어 이름을 두 벌 두면 어긋나기 때문이다.
    /// 여기에는 차례와 개수만 둔다.
    ///
    /// 차례는 태양계 순서다.
    /// </summary>
    public static class SymbolTags
    {
        /// <summary>태그 종류 수. 기획서 기준 9종.</summary>
        public const int Count = 9;

        /// 태그 9종을 태양계 차례로. 한 번만 만들어 둔다.
        private static readonly SymbolTagType[] Ordered =
        {
            SymbolTagType.Mercury,
            SymbolTagType.Venus,
            SymbolTagType.Earth,
            SymbolTagType.Mars,
            SymbolTagType.Jupiter,
            SymbolTagType.Saturn,
            SymbolTagType.Uranus,
            SymbolTagType.Neptune,
            SymbolTagType.Pluto,
        };

        /// <summary>
        /// 태그 9종을 태양계 차례로. 한 번 만든 목록을 고칠 수 없게 돌려준다. 읽기만 하는 곳은 이것을 쓴다.
        /// 예전에는 부를 때마다 새 배열을 만들었다. 2026년 10월 9일 외부 검토가 짚었다.
        /// </summary>
        public static IReadOnlyList<SymbolTagType> InOrder
        {
            get { return ReadOnlyOrdered; }
        }

        private static readonly System.Collections.ObjectModel.ReadOnlyCollection<SymbolTagType> ReadOnlyOrdered =
            Array.AsReadOnly(Ordered);

        /// <summary>태그 9종을 태양계 차례로 담은 새 배열. 받은 쪽이 고쳐 써도 되는 복사본이다.</summary>
        public static SymbolTagType[] All()
        {
            return (SymbolTagType[])Ordered.Clone();
        }

        /// <summary>
        /// 화면에 적는 태그 이름. 현재 빌드 화면과 같은 이름(`CurrentBuildVisualConfig`)을 쓴다.
        /// 설정이 없거나 이름이 비어 있으면 영어 이름이다.
        /// 보상 카드, 런 종료 결과, 아이템 상세, 이벤트가 같은 일을 따로 하던 것을 2026년 10월 18일 검토에서 여기로 모았다.
        /// </summary>
        public static string GetName(SymbolTagType tag, CurrentBuildVisualConfig names)
        {
            if (names == null)
            {
                return tag.ToString();
            }

            string name = names.GetTagVisual(tag).DisplayName;
            return string.IsNullOrEmpty(name) ? tag.ToString() : name;
        }

        /// <summary>
        /// 그 태그의 글자 색(`BuildTagVisual.TextColor`)을 입힌 태그 이름. 글자 칸의 서식 문법(rich text)으로 적는다.
        /// 보상 카드, 아이템 상세, 런 종료 결과처럼 태그를 글로 적는 곳은 태그마다 제 색으로 적는다(2026년 10월 25일 원재).
        /// 설정이 없으면 색 없이 이름만 돌려준다.
        /// </summary>
        public static string GetColoredName(SymbolTagType tag, CurrentBuildVisualConfig names)
        {
            string name = GetName(tag, names);
            if (names == null)
            {
                return name;
            }

            return "<color=#" + ToHex(names.GetTagVisual(tag).TextColor) + ">" + name + "</color>";
        }

        /// <summary>색을 RRGGBB 열여섯 진수로 적는다. 기기 문화권과 상관없이 같은 글이 나온다.</summary>
        public static string ToHex(UnityEngine.Color color)
        {
            return Channel(color.r) + Channel(color.g) + Channel(color.b);
        }

        private static string Channel(float value)
        {
            int byteValue = (int)Math.Round(Math.Max(0f, Math.Min(1f, value)) * 255f, MidpointRounding.AwayFromZero);
            return byteValue.ToString("X2", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
