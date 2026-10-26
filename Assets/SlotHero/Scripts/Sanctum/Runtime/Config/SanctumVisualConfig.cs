using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("PriceColor")]
        [SerializeField] private Color _priceColor = new Color(1f, 0.85f, 0.25f);
        public Color PriceColor { get { return _priceColor; } set { _priceColor = value; } }

        [Tooltip("골드가 모자랄 때의 가격 글자 색. 13 가격 · 공통 규칙 의 붉은 색 표기.")]
        [FormerlySerializedAs("PriceNotAffordableColor")]
        [SerializeField] private Color _priceNotAffordableColor = new Color(0.85f, 0.18f, 0.15f);
        public Color PriceNotAffordableColor { get { return _priceNotAffordableColor; } set { _priceNotAffordableColor = value; } }

        [Header("진열 자리")]
        [Tooltip("살 수 있는 자리.")]
        [FormerlySerializedAs("SlotNormalTint")]
        [SerializeField] private Color _slotNormalTint = Color.white;
        public Color SlotNormalTint { get { return _slotNormalTint; } set { _slotNormalTint = value; } }

        [Tooltip("골드가 모자라 살 수 없는 자리. 가격만 붉게 하고 물건은 그대로 두려면 흰색으로 둔다.")]
        [FormerlySerializedAs("SlotNotAffordableTint")]
        [SerializeField] private Color _slotNotAffordableTint = Color.white;
        public Color SlotNotAffordableTint { get { return _slotNotAffordableTint; } set { _slotNotAffordableTint = value; } }

        [Tooltip("이미 팔린 자리.")]
        [FormerlySerializedAs("SlotSoldTint")]
        [SerializeField] private Color _slotSoldTint = new Color(0.45f, 0.44f, 0.42f, 0.5f);
        public Color SlotSoldTint { get { return _slotSoldTint; } set { _slotSoldTint = value; } }

        [Tooltip("팔린 자리를 아예 감춘다. 끄면 위의 색으로 흐리게 남겨 둔다.")]
        [FormerlySerializedAs("HideSoldSlots")]
        [SerializeField] private bool _hideSoldSlots = true;
        public bool HideSoldSlots { get { return _hideSoldSlots; } set { _hideSoldSlots = value; } }

        [Header("야영")]
        [Tooltip("쓸 수 있는 야영.")]
        [FormerlySerializedAs("CampAvailableTint")]
        [SerializeField] private Color _campAvailableTint = Color.white;
        public Color CampAvailableTint { get { return _campAvailableTint; } set { _campAvailableTint = value; } }

        [Tooltip("이미 쓴 야영. 04 야영 의 사용 표시 는 회색으로 바꾸고 선택을 막는다.")]
        [FormerlySerializedAs("CampUsedTint")]
        [SerializeField] private Color _campUsedTint = new Color(0.45f, 0.44f, 0.42f, 1f);
        public Color CampUsedTint { get { return _campUsedTint; } set { _campUsedTint = value; } }
    }
}
