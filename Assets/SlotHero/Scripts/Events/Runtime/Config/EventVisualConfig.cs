using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Events
{
    /// <summary>
    /// 이벤트 화면의 색과 글자.
    /// 칸 색은 와이어프레임에서 재 왔고, 글자 크기와 색은
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EventVisualConfig",
        menuName = "Slot Hero/이벤트/이벤트 화면 표시 설정",
        order = 1)]
    public class EventVisualConfig : ScriptableObject
    {
        [Header("삽화")]
        [Tooltip("삽화 그림이 없는 화면에 까는 옅은 칸의 색. 자리를 알아볼 수 있게만 둔다.")]
        [FormerlySerializedAs("IllustrationPlaceholderColor")]
        [SerializeField] private Color _illustrationPlaceholderColor = new Color(1f, 1f, 1f, 0.12f);
        public Color IllustrationPlaceholderColor { get { return _illustrationPlaceholderColor; } set { _illustrationPlaceholderColor = value; } }

        [Header("칸 색")]
        [Tooltip("본문 칸과 고를 수 있는 선택지 칸. 본문은 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다. 선택지는 베이지 버튼 그림이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("PanelColor")]
        [SerializeField] private Color _panelColor = Color.white;
        public Color PanelColor { get { return _panelColor; } set { _panelColor = value; } }

        [Tooltip("이미 고른 선택지 칸. 와이어프레임은 흐리게 남긴다.")]
        [FormerlySerializedAs("ChosenPanelColor")]
        [SerializeField] private Color _chosenPanelColor = new Color(0.463f, 0.459f, 0.455f, 1f);
        public Color ChosenPanelColor { get { return _chosenPanelColor; } set { _chosenPanelColor = value; } }

        [Tooltip("조건을 채우지 못한 선택지 칸. 기획서는 칸 색을 바꾸지 않고 부족한 항목만 붉게 적는다.")]
        [FormerlySerializedAs("BlockedPanelColor")]
        [SerializeField] private Color _blockedPanelColor = Color.white;
        public Color BlockedPanelColor { get { return _blockedPanelColor; } set { _blockedPanelColor = value; } }

        [Tooltip("이미 고른 선택지의 투명도.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("ChosenAlpha")]
        [SerializeField] private float _chosenAlpha = 0.7f;
        public float ChosenAlpha { get { return _chosenAlpha; } set { _chosenAlpha = value; } }

        [Header("글자")]
        [Tooltip("본문 글자 크기. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("BodyFontSize")]
        [SerializeField] private float _bodyFontSize = UiScale.Px(24f);
        public float BodyFontSize { get { return _bodyFontSize; } set { _bodyFontSize = value; } }

        [Tooltip("본문이 칸을 넘칠 때 줄일 수 있는 가장 작은 글자 크기. 04 화면 공통 규칙 텍스트 표의 가장 작은 20픽셀.\n" +
                 "이벤트 기획서의 스토리는 길어 24픽셀로는 본문 칸을 넘치는 화면이 있다. " +
                 "06 문양 거래 의 첫 화면이 그렇다. 넘칠 때만 줄어들고 짧은 글은 24픽셀 그대로다.")]
        [Min(1f)]
        [FormerlySerializedAs("BodyMinFontSize")]
        [SerializeField] private float _bodyMinFontSize = UiScale.Px(20f);
        public float BodyMinFontSize { get { return _bodyMinFontSize; } set { _bodyMinFontSize = value; } }

        [Tooltip("선택지 글자 크기. 04 화면 공통 규칙 의 항목 텍스트는 26픽셀인데\n" +
                 "글이 긴 선택지가 여럿이면 화면 아래로 넘쳐 2026년 10월 5일 원재의 요청으로 24픽셀로 줄였다.")]
        [Min(1f)]
        [FormerlySerializedAs("ChoiceFontSize")]
        [SerializeField] private float _choiceFontSize = UiScale.Px(24f);
        public float ChoiceFontSize { get { return _choiceFontSize; } set { _choiceFontSize = value; } }

        [Tooltip("선택지가 쌓여 칸을 넘칠 때 줄일 수 있는 가장 작은 글자 크기. 04 화면 공통 규칙 텍스트 표의 가장 작은 20픽셀.\n" +
                 "넘칠 때만 1픽셀씩 줄이고 짧은 선택지는 24픽셀 그대로다.")]
        [Min(1f)]
        [FormerlySerializedAs("ChoiceMinFontSize")]
        [SerializeField] private float _choiceMinFontSize = UiScale.Px(20f);
        public float ChoiceMinFontSize { get { return _choiceMinFontSize; } set { _choiceMinFontSize = value; } }

        [Tooltip("본문 글자. 어두운 칸 위라 크림색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("TextColor")]
        [SerializeField] private Color _textColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color TextColor { get { return _textColor; } set { _textColor = value; } }

        [Tooltip("선택지 글자. 베이지 버튼 위라 어두운 색이다(2026년 10월 20일 원재).")]
        [SerializeField] private Color _choiceTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color ChoiceTextColor { get { return _choiceTextColor; } set { _choiceTextColor = value; } }

        [Tooltip("이미 고른 선택지의 글자. 고른 칸은 어둡게 가라앉으므로 크림색이다(2026년 10월 21일 코덱스 검토).")]
        [SerializeField] private Color _chosenTextColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color ChosenTextColor { get { return _chosenTextColor; } set { _chosenTextColor = value; } }

        [Tooltip("본문 칸 안쪽 여백. 와이어프레임 기준 좌우 30, 위 40.")]
        [FormerlySerializedAs("BodyPadding")]
        [SerializeField] private Vector2 _bodyPadding = UiScale.V(30f, 40f);
        public Vector2 BodyPadding { get { return _bodyPadding; } set { _bodyPadding = value; } }

        [Tooltip("선택지 칸 안쪽 좌우 여백. 와이어프레임 기준 20.")]
        [FormerlySerializedAs("ChoicePadding")]
        [SerializeField] private float _choicePadding = UiScale.Px(20f);
        public float ChoicePadding { get { return _choicePadding; } set { _choicePadding = value; } }

        [Header("선택지 글")]
        [Tooltip("선택지(베이지 버튼)의 얻는 것에 입히는 색. 베이지 위에서 읽히게 짙은 초록이다. " +
            "본문은 `BodyGainColor` 를 쓴다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("GainColor")]
        [SerializeField] private Color _gainColor = new Color(0.082f, 0.353f, 0.145f, 1f);
        public Color GainColor { get { return _gainColor; } set { _gainColor = value; } }

        [Tooltip("선택지(베이지 버튼)의 잃거나 치르는 것에 입히는 색. 베이지 위에서 읽히게 짙은 빨강이다. " +
            "본문은 `BodyCostColor` 를 쓴다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("CostColor")]
        [SerializeField] private Color _costColor = new Color(0.557f, 0.122f, 0.090f, 1f);
        public Color CostColor { get { return _costColor; } set { _costColor = value; } }

        [Tooltip("필요한 것을 갖췄을 때 입히는 색. 선택지 위라 짙은 초록이다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("RequirementMetColor")]
        [SerializeField] private Color _requirementMetColor = new Color(0.082f, 0.353f, 0.145f, 1f);
        public Color RequirementMetColor { get { return _requirementMetColor; } set { _requirementMetColor = value; } }

        [Tooltip("필요한 것을 갖추지 못했을 때 입히는 색. 선택지 위라 짙은 빨강이다(2026년 10월 21일 코덱스 검토).")]
        [FormerlySerializedAs("RequirementUnmetColor")]
        [SerializeField] private Color _requirementUnmetColor = new Color(0.557f, 0.122f, 0.090f, 1f);
        public Color RequirementUnmetColor { get { return _requirementUnmetColor; } set { _requirementUnmetColor = value; } }

        [Tooltip("본문(어두운 칸)의 얻는 것에 입히는 색. 어두운 칸 위에서 읽히게 밝은 초록이다(2026년 10월 21일 코덱스 검토).")]
        [SerializeField] private Color _bodyGainColor = new Color(0.498f, 0.824f, 0.478f, 1f);
        public Color BodyGainColor { get { return _bodyGainColor; } set { _bodyGainColor = value; } }

        [Tooltip("본문(어두운 칸)의 잃거나 치르는 것에 입히는 색. 어두운 칸 위에서 읽히게 밝은 빨강이다(2026년 10월 21일 코덱스 검토).")]
        [SerializeField] private Color _bodyCostColor = new Color(0.941f, 0.478f, 0.416f, 1f);
        public Color BodyCostColor { get { return _bodyCostColor; } set { _bodyCostColor = value; } }

        [Tooltip("선택지 문구와 결과 사이에 넣는 글자. 칸 폭을 아끼려고 양옆 빈칸을 하나씩만 둔다.")]
        [FormerlySerializedAs("Separator")]
        [SerializeField] private string _separator = " — ";
        public string Separator { get { return _separator; } set { _separator = value; } }

        [Tooltip("결과가 여러 줄일 때 사이에 넣는 글자.")]
        [FormerlySerializedAs("LineSeparator")]
        [SerializeField] private string _lineSeparator = ", ";
        public string LineSeparator { get { return _lineSeparator; } set { _lineSeparator = value; } }

        [Tooltip("결과를 알 수 없는 선택지에 적는 글자.")]
        [FormerlySerializedAs("HiddenMark")]
        [SerializeField] private string _hiddenMark = "?";
        public string HiddenMark { get { return _hiddenMark; } set { _hiddenMark = value; } }

        [Tooltip("필요한 항목을 감싸는 꼴. {0} 자리에 내용이 들어간다.")]
        [FormerlySerializedAs("RequirementFormat")]
        [SerializeField] private string _requirementFormat = " [{0}]";
        public string RequirementFormat { get { return _requirementFormat; } set { _requirementFormat = value; } }

        [Header("선택지 상태")]
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

        /// <summary>선택지 글을 만들 때 쓸 꼴을 지금 설정으로 만든다.</summary>
        public EventChoiceTextFormat GetTextFormat()
        {
            EventChoiceTextFormat format = new EventChoiceTextFormat();
            format.Separator = Separator;
            format.LineSeparator = LineSeparator;
            format.HiddenMark = HiddenMark;
            format.RequirementFormat = RequirementFormat;
            format.GainColorHex = ColorUtility.ToHtmlStringRGB(GainColor);
            format.CostColorHex = ColorUtility.ToHtmlStringRGB(CostColor);
            format.RequirementMetColorHex = ColorUtility.ToHtmlStringRGB(RequirementMetColor);
            format.RequirementUnmetColorHex = ColorUtility.ToHtmlStringRGB(RequirementUnmetColor);
            return format;
        }

        /// <summary>
        /// 본문(어두운 칸)에 쓰는 글 형식. 얻은 것과 잃은 것의 색만 밝은 쪽으로 바꾼다.
        /// 선택지와 같은 짙은 색을 쓰면 어두운 칸 위에서 읽히지 않는다(2026년 10월 21일 코덱스 검토).
        /// </summary>
        public EventChoiceTextFormat GetBodyTextFormat()
        {
            EventChoiceTextFormat format = GetTextFormat();
            format.GainColorHex = ColorUtility.ToHtmlStringRGB(BodyGainColor);
            format.CostColorHex = ColorUtility.ToHtmlStringRGB(BodyCostColor);
            return format;
        }

        /// <summary>선택지 글자 색. 이미 고른 칸은 어두워 크림색, 나머지는 베이지 위라 어두운 색이다.</summary>
        public Color GetChoiceTextColor(EventChoiceState state)
        {
            return state == EventChoiceState.Chosen ? ChosenTextColor : ChoiceTextColor;
        }

        /// <summary>선택지 칸의 상태에 맞는 색을 고른다.</summary>
        public Color GetPanelColor(EventChoiceState state, bool hovering, bool pressed)
        {
            if (state == EventChoiceState.Chosen)
            {
                Color chosen = ChosenPanelColor;
                chosen.a = ChosenPanelColor.a * ChosenAlpha;
                return chosen;
            }

            Color baseColor = state == EventChoiceState.Blocked ? BlockedPanelColor : PanelColor;

            // 고를 수 없는 칸은 눌리지도 않으므로 밝기를 건드리지 않는다.
            if (state != EventChoiceState.Selectable)
            {
                return baseColor;
            }

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
