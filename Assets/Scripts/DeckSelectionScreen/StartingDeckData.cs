using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartingDeck", menuName = "Game/Starting Deck")]
public class StartingDeckData : ScriptableObject
{
    [Header("표시 정보")]
    [SerializeField] private string deckName;
    [SerializeField] private string deckType;
    [TextArea(2, 5)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("시작 심볼 덱")]
    [SerializeField] private List<Symbol> symbols = new List<Symbol>();

    public string DeckName => deckName;
    public string Description => description;
    public string DeckType => deckType;
    public Sprite Icon => icon;
    public IReadOnlyList<Symbol> Symbols => symbols;
}