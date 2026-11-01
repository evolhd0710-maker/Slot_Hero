using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 코인 UI. 전투 UI 및 연출 초안 / 03 코인 UI 세부.
    ///
    ///   - 이번 턴에 꺼낸 코인 셋을 삼각형으로 놓는다. 꺼낸 것이 셋보다 적으면 그 자리는 원판 바탕의 빈 코인 자리로 둔다
    ///   - 코인에 마우스를 올리면 옆에 정보 팝업이 뜬다. 위와 왼쪽 아래 코인은 오른쪽에, 오른쪽 아래 코인은 왼쪽에 뜬다
    ///   - 코인을 누르면 태그 UI 로 바뀐다. 체력이 모자란 코인은 누를 수 없다
    ///   - 가운데 작은 원은 "코인 없이" 다. 초안에 설명이 없는 자리라 코인을 쓸 수 없을 때의 길로 두었다
    ///
    /// 그림은 피그마 `Jackpot 아트웍 (복사)` Frame 1 · 2 다. 원판 바탕에 그려진 코인 자리 셋에 코인을 놓고,
    /// 고를 수 있는 코인에 마우스를 올리면 붉은 빛 테두리를 두른다. 정보 팝업은 검은 설명 풍선이다.
    /// </summary>
    public class CoinDialView : MonoBehaviour
    {
        [Header("코인")]
        [SerializeField] private Button[] _coinButtons = new Button[0];
        [SerializeField] private Image[] _coinBacks = new Image[0];
        [SerializeField] private Image[] _coinIcons = new Image[0];
        [SerializeField] private TMP_Text[] _coinLabels = new TMP_Text[0];
        [SerializeField] private PointerRelay[] _coinHovers = new PointerRelay[0];

        [Tooltip("고를 수 있는 코인에 마우스를 올리면 켜는 붉은 빛 테두리. 피그마 Frame 2 의 코인 고름 강조다.")]
        [SerializeField] private Image[] _coinGlows = new Image[0];

        [Header("가운데")]
        [SerializeField] private Button _centerButton;
        [SerializeField] private Image _centerBack;
        [SerializeField] private TMP_Text _centerLabel;
        [SerializeField] private PointerRelay _centerHover;

        [Header("정보 팝업")]
        [SerializeField] private Image _popup;
        [SerializeField] private Image _popupIcon;
        [SerializeField] private TMP_Text _popupName;
        [SerializeField] private TMP_Text _popupStats;
        [SerializeField] private TMP_Text _popupDescription;

        private CombatLayoutConfig _layout;
        private CombatVisualConfig _visual;
        private Action<bool>[] _hoverHandlers = new Action<bool>[0];

        /// <summary>그 자리의 코인을 눌렀다.</summary>
        public event Action<int> CoinClicked;

        /// <summary>그 자리의 코인에 마우스가 올라가거나 벗어났다.</summary>
        public event Action<int, bool> CoinHoverChanged;

        /// <summary>가운데 "코인 없이" 를 눌렀다.</summary>
        public event Action CenterClicked;

        /// <summary>가운데에 마우스가 올라가거나 벗어났다.</summary>
        public event Action<bool> CenterHoverChanged;

        /// <summary>코인 자리 수.</summary>
        public int SlotCount
        {
            get { return _coinButtons.Length; }
        }

        private void Awake()
        {
            for (int i = 0; i < _coinButtons.Length; i++)
            {
                int index = i;
                if (_coinButtons[i] != null)
                {
                    _coinButtons[i].onClick.AddListener(() => HandleCoinClicked(index));
                }
            }

            _hoverHandlers = new Action<bool>[_coinHovers.Length];
            for (int i = 0; i < _coinHovers.Length; i++)
            {
                int index = i;
                _hoverHandlers[i] = hovering => HandleCoinHover(index, hovering);
                if (_coinHovers[i] != null)
                {
                    _coinHovers[i].HoverChanged += _hoverHandlers[i];
                }
            }

            if (_centerButton != null)
            {
                _centerButton.onClick.AddListener(HandleCenterClicked);
            }

            if (_centerHover != null)
            {
                _centerHover.HoverChanged += HandleCenterHover;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _coinButtons.Length; i++)
            {
                if (_coinButtons[i] != null)
                {
                    _coinButtons[i].onClick.RemoveAllListeners();
                }
            }

            for (int i = 0; i < _coinHovers.Length && i < _hoverHandlers.Length; i++)
            {
                if (_coinHovers[i] != null)
                {
                    _coinHovers[i].HoverChanged -= _hoverHandlers[i];
                }
            }

            if (_centerButton != null)
            {
                _centerButton.onClick.RemoveListener(HandleCenterClicked);
            }

            if (_centerHover != null)
            {
                _centerHover.HoverChanged -= HandleCenterHover;
            }
        }

        /// <summary>설정을 받고 자리와 색을 맞춘다.</summary>
        public void Configure(CombatLayoutConfig layout, CombatVisualConfig visual)
        {
            _layout = layout;
            _visual = visual;
            ApplyLayout();
        }

        /// <summary>그 자리의 코인을 그린다. 쓸 수 없으면 어둡게 하고 누를 수 없게 한다.</summary>
        public void SetCoin(int index, Sprite icon, string displayName, bool usable)
        {
            if (index < 0 || index >= _coinButtons.Length)
            {
                return;
            }

            float brightness = usable ? 1f : (_visual != null ? _visual.DisabledBrightness : 0.35f);
            Color coinColor = _visual != null ? _visual.CoinColor : Color.white;

            // 바탕 원은 끄지 않는다. 마우스를 받는 것은 이 원뿐이다(코인 그림은 받지 않는다).
            // 예전에는 그림이 있으면 원을 꺼서 코인을 눌러도 올려도 아무 일이 없었다. 그림이 있으면 투명하게만 둔다.
            if (_coinBacks.Length > index && _coinBacks[index] != null)
            {
                _coinBacks[index].enabled = true;
                _coinBacks[index].color = icon != null ? Color.clear : CombatVisualConfig.Scale(coinColor, brightness);
            }

            if (_coinIcons.Length > index && _coinIcons[index] != null)
            {
                _coinIcons[index].sprite = icon;
                _coinIcons[index].enabled = icon != null;
                _coinIcons[index].color = CombatVisualConfig.Scale(Color.white, brightness);
                _coinIcons[index].preserveAspect = true;
            }

            if (_coinLabels.Length > index && _coinLabels[index] != null)
            {
                _coinLabels[index].text = icon == null ? displayName : string.Empty;
            }

            if (_coinButtons[index] != null)
            {
                _coinButtons[index].interactable = usable;
            }

            SetGlow(index, false);
        }

        /// <summary>그 자리를 빈 코인 자리로 둔다. 꺼낸 코인이 없는 자리다.</summary>
        public void SetEmpty(int index)
        {
            if (index < 0 || index >= _coinButtons.Length)
            {
                return;
            }

            // 원판 바탕에 빈 코인 자리가 그려져 있어 바탕 원은 투명하게 둔다.
            if (_coinBacks.Length > index && _coinBacks[index] != null)
            {
                _coinBacks[index].enabled = true;
                _coinBacks[index].color = Color.clear;
            }

            SetGlow(index, false);

            if (_coinIcons.Length > index && _coinIcons[index] != null)
            {
                _coinIcons[index].enabled = false;
            }

            if (_coinLabels.Length > index && _coinLabels[index] != null)
            {
                _coinLabels[index].text = _visual != null ? _visual.EmptyCoinText : "X";
            }

            if (_coinButtons[index] != null)
            {
                _coinButtons[index].interactable = false;
            }
        }

        /// <summary>
        /// 그 자리 옆에 코인 정보 팝업을 띄운다.
        /// 위와 왼쪽 아래 코인은 코인 오른쪽에, 오른쪽 아래 코인은 왼쪽에 놓는다(초안 03).
        /// 피그마 Frame 2 처럼 코인 가운데보다 `CoinPopupLift` 만큼 올려 원판을 덜 가린다.
        /// </summary>
        public void ShowPopup(int index, Sprite icon, string title, string stats, string description)
        {
            if (_popup == null)
            {
                return;
            }

            _popup.gameObject.SetActive(true);
            _popup.transform.SetAsLastSibling();

            if (_popupIcon != null)
            {
                _popupIcon.sprite = icon;
                _popupIcon.enabled = icon != null;
                _popupIcon.preserveAspect = true;
            }

            SetText(_popupName, title);
            SetText(_popupStats, stats);
            SetText(_popupDescription, description);

            if (_layout == null)
            {
                return;
            }

            Vector2 coin = _layout.GetCoinOffset(index);
            float half = _layout.CoinSize * 0.5f;
            bool toLeft = coin.x > 0.01f;

            float x = toLeft
                ? coin.x - half - _layout.CoinPopupGap - _layout.CoinPopupSize.x
                : coin.x + half + _layout.CoinPopupGap;
            float y = coin.y - _layout.CoinPopupSize.y * 0.5f - _layout.CoinPopupLift;

            RectTransform rect = _popup.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = _layout.CoinPopupSize;
            rect.anchoredPosition = new Vector2(x, -y);
        }

        /// <summary>정보 팝업을 걷는다.</summary>
        public void HidePopup()
        {
            if (_popup != null)
            {
                _popup.gameObject.SetActive(false);
            }
        }

        /// <summary>코인과 가운데와 팝업의 자리를 잡는다.</summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            for (int i = 0; i < _coinButtons.Length; i++)
            {
                if (_coinButtons[i] == null)
                {
                    continue;
                }

                Vector2 offset = _layout.GetCoinOffset(i);
                PlaceCentered((RectTransform)_coinButtons[i].transform, new Vector2(offset.x, -offset.y), _layout.CoinSize);

                if (i < _coinGlows.Length && _coinGlows[i] != null)
                {
                    PlaceCentered(_coinGlows[i].rectTransform, Vector2.zero, _layout.CoinSize * _layout.CoinGlowScale);
                    _coinGlows[i].gameObject.SetActive(false);
                }
            }

            if (_centerButton != null)
            {
                PlaceCentered((RectTransform)_centerButton.transform, Vector2.zero, _layout.CoinCenterSize);
            }

            if (_centerLabel != null && _visual != null)
            {
                _centerLabel.text = _visual.NoCoinText;
            }

            if (_centerBack != null && _visual != null)
            {
                _centerBack.color = _visual.EmptyColor;
            }

            // 팝업 바탕은 피그마 설명 풍선 그림이라 색을 입히지 않는다.
            HidePopup();
        }

        private void SetGlow(int index, bool on)
        {
            if (index >= 0 && index < _coinGlows.Length && _coinGlows[index] != null)
            {
                _coinGlows[index].gameObject.SetActive(on);
            }
        }

        private static void PlaceCentered(RectTransform rect, Vector2 position, float size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = position;
        }

        private static void SetText(TMP_Text label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        private void HandleCoinClicked(int index)
        {
            if (CoinClicked != null)
            {
                CoinClicked(index);
            }
        }

        private void HandleCoinHover(int index, bool hovering)
        {
            bool usable = index < _coinButtons.Length && _coinButtons[index] != null && _coinButtons[index].interactable;
            SetGlow(index, hovering && usable);

            if (CoinHoverChanged != null)
            {
                CoinHoverChanged(index, hovering);
            }
        }

        private void HandleCenterClicked()
        {
            if (CenterClicked != null)
            {
                CenterClicked();
            }
        }

        private void HandleCenterHover(bool hovering)
        {
            if (CenterHoverChanged != null)
            {
                CenterHoverChanged(hovering);
            }
        }
    }
}
