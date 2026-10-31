namespace SlotHero.Sanctum
{
    /// <summary>
    /// 코인과 유물의 등급.
    /// 성소 기획서 v0.2 / 07 가격 의 코인 기준가, 유물 기준가 표와 1:1로 맞춘다.
    /// 문양은 등급이 없으므로 이 열거형을 쓰지 않는다.
    /// </summary>
    public enum ItemRarity
    {
        /// <summary>일반 (common).</summary>
        Common = 0,

        /// <summary>고급 (uncommon).</summary>
        Uncommon = 1,

        /// <summary>희귀 (rare).</summary>
        Rare = 2,

        /// <summary>특급 (epic).</summary>
        Epic = 3,

        /// <summary>전설 (legend).</summary>
        Legend = 4,
    }
}
