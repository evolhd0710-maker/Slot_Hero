namespace SlotHero.Title
{
    /// <summary>
    /// 타이틀 화면 메뉴 항목의 종류.
    /// 인게임 화면 기획서 v0.2 / 05 타이틀 화면 이 정한 넷이다.
    ///
    /// 화면에 어떤 차례로 놓을지는 <see cref="TitleVisualConfig.MenuEntries"/> 가 정한다.
    /// 기획서 본문과 와이어프레임이 모두 새 게임을 먼저 두었으므로 그 차례를 기본값으로 삼았다.
    /// </summary>
    public enum TitleMenuKind
    {
        /// <summary>새 런을 시작한다. 저장된 런이 있으면 확인 팝업을 거친다.</summary>
        NewGame = 0,

        /// <summary>저장된 런을 불러와 저장된 자리로 들어간다.</summary>
        Continue = 1,

        /// <summary>설정 화면으로 간다.</summary>
        Settings = 2,

        /// <summary>확인 팝업을 거쳐 게임을 끝낸다.</summary>
        Quit = 3,
    }
}
