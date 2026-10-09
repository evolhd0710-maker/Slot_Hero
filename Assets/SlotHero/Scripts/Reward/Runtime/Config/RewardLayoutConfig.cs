using UnityEngine;
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
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("보상 박스")]
        [Tooltip("획득 항목을 묶는 칸. 카드가 늘면 가로 폭만 늘어난다. (412, 182)")]
        public Vector2 BoxPosition = UiScale.V(410f, 180f);

        [Tooltip("카드 셋일 때의 크기. (1096 × 662)")]
        public Vector2 BoxSize = UiScale.V(1100f, 660f);

        [Tooltip("박스 좌우 안쪽 여백. 카드가 늘 때 이만큼을 양옆에 남긴다.")]
        public float BoxPadding = UiScale.Px(40f);

        [Header("제목")]
        [Tooltip("박스 왼쪽 위의 `보상` 글자. (412, 약 210)")]
        public Vector2 TitlePosition = UiScale.V(440f, 210f);

        public Vector2 TitleSize = UiScale.V(300f, 60f);

        [Header("골드 줄")]
        [Tooltip("기본 보상 줄. (760, 266)")]
        public Vector2 BaseGoldPosition = UiScale.V(760f, 270f);

        [Tooltip("오버킬 보상 줄. 기본 보상 아래다. (760, 366)")]
        public Vector2 OverkillGoldPosition = UiScale.V(760f, 370f);

        [Tooltip("골드 줄 하나의 크기. (400 × 90)")]
        public Vector2 GoldRowSize = UiScale.V(400f, 90f);

        [Tooltip("골드 줄 안쪽 좌우 여백.")]
        public float GoldRowPadding = UiScale.Px(30f);

        [Header("보상 카드")]
        [Tooltip("기획서가 300 × 300 으로 정했다.")]
        public Vector2 CardSize = UiScale.V(300f, 300f);

        [Tooltip("카드 줄의 y. (518)")]
        public float CardY = UiScale.Px(520f);

        [Tooltip("카드 사이 간격. (예시와 코인 2종 와이어프레임이 60 이다)")]
        public float CardGap = UiScale.Px(60f);

        [Tooltip("카드 안의 그림 크기.")]
        public float CardIconSize = UiScale.Px(190f);

        [Tooltip("카드 위쪽 끝에서 그림까지.")]
        public float CardIconTop = UiScale.Px(20f);

        [Header("건너뛰기")]
        [Tooltip("박스 아래에 놓는다. 와이어프레임은 (787, 874).\n" +
                 "버튼 글이 \"아이템 선택 건너뛰기\" 로 길어져 폭을 넓히고 가운데를 그대로 960 에 맞췄다.")]
        public Vector2 SkipPosition = UiScale.V(760f, 870f);

        [Tooltip("와이어프레임은 (346 × 92). 2026년 10월 4일에 버튼 글이 길어져 400 으로 넓혔다.")]
        public Vector2 SkipSize = UiScale.V(400f, 90f);

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

            return box.x >= 40f
                && box.x + size.x <= ReferenceResolution.x - 40f
                && SkipPosition.y + SkipSize.y <= ReferenceResolution.y - 40f;
        }
    }
}
