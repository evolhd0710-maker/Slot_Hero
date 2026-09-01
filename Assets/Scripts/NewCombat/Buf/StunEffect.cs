
using UnityEngine;
public class StunEffect : UnitEffect
{
    public override string DisplayName => "StunEffect";
    public override int Priority => -100;

    public StunEffect(int initialStacks = 1)
        : base(initialStacks)
    {
    }

    public override bool RespondsTo(TriggerTiming timing)
    {
        return timing == TriggerTiming.BeforeAction;
    }

    public override void OnTrigger(
        NewUnitBase owner,
        EffectTriggerContext context)
    {
        if (Stacks <= 0)
            return;
        Debug.Log($"{owner.data.name} 기절 발동. 발동 전 스택: {Stacks}");
        context.ActionCancelled = true;
        ConsumeStacks(1);
    }
}
