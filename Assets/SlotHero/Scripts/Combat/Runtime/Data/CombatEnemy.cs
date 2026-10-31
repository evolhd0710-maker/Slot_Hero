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
    /// 몬스터마다 행동 방식. `slot hero 테스트 몬스터.pptx`(Stage 1 몬스터 기획서)를 옮겼다.
    /// Plain 은 기획서가 없는 적이다. 매 턴 공격력만큼 한 번 때린다.
    /// </summary>
    public enum MonsterKind
    {
        Plain,
        Gladiator,
        General
    }

    /// <summary>
    /// 전투 하나의 적.
    ///
    /// 몬스터 기획서의 공통 행동 방식을 따른다.
    ///   - 방어도는 매 턴 소멸한다. 그 적이 행동하기 직전에 0 이 된다
    ///   - 버프(기세, 투창 명령)는 따로 끝나는 조건이 없으면 전투 내내 남는다
    ///   - 소환수는 본체가 쓰러지면 함께 사라진다
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

        /// <summary>적이 한 턴에 주는 피해. Plain 만 쓴다. 패턴 몬스터는 `CombatRuleConfig` 의 몬스터별 수치를 쓴다.</summary>
        public int Attack;

        /// <summary>행동 방식.</summary>
        public MonsterKind Monster;

        /// <summary>스테이지가 올라 패턴 피해에 더하는 값. 피해 계산 ① 고정 증감이다.</summary>
        public int AttackBonus;

        /// <summary>스테이지. 소환수 체력을 같은 스테이지로 맞출 때 쓴다. 1부터다.</summary>
        public int Stage = 1;

        /// <summary>방어도. 받는 피해를 먼저 막는다.</summary>
        public int Block;

        /// <summary>버프 '기세'. 1 당 주는 피해 1% 증가. 중첩된다(검투사의 함성).</summary>
        public int Momentum;

        /// <summary>
        /// 장군의 방패병 수. 방패병은 따로 서는 적이 아니라 장군이 가진 수다(2026년 10월 29일 장군 재설계).
        /// 턴 시작마다 늘고, 큰 피해를 줄일 때마다 하나씩 준다.
        /// </summary>
        public int Shieldmen;

        /// <summary>턴 시작에 미리 고른 패턴 칸. 적 차례에 이 패턴을 쓴다. 고르지 않았으면 -1 이다.</summary>
        public int PlannedPick = -1;

        /// <summary>
        /// 다음 적 차례에 할 행동. 정보 칸에 적는다. 피해는 지금 기세, 스테이지 보정, 유물로 셈한 예상값이다.
        /// 턴 시작에 정하고 영웅 공격 뒤 다시 셈한다. 적 차례가 끝나 다음 턴 시작 전까지는 null 이다.
        /// </summary>
        public EnemyAction NextAction;

        /// <summary>소환수인지. 본체가 쓰러지면 사라진다.</summary>
        public bool IsSummon;

        /// <summary>
        /// 몬스터 자리. 왼쪽이 0 이다. 왼쪽부터 행동하고 왼쪽부터 영웅의 공격을 받는다.
        /// 본체는 소환수 자리 오른쪽, 소환수는 본체 바로 왼쪽부터 채운다.
        /// </summary>
        public int Slot;

        /// <summary>이 전투에서 행동한 횟수. 첫 턴 고정 패턴(검투사의 타격)이 본다.</summary>
        public int Actions;

        /// <summary>이번 적 차례에 소환됐는지. 소환된 턴에는 행동하지 않는다.</summary>
        public bool JustSummoned;

        /// <summary>마지막 행동. 정보 칸에 적는다. 아직 행동하지 않았으면 null 이다.</summary>
        public EnemyAction LastAction;

        public CombatEnemy(string name, string description, int maxHealth, int attack)
            : this(name, description, maxHealth, attack, MonsterKind.Plain)
        {
        }

        public CombatEnemy(string name, string description, int maxHealth, int attack, MonsterKind monster)
        {
            Name = name;
            Description = description;
            MaxHealth = maxHealth < 1 ? 1 : maxHealth;
            Health = MaxHealth;
            Attack = attack < 0 ? 0 : attack;
            Monster = monster;
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
