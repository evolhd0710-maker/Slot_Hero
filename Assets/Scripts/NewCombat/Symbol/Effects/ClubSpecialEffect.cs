using UnityEngine;

[CreateAssetMenu(menuName = "Symbol Effects/Club Stun")]
public class ClubStunEffect : SymbolEffect
{
    [SerializeField] private float chancePerAttack = 0.05f;
    [SerializeField] private float maximumChance = 1f;

    public override void Apply(SymbolExecutionContext context)
    {
        float stunChance = Mathf.Clamp(context.FinalAttack * chancePerAttack, 0f, maximumChance);
        float roll = Random.value;

        if (roll > stunChance)
            return;

        context.Enemy.ApplyEffect(new StunEffect(1));

        Debug.Log($"{context.Symbol.name} 기절 성공. 확률: {stunChance:P0}");
    }
}