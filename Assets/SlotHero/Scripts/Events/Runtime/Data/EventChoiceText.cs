using System;
using System.Text;

namespace SlotHero.Events
{
    /// <summary>
    /// 선택지 칸에 적을 글을 만드는 규칙.
    /// 색은 글자 안에 태그로 넣으므로 글자 하나로 칸을 다 채울 수 있다.
    /// </summary>
    [Serializable]
    public struct EventChoiceTextFormat
    {
        /// <summary>선택지 문구와 결과 사이에 넣는 글자.</summary>
        public string Separator;

        /// <summary>결과가 여러 줄일 때 사이에 넣는 글자.</summary>
        public string LineSeparator;

        /// <summary>결과를 알 수 없는 선택지에 적는 글자.</summary>
        public string HiddenMark;

        /// <summary>필요한 항목을 감싸는 꼴. {0} 자리에 내용이 들어간다.</summary>
        public string RequirementFormat;

        /// <summary>얻는 것에 입히는 색. RRGGBB 여섯 자리.</summary>
        public string GainColorHex;

        /// <summary>잃거나 치르는 것에 입히는 색. RRGGBB 여섯 자리.</summary>
        public string CostColorHex;

        /// <summary>필요한 것을 갖췄을 때 입히는 색. RRGGBB 여섯 자리.</summary>
        public string RequirementMetColorHex;

        /// <summary>필요한 것을 갖추지 못했을 때 입히는 색. RRGGBB 여섯 자리.</summary>
        public string RequirementUnmetColorHex;

        /// <summary>기획서 예시와 같은 기본 꼴.</summary>
        public static EventChoiceTextFormat Default()
        {
            EventChoiceTextFormat format = new EventChoiceTextFormat();
            format.Separator = "  —  ";
            format.LineSeparator = ", ";
            format.HiddenMark = "?";
            format.RequirementFormat = " [{0}]";
            format.GainColorHex = "1E8A3C";
            format.CostColorHex = "C0392B";
            format.RequirementMetColorHex = "1E8A3C";
            format.RequirementUnmetColorHex = "C0392B";
            return format;
        }
    }

    /// <summary>
    /// 선택지 칸에 적을 글을 만든다.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 선택지 표시 세 가지를 그대로 따른다.
    ///
    /// 그 선택지를 고르는 데 필요한 것은 선택지 문구 바로 뒤에 괄호로 적고,
    /// 가지고 있으면 초록색, 가지고 있지 않으면 빨간색으로 보여 준다.
    ///
    /// 기획서 예시로 보면 이런 꼴이 나온다.
    /// 얻는 것은 초록색, 잃거나 치르는 것은 빨간색으로 적는다.
    ///
    /// 결과가 공개된 선택지 · 산책할까?  —  체력 5 증가
    /// 필요한 것을 갖추지 못한 선택지 · 간식 먹을래? [간식]  —  유물 획득 (괄호가 빨강)
    /// 필요한 것을 갖춘 선택지 · 간식 먹을래? [간식]  —  유물 획득 (괄호가 초록)
    /// 결과를 알 수 없는 선택지 · 쫓아가기  —  ?
    /// </summary>
    public static class EventChoiceText
    {
        // 결과 문장에서 얻는 것과 잃는 것을 감싸는 표시.
        // 결과 문장은 런 쪽(`IEventWorld.Apply`)이 짓는데 그쪽은 화면 색을 모른다.
        // 그래서 표시만 넣어 보내고 화면이 그릴 때 `ColorMarks` 로 색을 입힌다.
        private const string GainOpen = "<gain>";
        private const string GainClose = "</gain>";
        private const string CostOpen = "<cost>";
        private const string CostClose = "</cost>";

        /// <summary>결과 문장 안의 얻은 것을 표시한다. 화면에서 초록으로 나온다.</summary>
        public static string MarkGain(string text)
        {
            return GainOpen + text + GainClose;
        }

        /// <summary>결과 문장 안의 잃거나 치른 것을 표시한다. 화면에서 빨강으로 나온다.</summary>
        public static string MarkCost(string text)
        {
            return CostOpen + text + CostClose;
        }

