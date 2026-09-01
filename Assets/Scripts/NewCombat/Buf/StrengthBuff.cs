using UnityEngine;

public class StrengthBuff : UnitEffect
{
    public override string DisplayName => "StrengthBuff";
    public override int Priority => 10;

    public StrengthBuff(int initialStacks)
        : base(initialStacks)
    {
    }

    // 현재 스택만큼 모든 문양의 공격력을 증가시킨다.
    public override int ModifyOutgoingDamage(
        NewUnitBase owner,
        int damage,
        Symbol symbol,
        TurnContext turnContext)
    {
        return damage + Stacks;
    }
}
