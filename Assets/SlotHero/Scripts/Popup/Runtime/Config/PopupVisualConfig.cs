using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("DimColor")]
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.6f);
        public Color DimColor { get { return _dimColor; } set { _dimColor = value; } }

        [Tooltip("팝업 본체. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("PopupColor")]
        [SerializeField] private Color _popupColor = Color.white;
        public Color PopupColor { get { return _popupColor; } set { _popupColor = value; } }

        [Tooltip("제목 칸. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("TitlePanelColor")]
        [SerializeField] private Color _titlePanelColor = Color.white;
        public Color TitlePanelColor { get { return _titlePanelColor; } set { _titlePanelColor = value; } }

        [Tooltip("본문 칸. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("BodyPanelColor")]
        [SerializeField] private Color _bodyPanelColor = Color.white;
        public Color BodyPanelColor { get { return _bodyPanelColor; } set { _bodyPanelColor = value; } }

        [Header("버튼")]
        [Tooltip("평범한 버튼. 흰색이면 그린 버튼 그림(베이지) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("ButtonColor")]
        [SerializeField] private Color _buttonColor = Color.white;
        public Color ButtonColor { get { return _buttonColor; } set { _buttonColor = value; } }

        [Tooltip("평범한 버튼의 글자. 04 화면 공통 규칙 의 4A443C.")]
        [FormerlySerializedAs("ButtonTextColor")]
        [SerializeField] private Color _buttonTextColor = new Color(0.290f, 0.267f, 0.235f, 1f);
        public Color ButtonTextColor { get { return _buttonTextColor; } set { _buttonTextColor = value; } }

        [Tooltip("되돌릴 수 없는 조작의 버튼. 04 화면 공통 규칙 의 9E2B25.")]
        [FormerlySerializedAs("EmphasizedButtonColor")]
        [SerializeField] private Color _emphasizedButtonColor = new Color(0.620f, 0.169f, 0.145f, 1f);
        public Color EmphasizedButtonColor { get { return _emphasizedButtonColor; } set { _emphasizedButtonColor = value; } }

        [Tooltip("그 버튼의 글자. 04 화면 공통 규칙 의 FFFFFF.")]
        [FormerlySerializedAs("EmphasizedButtonTextColor")]
        [SerializeField] private Color _emphasizedButtonTextColor = Color.white;
        public Color EmphasizedButtonTextColor { get { return _emphasizedButtonTextColor; } set { _emphasizedButtonTextColor = value; } }

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

        [Tooltip("비활성일 때의 투명도. 04 화면 공통 규칙 기준 40퍼센트.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("DisabledAlpha")]
        [SerializeField] private float _disabledAlpha = 0.4f;
        public float DisabledAlpha { get { return _disabledAlpha; } set { _disabledAlpha = value; } }

        [Header("글자")]
        [Tooltip("팝업 제목. 04 화면 공통 규칙 의 40픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("TitleFontSize")]
        [SerializeField] private float _titleFontSize = UiScale.Px(40f);
        public float TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; } }

        [Tooltip("팝업 본문. 04 화면 공통 규칙 의 24픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("BodyFontSize")]
        [SerializeField] private float _bodyFontSize = UiScale.Px(24f);
        public float BodyFontSize { get { return _bodyFontSize; } set { _bodyFontSize = value; } }

        [Tooltip("버튼 글자. 04 화면 공통 규칙 의 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("ButtonFontSize")]
        [SerializeField] private float _buttonFontSize = UiScale.Px(30f);
        public float ButtonFontSize { get { return _buttonFontSize; } set { _buttonFontSize = value; } }

        [Tooltip("제목과 본문 글자. 어두운 칸 위라 크림색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("TextColor")]
        [SerializeField] private Color _textColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color TextColor { get { return _textColor; } set { _textColor = value; } }

        [Header("아이템 상세 오버레이")]
        [Tooltip("오버레이 본체. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("ItemDetailColor")]
        [SerializeField] private Color _itemDetailColor = Color.white;
        public Color ItemDetailColor { get { return _itemDetailColor; } set { _itemDetailColor = value; } }

        [Tooltip("오버레이 안쪽 칸. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("ItemDetailPanelColor")]
        [SerializeField] private Color _itemDetailPanelColor = Color.white;
        public Color ItemDetailPanelColor { get { return _itemDetailPanelColor; } set { _itemDetailPanelColor = value; } }

        [Tooltip("아이템 이름. 04 화면 공통 규칙 의 팝업 항목 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("ItemNameFontSize")]
        [SerializeField] private float _itemNameFontSize = UiScale.Px(30f);
        public float ItemNameFontSize { get { return _itemNameFontSize; } set { _itemNameFontSize = value; } }

        [Tooltip("가치와 희귀도. 04 화면 공통 규칙 의 팝업 값 표시 24픽셀 굵게.")]
        [Min(1f)]
        [FormerlySerializedAs("ItemValueFontSize")]
        [SerializeField] private float _itemValueFontSize = UiScale.Px(24f);
        public float ItemValueFontSize { get { return _itemValueFontSize; } set { _itemValueFontSize = value; } }

        [Tooltip("내용. 04 화면 공통 규칙 의 팝업 본문 텍스트 24픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("ItemBodyFontSize")]
        [SerializeField] private float _itemBodyFontSize = UiScale.Px(24f);
        public float ItemBodyFontSize { get { return _itemBodyFontSize; } set { _itemBodyFontSize = value; } }

        [Tooltip("플레이버 텍스트. 04 화면 공통 규칙 의 팝업 보조 텍스트 20픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("ItemFlavorFontSize")]
        [SerializeField] private float _itemFlavorFontSize = UiScale.Px(20f);
        public float ItemFlavorFontSize { get { return _itemFlavorFontSize; } set { _itemFlavorFontSize = value; } }

        [Tooltip("플레이버 텍스트 색. 어두운 칸 위의 옅은 크림색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("ItemFlavorColor")]
        [SerializeField] private Color _itemFlavorColor = new Color(0.737f, 0.694f, 0.612f, 1f);
        public Color ItemFlavorColor { get { return _itemFlavorColor; } set { _itemFlavorColor = value; } }

        [Tooltip("플레이버 텍스트를 감싸는 꼴. 기획서는 양옆에 따옴표를 두고 기울여 적게 한다.")]
        [FormerlySerializedAs("ItemFlavorFormat")]
        [SerializeField] private string _itemFlavorFormat = "\"{0}\"";
        public string ItemFlavorFormat { get { return _itemFlavorFormat; } set { _itemFlavorFormat = value; } }

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
