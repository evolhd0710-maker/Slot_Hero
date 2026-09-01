using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingRelicSelectionManager : MonoBehaviour
{
    [Header("선택 가능한 시작 유물 조합")]
    [SerializeField]
    private List<StartingRelicSetData> availableRelicSets =
        new List<StartingRelicSetData>();

    [Header("유물 조합 카드 UI")]
    [SerializeField] private RectTransform relicCardContainer;
    [SerializeField] private RelicSelectionCardUI relicCardPrefab;

    [Header("선택 결과 UI")]
    [SerializeField] private TMP_Text selectedRelicText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button previousButton;

    [Header("씬 이동")]
    [Tooltip("유물 선택 후 이동할 맵 씬 이름")]
    [SerializeField] private string nextSceneName = "MapScene";

    [Tooltip("이전 단계인 슬롯 조합 선택 씬 이름")]
    [SerializeField]
    private string previousSceneName =
        "StartingSymbolSelectionScene";

    private readonly List<RelicSelectionCardUI> createdRelicCards =
        new List<RelicSelectionCardUI>();

    private RelicSelectionCardUI selectedRelicCard;
    private StartingRelicSetData selectedRelicSet;

    private bool isLoading;

    private void Awake()
    {
        EnsureRunSelectionData();
    }

    private void Start()
    {
        if (RunSelectionData.Instance == null)
        {
            Debug.LogError(
                "RunSelectionData를 생성하지 못했습니다.",
                this
            );

            enabled = false;
            return;
        }

        // 이 화면에서는 ClearRun을 호출하지 않는다.
        // 앞의 슬롯 선택 화면에서 고른 덱을 유지해야 한다.
        if (!RunSelectionData.Instance.HasDeckSelection)
        {
            Debug.LogWarning(
                "선택된 시작 슬롯 조합이 없습니다. 슬롯 선택 화면을 거쳐야 합니다.",
                this
            );
        }

        CreateRelicCards();

        if (selectedRelicText != null)
            selectedRelicText.text = "시작 유물 조합을 선택하세요.";

        SetupButtons();
        RestorePreviousSelection();
        RefreshButtons();
    }

    private void SetupButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogWarning(
                "StartingRelicSelectionManager에 Start Button이 연결되지 않았습니다.",
                this
            );
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(GoToPreviousStep);
        }
        else
        {
            Debug.LogWarning(
                "StartingRelicSelectionManager에 Previous Button이 연결되지 않았습니다.",
                this
            );
        }
    }

    private void EnsureRunSelectionData()
    {
        if (RunSelectionData.Instance != null)
            return;

        GameObject runDataObject =
            new GameObject("RunSelectionData");

        runDataObject.AddComponent<RunSelectionData>();
    }

    private void CreateRelicCards()
    {
        if (relicCardContainer == null)
        {
            Debug.LogError(
                "유물 카드 Container가 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (relicCardPrefab == null)
        {
            Debug.LogError(
                "유물 카드 Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (availableRelicSets == null ||
            availableRelicSets.Count == 0)
        {
            Debug.LogWarning(
                "선택 가능한 시작 유물 조합이 등록되지 않았습니다.",
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

    private void RestorePreviousSelection()
    {
        if (RunSelectionData.Instance == null)
            return;

        StartingRelicSetData previousRelicSet =
            RunSelectionData.Instance.SelectedStartingRelicSet;

        if (previousRelicSet == null)
            return;

        foreach (RelicSelectionCardUI card in createdRelicCards)
        {
            if (card == null ||
                card.RelicSetData != previousRelicSet)
            {
                continue;
            }

            selectedRelicCard = card;
            selectedRelicSet = previousRelicSet;

            selectedRelicCard.SetSelected(true);

            if (selectedRelicText != null)
            {
                selectedRelicText.text =
                    $"선택한 유물 조합: {selectedRelicSet.SetName}";
            }

            break;
        }
    }

    public void SelectRelic(
        RelicSelectionCardUI card,
        StartingRelicSetData relicSet
    )
    {
        if (isLoading)
            return;

        if (card == null || relicSet == null)
            return;

        if (selectedRelicCard != null &&
            selectedRelicCard != card)
        {
            selectedRelicCard.SetSelected(false);
        }

        selectedRelicCard = card;
        selectedRelicSet = relicSet;

        selectedRelicCard.SetSelected(true);

        if (RunSelectionData.Instance != null)
        {
            RunSelectionData.Instance.SelectStartingRelicSet(
                relicSet
            );
        }

        if (selectedRelicText != null)
        {
            selectedRelicText.text =
                $"선택한 유물 조합: {relicSet.SetName}";
        }

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (startButton != null)
        {
            bool hasDeck =
                RunSelectionData.Instance != null &&
                RunSelectionData.Instance.HasDeckSelection;

            startButton.interactable =
                hasDeck &&
                selectedRelicSet != null &&
                !isLoading;
        }

        if (previousButton != null)
            previousButton.interactable = !isLoading;
    }

    private void GoToPreviousStep()
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(previousSceneName))
        {
            Debug.LogError(
                "이전 슬롯 선택 씬 이름이 비어 있습니다.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(previousSceneName))
        {
            Debug.LogError(
                $"{previousSceneName} 씬이 Build Profiles에 등록되지 않았거나 씬 이름이 잘못되었습니다.",
                this
            );

            return;
        }

        isLoading = true;
        RefreshButtons();

        SceneManager.LoadScene(previousSceneName);
    }

    private void StartGame()
    {
        if (isLoading)
            return;

        if (RunSelectionData.Instance == null)
        {
            Debug.LogError(
                "RunSelectionData가 없습니다.",
                this
            );

            return;
        }

        if (!RunSelectionData.Instance.HasDeckSelection)
        {
            Debug.LogWarning(
                "시작 슬롯 조합이 선택되지 않았습니다.",
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

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError(
                "이동할 맵 씬 이름이 비어 있습니다.",
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

        RunSelectionData.Instance.SelectStartingRelicSet(
            selectedRelicSet
        );

        if (!RunSelectionData.Instance.IsReady)
        {
            Debug.LogError(
                "시작 슬롯 또는 시작 유물 데이터가 완성되지 않았습니다.",
                this
            );

            return;
        }

        isLoading = true;
        RefreshButtons();

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);

        if (previousButton != null)
            previousButton.onClick.RemoveListener(GoToPreviousStep);
    }
}