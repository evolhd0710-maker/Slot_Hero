using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Profile
{
    /// <summary>
    /// 프로필 선택 화면과 현재 프로필 버튼의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기는
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ProfileVisualConfig",
        menuName = "Slot Hero/프로필/프로필 화면 표시 설정",
        order = 2)]
    public class ProfileVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("제목 칸과 뒤로 버튼. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다. 버튼은 베이지 그림이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("PanelColor")]
        [SerializeField] private Color _panelColor = Color.white;
        public Color PanelColor { get { return _panelColor; } set { _panelColor = value; } }

        [Tooltip("화면 바탕. 돌벽 배경 그림 위에 곱해진다. 흰색이면 그림 그대로다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("BackgroundColor")]
        [SerializeField] private Color _backgroundColor = Color.white;
        public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; } }

        [Header("프로필 카드")]
        [Tooltip("자리마다의 카드 색. 기획서가 카드마다 다른 배경색을 쓰게 한다.\n" +
            "카드는 다른 칸과 같은 어두운 쇠 테 칸 그림이고, 이 색을 곱해 와인 · 이끼 · 남색을 옅게 입힌다(2026년 10월 22일 원재).\n" +
            "예전 와이어프레임의 진한 빨강 · 초록 · 파랑은 다른 UI 와 어울리지 않아 바꿨다. 선형 색 공간이라 곱한 색이 생각보다 진하게 먹는다.")]
        [FormerlySerializedAs("CardColors")]
        [SerializeField] private List<Color> _cardColors = new List<Color>
        {
            new Color(1f, 0.72f, 0.68f, 1f),
            new Color(0.74f, 0.92f, 0.72f, 1f),
            new Color(0.70f, 0.80f, 1f, 1f),
        };
        public List<Color> CardColors { get { return _cardColors; } set { _cardColors = value; } }

        [Tooltip("프로필명과 제목. 어두운 칸과 색 카드 위에 놓여 크림색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("NameColor")]
        [SerializeField] private Color _nameColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color NameColor { get { return _nameColor; } set { _nameColor = value; } }

        [Tooltip("베이지 버튼 위의 글자. 뒤로 버튼에 쓴다(2026년 10월 20일 원재).")]
        [SerializeField] private Color _buttonTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color ButtonTextColor { get { return _buttonTextColor; } set { _buttonTextColor = value; } }

        [Tooltip("플레이 시간과 마지막 플레이 날짜. 기획서가 흰색으로 적게 한다.")]
        [FormerlySerializedAs("InfoColor")]
        [SerializeField] private Color _infoColor = Color.white;
        public Color InfoColor { get { return _infoColor; } set { _infoColor = value; } }

        [Tooltip("비어 있음. 04 화면 공통 규칙 의 버튼 텍스트 색상 2 흰색이다.")]
        [FormerlySerializedAs("EmptyTextColor")]
        [SerializeField] private Color _emptyTextColor = Color.white;
        public Color EmptyTextColor { get { return _emptyTextColor; } set { _emptyTextColor = value; } }

        [Header("수정과 삭제")]
        [Tooltip("버튼 바탕. 와이어프레임에서 카드 색이 살짝 비쳐 흰색 반투명으로 두었다.")]
        [FormerlySerializedAs("ActionButtonColor")]
        [SerializeField] private Color _actionButtonColor = new Color(1f, 1f, 1f, 0.9f);
        public Color ActionButtonColor { get { return _actionButtonColor; } set { _actionButtonColor = value; } }

        [Tooltip("수정 글자. 04 화면 공통 규칙 의 1F1B16.")]
        [FormerlySerializedAs("EditTextColor")]
        [SerializeField] private Color _editTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color EditTextColor { get { return _editTextColor; } set { _editTextColor = value; } }

        [Tooltip("삭제 글자. 되돌릴 수 없는 조작이라 04 화면 공통 규칙 의 강조색 9E2B25 를 쓴다.")]
        [FormerlySerializedAs("DeleteTextColor")]
        [SerializeField] private Color _deleteTextColor = new Color(0.620f, 0.169f, 0.145f, 1f);
        public Color DeleteTextColor { get { return _deleteTextColor; } set { _deleteTextColor = value; } }

        [Header("현재 프로필 버튼")]
        [Tooltip("버튼 칸 그림에 곱하는 색. 다른 칸과 같은 어두운 쇠 테 칸이라 흰색이면 그림 그대로다.\n" +
            "예전에는 흰 테두리 안을 검게 덮어 검은 상자로 보였다(2026년 10월 22일 원재가 다른 UI 와 맞추라고 했다).")]
        [FormerlySerializedAs("CurrentButtonBorderColor")]
        [SerializeField] private Color _currentButtonBorderColor = Color.white;
        public Color CurrentButtonBorderColor { get { return _currentButtonBorderColor; } set { _currentButtonBorderColor = value; } }

        [Tooltip("테두리 안쪽 바탕. 와이어프레임에서 배경이 그대로 비친다.")]
        [FormerlySerializedAs("CurrentButtonFillColor")]
        [SerializeField] private Color _currentButtonFillColor = new Color(0f, 0f, 0f, 0f);
        public Color CurrentButtonFillColor { get { return _currentButtonFillColor; } set { _currentButtonFillColor = value; } }

        [Tooltip("현재 프로필명 글자 색. 어두운 칸 위라 다른 칸 글자와 같은 크림색이다.")]
        [FormerlySerializedAs("CurrentButtonTextColor")]
        [SerializeField] private Color _currentButtonTextColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color CurrentButtonTextColor { get { return _currentButtonTextColor; } set { _currentButtonTextColor = value; } }

        [Header("글자 크기")]
        [Tooltip("제목. 04 화면 공통 규칙 의 화면 주요 텍스트 40픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("TitleFontSize")]
        [SerializeField] private float _titleFontSize = UiScale.Px(40f);
        public float TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; } }

        [Tooltip("프로필명. 04 화면 공통 규칙 의 주요 텍스트 32픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("NameFontSize")]
        [SerializeField] private float _nameFontSize = UiScale.Px(32f);
        public float NameFontSize { get { return _nameFontSize; } set { _nameFontSize = value; } }

        [Tooltip("플레이 정보. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("InfoFontSize")]
        [SerializeField] private float _infoFontSize = UiScale.Px(24f);
        public float InfoFontSize { get { return _infoFontSize; } set { _infoFontSize = value; } }

        [Tooltip("비어 있음. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("EmptyFontSize")]
        [SerializeField] private float _emptyFontSize = UiScale.Px(30f);
        public float EmptyFontSize { get { return _emptyFontSize; } set { _emptyFontSize = value; } }

        [Tooltip("수정과 삭제. 04 화면 공통 규칙 의 보조 텍스트 20픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("ActionFontSize")]
        [SerializeField] private float _actionFontSize = UiScale.Px(20f);
        public float ActionFontSize { get { return _actionFontSize; } set { _actionFontSize = value; } }

        [Tooltip("뒤로. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("BackFontSize")]
        [SerializeField] private float _backFontSize = UiScale.Px(30f);
        public float BackFontSize { get { return _backFontSize; } set { _backFontSize = value; } }

        [Tooltip("현재 프로필명. 와이어프레임에서 잰 26픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("CurrentButtonFontSize")]
        [SerializeField] private float _currentButtonFontSize = UiScale.Px(26f);
        public float CurrentButtonFontSize { get { return _currentButtonFontSize; } set { _currentButtonFontSize = value; } }

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

        [Tooltip("카드에 마우스를 올렸을 때의 크기. 04 화면 공통 규칙 의 카드 호버 5퍼센트 확대.")]
        [Range(1f, 1.5f)]
        [FormerlySerializedAs("CardHoverScale")]
        [SerializeField] private float _cardHoverScale = 1.05f;
        public float CardHoverScale { get { return _cardHoverScale; } set { _cardHoverScale = value; } }

        [Tooltip("읽을 수 없는 카드에 남기는 채도. 04 화면 공통 규칙 의 채도 제거라 0 이다.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("BrokenSaturation")]
        [SerializeField] private float _brokenSaturation = 0f;
        public float BrokenSaturation { get { return _brokenSaturation; } set { _brokenSaturation = value; } }

        [Header("문구")]
        [Tooltip("화면 제목.")]
        [FormerlySerializedAs("TitleText")]
        [SerializeField] private string _titleText = "프로필 선택";
        public string TitleText { get { return _titleText; } set { _titleText = value; } }

        [Tooltip("빈 자리에 적는 글.")]
        [FormerlySerializedAs("EmptyText")]
        [SerializeField] private string _emptyText = "비어 있음";
        public string EmptyText { get { return _emptyText; } set { _emptyText = value; } }

        [Tooltip("수정 버튼에 적는 글.")]
        [FormerlySerializedAs("EditText")]
        [SerializeField] private string _editText = "수정";
        public string EditText { get { return _editText; } set { _editText = value; } }

        [Tooltip("삭제 버튼에 적는 글.")]
        [FormerlySerializedAs("DeleteText")]
        [SerializeField] private string _deleteText = "삭제";
        public string DeleteText { get { return _deleteText; } set { _deleteText = value; } }

        [Tooltip("뒤로 버튼에 적는 글.")]
        [FormerlySerializedAs("BackText")]
        [SerializeField] private string _backText = "뒤로";
        public string BackText { get { return _backText; } set { _backText = value; } }

        [Tooltip("플레이 시간. {0} 시간, {1} 분.")]
        [FormerlySerializedAs("PlayTimeFormat")]
        [SerializeField] private string _playTimeFormat = "플레이 시간 {0}시간 {1}분";
        public string PlayTimeFormat { get { return _playTimeFormat; } set { _playTimeFormat = value; } }

        [Tooltip("마지막 플레이. {0} 자리에 날짜가 들어간다.")]
        [FormerlySerializedAs("LastPlayedFormat")]
        [SerializeField] private string _lastPlayedFormat = "마지막 플레이 {0}";
        public string LastPlayedFormat { get { return _lastPlayedFormat; } set { _lastPlayedFormat = value; } }

        [Tooltip("아직 논 적이 없을 때 날짜 자리에 넣는 글.")]
        [FormerlySerializedAs("LastPlayedNoneText")]
        [SerializeField] private string _lastPlayedNoneText = "없음";
        public string LastPlayedNoneText { get { return _lastPlayedNoneText; } set { _lastPlayedNoneText = value; } }

        [Tooltip("읽을 수 없는 자리에 적는 글.")]
        [FormerlySerializedAs("BrokenText")]
        [SerializeField] private string _brokenText = "불러올 수 없음";
        public string BrokenText { get { return _brokenText; } set { _brokenText = value; } }

        [Tooltip("고른 프로필이 없을 때 현재 프로필 버튼에 적는 글.")]
        [FormerlySerializedAs("NoProfileText")]
        [SerializeField] private string _noProfileText = "프로필 없음";
        public string NoProfileText { get { return _noProfileText; } set { _noProfileText = value; } }

        /// <summary>그 자리의 카드 색. 자리가 색보다 많으면 처음부터 다시 쓴다.</summary>
        public Color GetCardColor(int index)
        {
            if (CardColors.Count == 0)
            {
                return PanelColor;
            }

            int wrapped = index % CardColors.Count;
            if (wrapped < 0)
            {
                wrapped += CardColors.Count;
            }

            return CardColors[wrapped];
        }

        /// <summary>상태까지 더한 카드 색. 읽을 수 없는 자리는 채도를 뺀다.</summary>
        public Color GetCardColor(int index, bool hovering, bool pressed, bool broken)
        {
            Color baseColor = GetCardColor(index);

            if (broken)
            {
                baseColor = Desaturate(baseColor, BrokenSaturation);
            }

            return ApplyBrightness(baseColor, hovering, pressed);
        }

        /// <summary>수정과 삭제 버튼의 바탕색.</summary>
        public Color GetActionButtonColor(bool hovering, bool pressed)
        {
            return ApplyBrightness(ActionButtonColor, hovering, pressed);
        }

        /// <summary>제목 칸과 뒤로 버튼의 색.</summary>
        public Color GetPanelColor(bool hovering, bool pressed)
        {
            return ApplyBrightness(PanelColor, hovering, pressed);
        }

        /// <summary>카드에 마우스를 올렸을 때의 크기 배율.</summary>
        public float GetCardScale(bool hovering)
        {
            return hovering ? CardHoverScale : 1f;
        }

        /// <summary>채도를 뺀 색. 남길 채도를 0 으로 주면 회색이 된다.</summary>
        public static Color Desaturate(Color color, float keep)
        {
            float gray = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            return new Color(
                gray + (color.r - gray) * keep,
                gray + (color.g - gray) * keep,
                gray + (color.b - gray) * keep,
                color.a);
        }

        private Color ApplyBrightness(Color baseColor, bool hovering, bool pressed)
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
