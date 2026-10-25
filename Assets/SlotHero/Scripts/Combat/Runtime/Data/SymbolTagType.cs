namespace SlotHero.Combat
{
    /// <summary>
    /// 문양에 붙는 태그. 아홉 행성이다.
    ///
    /// 예전에는 저장소 원본 코드(`Assets/Scripts/NewNewCombat/SymbolTag/SymbolTagData.cs`)가 전역에 두었다.
    /// 2026년 10월 25일 원본 코드를 폐기하면서 이쪽으로 옮겼다.
    /// **차례를 바꾸지 않는다.** 저장 파일과 설정 에셋이 이 값을 숫자로 적어 둔다.
    /// 차례는 계산 순서(수금지화목토천해명)이기도 하다.
    /// </summary>
    public enum SymbolTagType
    {
        Mercury,
        Venus,
        Earth,
        Mars,
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Pluto
    }
}
