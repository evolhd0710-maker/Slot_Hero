using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.RunResult
{
    /// <summary>
    /// 런 종료 결과 화면의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기는
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RunResultVisualConfig",
        menuName = "Slot Hero/런 종료 결과/런 종료 결과 화면 표시 설정",
        order = 2)]
    public class RunResultVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("배경 그림이 없을 때 깔리는 색. 배경은 배경 요청 기획서 소관이라 검정만 둔다.")]
        [FormerlySerializedAs("BackgroundColor")]
        [SerializeField] private Color _backgroundColor = Color.black;
        public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; } }

        [Tooltip("여섯 칸과 타이틀로 버튼의 배경. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다. 타이틀로 버튼은 베이지 그림이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("PanelColor")]
        [SerializeField] private Color _panelColor = Color.white;
        public Color PanelColor { get { return _panelColor; } set { _panelColor = value; } }

        [Header("글자 색")]
        [Tooltip("칸 안의 글자. 어두운 칸 위라 크림색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("TextColor")]
        [SerializeField] private Color _textColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color TextColor { get { return _textColor; } set { _textColor = value; } }

        [Tooltip("타이틀로 버튼의 글자. 베이지 버튼 위라 어두운 색이다(2026년 10월 20일 원재).")]
        [SerializeField] private Color _buttonTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color ButtonTextColor { get { return _buttonTextColor; } set { _buttonTextColor = value; } }

        [Tooltip("패배. 어두운 칸 위에서 읽히게 강조 텍스트 9E2B25 를 밝혔다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("DefeatColor")]
        [SerializeField] private Color _defeatColor = new Color(0.851f, 0.298f, 0.247f, 1f);
        public Color DefeatColor { get { return _defeatColor; } set { _defeatColor = value; } }

        [Tooltip("클리어. 기본 글자색과 같은 크림색이다. 정해지면 바꾼다.")]
        [FormerlySerializedAs("ClearColor")]
        [SerializeField] private Color _clearColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color ClearColor { get { return _clearColor; } set { _clearColor = value; } }

        [Header("글자 크기")]
        [Tooltip("결과. 04 화면 공통 규칙 의 강조 텍스트 64픽셀 굵게. 와이어프레임 실측도 같다.")]
        [Min(1f)]
        [FormerlySerializedAs("OutcomeFontSize")]
        [SerializeField] private float _outcomeFontSize = UiScale.Px(64f);
        public float OutcomeFontSize { get { return _outcomeFontSize; } set { _outcomeFontSize = value; } }

        [Tooltip("도달 지점. 와이어프레임에서 잰 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("LocationFontSize")]
        [SerializeField] private float _locationFontSize = UiScale.Px(30f);
        public float LocationFontSize { get { return _locationFontSize; } set { _locationFontSize = value; } }

        [Tooltip("칸 안의 소제목. 04 화면 공통 규칙 의 항목 텍스트 26픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("HeadingFontSize")]
        [SerializeField] private float _headingFontSize = UiScale.Px(26f);
        public float HeadingFontSize { get { return _headingFontSize; } set { _headingFontSize = value; } }

        [Tooltip("칸 안의 본문. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("BodyFontSize")]
        [SerializeField] private float _bodyFontSize = UiScale.Px(24f);
        public float BodyFontSize { get { return _bodyFontSize; } set { _bodyFontSize = value; } }

        [Tooltip("타이틀로 버튼. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("ButtonFontSize")]
        [SerializeField] private float _buttonFontSize = UiScale.Px(30f);
        public float ButtonFontSize { get { return _buttonFontSize; } set { _buttonFontSize = value; } }

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
        [Tooltip("패배로 끝났을 때 적는 글. 기획서 본문은 사망, 와이어프레임은 패배다.")]
        [FormerlySerializedAs("DefeatText")]
        [SerializeField] private string _defeatText = "패배";
        public string DefeatText { get { return _defeatText; } set { _defeatText = value; } }

        [Tooltip("클리어로 끝났을 때 적는 글.")]
        [FormerlySerializedAs("ClearText")]
        [SerializeField] private string _clearText = "클리어";
        public string ClearText { get { return _clearText; } set { _clearText = value; } }

        [Tooltip("타이틀로 버튼에 적는 글.")]
        [FormerlySerializedAs("TitleButtonText")]
        [SerializeField] private string _titleButtonText = "타이틀로";
        public string TitleButtonText { get { return _titleButtonText; } set { _titleButtonText = value; } }

        [Header("줄을 잇는 방법")]
        [Tooltip("한 줄 안에서 항목을 잇는 글자. 와이어프레임 기준 가운뎃점이다.")]
        [FormerlySerializedAs("Separator")]
        [SerializeField] private string _separator = " · ";
        public string Separator { get { return _separator; } set { _separator = value; } }

        [Tooltip("도달 지점의 스테이지. {0} 자리에 번호가 들어간다.")]
        [FormerlySerializedAs("StageFormat")]
        [SerializeField] private string _stageFormat = "스테이지 {0}";
        public string StageFormat { get { return _stageFormat; } set { _stageFormat = value; } }

        [Tooltip("도달 지점의 단계. {0} 자리에 번호가 들어간다.")]
        [FormerlySerializedAs("RoomFormat")]
        [SerializeField] private string _roomFormat = "{0}번째 방";
        public string RoomFormat { get { return _roomFormat; } set { _roomFormat = value; } }

        [Header("최종 구성 소제목")]
        [Tooltip("문양 소제목. {0} 자리에 전체 장수가 들어간다.")]
        [FormerlySerializedAs("SymbolHeadingFormat")]
        [SerializeField] private string _symbolHeadingFormat = "문양 {0}장";
        public string SymbolHeadingFormat { get { return _symbolHeadingFormat; } set { _symbolHeadingFormat = value; } }

        [Tooltip("태그 소제목.")]
        [FormerlySerializedAs("TagHeadingText")]
        [SerializeField] private string _tagHeadingText = "태그 정보";
        public string TagHeadingText { get { return _tagHeadingText; } set { _tagHeadingText = value; } }

        [Tooltip("유물 소제목.")]
        [FormerlySerializedAs("RelicHeadingText")]
        [SerializeField] private string _relicHeadingText = "유물";
        public string RelicHeadingText { get { return _relicHeadingText; } set { _relicHeadingText = value; } }

        [Tooltip("코인 소제목.")]
        [FormerlySerializedAs("CoinHeadingText")]
        [SerializeField] private string _coinHeadingText = "코인";
        public string CoinHeadingText { get { return _coinHeadingText; } set { _coinHeadingText = value; } }

        [Tooltip("문양 한 종류의 표기. {0} 이름, {1} 개수.")]
        [FormerlySerializedAs("SymbolEntryFormat")]
        [SerializeField] private string _symbolEntryFormat = "{0} {1}";
        public string SymbolEntryFormat { get { return _symbolEntryFormat; } set { _symbolEntryFormat = value; } }

        [Tooltip("태그 하나의 표기. {0} 이름, {1} 장수.")]
        [FormerlySerializedAs("TagEntryFormat")]
        [SerializeField] private string _tagEntryFormat = "{0} {1}";
        public string TagEntryFormat { get { return _tagEntryFormat; } set { _tagEntryFormat = value; } }

        /// <summary>결과에 맞는 글.</summary>
        public string GetOutcomeText(RunOutcome outcome)
        {
            return outcome == RunOutcome.Clear ? ClearText : DefeatText;
        }

        /// <summary>결과에 맞는 색.</summary>
        public Color GetOutcomeColor(RunOutcome outcome)
        {
            return outcome == RunOutcome.Clear ? ClearColor : DefeatColor;
        }

        /// <summary>줄 하나의 글자 크기. 소제목은 굵고 크다.</summary>
        public float GetLineFontSize(bool strong)
        {
            return strong ? HeadingFontSize : BodyFontSize;
        }

        /// <summary>타이틀로 버튼의 색.</summary>
        public Color GetButtonColor(bool hovering, bool pressed)
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
                PanelColor.r * brightness,
                PanelColor.g * brightness,
                PanelColor.b * brightness,
                PanelColor.a);
        }
    }
}
