using UnityEngine;
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
        [Tooltip("표시줄 배경. 기획서는 단색만 정했고 색값은 미정이라 예시 이미지의 회색을 넣어 뒀다.")]
        public Color BackgroundColor = new Color(0.749f, 0.749f, 0.749f, 1f);

        [Header("① 플레이 시간")]
        [Tooltip("예시의 숫자 높이 48에서 되짚은 값.")]
        [Min(1f)]
        public float PlayTimeFontSize = UiScale.Px(66f);

        public Color PlayTimeColor = Color.white;

        [Tooltip("글자에 두르는 외곽선 색. 예시에는 검은 외곽선이 있다.")]
        public Color PlayTimeOutlineColor = Color.black;

        [Tooltip("외곽선 두께. 0이면 두르지 않는다.")]
        [Range(0f, 1f)]
        public float PlayTimeOutlineWidth = 0.2f;

        [Tooltip("분을 두 자리로 맞춘다. 꺼 두면 런을 시작할 때 0:00 으로 보인다.")]
        public bool PadPlayTimeMinutes;

        [Header("② 위치")]
        [Tooltip("예시의 숫자 높이 29에서 되짚은 값.")]
        [Min(1f)]
        public float StageFontSize = UiScale.Px(40f);

        public Color StageColor = Color.white;

        [Tooltip("현재 방 아이콘의 색. 맵 노드와 같은 흰 실루엣 그림을 쓰므로 여기 색이 그대로 아이콘 색이 된다. " +
                 "그림을 희게 바꾸기 전의 색 #242424 다.")]
        public Color RoomIconColor = new Color(0.141f, 0.141f, 0.141f, 1f);

        [Header("③ 체력")]
        [Tooltip("예시의 숫자 높이 32에서 되짚은 값.")]
        [Min(1f)]
        public float HealthFontSize = UiScale.Px(44f);

        [Tooltip("예시의 빨강.")]
        public Color HealthColor = new Color(1f, 0f, 0f, 1f);

        [Tooltip("체력 표기 방식. {0}은 현재 체력, {1}은 최대 체력이다.")]
        public string HealthFormat = "{0}/{1}";

        [Tooltip("체력이 늘었을 때 잠시 입히는 색.")]
        public Color HealthIncreaseColor = new Color(0.30f, 0.85f, 0.30f, 1f);

        [Tooltip("체력이 줄었을 때 잠시 입히는 색.")]
        public Color HealthDecreaseColor = new Color(1f, 0.42f, 0.42f, 1f);

        [Tooltip("색을 바꿔 두는 시간(초).")]
        [Min(0f)]
        public float HealthFlashSeconds = 0.6f;

        [Header("④ 골드")]
        [Tooltip("예시의 숫자 높이 26에서 되짚은 값.")]
        [Min(1f)]
        public float GoldFontSize = UiScale.Px(36f);

        [Tooltip("예시의 노랑. 기획서도 골드를 노란색으로 표기하라고 정했다.")]
        public Color GoldColor = new Color(0.969f, 0.741f, 0.067f, 1f);

        [Tooltip("골드 숫자 표기 방식. N0 은 천 단위마다 쉼표를 넣는다.")]
        public string GoldNumberFormat = "N0";

        [Tooltip("골드가 바뀔 때 숫자가 흘러가는 시간(초). 0이면 바로 바뀐다.")]
        [Min(0f)]
        public float GoldCountSeconds = 0.3f;

        [Header("⑤ ~ ⑦ 버튼")]
        [Tooltip("평소 색.")]
        public Color ButtonNormalTint = Color.white;

        [Tooltip("마우스를 올렸을 때 곱하는 밝기. " +
                 "인게임 화면 기획서 04 버튼 상태의 밝기 한 단계 상승을 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float ButtonHoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기. 밝기 한 단계 하락을 10퍼센트로 본다.")]
        [Range(0f, 1f)]
        public float ButtonPressedBrightness = 0.90f;

        [Tooltip("비활성일 때의 투명도. 인게임 화면 기획서 04 버튼 상태 기준 40퍼센트.")]
        [Range(0f, 1f)]
        public float ButtonDisabledAlpha = 0.4f;

        [Header("호버 팝업")]
        [Tooltip("팝업 배경 색.")]
        public Color TooltipBackgroundColor = new Color(0.12f, 0.11f, 0.09f, 0.9f);

        [Tooltip("팝업 글자 색.")]
        public Color TooltipTextColor = Color.white;

        [Tooltip("팝업 글자 크기.")]
        [Min(1f)]
        public float TooltipFontSize = UiScale.Px(24f);

        [Tooltip("팝업을 표시줄 아래로 얼마나 내릴지. 음수가 아래다.")]
        public float TooltipOffsetY = UiScale.Px(-10f);

        [Tooltip("팝업 글자 둘레의 여백. 가로는 이 값의 두 배, 세로도 두 배가 더해진다.")]
        [Min(0f)]
        public float TooltipPadding = UiScale.Px(10f);

        [Tooltip("팝업 한 줄의 최대 가로. 글이 길면 여기서 줄을 바꾼다.")]
        [Min(1f)]
        public float TooltipMaxWidth = UiScale.Px(320f);

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
