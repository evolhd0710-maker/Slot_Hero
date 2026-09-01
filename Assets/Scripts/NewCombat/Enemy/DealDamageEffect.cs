using UnityEngine;

[CreateAssetMenu(fileName = "DealDamageEffect", menuName = "Enemy/EnemyActionEffect/DealDamage")]
public class DealDamageEffect : EnemyActionEffect
{
    public int damage;

    public override void Apply(NewPlayer player, NewEnemy self)
    {
        player.TakeDamage(damage, self.data.name);
    }
}