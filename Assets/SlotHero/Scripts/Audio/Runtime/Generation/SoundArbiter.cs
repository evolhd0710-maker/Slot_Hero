using System.Collections.Generic;

namespace SlotHero.Audio
{
    /// <summary>
    /// 겹친 효과음 가운데 무엇을 낼지 고른다. `효과음과 이펙트 목록.md` 1.6 겹칠 때 규칙 1 ~ 3 이다.
    /// 유니티 소리 기능을 쓰지 않아 검증에서 따로 돌린다.
    ///
    /// 묶음은 한 프레임에 들어온 요청에서 시작한다. 한 번 누른 것에서 나는 알림은 같은 프레임에 함께 오기 때문이다.
    /// 소리를 낸 뒤 묶음 시간(0.1초) 안에 늦게 온 요청은 같은 묶음으로 보고, 등급이 더 높을 때만 낸다.
    /// 첫 소리를 0.1초 기다렸다 내면 누를 때마다 소리가 늦어 이렇게 했다.
    /// </summary>
    public sealed class SoundArbiter
    {
        private readonly SoundConfig _config;
        private readonly List<SoundCue> _pending = new List<SoundCue>();
        private readonly Dictionary<SoundCue, float> _lastPlayed = new Dictionary<SoundCue, float>();

        private bool _hasBundle;
        private float _bundleTime;
        private int _bundleGrade;

        public SoundArbiter(SoundConfig config)
        {
            _config = config;
        }

        /// <summary>이번 프레임에 낼 소리를 받아 둔다. 설정에 없는 소리는 받지 않는다.</summary>
        public void Request(SoundCue cue)
        {
            if (_config != null && _config.IndexOf(cue) >= 0)
            {
                _pending.Add(cue);
            }
        }

        /// <summary>받아 둔 요청이 있는지.</summary>
        public bool HasPending
        {
            get { return _pending.Count > 0; }
        }

        /// <summary>
        /// 받아 둔 요청에서 낼 소리 하나를 고르고 요청을 비운다. 낼 것이 없으면 None 이다.
        /// now 는 실제 흐른 시간(초)이다.
        /// </summary>
        public SoundCue Resolve(float now)
        {
            if (_pending.Count == 0)
            {
                return SoundCue.None;
            }

            // 규칙 1. 등급이 가장 높은 것 하나. 같은 등급이면 설정 목록에서 위에 있는 것.
            SoundCue best = SoundCue.None;
            int bestGrade = int.MaxValue;
            int bestIndex = int.MaxValue;

            for (int i = 0; i < _pending.Count; i++)
            {
                int index = _config.IndexOf(_pending[i]);
                int grade = _config.Cues[index].Grade;

                if (grade < bestGrade || (grade == bestGrade && index < bestIndex))
                {
                    best = _pending[i];
                    bestGrade = grade;
                    bestIndex = index;
                }
            }

            _pending.Clear();

            // 앞 묶음에 늦게 온 요청이다. 앞에 낸 소리보다 등급이 높지 않으면 내지 않는다.
            if (_hasBundle && now - _bundleTime < _config.BundleWindowSeconds && bestGrade >= _bundleGrade)
            {
                return SoundCue.None;
            }

            // 규칙 2. 같은 소리는 잠깐 사이에 다시 내지 않는다.
            float last;
            if (_lastPlayed.TryGetValue(best, out last) && now - last < _config.RepeatGapSeconds)
            {
                return SoundCue.None;
            }

            _hasBundle = true;
            _bundleTime = now;
            _bundleGrade = bestGrade;
            _lastPlayed[best] = now;
            return best;
        }

        /// <summary>
        /// 규칙 3. 동시에 울리는 소리 수를 넘을 때 어느 자리를 쓸지 고른다.
        ///
        /// playingGrades 는 자리마다 울리는 소리의 등급이고 빈 자리는 0 이다.
        /// 빈 자리가 있으면 그 자리, 없으면 새 소리까지 셋 가운데 등급이 가장 낮은 것을 끊는다.
        /// 새 소리가 가장 낮으면 -1 을 돌려주어 내지 않는다. 등급이 같으면 이미 울리는 쪽을 둔다.
        /// </summary>
        public static int ChooseVoice(int[] playingGrades, int newGrade)
        {
            if (playingGrades == null || playingGrades.Length == 0)
            {
                return -1;
            }

            for (int i = 0; i < playingGrades.Length; i++)
            {
                if (playingGrades[i] <= 0)
                {
                    return i;
                }
            }

            int lowest = -1;
            int lowestGrade = newGrade;

            for (int i = 0; i < playingGrades.Length; i++)
            {
                if (playingGrades[i] > lowestGrade)
                {
                    lowest = i;
                    lowestGrade = playingGrades[i];
                }
            }

            return lowest;
        }
    }
}
