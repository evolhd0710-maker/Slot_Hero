using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnemy : NewUnitBase
{
    [Header("현재 적 설정")]
    [SerializeField] private EnemyDefinitionData definition;

    [Header("이 적의 행동 목록")]
    [SerializeField] private List<EnemyActionData> actionPool = new List<EnemyActionData>();

    private EnemyBrainRuntime brain;
    private int turnCount;

    public event Action<EnemyActionData> ActionSelected;

    public EnemyDefinitionData Definition => definition;
    public IReadOnlyList<EnemyActionData> ActionPool => actionPool;
    public int TurnCount => turnCount;

    public void ApplyDefinition(EnemyDefinitionData newDefinition)
    {
        if (newDefinition == null)
        {
            Debug.LogError("적 설정 데이터가 없습니다.", this);
            return;
        }

        if (newDefinition.UnitData == null)
        {
            Debug.LogError($"{newDefinition.name}에 Unit Data가 연결되지 않았습니다.", newDefinition);
            return;
        }

        if (newDefinition.BrainData == null)
        {
            Debug.LogError($"{newDefinition.name}에 Brain Data가 연결되지 않았습니다.", newDefinition);
            return;
        }

        definition = newDefinition;
        data = newDefinition.UnitData;

        actionPool.Clear();

        foreach (EnemyActionData action in newDefinition.Actions)
        {
            if (action != null)
                actionPool.Add(action);
        }

        CreateBrain();

        Debug.Log($"적 전투 설정 적용: {newDefinition.EnemyName}", this);
    }

    private void CreateBrain()
    {
        brain = null;

        if (definition == null || definition.BrainData == null)
            return;

        brain = definition.BrainData.CreateRuntime();

        if (brain == null)
        {
            Debug.LogError($"{definition.name}의 Brain 생성에 실패했습니다.", this);
            return;
        }

        brain.Initialize(this, definition);
    }

    public override void Setup()
    {
        base.Setup();

        turnCount = 0;

        if (brain == null && definition != null)
            CreateBrain();

        brain?.OnBattleStart();
    }

    public EnemyActionData ChooseAction(NewPlayer player)
    {
        if (brain == null)
        {
            Debug.LogError($"{name}의 행동 Brain이 설정되지 않았습니다.", this);
            return null;
        }

        return brain.ChooseAction(player, turnCount);
    }

    public IEnumerator ExecuteTurn(NewPlayer player)
    {
        if (player == null)
        {
            Debug.LogError($"{name}이 행동할 대상 플레이어가 없습니다.", this);
            yield break;
        }

        turnCount++;

        TurnContext context = new TurnContext();
        bool cancelled = RaiseTiming(TriggerTiming.BeforeAction, context, source: player);

        if (cancelled)
        {
            Debug.Log($"{GetEnemyDisplayName()}이 기절해서 행동하지 못했습니다.");
            brain?.OnTurnCancelled(player, turnCount);
            yield break;
        }

        EnemyActionData action = ChooseAction(player);

        if (action == null)
        {
            Debug.LogError($"{GetEnemyDisplayName()}이 실행할 행동을 선택하지 못했습니다.", this);
            yield break;
        }

        Debug.Log($"{GetEnemyDisplayName()}의 행동: {action.actionName}");

        ActionSelected?.Invoke(action);

        action.Execute(player, this);
        brain?.OnActionExecuted(action, player, turnCount);

        yield return null;
    }

    public void NotifyBattleEnd()
    {
        brain?.OnBattleEnd();
    }

    private string GetEnemyDisplayName()
    {
        if (definition != null && !string.IsNullOrWhiteSpace(definition.EnemyName))
            return definition.EnemyName;

        if (data != null)
            return data.name;

        return name;
    }

    [ContextMenu("Test Stun Effect")]
    private void TestStunEffect()
    {
        ApplyEffect(new StunEffect(1));
    }
}