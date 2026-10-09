using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Popup
{
    /// <summary>
    /// 팝업과 오버레이의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기와 색은
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 그대로 쓴다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PopupVisualConfig",
        menuName = "Slot Hero/팝업/팝업 표시 설정",
        order = 1)]
    public class PopupVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("뒤 화면을 덮는 막. 기획서 기준 검정 60퍼센트.")]
        public Color DimColor = new Color(0f, 0f, 0f, 0.6f);

        [Tooltip("팝업 본체. 와이어프레임 기준 FCFCFC.")]
        public Color PopupColor = new Color(0.988f, 0.988f, 0.988f, 1f);

        [Tooltip("제목 칸. 와이어프레임 기준 F2F0EC.")]
        public Color TitlePanelColor = new Color(0.949f, 0.941f, 0.925f, 1f);

        [Tooltip("본문 칸. 와이어프레임 기준 D9D9D9.")]
        public Color BodyPanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Header("버튼")]
        [Tooltip("평범한 버튼. 와이어프레임 기준 F2F0EC.")]
        public Color ButtonColor = new Color(0.949f, 0.941f, 0.925f, 1f);

        [Tooltip("평범한 버튼의 글자. 04 화면 공통 규칙 의 4A443C.")]
        public Color ButtonTextColor = new Color(0.290f, 0.267f, 0.235f, 1f);

        [Tooltip("되돌릴 수 없는 조작의 버튼. 04 화면 공통 규칙 의 9E2B25.")]
        public Color EmphasizedButtonColor = new Color(0.620f, 0.169f, 0.145f, 1f);

        [Tooltip("그 버튼의 글자. 04 화면 공통 규칙 의 FFFFFF.")]
        public Color EmphasizedButtonTextColor = Color.white;

        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

        [Tooltip("비활성일 때의 투명도. 04 화면 공통 규칙 기준 40퍼센트.")]
        [Range(0f, 1f)]
        public float DisabledAlpha = 0.4f;

        [Header("글자")]
        [Tooltip("팝업 제목. 04 화면 공통 규칙 의 40픽셀 굵게.")]
        [Min(1f)]
        public float TitleFontSize = UiScale.Px(40f);

        [Tooltip("팝업 본문. 04 화면 공통 규칙 의 24픽셀.")]
        [Min(1f)]
        public float BodyFontSize = UiScale.Px(24f);

        [Tooltip("버튼 글자. 04 화면 공통 규칙 의 30픽셀 굵게.")]
        [Min(1f)]
        public float ButtonFontSize = UiScale.Px(30f);

        [Tooltip("어두운 글자 색. 04 화면 공통 규칙 의 1F1B16.")]
        public Color TextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Header("아이템 상세 오버레이")]
        [Tooltip("오버레이 본체. 와이어프레임 기준 F1EFEA.")]
        public Color ItemDetailColor = new Color(0.945f, 0.937f, 0.918f, 1f);

        [Tooltip("오버레이 안쪽 칸. 와이어프레임 기준 E4E0D8.")]
        public Color ItemDetailPanelColor = new Color(0.894f, 0.878f, 0.847f, 1f);

        [Tooltip("아이템 이름. 04 화면 공통 규칙 의 팝업 항목 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        public float ItemNameFontSize = UiScale.Px(30f);

        [Tooltip("가치와 희귀도. 04 화면 공통 규칙 의 팝업 값 표시 24픽셀 굵게.")]
        [Min(1f)]
        public float ItemValueFontSize = UiScale.Px(24f);

        [Tooltip("내용. 04 화면 공통 규칙 의 팝업 본문 텍스트 24픽셀.")]
        [Min(1f)]
        public float ItemBodyFontSize = UiScale.Px(24f);

        [Tooltip("플레이버 텍스트. 04 화면 공통 규칙 의 팝업 보조 텍스트 20픽셀.")]
        [Min(1f)]
        public float ItemFlavorFontSize = UiScale.Px(20f);

        [Tooltip("플레이버 텍스트 색. 04 화면 공통 규칙 의 6E665A.")]
        public Color ItemFlavorColor = new Color(0.431f, 0.400f, 0.353f, 1f);

        [Tooltip("플레이버 텍스트를 감싸는 꼴. 기획서는 양옆에 따옴표를 두고 기울여 적게 한다.")]
        public string ItemFlavorFormat = "\"{0}\"";

        /// <summary>버튼 상태에 맞는 색을 만든다.</summary>
        public Color GetButtonColor(bool emphasized, bool interactable, bool hovering, bool pressed)
        {
            Color baseColor = emphasized ? EmphasizedButtonColor : ButtonColor;

            if (!interactable)
            {
                baseColor.a *= DisabledAlpha;
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

        /// <summary>버튼 글자 색.</summary>
        public Color GetButtonTextColor(bool emphasized)
        {
            return emphasized ? EmphasizedButtonTextColor : ButtonTextColor;
        }
    }
}
