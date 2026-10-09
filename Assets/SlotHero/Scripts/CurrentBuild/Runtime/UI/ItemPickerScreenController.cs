using System;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 고르기 화면의 진행 처리.
    /// 이벤트와 성소에서 가진 것 중 하나를 직접 고를 때 연다. 2026년 10월 5일 원재가 정했다.
    ///
    /// 칸을 누르면 고르고, 고른 칸을 다시 누르면 푼다. 다른 칸을 누르면 그쪽으로 바꾼다.
    /// 확인을 누르면 <see cref="Picked"/> 를 알리고 닫는다. 취소나 ESC 면 <see cref="Cancelled"/> 를 알리고 닫는다.
    /// 고른 것으로 무엇을 할지는 연 쪽이 정한다.
    /// </summary>
    public class ItemPickerScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private ItemPickerScreenView _view;

        [Tooltip("정렬 칸을 누를 때 넘어가는 차례. 현재 빌드 화면과 같다.")]
        [SerializeField] private BuildSortOrder[] _sortCycle = { BuildSortOrder.Count, BuildSortOrder.Tag, BuildSortOrder.Name };

        [Tooltip("이 키로 취소한다.")]
        [SerializeField] private KeyCode[] _cancelKeys = { KeyCode.Escape };

        private ItemPickRequest _request;
        private IBuildIconSource _icons;
        private BuildSortOrder _sortOrder = BuildSortOrder.Count;
        private string _selectedId = string.Empty;

        /// <summary>확인을 눌렀을 때. 고른 것을 넘긴다.</summary>
        public event Action<BuildEntry> Picked;

        /// <summary>고르지 않고 닫았을 때.</summary>
        public event Action Cancelled;

        /// <summary>커서가 칸의 물건 위에 올라가거나 벗어났을 때. 아이템 상세 팝업을 띄우는 데 쓴다.</summary>
        public event Action<BuildEntry, bool> SlotHoverChanged;

        /// <summary>화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _screenRoot != null && _screenRoot.activeInHierarchy; }
        }

        /// <summary>지금 고른 것의 식별자. 없으면 빈 글이다.</summary>
        public string SelectedId
        {
            get { return _selectedId; }
        }

        /// <summary>지금 열린 요청.</summary>
        public ItemPickRequest Request
        {
            get { return _request; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.SlotClicked += Toggle;
                _view.SortClicked += ToggleSortOrder;
                _view.ConfirmClicked += Confirm;
                _view.CancelClicked += Cancel;
                _view.SlotHoverChanged += HandleSlotHover;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.SlotClicked -= Toggle;
                _view.SortClicked -= ToggleSortOrder;
                _view.ConfirmClicked -= Confirm;
                _view.CancelClicked -= Cancel;
                _view.SlotHoverChanged -= HandleSlotHover;
            }
        }

        private void Update()
        {
            if (!IsOpen || _cancelKeys == null || !CanCancel)
            {
                return;
            }

            for (int i = 0; i < _cancelKeys.Length; i++)
            {
                // 같은 ESC 를 흐름도 읽는다. 먼저 가져간 쪽만 쓴다. KeyGate 참고.
                if (KeyGate.TryUse(_cancelKeys[i]))
                {
                    Cancel();
                    return;
                }
            }
        }

        /// <summary>고르기 화면을 연다. 고른 것은 비운 채로 시작한다.</summary>
        public void Open(ItemPickRequest request, IBuildIconSource icons)
        {
            _request = request;
            _icons = icons;
            _selectedId = string.Empty;

            SetActive(true);
            Refresh();
        }

        /// <summary>화면만 닫는다. 아무것도 알리지 않는다. 흐름이 화면을 치울 때 쓴다.</summary>
        public void Close()
        {
            _request = null;
            _selectedId = string.Empty;
            SetActive(false);
        }

        /// <summary>그 칸을 고르거나 푼다. 이미 고른 칸이면 풀고, 아니면 그 칸으로 바꾼다.</summary>
        public void Toggle(BuildEntry entry)
        {
            if (!entry.IsValid)
            {
                return;
            }

            _selectedId = _selectedId == entry.Id ? string.Empty : entry.Id;
            Refresh();
        }

        /// <summary>고른 것으로 정하고 닫는다. 고른 것이 없으면 아무 일도 없다.</summary>
        public void Confirm()
        {
            if (_request == null || string.IsNullOrEmpty(_selectedId))
            {
                return;
            }

            BuildEntry picked = Find(_selectedId);
            Close();

            if (picked.IsValid && Picked != null)
            {
                Picked(picked);
            }
        }

        /// <summary>고르지 않고 닫을 수 있는지. 반드시 골라야 하는 요청이면 false 다.</summary>
        public bool CanCancel
        {
            get { return _request == null || _request.Cancellable; }
        }

        /// <summary>고르지 않고 닫는다. 반드시 골라야 하는 요청이면 아무 일도 없다.</summary>
        public void Cancel()
        {
            if (!IsOpen || !CanCancel)
            {
                return;
            }

            Close();

            if (Cancelled != null)
            {
                Cancelled();
            }
        }

        /// <summary>정렬 기준을 다음 것으로 넘긴다. 문양만 정렬한다.</summary>
        public void ToggleSortOrder()
        {
            if (_sortCycle == null || _sortCycle.Length == 0)
            {
                return;
            }

            int index = Array.IndexOf(_sortCycle, _sortOrder);
            _sortOrder = _sortCycle[(index + 1) % _sortCycle.Length];
            Refresh();
        }

        private void Refresh()
        {
            if (_view == null || _request == null)
            {
                return;
            }

            if (_request.Kind == ItemPickKind.Symbol)
            {
                CurrentBuildSort.SortSymbols(_request.Candidates, _sortOrder);
            }

            _view.Show(_request, _sortOrder, _icons, _selectedId);
        }

        private BuildEntry Find(string id)
        {
            for (int i = 0; i < _request.Candidates.Count; i++)
            {
                if (_request.Candidates[i].Id == id)
                {
                    return _request.Candidates[i];
                }
            }

            return new BuildEntry();
        }

        private void HandleSlotHover(BuildEntry entry, bool hovering)
        {
            if (SlotHoverChanged != null)
            {
                SlotHoverChanged(entry, hovering);
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
