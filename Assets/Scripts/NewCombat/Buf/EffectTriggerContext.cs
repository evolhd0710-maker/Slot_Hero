public class EffectTriggerContext
{
    public TurnContext TurnContext { get; }
    public NewUnitBase Source { get; }
    public NewUnitBase Target { get; }
    public Symbol Symbol { get; }

    public bool ActionCancelled
    {
        get =>
            TurnContext != null &&
            TurnContext.actionCancelled;

        set
        {
            if (TurnContext != null)
                TurnContext.actionCancelled = value;
        }
    }

    public EffectTriggerContext(
        TurnContext turnContext,
        NewUnitBase source = null,
        NewUnitBase target = null,
        Symbol symbol = null)
    {
        TurnContext = turnContext;
        Source = source;
        Target = target;
        Symbol = symbol;
    }
}