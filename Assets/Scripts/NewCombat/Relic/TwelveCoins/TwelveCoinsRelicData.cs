using UnityEngine;

[CreateAssetMenu(fileName = "TwelveCoinsRelic", menuName = "Relic/Twelve Coins")]
public class TwelveCoinsRelicData : RelicData
{
    [Header("12가지 동전 설정")]
    [SerializeField] private int requiredBluntCount = 3;
    [SerializeField] private int replayCount = 1;

    public int RequiredBluntCount => requiredBluntCount;
    public int ReplayCount => replayCount;

    public override RelicInstance CreateInstance()
    {
        return new TwelveCoinsRelicInstance(this);
    }
}