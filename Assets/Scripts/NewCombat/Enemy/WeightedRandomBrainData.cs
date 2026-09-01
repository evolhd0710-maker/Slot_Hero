using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeightedRandomBrain", menuName = "Game/Enemy/Brain/Weighted Random")]
public class WeightedRandomBrainData : EnemyBrainData
{
    public override EnemyBrainRuntime CreateRuntime()
    {
        return new WeightedRandomBrainRuntime();
    }
}

public class WeightedRandomBrainRuntime : EnemyBrainRuntime
{
    public override EnemyActionData ChooseAction(NewPlayer player, int turnNumber)
    {
        if (Owner == null)
            return null;

        IReadOnlyList<EnemyActionData> actionPool = Owner.ActionPool;
        List<(EnemyActionData action, float weight)> candidates = new List<(EnemyActionData, float)>();

        foreach (EnemyActionData action in actionPool)
        {
            if (action == null)
                continue;

            float weight = turnNumber == 1 && action.firstTurnWeight >= 0f
                ? action.firstTurnWeight
                : action.weight;

            if (weight > 0f)
                candidates.Add((action, weight));
        }

        if (candidates.Count == 0)
        {
            Debug.LogError($"{Owner.name}에게 실행 가능한 행동이 없습니다.", Owner);
            return null;
        }

        float totalWeight = 0f;

        foreach ((EnemyActionData action, float weight) candidate in candidates)
            totalWeight += candidate.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach ((EnemyActionData action, float weight) candidate in candidates)
        {
            cumulativeWeight += candidate.weight;

            if (roll <= cumulativeWeight)
                return candidate.action;
        }

        return candidates[candidates.Count - 1].action;
    }
}