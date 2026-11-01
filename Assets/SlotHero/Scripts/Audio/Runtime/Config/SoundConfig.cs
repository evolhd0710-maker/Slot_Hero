using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Audio
{
    /// <summary>
    /// 효과음 설정. 소리마다의 파일과 크기, 겹칠 때 규칙의 수치를 둔다.
    /// 기본값은 `효과음과 이펙트 목록.md` 1장의 표와 1.6 겹칠 때 규칙이다.
    ///
    /// 소리 파일은 `Slot Hero/소리 가져오기` 가 `코드/Audio/Sound` 에서 가져와 물린다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SoundConfig",
        menuName = "Slot Hero/소리/효과음 설정",
        order = 0)]
    public class SoundConfig : ScriptableObject
    {
        [Header("소리")]
        [Tooltip("효과음 목록. 등급이 같으면 위에 있는 것이 높다(1.6 겹칠 때 규칙).")]
        [SerializeField] private List<SoundCueEntry> _cues = DefaultCues();
        public List<SoundCueEntry> Cues { get { return _cues; } set { _cues = value; } }

        [Header("겹칠 때 규칙")]
        [Tooltip("규칙 1. 한 묶음에서 하나만 낸다. 소리를 낸 뒤 이 시간 안에 온 같은 등급 이하 소리는 같은 묶음으로 보고 내지 않는다.")]
        [SerializeField] private float _bundleWindowSeconds = 0.1f;
        public float BundleWindowSeconds { get { return _bundleWindowSeconds; } set { _bundleWindowSeconds = value; } }

        [Tooltip("규칙 2. 같은 소리는 이 시간 안에 다시 내지 않는다.")]
        [SerializeField] private float _repeatGapSeconds = 0.1f;
        public float RepeatGapSeconds { get { return _repeatGapSeconds; } set { _repeatGapSeconds = value; } }

        [Tooltip("규칙 3. 동시에 울리는 효과음 수.")]
        [SerializeField] private int _maxVoiceCount = 2;
        public int MaxVoiceCount { get { return _maxVoiceCount; } set { _maxVoiceCount = value; } }

        [Header("S23 행상 구매")]
        [Tooltip("치른 골드의 구간 경계. 오름차순. 30 과 80 이면 30 미만, 80 미만, 80 이상의 세 구간이고 " +
                 "구간 차례대로 행상 구매 소리의 파일을 하나씩 쓴다(2026년 10월 31일 원재).")]
        [SerializeField] private int[] _purchaseGoldSteps = { 30, 80 };
        public int[] PurchaseGoldSteps { get { return _purchaseGoldSteps; } set { _purchaseGoldSteps = value; } }

        /// <summary>
        /// 치른 골드가 몇 번째 구간인지. 0 부터 센다. 경계와 같으면 위 구간이다.
        /// 기본값이면 29 골드는 0, 30 골드는 1, 80 골드는 2 다.
        /// </summary>
        public int PurchaseClipIndex(int gold)
        {
            int index = 0;
            if (_purchaseGoldSteps == null)
            {
                return index;
            }

            for (int i = 0; i < _purchaseGoldSteps.Length; i++)
            {
                if (gold >= _purchaseGoldSteps[i])
                {
                    index = i + 1;
                }
            }

            return index;
        }

        /// <summary>소리 하나의 설정을 찾는다. 찾으면 그 자리를, 없으면 -1 을 돌려준다.</summary>
        public int IndexOf(SoundCue cue)
        {
            if (_cues == null)
            {
                return -1;
            }

            for (int i = 0; i < _cues.Count; i++)
            {
                if (_cues[i].Cue == cue)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 기본 목록. 등급과 길이는 `효과음과 이펙트 목록.md` 의 S23, S12, S24 다.
        /// S23 행상 구매는 2등급 맨 위라 산 물건의 받는 소리보다 높다.
        /// S12 체력 잃음은 3등급 맨 위다. 목록 1.3 표의 차례대로 S24 야영보다 위에 둔다.
        /// S24 야영은 체력 회복 소리보다 높다.
        /// </summary>
        public static List<SoundCueEntry> DefaultCues()
        {
            return new List<SoundCueEntry>
            {
                new SoundCueEntry(SoundCue.MerchantPurchase, 2, 1f, 0f, 0f),
                new SoundCueEntry(SoundCue.HealthLost, 3, 1f, 0f, 0f),
                new SoundCueEntry(SoundCue.Camp, 3, 1f, 2f, 0.5f),
                new SoundCueEntry(SoundCue.NodeSelect, 4, 1f, 0f, 0f),
            };
        }

        /// <summary>
        /// 기본 목록에 있는데 이 설정에 없는 소리를 더한다. 이미 있는 소리의 파일과 크기는 그대로 둔다.
        /// 차례는 기본 목록을 따른다(같은 등급이면 위에 있는 것이 높다). 더한 수를 돌려준다.
        /// 소리를 새로 붙일 때 이미 만든 설정 에셋에도 그 소리 칸이 생기게 한다.
        /// </summary>
        public int AddMissingDefaults()
        {
            if (_cues == null)
            {
                _cues = new List<SoundCueEntry>();
            }

            List<SoundCueEntry> defaults = DefaultCues();
            List<SoundCueEntry> merged = new List<SoundCueEntry>();
            int added = 0;

            for (int i = 0; i < defaults.Count; i++)
            {
                int index = IndexOf(defaults[i].Cue);
                if (index >= 0)
                {
                    merged.Add(_cues[index]);
                }
                else
                {
                    merged.Add(defaults[i]);
                    added++;
                }
            }

            // 기본 목록에 없는 소리는 뒤에 그대로 붙인다.
            for (int i = 0; i < _cues.Count; i++)
            {
                bool inDefaults = false;
                for (int d = 0; d < defaults.Count; d++)
                {
                    inDefaults |= defaults[d].Cue == _cues[i].Cue;
                }

                if (!inDefaults)
                {
                    merged.Add(_cues[i]);
                }
            }

            _cues = merged;
            return added;
        }
    }
}
