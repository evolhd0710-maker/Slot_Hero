using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Reward
{
    /// <summary>
    /// 보상 화면의 색과 글자.
    /// 인게임 화면 기획서 v0.2 / 10 보상 화면 의 와이어프레임에서 뽑은 값이다.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardVisualConfig", menuName = "Slot Hero/보상/표시 설정")]
    public class RewardVisualConfig : ScriptableObject
    {
        [Header("색")]
        [Tooltip("화면을 덮는 어두운 막. 와이어프레임 `11 보상 화면` 의 `배경 · 검정 50%` 다.\n" +
                 "예전에는 내보낸 그림에서 잰 #7B7B7B 를 불투명하게 깔았다. " +
                 "그 회색은 검정 50퍼센트가 아무것도 없는 바탕 위에 얹혀 보인 색일 뿐이라, " +
                 "게임에서는 뒤의 돌벽을 통째로 가렸다.")]
        [FormerlySerializedAs("DimColor")]
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.5f);
        public Color DimColor { get { return _dimColor; } set { _dimColor = value; } }

        [Tooltip("보상 박스. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("BoxColor")]
        [SerializeField] private Color _boxColor = Color.white;
        public Color BoxColor { get { return _boxColor; } set { _boxColor = value; } }

        [Tooltip("골드 줄. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("GoldRowColor")]
        [SerializeField] private Color _goldRowColor = Color.white;
        public Color GoldRowColor { get { return _goldRowColor; } set { _goldRowColor = value; } }

        [Tooltip("고르지 않은 카드. #6D6D6D")]
        [FormerlySerializedAs("CardColor")]
        [SerializeField] private Color _cardColor = new Color(0.427f, 0.427f, 0.427f, 1f);
        public Color CardColor { get { return _cardColor; } set { _cardColor = value; } }

        [Tooltip("고른 카드. #FCFCFC")]
        [FormerlySerializedAs("SelectedCardColor")]
        [SerializeField] private Color _selectedCardColor = new Color(0.988f, 0.988f, 0.988f, 1f);
        public Color SelectedCardColor { get { return _selectedCardColor; } set { _selectedCardColor = value; } }

        [Tooltip("고른 카드의 테두리. #9E2B25")]
        [FormerlySerializedAs("SelectedBorderColor")]
        [SerializeField] private Color _selectedBorderColor = new Color(0.620f, 0.169f, 0.145f, 1f);
        public Color SelectedBorderColor { get { return _selectedBorderColor; } set { _selectedBorderColor = value; } }

        [Tooltip("고른 카드 테두리 두께.")]
        [FormerlySerializedAs("SelectedBorderThickness")]
        [SerializeField] private float _selectedBorderThickness = UiScale.Px(5f);
        public float SelectedBorderThickness { get { return _selectedBorderThickness; } set { _selectedBorderThickness = value; } }

        [Tooltip("건너뛰기. 흰색이면 그린 버튼 그림(베이지) 그대로 보인다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("SkipColor")]
        [SerializeField] private Color _skipColor = Color.white;
        public Color SkipColor { get { return _skipColor; } set { _skipColor = value; } }

        [Header("글자 색")]
        [Tooltip("박스 위의 글자. 어두운 칸 위라 크림색이다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("TextColor")]
        [SerializeField] private Color _textColor = new Color(0.925f, 0.886f, 0.784f, 1f);
        public Color TextColor { get { return _textColor; } set { _textColor = value; } }

        [Tooltip("아래 버튼의 글자. 베이지 버튼 위라 어두운 색이다(2026년 10월 20일 원재).")]
        [SerializeField] private Color _skipTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color SkipTextColor { get { return _skipTextColor; } set { _skipTextColor = value; } }

        [Tooltip("고르지 않은 카드 위의 흰 글자.")]
        [FormerlySerializedAs("CardTextColor")]
        [SerializeField] private Color _cardTextColor = Color.white;
        public Color CardTextColor { get { return _cardTextColor; } set { _cardTextColor = value; } }

        [Tooltip("고른 카드 위의 검은 글자.")]
        [FormerlySerializedAs("SelectedCardTextColor")]
        [SerializeField] private Color _selectedCardTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color SelectedCardTextColor { get { return _selectedCardTextColor; } set { _selectedCardTextColor = value; } }

        [Tooltip("태그 줄처럼 한 단계 흐린 글자.")]
        [FormerlySerializedAs("SubTextColor")]
        [SerializeField] private Color _subTextColor = new Color(0.4f, 0.38f, 0.36f, 1f);
        public Color SubTextColor { get { return _subTextColor; } set { _subTextColor = value; } }

        [Header("글자 크기")]
        [Tooltip("04 화면 공통 규칙 텍스트 표의 값이다.")]
        [FormerlySerializedAs("TitleFontSize")]
        [SerializeField] private float _titleFontSize = UiScale.Px(34f);
        public float TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; } }

        [FormerlySerializedAs("GoldLabelFontSize")]
        [SerializeField] private float _goldLabelFontSize = UiScale.Px(30f);
        public float GoldLabelFontSize { get { return _goldLabelFontSize; } set { _goldLabelFontSize = value; } }
        [FormerlySerializedAs("GoldValueFontSize")]
        [SerializeField] private float _goldValueFontSize = UiScale.Px(34f);
        public float GoldValueFontSize { get { return _goldValueFontSize; } set { _goldValueFontSize = value; } }
        [FormerlySerializedAs("CardNameFontSize")]
        [SerializeField] private float _cardNameFontSize = UiScale.Px(30f);
        public float CardNameFontSize { get { return _cardNameFontSize; } set { _cardNameFontSize = value; } }
        [FormerlySerializedAs("CardTagFontSize")]
        [SerializeField] private float _cardTagFontSize = UiScale.Px(30f);
        public float CardTagFontSize { get { return _cardTagFontSize; } set { _cardTagFontSize = value; } }
        [FormerlySerializedAs("SkipFontSize")]
        [SerializeField] private float _skipFontSize = UiScale.Px(30f);
        public float SkipFontSize { get { return _skipFontSize; } set { _skipFontSize = value; } }

        [Header("글")]
        [FormerlySerializedAs("TitleText")]
        [SerializeField] private string _titleText = "보상";
        public string TitleText { get { return _titleText; } set { _titleText = value; } }
        [FormerlySerializedAs("BaseGoldText")]
        [SerializeField] private string _baseGoldText = "기본 보상";
        public string BaseGoldText { get { return _baseGoldText; } set { _baseGoldText = value; } }
        [FormerlySerializedAs("OverkillGoldText")]
        [SerializeField] private string _overkillGoldText = "오버킬 보상";
        public string OverkillGoldText { get { return _overkillGoldText; } set { _overkillGoldText = value; } }
        [Tooltip("아래 버튼. 고른 카드가 없을 때. 원재가 2026년 10월 4일에 정했다.")]
        [FormerlySerializedAs("SkipText")]
        [SerializeField] private string _skipText = "아이템 선택 건너뛰기";
        public string SkipText { get { return _skipText; } set { _skipText = value; } }

        [Tooltip("아래 버튼. 카드를 골라 두었을 때.")]
        [FormerlySerializedAs("ConfirmText")]
        [SerializeField] private string _confirmText = "보상 받기";
        public string ConfirmText { get { return _confirmText; } set { _confirmText = value; } }

        [Header("버튼")]
        [Tooltip("호버하면 밝기가 이만큼 오른다. 04 화면 공통 규칙 의 10퍼센트다.")]
        [FormerlySerializedAs("HoverBrightness")]
        [SerializeField] private float _hoverBrightness = 1.10f;
        public float HoverBrightness { get { return _hoverBrightness; } set { _hoverBrightness = value; } }

        [Tooltip("누르면 밝기가 이만큼 내린다.")]
        [FormerlySerializedAs("PressedBrightness")]
        [SerializeField] private float _pressedBrightness = 0.90f;
        public float PressedBrightness { get { return _pressedBrightness; } set { _pressedBrightness = value; } }

        /// <summary>카드 배경색. 고른 것과 아닌 것이 다르다.</summary>
        public Color GetCardColor(bool selected)
        {
            return selected ? SelectedCardColor : CardColor;
        }

        /// <summary>카드 위 글자색.</summary>
        public Color GetCardTextColor(bool selected)
        {
            return selected ? SelectedCardTextColor : CardTextColor;
        }

        /// <summary>호버와 누름을 밝기로 담는다.</summary>
        public Color GetTint(Color baseColor, bool hovering, bool pressed)
        {
            float scale = 1f;

            if (pressed)
            {
                scale = PressedBrightness;
            }
            else if (hovering)
            {
                scale = HoverBrightness;
            }

            return new Color(
                Mathf.Clamp01(baseColor.r * scale),
                Mathf.Clamp01(baseColor.g * scale),
                Mathf.Clamp01(baseColor.b * scale),
                baseColor.a);
        }

        /// <summary>고른 카드와 아닌 카드가 서로 다른 색인지.</summary>
        public bool SelectedCardStandsOut()
        {
            return SelectedCardColor != CardColor;
        }
    }
}
