using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 화면이 바뀔 때 검은 막을 걷으며 새 화면을 드러낸다(페이드 인). 0.2초다(2026년 10월 31일 원재).
    /// 효과음과 이펙트 목록의 E03 화면 전환이다.
    ///
    /// 화면을 덮는 검은 막 하나를 맨 위에 두고, 화면이 바뀌면 막을 다 덮은 상태에서 투명하게 걷는다.
    /// 막은 누르기를 받지 않는다. 걷히는 짧은 동안에도 새 화면은 그대로 눌린다.
    ///
    /// 게임이 돌 때만 움직인다. 편집 모드에서는 막을 바로 걷어 둔다.
    /// 시간은 멈춤과 상관없이 흐르는 시간(unscaledDeltaTime)에 연출 속도를 곱해 잰다(`EffectClock`).
    /// </summary>
    public class ScreenFade : MonoBehaviour
    {
        /// <summary>막을 걷는 데 걸리는 초.</summary>
        public const float DefaultSeconds = 0.2f;

        [Tooltip("투명도를 바꾸는 묶음. 검은 막 오브젝트에 붙는다.")]
        [SerializeField] private CanvasGroup _group;

        [Tooltip("막을 걷는 데 걸리는 초.")]
        [SerializeField, Min(0f)] private float _seconds = DefaultSeconds;

        private bool _playing;
        private float _time;

        /// <summary>막을 걷는 중인지.</summary>
        public bool Playing
        {
            get { return _playing; }
        }

        /// <summary>막을 걷는 데 걸리는 초.</summary>
        public float Seconds
        {
            get { return _seconds; }
            set { _seconds = Mathf.Max(0f, value); }
        }

        /// <summary>씬을 만들 때 묶음을 물린다.</summary>
        public void Setup(CanvasGroup group)
        {
            _group = group;
            SetAlpha(0f);
        }

        /// <summary>막을 다 덮은 상태에서 다시 걷기 시작한다.</summary>
        public void Play()
        {
            if (!Application.isPlaying || _seconds <= 0f)
            {
                Stop();
                return;
            }

            _playing = true;
            _time = 0f;
            transform.SetAsLastSibling();
            SetAlpha(1f);
        }

        /// <summary>막을 바로 걷는다.</summary>
        public void Stop()
        {
            _playing = false;
            SetAlpha(0f);
        }

        private void Update()
        {
            if (!_playing)
            {
                return;
            }

            _time += EffectClock.Delta;
            float t = AlphaAt(_time, _seconds);
            SetAlpha(t);

            if (t <= 0f)
            {
                _playing = false;
            }
        }

        /// <summary>시작하고 elapsed 초가 지났을 때 막의 투명도. 1 에서 0 으로 고르게 줄어든다.</summary>
        public static float AlphaAt(float elapsed, float seconds)
        {
            if (seconds <= 0f)
            {
                return 0f;
            }

            return 1f - Mathf.Clamp01(elapsed / seconds);
        }

        private void SetAlpha(float alpha)
        {
            if (_group != null)
            {
                _group.alpha = alpha;
                _group.blocksRaycasts = false;
                _group.interactable = false;
            }
        }
    }
}
