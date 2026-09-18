using System;

[Serializable]
public class TagBetRecord
{
    public SymbolTagType tagType;
    public BetCoinData coin;

    public TagBetRecord(
        SymbolTagType tagType,
        BetCoinData coin
    )
    {
        this.tagType = tagType;
        this.coin = coin;
    }
}