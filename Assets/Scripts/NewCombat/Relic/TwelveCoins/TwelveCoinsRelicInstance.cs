using System.Collections.Generic;
using UnityEngine;

public class TwelveCoinsRelicInstance : RelicInstance
{
    private TwelveCoinsRelicData TwelveCoinsData => (TwelveCoinsRelicData)Data;

    private bool isActive;
    private bool replayUsedThisTurn;
    private int bluntCountThisTurn;

    public TwelveCoinsRelicInstance(TwelveCoinsRelicData data) : base(data)
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
        if (turnContext == null)
        {
            ResetTurnState();
            return;
        }

        turnContext.tagCounts.TryGetValue(SymbolTag.둔기, out bluntCountThisTurn);
        isActive = bluntCountThisTurn >= TwelveCoinsData.RequiredBluntCount;

        if (isActive)
            Debug.Log($"{Data.RelicName} 발동 준비. 둔기 태그 개수: {bluntCountThisTurn}");
    }

    public override void AfterSymbolExecute(SymbolExecutionContext context)
    {
        if (!isActive || replayUsedThisTurn)
            return;

        if (context == null || context.Symbol == null || context.Cancelled)
            return;

        // 재사용된 심볼이 다시 이 유물을 발동하는 것을 방지
        if (context.IsReplay)
            return;

        if (!context.Symbol.HasTag(SymbolTag.둔기))
            return;

        // 심볼이 왼쪽부터 실행되므로 처음 만나는 둔기가 가장 왼쪽 둔기
        replayUsedThisTurn = true;
        context.TurnContext.RequestSymbolReplay(context.Symbol, TwelveCoinsData.ReplayCount);

        Debug.Log($"{Data.RelicName} 발동. 가장 왼쪽 둔기 {context.Symbol.name} 재사용 {TwelveCoinsData.ReplayCount}회");
    }

    public override void OnTurnEnd(TurnContext turnContext)
    {
        ResetTurnState();
    }

    private void ResetTurnState()
    {
        isActive = false;
        replayUsedThisTurn = false;
        bluntCountThisTurn = 0;
    }
}
