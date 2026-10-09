using UnityEngine;
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
        public Color IllustrationPlaceholderColor = new Color(1f, 1f, 1f, 0.12f);

        [Header("칸 색")]
        [Tooltip("본문 칸과 고를 수 있는 선택지 칸. 와이어프레임 기준 D9D9D9.")]
        public Color PanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Tooltip("이미 고른 선택지 칸. 와이어프레임은 흐리게 남긴다.")]
        public Color ChosenPanelColor = new Color(0.463f, 0.459f, 0.455f, 1f);

        [Tooltip("조건을 채우지 못한 선택지 칸. 기획서는 칸 색을 바꾸지 않고 부족한 항목만 붉게 적는다.")]
        public Color BlockedPanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Tooltip("이미 고른 선택지의 투명도.")]
        [Range(0f, 1f)]
        public float ChosenAlpha = 0.7f;

        [Header("글자")]
        [Tooltip("본문 글자 크기. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        public float BodyFontSize = UiScale.Px(24f);

        [Tooltip("본문이 칸을 넘칠 때 줄일 수 있는 가장 작은 글자 크기. 04 화면 공통 규칙 텍스트 표의 가장 작은 20픽셀.\n" +
                 "이벤트 기획서의 스토리는 길어 24픽셀로는 본문 칸을 넘치는 화면이 있다. " +
                 "06 문양 거래 의 첫 화면이 그렇다. 넘칠 때만 줄어들고 짧은 글은 24픽셀 그대로다.")]
        [Min(1f)]
        public float BodyMinFontSize = UiScale.Px(20f);

        [Tooltip("선택지 글자 크기. 04 화면 공통 규칙 의 항목 텍스트는 26픽셀인데\n" +
                 "글이 긴 선택지가 여럿이면 화면 아래로 넘쳐 2026년 10월 5일 원재의 요청으로 24픽셀로 줄였다.")]
        [Min(1f)]
        public float ChoiceFontSize = UiScale.Px(24f);

        [Tooltip("선택지가 쌓여 칸을 넘칠 때 줄일 수 있는 가장 작은 글자 크기. 04 화면 공통 규칙 텍스트 표의 가장 작은 20픽셀.\n" +
                 "넘칠 때만 1픽셀씩 줄이고 짧은 선택지는 24픽셀 그대로다.")]
        [Min(1f)]
        public float ChoiceMinFontSize = UiScale.Px(20f);

        [Tooltip("글자 색. 04 화면 공통 규칙 의 1F1B16.")]
        public Color TextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("본문 칸 안쪽 여백. 와이어프레임 기준 좌우 30, 위 40.")]
        public Vector2 BodyPadding = UiScale.V(30f, 40f);

        [Tooltip("선택지 칸 안쪽 좌우 여백. 와이어프레임 기준 20.")]
        public float ChoicePadding = UiScale.Px(20f);

        [Header("선택지 글")]
        [Tooltip("얻는 것에 입히는 색. 와이어프레임 기준 1E8A3C.")]
        public Color GainColor = new Color(0.118f, 0.541f, 0.235f, 1f);

        [Tooltip("잃거나 치르는 것에 입히는 색. 필요한 것이 모자랄 때와 같은 C0392B.")]
        public Color CostColor = new Color(0.753f, 0.224f, 0.169f, 1f);

        [Tooltip("필요한 것을 갖췄을 때 입히는 색.")]
        public Color RequirementMetColor = new Color(0.118f, 0.541f, 0.235f, 1f);

        [Tooltip("필요한 것을 갖추지 못했을 때 입히는 색. 와이어프레임 기준 C0392B.")]
        public Color RequirementUnmetColor = new Color(0.753f, 0.224f, 0.169f, 1f);

        [Tooltip("선택지 문구와 결과 사이에 넣는 글자. 칸 폭을 아끼려고 양옆 빈칸을 하나씩만 둔다.")]
        public string Separator = " — ";

        [Tooltip("결과가 여러 줄일 때 사이에 넣는 글자.")]
        public string LineSeparator = ", ";

        [Tooltip("결과를 알 수 없는 선택지에 적는 글자.")]
        public string HiddenMark = "?";

        [Tooltip("필요한 항목을 감싸는 꼴. {0} 자리에 내용이 들어간다.")]
        public string RequirementFormat = " [{0}]";

        [Header("선택지 상태")]
        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

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
