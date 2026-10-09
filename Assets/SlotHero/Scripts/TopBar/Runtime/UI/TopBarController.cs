using System;
using System.Globalization;
using UnityEngine;

namespace SlotHero.TopBar.UI
{
    /// <summary>
    /// 상단 표시줄의 값 갱신과 상호작용.
    /// 상단 UI 바 기획서 v0.2 의 05 정보 갱신 시점 과 06 상호작용 을 맡는다.
    ///
    /// 버튼이 여는 화면은 각 화면 기획서 소관이라 눌렸다는 것만 알린다.
    /// 값의 출처인 체력과 골드도 다른 시스템이 들고 있으므로 바뀐 값을 받기만 한다.
    /// </summary>
    public class TopBarController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private TopBarView _view;
        [SerializeField] private TopBarVisualConfig _visual;

        [Header("아이콘")]
        [Tooltip("② 위치 칸의 스테이지 아이콘. 붉은 깃발이다. " +
            "SetLocation 에 그림을 넘기지 않으면 이것을 쓴다.")]
        [SerializeField] private Sprite _stageIcon;

        [Tooltip("③ 체력 칸의 아이콘.")]
        [SerializeField] private Sprite _healthIcon;

        [Tooltip("④ 골드 칸의 아이콘.")]
        [SerializeField] private Sprite _goldIcon;

        private float _elapsedSeconds;
        private int _shownSecond = -1;

        private int _health;
        private int _maxHealth;
        private bool _healthKnown;

        private int _goldTarget;
        private int _goldShown;
        private float _goldFrom;
        private float _goldCountRemaining;

        private string _locationTooltip;

        /// <summary>표시줄의 버튼을 눌렀을 때. 어느 버튼인지 넘긴다.</summary>
        public event Action<TopBarButtonKind> ButtonClicked;

        /// <summary>마지막으로 받은 경과 시간. 표시만 한다. 시간을 재는 것은 흐름의 `RunClock` 하나다.</summary>
        public float ElapsedSeconds
        {
            get { return _elapsedSeconds; }
        }

        private void Awake()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetHealthIcon(_healthIcon);
            _view.SetGoldIcon(_goldIcon);

