using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 물건을 놓는 칸 하나.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 유물, 코인, 문양 칸이 모두 이것을 쓴다.
    ///
    /// 문양 칸만 개수 배지를 함께 보여 준다.
    /// 기획서의 "종류마다 한 칸에 개수를 함께 표시한다"에 해당한다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 배경 Image,
    /// 자식에 물건 그림 Image, 개수 배지(Image + TMP_Text).
    ///
    /// 커서가 물건 위에 올라가면 알린다. 기획서의 아이템 상세 팝업을 띄우는 데 쓴다. 빈 칸은 알리지 않는다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BuildSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        /// <summary>커서가 물건 위에 올라가거나 벗어났을 때.</summary>
        public event Action<BuildSlotView, bool> HoverChanged;

        /// <summary>물건이 놓인 칸을 눌렀을 때. 고르기 화면이 쓴다. 현재 빌드 화면에서는 아무도 듣지 않는다.</summary>
        public event Action<BuildSlotView> Clicked;

        [Tooltip("고른 칸의 테두리. 배경 뒤에 깔려 고르면 바깥으로 삐져나온다. 고르기 화면에서만 켠다.")]
        [SerializeField] private Image _selectedBorder;

        private bool _selected;

        /// <summary>고른 칸인지.</summary>
        public bool IsSelected
        {
            get { return _selected; }
        }

        /// <summary>고른 칸인지 정하고 테두리를 켜고 끈다.</summary>
        public void SetSelected(bool selected, CurrentBuildVisualConfig visual)
        {
            _selected = selected;

            if (_selectedBorder == null)
            {
                return;
            }

            // 테두리는 칸에 꼭 맞게 두고, 고른 동안만 배경을 두께만큼 안으로 줄여 테두리가 드러나게 한다.
            // 보상 화면의 고른 카드와 같은 방식이다. 테두리를 칸보다 크게 펼치면 펼친 칸 감사에 걸린다.
            float thickness = visual != null ? visual.PickSelectedBorderThickness : 0f;

            if (visual != null)
            {
                _selectedBorder.color = visual.PickSelectedBorderColor;
            }

            if (_background != null)
            {
                RectTransform back = _background.rectTransform;
                float inset = selected ? thickness : 0f;
                back.offsetMin = new Vector2(inset, inset);
                back.offsetMax = new Vector2(-inset, -inset);
            }

            if (_selectedBorder.gameObject.activeSelf != selected)
            {
                _selectedBorder.gameObject.SetActive(selected);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Press();
        }

        /// <summary>칸을 누른 셈 친다. 물건이 없는 칸은 알리지 않는다.</summary>
        public void Press()
        {
            if (Entry.IsValid && Clicked != null)
            {
                Clicked(this);
            }
        }

        private bool _hovering;

        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _countRoot;
        [SerializeField] private Image _countBackground;
        [SerializeField] private TMP_Text _countLabel;

        private RectTransform _rectTransform;

        /// <summary>이 칸에 놓인 것.</summary>
        public BuildEntry Entry { get; private set; }

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

        /// <summary>
        /// 칸의 내용을 반영한다.
        /// showCount 를 끄면 개수 배지를 감춘다. 유물과 코인 칸이 그렇다.
        /// </summary>
        public void Bind(
            BuildEntry entry,
            Vector2 slotSize,
            CurrentBuildLayoutConfig layout,
            CurrentBuildVisualConfig visual,
            IBuildIconSource icons,
            bool showCount)
        {
            Entry = entry;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RectTransform.sizeDelta = slotSize;

            if (_background != null && visual != null)
            {
                _background.color = visual.SlotColor;
            }

            if (_icon != null)
            {
                Sprite sprite = entry.IsValid && icons != null ? icons.GetIcon(entry) : null;
                if (sprite != null)
                {
                    _icon.sprite = sprite;
                }

                _icon.preserveAspect = true;
                _icon.enabled = entry.IsValid && _icon.sprite != null;
            }

            BindCount(entry, slotSize, layout, visual, showCount);
            gameObject.name = entry.IsValid ? "Slot_" + entry.Id : "Slot_빈칸";
        }

        /// <summary>칸을 비운다. 아직 얻지 못한 자리다.</summary>
        public void Clear(Vector2 slotSize, CurrentBuildVisualConfig visual)
        {
            Entry = new BuildEntry();

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RectTransform.sizeDelta = slotSize;

            if (_background != null && visual != null)
            {
                _background.color = visual.SlotColor;
            }

            if (_icon != null)
            {
                _icon.enabled = false;
            }

            if (_countRoot != null)
            {
                _countRoot.SetActive(false);
            }

            gameObject.name = "Slot_빈칸";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Entry.IsValid)
            {
                return;
            }

            _hovering = true;
            if (HoverChanged != null)
            {
                HoverChanged(this, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            EndHover();
        }

        private void OnDisable()
        {
            // 화면이 닫히면 벗어난 것을 받지 못한다. 띄워 둔 상세가 남지 않게 알린다.
            EndHover();
        }

        private void EndHover()
        {
            if (!_hovering)
            {
                return;
            }

            _hovering = false;
            if (HoverChanged != null)
            {
                HoverChanged(this, false);
            }
        }

        private void BindCount(
            BuildEntry entry,
            Vector2 slotSize,
            CurrentBuildLayoutConfig layout,
            CurrentBuildVisualConfig visual,
            bool showCount)
        {
            if (_countRoot == null)
            {
                return;
            }

            bool visible = showCount && entry.IsValid;
            if (_countRoot.activeSelf != visible)
            {
                _countRoot.SetActive(visible);
            }

            if (!visible || layout == null)
            {
                return;
            }

            RectTransform badge = (RectTransform)_countRoot.transform;

            // 배지는 칸의 오른쪽 아래 모서리에서 조금 안쪽에 놓는다.
            badge.anchorMin = new Vector2(1f, 0f);
            badge.anchorMax = new Vector2(1f, 0f);
            badge.pivot = new Vector2(1f, 0f);
            badge.sizeDelta = layout.CountBadgeSize;
            badge.anchoredPosition = new Vector2(-layout.CountBadgeInset, layout.CountBadgeInset);

            if (_countBackground != null && visual != null)
            {
                _countBackground.color = visual.CountBadgeColor;
            }

            if (_countLabel != null && visual != null)
            {
                _countLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    visual.CountBadgeFormat,
                    entry.Count);
                _countLabel.fontSize = visual.CountFontSize;
                _countLabel.color = visual.TextColor;
            }
        }
    }
}
