using System;
using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 팝업이 열릴 때 살짝 커지며 나타나고, 닫힐 때 살짝 작아지며 사라진다(2026년 10월 31일 원재).
    ///
    /// 열 때는 칸을 <see cref="DefaultFromScale"/> 에서 1 로 키우며 투명도를 0 에서 1 로 올린다.
    /// 닫을 때는 그 반대로 줄이고 흐리게 한 뒤 다 끝나면 받은 일을 한다(보통 뿌리를 끈다).
    /// 닫는 동안에는 누르기를 받지 않는다.
    ///
    /// 크기를 바꾸는 칸(target)과 투명도를 바꾸는 묶음(group)은 따로다. 어두운 막은 묶음에 함께 들어 흐려지고 크기는 그대로다.
    /// 칸의 피벗이 가운데여야 가운데에서 커진다. 팝업 칸이 그렇다.
    ///
    /// 게임이 돌 때만 움직인다. 편집 모드 검사와 그림 뽑기는 프레임이 돌지 않아 바로 끝 모습을 보인다.
    /// 시간은 멈춤과 상관없이 흐르는 시간(unscaledDeltaTime)으로 잰다.
    /// </summary>
    public class PopAnimation : MonoBehaviour
    {
        /// <summary>열리는 데 걸리는 초.</summary>
        public const float DefaultOpenSeconds = 0.15f;

        /// <summary>닫히는 데 걸리는 초.</summary>
        public const float DefaultCloseSeconds = 0.1f;

        /// <summary>열리기 시작할 때와 닫히고 난 때의 크기.</summary>
        public const float DefaultFromScale = 0.9f;

        [Tooltip("크기를 바꾸는 칸. 피벗이 가운데여야 한다.")]
        [SerializeField] private RectTransform _target;

        [Tooltip("투명도를 바꾸는 묶음. 비워 두면 같은 오브젝트에 붙여 쓴다.")]
        [SerializeField] private CanvasGroup _group;

        [SerializeField, Min(0f)] private float _openSeconds = DefaultOpenSeconds;
        [SerializeField, Min(0f)] private float _closeSeconds = DefaultCloseSeconds;
        [SerializeField, Range(0.5f, 1f)] private float _fromScale = DefaultFromScale;

        private bool _playing;
        private bool _closing;
        private float _time;
        private Action _onClosed;

        /// <summary>닫히는 연출이 도는 중인지.</summary>
        public bool Closing
        {
            get { return _playing && _closing; }
        }

        /// <summary>연출이 도는 중인지.</summary>
        public bool Playing
        {
            get { return _playing; }
        }

        /// <summary>씬을 만들 때 칸과 묶음을 물린다.</summary>
        public void Setup(RectTransform target, CanvasGroup group)
        {
            _target = target;
            _group = group;
        }

        /// <summary>열리는 연출을 시작한다. 닫히는 중이었으면 그것을 버린다.</summary>
        public void PlayOpen()
        {
            _onClosed = null;
            _closing = false;

            if (!CanAnimate())
            {
                Stop();
                return;
            }

            _playing = true;
            _time = 0f;
            SetInteractable(true);
            Apply(0f);
        }

        /// <summary>
        /// 닫히는 연출을 시작하고 끝나면 onClosed 를 부른다.
        /// 게임이 돌지 않거나 꺼져 있으면 바로 부른다.
        /// </summary>
        public void PlayClose(Action onClosed)
        {
            if (!CanAnimate())
            {
                Stop();
                if (onClosed != null)
                {
                    onClosed();
                }

                return;
            }

            _onClosed = onClosed;
            _closing = true;
            _playing = true;
            _time = 0f;
            SetInteractable(false);
            Apply(0f);
        }

        /// <summary>연출을 멈추고 끝 모습(크기 1, 투명도 1)으로 둔다. 기다리던 닫힘 일은 버린다.</summary>
        public void Stop()
        {
            _playing = false;
            _closing = false;
            _onClosed = null;
            SetInteractable(true);
            SetScale(1f);
            SetAlpha(1f);
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Update()
        {
            if (!_playing)
            {
                return;
            }

            float duration = _closing ? _closeSeconds : _openSeconds;
            _time += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(_time / duration);
            Apply(t);

            if (t < 1f)
            {
                return;
            }

            _playing = false;

            if (_closing)
            {
                Action done = _onClosed;
                _onClosed = null;
                _closing = false;
                SetInteractable(true);

                if (done != null)
                {
                    done();
                }

                // 닫힘 일이 뿌리를 끄지 않았으면 다시 보이게 둔다.
                SetScale(1f);
                SetAlpha(1f);
            }
        }

        private void Apply(float t)
        {
            SetScale(ScaleAt(t, _closing, _fromScale));
            SetAlpha(AlphaAt(t, _closing));
        }

        /// <summary>
        /// t(0 ~ 1) 때의 크기. 열 때는 처음에 빠르고 끝에 느리게 커지고, 닫을 때는 처음에 느리고 끝에 빠르게 줄어든다.
        /// </summary>
        public static float ScaleAt(float t, bool closing, float fromScale)
        {
            float clamped = Mathf.Clamp01(t);
            if (closing)
            {
                return Mathf.Lerp(1f, fromScale, clamped * clamped);
            }

            float eased = 1f - (1f - clamped) * (1f - clamped);
            return Mathf.Lerp(fromScale, 1f, eased);
        }

        /// <summary>t(0 ~ 1) 때의 투명도. 열 때는 0 에서 1, 닫을 때는 1 에서 0 이다.</summary>
        public static float AlphaAt(float t, bool closing)
        {
            float clamped = Mathf.Clamp01(t);
            return closing ? 1f - clamped : clamped;
        }

        private bool CanAnimate()
        {
            return Application.isPlaying && isActiveAndEnabled;
        }

        private void SetScale(float scale)
        {
            if (_target != null)
            {
                _target.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void SetAlpha(float alpha)
        {
            CanvasGroup group = Group();
            if (group != null)
            {
                group.alpha = alpha;
            }
        }

        private void SetInteractable(bool value)
        {
            CanvasGroup group = Group();
            if (group != null)
            {
                group.interactable = value;
                group.blocksRaycasts = true;
            }
        }

        private CanvasGroup Group()
        {
            if (_group == null && Application.isPlaying)
            {
                _group = GetComponent<CanvasGroup>();
                if (_group == null)
                {
                    _group = gameObject.AddComponent<CanvasGroup>();
                }
            }

            return _group;
        }
    }
}
