using UnityEngine;

public class DrainBuff : UnitEffect
{
    public override string DisplayName => "DrainBuff";
    public override bool ClearOnTurnEnd => false;

    public DrainBuff(int initialStacks = 1) : base(initialStacks)
    {
    }

    public override void OnDamageDealt(NewUnitBase owner, int damage, SymbolExecutionContext context)
    {
        if (Stacks <= 0 || damage <= 0)
            return;

        owner.Heal(Stacks);
        Debug.Log($"{owner.data.name} ÈíÇ÷ ¹ßµ¿. Ã¼·Â È¸º¹: {Stacks}, ÈíÇ÷ ½ºÅÃ: {Stacks}");
    }
}
