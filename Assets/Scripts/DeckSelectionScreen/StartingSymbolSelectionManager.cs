using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingSymbolSelectionManager : MonoBehaviour
{
    [Header("선택 가능한 시작 덱 조합")]
    [SerializeField]
    private List<StartingDeckData> availableDecks =
        new List<StartingDeckData>();

    [Header("덱 카드 UI")]
    [SerializeField] private RectTransform deckCardContainer;
    [SerializeField] private DeckSelectionCardUI deckCardPrefab;

    [Header("선택 결과 UI")]
    [SerializeField] private TMP_Text selectedDeckText;
    [SerializeField] private Button nextButton;

    [Header("다음 화면")]
    [Tooltip("덱 선택 후 이동할 유물 선택 씬 이름")]
    [SerializeField] private string nextSceneName = "RelicSelectionScene";

    [Tooltip("새 게임 시작 시 이전 런 데이터를 초기화할지 여부")]
    [SerializeField] private bool clearPreviousRunOnStart = true;

    private readonly List<DeckSelectionCardUI> createdDeckCards =
        new List<DeckSelectionCardUI>();

    private DeckSelectionCardUI selectedDeckCard;
    private StartingDeckData selectedDeck;

    private bool isLoading;

    private void Awake()
    {
        EnsureRunSelectionData();
    }

    private void Start()
    {
        if (RunSelectionData.Instance == null)
        {
            Debug.LogError("RunSelectionData를 생성하지 못했습니다.", this);
            enabled = false;
            return;
        }

        // 런 초기화는 첫 번째 선택 화면인 덱 선택 화면에서만 실행한다.
        if (clearPreviousRunOnStart)
            RunSelectionData.Instance.ClearRun();

        CreateDeckCards();

        if (selectedDeckText != null)
            selectedDeckText.text = "시작 덱 조합을 선택하세요.";

        if (nextButton != null)
        {
            nextButton.interactable = false;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(GoToRelicSelection);
        }
        else
        {
            Debug.LogWarning(
                "StartingSelectionManager에 Next Button이 연결되지 않았습니다.",
                this
            );
        }
    }

    /// <summary>
    /// 런 데이터를 보관할 오브젝트가 없다면 생성한다.
    /// </summary>
    private void EnsureRunSelectionData()
    {
        if (RunSelectionData.Instance != null)
            return;

        GameObject runDataObject = new GameObject("RunSelectionData");
        runDataObject.AddComponent<RunSelectionData>();
    }

    /// <summary>
    /// Inspector에 등록된 덱 조합들의 선택 카드를 생성한다.
    /// </summary>
    private void CreateDeckCards()
    {
        if (deckCardContainer == null)
        {
            Debug.LogError(
                "덱 카드 Container가 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (deckCardPrefab == null)
        {
            Debug.LogError(
                "덱 카드 Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (availableDecks == null || availableDecks.Count == 0)
        {
            Debug.LogWarning(
                "선택 가능한 시작 덱 조합이 등록되지 않았습니다.",
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
    /// 덱 선택 카드에서 호출한다.
    /// </summary>
    public void SelectDeck(
        DeckSelectionCardUI card,
        StartingDeckData deck
    )
    {
        if (isLoading)
            return;

        if (card == null || deck == null)
            return;

        if (selectedDeckCard != null &&
            selectedDeckCard != card)
        {
            selectedDeckCard.SetSelected(false);
        }

        selectedDeckCard = card;
        selectedDeck = deck;

        selectedDeckCard.SetSelected(true);

        if (RunSelectionData.Instance != null)
            RunSelectionData.Instance.SelectDeck(deck);

        if (selectedDeckText != null)
        {
            selectedDeckText.text =
                $"선택한 덱 조합: {deck.DeckName}";
        }

        RefreshNextButton();
    }

    /// <summary>
    /// 덱이 선택되었을 때만 다음 버튼을 활성화한다.
    /// </summary>
    private void RefreshNextButton()
    {
        if (nextButton == null)
            return;

        nextButton.interactable =
            selectedDeck != null &&
            !isLoading;
    }

    /// <summary>
    /// 선택된 덱을 저장하고 유물 선택 화면으로 이동한다.
    /// </summary>
    private void GoToRelicSelection()
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
                "이동할 유물 선택 씬 이름이 비어 있습니다.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError(
                $"{nextSceneName} 씬이 Build Profiles에 등록되지 않았거나 씬 이름이 잘못되었습니다.",
                this
            );

            return;
        }

        isLoading = true;
        RefreshNextButton();

        // 유물 선택 화면으로 넘어가기 전에 덱 데이터를 다시 확정한다.
        RunSelectionData.Instance.SelectDeck(selectedDeck);

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(GoToRelicSelection);
    }
}