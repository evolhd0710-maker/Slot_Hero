using System.Collections.Generic;

namespace SlotHero.Combat
{
    /// <summary>
    /// 공격 한 번의 계산.
    ///
    /// 슬롯머신 작동 방식 1.1.a 와 2026년 10월 25일에 정한 규칙을 따른다.
    ///   - 코인을 건 태그가 붙은 문양만 계산에 쓴다
    ///   - 계산에는 태그의 값이 아니라 **문양의 가치**를 더한다
    ///   - 공격력 = 문양 가치 합 × (중복 배수 + 코인 가중치)
    ///   - 공격은 한 번이다. 태그마다 따로 때리지 않는다
    ///
    /// 예. 해왕성에 소모 10 · 가중치 5 코인을 걸고 섬(7)과 난파선(9)이 나왔으면
    /// (7 + 9) × (2 + 5) = 112 다.
    /// </summary>
    public class AttackResult
    {
        public SymbolTagType Tag;

        /// <summary>건 코인. 코인 없이 고른 공격이면 빈 글이다.</summary>
        public string CoinId = string.Empty;

        /// <summary>계산에 쓴 릴 자리. 다음 턴에 이 자리만 다시 돈다.</summary>
        public List<int> ReelIndices = new List<int>();

        /// <summary>쓴 문양의 가치 합.</summary>
        public int ValueSum;

        /// <summary>쓴 문양 수. 중복 배수를 정한다.</summary>
        public int DuplicateCount;

        /// <summary>문양 수로 정해지는 중복 배수.</summary>
        public int DuplicateMultiplier;

        /// <summary>코인이 배수에 더한 값.</summary>
        public int CoinWeight;

        /// <summary>코인을 쓰느라 낸 체력.</summary>
        public int HealthPaid;

        /// <summary>문양 재사용으로 가치를 더 더한 횟수(별의 저편, 속임수 주사위).</summary>
        public int ReuseCount;

        /// <summary>마지막에 곱한 배율(곰 가죽 망토). 없으면 1 이다.</summary>
        public float DamageScale = 1f;

        /// <summary>소용돌이 칸으로 때렸는지(사건의 지평선).</summary>
        public bool FromVortex;

        /// <summary>최종 공격력.</summary>
        public int Damage;

        /// <summary>
        /// 맞은 적. 플레이어가 고른 대상이고, 고르지 않았으면 가장 왼쪽에 살아 있는 적이다(`CombatBattle.Target`).
        /// </summary>
        public CombatEnemy Target;

        /// <summary>맞은 적의 방어도가 막은 피해.</summary>
        public int Blocked;

        /// <summary>때린 뒤 맞은 적의 체력. 0 아래일 수 있다.</summary>
        public int EnemyHealthAfter;

        /// <summary>배수 전체. 중복 배수와 코인 가중치의 합이다.</summary>
        public int Multiplier
        {
            get { return DuplicateMultiplier + CoinWeight; }
        }
    }
}
