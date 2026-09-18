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


    [Header("베팅 버튼")]
    [SerializeField]
    private Button openCoinSelectButton;


    private void Start()
    {
        ShowBettingTable();

        RefreshBetButton();
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
    // 상태
    // =========================================================

    private void HandleStateChanged(
        SlotRoundState state
    )
    {
        RefreshBetButton();
    }


    private void HandleRoundReset()
    {
        ShowBettingTable();

        RefreshBetButton();
    }


    private void RefreshBetButton()
    {
        if (openCoinSelectButton == null)
            return;


        openCoinSelectButton.interactable =
            roundFlowController == null ||
            roundFlowController.CanChooseBetCoin;
    }
}