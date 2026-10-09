using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Sanctum.UI
{
    /// <summary>
    /// 행상 진열의 한 자리 표시.
    /// 성소 기획서 v0.2 / 08 화면 구성 의
    /// "물건에 커서를 올리면 상세 설명 팝업을 띄우고, 클릭 시 즉시 구매한다",
    /// "물건의 가격은 대부분 물건의 하단에 골드 아이콘과 함께 배치한다"를 맡는다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 자식에 물건 그림 Image,
    /// 가격 묶음(골드 아이콘 Image + 가격 TMP_Text)을 둔다.
    /// 자리는 문양, 코인, 유물이 모두 같은 프리팹을 쓸 수 있다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MerchantSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _priceRoot;
        [SerializeField] private TMP_Text _priceLabel;

        private RectTransform _rectTransform;
        private SanctumVisualConfig _visual;
        private Action<MerchantSlotView> _onClicked;
        private Action<MerchantSlotView, bool> _onHoverChanged;
        private bool _hovering;

        /// <summary>이 표시가 맡은 자리.</summary>
        public MerchantSlot Slot { get; private set; }

        /// <summary>지금 골드로 살 수 있는 자리인지.</summary>
        public bool Affordable { get; private set; }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = (RectTransform)transform;
                }

                return _rectTransform;
            }
        }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>자리의 내용과 지금 골드를 반영한다.</summary>
        public void Bind(
            MerchantSlot slot,
            SanctumVisualConfig visual,
            IMerchantIconSource icons,
            int currentGold,
            Action<MerchantSlotView> onClicked,
            Action<MerchantSlotView, bool> onHoverChanged)
        {
            Slot = slot;
            _visual = visual;
            _onClicked = onClicked;
            _onHoverChanged = onHoverChanged;

            bool hasItem = slot != null && slot.HasItem;
            bool sold = slot != null && slot.Sold;
            Affordable = hasItem && !sold && currentGold >= slot.Price;

            // 팔린 자리를 감추는 설정이면 자리째 끈다.
            bool visible = hasItem && (!sold || visual == null || !visual.HideSoldSlots);
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }

            if (!visible)
            {
                _hovering = false;
                return;
            }

            if (_icon != null)
            {
                Sprite sprite = icons != null ? icons.GetIcon(slot.Item) : null;
                if (sprite != null)
                {
                    _icon.sprite = sprite;
                }

                _icon.color = GetTint(sold);
            }

            if (_priceRoot != null)
            {
                _priceRoot.SetActive(!sold);
            }

            if (_priceLabel != null)
            {
                _priceLabel.text = slot.Price.ToString();

                // 13 가격 · 공통 규칙: 골드가 부족한 항목은 가격을 붉은 색으로 표기한다.
                _priceLabel.color = visual == null
                    ? Color.white
                    : (Affordable ? visual.PriceColor : visual.PriceNotAffordableColor);
            }

            if (_button != null)
            {
                _button.interactable = !sold;
            }

            ApplyHoverScale();
            gameObject.name = hasItem ? "Slot_" + slot.Item.Id : "Slot_빈자리";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Slot == null || !Slot.IsBuyable)
            {
                return;
            }

            _hovering = true;
            ApplyHoverScale();

            if (_onHoverChanged != null)
            {
                _onHoverChanged(this, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_hovering)
            {
                return;
            }

            _hovering = false;
            ApplyHoverScale();

            if (_onHoverChanged != null)
            {
                _onHoverChanged(this, false);
            }
        }

        private void ApplyHoverScale()
        {
            float scale = _hovering && _visual != null ? _visual.HoverScale : 1f;
            RectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private Color GetTint(bool sold)
        {
            if (_visual == null)
            {
                return Color.white;
            }

            if (sold)
            {
                return _visual.SlotSoldTint;
            }

            return Affordable ? _visual.SlotNormalTint : _visual.SlotNotAffordableTint;
        }

        private void HandleClick()
        {
            if (_onClicked != null)
            {
                _onClicked(this);
            }
        }
    }
}
