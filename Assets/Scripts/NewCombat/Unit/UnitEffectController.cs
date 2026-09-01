using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class UnitEffectController
{
    private readonly NewUnitBase owner;
    private readonly List<UnitEffect> activeEffects = new();

    public IReadOnlyList<UnitEffect> ActiveEffects => activeEffects;
    public event Action EffectsChanged;

    public UnitEffectController(NewUnitBase owner)
    {
        this.owner = owner;
    }

    public void AddEffect(UnitEffect newEffect)
    {
        if (newEffect == null || newEffect.IsExpired)
            return;

        UnitEffect existingEffect =
            activeEffects.Find(effect =>
                effect.GetType() == newEffect.GetType()
            );

        if (existingEffect != null)
        {
            existingEffect.OnReapply(owner, newEffect);

            Debug.Log(
                $"{owner.data.name}의 " +
                $"{existingEffect.DisplayName} 스택: " +
                $"{existingEffect.Stacks}"
            );
        }
        else
        {
            activeEffects.Add(newEffect);
            newEffect.OnApply(owner);

            Debug.Log(
                $"{owner.data.name}에 " +
                $"{newEffect.DisplayName} 효과 적용"
            );
        }

        RemoveExpiredEffects();
        NotifyEffectsChanged();

    }

    public void Trigger(
        TriggerTiming timing,
        EffectTriggerContext context)
    {
        UnitEffect[] triggeredEffects =
            activeEffects
                .Where(effect =>
                    effect.RespondsTo(timing)
                )
                .OrderBy(effect =>
                    effect.Priority
                )
                .ToArray();

        foreach (UnitEffect effect in triggeredEffects)
            effect.OnTrigger(owner, context);

        RemoveExpiredEffects();
        NotifyEffectsChanged();

    }

    public int ModifyOutgoingDamage(
        int rawDamage,
        Symbol symbol,
        TurnContext turnContext = null)
    {
        int modifiedDamage = rawDamage;

        UnitEffect[] effects =
            activeEffects
                .OrderBy(effect =>
                    effect.Priority
                )
                .ToArray();

        foreach (UnitEffect effect in effects)
        {
            modifiedDamage =
                effect.ModifyOutgoingDamage(
                    owner,
                    modifiedDamage,
                    symbol,
                    turnContext
                );
        }

        RemoveExpiredEffects();
        NotifyEffectsChanged();


        return modifiedDamage;
    }

    public bool HasEffect<T>() where T : UnitEffect
    {
        return activeEffects.Any(effect =>
            effect is T && !effect.IsExpired
        );
    }

    public T GetEffect<T>() where T : UnitEffect
    {
        return activeEffects
            .OfType<T>()
            .FirstOrDefault(effect =>
                !effect.IsExpired
            );
    }

    public void RemoveEffect<T>() where T : UnitEffect
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (!(activeEffects[i] is T))
                continue;

            activeEffects[i].OnRemove(owner);
            activeEffects.RemoveAt(i);
        }

        NotifyEffectsChanged();

    }

    public void ClearTurnLimitedEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (!activeEffects[i].ClearOnTurnEnd)
                continue;

            activeEffects[i].OnRemove(owner);
            activeEffects.RemoveAt(i);
        }
        NotifyEffectsChanged();

    }

    public void ClearAll()
    {
        foreach (UnitEffect effect in activeEffects)
            effect.OnRemove(owner);

        activeEffects.Clear();
        NotifyEffectsChanged();

    }

    private void RemoveExpiredEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (!activeEffects[i].IsExpired)
                continue;

            activeEffects[i].OnRemove(owner);
            activeEffects.RemoveAt(i);
        }
    }

    private void NotifyEffectsChanged()
    {
        EffectsChanged?.Invoke();
    }

    public void OnDamageDealt(int damage, SymbolExecutionContext context)
    {
        if (damage <= 0)
            return;

        UnitEffect[] effects = activeEffects.OrderBy(effect => effect.Priority).ToArray();

        foreach (UnitEffect effect in effects)
            effect.OnDamageDealt(owner, damage, context);

        RemoveExpiredEffects();
        NotifyEffectsChanged();
    }
}