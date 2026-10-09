using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Settings
{
    /// <summary>
    /// 설정 화면의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 07 설정 화면 과 그 와이어프레임에서 잰 값이다.
    ///
    /// 기획서가 정한 것은 제목이 왼쪽 위, 분류 탭과 설정 목록,
    /// 기본값 복원이 좌하단, 닫기가 우하단이라는 것이다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SettingsLayoutConfig",
        menuName = "Slot Hero/설정/설정 화면 자리 설정",
        order = 1)]
    public class SettingsLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("제목")]
        [Tooltip("제목 칸의 자리. 와이어프레임 실측 x 115 · y 76 을 10 단위로 옮긴 값이다.")]
        public Vector2 TitlePosition = UiScale.V(120f, 80f);

        [Tooltip("제목 칸의 크기. 와이어프레임 실측 576 × 86 을 10 단위로 옮긴 값이다.")]
        public Vector2 TitleSize = UiScale.V(580f, 90f);

        [Header("분류 탭")]
        [Tooltip("탭 칸의 자리. 와이어프레임 실측 x 115 · y 205 를 10 단위로 옮긴 값이다.")]
        public Vector2 TabPanelPosition = UiScale.V(120f, 210f);

        [Tooltip("탭 칸의 크기. 와이어프레임 실측 384 × 680 을 10 단위로 옮긴 값이다.")]
        public Vector2 TabPanelSize = UiScale.V(380f, 680f);

        [Tooltip("탭 하나의 크기. 와이어프레임 실측 352 × 88 을 10 단위로 옮긴 값이다.")]
        public Vector2 TabSize = UiScale.V(350f, 90f);

        [Tooltip("탭 사이 간격. 와이어프레임 실측 12 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        public float TabSpacing = UiScale.Px(10f);

        [Tooltip("탭 칸 안쪽 좌우 여백. 와이어프레임 실측 16 을 10 단위로 옮긴 값이다.")]
        public float TabPanelPadding = UiScale.Px(20f);

        [Tooltip("탭 칸 위쪽 여백. 와이어프레임 기준 20.")]
        public float TabPanelTopPadding = UiScale.Px(20f);

        [Header("설정 목록")]
        [Tooltip("목록 칸의 자리. 탭 칸과 60 띄운 값이다.")]
        public Vector2 ListPanelPosition = UiScale.V(560f, 210f);

        [Tooltip("목록 칸의 크기. 오른쪽 끝이 1800 이 되어 왼쪽 여백 120 과 대칭이 되는 값이다.")]
        public Vector2 ListPanelSize = UiScale.V(1240f, 680f);

        [Tooltip("항목 하나의 크기. 목록 칸에서 좌우 여백을 뺀 폭이다.")]
        public Vector2 ItemSize = UiScale.V(1160f, 70f);

        [Tooltip("항목 사이 간격. 와이어프레임 실측 25 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        public float ItemSpacing = UiScale.Px(20f);

        [Tooltip("목록 칸 안쪽 좌우 여백. 와이어프레임 실측 38 을 10 단위로 옮긴 값이다.")]
        public float ListPanelPadding = UiScale.Px(40f);

        [Tooltip("목록 칸 위쪽 여백. 와이어프레임 실측 54 를 10 단위로 옮긴 값이다.")]
        public float ListPanelTopPadding = UiScale.Px(50f);

        [Tooltip("항목 칸 안쪽 좌우 여백. 와이어프레임 실측 24 를 10 단위로 옮긴 값이다.")]
        public float ItemPadding = UiScale.Px(20f);

        [Header("소리 크기 줄")]
        [Tooltip("막대가 시작하는 자리. 항목 칸 왼쪽 끝에서 잰다. 이름이 들어갈 자리를 남긴다.\n" +
                 "와이어프레임에 사운드 탭이 없어 일반 탭의 칸 크기에 맞춰 내가 정했다.")]
        public float SliderLeft = UiScale.Px(300f);

        [Tooltip("막대의 높이. 끄는 손잡이는 이보다 크다.")]
        public float SliderHeight = UiScale.Px(14f);

        [Tooltip("끄는 손잡이 한 변.")]
        public float SliderHandleSize = UiScale.Px(30f);

        [Tooltip("막대 오른쪽에 숫자를 적는 칸의 폭. 100 이 들어간다.")]
        public float SliderValueWidth = UiScale.Px(80f);

        [Tooltip("음소거 버튼의 크기. 항목 칸 오른쪽 끝에 붙는다.")]
        public Vector2 MuteButtonSize = UiScale.V(130f, 50f);

        [Tooltip("막대, 숫자, 음소거 버튼 사이 간격.")]
        public float SliderGap = UiScale.Px(20f);

        [Header("드롭다운")]
        [Tooltip("펼친 목록의 선택지 한 칸 크기. 항목 칸 오른쪽 끝에 맞춰 그 아래로 펼친다.")]
        public Vector2 OptionSize = UiScale.V(300f, 60f);

        [Header("아래 버튼")]
        [Tooltip("기본값 복원의 자리. 와이어프레임 실측 x 115 · y 929 를 10 단위로 옮긴 값이다.")]
        public Vector2 RestorePosition = UiScale.V(120f, 930f);

        [Tooltip("기본값 복원의 크기. 와이어프레임 실측 365 × 86 을 10 단위로 옮긴 값이다.")]
        public Vector2 RestoreSize = UiScale.V(360f, 90f);

        [Tooltip("닫기의 자리. 오른쪽 끝을 목록 칸의 오른쪽 끝 1800 에 맞춘 값이다.")]
        public Vector2 ClosePosition = UiScale.V(1450f, 930f);

        [Tooltip("닫기의 크기. 와이어프레임 실측 346 × 86 을 10 단위로 옮긴 값이다.")]
        public Vector2 CloseSize = UiScale.V(350f, 90f);

        /// <summary>탭 하나의 자리. 화면 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetTabPosition(int index)
        {
            float x = TabPanelPosition.x + TabPanelPadding;
            float y = TabPanelPosition.y + TabPanelTopPadding + index * (TabSize.y + TabSpacing);
            return new Vector2(x, y);
        }

        /// <summary>설정 항목 하나의 자리. 화면 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetItemPosition(int index)
        {
            Vector2 local = GetItemLocalPosition(index);
            return new Vector2(ListPanelPosition.x + local.x, ListPanelPosition.y + local.y);
        }

        /// <summary>
        /// 설정 항목 하나의 자리. 목록 칸 왼쪽 위를 기준으로 잡은 값이다. 스크롤 내용 안에 놓을 때 쓴다.
        ///
        /// **화면 좌표로 놓으면 안 된다.** 예전에는 화면 좌표로 놓고 내용의 원점을 화면 왼쪽 위로 끌어 올렸는데,
        /// 플레이 모드에서는 스크롤 창이 매 프레임 내용을 창 안으로 끌어 넣어 끌어 올린 만큼 항목이 아래로 밀렸다.
        /// 편집 모드에서 뽑은 그림에서는 스크롤이 돌지 않아 멀쩡해 보였다.
        /// </summary>
        public Vector2 GetItemLocalPosition(int index)
        {
            float x = ListPanelPadding;
            float y = ListPanelTopPadding + index * (ItemSize.y + ItemSpacing);
            return new Vector2(x, y);
        }

        /// <summary>탭 칸 안에 들어가는 탭 수.</summary>
        public int GetTabsInPanel()
        {
            float step = TabSize.y + TabSpacing;
            if (step <= 0f)
            {
                return 0;
            }

            float available = TabPanelSize.y - TabPanelTopPadding * 2f;
            return Mathf.FloorToInt((available + TabSpacing) / step);
        }

        /// <summary>목록 칸 안에 들어가는 항목 수. 이보다 많으면 스크롤로 본다.</summary>
        public int GetItemsInPanel()
        {
            float step = ItemSize.y + ItemSpacing;
            if (step <= 0f)
            {
                return 0;
            }

            float available = ListPanelSize.y - ListPanelTopPadding * 2f;
            return Mathf.FloorToInt((available + ItemSpacing) / step);
        }

        /// <summary>항목 수에 맞춘 목록 내용의 높이. 스크롤 안에 담긴다.</summary>
        public float GetListContentHeight(int itemCount)
        {
            if (itemCount <= 0)
            {
                return ListPanelSize.y;
            }

            float step = ItemSize.y + ItemSpacing;
            float height = ListPanelTopPadding * 2f + itemCount * step - ItemSpacing;
            return height < ListPanelSize.y ? ListPanelSize.y : height;
        }

        /// <summary>그만큼의 항목을 보여 주려면 스크롤이 필요한지.</summary>
        public bool NeedsScroll(int itemCount)
        {
            return itemCount > GetItemsInPanel();
        }

        /// <summary>탭 칸과 목록 칸이 겹치지 않는지.</summary>
        public bool PanelsDoNotOverlap()
        {
            return TabPanelPosition.x + TabPanelSize.x <= ListPanelPosition.x;
        }

        private void OnValidate()
        {
            TabSize.x = Mathf.Max(1f, TabSize.x);
            TabSize.y = Mathf.Max(1f, TabSize.y);
            ItemSize.x = Mathf.Max(1f, ItemSize.x);
            ItemSize.y = Mathf.Max(1f, ItemSize.y);
            TabSpacing = Mathf.Max(0f, TabSpacing);
            ItemSpacing = Mathf.Max(0f, ItemSpacing);
        }
    }
}
