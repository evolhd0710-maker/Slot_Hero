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
        state == SlotRoundState.WaitingFirstBet ||
        state == SlotRoundState.SelectingHoldAndSecondBet;


    public bool CanChooseBetCoin =>
        CanPlaceBet;


    // 베팅하지 않고 넘어가는 것도
    // 베팅 가능한 타이밍에서만 허용
    public bool CanSkipBet =>
        CanPlaceBet;


    public bool CanToggleHold =>
        state == SlotRoundState.SelectingHoldAndSecondBet;


    // =========================================================
    // 이벤트
    // =========================================================

    public event Action<SlotRoundState>
        OnStateChanged;

    public event Action<int, bool>
        OnReelLockChanged;

    public event Action
        OnRoundReset;

    public event Action
        OnPlayerTurnCompleted;


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
    // 실제 베팅 완료
    // =========================================================

    public void HandleBetPlaced()
    {
        if (!CanPlaceBet)
        {
            Debug.LogWarning(
                $"현재 상태에서는 베팅할 수 없습니다. State={state}",
                this
            );

            return;
        }


        AdvanceCurrentBetStage(
            false
        );
    }


    // =========================================================
    // 베팅 안함
    // =========================================================

    public void SkipBet()
    {
        if (!CanSkipBet)
        {
            Debug.LogWarning(
                $"현재 상태에서는 베팅 안함을 선택할 수 없습니다. State={state}",
                this
            );

            return;
        }


        Debug.Log(
            $"[베팅 안함] State={state}",
            this
        );


        AdvanceCurrentBetStage(
            true
        );
    }


    // =========================================================
    // 현재 베팅 단계를 진행
    // =========================================================

    private void AdvanceCurrentBetStage(
        bool skippedBet
    )
    {
        if (slotMachine == null)
        {
            Debug.LogError(
                "SixReelSlotMachine이 연결되지 않았습니다.",
                this
            );

            AbortRound();

            return;
        }


        // =====================================================
        // 첫 번째 베팅 단계
        // =====================================================

        if (state ==
            SlotRoundState.WaitingFirstBet)
        {
            if (skippedBet)
            {
                Debug.Log(
                    "첫 번째 베팅 생략 → 전체 슬롯 회전",
                    this
                );
            }


            StartFirstSpin();

            return;
        }


        // =====================================================
        // 두 번째 베팅 단계
        // =====================================================

        if (state ==
            SlotRoundState.SelectingHoldAndSecondBet)
        {
            if (skippedBet)
            {
                Debug.Log(
                    "두 번째 베팅 생략",
                    this
                );
            }


            StartSecondAttempt();

            return;
        }
    }


    // =========================================================
    // 첫 번째 슬롯
    // =========================================================

    private void StartFirstSpin()
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
    }


    // =========================================================
    // 두 번째 시도
    // =========================================================

    private void StartSecondAttempt()
    {
        // =====================================================
        // 모든 릴이 HOLD
        //
        // 두 번째 회전 없이 바로 최종 계산
        // =====================================================

        if (slotMachine.AreAllReelsLocked)
        {
            Debug.Log(
                "모든 릴 HOLD → 두 번째 슬롯 생략 → 최종 계산",
                this
            );


            BeginResolution();

            return;
        }


        // =====================================================
        // 하나라도 풀려 있음
        //
        // 고정하지 않은 릴만 재회전
        // =====================================================

        SetState(
            SlotRoundState.SecondSpin
        );


        bool started =
            slotMachine.TrySpinUnlockedReels();


        if (!started)
        {
            Debug.LogError(
                "두 번째 슬롯 시작 실패",
                this
            );


            AbortRound();
        }
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
                "첫 번째 슬롯 완료 → HOLD 및 두 번째 선택 가능",
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
        }
    }


    // =========================================================
    // 최종 계산
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


        yield return damageResolver.Resolve(
            slotMachine.ResultSymbols
        );


        FinishPlayerTurn();
    }


    // =========================================================
    // 플레이어 턴 종료
    // =========================================================

    private void FinishPlayerTurn()
    {
        if (slotMachine != null)
        {
            slotMachine.ClearAllLocks();
        }


        SetState(
            SlotRoundState.WaitingEnemyTurn
        );


        // 베팅 코인 UI / HOLD UI 등 초기화
        OnRoundReset?.Invoke();


        Debug.Log(
            "플레이어 행동 종료 → 적 턴",
            this
        );


        OnPlayerTurnCompleted?.Invoke();
    }


    // =========================================================
    // 적 턴 종료 후
    // =========================================================

    public void BeginNextPlayerTurn()
    {
        if (state !=
            SlotRoundState.WaitingEnemyTurn)
        {
            Debug.LogWarning(
                $"다음 플레이어 턴을 시작할 수 없습니다. State={state}",
                this
            );

            return;
        }


        SetState(
            SlotRoundState.WaitingFirstBet
        );


        Debug.Log(
            "적 행동 종료 → 새로운 플레이어 턴",
            this
        );
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


        return true;
    }


    public bool IsReelLocked(
        int reelIndex
    )
    {
        if (slotMachine == null)
            return false;


        return slotMachine.IsReelLocked(
            reelIndex
        );
    }


    // =========================================================
    // 취소
    // =========================================================

    public void CancelRound()
    {
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
            bettingManager.CancelAllBets();
        }


        if (slotMachine != null)
        {
            slotMachine.ClearAllLocks();
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