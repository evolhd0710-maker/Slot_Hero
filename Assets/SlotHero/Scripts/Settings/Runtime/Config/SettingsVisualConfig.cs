using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Settings
{
    /// <summary>
    /// 설정 화면의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기는
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SettingsVisualConfig",
        menuName = "Slot Hero/설정/설정 화면 표시 설정",
        order = 2)]
    public class SettingsVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("뒤 화면을 덮는 막. 와이어프레임 기준 검정 50퍼센트.")]
        [FormerlySerializedAs("DimColor")]
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.5f);
        public Color DimColor { get { return _dimColor; } set { _dimColor = value; } }

        [Tooltip("제목 칸과 두 큰 칸, 아래 버튼. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다. 아래 버튼은 베이지 그림이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("PanelColor")]
        [SerializeField] private Color _panelColor = Color.white;
        public Color PanelColor { get { return _panelColor; } set { _panelColor = value; } }

        [Header("분류 탭")]
        [Tooltip("고르지 않은 탭. 흰색이면 그린 버튼 그림(베이지) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("TabColor")]
        [SerializeField] private Color _tabColor = Color.white;
        public Color TabColor { get { return _tabColor; } set { _tabColor = value; } }

        [Tooltip("고른 탭. 기획서는 선택된 탭만 진하게 표시하게 한다. 와이어프레임 기준 4A443C.")]
        [FormerlySerializedAs("SelectedTabColor")]
        [SerializeField] private Color _selectedTabColor = new Color(0.290f, 0.267f, 0.235f, 1f);
        public Color SelectedTabColor { get { return _selectedTabColor; } set { _selectedTabColor = value; } }

        [Tooltip("고르지 않은 탭의 글자. 04 화면 공통 규칙 의 1F1B16.")]
        [FormerlySerializedAs("TabTextColor")]
        [SerializeField] private Color _tabTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color TabTextColor { get { return _tabTextColor; } set { _tabTextColor = value; } }

        [Tooltip("고른 탭의 글자. 와이어프레임 기준 흰색.")]
        [FormerlySerializedAs("SelectedTabTextColor")]
        [SerializeField] private Color _selectedTabTextColor = Color.white;
        public Color SelectedTabTextColor { get { return _selectedTabTextColor; } set { _selectedTabTextColor = value; } }

        [Header("설정 항목")]
        [Tooltip("항목 칸. 흰색이면 그린 버튼 그림(베이지) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("ItemColor")]
        [SerializeField] private Color _itemColor = Color.white;
        public Color ItemColor { get { return _itemColor; } set { _itemColor = value; } }

        [Tooltip("항목 이름과 값의 글자 색.")]
        [FormerlySerializedAs("ItemTextColor")]
        [SerializeField] private Color _itemTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color ItemTextColor { get { return _itemTextColor; } set { _itemTextColor = value; } }

        [Tooltip("제목 칸의 글자. 어두운 칸 위라 크림색이다. 항목과 버튼 글자는 베이지 위라 어두운 색 그대로다(2026년 10월 20일 원재).")]
        [SerializeField] private Color _titleTextColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color TitleTextColor { get { return _titleTextColor; } set { _titleTextColor = value; } }

        [Header("소리 크기 줄")]
        [Tooltip("막대의 바탕.")]
        [FormerlySerializedAs("SliderTrackColor")]
        [SerializeField] private Color _sliderTrackColor = new Color(0.761f, 0.749f, 0.729f, 1f);
        public Color SliderTrackColor { get { return _sliderTrackColor; } set { _sliderTrackColor = value; } }

        [Tooltip("막대에서 찬 부분. 고른 탭과 같은 진한 색이다.")]
        [FormerlySerializedAs("SliderFillColor")]
        [SerializeField] private Color _sliderFillColor = new Color(0.290f, 0.267f, 0.235f, 1f);
        public Color SliderFillColor { get { return _sliderFillColor; } set { _sliderFillColor = value; } }

        [Tooltip("끄는 손잡이.")]
        [FormerlySerializedAs("SliderHandleColor")]
        [SerializeField] private Color _sliderHandleColor = Color.white;
        public Color SliderHandleColor { get { return _sliderHandleColor; } set { _sliderHandleColor = value; } }

        [Tooltip("음소거가 꺼져 있을 때 버튼 색. 흰색이면 그린 버튼 그림(베이지) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("MuteOffColor")]
        [SerializeField] private Color _muteOffColor = Color.white;
        public Color MuteOffColor { get { return _muteOffColor; } set { _muteOffColor = value; } }

        [Tooltip("음소거가 켜져 있을 때 버튼 색. 고른 탭과 같은 진한 색이다.")]
        [FormerlySerializedAs("MuteOnColor")]
        [SerializeField] private Color _muteOnColor = new Color(0.290f, 0.267f, 0.235f, 1f);
        public Color MuteOnColor { get { return _muteOnColor; } set { _muteOnColor = value; } }

        [Tooltip("음소거가 켜져 있을 때 버튼 글자 색.")]
        [FormerlySerializedAs("MuteOnTextColor")]
        [SerializeField] private Color _muteOnTextColor = Color.white;
        public Color MuteOnTextColor { get { return _muteOnTextColor; } set { _muteOnTextColor = value; } }

        [Tooltip("음소거 버튼에 적는 글.")]
        [FormerlySerializedAs("MuteText")]
        [SerializeField] private string _muteText = "음소거";
        public string MuteText { get { return _muteText; } set { _muteText = value; } }

        [Tooltip("음소거가 켜져 있을 때 막대가 흐려지는 정도. 1 이면 그대로다.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("MutedSliderAlpha")]
        [SerializeField] private float _mutedSliderAlpha = 0.4f;
        public float MutedSliderAlpha { get { return _mutedSliderAlpha; } set { _mutedSliderAlpha = value; } }

        [Header("드롭다운")]
        [Tooltip("펼친 목록의 바탕. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("DropdownPanelColor")]
        [SerializeField] private Color _dropdownPanelColor = Color.white;
        public Color DropdownPanelColor { get { return _dropdownPanelColor; } set { _dropdownPanelColor = value; } }

        [Tooltip("지금 고른 선택지 칸. 고른 탭과 같은 진한 색이다.")]
        [FormerlySerializedAs("SelectedOptionColor")]
        [SerializeField] private Color _selectedOptionColor = new Color(0.290f, 0.267f, 0.235f, 1f);
        public Color SelectedOptionColor { get { return _selectedOptionColor; } set { _selectedOptionColor = value; } }

        [Tooltip("지금 고른 선택지 글자.")]
        [FormerlySerializedAs("SelectedOptionTextColor")]
        [SerializeField] private Color _selectedOptionTextColor = Color.white;
        public Color SelectedOptionTextColor { get { return _selectedOptionTextColor; } set { _selectedOptionTextColor = value; } }

        [Tooltip("드롭다운 줄의 값 뒤에 붙여 펼칠 수 있다는 것을 알리는 글.")]
        [FormerlySerializedAs("DropdownMark")]
        [SerializeField] private string _dropdownMark = "  ▼";
        public string DropdownMark { get { return _dropdownMark; } set { _dropdownMark = value; } }

        [Header("글자")]
        [Tooltip("제목. 04 화면 공통 규칙 의 화면 주요 텍스트 40픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("TitleFontSize")]
        [SerializeField] private float _titleFontSize = UiScale.Px(40f);
        public float TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; } }

        [Tooltip("탭과 아래 버튼. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("ButtonFontSize")]
        [SerializeField] private float _buttonFontSize = UiScale.Px(30f);
        public float ButtonFontSize { get { return _buttonFontSize; } set { _buttonFontSize = value; } }

        [Tooltip("항목 이름과 값. 04 화면 공통 규칙 의 항목 텍스트 26픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("ItemFontSize")]
        [SerializeField] private float _itemFontSize = UiScale.Px(26f);
        public float ItemFontSize { get { return _itemFontSize; } set { _itemFontSize = value; } }

        [Header("상태")]
        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        [FormerlySerializedAs("HoverBrightness")]
        [SerializeField] private float _hoverBrightness = 1.10f;
        public float HoverBrightness { get { return _hoverBrightness; } set { _hoverBrightness = value; } }

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("PressedBrightness")]
        [SerializeField] private float _pressedBrightness = 0.90f;
        public float PressedBrightness { get { return _pressedBrightness; } set { _pressedBrightness = value; } }

        [Header("문구")]
        [Tooltip("화면 제목.")]
        [FormerlySerializedAs("TitleText")]
        [SerializeField] private string _titleText = "설정";
        public string TitleText { get { return _titleText; } set { _titleText = value; } }

        [Tooltip("기본값 복원 버튼에 적는 글.")]
        [FormerlySerializedAs("RestoreText")]
        [SerializeField] private string _restoreText = "기본값 복원";
        public string RestoreText { get { return _restoreText; } set { _restoreText = value; } }

        [Tooltip("닫기 버튼에 적는 글.")]
        [FormerlySerializedAs("CloseText")]
        [SerializeField] private string _closeText = "닫기";
        public string CloseText { get { return _closeText; } set { _closeText = value; } }

        /// <summary>탭 색. 고른 탭만 진하게 나온다.</summary>
        public Color GetTabColor(bool selected, bool hovering, bool pressed)
        {
            Color baseColor = selected ? SelectedTabColor : TabColor;
            return ApplyBrightness(baseColor, hovering, pressed);
        }

        /// <summary>탭 글자 색.</summary>
        public Color GetTabTextColor(bool selected)
        {
            return selected ? SelectedTabTextColor : TabTextColor;
        }

        /// <summary>항목 칸 색.</summary>
        public Color GetItemColor(bool hovering, bool pressed)
        {
            return ApplyBrightness(ItemColor, hovering, pressed);
        }

        /// <summary>아래 버튼 색.</summary>
        public Color GetButtonColor(bool hovering, bool pressed)
        {
            return ApplyBrightness(PanelColor, hovering, pressed);
        }

        /// <summary>마우스를 올렸거나 누르고 있을 때의 밝기를 곱한다.</summary>
        public Color ApplyBrightness(Color baseColor, bool hovering, bool pressed)
        {
            float brightness = 1f;
            if (pressed)
            {
                brightness = PressedBrightness;
            }
            else if (hovering)
            {
                brightness = HoverBrightness;
            }

            return new Color(
                baseColor.r * brightness,
                baseColor.g * brightness,
                baseColor.b * brightness,
                baseColor.a);
        }
    }
}
