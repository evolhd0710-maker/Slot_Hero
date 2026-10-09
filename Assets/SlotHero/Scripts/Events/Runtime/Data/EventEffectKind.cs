namespace SlotHero.Events
{
    /// <summary>
    /// 선택지 칸 안에 적는 한 줄의 종류.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 선택지 표시 규칙을 따른다.
    /// </summary>
    public enum EventEffectKind
    {
        /// <summary>얻는 것. 초록색으로 적는다.</summary>
        Gain = 0,

        /// <summary>치르는 것. 기본 글자색으로 적는다.</summary>
        Cost = 1,

        /// <summary>
        /// 그 선택지를 고르는 데 필요한 것.
        /// 선택지 문구 바로 뒤에 괄호로 적고,
        /// 가지고 있으면 초록색, 가지고 있지 않으면 빨간색으로 보여 준다.
        /// 가지고 있지 않은 것이 하나라도 있으면 그 선택지는 고를 수 없다.
        /// </summary>
        Requirement = 2,
    }
}
