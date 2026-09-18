using System;

[Serializable]
public class SymbolContribution
{
    public int reelIndex;
    public SymbolData symbol;
    public int value;

    public SymbolContribution(
        int reelIndex,
        SymbolData symbol,
        int value
    )
    {
        this.reelIndex = reelIndex;
        this.symbol = symbol;
        this.value = value;
    }
}