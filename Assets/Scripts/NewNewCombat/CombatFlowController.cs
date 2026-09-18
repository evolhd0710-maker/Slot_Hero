using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum CombatTurnState
{
    PlayerTurn,
    EnemyTurn,
    BattleEnded
}

public class CombatFlowController : MonoBehaviour
{
    [Header("플레이어 슬롯 흐름")]
    [SerializeField]
    private SlotRoundFlowController slotRoundFlow;


    [Header("전투 유닛")]
    [SerializeField]
    private NewPlayer player;

    [SerializeField]
    private NewEnemy enemy;


    [Header("적 행동 종료 후 대기")]
    [Tooltip(
        "적 행동 로직이 끝난 후 다시 플레이어 턴으로 넘어가기 전 대기 시간"
    )]
    [SerializeField, Min(0f)]
    private float enemyTurnEndDelay = 0.5f;


    private CombatTurnState state =
        CombatTurnState.PlayerTurn;


    private bool enemyTurnRunning;


    public CombatTurnState State =>
        state;


    public bool IsPlayerTurn =>
        state ==
        CombatTurnState.PlayerTurn;


    public bool IsEnemyTurn =>
        state ==
        CombatTurnState.EnemyTurn;


    // =========================================================
    // 이벤트
    // =========================================================

    public event Action<CombatTurnState>
        OnCombatStateChanged;

    public event Action
        OnPlayerTurnStarted;

    public event Action
        OnEnemyTurnStarted;

    public event Action
        OnEnemyTurnCompleted;

    public event Action
        OnBattleEnded;


    // =========================================================
    // Unity
    // =========================================================

    private void OnEnable()
    {
        if (slotRoundFlow != null)
        {
            slotRoundFlow.OnPlayerTurnCompleted -=
                HandlePlayerTurnCompleted;


            slotRoundFlow.OnPlayerTurnCompleted +=
                HandlePlayerTurnCompleted;
        }
    }


    private void OnDisable()
    {
        if (slotRoundFlow != null)
        {
            slotRoundFlow.OnPlayerTurnCompleted -=
                HandlePlayerTurnCompleted;
        }
    }


    private void Start()
    {
        SetState(
            CombatTurnState.PlayerTurn
        );
        enemy.Setup();
        player.Setup();
        Debug.Log("Enemy 현재 체력" + enemy.CurrentHealth);

        Debug.Log(
            "===== 전투 시작 / 플레이어 턴 =====",
            this
        );


        OnPlayerTurnStarted?.Invoke();
    }


    // =========================================================
    // 플레이어 행동 종료
    // =========================================================

    private void HandlePlayerTurnCompleted()
    {
        if (state ==
            CombatTurnState.BattleEnded)
        {
            return;
        }


        // 플레이어의 최종 공격으로 적이 죽음
        if (IsEnemyDead())
        {
            EndBattle();

            return;
        }


        BeginEnemyTurn();
    }


    // =========================================================
    // 적 턴 시작
    // =========================================================

    private void BeginEnemyTurn()
    {
        if (enemyTurnRunning)
            return;


        if (enemy == null)
        {
            Debug.LogError(
                "CombatFlowController에 NewEnemy가 연결되지 않았습니다.",
                this
            );

            return;
        }


        if (player == null)
        {
            Debug.LogError(
                "CombatFlowController에 NewPlayer가 연결되지 않았습니다.",
                this
            );

            return;
        }


        StartCoroutine(
            EnemyTurnRoutine()
        );
    }


    // =========================================================
    // 적 턴
    // =========================================================

