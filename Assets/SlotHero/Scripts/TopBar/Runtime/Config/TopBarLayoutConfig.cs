using UnityEngine;
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
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Tooltip("표시줄 높이. 기획서 기준 64. 가로는 화면 폭을 가득 채우므로 따로 두지 않는다.")]
        [Min(1f)]
        public float BarHeight = UiScale.Px(64f);

        [Tooltip("아이콘이 들어갈 수 있는 최대 한 변. 기획서 기준 60 이내.")]
        [Min(1f)]
        public float IconMaxSize = UiScale.Px(60f);

        [Tooltip("아이콘 위아래로 남겨 둘 최소 여유. 기획서 기준 2픽셀 이상.")]
        [Min(0f)]
        public float IconVerticalPadding = UiScale.Px(2f);

        [Header("런 요약 자리 · 화면 왼쪽 기준")]
        [Tooltip("① 플레이 시간이 시작되는 x. 예시 실측 29 를 10 단위로 옮긴 값이다.")]
        public float PlayTimeX = UiScale.Px(30f);

        [Tooltip("② 위치가 시작되는 x. 예시 실측 272 를 10 단위로 옮긴 값이다.")]
        public float LocationX = UiScale.Px(270f);

        [Tooltip("③ 체력이 시작되는 x. 예시 실측 836 을 10 단위로 옮긴 값이다.")]
        public float HealthX = UiScale.Px(840f);

        [Tooltip("④ 골드가 시작되는 x. 예시 실측 1279 를 10 단위로 옮긴 값이다.")]
        public float GoldX = UiScale.Px(1280f);

        [Header("버튼 자리 · 화면 오른쪽 기준")]
        [Tooltip("버튼 하나의 누르는 영역. 기획서 기준 64 × 64.")]
        [Min(1f)]
        public float ButtonHitSize = UiScale.Px(64f);

        [Tooltip("버튼 사이 간격. 누르는 영역 64 가 기획서 확정이라 버튼 자리 자체는 10 으로 떨어지지 않는다.")]
        [Min(0f)]
        public float ButtonSpacing = UiScale.Px(10f);

        [Tooltip("가장 오른쪽 버튼과 화면 오른쪽 끝 사이의 여백.")]
        [Min(0f)]
        public float ButtonRightMargin = UiScale.Px(10f);

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

        /// <summary>런 요약 한 칸의 자리. 기준점과 피벗을 화면 왼쪽 가운데에 둔 값이다.</summary>
        public Vector2 GetSummaryPosition(float x)
        {
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
            BarHeight = Mathf.Max(1f, BarHeight);
            IconMaxSize = Mathf.Max(1f, IconMaxSize);
            IconVerticalPadding = Mathf.Max(0f, IconVerticalPadding);
            ButtonHitSize = Mathf.Max(1f, ButtonHitSize);
            ButtonSpacing = Mathf.Max(0f, ButtonSpacing);
            ButtonRightMargin = Mathf.Max(0f, ButtonRightMargin);
        }
    }
}
