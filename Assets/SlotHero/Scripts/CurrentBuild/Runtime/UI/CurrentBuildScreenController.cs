using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 현재 빌드 화면의 진행 처리.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 을 따른다.
    /// 화면 우하단의 현재 빌드 버튼으로 열고, 오른쪽 아래 닫기나 단축키로 닫는다.
    ///
    /// 무엇을 가지고 있는지 모으는 일은 유물, 코인, 문양 기획서를 따르는 쪽이 맡는다.
    /// 이 스크립트는 받은 내용을 정렬해 그리고 닫을 때를 알리는 데까지만 한다.
    /// </summary>
    public class CurrentBuildScreenController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private CurrentBuildScreenView _view;

        [Header("정렬")]
        [Tooltip("처음 열었을 때의 정렬 기준. 화면 예시는 개수 순이다.")]
        [SerializeField] private BuildSortOrder _sortOrder = BuildSortOrder.Count;

        [Tooltip("정렬 칸을 누를 때 돌아가는 차례.")]
        [SerializeField] private BuildSortOrder[] _sortCycle =
        {
            BuildSortOrder.Count,
            BuildSortOrder.Tag,
            BuildSortOrder.Name,
        };

        [Header("단축키")]
        [Tooltip("이 키로도 닫는다. 기획서는 ESC 로 닫게 하고 와이어프레임은 Tab 도 함께 적었다.")]
        [SerializeField] private KeyCode[] _closeKeys = { KeyCode.Escape, KeyCode.Tab };

        private CurrentBuildSnapshot _snapshot;
        private IBuildIconSource _icons;

        /// <summary>화면이 열렸을 때.</summary>
        public event Action Opened;

        /// <summary>화면이 닫혔을 때. 열기 전 화면으로 돌아가는 처리는 받는 쪽이 한다.</summary>
        public event Action Closed;

        /// <summary>
        /// 커서가 칸의 물건 위에 올라가거나 벗어났을 때.
        /// 기획서의 아이템 상세 팝업은 받는 쪽이 띄운다. 물건 설명은 문양, 코인, 유물 기획서 소관이다.
        /// </summary>
        public event Action<BuildEntry, bool> SlotHoverChanged;

        /// <summary>지금 보고 있는 빌드 내용.</summary>
        public CurrentBuildSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        /// <summary>지금 정렬 기준.</summary>
        public BuildSortOrder SortOrder
        {
            get { return _sortOrder; }
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
                _view.SortClicked += ToggleSortOrder;
                _view.CloseClicked += Close;
                _view.SlotHoverChanged += HandleSlotHover;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.SortClicked -= ToggleSortOrder;
                _view.CloseClicked -= Close;
                _view.SlotHoverChanged -= HandleSlotHover;
            }
        }

        private void HandleSlotHover(BuildEntry entry, bool hovering)
        {
            if (SlotHoverChanged != null)
            {
                SlotHoverChanged(entry, hovering);
            }
        }

        private void Update()
        {
            if (!IsOpen || _closeKeys == null)
            {
                return;
            }

            for (int i = 0; i < _closeKeys.Length; i++)
            {
                // 같은 ESC 를 흐름도 읽는다. 먼저 가져간 쪽만 쓴다. KeyGate 참고.
                if (KeyGate.TryUse(_closeKeys[i]))
                {
                    Close();
                    return;
                }
            }
        }

        /// <summary>현재 빌드 화면을 연다. 현재 빌드 버튼이 부른다.</summary>
        public void Open(CurrentBuildSnapshot snapshot, IBuildIconSource icons)
        {
            _snapshot = snapshot;
            _icons = icons;

            if (_snapshot != null)
            {
                // 통계에 빠진 태그가 있어도 아홉 줄이 모두 나오게 한다.
            }

            SetActive(true);
            Refresh();

            if (Opened != null)
            {
                Opened();
            }
        }

        /// <summary>화면을 닫는다. 기획서대로 열기 전 화면으로 돌아간다.</summary>
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

        /// <summary>정렬 기준을 다음 것으로 넘긴다.</summary>
        public void ToggleSortOrder()
        {
            if (_sortCycle == null || _sortCycle.Length == 0)
            {
                return;
            }

            int index = 0;
            for (int i = 0; i < _sortCycle.Length; i++)
            {
                if (_sortCycle[i] == _sortOrder)
                {
                    index = i;
                    break;
                }
            }

            SetSortOrder(_sortCycle[(index + 1) % _sortCycle.Length]);
        }

        /// <summary>정렬 기준을 정한다.</summary>
        public void SetSortOrder(BuildSortOrder order)
        {
            _sortOrder = order;
            Refresh();
        }

        /// <summary>지금 내용을 정렬해 다시 그린다.</summary>
        public void Refresh()
        {
            if (_view == null || _snapshot == null)
            {
                return;
            }

            // 태그 막대는 화면이 그릴 때 통계를 만들어 태양계 차례로 놓는다. 여기서 따로 만들어 정렬하던 것은
            // 화면에 넘기지 않고 버려지던 계산이라 뺐다. 2026년 10월 9일 외부 검토가 짚었다.
            CurrentBuildSort.SortSymbols(_snapshot.Symbols, _sortOrder);

            _view.Show(_snapshot, _sortOrder, _icons);
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