    private IEnumerator EnemyTurnRoutine()
    {
        enemyTurnRunning =
            true;


        SetState(
            CombatTurnState.EnemyTurn
        );


        Debug.Log(
            "===== 적 턴 시작 =====",
            this
        );


        OnEnemyTurnStarted?.Invoke();


        // =====================================================
        // 기존 NewEnemy 그대로 재사용
        // =====================================================
        //
        // ExecuteTurn 안에서:
        //
        // turnCount 증가
        // BeforeAction 처리
        // 기절 검사
        // Brain 행동 선택
        // ActionSelected
        // EnemyActionData.Execute()
        // Brain 후처리
        //
        // 전부 실행된다.
        // =====================================================

        yield return enemy.ExecuteTurn(
            player
        );


        // 현재 NewEnemy.ExecuteTurn은 행동 후
        // 한 프레임만 기다리므로 약간의 연출 시간을 둔다.
        if (enemyTurnEndDelay > 0f)
        {
            yield return new WaitForSeconds(
                enemyTurnEndDelay
            );
        }


        enemyTurnRunning =
            false;


        Debug.Log(
            "===== 적 턴 종료 =====",
            this
        );


        OnEnemyTurnCompleted?.Invoke();


        // =====================================================
        // 사망 확인
        // =====================================================

        if (IsPlayerDead())
        {
            EndBattle();

            yield break;
        }


        // 반사 피해 등의 시스템이 생길 경우 대비
        if (IsEnemyDead())
        {
            EndBattle();

            yield break;
        }


        // =====================================================
        // 다시 플레이어 턴
        // =====================================================

        BeginPlayerTurn();
    }


    // =========================================================
    // 다음 플레이어 턴
    // =========================================================

    private void BeginPlayerTurn()
    {
        if (slotRoundFlow == null)
        {
            Debug.LogError(
                "SlotRoundFlowController가 연결되지 않았습니다.",
                this
            );

            return;
        }


        // 슬롯 시스템 잠금 해제
        slotRoundFlow
            .BeginNextPlayerTurn();


        SetState(
            CombatTurnState.PlayerTurn
        );


        Debug.Log(
            "===== 플레이어 턴 시작 =====",
            this
        );


        OnPlayerTurnStarted?.Invoke();
    }


    // =========================================================
    // 사망 확인
    // =========================================================

    private bool IsPlayerDead()
    {
        return
            player == null ||
            player.CurrentHealth <= 0;
    }


    private bool IsEnemyDead()
    {
        return
            enemy == null ||
            enemy.CurrentHealth <= 0;
    }


    // =========================================================
    // 전투 종료
    // =========================================================

    private void EndBattle()
    {
        if (state ==
            CombatTurnState.BattleEnded)
        {
            return;
        }

        enemyTurnRunning = false;

        SetState(
            CombatTurnState.BattleEnded
        );

        if (enemy != null)
        {
            enemy.NotifyBattleEnd();
        }


        // 적이 죽었다면 전투 승리
        if (enemy != null &&
            enemy.CurrentHealth <= 0)
        {
            Debug.Log(
                "===== 전투 승리 =====",
                this
            );

            OnBattleEnded?.Invoke();

            StartCoroutine(
                ReturnToMapRoutine()
            );

            return;
        }


        // 플레이어 사망
        Debug.Log(
            "===== 전투 패배 =====",
            this
        );


        OnBattleEnded?.Invoke();
    }


    // =========================================================
    // 상태
    // =========================================================

    private void SetState(
        CombatTurnState newState
    )
    {
        state =
            newState;


        Debug.Log(
            $"CombatTurnState → {state}",
            this
        );


        OnCombatStateChanged?.Invoke(
            state
        );
    }

    private IEnumerator ReturnToMapRoutine()
    {
        // 전투 종료 화면을 잠깐 보여주고 싶으면 유지
        yield return new WaitForSecondsRealtime(
            0.7f
        );


        // =====================================================
        // 맵 씬의 MapManager 찾기
        // =====================================================

        MapManager mapManager =
            FindFirstObjectByType<MapManager>(
                FindObjectsInactive.Include
            );


        if (mapManager == null)
        {
            Debug.LogError(
                "MapManager를 찾을 수 없습니다.",
                this
            );

            yield break;
        }


        // =====================================================
        // 맵 UI 다시 표시
        // =====================================================

        mapManager.ReturnToMap();


        // =====================================================
        // 현재 전투 씬만 제거
        // =====================================================

        Scene combatScene =
            gameObject.scene;


        Debug.Log(
            $"전투 씬 언로드: {combatScene.name}",
            this
        );


        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(
                combatScene
            );


        if (unloadOperation != null)
        {
            yield return unloadOperation;
        }
    }
}