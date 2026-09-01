/*
using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicController : MonoBehaviour
{
    [Header("시작 유물")]
    [SerializeField] private List<RelicData> startingRelics = new List<RelicData>();

    private readonly List<RelicInstance> activeRelics = new List<RelicInstance>();

    private NewPlayer player;
    private NewEnemy enemy;
    private int nextAcquisitionOrder;

    public IReadOnlyList<RelicInstance> ActiveRelics => activeRelics;

    public event Action RelicsChanged;

    public void Initialize(NewPlayer player, NewEnemy enemy)
    {
        ClearRuntimeRelics();

        this.player = player;
        this.enemy = enemy;
        nextAcquisitionOrder = 0;

        foreach (RelicData relicData in startingRelics)
            AddRelic(relicData);
    }

    public RelicInstance AddRelic(RelicData relicData)
    {
        if (relicData == null)
            return null;

        if (player == null || enemy == null)
        {
            Debug.LogError("RelicController가 초기화되지 않았습니다.");
            return null;
        }

        if (!relicData.AllowDuplicates && HasRelic(relicData))
        {
            Debug.Log($"{relicData.RelicName} 유물은 중복 보유할 수 없습니다.");
            return null;
        }

        RelicInstance relicInstance = relicData.CreateInstance();

        if (relicInstance == null)
        {
            Debug.LogError($"{relicData.name}에서 RelicInstance 생성에 실패했습니다.");
            return null;
        }

        relicInstance.AcquisitionOrder = nextAcquisitionOrder++;
        activeRelics.Add(relicInstance);

        SortRelics();
        relicInstance.Initialize(this, player, enemy);

        RelicsChanged?.Invoke();

        Debug.Log($"유물 획득: {relicData.RelicName}");

        return relicInstance;
    }

    public void RemoveRelic(RelicInstance relicInstance)
    {
        if (relicInstance == null || !activeRelics.Contains(relicInstance))
            return;

        relicInstance.Dispose();
        activeRelics.Remove(relicInstance);

        RelicsChanged?.Invoke();

        Debug.Log($"유물 제거: {relicInstance.Data.RelicName}");
    }

    public bool HasRelic(RelicData relicData)
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            if (relicInstance.Data == relicData)
                return true;
        }

        return false;
    }

    public bool HasRelic<T>() where T : RelicInstance
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            if (relicInstance is T)
                return true;
        }

        return false;
    }

    public T GetRelic<T>() where T : RelicInstance
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            if (relicInstance is T targetRelic)
                return targetRelic;
        }

        return null;
    }

    public void OnBattleStart(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnBattleStart(turnContext);
    }

    public void OnTurnStart(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnTurnStart(turnContext);
    }

    public void OnRollResolved(TurnContext turnContext, IReadOnlyList<Symbol> rolledSymbols)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnRollResolved(turnContext, rolledSymbols);
    }

    public void BeforeSymbolExecute(SymbolExecutionContext context)
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            relicInstance.BeforeSymbolExecute(context);

            if (context.Cancelled)
                break;
        }
    }

    public int ModifySymbolPower(SymbolExecutionContext context, int currentPower)
    {
        int modifiedPower = currentPower;

        foreach (RelicInstance relicInstance in activeRelics)
            modifiedPower = relicInstance.ModifySymbolPower(context, modifiedPower);

        return Mathf.Max(0, modifiedPower);
    }

    public void AfterSymbolExecute(SymbolExecutionContext context)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.AfterSymbolExecute(context);
    }

    public void OnDamageDealt(SymbolExecutionContext context, int damage)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnDamageDealt(context, damage);
    }

    public void OnPlayerDamaged(int damage, string reason)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnPlayerDamaged(damage, reason);
    }

    public void OnTurnEnd(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnTurnEnd(turnContext);
    }

    public void OnBattleEnd(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnBattleEnd(turnContext);
    }

    public void ClearRuntimeRelics()
    {
        for (int i = activeRelics.Count - 1; i >= 0; i--)
            activeRelics[i].Dispose();

        activeRelics.Clear();
        RelicsChanged?.Invoke();
    }

    private void SortRelics()
    {
        activeRelics.Sort(CompareRelics);
    }

    private int CompareRelics(RelicInstance left, RelicInstance right)
    {
        int priorityComparison = left.Priority.CompareTo(right.Priority);

        if (priorityComparison != 0)
            return priorityComparison;

        return left.AcquisitionOrder.CompareTo(right.AcquisitionOrder);
    }

    private void OnDestroy()
    {
        ClearRuntimeRelics();
    }
}
*/
using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicController : MonoBehaviour
{
    [Header("기본 시작 유물")]
    [SerializeField] private List<RelicData> startingRelics = new List<RelicData>();

    private readonly List<RelicInstance> activeRelics = new List<RelicInstance>();

    private NewPlayer player;
    private NewEnemy enemy;
    private int nextAcquisitionOrder;

    public IReadOnlyList<RelicInstance> ActiveRelics => activeRelics;

    public event Action RelicsChanged;

    public void Initialize(NewPlayer player, NewEnemy enemy)
    {
        ClearRuntimeRelics();

        this.player = player;
        this.enemy = enemy;
        nextAcquisitionOrder = 0;

        ApplySelectedStartingRelics();

        foreach (RelicData relicData in startingRelics)
            AddRelic(relicData);
    }

    private void ApplySelectedStartingRelics()
    {
        if (RunSelectionData.Instance == null)
        {
            Debug.Log("RunSelectionData가 없으므로 Inspector의 기본 유물을 사용합니다.");
            return;
        }

        if (!RunSelectionData.Instance.HasRelicSelection)
        {
            Debug.Log("선택한 시작 유물이 없으므로 Inspector의 기본 유물을 사용합니다.");
            return;
        }

        SetStartingRelics(RunSelectionData.Instance.CurrentRelics);

        Debug.Log($"선택한 시작 유물 적용 완료. 유물 수: {startingRelics.Count}");
    }

    public void SetStartingRelics(IEnumerable<RelicData> relics)
    {
        startingRelics.Clear();

        if (relics == null)
            return;

        foreach (RelicData relic in relics)
        {
            if (relic != null)
                startingRelics.Add(relic);
        }
    }

    public RelicInstance AddRelic(RelicData relicData)
    {
        if (relicData == null)
            return null;

        if (player == null || enemy == null)
        {
            Debug.LogError("RelicController가 초기화되지 않았습니다.");
            return null;
        }

        if (!relicData.AllowDuplicates && HasRelic(relicData))
        {
            Debug.Log($"{relicData.RelicName} 유물은 중복 보유할 수 없습니다.");
            return null;
        }

        RelicInstance relicInstance = relicData.CreateInstance();

        if (relicInstance == null)
        {
            Debug.LogError($"{relicData.name}에서 RelicInstance 생성에 실패했습니다.");
            return null;
        }

        relicInstance.AcquisitionOrder = nextAcquisitionOrder++;
        activeRelics.Add(relicInstance);

        SortRelics();
        relicInstance.Initialize(this, player, enemy);

        RelicsChanged?.Invoke();

        Debug.Log($"유물 획득: {relicData.RelicName}");

        return relicInstance;
    }

    public void RemoveRelic(RelicInstance relicInstance)
    {
        if (relicInstance == null || !activeRelics.Contains(relicInstance))
            return;

        relicInstance.Dispose();
        activeRelics.Remove(relicInstance);

        RelicsChanged?.Invoke();

        Debug.Log($"유물 제거: {relicInstance.Data.RelicName}");
    }

    public bool HasRelic(RelicData relicData)
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            if (relicInstance.Data == relicData)
                return true;
        }

        return false;
    }

    public bool HasRelic<T>() where T : RelicInstance
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            if (relicInstance is T)
                return true;
        }

        return false;
    }

    public T GetRelic<T>() where T : RelicInstance
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            if (relicInstance is T targetRelic)
                return targetRelic;
        }

        return null;
    }

    public void OnBattleStart(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnBattleStart(turnContext);
    }

    public void OnTurnStart(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnTurnStart(turnContext);
    }

    public void OnRollResolved(TurnContext turnContext, IReadOnlyList<Symbol> rolledSymbols)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnRollResolved(turnContext, rolledSymbols);
    }

    public void BeforeSymbolExecute(SymbolExecutionContext context)
    {
        foreach (RelicInstance relicInstance in activeRelics)
        {
            relicInstance.BeforeSymbolExecute(context);

            if (context.Cancelled)
                break;
        }
    }

    public int ModifySymbolPower(SymbolExecutionContext context, int currentPower)
    {
        int modifiedPower = currentPower;

        foreach (RelicInstance relicInstance in activeRelics)
            modifiedPower = relicInstance.ModifySymbolPower(context, modifiedPower);

        return Mathf.Max(0, modifiedPower);
    }

    public void AfterSymbolExecute(SymbolExecutionContext context)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.AfterSymbolExecute(context);
    }

    public void OnDamageDealt(SymbolExecutionContext context, int damage)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnDamageDealt(context, damage);
    }

    public void OnPlayerDamaged(int damage, string reason)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnPlayerDamaged(damage, reason);
    }

    public void OnTurnEnd(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnTurnEnd(turnContext);
    }

    public void OnBattleEnd(TurnContext turnContext)
    {
        foreach (RelicInstance relicInstance in activeRelics)
            relicInstance.OnBattleEnd(turnContext);
    }

    public void ClearRuntimeRelics()
    {
        for (int i = activeRelics.Count - 1; i >= 0; i--)
            activeRelics[i].Dispose();

        activeRelics.Clear();
        RelicsChanged?.Invoke();
    }

    private void SortRelics()
    {
        activeRelics.Sort(CompareRelics);
    }

    private int CompareRelics(RelicInstance left, RelicInstance right)
    {
        int priorityComparison = left.Priority.CompareTo(right.Priority);

        if (priorityComparison != 0)
            return priorityComparison;

        return left.AcquisitionOrder.CompareTo(right.AcquisitionOrder);
    }

    private void OnDestroy()
    {
        ClearRuntimeRelics();
    }
}