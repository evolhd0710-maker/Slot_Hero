using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SlotHero.Events
{
    /// <summary>
    /// 선택지 칸 안에 적는 한 줄.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의
    /// "획득과 소모를 해당 칸 안에 표시하며 획득은 초록색으로 표시한다"와
    /// 필요한 항목을 괄호로 적어 보유 여부를 색으로 알리는 데 쓴다.
    ///
    /// 무엇을 얻고 무엇을 치르고 무엇이 필요한지는 이벤트 기획서 소관이라
    /// 여기서는 화면에 적을 글과 그 종류, 그리고 채웠는지만 받는다.
    /// </summary>
    [Serializable]
    public struct EventEffectLine
    {
        [FormerlySerializedAs("Kind")]
        [SerializeField] private EventEffectKind _kind;
        public EventEffectKind Kind { get { return _kind; } set { _kind = value; } }

        /// <summary>화면에 적을 글. "체력 5 증가", "간식" 처럼 내용만 담는다.</summary>
        [FormerlySerializedAs("Text")]
        [SerializeField] private string _text;
        public string Text { get { return _text; } set { _text = value; } }

        /// <summary>
        /// 필요한 것을 갖췄는지. <see cref="EventEffectKind.Requirement"/>에만 쓴다.
        /// 갖췄으면 초록색, 아니면 빨간색으로 적는다.
        /// </summary>
        [FormerlySerializedAs("Met")]
        [SerializeField] private bool _met;
        public bool Met { get { return _met; } set { _met = value; } }

        /// <summary>
        /// 앞 줄과 쉼표 대신 "혹은"으로 잇는지. 둘 가운데 하나만 일어나는 결과에 쓴다.
        /// 19 두 개의 상자 의 "유물 혹은 체력 손실"(2026년 10월 28일 원재).
        /// </summary>
        [SerializeField] private bool _alternative;
        public bool Alternative { get { return _alternative; } set { _alternative = value; } }

        public EventEffectLine(EventEffectKind kind, string text, bool met)
        {
            _kind = kind;
            _text = text;
            _met = met;
            _alternative = false;
        }

        /// <summary>앞 줄과 "혹은"으로 잇는 줄로 바꾼다.</summary>
        public EventEffectLine OrPrevious()
        {
            EventEffectLine line = this;
            line._alternative = true;
            return line;
        }

        public static EventEffectLine Gain(string text)
        {
            return new EventEffectLine(EventEffectKind.Gain, text, true);
        }

        public static EventEffectLine Cost(string text)
        {
            return new EventEffectLine(EventEffectKind.Cost, text, true);
        }

        /// <summary>필요한 항목. met 이 false 면 그 선택지를 고를 수 없다.</summary>
        public static EventEffectLine Requirement(string text, bool met)
        {
            return new EventEffectLine(EventEffectKind.Requirement, text, met);
        }
    }
}
