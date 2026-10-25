using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.CurrentBuild;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 릴 한 칸. 문양 그림과 모서리의 태그 둘을 그린다.
    ///
    /// 1.0.b 초안 07 을 따라 도는 동안에는 문양 그림만 보이고, 멈춘 뒤 태그가 함께 나온다.
    /// 강조 테두리는 태그를 호버하거나 고르면 그 태그 색으로 두른다(초안 06).
    /// </summary>
    public class CombatReelView : MonoBehaviour
    {
        [SerializeField] private Image _highlight;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;

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

        /// <summary>멈춘 문양을 그린다.</summary>
        public void Show(ReelSymbol symbol, Sprite icon, BuildTagVisual first, BuildTagVisual second)
        {
            SetIcon(icon, symbol.DisplayName);
            SetTag(_firstTagBack, _firstTagIcon, _firstTagLabel, first, true);
            SetTag(_secondTagBack, _secondTagIcon, _secondTagLabel, second, true);
        }

        /// <summary>도는 동안의 한 장면. 문양 그림만 보인다.</summary>
        public void ShowSpinning(Sprite icon, string displayName)
        {
            SetIcon(icon, displayName);
            SetTag(_firstTagBack, _firstTagIcon, _firstTagLabel, new BuildTagVisual(), false);
            SetTag(_secondTagBack, _secondTagIcon, _secondTagLabel, new BuildTagVisual(), false);
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
