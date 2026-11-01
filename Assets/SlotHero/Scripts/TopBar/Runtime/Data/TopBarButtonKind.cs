namespace SlotHero.TopBar
{
    /// <summary>
    /// 상단 표시줄 우측에 놓이는 버튼.
    /// 상단 UI 바 기획서 v0.2 / 04 표시 항목 의 ⑤ ~ ⑦ 과 1:1로 맞춘다.
    /// </summary>
    public enum TopBarButtonKind
    {
        /// <summary>⑤ 지도. 현재 스테이지의 맵을 연다.</summary>
        Map = 0,

        /// <summary>⑥ 설정. 설정 화면을 연다.</summary>
        Settings = 1,

        /// <summary>⑦ 런 종료. 저장하고 나가기 혹은 런 포기를 고르는 확인 창을 연다.</summary>
        EndRun = 2,
    }
}
