using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicTooltipUI : MonoBehaviour
{
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TMP_Text relicNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Vector2 tooltipGap = new Vector2(20f, 20f);

    private RectTransform canvasRect;

    private void Awake()
    {
        if (tooltipRect == null)
            tooltipRect = transform as RectTransform;

        if (rootCanvas != null)
            canvasRect = rootCanvas.transform as RectTransform;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
            graphic.raycastTarget = false;

        Hide();
    }

    public void Show(RelicData relicData, RectTransform iconRect)
    {
        if (relicData == null || iconRect == null || tooltipRect == null || rootCanvas == null || canvasRect == null)
            return;

        relicNameText.text = relicData.RelicName;
        descriptionText.text = relicData.Description;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        Canvas.ForceUpdateCanvases();

        SetPosition(iconRect);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetPosition(RectTransform iconRect)
    {
        RectTransform parentRect = tooltipRect.parent as RectTransform;

        if (parentRect == null)
            return;

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        Vector3[] iconCorners = new Vector3[4];
        iconRect.GetWorldCorners(iconCorners);

        // 0: 왼쪽 아래
        // 1: 왼쪽 위
        // 2: 오른쪽 위
        // 3: 오른쪽 아래
        Vector2 iconBottomRightScreenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, iconCorners[0]);

        Vector2 targetScreenPosition = iconBottomRightScreenPosition + new Vector2(Mathf.Abs(tooltipGap.x), -Mathf.Abs(tooltipGap.y));

        // 툴팁의 왼쪽 위 모서리가 아이콘 오른쪽 아래에 위치
        tooltipRect.pivot = new Vector2(0f, 1f);

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, targetScreenPosition, eventCamera, out Vector3 worldPosition))
            return;

        tooltipRect.position = worldPosition;

        ClampInsideCanvas();
    }

    private void ClampInsideCanvas()
    {
        if (canvasRect == null || tooltipRect == null)
            return;

        Vector3[] canvasCorners = new Vector3[4];
        Vector3[] tooltipCorners = new Vector3[4];

        canvasRect.GetWorldCorners(canvasCorners);
        tooltipRect.GetWorldCorners(tooltipCorners);

        Vector3 correction = Vector3.zero;

        float canvasLeft = canvasCorners[0].x;
        float canvasBottom = canvasCorners[0].y;
        float canvasRight = canvasCorners[2].x;
        float canvasTop = canvasCorners[2].y;

        float tooltipLeft = tooltipCorners[0].x;
        float tooltipBottom = tooltipCorners[0].y;
        float tooltipRight = tooltipCorners[2].x;
        float tooltipTop = tooltipCorners[2].y;

        if (tooltipLeft < canvasLeft)
            correction.x += canvasLeft - tooltipLeft;
        else if (tooltipRight > canvasRight)
            correction.x -= tooltipRight - canvasRight;

        if (tooltipBottom < canvasBottom)
            correction.y += canvasBottom - tooltipBottom;
        else if (tooltipTop > canvasTop)
            correction.y -= tooltipTop - canvasTop;

        tooltipRect.position += correction;
    }
}