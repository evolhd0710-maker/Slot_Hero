namespace SlotHero.Save
{
    /// <summary>
    /// 저절로 저장되는 자리.
    /// 저장 시스템 기획서 v0.1 / 08 저장 시점 의 표를 그대로 옮긴 것이다.
    ///
    /// 런 데이터 넷, 메타 데이터 하나, 설정 데이터 둘이다.
    /// </summary>
    public enum AutoSavePoint
    {
        /// <summary>런 시작. 런 데이터를 새로 쓴다.</summary>
        RunStarted = 0,

        /// <summary>맵에서 방을 골라 들어간 순간. 런 데이터를 덮어쓴다.</summary>
        RoomEntered = 1,

        /// <summary>
        /// 방을 깨고 보상 목록이 정해진 순간. 런 데이터를 덮어쓴다.
        /// 보상을 고르기 전에 저장하므로 되돌아와도 같은 보상 목록이 나오고
        /// 보상을 두 번 받을 수는 없다. 10쪽의 "초기화되는 값"이 그렇게 정했다.
        /// </summary>
        RoomCleared = 2,

        /// <summary>런 종료. 런 데이터를 지우고 메타 데이터를 갱신한다.</summary>
        RunEnded = 3,

        /// <summary>설정 창에서 값을 바꾼 순간. 그 값을 바로 쓴다.</summary>
        SettingChanged = 4,

        /// <summary>설정 창의 저장 버튼. 바로 적용할 수 없는 그래픽 값을 쓴다.</summary>
        SettingsApplied = 5,
    }

    /// <summary>저장 시점을 다루는 데 쓰는 값.</summary>
    public static class AutoSavePoints
    {
        /// <summary>런 데이터를 건드리는 시점인지.</summary>
        public static bool TouchesRun(AutoSavePoint point)
        {
            return point == AutoSavePoint.RunStarted
                || point == AutoSavePoint.RoomEntered
                || point == AutoSavePoint.RoomCleared
                || point == AutoSavePoint.RunEnded;
        }

        /// <summary>메타 데이터를 건드리는 시점인지. 런 종료뿐이다.</summary>
        public static bool TouchesMeta(AutoSavePoint point)
        {
            return point == AutoSavePoint.RunEnded;
        }

        /// <summary>설정 데이터를 건드리는 시점인지.</summary>
        public static bool TouchesSettings(AutoSavePoint point)
        {
            return point == AutoSavePoint.SettingChanged
                || point == AutoSavePoint.SettingsApplied;
        }

        /// <summary>
        /// 자동 저장 안내를 띄우는 시점인지.
        /// 인게임 화면 기획서 v0.2 / 04 의 자동 저장 표시가 런 진행 중에만 뜬다.
        /// 설정 저장은 화면 위에 알릴 일이 아니라 빼고, 런 종료는 결과 화면으로 넘어가므로 뺀다.
        /// </summary>
        public static bool ShowsNotice(AutoSavePoint point)
        {
            return point == AutoSavePoint.RunStarted
                || point == AutoSavePoint.RoomEntered
                || point == AutoSavePoint.RoomCleared;
        }
    }
}
