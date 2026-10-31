using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("제목")]
        [Tooltip("제목 칸의 자리. 와이어프레임 실측 x 115 · y 76 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TitlePosition")]
        [SerializeField] private Vector2 _titlePosition = UiScale.V(120f, 80f);
        public Vector2 TitlePosition { get { return _titlePosition; } set { _titlePosition = value; } }

        [Tooltip("제목 칸의 크기. 와이어프레임 실측 576 × 86 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TitleSize")]
        [SerializeField] private Vector2 _titleSize = UiScale.V(580f, 90f);
        public Vector2 TitleSize { get { return _titleSize; } set { _titleSize = value; } }

        [Header("분류 탭")]
        [Tooltip("탭 칸의 자리. 와이어프레임 실측 x 115 · y 205 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TabPanelPosition")]
        [SerializeField] private Vector2 _tabPanelPosition = UiScale.V(120f, 210f);
        public Vector2 TabPanelPosition { get { return _tabPanelPosition; } set { _tabPanelPosition = value; } }

        [Tooltip("탭 칸의 크기. 와이어프레임 실측 384 × 680 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TabPanelSize")]
        [SerializeField] private Vector2 _tabPanelSize = UiScale.V(380f, 680f);
        public Vector2 TabPanelSize { get { return _tabPanelSize; } set { _tabPanelSize = value; } }

        [Tooltip("탭 하나의 크기. 와이어프레임 실측 352 × 88 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TabSize")]
        [SerializeField] private Vector2 _tabSize = UiScale.V(350f, 90f);
        public Vector2 TabSize { get { return _tabSize; } set { _tabSize = value; } }

        [Tooltip("탭 사이 간격. 와이어프레임 실측 12 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("TabSpacing")]
        [SerializeField] private float _tabSpacing = UiScale.Px(10f);
        public float TabSpacing { get { return _tabSpacing; } set { _tabSpacing = value; } }

        [Tooltip("탭 칸 안쪽 좌우 여백. 와이어프레임 실측 16 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("TabPanelPadding")]
        [SerializeField] private float _tabPanelPadding = UiScale.Px(20f);
        public float TabPanelPadding { get { return _tabPanelPadding; } set { _tabPanelPadding = value; } }

        [Tooltip("탭 칸 위쪽 여백. 와이어프레임 기준 20.")]
        [FormerlySerializedAs("TabPanelTopPadding")]
        [SerializeField] private float _tabPanelTopPadding = UiScale.Px(20f);
        public float TabPanelTopPadding { get { return _tabPanelTopPadding; } set { _tabPanelTopPadding = value; } }

        [Header("설정 목록")]
        [Tooltip("목록 칸의 자리. 탭 칸과 60 띄운 값이다.")]
        [FormerlySerializedAs("ListPanelPosition")]
        [SerializeField] private Vector2 _listPanelPosition = UiScale.V(560f, 210f);
        public Vector2 ListPanelPosition { get { return _listPanelPosition; } set { _listPanelPosition = value; } }

        [Tooltip("목록 칸의 크기. 오른쪽 끝이 1800 이 되어 왼쪽 여백 120 과 대칭이 되는 값이다.")]
        [FormerlySerializedAs("ListPanelSize")]
        [SerializeField] private Vector2 _listPanelSize = UiScale.V(1240f, 680f);
        public Vector2 ListPanelSize { get { return _listPanelSize; } set { _listPanelSize = value; } }

        [Tooltip("항목 하나의 크기. 목록 칸에서 좌우 여백을 뺀 폭이다.")]
        [FormerlySerializedAs("ItemSize")]
        [SerializeField] private Vector2 _itemSize = UiScale.V(1160f, 70f);
        public Vector2 ItemSize { get { return _itemSize; } set { _itemSize = value; } }

        [Tooltip("항목 사이 간격. 와이어프레임 실측 25 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        [FormerlySerializedAs("ItemSpacing")]
        [SerializeField] private float _itemSpacing = UiScale.Px(20f);
        public float ItemSpacing { get { return _itemSpacing; } set { _itemSpacing = value; } }

        [Tooltip("목록 칸 안쪽 좌우 여백. 와이어프레임 실측 38 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("ListPanelPadding")]
        [SerializeField] private float _listPanelPadding = UiScale.Px(40f);
        public float ListPanelPadding { get { return _listPanelPadding; } set { _listPanelPadding = value; } }

        [Tooltip("목록 칸 위쪽 여백. 와이어프레임 실측 54 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("ListPanelTopPadding")]
        [SerializeField] private float _listPanelTopPadding = UiScale.Px(50f);
        public float ListPanelTopPadding { get { return _listPanelTopPadding; } set { _listPanelTopPadding = value; } }

        [Tooltip("항목 칸 안쪽 좌우 여백. 와이어프레임 실측 24 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("ItemPadding")]
        [SerializeField] private float _itemPadding = UiScale.Px(20f);
        public float ItemPadding { get { return _itemPadding; } set { _itemPadding = value; } }

        [Header("소리 크기 줄")]
        [Tooltip("막대가 시작하는 자리. 항목 칸 왼쪽 끝에서 잰다. 이름이 들어갈 자리를 남긴다.\n" +
                 "와이어프레임에 사운드 탭이 없어 일반 탭의 칸 크기에 맞춰 내가 정했다.")]
        [FormerlySerializedAs("SliderLeft")]
        [SerializeField] private float _sliderLeft = UiScale.Px(300f);
        public float SliderLeft { get { return _sliderLeft; } set { _sliderLeft = value; } }

        [Tooltip("막대의 높이. 끄는 손잡이는 이보다 크다.")]
        [FormerlySerializedAs("SliderHeight")]
        [SerializeField] private float _sliderHeight = UiScale.Px(14f);
        public float SliderHeight { get { return _sliderHeight; } set { _sliderHeight = value; } }

        [Tooltip("끄는 손잡이 한 변.")]
        [FormerlySerializedAs("SliderHandleSize")]
        [SerializeField] private float _sliderHandleSize = UiScale.Px(30f);
        public float SliderHandleSize { get { return _sliderHandleSize; } set { _sliderHandleSize = value; } }

        [Tooltip("막대 오른쪽에 숫자를 적는 칸의 폭. 100 이 들어간다.")]
        [FormerlySerializedAs("SliderValueWidth")]
        [SerializeField] private float _sliderValueWidth = UiScale.Px(80f);
        public float SliderValueWidth { get { return _sliderValueWidth; } set { _sliderValueWidth = value; } }

        [Tooltip("음소거 버튼의 크기. 항목 칸 오른쪽 끝에 붙는다.")]
        [FormerlySerializedAs("MuteButtonSize")]
        [SerializeField] private Vector2 _muteButtonSize = UiScale.V(130f, 50f);
        public Vector2 MuteButtonSize { get { return _muteButtonSize; } set { _muteButtonSize = value; } }

        [Tooltip("막대, 숫자, 음소거 버튼 사이 간격.")]
        [FormerlySerializedAs("SliderGap")]
        [SerializeField] private float _sliderGap = UiScale.Px(20f);
        public float SliderGap { get { return _sliderGap; } set { _sliderGap = value; } }

        [Header("드롭다운")]
        [Tooltip("펼친 목록의 선택지 한 칸 크기. 항목 칸 오른쪽 끝에 맞춰 그 아래로 펼친다.")]
        [FormerlySerializedAs("OptionSize")]
        [SerializeField] private Vector2 _optionSize = UiScale.V(300f, 60f);
        public Vector2 OptionSize { get { return _optionSize; } set { _optionSize = value; } }

        [Header("아래 버튼")]
        [Tooltip("기본값 복원의 자리. 와이어프레임 실측 x 115 · y 929 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("RestorePosition")]
        [SerializeField] private Vector2 _restorePosition = UiScale.V(120f, 930f);
        public Vector2 RestorePosition { get { return _restorePosition; } set { _restorePosition = value; } }

        [Tooltip("기본값 복원의 크기. 와이어프레임 실측 365 × 86 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("RestoreSize")]
        [SerializeField] private Vector2 _restoreSize = UiScale.V(360f, 90f);
        public Vector2 RestoreSize { get { return _restoreSize; } set { _restoreSize = value; } }

        [Tooltip("닫기의 자리. 오른쪽 끝을 목록 칸의 오른쪽 끝 1800 에 맞춘 값이다.")]
        [FormerlySerializedAs("ClosePosition")]
        [SerializeField] private Vector2 _closePosition = UiScale.V(1450f, 930f);
        public Vector2 ClosePosition { get { return _closePosition; } set { _closePosition = value; } }

        [Tooltip("닫기의 크기. 와이어프레임 실측 346 × 86 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("CloseSize")]
        [SerializeField] private Vector2 _closeSize = UiScale.V(350f, 90f);
        public Vector2 CloseSize { get { return _closeSize; } set { _closeSize = value; } }

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
            _tabSize.x = Mathf.Max(1f, _tabSize.x);
            _tabSize.y = Mathf.Max(1f, _tabSize.y);
            _itemSize.x = Mathf.Max(1f, _itemSize.x);
            _itemSize.y = Mathf.Max(1f, _itemSize.y);
            _tabSpacing = Mathf.Max(0f, _tabSpacing);
            _itemSpacing = Mathf.Max(0f, _itemSpacing);
        }
    }
}
