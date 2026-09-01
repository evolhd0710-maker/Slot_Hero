using System.Collections.Generic;
using UnityEngine;

public class ObsidianBladeRelicInstance : RelicInstance
{
    private ObsidianBladeRelicData ObsidianBladeData => (ObsidianBladeRelicData)Data;

    public ObsidianBladeRelicInstance(ObsidianBladeRelicData data) : base(data)
    {
    }

    public override void OnRollResolved(TurnContext turnContext, IReadOnlyList<Symbol> rolledSymbols)
    {
        turnContext.tagCounts.TryGetValue(SymbolTag.신전, out int templeCount);

        if (templeCount < ObsidianBladeData.RequiredTempleCount)
            return;

        int lifestealAmount = templeCount * ObsidianBladeData.LifestealPerTemple;

        Player.ApplyEffect(new DrainBuff(lifestealAmount));
        Debug.Log($"{Data.RelicName} 발동. 신전 태그: {templeCount}, 흡혈 +{lifestealAmount}");
    }
}
