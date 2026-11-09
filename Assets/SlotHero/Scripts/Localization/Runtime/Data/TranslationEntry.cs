using System;
using UnityEngine;

namespace SlotHero.Localization
{
    /// <summary>
    /// 번역 한 줄. 한국어 원문과 영어 번역의 짝이다.
    ///
    /// 원문에 {0}, {1} 같은 자리가 있으면 틀이다. 화면 글자가 틀과 맞으면 자리에 든 글자를 다시 번역해 영어 틀에 넣는다.
    /// 영어 틀에서는 {0|card|cards} 처럼 쓰면 자리에 든 글자가 1 일 때 앞쪽, 아니면 뒤쪽 낱말을 붙인다.
    /// </summary>
    [Serializable]
    public class TranslationEntry
    {
        [SerializeField] private string _korean;
        public string Korean { get { return _korean; } set { _korean = value; } }

        [SerializeField] private string _english;
        public string English { get { return _english; } set { _english = value; } }

        public TranslationEntry()
        {
        }

        public TranslationEntry(string korean, string english)
        {
            _korean = korean;
            _english = english;
        }
    }
}
