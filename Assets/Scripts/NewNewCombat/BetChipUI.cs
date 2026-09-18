using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetChipUI : MonoBehaviour
{
    [Header("코인 표시")]
    [SerializeField]
    private Image coinImage;

    [SerializeField]
    private TMP_Text weightText;

    private RectTransform rectTransform;

    public BetCoinData CoinData { get; private set; }

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        if (coinImage == null)
        {
            coinImage =
                GetComponentInChildren<Image>(true);
        }

        // 생성된 코인이 이후 클릭을 가로채지 않도록 한다.
        Graphic[] graphics =
            GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic != null)
                graphic.raycastTarget = false;
        }
    }

    public void Bind(
        BetCoinData coin
    )
    {
        CoinData = coin;

        if (coin == null)
            return;

        if (coinImage != null)
        {
            coinImage.sprite =
                coin.Icon;

            coinImage.enabled =
                coin.Icon != null;

            coinImage.preserveAspect = true;
        }

        if (weightText != null)
        {
            weightText.text =
                $"+{coin.Weight}";
        }
    }

    public void SetPosition(
        Vector2 localPosition
    )
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        rectTransform.anchoredPosition =
            localPosition;
    }
}
