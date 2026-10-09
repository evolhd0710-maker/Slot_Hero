using System.Collections.Generic;
using UnityEngine;

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
        public int StageCount = 12;

        [Tooltip("격자의 줄 수. 기본 5줄.")]
        [Min(2)]
        public int RowCount = 5;

        [Header("경로 생성")]
        [Tooltip("경로를 그리는 마지막 단계. 이 단계까지만 경로를 그린다. 기본 10단계.")]
        [Min(2)]
        public int PathEndStage = 10;

        [Tooltip("경로 생성 횟수. 각 단계에서 다음 단계로 가는 간선의 최대 개수가 된다. 기본 6회.")]
        [Min(3)]
        public int PathCount = 6;

        [Tooltip("한 걸음에 바뀔 수 있는 줄의 최대 폭. 기본 2로 -2 ~ +2 범위가 된다.")]
        [Min(1)]
        public int MaxRowStep = 2;

        [Tooltip("두 번째 경로가 첫 번째 경로의 노드를 피하게 한다. 각 단계 최소 2개 노드를 보장한다.")]
        public bool SecondPathAvoidsFirst = true;

        [Tooltip("간선의 교차를 없애는 정리 과정을 거친다.")]
        public bool RemoveEdgeCrossings = true;

        [Header("보장 조건")]
        [Tooltip("시작 노드 최소 개수.")]
        [Min(1)]
        public int MinStartNodes = 3;

        [Tooltip("시작 노드 최대 개수.")]
        [Min(1)]
        public int MaxStartNodes = 5;

        [Tooltip("한 단계에 들어갈 노드의 최소 개수.")]
        [Min(1)]
        public int MinNodesPerStage = 2;

        [Tooltip("한 단계에 들어갈 노드의 최대 개수.")]
        [Min(1)]
        public int MaxNodesPerStage = 5;

        [Tooltip("보장 조건을 채우지 못했을 때 다시 생성하는 최대 횟수.")]
        [Min(1)]
        public int MaxGenerationAttempts = 40;

        [Header("지터")]
        [Tooltip("노드 화면 위치에 지터를 준다.")]
        public bool ApplyJitter = true;

        [Tooltip("격자 간격을 1로 봤을 때의 가로 지터 최대치.")]
        [Range(0f, 0.5f)]
        public float JitterX = 0.25f;

        [Tooltip("격자 간격을 1로 봤을 때의 세로 지터 최대치.")]
        [Range(0f, 0.5f)]
        public float JitterY = 0.20f;

        [Header("고정 배치")]
        [Tooltip("일반 몬스터로 고정하는 단계. 기본 1, 2단계.")]
        public int[] FixedNormalStages = { 1, 2 };

        [Tooltip("단계 전체를 성소로 고정하는 단계. 기본 11단계.")]
        public int FixedSanctumStage = 11;

        [Tooltip("보스 단계. 기본 12단계.")]
        public int BossStage = 12;

        [Tooltip("중반 성소를 배치하는 구간의 시작 단계. 기본 5단계.")]
        public int MidSanctumStageMin = 5;

        [Tooltip("중반 성소를 배치하는 구간의 끝 단계. 기본 8단계.")]
        public int MidSanctumStageMax = 8;

        [Header("방 타입 배분")]
        [Tooltip("방 타입별 등장 가중치. 합계 10000을 기준으로 한다.")]
        public List<RoomTypeWeight> RoomTypeWeights = new List<RoomTypeWeight>
        {
            new RoomTypeWeight(RoomType.Normal, 5000),
            new RoomTypeWeight(RoomType.Elite, 2000),
            new RoomTypeWeight(RoomType.Sanctum, 500),
            new RoomTypeWeight(RoomType.Event, 2500),
            new RoomTypeWeight(RoomType.Etc, 0),
        };

        [Tooltip("배분 대상 노드 수에 곱하는 토큰 수의 하한.")]
        [Min(1f)]
        public float TokenMultiplierMin = 1.1f;

        [Tooltip("배분 대상 노드 수에 곱하는 토큰 수의 상한.")]
        [Min(1f)]
        public float TokenMultiplierMax = 1.2f;

        [Tooltip("간선으로 이어진 두 노드가 같은 타입이 되면 안 되는 방 타입.")]
        public RoomType[] NonConsecutiveTypes = { RoomType.Sanctum, RoomType.Elite };

        [Tooltip("연속 불가 타입에서 우선 배치를 거치지 않고 풀에 남겨 두는 토큰 수.")]
        [Min(0)]
        public int ReservedTokensPerNonConsecutiveType = 2;

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

        /// <summary>해당 단계가 방 타입 배분 대상인지. 고정 단계는 대상이 아니다.</summary>
        public bool IsAssignableStage(int stage)
        {
            return !IsFixedNormalStage(stage)
                   && stage != FixedSanctumStage
                   && stage != BossStage;
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
            PathEndStage = Mathf.Clamp(PathEndStage, 2, StageCount);
            MaxStartNodes = Mathf.Clamp(MaxStartNodes, MinStartNodes, RowCount);
            MinStartNodes = Mathf.Clamp(MinStartNodes, 1, MaxStartNodes);
            MaxNodesPerStage = Mathf.Clamp(MaxNodesPerStage, MinNodesPerStage, RowCount);
            MinNodesPerStage = Mathf.Clamp(MinNodesPerStage, 1, MaxNodesPerStage);
            MaxRowStep = Mathf.Clamp(MaxRowStep, 1, RowCount - 1);
            MidSanctumStageMax = Mathf.Clamp(MidSanctumStageMax, MidSanctumStageMin, PathEndStage);
            MidSanctumStageMin = Mathf.Clamp(MidSanctumStageMin, 1, MidSanctumStageMax);
            TokenMultiplierMax = Mathf.Max(TokenMultiplierMax, TokenMultiplierMin);
        }
    }
}
