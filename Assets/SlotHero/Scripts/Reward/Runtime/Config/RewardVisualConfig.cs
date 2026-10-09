using UnityEngine;
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
        public Color DimColor = new Color(0f, 0f, 0f, 0.5f);

        [Tooltip("보상 박스. #CDCDCD")]
        public Color BoxColor = new Color(0.804f, 0.804f, 0.804f, 1f);

        [Tooltip("골드 줄. #D9D9D9")]
        public Color GoldRowColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Tooltip("고르지 않은 카드. #6D6D6D")]
        public Color CardColor = new Color(0.427f, 0.427f, 0.427f, 1f);

        [Tooltip("고른 카드. #FCFCFC")]
        public Color SelectedCardColor = new Color(0.988f, 0.988f, 0.988f, 1f);

        [Tooltip("고른 카드의 테두리. #9E2B25")]
        public Color SelectedBorderColor = new Color(0.620f, 0.169f, 0.145f, 1f);

        [Tooltip("고른 카드 테두리 두께.")]
        public float SelectedBorderThickness = UiScale.Px(5f);

        [Tooltip("건너뛰기. #D9D9D9")]
        public Color SkipColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Header("글자 색")]
        [Tooltip("박스 위의 검은 글자. #1F1B16")]
        public Color TextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("고르지 않은 카드 위의 흰 글자.")]
        public Color CardTextColor = Color.white;

        [Tooltip("고른 카드 위의 검은 글자.")]
        public Color SelectedCardTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("태그 줄처럼 한 단계 흐린 글자.")]
        public Color SubTextColor = new Color(0.4f, 0.38f, 0.36f, 1f);

        [Header("글자 크기")]
        [Tooltip("04 화면 공통 규칙 텍스트 표의 값이다.")]
        public float TitleFontSize = UiScale.Px(34f);

        public float GoldLabelFontSize = UiScale.Px(30f);
        public float GoldValueFontSize = UiScale.Px(34f);
        public float CardNameFontSize = UiScale.Px(30f);
        public float CardTagFontSize = UiScale.Px(24f);
        public float SkipFontSize = UiScale.Px(30f);

        [Header("글")]
        public string TitleText = "보상";
        public string BaseGoldText = "기본 보상";
        public string OverkillGoldText = "오버킬 보상";
        [Tooltip("아래 버튼. 고른 카드가 없을 때. 원재가 2026년 10월 4일에 정했다.")]
        public string SkipText = "아이템 선택 건너뛰기";

        [Tooltip("아래 버튼. 카드를 골라 두었을 때.")]
        public string ConfirmText = "보상 받기";

        [Header("버튼")]
        [Tooltip("호버하면 밝기가 이만큼 오른다. 04 화면 공통 규칙 의 10퍼센트다.")]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르면 밝기가 이만큼 내린다.")]
        public float PressedBrightness = 0.90f;

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
