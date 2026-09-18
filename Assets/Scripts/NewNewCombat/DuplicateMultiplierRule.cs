using System;

[Serializable]
public class DuplicateMultiplierRule
{
    public int count;
    public int multiplier;

    public DuplicateMultiplierRule(
        int count,
        int multiplier
    )
    {
        this.count = count;
        this.multiplier = multiplier;
    }
}