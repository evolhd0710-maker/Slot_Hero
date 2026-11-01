using UnityEngine;

namespace SlotHero.Audio
{
    /// <summary>
    /// 효과음을 낸다. 흐름이 알림을 받아 `Play` 를 부르면 겹칠 때 규칙(`SoundArbiter`)을 거쳐 소리를 낸다.
    /// `효과음과 이펙트 목록.md` 1장의 효과음과 1.6 겹칠 때 규칙을 맡는다.
    ///
    /// 소리 자리(`AudioSource`)는 동시에 울리는 수만큼 처음 낼 때 만든다.
    /// 소리 크기는 설정 화면의 전체와 효과음 크기를 곱한 값이다(인게임 화면 기획서 07 설정 화면의 사운드 탭).
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private SoundConfig _config;
        public SoundConfig Config { get { return _config; } set { _config = value; } }

        /// 소리 자리 하나. 지금 무엇을 내고 있고 언제 줄이기 시작하는지 든다.
        private sealed class Voice
        {
            public AudioSource Source;
            public SoundCue Cue;
            public int Grade;
            public float BaseVolume;
            public float FadeStart;
            public float FadeSeconds;
        }

        private SoundArbiter _arbiter;
        private readonly SoundRandom _random = new SoundRandom(1);
        private Voice[] _voices;
        private int[] _lastClip;

        /// 소리마다 이번 묶음에서 쓰라고 정해 받은 파일 자리. 정하지 않았으면 -1 이고 무작위로 고른다.
        private int[] _requestedClip;

        /// 실제 흐른 시간. 연출 속도(`Time.timeScale`)를 바꿔도 소리 길이가 같도록 `unscaledDeltaTime` 을 센다.
        private float _clock;

        /// 설정의 전체 크기와 효과음 크기를 곱한 값. 0 ~ 1.
        private float _volume = 1f;

        /// <summary>지금 소리 크기. 0 ~ 1. 게임 씬 검사가 본다.</summary>
        public float Volume
        {
            get { return _volume; }
        }

        /// <summary>
        /// 겹칠 때 규칙을 지나 마지막으로 고른 소리. 게임 씬 검사가 본다.
        /// 음소거여서 실제로 울리지 않았어도 고른 것은 적는다.
        /// </summary>
        public SoundCue LastChosen { get; private set; }

        /// <summary>
        /// 마지막으로 고른 소리에서 쓴 파일의 자리. 게임 씬 검사가 본다.
        /// 음소거여도 고른 자리는 적는다. 파일이 없으면 -1 이다.
        /// </summary>
        public int LastClipIndex { get; private set; }

        /// <summary>소리를 낸다. 같은 프레임에 들어온 것끼리 겹칠 때 규칙으로 하나를 고른다.</summary>
        public void Play(SoundCue cue)
        {
            Play(cue, -1);
        }

        /// <summary>
        /// 소리를 낸다. 파일이 여럿이면 `clipIndex` 자리의 파일을 쓴다. -1 이면 무작위로 고른다.
        /// 한 묶음에 같은 소리가 여러 번 오면 마지막에 정한 자리를 쓴다.
        /// </summary>
        public void Play(SoundCue cue, int clipIndex)
        {
            EnsureParts();

            int index = _config != null ? _config.IndexOf(cue) : -1;
            if (index >= 0 && index < _requestedClip.Length)
            {
                _requestedClip[index] = clipIndex;
            }

            _arbiter.Request(cue);
        }

        /// <summary>
        /// S23 행상 구매 소리를 낸다. 치른 골드의 구간(`SoundConfig.PurchaseGoldSteps`)에 맞는 파일을 쓴다.
        /// 기본값은 30 미만이 동전 한 닢, 80 미만이 부드러운 바닥의 자루, 그 이상이 나무 바닥의 자루다(2026년 10월 31일 원재).
        /// </summary>
        public void PlayPurchase(int gold)
        {
            Play(SoundCue.MerchantPurchase, _config != null ? _config.PurchaseClipIndex(gold) : -1);
        }

        /// <summary>
        /// 설정 화면의 소리 크기를 적용한다. 크기는 0 ~ 100 이다. 음소거면 0 으로 본다.
        /// 울리고 있는 소리에도 바로 적용한다.
        /// </summary>
        public void SetVolume(float master, bool masterMuted, float sfx, bool sfxMuted)
        {
            float a = masterMuted ? 0f : Mathf.Clamp01(master / 100f);
            float b = sfxMuted ? 0f : Mathf.Clamp01(sfx / 100f);
            _volume = a * b;
            ApplyVolumes();
        }

