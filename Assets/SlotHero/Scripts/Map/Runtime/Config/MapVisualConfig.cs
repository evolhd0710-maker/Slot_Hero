using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Map
{
    /// <summary>방 타입 하나의 표시용 자료.</summary>
    [Serializable]
    public struct RoomTypeVisual
    {
        [FormerlySerializedAs("RoomType")]
        [SerializeField] private RoomType _roomType;
        public RoomType RoomType { get { return _roomType; } set { _roomType = value; } }

        [Tooltip("노드 아이콘. 비워 두면 아이콘 없이 색만 쓴다.")]
        [FormerlySerializedAs("Icon")]
        [SerializeField] private Sprite _icon;
        public Sprite Icon { get { return _icon; } set { _icon = value; } }

        [Tooltip("아이콘 안쪽 무늬(몬스터 눈, 성소 받침의 선 같은 것). 몸통과 따로 밝은 색으로 얹는다(`MapVisualConfig.IconDetailColor`). " +
                 "몸통에 진한 갈색을 곱하면 검은 무늬가 묻혀 나눴다(2026년 10월 25일 원재). 비워 두면 얹지 않는다.")]
        [SerializeField] private Sprite _detailIcon;
        public Sprite DetailIcon { get { return _detailIcon; } set { _detailIcon = value; } }

        [Tooltip("맵 생성 규칙 기획서 04 방 목록의 노드 표시 색.")]
        [FormerlySerializedAs("Color")]
        [SerializeField] private Color _color;
        public Color Color { get { return _color; } set { _color = value; } }
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
        [FormerlySerializedAs("ParchmentSize")]
        [SerializeField] private Vector2 _parchmentSize = UiScale.V(1800f, 1200f);
        public Vector2 ParchmentSize { get { return _parchmentSize; } set { _parchmentSize = value; } }

        // 효과음과 이펙트 목록 E05 두루마리 펼치기. 맵 화면을 열 때 가운데에 말려 있던 두루마리가 양쪽으로 풀린다.
        // 그림은 `Map/Art/두루마리 완성9 20261031` 이고 조각 일곱이 모두 지도_양피지와 같은 1774 x 1183 판에 제자리로 놓여 있다.
        // 아래 네 칸은 그 판 위의 픽셀 실측이다. 왼쪽 위가 (0, 0) 이고 아래로 갈수록 y 가 커진다. 그림을 바꾸면 다시 잰다.
        [Header("두루마리 펼치기")]
        [Tooltip("두루마리가 다 펼쳐지는 데 걸리는 초. 0 이면 펼치지 않고 바로 다 펼친 지도가 나온다. 0.5초는 2026년 10월 31일 원재가 정했다.")]
        [SerializeField, Min(0f)] private float _unrollSeconds = 0.5f;
        public float UnrollSeconds { get { return _unrollSeconds; } set { _unrollSeconds = value; } }

        [Tooltip("맵 화면을 닫을 때 두루마리를 다시 마는 데 걸리는 초. 펼치기를 거꾸로 돌린다. " +
                 "0 이면 말지 않고 바로 닫는다. 0.3초는 2026년 10월 31일 원재가 정했다.")]
        [SerializeField, Min(0f)] private float _rollUpSeconds = 0.3f;
        public float RollUpSeconds { get { return _rollUpSeconds; } set { _rollUpSeconds = value; } }

        [Tooltip("말린 두루마리 그림이 사라지기 시작하는 진행도(0 ~ 1). 이때부터 끝까지 서서히 투명해지고 아래의 펼친 두루마리 그림이 드러난다.")]
        [SerializeField, Range(0f, 0.99f)] private float _unrollRolledFadeStart = 0.5f;
        public float UnrollRolledFadeStart { get { return _unrollRolledFadeStart; } set { _unrollRolledFadeStart = value; } }

        [Tooltip("다 펼친 뒤 노드와 간선이 나타나는 데 걸리는 초. 이 동안에도 노드를 누를 수 없다.")]
        [SerializeField, Min(0f)] private float _unrollNodeFadeSeconds = 0.15f;
        public float UnrollNodeFadeSeconds { get { return _unrollNodeFadeSeconds; } set { _unrollNodeFadeSeconds = value; } }

        [Tooltip("`말림_왼쪽` 의 그림이 차지하는 칸. x, y, 가로, 세로. 판 위 픽셀 실측이다.")]
        [SerializeField] private Rect _rolledLeftArt = new Rect(714f, 224f, 177f, 747f);
        public Rect RolledLeftArt { get { return _rolledLeftArt; } set { _rolledLeftArt = value; } }

        [Tooltip("`말림_오른쪽` 의 그림이 차지하는 칸. x, y, 가로, 세로. 판 위 픽셀 실측이다.")]
        [SerializeField] private Rect _rolledRightArt = new Rect(884f, 224f, 177f, 747f);
        public Rect RolledRightArt { get { return _rolledRightArt; } set { _rolledRightArt = value; } }

        [Tooltip("`펼침_왼쪽` 의 그림이 차지하는 칸. x, y, 가로, 세로. 판 위 픽셀 실측이다. " +
                 "가운데 x 가 종이(`펼침_가운데`) 왼쪽 끝과 맞아 종이를 드러내는 가림판의 왼쪽 끝도 이것을 따라간다.")]
        [SerializeField] private Rect _openLeftArt = new Rect(102f, 259f, 97f, 714f);
        public Rect OpenLeftArt { get { return _openLeftArt; } set { _openLeftArt = value; } }

        [Tooltip("`펼침_오른쪽` 의 그림이 차지하는 칸. x, y, 가로, 세로. 판 위 픽셀 실측이다. " +
                 "가운데 x 가 종이 오른쪽 끝과 맞는다.")]
        [SerializeField] private Rect _openRightArt = new Rect(1577f, 222f, 97f, 711f);
        public Rect OpenRightArt { get { return _openRightArt; } set { _openRightArt = value; } }

        /// <summary>
        /// 펼치는 곡선. 처음은 빠르고 끝은 느리다(3차 감속). t 는 0 ~ 1 이다.
        /// 효과음과 이펙트 목록 E05 두루마리 펼치기.
        /// </summary>
        public static float GetUnrollEase(float t)
        {
            float rest = 1f - Mathf.Clamp01(t);
            return 1f - rest * rest * rest;
        }

        /// <summary>
        /// 말린 두루마리 그림의 불투명도. fadeStart 까지는 1 이고 진행도 1 에서 0 이 된다.
        /// 효과음과 이펙트 목록 E05 두루마리 펼치기.
        /// </summary>
        public static float GetRolledAlpha(float progress, float fadeStart)
        {
            if (fadeStart >= 1f)
            {
                return progress >= 1f ? 0f : 1f;
            }

            return 1f - Mathf.Clamp01((progress - fadeStart) / (1f - fadeStart));
        }

        /// <summary>
        /// 그림 판 위의 픽셀 자리를 화면에 그린 양피지의 가운데 기준 자리로 바꾼다.
        /// 판은 왼쪽 위가 (0, 0) 이고 아래로 y 가 커지며, 화면은 가운데가 (0, 0) 이고 위로 y 가 커진다.
        /// </summary>
        public static Vector2 ArtToLocal(Vector2 artPoint, Vector2 artSize, Vector2 drawSize)
        {
            float scaleX = artSize.x > 0f ? drawSize.x / artSize.x : 1f;
            float scaleY = artSize.y > 0f ? drawSize.y / artSize.y : 1f;
            return new Vector2((artPoint.x - artSize.x * 0.5f) * scaleX, (artSize.y * 0.5f - artPoint.y) * scaleY);
        }

        [Header("크기")]
        [Tooltip("노드를 늘어놓는 영역의 크기. 화면 가운데에 놓는다.\n" +
                 "**기획서는 1600 x 800 인데 와이어프레임대로 1300 x 500 이다.** 2026년 10월 3일에 원재가 정했다. " +
                 "양피지의 종이 몸통이 화면 기준 가로 약 1270, 세로 약 510 이라 " +
                 "1600 x 800 은 말린 끝과 찢긴 가장자리 밖으로 넘친다. " +
                 "와이어프레임 `10 맵 화면` 의 `맵 그래프.png` 실측 (310, 290) 1300 x 500 이다.")]
        [FormerlySerializedAs("MapSize")]
        [SerializeField] private Vector2 _mapSize = UiScale.V(1300f, 500f);
        public Vector2 MapSize { get { return _mapSize; } set { _mapSize = value; } }

        [Tooltip("노드 칸 한 변의 크기. 기획서 기준 70.\n" +
                 "아이콘 그림은 칸을 다 채우지 않는다. 일반 방 모양이 64 가운데 50 x 47 이라 " +
                 "70 칸에서는 55 x 51 로 보인다. 와이어프레임의 일반 방 모양 57 x 48 과 거의 같다.")]
        [FormerlySerializedAs("NodeSize")]
        [SerializeField] private float _nodeSize = UiScale.Px(70f);
        public float NodeSize { get { return _nodeSize; } set { _nodeSize = value; } }

        [Tooltip("아이콘 모양이 노드 칸에서 실제로 차지하는 비율. 간선 시작 자리와 누르는 영역을 여기에 맞춘다.\n" +
                 "일반 방 아이콘이 64 가운데 50 을 차지해 0.78 이다. 방 대부분이 일반 방이다.")]
        [Range(0.5f, 1f)]
        [FormerlySerializedAs("IconShapeRatio")]
        [SerializeField] private float _iconShapeRatio = 0.78f;
        public float IconShapeRatio { get { return _iconShapeRatio; } set { _iconShapeRatio = value; } }

        [Tooltip("가장 넓은 아이콘 모양의 비율. 노드끼리 닿는지 볼 때 쓴다.\n" +
                 "이벤트 방 아이콘이 64 가운데 54 를 차지해 0.85 다.")]
        [Range(0.5f, 1f)]
        [FormerlySerializedAs("WidestIconShapeRatio")]
        [SerializeField] private float _widestIconShapeRatio = 0.85f;
        public float WidestIconShapeRatio { get { return _widestIconShapeRatio; } set { _widestIconShapeRatio = value; } }

        [Tooltip("간선을 아이콘 모양에서 몇 픽셀 떨어뜨려 그릴지.\n" +
                 "**기획서는 노드에서 10 인데 와이어프레임 실측으로 3 이다.** " +
                 "예전에는 칸 가장자리에서 10 을 띄워 눈에 보이는 틈이 18 가까이 됐다. " +
                 "지금은 아이콘 모양에서 잰다.")]
        [FormerlySerializedAs("EdgeGap")]
        [SerializeField] private float _edgeGap = UiScale.Px(3f);
        public float EdgeGap { get { return _edgeGap; } set { _edgeGap = value; } }

        [Tooltip("간선의 두께. **와이어프레임 실측 3 이다.** " +
                 "자리와 크기를 10 단위로 맞추는 규칙에서 뺐다. 10 으로 두면 와이어프레임보다 세 배 굵어 보인다.")]
        [FormerlySerializedAs("EdgeThickness")]
        [SerializeField] private float _edgeThickness = UiScale.Px(3f);
        public float EdgeThickness { get { return _edgeThickness; } set { _edgeThickness = value; } }

        [Tooltip("완료 체크 그림의 크기. 그림 원본 32 x 32 가운데 모양이 28 x 22 다.\n" +
                 "와이어프레임의 체크 모양이 약 31 x 25 라 36 으로 키워 31.5 x 24.8 로 맞췄다. " +
                 "간선 두께처럼 10 단위 규칙에서 뺐다.")]
        [FormerlySerializedAs("CheckMarkSize")]
        [SerializeField] private Vector2 _checkMarkSize = UiScale.V(36f, 36f);
        public Vector2 CheckMarkSize { get { return _checkMarkSize; } set { _checkMarkSize = value; } }

        [Tooltip("완료 체크를 노드 칸 오른쪽 위 모서리에서 얼마나 밀지. 오른쪽과 위가 양수다.\n" +
                 "와이어프레임은 체크가 아이콘 모양 오른쪽 끝에서 6, 위 끝에서 4 튀어나와 있다. " +
                 "70 칸에서는 아이콘 모양이 칸 모서리에서 약 9 안쪽이라 체크 그림 모서리를 칸 모서리에 맞추면 그 자리가 된다.")]
        [FormerlySerializedAs("CheckMarkOffset")]
        [SerializeField] private Vector2 _checkMarkOffset = Vector2.zero;
        public Vector2 CheckMarkOffset { get { return _checkMarkOffset; } set { _checkMarkOffset = value; } }

        [Header("방 타입 표시")]
        [Tooltip("노드 아이콘에 방 타입 색을 입힐지 여부. " +
                 "맵 화면 시안은 방 타입을 아이콘 모양으로 구분하고 색은 진행 상태에만 쓰므로 기본은 꺼 둔다. " +
                 "켜면 맵 생성 규칙 기획서 04 방 목록의 노드 표시 색을 아이콘에 입힌다.")]
        [FormerlySerializedAs("UseRoomTypeColor")]
        [SerializeField] private bool _useRoomTypeColor;
        public bool UseRoomTypeColor { get { return _useRoomTypeColor; } set { _useRoomTypeColor = value; } }

        [FormerlySerializedAs("RoomVisuals")]
        [SerializeField] private List<RoomTypeVisual> _roomVisuals = new List<RoomTypeVisual>
        {
            new RoomTypeVisual { RoomType = RoomType.Normal, Color = new Color(0.95f, 0.78f, 0.20f) },
            new RoomTypeVisual { RoomType = RoomType.Elite, Color = new Color(0.62f, 0.17f, 0.15f) },
            new RoomTypeVisual { RoomType = RoomType.Sanctum, Color = new Color(0.25f, 0.60f, 0.35f) },
            new RoomTypeVisual { RoomType = RoomType.Event, Color = new Color(0.55f, 0.53f, 0.50f) },
            new RoomTypeVisual { RoomType = RoomType.Etc, Color = new Color(0.70f, 0.68f, 0.65f) },
            new RoomTypeVisual { RoomType = RoomType.Boss, Color = new Color(0.12f, 0.11f, 0.09f) },
        };
        public List<RoomTypeVisual> RoomVisuals { get { return _roomVisuals; } set { _roomVisuals = value; } }

        [Header("노드 상태")]
        // 노드 아이콘은 흑백으로 그린 그림이다. 지도 위에서는 예전처럼 검은 실루엣으로 보이게
        // 와이어프레임의 어두운 갈색 #2B2016 을 곱한다. 방 종류 색은 입히지 않는다(2026년 10월 20일 원재).
        // 지나친 방만 옅은 회갈색으로 해 갈 수 없는 곳을 알린다.

        [Tooltip("완료한 방. 노드 우상단에 체크를 함께 표시한다. 와이어프레임의 #2B2016.")]
        [FormerlySerializedAs("ClearedTint")]
        [SerializeField] private Color _clearedTint = new Color(0.169f, 0.125f, 0.086f, 1f);
        public Color ClearedTint { get { return _clearedTint; } set { _clearedTint = value; } }

        [Tooltip("지나친 단계의 다른 방과 더는 닿을 수 없는 방. 와이어프레임 `10 맵 화면 · 지나간 경로` 의 #958E83.")]
        [FormerlySerializedAs("SkippedTint")]
        [SerializeField] private Color _skippedTint = new Color(0.584f, 0.557f, 0.514f, 1f);
        public Color SkippedTint { get { return _skippedTint; } set { _skippedTint = value; } }

        [Tooltip("이후 단계의 방. 와이어프레임의 #2B2016.")]
        [FormerlySerializedAs("FutureTint")]
        [SerializeField] private Color _futureTint = new Color(0.169f, 0.125f, 0.086f, 1f);
        public Color FutureTint { get { return _futureTint; } set { _futureTint = value; } }

        [Tooltip("지금 고를 수 있는 방. 와이어프레임은 다른 방과 같은 #2B2016 이고 크기로만 가른다.")]
        [FormerlySerializedAs("SelectableTint")]
        [SerializeField] private Color _selectableTint = new Color(0.169f, 0.125f, 0.086f, 1f);
        public Color SelectableTint { get { return _selectableTint; } set { _selectableTint = value; } }

        [Tooltip("지금 머물러 있는 방. 와이어프레임의 #2B2016.")]
        [FormerlySerializedAs("CurrentTint")]
        [SerializeField] private Color _currentTint = new Color(0.169f, 0.125f, 0.086f, 1f);
        public Color CurrentTint { get { return _currentTint; } set { _currentTint = value; } }

        [Header("연출")]
        [Tooltip("지금 방 노드가 밝아졌다 어두워지는 한 주기(초). 2026년 10월 31일 원재가 2초로 정했다.")]
        [SerializeField, Min(0f)] private float _currentPulseSeconds = 2f;
        public float CurrentPulseSeconds { get { return _currentPulseSeconds; } set { _currentPulseSeconds = value; } }

        [Tooltip("지금 방 노드가 가장 밝을 때 더하는 밝기. 0.3 이면 30 퍼센트 밝아진다(2026년 10월 31일 원재).")]
        [SerializeField, Range(0f, 1f)] private float _currentPulseBrightness = 0.3f;
        public float CurrentPulseBrightness { get { return _currentPulseBrightness; } set { _currentPulseBrightness = value; } }

        [Tooltip("노드를 고르면 그 노드가 반짝이는 시간(초). 반짝임이 끝나면 방으로 들어간다(2026년 10월 31일 원재).")]
        [SerializeField, Min(0f)] private float _selectFlashSeconds = 0.3f;
        public float SelectFlashSeconds { get { return _selectFlashSeconds; } set { _selectFlashSeconds = value; } }

        [Tooltip("반짝임이 가장 밝을 때의 색. 노드 색이 이 색으로 갔다가 돌아온다.")]
        [SerializeField] private Color _selectFlashColor = new Color(1f, 0.93f, 0.72f, 1f);
        public Color SelectFlashColor { get { return _selectFlashColor; } set { _selectFlashColor = value; } }

        /// <summary>
        /// 지금 방 노드에 곱하는 밝기. 0초에 1 로 시작해 주기의 절반에 1 + 밝기 로 가장 밝고 한 주기에 1 로 돌아온다.
        /// </summary>
        public static float GetPulseMultiplier(float elapsed, float period, float brightness)
        {
            if (period <= 0f)
            {
                return 1f;
            }

            float phase = (elapsed % period) / period;
            float wave = (1f - Mathf.Cos(phase * Mathf.PI * 2f)) * 0.5f;
            return 1f + brightness * wave;
        }

        /// <summary>반짝임의 세기. 0 에서 시작해 가운데에서 1 이 되고 끝에서 0 으로 돌아온다. t 는 0 ~ 1 이다.</summary>
        public static float GetFlashAmount(float t)
        {
            return Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
        }

        [Tooltip("아이콘 안쪽 무늬(눈, 선)의 색. 진한 갈색 몸통 위에서 잘 보이게 양피지 색으로 둔다(2026년 10월 25일 원재).")]
        [SerializeField] private Color _iconDetailColor = new Color(0.933f, 0.871f, 0.729f, 1f);
        public Color IconDetailColor { get { return _iconDetailColor; } set { _iconDetailColor = value; } }

        [Tooltip("완료 표시 체크의 색. 스프라이트가 이미 초록이면 흰색으로 두면 된다.")]
        [FormerlySerializedAs("CheckMarkColor")]
        [SerializeField] private Color _checkMarkColor = Color.white;
        public Color CheckMarkColor { get { return _checkMarkColor; } set { _checkMarkColor = value; } }

        [Tooltip("고를 수 있는 노드를 키우는 비율. 04 화면 공통 규칙의 카드 상태를 따라 5퍼센트를 기본으로 한다.")]
        [FormerlySerializedAs("SelectableScale")]
        [SerializeField] private float _selectableScale = 1.05f;
        public float SelectableScale { get { return _selectableScale; } set { _selectableScale = value; } }

        [Header("간선 상태")]
        [Tooltip("지나온 간선. 와이어프레임의 #2B2016, 불투명도 0.9.")]
        [FormerlySerializedAs("TraveledEdgeColor")]
        [SerializeField] private Color _traveledEdgeColor = new Color(0.169f, 0.125f, 0.086f, 0.9f);
        public Color TraveledEdgeColor { get { return _traveledEdgeColor; } set { _traveledEdgeColor = value; } }

        [Tooltip("아직 갈 수 있는 간선. 와이어프레임은 지나온 간선과 같은 색이다.")]
        [FormerlySerializedAs("OpenEdgeColor")]
        [SerializeField] private Color _openEdgeColor = new Color(0.169f, 0.125f, 0.086f, 0.9f);
        public Color OpenEdgeColor { get { return _openEdgeColor; } set { _openEdgeColor = value; } }

        [Tooltip("지나친 방에 닿는 간선. 더는 갈 수 없는 길이다. " +
                 "와이어프레임 `10 맵 화면 · 지나간 경로` 의 #968F84, 불투명도 0.78.")]
        [FormerlySerializedAs("ClosedEdgeColor")]
        [SerializeField] private Color _closedEdgeColor = new Color(0.588f, 0.561f, 0.518f, 0.78f);
        public Color ClosedEdgeColor { get { return _closedEdgeColor; } set { _closedEdgeColor = value; } }

        [Header("지도 잠깐 보기")]
        [Tooltip("방 진행 중에 지도 버튼으로 지도를 잠깐 띄울 때 지도 뒤에 까는 막의 색. " +
                 "원재가 2026년 10월 5일에 반투명 검은 막 위에 지도를 띄우는 방식으로 정했다. " +
                 "불투명도는 75퍼센트다. 유니티가 반투명을 선형 색 공간에서 섞어 60퍼센트로는 " +
                 "눈에 35퍼센트쯤으로만 어두워져 아래 방 화면이 거의 그대로 보였다.")]
        [FormerlySerializedAs("PeekDimColor")]
        [SerializeField] private Color _peekDimColor = new Color(0f, 0f, 0f, 0.75f);
        public Color PeekDimColor { get { return _peekDimColor; } set { _peekDimColor = value; } }

        [Header("화면 이동")]
        [Tooltip("지도가 화면보다 넓을 때 좌우 이동을 허용한다. 세로 이동과 확대 축소는 지원하지 않는다.")]
        [FormerlySerializedAs("AllowHorizontalPan")]
        [SerializeField] private bool _allowHorizontalPan = true;
        public bool AllowHorizontalPan { get { return _allowHorizontalPan; } set { _allowHorizontalPan = value; } }

        [Tooltip("휠 한 칸에 움직이는 거리.")]
        [FormerlySerializedAs("ScrollSpeed")]
        [SerializeField] private float _scrollSpeed = UiScale.Px(120f);
        public float ScrollSpeed { get { return _scrollSpeed; } set { _scrollSpeed = value; } }

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
        /// 누르는 영역끼리 어떤 지터에서도 겹치지 않는지.
        /// 누르는 영역은 노드 칸에서 아이콘 모양 비율만큼만 쓴다.
        ///
        /// 커서를 받는 것은 고를 수 있는 노드뿐이고 그 노드는 모두 같은 단계에 있다(`MapNodeView.Bind`).
        /// 그래서 같은 단계 위아래 줄 사이만 본다. 고를 수 있는 노드는 `SelectableScale` 만큼 커 있고,
        /// 커서를 올린 하나는 hoverRatio 만큼 더 커진다. 예전에는 커지기 전 크기로 재어 실제보다 작게 봤다
        /// (2026년 10월 22일 코덱스 검토).
        /// </summary>
        public bool ClickAreasNeverOverlap(int rowCount, float jitterY, float hoverRatio)
        {
            if (rowCount < 2)
            {
                return true;
            }

            float row = (MapSize.y - NodeSize) / (rowCount - 1);
            float closest = row * (1f - jitterY * 2f);
            float click = NodeSize * IconShapeRatio * SelectableScale;
            float hovered = click * hoverRatio;
            return closest > (click + hovered) * 0.5f;
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
