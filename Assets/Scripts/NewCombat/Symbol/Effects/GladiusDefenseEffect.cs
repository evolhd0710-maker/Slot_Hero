using UnityEngine;

[CreateAssetMenu(fileName = "GladiusDefenseEffect", menuName = "Effects/Gladius Defense")]
public class GladiusDefenseEffect : SymbolEffect
{
    public override void Apply(SymbolExecutionContext context)
    {
        int defense = context.FinalDefense + context.CountInTurn;
        context.TurnContext.totalDefense += defense;
        context.ReportDefense(defense);
    }
}