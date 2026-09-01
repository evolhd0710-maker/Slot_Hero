using UnityEngine;

[CreateAssetMenu(fileName = "MonumentEffect", menuName = "Effects/MonumentEffect")]
public class MonumentEffect : SymbolEffect
{
    [SerializeField] private int amount = 1;

    public override void Apply(SymbolExecutionContext context)
    {
        // ===== 변경 시작: 잘못된 버프 수치 및 Context 방어 =====

        if (context == null || context.Player == null || amount <= 0)
            return;

        // ===== 변경 끝 =====

        context.Player.ApplyEffect(new StrengthBuff(amount));
    }
}
