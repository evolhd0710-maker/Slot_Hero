using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyAction", menuName = "Enemy/EnemyAction")]
public class EnemyActionData : ScriptableObject
{
    [Header("기본 정보")]
    public string actionName;

    [Header("이 행동이 실행할 효과들 (조립식)")]
    public List<EnemyActionEffect> effects;

    [Header("일반 가중치 (상대비율, 합이 100이 아니어도 됨)")]
    public float weight = 30f;

    [Header("1턴 전용 가중치. -1이면 무시하고 일반 weight 사용, 0이면 1턴에 발동 불가")]
    public float firstTurnWeight = -1f;

    [Header("행동 애니메이션")]
    [SerializeField] private string animationTrigger = "Attack";

    public string AnimationTrigger => animationTrigger;

    public void Execute(NewPlayer player, NewEnemy self)
    {
        foreach (var effect in effects)
        {
            effect.Apply(player, self);
        }
    }
}
