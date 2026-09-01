using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Game/Enemy/Enemy Definition")]
public class EnemyDefinitionData : ScriptableObject
{
    [Header("표시 정보")]
    [SerializeField] private string enemyName;

    [TextArea(2, 5)]
    [SerializeField] private string description;

    [Header("전투 데이터")]
    [SerializeField] private CharacterData unitData;
    [SerializeField] private List<EnemyActionData> actions = new List<EnemyActionData>();

    [Header("행동 방식")]
    [SerializeField] private EnemyBrainData brainData;

    [Header("외형")]
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Vector2 visualSize = new Vector2(400f, 600f);
    [SerializeField] private Vector2 visualOffset = Vector2.zero;
    [SerializeField] private RuntimeAnimatorController animatorController;




    public Vector2 VisualSize => visualSize;
    public Vector2 VisualOffset => visualOffset;

    public string EnemyName => enemyName;
    public string Description => description;
    public CharacterData UnitData => unitData;
    public IReadOnlyList<EnemyActionData> Actions => actions;
    public EnemyBrainData BrainData => brainData;
    public Sprite EnemySprite => enemySprite;
    public RuntimeAnimatorController AnimatorController => animatorController;
}
