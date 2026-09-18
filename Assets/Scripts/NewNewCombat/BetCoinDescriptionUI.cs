using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetCoinDescriptionUI : MonoBehaviour
{
    [Header("설명 영역")]
    [SerializeField]
    private GameObject descriptionRoot;

    [Header("코인 아이콘")]
    [SerializeField]
    private Image coinIcon;

    [Header("코인 이름")]
    [SerializeField]
    private TMP_Text coinNameText;

    [Header("체력 비용")]
    [SerializeField]
    private TMP_Text healthCostText;

    [Header("가중치")]
    [SerializeField]
    private TMP_Text weightText;


    private void Awake()
    {
        Clear();
    }


    // =========================================================
    // 코인 설명 표시
    // =========================================================

    public void Show(
        BetCoinData coin
    )
    {
        if (coin == null)
        {
            Clear();
            return;
        }


        if (descriptionRoot != null)
        {
            descriptionRoot.SetActive(
                true
            );
        }


        if (coinIcon != null)
        {
            coinIcon.sprite =
                coin.Icon;

            coinIcon.enabled =
                coin.Icon != null;

            coinIcon.preserveAspect =
                true;
        }


        if (coinNameText != null)
        {
            coinNameText.text =
                coin.DisplayName;
        }


        if (healthCostText != null)
        {
            healthCostText.text =
                $"체력 비용 : {coin.HealthCost}";
        }


        if (weightText != null)
        {
            weightText.text =
                $"가중치 : +{coin.Weight}";
        }
    }


    // =========================================================
    // 코인 설명 제거
    // =========================================================

    public void Clear()
    {
        if (descriptionRoot != null)
        {
            descriptionRoot.SetActive(
                false
            );
        }
    }


    /*
     * 이전에 내가 잘못 준 코드에서
     * Hide()를 호출하는 부분이 혹시 남아 있어도
     * 컴파일 에러가 나지 않도록 둔다.
     *
     * 실제 역할은 Clear()와 동일하다.
     */
    public void Hide()
    {
        Clear();
    }
}