using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Audio
{
    /// <summary>
    /// 버튼을 누르면 S01 버튼 누름 소리를 낸다(2026년 11월 13일 원재). `효과음과 이펙트 목록.md` 1.5 의 "모든 버튼" 이다.
    ///
    /// 게임 씬을 지을 때 씬의 버튼마다 하나씩 붙고(`BuildGameScene`), 복제해 쓰는 칸의 프리팹에도 붙는다(`UiSceneBuilder.SavePrefab`).
    /// 프리팹은 씬의 소리 창구를 물 수 없어 비어 있으면 지금 소리 창구(`SoundPlayer.Current`)를 쓴다.
    /// 누름 소리는 5등급이라 같은 때에 난 다른 소리(노드 고르기, 행상 구매, 현재 빌드 열기 등)가 있으면 그쪽이 난다.
    /// 버튼이 눌리지 않는 상태(interactable 꺼짐)면 버튼이 알리지 않아 소리도 나지 않는다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        [Tooltip("소리를 내는 곳. 게임 씬의 흐름 오브젝트에 붙은 SoundPlayer 다.")]
        [SerializeField] private SoundPlayer _sound;

        private Button _button;

        /// <summary>소리를 내는 곳. 씬을 지을 때 물린다.</summary>
        public SoundPlayer Sound
        {
            get { return _sound; }
            set { _sound = value; }
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            SoundPlayer sound = _sound != null ? _sound : SoundPlayer.Current;
            if (sound != null)
            {
                sound.Play(SoundCue.ButtonPress);
            }
        }
    }
}
