using UnityEngine;

[CreateAssetMenu(fileName = "GainShieldEffect", menuName = "Enemy/EnemyActionEffect/GainShield")]
public class GainShieldEffect : EnemyActionEffect
{
    public int amount;

    public override void Apply(NewPlayer player, NewEnemy self)
    {
        self.AddShield(amount);
    }
}
