namespace SlotHero.Combat
{
    /// <summary>
    /// 전투에 들고 들어간 유물 하나.
    ///
    /// 쌓인 횟수(`Stack`)는 런에 저장된 값(`OwnedRelic.Stack`)에서 시작하고,
    /// 전투 중에 오르면 `CombatBattle.RelicStackChanged` 로 알려 흐름이 런에 다시 적는다.
    /// 정의를 모르는 유물(목록에만 있는 것)은 효과 없이 그림과 이름만 놓인다.
    /// </summary>
    public class CombatRelic
    {
        public string Id;
        public string DisplayName;

        /// <summary>효과. 모르는 유물이면 null 이다.</summary>
        public RelicDefinition Definition;

        /// <summary>쌓인 횟수. 12개의 금화처럼 영구히 오르는 유물이 쓴다.</summary>
        public int Stack;

        public CombatRelic(string id, string displayName, RelicDefinition definition, int stack)
        {
            Id = id;
            DisplayName = displayName;
            Definition = definition;
            Stack = stack;
        }

        /// <summary>유물 고유 수치. 정의가 없으면 0 이다.</summary>
        public int Counter
        {
            get { return Definition != null ? Definition.GetCounter(Stack) : 0; }
        }

        /// <summary>고유 수치를 쓰는 유물인지. 칸에 지금 값을 적는다.</summary>
        public bool ShowsCounter
        {
            get { return Definition != null && Definition.Has(RelicEffectKind.CoinWeightFromCounter); }
        }

        /// <summary>지금 값을 채운 소개문.</summary>
        public string Description
        {
            get { return Definition != null ? Definition.GetDescription(Stack) : string.Empty; }
        }
    }
}
