using UnityEngine;

public abstract class EnemyBrainData : ScriptableObject
{
    public abstract EnemyBrainRuntime CreateRuntime();
}
