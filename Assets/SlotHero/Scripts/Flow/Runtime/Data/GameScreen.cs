namespace SlotHero.Flow
{
    /// <summary>
    /// 지금 어느 화면에 있는지.
    /// 인게임 화면 기획서 v0.2 가 장으로 나눈 화면들을 한 줄로 늘어놓은 것이다.
    ///
    /// 한 씬 안에서 오브젝트를 켜고 끄는 방식이라 화면 하나가 곧 씬 하나는 아니다.
    /// 설정과 팝업과 현재 빌드는 다른 화면 위에 겹쳐 뜨므로 여기 넣지 않는다.
    /// 그것들은 어느 화면 위에서든 열릴 수 있고 닫으면 원래 화면으로 돌아간다.
    /// </summary>
    public enum GameScreen
    {
        /// <summary>아무것도 열려 있지 않다.</summary>
        None = 0,

        /// <summary>05 타이틀 화면.</summary>
        Title = 1,

        /// <summary>06 프로필 선택 화면.</summary>
        ProfileSelect = 2,

        /// <summary>09 맵 화면.</summary>
        Map = 3,

        /// <summary>성소. 야영과 행상이 그 안에 있다.</summary>
        Sanctum = 4,

        /// <summary>11 이벤트 화면.</summary>
        Event = 5,

        /// <summary>전투. 아직 코드가 없어 대역이 대신 선다.</summary>
        Combat = 6,

        /// <summary>10 보상 화면. 방을 깨고 나면 뜬다.</summary>
        Reward = 7,

        /// <summary>12 런 종료 결과 화면.</summary>
        RunResult = 8,
    }

    /// <summary>화면을 다루는 데 쓰는 값.</summary>
    public static class GameScreens
    {
        /// <summary>런이 돌고 있는 화면인지. 상단 표시줄과 현재 빌드 버튼이 여기서만 보인다.</summary>
        public static bool IsInRun(GameScreen screen)
        {
            return screen == GameScreen.Map
                || screen == GameScreen.Sanctum
                || screen == GameScreen.Event
                || screen == GameScreen.Combat
                || screen == GameScreen.Reward;
        }

        /// <summary>방 안에 있는 화면인지. 맵으로 돌아갈 수 있는 자리다.</summary>
        public static bool IsInRoom(GameScreen screen)
        {
            return screen == GameScreen.Sanctum
                || screen == GameScreen.Event
                || screen == GameScreen.Combat;
        }
    }
}
