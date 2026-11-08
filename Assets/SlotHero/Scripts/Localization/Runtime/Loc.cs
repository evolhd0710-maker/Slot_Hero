using System;
using System.Collections.Generic;

namespace SlotHero.Localization
{
    /// <summary>
    /// 지금 언어와 번역표를 한 곳에 둔다(2026년 11월 8일 원재).
    ///
    /// 흐름(`GameFlowController`)이 게임을 켤 때 번역표를 넣고, 설정의 언어가 바뀌면 언어를 바꾼다.
    /// 화면 글자는 `TextLocalizer` 가 그리기 직전에 `T` 로 바꾼다. 언어가 바뀌면 `Changed` 로 알려 모든 글자를 다시 그린다.
    /// </summary>
    public static class Loc
    {
        private static readonly Translator Table = new Translator();
        private static GameLanguage _language = GameLanguage.Korean;

        /// <summary>언어가 바뀌면 부른다.</summary>
        public static event Action Changed;

        /// <summary>지금 언어.</summary>
        public static GameLanguage Language
        {
            get { return _language; }
        }

        /// <summary>번역표. 검사가 못 찾은 글자를 읽는다.</summary>
        public static Translator Translator
        {
            get { return Table; }
        }

        /// <summary>번역표를 넣는다. 영어로 보는 중이면 글자를 다시 그린다.</summary>
        public static void SetTable(IList<TranslationEntry> entries)
        {
            Table.Load(entries);
            if (_language != GameLanguage.Korean)
            {
                Notify();
            }
        }

        /// <summary>언어를 바꾼다. 같은 언어면 아무 일도 하지 않는다.</summary>
        public static void SetLanguage(GameLanguage language)
        {
            if (_language == language)
            {
                return;
            }

            _language = language;
            Notify();
        }

        /// <summary>설정 값(드롭다운 차례)을 언어로 읽는다. 모르는 값은 한국어다.</summary>
        public static GameLanguage FromSettingIndex(float index)
        {
            return Math.Abs(index - (float)GameLanguage.English) < 0.5f ? GameLanguage.English : GameLanguage.Korean;
        }

        /// <summary>한국어 글자를 지금 언어로 바꾼다.</summary>
        public static string T(string korean)
        {
            return _language == GameLanguage.Korean ? korean : Table.Translate(korean);
        }

        /// <summary>맥락이 붙은 한국어 글자를 지금 언어로 바꾼다. 같은 말이 화면마다 뜻이 다를 때 쓴다.</summary>
        public static string T(string korean, string context)
        {
            return _language == GameLanguage.Korean ? korean : Table.Translate(korean, context);
        }

        private static void Notify()
        {
            Action changed = Changed;
            if (changed != null)
            {
                changed();
            }
        }
    }
}
