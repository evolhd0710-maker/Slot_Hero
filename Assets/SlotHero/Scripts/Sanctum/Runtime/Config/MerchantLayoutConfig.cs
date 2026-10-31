using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상 화면의 자리와 크기.
    ///
    /// 성소 기획서는 자리를 정하지 않고 "1920 × 1080 고정이라 프리팹에서 맞춘다"로 넘겼다.
    /// 그래서 값은 전부 `행상 예시.psd` 의 레이어 경계에서 잰 것이다.
    /// 괄호 안이 PSD 실측값이고, 화면 값은 10 단위로 맞췄다.
    ///
    /// 진열은 왼쪽 돗자리에 문양 넷과 코인 둘, 오른쪽 양탄자에 문양 변경과 유물 셋이다.
    /// 자리 수는 `SanctumConfig` 가 쥐고 있고 여기는 그 자리를 어디에 놓을지만 정한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MerchantLayoutConfig",
        menuName = "Slot Hero/성소/행상 화면 자리 설정",
        order = 4)]
    public class MerchantLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("깔개")]
        [Tooltip("문양과 코인을 올려 둔 돗자리의 왼쪽 위. (197, 108)")]
        [FormerlySerializedAs("LeftMatPosition")]
        [SerializeField] private Vector2 _leftMatPosition = UiScale.V(200f, 110f);
        public Vector2 LeftMatPosition { get { return _leftMatPosition; } set { _leftMatPosition = value; } }

        [Tooltip("돗자리 크기. (706 × 793)")]
        [FormerlySerializedAs("LeftMatSize")]
        [SerializeField] private Vector2 _leftMatSize = UiScale.V(710f, 790f);
        public Vector2 LeftMatSize { get { return _leftMatSize; } set { _leftMatSize = value; } }

        [Tooltip("문양 변경과 유물을 올려 둔 양탄자의 왼쪽 위. (1020, 117)")]
        [FormerlySerializedAs("RightMatPosition")]
        [SerializeField] private Vector2 _rightMatPosition = UiScale.V(1020f, 120f);
        public Vector2 RightMatPosition { get { return _rightMatPosition; } set { _rightMatPosition = value; } }

        [Tooltip("양탄자 크기. (694 × 774)")]
        [FormerlySerializedAs("RightMatSize")]
        [SerializeField] private Vector2 _rightMatSize = UiScale.V(690f, 770f);
        public Vector2 RightMatSize { get { return _rightMatSize; } set { _rightMatSize = value; } }

        [Header("문양 자리 · 2 × 2")]
        [Tooltip("첫 문양 자리의 왼쪽 위. (353, 207)")]
        [FormerlySerializedAs("SymbolFirstPosition")]
        [SerializeField] private Vector2 _symbolFirstPosition = UiScale.V(350f, 210f);
        public Vector2 SymbolFirstPosition { get { return _symbolFirstPosition; } set { _symbolFirstPosition = value; } }

        [Tooltip("문양 자리 하나의 크기. 예전 바닥 그림은 169 × 176 이었다. " +
            "새 흑요석 판과 문양, 태그 테두리가 모두 정사각이라 170 × 170 으로 맞췄다(2026년 10월 21일 원재).")]
        [FormerlySerializedAs("SymbolSlotSize")]
        [SerializeField] private Vector2 _symbolSlotSize = UiScale.V(170f, 170f);
        public Vector2 SymbolSlotSize { get { return _symbolSlotSize; } set { _symbolSlotSize = value; } }

        [Tooltip("문양 자리 사이의 가로와 세로 간격. (실측 213, 209) 가격 줄이 문양 칸에 걸쳐 2026년 10월 25일 원재가 옮기라고 해 줄 사이를 230 으로 넓혔다.")]
        [FormerlySerializedAs("SymbolSpacing")]
        [SerializeField] private Vector2 _symbolSpacing = UiScale.V(210f, 230f);
        public Vector2 SymbolSpacing { get { return _symbolSpacing; } set { _symbolSpacing = value; } }

        [Tooltip("한 줄에 놓는 문양 수.")]
        [Min(1)]
        [FormerlySerializedAs("SymbolColumns")]
        [SerializeField] private int _symbolColumns = 2;
        public int SymbolColumns { get { return _symbolColumns; } set { _symbolColumns = value; } }

        [Header("코인 자리 · 가로")]
        [Tooltip("첫 코인 자리의 왼쪽 위. (364, 634) 문양 둘째 줄과 그 가격이 내려가 670 으로 옮겼다(2026년 10월 25일).")]
        [FormerlySerializedAs("CoinFirstPosition")]
        [SerializeField] private Vector2 _coinFirstPosition = UiScale.V(360f, 670f);
        public Vector2 CoinFirstPosition { get { return _coinFirstPosition; } set { _coinFirstPosition = value; } }

        [Tooltip("코인 자리 하나의 크기. 받침 그림 크기다. (145 × 145)")]
        [FormerlySerializedAs("CoinSlotSize")]
        [SerializeField] private Vector2 _coinSlotSize = UiScale.V(150f, 150f);
        public Vector2 CoinSlotSize { get { return _coinSlotSize; } set { _coinSlotSize = value; } }

        [Tooltip("코인 자리 사이의 가로 간격. (실측 195)")]
        [FormerlySerializedAs("CoinSpacing")]
        [SerializeField] private float _coinSpacing = UiScale.Px(200f);
        public float CoinSpacing { get { return _coinSpacing; } set { _coinSpacing = value; } }

        [Header("유물 자리 · 가로")]
        [Tooltip("첫 유물 자리의 왼쪽 위. (1172, 579) 육각 받침을 세로로 세우며 받침 가운데 곧은 변 사이로 옮겼다(2026년 10월 25일 원재).")]
        [FormerlySerializedAs("RelicFirstPosition")]
        [SerializeField] private Vector2 _relicFirstPosition = UiScale.V(1210f, 560f);
        public Vector2 RelicFirstPosition { get { return _relicFirstPosition; } set { _relicFirstPosition = value; } }

        [Tooltip("유물 자리 하나의 크기. 칸 그림(행상_유물칸)이 이 크기로 깔린다. (약 92 × 115) 세로 받침의 폭에 셋이 들어가게 90 × 108 이다(2026년 10월 25일).")]
        [FormerlySerializedAs("RelicSlotSize")]
        [SerializeField] private Vector2 _relicSlotSize = UiScale.V(90f, 108f);
        public Vector2 RelicSlotSize { get { return _relicSlotSize; } set { _relicSlotSize = value; } }

        [Tooltip("유물 자리 사이의 가로 간격. 예전 실측은 134 와 137 이었다. 세로 받침의 폭에 맞춰 110 이다(2026년 10월 25일).")]
        [FormerlySerializedAs("RelicSpacing")]
        [SerializeField] private float _relicSpacing = UiScale.Px(110f);
        public float RelicSpacing { get { return _relicSpacing; } set { _relicSpacing = value; } }

        [Header("유물 받침")]
        [Tooltip("유물 세 칸을 얹는 육각 받침의 왼쪽 위. 예전에는 양탄자 그림에 박혀 있었고 2026년 10월 19일에 따로 떼었다. " +
            "옛 양탄자에서 잰 자리(1108, 445)를 10 단위로 옮겼다. 2026년 10월 25일 원재가 위아래가 뾰족한 세로 육각형으로 세우라고 해 " +
            "그림을 90 도 돌리고 양탄자 가운데(1365)에 맞춰 문양 변경 칸 아래로 옮겼다. 위 꼭짓점에 새로고침 구슬이 앉는다.")]
        [SerializeField] private Vector2 _relicBoardPosition = UiScale.V(1170f, 380f);
        public Vector2 RelicBoardPosition { get { return _relicBoardPosition; } set { _relicBoardPosition = value; } }

        [Tooltip("육각 받침의 크기. 옛 가로 받침은 옛 양탄자에서 잰 약 504 × 397 이었다. " +
            "세로로 세운 그림(800 × 1000)의 비율로 양탄자 안 금테 위에 들어가게 390 × 480 이다(2026년 10월 25일).")]
        [SerializeField] private Vector2 _relicBoardSize = UiScale.V(390f, 480f);
        public Vector2 RelicBoardSize { get { return _relicBoardSize; } set { _relicBoardSize = value; } }

        [Header("문양 변경")]
        [Tooltip("보라 소용돌이 칸의 왼쪽 위. (1235, 150)")]
        [FormerlySerializedAs("SymbolChangePosition")]
        [SerializeField] private Vector2 _symbolChangePosition = UiScale.V(1250f, 135f);
        public Vector2 SymbolChangePosition { get { return _symbolChangePosition; } set { _symbolChangePosition = value; } }

        [Tooltip("보라 소용돌이 칸의 크기. (약 245 × 252) 세로 받침이 들어갈 자리를 내려고 230 × 240 으로 조금 줄였다(2026년 10월 25일).")]
        [FormerlySerializedAs("SymbolChangeSize")]
        [SerializeField] private Vector2 _symbolChangeSize = UiScale.V(230f, 240f);
        public Vector2 SymbolChangeSize { get { return _symbolChangeSize; } set { _symbolChangeSize = value; } }

        [Header("유물 새로고침")]
        [Tooltip("새로고침 버튼(구슬)의 왼쪽 위. 예전에는 양탄자에 박힌 구슬 위에 작은 표시만 얹었고 " +
            "2026년 10월 19일에 구슬 자체를 버튼 그림으로 떼었다. 유물 칸 셋의 가운데에 맞춘다.")]
        [FormerlySerializedAs("RefreshPosition")]
        [SerializeField] private Vector2 _refreshPosition = UiScale.V(1320f, 345f);
        public Vector2 RefreshPosition { get { return _refreshPosition; } set { _refreshPosition = value; } }

        [Tooltip("새로고침 버튼(구슬과 받침대) 크기. 그림은 200 × 260 이다. 세로 받침의 위 꼭짓점에 앉게 90 × 117 이다(2026년 10월 25일).")]
        [FormerlySerializedAs("RefreshSize")]
        [SerializeField] private Vector2 _refreshSize = UiScale.V(90f, 117f);
        public Vector2 RefreshSize { get { return _refreshSize; } set { _refreshSize = value; } }

        [Header("가격")]
        [Tooltip("가격 한 줄이 차지하는 칸. 골드 그림과 숫자가 가운데 맞춤으로 들어간다.")]
        [FormerlySerializedAs("PriceSize")]
        [SerializeField] private Vector2 _priceSize = UiScale.V(150f, 30f);
        public Vector2 PriceSize { get { return _priceSize; } set { _priceSize = value; } }

        [Tooltip("가격의 골드 그림 크기. (31 × 28)")]
        [FormerlySerializedAs("PriceIconSize")]
        [SerializeField] private Vector2 _priceIconSize = UiScale.V(30f, 30f);
        public Vector2 PriceIconSize { get { return _priceIconSize; } set { _priceIconSize = value; } }

        [Tooltip("골드 그림과 숫자 사이 간격.")]
        [FormerlySerializedAs("PriceIconGap")]
        [SerializeField] private float _priceIconGap = UiScale.Px(10f);
        public float PriceIconGap { get { return _priceIconGap; } set { _priceIconGap = value; } }

        [Tooltip("가격 글자 크기.")]
        [FormerlySerializedAs("PriceFontSize")]
        [SerializeField] private float _priceFontSize = UiScale.Px(28f);
        public float PriceFontSize { get { return _priceFontSize; } set { _priceFontSize = value; } }

        [Tooltip("문양 가격 줄의 위쪽이 문양 자리 아래 끝에서 떨어진 거리. 음수면 자리에 걸친다. (실측 -12) " +
            "-10 이라 가격이 문양 칸 아래 테두리에 겹쳐 2026년 10월 25일 원재가 옮기라고 해 칸 아래로 뗐다.")]
        [FormerlySerializedAs("SymbolPriceOffsetY")]
        [SerializeField] private float _symbolPriceOffsetY = UiScale.Px(4f);
        public float SymbolPriceOffsetY { get { return _symbolPriceOffsetY; } set { _symbolPriceOffsetY = value; } }

        [Tooltip("코인 가격 줄. 받침이 둥글어 문양보다 아래에 있다. (실측 8)")]
        [FormerlySerializedAs("CoinPriceOffsetY")]
        [SerializeField] private float _coinPriceOffsetY = UiScale.Px(10f);
        public float CoinPriceOffsetY { get { return _coinPriceOffsetY; } set { _coinPriceOffsetY = value; } }

        [Tooltip("유물 가격 줄. (실측 13)")]
        [FormerlySerializedAs("RelicPriceOffsetY")]
        [SerializeField] private float _relicPriceOffsetY = UiScale.Px(10f);
        public float RelicPriceOffsetY { get { return _relicPriceOffsetY; } set { _relicPriceOffsetY = value; } }

        [Tooltip("새로고침 가격 줄. 구슬 받침대 바로 아래, 유물 칸 위에 놓인다.")]
        [FormerlySerializedAs("RefreshPriceOffsetY")]
        [SerializeField] private float _refreshPriceOffsetY = UiScale.Px(0f);
        public float RefreshPriceOffsetY { get { return _refreshPriceOffsetY; } set { _refreshPriceOffsetY = value; } }

        [Header("나가기")]
        [Tooltip("아래를 가리키는 화살표 버튼의 왼쪽 위. (756, 917)")]
        [FormerlySerializedAs("BackPosition")]
        [SerializeField] private Vector2 _backPosition = UiScale.V(760f, 920f);
        public Vector2 BackPosition { get { return _backPosition; } set { _backPosition = value; } }

        [Tooltip("화살표 버튼의 크기. (375 × 133)")]
        [FormerlySerializedAs("BackSize")]
        [SerializeField] private Vector2 _backSize = UiScale.V(370f, 130f);
        public Vector2 BackSize { get { return _backSize; } set { _backSize = value; } }

        [Tooltip("화살표 안에 적는 글자 크기.")]
        [FormerlySerializedAs("BackFontSize")]
        [SerializeField] private float _backFontSize = UiScale.Px(44f);
        public float BackFontSize { get { return _backFontSize; } set { _backFontSize = value; } }

        [Tooltip("글자를 화살표 머리 쪽으로 밀지 않으려고 아래에 비워 두는 높이. (화살표 머리 약 45)")]
        [FormerlySerializedAs("BackArrowHeadHeight")]
        [SerializeField] private float _backArrowHeadHeight = UiScale.Px(50f);
        public float BackArrowHeadHeight { get { return _backArrowHeadHeight; } set { _backArrowHeadHeight = value; } }

        /// <summary>index 번째 문양 자리의 왼쪽 위. 왼쪽에서 오른쪽, 위에서 아래로 센다.</summary>
        public Vector2 GetSymbolPosition(int index)
        {
            int column = SymbolColumns <= 0 ? 0 : index % SymbolColumns;
            int row = SymbolColumns <= 0 ? index : index / SymbolColumns;

            return new Vector2(
                SymbolFirstPosition.x + column * SymbolSpacing.x,
                SymbolFirstPosition.y + row * SymbolSpacing.y);
        }

        /// <summary>index 번째 코인 자리의 왼쪽 위.</summary>
        public Vector2 GetCoinPosition(int index)
        {
            return new Vector2(CoinFirstPosition.x + index * CoinSpacing, CoinFirstPosition.y);
        }

        /// <summary>index 번째 유물 자리의 왼쪽 위.</summary>
        public Vector2 GetRelicPosition(int index)
        {
            return new Vector2(RelicFirstPosition.x + index * RelicSpacing, RelicFirstPosition.y);
        }

        /// <summary>자리 아래에 붙는 가격 줄의 왼쪽 위. 자리 가운데에 맞춘다.</summary>
        public Vector2 GetPricePosition(Vector2 slotPosition, Vector2 slotSize, float offsetY)
        {
            return new Vector2(
                slotPosition.x + slotSize.x * 0.5f - PriceSize.x * 0.5f,
                slotPosition.y + slotSize.y + offsetY);
        }

        /// <summary>문양 변경 가격은 소용돌이 한가운데에 적는다.</summary>
        public Vector2 GetSymbolChangePricePosition()
        {
            return new Vector2(
                SymbolChangePosition.x + SymbolChangeSize.x * 0.5f - PriceSize.x * 0.5f,
                SymbolChangePosition.y + SymbolChangeSize.y * 0.5f - PriceSize.y * 0.5f);
        }

        /// <summary>문양 자리 넷이 모두 왼쪽 돗자리 안에 들어가는지.</summary>
        public bool SymbolsFitLeftMat(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetSymbolPosition(i), SymbolSlotSize, LeftMatPosition, LeftMatSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>코인 자리가 모두 왼쪽 돗자리 안에 들어가는지.</summary>
        public bool CoinsFitLeftMat(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetCoinPosition(i), CoinSlotSize, LeftMatPosition, LeftMatSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>유물 자리가 모두 오른쪽 양탄자 안에 들어가는지.</summary>
        public bool RelicsFitRightMat(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetRelicPosition(i), RelicSlotSize, RightMatPosition, RightMatSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>육각 받침이 오른쪽 양탄자 안에 들어가고 유물 자리가 모두 받침 안에 들어가는지.</summary>
        public bool RelicsFitBoard(int count)
        {
            if (!Inside(RelicBoardPosition, RelicBoardSize, RightMatPosition, RightMatSize))
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetRelicPosition(i), RelicSlotSize, RelicBoardPosition, RelicBoardSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>새로고침 가격 줄이 유물 칸에 닿지 않는지.</summary>
        public bool RefreshPriceAboveRelics()
        {
            Vector2 price = GetPricePosition(RefreshPosition, RefreshSize, RefreshPriceOffsetY);
            return price.y + PriceSize.y <= RelicFirstPosition.y;
        }

        /// <summary>문양 자리와 코인 자리가 위아래로 겹치지 않는지.</summary>
        public bool SymbolsAndCoinsDoNotOverlap(int symbolCount)
        {
            float bottom = 0f;
            for (int i = 0; i < symbolCount; i++)
            {
                float y = GetSymbolPosition(i).y + SymbolSlotSize.y;
                if (y > bottom)
                {
                    bottom = y;
                }
            }

            return bottom <= CoinFirstPosition.y;
        }

        /// <summary>두 깔개가 서로 겹치지 않는지.</summary>
        public bool MatsDoNotOverlap()
        {
            return LeftMatPosition.x + LeftMatSize.x <= RightMatPosition.x;
        }

        /// <summary>나가기가 두 깔개 사이 아래에 있는지.</summary>
        public bool BackIsBelowMats()
        {
            return BackPosition.y >= LeftMatPosition.y + LeftMatSize.y
                || BackPosition.y >= RightMatPosition.y + RightMatSize.y;
        }

        /// <summary>나가기 글자가 들어갈 칸. 화살표 머리를 뺀 위쪽이다.</summary>
        public Vector2 GetBackLabelSize()
        {
            return new Vector2(BackSize.x, BackSize.y - BackArrowHeadHeight);
        }

        private static bool Inside(Vector2 position, Vector2 size, Vector2 boxPosition, Vector2 boxSize)
        {
            return position.x >= boxPosition.x
                && position.y >= boxPosition.y
                && position.x + size.x <= boxPosition.x + boxSize.x
                && position.y + size.y <= boxPosition.y + boxSize.y;
        }

        private void OnValidate()
        {
            _symbolColumns = Mathf.Max(1, _symbolColumns);
            _coinSpacing = Mathf.Max(0f, _coinSpacing);
            _relicSpacing = Mathf.Max(0f, _relicSpacing);
            _priceIconGap = Mathf.Max(0f, _priceIconGap);
            _backArrowHeadHeight = Mathf.Clamp(_backArrowHeadHeight, 0f, _backSize.y - 1f);
        }
    }
}
