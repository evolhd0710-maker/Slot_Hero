using UnityEngine;
[CreateAssetMenu(fileName = "ScutumSpecailEffect", menuName = "SpecialEffects/Scutum")]
public class ScutumSpecailEffect : SymbolEffect
{
    public int amount = 1;

    public override void Apply(SymbolExecutionContext context)
    {
        context.Player.ApplyEffect(new TestudoBuff(amount));
    }
}
