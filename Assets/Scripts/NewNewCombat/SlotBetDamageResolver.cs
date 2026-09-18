using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotBetDamageResolver : MonoBehaviour
{
    [Header("계산")]
    [SerializeField]
    private SymbolResultCalculator calculator;


    [Header("계산 연출")]
    [SerializeField]
    private SlotCalculationPresenter calculationPresenter;


    [Header("베팅")]
    [SerializeField]
    private BettingManager bettingManager;


    [Header("공격 대상")]
    [SerializeField]
    private NewEnemy enemy;


    [Header("적 체력 UI")]
    [SerializeField]
    private UnitHealthUI enemyHealthUI;


    [Header("예상 피해 표시 시간")]
    [SerializeField, Min(0f)]
    private float predictedDamageDisplayTime = 0.7f;


    private SymbolCalculationResult lastResult;

    private bool isResolving;


    public SymbolCalculationResult LastResult =>
        lastResult;


    public bool IsResolving =>
        isResolving;


    public event Action<SymbolCalculationResult>
        OnResolutionCompleted;


    // =========================================================
    // 최종 슬롯 결과 처리
    // =========================================================

    public IEnumerator Resolve(
        IReadOnlyList<SymbolData> symbols
    )
    {
        if (isResolving)
            yield break;


        if (symbols == null)
        {
            Debug.LogError(
                "계산할 슬롯 결과가 없습니다.",
                this
            );

            yield break;
        }


        isResolving =
            true;


        // =====================================================
        // 계산
        // =====================================================

        if (calculator == null)
        {
            Debug.LogError(
                "SymbolResultCalculator가 연결되지 않았습니다.",
                this
            );


            CancelBetsAndFinish();

            yield break;
        }


        lastResult =
            calculator.Calculate(
                symbols,
                bettingManager
            );


        if (lastResult == null)
        {
            Debug.LogError(
                "슬롯 계산 결과가 없습니다.",
                this
            );


            CancelBetsAndFinish();

            yield break;
        }


        // =====================================================
        // 계산 과정 연출
        // =====================================================

        if (calculationPresenter != null)
        {
            yield return calculationPresenter
                .PlayCalculation(
                    lastResult
                );
        }


        // =====================================================
        // 적의 예상 체력 표시
        // =====================================================

        if (enemyHealthUI != null &&
            lastResult.totalDamage > 0)
        {
            enemyHealthUI
                .ShowPredictedDamage(
                    lastResult.totalDamage
                );
        }


        // 예상 체력을 잠깐 보여준다.
        if (lastResult.totalDamage > 0 &&
            predictedDamageDisplayTime > 0f)
        {
            yield return new WaitForSeconds(
                predictedDamageDisplayTime
            );
        }


        // =====================================================
        // 실제 피해
        // =====================================================

        if (enemy != null &&
            lastResult.totalDamage > 0)
        {
            enemy.TakeDamage(
                lastResult.totalDamage,
                "슬롯 최종 공격"
            );
        }


        // =====================================================
        // 예상 피해 제거
        // =====================================================

        if (enemyHealthUI != null)
        {
            enemyHealthUI
                .ClearPredictedDamage();
        }


        // =====================================================
        // 이번 턴 베팅 소비
        // =====================================================

        if (bettingManager != null)
        {
            bettingManager
                .ConsumeCurrentBets();
        }


        // =====================================================
        // 종료
        // =====================================================

        FinishResolution(
            lastResult
        );
    }


    // =========================================================
    // 오류 처리
    // =========================================================

    private void CancelBetsAndFinish()
    {
        if (bettingManager != null)
        {
            bettingManager
                .CancelAllBets();
        }


        if (enemyHealthUI != null)
        {
            enemyHealthUI
                .ClearPredictedDamage();
        }


        FinishResolution(
            null
        );
    }


    // =========================================================
    // 종료
    // =========================================================

    private void FinishResolution(
        SymbolCalculationResult result
    )
    {
        isResolving =
            false;


        Debug.Log(
            $"슬롯 공격 처리 종료 | 피해 = " +
            $"{(result != null ? result.totalDamage : 0)}",
            this
        );


        OnResolutionCompleted?.Invoke(
            result
        );
    }
}