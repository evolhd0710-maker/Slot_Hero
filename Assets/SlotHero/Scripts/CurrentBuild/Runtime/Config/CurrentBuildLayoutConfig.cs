using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;
using SlotHero.Combat;

namespace SlotHero.CurrentBuild
{
    /// <summary>
    /// 현재 빌드 화면의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 과 그 와이어프레임에서 잰 값이다.
    ///
    /// 기획서가 정한 것은 유물 슬롯 6개, 코인 최대 5개이며 유물보다 10퍼센트 작게,
    /// 문양은 가로 8칸에 유효 슬롯 최대 36개, 정렬은 코인 줄 오른쪽, 닫기는 오른쪽 아래다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CurrentBuildLayoutConfig",
        menuName = "Slot Hero/현재 빌드/현재 빌드 화면 자리 설정",
        order = 0)]
    public class CurrentBuildLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Tooltip("왼쪽 줄들이 시작되는 x. 와이어프레임 기준 60.")]
        [FormerlySerializedAs("LeftMargin")]
        [SerializeField] private float _leftMargin = UiScale.Px(60f);
        public float LeftMargin { get { return _leftMargin; } set { _leftMargin = value; } }

        [Tooltip("문양이 많아 스크롤할 때 마지막 줄 아래에 남기는 여백.")]
        [Min(0f)]
        [FormerlySerializedAs("ContentBottomPadding")]
        [SerializeField] private float _contentBottomPadding = UiScale.Px(60f);
        public float ContentBottomPadding { get { return _contentBottomPadding; } set { _contentBottomPadding = value; } }

        [Tooltip("칸 사이 간격. 와이어프레임 실측 16 을 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("SlotSpacing")]
        [SerializeField] private float _slotSpacing = UiScale.Px(20f);
        public float SlotSpacing { get { return _slotSpacing; } set { _slotSpacing = value; } }

        [Tooltip("줄 이름과 그 아래 첫 칸 사이의 거리. 와이어프레임 실측 44 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("LabelToSlotGap")]
        [SerializeField] private float _labelToSlotGap = UiScale.Px(40f);
        public float LabelToSlotGap { get { return _labelToSlotGap; } set { _labelToSlotGap = value; } }

        [Header("1열 유물")]
        [Tooltip("유물 줄 이름의 y. 와이어프레임 실측 86 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("RelicLabelY")]
        [SerializeField] private float _relicLabelY = UiScale.Px(90f);
        public float RelicLabelY { get { return _relicLabelY; } set { _relicLabelY = value; } }

        [Tooltip("유물 칸의 크기. 와이어프레임 실측 151 × 151 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("RelicSlotSize")]
        [SerializeField] private Vector2 _relicSlotSize = UiScale.V(150f, 150f);
        public Vector2 RelicSlotSize { get { return _relicSlotSize; } set { _relicSlotSize = value; } }

        [Tooltip("유물 슬롯 수. 기획서 기준 6개.")]
        [Min(1)]
        [FormerlySerializedAs("RelicSlotCount")]
        [SerializeField] private int _relicSlotCount = 6;
        public int RelicSlotCount { get { return _relicSlotCount; } set { _relicSlotCount = value; } }

        [Header("2열 코인")]
        [Tooltip("코인 줄 이름의 y. 유물 칸 아래로 20 띄운 값이다. 와이어프레임 실측은 305 였다.")]
        [FormerlySerializedAs("CoinLabelY")]
        [SerializeField] private float _coinLabelY = UiScale.Px(300f);
        public float CoinLabelY { get { return _coinLabelY; } set { _coinLabelY = value; } }

        [Tooltip("유물 칸에 대한 코인 칸의 비율. 기획서 기준 10퍼센트 작게.")]
        [Range(0.1f, 1f)]
        [FormerlySerializedAs("CoinSlotScale")]
        [SerializeField] private float _coinSlotScale = 0.9f;
        public float CoinSlotScale { get { return _coinSlotScale; } set { _coinSlotScale = value; } }

        [Tooltip("코인 슬롯 수. 기획서 기준 최대 5개.")]
        [Min(1)]
        [FormerlySerializedAs("CoinSlotCount")]
        [SerializeField] private int _coinSlotCount = 5;
        public int CoinSlotCount { get { return _coinSlotCount; } set { _coinSlotCount = value; } }

        [Header("3열 이후 문양")]
        [Tooltip("문양 줄 이름의 y. 코인 칸 아래로 25 띄운 값이다. 와이어프레임 실측은 507 이었다.")]
        [FormerlySerializedAs("SymbolLabelY")]
        [SerializeField] private float _symbolLabelY = UiScale.Px(500f);
        public float SymbolLabelY { get { return _symbolLabelY; } set { _symbolLabelY = value; } }

        [Tooltip("문양 칸의 크기. 와이어프레임 실측은 151 × 157 이었다. " +
            "흑요석 판과 문양, 태그 테두리가 모두 정사각이라 칸도 정사각 150 × 150 으로 맞췄다(2026년 10월 21일 원재).")]
        [FormerlySerializedAs("SymbolSlotSize")]
        [SerializeField] private Vector2 _symbolSlotSize = UiScale.V(150f, 150f);
        public Vector2 SymbolSlotSize { get { return _symbolSlotSize; } set { _symbolSlotSize = value; } }

        [Header("글자 칸")]
        [Tooltip(
            "줄 이름과 칸 제목이 차지하는 칸의 크기.\n" +
            "이걸 안 잡으면 유니티가 넣어 주는 100 × 100 이 그대로 남는다. " +
            "글자가 그 칸 한가운데에 놓여 아래로 밀리고, 긴 이름은 한 자씩 줄바꿈된다.")]
        [FormerlySerializedAs("LabelSize")]
        [SerializeField] private Vector2 _labelSize = UiScale.V(300f, 50f);
        public Vector2 LabelSize { get { return _labelSize; } set { _labelSize = value; } }

        [Tooltip("문양을 한 줄에 몇 칸 놓을지. 기획서 기준 가로 8칸.")]
        [Min(1)]
        [FormerlySerializedAs("SymbolColumns")]
        [SerializeField] private int _symbolColumns = 8;
        public int SymbolColumns { get { return _symbolColumns; } set { _symbolColumns = value; } }

        [Tooltip("문양 유효 슬롯의 최대 수. 기획서 기준 36개.")]
        [Min(1)]
        [FormerlySerializedAs("SymbolSlotLimit")]
        [SerializeField] private int _symbolSlotLimit = 36;
        public int SymbolSlotLimit { get { return _symbolSlotLimit; } set { _symbolSlotLimit = value; } }

        [Tooltip("문양 줄 사이 간격. 와이어프레임 실측 16 을 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("SymbolRowSpacing")]
        [SerializeField] private float _symbolRowSpacing = UiScale.Px(20f);
        public float SymbolRowSpacing { get { return _symbolRowSpacing; } set { _symbolRowSpacing = value; } }

        [Tooltip("개수 배지의 크기. 와이어프레임 실측 62 × 30 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("CountBadgeSize")]
        [SerializeField] private Vector2 _countBadgeSize = UiScale.V(60f, 30f);
        public Vector2 CountBadgeSize { get { return _countBadgeSize; } set { _countBadgeSize = value; } }

        [Tooltip("개수 배지를 칸의 오른쪽 아래에서 얼마나 안쪽에 둘지. 와이어프레임 실측 8 을 옮겼다.")]
        [FormerlySerializedAs("CountBadgeInset")]
        [SerializeField] private float _countBadgeInset = UiScale.Px(10f);
        public float CountBadgeInset { get { return _countBadgeInset; } set { _countBadgeInset = value; } }

        [Header("정렬")]
        [Tooltip("정렬 칸의 자리. 와이어프레임 실측 x 1080 · y 383 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("SortPosition")]
        [SerializeField] private Vector2 _sortPosition = UiScale.V(1080f, 380f);
        public Vector2 SortPosition { get { return _sortPosition; } set { _sortPosition = value; } }

        [Tooltip("정렬 칸의 크기. 와이어프레임 실측 300 × 64 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("SortSize")]
        [SerializeField] private Vector2 _sortSize = UiScale.V(300f, 60f);
        public Vector2 SortSize { get { return _sortSize; } set { _sortSize = value; } }

        [Header("오른쪽 칸")]
        [Tooltip("태그 비중 칸의 자리. 와이어프레임 실측 x 1440 · y 88 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TagPanelPosition")]
        [SerializeField] private Vector2 _tagPanelPosition = UiScale.V(1440f, 90f);
        public Vector2 TagPanelPosition { get { return _tagPanelPosition; } set { _tagPanelPosition = value; } }

        [Tooltip("태그 비중 칸의 크기. 와이어프레임 실측 420 × 828 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TagPanelSize")]
        [SerializeField] private Vector2 _tagPanelSize = UiScale.V(420f, 830f);
        public Vector2 TagPanelSize { get { return _tagPanelSize; } set { _tagPanelSize = value; } }

        [Tooltip("칸 안쪽 여백. 와이어프레임 실측 24 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("PanelPadding")]
        [SerializeField] private float _panelPadding = UiScale.Px(20f);
        public float PanelPadding { get { return _panelPadding; } set { _panelPadding = value; } }

        [Tooltip("칸 제목 `문양 24장` 과 `태그 비중` 사이의 세로 간격.")]
        [FormerlySerializedAs("PanelTitleGap")]
        [SerializeField] private float _panelTitleGap = UiScale.Px(50f);
        public float PanelTitleGap { get { return _panelTitleGap; } set { _panelTitleGap = value; } }

        [Tooltip("첫 태그 막대의 y. 와이어프레임 실측 244 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("FirstTagBarY")]
        [SerializeField] private float _firstTagBarY = UiScale.Px(240f);
        public float FirstTagBarY { get { return _firstTagBarY; } set { _firstTagBarY = value; } }

        [Tooltip("태그 한 줄이 차지하는 높이. 와이어프레임 실측 76 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        [FormerlySerializedAs("TagRowHeight")]
        [SerializeField] private float _tagRowHeight = UiScale.Px(80f);
        public float TagRowHeight { get { return _tagRowHeight; } set { _tagRowHeight = value; } }

        [Tooltip("태그 막대의 크기. 와이어프레임 실측 300 × 16 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TagBarSize")]
        [SerializeField] private Vector2 _tagBarSize = UiScale.V(300f, 20f);
        public Vector2 TagBarSize { get { return _tagBarSize; } set { _tagBarSize = value; } }

        [Tooltip("태그 막대가 시작되는 x. 와이어프레임 실측 1536 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TagBarX")]
        [SerializeField] private float _tagBarX = UiScale.Px(1540f);
        public float TagBarX { get { return _tagBarX; } set { _tagBarX = value; } }

        [Tooltip("태그 기호가 놓이는 x. 태그 칸 왼쪽에서 안쪽 여백만큼 들어온 자리다.")]
        [FormerlySerializedAs("TagSymbolX")]
        [SerializeField] private float _tagSymbolX = UiScale.Px(1460f);
        public float TagSymbolX { get { return _tagSymbolX; } set { _tagSymbolX = value; } }

        [Header("닫기")]
        [Tooltip("닫기 칸의 자리. 와이어프레임 실측 x 1440 · y 956 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("ClosePosition")]
        [SerializeField] private Vector2 _closePosition = UiScale.V(1440f, 960f);
        public Vector2 ClosePosition { get { return _closePosition; } set { _closePosition = value; } }

        [Tooltip("닫기 칸의 크기. 와이어프레임 실측 420 × 64 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("CloseSize")]
        [SerializeField] private Vector2 _closeSize = UiScale.V(420f, 60f);
        public Vector2 CloseSize { get { return _closeSize; } set { _closeSize = value; } }

        [Header("고르기 화면")]
        [Tooltip("고르기 화면의 제목 y. 유물 줄 이름과 같은 자리다.\n" +
                 "이벤트와 성소에서 가진 것 중 하나를 고를 때 현재 빌드 화면과 비슷한 화면을 띄운다. 2026년 10월 5일 원재가 정했다.\n" +
                 "정렬 칸이 첫 줄 문양 테두리를 10 덮어 100 으로 내렸다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("PickTitleY")]
        [SerializeField] private float _pickTitleY = UiScale.Px(100f);
        public float PickTitleY { get { return _pickTitleY; } set { _pickTitleY = value; } }

        [Tooltip("문양을 고를 때 정렬 칸의 자리. 제목 줄 오른쪽 끝에 둔다. " +
                 "위는 상단 표시줄 장식 띠(64 + 12) 바로 아래, 아래는 첫 줄 문양보다 위다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("PickSortPosition")]
        [SerializeField] private Vector2 _pickSortPosition = UiScale.V(1080f, 76f);
        public Vector2 PickSortPosition { get { return _pickSortPosition; } set { _pickSortPosition = value; } }

        [Tooltip("고른 것과 그 결과를 적는 아래 줄의 자리. 왼쪽 칸들 아래다.")]
        [FormerlySerializedAs("PickResultPosition")]
        [SerializeField] private Vector2 _pickResultPosition = UiScale.V(60f, 960f);
        public Vector2 PickResultPosition { get { return _pickResultPosition; } set { _pickResultPosition = value; } }

        [Tooltip("아래 줄의 크기. 오른쪽 칸 앞까지 왼쪽 여백과 같은 60 을 남긴다.")]
        [FormerlySerializedAs("PickResultSize")]
        [SerializeField] private Vector2 _pickResultSize = UiScale.V(1320f, 60f);
        public Vector2 PickResultSize { get { return _pickResultSize; } set { _pickResultSize = value; } }

        [Tooltip("확인 버튼의 자리. 닫기 칸 자리의 왼쪽 반이다.")]
        [FormerlySerializedAs("PickConfirmPosition")]
        [SerializeField] private Vector2 _pickConfirmPosition = UiScale.V(1440f, 960f);
        public Vector2 PickConfirmPosition { get { return _pickConfirmPosition; } set { _pickConfirmPosition = value; } }

        [Tooltip("확인 버튼의 크기.")]
        [FormerlySerializedAs("PickConfirmSize")]
        [SerializeField] private Vector2 _pickConfirmSize = UiScale.V(200f, 60f);
        public Vector2 PickConfirmSize { get { return _pickConfirmSize; } set { _pickConfirmSize = value; } }

        [Tooltip("취소 버튼의 자리. 닫기 칸 자리의 오른쪽 반이다.")]
        [FormerlySerializedAs("PickCancelPosition")]
        [SerializeField] private Vector2 _pickCancelPosition = UiScale.V(1660f, 960f);
        public Vector2 PickCancelPosition { get { return _pickCancelPosition; } set { _pickCancelPosition = value; } }

        [Tooltip("취소 버튼의 크기.")]
        [FormerlySerializedAs("PickCancelSize")]
        [SerializeField] private Vector2 _pickCancelSize = UiScale.V(200f, 60f);
        public Vector2 PickCancelSize { get { return _pickCancelSize; } set { _pickCancelSize = value; } }

        [Tooltip("칸이 많아 스크롤할 때 칸이 아래 줄에 가리지 않도록 스크롤 창을 여기서 끊는다.")]
        [FormerlySerializedAs("PickViewportBottom")]
        [SerializeField] private float _pickViewportBottom = UiScale.Px(940f);
        public float PickViewportBottom { get { return _pickViewportBottom; } set { _pickViewportBottom = value; } }

        /// <summary>고르기 화면에서 칸 하나의 자리. 제목 아래에서 시작해 가로 칸 수를 넘기면 아래 줄로 내려간다.</summary>
        public Vector2 GetPickSlotPosition(int index, Vector2 slotSize)
        {
            int column = index % SymbolColumns;
            int row = index / SymbolColumns;

            float x = LeftMargin + column * (slotSize.x + SlotSpacing);
            float y = PickTitleY + LabelToSlotGap + row * (slotSize.y + SymbolRowSpacing);
            return new Vector2(x, y);
        }

        /// <summary>고르기 화면 칸들의 높이. 스크롤 내용의 높이가 된다.</summary>
        public float GetPickContentHeight(int count, Vector2 slotSize)
        {
            int rows = count <= 0 ? 0 : Mathf.CeilToInt((float)count / SymbolColumns);
            float bottom = rows <= 0
                ? PickTitleY + LabelToSlotGap
                : PickTitleY + LabelToSlotGap + (rows - 1) * (slotSize.y + SymbolRowSpacing) + slotSize.y;
            return bottom + ContentBottomPadding;
        }

        /// <summary>코인 칸의 크기. 유물 칸에서 비율을 곱한 값이다.</summary>
        public Vector2 GetCoinSlotSize()
        {
            return new Vector2(
                Mathf.Round(RelicSlotSize.x * CoinSlotScale),
                Mathf.Round(RelicSlotSize.y * CoinSlotScale));
        }

        /// <summary>유물 칸 하나의 자리. 화면 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetRelicSlotPosition(int index)
        {
            float x = LeftMargin + index * (RelicSlotSize.x + SlotSpacing);
            return new Vector2(x, RelicLabelY + LabelToSlotGap);
        }

        /// <summary>코인 칸 하나의 자리.</summary>
        public Vector2 GetCoinSlotPosition(int index)
        {
            float width = GetCoinSlotSize().x;
            float x = LeftMargin + index * (width + SlotSpacing);
            return new Vector2(x, CoinLabelY + LabelToSlotGap);
        }

        /// <summary>문양 칸 하나의 자리. 가로 칸 수를 넘기면 아래 줄로 내려간다.</summary>
        public Vector2 GetSymbolSlotPosition(int index)
        {
            int column = index % SymbolColumns;
            int row = index / SymbolColumns;

            float x = LeftMargin + column * (SymbolSlotSize.x + SlotSpacing);
            float y = SymbolLabelY + LabelToSlotGap + row * (SymbolSlotSize.y + SymbolRowSpacing);
            return new Vector2(x, y);
        }

        /// <summary>태그 막대 하나의 자리.</summary>
        public Vector2 GetTagBarPosition(int index)
        {
            return new Vector2(TagBarX, FirstTagBarY + index * TagRowHeight);
        }

        /// <summary>태그 막대의 길이. 비율이 0이면 0이 되어 막대가 비워진다.</summary>
        public float GetTagBarWidth(float ratio)
        {
            if (ratio <= 0f)
            {
                return 0f;
            }

            return TagBarSize.x * (ratio > 1f ? 1f : ratio);
        }

        /// <summary>유효 슬롯을 다 쓰면 문양이 몇 줄이 되는지.</summary>
        public int GetSymbolRowCount()
        {
            return Mathf.CeilToInt((float)SymbolSlotLimit / SymbolColumns);
        }

        /// <summary>문양 첫 줄의 y.</summary>
        public float GetSymbolTop()
        {
            return SymbolLabelY + LabelToSlotGap;
        }

        /// <summary>화면 아래로 넘지 않고 놓을 수 있는 문양 줄 수.</summary>
        public int GetSymbolRowsInScreen()
        {
            float step = SymbolSlotSize.y + SymbolRowSpacing;
            if (step <= 0f)
            {
                return 0;
            }

            float available = ReferenceResolution.y - GetSymbolTop();
            if (available <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt((available + SymbolRowSpacing) / step);
        }

        /// <summary>
        /// 유효 슬롯을 다 채워도 화면 안에 들어가는지.
        /// 와이어프레임의 칸 크기로는 들어가지 않아 그만큼 있을 때는 스크롤로 본다.
        /// </summary>
        public bool SymbolSlotsFitScreen()
        {
            return GetSymbolRowCount() <= GetSymbolRowsInScreen();
        }

        /// <summary>문양 종류 수에 따라 몇 줄이 되는지. 가진 종류만큼만 칸을 만든다.</summary>
        public int GetSymbolRowCountFor(int symbolCount)
        {
            if (symbolCount <= 0)
            {
                return 0;
            }

            int capped = symbolCount < SymbolSlotLimit ? symbolCount : SymbolSlotLimit;
            return Mathf.CeilToInt((float)capped / SymbolColumns);
        }

        /// <summary>
        /// 스크롤이 훑는 창의 크기.
        /// 세로는 화면 전체, 가로는 오른쪽 칸 앞까지다.
        /// 창을 화면 왼쪽 위에 맞춰 두면 칸 자리를 기획서 좌표 그대로 쓸 수 있다.
        /// </summary>
        public Vector2 GetScrollViewportSize()
        {
            return new Vector2(TagPanelPosition.x, ReferenceResolution.y);
        }

        /// <summary>
        /// 스크롤 안에 담기는 내용의 높이. 문양 줄 수에 따라 늘어난다.
        /// 마지막 줄이 화면 안에 다 보이는데 아래 여백 때문에만 넘치는 경우에는
        /// 여백을 버리고 화면 높이에 맞춘다. 볼 것이 없는데 스크롤이 생기지 않게 하기 위해서다.
        /// </summary>
        public float GetContentHeight(int symbolCount)
        {
            int rows = GetSymbolRowCountFor(symbolCount);

            float bottom = rows <= 0
                ? GetSymbolTop()
                : GetSymbolTop() + (rows - 1) * (SymbolSlotSize.y + SymbolRowSpacing) + SymbolSlotSize.y;

            float height = bottom + ContentBottomPadding;

            if (bottom <= ReferenceResolution.y && height > ReferenceResolution.y)
            {
                return ReferenceResolution.y;
            }

            return height;
        }

        /// <summary>그만큼의 문양을 보여 주려면 스크롤이 필요한지.</summary>
        public bool NeedsScroll(int symbolCount)
        {
            return GetContentHeight(symbolCount) > ReferenceResolution.y;
        }

        /// <summary>태그 9종이 오른쪽 칸 안에 들어가는지.</summary>
        public bool TagRowsFitPanel()
        {
            float lastBarBottom = FirstTagBarY + (SymbolTags.Count - 1) * TagRowHeight + TagBarSize.y;
            return lastBarBottom <= TagPanelPosition.y + TagPanelSize.y;
        }

        private void OnValidate()
        {
            _slotSpacing = Mathf.Max(0f, _slotSpacing);
            _relicSlotSize.x = Mathf.Max(1f, _relicSlotSize.x);
            _relicSlotSize.y = Mathf.Max(1f, _relicSlotSize.y);
            _symbolSlotSize.x = Mathf.Max(1f, _symbolSlotSize.x);
            _symbolSlotSize.y = Mathf.Max(1f, _symbolSlotSize.y);
            _relicSlotCount = Mathf.Max(1, _relicSlotCount);
            _coinSlotCount = Mathf.Max(1, _coinSlotCount);
            _symbolColumns = Mathf.Max(1, _symbolColumns);
            _symbolSlotLimit = Mathf.Max(1, _symbolSlotLimit);
            _tagRowHeight = Mathf.Max(1f, _tagRowHeight);
            _contentBottomPadding = Mathf.Max(0f, _contentBottomPadding);
        }
    }
}
