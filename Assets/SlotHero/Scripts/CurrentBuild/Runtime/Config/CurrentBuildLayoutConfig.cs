using UnityEngine;
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
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Tooltip("왼쪽 줄들이 시작되는 x. 와이어프레임 기준 60.")]
        public float LeftMargin = UiScale.Px(60f);

        [Tooltip("문양이 많아 스크롤할 때 마지막 줄 아래에 남기는 여백.")]
        [Min(0f)]
        public float ContentBottomPadding = UiScale.Px(60f);

        [Tooltip("칸 사이 간격. 와이어프레임 실측 16 을 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        public float SlotSpacing = UiScale.Px(20f);

        [Tooltip("줄 이름과 그 아래 첫 칸 사이의 거리. 와이어프레임 실측 44 를 10 단위로 옮긴 값이다.")]
        public float LabelToSlotGap = UiScale.Px(40f);

        [Header("1열 유물")]
        [Tooltip("유물 줄 이름의 y. 와이어프레임 실측 86 을 10 단위로 옮긴 값이다.")]
        public float RelicLabelY = UiScale.Px(90f);

        [Tooltip("유물 칸의 크기. 와이어프레임 실측 151 × 151 을 10 단위로 옮긴 값이다.")]
        public Vector2 RelicSlotSize = UiScale.V(150f, 150f);

        [Tooltip("유물 슬롯 수. 기획서 기준 6개.")]
        [Min(1)]
        public int RelicSlotCount = 6;

        [Header("2열 코인")]
        [Tooltip("코인 줄 이름의 y. 유물 칸 아래로 20 띄운 값이다. 와이어프레임 실측은 305 였다.")]
        public float CoinLabelY = UiScale.Px(300f);

        [Tooltip("유물 칸에 대한 코인 칸의 비율. 기획서 기준 10퍼센트 작게.")]
        [Range(0.1f, 1f)]
        public float CoinSlotScale = 0.9f;

        [Tooltip("코인 슬롯 수. 기획서 기준 최대 5개.")]
        [Min(1)]
        public int CoinSlotCount = 5;

        [Header("3열 이후 문양")]
        [Tooltip("문양 줄 이름의 y. 코인 칸 아래로 25 띄운 값이다. 와이어프레임 실측은 507 이었다.")]
        public float SymbolLabelY = UiScale.Px(500f);

        [Tooltip("문양 칸의 크기. 와이어프레임 실측 151 × 157 을 10 단위로 옮긴 값이다.")]
        public Vector2 SymbolSlotSize = UiScale.V(150f, 160f);

        [Header("글자 칸")]
        [Tooltip(
            "줄 이름과 칸 제목이 차지하는 칸의 크기.\n" +
            "이걸 안 잡으면 유니티가 넣어 주는 100 × 100 이 그대로 남는다. " +
            "글자가 그 칸 한가운데에 놓여 아래로 밀리고, 긴 이름은 한 자씩 줄바꿈된다.")]
        public Vector2 LabelSize = UiScale.V(300f, 50f);

        [Tooltip("문양을 한 줄에 몇 칸 놓을지. 기획서 기준 가로 8칸.")]
        [Min(1)]
        public int SymbolColumns = 8;

        [Tooltip("문양 유효 슬롯의 최대 수. 기획서 기준 36개.")]
        [Min(1)]
        public int SymbolSlotLimit = 36;

        [Tooltip("문양 줄 사이 간격. 와이어프레임 실측 16 을 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        public float SymbolRowSpacing = UiScale.Px(20f);

        [Tooltip("개수 배지의 크기. 와이어프레임 실측 62 × 30 을 10 단위로 옮긴 값이다.")]
        public Vector2 CountBadgeSize = UiScale.V(60f, 30f);

        [Tooltip("개수 배지를 칸의 오른쪽 아래에서 얼마나 안쪽에 둘지. 와이어프레임 실측 8 을 옮겼다.")]
        public float CountBadgeInset = UiScale.Px(10f);

        [Header("정렬")]
        [Tooltip("정렬 칸의 자리. 와이어프레임 실측 x 1080 · y 383 을 10 단위로 옮긴 값이다.")]
        public Vector2 SortPosition = UiScale.V(1080f, 380f);

        [Tooltip("정렬 칸의 크기. 와이어프레임 실측 300 × 64 를 10 단위로 옮긴 값이다.")]
        public Vector2 SortSize = UiScale.V(300f, 60f);

        [Header("오른쪽 칸")]
        [Tooltip("태그 비중 칸의 자리. 와이어프레임 실측 x 1440 · y 88 을 10 단위로 옮긴 값이다.")]
        public Vector2 TagPanelPosition = UiScale.V(1440f, 90f);

        [Tooltip("태그 비중 칸의 크기. 와이어프레임 실측 420 × 828 을 10 단위로 옮긴 값이다.")]
        public Vector2 TagPanelSize = UiScale.V(420f, 830f);

        [Tooltip("칸 안쪽 여백. 와이어프레임 실측 24 를 10 단위로 옮긴 값이다.")]
        public float PanelPadding = UiScale.Px(20f);

        [Tooltip("칸 제목 `문양 24장` 과 `태그 비중` 사이의 세로 간격.")]
        public float PanelTitleGap = UiScale.Px(50f);

        [Tooltip("첫 태그 막대의 y. 와이어프레임 실측 244 를 10 단위로 옮긴 값이다.")]
        public float FirstTagBarY = UiScale.Px(240f);

        [Tooltip("태그 한 줄이 차지하는 높이. 와이어프레임 실측 76 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        public float TagRowHeight = UiScale.Px(80f);

        [Tooltip("태그 막대의 크기. 와이어프레임 실측 300 × 16 을 10 단위로 옮긴 값이다.")]
        public Vector2 TagBarSize = UiScale.V(300f, 20f);

        [Tooltip("태그 막대가 시작되는 x. 와이어프레임 실측 1536 을 10 단위로 옮긴 값이다.")]
        public float TagBarX = UiScale.Px(1540f);

        [Tooltip("태그 기호가 놓이는 x. 태그 칸 왼쪽에서 안쪽 여백만큼 들어온 자리다.")]
        public float TagSymbolX = UiScale.Px(1460f);

        [Header("닫기")]
        [Tooltip("닫기 칸의 자리. 와이어프레임 실측 x 1440 · y 956 을 10 단위로 옮긴 값이다.")]
        public Vector2 ClosePosition = UiScale.V(1440f, 960f);

        [Tooltip("닫기 칸의 크기. 와이어프레임 실측 420 × 64 를 10 단위로 옮긴 값이다.")]
        public Vector2 CloseSize = UiScale.V(420f, 60f);

        [Header("고르기 화면")]
        [Tooltip("고르기 화면의 제목 y. 유물 줄 이름과 같은 자리다.\n" +
                 "이벤트와 성소에서 가진 것 중 하나를 고를 때 현재 빌드 화면과 비슷한 화면을 띄운다. 2026년 10월 5일 원재가 정했다.")]
        public float PickTitleY = UiScale.Px(90f);

        [Tooltip("문양을 고를 때 정렬 칸의 자리. 제목 줄 오른쪽 끝에 둔다.")]
        public Vector2 PickSortPosition = UiScale.V(1080f, 80f);

        [Tooltip("고른 것과 그 결과를 적는 아래 줄의 자리. 왼쪽 칸들 아래다.")]
        public Vector2 PickResultPosition = UiScale.V(60f, 960f);

        [Tooltip("아래 줄의 크기. 오른쪽 칸 앞까지 왼쪽 여백과 같은 60 을 남긴다.")]
        public Vector2 PickResultSize = UiScale.V(1320f, 60f);

        [Tooltip("확인 버튼의 자리. 닫기 칸 자리의 왼쪽 반이다.")]
        public Vector2 PickConfirmPosition = UiScale.V(1440f, 960f);

        [Tooltip("확인 버튼의 크기.")]
        public Vector2 PickConfirmSize = UiScale.V(200f, 60f);

        [Tooltip("취소 버튼의 자리. 닫기 칸 자리의 오른쪽 반이다.")]
        public Vector2 PickCancelPosition = UiScale.V(1660f, 960f);

        [Tooltip("취소 버튼의 크기.")]
        public Vector2 PickCancelSize = UiScale.V(200f, 60f);

        [Tooltip("칸이 많아 스크롤할 때 칸이 아래 줄에 가리지 않도록 스크롤 창을 여기서 끊는다.")]
        public float PickViewportBottom = UiScale.Px(940f);

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
            SlotSpacing = Mathf.Max(0f, SlotSpacing);
            RelicSlotSize.x = Mathf.Max(1f, RelicSlotSize.x);
            RelicSlotSize.y = Mathf.Max(1f, RelicSlotSize.y);
            SymbolSlotSize.x = Mathf.Max(1f, SymbolSlotSize.x);
            SymbolSlotSize.y = Mathf.Max(1f, SymbolSlotSize.y);
            RelicSlotCount = Mathf.Max(1, RelicSlotCount);
            CoinSlotCount = Mathf.Max(1, CoinSlotCount);
            SymbolColumns = Mathf.Max(1, SymbolColumns);
            SymbolSlotLimit = Mathf.Max(1, SymbolSlotLimit);
            TagRowHeight = Mathf.Max(1f, TagRowHeight);
            ContentBottomPadding = Mathf.Max(0f, ContentBottomPadding);
        }
    }
}
