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
            bool campAvailable = state != null && state.CanUseCamp(config);

            if (_campButton != null)
            {
                _campButton.interactable = campAvailable;
            }

            if (_campImage != null && visual != null)
            {
                // 쓴 야영은 꺼진 모닥불 그림으로 바꾼다.
                // 그림이 둘 다 있을 때만 그렇게 하고, 없으면 색만 흐리게 해 둔다.
                bool hasBothSprites = _campAvailableSprite != null && _campUsedSprite != null;

                if (hasBothSprites)
                {
                    _campImage.sprite = campAvailable ? _campAvailableSprite : _campUsedSprite;

                    // 이 칸이 버튼의 Target Graphic 이면 버튼이 상태마다 overrideSprite 를 덮어쓴다.
                    // 덮인 것을 치워야 방금 넣은 sprite 가 화면에 나온다.
                    _campImage.overrideSprite = null;

                    _campImage.color = visual.CampAvailableTint;
                }
                else
                {
                    _campImage.color = campAvailable ? visual.CampAvailableTint : visual.CampUsedTint;
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

        private void HandleCampClick()
        {
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
