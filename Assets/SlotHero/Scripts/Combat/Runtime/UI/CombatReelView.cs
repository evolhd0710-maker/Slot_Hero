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
    /// 강조 테두리는 태그를 호버하거나 고르면 그 태그 색으로 두른다(초안 06).
    ///
    /// 태그는 다른 화면의 문양 칸과 같은 태그 테두리(`TagCorners`)로 보인다(2026년 10월 26일 원재).
    /// 흑요석 판 위에 문양을 얹고 우상단에 첫 태그, 좌하단에 둘째 태그의 테두리와 행성 기호를 두른다.
    /// 아래 모서리 태그 원은 예전 방식이다. 씬에 그 칸이 있을 때만 함께 그린다.
    /// </summary>
    public class CombatReelView : MonoBehaviour
    {
        [SerializeField] private Image _highlight;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;

        [Tooltip("흑요석 판과 태그 테두리. 다른 화면의 문양 칸과 같은 부품이다.")]
        [SerializeField] private TagCorners _tagCorners;

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

        /// <summary>강조 테두리를 두르거나 걷는다.</summary>
        public void SetHighlight(bool on, Color color)
        {
            if (_highlight == null)
            {
                return;
            }

            _highlight.gameObject.SetActive(on);
            _highlight.color = color;
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
