using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Localization
{
    /// <summary>
    /// 영어 번역표(2026년 11월 8일 원재).
    ///
    /// 원본은 `코드/Localization/Text/영어.tsv` 다. 메뉴 Slot Hero / 번역 가져오기 가 그 파일을 읽어 이 에셋에 담는다.
    /// 표를 고칠 때는 TSV 를 고치고 다시 가져온다.
    /// </summary>
    [CreateAssetMenu(menuName = "Slot Hero/언어/번역표", fileName = "LocalizationConfig")]
    public class LocalizationConfig : ScriptableObject
    {
        [Tooltip("한국어 원문과 영어 번역의 짝. 원문에 {0} 같은 자리가 있으면 틀이다.")]
        [SerializeField] private List<TranslationEntry> _english = new List<TranslationEntry>();
        public List<TranslationEntry> English { get { return _english; } set { _english = value; } }

        /// <summary>
        /// TSV 글을 번역 줄로 읽는다. 한 줄에 한국어, 탭, 영어다. # 으로 시작하는 줄과 빈 줄은 건너뛴다.
        /// 칸 안의 \n 은 줄바꿈으로, \t 는 탭으로 읽는다.
        /// </summary>
        public static List<TranslationEntry> ParseTsv(string text)
        {
            List<TranslationEntry> entries = new List<TranslationEntry>();
            if (string.IsNullOrEmpty(text))
            {
                return entries;
            }

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                int tab = line.IndexOf('\t');
                if (tab <= 0 || tab == line.Length - 1)
                {
                    continue;
                }

                entries.Add(new TranslationEntry(Unescape(line.Substring(0, tab)), Unescape(line.Substring(tab + 1))));
            }

            return entries;
        }

        private static string Unescape(string cell)
        {
            return cell.Replace("\\n", "\n").Replace("\\t", "\t");
        }
    }
}
