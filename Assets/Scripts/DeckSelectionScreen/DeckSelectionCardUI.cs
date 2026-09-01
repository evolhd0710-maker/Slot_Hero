using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelectionCardUI : MonoBehaviour
{
    [Header("µ¦ Ä«µå UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image deckIcon;
    [SerializeField] private TMP_Text deckNameText;
    [SerializeField] private TMP_Text deckTypeText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject selectedFrame;

    private StartingDeckData deckData;
    private StartingSymbolSelectionManager selectionManager;

    private void Awake()
    {
        if (selectButton == null)
            selectButton = GetComponent<Button>();
    }

    public void Bind(StartingDeckData data, StartingSymbolSelectionManager manager)
    {

        gameObject.SetActive(true);

        if (selectButton == null)
            selectButton = GetComponent<Button>();

        if (selectButton != null)
            selectButton.interactable = true;

        deckData = data;
        selectionManager = manager;

        if (deckNameText != null)
            deckNameText.text = data != null ? data.DeckName : string.Empty;

        if (deckTypeText != null)
            deckTypeText.text = data != null ? data.DeckType : string.Empty;

        if (descriptionText != null)
            descriptionText.text = data != null ? data.Description : string.Empty;

        if (deckIcon != null)
        {
            deckIcon.sprite = data != null ? data.Icon : null;
            deckIcon.enabled = data != null && data.Icon != null;
            deckIcon.preserveAspect = true;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnCardClicked);
        }

        SetSelected(false);
    }

    private void OnCardClicked()
    {
        if (deckData == null || selectionManager == null)
            return;

        selectionManager.SelectDeck(this, deckData);
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
            selectedFrame.SetActive(selected);
    }
}