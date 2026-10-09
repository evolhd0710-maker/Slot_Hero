using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 고르기 화면 표시.
    /// 현재 빌드 화면과 비슷한 꼴로 고를 것을 늘어놓는다. 2026년 10월 5일 원재가 정했다.
    ///
    /// 유물을 고르면 유물 칸만, 코인을 고르면 코인 칸만 놓는다.
    /// 문양을 고르면 현재 빌드 화면처럼 정렬 칸과 오른쪽 태그 비중 칸이 함께 나온다.
    ///
    /// 칸 크기와 오른쪽 칸 자리는 현재 빌드 화면 설정을 그대로 쓴다. 고르기 화면에만 있는 자리는
    /// <see cref="CurrentBuildLayoutConfig"/> 의 `고르기 화면` 항목이다.
    ///
    /// 칸을 누르면 테두리로 골라지고, 같은 칸을 다시 누르면 풀린다. 아래 줄에 고른 것과 그 결과가 적히고
    /// 확인 버튼으로 정한다. 보상 화면의 카드 고르기와 같은 방식이다.
    /// </summary>
    public class ItemPickerScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private CurrentBuildLayoutConfig _layout;
        [SerializeField] private CurrentBuildVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _dim;
        [SerializeField] private TMP_Text _titleLabel;

        [Header("칸")]
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private RectTransform _scrollViewport;
        [SerializeField] private RectTransform _scrollContent;
        [SerializeField] private RectTransform _slotLayer;
        [SerializeField] private BuildSlotView _slotPrefab;

        [Header("정렬. 문양만")]
        [SerializeField] private Button _sortButton;
        [SerializeField] private Image _sortPanel;
        [SerializeField] private TMP_Text _sortLabel;
        [SerializeField] private TMP_Text _sortValueLabel;

        [Header("오른쪽 칸. 문양만")]
        [SerializeField] private Image _tagPanel;
        [SerializeField] private TMP_Text _symbolCountLabel;
        [SerializeField] private TMP_Text _tagTitleLabel;
        [SerializeField] private RectTransform _tagLayer;
        [SerializeField] private BuildTagBarView _tagBarPrefab;

        [Header("아래")]
        [SerializeField] private Image _resultPanel;
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _confirmPanel;
        [SerializeField] private TMP_Text _confirmLabel;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Image _cancelPanel;
        [SerializeField] private TMP_Text _cancelLabel;

        private readonly List<BuildSlotView> _slots = new List<BuildSlotView>();
        private readonly List<BuildTagBarView> _tagBars = new List<BuildTagBarView>();

        /// <summary>칸을 눌렀을 때.</summary>
        public event Action<BuildEntry> SlotClicked;

        /// <summary>커서가 칸의 물건 위에 올라가거나 벗어났을 때. 아이템 상세 팝업을 띄우는 데 쓴다.</summary>
        public event Action<BuildEntry, bool> SlotHoverChanged;

        /// <summary>정렬 칸을 눌렀을 때.</summary>
        public event Action SortClicked;

        /// <summary>확인을 눌렀을 때.</summary>
        public event Action ConfirmClicked;

        /// <summary>취소를 눌렀을 때.</summary>
        public event Action CancelClicked;

        /// <summary>지금 놓인 칸. 꺼진 칸은 빠진다.</summary>
        public List<BuildSlotView> GetShownSlots()
        {
            List<BuildSlotView> shown = new List<BuildSlotView>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].gameObject.activeSelf)
                {
                    shown.Add(_slots[i]);
                }
            }

            return shown;
        }

        /// <summary>확인 버튼을 누를 수 있는지.</summary>
        public bool CanConfirm
        {
            get { return _confirmButton != null && _confirmButton.interactable; }
        }

        /// <summary>취소 칸이 보이는지. 반드시 골라야 하는 요청이면 감춘다.</summary>
        public bool CanCancelShown
        {
            get { return _cancelPanel != null && _cancelPanel.gameObject.activeSelf; }
        }

        /// <summary>아래 줄에 적힌 글.</summary>
        public string ResultText
        {
            get { return _resultLabel != null ? _resultLabel.text : string.Empty; }
        }

        /// <summary>정렬 칸과 태그 비중 칸이 켜져 있는지. 문양을 고를 때만 켜진다.</summary>
        public bool ShowsSymbolPanels
        {
            get { return _tagPanel != null && _tagPanel.gameObject.activeSelf; }
        }

        private void Awake()
        {
            if (_sortButton != null)
            {
                _sortButton.onClick.AddListener(HandleSortClicked);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(HandleConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(HandleCancelClicked);
            }

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_sortButton != null)
            {
                _sortButton.onClick.RemoveListener(HandleSortClicked);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(HandleCancelClicked);
            }
        }

        /// <summary>자리와 색을 설정대로 맞춘다.</summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            PlaceTopLeft(_scrollViewport, Vector2.zero,
                new Vector2(_layout.TagPanelPosition.x, _layout.PickViewportBottom));

            if (_scroll != null)
            {
                _scroll.horizontal = false;
                _scroll.vertical = true;
            }

            PlaceLabel(_titleLabel, _layout.LeftMargin, _layout.PickTitleY);
            PlaceTopLeft(_sortPanel != null ? _sortPanel.rectTransform : null, _layout.PickSortPosition, _layout.SortSize);
            PlaceTopLeft(_tagPanel != null ? _tagPanel.rectTransform : null, _layout.TagPanelPosition, _layout.TagPanelSize);
            PlaceLabel(_symbolCountLabel,
                _layout.TagPanelPosition.x + _layout.PanelPadding,
                _layout.TagPanelPosition.y + _layout.PanelPadding);
            PlaceLabel(_tagTitleLabel,
                _layout.TagPanelPosition.x + _layout.PanelPadding,
                _layout.TagPanelPosition.y + _layout.PanelPadding + _layout.PanelTitleGap);

            PlaceTopLeft(_resultPanel != null ? _resultPanel.rectTransform : null, _layout.PickResultPosition, _layout.PickResultSize);
            PlaceTopLeft(_confirmPanel != null ? _confirmPanel.rectTransform : null, _layout.PickConfirmPosition, _layout.PickConfirmSize);
            PlaceTopLeft(_cancelPanel != null ? _cancelPanel.rectTransform : null, _layout.PickCancelPosition, _layout.PickCancelSize);

            ApplyStyle();
        }

        /// <summary>색과 글자를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_dim != null)
            {
                _dim.color = _visual.DimColor;
            }

            Paint(_sortPanel, _visual.PanelColor);
            Paint(_tagPanel, _visual.PanelColor);
            Paint(_resultPanel, _visual.PanelColor);
            Paint(_confirmPanel, _visual.PanelColor);
            Paint(_cancelPanel, _visual.PanelColor);

            Style(_titleLabel, _visual.SectionFontSize, _visual.TextColor);
            Style(_sortLabel, _visual.SectionFontSize, _visual.TextColor);
            Style(_sortValueLabel, _visual.TagFontSize, _visual.SubTextColor);
            Style(_symbolCountLabel, _visual.PanelTitleFontSize, _visual.TextColor);
            Style(_tagTitleLabel, _visual.PanelTitleFontSize, _visual.TextColor);
            Style(_resultLabel, _visual.SectionFontSize, _visual.TextColor);
            Style(_confirmLabel, _visual.SectionFontSize, _visual.TextColor);
            Style(_cancelLabel, _visual.SectionFontSize, _visual.TextColor);

            if (_cancelLabel != null)
            {
                _cancelLabel.text = _visual.PickCancelText;
            }
        }

        /// <summary>
        /// 후보를 늘어놓는다. 고른 것의 식별자를 넘기면 그 칸에 테두리를 두르고 아래 줄에 결과를 적는다.
        /// 후보는 넘긴 차례 그대로 놓는다. 문양 정렬은 조종기가 미리 한다.
        /// </summary>
        public void Show(ItemPickRequest request, BuildSortOrder order, IBuildIconSource icons, string selectedId)
        {
            if (request == null || _layout == null)
            {
                return;
            }

            bool symbols = request.Kind == ItemPickKind.Symbol;
            Vector2 slotSize = GetSlotSize(request.Kind);
            int count = request.Candidates != null ? request.Candidates.Count : 0;

            if (_titleLabel != null)
            {
                _titleLabel.text = request.Title;
            }

            while (_slots.Count < count && _slotPrefab != null && _slotLayer != null)
            {
                BuildSlotView made = Instantiate(_slotPrefab, _slotLayer);
                made.Clicked += HandleSlotClicked;
                made.HoverChanged += HandleSlotHover;
                _slots.Add(made);
            }

            BuildEntry selected = new BuildEntry();

            for (int i = 0; i < _slots.Count; i++)
            {
                BuildSlotView view = _slots[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                BuildEntry entry = request.Candidates[i];
                view.Bind(entry, slotSize, _layout, _visual, icons, symbols && request.ShowCount);
                PlaceTopLeft(view.RectTransform, _layout.GetPickSlotPosition(i, slotSize), slotSize);

                bool isSelected = !string.IsNullOrEmpty(selectedId) && entry.Id == selectedId;
                view.SetSelected(isSelected, _visual);
                if (isSelected)
                {
                    selected = entry;
                }
            }

            ApplyContentHeight(count, slotSize);

            SetActive(_sortPanel != null ? _sortPanel.gameObject : null, symbols);
            SetActive(_tagPanel != null ? _tagPanel.gameObject : null, symbols);
            SetActive(_symbolCountLabel != null ? _symbolCountLabel.gameObject : null, symbols);
            SetActive(_tagTitleLabel != null ? _tagTitleLabel.gameObject : null, symbols);
            SetActive(_tagLayer != null ? _tagLayer.gameObject : null, symbols);

            if (symbols)
            {
                SetSortLabel(order);
                BuildTagBars(request.Build);
            }

            ShowResult(request, selected);
        }

        private void ShowResult(ItemPickRequest request, BuildEntry selected)
        {
            bool has = selected.IsValid;

            if (_resultLabel != null && _visual != null)
            {
                string format = string.IsNullOrEmpty(request.ResultFormat) ? "[{0}]" : request.ResultFormat;
                _resultLabel.text = has
                    ? string.Format(CultureInfo.InvariantCulture, format, selected.DisplayName)
                    : _visual.PickNoneText;
            }

            if (_confirmLabel != null && _visual != null)
            {
                _confirmLabel.text = string.IsNullOrEmpty(request.ConfirmText) ? _visual.PickConfirmText : request.ConfirmText;
            }

            if (_cancelLabel != null && _visual != null)
            {
                _cancelLabel.text = string.IsNullOrEmpty(request.CancelText) ? _visual.PickCancelText : request.CancelText;
            }

            // 반드시 골라야 하는 요청이면 취소 칸을 감춘다.
            SetActive(_cancelPanel != null ? _cancelPanel.gameObject : null, request.Cancellable);

            if (_confirmButton != null)
            {
                _confirmButton.interactable = has;
            }

            if (_confirmPanel != null && _visual != null)
            {
                Color color = _visual.PanelColor;
                color.a *= has ? 1f : _visual.PickDisabledAlpha;
                _confirmPanel.color = color;
            }
        }

        /// <summary>칸 줄 수에 맞춰 스크롤 내용의 높이를 잡는다. 창에 다 들어가면 창 높이다.</summary>
        private void ApplyContentHeight(int count, Vector2 slotSize)
        {
            if (_scrollContent == null)
            {
                return;
            }

            float height = _layout.GetPickContentHeight(count, slotSize);
            if (height < _layout.PickViewportBottom)
            {
                height = _layout.PickViewportBottom;
            }

            _scrollContent.anchorMin = new Vector2(0f, 1f);
            _scrollContent.anchorMax = new Vector2(0f, 1f);
            _scrollContent.pivot = new Vector2(0f, 1f);
            _scrollContent.sizeDelta = new Vector2(_layout.TagPanelPosition.x, height);
            _scrollContent.anchoredPosition = Vector2.zero;

            if (_scroll != null)
            {
                _scroll.StopMovement();
            }
        }

        private Vector2 GetSlotSize(ItemPickKind kind)
        {
            switch (kind)
            {
                case ItemPickKind.Coin:
                    return _layout.GetCoinSlotSize();
                case ItemPickKind.Relic:
                    return _layout.RelicSlotSize;
                default:
                    return _layout.SymbolSlotSize;
            }
        }

        /// <summary>오른쪽 태그 비중 칸. 지금 가진 문양으로 센다.</summary>
        private void BuildTagBars(CurrentBuildSnapshot build)
        {
            if (_tagLayer == null || _tagBarPrefab == null || build == null)
            {
                return;
            }

            if (_symbolCountLabel != null && _visual != null)
            {
                _symbolCountLabel.text = string.Format(CultureInfo.InvariantCulture, _visual.SymbolCountFormat, build.TotalSymbolCount);
            }

            List<BuildTagStat> stats = build.GetTagStats();

            while (_tagBars.Count < stats.Count)
            {
                _tagBars.Add(Instantiate(_tagBarPrefab, _tagLayer));
            }

            float width = _layout.TagBarX + _layout.TagBarSize.x - _layout.TagSymbolX;

            for (int i = 0; i < _tagBars.Count; i++)
            {
                BuildTagBarView bar = _tagBars[i];
                if (bar == null)
                {
                    continue;
                }

                if (i >= stats.Count)
                {
                    bar.gameObject.SetActive(false);
                    continue;
                }

                bar.Bind(stats[i], build.GetTagRatio(stats[i].Tag), _layout, _visual);
                Vector2 position = _layout.GetTagBarPosition(i);
                PlaceTopLeft(
                    bar.RectTransform,
                    new Vector2(_layout.TagSymbolX, position.y - _layout.TagRowHeight + _layout.TagBarSize.y),
                    new Vector2(width, _layout.TagRowHeight));
            }
        }

        private void SetSortLabel(BuildSortOrder order)
        {
            if (_sortValueLabel == null)
            {
                return;
            }

            switch (order)
            {
                case BuildSortOrder.Count:
                    _sortValueLabel.text = "개수 순";
                    break;
                case BuildSortOrder.Tag:
                    _sortValueLabel.text = "태그 순";
                    break;
                default:
                    _sortValueLabel.text = "이름 순";
                    break;
            }
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null && target.activeSelf != value)
            {
                target.SetActive(value);
            }
        }

        private static void Paint(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
        }

        private static void Style(TMP_Text label, float fontSize, Color color)
        {
            if (label != null)
            {
                label.fontSize = fontSize;
                label.color = color;
            }
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다.</summary>
        private static void PlaceTopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(position.x, -position.y);
        }

        /// <summary>글자 하나를 화면 왼쪽 위 기준 자리에 놓는다. 크기도 함께 잡는다.</summary>
        private void PlaceLabel(TMP_Text label, float x, float y)
        {
            if (label != null)
            {
                PlaceTopLeft(label.rectTransform, new Vector2(x, y), _layout.LabelSize);
            }
        }

        private void HandleSlotClicked(BuildSlotView view)
        {
            if (view != null && SlotClicked != null)
            {
                SlotClicked(view.Entry);
            }
        }

        private void HandleSlotHover(BuildSlotView view, bool hovering)
        {
            if (view != null && SlotHoverChanged != null)
            {
                SlotHoverChanged(view.Entry, hovering);
            }
        }

        private void HandleSortClicked()
        {
            if (SortClicked != null)
            {
                SortClicked();
            }
        }

        private void HandleConfirmClicked()
        {
            if (ConfirmClicked != null)
            {
                ConfirmClicked();
            }
        }

        private void HandleCancelClicked()
        {
            if (CancelClicked != null)
            {
                CancelClicked();
            }
        }
    }
}
