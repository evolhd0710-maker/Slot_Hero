using System;
using System.Collections.Generic;

namespace SlotHero.Map
{
    /// <summary>
    /// 맵 위에서의 진행 상태. 지금까지 지나온 노드와 현재 위치를 들고 있다.
    /// 화면 표시에 필요한 최소한만 담으며, 각 방 안에서의 내용은 다루지 않는다.
    /// </summary>
    [Serializable]
    public class MapProgress
    {
        /// <summary>현재 머물러 있는 노드 번호. 아직 아무 방도 고르지 않았으면 -1.</summary>
        public int CurrentNodeId = -1;

        /// <summary>현재 방을 완료했는지 여부. 완료해야 다음 노드를 고를 수 있다.</summary>
        public bool CurrentRoomCleared;

        /// <summary>지나온 노드 번호를 순서대로 담는다. 현재 노드도 포함한다.</summary>
        public List<int> VisitedNodeIds = new List<int>();

        /// <summary>아직 아무 방도 고르지 않은 상태인지.</summary>
        public bool IsAtStart
        {
            get { return CurrentNodeId < 0; }
        }

        /// <summary>해당 노드를 이미 지나왔는지.</summary>
        public bool IsVisited(int nodeId)
        {
            return VisitedNodeIds.Contains(nodeId);
        }

        /// <summary>해당 노드를 완료했는지. 현재 방은 완료 표시 전까지 완료가 아니다.</summary>
        public bool IsCleared(int nodeId)
        {
            if (!IsVisited(nodeId))
            {
                return false;
            }

            if (nodeId == CurrentNodeId)
            {
                return CurrentRoomCleared;
            }

            return true;
        }

        /// <summary>간선을 지나왔는지. 지나온 노드가 순서대로 이어진 경우에만 참이다.</summary>
        public bool IsTraveled(MapEdge edge)
        {
            for (int i = 0; i + 1 < VisitedNodeIds.Count; i++)
            {
                if (VisitedNodeIds[i] == edge.FromNodeId && VisitedNodeIds[i + 1] == edge.ToNodeId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지금 고를 수 있는 노드 목록을 채운다.
        /// 시작 전에는 1단계 전체, 그 뒤에는 현재 노드에서 이어지는 다음 단계의 노드이다.
        /// 현재 방을 완료하지 않았으면 고를 수 있는 노드가 없다.
        /// </summary>
        public void GetSelectableNodes(StageMap map, List<MapNode> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            result.Clear();
            if (map == null)
            {
                return;
            }

            if (IsAtStart)
            {
                IReadOnlyList<MapNode> startNodes = map.GetStartNodes();
                for (int i = 0; i < startNodes.Count; i++)
                {
                    result.Add(startNodes[i]);
                }

                return;
            }

            if (!CurrentRoomCleared)
            {
                return;
            }

            MapNode current = map.GetNode(CurrentNodeId);
            if (current == null)
            {
                return;
            }

            for (int i = 0; i < current.NextNodeIds.Count; i++)
            {
                MapNode next = map.GetNode(current.NextNodeIds[i]);
                if (next != null)
                {
                    result.Add(next);
                }
            }
        }

        /// <summary>고를 수 있는 노드인지 확인한다.</summary>
        public bool CanSelect(StageMap map, int nodeId)
        {
            if (map == null)
            {
                return false;
            }

            if (IsAtStart)
            {
                MapNode node = map.GetNode(nodeId);
                return node != null && node.Stage == 1;
            }

            if (!CurrentRoomCleared)
            {
                return false;
            }

            MapNode current = map.GetNode(CurrentNodeId);
            return current != null && current.NextNodeIds.Contains(nodeId);
        }

        /// <summary>해당 노드로 들어간다. 고를 수 없는 노드면 false를 돌려주고 아무것도 바꾸지 않는다.</summary>
        public bool EnterNode(StageMap map, int nodeId)
        {
            if (!CanSelect(map, nodeId))
            {
                return false;
            }

            CurrentNodeId = nodeId;
            CurrentRoomCleared = false;
            VisitedNodeIds.Add(nodeId);
            return true;
        }

        /// <summary>현재 방을 완료 처리한다.</summary>
        public void ClearCurrentRoom()
        {
            if (!IsAtStart)
            {
                CurrentRoomCleared = true;
            }
        }

        /// <summary>진행 상태를 처음으로 되돌린다.</summary>
        public void Reset()
        {
            CurrentNodeId = -1;
            CurrentRoomCleared = false;
            VisitedNodeIds.Clear();
        }
    }
}
