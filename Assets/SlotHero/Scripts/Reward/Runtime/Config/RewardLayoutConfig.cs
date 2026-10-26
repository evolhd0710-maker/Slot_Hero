using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Reward
{
    /// <summary>
    /// 보상 화면의 자리와 크기.
    /// 인게임 화면 기획서 v0.2 / 10 보상 화면 과 그 와이어프레임 석 장에서 잰 값이다.
    ///
    /// 값은 전부 10 단위로 맞췄다. 괄호 안이 와이어프레임 실측값이다.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardLayoutConfig", menuName = "Slot Hero/보상/자리 설정")]
    public class RewardLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("보상 박스")]
        [Tooltip("획득 항목을 묶는 칸. 카드가 늘면 가로 폭만 늘어난다. (412, 182)")]
        [FormerlySerializedAs("BoxPosition")]
        [SerializeField] private Vector2 _boxPosition = UiScale.V(410f, 180f);
        public Vector2 BoxPosition { get { return _boxPosition; } set { _boxPosition = value; } }

        [Tooltip("카드 셋일 때의 크기. (1096 × 662)")]
        [FormerlySerializedAs("BoxSize")]
        [SerializeField] private Vector2 _boxSize = UiScale.V(1100f, 660f);
        public Vector2 BoxSize { get { return _boxSize; } set { _boxSize = value; } }

        [Tooltip("박스 좌우 안쪽 여백. 카드가 늘 때 이만큼을 양옆에 남긴다.")]
        [FormerlySerializedAs("BoxPadding")]
        [SerializeField] private float _boxPadding = UiScale.Px(40f);
        public float BoxPadding { get { return _boxPadding; } set { _boxPadding = value; } }

        [Header("제목")]
        [Tooltip("박스 왼쪽 위의 `보상` 글자. (412, 약 210)")]
        [FormerlySerializedAs("TitlePosition")]
        [SerializeField] private Vector2 _titlePosition = UiScale.V(440f, 210f);
        public Vector2 TitlePosition { get { return _titlePosition; } set { _titlePosition = value; } }

        [FormerlySerializedAs("TitleSize")]
        [SerializeField] private Vector2 _titleSize = UiScale.V(300f, 60f);
        public Vector2 TitleSize { get { return _titleSize; } set { _titleSize = value; } }

        [Header("골드 줄")]
        [Tooltip("기본 보상 줄. (760, 266)")]
        [FormerlySerializedAs("BaseGoldPosition")]
        [SerializeField] private Vector2 _baseGoldPosition = UiScale.V(760f, 270f);
        public Vector2 BaseGoldPosition { get { return _baseGoldPosition; } set { _baseGoldPosition = value; } }

        [Tooltip("오버킬 보상 줄. 기본 보상 아래다. (760, 366)")]
        [FormerlySerializedAs("OverkillGoldPosition")]
        [SerializeField] private Vector2 _overkillGoldPosition = UiScale.V(760f, 370f);
        public Vector2 OverkillGoldPosition { get { return _overkillGoldPosition; } set { _overkillGoldPosition = value; } }

        [Tooltip("골드 줄 하나의 크기. (400 × 90)")]
        [FormerlySerializedAs("GoldRowSize")]
        [SerializeField] private Vector2 _goldRowSize = UiScale.V(400f, 90f);
        public Vector2 GoldRowSize { get { return _goldRowSize; } set { _goldRowSize = value; } }

        [Tooltip("골드 줄 안쪽 좌우 여백.")]
        [FormerlySerializedAs("GoldRowPadding")]
        [SerializeField] private float _goldRowPadding = UiScale.Px(30f);
        public float GoldRowPadding { get { return _goldRowPadding; } set { _goldRowPadding = value; } }

        [Header("보상 카드")]
        [Tooltip("기획서가 300 × 300 으로 정했다.")]
        [FormerlySerializedAs("CardSize")]
        [SerializeField] private Vector2 _cardSize = UiScale.V(300f, 300f);
        public Vector2 CardSize { get { return _cardSize; } set { _cardSize = value; } }

        [Tooltip("카드 줄의 y. (518)")]
        [FormerlySerializedAs("CardY")]
        [SerializeField] private float _cardY = UiScale.Px(520f);
        public float CardY { get { return _cardY; } set { _cardY = value; } }

        [Tooltip("카드 사이 간격. (예시와 코인 2종 와이어프레임이 60 이다)")]
        [FormerlySerializedAs("CardGap")]
        [SerializeField] private float _cardGap = UiScale.Px(60f);
        public float CardGap { get { return _cardGap; } set { _cardGap = value; } }

        [Tooltip("카드 안의 그림 크기. 190 이면 태그 글이 카드 아래 테두리에 붙어 176 으로 줄였다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("CardIconSize")]
        [SerializeField] private float _cardIconSize = UiScale.Px(176f);
        public float CardIconSize { get { return _cardIconSize; } set { _cardIconSize = value; } }

        [Tooltip("카드 위쪽 끝에서 그림까지.")]
        [FormerlySerializedAs("CardIconTop")]
        [SerializeField] private float _cardIconTop = UiScale.Px(18f);
        public float CardIconTop { get { return _cardIconTop; } set { _cardIconTop = value; } }

        [Header("건너뛰기")]
        [Tooltip("박스 아래에 놓는다. 와이어프레임은 (787, 874).\n" +
                 "버튼 글이 \"아이템 선택 건너뛰기\" 로 길어져 폭을 넓히고 가운데를 그대로 960 에 맞췄다.")]
        [FormerlySerializedAs("SkipPosition")]
        [SerializeField] private Vector2 _skipPosition = UiScale.V(760f, 870f);
        public Vector2 SkipPosition { get { return _skipPosition; } set { _skipPosition = value; } }

        [Tooltip("와이어프레임은 (346 × 92). 2026년 10월 4일에 버튼 글이 길어져 400 으로 넓혔다.")]
        [FormerlySerializedAs("SkipSize")]
        [SerializeField] private Vector2 _skipSize = UiScale.V(400f, 90f);
        public Vector2 SkipSize { get { return _skipSize; } set { _skipSize = value; } }

        /// <summary>
        /// 카드를 늘어놓았을 때의 전체 가로 폭.
        /// 카드 크기는 그대로 두고 개수만큼 늘어난다.
        /// </summary>
        public float GetCardsWidth(int count)
        {
            if (count <= 0)
            {
                return 0f;
            }

            return count * CardSize.x + (count - 1) * CardGap;
        }

        /// <summary>카드 하나의 자리. 화면 가운데를 기준으로 가로로 늘어놓는다.</summary>
        public Vector2 GetCardPosition(int index, int count)
        {
            float left = ReferenceResolution.x * 0.5f - GetCardsWidth(count) * 0.5f;
            return new Vector2(left + index * (CardSize.x + CardGap), CardY);
        }

        /// <summary>
        /// 카드 개수에 맞춘 보상 박스 크기.
        /// 10 보상 화면 이 "카드 크기는 유지하고 보상 박스의 가로 폭만 늘린다"로 정했다.
        /// </summary>
        public Vector2 GetBoxSize(int cardCount)
        {
            float needed = GetCardsWidth(cardCount) + BoxPadding * 2f;
            float width = needed > BoxSize.x ? needed : BoxSize.x;
            return new Vector2(width, BoxSize.y);
        }

        /// <summary>카드 개수에 맞춘 보상 박스 자리. 늘어나도 가운데를 지킨다.</summary>
        public Vector2 GetBoxPosition(int cardCount)
        {
            float width = GetBoxSize(cardCount).x;
            return new Vector2(ReferenceResolution.x * 0.5f - width * 0.5f, BoxPosition.y);
        }

        /// <summary>카드가 박스 안에 들어가는지.</summary>
        public bool CardsFitBox(int cardCount)
        {
            Vector2 box = GetBoxPosition(cardCount);
            Vector2 size = GetBoxSize(cardCount);

            Vector2 first = GetCardPosition(0, cardCount);
            float lastRight = GetCardPosition(cardCount - 1, cardCount).x + CardSize.x;

            return first.x >= box.x && lastRight <= box.x + size.x
                && CardY >= box.y && CardY + CardSize.y <= box.y + size.y;
        }

        /// <summary>건너뛰기가 박스 아래에 있는지.</summary>
        public bool SkipIsBelowBox()
        {
            return SkipPosition.y >= BoxPosition.y + BoxSize.y;
        }

        /// <summary>골드 줄 둘이 겹치지 않는지.</summary>
        public bool GoldRowsDoNotOverlap()
        {
            return OverkillGoldPosition.y >= BaseGoldPosition.y + GoldRowSize.y;
        }

        /// <summary>화면 가장자리 40픽셀 안전 영역 안에 들어가는지.</summary>
        public bool FitsSafeArea(int cardCount)
        {
            Vector2 box = GetBoxPosition(cardCount);
            Vector2 size = GetBoxSize(cardCount);
            float margin = UiScale.Px(40f);

            return box.x >= margin
                && box.x + size.x <= ReferenceResolution.x - margin
                && SkipPosition.y + SkipSize.y <= ReferenceResolution.y - margin;
        }
    }
}
