using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image relicImage;

    private RelicData relicData;
    private RelicTooltipUI tooltipUI;

    public void Bind(RelicInstance relicInstance, RelicTooltipUI tooltip)
    {
        if (relicInstance == null)
        {
            Clear();
            return;
        }

        relicData = relicInstance.Data;
        tooltipUI = tooltip;

        if (relicData == null)
        {
            Debug.LogError($"{name}에 전달된 RelicData가 없습니다.");
            Clear();
            return;
        }

        if (relicImage == null)
        {
            Debug.LogError($"{name}의 Relic Image가 연결되지 않았습니다.");
            return;
        }

        relicImage.sprite = relicData.Icon;
        relicImage.enabled = relicData.Icon != null;
        relicImage.color = Color.white;
        relicImage.preserveAspect = true;

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        relicData = null;
        tooltipUI = null;

        if (relicImage != null)
        {
            relicImage.sprite = null;
            relicImage.enabled = false;
        }

        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (relicData == null || tooltipUI == null || relicImage == null)
            return;

        tooltipUI.Show(relicData, relicImage.rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipUI?.Hide();
    }

    private void OnDisable()
    {
        tooltipUI?.Hide();
    }

    private void Reset()
    {
        relicImage = GetComponentInChildren<Image>(true);
    }
}