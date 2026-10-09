using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SlotHero.CurrentBuild;
using SlotHero.Combat;

namespace SlotHero.RunResult
{
    /// <summary>
    /// 런 종료 결과 화면에 적을 글을 만든다.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의 와이어프레임 예시를 그대로 낼 수 있게 맞췄다.
    ///
    /// 칸 하나는 글상자 하나로 그린다.
    /// 줄마다 글상자를 두지 않은 것은 와이어프레임에서 긴 줄이 저절로 접혀
    /// 두 줄이 되면서도 줄 간격을 그대로 지키기 때문이다.
    /// 접는 일은 TextMeshPro 에 맡기고 여기서는 줄을 글 하나로 잇기만 한다.
    /// </summary>
    public static class RunResultText
    {
        /// <summary>항목들을 구분자로 잇는다. 빈 항목은 건너뛴다.</summary>
        public static string Join(IList<string> parts, string separator)
        {
            if (parts == null || parts.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < parts.Count; i++)
            {
                if (string.IsNullOrEmpty(parts[i]))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(parts[i]);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 도달 지점 한 줄.
        /// 기획서의 "스테이지와 단계, 마지막으로 상대한 몬스터를 표시한다"에 해당한다.
        /// 와이어프레임 예시는 "스테이지 1 · 9번째 방 · 엘리트에게 쓰러짐"이다.
        /// </summary>
        public static string Location(RunResultVisualConfig visual, int stage, int room, string ending)
        {
            if (visual == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            parts.Add(string.Format(CultureInfo.InvariantCulture, visual.StageFormat, stage));
            parts.Add(string.Format(CultureInfo.InvariantCulture, visual.RoomFormat, room));

            if (!string.IsNullOrEmpty(ending))
            {
                parts.Add(ending);
            }

            return Join(parts, visual.Separator);
        }

        /// <summary>
        /// 최종 구성 줄을 만든다.
        /// 기획서의 "마지막 시점에서 보유한 아이템을 간략하게 표시한다"에 해당한다.
        ///
        /// 현재 빌드 화면이 쓰는 자료를 그대로 받는다.
        /// 태그 이름은 한곳에만 두려고 현재 빌드 화면의 표시 설정에서 가져온다.
        /// </summary>
        public static List<ResultLine> BuildFinalBuild(
            CurrentBuildSnapshot snapshot,
            CurrentBuildVisualConfig build,
            RunResultVisualConfig visual)
        {
            List<ResultLine> lines = new List<ResultLine>();
            if (snapshot == null || visual == null)
            {
                return lines;
            }

            AddSymbolSection(lines, snapshot, build, visual);
            AddEntrySection(lines, visual.RelicHeadingText, snapshot.Relics, visual.Separator);
            AddEntrySection(lines, visual.CoinHeadingText, snapshot.Coins, visual.Separator);

            return lines;
        }

        /// <summary>
        /// 줄 목록을 글상자 하나에 넣을 글로 바꾼다.
        /// 소제목은 굵게 하고 글자 크기를 올린다.
        /// 본문 크기는 글상자 자체에 두므로 여기서 적지 않는다.
        /// </summary>
        public static string ToRichText(IList<ResultLine> lines, RunResultVisualConfig visual)
        {
            if (lines == null || lines.Count == 0)
            {
                return string.Empty;
            }

            string size = visual != null
                ? visual.HeadingFontSize.ToString("0.##", CultureInfo.InvariantCulture)
                : "26";

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                ResultLine line = lines[i];
                if (line.IsBlank)
                {
                    continue;
                }

                if (line.Strong)
                {
                    builder.Append("<b><size=").Append(size).Append('>');
                    builder.Append(line.Text);
                    builder.Append("</size></b>");
                }
                else
                {
                    builder.Append(line.Text);
                }
            }

            return builder.ToString();
        }

        /// <summary>문양과 태그를 한 묶음으로 넣는다. 와이어프레임에서 둘 사이에 빈 줄이 없다.</summary>
        private static void AddSymbolSection(
            List<ResultLine> lines,
            CurrentBuildSnapshot snapshot,
            CurrentBuildVisualConfig build,
            RunResultVisualConfig visual)
        {
            List<string> symbols = new List<string>();
            for (int i = 0; i < snapshot.Symbols.Count; i++)
            {
                BuildEntry entry = snapshot.Symbols[i];
                if (!entry.IsValid)
                {
                    continue;
                }

                symbols.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    visual.SymbolEntryFormat,
                    entry.DisplayName,
                    entry.Count));
            }

            if (symbols.Count == 0)
            {
                return;
            }

            lines.Add(ResultLine.Heading(string.Format(
                CultureInfo.InvariantCulture,
                visual.SymbolHeadingFormat,
                snapshot.TotalSymbolCount)));
            lines.Add(ResultLine.Body(Join(symbols, visual.Separator)));

            List<string> tags = new List<string>();
            IReadOnlyList<SymbolTagType> all = SymbolTags.InOrder;
            for (int i = 0; i < all.Count; i++)
            {
                int count = snapshot.GetTagCount(all[i]);
                if (count <= 0)
                {
                    // 0장인 태그는 적지 않는다. 간략하게 표시하라는 기획서에 맞춘 것이다.
                    continue;
                }

                string name = GetTagName(build, all[i]);
                tags.Add(string.Format(CultureInfo.InvariantCulture, visual.TagEntryFormat, name, count));
            }

            if (tags.Count == 0)
            {
                return;
            }

            lines.Add(ResultLine.Heading(visual.TagHeadingText));
            lines.Add(ResultLine.Body(Join(tags, visual.Separator)));
        }

        /// <summary>유물이나 코인처럼 이름만 잇는 묶음을 넣는다.</summary>
        private static void AddEntrySection(
            List<ResultLine> lines,
            string heading,
            List<BuildEntry> entries,
            string separator)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            List<string> names = new List<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsValid)
                {
                    names.Add(entries[i].DisplayName);
                }
            }

            if (names.Count == 0)
            {
                return;
            }

            if (lines.Count > 0)
            {
                // 묶음 사이에 빈 줄 하나를 넣는다. 와이어프레임과 같다.
                lines.Add(ResultLine.Blank());
            }

            lines.Add(ResultLine.Heading(heading));
            lines.Add(ResultLine.Body(Join(names, separator)));
        }

        /// <summary>태그 이름. 표시 설정이 없으면 열거형 이름을 그대로 쓴다.</summary>
        private static string GetTagName(CurrentBuildVisualConfig build, SymbolTagType tag)
        {
            if (build == null)
            {
                return tag.ToString();
            }

            BuildTagVisual visual = build.GetTagVisual(tag);
            return string.IsNullOrEmpty(visual.DisplayName) ? tag.ToString() : visual.DisplayName;
        }
    }
}
