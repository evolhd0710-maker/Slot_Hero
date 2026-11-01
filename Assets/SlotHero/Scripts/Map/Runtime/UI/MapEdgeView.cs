using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Map.UI
{
    /// <summary>
    /// 간선 하나가 지금 어떻게 보여야 하는지.
    /// 와이어프레임 `10 맵 화면 · 지나간 경로` 는 지나온 길과 앞으로 갈 길을 같은 진한 색으로,
    /// 지나친 방에 닿는 길만 회색으로 그린다.
    /// </summary>
    public enum MapEdgeDisplayState
    {
        /// <summary>지나온 간선.</summary>
        Traveled,

        /// <summary>아직 갈 수 있는 간선.</summary>
        Open,

        /// <summary>지나친 방에 닿아 더는 갈 수 없는 간선.</summary>
        Closed,
    }

    /// <summary>
    /// 노드 사이를 잇는 간선 하나의 표시.
    /// 아이콘 모양 가장자리에서 MapVisualConfig.EdgeGap 만큼 떨어진 지점부터 그린다.
    /// 프리팹 구성: 루트에 이 스크립트와 Image 하나. 피벗은 (0, 0.5)로 둔다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MapEdgeView : MonoBehaviour
    {
        [SerializeField] private Image _line;

        private RectTransform _rectTransform;

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
            if (_line == null)
            {
                _line = GetComponent<Image>();
            }
        }

        /// <summary>두 지점을 잇도록 위치, 길이, 각도를 맞춘다.</summary>
        public void Bind(Vector2 from, Vector2 to, MapEdgeDisplayState state, MapVisualConfig visual)
        {
            Vector2 delta = to - from;
            float distance = delta.magnitude;

            // 아이콘 모양 반지름과 간격만큼 양끝을 잘라 낸다.
            // 아이콘 그림은 노드 칸을 다 채우지 않아 칸 가장자리에서 재면 틈이 너무 벌어진다.
            float inset = visual.NodeSize * 0.5f * visual.IconShapeRatio + visual.EdgeGap;
            float length = Mathf.Max(0f, distance - inset * 2f);

            Vector2 direction = distance > 0.0001f ? delta / distance : Vector2.right;
            Vector2 start = from + direction * inset;

            RectTransform rect = RectTransform;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = start;
            rect.sizeDelta = new Vector2(length, visual.EdgeThickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            if (_line != null)
            {
                _line.color = GetColor(state, visual);
                _line.raycastTarget = false;
            }

            gameObject.SetActive(length > 0.5f);
        }

        /// <summary>간선 상태에 맞는 색.</summary>
        public static Color GetColor(MapEdgeDisplayState state, MapVisualConfig visual)
        {
            switch (state)
            {
                case MapEdgeDisplayState.Traveled:
                    return visual.TraveledEdgeColor;
                case MapEdgeDisplayState.Closed:
                    return visual.ClosedEdgeColor;
                default:
                    return visual.OpenEdgeColor;
            }
        }
    }
}
