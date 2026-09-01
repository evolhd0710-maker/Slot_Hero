using UnityEngine;

public abstract class OutgoingDamageModifier
{
    public int stacks;
    public virtual int Priority => 0;
    public virtual bool ClearOnTurnEnd => false;

    public OutgoingDamageModifier(int initialStacks)
    {
        stacks = initialStacks;
    }

    public abstract int Modify(int rawDamage, Symbol symbol);

    public virtual void OnReapply(OutgoingDamageModifier newInstance)
    {
        stacks += newInstance.stacks;
    }
}
