using System;

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
        public EventEffectKind Kind;

        /// <summary>화면에 적을 글. "체력 5 증가", "간식" 처럼 내용만 담는다.</summary>
        public string Text;

        /// <summary>
        /// 필요한 것을 갖췄는지. <see cref="EventEffectKind.Requirement"/>에만 쓴다.
        /// 갖췄으면 초록색, 아니면 빨간색으로 적는다.
        /// </summary>
        public bool Met;

        public EventEffectLine(EventEffectKind kind, string text, bool met)
        {
            Kind = kind;
            Text = text;
            Met = met;
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
