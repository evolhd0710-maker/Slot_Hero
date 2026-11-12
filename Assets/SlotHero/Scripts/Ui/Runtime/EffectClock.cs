using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 연출이 쓰는 시계. 설정 일반 탭의 "연출 속도"(보통 / 빠름 / 스킵)를 연출에 건다(2026년 11월 11일 원재).
    ///
    /// 전투 밖 연출(지도 두루마리, 보상, 이벤트 선택지, 야영, 행상, 팝업, 화면 전환, 골드 오르기)은
    /// 멈춤과 상관없이 흐르는 시간(`unscaledDeltaTime`)으로 돈다. 여기에 연출 속도를 곱한 `Delta` 를 쓴다.
    /// 전투와 태그 공격 이펙트는 유니티 시간(`Time.timeScale`)으로 돈다. 흐름이 같은 배율을 `Time.timeScale` 에도 건다.
    /// 런 시계와 소리 길이는 실제 시간(`unscaledDeltaTime`)을 그대로 써서 빨라지지 않는다.
    /// </summary>
    public static class EffectClock
    {
        private static float _speed = 1f;

        /// <summary>연출 속도 배율. 보통 1 이다.</summary>
        public static float Speed
        {
            get { return _speed; }
            set { _speed = value > 0.1f ? value : 0.1f; }
        }

        /// <summary>스킵. 건너뛸 수 있는 연출(`EffectSkip`)을 누르지 않아도 바로 끝 모습으로 넘긴다. 전투 슬롯 회전도 넘긴다.</summary>
        public static bool AutoSkip { get; set; }

        /// <summary>연출이 한 프레임에 흐르는 시간. 멈춤과 상관없이 흐르는 시간에 연출 속도를 곱한 값이다.</summary>
        public static float Delta
        {
            get { return Time.unscaledDeltaTime * _speed; }
        }

        /// <summary>보통 속도로 되돌린다. 검사를 마칠 때 쓴다.</summary>
        public static void Reset()
        {
            _speed = 1f;
            AutoSkip = false;
        }
    }
}
