using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Map
{
    /// <summary>방 타입 하나의 표시용 자료.</summary>
    [Serializable]
    public struct RoomTypeVisual
    {
        public RoomType RoomType;

        [Tooltip("노드 아이콘. 비워 두면 아이콘 없이 색만 쓴다.")]
        public Sprite Icon;

        [Tooltip("맵 생성 규칙 기획서 04 방 목록의 노드 표시 색.")]
        public Color Color;
    }

    /// <summary>
    /// 맵 화면의 크기와 색을 담는다.
    /// 기본값은 인게임 화면 기획서 v0.2 / 09 맵 화면 기준이다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MapVisualConfig",
        menuName = "Slot Hero/맵/맵 화면 표시 설정",
        order = 1)]
    public class MapVisualConfig : ScriptableObject
    {
        [Header("양피지")]
        [Tooltip("지도 뒤에 까는 양피지 그림의 크기. 화면 가운데에 놓는다. " +
                 "와이어프레임 `10 맵 화면` 의 `지도 배경 · 양피지_세로확장2.png` 실측 (60, -60) 1800 x 1200 이다. " +
                 "양 끝의 말린 부분까지 들어간 크기라 화면 위아래로 조금 넘친다.")]
        public Vector2 ParchmentSize = UiScale.V(1800f, 1200f);

        [Header("크기")]
        [Tooltip("노드를 늘어놓는 영역의 크기. 화면 가운데에 놓는다.\n" +
                 "**기획서는 1600 x 800 인데 와이어프레임대로 1300 x 500 이다.** 2026년 10월 3일에 원재가 정했다. " +
                 "양피지의 종이 몸통이 화면 기준 가로 약 1270, 세로 약 510 이라 " +
                 "1600 x 800 은 말린 끝과 찢긴 가장자리 밖으로 넘친다. " +
                 "와이어프레임 `10 맵 화면` 의 `맵 그래프.png` 실측 (310, 290) 1300 x 500 이다.")]
        public Vector2 MapSize = UiScale.V(1300f, 500f);

        [Tooltip("노드 칸 한 변의 크기. 기획서 기준 70.\n" +
                 "아이콘 그림은 칸을 다 채우지 않는다. 일반 방 모양이 64 가운데 50 x 47 이라 " +
                 "70 칸에서는 55 x 51 로 보인다. 와이어프레임의 일반 방 모양 57 x 48 과 거의 같다.")]
        public float NodeSize = UiScale.Px(70f);

        [Tooltip("아이콘 모양이 노드 칸에서 실제로 차지하는 비율. 간선 시작 자리와 누르는 영역을 여기에 맞춘다.\n" +
                 "일반 방 아이콘이 64 가운데 50 을 차지해 0.78 이다. 방 대부분이 일반 방이다.")]
        [Range(0.5f, 1f)]
        public float IconShapeRatio = 0.78f;

        [Tooltip("가장 넓은 아이콘 모양의 비율. 노드끼리 닿는지 볼 때 쓴다.\n" +
                 "이벤트 방 아이콘이 64 가운데 54 를 차지해 0.85 다.")]
        [Range(0.5f, 1f)]
        public float WidestIconShapeRatio = 0.85f;

        [Tooltip("간선을 아이콘 모양에서 몇 픽셀 떨어뜨려 그릴지.\n" +
                 "**기획서는 노드에서 10 인데 와이어프레임 실측으로 3 이다.** " +
                 "예전에는 칸 가장자리에서 10 을 띄워 눈에 보이는 틈이 18 가까이 됐다. " +
                 "지금은 아이콘 모양에서 잰다.")]
        public float EdgeGap = UiScale.Px(3f);

        [Tooltip("간선의 두께. **와이어프레임 실측 3 이다.** " +
                 "자리와 크기를 10 단위로 맞추는 규칙에서 뺐다. 10 으로 두면 와이어프레임보다 세 배 굵어 보인다.")]
        public float EdgeThickness = UiScale.Px(3f);

        [Tooltip("완료 체크 그림의 크기. 그림 원본 32 x 32 가운데 모양이 28 x 22 다.\n" +
                 "와이어프레임의 체크 모양이 약 31 x 25 라 36 으로 키워 31.5 x 24.8 로 맞췄다. " +
                 "간선 두께처럼 10 단위 규칙에서 뺐다.")]
        public Vector2 CheckMarkSize = UiScale.V(36f, 36f);

        [Tooltip("완료 체크를 노드 칸 오른쪽 위 모서리에서 얼마나 밀지. 오른쪽과 위가 양수다.\n" +
                 "와이어프레임은 체크가 아이콘 모양 오른쪽 끝에서 6, 위 끝에서 4 튀어나와 있다. " +
                 "70 칸에서는 아이콘 모양이 칸 모서리에서 약 9 안쪽이라 체크 그림 모서리를 칸 모서리에 맞추면 그 자리가 된다.")]
        public Vector2 CheckMarkOffset = Vector2.zero;

        [Header("방 타입 표시")]
        [Tooltip("노드 아이콘에 방 타입 색을 입힐지 여부. " +
                 "맵 화면 시안은 방 타입을 아이콘 모양으로 구분하고 색은 진행 상태에만 쓰므로 기본은 꺼 둔다. " +
                 "켜면 맵 생성 규칙 기획서 04 방 목록의 노드 표시 색을 아이콘에 입힌다.")]
        public bool UseRoomTypeColor;

        public List<RoomTypeVisual> RoomVisuals = new List<RoomTypeVisual>
        {
            new RoomTypeVisual { RoomType = RoomType.Normal, Color = new Color(0.95f, 0.78f, 0.20f) },
            new RoomTypeVisual { RoomType = RoomType.Elite, Color = new Color(0.62f, 0.17f, 0.15f) },
            new RoomTypeVisual { RoomType = RoomType.Sanctum, Color = new Color(0.25f, 0.60f, 0.35f) },
            new RoomTypeVisual { RoomType = RoomType.Event, Color = new Color(0.55f, 0.53f, 0.50f) },
            new RoomTypeVisual { RoomType = RoomType.Etc, Color = new Color(0.70f, 0.68f, 0.65f) },
            new RoomTypeVisual { RoomType = RoomType.Boss, Color = new Color(0.12f, 0.11f, 0.09f) },
        };

        [Header("노드 상태")]
        // 노드 아이콘은 흰 실루엣이다. 여기 색이 그대로 아이콘 색이 된다.
        // 어두운 그림에 색을 곱하면 더 어두워지기만 해 와이어프레임의 갈색과 옅은 회색이 나올 수 없었다.

        [Tooltip("완료한 방. 노드 우상단에 초록 체크를 함께 표시한다. 와이어프레임의 #2B2016.")]
        public Color ClearedTint = new Color(0.169f, 0.125f, 0.086f, 1f);

        [Tooltip("지나친 단계의 다른 방과 더는 닿을 수 없는 방. 와이어프레임 `10 맵 화면 · 지나간 경로` 의 #958E83.")]
        public Color SkippedTint = new Color(0.584f, 0.557f, 0.514f, 1f);

        [Tooltip("이후 단계의 방. 와이어프레임의 #2B2016.")]
        public Color FutureTint = new Color(0.169f, 0.125f, 0.086f, 1f);

        [Tooltip("지금 고를 수 있는 방. 와이어프레임은 다른 방과 같은 #2B2016 이고 크기로만 가른다.")]
        public Color SelectableTint = new Color(0.169f, 0.125f, 0.086f, 1f);

        [Tooltip("지금 머물러 있는 방. 와이어프레임의 #2B2016.")]
        public Color CurrentTint = new Color(0.169f, 0.125f, 0.086f, 1f);

        [Tooltip("완료 표시 체크의 색. 스프라이트가 이미 초록이면 흰색으로 두면 된다.")]
        public Color CheckMarkColor = Color.white;

        [Tooltip("고를 수 있는 노드를 키우는 비율. 04 화면 공통 규칙의 카드 상태를 따라 5퍼센트를 기본으로 한다.")]
        public float SelectableScale = 1.05f;

        [Header("간선 상태")]
        [Tooltip("지나온 간선. 와이어프레임의 #2B2016, 불투명도 0.9.")]
        public Color TraveledEdgeColor = new Color(0.169f, 0.125f, 0.086f, 0.9f);

        [Tooltip("아직 갈 수 있는 간선. 와이어프레임은 지나온 간선과 같은 색이다.")]
        public Color OpenEdgeColor = new Color(0.169f, 0.125f, 0.086f, 0.9f);

        [Tooltip("지나친 방에 닿는 간선. 더는 갈 수 없는 길이다. " +
                 "와이어프레임 `10 맵 화면 · 지나간 경로` 의 #968F84, 불투명도 0.78.")]
        public Color ClosedEdgeColor = new Color(0.588f, 0.561f, 0.518f, 0.78f);

        [Header("지도 잠깐 보기")]
        [Tooltip("방 진행 중에 지도 버튼으로 지도를 잠깐 띄울 때 지도 뒤에 까는 막의 색. " +
                 "원재가 2026년 10월 5일에 반투명 검은 막 위에 지도를 띄우는 방식으로 정했다. " +
                 "불투명도는 75퍼센트다. 유니티가 반투명을 선형 색 공간에서 섞어 60퍼센트로는 " +
                 "눈에 35퍼센트쯤으로만 어두워져 아래 방 화면이 거의 그대로 보였다.")]
        public Color PeekDimColor = new Color(0f, 0f, 0f, 0.75f);

        [Header("화면 이동")]
        [Tooltip("지도가 화면보다 넓을 때 좌우 이동을 허용한다. 세로 이동과 확대 축소는 지원하지 않는다.")]
        public bool AllowHorizontalPan = true;

        [Tooltip("휠 한 칸에 움직이는 거리.")]
        public float ScrollSpeed = UiScale.Px(120f);

        /// <summary>
        /// 이웃 노드 중심이 어떤 지터에서도 이만큼은 떨어져 있다. 가로와 세로 가운데 좁은 쪽.
        ///
        /// 두 이웃 노드가 서로를 향해 지터 끝까지 쏠리면 중심 사이가
        /// 격자 간격 × (1 - 지터 × 2) 까지 좁아진다. 가로는 단계 사이, 세로는 줄 사이를 본다.
        /// </summary>
        public float ClosestNodeDistance(int stageCount, int rowCount, float jitterX, float jitterY)
        {
            float closest = float.MaxValue;

            if (stageCount >= 2)
            {
                float column = (MapSize.x - NodeSize) / (stageCount - 1);
                closest = Mathf.Min(closest, column * (1f - jitterX * 2f));
            }

            if (rowCount >= 2)
            {
                float row = (MapSize.y - NodeSize) / (rowCount - 1);
                closest = Mathf.Min(closest, row * (1f - jitterY * 2f));
            }

            return closest;
        }

        /// <summary>
        /// 이웃 노드의 아이콘 모양이 어떤 지터에서도 겹치지 않는지.
        ///
        /// 노드 칸은 아이콘 모양보다 넓어 칸끼리는 조금 겹쳐도 된다.
        /// 가장 넓은 아이콘 모양끼리 닿지 않으면 된다.
        /// </summary>
        public bool NodesNeverOverlap(int stageCount, int rowCount, float jitterX, float jitterY)
        {
            return ClosestNodeDistance(stageCount, rowCount, jitterX, jitterY) > NodeSize * WidestIconShapeRatio;
        }

        /// <summary>
        /// 이웃 노드의 누르는 영역이 어떤 지터에서도 겹치지 않는지.
        /// 누르는 영역은 노드 칸에서 아이콘 모양 비율만큼만 쓴다.
        /// </summary>
        public bool ClickAreasNeverOverlap(int stageCount, int rowCount, float jitterX, float jitterY)
        {
            return ClosestNodeDistance(stageCount, rowCount, jitterX, jitterY) > NodeSize * IconShapeRatio;
        }

        /// <summary>노드 영역이 양피지 안에 들어가는지. 둘 다 화면 가운데에 놓인다.</summary>
        public bool MapFitsInsideParchment()
        {
            return MapSize.x < ParchmentSize.x && MapSize.y < ParchmentSize.y;
        }

        /// <summary>해당 방 타입의 표시 자료를 찾는다.</summary>
        public RoomTypeVisual GetVisual(RoomType roomType)
        {
            for (int i = 0; i < RoomVisuals.Count; i++)
            {
                if (RoomVisuals[i].RoomType == roomType)
                {
                    return RoomVisuals[i];
                }
            }

            return new RoomTypeVisual { RoomType = roomType, Color = Color.white };
        }
    }
}
