using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Tools
{
    /// <summary>
    /// 효과음 후보 듣기 씬의 조종기(2026년 11월 13일 원재).
    /// `효과음과 이펙트 목록.md` 6.3 · 6.9 에서 후보만 정하고 게임에 붙이지 않은 효과음을, 받아 둔 묶음에서 꺼내 소리마다 줄로 늘어놓는다.
    /// 칸을 누르면 그 파일을 처음부터 낸다. 앞 소리가 울리고 있으면 끊는다. "멈춤" 은 소리를 끊는다.
    ///
    /// **게임 코드가 아니다.** 게임 흐름은 이것을 쓰지 않는다. 고른 소리는 `ImportSounds` 의 표에 넣어 붙인다.
    /// </summary>
    public class SoundAuditionDriver : MonoBehaviour
    {
        [SerializeField] private AudioSource _audio;
        [SerializeField] private TMP_Text _infoLabel;
        [SerializeField] private Button _stopButton;

        [Tooltip("칸마다 내는 소리. 칸과 같은 차례다.")]
        [SerializeField] private List<AudioClip> _clips = new List<AudioClip>();

        [Tooltip("소리 칸. 소리와 같은 차례다.")]
        [SerializeField] private List<Button> _buttons = new List<Button>();

        [Tooltip("칸마다 무슨 소리의 몇 번 후보인지. 누르면 위에 적는다.")]
        [SerializeField] private List<string> _titles = new List<string>();

        private int _playing = -1;

        private void Start()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                int index = i;
                if (_buttons[i] != null)
                {
                    _buttons[i].onClick.AddListener(() => Play(index));
                }
            }

            if (_stopButton != null)
            {
                _stopButton.onClick.AddListener(Stop);
            }
        }

        /// <summary>그 차례의 소리를 처음부터 낸다.</summary>
        public void Play(int index)
        {
            if (index < 0 || index >= _clips.Count || _clips[index] == null)
            {
                return;
            }

            _playing = index;
            if (_audio != null)
            {
                _audio.Stop();
                _audio.clip = _clips[index];
                _audio.Play();
            }

            string title = index < _titles.Count ? _titles[index] : _clips[index].name;
            SetInfo("지금 · " + title + " · " + _clips[index].length.ToString("0.00") + "초");
        }

        /// <summary>소리를 끊는다.</summary>
        public void Stop()
        {
            if (_audio != null)
            {
                _audio.Stop();
            }

            _playing = -1;
            SetInfo("칸을 누르면 그 소리를 낸다.");
        }

        /// <summary>마지막으로 낸 칸. 없으면 -1 이다.</summary>
        public int Playing
        {
            get { return _playing; }
        }

        private void SetInfo(string text)
        {
            if (_infoLabel != null)
            {
                _infoLabel.text = text;
            }
        }
    }
}
