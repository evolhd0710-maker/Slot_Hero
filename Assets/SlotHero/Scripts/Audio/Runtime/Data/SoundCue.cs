namespace SlotHero.Audio
{
    /// <summary>
    /// 효과음 하나의 이름. 값은 `효과음과 이펙트 목록.md` 1장의 번호다. S23 이면 23 이다.
    /// 목록에서 소리를 정해 붙일 때마다 하나씩 더한다.
    /// </summary>
    public enum SoundCue
    {
        /// <summary>정해지지 않은 상태.</summary>
        None = 0,

        /// <summary>S01 버튼 누름. 게임 씬의 모든 버튼을 누를 때(2026년 11월 13일 원재).</summary>
        ButtonPress = 1,

        /// <summary>S06 현재 빌드 열고 닫기. 현재 빌드 화면과 문양 고르기 화면이 열리고 닫힐 때(2026년 11월 13일 원재).</summary>
        CurrentBuildToggle = 6,

        /// <summary>
        /// S07 노드 고르기. 맵에서 다음 방 노드를 누를 때. 노드가 반짝이기 시작할 때 낸다(2026년 10월 31일 원재).
        /// </summary>
        NodeSelect = 7,

        /// <summary>S09 골드 얻음. 런의 골드가 늘 때(2026년 11월 13일 원재).</summary>
        GoldGained = 9,

        /// <summary>
        /// S12 체력 잃음. 피격음이다. 전투에서 맞을 때와 이벤트에서 체력을 잃을 때 함께 쓴다(2026년 10월 30일 원재).
        /// </summary>
        HealthLost = 12,

        /// <summary>S15 코인 받음. 가진 코인이 늘 때. 보상, 이벤트, 행상 모두다(2026년 11월 13일 원재).</summary>
        CoinGained = 15,

        /// <summary>S19 런 시작. 새 게임과 이어하기로 런에 들어갈 때(2026년 11월 13일 원재).</summary>
        RunStart = 19,

        /// <summary>
        /// S20 스테이지 전환. 보스를 잡고 다음 스테이지로 넘어갈 때(2026년 11월 13일 원재).
        /// 스테이지 전환 화면이 아직 없어 다음 스테이지 맵을 만드는 때(`GameFlowController.AdvanceStage`)에 낸다.
        /// </summary>
        StageTransition = 20,

        /// <summary>S23 행상 구매. 행상에서 물건을 살 때. 동전 소리 셋 가운데 하나를 낸다(2026년 10월 29일 원재).</summary>
        MerchantPurchase = 23,

        /// <summary>
        /// S25 설정 값 올림. 설정의 수치 항목(소리 크기 막대 등)이 한 단계 오를 때(2026년 11월 13일 원재).
        /// 선택지 넘기기, 음소거, 드롭다운, 탭은 S01 버튼 누름을 쓴다.
        /// </summary>
        SettingStepUp = 25,

        /// <summary>S26 설정 값 내림. 설정의 수치 항목이 한 단계 내릴 때(2026년 11월 13일 원재).</summary>
        SettingStepDown = 26,

        /// <summary>S24 야영. 야영을 쓸 때. 모닥불 소리를 2초 낸 뒤 0.5초 동안 줄인다(2026년 10월 29일 원재).</summary>
        Camp = 24,
    }
}
