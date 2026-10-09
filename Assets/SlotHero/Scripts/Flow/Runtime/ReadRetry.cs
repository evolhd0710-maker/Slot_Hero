using System;

namespace SlotHero.Flow
{
    /// <summary>
    /// 저장 파일을 잠시 읽지 못할 때 기다렸다 다시 읽는다.
    ///
    /// 백신 검사나 동기화 프로그램이 파일을 붙잡는 일은 오래 걸릴 수 있다. 그래서 바로 실패로 끝내지 않고
    /// 정해 둔 초만큼 기다린 뒤 다시 읽어 보기를 정해 둔 횟수만큼 한다.
    /// 2026년 10월 9일 원재가 "팝업을 띄운 후 여유를 두고 재시도하는 게 좋겠다" 고 정했다.
    ///
    /// 시간은 흐름이 매 프레임 `Tick` 으로 넘긴다. 코루틴을 쓰지 않아 유니티 없이 검사할 수 있다.
    /// 팝업은 모른다. 띄우고 걷는 것은 흐름이 한다.
    /// </summary>
    public class ReadRetry
    {
        private Func<bool> _readable;
        private Action _onReadable;
        private Action _onGaveUp;
        private float _delay;
        private float _wait;
        private int _left;

        /// <summary>기다리는 중인지.</summary>
        public bool IsWaiting
        {
            get { return _readable != null; }
        }

        /// <summary>남은 다시 읽기 횟수.</summary>
        public int RetriesLeft
        {
            get { return _left; }
        }

        /// <summary>
        /// 기다리기 시작한다. 지금은 읽지 못한 것으로 보고 delay 초 뒤에 처음 다시 읽는다.
        /// 읽히면 onReadable, count 번 다시 읽어도 못 읽으면 onGaveUp 을 부른다.
        /// 이미 기다리던 것이 있으면 그것은 아무것도 부르지 않고 놓는다.
        /// </summary>
        public void Begin(Func<bool> readable, float delay, int count, Action onReadable, Action onGaveUp)
        {
            _readable = readable;
            _onReadable = onReadable;
            _onGaveUp = onGaveUp;
            _delay = delay < 0f ? 0f : delay;
            _wait = _delay;
            _left = count < 0 ? 0 : count;

            // 다시 읽을 횟수가 없으면 기다리지 않고 바로 포기한다.
            if (readable == null || _left == 0)
            {
                Finish(false);
            }
        }

        /// <summary>시간을 흘린다. 기다린 시간이 차면 다시 읽어 본다.</summary>
        public void Tick(float seconds)
        {
            if (!IsWaiting)
            {
                return;
            }

            _wait -= seconds;
            if (_wait > 0f)
            {
                return;
            }

            if (_readable())
            {
                Finish(true);
                return;
            }

            _left--;
            if (_left <= 0)
            {
                Finish(false);
                return;
            }

            _wait = _delay;
        }

        /// <summary>기다리기를 그만둔다. 아무것도 부르지 않는다.</summary>
        public void Cancel()
        {
            _readable = null;
            _onReadable = null;
            _onGaveUp = null;
            _left = 0;
        }

        /// <summary>끝낸다. 부를 것을 먼저 비우고 부른다. 부른 쪽이 곧바로 다시 기다리기를 시작할 수 있다.</summary>
        private void Finish(bool readable)
        {
            Action next = readable ? _onReadable : _onGaveUp;
            Cancel();

            if (next != null)
            {
                next();
            }
        }
    }
}
