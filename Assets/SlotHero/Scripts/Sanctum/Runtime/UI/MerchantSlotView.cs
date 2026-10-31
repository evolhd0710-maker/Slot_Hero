using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SlotHero.Ui;

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
    ///
    /// 커서를 올렸을 때의 5퍼센트 확대는 같은 칸의 `HoverScale` 이 맡는다.
    /// 예전에는 여기서도 크기를 바꿔 두 곳이 같은 일을 했다. 2026년 10월 18일 검토에서 하나로 모았다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MerchantSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;

        [Tooltip("문양 상품에서 태그 둘을 우상단과 좌하단에 보여 주는 테두리. 코인과 유물에서는 감춘다(2026년 10월 19일 원재).")]
        [SerializeField] private TagCorners _tagCorners;

        [SerializeField] private Button _button;
        [SerializeField] private GameObject _priceRoot;
        [SerializeField] private TMP_Text _priceLabel;

        private SanctumVisualConfig _visual;
        private Action<MerchantSlotView> _onClicked;
        private Action<MerchantSlotView, bool> _onHoverChanged;
        private bool _hovering;

        /// <summary>이 표시가 맡은 자리.</summary>
        public MerchantSlot Slot { get; private set; }

        /// <summary>지금 골드로 살 수 있는 자리인지.</summary>
        public bool Affordable { get; private set; }

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

            // 팔렸거나 감추는 자리면 커서가 벗어났다고 먼저 알린다. 꺼진 자리에는 벗어남이 오지 않아
            // 산 물건의 상세 설명이 커서를 따라다녔다(2026년 10월 22일 코덱스 검토).
            if (!visible || sold)
            {
                EndHover();
            }

            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }

            if (!visible)
            {
                // 가격 줄은 자리의 형제라 자리를 꺼도 남는다. 함께 끈다(2026년 10월 21일 코덱스 검토).
                if (_priceRoot != null)
                {
                    _priceRoot.SetActive(false);
                }

                return;
            }

            if (_icon != null)
            {
                // 그림이 없는 물건이면 비워 둔다. 예전 물건의 그림이 남으면 다른 물건을 사게 된다.
                Sprite sprite = icons != null ? icons.GetIcon(slot.Item) : null;
                _icon.sprite = sprite;
                _icon.enabled = sprite != null;
                _icon.color = GetTint(sold);
            }

            if (_tagCorners != null)
            {
                Sprite topRight;
                Sprite bottomLeft;
                if (icons != null && icons.TryGetTagCorners(slot.Item, out topRight, out bottomLeft))
                {
                    // 진열 칸 그림이 이미 흑요석 판이라 판은 따로 깔지 않는다.
                    _tagCorners.Show(topRight, bottomLeft, null, icons.SymbolFrameInset);

                    Sprite topGlyph;
                    Sprite bottomGlyph;
                    icons.TryGetTagGlyphs(slot.Item, out topGlyph, out bottomGlyph);
                    _tagCorners.ShowGlyphs(topGlyph, bottomGlyph, icons.CornerGlyphSizeRatio, icons.CornerGlyphFromCorner);
                }
                else
                {
                    _tagCorners.Hide();
                }
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

            gameObject.name = hasItem ? "Slot_" + slot.Item.Id : "Slot_빈자리";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Slot == null || !Slot.IsBuyable)
            {
                return;
            }

            _hovering = true;

            if (_onHoverChanged != null)
            {
                _onHoverChanged(this, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            EndHover();
        }

        /// <summary>화면이 닫혀 자리가 꺼질 때도 벗어남이 오지 않는다. 띄워 둔 상세가 남지 않게 알린다.</summary>
        private void OnDisable()
        {
            EndHover();
        }

        private void EndHover()
        {
            if (!_hovering)
            {
                return;
            }

            _hovering = false;

            if (_onHoverChanged != null)
            {
                _onHoverChanged(this, false);
            }
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
