using UnityEngine;

[CreateAssetMenu(fileName = "ScyposSpecialEffect", menuName = "SpecialEffects/Scypos")]
public class ScyposSpecialEffect : SymbolEffect
{

    public override void Apply(SymbolExecutionContext context)
    {
        int damage = context.FinalAttack;

        if (damage <= 0)
            return;

        context.Enemy.TakeDamage(damage, "사이포스 특수효과");
        context.ReportDamage(damage);
        Debug.Log($"{context.Symbol.symbolType} 특수효과 즉시 피해: {damage}");
    }

}

