using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.TopBar
{
    /// <summary>
    /// 상단 표시줄의 규격과 각 항목의 자리.
    /// 기본값은 상단 UI 바 기획서 v0.2 의 03 표시줄 규격 과 04 표시 항목 을 따르고,
    /// 기획서가 수치를 적지 않은 자리는 상단 표시줄 예시 이미지에서 재 왔다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TopBarLayoutConfig",
        menuName = "Slot Hero/상단 표시줄/표시줄 자리 설정",
        order = 0)]
    public class TopBarLayoutConfig : ScriptableObject
    {
        [Header("표시줄")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Tooltip("표시줄 높이. 기획서 기준 64. 가로는 화면 폭을 가득 채우므로 따로 두지 않는다.")]
        [Min(1f)]
        [FormerlySerializedAs("BarHeight")]
        [SerializeField] private float _barHeight = UiScale.Px(64f);
        public float BarHeight { get { return _barHeight; } set { _barHeight = value; } }

        [Tooltip("표시줄 아래에 붙는 장식 띠(쇠 띠와 크림색 선)의 높이. 표시줄 64 영역 바깥 아래에 둔다. " +
            "64 안은 단색이라 아이콘과 글자가 세로 가운데에 놓여 보인다(2026년 10월 20일 원재).")]
        [SerializeField] private float _trimHeight = UiScale.Px(12f);
        public float TrimHeight { get { return _trimHeight; } set { _trimHeight = value; } }

        [Tooltip("아이콘 한 변. 기획서는 60 이내, 확정 수치는 40 × 40 이다. " +
            "표시줄 그림의 아래 12 픽셀은 쇠 띠와 크림색 선이라 60 이면 아이콘이 띠를 넘는다. 40 으로 둔다(2026년 10월 20일 원재). " +
            "2026년 10월 25일 원재가 아이콘을 전체적으로 더 크게 하라고 해 50 이다. 띠는 지금 표시줄 아래 바깥에 붙어 위아래 7 씩 남는다.")]
        [Min(1f)]
        [FormerlySerializedAs("IconMaxSize")]
        [SerializeField] private float _iconMaxSize = UiScale.Px(50f);
        public float IconMaxSize { get { return _iconMaxSize; } set { _iconMaxSize = value; } }

        [Tooltip("아이콘 위아래로 남겨 둘 최소 여유. 기획서 기준 2픽셀 이상.")]
        [Min(0f)]
        [FormerlySerializedAs("IconVerticalPadding")]
        [SerializeField] private float _iconVerticalPadding = UiScale.Px(2f);
        public float IconVerticalPadding { get { return _iconVerticalPadding; } set { _iconVerticalPadding = value; } }

        [Header("런 요약 자리 · 화면 왼쪽 기준")]
        [Tooltip("① 플레이 시간이 시작되는 x. 예시 실측 29 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("PlayTimeX")]
        [SerializeField] private float _playTimeX = UiScale.Px(30f);
        public float PlayTimeX { get { return _playTimeX; } set { _playTimeX = value; } }

        [Tooltip("② 위치가 시작되는 x. 예시 실측 272 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("LocationX")]
        [SerializeField] private float _locationX = UiScale.Px(270f);
        public float LocationX { get { return _locationX; } set { _locationX = value; } }

        [Tooltip("③ 체력이 시작되는 x. 예시 실측 836 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("HealthX")]
        [SerializeField] private float _healthX = UiScale.Px(840f);
        public float HealthX { get { return _healthX; } set { _healthX = value; } }

        [Tooltip("④ 골드가 시작되는 x. 예시 실측 1279 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("GoldX")]
        [SerializeField] private float _goldX = UiScale.Px(1280f);
        public float GoldX { get { return _goldX; } set { _goldX = value; } }

        [Header("버튼 자리 · 화면 오른쪽 기준")]
        [Tooltip("버튼 하나의 누르는 영역. 기획서 기준 64 × 64.")]
        [Min(1f)]
        [FormerlySerializedAs("ButtonHitSize")]
        [SerializeField] private float _buttonHitSize = UiScale.Px(64f);
        public float ButtonHitSize { get { return _buttonHitSize; } set { _buttonHitSize = value; } }

        [Tooltip("버튼 사이 간격. 누르는 영역 64 가 기획서 확정이라 버튼 자리 자체는 10 으로 떨어지지 않는다.")]
        [Min(0f)]
        [FormerlySerializedAs("ButtonSpacing")]
        [SerializeField] private float _buttonSpacing = UiScale.Px(10f);
        public float ButtonSpacing { get { return _buttonSpacing; } set { _buttonSpacing = value; } }

        [Tooltip("가장 오른쪽 버튼과 화면 오른쪽 끝 사이의 여백.")]
        [Min(0f)]
        [FormerlySerializedAs("ButtonRightMargin")]
        [SerializeField] private float _buttonRightMargin = UiScale.Px(10f);
        public float ButtonRightMargin { get { return _buttonRightMargin; } set { _buttonRightMargin = value; } }

        /// <summary>버튼은 지도, 설정, 런 종료 순서로 왼쪽에서 오른쪽으로 놓는다.</summary>
        public const int ButtonCount = 3;

        /// <summary>
        /// 버튼의 자리. 기준점과 피벗을 화면 오른쪽 가운데에 둔 값이다.
        /// 가장 오른쪽 버튼이 런 종료이므로 오른쪽에서부터 거꾸로 센다.
        /// </summary>
        public Vector2 GetButtonPosition(TopBarButtonKind kind)
        {
            int index = (int)kind;
            int fromRight = ButtonCount - 1 - index;
            float x = -(ButtonRightMargin + fromRight * (ButtonHitSize + ButtonSpacing));
            return new Vector2(x, 0f);
        }

        /// <summary>아이콘 한 변의 크기. 표시줄 높이에서 위아래 여유를 뺀 값을 넘지 않는다.</summary>
        public float GetIconSize()
        {
            float limit = BarHeight - IconVerticalPadding * 2f;
            return IconMaxSize < limit ? IconMaxSize : limit;
        }

        /// <summary>왼쪽 항목들이 서로 겹치지 않고 오른쪽 버튼 자리까지 넘지 않는지.</summary>
        public bool SummaryOrderIsValid()
        {
            if (PlayTimeX >= LocationX || LocationX >= HealthX || HealthX >= GoldX)
            {
                return false;
            }

            float leftmostButton = ReferenceResolution.x
                                   - ButtonRightMargin
                                   - ButtonCount * ButtonHitSize
                                   - (ButtonCount - 1) * ButtonSpacing;

            return GoldX < leftmostButton;
        }

        private void OnValidate()
        {
            _barHeight = Mathf.Max(1f, _barHeight);
            _iconMaxSize = Mathf.Max(1f, _iconMaxSize);
            _iconVerticalPadding = Mathf.Max(0f, _iconVerticalPadding);
            _buttonHitSize = Mathf.Max(1f, _buttonHitSize);
            _buttonSpacing = Mathf.Max(0f, _buttonSpacing);
            _buttonRightMargin = Mathf.Max(0f, _buttonRightMargin);
        }
    }
}
