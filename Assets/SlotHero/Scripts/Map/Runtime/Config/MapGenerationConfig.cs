using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SlotHero.Map
{
    /// <summary>
    /// 맵 생성에 쓰는 수치 묶음.
    /// 기본값은 맵 생성 규칙 기획서 v0.2 의 스테이지 1 기준이다.
    /// 이후 스테이지는 이 에셋을 복제해 단계 수, 가중치, 고정 배치 값만 바꾼다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MapGenerationConfig",
        menuName = "Slot Hero/맵/맵 생성 설정",
        order = 0)]
    public class MapGenerationConfig : ScriptableObject
    {
        [Header("격자")]
        [Tooltip("스테이지 내 단계 수. 기본 12단계.")]
        [Min(3)]
        [FormerlySerializedAs("StageCount")]
        [SerializeField] private int _stageCount = 12;
        public int StageCount { get { return _stageCount; } set { _stageCount = value; } }

        [Tooltip("격자의 줄 수. 기본 5줄.")]
        [Min(2)]
        [FormerlySerializedAs("RowCount")]
        [SerializeField] private int _rowCount = 5;
        public int RowCount { get { return _rowCount; } set { _rowCount = value; } }

        [Header("경로 생성")]
        [Tooltip("경로를 그리는 마지막 단계. 이 단계까지만 경로를 그린다. 기본 10단계.")]
        [Min(2)]
        [FormerlySerializedAs("PathEndStage")]
        [SerializeField] private int _pathEndStage = 10;
        public int PathEndStage { get { return _pathEndStage; } set { _pathEndStage = value; } }

        [Tooltip("경로 생성 횟수. 각 단계에서 다음 단계로 가는 간선의 최대 개수가 된다. 기본 6회.")]
        [Min(3)]
        [FormerlySerializedAs("PathCount")]
        [SerializeField] private int _pathCount = 6;
        public int PathCount { get { return _pathCount; } set { _pathCount = value; } }

        [Tooltip("한 걸음에 바뀔 수 있는 줄의 최대 폭. 기본 2로 -2 ~ +2 범위가 된다.")]
        [Min(1)]
        [FormerlySerializedAs("MaxRowStep")]
        [SerializeField] private int _maxRowStep = 2;
        public int MaxRowStep { get { return _maxRowStep; } set { _maxRowStep = value; } }

        [Tooltip("두 번째 경로가 첫 번째 경로의 노드를 피하게 한다. 각 단계 최소 2개 노드를 보장한다.")]
        [FormerlySerializedAs("SecondPathAvoidsFirst")]
        [SerializeField] private bool _secondPathAvoidsFirst = true;
        public bool SecondPathAvoidsFirst { get { return _secondPathAvoidsFirst; } set { _secondPathAvoidsFirst = value; } }

        [Tooltip("간선의 교차를 없애는 정리 과정을 거친다.")]
        [FormerlySerializedAs("RemoveEdgeCrossings")]
        [SerializeField] private bool _removeEdgeCrossings = true;
        public bool RemoveEdgeCrossings { get { return _removeEdgeCrossings; } set { _removeEdgeCrossings = value; } }

        [Header("보장 조건")]
        [Tooltip("시작 노드 최소 개수.")]
        [Min(1)]
        [FormerlySerializedAs("MinStartNodes")]
        [SerializeField] private int _minStartNodes = 3;
        public int MinStartNodes { get { return _minStartNodes; } set { _minStartNodes = value; } }

        [Tooltip("시작 노드 최대 개수.")]
        [Min(1)]
        [FormerlySerializedAs("MaxStartNodes")]
        [SerializeField] private int _maxStartNodes = 5;
        public int MaxStartNodes { get { return _maxStartNodes; } set { _maxStartNodes = value; } }

        [Tooltip("한 단계에 들어갈 노드의 최소 개수.")]
        [Min(1)]
        [FormerlySerializedAs("MinNodesPerStage")]
        [SerializeField] private int _minNodesPerStage = 2;
        public int MinNodesPerStage { get { return _minNodesPerStage; } set { _minNodesPerStage = value; } }

        [Tooltip("한 단계에 들어갈 노드의 최대 개수.")]
        [Min(1)]
        [FormerlySerializedAs("MaxNodesPerStage")]
        [SerializeField] private int _maxNodesPerStage = 5;
        public int MaxNodesPerStage { get { return _maxNodesPerStage; } set { _maxNodesPerStage = value; } }

        [Tooltip("보장 조건을 채우지 못했을 때 다시 생성하는 최대 횟수.")]
        [Min(1)]
        [FormerlySerializedAs("MaxGenerationAttempts")]
        [SerializeField] private int _maxGenerationAttempts = 40;
        public int MaxGenerationAttempts { get { return _maxGenerationAttempts; } set { _maxGenerationAttempts = value; } }

        [Header("지터")]
        [Tooltip("노드 화면 위치에 지터를 준다.")]
        [FormerlySerializedAs("ApplyJitter")]
        [SerializeField] private bool _applyJitter = true;
        public bool ApplyJitter { get { return _applyJitter; } set { _applyJitter = value; } }

        [Tooltip("격자 간격을 1로 봤을 때의 가로 지터 최대치.")]
        [Range(0f, 0.5f)]
        [FormerlySerializedAs("JitterX")]
        [SerializeField] private float _jitterX = 0.25f;
        public float JitterX { get { return _jitterX; } set { _jitterX = value; } }

        [Tooltip("격자 간격을 1로 봤을 때의 세로 지터 최대치.")]
        [Range(0f, 0.5f)]
        [FormerlySerializedAs("JitterY")]
        [SerializeField] private float _jitterY = 0.20f;
        public float JitterY { get { return _jitterY; } set { _jitterY = value; } }

        [Header("고정 배치")]
        [Tooltip("일반 몬스터로 고정하는 단계. 기본 1, 2단계.")]
        [FormerlySerializedAs("FixedNormalStages")]
        [SerializeField] private int[] _fixedNormalStages = { 1, 2 };
        public int[] FixedNormalStages { get { return _fixedNormalStages; } set { _fixedNormalStages = value; } }

        [Tooltip("단계 전체를 성소로 고정하는 단계. 기본 11단계.")]
        [FormerlySerializedAs("FixedSanctumStage")]
        [SerializeField] private int _fixedSanctumStage = 11;
        public int FixedSanctumStage { get { return _fixedSanctumStage; } set { _fixedSanctumStage = value; } }

        [Tooltip("보스 단계. 기본 12단계.")]
        [FormerlySerializedAs("BossStage")]
        [SerializeField] private int _bossStage = 12;
        public int BossStage { get { return _bossStage; } set { _bossStage = value; } }

        [Tooltip("중반 성소를 배치하는 구간의 시작 단계. 기본 5단계.")]
        [FormerlySerializedAs("MidSanctumStageMin")]
        [SerializeField] private int _midSanctumStageMin = 5;
        public int MidSanctumStageMin { get { return _midSanctumStageMin; } set { _midSanctumStageMin = value; } }

        [Tooltip("중반 성소를 배치하는 구간의 끝 단계. 기본 8단계.")]
        [FormerlySerializedAs("MidSanctumStageMax")]
        [SerializeField] private int _midSanctumStageMax = 8;
        public int MidSanctumStageMax { get { return _midSanctumStageMax; } set { _midSanctumStageMax = value; } }

        [Header("방 타입 배분")]
        [Tooltip("방 타입별 등장 가중치. 합계 10000을 기준으로 한다.")]
        [FormerlySerializedAs("RoomTypeWeights")]
        [SerializeField] private List<RoomTypeWeight> _roomTypeWeights = new List<RoomTypeWeight>
        {
            new RoomTypeWeight(RoomType.Normal, 5000),
            new RoomTypeWeight(RoomType.Elite, 2000),
            new RoomTypeWeight(RoomType.Sanctum, 500),
            new RoomTypeWeight(RoomType.Event, 2500),
            new RoomTypeWeight(RoomType.Etc, 0),
        };
        public List<RoomTypeWeight> RoomTypeWeights { get { return _roomTypeWeights; } set { _roomTypeWeights = value; } }

        [Tooltip("배분 대상 노드 수에 곱하는 토큰 수의 하한.")]
        [Min(1f)]
        [FormerlySerializedAs("TokenMultiplierMin")]
        [SerializeField] private float _tokenMultiplierMin = 1.1f;
        public float TokenMultiplierMin { get { return _tokenMultiplierMin; } set { _tokenMultiplierMin = value; } }

        [Tooltip("배분 대상 노드 수에 곱하는 토큰 수의 상한.")]
        [Min(1f)]
        [FormerlySerializedAs("TokenMultiplierMax")]
        [SerializeField] private float _tokenMultiplierMax = 1.2f;
        public float TokenMultiplierMax { get { return _tokenMultiplierMax; } set { _tokenMultiplierMax = value; } }

        [Tooltip("간선으로 이어진 두 노드가 같은 타입이 되면 안 되는 방 타입.")]
        [FormerlySerializedAs("NonConsecutiveTypes")]
        [SerializeField] private RoomType[] _nonConsecutiveTypes = { RoomType.Sanctum, RoomType.Elite };
        public RoomType[] NonConsecutiveTypes { get { return _nonConsecutiveTypes; } set { _nonConsecutiveTypes = value; } }

        [Tooltip("연속 불가 타입에서 우선 배치를 거치지 않고 풀에 남겨 두는 토큰 수.")]
        [Min(0)]
        [FormerlySerializedAs("ReservedTokensPerNonConsecutiveType")]
        [SerializeField] private int _reservedTokensPerNonConsecutiveType = 2;
        public int ReservedTokensPerNonConsecutiveType { get { return _reservedTokensPerNonConsecutiveType; } set { _reservedTokensPerNonConsecutiveType = value; } }

        /// <summary>가중치 합계.</summary>
        public int TotalWeight
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < RoomTypeWeights.Count; i++)
                {
                    sum += Mathf.Max(0, RoomTypeWeights[i].Weight);
                }

                return sum;
            }
        }

        /// <summary>해당 단계가 일반 몬스터 고정 단계인지.</summary>
        public bool IsFixedNormalStage(int stage)
        {
            if (FixedNormalStages == null)
            {
                return false;
            }

            for (int i = 0; i < FixedNormalStages.Length; i++)
            {
                if (FixedNormalStages[i] == stage)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>해당 방 타입이 연속 배치 불가인지.</summary>
        public bool IsNonConsecutive(RoomType roomType)
        {
            if (NonConsecutiveTypes == null)
            {
                return false;
            }

            for (int i = 0; i < NonConsecutiveTypes.Length; i++)
            {
                if (NonConsecutiveTypes[i] == roomType)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnValidate()
        {
            _pathEndStage = Mathf.Clamp(_pathEndStage, 2, _stageCount);
            _maxStartNodes = Mathf.Clamp(_maxStartNodes, _minStartNodes, _rowCount);
            _minStartNodes = Mathf.Clamp(_minStartNodes, 1, _maxStartNodes);
            _maxNodesPerStage = Mathf.Clamp(_maxNodesPerStage, _minNodesPerStage, _rowCount);
            _minNodesPerStage = Mathf.Clamp(_minNodesPerStage, 1, _maxNodesPerStage);
            _maxRowStep = Mathf.Clamp(_maxRowStep, 1, _rowCount - 1);
            _midSanctumStageMax = Mathf.Clamp(_midSanctumStageMax, _midSanctumStageMin, _pathEndStage);
            _midSanctumStageMin = Mathf.Clamp(_midSanctumStageMin, 1, _midSanctumStageMax);
            _tokenMultiplierMax = Mathf.Max(_tokenMultiplierMax, _tokenMultiplierMin);
        }
    }
}
