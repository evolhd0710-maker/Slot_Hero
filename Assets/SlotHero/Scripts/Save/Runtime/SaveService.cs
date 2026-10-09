using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Profile;
using SlotHero.Settings;
using SlotHero.Title;

namespace SlotHero.Save
{
    /// <summary>
    /// 저장을 맡는 창구. 화면들이 이것만 보면 된다.
    /// 저장 시스템 기획서 v0.1 / 08 저장 시점 의 표를 실제로 부르는 자리다.
    ///
    /// 자료를 읽고 쓰는 일은 `SaveRepository` 가 하고
    /// 여기서는 언제 부를지와 화면에 무엇을 넘길지를 정한다.
    ///
    /// 프로필 목록과 저장된 런 상태를 화면이 쓰는 꼴로 바꿔 주는 것도 여기 일이다.
    /// </summary>
    public class SaveService : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private SaveConfig _config;

        [Header("동작")]
        [Tooltip("켜면 Awake 에서 저장 폴더를 열고 남은 임시 파일을 치운다.")]
        [SerializeField] private bool _initializeOnAwake = true;

        private SaveRepository _repository;
        private ISaveStorage _storage;

        private int _profileIndex = -1;
        private MetaSaveData _meta;
        private RunSaveData _run;

        /// 지금 프로필의 메타를 쓰다 실패해 파일이 메모리보다 옛것인지. 다음 런 저장 때 다시 쓴다(`SaveCurrentMeta`).
        private bool _metaPending;

        /// 설정 파일을 읽지 못한 채 기본값으로 시작했는지. 이때는 파일을 덮어쓰지 않는다(`OnSettingChanged`).
        private bool _settingsUnread;

        /// 설정을 읽지 못한 동안 바꾼 항목. 다시 읽히면 파일의 값 위에 이것만 얹는다.
        private readonly HashSet<string> _touchedSettings = new HashSet<string>();

        /// 설정을 읽지 못해 쓰지 않았다는 알림을 이미 띄웠는지. 값을 끌 때마다 뜨지 않게 한 번만 띄운다.
        private bool _settingsDeferNoticeShown;
        private SettingsSaveData _settings;

        /// <summary>저장이 일어났다. 자동 저장 안내를 띄우는 쪽이 듣는다.</summary>
        public event Action<AutoSavePoint> Saved;

        /// <summary>팝업을 띄워야 하는 일이 생겼다. 글이 함께 온다.</summary>
        public event Action<SaveOutcome, string> NoticeRequested;

        /// <summary>
        /// 런 데이터를 막 쓰려고 한다. 쓰기 전에 런 데이터에 옮겨 담을 것이 있으면 여기서 담는다.
        /// 흐름이 유물 순서를 담는다. 쓰는 자리가 여럿이라 자리마다 담으면 빠뜨리기 쉽다.
        /// </summary>
        public event Action<RunSaveData> RunSaving;

        /// <summary>지금 고른 프로필 번호. 아직 고르지 않았으면 -1 이다.</summary>
        public int ProfileIndex
        {
            get { return _profileIndex; }
        }

        /// <summary>지금 프로필의 메타 데이터.</summary>
        public MetaSaveData Meta
        {
            get { return _meta; }
        }

        /// <summary>진행 중인 런. 없으면 null 이다.</summary>
        public RunSaveData Run
        {
            get { return _run; }
        }

        /// <summary>기기 설정.</summary>
        public SettingsSaveData Settings
        {
            get { return _settings; }
        }

        /// <summary>저장 설정 에셋.</summary>
        public SaveConfig Config
        {
            get { return _config; }
        }

        /// <summary>
        /// 오늘 날짜를 주는 곳. 런 데이터를 저장할 때 마지막으로 논 날짜에 적는다.
        /// 비워 두면 기기 시계로 `FormatToday` 를 쓴다. 검사가 날짜를 고정할 때 바꾼다.
        /// </summary>
        public Func<string> Today;

