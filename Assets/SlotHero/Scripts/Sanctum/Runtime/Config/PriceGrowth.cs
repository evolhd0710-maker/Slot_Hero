namespace SlotHero.Sanctum
{
    /// <summary>
    /// 쓸 때마다 값이 오르는 기능의 가격이 오르는 방식.
    /// 성소 기획서 v0.2 / 07 가격 의 문양 변경 · 유물 새로고침 기준가 표에서 둘의 방식이 다르다.
    /// </summary>
    public enum PriceGrowth
    {
        /// <summary>
        /// 증가 폭이 늘 같다. 유물 새로고침의 "증가 폭 10", 20 &gt; 30 &gt; 40 &gt; 50 &gt; 60 이 여기다.
        /// </summary>
        Linear = 0,

        /// <summary>
        /// 증가 폭 자체가 한 번 쓸 때마다 한 단위씩 커진다.
        /// 문양 변경의 "이전 가격 + n × 10", 10 &gt; 30 &gt; 60 &gt; 100 &gt; 150 이 여기다.
        /// </summary>
        Triangular = 1,
    }
}
