using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Map.UI
{
    /// <summary>맵 화면에서 노드가 가질 수 있는 표시 상태.</summary>
    public enum MapNodeDisplayState
    {
        /// <summary>이후 단계의 방. 진한 갈색으로 표시한다.</summary>
        Future,

        /// <summary>지금 고를 수 있는 방.</summary>
        Selectable,

        /// <summary>지금 머물러 있는 방.</summary>
        Current,

        /// <summary>완료한 방. 우상단에 초록 체크를 표시한다.</summary>
        Cleared,

        /// <summary>지나친 단계의 다른 방과 더는 닿을 수 없는 방. 옅은 회색으로 표시한다.</summary>
        Skipped,
    }

    /// <summary>
    /// 맵 노드 하나의 표시. 기획서 기준 70 x 70 이다.
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 자식에 아이콘 Image와 완료 체크 Image.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MapNodeView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _checkMark;
        [SerializeField] private Button _button;

        private RectTransform _rectTransform;
        private Action<MapNodeView> _onClicked;

        /// <summary>이 표시가 담당하는 노드.</summary>
        public MapNode Node { get; private set; }

        /// <summary>현재 표시 상태.</summary>
        public MapNodeDisplayState State { get; private set; }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = (RectTransform)transform;
                }

                return _rectTransform;
            }
        }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>노드 정보와 상태를 반영한다.</summary>
        public void Bind(
            MapNode node,
            MapNodeDisplayState state,
            MapVisualConfig visual,
            MapLayout layout,
            Action<MapNodeView> onClicked)
        {
            Node = node;
            State = state;
            _onClicked = onClicked;

            RectTransform rect = RectTransform;
            rect.sizeDelta = new Vector2(visual.NodeSize, visual.NodeSize);
            rect.anchoredPosition = layout.GetPosition(node);

            bool selectable = state == MapNodeDisplayState.Selectable;
            float scale = selectable ? visual.SelectableScale : 1f;
            rect.localScale = new Vector3(scale, scale, 1f);

            if (_icon != null)
            {
                RoomTypeVisual roomVisual = visual.GetVisual(node.RoomType);
                if (roomVisual.Icon != null)
                {
                    _icon.sprite = roomVisual.Icon;
                }

                Color baseColor = visual.UseRoomTypeColor ? roomVisual.Color : Color.white;
                _icon.color = baseColor * GetStateTint(state, visual);

                // 누르는 영역을 아이콘 모양 크기로 줄인다.
                // 노드 칸은 아이콘 모양보다 넓어 이웃 칸끼리 조금 겹칠 수 있다. 겹친 자리를 누르면 엉뚱한 방이 골라진다.
                float padding = GetClickPadding(visual);
                _icon.raycastPadding = new Vector4(padding, padding, padding, padding);
            }

            if (_checkMark != null)
            {
                bool showCheck = state == MapNodeDisplayState.Cleared;
                _checkMark.enabled = showCheck;
                _checkMark.color = visual.CheckMarkColor;
                _checkMark.raycastTarget = false;

                RectTransform checkRect = _checkMark.rectTransform;
                checkRect.anchorMin = new Vector2(1f, 1f);
                checkRect.anchorMax = new Vector2(1f, 1f);
                checkRect.pivot = new Vector2(1f, 1f);
                checkRect.sizeDelta = visual.CheckMarkSize;
                checkRect.anchoredPosition = visual.CheckMarkOffset;
            }

            if (_button != null)
            {
                _button.interactable = selectable;
            }

            gameObject.name = string.Format("Node_{0}_{1}단계{2}줄", node.Id, node.Stage, node.Row);
        }

        /// <summary>노드 칸 각 변에서 누르는 영역을 안쪽으로 줄이는 거리.</summary>
        public static float GetClickPadding(MapVisualConfig visual)
        {
            return visual.NodeSize * (1f - visual.IconShapeRatio) * 0.5f;
        }

        private static Color GetStateTint(MapNodeDisplayState state, MapVisualConfig visual)
        {
            switch (state)
            {
                case MapNodeDisplayState.Cleared:
                    return visual.ClearedTint;
                case MapNodeDisplayState.Skipped:
                    return visual.SkippedTint;
                case MapNodeDisplayState.Selectable:
                    return visual.SelectableTint;
                case MapNodeDisplayState.Current:
                    return visual.CurrentTint;
                default:
                    return visual.FutureTint;
            }
        }

        private void HandleClick()
        {
            if (_onClicked != null)
            {
                _onClicked(this);
            }
        }
    }
}
