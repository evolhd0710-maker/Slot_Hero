using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StartingRelicSet",
    menuName = "Game/Starting Relic Set"
)]
public class StartingRelicSetData : ScriptableObject
{
    [Header("유물 세트 정보")]
    [SerializeField] private string setName;
    [TextArea]
    [SerializeField] private string description;

    [Header("이 세트에 포함된 유물")]
    [SerializeField] private List<RelicData> relics = new List<RelicData>();

    public string SetName => setName;
    public string Description => description;
    public IReadOnlyList<RelicData> Relics => relics;
}