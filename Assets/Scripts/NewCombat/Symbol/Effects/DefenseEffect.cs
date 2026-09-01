using UnityEngine;

[CreateAssetMenu(menuName = "Symbol Effects/Defense")]
public class DefenseEffect : SymbolEffect
{
    public override void Apply(SymbolExecutionContext context)
    {
        int defense = context.FinalDefense;

        if (defense <= 0)
            return;

        context.TurnContext.totalDefense += defense;
        context.ReportDefense(defense);

        Debug.Log($"{context.Symbol.name} 예약 방어도{defense} : 현재 누적 방어도: {context.TurnContext.totalDefense} ");
    }
}
