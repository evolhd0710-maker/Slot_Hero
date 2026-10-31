using System;
using UnityEngine;

namespace SlotHero.Profile.UI
{
    /// <summary>
    /// 프로필 선택 화면의 흐름.
    /// 인게임 화면 기획서 v0.2 / 06 프로필 선택 화면 을 맡는다.
    ///
    /// 기획서가 정한 네 갈래를 그대로 나눈다.
    /// 채워진 자리를 누르면 그 프로필로 전환하고,
    /// 빈 자리를 누르면 이름 입력 상태로 바꾸고,
    /// 삭제를 누르면 확인 팝업을 띄우라고 알리고,
    /// 읽을 수 없는 자리를 누르면 저장 데이터 오류 팝업을 띄우라고 알린다.
    ///
    /// 팝업은 Popup 폴더의 공통 틀이 맡고 여기서는 띄워 달라고 알리기만 한다.
    /// </summary>
    public class ProfileSelectScreenController : MonoBehaviour
    {
        [Header("화면")]
        [SerializeField] private ProfileSelectScreenView _view;

        [Header("설정")]
        [Tooltip("켜 두면 시작할 때 화면을 감춘다. Open 을 부르면 나타난다.")]
        // 기본은 끈다. 이 조종기는 제가 숨기는 화면 위에 붙어 있고, 게임 씬은 화면을 꺼 둔 채 시작한다.
        // 꺼진 오브젝트는 처음 켜질 때 Awake 가 돌므로, 켜 두면 흐름이 처음 열 때
        // 켜지는 순간 도로 숨겨 화면이 안 나온다. 켜진 채 시작하는 씬에서만 켠다.
        [SerializeField] private bool _hideOnAwake;

        [Tooltip("빈 자리에 프로필을 만들 때 입력칸에 미리 넣어 두는 이름.")]
        [SerializeField] private string _newProfileName = string.Empty;

        private ProfileList _list;

        /// <summary>프로필을 골랐을 때. 그 프로필로 전환하는 쪽이 받는다.</summary>
        public event Action<int> ProfileChosen;

        /// <summary>
        /// 삭제를 눌렀을 때. 프로필 삭제 확인 팝업을 띄우는 쪽이 받는다.
        /// 확인을 받으면 <see cref="ConfirmDelete"/> 를 부른다.
        /// </summary>
        public event Action<int> DeleteRequested;

        /// <summary>
        /// 읽을 수 없는 자리를 눌렀을 때.
        /// 저장 데이터 오류 팝업을 띄우는 쪽이 받는다.
        /// </summary>
        public event Action<int> BrokenProfileOpened;

        /// <summary>빈 자리에 프로필을 새로 만들었을 때. 자리와 이름을 넘긴다.</summary>
        public event Action<int, string> ProfileCreated;

        /// <summary>프로필 이름을 고쳤을 때. 자리와 새 이름을 넘긴다.</summary>
        public event Action<int, string> ProfileRenamed;

        /// <summary>프로필을 지웠을 때.</summary>
        public event Action<int> ProfileDeleted;

        /// <summary>뒤로를 눌렀을 때. 타이틀 화면으로 돌려보내는 쪽이 받는다.</summary>
        public event Action BackRequested;

