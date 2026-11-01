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

        [Header("유물 진열 새로고침 연출")]
        [Tooltip("새 유물 한 칸이 암흑물질에서 형체를 갖추는 시간(초). 2026년 10월 31일 원재가 고른 연출이다.")]
        [SerializeField, Min(0f)] private float _relicFormSeconds = 0.9f;
        public float RelicFormSeconds { get { return _relicFormSeconds; } set { _relicFormSeconds = value; } }

        [Tooltip("칸마다 늦게 시작하는 시간(초). 왼쪽 칸부터 차례로 생긴다.")]
        [SerializeField, Min(0f)] private float _relicFormStagger = 0.12f;
        public float RelicFormStagger { get { return _relicFormStagger; } set { _relicFormStagger = value; } }

        [Tooltip("암흑물질 구름의 색. 보랏빛이 도는 검정이다.")]
        [SerializeField] private Color _darkMatterColor = new Color(0.16f, 0.06f, 0.24f, 0.95f);
        public Color DarkMatterColor { get { return _darkMatterColor; } set { _darkMatterColor = value; } }

        [Header("야영 연출")]
        [Tooltip("야영을 쓰면 화면이 어두워지는 시간(초). 가장 어두울 때 모닥불이 꺼진 그림으로 바뀐다(2026년 10월 31일 원재). " +
                 "2026년 11월 1일 원재가 눈이 감기는 느낌으로 서서히 어두워지게 하며 0.4 에서 0.5 로 늘렸다. 처음과 끝이 느린 곡선이다.")]
        [SerializeField, Min(0f)] private float _campDarkenSeconds = 0.5f;
        public float CampDarkenSeconds { get { return _campDarkenSeconds; } set { _campDarkenSeconds = value; } }

        [Tooltip("가장 어두운 뒤 회복과 함께 화면이 천천히 밝아지는 시간(초). " +
                 "예전에는 회복 빛 0.9초 뒤 0.4초 동안 따로 밝아졌다. 2026년 11월 1일 원재가 회복과 함께 밝아지게 하며 둘을 합친 1.2초로 했다. 전체 길이는 1.7초 그대로다. " +
                 "같은 날 원재가 초록 회복 빛과 빛 알갱이를 뺐다.")]
        [SerializeField, Min(0f)] private float _campHealSeconds = 1.2f;
        public float CampHealSeconds { get { return _campHealSeconds; } set { _campHealSeconds = value; } }

        [Tooltip("밝아지는 시간 가운데 처음 이만큼(비율)은 어두운 채로 있다. 그 뒤 남은 시간 동안 처음과 끝이 느린 곡선으로 밝아진다.")]
        [SerializeField, Range(0f, 0.9f)] private float _campBrightenDelay = 0.15f;
        public float CampBrightenDelay { get { return _campBrightenDelay; } set { _campBrightenDelay = value; } }

        [Tooltip("가장 어두울 때 화면을 덮는 검은 막의 진하기. 1 이면 완전히 검다(2026년 10월 31일 원재).")]
        [SerializeField, Range(0f, 1f)] private float _campDarkness = 1f;
        public float CampDarkness { get { return _campDarkness; } set { _campDarkness = value; } }
    }
}
