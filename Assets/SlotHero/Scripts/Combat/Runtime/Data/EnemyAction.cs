namespace SlotHero.Combat
{
    /// <summary>몬스터 기획서의 패턴 분류. 분류 태그 색상 표의 칸이다. 지금 쓰는 것만 둔다.</summary>
    public enum PatternKind
    {
        Attack,
        Defend,
        Buff,
        Summon,
        Idle
    }

    /// <summary>
    /// 적 하나가 한 번 행동한 것. 화면이 차례대로 연출하고 정보 칸에 "지난 행동" 으로 적는다.
    /// </summary>
    public class EnemyAction
    {
        /// <summary>행동한 적.</summary>
        public CombatEnemy Actor;

        /// <summary>패턴 이름. 기획서의 이름 그대로다(타격, 방어, 함성 …).</summary>
        public string Pattern = string.Empty;

        public PatternKind Kind;

        /// <summary>영웅이 실제로 받은 피해. 유물(철제 방패)로 줄인 뒤다.</summary>
        public int Damage;

        /// <summary>얻은 방어도.</summary>
        public int BlockGained;

        /// <summary>행동 설명. 다음 행동 미리보기에만 채운다. 정보 칸에 적는다.</summary>
        public string Description = string.Empty;

        /// <summary>소환한 적. 소환하지 않았으면 null 이다.</summary>
        public CombatEnemy Summoned;

        /// <summary>안내 글에 적는 한 줄. "검투사 · 타격 6" 꼴이다.</summary>
        public string Summary
        {
            get
            {
                string name = Actor != null ? Actor.Name : string.Empty;
                switch (Kind)
                {
                    case PatternKind.Attack:
                        return name + " · " + Pattern + " " + Damage;
                    case PatternKind.Defend:
                        return name + " · " + Pattern + " " + BlockGained;
                    default:
                        return name + " · " + Pattern;
                }
            }
        }
    }
}