        /// <summary>지금 화면에 올라간 목록.</summary>
        public ProfileList List
        {
            get { return _list; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.CardClicked += HandleCardClicked;
                _view.EditClicked += HandleEditClicked;
                _view.DeleteClicked += HandleDeleteClicked;
                _view.NameSubmitted += HandleNameSubmitted;
                _view.BackClicked += HandleBackClicked;

                if (_hideOnAwake)
                {
                    _view.SetVisible(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.CardClicked -= HandleCardClicked;
                _view.EditClicked -= HandleEditClicked;
                _view.DeleteClicked -= HandleDeleteClicked;
                _view.NameSubmitted -= HandleNameSubmitted;
                _view.BackClicked -= HandleBackClicked;
            }
        }

        /// <summary>프로필 목록을 받아 화면을 연다.</summary>
        public void Open(ProfileList list)
        {
            if (list == null)
            {
                return;
            }

            _list = list;
            _list.EnsureSlots(GetSlotCount(list));

            if (_view != null)
            {
                _view.Show(_list);
                _view.SetVisible(true);
            }
        }

        /// <summary>화면을 감춘다.</summary>
        public void Close()
        {
            if (_view != null)
            {
                _view.SetVisible(false);
            }
        }

        /// <summary>
        /// 삭제 확인 팝업에서 예를 골랐을 때 부른다.
        /// 기획서의 "프로필을 삭제하고 해당 자리를 빈 자리로 되돌린다"를 여기서 한다.
        /// </summary>
        public void ConfirmDelete(int index)
        {
            if (_list == null)
            {
                return;
            }

            _list.Clear(index);
            Refresh();

            if (ProfileDeleted != null)
            {
                ProfileDeleted(index);
            }
        }

        /// <summary>목록이 바뀐 뒤 화면을 다시 그린다.</summary>
        public void Refresh()
        {
            if (_view != null && _list != null)
            {
                _view.Show(_list);
            }
        }

        private void HandleCardClicked(int index)
        {
            if (_list == null)
            {
                return;
            }

            ProfileSummary summary = _list.Get(index);

            if (summary.IsBroken)
            {
                // 기획서 · 저장 파일을 읽을 수 없는 카드 입력 시 저장 데이터 오류 팝업을 출력한다
                if (BrokenProfileOpened != null)
                {
                    BrokenProfileOpened(index);
                }

                return;
            }

            if (summary.IsEmpty)
            {
                // 기획서 · 입력 시 해당 자리에 프로필을 생성하고 프로필명 입력 상태로 전환한다
                BeginEditName(index, _newProfileName);
                return;
            }

            // 기획서 · 입력 시 해당 프로필로 전환한다
            _list.CurrentIndex = index;

            if (ProfileChosen != null)
            {
                ProfileChosen(index);
            }
        }

        private void HandleEditClicked(int index)
        {
            if (_list == null)
            {
                return;
            }

            // 기획서 · 해당 카드의 프로필명 입력 상태로 전환한다
            BeginEditName(index, _list.Get(index).Name);
        }

        private void HandleDeleteClicked(int index)
        {
            // 기획서 · 입력 시 프로필 삭제 확인 팝업을 출력한다
            if (DeleteRequested != null)
            {
                DeleteRequested(index);
            }
        }

        private void HandleNameSubmitted(int index, string value)
        {
            if (_list == null)
            {
                return;
            }

            ProfileCardView card = _view != null ? _view.GetCard(index) : null;
            if (card != null)
            {
                card.EndEditName();
            }

            string name = value == null ? string.Empty : value.Trim();
            ProfileSummary before = _list.Get(index);

            if (string.IsNullOrEmpty(name))
            {
                // 이름을 비우면 만들지도 고치지도 않는다. 빈 자리는 빈 자리로 남는다.
                Refresh();
                return;
            }

            bool wasEmpty = before.IsEmpty;
            before.Name = name;
            _list.Set(index, before);
            Refresh();

            if (wasEmpty)
            {
                if (ProfileCreated != null)
                {
                    ProfileCreated(index, name);
                }

                return;
            }

            if (ProfileRenamed != null)
            {
                ProfileRenamed(index, name);
            }
        }

        private void BeginEditName(int index, string startingName)
        {
            ProfileCardView card = _view != null ? _view.GetCard(index) : null;
            if (card != null)
            {
                card.BeginEditName(startingName);
            }
        }

        private void HandleBackClicked()
        {
            // 기획서 · 타이틀 화면으로 이동한다
            if (BackRequested != null)
            {
                BackRequested();
            }
        }

        private int GetSlotCount(ProfileList list)
        {
            if (list.Count > 0)
            {
                return list.Count;
            }

            return ProfileList.DefaultSlotCount;
        }
    }
}
