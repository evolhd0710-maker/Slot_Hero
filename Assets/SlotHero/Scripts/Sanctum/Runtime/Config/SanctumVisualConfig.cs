using UnityEngine;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 성소와 행상 화면의 색과 연출 수치.
    /// 기본값은 성소 기획서 v0.2 의 04 야영, 08 화면 구성, 13 가격 · 공통 규칙 기준이다.
    ///
    /// 물건과 바닥의 자리 잡기는 화면마다 고정이라 프리팹에서 맡고,
    /// 여기에는 상태에 따라 코드가 바꿔야 하는 값만 둔다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SanctumVisualConfig",
        menuName = "Slot Hero/성소/성소 화면 표시 설정",
        order = 2)]
    public class SanctumVisualConfig : ScriptableObject
    {
        [Header("가격 표기")]
        [Tooltip("살 수 있을 때의 가격 글자 색.")]
        public Color PriceColor = new Color(1f, 0.85f, 0.25f);

        [Tooltip("골드가 모자랄 때의 가격 글자 색. 13 가격 · 공통 규칙 의 붉은 색 표기.")]
        public Color PriceNotAffordableColor = new Color(0.85f, 0.18f, 0.15f);

        [Header("진열 자리")]
        [Tooltip("살 수 있는 자리.")]
        public Color SlotNormalTint = Color.white;

        [Tooltip("골드가 모자라 살 수 없는 자리. 가격만 붉게 하고 물건은 그대로 두려면 흰색으로 둔다.")]
        public Color SlotNotAffordableTint = Color.white;

        [Tooltip("이미 팔린 자리.")]
        public Color SlotSoldTint = new Color(0.45f, 0.44f, 0.42f, 0.5f);

        [Tooltip("팔린 자리를 아예 감춘다. 끄면 위의 색으로 흐리게 남겨 둔다.")]
        public bool HideSoldSlots = true;

        [Tooltip("커서를 올린 자리를 키우는 비율. 인게임 화면 기획서 04 화면 공통 규칙 의 카드 상태를 따라 5퍼센트.")]
        public float HoverScale = 1.05f;

        [Header("야영")]
        [Tooltip("쓸 수 있는 야영.")]
        public Color CampAvailableTint = Color.white;

        [Tooltip("이미 쓴 야영. 04 야영 의 사용 표시 는 회색으로 바꾸고 선택을 막는다.")]
        public Color CampUsedTint = new Color(0.45f, 0.44f, 0.42f, 1f);

        [Tooltip("야영 사용 피드백으로 체력 수치에 잠시 입히는 색.")]
        public Color CampHealFeedbackColor = new Color(0.35f, 0.85f, 0.35f);

        [Tooltip("체력 수치를 그 색으로 두는 시간(초).")]
        [Min(0f)]
        public float CampHealFeedbackSeconds = 1.2f;
    }
}
