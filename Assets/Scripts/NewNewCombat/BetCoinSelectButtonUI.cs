using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetCoinSelectButtonUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("코인")]
    [SerializeField]
    private BetCoinData coinData;


    [Header("버튼")]
    [SerializeField]
    private Button button;


    [Header("버튼 표시 - 선택")]
    [SerializeField]
    private Image coinIcon;

    [SerializeField]
    private TMP_Text coinNameText;


    [Header("베팅 UI")]
    [SerializeField]
    private BettingUIFlowController flowController;


    [Header("코인 설명 UI")]
    [SerializeField]
    private BetCoinDescriptionUI descriptionUI;


    // =========================================================
    // 이번에 추가된 부분
    // =========================================================

    [Header("플레이어 체력 UI")]
    [Tooltip(
        "코인 버튼에 마우스를 올렸을 때 " +
        "Health Cost만큼 감소할 예상 체력을 표시합니다."
    )]
    [SerializeField]
    private UnitHealthUI playerHealthUI;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }


        RefreshButton();
    }


    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                SelectCoin
            );

            button.onClick.AddListener(
                SelectCoin
            );
        }


        RefreshButton();
    }


    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                SelectCoin
            );
        }


        /*
         * 코인 선택 패널이 꺼질 경우
         * PointerExit이 발생하지 않을 수도 있으므로
         * 여기서도 체력 예상 표시를 제거한다.
         */
        ClearHealthPreview();


        if (descriptionUI != null)
        {
            descriptionUI.Clear();
        }
    }


    // =========================================================
    // 버튼 정보
    // =========================================================

    private void RefreshButton()
    {
        if (coinData == null)
            return;


        if (coinIcon != null)
        {
            coinIcon.sprite =
                coinData.Icon;

            coinIcon.enabled =
                coinData.Icon != null;

            coinIcon.preserveAspect =
                true;
        }


        if (coinNameText != null)
        {
            coinNameText.text =
                coinData.DisplayName;
        }
    }


    // =========================================================
    // Hover 시작
    // =========================================================

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        if (coinData == null)
            return;


        // -----------------------------------------
        // 기존 코인 설명
        // -----------------------------------------

        if (descriptionUI != null)
        {
            descriptionUI.Show(
                coinData
            );
        }


        // -----------------------------------------
        // 플레이어 체력 예상 소모
        // -----------------------------------------

        if (playerHealthUI != null)
        {
            playerHealthUI
                .ShowPredictedDamage(
                    coinData.HealthCost
                );
        }
    }


    // =========================================================
    // Hover 종료
    // =========================================================

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        // 기존 코인 설명 제거
        if (descriptionUI != null)
        {
            descriptionUI.Clear();
        }


        // 플레이어 체력 예상 표시 제거
        ClearHealthPreview();
    }


    // =========================================================
    // 코인 선택
    // =========================================================

    private void SelectCoin()
    {
        if (coinData == null)
            return;


        /*
         * 클릭하면 코인 선택 패널이 바로 꺼질 수 있기 때문에
         * PointerExit에만 의존하지 않는다.
         */
        ClearHealthPreview();


        if (descriptionUI != null)
        {
            descriptionUI.Clear();
        }


        if (flowController != null)
        {
            flowController.SelectCoin(
                coinData
            );
        }
    }


    // =========================================================
    // 체력 예상 표시 제거
    // =========================================================

    private void ClearHealthPreview()
    {
        if (playerHealthUI != null)
        {
            playerHealthUI
                .ClearPredictedDamage();
        }
    }
}