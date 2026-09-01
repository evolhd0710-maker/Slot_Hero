using UnityEngine;

public abstract class StatusEffectBase 
{
    public int stacks;
    public abstract TriggerTiming Timing { get; }
    public abstract string DisplayName { get; }

    public StatusEffectBase(int initialStacks)
    {
        stacks = initialStacks;
    }

    public abstract void OnTrigger(NewUnitBase self, TurnContext context);
}