        /// <summary>메타에 적는 오늘 날짜. `yyyy-MM-dd` 라 글자끼리 견주면 날짜 순서가 된다.</summary>
        public static string FormatToday()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                Initialize(null);
            }
        }

        /// <summary>
        /// 저장 창구를 연다.
        /// `storage` 를 주지 않으면 진짜 파일에 쓴다. 검사할 때는 메모리 창구를 넣는다.
        /// </summary>
        public void Initialize(ISaveStorage storage)
        {
            if (_config == null)
            {
                Debug.LogWarning("저장 설정 에셋이 비어 있다. 저장이 돌지 않는다.", this);
                return;
            }

            _storage = storage != null ? storage : FileSaveStorage.CreateDefault(_config);
            _repository = new SaveRepository(_config, _storage);

            // 쓰다 꺼져 남은 임시 파일을 먼저 치운다.
            _repository.CleanTempFiles();

            // 설정을 읽지 못했으면 이번에는 기본값으로 시작하되 파일을 덮어쓰지 않는다(`OnSettingChanged`).
            // 알림은 흐름이 기다렸다 다시 읽은 뒤에도 못 읽을 때 띄운다.
            _settings = _repository.LoadSettings(out SaveOutcome settingsOutcome);
            _settingsUnread = settingsOutcome == SaveOutcome.ReadFailed;
            _settingsDeferNoticeShown = false;
            _touchedSettings.Clear();

            if (!_settingsUnread)
            {
                Report(settingsOutcome);
            }

            EnsureFirstProfile();
        }

        /// <summary>
        /// 게임을 처음 켰을 때 첫 프로필을 미리 만들어 둔다.
        ///
        /// 이게 없으면 프로필 선택 화면의 세 칸이 모두 비어 있어
        /// 이름을 직접 쳐 넣어야 게임을 시작할 수 있다.
        ///
        /// **이름은 원래 스팀이나 스토브 닉네임을 받아 쓸 자리다.**
        /// 아직 연동이 없어 설정의 `FirstProfileName` 을 쓴다.
        ///
        /// 하나라도 프로필이 있으면 손대지 않는다.
        /// 플레이어가 첫 칸을 일부러 지운 것을 다시 만들어 주면 안 된다.
        /// </summary>
        private void EnsureFirstProfile()
        {
            if (_config == null || !_config.CreateFirstProfileOnStart)
            {
                return;
            }

            for (int i = 0; i < _config.ProfileCount; i++)
            {
                if (_repository.ProfileExists(i))
                {
                    return;
                }
            }

            string name = _config.FirstProfileName;
            if (string.IsNullOrEmpty(name))
            {
                name = _repository.GetDefaultProfileName(0);
            }

            RenameProfile(0, name);
        }

        // ---- 프로필 ----

        /// <summary>
        /// 프로필 화면에 넘길 목록을 만든다.
        /// 읽다가 문제가 생긴 프로필은 손상으로 표시한다.
        /// </summary>
        public ProfileList BuildProfileList()
        {
            ProfileList list = ProfileList.CreateEmpty(_config.ProfileCount);

            for (int i = 0; i < _config.ProfileCount; i++)
            {
                if (!_repository.ProfileExists(i))
                {
                    list.Clear(i);
                    continue;
                }

                MetaSaveData meta = _repository.LoadMeta(i, out SaveOutcome outcome);

                // 읽지 못했으면 손상과 따로 표시한다. 눌렀을 때 "다시 시도" 를 안내하고 아무것도 고치지 않는다.
                // 이름을 모르므로 기본 이름을 적는다. 비워 두면 빈 자리로 보여 그 위에 새 프로필을 만들 수 있다.
                if (outcome == SaveOutcome.ReadFailed || meta == null)
                {
                    list.Set(i, ProfileSummary.Unreadable(_repository.GetDefaultProfileName(i)));
                    continue;
                }

                // 백업으로 되살렸으면 알린다. 되살린 본 파일을 써 두므로 다음에 읽을 때는 다시 뜨지 않는다.
                if (outcome == SaveOutcome.MetaRestoredFromBackup)
                {
                    Report(outcome);
                }

                if (outcome == SaveOutcome.MetaResetProfile)
                {
                    list.Set(i, ProfileSummary.Broken(meta.ProfileName));
                    continue;
                }

                list.Set(i, ProfileSummary.Filled(
                    meta.ProfileName, meta.TotalPlayTimeSeconds, meta.LastPlayedDate));
            }

            list.CurrentIndex = _profileIndex;
            return list;
        }

        /// <summary>
        /// 게임을 켤 때 들어갈 프로필을 고른다.
        ///
        /// 마지막으로 논 프로필을 고른다. 날짜 꼴이 `yyyy-MM-dd` 라 글자끼리 견주면 된다.
        /// 날짜가 같거나 아직 논 적이 없으면 번호가 작은 쪽이다.
        /// 읽을 수 없는 프로필은 고르지 않는다. 그 자리를 골라 봐야 오류 팝업만 뜬다.
        ///
        /// 쓸 수 있는 프로필이 하나도 없으면 -1 을 돌려준다.
        /// 그때는 프로필 선택 화면에서 이름을 받아야 게임을 시작할 수 있다.
        ///
        /// 05 타이틀 화면 에 현재 프로필 버튼이 있으므로
        /// 알아서 고른 뒤에도 프로필을 바꿀 길은 남아 있다.
        /// </summary>
        public int PickStartingProfile()
        {
            if (_config == null || _repository == null)
            {
                return -1;
            }

            // 고르는 규칙 자체는 `ProfileList` 에 있다. 거기 두면 유니티 없이 검사할 수 있다.
            return BuildProfileList().PickStarting();
        }

        /// <summary>
        /// 프로필을 고른다. 메타와 런을 읽어 들인다. 고르지 못했으면 false 다.
        /// 메타와 런이 어긋나면 여기서 맞춘다.
        /// 메타를 읽지 못했으면(`SaveOutcome.ReadFailed`) 고르지 않는다. 지금 고른 프로필도 그대로다.
        /// </summary>
        public bool SelectProfile(int profileIndex)
        {
            return SelectProfile(profileIndex, false);
        }

        /// <summary>
        /// 프로필을 고른다. restoreFromBackup 을 켜면 본 메타를 읽지 못할 때 백업으로 되살려 고른다.
        /// 되살리는 것은 백업과 런이 온전할 때뿐이다(`SaveRepository.RestoreUnreadableMeta`).
        /// 흐름이 기다리며 다시 읽어도 끝내 읽지 못했을 때 켠다.
        ///
        /// **읽기 실패는 여기서 알리지 않는다.** 기다렸다 다시 읽는 것과 끝내 못 읽었을 때의 알림은 흐름이 맡는다.
        /// 여기서도 알리면 기다리는 팝업 위에 실패 팝업이 겹친다.
        /// </summary>
        public bool SelectProfile(int profileIndex, bool restoreFromBackup)
        {
            if (!_config.IsValidProfileIndex(profileIndex))
            {
                return false;
            }

            // 지금 프로필의 메타를 아직 쓰지 못했으면 먼저 쓴다. 파일의 메타는 그보다 옛것이라
            // 그대로 읽으면 "런 없음" 이 적힌 메타가 들어와 멀쩡한 런을 지우고 런 횟수도 되돌아간다.
            // 다른 프로필로 옮길 때도 먼저 써 본다. 옮기면 메모리의 메타를 놓기 때문이다.
            // 옮긴 뒤 끝내 못 쓴 메타는 다음에 그 프로필을 고를 때 `ReconcileRunFlag` 가 런에 맞춰 되살린다.
            if (_metaPending && _meta != null)
            {
                SaveCurrentMeta();

                // 그래도 쓰지 못했으면 메타는 메모리의 것을 그대로 두고 **런만 파일에서 다시 읽는다.**
                // 메타가 런보다 중요하다(2026년 10월 9일 원재). 런은 파일의 것, 곧 방에 들어갈 때의 것으로 돌아가야
                // 방 안에서 쓴 골드나 채운 체력을 들고 다시 들어가지 않는다. 2026년 10월 9일 외부 검토.
                if (_metaPending && profileIndex == _profileIndex)
                {
                    ReadRunOnly(profileIndex);
                    return true;
                }
            }

            SaveOutcome metaOutcome;
            MetaSaveData meta = restoreFromBackup
                ? _repository.RestoreUnreadableMeta(profileIndex, out metaOutcome)
                : _repository.LoadMeta(profileIndex, out metaOutcome);

            if (metaOutcome != SaveOutcome.ReadFailed)
            {
                Report(metaOutcome);
            }

            if (meta == null)
            {
                return false;
            }

            _profileIndex = profileIndex;
            _meta = meta;
            _metaPending = false;

            // **메타와 런 파일을 먼저 맞추고 나서 런을 읽는다.**
            // 예전에는 런을 먼저 읽어, 메타는 "런 없음" 인데 런 파일이 남은 경우 파일은 지워도
            // 메모리의 런이 남아 이어하기가 켜졌다. 2026년 10월 8일에 순서를 바꿨다.
            // 런 종료가 메타를 쓴 뒤 런 파일을 지우기 전에 꺼지면 바로 이 모양이 남는다.
            Report(_repository.ReconcileRunFlag(_meta));
            ReadRunOnly(profileIndex);
            return true;
        }

        /// <summary>
        /// 지금 메타는 그대로 두고 런만 파일에서 읽는다. 메타와 런을 맞추는 것은 부르는 쪽이 먼저 한다.
        /// 런이 망가져 지웠으면 메타의 "런 있음" 도 내리고, 예전 런이면 런 횟수를 그 번호까지 맞춘다.
        /// </summary>
        private void ReadRunOnly(int profileIndex)
        {
            _run = _repository.LoadRun(profileIndex, out SaveOutcome runOutcome);
            if (runOutcome != SaveOutcome.ReadFailed)
            {
                Report(runOutcome);
            }

            // 런을 버렸으면 메타의 적힌 값도 함께 내려 준다.
            if (runOutcome == SaveOutcome.RunDiscarded && _meta.HasSavedRun)
            {
                _meta.HasSavedRun = false;
                SaveCurrentMeta();
            }

            // 런 횟수를 시작할 때 세기 전에 시작한 런이면 그 번호까지 맞춘다.
            // 안 맞추면 그 런을 버리고 새로 시작할 때 같은 번호가 다시 붙는다. 2026년 10월 9일 외부 검토.
            if (CountRun(_run))
            {
                SaveCurrentMeta();
            }
        }

        /// <summary>
        /// 그 프로필의 메타를 지금 읽을 수 없는지. 손상과 다르다. 본 메타가 온전하면 백업이 잠겨 있어도 읽을 수 있다(`SaveRepository.CannotReadMeta`).
        /// </summary>
        public bool CannotReadProfile(int profileIndex)
        {
            return _repository != null && _config.IsValidProfileIndex(profileIndex) && _repository.CannotReadMeta(profileIndex);
        }

        /// <summary>그 프로필의 런을 지금 읽을 수 없는지. 손상과 다르다.</summary>
        public bool CannotReadRun(int profileIndex)
        {
            return _repository != null && _config.IsValidProfileIndex(profileIndex) && _repository.CannotReadRun(profileIndex);
        }

        /// <summary>
        /// 그 프로필의 메타와 런을 모두 지금 읽을 수 있는지. 흐름이 기다렸다 다시 읽을 때 이것으로 본다.
        /// 저장 데이터가 없는 프로필은 읽을 것이 없으므로 true 다.
        /// </summary>
        public bool CanReadProfile(int profileIndex)
        {
            return !CannotReadProfile(profileIndex) && !CannotReadRun(profileIndex);
        }

        /// <summary>지금 읽을 수 없는 프로필이 하나라도 있는지. 게임을 켤 때 기다렸다 다시 읽을지 정한다.</summary>
        public bool HasUnreadableProfile()
        {
            if (_repository == null)
            {
                return false;
            }

            for (int i = 0; i < _config.ProfileCount; i++)
            {
                if (!CanReadProfile(i))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지금 프로필의 런을 파일에서 다시 읽는다. 이어하기가 이것을 거친다.
        ///
        /// 방 안에서 "저장하고 나가기" 를 하면 파일에는 방에 들어갈 때의 런이 남지만
        /// 메모리의 런은 방 안에서 바뀐 채로 남는다. 메모리의 것으로 이어 하면
        /// 게임을 껐다 켜서 이어 할 때와 결과가 달라진다.
        /// 저장 시스템 기획서 v0.1 / 08 저장 시점 은 방 진입과 방 완료에만 런을 쓴다.
        /// </summary>
        public void ReloadRun()
        {
            ReloadRun(false);
        }

        /// <summary>
        /// 지금 프로필의 런을 파일에서 다시 읽는다. restoreFromBackup 을 켜면 본 메타를 읽지 못할 때 백업으로 되살린다.
        /// 흐름이 기다리며 다시 읽어도 끝내 못 읽었을 때 켠다(`SelectProfile(int, bool)`).
        /// </summary>
        public void ReloadRun(bool restoreFromBackup)
        {
            if (_profileIndex < 0)
            {
                return;
            }

            // 다시 읽지 못했으면 메모리에 남은 런으로 이어 하지 않는다. 파일과 다를 수 있다.
            if (!SelectProfile(_profileIndex, restoreFromBackup))
            {
                _run = null;
            }
        }

        /// <summary>프로필을 지운다. 그 프로필을 고르고 있었으면 놓는다.</summary>
        public void DeleteProfile(int profileIndex)
        {
            _repository.DeleteProfile(profileIndex);

            if (_profileIndex == profileIndex)
            {
                _profileIndex = -1;
                _meta = null;
                _run = null;
                _metaPending = false;
            }
        }

        /// <summary>프로필 이름을 바꾼다.</summary>
        public void RenameProfile(int profileIndex, string name)
        {
            SaveOutcome outcome = SaveOutcome.Ok;
            MetaSaveData meta = _profileIndex == profileIndex && _meta != null
                ? _meta
                : _repository.LoadMeta(profileIndex, out outcome);

            // 읽지 못했으면 이름을 바꾸지 않는다. 새로 만든 메타로 덮으면 그 프로필의 기록을 잃는다.
            if (meta == null)
            {
                Report(outcome);
                return;
            }

            meta.ProfileName = name ?? string.Empty;

            if (meta == _meta)
            {
                SaveCurrentMeta();
                return;
            }

            Report(_repository.SaveMeta(meta));
        }

        // ---- 타이틀 화면 ----

        /// <summary>
        /// 저장된 런이 어떤 상태인지.
        /// 타이틀 화면의 이어하기가 켜지는지와 새 게임이 확인 팝업을 거치는지가 여기서 갈린다.
        /// </summary>
        public SavedRunState GetSavedRunState()
        {
            if (_profileIndex < 0 || _meta == null)
            {
                return SavedRunState.None;
            }

            if (_run != null)
            {
                return SavedRunState.Ready;
            }

            // 지금 읽지 못하는 것은 손상과 따로 둔다. 손상 알림을 띄우지 않고 이어하기만 끈다.
            // 메타를 못 읽은 것도 같다. 이어하기에서 메타를 다시 읽다 막히면 런이 멀쩡해도 런을 놓으므로,
            // 메타를 보지 않으면 멀쩡한 런이 손상으로 나와 새 게임으로 지워질 수 있었다. 2026년 10월 9일 전체 검토.
            if (!CanReadProfile(_profileIndex))
            {
                return SavedRunState.Unreadable;
            }

            // 메타는 런이 있다는데 읽지 못했으면 손상이다.
            // 파일 자체가 없으면 저장된 런이 없는 것이다.
            if (_meta.HasSavedRun || _repository.HasRunFile(_profileIndex))
            {
                return SavedRunState.Broken;
            }

            return SavedRunState.None;
        }

        // ---- 08 저장 시점 ----

        /// <summary>
        /// 런 시작. 런 데이터를 새로 쓴다.
        /// 런 번호는 메타의 런 횟수에서 이어 매긴다.
        /// </summary>
        public RunSaveData StartRun(int seed)
        {
            return StartRun(seed, null);
        }

        /// <summary>
        /// 런 시작. 런 데이터를 새로 만들고 `prepare` 로 채운 뒤에 한 번 쓴다.
        ///
        /// 흐름이 시작 아이템과 유물 순서를 `prepare` 에서 넣는다.
        /// 예전에는 빈 런을 먼저 쓰고 시작 아이템을 준 뒤 한 번 더 썼다.
        /// 그 사이에 꺼지면 시작 아이템이 없는 런이 남아 이어하기로 그 런을 했다.
        ///
        /// 런 횟수는 여기서 센다. 오른 값이 이 런의 번호다(`MetaSaveData.RunCount`).
        /// </summary>
        public RunSaveData StartRun(int seed, Action<RunSaveData> prepare)
        {
            if (_meta == null)
            {
                return null;
            }

            _meta.RunCount++;
            _run = RunSaveData.StartNew(_meta.RunCount, seed, _config.StartingMaxHealth);

            if (prepare != null)
            {
                prepare(_run);
            }

            _meta.HasSavedRun = true;
            MarkPlayedToday();

            // **메타를 먼저 쓰고 런을 쓴다.** 그 사이에 꺼지면 "런 있음" 인데 런 파일이 없는 모양이 남는다.
            // 이것은 프로필을 고를 때 메타의 적힌 값만 내리면 된다. 거꾸로면 "런 없음" 인데 런 파일이 남아 그 런을 지운다.
            // 메타 쓰기가 실패하면 런을 저장할 때마다 다시 쓴다(`WriteRun`). 2026년 10월 9일 외부 검토, 원재가 고치라고 했다.
            SaveCurrentMeta();
            WriteRunFile();
            Notify(AutoSavePoint.RunStarted);

            return _run;
        }

        /// <summary>
        /// 런 데이터를 쓴다. 런을 쓰는 곳은 모두 이것을 지난다.
        /// 마지막으로 논 날짜가 오늘이 아니거나 앞서 메타를 쓰지 못했으면 메타를 먼저 쓴다. 그 밖에는 메타를 쓰지 않는다.
        /// </summary>
        private void WriteRun()
        {
            if (_run == null)
            {
                return;
            }

            if (MarkPlayedToday() || _metaPending)
            {
                SaveCurrentMeta();
            }

            WriteRunFile();
        }

        /// <summary>
        /// 지금 프로필의 메타를 쓴다. 실패하면 `_metaPending` 을 켜 두고, 다음에 런을 저장하거나 프로필을 다시 읽을 때 다시 쓴다.
        ///
        /// **메타는 런보다 중요하다.** 런 시작에 메타 쓰기가 한 번 실패하면 파일의 메타는 "런 없음" 으로 남는다.
        /// 예전에는 다시 쓰지 않아, 다음에 켤 때 메타와 런을 맞추는 단계가 그날 한 런을 통째로 지웠다.
        /// 2026년 10월 9일 외부 검토가 짚었고 원재가 "메타 손상은 작지 않은 문제" 라며 고치라고 했다.
        /// </summary>
        private void SaveCurrentMeta()
        {
            if (_meta == null)
            {
                return;
            }

            SaveOutcome outcome = _repository.SaveMeta(_meta);
            _metaPending = !SaveOutcomes.IsSuccess(outcome);
            Report(outcome);
        }

        /// <summary>런 파일만 쓴다. 쓰기 전에 `RunSaving` 을 알린다.</summary>
        private void WriteRunFile()
        {
            if (_run == null)
            {
                return;
            }

            if (RunSaving != null)
            {
                RunSaving(_run);
            }

            Report(_repository.SaveRun(_profileIndex, _run));
        }

        /// <summary>메모리의 메타에 마지막으로 논 날짜를 오늘로 적는다. 바뀌었으면 true 다. 쓰지는 않는다.</summary>
        private bool MarkPlayedToday()
        {
            string today = Today != null ? Today() : FormatToday();
            if (_meta == null || string.IsNullOrEmpty(today) || _meta.LastPlayedDate == today)
            {
                return false;
            }

            _meta.LastPlayedDate = today;
            return true;
        }

        /// <summary>맵에서 방을 골라 들어갔다. 런 데이터를 덮어쓴다.</summary>
        public void OnRoomEntered(int nodeId)
        {
            if (_run == null)
            {
                return;
            }

            _run.CurrentNodeId = nodeId;
            _run.CurrentRoomCleared = false;

            if (!_run.VisitedNodeIds.Contains(nodeId))
            {
                _run.VisitedNodeIds.Add(nodeId);
            }

            WriteRun();
            Notify(AutoSavePoint.RoomEntered);
        }

        /// <summary>
        /// 방을 깨고 보상 목록이 정해졌다. 런 데이터를 덮어쓴다.
        /// 보상을 고르기 전에 저장하므로 되돌아와도 보상을 두 번 받을 수 없다.
        ///
        /// 이미 깬 방이면 아무 일도 하지 않는다.
        /// 화면을 정리하는 길이 여럿이라 같은 방에서 두 번 불릴 수 있는데
        /// 그때마다 세면 지나온 방 수가 부풀어 오른다.
        /// </summary>
        public void OnRoomCleared()
        {
            if (_run == null || _run.CurrentRoomCleared)
            {
                return;
            }

            _run.CurrentRoomCleared = true;
            _run.Statistics.RoomsCleared++;

            WriteRun();
            Notify(AutoSavePoint.RoomCleared);
        }

        /// <summary>스테이지를 넘긴다. 지나온 방 목록을 비우고 다시 센다.</summary>
        public void OnStageAdvanced(int stageIndex)
        {
            if (_run == null)
            {
                return;
            }

            _run.StageIndex = stageIndex;
            _run.CurrentNodeId = -1;
            _run.CurrentRoomCleared = false;
            _run.VisitedNodeIds.Clear();

            WriteRun();
        }

        /// <summary>
        /// 런이 끝났다. 메타 데이터에 완료 기록을 쓰고 런 데이터를 지운다.
        /// 끝난 런은 결과 화면이 쓸 수 있도록 그대로 돌려준다.
        ///
        /// **완료 기록을 쓴 뒤에 런을 지운다.** 기록 쓰기가 실패하면 런을 지우지 않고 saved 를 false 로 돌려준다.
        /// 그때 메모리의 메타도 쓰기 전으로 되돌린다. 다시 끝내도 기록이 두 번 쌓이지 않게 하려는 것이다.
        /// 런 파일은 방에 들어갈 때의 것이 남아, 이어하면 그 방부터 다시 한다.
        ///
        /// 기록을 쓰고 런을 지우기 전에 꺼지면 메타는 "런 없음" 인데 런 파일이 남는다.
        /// 다음에 프로필을 고를 때 `ReconcileRunFlag` 가 그 파일을 지운다. 기록은 이미 한 번 쌓였으므로 두 번 쌓이지 않는다.
        ///
        /// 예전에는 런을 먼저 지우고 기록을 써, 기록 쓰기가 실패하면 진행도 기록도 모두 잃었다. 2026년 10월 8일에 고쳤다.
        /// </summary>
        public RunSaveData EndRun(bool cleared, string today, out bool saved)
        {
            saved = false;

            if (_run == null || _meta == null)
            {
                return null;
            }

            RunSaveData finished = _run;

            // 쓰기가 실패하면 되돌릴 수 있게 지금 메타를 떠 둔다. 기록 목록까지 함께 되돌려야 하므로 글로 뜬다.
            string metaBefore = JsonUtility.ToJson(_meta);

            _meta.AccumulateRun(finished, cleared, today, _config.RunRecordLimit);

            SaveOutcome outcome = _repository.SaveMeta(_meta);
            if (!SaveOutcomes.IsSuccess(outcome))
            {
                _meta = JsonUtility.FromJson<MetaSaveData>(metaBefore);
                Report(outcome);
                return finished;
            }

            _metaPending = false;
            _repository.DeleteRun(_profileIndex);
            _run = null;
            saved = true;
            Notify(AutoSavePoint.RunEnded);

            return finished;
        }

        /// <summary>런 데이터만 버린다. 손상된 런을 지우거나 새 게임으로 갈아탈 때 쓴다.</summary>
        public void DiscardRun()
        {
            // 버리는 런의 번호는 이미 쓴 것이다. 다음 런이 그 번호를 다시 받지 않게 센다.
            bool counted = CountRun(_run);

            _repository.DeleteRun(_profileIndex);
            _run = null;

            if (_meta != null && (_meta.HasSavedRun || counted || _metaPending))
            {
                _meta.HasSavedRun = false;
                SaveCurrentMeta();
            }
        }

        /// <summary>
        /// 메타의 런 횟수가 그 런 번호보다 작으면 그 번호까지 올린다. 올렸으면 true 다. 쓰지는 않는다.
        /// 런 횟수를 시작할 때 세기 전에 시작한 런이 그렇다(`MetaSaveData.RunCount`).
        /// </summary>
        private bool CountRun(RunSaveData run)
        {
            if (_meta == null || run == null || _meta.RunCount >= run.RunNumber)
            {
                return false;
            }

            _meta.RunCount = run.RunNumber;
            return true;
        }

        /// <summary>런 진행 상황을 그대로 다시 쓴다. 체력이나 골드가 바뀌었을 때 쓴다.</summary>
        public void FlushRun()
        {
            if (_run != null)
            {
                WriteRun();
            }
        }

        // ---- 설정 데이터 ----

        /// <summary>설정 파일을 읽지 못해 지금 값이 기본값에서 시작한 것인지. 이때는 파일을 덮어쓰지 않는다.</summary>
        public bool IsSettingsUnread
        {
            get { return _settingsUnread; }
        }

        /// <summary>설정 창에서 값을 바꿨다. 그 값을 바로 쓴다.</summary>
        public void OnSettingChanged(SettingsValues values)
        {
            OnSettingChanged(values, null);
        }

        /// <summary>
        /// 설정 창에서 changedId 항목을 바꿨다. 그 값을 바로 쓴다. 설정 값을 다시 읽어 바뀌었으면 true 다.
        ///
        /// **설정 파일을 읽지 못한 채 시작했으면 덮어쓰지 않는다.** 지금 값은 기본값에서 시작했으므로
        /// 그대로 쓰면 바꾸지 않은 항목까지 기본값으로 덮여 기존 설정이 사라진다. 2026년 10월 9일 외부 검토.
        /// 대신 바꾼 항목을 적어 두고 파일을 다시 읽어 본다. 읽히면 파일의 값 위에 바꾼 항목만 얹어 쓴다
        /// (`TryRecoverSettings`). 아직 못 읽으면 이번 변경은 이번 실행에서만 쓰고 한 번만 알린다.
        /// </summary>
        public bool OnSettingChanged(SettingsValues values, string changedId)
        {
            if (_settings == null)
            {
                _settings = SettingsSaveData.CreateDefault();
            }

            if (values != null)
            {
                _settings.Values = values;
            }

            if (_settingsUnread)
            {
                if (!string.IsNullOrEmpty(changedId))
                {
                    _touchedSettings.Add(changedId);
                }

                if (!TryRecoverSettings())
                {
                    if (!_settingsDeferNoticeShown)
                    {
                        _settingsDeferNoticeShown = true;
                        Report(SaveOutcome.ReadFailed);
                    }

                    return false;
                }

                Notify(AutoSavePoint.SettingChanged);
                return true;
            }

            Report(_repository.SaveSettings(_settings));
            Notify(AutoSavePoint.SettingChanged);
            return false;
        }

        /// <summary>
        /// 설정 파일을 읽지 못한 채 시작했으면 다시 읽어 본다. 읽혔으면 true 다.
        /// 읽힌 값 위에 그동안 바꾼 항목만 얹고, 바꾼 것이 있으면 파일에 쓴다.
        /// 설정 화면이 같은 값 묶음을 들고 있으므로 묶음을 갈아 끼우지 않고 안의 값만 바꾼다.
        /// 파일이 그새 없어졌거나 망가졌으면 기본값 위에 얹는다.
        /// </summary>
        public bool TryRecoverSettings()
        {
            if (!_settingsUnread || _repository == null)
            {
                return false;
            }

            SettingsSaveData disk = _repository.LoadSettings(out SaveOutcome outcome);
            if (outcome == SaveOutcome.ReadFailed)
            {
                return false;
            }

            if (_settings == null)
            {
                _settings = SettingsSaveData.CreateDefault();
            }

            if (_settings.Values == null)
            {
                _settings.Values = new SettingsValues();
            }

            foreach (string id in _touchedSettings)
            {
                disk.Values.SetRaw(id, _settings.Values.GetRaw(id, 0f));
            }

            _settings.Values.Entries.Clear();
            _settings.Values.Entries.AddRange(disk.Values.Entries);

            bool changed = _touchedSettings.Count > 0;
            _settingsUnread = false;
            _settingsDeferNoticeShown = false;
            _touchedSettings.Clear();

            if (changed)
            {
                Report(_repository.SaveSettings(_settings));
            }

            return true;
        }

        /// <summary>설정 창의 저장 버튼. 바로 적용할 수 없는 그래픽 값을 쓴다.</summary>
        public void OnSettingsApplied(SettingsValues values)
        {
            OnSettingChanged(values);
            Notify(AutoSavePoint.SettingsApplied);
        }

        // ---- 안쪽 ----

        private void Notify(AutoSavePoint point)
        {
            if (Saved != null)
            {
                Saved(point);
            }
        }

        private void Report(SaveOutcome outcome)
        {
            if (!SaveOutcomes.NeedsNotice(outcome))
            {
                return;
            }

            if (NoticeRequested != null)
            {
                NoticeRequested(outcome, SaveOutcomes.GetNoticeText(outcome));
            }
        }
    }
}
