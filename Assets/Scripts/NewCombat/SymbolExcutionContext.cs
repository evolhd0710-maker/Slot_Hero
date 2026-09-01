using UnityEngine;

public class SymbolExecutionContext
{
    public NewPlayer Player { get; }
    public NewEnemy Enemy { get; }
    public TurnContext TurnContext { get; }
    public RelicController RelicController { get; }

    public Symbol Symbol { get; }
    public SymbolExecutionContext ParentExecution { get; }

    public int CountInTurn { get; }

    public int BaseAttack { get; }
    public int FinalAttack { get; private set; }

    public int BaseDefense { get; }
    public int FinalDefense { get; private set; }

    public int DamageDealt { get; private set; }
    public int DefenseGained { get; private set; }

    public bool Cancelled { get; set; }

    public int BasePower => BaseAttack;
    public int FinalPower => FinalAttack;
    public bool IsReplay { get; }

    public SymbolExecutionContext(NewPlayer player, NewEnemy enemy, TurnContext turnContext, Symbol symbol, int countInTurn, int baseAttack, int baseDefense, RelicController relicController, bool isReplay)
    {
        Player = player;
        Enemy = enemy;
        TurnContext = turnContext;
        RelicController = relicController;
        Symbol = symbol;
        CountInTurn = countInTurn;

        BaseAttack = Mathf.Max(0, baseAttack);
        FinalAttack = BaseAttack;

        BaseDefense = Mathf.Max(0, baseDefense);
        FinalDefense = BaseDefense;

        ParentExecution = turnContext?.CurrentSymbolExecution;
        IsReplay = isReplay;
    }

    public void SetFinalAttack(int value)
    {
        FinalAttack = Mathf.Max(0, value);
    }

    public void SetFinalDefense(int value)
    {
        FinalDefense = Mathf.Max(0, value);
    }

    public void SetFinalPower(int value)
    {
        SetFinalAttack(value);
    }

    public void ReportDamage(int amount)
    {
        if (amount <= 0)
            return;

        DamageDealt += amount;
        Player?.NotifyDamageDealt(amount, this);
        RelicController?.OnDamageDealt(this, amount);
    }

    public void ReportDefense(int amount)
    {
        if (amount <= 0)
            return;

        DefenseGained += amount;
    }
}