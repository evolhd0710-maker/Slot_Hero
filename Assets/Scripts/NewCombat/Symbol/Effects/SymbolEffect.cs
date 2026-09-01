using UnityEngine;

public abstract class SymbolEffect : ScriptableObject
{
        public abstract void Apply(SymbolExecutionContext context);
}
