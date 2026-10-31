using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.TopBar
{
    /// <summary>
    /// 상단 표시줄의 색, 글자, 갱신 연출 수치.
    /// 색과 글자 크기는 상단 표시줄 예시 이미지에서 재 왔다.
    /// 갱신 연출은 상단 UI 바 기획서 v0.2 / 05 정보 갱신 시점 을 따른다.
    ///
    /// 기획서의 글자 크기는 포인트로 적혀 있어 화면 픽셀과 바로 맞지 않는다.
    /// 여기에는 예시에서 잰 숫자 높이로 되짚은 픽셀 크기를 넣어 두고,
    /// 실제 폰트를 넣은 뒤에 다시 맞춘다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TopBarVisualConfig",
        menuName = "Slot Hero/상단 표시줄/표시줄 표시 설정",
        order = 1)]
    public class TopBarVisualConfig : ScriptableObject
    {
        [Header("배경")]
        [Tooltip("표시줄 배경 그림(`표시줄_바탕`)에 곱하는 색. 흰색이면 그린 그림 그대로다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("BackgroundColor")]
        [SerializeField] private Color _backgroundColor = Color.white;
        public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; } }

        [Header("① 플레이 시간")]
        [Tooltip("예시의 숫자 높이 48에서 되짚은 값은 66 이었다. 표시줄 그림의 아래 12 픽셀이 쇠 띠라 66 이면 숫자가 띠를 넘어 체력 숫자와 같은 44 로 줄였다(2026년 10월 20일 원재).")]
        [Min(1f)]
        [FormerlySerializedAs("PlayTimeFontSize")]
        [SerializeField] private float _playTimeFontSize = UiScale.Px(44f);
        public float PlayTimeFontSize { get { return _playTimeFontSize; } set { _playTimeFontSize = value; } }

        [FormerlySerializedAs("PlayTimeColor")]
        [SerializeField] private Color _playTimeColor = Color.white;
        public Color PlayTimeColor { get { return _playTimeColor; } set { _playTimeColor = value; } }

        [Tooltip("글자에 두르는 외곽선 색. 예시에는 검은 외곽선이 있다.")]
        [FormerlySerializedAs("PlayTimeOutlineColor")]
        [SerializeField] private Color _playTimeOutlineColor = Color.black;
        public Color PlayTimeOutlineColor { get { return _playTimeOutlineColor; } set { _playTimeOutlineColor = value; } }

        [Tooltip("외곽선 두께. 0이면 두르지 않는다.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("PlayTimeOutlineWidth")]
        [SerializeField] private float _playTimeOutlineWidth = 0.2f;
        public float PlayTimeOutlineWidth { get { return _playTimeOutlineWidth; } set { _playTimeOutlineWidth = value; } }

        [Tooltip("분을 두 자리로 맞춘다. 꺼 두면 런을 시작할 때 0:00 으로 보인다.")]
        [FormerlySerializedAs("PadPlayTimeMinutes")]
        [SerializeField] private bool _padPlayTimeMinutes;
        public bool PadPlayTimeMinutes { get { return _padPlayTimeMinutes; } set { _padPlayTimeMinutes = value; } }

        [Header("② 위치")]
        [Tooltip("예시의 숫자 높이 29에서 되짚은 값.")]
        [Min(1f)]
        [FormerlySerializedAs("StageFontSize")]
        [SerializeField] private float _stageFontSize = UiScale.Px(40f);
        public float StageFontSize { get { return _stageFontSize; } set { _stageFontSize = value; } }

        [FormerlySerializedAs("StageColor")]
        [SerializeField] private Color _stageColor = Color.white;
        public Color StageColor { get { return _stageColor; } set { _stageColor = value; } }

        [Tooltip("현재 방 아이콘의 색. 맵 노드와 같은 흑백 그림을 쓴다. 색을 덧입히지 않으므로 흰색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("RoomIconColor")]
        [SerializeField] private Color _roomIconColor = Color.white;
        public Color RoomIconColor { get { return _roomIconColor; } set { _roomIconColor = value; } }

        [Tooltip("현재 방 아이콘의 안쪽 무늬(몬스터 눈, 성소 모양) 색. 흰 실루엣 위에 어둡게 얹어 무늬가 보이게 한다. " +
            "지도 노드가 무늬를 양피지 색으로 얹는 것과 같은 방식이다(2026년 10월 25일 원재).")]
        [SerializeField] private Color _roomIconDetailColor = new Color(0.157f, 0.137f, 0.118f, 1f);
        public Color RoomIconDetailColor { get { return _roomIconDetailColor; } set { _roomIconDetailColor = value; } }

        [Header("③ 체력")]
        [Tooltip("예시의 숫자 높이 32에서 되짚은 값.")]
        [Min(1f)]
        [FormerlySerializedAs("HealthFontSize")]
        [SerializeField] private float _healthFontSize = UiScale.Px(44f);
        public float HealthFontSize { get { return _healthFontSize; } set { _healthFontSize = value; } }

        [Tooltip("예시의 빨강.")]
        [FormerlySerializedAs("HealthColor")]
        [SerializeField] private Color _healthColor = new Color(1f, 0f, 0f, 1f);
        public Color HealthColor { get { return _healthColor; } set { _healthColor = value; } }

        [Tooltip("체력 표기 방식. {0}은 현재 체력, {1}은 최대 체력이다.")]
        [FormerlySerializedAs("HealthFormat")]
        [SerializeField] private string _healthFormat = "{0}/{1}";
        public string HealthFormat { get { return _healthFormat; } set { _healthFormat = value; } }

        [Tooltip("체력이 늘었을 때 잠시 입히는 색.")]
        [FormerlySerializedAs("HealthIncreaseColor")]
        [SerializeField] private Color _healthIncreaseColor = new Color(0.30f, 0.85f, 0.30f, 1f);
        public Color HealthIncreaseColor { get { return _healthIncreaseColor; } set { _healthIncreaseColor = value; } }

        [Tooltip("체력이 줄었을 때 잠시 입히는 색.")]
        [FormerlySerializedAs("HealthDecreaseColor")]
        [SerializeField] private Color _healthDecreaseColor = new Color(1f, 0.42f, 0.42f, 1f);
        public Color HealthDecreaseColor { get { return _healthDecreaseColor; } set { _healthDecreaseColor = value; } }

        [Tooltip("색을 바꿔 두는 시간(초).")]
        [Min(0f)]
        [FormerlySerializedAs("HealthFlashSeconds")]
        [SerializeField] private float _healthFlashSeconds = 0.6f;
        public float HealthFlashSeconds { get { return _healthFlashSeconds; } set { _healthFlashSeconds = value; } }

        [Header("④ 골드")]
        [Tooltip("예시의 숫자 높이 26에서 되짚은 값.")]
        [Min(1f)]
        [FormerlySerializedAs("GoldFontSize")]
        [SerializeField] private float _goldFontSize = UiScale.Px(36f);
        public float GoldFontSize { get { return _goldFontSize; } set { _goldFontSize = value; } }

        [Tooltip("예시의 노랑. 기획서도 골드를 노란색으로 표기하라고 정했다.")]
        [FormerlySerializedAs("GoldColor")]
        [SerializeField] private Color _goldColor = new Color(0.969f, 0.741f, 0.067f, 1f);
        public Color GoldColor { get { return _goldColor; } set { _goldColor = value; } }

        [Tooltip("골드 숫자 표기 방식. N0 은 천 단위마다 쉼표를 넣는다.")]
        [FormerlySerializedAs("GoldNumberFormat")]
        [SerializeField] private string _goldNumberFormat = "N0";
        public string GoldNumberFormat { get { return _goldNumberFormat; } set { _goldNumberFormat = value; } }

        [Tooltip("골드가 바뀔 때 숫자가 흘러가는 시간(초). 0이면 바로 바뀐다.")]
        [Min(0f)]
        [FormerlySerializedAs("GoldCountSeconds")]
        [SerializeField] private float _goldCountSeconds = 0.3f;
        public float GoldCountSeconds { get { return _goldCountSeconds; } set { _goldCountSeconds = value; } }

        [Header("⑤ ~ ⑦ 버튼")]
        [Tooltip("평소 색.")]
        [FormerlySerializedAs("ButtonNormalTint")]
        [SerializeField] private Color _buttonNormalTint = Color.white;
        public Color ButtonNormalTint { get { return _buttonNormalTint; } set { _buttonNormalTint = value; } }

        [Tooltip("마우스를 올렸을 때 곱하는 밝기. " +
                 "인게임 화면 기획서 04 버튼 상태의 밝기 한 단계 상승을 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        [FormerlySerializedAs("ButtonHoverBrightness")]
        [SerializeField] private float _buttonHoverBrightness = 1.10f;
        public float ButtonHoverBrightness { get { return _buttonHoverBrightness; } set { _buttonHoverBrightness = value; } }

        [Tooltip("누르고 있을 때 곱하는 밝기. 밝기 한 단계 하락을 10퍼센트로 본다.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("ButtonPressedBrightness")]
        [SerializeField] private float _buttonPressedBrightness = 0.90f;
        public float ButtonPressedBrightness { get { return _buttonPressedBrightness; } set { _buttonPressedBrightness = value; } }

        [Tooltip("비활성일 때의 투명도. 인게임 화면 기획서 04 버튼 상태 기준 40퍼센트.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("ButtonDisabledAlpha")]
        [SerializeField] private float _buttonDisabledAlpha = 0.4f;
        public float ButtonDisabledAlpha { get { return _buttonDisabledAlpha; } set { _buttonDisabledAlpha = value; } }

        [Header("호버 팝업")]
        [Tooltip("팝업 배경 색.")]
        [FormerlySerializedAs("TooltipBackgroundColor")]
        [SerializeField] private Color _tooltipBackgroundColor = new Color(0.12f, 0.11f, 0.09f, 0.9f);
        public Color TooltipBackgroundColor { get { return _tooltipBackgroundColor; } set { _tooltipBackgroundColor = value; } }

        [Tooltip("팝업 글자 색.")]
        [FormerlySerializedAs("TooltipTextColor")]
        [SerializeField] private Color _tooltipTextColor = Color.white;
        public Color TooltipTextColor { get { return _tooltipTextColor; } set { _tooltipTextColor = value; } }

        [Tooltip("팝업 글자 크기.")]
        [Min(1f)]
        [FormerlySerializedAs("TooltipFontSize")]
        [SerializeField] private float _tooltipFontSize = UiScale.Px(24f);
        public float TooltipFontSize { get { return _tooltipFontSize; } set { _tooltipFontSize = value; } }

        [Tooltip("팝업을 표시줄 아래로 얼마나 내릴지. 음수가 아래다.")]
        [FormerlySerializedAs("TooltipOffsetY")]
        [SerializeField] private float _tooltipOffsetY = UiScale.Px(-10f);
        public float TooltipOffsetY { get { return _tooltipOffsetY; } set { _tooltipOffsetY = value; } }

        [Tooltip("팝업 글자 둘레의 여백. 가로는 이 값의 두 배, 세로도 두 배가 더해진다.")]
        [Min(0f)]
        [FormerlySerializedAs("TooltipPadding")]
        [SerializeField] private float _tooltipPadding = UiScale.Px(10f);
        public float TooltipPadding { get { return _tooltipPadding; } set { _tooltipPadding = value; } }

        [Tooltip("팝업 한 줄의 최대 가로. 글이 길면 여기서 줄을 바꾼다.")]
        [Min(1f)]
        [FormerlySerializedAs("TooltipMaxWidth")]
        [SerializeField] private float _tooltipMaxWidth = UiScale.Px(320f);
        public float TooltipMaxWidth { get { return _tooltipMaxWidth; } set { _tooltipMaxWidth = value; } }

        /// <summary>버튼 상태에 맞는 색을 만든다.</summary>
        public Color GetButtonTint(bool interactable, bool hovering, bool pressed)
        {
            if (!interactable)
            {
                Color disabled = ButtonNormalTint;
                disabled.a = ButtonNormalTint.a * ButtonDisabledAlpha;
                return disabled;
            }

            float brightness = 1f;
            if (pressed)
            {
                brightness = ButtonPressedBrightness;
            }
            else if (hovering)
            {
                brightness = ButtonHoverBrightness;
            }

            return new Color(
                ButtonNormalTint.r * brightness,
                ButtonNormalTint.g * brightness,
                ButtonNormalTint.b * brightness,
                ButtonNormalTint.a);
        }
    }
}
