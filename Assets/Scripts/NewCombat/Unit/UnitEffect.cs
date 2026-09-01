using UnityEngine;

// ===== 변경 시작: 통합 유닛 효과 기반 클래스 추가 =====

public enum EffectStackRule
{
    AddStacks,
    Replace,
    Ignore
}

public abstract class UnitEffect
{
    public int Stacks { get; protected set; }

    public virtual string DisplayName => GetType().Name;
    public virtual int Priority => 0;
    public virtual int MaxStacks => int.MaxValue;
    public virtual bool ClearOnTurnEnd => false;
    public virtual EffectStackRule StackRule => EffectStackRule.AddStacks;

    public bool IsExpired => Stacks <= 0;

    public virtual string EffectId => GetType().Name;
    public virtual bool ShowInUI => true;

    protected UnitEffect(int initialStacks)
    {
        Stacks = Mathf.Max(0, initialStacks);
    }

    public virtual void OnApply(NewUnitBase owner)
    {
    }

    public virtual void OnReapply(
        NewUnitBase owner,
        UnitEffect newEffect)
    {
        switch (StackRule)
        {
            case EffectStackRule.AddStacks:
                AddStacks(newEffect.Stacks);
                break;

            case EffectStackRule.Replace:
                Stacks = Mathf.Clamp(
                    newEffect.Stacks,
                    0,
                    MaxStacks
                );
                break;

            case EffectStackRule.Ignore:
                break;
        }
    }

    public virtual bool RespondsTo(TriggerTiming timing)
    {
        return false;
    }

    public virtual void OnTrigger(
        NewUnitBase owner,
        EffectTriggerContext context)
    {
    }

    public virtual int ModifyOutgoingDamage(
        NewUnitBase owner,
        int damage,
        Symbol symbol,
        TurnContext turnContext)
    {
        return damage;
    }

    public virtual void OnRemove(NewUnitBase owner)
    {
    }

    public virtual void OnDamageDealt(NewUnitBase owner, int damage, SymbolExecutionContext context)
    {
    }

    protected void AddStacks(int amount)
    {
        Stacks = Mathf.Clamp(
            Stacks + amount,
            0,
            MaxStacks
        );
    }

    protected void ConsumeStacks(int amount)
    {
        Stacks = Mathf.Max(
            0,
            Stacks - amount
        );
    }
}
