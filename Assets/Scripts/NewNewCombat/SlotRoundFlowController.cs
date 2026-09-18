using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotRoundFlowController : MonoBehaviour
{
    [Header("슬롯")]
    [SerializeField]
    private SixReelSlotMachine slotMachine;

    [Header("최종 계산")]
    [SerializeField]
    private SlotBetDamageResolver damageResolver;

    [Header("베팅")]
    [SerializeField]
    private BettingManager bettingManager;


    private SlotRoundState state =
        SlotRoundState.WaitingFirstBet;


    public SlotRoundState State =>
        state;


    // =========================================================
    // 현재 가능한 행동
    // =========================================================

    public bool CanPlaceBet =>
        state ==
            SlotRoundState.WaitingFirstBet ||
        state ==
            SlotRoundState.SelectingHoldAndSecondBet;


    public bool CanChooseBetCoin =>
        CanPlaceBet;


    public bool CanToggleHold =>
        state ==
        SlotRoundState.SelectingHoldAndSecondBet;


    // =========================================================
    // 이벤트
    // =========================================================

    public event Action<SlotRoundState>
        OnStateChanged;


    public event Action<int, bool>
        OnReelLockChanged;


    /// <summary>
    /// 베팅판 / HOLD UI 등을 초기화할 때 사용.
    /// </summary>
    public event Action
        OnRoundReset;


    /// <summary>
    /// 플레이어의 슬롯 계산과 최종 공격이
    /// 전부 끝났을 때 발생.
    ///
    /// CombatFlowController가 이 이벤트를 받아
    /// 적 턴을 시작한다.
    /// </summary>
    public event Action
        OnPlayerTurnCompleted;


    /// <summary>
    /// 적 턴 종료 후 새로운 플레이어 턴이 시작될 때.
    /// </summary>
    public event Action
        OnPlayerTurnStarted;


    // =========================================================
    // Unity
    // =========================================================

    private void OnEnable()
    {
        if (slotMachine == null)
            return;


        slotMachine.OnSpinCompleted -=
            HandleSpinCompleted;


        slotMachine.OnSpinCompleted +=
            HandleSpinCompleted;
    }


    private void OnDisable()
    {
        if (slotMachine == null)
            return;


        slotMachine.OnSpinCompleted -=
            HandleSpinCompleted;
    }


    // =========================================================
    // 베팅 완료
    //
    // BettingTableUI에서 코인이 실제 배치된 뒤 호출
    // =========================================================

    public void HandleBetPlaced()
    {
        if (slotMachine == null)
        {
            Debug.LogError(
                "SlotRoundFlowController에 " +
                "SixReelSlotMachine이 연결되지 않았습니다.",
                this
            );

            AbortRound();

            return;
        }


        // =====================================================
        // 첫 번째 베팅
        // =====================================================

        if (state ==
            SlotRoundState.WaitingFirstBet)
        {
            SetState(
                SlotRoundState.FirstSpin
            );


            bool started =
                slotMachine.TrySpinAll();


            if (!started)
            {
                Debug.LogError(
                    "첫 번째 슬롯 시작 실패",
                    this
                );

                AbortRound();
            }


            return;
        }


        // =====================================================
        // 두 번째 베팅
        // =====================================================

        if (state ==
            SlotRoundState.SelectingHoldAndSecondBet)
        {
            // ---------------------------------------------
            // 6개 전부 HOLD
            //
            // 재회전 없이 현재 결과를 그대로 최종 계산
            // ---------------------------------------------

            if (slotMachine.AreAllReelsLocked)
            {
                Debug.Log(
                    "6개 릴 전부 HOLD → " +
                    "두 번째 슬롯 생략 → 바로 계산",
                    this
                );


                BeginResolution();

                return;
            }


            // ---------------------------------------------
            // 일부 릴만 HOLD
            //
            // HOLD하지 않은 릴만 다시 회전
            // ---------------------------------------------

            SetState(
                SlotRoundState.SecondSpin
            );


            bool started =
                slotMachine
                    .TrySpinUnlockedReels();


            if (!started)
            {
                Debug.LogError(
                    "두 번째 슬롯 시작 실패",
                    this
                );

                AbortRound();
            }


            return;
        }


        Debug.LogWarning(
            $"현재 상태에서는 베팅할 수 없습니다. " +
            $"State={state}",
            this
        );
    }


    // =========================================================
    // 슬롯 완료
    // =========================================================

    private void HandleSpinCompleted(
        IReadOnlyList<SymbolData> results
    )
    {
        // =====================================================
        // 첫 번째 슬롯 완료
        // =====================================================

        if (state ==
            SlotRoundState.FirstSpin)
        {
            SetState(
                SlotRoundState
                    .SelectingHoldAndSecondBet
            );


            Debug.Log(
                "첫 번째 슬롯 결과 확정 → " +
                "릴 HOLD 및 두 번째 베팅 가능",
                this
            );


            return;
        }


        // =====================================================
        // 두 번째 슬롯 완료
        // =====================================================

        if (state ==
            SlotRoundState.SecondSpin)
        {
            BeginResolution();

            return;
        }
    }


    // =========================================================
    // 플레이어 최종 계산 시작
    // =========================================================

    private void BeginResolution()
    {
        if (state ==
            SlotRoundState.Resolving)
        {
            return;
        }


        SetState(
            SlotRoundState.Resolving
        );


        StartCoroutine(
            ResolveRoutine()
        );
    }


    private IEnumerator ResolveRoutine()
    {
        if (damageResolver == null)
        {
            Debug.LogError(
                "SlotBetDamageResolver가 연결되지 않았습니다.",
                this
            );


            AbortRound();

            yield break;
        }


        // =====================================================
        // 계산 연출
        // → 최종 공격
        // → 베팅 Consume
        // =====================================================

        yield return damageResolver.Resolve(
            slotMachine.ResultSymbols
        );


        // =====================================================
        // 플레이어 행동 완전 종료
        // =====================================================

        FinishPlayerTurn();
    }


    // =========================================================
    // 플레이어 턴 종료
    // =========================================================

    private void FinishPlayerTurn()
    {
        // HOLD 초기화
        if (slotMachine != null)
        {
            slotMachine.ClearAllLocks();
        }


        /*
         * 여기서 WaitingFirstBet으로 돌아가면 안 된다.
         *
         * 적이 행동해야 하므로
         * WaitingEnemyTurn 상태에서 잠근다.
         */

        SetState(
            SlotRoundState.WaitingEnemyTurn
        );


        // 베팅판 위 코인 / 선택 코인 / HOLD 표시 제거
        OnRoundReset?.Invoke();


        Debug.Log(
            "플레이어 행동 종료 → 적 턴 대기",
            this
        );


        // CombatFlowController에게 알림
        OnPlayerTurnCompleted?.Invoke();
    }


    // =========================================================
    // 적 행동 종료 후 호출
    // =========================================================

    public void BeginNextPlayerTurn()
    {
        if (state !=
            SlotRoundState.WaitingEnemyTurn)
        {
            Debug.LogWarning(
                $"플레이어 턴을 시작할 수 없는 상태입니다. " +
                $"State={state}",
                this
            );

            return;
        }


        SetState(
            SlotRoundState.WaitingFirstBet
        );


        Debug.Log(
            "적 행동 종료 → 새로운 플레이어 베팅 턴 시작",
            this
        );


        OnPlayerTurnStarted?.Invoke();
    }


    // =========================================================
    // HOLD
    // =========================================================

    public bool TryToggleReelLock(
        int reelIndex,
        out bool locked
    )
    {
        locked =
            false;


        if (!CanToggleHold)
            return false;


        if (slotMachine == null)
            return false;


        locked =
            slotMachine.ToggleReelLock(
                reelIndex
            );


        OnReelLockChanged?.Invoke(
            reelIndex,
            locked
        );


        Debug.Log(
            $"Reel {reelIndex} HOLD = {locked}",
            this
        );


        return true;
    }


    public bool IsReelLocked(
        int reelIndex
    )
    {
        if (slotMachine == null)
            return false;


        return
            slotMachine.IsReelLocked(
                reelIndex
            );
    }


    // =========================================================
    // 취소
    // =========================================================

    public void CancelRound()
    {
        // 회전 / 계산 / 적 턴 대기 중에는 취소 불가능
        if (state ==
                SlotRoundState.FirstSpin ||

            state ==
                SlotRoundState.SecondSpin ||

            state ==
                SlotRoundState.Resolving ||

            state ==
                SlotRoundState.WaitingEnemyTurn)
        {
            return;
        }


        AbortRound();
    }


    private void AbortRound()
    {
        if (bettingManager != null)
        {
            bettingManager
                .CancelAllBets();
        }


        if (slotMachine != null)
        {
            slotMachine
                .ClearAllLocks();
        }


        SetState(
            SlotRoundState.WaitingFirstBet
        );


        OnRoundReset?.Invoke();
    }


    // =========================================================
    // 상태
    // =========================================================

    private void SetState(
        SlotRoundState newState
    )
    {
        state =
            newState;


        Debug.Log(
            $"SlotRoundState → {state}",
            this
        );


        OnStateChanged?.Invoke(
            state
        );
    }
}