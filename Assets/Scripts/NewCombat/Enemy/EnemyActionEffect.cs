using UnityEngine;

[CreateAssetMenu(fileName = "EnemyActionEffect", menuName = "Scriptable Objects/EnemyActionEffect")]
public abstract class EnemyActionEffect : ScriptableObject
{
    public abstract void Apply(NewPlayer player, NewEnemy self);
}