        /// <summary>표시를 색으로 바꾼다. 색이 비어 있으면 표시만 지운다.</summary>
        public static string ColorMarks(string text, EventChoiceTextFormat format)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('<') < 0)
            {
                return text;
            }

            string gainOpen = string.IsNullOrEmpty(format.GainColorHex) ? string.Empty : "<color=#" + format.GainColorHex + ">";
            string costOpen = string.IsNullOrEmpty(format.CostColorHex) ? string.Empty : "<color=#" + format.CostColorHex + ">";

            return text
                .Replace(GainOpen, gainOpen)
                .Replace(GainClose, gainOpen.Length > 0 ? "</color>" : string.Empty)
                .Replace(CostOpen, costOpen)
                .Replace(CostClose, costOpen.Length > 0 ? "</color>" : string.Empty);
        }

        /// <summary>표시를 지운 맨글. 검사와 글 길이 재기에 쓴다.</summary>
        public static string StripMarks(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return text.Replace(GainOpen, string.Empty).Replace(GainClose, string.Empty)
                .Replace(CostOpen, string.Empty).Replace(CostClose, string.Empty);
        }

        public static string Build(EventChoice choice, EventChoiceTextFormat format)
        {
            if (choice == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(choice.Label);

            // 필요한 것은 선택지 문구 바로 뒤에 붙여 무엇이 있어야 하는지 먼저 보이게 한다.
            AppendRequirements(builder, choice, format);

            // 결과를 알 수 없는 선택지도 **알고 치르는 것은 적는다.**
            // 예전에는 물음표만 적어 34 도박 의 건 골드, 20 환전 의 10골드가 보이지 않았다.
            bool any = AppendResult(builder, choice, format);

            if (choice.ResultHidden)
            {
                builder.Append(any ? format.LineSeparator : format.Separator);
                builder.Append(format.HiddenMark);
            }

            return builder.ToString();
        }

        private static void AppendRequirements(
            StringBuilder builder,
            EventChoice choice,
            EventChoiceTextFormat format)
        {
            if (choice.Lines == null)
            {
                return;
            }

            for (int i = 0; i < choice.Lines.Count; i++)
            {
                EventEffectLine line = choice.Lines[i];
                if (line.Kind != EventEffectKind.Requirement)
                {
                    continue;
                }

                string wrapped = string.IsNullOrEmpty(format.RequirementFormat)
                    ? line.Text
                    : string.Format(format.RequirementFormat, line.Text);

                string colorHex = line.Met
                    ? format.RequirementMetColorHex
                    : format.RequirementUnmetColorHex;

                builder.Append(Colored(wrapped, colorHex));
            }
        }

        /// <summary>얻고 잃는 줄을 붙인다. 하나라도 붙였으면 true.</summary>
        private static bool AppendResult(StringBuilder builder, EventChoice choice, EventChoiceTextFormat format)
        {
            if (choice.Lines == null)
            {
                return false;
            }

            bool first = true;

            for (int i = 0; i < choice.Lines.Count; i++)
            {
                EventEffectLine line = choice.Lines[i];
                if (line.Kind == EventEffectKind.Requirement)
                {
                    continue;
                }

                if (first)
                {
                    builder.Append(format.Separator);
                    first = false;
                }
                else
                {
                    builder.Append(format.LineSeparator);
                }

                // 얻는 것은 초록, 잃거나 치르는 것은 빨강이다.
                // 예전에는 잃는 것을 색 없이 적어, 빨강은 필요한 것이 모자랄 때만 나왔다.
                builder.Append(line.Kind == EventEffectKind.Gain
                    ? Colored(line.Text, format.GainColorHex)
                    : Colored(line.Text, format.CostColorHex));
            }

            return !first;
        }

        private static string Colored(string text, string colorHex)
        {
            if (string.IsNullOrEmpty(colorHex))
            {
                return text;
            }

            return "<color=#" + colorHex + ">" + text + "</color>";
        }
    }
}
