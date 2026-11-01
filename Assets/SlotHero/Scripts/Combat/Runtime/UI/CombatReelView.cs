using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.CurrentBuild;
using SlotHero.Ui;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 릴 한 칸. 문양 그림과 모서리의 태그 둘을 그린다.
    ///
    /// 1.0.b 초안 07 을 따라 도는 동안에는 문양 그림만 보이고, 멈춘 뒤 태그가 함께 나온다.
    /// 태그를 호버하거나 고르면 무채색으로 그리던 피그마 릴 틀에 그 태그 색을 입혀 강조한다(초안 06).
    /// 예전에는 칸 바깥에 태그 색 사각 테두리를 따로 둘렀다. 2026년 10월 31일 원재가 릴 틀에 색을 입히는 것으로 바꿨다.
    ///
    /// 태그는 다른 화면의 문양 칸과 같은 태그 테두리(`TagCorners`)로 보인다(2026년 10월 26일 원재).
    /// 흑요석 판 위에 문양을 얹고 우상단에 첫 태그, 좌하단에 둘째 태그의 테두리와 행성 기호를 두른다.
    /// 아래 모서리 태그 원은 예전 방식이다. 씬에 그 칸이 있을 때만 함께 그린다.
    /// </summary>
    public class CombatReelView : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;

        [Tooltip("흑요석 판과 태그 테두리. 다른 화면의 문양 칸과 같은 부품이다.")]
        [SerializeField] private TagCorners _tagCorners;

        [Tooltip("피그마 릴 틀 그림. 모든 릴이 회색 틀 한 장(`릴틀_3`)을 쓰고 무채색 재질로 그린다(2026년 11월 1일 원재). " +
                 "기본은 어두운 색을 곱하고, 강조할 때 이 틀에 태그 색을 입힌다.")]
        [SerializeField] private Image _frame;
        public Image Frame { get { return _frame; } }

        [Tooltip("강조한 틀 둘레에 번지는 빛(`릴틀_빛`). 태그 색을 곱하고 밝기가 오르내린다(2026년 11월 1일 원재). 비우면 빛 없이 틀 색만 바꾼다.")]
        [SerializeField] private Image _glow;
        public Image Glow { get { return _glow; } }

        [Tooltip("강조한 틀 위에 덧칠하는 틀 띠 가장자리 선(`릴틀_흰`). 밝힌 태그 색을 곱하고 빛과 함께 오르내린다(2026년 11월 1일 원재). 비우면 덧칠 없이 빛만 낸다.")]
        [SerializeField] private Image _shine;
        public Image Shine { get { return _shine; } }

        // 틀 표시. 전투 화면이 표시 설정에서 넣어 준다.
        private CombatVisualConfig _style;
        private Color _idleFrameColor = Color.white;

        // 강조 중인지와 그 태그 색. 빛의 밝기를 매 장면 다시 칠한다.
        private bool _highlighted;
        private Color _highlightColor;
        private float _glowTime;

        [Header("태그")]
        [SerializeField] private Image _firstTagBack;
        [SerializeField] private Image _firstTagIcon;
        [SerializeField] private TMP_Text _firstTagLabel;
        [SerializeField] private Image _secondTagBack;
        [SerializeField] private Image _secondTagIcon;
        [SerializeField] private TMP_Text _secondTagLabel;

        public RectTransform RectTransform
        {
            get { return (RectTransform)transform; }
        }

        /// <summary>멈춘 문양을 그린다. tags 가 있으면 흑요석 판과 태그 테두리를 함께 두른다.</summary>
        public void Show(ReelSymbol symbol, Sprite icon, BuildTagVisual first, BuildTagVisual second, CurrentBuildVisualConfig tags)
        {
            SetIcon(icon, symbol.DisplayName);
            SetTag(_firstTagBack, _firstTagIcon, _firstTagLabel, first, true);
            SetTag(_secondTagBack, _secondTagIcon, _secondTagLabel, second, true);
            ShowTagCorners(symbol, tags);
        }

        /// <summary>도는 동안의 한 장면. 문양 그림만 보인다.</summary>
        public void ShowSpinning(Sprite icon, string displayName)
        {
            SetIcon(icon, displayName);
            SetTag(_firstTagBack, _firstTagIcon, _firstTagLabel, new BuildTagVisual(), false);
            SetTag(_secondTagBack, _secondTagIcon, _secondTagLabel, new BuildTagVisual(), false);

            if (_tagCorners != null)
            {
                _tagCorners.Hide();
            }
        }

        /// <summary>
        /// 다른 화면의 문양 칸과 같이 판을 깔고 태그 테두리와 기호를 두른다.
        /// 첫 태그는 우상단, 둘째 태그는 좌하단이다(2026년 10월 19일, 26일 원재).
        /// </summary>
        private void ShowTagCorners(ReelSymbol symbol, CurrentBuildVisualConfig tags)
        {
            if (_tagCorners == null)
            {
                return;
            }

            if (tags == null)
            {
                _tagCorners.Hide();
                return;
            }

            Sprite topRight;
            Sprite bottomLeft;
            tags.GetTagCorners(symbol.FirstTag, symbol.SecondTag, out topRight, out bottomLeft);
            _tagCorners.Show(topRight, bottomLeft, tags.GetSymbolPlate(symbol.Id), tags.SymbolFrameInset);

            Sprite topGlyph;
            Sprite bottomGlyph;
            tags.GetTagGlyphs(symbol.FirstTag, symbol.SecondTag, out topGlyph, out bottomGlyph);
            _tagCorners.ShowGlyphs(topGlyph, bottomGlyph, tags.CornerGlyphSizeRatio, tags.CornerGlyphFromCorner);
        }

        /// <summary>
        /// 릴 틀에 태그 색을 입히거나 걷는다. 걷으면 기본 틀 색(어두운 회색)으로 돌아간다.
        /// 무채색 재질은 회색에 이 색을 곱해 그리므로 틀이 그 태그 색으로 물든다(2026년 10월 31일 원재).
        /// 강조하는 동안 틀 둘레의 빛이 같은 태그 색으로 밝아졌다 어두워지며 반짝인다(2026년 11월 1일 원재).
        /// </summary>
        public void SetHighlight(bool on, Color color)
        {
            bool wasHighlighted = _highlighted;
            _highlighted = on;
            _highlightColor = new Color(color.r, color.g, color.b, 1f);
            if (on && !wasHighlighted)
            {
                _glowTime = 0f;
            }

            if (_frame != null)
            {
                _frame.color = on ? _highlightColor : _idleFrameColor;
            }

            PaintGlow();
        }

        /// <summary>
        /// 틀의 기본 색과 강조 빛의 반짝임을 정한다. 전투 화면이 표시 설정을 넣는다.
        /// </summary>
        public void SetFrameStyle(CombatVisualConfig style)
        {
            _style = style;
            _idleFrameColor = style != null ? style.ReelFrameIdleColor : Color.white;

            if (_frame != null && !_highlighted)
            {
                _frame.color = _idleFrameColor;
            }

            PaintGlow();
        }

        private void Update()
        {
            if (!_highlighted || (_glow == null && _shine == null))
            {
                return;
            }

            _glowTime += Time.unscaledDeltaTime;
            PaintGlow();
        }

        /// <summary>
        /// 강조 빛과 덧칠을 지금 시각의 밝기로 칠한다. 처음에 가장 밝고 코사인으로 오르내린다.
        /// 색은 태그 색을 흰색 쪽으로 밝힌 것이다(2026년 11월 1일 원재).
        /// </summary>
        private void PaintGlow()
        {
            SetActive(_glow, _highlighted);
            SetActive(_shine, _highlighted);
            if (!_highlighted || _style == null)
            {
                return;
            }

            float pulse = Mathf.Max(0.05f, _style.ReelGlowPulseSeconds);
            float wave = 0.5f + 0.5f * Mathf.Cos(_glowTime / pulse * Mathf.PI * 2f);
            Color bright = _style.GetReelGlowColor(_highlightColor);

            if (_glow != null)
            {
                Color glow = bright;
                glow.a = Mathf.Lerp(_style.ReelGlowMinAlpha, _style.ReelGlowMaxAlpha, wave);
                _glow.color = glow;
            }

            if (_shine != null)
            {
                Color shine = bright;
                shine.a = Mathf.Lerp(_style.ReelShineMinAlpha, _style.ReelShineMaxAlpha, wave);
                _shine.color = shine;
            }
        }

        private static void SetActive(Image image, bool on)
        {
            if (image != null && image.gameObject.activeSelf != on)
            {
                image.gameObject.SetActive(on);
            }
        }

        /// <summary>지금 릴 틀에 입힌 색. 강조하지 않으면 기본 틀 색이다.</summary>
        public Color FrameColor
        {
            get { return _frame != null ? _frame.color : Color.white; }
        }

        /// <summary>릴 틀 그림.</summary>
        public Sprite FrameSprite
        {
            get { return _frame != null ? _frame.sprite : null; }
        }

        /// <summary>강조하지 않은 틀에 곱하는 색.</summary>
        public Color IdleFrameColor
        {
            get { return _idleFrameColor; }
        }

        /// <summary>틀 둘레의 강조 빛이 켜져 있는지.</summary>
        public bool GlowVisible
        {
            get { return _glow != null && _glow.gameObject.activeSelf; }
        }

        /// <summary>틀 둘레 강조 빛의 지금 색. 불투명도가 반짝임에 따라 오르내린다.</summary>
        public Color GlowColor
        {
            get { return _glow != null ? _glow.color : Color.clear; }
        }

        /// <summary>틀 위 덧칠이 켜져 있는지.</summary>
        public bool ShineVisible
        {
            get { return _shine != null && _shine.gameObject.activeSelf; }
        }

        /// <summary>강조 빛의 시각을 옮긴다. 검사가 반짝임을 확인할 때 쓴다.</summary>
        public void AdvanceGlow(float seconds)
        {
            _glowTime += seconds;
            PaintGlow();
        }

        /// <summary>
        /// 피그마 릴 틀에 입힐 재질을 정한다. 무채색 재질이면 회색으로 그린다(2026년 10월 31일 원재).
        /// 태그를 호버하거나 그 태그로 공격할 때 `SetHighlight` 가 태그 색을 입힌다.
        /// 문양의 태그를 보여 주는 태그 테두리와 기호는 늘 제 색이라 건드리지 않는다(2026년 10월 31일 원재).
        /// </summary>
        public void SetFrameMaterial(Material material)
        {
            if (_frame != null)
            {
                _frame.material = material;
            }
        }

        /// <summary>바탕색을 정한다.</summary>
        public void SetBackground(Color color)
        {
            if (_background != null)
            {
                _background.color = color;
            }
        }

        private void SetIcon(Sprite icon, string displayName)
        {
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.enabled = icon != null;
                _icon.preserveAspect = true;
            }

            // 그림이 없으면 이름이라도 보이게 한다.
            if (_nameLabel != null)
            {
                _nameLabel.text = icon == null ? displayName : string.Empty;
            }
        }

        private static void SetTag(Image back, Image icon, TMP_Text label, BuildTagVisual visual, bool visible)
        {
            if (back != null)
            {
                back.gameObject.SetActive(visible);
                back.color = visual.BarColor;
            }

            if (icon != null)
            {
                icon.sprite = visual.Icon;
                icon.enabled = visible && visual.Icon != null;
                icon.preserveAspect = true;
            }

            if (label != null)
            {
                label.text = visible && visual.Icon == null ? visual.Symbol : string.Empty;
            }
        }
    }
}
