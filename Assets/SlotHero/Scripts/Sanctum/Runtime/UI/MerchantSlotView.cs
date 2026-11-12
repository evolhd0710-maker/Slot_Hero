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

        [Tooltip("유물 진열을 새로 고칠 때 물건 그림 뒤에서 뭉치는 암흑물질 구름. 문양과 코인 칸에서는 쓰지 않는다.")]
        [SerializeField] private Image _darkMatter;

        // 암흑물질에서 형체가 생기는 연출(2026년 10월 31일 원재). 늦게 시작하는 시간과 지난 시간.
        private bool _forming;
        private float _formTime;
        private float _formDelay;

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

        /// <summary>
        /// 새 물건이 암흑물질에서 형체를 갖추는 연출을 시작한다. 유물 진열을 새로 고칠 때 부른다(2026년 10월 31일 원재).
        /// ① 보랏빛 검은 구름이 크게 번지며 뭉치고 ② 그 안에서 검은 그림자가 줄어들며 또렷해지고
        /// ③ 그림자에 색이 돌아오면서 구름이 흩어진다. order 번째 칸은 칸마다 늦는 시간 × order 만큼 늦게 시작한다.
        /// 게임이 돌 때만 움직인다.
        /// </summary>
        public void PlayForming(int order)
        {
            if (!Application.isPlaying || _visual == null || !isActiveAndEnabled || Slot == null || Slot.Sold)
            {
                return;
            }

            _forming = true;
            _formTime = 0f;
            _formDelay = Mathf.Max(0f, _visual.RelicFormStagger * order);
            ApplyForming(0f);

            // 연출 중 화면을 누르면 바로 다 생긴 모습이 된다(2026년 11월 1일 원재).
            EffectSkip.Begin(this, StopForming);
        }

        /// <summary>연출이 도는 중인지.</summary>
        public bool Forming
        {
            get { return _forming; }
        }

        private void Update()
        {
            if (!_forming)
            {
                return;
            }

            _formTime += EffectClock.Delta;
            float seconds = _visual != null ? _visual.RelicFormSeconds : 0f;
            float t = seconds <= 0f ? 1f : Mathf.Clamp01((_formTime - _formDelay) / seconds);
            ApplyForming(t);

            if (t >= 1f)
            {
                StopForming();
            }
        }

        private void StopForming()
        {
            EffectSkip.End(this);
            _forming = false;

            if (_darkMatter != null)
            {
                _darkMatter.gameObject.SetActive(false);
            }

            if (_icon != null)
            {
                _icon.rectTransform.localScale = Vector3.one;
                _icon.color = GetTint(Slot != null && Slot.Sold);
            }
        }

        private void ApplyForming(float t)
        {
            FormState state = GetFormState(t);

            if (_darkMatter != null && _visual != null)
            {
                Color cloud = _visual.DarkMatterColor;
                _darkMatter.gameObject.SetActive(state.CloudAlpha > 0f);
                _darkMatter.color = new Color(cloud.r, cloud.g, cloud.b, cloud.a * state.CloudAlpha);
                _darkMatter.rectTransform.localScale = new Vector3(state.CloudScale, state.CloudScale, 1f);
                _darkMatter.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -120f * t);
            }

            if (_icon != null)
            {
                Color tint = GetTint(false);
                Color shown = Color.Lerp(new Color(0f, 0f, 0f, tint.a), tint, state.ColorAmount);
                shown.a *= state.IconAlpha;
                _icon.color = shown;
                _icon.rectTransform.localScale = new Vector3(state.IconScale, state.IconScale, 1f);
            }
        }

        /// <summary>형체가 생기는 연출의 한 순간.</summary>
        public struct FormState
        {
            public float CloudAlpha;
            public float CloudScale;
            public float IconAlpha;
            public float IconScale;

            /// <summary>0 이면 검은 그림자, 1 이면 원래 색이다.</summary>
            public float ColorAmount;
        }

        /// <summary>
        /// t(0 ~ 1) 때의 모습.
        /// 구름은 0 ~ 0.35 에 1.6 배에서 1 배로 뭉치며 짙어지고, 0.6 ~ 1 에 0.7 배로 줄며 흩어진다.
        /// 그림자는 0.25 ~ 0.65 에 1.25 배에서 1 배로 줄며 또렷해지고, 0.6 ~ 1 에 색이 돌아온다.
        /// </summary>
        public static FormState GetFormState(float t)
        {
            float clamped = Mathf.Clamp01(t);
            FormState state = new FormState();

            float gather = Mathf.Clamp01(clamped / 0.35f);
            float fade = Mathf.Clamp01((clamped - 0.6f) / 0.4f);
            state.CloudAlpha = gather * (1f - fade);
            state.CloudScale = clamped < 0.6f
                ? Mathf.Lerp(1.6f, 1f, 1f - (1f - gather) * (1f - gather))
                : Mathf.Lerp(1f, 0.7f, fade);

            float shape = Mathf.Clamp01((clamped - 0.25f) / 0.4f);
            state.IconAlpha = shape;
            state.IconScale = Mathf.Lerp(1.25f, 1f, 1f - (1f - shape) * (1f - shape));
            state.ColorAmount = fade;
            return state;
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

        /// <summary>
        /// 화면이 닫혀 자리가 꺼질 때도 벗어남이 오지 않는다. 띄워 둔 상세가 남지 않게 알린다.
        /// 형체가 생기는 연출 도중이면 끝 모습으로 둔다.
        /// </summary>
        private void OnDisable()
        {
            EndHover();
            StopForming();
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
