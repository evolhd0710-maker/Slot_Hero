using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Reward.UI
{
    /// <summary>
    /// 보상 카드 하나.
    /// 인게임 화면 기획서 v0.2 / 10 보상 화면 의 "보상 카드"다.
    ///
    /// 300 × 300 이고 그림과 이름, 문양이면 태그 둘을 담는다.
    /// 고른 카드만 밝게 바뀌고 테두리가 생긴다.
    /// 누르면 즉시 획득하고 화면이 닫히므로 여기서는 눌렸다는 것만 알린다.
    /// </summary>
    public class RewardCardView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("붙일 것")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Image _border;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _tagLabel;

        private RewardCard _card;
        private RewardVisualConfig _visual;
        private bool _selected;
        private bool _hovering;
        private bool _pressed;

        /// <summary>카드를 눌렀다. 식별자가 함께 온다.</summary>
        public event Action<string> Clicked;

        /// <summary>카드 위에 올라가거나 벗어났다. 아이템 상세를 띄우는 쪽이 듣는다.</summary>
        public event Action<RewardCard, bool> HoverChanged;

        /// <summary>담고 있는 카드.</summary>
        public RewardCard Card
        {
            get { return _card; }
        }

        /// <summary>이 카드의 RectTransform. 자리를 잡는 쪽이 쓴다.</summary>
        public RectTransform RectTransform
        {
            get { return (RectTransform)transform; }
        }

        private void Awake()
        {
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

        /// <summary>카드 하나를 담는다.</summary>
        public void Bind(
            RewardCard card,
            RewardLayoutConfig layout,
            RewardVisualConfig visual,
            string tagText,
            Sprite icon)
        {
            _card = card;
            _visual = visual;
            _selected = false;

            gameObject.name = card != null ? "RewardCard_" + card.Id : "RewardCard";

            if (layout != null)
            {
                RectTransform.sizeDelta = layout.CardSize;
            }

            if (_nameLabel != null && card != null)
            {
                _nameLabel.text = card.DisplayName;

                if (visual != null)
                {
                    _nameLabel.fontSize = visual.CardNameFontSize;
                }
            }

            if (_tagLabel != null)
            {
                _tagLabel.text = tagText ?? string.Empty;
                _tagLabel.gameObject.SetActive(!string.IsNullOrEmpty(tagText));

                if (visual != null)
                {
                    _tagLabel.fontSize = visual.CardTagFontSize;
                }
            }

            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.enabled = icon != null;
            }

            ApplyStyle();
        }

        /// <summary>고른 카드인지 정한다. 고른 것만 밝아지고 테두리가 생긴다.</summary>
        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyStyle();
        }

        /// <summary>지금 색과 테두리를 다시 그린다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            Color baseColor = _visual.GetCardColor(_selected);

            if (_background != null)
            {
                _background.color = _visual.GetTint(baseColor, _hovering, _pressed);
            }

            if (_border != null)
            {
                _border.color = _visual.SelectedBorderColor;
                _border.gameObject.SetActive(_selected);
            }

            Color textColor = _visual.GetCardTextColor(_selected);

            if (_nameLabel != null)
            {
                _nameLabel.color = textColor;
            }

            if (_tagLabel != null)
            {
                _tagLabel.color = textColor;
            }
        }

        /// <inheritdoc />
        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            ApplyStyle();

            if (HoverChanged != null)
            {
                HoverChanged(_card, true);
            }
        }

        /// <inheritdoc />
        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            ApplyStyle();

            if (HoverChanged != null)
            {
                HoverChanged(_card, false);
            }
        }

        /// <inheritdoc />
        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            ApplyStyle();
        }

        /// <inheritdoc />
        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            ApplyStyle();
        }

        private void HandleClicked()
        {
            if (_card != null && Clicked != null)
            {
                Clicked(_card.Id);
            }
        }
    }
}
