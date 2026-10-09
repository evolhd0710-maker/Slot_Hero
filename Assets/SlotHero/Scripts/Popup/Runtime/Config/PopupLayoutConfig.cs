using UnityEngine;
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
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("팝업 본체")]
        [Tooltip("팝업 본체의 최대 크기. 기획서 기준 1080 × 820.")]
        public Vector2 MaxPopupSize = UiScale.V(1080f, 820f);

        [Tooltip("본체 안쪽 좌우 여백. 와이어프레임 실측 41 을 10 단위로 옮긴 값이다. 제목 칸 폭이 1000 이 된다.")]
        public float PopupPadding = UiScale.Px(40f);

        [Tooltip("본체 위쪽 여백. 와이어프레임 기준 60.")]
        public float PopupTopPadding = UiScale.Px(60f);

        [Tooltip("본체 아래쪽 여백. 와이어프레임 기준 40.")]
        public float PopupBottomPadding = UiScale.Px(40f);

        [Header("제목과 본문")]
        [Tooltip("제목 칸의 높이. 와이어프레임 실측 76 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        public float TitleHeight = UiScale.Px(80f);

        [Tooltip("제목과 본문 사이 간격. 와이어프레임 기준 30.")]
        public float TitleToBodyGap = UiScale.Px(30f);

        [Tooltip("본문과 버튼 사이 간격. 와이어프레임 기준 40.")]
        public float BodyToButtonGap = UiScale.Px(40f);

        [Tooltip("본문 칸 안쪽 여백. 와이어프레임 실측 32 를 10 단위로 옮긴 값이다.")]
        public float BodyPadding = UiScale.Px(30f);

        [Header("버튼")]
        [Tooltip("버튼 칸의 높이. 와이어프레임 실측 97 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        public float ButtonHeight = UiScale.Px(100f);

        [Header("아이템 상세 오버레이")]
        [Tooltip("아이템 상세 팝업의 크기. 기획서 기준 500 × 380.")]
        public Vector2 ItemDetailSize = UiScale.V(500f, 380f);

        [Tooltip("커서에서 팝업까지 띄우는 거리.")]
        public Vector2 ItemDetailCursorOffset = UiScale.V(20f, -20f);

        [Tooltip("화면 가장자리에서 남기는 최소 여백. 인게임 화면 기획서 04 의 안전 영역 40.")]
        public float ScreenEdgePadding = UiScale.Px(40f);

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
            MaxPopupSize.x = Mathf.Max(1f, MaxPopupSize.x);
            MaxPopupSize.y = Mathf.Max(1f, MaxPopupSize.y);
            TitleHeight = Mathf.Max(1f, TitleHeight);
            ButtonHeight = Mathf.Max(1f, ButtonHeight);
            ItemDetailSize.x = Mathf.Max(1f, ItemDetailSize.x);
            ItemDetailSize.y = Mathf.Max(1f, ItemDetailSize.y);
            ScreenEdgePadding = Mathf.Max(0f, ScreenEdgePadding);
        }
    }
}
