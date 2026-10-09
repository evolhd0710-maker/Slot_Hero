using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Settings.UI
{
    /// <summary>
    /// 설정 화면 표시.
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 의 배치를 맡는다.
    /// 제목이 왼쪽 위, 분류 탭이 왼쪽, 설정 목록이 오른쪽,
    /// 기본값 복원이 좌하단, 닫기가 우하단이다.
    ///
    /// 탭과 항목은 수가 정해져 있지 않으므로 프리팹을 복제해 만든다.
    /// 항목이 목록 칸보다 많으면 그 안에서만 세로로 스크롤한다.
    /// 항목은 목록 칸 맨 위부터 차례로 붙는다.
    ///
    /// 드롭다운 항목을 누르면 맨 위 층에 선택지 목록을 펼친다.
    /// 목록 바깥을 누르거나, 탭을 바꾸거나, 화면을 닫으면 접힌다.
    /// </summary>
    public class SettingsScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private SettingsLayoutConfig _layout;
        [SerializeField] private SettingsVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _dim;
        [SerializeField] private Image _titlePanel;
        [SerializeField] private TMP_Text _titleLabel;

        [Header("분류 탭")]
        [SerializeField] private Image _tabPanel;
        [SerializeField] private RectTransform _tabLayer;
        [SerializeField] private SettingsTabView _tabPrefab;

        [Header("설정 목록")]
        [SerializeField] private Image _listPanel;
        [SerializeField] private ScrollRect _listScroll;
        [SerializeField] private RectTransform _listViewport;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private SettingsRowView _rowPrefab;

        [Header("드롭다운")]
        [Tooltip("펼친 목록을 담는 맨 위 층. 화면 전체를 덮는다. 평소에는 꺼져 있다.")]
        [SerializeField] private GameObject _dropdownLayer;

        [Tooltip("목록 바깥을 누르면 닫는 투명 버튼. 층 전체를 덮는다.")]
        [SerializeField] private Button _dropdownBlocker;

        [Tooltip("선택지를 늘어놓는 칸.")]
        [SerializeField] private Image _dropdownPanel;

        [SerializeField] private SettingsOptionView _optionPrefab;

        [Header("아래 버튼")]
        [SerializeField] private Button _restoreButton;
        [SerializeField] private Image _restorePanel;
        [SerializeField] private TMP_Text _restoreLabel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _closePanel;
        [SerializeField] private TMP_Text _closeLabel;

        private readonly List<SettingsTabView> _tabs = new List<SettingsTabView>();
        private readonly List<SettingsRowView> _rows = new List<SettingsRowView>();
        private readonly List<SettingsOptionView> _options = new List<SettingsOptionView>();
        private SettingDefinition _dropdownDefinition;

        /// <summary>탭을 눌렀을 때. 몇 번째 탭인지 넘긴다.</summary>
        public event Action<int> TabClicked;

        /// <summary>항목을 눌러 값을 바꿀 때. 항목과 앞뒤 방향을 넘긴다.</summary>
        public event Action<SettingDefinition, bool> ValueChangeRequested;

        /// <summary>
        /// 값을 바로 정할 때. 막대를 끌었거나 음소거를 눌렀거나 드롭다운에서 골랐을 때다.
        /// </summary>
        public event Action<SettingDefinition, float> ValueSetRequested;

        /// <summary>드롭다운 목록이 펼쳐져 있는지.</summary>
        public bool IsDropdownOpen
        {
            get { return _dropdownLayer != null && _dropdownLayer.activeSelf; }
        }

        /// <summary>펼친 드롭다운이 맡은 항목. 닫혀 있으면 null 이다.</summary>
        public SettingDefinition DropdownDefinition
        {
            get { return IsDropdownOpen ? _dropdownDefinition : null; }
        }

        /// <summary>펼친 드롭다운의 선택지. 닫혀 있으면 비어 있다.</summary>
        public List<SettingsOptionView> GetOpenOptions()
        {
            List<SettingsOptionView> open = new List<SettingsOptionView>();

            if (!IsDropdownOpen)
            {
                return open;
            }

            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i] != null && _options[i].gameObject.activeSelf)
                {
                    open.Add(_options[i]);
                }
            }

            return open;
        }

        /// <summary>지금 늘어놓은 줄. 꺼진 줄은 빠진다.</summary>
        public List<SettingsRowView> GetShownRows()
        {
            List<SettingsRowView> shown = new List<SettingsRowView>();

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null && _rows[i].gameObject.activeSelf)
                {
                    shown.Add(_rows[i]);
                }
            }

            return shown;
        }

        /// <summary>기본값 복원을 눌렀을 때.</summary>
        public event Action RestoreClicked;

        /// <summary>닫기를 눌렀을 때.</summary>
        public event Action CloseClicked;

        private void Awake()
        {
            if (_restoreButton != null)
            {
                _restoreButton.onClick.AddListener(HandleRestoreClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(HandleCloseClicked);
            }

            if (_dropdownBlocker != null)
            {
                _dropdownBlocker.onClick.AddListener(CloseDropdown);
            }

            ApplyLayout();
            CloseDropdown();
        }

        private void OnDisable()
        {
            // 화면이 닫히면 펼친 목록도 접는다. 다시 열었을 때 남아 있지 않게 한다.
            CloseDropdown();
        }

        private void OnDestroy()
        {
            if (_restoreButton != null)
            {
                _restoreButton.onClick.RemoveListener(HandleRestoreClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(HandleCloseClicked);
            }

            if (_dropdownBlocker != null)
            {
                _dropdownBlocker.onClick.RemoveListener(CloseDropdown);
            }
        }

        /// <summary>
        /// 기획서 07장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.PanelsDoNotOverlap())
            {
                Debug.LogWarning("분류 탭 칸과 설정 목록 칸이 겹친다. 자리를 다시 본다.", this);
            }

            PlaceTopLeft(_titlePanel, _layout.TitlePosition, _layout.TitleSize);
            PlaceTopLeft(_tabPanel, _layout.TabPanelPosition, _layout.TabPanelSize);
            PlaceTopLeft(_listPanel, _layout.ListPanelPosition, _layout.ListPanelSize);
            PlaceTopLeft(_restorePanel, _layout.RestorePosition, _layout.RestoreSize);
            PlaceTopLeft(_closePanel, _layout.ClosePosition, _layout.CloseSize);

            if (_listViewport != null)
            {
                // 목록 칸 전체를 훑는 창으로 삼는다. 안쪽 자리는 목록 칸 왼쪽 위를 기준으로 잡는다.
                PlaceTopLeftRect(_listViewport, _layout.ListPanelPosition, _layout.ListPanelSize);
            }

            if (_listScroll != null)
            {
                _listScroll.horizontal = false;
                _listScroll.vertical = true;
            }

            ApplyStyle();
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

            SetPanelColor(_titlePanel);
            SetPanelColor(_tabPanel);
            SetPanelColor(_listPanel);
            SetPanelColor(_restorePanel);
            SetPanelColor(_closePanel);

            if (_dropdownPanel != null)
            {
                _dropdownPanel.color = _visual.DropdownPanelColor;
            }

            if (_titleLabel != null)
            {
                _titleLabel.text = _visual.TitleText;
                _titleLabel.fontSize = _visual.TitleFontSize;
                _titleLabel.color = _visual.ItemTextColor;
            }

            if (_restoreLabel != null)
            {
                _restoreLabel.text = _visual.RestoreText;
                _restoreLabel.fontSize = _visual.ButtonFontSize;
                _restoreLabel.color = _visual.ItemTextColor;
            }

            if (_closeLabel != null)
            {
                _closeLabel.text = _visual.CloseText;
                _closeLabel.fontSize = _visual.ButtonFontSize;
                _closeLabel.color = _visual.ItemTextColor;
            }
        }

        /// <summary>분류 탭을 만든다.</summary>
        public void ShowTabs(SettingsCatalogConfig catalog, int selectedIndex)
        {
            if (catalog == null || _tabLayer == null || _tabPrefab == null || _layout == null)
            {
                return;
            }

            int count = catalog.TabCount;

            while (_tabs.Count < count)
            {
                _tabs.Add(Instantiate(_tabPrefab, _tabLayer));
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                SettingsTabView view = _tabs[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                view.Bind(catalog.GetTab(i), i, _layout.TabSize, _visual, HandleTabClicked);
                view.SetSelected(i == selectedIndex);
                PlaceTopLeftRect(view.RectTransform, _layout.GetTabPosition(i), _layout.TabSize);
            }
        }

        /// <summary>고른 탭의 항목을 늘어놓는다.</summary>
        public void ShowItems(SettingsTab tab, SettingsValues values)
        {
            if (_listContent == null || _rowPrefab == null || _layout == null)
            {
                return;
            }

            CloseDropdown();

            int count = tab != null ? tab.ItemCount : 0;

            while (_rows.Count < count)
            {
                _rows.Add(Instantiate(_rowPrefab, _listContent));
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                SettingsRowView row = _rows[i];
                if (row == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    row.gameObject.SetActive(false);
                    continue;
                }

                SettingDefinition definition = tab.Items[i];
                float value = values != null ? values.Get(definition) : definition.DefaultValue;
                SettingDefinition mute = definition != null ? definition.GetMuteDefinition() : null;
                bool muted = mute != null && values != null && values.Get(mute) >= 0.5f;

                row.Bind(
                    definition, value, muted, _layout.ItemSize, _layout, _visual,
                    HandleValueChangeRequested, HandleValueSetRequested, OpenDropdown);

                // 줄은 스크롤 내용 안에 놓으므로 목록 칸 기준 자리를 쓴다. GetItemLocalPosition 설명 참고.
                PlaceTopLeftRect(row.RectTransform, _layout.GetItemLocalPosition(i), _layout.ItemSize);
            }

            ApplyContentHeight(count);
        }

        /// <summary>
        /// 그 줄 아래로 드롭다운 목록을 펼친다. 줄 오른쪽 끝에 맞춘다.
        /// 이미 같은 줄의 목록이 펼쳐져 있으면 접는다.
        /// </summary>
        public void OpenDropdown(SettingsRowView row)
        {
            if (row == null || row.Definition == null)
            {
                return;
            }

            if (IsDropdownOpen && _dropdownDefinition == row.Definition)
            {
                CloseDropdown();
                return;
            }

            if (_dropdownLayer == null || _dropdownPanel == null || _optionPrefab == null || _layout == null)
            {
                return;
            }

            SettingDefinition definition = row.Definition;
            int count = definition.OptionCount;
            int selected = (int)definition.Clamp(row.Value);

            _dropdownDefinition = definition;
            _dropdownLayer.SetActive(true);
            _dropdownLayer.transform.SetAsLastSibling();

            RectTransform panel = _dropdownPanel.rectTransform;

            while (_options.Count < count)
            {
                _options.Add(Instantiate(_optionPrefab, panel));
            }

            for (int i = 0; i < _options.Count; i++)
            {
                SettingsOptionView option = _options[i];
                if (option == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    option.gameObject.SetActive(false);
                    continue;
                }

                option.Bind(definition.Options[i], i, i == selected, _layout.OptionSize, _visual, HandleOptionClicked);
                PlaceTopLeftRect(option.RectTransform, new Vector2(0f, i * _layout.OptionSize.y), _layout.OptionSize);
            }

            // 줄의 오른쪽 아래 모서리에 목록의 오른쪽 위를 붙인다.
            // 다른 칸처럼 화면 왼쪽 위에 매어 둔다. 가운데에 매면 화면 크기가 바뀔 때 줄과 따로 움직인다.
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(_layout.OptionSize.x, _layout.OptionSize.y * count);
            panel.position = row.GetValueCornerWorld();
        }

        /// <summary>펼친 드롭다운 목록을 접는다.</summary>
        public void CloseDropdown()
        {
            _dropdownDefinition = null;

            if (_dropdownLayer != null && _dropdownLayer.activeSelf)
            {
                _dropdownLayer.SetActive(false);
            }
        }

        /// <summary>고른 탭을 바꿔 칠한다.</summary>
        public void SetSelectedTab(int index)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i] != null)
                {
                    _tabs[i].SetSelected(i == index);
                }
            }
        }

        /// <summary>그 항목의 값만 다시 적는다.</summary>
        public void RefreshValue(SettingDefinition definition, float value)
        {
            if (definition == null)
            {
                return;
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                SettingsRowView row = _rows[i];
                if (row == null || !row.gameObject.activeSelf)
                {
                    continue;
                }

                if (row.Definition == definition)
                {
                    row.SetValue(value);
                    return;
                }

                // 음소거 값은 줄이 따로 없고 그 소리 크기 줄의 버튼으로 나온다.
                if (row.MuteDefinition == definition)
                {
                    row.SetMuted(value >= 0.5f);
                    return;
                }
            }
        }

        /// <summary>항목 수에 맞춰 스크롤 내용의 높이를 잡는다.</summary>
        private void ApplyContentHeight(int itemCount)
        {
            if (_listContent == null || _layout == null)
            {
                return;
            }

            float height = _layout.GetListContentHeight(itemCount);

            _listContent.anchorMin = new Vector2(0f, 1f);
            _listContent.anchorMax = new Vector2(0f, 1f);
            _listContent.pivot = new Vector2(0f, 1f);
            _listContent.sizeDelta = new Vector2(_layout.ListPanelSize.x, height);

            // 창 왼쪽 위에 붙여 맨 위부터 보이게 한다.
            // 스크롤 창이 매 프레임 내용을 창 안으로 끌어 넣으므로 원점을 다른 곳에 두면 항목이 밀린다.
            _listContent.anchoredPosition = Vector2.zero;

            if (_listScroll != null)
            {
                _listScroll.StopMovement();
                _listScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void SetPanelColor(Image image)
        {
            if (image != null && _visual != null)
            {
                image.color = _visual.PanelColor;
            }
        }

        private static void PlaceTopLeft(Image image, Vector2 position, Vector2 size)
        {
            if (image != null)
            {
                PlaceTopLeftRect(image.rectTransform, position, size);
            }
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다. 기획서 좌표를 그대로 넣을 수 있다.</summary>
        private static void PlaceTopLeftRect(RectTransform rect, Vector2 position, Vector2 size)
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

        private void HandleTabClicked(SettingsTabView view)
        {
            CloseDropdown();

            if (view != null && TabClicked != null)
            {
                TabClicked(view.Index);
            }
        }

        private void HandleValueChangeRequested(SettingsRowView row, bool forward)
        {
            if (row != null && row.Definition != null && ValueChangeRequested != null)
            {
                ValueChangeRequested(row.Definition, forward);
            }
        }

        private void HandleValueSetRequested(SettingDefinition definition, float value)
        {
            if (definition != null && ValueSetRequested != null)
            {
                ValueSetRequested(definition, value);
            }
        }

        private void HandleOptionClicked(SettingsOptionView option)
        {
            SettingDefinition definition = _dropdownDefinition;
            CloseDropdown();

            if (option != null && definition != null && ValueSetRequested != null)
            {
                ValueSetRequested(definition, option.Index);
            }
        }

        private void HandleRestoreClicked()
        {
            if (RestoreClicked != null)
            {
                RestoreClicked();
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
