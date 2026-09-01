using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicSelectionCardUI : MonoBehaviour
{
    [Header("유물 조합 카드 UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image relicIcon;
    [SerializeField] private TMP_Text relicNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject selectedFrame;

    private StartingRelicSetData relicSetData;
    private StartingRelicSelectionManager selectionManager;

    public StartingRelicSetData RelicSetData => relicSetData;

    private void Awake()
    {
        if (selectButton == null)
            selectButton = GetComponent<Button>();

        if (selectButton == null)
            selectButton = GetComponentInChildren<Button>();
    }

    /// <summary>
    /// 카드에 유물 조합 데이터와 선택 매니저를 연결한다.
    /// </summary>
    public void Bind(
        StartingRelicSetData data,
        StartingRelicSelectionManager manager
    )
    {
        relicSetData = data;
        selectionManager = manager;

        RefreshCardUI();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnCardClicked);
        }

        SetSelected(false);
    }

    /// <summary>
    /// 유물 조합의 이름, 설명, 대표 아이콘을 표시한다.
    /// </summary>
    private void RefreshCardUI()
    {
        if (relicNameText != null)
        {
            relicNameText.text = relicSetData != null
                ? relicSetData.SetName
                : string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text = relicSetData != null
                ? BuildDescriptionText(relicSetData)
                : string.Empty;
        }

        if (relicIcon != null)
        {
            Sprite representativeIcon =
                GetRepresentativeIcon(relicSetData);

            relicIcon.sprite = representativeIcon;
            relicIcon.enabled = representativeIcon != null;
            relicIcon.preserveAspect = true;
            relicIcon.raycastTarget = false;
        }
    }

    /// <summary>
    /// 세트 설명 아래에 포함된 유물 이름을 표시한다.
    /// </summary>
    private string BuildDescriptionText(
        StartingRelicSetData data
    )
    {
        if (data == null)
            return string.Empty;

        List<string> relicNames = new List<string>();

        if (data.Relics != null)
        {
            foreach (RelicData relic in data.Relics)
            {
                if (relic == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(relic.RelicName))
                    relicNames.Add(relic.RelicName);
            }
        }

        string includedRelicsText = relicNames.Count > 0
            ? $"{string.Join(", ", relicNames)}"
            : "포함된 유물이 없습니다.";

        if (string.IsNullOrWhiteSpace(data.Description))
            return includedRelicsText;

        return $"{data.Description}\n\n{includedRelicsText}";
    }

    /// <summary>
    /// 세트에 포함된 첫 번째 유효한 유물 아이콘을 대표 이미지로 사용한다.
    /// </summary>
    private Sprite GetRepresentativeIcon(
        StartingRelicSetData data
    )
    {
        if (data == null || data.Relics == null)
            return null;

        foreach (RelicData relic in data.Relics)
        {
            if (relic != null && relic.Icon != null)
                return relic.Icon;
        }

        return null;
    }

    private void OnCardClicked()
    {
        if (relicSetData == null ||
            selectionManager == null)
        {
            return;
        }

        selectionManager.SelectRelic(
            this,
            relicSetData
        );
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
            selectedFrame.SetActive(selected);
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnCardClicked);
    }
}
