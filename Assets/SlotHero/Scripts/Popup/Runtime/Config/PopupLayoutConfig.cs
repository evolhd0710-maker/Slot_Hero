using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Popup
{
    /// <summary>
    /// 팝업과 오버레이의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 08 화면 팝업 의 공통 틀과
    /// 13 현재 빌드 화면 의 아이템 상세 팝업, 그리고 와이어프레임에서 잰 값이다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PopupLayoutConfig",
        menuName = "Slot Hero/팝업/팝업 자리 설정",
        order = 0)]
    public class PopupLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("팝업 본체")]
        [Tooltip("팝업 본체의 최대 크기. 기획서 기준 1080 × 820.")]
        [FormerlySerializedAs("MaxPopupSize")]
        [SerializeField] private Vector2 _maxPopupSize = UiScale.V(1080f, 820f);
        public Vector2 MaxPopupSize { get { return _maxPopupSize; } set { _maxPopupSize = value; } }

        [Tooltip("본체 안쪽 좌우 여백. 와이어프레임 실측 41 을 10 단위로 옮긴 값이다. 제목 칸 폭이 1000 이 된다.")]
        [FormerlySerializedAs("PopupPadding")]
        [SerializeField] private float _popupPadding = UiScale.Px(40f);
        public float PopupPadding { get { return _popupPadding; } set { _popupPadding = value; } }

        [Tooltip("본체 위쪽 여백. 와이어프레임 기준 60.")]
        [FormerlySerializedAs("PopupTopPadding")]
        [SerializeField] private float _popupTopPadding = UiScale.Px(60f);
        public float PopupTopPadding { get { return _popupTopPadding; } set { _popupTopPadding = value; } }

        [Tooltip("본체 아래쪽 여백. 와이어프레임 기준 40.")]
        [FormerlySerializedAs("PopupBottomPadding")]
        [SerializeField] private float _popupBottomPadding = UiScale.Px(40f);
        public float PopupBottomPadding { get { return _popupBottomPadding; } set { _popupBottomPadding = value; } }

        [Header("제목과 본문")]
        [Tooltip("제목 칸의 높이. 와이어프레임 실측 76 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        [FormerlySerializedAs("TitleHeight")]
        [SerializeField] private float _titleHeight = UiScale.Px(80f);
        public float TitleHeight { get { return _titleHeight; } set { _titleHeight = value; } }

        [Tooltip("제목과 본문 사이 간격. 와이어프레임 기준 30.")]
        [FormerlySerializedAs("TitleToBodyGap")]
        [SerializeField] private float _titleToBodyGap = UiScale.Px(30f);
        public float TitleToBodyGap { get { return _titleToBodyGap; } set { _titleToBodyGap = value; } }

        [Tooltip("본문과 버튼 사이 간격. 와이어프레임 기준 40.")]
        [FormerlySerializedAs("BodyToButtonGap")]
        [SerializeField] private float _bodyToButtonGap = UiScale.Px(40f);
        public float BodyToButtonGap { get { return _bodyToButtonGap; } set { _bodyToButtonGap = value; } }

        [Tooltip("본문 칸 안쪽 여백. 와이어프레임 실측 32 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("BodyPadding")]
        [SerializeField] private float _bodyPadding = UiScale.Px(30f);
        public float BodyPadding { get { return _bodyPadding; } set { _bodyPadding = value; } }

        [Header("버튼")]
        [Tooltip("버튼 칸의 높이. 와이어프레임 실측 97 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        [FormerlySerializedAs("ButtonHeight")]
        [SerializeField] private float _buttonHeight = UiScale.Px(100f);
        public float ButtonHeight { get { return _buttonHeight; } set { _buttonHeight = value; } }

        // 아래 넷은 08 화면 팝업 의 "버튼 개수에 따른 배치" 다. 둘과 셋 모두 전체 폭이 980 이 되어 같은 자리에서 시작한다.
        // 예전에는 `PopupLayoutMath` 에 박혀 있어 본체 폭과 함께 조정할 수 없었다(2026년 10월 22일 코덱스 검토).
        [Tooltip("버튼이 하나이거나 둘일 때의 폭. 기획서 기준 450.")]
        [Min(1f)]
        [SerializeField] private float _wideButtonWidth = UiScale.Px(450f);
        public float WideButtonWidth { get { return _wideButtonWidth; } set { _wideButtonWidth = value; } }

        [Tooltip("버튼이 셋일 때의 폭. 기획서 기준 300.")]
        [Min(1f)]
        [SerializeField] private float _narrowButtonWidth = UiScale.Px(300f);
        public float NarrowButtonWidth { get { return _narrowButtonWidth; } set { _narrowButtonWidth = value; } }

        [Tooltip("버튼이 둘일 때의 간격. 기획서 기준 80.")]
        [Min(0f)]
        [SerializeField] private float _wideButtonGap = UiScale.Px(80f);
        public float WideButtonGap { get { return _wideButtonGap; } set { _wideButtonGap = value; } }

        [Tooltip("버튼이 셋일 때의 간격. 기획서 기준 40.")]
        [Min(0f)]
        [SerializeField] private float _narrowButtonGap = UiScale.Px(40f);
        public float NarrowButtonGap { get { return _narrowButtonGap; } set { _narrowButtonGap = value; } }

        [Header("아이템 상세 오버레이")]
        [Tooltip("아이템 상세 팝업의 크기. 기획서 기준 500 × 380.")]
        [FormerlySerializedAs("ItemDetailSize")]
        [SerializeField] private Vector2 _itemDetailSize = UiScale.V(500f, 380f);
        public Vector2 ItemDetailSize { get { return _itemDetailSize; } set { _itemDetailSize = value; } }

        [Tooltip("커서에서 팝업까지 띄우는 거리.")]
        [FormerlySerializedAs("ItemDetailCursorOffset")]
        [SerializeField] private Vector2 _itemDetailCursorOffset = UiScale.V(20f, -20f);
        public Vector2 ItemDetailCursorOffset { get { return _itemDetailCursorOffset; } set { _itemDetailCursorOffset = value; } }

        [Tooltip("화면 가장자리에서 남기는 최소 여백. 인게임 화면 기획서 04 의 안전 영역 40.")]
        [FormerlySerializedAs("ScreenEdgePadding")]
        [SerializeField] private float _screenEdgePadding = UiScale.Px(40f);
        public float ScreenEdgePadding { get { return _screenEdgePadding; } set { _screenEdgePadding = value; } }

        /// <summary>제목과 버튼, 여백이 차지하는 세로 길이. 본문을 뺀 나머지다.</summary>
        public float GetChromeHeight()
        {
            return PopupTopPadding
                   + TitleHeight
                   + TitleToBodyGap
                   + BodyToButtonGap
                   + ButtonHeight
                   + PopupBottomPadding;
        }

        /// <summary>본문 칸이 가질 수 있는 최대 높이.</summary>
        public float GetMaxBodyHeight()
        {
            float max = MaxPopupSize.y - GetChromeHeight();
            return max > 0f ? max : 0f;
        }

        /// <summary>
        /// 본문에 담고 싶은 높이를 넣으면 실제로 쓸 본문 높이를 돌려준다.
        /// 기획서는 내용이 적으면 세로를 줄이고 길면 본문 안에서만 스크롤하게 한다.
        /// </summary>
        public float GetBodyHeight(float desiredHeight)
        {
            float max = GetMaxBodyHeight();
            if (desiredHeight < 0f)
            {
                return 0f;
            }

            return desiredHeight > max ? max : desiredHeight;
        }

        /// <summary>본문 높이에 맞춘 팝업 본체의 크기.</summary>
        public Vector2 GetPopupSize(float desiredBodyHeight)
        {
            return new Vector2(MaxPopupSize.x, GetChromeHeight() + GetBodyHeight(desiredBodyHeight));
        }

        /// <summary>본문이 최대 높이를 넘겨 스크롤이 필요한지.</summary>
        public bool NeedsBodyScroll(float desiredBodyHeight)
        {
            return desiredBodyHeight > GetMaxBodyHeight();
        }

        /// <summary>본체 안에서 제목과 본문, 버튼이 쓰는 가로 길이.</summary>
        public float GetContentWidth()
        {
            return MaxPopupSize.x - PopupPadding * 2f;
        }

        /// <summary>제목 칸의 자리. 본체 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetTitlePosition()
        {
            return new Vector2(PopupPadding, PopupTopPadding);
        }

        /// <summary>본문 칸의 자리.</summary>
        public Vector2 GetBodyPosition()
        {
            return new Vector2(PopupPadding, PopupTopPadding + TitleHeight + TitleToBodyGap);
        }

        /// <summary>버튼 줄의 자리. 본체 아래쪽에서 거꾸로 센다.</summary>
        public float GetButtonTop(float popupHeight)
        {
            return popupHeight - PopupBottomPadding - ButtonHeight;
        }

        /// <summary>
        /// 아이템 상세 팝업이 커서 옆에 놓일 자리.
        /// 13 현재 빌드 화면 의 "화면 가장자리에 닿을 경우 가장자리를 벗어나지 않게 배치한다"를 지킨다.
        /// 넘겨받는 커서 자리와 돌려주는 값 모두 화면 왼쪽 위를 기준으로 한다.
        /// </summary>
        public Vector2 GetItemDetailPosition(Vector2 cursor)
        {
            float x = cursor.x + ItemDetailCursorOffset.x;
            float y = cursor.y + ItemDetailCursorOffset.y;

            float maxX = ReferenceResolution.x - ScreenEdgePadding - ItemDetailSize.x;
            float maxY = ReferenceResolution.y - ScreenEdgePadding - ItemDetailSize.y;
            float minX = ScreenEdgePadding;
            float minY = ScreenEdgePadding;

            if (maxX < minX)
            {
                maxX = minX;
            }

            if (maxY < minY)
            {
                maxY = minY;
            }

            return new Vector2(Mathf.Clamp(x, minX, maxX), Mathf.Clamp(y, minY, maxY));
        }

        private void OnValidate()
        {
            _maxPopupSize.x = Mathf.Max(1f, _maxPopupSize.x);
            _maxPopupSize.y = Mathf.Max(1f, _maxPopupSize.y);
            _titleHeight = Mathf.Max(1f, _titleHeight);
            _buttonHeight = Mathf.Max(1f, _buttonHeight);
            _itemDetailSize.x = Mathf.Max(1f, _itemDetailSize.x);
            _itemDetailSize.y = Mathf.Max(1f, _itemDetailSize.y);
            _screenEdgePadding = Mathf.Max(0f, _screenEdgePadding);
        }
    }
}
