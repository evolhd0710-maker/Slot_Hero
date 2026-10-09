using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Combat;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 태그 한 줄. 기호와 이름, 장수, 막대로 이뤄진다.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의
    /// "태그마다 고유한 색을 사용하며 보유하지 않은 태그는 막대를 비운다"를 맡는다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트,
    /// 자식에 기호 Image, 기호 TMP_Text, 이름 TMP_Text, 장수 TMP_Text,
    /// 막대 바닥 Image 와 그 안의 막대 Image.
    ///
    /// 기호는 그림과 글자 둘 중 하나로 나온다.
    /// 표시 설정에 그림이 들어 있으면 그림을 쓰고 글자는 감춘다.
    /// 그림이 없으면 글자를 쓴다. 다만 글꼴에 행성 기호가 없으면 네모로 나온다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BuildTagBarView : MonoBehaviour
    {
        [SerializeField] private Image _symbolIcon;
        [SerializeField] private TMP_Text _symbolLabel;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private Image _barBackground;
        [SerializeField] private Image _bar;

        private RectTransform _rectTransform;

        /// <summary>이 줄이 맡은 태그.</summary>
        public SymbolTagType Tag { get; private set; }

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

        /// <summary>태그 하나의 통계를 반영한다. ratio 는 전체 문양 중 그 태그가 차지하는 비율이다.</summary>
        public void Bind(
            BuildTagStat stat,
            float ratio,
            CurrentBuildLayoutConfig layout,
            CurrentBuildVisualConfig visual)
        {
            Tag = stat.Tag;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (visual == null || layout == null)
            {
                return;
            }

            BuildTagVisual tagVisual = visual.GetTagVisual(stat.Tag);
            bool empty = stat.Count <= 0;
            Color textColor = empty ? visual.EmptyTagTextColor : visual.SubTextColor;

            // 기호는 그림이 있으면 그림을, 없으면 글자를 쓴다.
            bool hasIcon = tagVisual.Icon != null;

            if (_symbolIcon != null)
            {
                _symbolIcon.gameObject.SetActive(hasIcon);

                if (hasIcon)
                {
                    _symbolIcon.sprite = tagVisual.Icon;
                    _symbolIcon.color = textColor;
                }
            }

            if (_symbolLabel != null)
            {
                _symbolLabel.gameObject.SetActive(!hasIcon);
                _symbolLabel.text = tagVisual.Symbol;
                _symbolLabel.fontSize = visual.TagSymbolFontSize;
                _symbolLabel.color = textColor;
            }

            if (_nameLabel != null)
            {
                _nameLabel.text = tagVisual.DisplayName;
                _nameLabel.fontSize = visual.TagFontSize;
                _nameLabel.color = textColor;
            }

            if (_countLabel != null)
            {
                _countLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    visual.TagCountFormat,
                    stat.Count);
                _countLabel.fontSize = visual.TagFontSize;
                _countLabel.color = textColor;
            }

            if (_barBackground != null)
            {
                _barBackground.color = visual.TagBarBackColor;
                _barBackground.rectTransform.sizeDelta = layout.TagBarSize;
            }

            if (_bar != null)
            {
                // 가지지 않은 태그는 길이가 0이 되어 막대가 비워진다.
                float width = layout.GetTagBarWidth(ratio);
                RectTransform barRect = _bar.rectTransform;
                barRect.anchorMin = new Vector2(0f, 0.5f);
                barRect.anchorMax = new Vector2(0f, 0.5f);
                barRect.pivot = new Vector2(0f, 0.5f);
                barRect.anchoredPosition = Vector2.zero;
                barRect.sizeDelta = new Vector2(width, layout.TagBarSize.y);

                _bar.color = tagVisual.BarColor;
                _bar.enabled = width > 0f;
            }

            gameObject.name = "Tag_" + tagVisual.DisplayName;
        }
    }
}
