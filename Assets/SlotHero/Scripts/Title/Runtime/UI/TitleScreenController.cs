using System;
using UnityEngine;

namespace SlotHero.Title.UI
{
    /// <summary>
    /// 타이틀 화면의 흐름.
    /// 인게임 화면 기획서 v0.2 / 05 타이틀 화면 을 맡는다.
    ///
    /// 기획서가 정한 갈림길을 그대로 나눈다.
    /// 저장된 런이 있으면 새 게임이 확인 팝업을 거치고 이어하기를 누를 수 있다.
    /// 저장된 런이 없으면 이어하기가 비활성이고 새 게임은 바로 시작한다.
    /// 런 데이터가 손상되었으면 화면을 열 때 알림을 띄우고 이어하기를 비활성으로 바꾼다.
    /// 손상되었어도 저장된 런이므로 새 게임은 확인 팝업을 거친다.
    ///
    /// 팝업은 Popup 폴더의 공통 틀이 맡고 여기서는 띄워 달라고 알리기만 한다.
    /// </summary>
    public class TitleScreenController : MonoBehaviour
    {
        [Header("화면")]
        [SerializeField] private TitleScreenView _view;

        [Header("설정")]
        [SerializeField] private TitleVisualConfig _visual;

        [Tooltip("켜 두면 시작할 때 화면을 감춘다. Open 을 부르면 나타난다.")]
        [SerializeField] private bool _hideOnAwake;

        private SavedRunState _savedRun = SavedRunState.None;
        private bool _brokenNoticeShown;

        /// <summary>새 런을 바로 시작할 때. 저장된 런이 없거나 확인을 받은 뒤다.</summary>
        public event Action NewGameStarted;

        /// <summary>
        /// 새 게임을 눌렀는데 저장된 런이 있을 때.
        /// 새 게임 확인 팝업을 띄우는 쪽이 받는다. 확인을 받으면 <see cref="ConfirmNewGame"/> 를 부른다.
        /// </summary>
        public event Action NewGameConfirmRequested;

        /// <summary>이어하기를 눌렀을 때. 저장된 자리로 들여보내는 쪽이 받는다.</summary>
        public event Action ContinueRequested;

        /// <summary>설정을 눌렀을 때.</summary>
        public event Action SettingsRequested;

        /// <summary>
        /// 종료를 눌렀을 때. 게임 종료 확인 팝업을 띄우는 쪽이 받는다.
        /// 확인을 받으면 그쪽에서 게임을 끝낸다.
        /// </summary>
        public event Action QuitConfirmRequested;

        /// <summary>
        /// 런 데이터 손상 알림을 띄워야 할 때.
        /// 화면을 한 번 여는 동안 한 번만 부른다.
        /// </summary>
        public event Action RunDataBrokenNotice;

        /// <summary>
        /// 메뉴에 마우스가 올라가거나 벗어났을 때. 누를 수 없는 까닭을 함께 넘긴다.
        /// 04 화면 공통 규칙 의 비활성 이유를 띄우는 데 쓴다.
        /// </summary>
        public event Action<TitleMenuKind, bool, string> MenuHoverChanged;

        /// <summary>지금 저장된 런의 상태.</summary>
        public SavedRunState SavedRun
        {
            get { return _savedRun; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.MenuClicked += HandleMenuClicked;
                _view.MenuHoverChanged += HandleMenuHoverChanged;

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
                _view.MenuClicked -= HandleMenuClicked;
                _view.MenuHoverChanged -= HandleMenuHoverChanged;
            }
        }

        /// <summary>
        /// 화면을 연다.
        /// 런 데이터가 손상되었으면 여는 순간 알림을 띄우라고 알린다.
        /// </summary>
        public void Open(SavedRunState savedRun, string buildNumber)
        {
            _savedRun = savedRun;

            if (_view != null)
            {
                _view.ShowMenu();
                _view.SetVersion(buildNumber);
                _view.SetVisible(true);
            }

            ApplyState();

            // 기획서 · 런 데이터가 존재하나 문제가 생긴 경우
            //           런 데이터 손상 알림을 출력하고 비활성으로 변경한다
            if (SavedRunStates.NeedsBrokenNotice(_savedRun) && !_brokenNoticeShown)
            {
                _brokenNoticeShown = true;

                if (RunDataBrokenNotice != null)
                {
                    RunDataBrokenNotice();
                }
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
        /// 저장된 런 상태를 바꾼다.
        /// 새 런을 시작하거나 프로필을 바꾼 뒤 돌아왔을 때 부른다.
        /// </summary>
        public void SetSavedRunState(SavedRunState savedRun)
        {
            _savedRun = savedRun;

            if (savedRun != SavedRunState.Broken)
            {
                _brokenNoticeShown = false;
            }

            ApplyState();
        }

        /// <summary>새 게임 확인 팝업에서 시작을 골랐을 때 부른다.</summary>
        public void ConfirmNewGame()
        {
            if (NewGameStarted != null)
            {
                NewGameStarted();
            }
        }

        /// <summary>저장된 런 상태에 맞춰 메뉴를 다시 그린다.</summary>
        public void ApplyState()
        {
            if (_view == null || _visual == null)
            {
                return;
            }

            TitleMenuItemView item = _view.GetItem(TitleMenuKind.Continue);
            if (item == null)
            {
                return;
            }

            bool canContinue = SavedRunStates.CanContinue(_savedRun);
            item.SetInteractable(canContinue, _visual.GetContinueDisabledReason(_savedRun));
        }

        private void HandleMenuClicked(TitleMenuKind kind)
        {
            switch (kind)
            {
                case TitleMenuKind.NewGame:
                    HandleNewGame();
                    break;

                case TitleMenuKind.Continue:
                    // 기획서 · 저장된 런 데이터를 불러와 저장된 위치의 런 진행 화면으로 진입한다
                    if (SavedRunStates.CanContinue(_savedRun) && ContinueRequested != null)
                    {
                        ContinueRequested();
                    }

                    break;

                case TitleMenuKind.Settings:
                    if (SettingsRequested != null)
                    {
                        SettingsRequested();
                    }

                    break;

                case TitleMenuKind.Quit:
                    // 기획서 · 게임 종료 확인 팝업을 거쳐 게임을 종료한다
                    if (QuitConfirmRequested != null)
                    {
                        QuitConfirmRequested();
                    }

                    break;
            }
        }

        private void HandleNewGame()
        {
            // 기획서 · 저장된 런이 있을 경우 새 게임 확인 팝업을 띄운다
            if (SavedRunStates.NeedsNewGameConfirm(_savedRun))
            {
                if (NewGameConfirmRequested != null)
                {
                    NewGameConfirmRequested();
                }

                return;
            }

            // 기획서 · 새 게임 입력 시 확인 팝업 없이 즉시 시작한다
            if (NewGameStarted != null)
            {
                NewGameStarted();
            }
        }

        private void HandleMenuHoverChanged(TitleMenuKind kind, bool hovering, string reason)
        {
            if (MenuHoverChanged != null)
            {
                MenuHoverChanged(kind, hovering, reason);
            }
        }
    }
}
