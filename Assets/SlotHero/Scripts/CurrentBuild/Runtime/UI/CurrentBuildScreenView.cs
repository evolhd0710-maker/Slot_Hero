using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 현재 빌드 화면 표시.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 배치를 맡는다.
    /// 1열 유물 6칸, 2열 코인 5칸, 3열부터 문양 가로 8칸,
    /// 오른쪽 칸에 문양 개수와 태그 9종, 오른쪽 아래에 닫기다.
    ///
    /// 칸은 수가 정해져 있지 않으므로 프리팹을 복제해 만든다.
    /// </summary>
    public class CurrentBuildScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private CurrentBuildLayoutConfig _layout;
        [SerializeField] private CurrentBuildVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _dim;

        [Header("줄 이름")]
        [SerializeField] private TMP_Text _relicLabel;
        [SerializeField] private TMP_Text _coinLabel;
        [SerializeField] private TMP_Text _symbolLabel;

        [Header("스크롤")]
        [Tooltip("왼쪽 줄들을 담는 스크롤. 문양이 많아 화면을 넘칠 때 세로로 훑는다.")]
        [SerializeField] private ScrollRect _scroll;

        [Tooltip("스크롤이 훑는 창. 오른쪽 칸과 닫기는 이 밖에 두어 늘 보이게 한다.")]
        [SerializeField] private RectTransform _scrollViewport;

        [Tooltip("스크롤 안에서 움직이는 내용. 유물과 코인, 문양 줄이 모두 이 아래에 있다.")]
        [SerializeField] private RectTransform _scrollContent;

        [Header("칸")]
        [SerializeField] private RectTransform _relicLayer;
        [SerializeField] private RectTransform _coinLayer;
        [SerializeField] private RectTransform _symbolLayer;
        [SerializeField] private BuildSlotView _slotPrefab;

        [Header("정렬")]
        [SerializeField] private Button _sortButton;
        [SerializeField] private Image _sortPanel;
        [SerializeField] private TMP_Text _sortLabel;
        [SerializeField] private TMP_Text _sortValueLabel;

        [Header("오른쪽 칸")]
        [SerializeField] private Image _tagPanel;
        [SerializeField] private TMP_Text _symbolCountLabel;
        [SerializeField] private TMP_Text _tagTitleLabel;
        [SerializeField] private RectTransform _tagLayer;
        [SerializeField] private BuildTagBarView _tagBarPrefab;

        [Header("닫기")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _closePanel;
        [SerializeField] private TMP_Text _closeLabel;
        [SerializeField] private TMP_Text _closeShortcutLabel;

        private readonly List<BuildSlotView> _relicSlots = new List<BuildSlotView>();
        private readonly List<BuildSlotView> _coinSlots = new List<BuildSlotView>();
        private readonly List<BuildSlotView> _symbolSlots = new List<BuildSlotView>();
        private readonly List<BuildTagBarView> _tagBars = new List<BuildTagBarView>();

        /// <summary>정렬 칸을 눌렀을 때.</summary>
        public event Action SortClicked;

        /// <summary>닫기를 눌렀을 때.</summary>
        public event Action CloseClicked;

        /// <summary>커서가 칸의 물건 위에 올라가거나 벗어났을 때. 아이템 상세 팝업을 띄우는 데 쓴다.</summary>
        public event Action<BuildEntry, bool> SlotHoverChanged;

        /// <summary>칸을 하나 만들고 커서 알림을 잇는다.</summary>
        private BuildSlotView MakeSlot(RectTransform layer)
        {
            BuildSlotView view = Instantiate(_slotPrefab, layer);
            view.HoverChanged += HandleSlotHover;
            return view;
        }

        private void HandleSlotHover(BuildSlotView view, bool hovering)
        {
            if (view != null && SlotHoverChanged != null)
            {
                SlotHoverChanged(view.Entry, hovering);
            }
        }

        private void Awake()
        {
            if (_sortButton != null)
            {
                _sortButton.onClick.AddListener(HandleSortClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(HandleCloseClicked);
            }

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_sortButton != null)
            {
                _sortButton.onClick.RemoveListener(HandleSortClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(HandleCloseClicked);
            }
        }

        /// <summary>
        /// 기획서 13장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.TagRowsFitPanel())
            {
                Debug.LogWarning(
                    "태그 아홉 줄이 오른쪽 칸 높이를 넘어간다. 줄 높이나 칸 크기를 다시 본다.",
                    this);
            }

            if (!_layout.SymbolSlotsFitScreen() && _scroll == null)
            {
                Debug.LogWarning(
                    "문양 유효 슬롯 " + _layout.SymbolSlotLimit + "개를 다 채우면 화면 아래로 넘어가는데 " +
                    "스크롤이 물려 있지 않다. 지금 칸 크기로 한 화면에 들어가는 것은 " +
                    _layout.GetSymbolRowsInScreen() * _layout.SymbolColumns + "칸까지다.",
                    this);
            }

            ApplyScrollArea();

            PlaceLabel(_relicLabel, _layout.LeftMargin, _layout.RelicLabelY);
            PlaceLabel(_coinLabel, _layout.LeftMargin, _layout.CoinLabelY);
            PlaceLabel(_symbolLabel, _layout.LeftMargin, _layout.SymbolLabelY);

            PlaceTopLeft(_sortPanel != null ? _sortPanel.rectTransform : null,
                _layout.SortPosition, _layout.SortSize);
            PlaceTopLeft(_tagPanel != null ? _tagPanel.rectTransform : null,
                _layout.TagPanelPosition, _layout.TagPanelSize);
            PlaceTopLeft(_closePanel != null ? _closePanel.rectTransform : null,
                _layout.ClosePosition, _layout.CloseSize);

            if (_symbolCountLabel != null)
            {
                PlaceLabel(_symbolCountLabel,
                    _layout.TagPanelPosition.x + _layout.PanelPadding,
                    _layout.TagPanelPosition.y + _layout.PanelPadding);
            }

            if (_tagTitleLabel != null)
            {
                PlaceLabel(_tagTitleLabel,
                    _layout.TagPanelPosition.x + _layout.PanelPadding,
                    _layout.TagPanelPosition.y + _layout.PanelPadding + _layout.PanelTitleGap);
            }

            ApplyStyle();
            PlaceTagBars();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
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

            ApplyLabelStyle(_relicLabel, _visual.SectionFontSize, _visual.TextColor);
            ApplyLabelStyle(_coinLabel, _visual.SectionFontSize, _visual.TextColor);
            ApplyLabelStyle(_symbolLabel, _visual.SectionFontSize, _visual.TextColor);
            ApplyLabelStyle(_symbolCountLabel, _visual.PanelTitleFontSize, _visual.TextColor);
            ApplyLabelStyle(_tagTitleLabel, _visual.PanelTitleFontSize, _visual.TextColor);
            ApplyLabelStyle(_sortLabel, _visual.SectionFontSize, _visual.TextColor);
            ApplyLabelStyle(_sortValueLabel, _visual.TagFontSize, _visual.SubTextColor);
            ApplyLabelStyle(_closeLabel, _visual.SectionFontSize, _visual.TextColor);
            ApplyLabelStyle(_closeShortcutLabel, _visual.TagFontSize, _visual.SubTextColor);

            if (_tagPanel != null)
            {
                _tagPanel.color = _visual.PanelColor;
            }

            if (_closePanel != null)
            {
                _closePanel.color = _visual.PanelColor;
            }

            if (_sortPanel != null)
            {
                _sortPanel.color = _visual.PanelColor;
            }

            if (_closeShortcutLabel != null)
            {
                _closeShortcutLabel.text = _visual.CloseShortcutText;
            }
        }

        /// <summary>
        /// 고정 칸을 몇 개 놓을지. 소지 한도가 정해져 있으면 그 수(0 이면 칸이 없다), 아니면 설정의 칸 수다.
        /// 한도가 줄어 잠시 넘친 동안에도 가진 것이 가려지지 않게 가진 수보다 적게 놓지 않는다.
        /// </summary>
        private static int SlotCount(int capacity, int fallback, int owned)
        {
            int count = capacity >= 0 ? capacity : fallback;
            return owned > count ? owned : count;
        }

        /// <summary>빌드 내용을 그대로 그린다.</summary>
        public void Show(CurrentBuildSnapshot snapshot, BuildSortOrder order, IBuildIconSource icons)
        {
            if (snapshot == null || _layout == null)
            {
                return;
            }

            // 칸 수는 지금 소지 한도를 따른다. 한도는 조건에 따라 바뀔 수 있다(2026년 10월 9일 원재).
            BuildFixedSlots(_relicSlots, _relicLayer, snapshot.Relics,
                SlotCount(snapshot.RelicCapacity, _layout.RelicSlotCount, snapshot.Relics.Count),
                _layout.RelicSlotSize, icons, false, true);

            BuildFixedSlots(_coinSlots, _coinLayer, snapshot.Coins,
                SlotCount(snapshot.CoinCapacity, _layout.CoinSlotCount, snapshot.Coins.Count),
                _layout.GetCoinSlotSize(), icons, false, false);

            BuildSymbolSlots(snapshot, icons);
            BuildTagBars(snapshot);

            if (_symbolCountLabel != null && _visual != null)
            {
                _symbolCountLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    _visual.SymbolCountFormat,
                    snapshot.TotalSymbolCount);
            }

            SetSortLabel(order);

            int symbolCount = snapshot.Symbols != null ? snapshot.Symbols.Count : 0;
            ApplyContentHeight(symbolCount);
        }

        /// <summary>
        /// 스크롤이 훑는 창을 잡는다.
        /// 세로는 화면 전체, 가로는 오른쪽 칸 앞까지다.
        /// 창을 화면 왼쪽 위에 맞춰 두므로 안쪽 칸은 기획서 좌표를 그대로 쓴다.
        /// 오른쪽 칸과 닫기는 이 창 밖에 있어 스크롤해도 늘 보인다.
        /// </summary>
        private void ApplyScrollArea()
        {
            if (_scrollViewport == null)
            {
                return;
            }

            PlaceTopLeft(_scrollViewport, Vector2.zero, _layout.GetScrollViewportSize());

            if (_scroll != null)
            {
                _scroll.horizontal = false;
                _scroll.vertical = true;
            }
        }

        /// <summary>
        /// 문양 줄 수에 맞춰 스크롤 내용의 높이를 잡는다.
        /// 한 화면에 다 들어가면 높이가 창과 같아져 스크롤이 움직이지 않는다.
        /// </summary>
        private void ApplyContentHeight(int symbolCount)
        {
            if (_scrollContent == null || _layout == null)
            {
                return;
            }

            Vector2 viewport = _layout.GetScrollViewportSize();
            float height = _layout.GetContentHeight(symbolCount);
            if (height < viewport.y)
            {
                height = viewport.y;
            }

            _scrollContent.anchorMin = new Vector2(0f, 1f);
            _scrollContent.anchorMax = new Vector2(0f, 1f);
            _scrollContent.pivot = new Vector2(0f, 1f);
            _scrollContent.sizeDelta = new Vector2(viewport.x, height);
            _scrollContent.anchoredPosition = Vector2.zero;
        }

        /// <summary>정렬 칸에 지금 기준을 적는다.</summary>
        public void SetSortLabel(BuildSortOrder order)
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

        /// <summary>유물과 코인처럼 칸 수가 정해진 줄을 채운다. 남는 칸은 비워 둔다.</summary>
        private void BuildFixedSlots(
            List<BuildSlotView> views,
            RectTransform layer,
            List<BuildEntry> entries,
            int slotCount,
            Vector2 slotSize,
            IBuildIconSource icons,
            bool showCount,
            bool relicRow)
        {
            if (layer == null || _slotPrefab == null)
            {
                return;
            }

            while (views.Count < slotCount)
            {
                views.Add(MakeSlot(layer));
            }

            for (int i = 0; i < views.Count; i++)
            {
                BuildSlotView view = views[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= slotCount)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                if (entries != null && i < entries.Count && entries[i].IsValid)
                {
                    view.Bind(entries[i], slotSize, _layout, _visual, icons, showCount);
                }
                else
                {
                    view.Clear(slotSize, _visual);
                }

                Vector2 position = relicRow
                    ? _layout.GetRelicSlotPosition(i)
                    : _layout.GetCoinSlotPosition(i);
                PlaceTopLeft(view.RectTransform, position, slotSize);
            }
        }

        /// <summary>
        /// 문양 줄을 채운다.
        /// 기획서대로 가진 종류만 칸을 만들고 유효 슬롯 수를 넘기지 않는다.
        /// </summary>
        private void BuildSymbolSlots(CurrentBuildSnapshot snapshot, IBuildIconSource icons)
        {
            if (_symbolLayer == null || _slotPrefab == null)
            {
                return;
            }

            int count = snapshot.Symbols != null ? snapshot.Symbols.Count : 0;
            if (count > _layout.SymbolSlotLimit)
            {
                Debug.LogWarning(
                    "문양 종류가 " + count + "가지라 유효 슬롯 " + _layout.SymbolSlotLimit + "개를 넘어 뒤쪽을 그리지 않는다.",
                    this);
                count = _layout.SymbolSlotLimit;
            }

            while (_symbolSlots.Count < count)
            {
                _symbolSlots.Add(MakeSlot(_symbolLayer));
            }

            for (int i = 0; i < _symbolSlots.Count; i++)
            {
                BuildSlotView view = _symbolSlots[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                view.Bind(snapshot.Symbols[i], _layout.SymbolSlotSize, _layout, _visual, icons, true);
                PlaceTopLeft(view.RectTransform, _layout.GetSymbolSlotPosition(i), _layout.SymbolSlotSize);
            }
        }

        /// <summary>태그 9종을 모두 만든다.</summary>
        private void BuildTagBars(CurrentBuildSnapshot snapshot)
        {
            if (_tagLayer == null || _tagBarPrefab == null)
            {
                return;
            }

            List<BuildTagStat> stats = snapshot.GetTagStats();
            int count = stats != null ? stats.Count : 0;

            while (_tagBars.Count < count)
            {
                _tagBars.Add(Instantiate(_tagBarPrefab, _tagLayer));
            }

            for (int i = 0; i < _tagBars.Count; i++)
            {
                BuildTagBarView bar = _tagBars[i];
                if (bar == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    bar.gameObject.SetActive(false);
                    continue;
                }

                bar.Bind(stats[i], snapshot.GetTagRatio(stats[i].Tag), _layout, _visual);
            }

            PlaceTagBars();
        }

        /// <summary>태그 줄을 위에서 아래로 늘어놓는다.</summary>
        private void PlaceTagBars()
        {
            if (_layout == null)
            {
                return;
            }

            int placed = 0;

            for (int i = 0; i < _tagBars.Count; i++)
            {
                BuildTagBarView bar = _tagBars[i];
                if (bar == null || !bar.gameObject.activeSelf)
                {
                    continue;
                }

                Vector2 position = _layout.GetTagBarPosition(placed);

                // 줄 전체는 기호부터 막대 끝까지를 감싼다.
                float width = _layout.TagBarX + _layout.TagBarSize.x - _layout.TagSymbolX;
                PlaceTopLeft(
                    bar.RectTransform,
                    new Vector2(_layout.TagSymbolX, position.y - _layout.TagRowHeight + _layout.TagBarSize.y),
                    new Vector2(width, _layout.TagRowHeight));
                placed++;
            }
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다. 기획서 좌표를 그대로 넣을 수 있다.</summary>
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

        /// <summary>
        /// 글자 하나를 화면 왼쪽 위 기준 자리에 놓는다.
        ///
        /// 크기를 함께 잡는다. 안 잡으면 유니티가 넣어 준 100 × 100 이 남아
        /// 가운데 맞춤인 글자가 칸 절반만큼 아래로 밀려 문양 칸 뒤로 들어가고,
        /// 긴 이름은 폭이 모자라 한 자씩 줄바꿈된다.
        /// </summary>
        private void PlaceLabel(TMP_Text label, float x, float y)
        {
            if (label == null)
            {
                return;
            }

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = _layout.LabelSize;
            rect.anchoredPosition = new Vector2(x, -y);
        }

        private static void ApplyLabelStyle(TMP_Text label, float fontSize, Color color)
        {
            if (label == null)
            {
                return;
            }

            label.fontSize = fontSize;
            label.color = color;
        }

        private void HandleSortClicked()
        {
            if (SortClicked != null)
            {
                SortClicked();
            }
        }

        private void HandleCloseClicked()
        {
            if (CloseClicked != null)
            {
                CloseClicked();
            }
        }
    }
}
