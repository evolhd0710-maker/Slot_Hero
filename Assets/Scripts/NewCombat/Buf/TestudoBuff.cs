using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class TestudoBuff : UnitEffect
{
    private const int RequiredStacks = 3;

    public override string DisplayName => "TestudoBuff";
    public override int Priority => 100;

    public TestudoBuff(int initialStacks)
        : base(initialStacks)
    {
    }

    public override int ModifyOutgoingDamage(
        NewUnitBase owner,
        int damage,
        Symbol symbol,
        TurnContext turnContext)
    {
        if (Stacks < RequiredStacks)
            return damage;

        ConsumeStacks(RequiredStacks);

        return damage * 2;
    }
}

