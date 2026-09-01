public abstract class EnemyBrainRuntime
{
    protected NewEnemy Owner { get; private set; }
    protected EnemyDefinitionData Definition { get; private set; }

    public virtual void Initialize(NewEnemy owner, EnemyDefinitionData definition)
    {
        Owner = owner;
        Definition = definition;
    }

    public virtual void OnBattleStart()
    {
    }

    public abstract EnemyActionData ChooseAction(NewPlayer player, int turnNumber);

    public virtual void OnActionExecuted(EnemyActionData action, NewPlayer player, int turnNumber)
    {
    }

    public virtual void OnTurnCancelled(NewPlayer player, int turnNumber)
    {
    }

    public virtual void OnBattleEnd()
    {
    }
}