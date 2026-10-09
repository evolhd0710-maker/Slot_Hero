using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;
using SlotHero.CurrentBuild;
using SlotHero.CurrentBuild.UI;
using SlotHero.Events;
using SlotHero.Events.UI;
using SlotHero.Hud.UI;
using SlotHero.Map;
using SlotHero.Map.UI;
using SlotHero.Popup;
using SlotHero.Popup.UI;
using SlotHero.Profile;
using SlotHero.Profile.UI;
using SlotHero.Reward;
using SlotHero.Reward.UI;
using SlotHero.RunResult;
using SlotHero.RunResult.UI;
using SlotHero.Sanctum;
using SlotHero.Sanctum.UI;
using SlotHero.Save;
using SlotHero.Settings;
using SlotHero.Settings.UI;
using SlotHero.Title;
using SlotHero.Title.UI;
using SlotHero.TopBar;
using SlotHero.TopBar.UI;

namespace SlotHero.Flow
{
    /// <summary>
    /// 화면을 이어 주는 곳.
    /// 타이틀에서 프로필로, 런으로, 맵에서 방으로, 런 종료 결과로 넘긴다.
    ///
    /// 화면 하나하나는 자기 일만 알고 다음에 무엇이 오는지는 모른다.
    /// 그것을 아는 것은 여기뿐이다. 화면들이 서로를 부르지 않게 하려는 것이다.
    ///
    /// 저장 시스템 기획서 v0.1 / 08 저장 시점 의 네 자리를 부르는 것도 여기 일이다.
    /// 런 시작, 방 진입, 방 완료, 런 종료 넷이다.
    ///
    /// 한 씬 안에서 오브젝트를 켜고 끈다. 씬을 갈아 끼우지 않는다.
    /// 설정과 팝업과 현재 빌드가 다른 화면 위에 겹쳐 떠야 하기 때문이다.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("저장")]
        [SerializeField] private SaveService _save;

        [Header("화면")]
        [SerializeField] private TitleScreenController _title;
        [SerializeField] private ProfileSelectScreenController _profileSelect;
        [SerializeField] private MapScreenController _map;
        [SerializeField] private SanctumController _sanctum;
        [SerializeField] private EventScreenController _event;
        [SerializeField] private RewardScreenController _reward;
        [SerializeField] private RunResultScreenController _runResult;

        [Tooltip("타이틀 화면 오른쪽 위의 현재 프로필 버튼. 눌러 프로필 선택 화면으로 간다.")]
        [SerializeField] private CurrentProfileButton _currentProfileButton;

        [Header("겹쳐 뜨는 것")]
        [SerializeField] private SettingsScreenController _settings;
        [SerializeField] private CurrentBuildScreenController _currentBuild;
        [SerializeField] private PopupPresenter _popup;

        [Tooltip("아이템 상세 오버레이. 행상, 보상 카드, 현재 빌드 칸에 커서를 올리면 뜬다.")]
        [SerializeField] private ItemDetailView _itemDetail;

        [Tooltip("고르기 화면. 성소 문양 변경과 08 피의 거래 처럼 가진 것이나 목록에서 하나를 직접 고를 때 연다.")]
        [SerializeField] private ItemPickerScreenController _picker;

        [Header("런 중에 보이는 것")]
        [SerializeField] private TopBarController _topBar;
        [SerializeField] private RunHudController _hud;

        [Header("설정 에셋")]
        [SerializeField] private RunCatalogConfig _catalogConfig;
        [SerializeField] private RunEventConfig _eventConfig;
        [SerializeField] private EventIllustrationConfig _eventIllustrations;
        [SerializeField] private RewardRuleConfig _rewardRules;
        [SerializeField] private MapVisualConfig _mapVisual;
        [SerializeField] private CurrentBuildVisualConfig _buildVisual;
        [SerializeField] private RunResultVisualConfig _resultVisual;

        [Header("전투")]
        [Tooltip("전투 창구. 전투 코드가 오기 전에는 대역을 붙인다.")]
        [SerializeField] private MonoBehaviour _combatEntryBehaviour;

        [Header("수치")]
        [Tooltip("타이틀 화면에 적는 빌드 번호.")]
        [SerializeField] private string _buildNumber = "0.1.0";

        [Tooltip("설정 창을 여는 키. 겹쳐 뜬 화면이 있으면 그쪽이 먼저 가져간다.")]
        [SerializeField] private KeyCode _settingsKey = KeyCode.Escape;

        [Tooltip("런 하나에 있는 스테이지 수. 마지막 스테이지의 보스를 깨면 런을 깬 것이다. " +
            "맵 한 장의 단계 열둘과 다른 값이다. 2스테이지 이후가 아직 정해지지 않아 1 로 둔다.")]
        [SerializeField] private int _stageCount = 1;

        private GameScreen _screen = GameScreen.None;
        private GameScreen _screenBehindOverlay = GameScreen.None;

        private RunCatalog _catalog;
        private ICombatEntry _combat;

        // 2026년 10월 8일에 책임 다섯을 떼어 냈다. 외부 검토가 셋을 권했고 원재가 하라고 했으며,
        // 원재가 지도 버튼과 팝업 처리도 나누라고 했다. 여기는 화면을 넘기고 다섯을 잇는 일만 한다.

        /// 런을 열고 닫는다. 저장과 복구.
        private RunSession _session;

        /// 방 보상 정산.
        private RoomRewardFlow _rewards;

        /// 이벤트 방 진행.
        private EventRoomFlow _events;

        /// 지도 버튼과 지도 잠깐 보기.
        private MapPeekFlow _mapPeek;

        /// 흐름이 띄우는 팝업의 글과 버튼.
        private FlowPopups _popups;

        private int _sanctumIndex;

        /// 지금 성소 상태가 어느 방의 것인지. 같은 방에 다시 들어오면 그대로 이어 간다.
        private int _sanctumNodeId = -1;

        private readonly RunClock _clock = new RunClock();

        /// 저장 파일을 잠시 읽지 못할 때 기다렸다 다시 읽는다.
        private readonly ReadRetry _readRetry = new ReadRetry();

        /// <summary>지금 열려 있는 화면.</summary>
        public GameScreen Screen
        {
            get { return _screen; }
        }

        /// <summary>진행 중인 런. 없으면 null 이다.</summary>
        public RunContext Context
        {
            get { return _session != null ? _session.Context : null; }
        }

        private void Awake()
        {
            EnsureParts();
        }

        /// <summary>
        /// 물건 목록과 떼어 낸 셋을 만든다. 한 번만 만든다.
        /// `OnEnable` 에서도 부른다. 편집 모드 검사처럼 `Awake` 보다 먼저 묶일 때도 비어 있지 않게 하려는 것이다.
        /// </summary>
        private void EnsureParts()
        {
            if (_catalog != null)
            {
                return;
            }

            _catalog = new RunCatalog(_catalogConfig);
            _session = new RunSession(_save, _catalog, RefreshTopBar);
            _rewards = new RoomRewardFlow(_rewardRules, _catalogConfig, _save);
            _events = new EventRoomFlow(_eventConfig, _catalogConfig, _buildVisual);
            _popups = new FlowPopups(_popup);
            _mapPeek = new MapPeekFlow(
                _map, _topBar, _currentBuild,
                () => Context != null,
                IsRoomInProgress,
                IsInputBlockingOverlayOpen);
            _combat = _combatEntryBehaviour as ICombatEntry;

            if (_combatEntryBehaviour != null && _combat == null)
            {
                Debug.LogWarning("전투 창구가 ICombatEntry 를 구현하지 않았다.", this);
            }
        }

