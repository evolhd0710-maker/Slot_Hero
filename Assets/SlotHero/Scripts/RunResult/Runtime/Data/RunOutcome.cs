namespace SlotHero.RunResult
{
    /// <summary>
    /// 런이 어떻게 끝났는지.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의 "결과 · 사망 혹은 클리어"다.
    ///
    /// 기획서 본문은 "사망"이라 적었고 와이어프레임은 "패배"로 적었다.
    /// 화면에 적는 글은 <see cref="RunResultVisualConfig"/> 에서 바꿀 수 있게 두었다.
    /// </summary>
    public enum RunOutcome
    {
        /// <summary>주인공이 쓰러져 끝났다.</summary>
        Defeat = 0,

        /// <summary>마지막까지 깨고 끝났다.</summary>
        Clear = 1,
    }
}
