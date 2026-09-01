using UnityEngine;

[CreateAssetMenu(menuName = "Symbol Effects/Damage")]
public class DamageEffect : SymbolEffect
{
    public override void Apply(SymbolExecutionContext context)
    {
        int damage = context.FinalPower;

        if (damage <= 0)
            return;

        context.TurnContext.totalDamage += damage;
        Debug.Log($"{context.Symbol.symbolType} 예약 피해: {damage}, 현재 누적 피해: {context.TurnContext.totalDamage}");

    }
}