namespace SlotHero.Combat
{
    /// <summary>전투에 나오는 적의 종류. 맵의 방 종류에서 흐름이 골라 넘긴다.</summary>
    public enum CombatEnemyKind
    {
        Normal,
        Elite,
        Boss
    }

    /// <summary>
    /// 전투 하나의 적.
    ///
    /// **임시다.** 적 기획서가 아직 없다. 슬롯머신 작동 방식 1.1.a / 적 행동 은
    /// "적의 행동은 시스템적 제한으로 이뤄진다", "HP 압박으로 시간 압박을 준다" 고만 적었다.
    /// 그래서 지금은 매 턴 같은 피해로 한 번 때린다.
    ///
    /// 체력은 0 아래로도 내려간다. 전투 UI 초안이 오버킬을 보이려고 음수까지 표시하게 했다.
    /// </summary>
    public class CombatEnemy
    {
        public string Name;

        /// <summary>몬스터 정보 칸에 적는 설명. 보스 등 특수 능력을 적는 자리다.</summary>
        public string Description;

        public int MaxHealth;
        public int Health;

        /// <summary>적이 한 턴에 주는 피해.</summary>
        public int Attack;

        public CombatEnemy(string name, string description, int maxHealth, int attack)
        {
            Name = name;
            Description = description;
            MaxHealth = maxHealth < 1 ? 1 : maxHealth;
            Health = MaxHealth;
            Attack = attack < 0 ? 0 : attack;
        }

        /// <summary>쓰러졌는지.</summary>
        public bool IsDead
        {
            get { return Health <= 0; }
        }

        /// <summary>0 아래로 넘친 피해. 쓰러지지 않았으면 0 이다.</summary>
        public int Overkill
        {
            get { return Health < 0 ? -Health : 0; }
        }
    }
}
