using System;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Ui;

namespace SlotHero.Sanctum.UI
{
    /// <summary>
    /// 야영을 쓸 때의 연출(2026년 10월 31일 원재). 효과음과 이펙트 목록의 E10 이다.
    ///
    /// ① 눈이 감기듯 화면이 서서히 완전히 어두워진다. 처음과 끝이 느린 곡선이다.
    ///    가장 어두울 때 모닥불이 꺼진 그림으로 바뀐다(바꾸는 일은 부르는 쪽이 넘긴다).
    /// ② 회복과 함께 화면이 천천히 밝아진다. 처음 잠깐은 어두운 채다.
    /// 2026년 11월 1일 원재가 ② 와 따로 있던 밝아지기를 회복과 함께 하게 했다. 전체 길이는 1.7초 그대로다.
    /// 같은 날 원재가 까만 화면 가운데의 초록 회복 빛과 빛 알갱이를 뺐다.
    ///
    /// 연출이 도는 동안 검은 막이 누르기를 막는다. 시간은 성소 표시 설정에 있다.
    /// 게임이 돌 때만 움직인다. 편집 모드 검사는 연출 없이 바로 끝 모습이다.
    /// </summary>
    public class CampEffectView : MonoBehaviour
    {
        private enum Phase
        {
            None,
            Darken,
            Heal,
        }

        [Tooltip("화면을 덮는 검은 막.")]
        [SerializeField] private Image _dim;

        private Phase _phase = Phase.None;
        private float _time;
        private SanctumVisualConfig _visual;
        private Action _onDarkest;

        /// <summary>연출이 도는 중인지.</summary>
        public bool Playing
        {
            get { return _phase != Phase.None; }
        }

        /// <summary>
        /// 연출을 시작한다. 가장 어두울 때 onDarkest 를 부른다.
        /// 게임이 돌지 않으면 연출 없이 바로 onDarkest 를 부른다.
        /// </summary>
        public void Play(SanctumVisualConfig visual, Action onDarkest)
        {
            _visual = visual;
            _onDarkest = onDarkest;

            if (!Application.isPlaying || visual == null || !isActiveAndEnabled)
            {
                Stop();
                return;
            }

            _phase = Phase.Darken;
            _time = 0f;
            SetDim(0f, true);

            // 연출 중 화면을 누르면 끝 모습(꺼진 모닥불, 밝은 화면)으로 넘긴다(2026년 11월 1일 원재).
            EffectSkip.Begin(this, Stop);
        }

        /// <summary>연출을 멈추고 막을 걷는다. 아직 부르지 않은 onDarkest 는 여기서 부른다.</summary>
        public void Stop()
        {
            EffectSkip.End(this);
            _phase = Phase.None;
            CallDarkest();
            SetDim(0f, false);
        }

        private void OnDisable()
        {
            if (_phase != Phase.None)
            {
                Stop();
            }
        }

        private void Update()
        {
            if (_phase == Phase.None || _visual == null)
            {
                return;
            }

            _time += EffectClock.Delta;

            switch (_phase)
            {
                case Phase.Darken:
                    {
                        float t = Progress(_visual.CampDarkenSeconds);
                        SetDim(_visual.CampDarkness * Smooth(t), true);
                        if (t >= 1f)
                        {
                            CallDarkest();
                            Next(Phase.Heal);
                        }

                        break;
                    }

                case Phase.Heal:
                    {
                        float t = Progress(_visual.CampHealSeconds);
                        SetDim(_visual.CampDarkness * (1f - GetBrighten(t, _visual.CampBrightenDelay)), true);
                        if (t >= 1f)
                        {
                            Stop();
                        }

                        break;
                    }
            }
        }

        private float Progress(float seconds)
        {
            return seconds <= 0f ? 1f : Mathf.Clamp01(_time / seconds);
        }

        private void Next(Phase phase)
        {
            _phase = phase;
            _time = 0f;
        }

        private void CallDarkest()
        {
            Action done = _onDarkest;
            _onDarkest = null;
            if (done != null)
            {
                done();
            }
        }

        private void SetDim(float alpha, bool visible)
        {
            if (_dim == null)
            {
                return;
            }

            _dim.gameObject.SetActive(visible);
            _dim.color = new Color(0f, 0f, 0f, alpha);
            _dim.raycastTarget = visible;
        }

        /// <summary>처음과 끝이 느린 곡선(0 ~ 1). 눈이 감기거나 뜨는 것처럼 서서히 시작해 서서히 멈춘다.</summary>
        public static float Smooth(float t)
        {
            float clamped = Mathf.Clamp01(t);
            return clamped * clamped * (3f - 2f * clamped);
        }

        /// <summary>
        /// 회복이 t(0 ~ 1) 일 때 화면이 밝아진 정도(0 ~ 1). delay 비율까지는 어두운 채이고 그 뒤 곡선으로 밝아져 끝에 다 밝아진다.
        /// </summary>
        public static float GetBrighten(float t, float delay)
        {
            if (delay >= 1f)
            {
                return t >= 1f ? 1f : 0f;
            }

            return Smooth((t - delay) / (1f - delay));
        }
    }
}
