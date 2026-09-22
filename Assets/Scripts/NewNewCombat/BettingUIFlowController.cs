using UnityEngine;
using UnityEngine.UI;

public class BettingUIFlowController : MonoBehaviour
{
    [Header("패널")]
    [SerializeField]
    private GameObject bettingTablePanel;

    [SerializeField]
    private GameObject coinSelectPanel;


    [Header("베팅판")]
    [SerializeField]
    private BettingTableUI bettingTableUI;


    [Header("슬롯 라운드")]
    [SerializeField]
    private SlotRoundFlowController roundFlowController;


    [Header("버튼")]
    [SerializeField]
    private Button openCoinSelectButton;

    [SerializeField]
    private Button skipBetButton;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ShowBettingTable();

        RefreshButtons();
    }


    private void OnEnable()
    {
        if (roundFlowController != null)
        {
            roundFlowController.OnStateChanged -=
                HandleStateChanged;

            roundFlowController.OnRoundReset -=
                HandleRoundReset;


            roundFlowController.OnStateChanged +=
                HandleStateChanged;

            roundFlowController.OnRoundReset +=
                HandleRoundReset;
        }


        if (skipBetButton != null)
        {
            skipBetButton.onClick.RemoveListener(
                SkipBet
            );

            skipBetButton.onClick.AddListener(
                SkipBet
            );
        }


        RefreshButtons();
    }


    private void OnDisable()
    {
        if (roundFlowController != null)
        {
            roundFlowController.OnStateChanged -=
                HandleStateChanged;

            roundFlowController.OnRoundReset -=
                HandleRoundReset;
        }


        if (skipBetButton != null)
        {
            skipBetButton.onClick.RemoveListener(
                SkipBet
            );
        }
    }


    // =========================================================
    // 베팅 화면
    // =========================================================

    public void ShowBettingTable()
    {
        if (bettingTablePanel != null)
        {
            bettingTablePanel.SetActive(
                true
            );
        }


        if (coinSelectPanel != null)
        {
            coinSelectPanel.SetActive(
                false
            );
        }
    }


    // =========================================================
    // 코인 선택 화면
    // =========================================================

    public void ShowCoinSelection()
    {
        if (roundFlowController != null &&
            !roundFlowController.CanChooseBetCoin)
        {
            return;
        }


        if (bettingTablePanel != null)
        {
            bettingTablePanel.SetActive(
                false
            );
        }


        if (coinSelectPanel != null)
        {
            coinSelectPanel.SetActive(
                true
            );
        }
    }


    // =========================================================
    // 코인 선택
    // =========================================================

    public void SelectCoin(
        BetCoinData coin
    )
    {
        if (coin == null)
            return;


        if (roundFlowController != null &&
            !roundFlowController.CanChooseBetCoin)
        {
            return;
        }


        if (bettingTableUI == null)
            return;


        bettingTableUI.SetSelectedCoin(
            coin
        );


        ShowBettingTable();
    }


    public void BackToBettingTable()
    {
        ShowBettingTable();
    }


    // =========================================================
    // 베팅 안함
    // =========================================================

    public void SkipBet()
    {
        if (roundFlowController == null)
            return;


        if (!roundFlowController.CanSkipBet)
            return;


        /*
         * 혹시 코인을 선택한 상태에서
         * 베팅 안함을 눌렀다면 선택 코인을 제거한다.
         *
         * 실제 베팅판에 놓지 않았으므로
         * 체력과 코인은 전혀 소모되지 않는다.
         */
        if (bettingTableUI != null)
        {
            bettingTableUI.ClearSelectedCoin();
        }


        // 코인 선택 패널에서 눌렀을 가능성까지 대비
        ShowBettingTable();


        roundFlowController.SkipBet();
    }


    // =========================================================
    // 상태
    // =========================================================

    private void HandleStateChanged(
        SlotRoundState state
    )
    {
        RefreshButtons();
    }


    private void HandleRoundReset()
    {
        ShowBettingTable();

        RefreshButtons();
    }


    private void RefreshButtons()
    {
        bool canBet =
            roundFlowController == null ||
            roundFlowController.CanChooseBetCoin;


        if (openCoinSelectButton != null)
        {
            openCoinSelectButton.interactable =
                canBet;
        }


        if (skipBetButton != null)
        {
            skipBetButton.interactable =
                roundFlowController == null ||
                roundFlowController.CanSkipBet;
        }
    }
}