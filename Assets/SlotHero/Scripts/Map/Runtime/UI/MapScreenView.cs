using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SlotHero.Map.UI
{
    /// <summary>
    /// 맵 화면의 표시를 담당한다. 노드와 간선을 만들고 상태에 맞게 갱신한다.
    /// 방 진입 같은 진행 처리는 <see cref="MapScreenController"/>가 맡는다.
    ///
    /// 계층 구성
    ///   MapScreen (Canvas 아래)
    ///     Viewport        : 이 스크립트를 붙인다. 지도 영역 1600 x 800, Mask 와 투명 Image
    ///       Content       : 크기 1600 x 800, 피벗 가운데
    ///         EdgeLayer
    ///         NodeLayer
    /// </summary>
    public class MapScreenView : MonoBehaviour, IDragHandler, IScrollHandler
    {
        [Header("참조")]
        [SerializeField] private MapVisualConfig _visual;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _edgeLayer;
        [SerializeField] private RectTransform _nodeLayer;
        [SerializeField] private MapNodeView _nodePrefab;
        [SerializeField] private MapEdgeView _edgePrefab;

        [Header("표시 규칙")]
        [Tooltip("현재 위치에서 더 이상 갈 수 없는 방도 회색으로 흐리게 표시한다.")]
        [SerializeField] private bool _dimUnreachable = true;

        private readonly List<MapNodeView> _nodeViews = new List<MapNodeView>();
        private readonly List<MapEdgeView> _edgeViews = new List<MapEdgeView>();
        private readonly Dictionary<int, MapNodeView> _nodeViewById = new Dictionary<int, MapNodeView>();

        private StageMap _map;
        private MapProgress _progress;
        private MapLayout _layout;

        /// <summary>노드를 눌렀을 때. 고를 수 없는 노드는 애초에 눌리지 않는다.</summary>
        public event Action<MapNode> NodeClicked;

        public MapVisualConfig Visual
        {
            get { return _visual; }
        }

        /// <summary>맵을 받아 노드와 간선 표시를 새로 만든다.</summary>
        public void Build(StageMap map, MapProgress progress)
        {
            _map = map;
            _progress = progress;

            ClearViews();

            if (_map == null || _visual == null)
            {
                return;
            }

            _layout = new MapLayout(_visual, _map);

            if (_content != null)
            {
                // 기준점과 피벗을 보이는 칸 한가운데에 모은다.
                // 이걸 안 하고 네 귀퉁이에 펼쳐 두면 `sizeDelta` 가 크기가 아니라
                // **부모 바깥으로 더 나가는 여백**으로 읽혀 지도가 배율만큼 커진다.
                // 그러면 노드가 사방으로 흩어져 스크롤해야 전부 볼 수 있게 된다.
                _content.anchorMin = new Vector2(0.5f, 0.5f);
                _content.anchorMax = new Vector2(0.5f, 0.5f);
                _content.pivot = new Vector2(0.5f, 0.5f);
                _content.sizeDelta = _visual.MapSize;
                _content.anchoredPosition = Vector2.zero;
            }

            for (int i = 0; i < _map.Edges.Count; i++)
            {
                MapEdgeView view = Instantiate(_edgePrefab, _edgeLayer);
                _edgeViews.Add(view);
            }

            for (int i = 0; i < _map.Nodes.Count; i++)
            {
                MapNodeView view = Instantiate(_nodePrefab, _nodeLayer);
                _nodeViews.Add(view);
                _nodeViewById[_map.Nodes[i].Id] = view;
            }

            Refresh();
        }

        /// <summary>진행 상태만 바뀌었을 때 표시를 다시 맞춘다.</summary>
        public void Refresh()
        {
            if (_map == null || _visual == null)
            {
                return;
            }

            HashSet<int> reachable = _dimUnreachable ? CollectReachableFromCurrent() : null;

            Dictionary<int, MapNodeDisplayState> nodeStates = new Dictionary<int, MapNodeDisplayState>();

            for (int i = 0; i < _map.Nodes.Count && i < _nodeViews.Count; i++)
            {
                MapNode node = _map.Nodes[i];
                MapNodeDisplayState state = GetDisplayState(node, reachable);
                nodeStates[node.Id] = state;
                _nodeViews[i].Bind(node, state, _visual, _layout, HandleNodeClicked);
            }

            for (int i = 0; i < _map.Edges.Count && i < _edgeViews.Count; i++)
            {
                MapEdge edge = _map.Edges[i];
                MapNode from = _map.GetNode(edge.FromNodeId);
                MapNode to = _map.GetNode(edge.ToNodeId);
                if (from == null || to == null)
                {
                    continue;
                }

                MapEdgeDisplayState state = GetEdgeState(edge, nodeStates);
                _edgeViews[i].Bind(_layout.GetPosition(from), _layout.GetPosition(to), state, _visual);
            }
        }

        /// <summary>
        /// 간선의 표시 상태. 지나온 길이 먼저이고, 양끝 가운데 하나라도 지나친 방이면 막힌 길이다.
        /// 와이어프레임 `10 맵 화면 · 지나간 경로` 를 따른다.
        /// </summary>
        private MapEdgeDisplayState GetEdgeState(MapEdge edge, Dictionary<int, MapNodeDisplayState> nodeStates)
        {
            if (_progress != null && _progress.IsTraveled(edge))
            {
                return MapEdgeDisplayState.Traveled;
            }

            MapNodeDisplayState fromState;
            MapNodeDisplayState toState;
            bool fromSkipped = nodeStates.TryGetValue(edge.FromNodeId, out fromState) && fromState == MapNodeDisplayState.Skipped;
            bool toSkipped = nodeStates.TryGetValue(edge.ToNodeId, out toState) && toState == MapNodeDisplayState.Skipped;

            return fromSkipped || toSkipped ? MapEdgeDisplayState.Closed : MapEdgeDisplayState.Open;
        }

        /// <summary>노드 번호로 표시를 찾는다. 연출을 붙일 때 쓴다.</summary>
        public MapNodeView GetNodeView(int nodeId)
        {
            MapNodeView view;
            return _nodeViewById.TryGetValue(nodeId, out view) ? view : null;
        }

        /// <summary>현재 노드가 화면 밖에 있으면 가로로 끌어와 보이게 한다.</summary>
        public void FocusOnCurrentNode()
        {
            if (_map == null || _progress == null || _content == null || _viewport == null)
            {
                return;
            }

            if (_progress.IsAtStart)
            {
                return;
            }

            MapNode current = _map.GetNode(_progress.CurrentNodeId);
            if (current == null)
            {
                return;
            }

            float target = -_layout.GetPosition(current).x;
            _content.anchoredPosition = ClampContentPosition(new Vector2(target, 0f));
        }

        private MapNodeDisplayState GetDisplayState(MapNode node, HashSet<int> reachable)
        {
            if (_progress == null)
            {
                return MapNodeDisplayState.Future;
            }

            if (_progress.IsVisited(node.Id))
            {
                bool isCurrentAndUncleared = node.Id == _progress.CurrentNodeId && !_progress.CurrentRoomCleared;
                return isCurrentAndUncleared ? MapNodeDisplayState.Current : MapNodeDisplayState.Cleared;
            }

            if (_progress.CanSelect(_map, node.Id))
            {
                return MapNodeDisplayState.Selectable;
            }

            // 지나친 단계의 다른 방
            int currentStage = 0;
            if (!_progress.IsAtStart)
            {
                MapNode current = _map.GetNode(_progress.CurrentNodeId);
                currentStage = current != null ? current.Stage : 0;
            }

            if (node.Stage <= currentStage)
            {
                return MapNodeDisplayState.Skipped;
            }

            // 현재 위치에서 더 이상 닿을 수 없는 방
            if (reachable != null && !reachable.Contains(node.Id))
            {
                return MapNodeDisplayState.Skipped;
            }

            return MapNodeDisplayState.Future;
        }

        private HashSet<int> CollectReachableFromCurrent()
        {
            HashSet<int> reachable = new HashSet<int>();
            if (_map == null)
            {
                return reachable;
            }

            Queue<int> queue = new Queue<int>();

            if (_progress == null || _progress.IsAtStart)
            {
                IReadOnlyList<MapNode> startNodes = _map.GetStartNodes();
                for (int i = 0; i < startNodes.Count; i++)
                {
                    if (reachable.Add(startNodes[i].Id))
                    {
                        queue.Enqueue(startNodes[i].Id);
                    }
                }
            }
            else if (reachable.Add(_progress.CurrentNodeId))
            {
                queue.Enqueue(_progress.CurrentNodeId);
            }

            while (queue.Count > 0)
            {
                MapNode node = _map.GetNode(queue.Dequeue());
                if (node == null)
                {
                    continue;
                }

                for (int i = 0; i < node.NextNodeIds.Count; i++)
                {
                    if (reachable.Add(node.NextNodeIds[i]))
                    {
                        queue.Enqueue(node.NextNodeIds[i]);
                    }
                }
            }

            return reachable;
        }

        private void HandleNodeClicked(MapNodeView view)
        {
            if (view == null || view.Node == null)
            {
                return;
            }

            if (NodeClicked != null)
            {
                NodeClicked(view.Node);
            }
        }

        private void ClearViews()
        {
            for (int i = 0; i < _nodeViews.Count; i++)
            {
                if (_nodeViews[i] != null)
                {
                    Destroy(_nodeViews[i].gameObject);
                }
            }

            for (int i = 0; i < _edgeViews.Count; i++)
            {
                if (_edgeViews[i] != null)
                {
                    Destroy(_edgeViews[i].gameObject);
                }
            }

            _nodeViews.Clear();
            _edgeViews.Clear();
            _nodeViewById.Clear();
        }

        // 화면 이동. 기획서에 따라 좌우만 움직이며 세로 이동과 확대 축소는 지원하지 않는다.

        public void OnDrag(PointerEventData eventData)
        {
            if (_visual == null || !_visual.AllowHorizontalPan || _content == null)
            {
                return;
            }

            Vector2 next = _content.anchoredPosition + new Vector2(eventData.delta.x, 0f);
            _content.anchoredPosition = ClampContentPosition(next);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_visual == null || !_visual.AllowHorizontalPan || _content == null)
            {
                return;
            }

            float amount = eventData.scrollDelta.y * _visual.ScrollSpeed;
            Vector2 next = _content.anchoredPosition + new Vector2(amount, 0f);
            _content.anchoredPosition = ClampContentPosition(next);
        }

        private Vector2 ClampContentPosition(Vector2 position)
        {
            if (_viewport == null || _content == null)
            {
                return new Vector2(position.x, 0f);
            }

            float overflow = Mathf.Max(0f, _content.rect.width - _viewport.rect.width);
            float limit = overflow * 0.5f;
            return new Vector2(Mathf.Clamp(position.x, -limit, limit), 0f);
        }
    }
}
