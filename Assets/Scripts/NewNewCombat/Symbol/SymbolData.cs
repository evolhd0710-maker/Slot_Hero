using UnityEngine;

[CreateAssetMenu(
    fileName = "SymbolData",
    menuName = "Scriptable Objects/SymbolData"
)]
public class SymbolData : ScriptableObject
{
    [Header("문양 정보")]
    [SerializeField] private string symbolName;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("태그 조합")]
    [SerializeField] private SymbolTagData firstTag;
    [SerializeField] private SymbolTagData secondTag;

    [Header("문양 가치")]
    [SerializeField] private int value;

    public string SymbolName => symbolName;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public SymbolTagData FirstTag => firstTag;
    public SymbolTagData SecondTag => secondTag;

    public int Value => value;

    public bool HasTag(SymbolTagData tag)
    {
        return firstTag == tag ||
               secondTag == tag;
    }
}