using System;
using System.Collections.Generic;

[Serializable]
public class TagCalculationResult
{
    public SymbolTagType tagType;

    public string tagName;

    public int duplicateCount;

    public List<SymbolContribution> contributions =
        new List<SymbolContribution>();

    public int symbolValueSum;

    public int tagBetWeight;

    public int baseMultiplier;

    public int duplicateBetWeight;

    public int finalMultiplier;

    public int finalScore;
}