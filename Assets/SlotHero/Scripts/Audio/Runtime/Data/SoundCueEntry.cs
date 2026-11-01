using System;
using UnityEngine;

namespace SlotHero.Audio
{
    /// <summary>
    /// 효과음 하나의 설정. 어떤 소리 파일을 얼마나 크게, 얼마나 길게 내는지.
    /// `효과음과 이펙트 목록.md` 1장의 표 한 줄에 대응한다.
    /// </summary>
    [Serializable]
    public struct SoundCueEntry
    {
        [SerializeField] private SoundCue _cue;
        public SoundCue Cue { get { return _cue; } set { _cue = value; } }

        [Tooltip("재생 등급. 1 이 가장 높다. 1.6 겹칠 때 규칙이 겹친 소리 가운데 무엇을 낼지 이것으로 고른다. " +
                 "1 런의 단락, 2 물건을 얻고 잃는 결과, 3 자원, 4 장면, 5 조작이다.")]
        [SerializeField] private int _grade;
        public int Grade { get { return _grade; } set { _grade = value; } }

        [Tooltip("낼 소리 파일. 둘 이상이면 낼 때마다 하나를 고르고, 바로 앞에 낸 것은 되도록 피한다.")]
        [SerializeField] private AudioClip[] _clips;
        public AudioClip[] Clips { get { return _clips; } set { _clips = value; } }

        [Tooltip("소리 크기. 0 ~ 1. 설정의 전체와 효과음 크기를 곱하기 전 값이다.")]
        [SerializeField] private float _volume;
        public float Volume { get { return _volume; } set { _volume = value; } }

        [Tooltip("이만큼 낸 뒤 줄여 끝낸다. 0 이면 파일 끝까지 낸다.")]
        [SerializeField] private float _playSeconds;
        public float PlaySeconds { get { return _playSeconds; } set { _playSeconds = value; } }

        [Tooltip("줄여 끝내는 데 걸리는 시간. 낼 시간(PlaySeconds)이 0 이면 쓰지 않는다.")]
        [SerializeField] private float _fadeSeconds;
        public float FadeSeconds { get { return _fadeSeconds; } set { _fadeSeconds = value; } }

        public SoundCueEntry(SoundCue cue, int grade, float volume, float playSeconds, float fadeSeconds)
        {
            _cue = cue;
            _grade = grade;
            _clips = new AudioClip[0];
            _volume = volume;
            _playSeconds = playSeconds;
            _fadeSeconds = fadeSeconds;
        }
    }
}
