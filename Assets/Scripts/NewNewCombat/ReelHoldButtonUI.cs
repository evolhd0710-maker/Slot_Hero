using UnityEngine;
using UnityEngine.UI;

public class ReelHoldButtonUI : MonoBehaviour
{
    [Header("릴 번호")]
    [SerializeField, Range(0, 5)]
    private int reelIndex;


    [Header("라운드 흐름")]
    [SerializeField]
    private SlotRoundFlowController flowController;


    [Header("버튼")]
    [SerializeField]
    private Button button;


    [Header("HOLD 표시")]
    [SerializeField]
    private GameObject holdVisual;


    private void Awake()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }
    }


    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                ToggleHold
            );

            button.onClick.AddListener(
                ToggleHold
            );
        }


        if (flowController != null)
        {
            flowController.OnStateChanged -=
                HandleStateChanged;

            flowController.OnReelLockChanged -=
                HandleReelLockChanged;

            flowController.OnRoundReset -=
                HandleRoundReset;


            flowController.OnStateChanged +=
                HandleStateChanged;

            flowController.OnReelLockChanged +=
                HandleReelLockChanged;

            flowController.OnRoundReset +=
                HandleRoundReset;
        }


        Refresh();
    }


    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                ToggleHold
            );
        }


        if (flowController != null)
        {
            flowController.OnStateChanged -=
                HandleStateChanged;

            flowController.OnReelLockChanged -=
                HandleReelLockChanged;

            flowController.OnRoundReset -=
                HandleRoundReset;
        }
    }


    private void ToggleHold()
    {
        if (flowController == null)
            return;


        bool success =
            flowController.TryToggleReelLock(
                reelIndex,
                out bool locked
            );


        if (!success)
            return;


        SetHoldVisual(
            locked
        );
    }


    private void HandleStateChanged(
        SlotRoundState state
    )
    {
        Refresh();
    }


    private void HandleReelLockChanged(
        int changedIndex,
        bool locked
    )
    {
        if (changedIndex !=
            reelIndex)
        {
            return;
        }


        SetHoldVisual(
            locked
        );
    }


    private void HandleRoundReset()
    {
        Refresh();
    }


    private void Refresh()
    {
        if (button != null)
        {
            button.interactable =
                flowController != null &&
                flowController.CanToggleHold;
        }


        bool locked =
            flowController != null &&
            flowController.IsReelLocked(
                reelIndex
            );


        SetHoldVisual(
            locked
        );
    }


    private void SetHoldVisual(
        bool active
    )
    {
        if (holdVisual != null)
        {
            holdVisual.SetActive(
                active
            );
        }
    }
}