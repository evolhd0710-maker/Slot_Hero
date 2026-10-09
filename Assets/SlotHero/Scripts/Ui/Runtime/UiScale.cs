using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 화면 좌표의 배율.
    ///
    /// 기획서와 와이어프레임은 1920 × 1080 으로 그려져 있다.
    /// 설정 에셋에는 그 값에 `Factor` 를 곱한 값이 들어가고, 캔버스 기준 해상도도 같은 배율을 탄다.
    ///
    /// **지금 배율은 1 이다. 화면 좌표가 FHD 1920 × 1080 그대로다.**
    /// 2026년 9월 27일에 고해상도 그림을 줄이지 않고 놓으려고 12 로 올렸다가,
    /// 2026년 10월 8일 원재가 "FHD 사이즈에 맞춰 다시 줄이자" 고 정해 1 로 되돌렸다.
    ///
    /// 설정 에셋의 값은 `UiScale.Px(410f)` 처럼 적는 방식을 그대로 둔다.
    /// 괄호 안이 와이어프레임에서 잰 값이라 기획서와 코드를 계속 맞춰 볼 수 있고,
    /// 다시 배율을 바꿀 일이 생기면 `Factor` 하나만 고치면 된다.
    /// </summary>
    public static class UiScale
    {
        /// <summary>와이어프레임 값에 곱하는 배율.</summary>
        public const float Factor = 1f;

        /// <summary>
        /// 지금 쓰는 배율.
        ///
        /// 검사 프로그램이 `UseWireframeUnits()` 나 `UseFactorForTests()` 로 바꾼다.
        /// **게임에서는 건드리지 않는다.**
        /// </summary>
        private static float _factor = Factor;

        /// <summary>지금 쓰는 배율.</summary>
        public static float Current
        {
            get { return _factor; }
        }

        /// <summary>와이어프레임 값을 설정 값으로 바꾼다.</summary>
        public static float Px(float wireframeValue)
        {
            return wireframeValue * _factor;
        }

        /// <summary>와이어프레임 값 둘을 설정 값으로 바꾼다.</summary>
        public static Vector2 V(float wireframeX, float wireframeY)
        {
            return new Vector2(wireframeX * _factor, wireframeY * _factor);
        }

        /// <summary>설정 값을 와이어프레임 값으로 되돌린다.</summary>
        public static float ToWireframe(float scaledValue)
        {
            return _factor == 0f ? scaledValue : scaledValue / _factor;
        }

        /// <summary>
        /// 배율을 1 로 둔다. **검사 프로그램만 부른다.**
        /// 설정 에셋의 기본값이 와이어프레임 실측값 그대로 나온다.
        /// </summary>
        public static void UseWireframeUnits()
        {
            _factor = 1f;
        }

        /// <summary>
        /// 배율을 아무 값으로나 바꾼다. **검사 프로그램만 부른다.**
        /// 배율이 1 이면 `Px` 를 빠뜨려도 티가 안 나므로, 다른 배율로 만들어 견줘 볼 때 쓴다.
        /// </summary>
        public static void UseFactorForTests(float factor)
        {
            _factor = factor;
        }

        /// <summary>배율을 원래대로 돌린다.</summary>
        public static void UseGameUnits()
        {
            _factor = Factor;
        }
    }
}
