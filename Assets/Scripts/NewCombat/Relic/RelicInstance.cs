using System;
using System.Collections.Generic;

public abstract class RelicInstance
{
    public RelicData Data { get; }
    public RelicController Controller { get; private set; }
    public NewPlayer Player { get; private set; }
    public NewEnemy Enemy { get; private set; }

    public int Priority => Data.Priority;
    public int AcquisitionOrder { get; internal set; }
    public bool IsInitialized { get; private set; }

    protected RelicInstance(RelicData data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    internal void Initialize(RelicController controller, NewPlayer player, NewEnemy enemy)
    {
        if (IsInitialized)
            return;

        Controller = controller;
        Player = player;
        Enemy = enemy;
        IsInitialized = true;

        OnAdded();
    }

    internal void Dispose()
    {
        if (!IsInitialized)
            return;

        OnRemoved();

        Controller = null;
        Player = null;
        Enemy = null;
        IsInitialized = false;
    }

    public virtual void OnAdded()
    {
    }

    public virtual void OnRemoved()
    {
    }

    public virtual void OnBattleStart(TurnContext turnContext)
    {
    }

    public virtual void OnTurnStart(TurnContext turnContext)
    {
    }

    public virtual void OnRollResolved(TurnContext turnContext, IReadOnlyList<Symbol> rolledSymbols)
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

    public virtual void OnPlayerDamaged(int damage, string reason)
    {
    }

    public virtual void OnTurnEnd(TurnContext turnContext)
    {
    }

    public virtual void OnBattleEnd(TurnContext turnContext)
    {
    }
}