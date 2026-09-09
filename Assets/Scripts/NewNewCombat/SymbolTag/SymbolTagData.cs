using UnityEngine;
//각 문양에 들어가는 태그 데이터 구조
public enum SymbolTagType
{
    Mercury,
    Venus,
    Earth,
    Mars,
    Jupiter,
    Saturn,
    Uranus,
    Neptune,
    Pluto
}
[CreateAssetMenu(fileName = "SymbolTagData", menuName = "Scriptable Objects/SymbolTagData")]
public class SymbolTagData : ScriptableObject
{
    [SerializeField] private SymbolTagType tagType;
    [SerializeField] private string displayName;
    [SerializeField] private int value;

    public SymbolTagType TagType => tagType;
    public string DisplayName => displayName;
    public int Value => value;
}
