namespace SlotHero.Title
{
    /// <summary>
    /// 저장된 런이 어떤 상태인지.
    /// 인게임 화면 기획서 v0.2 / 05 타이틀 화면 의
    /// "타이틀 화면 · 이어하기 비활성" 장이 정한 세 갈래다.
    ///
    /// 이어하기를 누를 수 있는지와 새 게임이 확인 팝업을 거치는지가 여기서 갈린다.
    /// </summary>
    public enum SavedRunState
    {
        /// <summary>
        /// 저장된 런이 없다.
        /// 이어하기가 비활성이고 새 게임은 확인 팝업 없이 바로 시작한다.
        /// </summary>
        None = 0,

        /// <summary>
        /// 저장된 런이 있고 읽을 수 있다.
        /// 이어하기가 활성이고 새 게임은 확인 팝업을 거친다.
        /// </summary>
        Ready = 1,

        /// <summary>
        /// 런 데이터가 있으나 문제가 생겼다.
        /// 화면을 열 때 런 데이터 손상 알림을 띄우고 이어하기를 비활성으로 바꾼다.
        /// </summary>
        Broken = 2,

        /// <summary>
        /// 런 파일이 있는데 지금 읽지 못했다. 손상과 달리 지우거나 알리지 않고 이어하기만 끈다.
        /// 새 게임은 저장된 런이 있으므로 확인 팝업을 거친다. 2026년 10월 9일에 더했다.
        /// </summary>
        Unreadable = 3,
    }

    /// <summary>저장된 런 상태를 다루는 데 쓰는 값.</summary>
    public static class SavedRunStates
    {
        /// <summary>이어하기를 누를 수 있는지. 읽을 수 있는 런이 있을 때만 누른다.</summary>
        public static bool CanContinue(SavedRunState state)
        {
            return state == SavedRunState.Ready;
        }

        /// <summary>
        /// 새 게임이 확인 팝업을 거쳐야 하는지.
        /// 기획서가 "저장된 런이 있을 경우 새 게임 확인 팝업을 띄운다"로 정했다.
        /// 손상된 런도 저장된 런이므로 지우기 전에 묻는다.
        /// 확인 없이 바로 시작하는 것은 저장된 런이 아예 없을 때뿐이다.
        /// </summary>
        public static bool NeedsNewGameConfirm(SavedRunState state)
        {
            return state != SavedRunState.None;
        }

        /// <summary>화면을 열 때 런 데이터 손상 알림을 띄워야 하는지.</summary>
        public static bool NeedsBrokenNotice(SavedRunState state)
        {
            return state == SavedRunState.Broken;
        }
    }
}
