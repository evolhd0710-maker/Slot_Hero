using System.Collections.Generic;
using UnityEngine;

public class EagleFlagRelicInstance : RelicInstance
{
    private EagleFlagRelicData EagleFlagData => (EagleFlagRelicData)Data;

    private bool isActive;
    private int empireCountThisTurn;

    public EagleFlagRelicInstance(EagleFlagRelicData data) : base(data)
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
        turnContext.tagCounts.TryGetValue(SymbolTag.제국, out empireCountThisTurn);

        isActive = empireCountThisTurn >= EagleFlagData.RequiredEmpireCount;

        if (isActive)
            Debug.Log($"{Data.RelicName} 발동. 제국 태그: {empireCountThisTurn}, 제국 심볼 피해량: {EagleFlagData.DamageMultiplier:0.##}배");
    }

    public override int ModifySymbolPower(SymbolExecutionContext context, int currentPower)
    {
        if (!isActive || currentPower <= 0)
            return currentPower;

        if (context.Symbol == null || !context.Symbol.HasTag(SymbolTag.제국))
            return currentPower;

        int modifiedPower = Mathf.FloorToInt(currentPower * EagleFlagData.DamageMultiplier);

        Debug.Log($"{Data.RelicName} 적용. {context.Symbol.name} 공격력: {currentPower} → {modifiedPower}");

        return modifiedPower;
    }

    public override void OnTurnEnd(TurnContext turnContext)
    {
        ResetTurnState();
    }

    private void ResetTurnState()
    {
        isActive = false;
        empireCountThisTurn = 0;
    }
}