        /// <summary>
        /// 받아 둔 요청을 지금 처리한다. 매 프레임 `LateUpdate` 가 부르고, 게임 씬 검사가 프레임을 기다리지 않고 부른다.
        /// </summary>
        public void Flush()
        {
            EnsureParts();
            if (!_arbiter.HasPending)
            {
                return;
            }

            SoundCue cue = _arbiter.Resolve(_clock);
            if (cue != SoundCue.None)
            {
                LastChosen = cue;
                StartVoice(cue);
            }

            // 정해 받은 파일 자리는 이번 묶음에만 쓴다.
            for (int i = 0; i < _requestedClip.Length; i++)
            {
                _requestedClip[i] = -1;
            }
        }

        /// <summary>
        /// 시간을 흘린다. 게임 씬 검사는 프레임이 돌지 않아 시간이 멈춰 있다.
        /// 앞 검사의 소리와 한 묶음으로 묶이지 않게 소리를 내기 전에 부른다.
        /// </summary>
        public void AdvanceClock(float seconds)
        {
            _clock += Mathf.Max(0f, seconds);
        }

        private void LateUpdate()
        {
            _clock += Time.unscaledDeltaTime;
            Flush();
            UpdateFades();
        }

        private void EnsureParts()
        {
            if (_arbiter == null)
            {
                _arbiter = new SoundArbiter(_config);
            }

            if (_lastClip == null)
            {
                int count = _config != null && _config.Cues != null ? _config.Cues.Count : 0;
                _lastClip = new int[count];
                _requestedClip = new int[count];
                for (int i = 0; i < count; i++)
                {
                    _lastClip[i] = -1;
                    _requestedClip[i] = -1;
                }
            }
        }

        private void EnsureVoices()
        {
            if (_voices != null)
            {
                return;
            }

            int count = _config != null ? Mathf.Max(1, _config.MaxVoiceCount) : 1;
            _voices = new Voice[count];

            for (int i = 0; i < count; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                _voices[i] = new Voice { Source = source };
            }
        }

        private void StartVoice(SoundCue cue)
        {
            int index = _config.IndexOf(cue);
            SoundCueEntry entry = _config.Cues[index];
            LastClipIndex = -1;

            if (entry.Clips == null || entry.Clips.Length == 0)
            {
                return;
            }

            int requested = _requestedClip[index];
            int pick = requested >= 0
                ? Mathf.Min(requested, entry.Clips.Length - 1)
                : _random.Pick(entry.Clips.Length, _lastClip[index]);
            LastClipIndex = pick;

            AudioClip clip = entry.Clips[pick];
            if (clip == null || _volume <= 0f)
            {
                return;
            }

            EnsureVoices();

            int[] grades = new int[_voices.Length];
            for (int i = 0; i < _voices.Length; i++)
            {
                grades[i] = _voices[i].Source.isPlaying ? _voices[i].Grade : 0;
            }

            int slot = SoundArbiter.ChooseVoice(grades, entry.Grade);
            if (slot < 0)
            {
                return;
            }

            _lastClip[index] = pick;

            Voice voice = _voices[slot];
            voice.Source.Stop();
            voice.Source.clip = clip;
            voice.Cue = cue;
            voice.Grade = entry.Grade;
            voice.BaseVolume = Mathf.Clamp01(entry.Volume);
            voice.FadeStart = entry.PlaySeconds > 0f ? _clock + entry.PlaySeconds : -1f;
            voice.FadeSeconds = entry.FadeSeconds;
            voice.Source.volume = voice.BaseVolume * _volume;
            voice.Source.Play();
        }

        /// <summary>정한 시간만큼 낸 소리를 줄여 끝낸다.</summary>
        private void UpdateFades()
        {
            if (_voices == null)
            {
                return;
            }

            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];
                if (voice.FadeStart < 0f || !voice.Source.isPlaying || _clock < voice.FadeStart)
                {
                    continue;
                }

                float t = voice.FadeSeconds > 0f ? (_clock - voice.FadeStart) / voice.FadeSeconds : 1f;
                if (t >= 1f)
                {
                    voice.Source.Stop();
                    voice.FadeStart = -1f;
                    continue;
                }

                voice.Source.volume = voice.BaseVolume * _volume * (1f - t);
            }
        }

        private void ApplyVolumes()
        {
            if (_voices == null)
            {
                return;
            }

            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];
                if (voice.Source.isPlaying && (voice.FadeStart < 0f || _clock < voice.FadeStart))
                {
                    voice.Source.volume = voice.BaseVolume * _volume;
                }
            }
        }
    }
}
