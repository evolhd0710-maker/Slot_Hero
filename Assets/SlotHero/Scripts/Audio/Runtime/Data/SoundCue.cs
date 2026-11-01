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

        /// <summary>
        /// S07 노드 고르기. 맵에서 다음 방 노드를 누를 때. 노드가 반짝이기 시작할 때 낸다(2026년 10월 31일 원재).
        /// </summary>
        NodeSelect = 7,

        /// <summary>
        /// S12 체력 잃음. 피격음이다. 전투에서 맞을 때와 이벤트에서 체력을 잃을 때 함께 쓴다(2026년 10월 30일 원재).
        /// </summary>
        HealthLost = 12,

        /// <summary>S23 행상 구매. 행상에서 물건을 살 때. 동전 소리 셋 가운데 하나를 낸다(2026년 10월 29일 원재).</summary>
        MerchantPurchase = 23,

        /// <summary>S24 야영. 야영을 쓸 때. 모닥불 소리를 2초 낸 뒤 0.5초 동안 줄인다(2026년 10월 29일 원재).</summary>
        Camp = 24,
    }
}
