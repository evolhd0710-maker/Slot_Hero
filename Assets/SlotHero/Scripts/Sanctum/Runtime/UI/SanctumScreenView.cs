using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Sanctum.UI
{
    /// <summary>
    /// 성소 화면 표시.
    /// 성소 기획서 v0.2 / 08 화면 구성 의 성소 화면 배치를 맡는다.
    /// 좌측에 야영, 우측에 행상, 우측 하단에 나가기 버튼을 둔다.
    ///
    /// 자리 잡기는 1920 × 1080 고정이라 프리팹에서 맞추고,
    /// 여기서는 야영을 쓴 뒤의 상태 변화만 코드로 바꾼다.
    /// </summary>
    public class SanctumScreenView : MonoBehaviour
    {
        [Header("야영")]
        [SerializeField] private Button _campButton;
        [SerializeField] private Image _campImage;

        [Tooltip("쓸 수 있는 야영. 불이 붙어 있는 모닥불이다.")]
        [SerializeField] private Sprite _campAvailableSprite;

        [Tooltip("이미 쓴 야영. 불이 꺼진 모닥불이다. 비워 두면 그림 대신 색만 흐려진다.")]
        [SerializeField] private Sprite _campUsedSprite;

        [Tooltip("야영을 쓸 때의 연출. 화면이 어두워지고 회복 빛이 떠올랐다가 다시 밝아진다(2026년 10월 31일 원재).")]
        [SerializeField] private CampEffectView _campEffect;

        [Tooltip("연출 시간과 색. 성소 표시 설정이다.")]
        [SerializeField] private SanctumVisualConfig _visual;

        // 연출이 가장 어두워질 때까지 불 붙은 모닥불을 그대로 둔다. 그때 꺼진 그림으로 바꾼다.
        private bool _holdCampLit;
        private SanctumState _lastState;
        private SanctumConfig _lastConfig;
        private SanctumVisualConfig _lastVisual;

        [Header("행상")]
        [SerializeField] private Button _merchantButton;

        [Header("나가기")]
        [SerializeField] private Button _exitButton;

        /// <summary>야영을 골랐을 때. 03 진입 흐름 의 야영 선택.</summary>
        public event Action CampClicked;

        /// <summary>행상을 골랐을 때. 03 진입 흐름 의 행상 선택.</summary>
        public event Action MerchantClicked;

        /// <summary>나가기를 골랐을 때. 03 진입 흐름 의 나가기 선택.</summary>
        public event Action ExitClicked;

        private void Awake()
        {
            if (_campButton != null)
            {
                _campButton.onClick.AddListener(HandleCampClick);
            }

            if (_merchantButton != null)
            {
                _merchantButton.onClick.AddListener(HandleMerchantClick);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(HandleExitClick);
            }
        }

        private void OnDestroy()
        {
            if (_campButton != null)
            {
                _campButton.onClick.RemoveListener(HandleCampClick);
            }

            if (_merchantButton != null)
            {
                _merchantButton.onClick.RemoveListener(HandleMerchantClick);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(HandleExitClick);
            }
        }

        /// <summary>
        /// 지금 상태를 화면에 반영한다.
        /// 04 야영 의 사용 표시: 쓴 뒤에는 아이콘을 회색으로 바꾸고 선택을 막는다.
        /// 야영과 행상은 순서에 제한이 없으므로 행상 버튼은 늘 열어 둔다.
        /// </summary>
        public void Refresh(SanctumState state, SanctumConfig config, SanctumVisualConfig visual)
        {
            _lastState = state;
            _lastConfig = config;
            _lastVisual = visual;

            bool campAvailable = state != null && state.CanUseCamp(config);

            if (_campButton != null)
            {
                _campButton.interactable = campAvailable;
            }

            // 야영 연출이 가장 어두워지기 전에는 불 붙은 모닥불을 보인다.
            bool campLit = campAvailable || _holdCampLit;

            if (_campImage != null && visual != null)
            {
                // 쓴 야영은 꺼진 모닥불 그림으로 바꾼다.
                // 그림이 둘 다 있을 때만 그렇게 하고, 없으면 색만 흐리게 해 둔다.
                bool hasBothSprites = _campAvailableSprite != null && _campUsedSprite != null;

                if (hasBothSprites)
                {
                    _campImage.sprite = campLit ? _campAvailableSprite : _campUsedSprite;

                    // 이 칸이 버튼의 Target Graphic 이면 버튼이 상태마다 overrideSprite 를 덮어쓴다.
                    // 덮인 것을 치워야 방금 넣은 sprite 가 화면에 나온다.
                    _campImage.overrideSprite = null;

                    _campImage.color = visual.CampAvailableTint;
                }
                else
                {
                    _campImage.color = campLit ? visual.CampAvailableTint : visual.CampUsedTint;
                }
            }

            if (_merchantButton != null)
            {
                _merchantButton.interactable = true;
            }

            if (_exitButton != null)
            {
                _exitButton.interactable = true;
            }
        }

        /// <summary>
        /// 야영 연출을 시작한다. 가장 어두울 때 모닥불을 꺼진 그림으로 바꾼다.
        /// 연출이 없거나 게임이 돌지 않으면 바로 바뀐다.
        /// </summary>
        public void PlayCampEffect()
        {
            if (_campEffect == null || !Application.isPlaying)
            {
                return;
            }

            _holdCampLit = true;
            _campEffect.Play(_visual, ReleaseCampLit);
        }

        /// <summary>야영 연출이 도는 중인지.</summary>
        public bool CampEffectPlaying
        {
            get { return _campEffect != null && _campEffect.Playing; }
        }

        private void ReleaseCampLit()
        {
            _holdCampLit = false;
            Refresh(_lastState, _lastConfig, _lastVisual);
        }

        private void HandleCampClick()
        {
            // 연출을 먼저 걸어 두어야 야영을 쓰고 화면을 다시 그릴 때 모닥불이 바로 꺼지지 않는다.
            PlayCampEffect();

            if (CampClicked != null)
            {
                CampClicked();
            }
        }

        private void HandleMerchantClick()
        {
            if (MerchantClicked != null)
            {
                MerchantClicked();
            }
        }

        private void HandleExitClick()
        {
            if (ExitClicked != null)
            {
                ExitClicked();
            }
        }
    }
}
