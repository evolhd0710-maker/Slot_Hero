using System;
using System.Collections.Generic;

[Serializable]
public class SymbolCalculationResult
{
    public int totalDamage;

    public List<TagCalculationResult> tagResults =
        new List<TagCalculationResult>();
}