            TopBarButton[] buttons = _view.Buttons;
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] == null)
                    {
                        continue;
                    }

                    buttons[i].Clicked += HandleButtonClicked;
                    buttons[i].HoverChanged += HandleButtonHover;
                }
            }

            if (_view.LocationHover != null)
            {
                _view.LocationHover.HoverChanged += HandleLocationHover;
            }
        }

        private void OnDestroy()
        {
            if (_view == null)
            {
                return;
            }

            TopBarButton[] buttons = _view.Buttons;
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] == null)
                    {
                        continue;
                    }

                    buttons[i].Clicked -= HandleButtonClicked;
                    buttons[i].HoverChanged -= HandleButtonHover;
                }
            }

            if (_view.LocationHover != null)
            {
                _view.LocationHover.HoverChanged -= HandleLocationHover;
            }
        }

        private void Update()
        {
            UpdateGold();
        }

        // ── 플레이 시간 ─────────────────────────────────────────

        /// <summary>
        /// 경과 시간을 넘겨받아 보여 준다. 흐름이 매 프레임 `RunClock` 의 값을 넘긴다.
        ///
        /// **표시줄은 시간을 재지 않는다.** 예전에는 여기에도 시계가 있어 둘이 따로 흘렀고,
        /// 이 함수가 보여 준 초를 지워 매 프레임 글자를 새로 만들었다. 2026년 10월 8일에 고쳤다.
        /// 05 정보 갱신 시점 대로 초가 바뀔 때만 글자를 바꾼다.
        /// </summary>
        public void SetElapsedSeconds(float seconds)
        {
            _elapsedSeconds = seconds < 0f ? 0f : seconds;
            RefreshPlayTime();
        }

        private void RefreshPlayTime()
        {
            int second = (int)_elapsedSeconds;
            if (second == _shownSecond)
            {
                return;
            }

            _shownSecond = second;

            if (_view != null)
            {
                bool pad = _visual == null || _visual.PadPlayTimeMinutes;
                _view.SetPlayTime(PlayTimeFormat.Format(_elapsedSeconds, pad));
            }
        }

        // ── 위치 ────────────────────────────────────────────────

        /// <summary>
        /// ② 위치. 새로운 방에 들어갈 때 부른다. 05 정보 갱신 시점 대로 즉시 바뀐다.
        /// 방 타입마다의 아이콘은 맵 쪽이 들고 있으므로 그림을 그대로 받는다.
        /// tooltipText 는 06 상호작용 의 "현재 스테이지의 이름, 현재 방의 종류"이다.
        ///
        /// stage 는 맵의 단계다. 스테이지 번호가 아니다.
        /// stageIcon 을 비워 두면 이 조종기에 물려 둔 깃발 그림을 쓴다.
        /// 스테이지마다 깃발이 달라지면 그때 부르는 쪽이 그림을 넘기면 된다.
        /// </summary>
        public void SetLocation(Sprite stageIcon, int stage, Sprite roomIcon, string tooltipText)
        {
            _locationTooltip = tooltipText;

            if (_view != null)
            {
                _view.SetLocation(
                    stageIcon != null ? stageIcon : _stageIcon,
                    stage.ToString(CultureInfo.InvariantCulture),
                    roomIcon);
            }
        }

        // ── 체력 ────────────────────────────────────────────────

        /// <summary>
        /// ③ 체력. 05 정보 갱신 시점 대로 값은 즉시 바뀌고 짧은 시간 색이 달라진다.
        /// 런을 시작하거나 불러올 때처럼 연출이 필요 없으면 animate 를 끈다.
        /// </summary>
        public void SetHealth(int current, int max, bool animate)
        {
            int previous = _health;
            bool hadValue = _healthKnown;

            _health = current;
            _maxHealth = max;
            _healthKnown = true;

            if (_view == null)
            {
                return;
            }

            string format = _visual != null && !string.IsNullOrEmpty(_visual.HealthFormat)
                ? _visual.HealthFormat
                : "{0}/{1}";
            _view.SetHealthText(string.Format(CultureInfo.InvariantCulture, format, current, max));

            if (!animate || !hadValue || current == previous || _visual == null)
            {
                return;
            }

            Color flash = current > previous ? _visual.HealthIncreaseColor : _visual.HealthDecreaseColor;
            _view.FlashHealth(flash, _visual.HealthFlashSeconds);
        }

        // ── 골드 ────────────────────────────────────────────────

        /// <summary>
        /// ④ 골드. 05 정보 갱신 시점 의 "짧은 시간 동안 변화 연출 후 최종 값을 띄운다"에 따라
        /// 지금 보이는 숫자에서 새 값까지 흘러간다.
        /// </summary>
        public void SetGold(int value, bool animate)
        {
            _goldTarget = value;

            float seconds = _visual != null ? _visual.GoldCountSeconds : 0f;

            if (!animate || seconds <= 0f)
            {
                _goldCountRemaining = 0f;
                _goldShown = value;
                RefreshGoldText();
                return;
            }

            _goldFrom = _goldShown;
            _goldCountRemaining = seconds;
        }

        private void UpdateGold()
        {
            if (_goldCountRemaining <= 0f)
            {
                return;
            }

            float total = _visual != null ? _visual.GoldCountSeconds : 0f;
            _goldCountRemaining -= Time.unscaledDeltaTime;

            if (_goldCountRemaining <= 0f || total <= 0f)
            {
                _goldCountRemaining = 0f;
                _goldShown = _goldTarget;
                RefreshGoldText();
                return;
            }

            float done = 1f - _goldCountRemaining / total;
            int next = Mathf.RoundToInt(Mathf.Lerp(_goldFrom, _goldTarget, done));

            if (next == _goldShown)
            {
                return;
            }

            _goldShown = next;
            RefreshGoldText();
        }

        private void RefreshGoldText()
        {
            if (_view == null)
            {
                return;
            }

            string numberFormat = _visual != null && !string.IsNullOrEmpty(_visual.GoldNumberFormat)
                ? _visual.GoldNumberFormat
                : "N0";
            _view.SetGoldText(_goldShown.ToString(numberFormat, CultureInfo.InvariantCulture));
        }

        // ── 버튼 ────────────────────────────────────────────────

        /// <summary>
        /// 버튼을 누를 수 있는지 정한다.
        /// 06 상호작용 의 비활성 조건은 지도가 맵 화면을 볼 때, 설정이 설정 화면을 볼 때,
        /// 런 종료가 런 종료 팝업이 떠 있을 때이다.
        /// </summary>
        public void SetButtonInteractable(TopBarButtonKind kind, bool value)
        {
            if (_view == null)
            {
                return;
            }

            TopBarButton button = _view.GetButton(kind);
            if (button != null)
            {
                button.SetInteractable(value);
            }
        }

        private void HandleButtonClicked(TopBarButton button)
        {
            if (ButtonClicked != null)
            {
                ButtonClicked(button.Kind);
            }
        }

        private void HandleButtonHover(TopBarButton button, bool hovering)
        {
            if (_view == null || _view.Tooltip == null)
            {
                return;
            }

            if (hovering)
            {
                _view.Tooltip.Show(button.RectTransform, button.TooltipText);
            }
            else
            {
                _view.Tooltip.Hide();
            }
        }

        private void HandleLocationHover(TopBarHoverArea area, bool hovering)
        {
            if (_view == null || _view.Tooltip == null)
            {
                return;
            }

            if (hovering)
            {
                _view.Tooltip.Show(area.RectTransform, _locationTooltip);
            }
            else
            {
                _view.Tooltip.Hide();
            }
        }
    }
}
