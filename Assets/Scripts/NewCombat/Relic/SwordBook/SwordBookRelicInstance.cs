using System.Collections.Generic;
using UnityEngine;

public class SwordBookRelicInstance : RelicInstance
{
    private SwordBookRelicData SwordBookData => (SwordBookRelicData)Data;

    private int swordCountThisTurn;
    private int attackBonusThisTurn;

    public int SwordCountThisTurn => swordCountThisTurn;
    public int AttackBonusThisTurn => attackBonusThisTurn;

    public SwordBookRelicInstance(SwordBookRelicData data) : base(data)
    {
    }

    public override void OnBattleStart(TurnContext turnContext)
    {
        ResetTurnState();
    }

    public override void OnTurnStart(TurnContext turnContext)
    {
        ResetTurnState();
    }

    public override void OnRollResolved(TurnContext turnContext, IReadOnlyList<Symbol> rolledSymbols)
    {
        turnContext.tagCounts.TryGetValue(SymbolTag.도검, out swordCountThisTurn);
        attackBonusThisTurn = swordCountThisTurn * SwordBookData.AttackPerSword;

        if (attackBonusThisTurn > 0)
            Debug.Log($"검술 교본 발동: 도검 태그 {swordCountThisTurn}개, 도검 문양 공격력 +{attackBonusThisTurn}");
    }

    public override int ModifySymbolPower(SymbolExecutionContext context, int currentPower)
    {
        if (context.Symbol == null || !context.Symbol.HasTag(SymbolTag.도검))
            return currentPower;

        return currentPower + attackBonusThisTurn;
    }

    public override void OnTurnEnd(TurnContext turnContext)
    {
        ResetTurnState();
    }

    private void ResetTurnState()
    {
        swordCountThisTurn = 0;
        attackBonusThisTurn = 0;
    }
}
