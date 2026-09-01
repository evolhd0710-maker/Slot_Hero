/*
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingSelectionManager : MonoBehaviour
{
    [Header("선택 가능한 시작 덱 조합")]
    [SerializeField]
    private List<StartingDeckData> availableDecks =
        new List<StartingDeckData>();

    [Header("선택 가능한 시작 유물 조합")]
    [SerializeField]
    private List<StartingRelicSetData> availableRelicSets =
        new List<StartingRelicSetData>();

    [Header("덱 카드 UI")]
    [SerializeField] private RectTransform deckCardContainer;
    [SerializeField] private DeckSelectionCardUI deckCardPrefab;

    [Header("유물 조합 카드 UI")]
    [SerializeField] private RectTransform relicCardContainer;
    [SerializeField] private RelicSelectionCardUI relicCardPrefab;

    [Header("선택 결과 UI")]
    [SerializeField] private Button startButton;

    [Header("게임 시작")]
    [SerializeField] private string nextSceneName = "MapScene";
    [SerializeField] private bool clearPreviousRunOnStart = true;

    private readonly List<DeckSelectionCardUI> createdDeckCards =
        new List<DeckSelectionCardUI>();

    private readonly List<RelicSelectionCardUI> createdRelicCards =
        new List<RelicSelectionCardUI>();

    private DeckSelectionCardUI selectedDeckCard;
    private RelicSelectionCardUI selectedRelicCard;

    private StartingDeckData selectedDeck;
    private StartingRelicSetData selectedRelicSet;

    private bool isLoading;

    private void Awake()
    {
        EnsureRunSelectionData();
    }

    private void Start()
    {
        if (clearPreviousRunOnStart)
            RunSelectionData.Instance.ClearRun();

        CreateDeckCards();
        CreateRelicCards();


        if (startButton != null)
        {
            startButton.interactable = false;
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
    }

    /// <summary>
    /// 런 선택 데이터를 보관할 오브젝트가 없다면 새로 생성한다.
    /// </summary>
    private void EnsureRunSelectionData()
    {
        if (RunSelectionData.Instance != null)
            return;

        GameObject runDataObject = new GameObject("RunSelectionData");
        runDataObject.AddComponent<RunSelectionData>();
    }

    /// <summary>
    /// Inspector에 등록된 시작 덱 조합만큼 선택 카드를 생성한다.
    /// </summary>
    private void CreateDeckCards()
    {
        if (deckCardContainer == null || deckCardPrefab == null)
        {
            Debug.LogError(
                "덱 카드 Container 또는 Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }

        foreach (StartingDeckData deck in availableDecks)
        {
            if (deck == null)
                continue;

            DeckSelectionCardUI card = Instantiate(
                deckCardPrefab,
                deckCardContainer,
                false
            );

            card.Bind(deck, this);
            card.SetSelected(false);

            createdDeckCards.Add(card);
        }
    }

    /// <summary>
    /// Inspector에 등록된 시작 유물 조합만큼 선택 카드를 생성한다.
    /// </summary>
    private void CreateRelicCards()
    {
        if (relicCardContainer == null || relicCardPrefab == null)
        {
            Debug.LogError(
                "유물 조합 카드 Container 또는 Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }

        foreach (StartingRelicSetData relicSet in availableRelicSets)
        {
            if (relicSet == null)
                continue;

            RelicSelectionCardUI card = Instantiate(
                relicCardPrefab,
                relicCardContainer,
                false
            );

            card.Bind(relicSet, this);
            card.SetSelected(false);

            createdRelicCards.Add(card);
        }
    }

    /// <summary>
    /// 시작 덱 조합 하나를 선택한다.
    /// </summary>
    public void SelectDeck(
        DeckSelectionCardUI card,
        StartingDeckData deck
    )
    {
        if (card == null || deck == null)
            return;

        if (selectedDeckCard != null)
            selectedDeckCard.SetSelected(false);

        selectedDeckCard = card;
        selectedDeck = deck;

        selectedDeckCard.SetSelected(true);

        if (RunSelectionData.Instance != null)
            RunSelectionData.Instance.SelectDeck(deck);

        RefreshStartButton();
    }

    /// <summary>
    /// 시작 유물 조합 하나를 선택한다.
    /// 해당 조합 안에 들어 있는 모든 유물을 게임 시작 시 적용한다.
    /// </summary>
    public void SelectRelic(
        RelicSelectionCardUI card,
        StartingRelicSetData relicSet
    )
    {
        if (card == null || relicSet == null)
            return;

        if (selectedRelicCard != null)
            selectedRelicCard.SetSelected(false);

        selectedRelicCard = card;
        selectedRelicSet = relicSet;

        selectedRelicCard.SetSelected(true);

        if (RunSelectionData.Instance != null)
        {
            RunSelectionData.Instance.SelectStartingRelicSet(
                relicSet
            );
        }

        RefreshStartButton();
    }

    /// <summary>
    /// 덱 조합과 유물 조합을 모두 선택해야 시작 버튼을 활성화한다.
    /// </summary>
    private void RefreshStartButton()
    {
        if (startButton == null)
            return;

        startButton.interactable =
            selectedDeck != null &&
            selectedRelicSet != null &&
            !isLoading;
    }

    /// <summary>
    /// 선택된 덱 조합과 유물 조합을 런 데이터에 저장하고 맵 씬으로 이동한다.
    /// </summary>
    private void StartGame()
    {
        if (isLoading)
            return;

        if (selectedDeck == null)
        {
            Debug.LogWarning(
                "시작 덱 조합을 선택해야 합니다.",
                this
            );

            return;
        }

        if (selectedRelicSet == null)
        {
            Debug.LogWarning(
                "시작 유물 조합을 선택해야 합니다.",
                this
            );

            return;
        }

        if (RunSelectionData.Instance == null)
        {
            Debug.LogError(
                "RunSelectionData가 없습니다.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError(
                "이동할 씬 이름이 비어 있습니다.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError(
                $"{nextSceneName} 씬이 Build Profiles에 등록되지 않았거나 이름이 잘못되었습니다.",
                this
            );

            return;
        }

        isLoading = true;
        RefreshStartButton();

        RunSelectionData.Instance.SelectDeck(selectedDeck);

        RunSelectionData.Instance.SelectStartingRelicSet(
            selectedRelicSet
        );

        SceneManager.LoadScene(nextSceneName);
    }
}
*/