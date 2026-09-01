using System.Collections.Generic;
using UnityEngine;

public abstract class RelicEffect : ScriptableObject
{
    [Header("유물 정보")]
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;

    public virtual int Priority => 0;

    public virtual void OnAdded(NewPlayer player, NewEnemy enemy)
    {
    }

    public virtual void OnRemoved(NewPlayer player, NewEnemy enemy)
    {
    }

    public virtual void OnBattleStart(NewPlayer player, NewEnemy enemy, TurnContext turnContext)
    {
    }

    public virtual void OnTurnStart(NewPlayer player, NewEnemy enemy, TurnContext turnContext)
    {
    }

    public virtual void OnRollResolved(NewPlayer player, NewEnemy enemy, TurnContext turnContext, IReadOnlyList<Symbol> rolledSymbols)
    {
    }

    public virtual void BeforeSymbolExecute(SymbolExecutionContext context)
    {
    }

    public virtual int ModifySymbolPower(SymbolExecutionContext context, int currentPower)
    {
        return currentPower;
    }

    public virtual void AfterSymbolExecute(SymbolExecutionContext context)
    {
    }

    public virtual void OnDamageDealt(SymbolExecutionContext context, int damage)
    {
    }

    public virtual void OnPlayerDamaged(NewPlayer player, NewEnemy enemy, int damage, string reason)
    {
    }

    public virtual void OnTurnEnd(NewPlayer player, NewEnemy enemy, TurnContext turnContext)
    {
    }

    public virtual void OnBattleEnd(NewPlayer player, NewEnemy enemy, TurnContext turnContext)
    {
    }
}
