using UnityEngine;

[CreateAssetMenu(
    fileName = "BetCoinData",
    menuName = "Scriptable Objects/BetCoinData"
)]
public class BetCoinData : ScriptableObject
{
    [Header("코인 정보")]
    [SerializeField] private string coinName;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("구매")]
    [SerializeField, Min(0)] private int healthCost;

    [Header("베팅 가중치")]
    [SerializeField, Min(0)] private int weight;

    public string CoinName => coinName;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public int HealthCost => healthCost;
    public int Weight => weight;
}