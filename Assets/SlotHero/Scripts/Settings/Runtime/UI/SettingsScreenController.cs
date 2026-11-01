using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Settings.UI
{
    /// <summary>
    /// 설정 화면의 진행 처리.
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 을 따른다.
    ///
    /// 값이 바뀌면 바로 적는다. 확인 버튼 없이 닫기만 있기 때문이다.
    /// 실제로 그 값을 쓰는 일은 각 시스템이 <see cref="ValueChanged"/>를 받아 한다.
    ///
    /// 기본값 복원은 확인 팝업을 거쳐야 하므로 여기서는 눌렸다는 것만 알린다.
    /// 팝업을 띄운 쪽이 확인을 받고 <see cref="RestoreDefaults"/>를 부른다.
    /// </summary>
    public class SettingsScreenController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private SettingsScreenView _view;
        [SerializeField] private SettingsCatalogConfig _catalog;

        [Header("기본값 복원")]
        [Tooltip("켜면 지금 보고 있는 탭만 되돌린다. " +
                 "07 설정 화면 은 전체를 되돌리게 하고 08 화면 팝업 은 지금 탭이라고 적어 문서가 어긋난다. " +
                 "기본은 07장을 따라 전체다.")]
        [SerializeField] private bool _restoreCurrentTabOnly;

        [Header("단축키")]
        [Tooltip("이 키로도 닫는다. 기획서에 설정 화면의 단축키는 없고 다른 화면을 따라 넣었다.")]
        [SerializeField] private KeyCode[] _closeKeys = { KeyCode.Escape };

        private SettingsValues _values = new SettingsValues();
        private int _tabIndex;

        /// <summary>값이 바뀌었을 때. 그 값을 실제로 쓰는 쪽이 받는다.</summary>
        public event Action<SettingDefinition, float> ValueChanged;

        /// <summary>
        /// 기본값 복원을 눌렀을 때.
        /// 07 설정 화면 이 확인 팝업을 거치게 하므로 여기서 되돌리지 않는다.
        /// 팝업에서 확인을 받으면 <see cref="RestoreDefaults"/>를 부른다.
        /// </summary>
        public event Action RestoreRequested;

        /// <summary>기본값 복원이 실제로 끝났을 때.</summary>
        public event Action RestoreApplied;

        /// <summary>화면이 닫혔을 때. 이전 화면으로 돌아가는 처리는 받는 쪽이 한다.</summary>
        public event Action Closed;

        /// <summary>지금 설정 값. 저장 시스템이 기기 설정으로 실어 보낸다.</summary>
        public SettingsValues Values
        {
            get { return _values; }
        }

        /// <summary>
        /// 키 입력을 받지 않는지. 설정 위에 팝업이 떠 있는 동안 흐름이 켠다.
        /// 켜 두지 않으면 기본값 복원 확인 팝업이 떠 있는데 ESC 가 그 아래 설정 화면을 닫는다.
        /// </summary>
        public bool InputBlocked { get; set; }

        /// <summary>기본값 복원을 눌러도 그대로 두는 항목의 이름. 확인 팝업에 적는다.</summary>
        public List<string> GetKeptOnRestoreNames()
        {
            return _catalog != null ? _catalog.CollectKeptOnRestoreNames() : new List<string>();
        }

        /// <summary>기본값 복원이 지금 탭만 되돌리는지.</summary>
        public bool RestoresCurrentTabOnly
        {
            get { return _restoreCurrentTabOnly; }
        }

        /// <summary>지금 보고 있는 탭의 자리.</summary>
        public int TabIndex
        {
            get { return _tabIndex; }
        }

        /// <summary>화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _screenRoot != null && _screenRoot.activeInHierarchy; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.TabClicked += SelectTab;
                _view.ValueChangeRequested += HandleValueChangeRequested;
                _view.ValueSetRequested += SetValue;
                _view.RestoreClicked += HandleRestoreClicked;
                _view.CloseClicked += Close;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.TabClicked -= SelectTab;
                _view.ValueChangeRequested -= HandleValueChangeRequested;
                _view.ValueSetRequested -= SetValue;
                _view.RestoreClicked -= HandleRestoreClicked;
                _view.CloseClicked -= Close;
            }
        }

        private void Update()
        {
            if (!IsOpen || _closeKeys == null || InputBlocked)
            {
                return;
            }

            for (int i = 0; i < _closeKeys.Length; i++)
            {
                // 같은 ESC 를 흐름도 읽는다. 먼저 가져간 쪽만 쓴다. KeyGate 참고.
                if (KeyGate.TryUse(_closeKeys[i]))
                {
                    // 드롭다운이 펼쳐져 있으면 그것만 접는다. 화면은 다음 번에 닫는다.
                    if (_view != null && _view.IsDropdownOpen)
                    {
                        _view.CloseDropdown();
                    }
                    else
                    {
                        Close();
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// 설정 화면을 연다.
        /// 저장해 둔 값을 넘기면 그대로 쓰고, 비워 두면 기본값으로 시작한다.
        /// </summary>
        public void Open(SettingsValues values)
        {
            _values = values ?? new SettingsValues();

            if (_catalog != null)
            {
                List<SettingDefinition> all = _catalog.CollectAllItems();

                // 새로 생긴 항목은 기본값으로 채우고 사라진 항목의 값은 버린다.
                _values.FillMissing(all);
                _values.DropUnknown(all);

                if (!_catalog.HasUniqueIds())
                {
                    Debug.LogWarning("설정 항목 식별자가 겹치거나 비어 있다. 저장이 엉킨다.", this);
                }
            }

            SetActive(true);
            SelectTab(_tabIndex);
        }

        /// <summary>화면을 닫는다. 기획서대로 이전 화면으로 돌아간다.</summary>
        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            SetActive(false);

            if (Closed != null)
            {
                Closed();
            }
        }

        /// <summary>그 자리의 탭을 고른다.</summary>
        public void SelectTab(int index)
        {
            if (_catalog == null || _view == null)
            {
                return;
            }

            int count = _catalog.TabCount;
            if (count <= 0)
            {
                return;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index > count - 1)
            {
                index = count - 1;
            }

            _tabIndex = index;

            _view.ShowTabs(_catalog, _tabIndex);
            _view.ShowItems(_catalog.GetTab(_tabIndex), _values);
        }

        /// <summary>
        /// 기본값으로 되돌린다. 확인 팝업에서 확인을 받은 뒤에 부른다.
        /// 설정에 따라 전체를 되돌리거나 지금 탭만 되돌린다.
        /// </summary>
        public void RestoreDefaults()
        {
            if (_catalog == null)
            {
                return;
            }

            List<SettingDefinition> targets = _restoreCurrentTabOnly
                ? GetCurrentTabItems()
                : _catalog.CollectAllItems();

            // 해상도와 창 모드처럼 `KeepOnRestore` 가 켜진 항목은 건너뛴다.
            List<SettingDefinition> changed = new List<SettingDefinition>();
            _values.RestoreDefaults(targets, changed);

            for (int i = 0; i < changed.Count; i++)
            {
                if (ValueChanged != null)
                {
                    ValueChanged(changed[i], _values.Get(changed[i]));
                }
            }

            SelectTab(_tabIndex);

            if (RestoreApplied != null)
            {
                RestoreApplied();
            }
        }

        /// <summary>값을 하나 바꾼다. 값이 실제로 바뀌면 알린다.</summary>
        public void SetValue(SettingDefinition definition, float value)
        {
            if (definition == null)
            {
                return;
            }

            if (!_values.Set(definition, value))
            {
                return;
            }

            float applied = _values.Get(definition);

            if (_view != null)
            {
                _view.RefreshValue(definition, applied);
            }

            if (ValueChanged != null)
            {
                ValueChanged(definition, applied);
            }
        }

        private List<SettingDefinition> GetCurrentTabItems()
        {
            List<SettingDefinition> items = new List<SettingDefinition>();
            SettingsTab tab = _catalog.GetTab(_tabIndex);

            if (tab != null && tab.Items != null)
            {
                for (int i = 0; i < tab.Items.Count; i++)
                {
                    if (tab.Items[i] != null)
                    {
                        items.Add(tab.Items[i]);
                    }
                }
            }

            return items;
        }

        private void HandleValueChangeRequested(SettingDefinition definition, bool forward)
        {
            if (definition == null)
            {
                return;
            }

            float current = _values.Get(definition);
            float next = forward ? definition.Next(current) : definition.Previous(current);
            SetValue(definition, next);
        }

        private void HandleRestoreClicked()
        {
            if (RestoreRequested != null)
            {
                RestoreRequested();
            }
        }

        private void SetActive(bool value)
        {
            if (_screenRoot != null && _screenRoot.activeSelf != value)
            {
                _screenRoot.SetActive(value);
            }
        }
    }
}
