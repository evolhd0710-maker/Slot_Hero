namespace SlotHero.Combat
{
    /// <summary>
    /// 전투가 지금 어디까지 왔는지. 전투 UI 및 연출 초안 / 02 전투 UI 연출 흐름 의 칸들이다.
    ///
    /// 문양 회전 → 코인 선택 ⇄ 태그 선택 → 공격 → 적 공격 → 재굴림 → 코인 선택 …
    /// </summary>
    public enum CombatPhase
    {
        /// <summary>아직 열지 않았다.</summary>
        None,

        /// <summary>제공된 코인 셋 중 하나를 고른다.</summary>
        ChoosingCoin,

        /// <summary>코인을 걸 태그를 고른다. 가운데 코인을 누르면 코인 선택으로 돌아간다.</summary>
        ChoosingTag,

        /// <summary>공격을 마쳤고 적이 살아 있다. 적이 때릴 차례다.</summary>
        EnemyTurn,

        /// <summary>적이 때렸고 플레이어가 살아 있다. 쓴 문양을 다시 돌릴 차례다.</summary>
        Rerolling,

        /// <summary>적을 쓰러뜨렸다.</summary>
        Won,

        /// <summary>플레이어가 쓰러졌다.</summary>
        Lost
    }

    /// <summary>끝난 전투의 결과. 흐름이 `CombatResult` 로 바꾼다.</summary>
    public struct CombatOutcome
    {
        public bool Won;

        /// <summary>이 전투에서 준 가장 큰 피해.</summary>
        public int BiggestHit;

        /// <summary>마지막 공격이 0 아래로 넘친 피해.</summary>
        public int Overkill;

        /// <summary>공격한 횟수.</summary>
        public int Turns;
    }
}
