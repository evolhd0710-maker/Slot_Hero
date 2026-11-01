namespace SlotHero.Reward
{
    /// <summary>
    /// 보상 화면이 열린 뒤 연출마다 시작하는 때(초). 화면이 열린 때가 0 이다.
    /// 기본 보상 골드 → 오버킬 골드 → 오버킬 회복 → 카드 반짝임 차례다(2026년 11월 1일 원재).
    /// `RewardVisualConfig.GetRevealTimeline` 이 만든다.
    /// </summary>
    public struct RewardRevealTimeline
    {
        /// <summary>기본 보상 골드 숫자가 오르기 시작하는 때.</summary>
        public float BaseGoldStart;

        /// <summary>오버킬 골드 숫자가 오르기 시작하는 때.</summary>
        public float OverkillGoldStart;

        /// <summary>오버킬 회복이 튀어나오기 시작하는 때.</summary>
        public float HealPopStart;

        /// <summary>카드가 반짝이기 시작하는 때.</summary>
        public float CardFlashStart;

        /// <summary>모든 연출이 끝나는 때.</summary>
        public float End;
    }
}
