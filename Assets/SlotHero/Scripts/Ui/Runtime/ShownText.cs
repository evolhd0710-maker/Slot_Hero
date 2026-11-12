using System;
using TMPro;
using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 글상자에 실제로 그려질 글로 크기를 잰다.
    ///
    /// 영어 설정은 글상자의 `text` 를 한국어로 두고 그리기 직전에만 영어로 바꾼다(`TextLocalizer`, 글자 전처리).
    /// 그런데 TMP 의 `GetPreferredValues(글, 폭, 높이)` 는 넘긴 글을 전처리 없이 잰다.
    /// 그래서 한국어 길이로 칸을 잡고, 더 긴 영어가 좁은 칸에서 줄이 바뀌거나 칸을 넘쳤다(2026년 11월 10일 원재).
    /// 글에 맞춰 칸 크기를 정하는 곳은 이것으로 잰다.
    /// </summary>
    public static class ShownText
    {
        /// <summary>
        /// 그려질 글이 바뀌었다(언어를 바꿨다). 글에 맞춰 잡아 둔 칸을 다시 재야 하는 화면이 듣는다.
        /// 언어(`Loc`)가 알린다. 화면 영역이 언어 영역을 몰라도 되게 여기 둔다.
        /// </summary>
        public static event Action Changed;

        /// <summary>그려질 글이 바뀌었다고 알린다.</summary>
        public static void NotifyChanged()
        {
            Action changed = Changed;
            if (changed != null)
            {
                changed();
            }
        }

        /// <summary>그 글상자에 그려질 글. 전처리가 붙어 있으면 거친 글이다.</summary>
        public static string Of(TMP_Text label, string text)
        {
            if (label == null || text == null)
            {
                return text;
            }

            ITextPreprocessor preprocessor = label.textPreprocessor;
            return preprocessor != null ? preprocessor.PreprocessText(text) : text;
        }

        /// <summary>그려질 글로 잰 크기. 높이를 0 으로 주면 그 폭에 맞춘 높이를 돌려준다.</summary>
        public static Vector2 Measure(TMP_Text label, string text, float width, float height)
        {
            if (label == null)
            {
                return Vector2.zero;
            }

            return label.GetPreferredValues(Of(label, text) ?? string.Empty, width, height);
        }
    }
}
