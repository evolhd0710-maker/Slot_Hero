using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Hud
{
    /// <summary>
    /// 고정 자리 요소의 상태 표현 수치.
    /// 기본값은 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 버튼과 카드의 상태를 따른다.
    ///
    /// 기획서는 밝기를 "한 단계"라고만 적었으므로 그 폭은 내가 정해 여기에 뺐다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "HudVisualConfig",
        menuName = "Slot Hero/화면 공통/고정 자리 표시 설정",
        order = 1)]
    public class HudVisualConfig : ScriptableObject
    {
        [Header("버튼 상태")]
        [Tooltip("평소 색.")]
        public Color NormalTint = Color.white;

        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 기획서의 밝기 한 단계 상승을 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기. 기획서의 밝기 한 단계 하락을 10퍼센트로 본다.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

        [Tooltip("누르고 있을 때 아래로 내리는 거리. 버튼 높이에 대한 비율. 기획서 기준 2퍼센트.")]
        [Range(0f, 0.2f)]
        public float PressedOffsetRatio = 0.02f;

        [Tooltip("비활성일 때의 투명도. 기획서 기준 40퍼센트.")]
        [Range(0f, 1f)]
        public float DisabledAlpha = 0.4f;

        [Header("자동 저장 표시")]
        [Tooltip("표시에 쓰는 문구.")]
        public string AutoSaveText = "자동 저장중...";

        [Tooltip("글자 색. 와이어프레임 기준 흰색 50퍼센트.")]
        public Color AutoSaveColor = new Color(1f, 1f, 1f, 0.5f);

        [Tooltip("자동 저장 표시의 글자 크기. 표시 칸이 200 × 40 이라 24 면 한 줄로 들어간다.")]
        public float AutoSaveFontSize = UiScale.Px(24f);

        [Tooltip("표시를 그대로 띄워 두는 시간(초).")]
        [Min(0f)]
        public float AutoSaveShowSeconds = 0.5f;

        [Tooltip("사라질 때 흐려지는 시간(초). 위 시간과 합쳐 전체 표시 시간이 된다.")]
        [Min(0f)]
        public float AutoSaveFadeSeconds = 0.5f;

        /// <summary>버튼 상태에 맞는 색을 만든다. 채도 제거는 스프라이트를 바꿔 처리한다.</summary>
        public Color GetTint(bool interactable, bool hovering, bool pressed)
        {
            if (!interactable)
            {
                Color disabled = NormalTint;
                disabled.a = NormalTint.a * DisabledAlpha;
                return disabled;
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
                NormalTint.r * brightness,
                NormalTint.g * brightness,
                NormalTint.b * brightness,
                NormalTint.a);
        }
    }
}
