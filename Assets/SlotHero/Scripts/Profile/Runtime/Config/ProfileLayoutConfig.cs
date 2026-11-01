using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Profile
{
    /// <summary>
    /// 프로필 선택 화면과 현재 프로필 버튼의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 05 타이틀 화면, 06 프로필 선택 화면 과
    /// 그 와이어프레임에서 잰 값이다.
    ///
    /// 기획서가 정한 것은 제목이 왼쪽 위, 수정이 카드 오른쪽 위, 삭제가 그 오른쪽,
    /// 뒤로가 우측 하단이라는 것까지다. 나머지는 와이어프레임에서 재 왔다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ProfileLayoutConfig",
        menuName = "Slot Hero/프로필/프로필 화면 자리 설정",
        order = 1)]
    public class ProfileLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("제목")]
        [Tooltip("제목 칸의 자리. 와이어프레임 실측 x 115 · y 76 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TitlePosition")]
        [SerializeField] private Vector2 _titlePosition = UiScale.V(120f, 80f);
        public Vector2 TitlePosition { get { return _titlePosition; } set { _titlePosition = value; } }

        [Tooltip("제목 칸의 크기. 와이어프레임 실측 768 × 97 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TitleSize")]
        [SerializeField] private Vector2 _titleSize = UiScale.V(770f, 100f);
        public Vector2 TitleSize { get { return _titleSize; } set { _titleSize = value; } }

        [Header("프로필 카드")]
        [Tooltip("첫 카드의 자리. 와이어프레임 실측 x 115 · y 416 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("CardPosition")]
        [SerializeField] private Vector2 _cardPosition = UiScale.V(120f, 420f);
        public Vector2 CardPosition { get { return _cardPosition; } set { _cardPosition = value; } }

        [Tooltip("카드 하나의 크기. 와이어프레임 실측 518 × 260 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("CardSize")]
        [SerializeField] private Vector2 _cardSize = UiScale.V(520f, 260f);
        public Vector2 CardSize { get { return _cardSize; } set { _cardSize = value; } }

        [Tooltip("카드 사이 간격. 카드 셋과 좌우 여백을 더해 화면 폭 1920 이 딱 떨어지는 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("CardSpacing")]
        [SerializeField] private float _cardSpacing = UiScale.Px(60f);
        public float CardSpacing { get { return _cardSpacing; } set { _cardSpacing = value; } }

        [Header("카드 안쪽")]
        [Tooltip("카드 안쪽 좌우 여백. 와이어프레임 실측 25 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("CardPadding")]
        [SerializeField] private float _cardPadding = UiScale.Px(20f);
        public float CardPadding { get { return _cardPadding; } set { _cardPadding = value; } }

        [Tooltip("프로필명 자리. 카드 왼쪽 위에서 잰 값이다. 와이어프레임 실측 68 을 옮겼다.")]
        [FormerlySerializedAs("NameTop")]
        [SerializeField] private float _nameTop = UiScale.Px(70f);
        public float NameTop { get { return _nameTop; } set { _nameTop = value; } }

        [Tooltip("프로필명 칸의 높이. 32픽셀 글자가 들어갈 만큼 둔다.")]
        [Min(1f)]
        [FormerlySerializedAs("NameHeight")]
        [SerializeField] private float _nameHeight = UiScale.Px(40f);
        public float NameHeight { get { return _nameHeight; } set { _nameHeight = value; } }

        [Tooltip("플레이 정보 첫 줄 자리. 카드 위에서 잰 값이다. 와이어프레임 실측 126 을 옮겼다.")]
        [FormerlySerializedAs("InfoTop")]
        [SerializeField] private float _infoTop = UiScale.Px(130f);
        public float InfoTop { get { return _infoTop; } set { _infoTop = value; } }

        [Tooltip("플레이 정보 한 줄의 높이.")]
        [Min(1f)]
        [FormerlySerializedAs("InfoHeight")]
        [SerializeField] private float _infoHeight = UiScale.Px(30f);
        public float InfoHeight { get { return _infoHeight; } set { _infoHeight = value; } }

        [Tooltip("플레이 정보 줄 사이 간격. 와이어프레임 실측 44 를 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        [FormerlySerializedAs("InfoSpacing")]
        [SerializeField] private float _infoSpacing = UiScale.Px(40f);
        public float InfoSpacing { get { return _infoSpacing; } set { _infoSpacing = value; } }

        [Header("수정과 삭제")]
        [Tooltip("버튼 하나의 크기. 와이어프레임 실측 56 × 56 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("ActionButtonSize")]
        [SerializeField] private Vector2 _actionButtonSize = UiScale.V(60f, 60f);
        public Vector2 ActionButtonSize { get { return _actionButtonSize; } set { _actionButtonSize = value; } }

        [Tooltip("두 버튼 사이 간격. 와이어프레임 실측 12 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("ActionButtonSpacing")]
        [SerializeField] private float _actionButtonSpacing = UiScale.Px(10f);
        public float ActionButtonSpacing { get { return _actionButtonSpacing; } set { _actionButtonSpacing = value; } }

        [Tooltip("버튼이 카드 오른쪽 위에서 떨어진 거리. 와이어프레임 기준 20.")]
        [Min(0f)]
        [FormerlySerializedAs("ActionButtonMargin")]
        [SerializeField] private float _actionButtonMargin = UiScale.Px(20f);
        public float ActionButtonMargin { get { return _actionButtonMargin; } set { _actionButtonMargin = value; } }

        [Header("뒤로")]
        [Tooltip("뒤로 버튼의 자리. 오른쪽 끝을 카드 줄의 오른쪽 끝 1800 에 맞춘 값이다.")]
        [FormerlySerializedAs("BackPosition")]
        [SerializeField] private Vector2 _backPosition = UiScale.V(1510f, 920f);
        public Vector2 BackPosition { get { return _backPosition; } set { _backPosition = value; } }

        [Tooltip("뒤로 버튼의 크기. 와이어프레임 실측 288 × 86 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("BackSize")]
        [SerializeField] private Vector2 _backSize = UiScale.V(290f, 90f);
        public Vector2 BackSize { get { return _backSize; } set { _backSize = value; } }

        [Header("타이틀 화면의 현재 프로필 버튼")]
        [Tooltip("현재 프로필 버튼의 자리. 오른쪽 끝을 카드 줄의 오른쪽 끝 1800 에 맞춘 값이다.")]
        [FormerlySerializedAs("CurrentButtonPosition")]
        [SerializeField] private Vector2 _currentButtonPosition = UiScale.V(1360f, 50f);
        public Vector2 CurrentButtonPosition { get { return _currentButtonPosition; } set { _currentButtonPosition = value; } }

        [Tooltip("현재 프로필 버튼의 크기. 런 종료 결과의 타이틀로 버튼과 같은 440 × 100 이다.")]
        [FormerlySerializedAs("CurrentButtonSize")]
        [SerializeField] private Vector2 _currentButtonSize = UiScale.V(440f, 100f);
        public Vector2 CurrentButtonSize { get { return _currentButtonSize; } set { _currentButtonSize = value; } }

        [Tooltip("현재 프로필 버튼 안쪽 왼쪽 여백. 와이어프레임 기준 20.")]
        [Min(0f)]
        [FormerlySerializedAs("CurrentButtonPadding")]
        [SerializeField] private float _currentButtonPadding = UiScale.Px(20f);
        public float CurrentButtonPadding { get { return _currentButtonPadding; } set { _currentButtonPadding = value; } }

        /// <summary>카드 하나의 자리. 화면 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetCardPosition(int index)
        {
            float x = CardPosition.x + index * (CardSize.x + CardSpacing);
            return new Vector2(x, CardPosition.y);
        }

        /// <summary>카드 안쪽에서 프로필명이 놓이는 자리.</summary>
        public Vector2 GetNamePosition(Vector2 cardPosition)
        {
            return new Vector2(cardPosition.x + CardPadding, cardPosition.y + NameTop);
        }

        /// <summary>카드 안쪽에서 플레이 정보 한 줄이 놓이는 자리.</summary>
        public Vector2 GetInfoPosition(Vector2 cardPosition, int line)
        {
            float y = cardPosition.y + InfoTop + line * InfoSpacing;
            return new Vector2(cardPosition.x + CardPadding, y);
        }

        /// <summary>카드 안쪽 글이 쓸 수 있는 가로 폭.</summary>
        public float GetCardTextWidth()
        {
            return CardSize.x - CardPadding * 2f;
        }

        /// <summary>
        /// 수정과 삭제 버튼의 자리.
        /// 오른쪽 끝부터 세어 0 이 맨 오른쪽인 삭제, 1 이 그 왼쪽인 수정이다.
        /// </summary>
        public Vector2 GetActionButtonPosition(Vector2 cardPosition, int indexFromRight)
        {
            float right = cardPosition.x + CardSize.x - ActionButtonMargin;
            float x = right - ActionButtonSize.x
                      - indexFromRight * (ActionButtonSize.x + ActionButtonSpacing);
            return new Vector2(x, cardPosition.y + ActionButtonMargin);
        }

        /// <summary>마지막 카드의 오른쪽 끝.</summary>
        public float GetCardsRight(int cardCount)
        {
            if (cardCount <= 0)
            {
                return CardPosition.x;
            }

            return GetCardPosition(cardCount - 1).x + CardSize.x;
        }

        /// <summary>카드 줄이 화면 좌우 가운데에 놓이는지. 와이어프레임 기준 양쪽 다 115다.</summary>
        public bool CardsAreCentered(int cardCount)
        {
            float left = CardPosition.x;
            float right = ReferenceResolution.x - GetCardsRight(cardCount);
            return Mathf.Abs(left - right) < 1f;
        }

        /// <summary>뒤로 버튼의 오른쪽 끝이 카드 줄의 오른쪽 끝과 맞는지.</summary>
        public bool BackIsAlignedWithCards(int cardCount)
        {
            float back = BackPosition.x + BackSize.x;
            return Mathf.Abs(back - GetCardsRight(cardCount)) < 1f;
        }

        /// <summary>현재 프로필 버튼의 오른쪽 끝이 카드 줄의 오른쪽 끝과 맞는지.</summary>
        public bool CurrentButtonIsAlignedWithCards(int cardCount)
        {
            float button = CurrentButtonPosition.x + CurrentButtonSize.x;
            return Mathf.Abs(button - GetCardsRight(cardCount)) < 1f;
        }

        private void OnValidate()
        {
            _titleSize.x = Mathf.Max(1f, _titleSize.x);
            _titleSize.y = Mathf.Max(1f, _titleSize.y);
            _cardSize.x = Mathf.Max(1f, _cardSize.x);
            _cardSize.y = Mathf.Max(1f, _cardSize.y);
            _backSize.x = Mathf.Max(1f, _backSize.x);
            _backSize.y = Mathf.Max(1f, _backSize.y);
            _currentButtonSize.x = Mathf.Max(1f, _currentButtonSize.x);
            _currentButtonSize.y = Mathf.Max(1f, _currentButtonSize.y);
            _actionButtonSize.x = Mathf.Max(1f, _actionButtonSize.x);
            _actionButtonSize.y = Mathf.Max(1f, _actionButtonSize.y);
            _cardSpacing = Mathf.Max(0f, _cardSpacing);
            _cardPadding = Mathf.Max(0f, _cardPadding);
            _infoSpacing = Mathf.Max(1f, _infoSpacing);
        }
    }
}