        private void OnEnable()
        {
            EnsureParts();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// 게임을 켠 직후.
        ///
        /// 쓸 수 있는 프로필이 있으면 **타이틀 화면부터** 시작한다.
        /// 저장 쪽이 첫 프로필을 미리 만들어 두므로 처음 켠 사람도 타이틀로 간다.
        /// 프로필을 바꾸려면 05 타이틀 화면 의 현재 프로필 버튼을 누르면 된다.
        ///
        /// 프로필이 하나도 없을 때만 프로필 선택 화면에서 시작한다.
        /// 이름을 받지 않으면 런을 시작할 수가 없기 때문이다.
        /// </summary>
        private void Start()
        {
            CloseAll();

            if (_save == null)
            {
                GoToProfileSelect();
                return;
            }

            // 읽지 못하는 프로필이나 설정이 있으면 기다렸다 다시 읽은 뒤에 고른다. 마지막으로 논 프로필이 잠겨 있으면
            // 바로 고를 때 그 프로필을 건너뛰고 다른 프로필로 들어가기 때문이다. 끝내 못 읽어도 고르기는 한다.
            WaitForSaveFiles(
                () =>
                {
                    _save.TryRecoverSettings();
                    return !_save.HasUnreadableProfile() && !_save.IsSettingsUnread;
                },
                () => EnterStartingProfile(true),
                () =>
                {
                    // 끝내 못 읽은 것이 남았다. 설정은 이번에는 기본값으로 쓰되 파일을 덮지 않고, 프로필은 그 칸을 누를 때 다시 기다린다.
                    _popups.NotifyReadFailed();
                    EnterStartingProfile(false);
                },
                () => EnterStartingProfile(false));
        }

        /// <summary>
        /// 게임을 켤 때 들어갈 프로필을 골라 들어간다. 고를 수 없으면 프로필 선택 화면이다.
        /// waitAtTitle 을 끄면 타이틀에서 런을 못 읽어도 다시 기다리지 않고 이어하기만 꺼 둔다.
        /// 게임을 켤 때 이미 기다렸다 포기하거나 그만둔 뒤라 같은 파일을 또 30초 기다리지 않으려는 것이다. 2026년 10월 9일 전체 검토.
        /// </summary>
        private void EnterStartingProfile(bool waitAtTitle)
        {
            int profile = _save.PickStartingProfile();

            if (profile >= 0 && _save.SelectProfile(profile))
            {
                GoToTitle(false, waitAtTitle);
                return;
            }

            GoToProfileSelect();
        }

        private void Update()
        {
            _readRetry.Tick(Time.unscaledDeltaTime);
            TickClock();
            RefreshSettingsInput();
            ReadKeys();
            _mapPeek.RefreshButton();
            HideItemDetailOnScreenChange();
        }

        /// <summary>
        /// 런 시간을 센다. 설정 화면이 떠 있을 때만 멈춰 있다.
        ///
        /// **런 시간을 재는 곳은 여기 하나다.** 상단 표시줄은 받은 값을 보여 주기만 한다.
        /// 예전에는 표시줄에도 시계가 있어 둘이 따로 흘렀다. 2026년 10월 8일에 하나로 모았다.
        /// 실제로 흐른 시간을 센다(`unscaledDeltaTime`). 연출 속도를 `Time.timeScale` 로 바꿔도 빨라지지 않는다.
        /// </summary>
        private void TickClock()
        {
            RunContext context = Context;
            if (context == null)
            {
                return;
            }

            // 멈출지는 매 프레임 지금 상태를 읽어 정한다.
            _clock.SetPaused(IsClockPaused());
            _clock.Tick(Time.unscaledDeltaTime);
            context.Run.Status.ElapsedSeconds = (int)_clock.Seconds;

            if (_topBar != null)
            {
                _topBar.SetElapsedSeconds(_clock.Seconds);
            }
        }

        /// <summary>
        /// 키를 본다.
        ///
        /// ESC 는 겹쳐 뜬 화면이 있으면 그것을 닫고 없으면 설정을 연다.
        /// 설정과 현재 빌드 화면은 자기 ESC 를 스스로 다루므로 여기서는 건드리지 않는다.
        /// </summary>
        private void ReadKeys()
        {
            if (!Input.GetKeyDown(_settingsKey))
            {
                return;
            }

            // 겹쳐 뜬 것이 있으면 그쪽이 ESC 를 쓴다. 여기서는 가져가지 않는다.
            if (IsOverlayOpen())
            {
                return;
            }

            // 지도를 잠깐 띄워 보고 있으면 ESC 는 방으로 돌아가는 데 쓴다. 지도 버튼을 한 번 더 누른 것과 같다.
            if (_mapPeek.IsPeeking)
            {
                if (KeyGate.TryUse(_settingsKey))
                {
                    _mapPeek.End();
                }

                return;
            }

            // 프로필 선택은 그 앞이 없다. 여기서 설정을 열면 나갈 길이 헷갈린다.
            if (_screen == GameScreen.None || _screen == GameScreen.ProfileSelect)
            {
                return;
            }

            // 쓰기로 했으니 이제 가져간다.
            // 같은 프레임에 겹쳐 뜬 화면이 이 ESC 로 이미 닫혔으면 가져가지 못한다.
            // 그러면 닫자마자 설정이 다시 열리는 일이 없다.
            if (!KeyGate.TryUse(_settingsKey))
            {
                return;
            }

            OpenSettings();
        }

        /// <summary>겹쳐 뜬 화면이 하나라도 열려 있는지.</summary>
        private bool IsOverlayOpen()
        {
            if (_settings != null && _settings.IsOpen)
            {
                return true;
            }

            if (_currentBuild != null && _currentBuild.IsOpen)
            {
                return true;
            }

            if (_picker != null && _picker.IsOpen)
            {
                return true;
            }

            return _popups.IsOpen;
        }

        /// <summary>
        /// 다른 조작을 막는 것이 떠 있는지. 설정, 팝업, 고르기 화면이다.
        /// 현재 빌드는 막지 않는다. 지도 버튼으로 닫을 수 있어야 하기 때문이다.
        /// </summary>
        private bool IsInputBlockingOverlayOpen()
        {
            return (_settings != null && _settings.IsOpen)
                || _popups.IsOpen
                || (_picker != null && _picker.IsOpen);
        }

        /// <summary>
        /// 런 시간을 멈출지. **설정 화면만 멈춘다.**
        ///
        /// 현재 빌드, 잠깐 띄운 지도, 고르기 화면, 팝업은 모두 시간이 흐른다.
        /// 2026년 10월 6일 원재가 "팝업 때도 시간이 흘러야 하고 설정 화면 때만 멈춘다" 고 정했다.
        /// 예전에는 현재 빌드, 고르기 화면, 팝업에서도 멈췄다.
        /// </summary>
        private bool IsClockPaused()
        {
            return _settings != null && _settings.IsOpen;
        }

        // ---- 화면 넘기기 ----

        /// <summary>프로필 선택 화면을 연다. 게임을 켜면 여기서 시작한다.</summary>
        public void GoToProfileSelect()
        {
            CloseAll();
            _screen = GameScreen.ProfileSelect;

            if (_profileSelect != null && _save != null)
            {
                _profileSelect.Open(_save.BuildProfileList());
            }
        }

        /// <summary>타이틀 화면을 연다.</summary>
        public void GoToTitle()
        {
            GoToTitle(false, true);
        }

        /// <summary>
        /// 타이틀 화면을 연다. 저장된 런을 지금 읽지 못하면 waitIfUnreadable 일 때 기다렸다 다시 읽는다.
        /// continueWhenReadable 을 켜면 다시 읽혔을 때 바로 이어 한다. 이어하기를 눌렀다가 못 읽은 경우다.
        /// </summary>
        private void GoToTitle(bool continueWhenReadable, bool waitIfUnreadable)
        {
            CloseAll();
            _screen = GameScreen.Title;
            StopRunTimer();

            RefreshTitle();
            RefreshCurrentProfileButton();

            if (waitIfUnreadable && _save != null && _save.GetSavedRunState() == SavedRunState.Unreadable)
            {
                WaitForTitleRun(continueWhenReadable);
            }
        }

        /// <summary>타이틀을 지금 저장 상태로 다시 연다. 손상이면 손상 알림도 여기서 뜬다.</summary>
        private void RefreshTitle()
        {
            if (_title != null && _save != null && _screen == GameScreen.Title)
            {
                _title.Open(_save.GetSavedRunState(), _buildNumber);
            }
        }

        /// <summary>
        /// 타이틀에서 저장된 런이나 메타를 읽지 못했다. 기다렸다 다시 읽는다.
        /// 끝내 못 읽으면 백업과 런이 온전할 때 메타를 백업으로 되살리고 런과 이어 붙인다(`SaveService.ReloadRun(bool)`).
        /// 그래도 못 읽으면 읽지 못했다고 알리고 이어하기를 꺼 둔다. 타이틀을 다시 열면 다시 기다린다.
        /// </summary>
        private void WaitForTitleRun(bool continueWhenReadable)
        {
            WaitForSaveFiles(
                () =>
                {
                    _save.ReloadRun();
                    return _save.GetSavedRunState() != SavedRunState.Unreadable;
                },
                () => AfterTitleRunRead(continueWhenReadable),
                () =>
                {
                    _save.ReloadRun(true);
                    AfterTitleRunRead(continueWhenReadable);
                },
                null);
        }

        private void AfterTitleRunRead(bool continueWhenReadable)
        {
            if (_screen != GameScreen.Title)
            {
                return;
            }

            SavedRunState state = _save.GetSavedRunState();

            // 방금 다시 읽은 런으로 이어 한다. 또 읽으면 백업으로 되살린 경우 본 파일이 아직 잠겨 다시 막힌다.
            if (state == SavedRunState.Ready && continueWhenReadable)
            {
                ContinueRun(false);
                return;
            }

            RefreshTitle();

            if (state == SavedRunState.Unreadable)
            {
                _popups.NotifyReadFailed();
            }
        }

        /// <summary>
        /// 저장 파일을 읽을 수 있을 때까지 기다린다. 지금 읽히면 바로 onReadable 이다.
        /// 못 읽으면 기다리는 팝업을 띄우고 `SaveConfig.ReadRetryDelaySeconds` 마다 `ReadRetryCount` 번 다시 읽는다.
        /// 끝내 못 읽으면 onGaveUp, 그만두기를 누르면 onStopped 를 부른다.
        /// readable 은 다시 읽어 보는 일 자체다. 읽혔으면 true 를 돌려준다.
        /// 2026년 10월 9일 원재가 "팝업을 띄운 후 여유를 두고 재시도" 하라고 정했다.
        /// </summary>
        private void WaitForSaveFiles(Func<bool> readable, Action onReadable, Action onGaveUp, Action onStopped)
        {
            if (readable())
            {
                if (onReadable != null)
                {
                    onReadable();
                }

                return;
            }

            SaveConfig config = _save != null ? _save.Config : null;
            float delay = config != null ? config.ReadRetryDelaySeconds : 0f;
            int count = config != null ? config.ReadRetryCount : 0;

            _popups.ShowReadWaiting(() =>
            {
                _readRetry.Cancel();

                if (onStopped != null)
                {
                    onStopped();
                }
            });

            _readRetry.Begin(
                readable,
                delay,
                count,
                () =>
                {
                    _popups.HideReadWaiting();

                    if (onReadable != null)
                    {
                        onReadable();
                    }
                },
                () =>
                {
                    _popups.HideReadWaiting();

                    if (onGaveUp != null)
                    {
                        onGaveUp();
                    }
                });
        }

        /// <summary>
        /// 타이틀 오른쪽 위의 현재 프로필 이름을 지금 프로필로 맞춘다.
        /// 프로필을 바꾸거나 이름을 고치거나 지운 뒤에 부른다.
        /// </summary>
        private void RefreshCurrentProfileButton()
        {
            if (_currentProfileButton != null && _save != null)
            {
                _currentProfileButton.Show(_save.BuildProfileList());
            }
        }

        /// <summary>
        /// 방을 마치고 맵 화면으로 돌아간다. 지금 방을 깬 것으로 치고 다음 방을 고르게 한다.
        ///
        /// **지도 버튼은 여기로 오지 않는다.** 예전에는 왔고, 맵이 지금 방을 깬 것으로 쳐서
        /// 지도를 누르기만 해도 방이 끝났다. 지도 버튼은 `MapPeekFlow` 가 맡는다.
        /// </summary>
        public void GoToMap()
        {
            _screen = GameScreen.Map;
            _mapPeek.Forget();

            CloseRoomScreens();

            if (_reward != null)
            {
                _reward.Close();
            }

            if (_map != null)
            {
                _map.ReturnToMap();
            }

            ShowRunChrome(true);
            RefreshTopBar();
        }

        // ---- 런 시작과 이어하기 ----

        /// <summary>
        /// 런을 새로 시작한다.
        /// 08 저장 시점 의 "런 시작"이다.
        /// </summary>
        public void StartNewRun()
        {
            int seed = RunSession.MakeSeed();

            // 시작 아이템과 유물 순서를 넣은 뒤에 처음 쓴다. `RunSession.StartNew` 참고.
            RunSaveData run = _session.StartNew(seed);

            if (run == null)
            {
                return;
            }

            _sanctumIndex = 0;
            _sanctumNodeId = -1;

            CloseAll();
            _screen = GameScreen.Map;

            if (_map != null)
            {
                _map.StartNewStage(seed, run.StageIndex);
                _map.Open();
            }

            StartRunTimer(run.Status.ElapsedSeconds);
            ShowRunChrome(true);
            RefreshTopBar(false);
        }

        /// <summary>
        /// 저장된 런을 이어 한다. 늘 파일에서 다시 읽은 런으로 한다(`RunSession.Resume`).
        /// 그다음 그 런이 멈춘 자리에 맞춰 방이나 보상 화면, 다음 스테이지로 보낸다.
        /// </summary>
        public void ContinueRun()
        {
            ContinueRun(true);
        }

        /// <summary>
        /// 저장된 런을 이어 한다. reload 를 끄면 저장 창구가 막 다시 읽은 런을 그대로 쓴다.
        /// 타이틀에서 저장 파일을 기다렸다 다시 읽은 직후에 끈다(`AfterTitleRunRead`).
        /// </summary>
        private void ContinueRun(bool reload)
        {
            RunSaveData run = _session.Resume(reload);

            // 다시 읽지 못했으면 타이틀로 돌아간다. 잠시 못 읽은 것이면 거기서 기다렸다 다시 읽고, 읽히면 바로 이어 한다.
            if (run == null)
            {
                GoToTitle(true, true);
                return;
            }

            _sanctumIndex = 0;
            _sanctumNodeId = -1;

            CloseAll();
            _screen = GameScreen.Map;

            if (_map != null)
            {
                // 맵은 시드에서 다시 만든다. 지나온 길만 저장된 값으로 되돌린다.
                _map.StartNewStage(run.Seed, run.StageIndex);
                RestoreProgress(run);
                _map.Open();
            }

            StartRunTimer(run.Status.ElapsedSeconds);
            ShowRunChrome(true);
            RefreshTopBar(false);

            // **방 안에서 끝난 런은 그 방에 다시 들어간다.**
            // 저장은 방에 들어갈 때 하므로 방 안에서 "저장하고 나가기" 를 하거나 게임이 꺼지면
            // 지금 방을 아직 깨지 않은 채로 남는다. 그대로 맵만 열면 고를 수 있는 다음 방이 없고
            // 그 방으로 돌아갈 길도 없어 런이 막혔다. 방은 들어간 순간부터 다시 시작한다.
            MapNode current = GetCurrentNode();
            if (current != null && !run.CurrentRoomCleared)
            {
                EnterRoom(current);
                return;
            }

            // 방을 깨고 보상을 받기 전에 나간 런은 그 방의 보상 화면에서 시작한다.
            if (current != null && run.RewardPending)
            {
                ReopenReward(current);
                return;
            }

            // 보스 방을 깨고 보상까지 받았는데 다음 스테이지로 넘어가기 전에 꺼진 런.
            // 보스는 스테이지의 마지막 방이라 맵만 열면 고를 방이 없어 런이 막힌다. 여기서 마저 넘긴다.
            if (current != null && current.RoomType == RoomType.Boss && run.CurrentRoomCleared)
            {
                FinishBossStage();
            }
        }

        /// <summary>
        /// 받지 않고 남겨 둔 보상 화면을 다시 띄운다. 이어하기가 쓴다.
        ///
        /// 보상은 시드와 방에서 다시 만들고 오버킬은 저장해 둔 값을 얹는다.
        /// 모든 보상은 받을 때 한 번에 받으므로 다시 띄운 화면에는 보상 전부가 그대로 있다.
        /// </summary>
        private void ReopenReward(MapNode node)
        {
            RewardOffer offer = _rewards.Make(Context, node);

            if (offer == null || offer.IsEmpty)
            {
                // 받을 것이 없으면 띄울 까닭이 없다. 남은 표시만 지우고 방 다음으로 넘어간다.
                _rewards.ClearPending(Context);

                if (node.RoomType == RoomType.Boss)
                {
                    FinishBossStage();
                }

                return;
            }

            // 보상은 반투명한 막 위에 뜬다. 맵을 열어 두면 그 뒤로 비친다.
            if (_map != null)
            {
                _map.Close();
            }

            OpenReward(offer);
        }

        /// <summary>
        /// 런을 끝낸다.
        /// 08 저장 시점 의 "런 종료"다. 런 데이터를 지우고 메타를 갱신한다.
        /// </summary>
        public void EndRun(bool cleared, string endingText)
        {
            if (Context == null)
            {
                return;
            }

            // 완료 기록을 쓰지 못하면 저장 쪽이 런을 지우지 않고 쓰기 실패 팝업을 띄운다.
            // 결과 화면은 그대로 보여 준다. 런 파일이 남아 타이틀에서 그 방부터 이어 할 수 있다.
            CurrentBuildSnapshot finalBuild;
            RunSaveData finished = _session.End(cleared, RunSession.Today(), out finalBuild, out bool _);

            StopRunTimer();

            CloseAll();
            _screen = GameScreen.RunResult;

            if (_runResult != null)
            {
                _runResult.Open(RunResultMaker.Make(
                    finished, finalBuild, cleared, endingText,
                    _save != null ? _save.Meta : null, _resultVisual, _buildVisual));
            }
        }

        // ---- 방 ----

        /// <summary>
        /// 방에 들어간다.
        /// 08 저장 시점 의 "방 진입"이다.
        /// </summary>
        private void EnterRoom(MapNode node)
        {
            if (node == null || Context == null)
            {
                return;
            }

            // 앞 방의 전투 골드와 오버킬이 남아 있으면 이 방의 보상에 잘못 얹힌다. 들어갈 때 비운다.
            _rewards.BeginRoom(Context.Run);
            _save.OnRoomEntered(node.Id);
            RefreshTopBar();
            _mapPeek.Forget();

            // **방에 들어가면 맵을 닫는다.**
            // 예전에는 열어 둔 채 방 화면을 그 위에 얹었고, 방 화면이 화면을 꽉 채우는
            // 불투명한 바탕을 깔아 맵을 가렸다. 그 바탕을 걷어 공용 배경이 보이게 하자
            // 이벤트 방 뒤로 맵이 비쳐 보였다. 가리는 것에 기대지 않고 닫는다.
            if (_map != null)
            {
                _map.Close();
            }

            switch (node.RoomType)
            {
                case RoomType.Sanctum:
                    OpenSanctum(node);
                    break;

                case RoomType.Event:
                    OpenEvent(node);
                    break;

                default:
                    OpenCombat(node, CombatOptions.None);
                    break;
            }
        }

        /// <summary>
        /// 방을 깼다.
        /// 08 저장 시점 의 "방 완료"다. 보상을 고르기 전에 저장한다.
        /// </summary>
        private void ClearRoom()
        {
            if (_save == null)
            {
                return;
            }

            _save.OnRoomCleared();
            RefreshTopBar();
        }

        /// <summary>
        /// 성소에 들어간다.
        ///
        /// **같은 방에 다시 들어오면 보던 자리를 그대로 이어 간다.**
        /// 지도 버튼으로 잠깐 맵을 봤다가 돌아오는 길이 그렇다.
        /// 새로 만들면 진열이 다시 굴려져 마음에 드는 물건이 나올 때까지
        /// 지도를 들락거리면 되고, 쓴 야영도 되살아난다.
        /// </summary>
        private void OpenSanctum(MapNode node)
        {
            _screen = GameScreen.Sanctum;

            if (_sanctum == null)
            {
                LeaveRoom();
                return;
            }

            if (_sanctumNodeId == node.Id && _sanctum.State != null)
            {
                _sanctum.Open();
                return;
            }

            _sanctumNodeId = node.Id;

            // 성소마다 다른 진열이 나오게 하는 번호. 예전에는 이번에 켠 뒤로 몇 번째 성소인지 셌는데,
            // 그러면 이어하기를 할 때마다 처음부터 다시 세어 같은 성소에서 다른 진열이 나왔다.
            // 방 번호에서 정하면 언제 들어가도 같다.
            _sanctumIndex = RoomKey(node);
            _sanctum.Enter(
                Context.Run.Seed, _sanctumIndex, _catalog, Context, Context.Gold, _session.RelicPool,
                _catalog);
        }

        /// <summary>
        /// 이벤트 방에 들어간다. 각본을 고르고 굴리는 일은 `EventRoomFlow` 가 한다.
        /// 각본을 고를 수 없으면 방을 마친다. 그러지 않으면 화면이 멈춰 런이 막힌다.
        /// </summary>
        private void OpenEvent(MapNode node)
        {
            _screen = GameScreen.Event;

            if (_event == null)
            {
                LeaveRoom();
                return;
            }

            string eventId;
            EventPage page = _events.Begin(Context, RoomKey(node), out eventId);

            if (page == null)
            {
                LeaveRoom();
                return;
            }

            _event.Open(eventId, page, _eventIllustrations);
        }

        /// <summary>
        /// 전투를 연다. 방 전투는 `CombatOptions.None` 을, 이벤트가 연 전투는 이벤트가 정한 부정 효과와 보상 두 배를 넘긴다.
        /// </summary>
        private void OpenCombat(MapNode node, CombatOptions options)
        {
            _screen = GameScreen.Combat;

            if (_combat == null)
            {
                LeaveRoom();
                return;
            }

            _combat.Enter(Context, node, options, HandleCombatFinished);
        }

        private void HandleCombatFinished(CombatResult result)
        {
            if (Context == null)
            {
                return;
            }

            if (!result.Won)
            {
                EndRun(false, "쓰러짐");
                return;
            }

            RunSaveData run = Context.Run;
            run.Statistics.ReportHit(result.BiggestHit);

            // 전투 골드와 오버킬은 지금 주지 않고 보상에 쌓는다. `RoomRewardFlow.AddCombat` 참고.
            // 이벤트가 연 전투면 보상 두 배와 이벤트 전투 카드 고르기가 함께 쌓인다.
            _rewards.AddCombat(run, result, _events.IsWaitingForCombat ? _events.CombatOptions : CombatOptions.None);

            MapNode node = GetCurrentNode();
            if (node != null && node.RoomType == RoomType.Elite)
            {
                run.Statistics.ElitesKilled++;
            }
            else
            {
                run.Statistics.MonstersKilled++;
            }

            // **마지막 스테이지의 보스만 보상이 없다.** 방 완료를 저장하고 런을 깬 것으로 끝낸다.
            // 다른 스테이지의 보스는 일반 방처럼 보상을 받고, 받은 뒤 다음 스테이지로 넘어간다(ContinueAfterRoom).
            // 2026년 10월 6일 원재가 정했다. 예전에는 보스면 보상 없이 바로 다음 스테이지로 갔다.
            if (node != null && node.RoomType == RoomType.Boss && run.StageIndex >= _stageCount)
            {
                ClearRoom();
                EndRun(true, "보스 격파");
                return;
            }

            // 이벤트가 연 전투였으면 방을 끝내지 않고 이벤트로 돌아간다.
            // 기획서가 "전투가 끝나면 마무리 화면으로 넘어간다"로 정했기 때문이다.
            if (_events.IsWaitingForCombat)
            {
                string choiceId;
                EventStep step = _events.ResumeAfterCombat(out choiceId);

                _screen = GameScreen.Event;
                ApplyEventStep(step, choiceId);
                return;
            }

            LeaveRoom();
        }

        /// <summary>
        /// 다음 스테이지로 넘어간다.
        /// 맵을 시드와 새 스테이지 번호로 다시 만들고 지나온 방을 비운다.
        ///
        /// 같은 런 시드에 스테이지 번호만 바꾸므로 같은 런이면 늘 같은 맵이 나온다.
        /// 맵 생성 규칙 기획서가 `MapRandom.ForStage(runSeed, stageIndex)` 로 그렇게 정했다.
        /// </summary>
        private void AdvanceStage()
        {
            if (Context == null || _save == null)
            {
                return;
            }

            RunSaveData run = Context.Run;
            _save.OnStageAdvanced(run.StageIndex + 1);

            if (_map != null)
            {
                _map.StartNewStage(run.Seed, run.StageIndex);
            }

            _screen = GameScreen.Map;

            if (_combat != null)
            {
                _combat.Leave();
            }

            if (_map != null)
            {
                _map.Open();
            }

            ShowRunChrome(true);
            RefreshTopBar(false);
        }

        /// <summary>
        /// 방을 마친다.
        /// 방 완료를 저장한 뒤 보상 화면을 연다. 보상이 없으면 바로 맵으로 돌아간다.
        ///
        /// 08 저장 시점 이 "보상 목록이 정해진 순간" 저장하라고 정했으므로
        /// 보상 화면을 열기 전에 저장한다. 되돌아와도 같은 보상이 나오고 두 번 받을 수 없다.
        /// </summary>
        private void LeaveRoom()
        {
            // 지도를 띄운 채로 방이 끝나는 일은 막이 가려 없지만, 혹시 그러면 지도부터 걷는다.
            if (_mapPeek.IsPeeking)
            {
                _mapPeek.End();
            }

            // 이 방은 끝났다. 다음에 성소에 들어가면 진열을 새로 만든다.
            _sanctumNodeId = -1;
            _events.Forget();

            // 보상을 먼저 정해 두고, 받을 것이 있으면 방 완료와 함께 "보상 남음" 을 저장한다.
            // 카드를 고르기 전에 나갔다 이어하면 보상 화면을 다시 띄우려는 것이다.
            RewardOffer offer = _rewards.Make(Context, GetCurrentNode());
            _rewards.MarkPending(Context, offer);

            ClearRoom();

            if (!OpenReward(offer))
            {
                ContinueAfterRoom();
            }
        }

        /// <summary>
        /// 방과 그 보상까지 끝났다. 보스 방이면 다음 스테이지로, 아니면 맵으로 간다.
        /// 마지막 스테이지의 보스는 여기까지 오지 않는다. 깨는 순간 런이 끝난다.
        /// </summary>
        private void ContinueAfterRoom()
        {
            MapNode node = GetCurrentNode();

            if (node != null && node.RoomType == RoomType.Boss)
            {
                FinishBossStage();
                return;
            }

            GoToMap();
        }

        /// <summary>보스 방을 끝까지 마쳤다. 마지막 스테이지면 런을 깬 것이고 아니면 다음 스테이지로 넘어간다.</summary>
        private void FinishBossStage()
        {
            if (Context == null)
            {
                return;
            }

            if (Context.Run.StageIndex >= _stageCount)
            {
                EndRun(true, "보스 격파");
                return;
            }

            AdvanceStage();
        }

        /// <summary>
        /// 보상 화면을 연다. 열 것이 없으면 남은 보상 표시를 지우고 false 를 돌려준다.
        /// 무엇을 받을지 정하는 일은 `RoomRewardFlow` 가 한다. 여기서는 화면만 연다.
        /// </summary>
        private bool OpenReward(RewardOffer offer)
        {
            if (_reward == null || offer == null || offer.IsEmpty)
            {
                _rewards.ClearPending(Context);
                return false;
            }

            // 방 화면을 닫고 보상을 연다. 보상은 반투명한 막 위에 떠서
            // 방 화면을 남겨 두면 그 뒤로 비쳐 보인다.
            CloseRoomScreens();

            _screen = GameScreen.Reward;
            _reward.Open(offer, _catalog);

            // 화면이 열렸으면 다음으로 넘기는 것은 화면이 닫힐 때(HandleRewardClosed) 한다.
            // 여기서 또 넘기면 보스 방에서 스테이지 하나를 건너뛴다.
            return true;
        }

        /// <summary>보상을 받았다. 골드 전부와 고른 카드를 한 번에 받고 한 번에 저장한다(`RoomRewardFlow.Receive`).</summary>
        private void HandleRewardReceived(int gold, RewardCard card)
        {
            _rewards.Receive(Context, gold, card);
        }

        /// <summary>
        /// 보상 화면이 닫혔다. 이벤트 전투의 고르기가 남았으면 다음 고르기를 연다(35 도전자 의 보상 두 배).
        /// 아니면 남은 보상은 없다. 방 다음으로 넘어간다.
        /// </summary>
        private void HandleRewardClosed()
        {
            if (_rewards.HasMoreRounds(Context))
            {
                if (OpenReward(_rewards.Make(Context, GetCurrentNode())))
                {
                    return;
                }
            }

            _rewards.ClearPending(Context);
            ContinueAfterRoom();
        }

        // ---- 겹쳐 뜨는 화면 ----

        /// <summary>설정 화면을 연다. 어느 화면 위에서든 열린다.</summary>
        public void OpenSettings()
        {
            if (_settings == null || _save == null)
            {
                return;
            }

            _screenBehindOverlay = _screen;

            // 설정 파일을 읽지 못한 채 시작했으면 열기 전에 다시 읽어 본다. 읽히면 저장해 둔 값으로 연다.
            _save.TryRecoverSettings();
            _settings.Open(_save.Settings.Values);
        }

        /// <summary>
        /// 현재 빌드 화면을 연다.
        ///
        /// 지도 버튼과 같은 방식으로 움직인다. 띄우는 화면만 다르다.
        /// 지도를 잠깐 띄운 중이면 지도를 걷고 연다. 지도 버튼도 현재 빌드가 떠 있으면 그것을 닫고 지도를 띄운다.
        /// 예전에는 지도 위에 겹쳐 열려 닫으면 지도가 다시 보였다. 2026년 10월 6일 원재가 둘을 같게 하라고 했다.
        /// </summary>
        public void OpenCurrentBuild()
        {
            if (_currentBuild == null || Context == null)
            {
                return;
            }

            if (_mapPeek.IsPeeking)
            {
                _mapPeek.End();
            }

            _currentBuild.Open(Context.BuildSnapshot(), _catalog);
        }

        // ---- 아이템 상세 ----
        //
        // 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 아이템 상세 팝업.
        // 커서가 물건 위에 올라가면 커서 옆에 뜨고 벗어나면 사라진다.
        // 행상 진열, 보상 카드, 현재 빌드 칸 셋이 같은 오버레이를 쓴다.

        /// 오버레이를 띄울 때 열려 있던 화면. 화면이 바뀌면 지운다.
        private GameScreen _itemDetailScreen = GameScreen.None;
        private bool _itemDetailOverBuild;

        private void HandleMerchantDetail(MerchantItem item, bool entered)
        {
            SetItemDetail(item.Id, entered);
        }

        private void HandleRewardCardDetail(RewardCard card, bool hovering)
        {
            SetItemDetail(card != null ? card.Id : string.Empty, hovering);
        }

        private void HandleBuildSlotDetail(BuildEntry entry, bool hovering)
        {
            SetItemDetail(entry.Id, hovering);
        }

        private void SetItemDetail(string id, bool show)
        {
            if (_itemDetail == null)
            {
                return;
            }

            if (!show)
            {
                _itemDetail.Hide();
                return;
            }

            ItemDetailSpec spec = ItemDetailTexts.Describe(id, _catalogConfig, _buildVisual);
            if (!spec.IsValid)
            {
                _itemDetail.Hide();
                return;
            }

            _itemDetailScreen = _screen;
            _itemDetailOverBuild = _currentBuild != null && _currentBuild.IsOpen;
            _itemDetail.ShowAtCursor(spec);
        }

        /// <summary>
        /// 화면이 바뀌면 오버레이를 지운다.
        /// 물건을 누르면 그 칸이 사라지거나 화면이 넘어가 커서가 벗어났다는 알림이 오지 않을 수 있다.
        /// </summary>
        private void HideItemDetailOnScreenChange()
        {
            if (_itemDetail == null || !_itemDetail.IsShowing)
            {
                return;
            }

            bool overBuild = _currentBuild != null && _currentBuild.IsOpen;
            if (_screen != _itemDetailScreen || overBuild != _itemDetailOverBuild)
            {
                _itemDetail.Hide();
            }
        }

        // ---- 이벤트 묶기 ----

        private void Subscribe()
        {
            if (_title != null)
            {
                _title.ContinueRequested += ContinueRun;
                _title.NewGameStarted += StartNewRun;
                _title.NewGameConfirmRequested += ShowNewGameConfirm;
                _title.SettingsRequested += OpenSettings;
                _title.QuitConfirmRequested += ShowQuitConfirm;
                _title.RunDataBrokenNotice += ShowRunBrokenNotice;
            }

            if (_profileSelect != null)
            {
                _profileSelect.ProfileChosen += HandleProfileChosen;
                _profileSelect.ProfileDeleted += HandleProfileDeleted;
                _profileSelect.ProfileCreated += HandleProfileRenamed;
                _profileSelect.ProfileRenamed += HandleProfileRenamed;

                // 아래 셋을 빠뜨리면 그 버튼들이 눌려도 아무 일이 일어나지 않는다.
                // 삭제는 확인 팝업을 거쳐야 하므로 화면이 지우지 않고 알리기만 한다.
                _profileSelect.DeleteRequested += ShowProfileDeleteConfirm;
                _profileSelect.BackRequested += HandleProfileBack;
                _profileSelect.BrokenProfileOpened += ShowProfileBrokenNotice;
            }

            if (_currentProfileButton != null)
            {
                _currentProfileButton.Clicked += GoToProfileSelect;
            }

            if (_map != null)
            {
                _map.RoomEntered += EnterRoom;
            }

            if (_sanctum != null)
            {
                _sanctum.Exited += LeaveRoom;
                _sanctum.SymbolChangeTargetRequested += ShowSymbolChangeConfirm;
                _sanctum.RoomNeeded += HandleSanctumRoomNeeded;
                _sanctum.SymbolChanged += HandleSymbolChanged;
                // `ActionFailed` 는 받지 않는다. 성소 기획서 06 골드 는 "모자라면 소모 불가" 와 붉은 가격까지만 정했다.
                // 예전에는 "골드가 모자랍니다" 팝업을 띄웠는데 기획서에도 요청에도 없던 것이라 2026년 10월 6일에 뺐다.
                _sanctum.ItemDetailRequested += HandleMerchantDetail;
            }

            if (_event != null)
            {
                _event.Finished += HandleEventFinished;
                _event.ChoiceSelected += HandleEventChoice;
            }

            if (_reward != null)
            {
                _reward.Received += HandleRewardReceived;
                _reward.Closed += HandleRewardClosed;
                _reward.CanReceive = CanReceiveReward;
                _reward.CardHoverChanged += HandleRewardCardDetail;
            }

            if (_runResult != null)
            {
                _runResult.TitleRequested += GoToTitle;
            }

            if (_settings != null)
            {
                _settings.ValueChanged += HandleSettingChanged;
                _settings.RestoreRequested += ShowSettingsRestoreConfirm;
                _settings.RestoreApplied += HandleSettingsRestored;
                _settings.Closed += HandleSettingsClosed;
            }

            if (_topBar != null)
            {
                _topBar.ButtonClicked += HandleTopBarButton;
            }

            if (_hud != null)
            {
                _hud.CurrentBuildRequested += OpenCurrentBuild;
            }

            if (_currentBuild != null)
            {
                _currentBuild.Opened += HandleCurrentBuildOpened;
                _currentBuild.Closed += HandleCurrentBuildClosed;
                _currentBuild.SlotHoverChanged += HandleBuildSlotDetail;
            }

            if (_picker != null)
            {
                _picker.Picked += HandlePicked;
                _picker.Cancelled += HandlePickCancelled;
                _picker.SlotHoverChanged += HandleBuildSlotDetail;
            }

            if (_save != null)
            {
                _save.Saved += HandleAutoSaved;
                _save.NoticeRequested += HandleSaveNotice;
                // 런 데이터를 쓰기 직전에 유물 순서를 옮겨 담는다. `RunSession.StoreRelicPool` 참고.
                _save.RunSaving += _session.StoreRelicPool;
            }
        }

        private void Unsubscribe()
        {
            if (_title != null)
            {
                _title.ContinueRequested -= ContinueRun;
                _title.NewGameStarted -= StartNewRun;
                _title.NewGameConfirmRequested -= ShowNewGameConfirm;
                _title.SettingsRequested -= OpenSettings;
                _title.QuitConfirmRequested -= ShowQuitConfirm;
                _title.RunDataBrokenNotice -= ShowRunBrokenNotice;
            }

            if (_profileSelect != null)
            {
                _profileSelect.ProfileChosen -= HandleProfileChosen;
                _profileSelect.ProfileDeleted -= HandleProfileDeleted;
                _profileSelect.ProfileCreated -= HandleProfileRenamed;
                _profileSelect.ProfileRenamed -= HandleProfileRenamed;
                _profileSelect.DeleteRequested -= ShowProfileDeleteConfirm;
                _profileSelect.BackRequested -= HandleProfileBack;
                _profileSelect.BrokenProfileOpened -= ShowProfileBrokenNotice;
            }

            if (_currentProfileButton != null)
            {
                _currentProfileButton.Clicked -= GoToProfileSelect;
            }

            if (_map != null)
            {
                _map.RoomEntered -= EnterRoom;
            }

            if (_sanctum != null)
            {
                _sanctum.Exited -= LeaveRoom;
                _sanctum.SymbolChangeTargetRequested -= ShowSymbolChangeConfirm;
                _sanctum.RoomNeeded -= HandleSanctumRoomNeeded;
                _sanctum.SymbolChanged -= HandleSymbolChanged;
                _sanctum.ItemDetailRequested -= HandleMerchantDetail;
            }

            if (_event != null)
            {
                _event.Finished -= HandleEventFinished;
                _event.ChoiceSelected -= HandleEventChoice;
            }

            if (_reward != null)
            {
                _reward.Received -= HandleRewardReceived;
                _reward.Closed -= HandleRewardClosed;
                _reward.CanReceive = null;
                _reward.CardHoverChanged -= HandleRewardCardDetail;
            }

            if (_runResult != null)
            {
                _runResult.TitleRequested -= GoToTitle;
            }

            if (_settings != null)
            {
                _settings.ValueChanged -= HandleSettingChanged;
                _settings.RestoreRequested -= ShowSettingsRestoreConfirm;
                _settings.RestoreApplied -= HandleSettingsRestored;
                _settings.Closed -= HandleSettingsClosed;
            }

            if (_topBar != null)
            {
                _topBar.ButtonClicked -= HandleTopBarButton;
            }

            if (_hud != null)
            {
                _hud.CurrentBuildRequested -= OpenCurrentBuild;
            }

            if (_currentBuild != null)
            {
                _currentBuild.Opened -= HandleCurrentBuildOpened;
                _currentBuild.Closed -= HandleCurrentBuildClosed;
                _currentBuild.SlotHoverChanged -= HandleBuildSlotDetail;
            }

            if (_picker != null)
            {
                _picker.Picked -= HandlePicked;
                _picker.Cancelled -= HandlePickCancelled;
                _picker.SlotHoverChanged -= HandleBuildSlotDetail;
            }

            if (_save != null)
            {
                _save.Saved -= HandleAutoSaved;
                _save.NoticeRequested -= HandleSaveNotice;
                _save.RunSaving -= _session.StoreRelicPool;
            }
        }

        // ---- 받아 처리하기 ----

        private void HandleProfileChosen(int index)
        {
            if (_save == null)
            {
                return;
            }

            EnterProfile(index);
        }

        /// <summary>
        /// 그 프로필로 들어간다. 메타를 지금 읽지 못하면 기다렸다 다시 읽는다.
        /// 끝내 못 읽으면 백업과 런이 온전할 때 백업으로 되살려 들어가고(`SaveService.SelectProfile(int, bool)`),
        /// 그래도 안 되면 읽지 못했다고 알리고 프로필 선택 화면에 남는다. 그만두기를 눌러도 남는다.
        /// </summary>
        private void EnterProfile(int index)
        {
            WaitForSaveFiles(
                () => _save.SelectProfile(index),
                GoToTitle,
                () =>
                {
                    if (_save.SelectProfile(index, true))
                    {
                        GoToTitle();
                        return;
                    }

                    _popups.NotifyReadFailed();
                    RefreshProfileSelect();
                },
                RefreshProfileSelect);
        }

        /// <summary>프로필 선택 화면이 떠 있으면 목록을 다시 읽어 그린다.</summary>
        private void RefreshProfileSelect()
        {
            if (_screen == GameScreen.ProfileSelect && _profileSelect != null && _save != null)
            {
                _profileSelect.Open(_save.BuildProfileList());
            }
        }

        private void HandleProfileDeleted(int index)
        {
            if (_save != null)
            {
                _save.DeleteProfile(index);
            }

            RefreshCurrentProfileButton();
        }

        private void HandleProfileRenamed(int index, string name)
        {
            if (_save != null)
            {
                _save.RenameProfile(index, name);
            }

            RefreshCurrentProfileButton();
        }

        /// <summary>
        /// 프로필 선택 화면에서 뒤로를 눌렀을 때.
        /// 고른 프로필이 없으면 타이틀로 보내지 않는다. 타이틀에서 할 수 있는 것이 없기 때문이다.
        /// </summary>
        private void HandleProfileBack()
        {
            if (_save == null || _save.ProfileIndex < 0)
            {
                return;
            }

            GoToTitle();
        }

        /// <summary>
        /// 삭제를 눌렀을 때. 06 프로필 선택 화면 의 "프로필 삭제 확인 팝업"이다.
        /// 확인을 받은 뒤에야 화면이 그 자리를 비운다.
        /// </summary>
        private void ShowProfileDeleteConfirm(int index)
        {
            if (!_popups.Available || _profileSelect == null)
            {
                return;
            }

            string name = string.Empty;
            if (_profileSelect.List != null)
            {
                name = _profileSelect.List.Get(index).Name;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = (index + 1) + "번 프로필";
            }

            // 삭제를 누르면 화면이 그 자리를 비우고 다시 그린 뒤 ProfileDeleted 를 알린다.
            // 실제 파일을 지우는 것은 그 알림을 받아 HandleProfileDeleted 가 한다.
            _popups.ConfirmProfileDelete(name, () => _profileSelect.ConfirmDelete(index));
        }

        /// <summary>읽을 수 없는 프로필을 눌렀을 때. 06 프로필 선택 화면 의 저장 데이터 오류 팝업이다.</summary>
        private void ShowProfileBrokenNotice(int index)
        {
            // 손상이 아니라 지금 잠시 읽지 못한 칸이면 삭제를 권하지 않고 기다렸다 다시 읽어 들어간다.
            bool readFailed = _profileSelect != null && _profileSelect.List != null && _profileSelect.List.Get(index).IsReadFailed;
            if (_save != null && readFailed)
            {
                EnterProfile(index);
                return;
            }

            _popups.NotifyProfileBroken();
        }

        // ---- 행상의 문양 변경 ----

        // ---- 고르기 화면 ----

        /// 고르기 화면에서 고른 것을 받을 곳. 화면을 연 쪽이 넣는다.
        private Action<BuildEntry> _onPicked;

        /// 고르기 화면을 취소했을 때 할 일.
        private Action _onPickCancelled;

        /// <summary>고르기 화면을 연다. 고르면 onPicked, 취소하면 onCancelled 를 부른다.</summary>
        private void OpenPicker(ItemPickRequest request, Action<BuildEntry> onPicked, Action onCancelled)
        {
            _onPicked = onPicked;
            _onPickCancelled = onCancelled;
            _picker.Open(request, _catalog);
        }

        /// <summary>고르기 화면을 아무것도 알리지 않고 닫는다. 화면을 치울 때 쓴다.</summary>
        private void ClosePicker()
        {
            _onPicked = null;
            _onPickCancelled = null;

            if (_picker != null)
            {
                _picker.Close();
            }
        }

        private void HandlePicked(BuildEntry entry)
        {
            Action<BuildEntry> callback = _onPicked;
            _onPicked = null;
            _onPickCancelled = null;

            if (callback != null)
            {
                callback(entry);
            }
        }

        private void HandlePickCancelled()
        {
            Action callback = _onPickCancelled;
            _onPicked = null;
            _onPickCancelled = null;

            if (callback != null)
            {
                callback();
            }
        }

        // ---- 소지 한도 ----

        /// <summary>
        /// 코인이나 유물을 count 개 버릴 것을 고르게 한다. 요청은 `OwnedLimitFlow` 가 만든다.
        /// 다 고르면 고른 것을 한꺼번에 버리고 onRoom 을 부른다. 도중에 포기하면 아무것도 버리지 않고 onGaveUp 을 부른다.
        /// incomingName 이 비면 한도가 줄어 반드시 버리는 경우라 닫을 수 없다.
        /// 2026년 10월 9일 원재가 "기존에 가지고 있던 것을 버리고 획득하거나 획득을 포기해야 한다" 고 정했다.
        /// </summary>
        private void AskDiscard(ItemPickKind kind, int count, string incomingName, Action onRoom, Action onGaveUp)
        {
            AskDiscardStep(kind, count, incomingName, new List<string>(), onRoom, onGaveUp);
        }

        private void AskDiscardStep(
            ItemPickKind kind, int remaining, string incomingName, List<string> chosen, Action onRoom, Action onGaveUp)
        {
            if (Context == null)
            {
                return;
            }

            if (remaining <= 0)
            {
                for (int i = 0; i < chosen.Count; i++)
                {
                    Context.Discard(kind, chosen[i]);
                }

                RefreshTopBar();

                if (onRoom != null)
                {
                    onRoom();
                }

                return;
            }

            ItemPickRequest request = OwnedLimitFlow.MakeDiscardRequest(Context, kind, incomingName, chosen);

            // 고르기 화면이 없으면 물을 수 없다. 받을 때는 포기하고, 반드시 버려야 할 때는 최근에 얻은 것부터 버린다.
            // 한도가 0 이라 버릴 것이 없어도 고르기 화면은 띄운다. 칸 없이 포기만 할 수 있어 왜 못 받는지 보인다.
            if (_picker == null || (!request.Cancellable && request.Candidates.Count == 0))
            {
                if (request.Cancellable)
                {
                    if (onGaveUp != null)
                    {
                        onGaveUp();
                    }

                    return;
                }

                if (request.Candidates.Count > 0)
                {
                    chosen.Add(request.Candidates[request.Candidates.Count - 1].Id);
                }

                AskDiscardStep(kind, request.Candidates.Count > 0 ? remaining - 1 : 0, incomingName, chosen, onRoom, onGaveUp);
                return;
            }

            OpenPicker(
                request,
                entry =>
                {
                    chosen.Add(entry.Id);
                    AskDiscardStep(kind, remaining - 1, incomingName, chosen, onRoom, onGaveUp);
                },
                () =>
                {
                    if (onGaveUp != null)
                    {
                        onGaveUp();
                    }
                });
        }

        /// <summary>코인이나 유물이 소지 한도를 넘었는지. 한도가 줄면 생긴다.</summary>
        private bool HasOverflow()
        {
            return Context != null
                && (OwnedLimitFlow.Overflow(Context, ItemPickKind.Coin) > 0 || OwnedLimitFlow.Overflow(Context, ItemPickKind.Relic) > 0);
        }

        /// <summary>
        /// 소지 한도가 줄어 넘친 만큼 바로 버리게 한다. 코인부터 고르고 유물을 고른다. 닫을 수 없다.
        /// 2026년 10월 9일 원재가 "넘친 만큼 바로 버리게 한다" 로 정했다. 다 버리면 done 을 부른다.
        /// </summary>
        private void ResolveOverflow(Action done)
        {
            if (Context != null)
            {
                int coins = OwnedLimitFlow.Overflow(Context, ItemPickKind.Coin);
                if (coins > 0)
                {
                    AskDiscard(ItemPickKind.Coin, coins, string.Empty, () => ResolveOverflow(done), null);
                    return;
                }

                int relics = OwnedLimitFlow.Overflow(Context, ItemPickKind.Relic);
                if (relics > 0)
                {
                    AskDiscard(ItemPickKind.Relic, relics, string.Empty, () => ResolveOverflow(done), null);
                    return;
                }
            }

            if (done != null)
            {
                done();
            }
        }

        /// <summary>
        /// 보상 화면에서 고른 카드를 받아도 되는지. 코인이나 유물이 소지 한도까지 찼으면 버릴 것을 고르게 한다.
        /// 버리면 보상 화면이 마저 받고, 포기하면 보상 화면에 남아 다른 카드나 건너뛰기를 고를 수 있다.
        /// </summary>
        private bool CanReceiveReward(RewardCard card)
        {
            ItemPickKind kind;
            if (Context == null || !OwnedLimitFlow.TryGetKind(card, out kind))
            {
                return true;
            }

            int need = OwnedLimitFlow.DiscardsNeeded(Context, kind, 1);
            if (need <= 0)
            {
                return true;
            }

            AskDiscard(kind, need, card.DisplayName, () =>
            {
                if (_reward != null && _reward.IsOpen)
                {
                    _reward.Confirm();
                }
            }, null);
            return false;
        }

        /// <summary>
        /// 행상에서 코인이나 유물을 사려는데 소지 한도까지 찼다. 사기 전에 버릴 것을 고르게 한다.
        /// 버리면 마저 사고, 포기하면 사지 않아 골드도 그대로다. 2026년 10월 9일 원재가 정했다.
        /// </summary>
        private void HandleSanctumRoomNeeded(MerchantItem item)
        {
            ItemPickKind kind;
            if (_sanctum == null || Context == null || !OwnedLimitFlow.TryGetKind(item, out kind))
            {
                if (_sanctum != null)
                {
                    _sanctum.CancelPendingPurchase();
                }

                return;
            }

            string name = string.IsNullOrEmpty(item.DisplayName) ? item.Id : item.DisplayName;
            AskDiscard(
                kind,
                OwnedLimitFlow.DiscardsNeeded(Context, kind, 1),
                name,
                () => _sanctum.ConfirmPendingPurchase(),
                () => _sanctum.CancelPendingPurchase());
        }

        /// <summary>가진 문양을 고르기 화면 후보로 만든다. 현재 빌드 화면과 같은 칸이다.</summary>
        private List<BuildEntry> OwnedSymbolEntries(CurrentBuildSnapshot build)
        {
            return build != null ? new List<BuildEntry>(build.Symbols) : new List<BuildEntry>();
        }

        /// <summary>
        /// 문양 변경에서 바꿀 문양을 고른다.
        /// 05 행상 구성 의 문양 변경은 가진 문양 가운데 하나를 플레이어가 직접 고르는 것이다.
        /// 2026년 10월 5일에 고르기 화면을 만들어 그 화면에서 고르게 했다. 고르면 값을 치르고 바꾼다.
        /// 취소하면 값은 치르지 않는다.
        ///
        /// 고르기 화면이 없을 때만 예전처럼 시드 난수로 하나 골라 확인 팝업에 보여 준다.
        /// </summary>
        private void ShowSymbolChangeConfirm(IReadOnlyList<MerchantItem> owned)
        {
            if (_sanctum == null || Context == null)
            {
                CancelSymbolChange();
                return;
            }

            if (_picker != null)
            {
                CurrentBuildSnapshot build = Context.BuildSnapshot();
                ItemPickRequest request = new ItemPickRequest();
                request.Kind = ItemPickKind.Symbol;
                request.Title = "바꿀 문양 선택";
                request.Candidates = OwnedSymbolEntries(build);
                request.Build = build;
                request.ShowCount = true;
                request.ResultFormat = "[{0}] 을 무작위 문양으로 바꾼다";
                request.ConfirmText = "바꾸기";

                OpenPicker(request, HandleSymbolChangePicked, CancelSymbolChange);
                return;
            }

            if (!_popups.Available)
            {
                CancelSymbolChange();
                return;
            }

            int index = SymbolChangePicker.Pick(
                ToList(owned),
                Context.Run.Seed,
                _sanctumIndex,
                _sanctum.State != null ? _sanctum.State.SymbolChangeCount : 0);

            if (index < 0)
            {
                CancelSymbolChange();
                return;
            }

            string symbolId = owned[index].Id;
            string name = string.IsNullOrEmpty(owned[index].DisplayName)
                ? owned[index].Id
                : owned[index].DisplayName;

            _popups.ConfirmSymbolChangeFallback(
                name,
                () => _sanctum.ConfirmSymbolChange(symbolId),
                CancelSymbolChange);
        }

        private void HandleSymbolChangePicked(BuildEntry entry)
        {
            if (_sanctum != null)
            {
                _sanctum.ConfirmSymbolChange(entry.Id);
            }
        }

        private void CancelSymbolChange()
        {
            if (_sanctum != null)
            {
                _sanctum.CancelSymbolChange();
            }
        }

        /// <summary>
        /// 문양이 바뀌었다. 덱은 `RunCatalog.TryReplaceSymbol` 이 이미 고쳐 두었다.
        ///
        /// **여기서 저장하지 않는다.** 행상에서 산 것과 같이 방을 끝낼 때 함께 저장된다.
        /// 예전에는 여기서만 따로 저장해, 방 안에서 나갔다 이어하면 문양 변경은 남고
        /// 성소는 처음부터 다시 열려 변경 값도 야영도 되살아났다.
        /// </summary>
        private void HandleSymbolChanged(string oldSymbolId, MerchantItem newSymbol)
        {
            if (_save == null || Context == null)
            {
                return;
            }

            RefreshTopBar();

            string name = string.IsNullOrEmpty(newSymbol.DisplayName)
                ? newSymbol.Id
                : newSymbol.DisplayName;

            // **임시다.** 바뀐 문양은 팝업이 아니라 연출로 보여 줘야 한다. `FlowPopups.NotifySymbolChanged` 참고.
            _popups.NotifySymbolChanged(name);
        }

        /// <summary>읽기 전용 목록을 고르는 쪽이 쓰는 꼴로 옮긴다.</summary>
        private static List<MerchantItem> ToList(IReadOnlyList<MerchantItem> source)
        {
            List<MerchantItem> list = new List<MerchantItem>();

            if (source == null)
            {
                return list;
            }

            for (int i = 0; i < source.Count; i++)
            {
                list.Add(source[i]);
            }

            return list;
        }

        /// <summary>
        /// 이벤트 선택지를 골랐다. 각본을 굴리는 일은 `EventRoomFlow` 가 한다.
        ///
        /// 고르기 화면을 여는 선택지면 고르기 화면부터 연다. 고르면 그것을 대상으로 이어 가고,
        /// 취소하면 이벤트 화면에 그대로 남는다.
        /// 전투를 여는 선택지는 화면을 넘기지 않고 전투부터 시작한다. 전투가 끝나면 `HandleCombatFinished` 가 돌아온다.
        /// </summary>
        private void HandleEventChoice(string eventId, string choiceId)
        {
            if (_event == null || _events.Token == null)
            {
                return;
            }

            // 받을 코인이나 유물이 소지 한도를 넘으면 먼저 버릴 것을 고르게 한다.
            // 포기하면 그 선택지를 고르지 않은 것으로 하고 이벤트 화면에 남는다. 2026년 10월 9일 원재가 정했다.
            ItemPickKind kind;
            int count;
            string incomingName;
            if (Context != null && _events.TryGetRoomNeed(choiceId, out kind, out count, out incomingName))
            {
                object roomToken = _events.Token;
                AskDiscard(kind, count, incomingName, () =>
                {
                    if (_events.Token == roomToken)
                    {
                        ContinueEventChoice(choiceId);
                    }
                }, null);
                return;
            }

            ContinueEventChoice(choiceId);
        }

        /// <summary>소지 한도를 맞춘 뒤 이벤트 선택지를 마저 처리한다.</summary>
        private void ContinueEventChoice(string choiceId)
        {
            EventPickSource source = _events.GetPickSource(choiceId);
            if (source != EventPickSource.None && _picker != null && Context != null)
            {
                object token = _events.Token;
                OpenPicker(
                    _events.MakePickRequest(source, Context),
                    entry =>
                    {
                        // 고르는 사이 방이 바뀌었으면 받지 않는다.
                        if (_events.Token == token)
                        {
                            ApplyEventStep(_events.Choose(choiceId, entry.Id), choiceId);
                        }
                    },
                    null);
                return;
            }

            ApplyEventStep(_events.Choose(choiceId, string.Empty), choiceId);
        }

        /// <summary>각본이 내놓은 한 걸음을 화면에 반영한다. 무엇을 할지는 `EventRoomFlow.Interpret` 가 정한다.</summary>
        private void ApplyEventStep(EventStep step, string choiceId)
        {
            // **강제 손실로 체력이 0 이 되면 런이 끝난다.**
            // 4장 공통 규칙 이 다른 선택지가 모두 막히면 체력 손실 선택지를 열게 했으므로 이 길로 쓰러질 수 있다.
            // 2026년 10월 8일 외부 검토가 짚어 이었다.
            if (Context != null && Context.Health <= 0)
            {
                EndRun(false, "쓰러짐");
                return;
            }

            // 작업으로 소지 한도가 줄어 넘쳤으면 넘친 만큼 먼저 버리게 한다. 2026년 10월 9일 원재가 정했다.
            if (HasOverflow())
            {
                object token = _events.Token;
                ResolveOverflow(() =>
                {
                    if (_events.Token == token)
                    {
                        ShowEventStep(step, choiceId);
                    }
                });
                return;
            }

            ShowEventStep(step, choiceId);
        }

        /// <summary>각본이 내놓은 한 걸음을 할 일대로 화면에 반영한다.</summary>
        private void ShowEventStep(EventStep step, string choiceId)
        {
            switch (_events.Interpret(step, choiceId))
            {
                case EventStepAction.StartCombat:
                    // 도전 이벤트가 정한 부정 효과와 보상 두 배를 전투에 넘긴다(3장 도전 이벤트 공통 규칙).
                    OpenCombat(GetCurrentNode(), _events.CombatOptions);
                    break;

                case EventStepAction.Leave:
                    LeaveRoom();
                    break;

                default:
                    _event.ApplyResult(step.Result);
                    break;
            }
        }

        private void HandleEventFinished(string eventId)
        {
            LeaveRoom();
        }

        private void HandleTopBarButton(TopBarButtonKind kind)
        {
            switch (kind)
            {
                case TopBarButtonKind.Map:
                    _mapPeek.HandleButton();
                    break;

                case TopBarButtonKind.Settings:
                    OpenSettings();
                    break;

                case TopBarButtonKind.EndRun:
                    ShowEndRunConfirm();
                    break;
            }
        }

        // ---- 지도 버튼 ----
        //
        // 지도 버튼과 지도 잠깐 보기는 `MapPeekFlow` 가 맡는다. 2026년 10월 8일에 떼어 냈다.
        // 여기서는 방 진행 중인지만 알려 준다.

        /// <summary>지도를 잠깐 띄워 보고 있는지.</summary>
        public bool IsMapPeeking
        {
            get { return _mapPeek != null && _mapPeek.IsPeeking; }
        }

        /// <summary>방 진행 중인지. 방에 들어가 아직 맵으로 돌아오지 않았다. 보상 화면도 방 진행 중이다.</summary>
        private bool IsRoomInProgress()
        {
            return _screen == GameScreen.Sanctum
                || _screen == GameScreen.Event
                || _screen == GameScreen.Combat
                || _screen == GameScreen.Reward;
        }

        /// <summary>지도 버튼을 지금 상태에 맞게 켜고 끈다. 게임 씬 검사가 `Update` 대신 부른다.</summary>
        private void RefreshMapButton()
        {
            _mapPeek.RefreshButton();
        }

        private void HandleSettingChanged(SettingDefinition definition, float value)
        {
            if (_save != null && _settings != null)
            {
                // 설정 파일을 읽지 못한 채 시작했다가 이제 읽혔으면 값이 파일의 것으로 바뀐다. 화면을 다시 그린다.
                if (_save.OnSettingChanged(_settings.Values, definition != null ? definition.Id : null) && _settings.IsOpen)
                {
                    _settings.SelectTab(_settings.TabIndex);
                }
            }
        }

        /// <summary>
        /// 설정의 기본값 복원을 눌렀다. 07 설정 화면 의 "기본값 복원 확인 팝업" 을 띄운다.
        ///
        /// **예전에는 이 알림을 받는 곳이 없어 버튼을 눌러도 아무 일이 없었다.**
        /// 확인용 씬의 도우미만 받고 있었고, 게임 씬 검사는 버튼을 거치지 않고 되돌리기를 직접 불러 놓쳤다.
        /// 2026년 10월 8일에 찾아 고쳤다.
        ///
        /// 해상도와 창 모드처럼 되돌리지 않는 항목은 팝업에 이름을 적어 알린다.
        /// </summary>
        private void ShowSettingsRestoreConfirm()
        {
            if (_settings == null)
            {
                return;
            }

            if (!_popups.Available)
            {
                _settings.RestoreDefaults();
                return;
            }

            _popups.ConfirmSettingsRestore(
                _settings.RestoresCurrentTabOnly,
                _settings.GetKeptOnRestoreNames(),
                () =>
                {
                    if (_settings != null && _settings.IsOpen)
                    {
                        _settings.RestoreDefaults();
                    }
                });
        }

        /// <summary>
        /// 설정 화면이 키를 받을지 매 프레임 맞춘다. 설정 위에 팝업이 떠 있으면 받지 않는다.
        /// 지도 버튼처럼 여닫는 자리마다 맞추면 빠뜨리기 쉬워 지금 상태를 그때그때 본다.
        /// </summary>
        private void RefreshSettingsInput()
        {
            if (_settings != null)
            {
                _settings.InputBlocked = _popups.IsOpen;
            }
        }

        private void HandleSettingsRestored()
        {
            if (_save != null && _settings != null)
            {
                _save.OnSettingsApplied(_settings.Values);
            }
        }

        private void HandleSettingsClosed()
        {
            _screen = _screenBehindOverlay != GameScreen.None ? _screenBehindOverlay : _screen;
            _screenBehindOverlay = GameScreen.None;
        }

        private void HandleCurrentBuildOpened()
        {
            if (_hud != null)
            {
                _hud.SetCurrentBuildOpen(true);
            }
        }

        private void HandleCurrentBuildClosed()
        {
            if (_hud != null)
            {
                _hud.SetCurrentBuildOpen(false);
            }
        }

        private void HandleAutoSaved(AutoSavePoint point)
        {
            if (_hud != null && AutoSavePoints.ShowsNotice(point))
            {
                _hud.NotifyAutoSaved();
            }
        }

        private void HandleSaveNotice(SaveOutcome outcome, string text)
        {
            _popups.NotifySave(outcome, text);
        }

        // ---- 팝업 ----
        //
        // 팝업의 글과 버튼은 `FlowPopups` 가 맡는다. 2026년 10월 8일에 떼어 냈다.
        // 여기서는 확인을 받으면 무엇을 할지만 정한다.

        private void ShowNewGameConfirm()
        {
            if (_title == null)
            {
                return;
            }

            _popups.ConfirmNewGame(DescribeSavedRun(), () =>
            {
                if (_save != null)
                {
                    _save.DiscardRun();
                }

                _title.ConfirmNewGame();
            });
        }

        private void ShowEndRunConfirm()
        {
            _popups.AskEndRun(HandleEndRunConfirmClosed);
        }

        private void HandleEndRunConfirmClosed(string buttonId)
        {
            if (buttonId == FlowPopups.EndRunAbandonId)
            {
                EndRun(false, "런 포기");
                return;
            }

            if (buttonId == FlowPopups.EndRunSaveId)
            {
                // **방 안에서는 지금 상태를 쓰지 않는다.** 방에 들어갈 때 저장한 것을 그대로 둔다.
                // 이어하면 그 방에 처음부터 다시 들어가므로, 방 안에서 바뀐 것까지 써 두면
                // 야영으로 채운 체력이나 이벤트에서 받은 것을 들고 다시 들어가 한 번 더 받을 수 있다.
                // 저장 시스템 기획서 08 저장 시점 의 자동 저장도 방 진입과 방 완료 두 자리뿐이다.
                bool insideRoom = _screen == GameScreen.Sanctum
                    || _screen == GameScreen.Event
                    || _screen == GameScreen.Combat;

                if (_save != null && !insideRoom)
                {
                    _save.FlushRun();
                }

                StopRunTimer();
                _session.Detach();
                GoToTitle();
            }
        }

        private void ShowQuitConfirm()
        {
            _popups.ConfirmQuit(Application.Quit);
        }

        private void ShowRunBrokenNotice()
        {
            _popups.NotifyRunBroken();
        }
        // ---- 안쪽 ----

        private void CloseAll()
        {
            // 지도를 띄운 채로 런이 끝날 수 있다. 런 종료 팝업이 지도 위에서도 뜨기 때문이다.
            _mapPeek.Forget();

            // 화면을 옮기면 기다리던 다시 읽기도 놓는다. 다시 읽을 곳은 새 화면이 정한다.
            _readRetry.Cancel();
            _popups.HideReadWaiting();

            if (_title != null)
            {
                _title.Close();
            }

            if (_profileSelect != null)
            {
                _profileSelect.Close();
            }

            if (_map != null)
            {
                _map.Close();
            }

            // 화면을 치우는 것이지 방을 끝내는 것이 아니다.
            // `Exit` 을 부르면 `Exited` 가 울려 런을 끝내는 도중에 방 완료가 저장되고
            // 보상 화면까지 열린다.
            if (_sanctum != null)
            {
                _sanctum.Close();
            }

            if (_event != null)
            {
                _event.Close();
            }

            if (_reward != null)
            {
                _reward.Close();
            }

            if (_runResult != null)
            {
                _runResult.Close();
            }

            if (_currentBuild != null)
            {
                _currentBuild.Close();
            }

            if (_settings != null)
            {
                _settings.Close();
            }

            if (_itemDetail != null)
            {
                _itemDetail.Hide();
            }

            ClosePicker();
            _events.Forget();
            ShowRunChrome(false);
            _screen = GameScreen.None;
        }

        /// <summary>
        /// 성소와 이벤트와 전투 화면을 닫는다. 방을 끝낸 것으로 치지 않는다.
        /// 맵으로 돌아가거나 보상을 열 때 부른다.
        /// </summary>
        private void CloseRoomScreens()
        {
            // 성소는 `Exit` 이 아니라 `Close` 다. `Exit` 은 방을 끝냈다고 알린다.
            if (_sanctum != null)
            {
                _sanctum.Close();
            }

            if (_event != null)
            {
                _event.Close();
            }

            if (_combat != null)
            {
                _combat.Leave();
            }
        }

        /// <summary>런 중에만 보이는 상단 표시줄과 현재 빌드 버튼을 켜고 끈다.</summary>
        private void ShowRunChrome(bool show)
        {
            if (_topBar != null)
            {
                _topBar.gameObject.SetActive(show);
            }

            if (_hud != null)
            {
                _hud.gameObject.SetActive(show);
            }
        }

        private void RefreshTopBar()
        {
            RefreshTopBar(true);
        }

        /// <summary>
        /// 상단 표시줄을 지금 런에 맞춘다.
        ///
        /// 런을 막 시작했거나 이어했을 때는 굴리지 않는다.
        /// 굴리면 앞서 보던 값에서 새 값으로 흘러가 잠깐 엉뚱한 숫자가 보인다.
        /// </summary>
        private void RefreshTopBar(bool animate)
        {
            if (_topBar == null || Context == null)
            {
                return;
            }

            RunSaveData run = Context.Run;
            _topBar.SetHealth(run.Status.Health, run.Status.MaxHealth, animate);
            _topBar.SetGold(run.Status.Gold, animate);

            MapNode node = GetCurrentNode();
            Sprite roomIcon = null;
            string roomName = string.Empty;

            if (node != null && _mapVisual != null)
            {
                roomIcon = _mapVisual.GetVisual(node.RoomType).Icon;
                roomName = RoomTypes.GetDisplayName(node.RoomType);
            }

            // **적는 숫자는 맵의 단계다. 스테이지 번호가 아니다.**
            // 02 구성요소 의 위치는 "스테이지 아이콘 + 단계 + 방 아이콘" 이고
            // 05 정보 갱신 시점 이 "새로운 방 진입 시" 바뀌라고 정했다.
            // 스테이지 번호를 적으면 스테이지가 아직 하나뿐이라 방을 옮겨도 늘 1 로 멈춰 있다.
            int step = node != null ? node.Stage : 0;
            _topBar.SetLocation(null, step, roomIcon, roomName);
        }

        /// <summary>
        /// 방 하나를 런 전체에서 가리키는 번호. 이벤트 고르기, 이벤트 난수, 성소 진열이 쓴다.
        ///
        /// 노드 번호는 스테이지마다 0 부터 다시 매겨진다. 노드 번호만 쓰면 2스테이지의 5번 방에서
        /// 1스테이지의 5번 방과 똑같은 이벤트와 진열이 나온다. 그래서 스테이지를 섞는다.
        /// 한 스테이지의 노드는 마흔일곱을 넘지 않아 1000 이면 겹치지 않는다.
        /// </summary>
        private int RoomKey(MapNode node)
        {
            int stage = Context != null ? Context.Run.StageIndex : 1;
            return stage * 1000 + (node != null ? node.Id : 0);
        }

        private MapNode GetCurrentNode()
        {
            if (_map == null || _map.Map == null || Context == null)
            {
                return null;
            }

            return _map.Map.GetNode(Context.Run.CurrentNodeId);
        }

        /// <summary>저장된 지나온 길을 맵에 되돌린다.</summary>
        private void RestoreProgress(RunSaveData run)
        {
            if (_map == null || _map.Map == null)
            {
                return;
            }

            MapProgress progress = new MapProgress();
            progress.CurrentNodeId = run.CurrentNodeId;
            progress.CurrentRoomCleared = run.CurrentRoomCleared;
            progress.VisitedNodeIds.Clear();
            progress.VisitedNodeIds.AddRange(run.VisitedNodeIds);

            _map.Load(_map.Map, progress);
        }

        private void StartRunTimer(int fromSeconds)
        {
            _clock.Start(fromSeconds);

            if (_topBar != null)
            {
                _topBar.SetElapsedSeconds(_clock.Seconds);
            }
        }

        private void StopRunTimer()
        {
            _clock.Stop();
        }

        private string DescribeSavedRun()
        {
            if (_save == null || _save.Run == null)
            {
                return string.Empty;
            }

            RunSaveData run = _save.Run;
            return "스테이지 " + run.StageIndex + " · " + run.Statistics.RoomsCleared + "번째 방\n"
                + "유물 " + run.Owned.Relics.Count + "개 · 코인 " + run.Owned.CoinIds.Count
                + "개 · 문양 " + run.Owned.TotalSymbolCount + "장";
        }
    }
}
