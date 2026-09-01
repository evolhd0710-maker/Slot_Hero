using UnityEngine;

public enum RelicRarity
{
    Common,
    Uncommon,
    Rare,
    Boss,
    Special
}

public abstract class RelicData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string relicId;
    [SerializeField] private string relicName;

    [TextArea]
    [SerializeField] private string relicDescription;

    [SerializeField] private Sprite icon;
    [SerializeField] private RelicRarity rarity;

    [Header("실행 설정")]
    [SerializeField] private int priority;
    [SerializeField] private bool allowDuplicates;

    public string RelicId => relicId;
    public string RelicName => relicName;
    public string Description => relicDescription;
    public Sprite Icon => icon;
    public RelicRarity Rarity => rarity;
    public int Priority => priority;
    public bool AllowDuplicates => allowDuplicates;

    public abstract RelicInstance CreateInstance();

    protected virtual void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(relicId))
            relicId = name;
    }
}