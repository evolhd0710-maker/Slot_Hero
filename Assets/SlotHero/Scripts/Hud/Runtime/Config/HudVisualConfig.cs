using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("NormalTint")]
        [SerializeField] private Color _normalTint = Color.white;
        public Color NormalTint { get { return _normalTint; } set { _normalTint = value; } }

        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 기획서의 밝기 한 단계 상승을 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        [FormerlySerializedAs("HoverBrightness")]
        [SerializeField] private float _hoverBrightness = 1.10f;
        public float HoverBrightness { get { return _hoverBrightness; } set { _hoverBrightness = value; } }

        [Tooltip("누르고 있을 때 곱하는 밝기. 기획서의 밝기 한 단계 하락을 10퍼센트로 본다.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("PressedBrightness")]
        [SerializeField] private float _pressedBrightness = 0.90f;
        public float PressedBrightness { get { return _pressedBrightness; } set { _pressedBrightness = value; } }

        [Tooltip("누르고 있을 때 아래로 내리는 거리. 버튼 높이에 대한 비율. 기획서 기준 2퍼센트.")]
        [Range(0f, 0.2f)]
        [FormerlySerializedAs("PressedOffsetRatio")]
        [SerializeField] private float _pressedOffsetRatio = 0.02f;
        public float PressedOffsetRatio { get { return _pressedOffsetRatio; } set { _pressedOffsetRatio = value; } }

        [Tooltip("비활성일 때의 투명도. 기획서 기준 40퍼센트.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("DisabledAlpha")]
        [SerializeField] private float _disabledAlpha = 0.4f;
        public float DisabledAlpha { get { return _disabledAlpha; } set { _disabledAlpha = value; } }

        [Header("자동 저장 표시")]
        [Tooltip("표시에 쓰는 문구.")]
        [FormerlySerializedAs("AutoSaveText")]
        [SerializeField] private string _autoSaveText = "자동 저장중...";
        public string AutoSaveText { get { return _autoSaveText; } set { _autoSaveText = value; } }

        [Tooltip("글자 색. 와이어프레임 기준 흰색 50퍼센트.")]
        [FormerlySerializedAs("AutoSaveColor")]
        [SerializeField] private Color _autoSaveColor = new Color(1f, 1f, 1f, 0.5f);
        public Color AutoSaveColor { get { return _autoSaveColor; } set { _autoSaveColor = value; } }

        [Tooltip("자동 저장 표시의 글자 크기. 표시 칸이 200 × 40 이라 24 면 한 줄로 들어간다.")]
        [FormerlySerializedAs("AutoSaveFontSize")]
        [SerializeField] private float _autoSaveFontSize = UiScale.Px(24f);
        public float AutoSaveFontSize { get { return _autoSaveFontSize; } set { _autoSaveFontSize = value; } }

        [Tooltip("표시를 그대로 띄워 두는 시간(초).")]
        [Min(0f)]
        [FormerlySerializedAs("AutoSaveShowSeconds")]
        [SerializeField] private float _autoSaveShowSeconds = 0.5f;
        public float AutoSaveShowSeconds { get { return _autoSaveShowSeconds; } set { _autoSaveShowSeconds = value; } }

        [Tooltip("사라질 때 흐려지는 시간(초). 위 시간과 합쳐 전체 표시 시간이 된다.")]
        [Min(0f)]
        [FormerlySerializedAs("AutoSaveFadeSeconds")]
        [SerializeField] private float _autoSaveFadeSeconds = 0.5f;
        public float AutoSaveFadeSeconds { get { return _autoSaveFadeSeconds; } set { _autoSaveFadeSeconds = value; } }

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
