using UnityEngine;
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
        public Color DimColor = new Color(0f, 0f, 0f, 0.5f);

        [Tooltip("제목 칸과 두 큰 칸, 아래 버튼. 와이어프레임 기준 D9D9D9.")]
        public Color PanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Header("분류 탭")]
        [Tooltip("고르지 않은 탭. 와이어프레임 기준 F2F0EC.")]
        public Color TabColor = new Color(0.949f, 0.941f, 0.925f, 1f);

        [Tooltip("고른 탭. 기획서는 선택된 탭만 진하게 표시하게 한다. 와이어프레임 기준 4A443C.")]
        public Color SelectedTabColor = new Color(0.290f, 0.267f, 0.235f, 1f);

        [Tooltip("고르지 않은 탭의 글자. 04 화면 공통 규칙 의 1F1B16.")]
        public Color TabTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("고른 탭의 글자. 와이어프레임 기준 흰색.")]
        public Color SelectedTabTextColor = Color.white;

        [Header("설정 항목")]
        [Tooltip("항목 칸. 와이어프레임 기준 F2F0EC.")]
        public Color ItemColor = new Color(0.949f, 0.941f, 0.925f, 1f);

        [Tooltip("항목 이름과 값의 글자 색.")]
        public Color ItemTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Header("소리 크기 줄")]
        [Tooltip("막대의 바탕.")]
        public Color SliderTrackColor = new Color(0.761f, 0.749f, 0.729f, 1f);

        [Tooltip("막대에서 찬 부분. 고른 탭과 같은 진한 색이다.")]
        public Color SliderFillColor = new Color(0.290f, 0.267f, 0.235f, 1f);

        [Tooltip("끄는 손잡이.")]
        public Color SliderHandleColor = Color.white;

        [Tooltip("음소거가 꺼져 있을 때 버튼 색. 칸 색과 같다.")]
        public Color MuteOffColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Tooltip("음소거가 켜져 있을 때 버튼 색. 고른 탭과 같은 진한 색이다.")]
        public Color MuteOnColor = new Color(0.290f, 0.267f, 0.235f, 1f);

        [Tooltip("음소거가 켜져 있을 때 버튼 글자 색.")]
        public Color MuteOnTextColor = Color.white;

        [Tooltip("음소거 버튼에 적는 글.")]
        public string MuteText = "음소거";

        [Tooltip("음소거가 켜져 있을 때 막대가 흐려지는 정도. 1 이면 그대로다.")]
        [Range(0f, 1f)]
        public float MutedSliderAlpha = 0.4f;

        [Header("드롭다운")]
        [Tooltip("펼친 목록의 바탕.")]
        public Color DropdownPanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Tooltip("지금 고른 선택지 칸. 고른 탭과 같은 진한 색이다.")]
        public Color SelectedOptionColor = new Color(0.290f, 0.267f, 0.235f, 1f);

        [Tooltip("지금 고른 선택지 글자.")]
        public Color SelectedOptionTextColor = Color.white;

        [Tooltip("드롭다운 줄의 값 뒤에 붙여 펼칠 수 있다는 것을 알리는 글.")]
        public string DropdownMark = "  ▼";

        [Header("글자")]
        [Tooltip("제목. 04 화면 공통 규칙 의 화면 주요 텍스트 40픽셀 굵게.")]
        [Min(1f)]
        public float TitleFontSize = UiScale.Px(40f);

        [Tooltip("탭과 아래 버튼. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        public float ButtonFontSize = UiScale.Px(30f);

        [Tooltip("항목 이름과 값. 04 화면 공통 규칙 의 항목 텍스트 26픽셀 굵게.")]
        [Min(1f)]
        public float ItemFontSize = UiScale.Px(26f);

        [Header("상태")]
        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

        [Header("문구")]
        [Tooltip("화면 제목.")]
        public string TitleText = "설정";

        [Tooltip("기본값 복원 버튼에 적는 글.")]
        public string RestoreText = "기본값 복원";

        [Tooltip("닫기 버튼에 적는 글.")]
        public string CloseText = "닫기";

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
