using System;
using System.Collections.Generic;

namespace SlotHero.Map
{
    /// <summary>
    /// 스테이지 하나의 맵 전체. 생성 결과물이며 저장 시스템에 그대로 실어 보낼 수 있다.
    /// 같은 시드와 같은 설정이면 항상 같은 내용이 나온다.
    /// </summary>
    [Serializable]
    public class StageMap
    {
        /// <summary>런 전체에 부여된 시드.</summary>
        public int Seed;

        /// <summary>스테이지 번호. 1부터 시작한다.</summary>
        public int StageIndex;

        /// <summary>단계 수. 기본 12.</summary>
        public int StageCount;

        /// <summary>줄 수. 기본 5.</summary>
        public int RowCount;

        public List<MapNode> Nodes = new List<MapNode>();
        public List<MapEdge> Edges = new List<MapEdge>();

        [NonSerialized] private Dictionary<int, MapNode> _nodeById;
        [NonSerialized] private List<MapNode>[] _nodesByStage;

        /// <summary>번호로 노드를 찾는다. 없으면 null.</summary>
        public MapNode GetNode(int nodeId)
        {
            EnsureIndex();
            MapNode node;
            return _nodeById.TryGetValue(nodeId, out node) ? node : null;
        }

        /// <summary>해당 단계의 노드 목록. 단계는 1부터 시작한다.</summary>
        public IReadOnlyList<MapNode> GetNodesOfStage(int stage)
        {
            EnsureIndex();
            if (stage < 1 || stage > StageCount)
            {
                return Array.Empty<MapNode>();
            }

            return _nodesByStage[stage - 1];
        }

        /// <summary>시작 노드 목록. 1단계의 모든 노드이다.</summary>
        public IReadOnlyList<MapNode> GetStartNodes()
        {
            return GetNodesOfStage(1);
        }

        /// <summary>보스 노드. 마지막 단계의 단일 노드이다.</summary>
        public MapNode GetBossNode()
        {
            IReadOnlyList<MapNode> last = GetNodesOfStage(StageCount);
            return last.Count > 0 ? last[0] : null;
        }

        /// <summary>두 노드가 간선으로 직접 이어져 있는지 확인한다. 방향은 보지 않는다.</summary>
        public bool AreConnected(int nodeIdA, int nodeIdB)
        {
            MapNode a = GetNode(nodeIdA);
            if (a == null)
            {
                return false;
            }

            return a.NextNodeIds.Contains(nodeIdB) || a.PrevNodeIds.Contains(nodeIdB);
        }

        /// <summary>노드와 간선을 다시 읽어 내부 색인을 만든다. 노드를 직접 손댄 뒤에는 호출해야 한다.</summary>
        public void RebuildIndex()
        {
            _nodeById = null;
            _nodesByStage = null;
            EnsureIndex();
        }

        private void EnsureIndex()
        {
            if (_nodeById != null && _nodesByStage != null)
            {
                return;
            }

            _nodeById = new Dictionary<int, MapNode>(Nodes.Count);
            _nodesByStage = new List<MapNode>[Math.Max(StageCount, 1)];
            for (int i = 0; i < _nodesByStage.Length; i++)
            {
                _nodesByStage[i] = new List<MapNode>();
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                MapNode node = Nodes[i];
                _nodeById[node.Id] = node;

                int stageArrayIndex = node.Stage - 1;
                if (stageArrayIndex >= 0 && stageArrayIndex < _nodesByStage.Length)
                {
                    _nodesByStage[stageArrayIndex].Add(node);
                }
            }

            for (int i = 0; i < _nodesByStage.Length; i++)
            {
                _nodesByStage[i].Sort((a, b) => a.Row.CompareTo(b.Row));
            }
        }
    }
}
