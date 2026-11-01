using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Map.UI
{
    /// <summary>
    /// 격자 좌표를 지도 영역 안의 픽셀 좌표로 바꾼다.
    /// 지도 영역의 가운데가 원점이며 위쪽이 양수이다.
    /// </summary>
    public struct MapLayout
    {
        public Vector2 MapSize;
        public float NodeSize;
        public int StageCount;
        public int RowCount;

        public MapLayout(MapVisualConfig visual, StageMap map)
        {
            // 설정이 없을 때 쓰는 값. `MapVisualConfig` 의 기본값과 같게 둔다.
            MapSize = visual != null ? visual.MapSize : UiScale.V(1300f, 500f);
            NodeSize = visual != null ? visual.NodeSize : UiScale.Px(70f);
            StageCount = map != null ? Mathf.Max(map.StageCount, 2) : 12;
            RowCount = map != null ? Mathf.Max(map.RowCount, 2) : 5;
        }

        /// <summary>단계 사이의 가로 간격.</summary>
        public float ColumnSpacing
        {
            get { return (MapSize.x - NodeSize) / (StageCount - 1); }
        }

        /// <summary>줄 사이의 세로 간격.</summary>
        public float RowSpacing
        {
            get { return (MapSize.y - NodeSize) / (RowCount - 1); }
        }

        /// <summary>노드의 지도 영역 안 좌표. 지터를 반영한다.</summary>
        public Vector2 GetPosition(MapNode node)
        {
            if (node == null)
            {
                return Vector2.zero;
            }

            float left = -MapSize.x * 0.5f + NodeSize * 0.5f;
            float top = MapSize.y * 0.5f - NodeSize * 0.5f;

            float x = left + (node.Stage - 1) * ColumnSpacing + node.JitterX * ColumnSpacing;
            float y = top - node.Row * RowSpacing + node.JitterY * RowSpacing;

            // 지터가 첫 칸과 마지막 칸을 지도 밖으로 밀어낸다.
            // 그대로 두면 가장자리 노드가 잘려 보이므로 지도 안에 가둔다.
            return new Vector2(
                Mathf.Clamp(x, left, -left),
                Mathf.Clamp(y, -top, top));
        }
    }
}
