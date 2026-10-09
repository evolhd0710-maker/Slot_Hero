using UnityEngine;

namespace SlotHero.Flow
{
    /// <summary>
    /// 런 시간을 재고 멈춘다.
    ///
    /// 설정 화면이 떠 있는 동안만 멈춘다. 팝업, 현재 빌드, 지도, 고르기 화면에서는 흐른다.
    /// 2026년 10월 6일 원재가 정했다. 무엇이 떠 있을 때 멈출지는 `GameFlowController.IsClockPaused` 가 정한다.
    ///
    /// 멈출지 말지는 화면이 열리고 닫힐 때 알리는 대신 **매 프레임 지금 상태를 읽어** 정한다.
    /// 여는 자리와 닫는 자리를 하나하나 이으면 한 군데만 빠뜨려도 시계가 영영 멈춰 있는다.
    /// 지금 무엇이 열려 있는지를 그때그때 보면 어긋날 자리가 없다.
    /// </summary>
    public class RunClock
    {
        private float _seconds;
        private bool _running;
        private bool _paused;

        /// <summary>런을 시작하고 흐른 시간. 초다.</summary>
        public float Seconds
        {
            get { return _seconds; }
        }

        /// <summary>지금 시간이 흐르고 있는지.</summary>
        public bool IsTicking
        {
            get { return _running && !IsPaused; }
        }

        /// <summary>멈춰 있는지.</summary>
        public bool IsPaused
        {
            get { return _paused; }
        }

        /// <summary>런을 시작하거나 이어 한다. 저장된 시간에서 이어 센다.</summary>
        public void Start(float fromSeconds)
        {
            _seconds = fromSeconds;
            _running = true;
        }

        /// <summary>런이 끝났다. 멈춤도 함께 푼다.</summary>
        public void Stop()
        {
            _running = false;
            _paused = false;
        }

        /// <summary>한 프레임 흐른다. 멈춰 있으면 아무 일도 없다.</summary>
        public void Tick(float deltaSeconds)
        {
            if (!IsTicking)
            {
                return;
            }

            _seconds += deltaSeconds;
        }

        /// <summary>
        /// 멈출지 말지를 정한다.
        /// 흐름 조종기가 매 프레임 지금 열려 있는 화면을 보고 넘겨 준다.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _paused = paused;
        }
    }
}
