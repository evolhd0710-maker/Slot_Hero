using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SlotHero.Ui;
using SlotHero.CurrentBuild;
using SlotHero.CurrentBuild.UI;
using SlotHero.Events;
using SlotHero.Events.UI;
using SlotHero.Flow;
using SlotHero.Hud;
using SlotHero.Hud.UI;
using SlotHero.Map;
using SlotHero.Map.UI;
using SlotHero.Popup;
using SlotHero.Popup.UI;
using SlotHero.Profile;
using SlotHero.Profile.UI;
using SlotHero.Reward;
using SlotHero.Reward.UI;
using SlotHero.Sanctum;
using SlotHero.Sanctum.UI;
using SlotHero.Save;
using SlotHero.Settings;
using SlotHero.Settings.UI;
using SlotHero.Title.UI;
using SlotHero.TopBar;
using SlotHero.TopBar.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 게임 씬이 한 바퀴 도는지 본다.
    /// 메뉴 Slot Hero / 게임 씬 굴려 보기 에서 실행한다.
    ///
    /// 플레이 모드로 들어가지 않고 `Awake` 와 `OnEnable` 과 `Start` 를 직접 불러
    /// 플레이와 같은 상태를 만든 뒤 흐름을 손으로 밀어 본다.
    /// 프로필 고르기, 런 시작, 방 진입, 방 완료, 런 종료까지 이어지는지와
    /// 저장 파일이 제대로 생기고 지워지는지를 함께 본다.
    ///
    /// 저장은 메모리에만 한다. 진짜 저장 파일을 건드리지 않는다.
    /// </summary>
    public static class CheckGameScene
    {
        private static int _failed;

        /// 이미 Awake 를 불러 준 것들. 편집 모드에서는 Instantiate 가 Awake 를 부르지 않아
        /// 새로 찍어 낸 것이 생길 때마다 손으로 불러 줘야 한다.
        private static HashSet<MonoBehaviour> _awakened = new HashSet<MonoBehaviour>();
        private static int _checked;

        [MenuItem("Slot Hero/게임 씬 굴려 보기", priority = 2)]
        public static void Check()
        {
            _failed = 0;
            _checked = 0;

            string path = UiSceneBuilder.SceneFolder + "/게임.unity";

            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("게임 씬이 없다. 먼저 만들어야 한다: " + path);
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            GameFlowController flow = Object.FindAnyObjectByType<GameFlowController>();
            SaveService save = Object.FindAnyObjectByType<SaveService>();

            Report("흐름 조종기가 있다", flow != null);
            Report("저장 창구가 있다", save != null);

            if (flow == null || save == null)
            {
                Done();
                return;
            }

            CheckInputHandling();
            CheckNoDemoItems();
            CheckBackdrops();

            // 게임 씬이 모든 화면을 끈 상태에서 구조를 본다.
            // 켜도 안 보이는 칸과 따로 노는 어두운 막을 찾는다.
            List<string> structure = AuditGameScene.Find();
            for (int i = 0; i < structure.Count; i++)
            {
                Debug.LogWarning("  구조  " + structure[i]);
            }

            Report("켜도 안 보이는 칸이 없다", structure.Count, 0);

            // 플레이 모드와 같은 상태로 만든다. 저장은 메모리 창구를 쓴다.
            _awakened = new HashSet<MonoBehaviour>();
            AwakenNew();

            MemorySaveStorage storage = new MemorySaveStorage();
            save.Initialize(storage);

            CallOn("OnEnable", new HashSet<MonoBehaviour>());

            // Start 는 타이틀을 연다.
            // 저장 쪽이 첫 프로필을 미리 만들어 두므로 처음 켜도 프로필 선택을 거치지 않는다.
            // 이게 프로필 선택으로 열리면 플레이어가 켤 때마다 프로필부터 고르게 된다.
            Invoke(flow, "Start");
            Report("게임을 켜면 타이틀이 열린다", flow.Screen == GameScreen.Title, (int)flow.Screen);
            Report("첫 프로필이 알아서 골라진다", save.ProfileIndex == 0, save.ProfileIndex);
            Report("아직 저장된 런이 없다", save.GetSavedRunState() == SlotHero.Title.SavedRunState.None);
            CheckTitleArt();
            CapturePreviewScenes.CaptureOpenScene("게임 2 타이틀");

            // 타이틀 오른쪽 위의 현재 프로필 버튼으로 프로필 선택 화면에 간다.
            // 이 길이 막히면 프로필을 바꿀 방법이 아예 없다.
            CheckCurrentProfileButton(flow, save);

            // 화면을 열면서 새로 찍어 낸 카드는 아직 Awake 를 받지 못했다.
            // 편집 모드에서는 Instantiate 가 Awake 를 부르지 않기 때문이다.
            // 이것을 빠뜨리면 카드가 입력칸에 귀를 대지 못해 이름을 쳐도 아무 일이 없다.
            AwakenNew();

            // 프로필 목록이 만들어진다.
            Report("프로필 칸이 셋", save.BuildProfileList().Count == 3, save.BuildProfileList().Count);

            CheckFirstProfilePrefilled(save, storage);
            CapturePreviewScenes.CaptureOpenScene("게임 1 프로필 선택");

            // 빈 자리를 눌러 이름을 치고 프로필을 만든다.
            // 이 길이 막히면 실제 빌드에서 게임을 시작할 수가 없다.
            CheckProfileCreation(save);

            // 삭제 버튼을 실제로 눌러 본다. 확인 팝업을 거쳐 자리가 비는지 본다.
            CheckProfileDelete(flow, save, storage);

            // 프로필을 고르면 타이틀로 간다.
            Invoke(flow, "HandleProfileChosen", 0);
            Report("프로필을 고르면 타이틀이 열린다", flow.Screen == GameScreen.Title);
            Report("고른 프로필이 0번", save.ProfileIndex == 0, save.ProfileIndex);

            // 런을 시작한다. 08 저장 시점 의 "런 시작"이다.
            // 처음 쓰는 런에 시작 아이템과 유물 순서가 이미 들어 있는지 본다.
            // 예전에는 빈 런을 먼저 쓰고 한 번 더 써서, 그 사이에 꺼지면 시작 아이템이 없는 런이 남았다.
            int runWrites = 0;
            int firstWriteSymbols = -1;
            int firstWriteRelicOrder = -1;
            System.Action<RunSaveData> watchWrite = written =>
            {
                runWrites++;
                if (runWrites == 1)
                {
                    firstWriteSymbols = written.Owned.TotalSymbolCount;
                    firstWriteRelicOrder = written.RelicPool != null ? written.RelicPool.Order.Count : 0;
                }
            };

            save.RunSaving += watchWrite;
            flow.StartNewRun();
            save.RunSaving -= watchWrite;

            Report("런 시작은 런 데이터를 한 번 쓴다", runWrites, 1);
            Report("처음 쓴 런에 시작 덱이 들어 있다", firstWriteSymbols > 0, firstWriteSymbols);
            Report("처음 쓴 런에 유물 순서가 들어 있다", firstWriteRelicOrder > 0, firstWriteRelicOrder);

            Report("런을 시작하면 맵이 열린다", flow.Screen == GameScreen.Map);
            Report("런 자료가 생겼다", save.Run != null);
            Report("런 파일이 쓰였다", storage.Exists("profile0_run.json"));
            Report("메타가 런이 있다고 적는다", save.Meta.HasSavedRun);
            Report("이제 이어하기를 누를 수 있다",
                save.GetSavedRunState() == SlotHero.Title.SavedRunState.Ready);

            if (save.Run == null)
            {
                Done();
                return;
            }

            // 시작 덱은 **모든 문양 한 장씩**이다. 2026년 10월 2일에 그렇게 정했다.
            RunCatalogConfig catalog = GetField<RunCatalogConfig>(flow, "_catalogConfig");
            int symbolKinds = catalog != null ? catalog.Symbols.Count : 0;

            Report("문양이 서른여섯 종 있다", symbolKinds, 36);
            Report("시작 덱이 모든 문양 한 장씩",
                save.Run.Owned.TotalSymbolCount == symbolKinds, save.Run.Owned.TotalSymbolCount);
            Report("시작 덱에 같은 문양이 겹치지 않는다",
                save.Run.Owned.Symbols.Count == symbolKinds, save.Run.Owned.Symbols.Count);
            // 성소 기획서 06 재화 의 "시작 수치 0". 2026년 10월 8일에 100 에서 고쳤다.
            Report("시작 골드 0", save.Run.Status.Gold == 0, save.Run.Status.Gold);
            Report("체력이 가득", save.Run.Status.Health == save.Run.Status.MaxHealth,
                save.Run.Status.Health);

            // 맵이 만들어졌는지.
            MapScreenController map = Object.FindAnyObjectByType<MapScreenController>();
            Report("맵이 만들어졌다", map != null && map.Map != null);

            if (map == null || map.Map == null)
            {
                Done();
                return;
            }

            Report("노드가 서른 개 이상", map.Map.Nodes.Count >= 30, map.Map.Nodes.Count);
            CheckMapFitsScreen();
            CheckParchment(map);
            CheckCurrentBuildButton();
            CheckTooltip();
            CheckEveryButtonShowsHover();
            CapturePreviewScenes.CaptureOpenScene("게임 3 맵");

            CheckPauseAndSettings(flow, save);

            // 방에 들어가 본다. 성소와 이벤트와 전투를 각각 한 번씩 본다.
            EnterOneOf(flow, save, storage, map, RoomType.Sanctum, GameScreen.Sanctum, "성소");
            EnterOneOf(flow, save, storage, map, RoomType.Event, GameScreen.Event, "이벤트");
            EnterOneOf(flow, save, storage, map, RoomType.Normal, GameScreen.Combat, "전투");

            // 버튼 수가 다른 팝업을 번갈아 띄워 본다.
            // 작은 팝업이 끼면 그 뒤의 큰 팝업에서 버튼이 사라지던 문제를 본다.
            CheckPopupButtonCountChanges(flow);

            // 상단 표시줄의 런 종료 버튼을 실제로 눌러 본다.
            // 버튼에서 팝업을 거쳐 런이 끝나기까지가 이어지는지 본다.
            CheckEndRunButton(flow, save);

            // 런을 끝낸다. 08 저장 시점 의 "런 종료"다.
            // 흐름을 직접 부르지 않고 **상단 표시줄 버튼을 눌러 런 포기를 고른다.**
            // 플레이어가 실제로 지나가는 길이 이것이다.
            int runsBefore = save.Meta.Lifetime.RunsPlayed;
            PressEndRun("abandon");

            Report("런을 끝내면 결과 화면이 열린다", flow.Screen == GameScreen.RunResult);
            Report("런 파일이 지워졌다", !storage.Exists("profile0_run.json"));
            Report("메타가 런이 없다고 적는다", !save.Meta.HasSavedRun);
            Report("누적 런이 하나 올랐다",
                save.Meta.Lifetime.RunsPlayed == runsBefore + 1, save.Meta.Lifetime.RunsPlayed);
            Report("메타 파일이 남아 있다", storage.Exists("profile0_meta.json"));
            Report("메타 백업도 남아 있다", storage.Exists("profile0_meta.backup.json"));
            CapturePreviewScenes.CaptureOpenScene("게임 5 런 종료 결과");

            // 결과 화면에서 타이틀로 돌아간다.
            flow.GoToTitle();
            Report("타이틀로 돌아간다", flow.Screen == GameScreen.Title);
            Report("저장된 런이 없어졌다",
                save.GetSavedRunState() == SlotHero.Title.SavedRunState.None);

            // 이어하기. 런을 새로 만들고 저장된 값으로 되살린다.
            flow.StartNewRun();
            int seed = save.Run.Seed;
            save.Run.Status.Gold = 250;
            save.FlushRun();

            save.SelectProfile(0);
            Report("다시 읽은 런의 시드가 같다", save.Run != null && save.Run.Seed == seed);
            Report("다시 읽은 골드가 같다", save.Run != null && save.Run.Status.Gold == 250,
                save.Run != null ? save.Run.Status.Gold : -1);

            flow.ContinueRun();
            Report("이어하면 맵이 열린다", flow.Screen == GameScreen.Map);
            Report("이어한 맵의 시드가 같다", map.Map != null && map.Map.Seed == seed);

            CheckContinueInsideRoom(flow, save, map);
            CheckContinueRepeatsRoom(flow, save, map);
            CheckContinueAtReward(flow, save, map);
            CheckChallengeEvent(flow, save, map);
            CheckRewardWhenFull(flow, save);
            CheckOverflowDiscard(flow, save);

            // 런을 끝내므로 맨 뒤에 둔다.
            CheckRewardByRoom(flow, save, map);

            // 런 파일을 망가뜨리므로 런을 다 쓴 뒤에 본다.
            CheckBrokenRunNotice(flow, save, storage);
            CheckReadRetry(flow, save, storage);

            // 여기까지 오면서 화면을 거의 다 한 번씩 열어 자리를 잡았다.
            // 이제 펼친 칸에 크기가 들어간 곳을 찾는다. 맵이 이래서 두 배가 됐었다.
            List<string> stretched = AuditGameScene.FindStretchedWithSize();
            for (int i = 0; i < stretched.Count; i++)
            {
                Debug.LogWarning("  펼침  " + stretched[i]);
            }

            Report("펼친 칸에 크기가 들어간 곳이 없다", stretched.Count, 0);

            List<string> unsized = AuditGameScene.FindUnsizedLabels();
            for (int i = 0; i < unsized.Count; i++)
            {
                Debug.LogWarning("  글자  " + unsized[i]);
            }

            Report("크기를 안 잡은 글자 칸이 없다", unsized.Count, 0);

            Done();
        }

        /// <summary>
        /// 설정을 열면 런 시간이 멈추고 닫으면 다시 흐르는지 본다.
        ///
        /// 편집 모드에서는 `Update` 가 돌지 않아 ESC 키 자체는 눌러 볼 수 없다.
        /// 대신 `Update` 안의 두 갈래를 직접 불러 같은 길을 밟는다.
        /// </summary>
        private static void CheckPauseAndSettings(GameFlowController flow, SaveService save)
        {
            RunClock clock = GetField<RunClock>(flow, "_clock");
            Report("런 시계가 있다", clock != null);

            if (clock == null)
            {
                return;
            }

            Invoke(flow, "TickClock");
            Report("맵에서는 시간이 흐른다", !clock.IsPaused);

            // 런 시간을 재는 곳은 흐름 하나다. 상단 표시줄은 받은 값을 보여 주기만 한다.
            // 예전에는 표시줄에도 시계가 있어 둘이 따로 흘렀다. 2026년 10월 8일에 하나로 모았다.
            TopBarController topBar = GetField<TopBarController>(flow, "_topBar");
            if (topBar != null)
            {
                topBar.SetElapsedSeconds(12.25f);
                for (int i = 0; i < 5; i++)
                {
                    Invoke(topBar, "Update");
                }

                Report("상단 표시줄은 스스로 시간을 더하지 않는다", Mathf.Approximately(topBar.ElapsedSeconds, 12.25f),
                    Mathf.RoundToInt(topBar.ElapsedSeconds * 100f));

                Invoke(flow, "TickClock");
                Report("표시줄은 흐름의 시계 값을 받는다",
                    Mathf.Approximately(topBar.ElapsedSeconds, clock.Seconds), Mathf.RoundToInt(topBar.ElapsedSeconds));
            }

            // ESC 를 누른 셈 친다.
            flow.OpenSettings();
            Report("설정이 열린다", flow.Screen == GameScreen.Map);

            SettingsScreenController settings =
                Object.FindAnyObjectByType<SettingsScreenController>();
            Report("설정 화면이 켜졌다", settings != null && settings.IsOpen);

            if (settings != null)
            {
                CheckSettingsScreen(settings);
            }

            Invoke(flow, "TickClock");
            Report("설정이 열려 있으면 시간이 멈춘다", clock.IsPaused);

            float before = clock.Seconds;
            Invoke(flow, "TickClock");
            Report("멈춘 동안에는 시간이 늘지 않는다", clock.Seconds == before, (int)clock.Seconds);

            // 겹쳐 뜬 것이 있으면 ESC 가 설정을 또 열지 않는다.
            Report("겹쳐 뜬 것이 있으면 ESC 를 가져가지 않는다",
                (bool)InvokeResult(flow, "IsOverlayOpen"));

            if (settings != null)
            {
                settings.Close();
                Invoke(flow, "HandleSettingsClosed");
            }

            Invoke(flow, "TickClock");
            Report("설정을 닫으면 다시 흐른다", !clock.IsPaused);

            // 팝업이 떠도 시간은 흐른다. 설정 화면만 멈춘다. 2026년 10월 6일 원재가 정했다.
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            if (popup != null)
            {
                popup.Show(PopupSpec.Notice("시계 검사", string.Empty, "확인"));
                Invoke(flow, "TickClock");
                Report("팝업이 떠 있어도 시간이 흐른다", popup.IsOpen && !clock.IsPaused);
                PressPopupButton(popup, "confirm");
            }
        }

        /// <summary>
        /// 설정 화면의 줄과 값 고르기가 도는지 본다. 2026년 10월 5일 원재의 요청이다.
        ///   줄이 목록 칸 맨 위부터 붙는다. 예전에는 플레이 모드에서 아래로 밀려 붙었다
        ///   긴 줄은 마우스를 올려도 조금만 커진다
        ///   언어는 드롭다운으로 고른다
        ///   소리 크기는 0 ~ 100 막대로 고르고 오른쪽 음소거 버튼으로 끄고 켠다
        /// </summary>
        private static void CheckSettingsScreen(SettingsScreenController settings)
        {
            SettingsScreenView view = GetField<SettingsScreenView>(settings, "_view");
            SettingsLayoutConfig layout = view != null ? GetField<SettingsLayoutConfig>(view, "_layout") : null;
            SettingsCatalogConfig catalog = GetField<SettingsCatalogConfig>(settings, "_catalog");
            ScrollRect scroll = view != null ? GetField<ScrollRect>(view, "_listScroll") : null;
            RectTransform viewport = view != null ? GetField<RectTransform>(view, "_listViewport") : null;

            Report("설정 화면 부품이 다 있다",
                view != null && layout != null && catalog != null && scroll != null && viewport != null);

            if (view == null || layout == null || catalog == null || scroll == null || viewport == null)
            {
                return;
            }

            // 화면이 연 줄은 편집 모드에서 Awake 가 돌지 않았으므로 깨운다.
            AwakenNew();
            settings.SelectTab(0);

            // 플레이 모드처럼 스크롤 창이 한 번 돌게 한다. 예전에는 이때 줄이 아래로 밀렸다.
            Canvas.ForceUpdateCanvases();
            Invoke(scroll, "LateUpdate");

            List<SettingsRowView> rows = view.GetShownRows();
            Report("일반 탭 줄이 넷", rows.Count, 4);

            if (rows.Count < 4)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            rows[0].RectTransform.GetWorldCorners(corners);
            Vector3 firstTop = viewport.InverseTransformPoint(corners[1]);
            Report("첫 줄이 목록 칸 맨 위에 붙는다",
                Mathf.Abs(firstTop.x - layout.ListPanelPadding) < UiScale.Px(1f)
                && Mathf.Abs(firstTop.y + layout.ListPanelTopPadding) < UiScale.Px(1f),
                Mathf.RoundToInt(-firstTop.y / UiScale.Factor));

            rows[3].RectTransform.GetWorldCorners(corners);
            Vector3 lastBottom = viewport.InverseTransformPoint(corners[0]);
            Report("마지막 줄이 목록 칸 안에 있다", lastBottom.y > -layout.ListPanelSize.y,
                Mathf.RoundToInt(-lastBottom.y / UiScale.Factor));

            // 긴 줄은 덜 커진다.
            HoverScale hover = rows[0].GetComponent<HoverScale>();
            Report("줄에 호버 확대가 있다", hover != null);
            if (hover != null)
            {
                float width = rows[0].RectTransform.rect.width;
                hover.OnPointerEnter(null);
                float grown = rows[0].transform.localScale.x * width - width;
                hover.OnPointerExit(null);
                Report("줄에 마우스를 올려도 24 픽셀까지만 커진다",
                    grown > 0f && grown <= HoverScale.DefaultMaxGrow + 0.5f, Mathf.RoundToInt(grown / UiScale.Factor));
                Report("마우스가 벗어나면 제 크기", Mathf.Abs(rows[0].transform.localScale.x - 1f) < 0.0001f);
            }

            CheckSettingsDropdown(settings, view, rows);
            CheckSettingsChoice(settings, rows);
            CheckSettingsSound(settings, view, catalog);

            CheckSettingsRestoreButton(settings, view);
        }

        /// <summary>
        /// 기본값 복원 버튼을 **진짜로 눌러** 본다.
        ///
        /// 예전 검사는 `RestoreDefaults` 를 직접 불러 통과했는데, 게임에서는 버튼의 알림을 받는 곳이 없어
        /// 눌러도 아무 일이 없었다. 2026년 10월 8일에 찾아 고쳤다.
        /// 확인 팝업을 거치고, 창 모드는 그대로 두고 나머지만 되돌리는지 본다.
        /// </summary>
        private static void CheckSettingsRestoreButton(SettingsScreenController settings, SettingsScreenView view)
        {
            GameFlowController flow = Object.FindAnyObjectByType<GameFlowController>();
            PopupPresenter popup = flow != null ? GetField<PopupPresenter>(flow, "_popup") : null;
            SaveService save = flow != null ? GetField<SaveService>(flow, "_save") : null;
            Button restore = GetField<Button>(view, "_restoreButton");

            Report("기본값 복원 검사에 필요한 것이 다 있다",
                flow != null && popup != null && save != null && restore != null);

            if (flow == null || popup == null || save == null || restore == null)
            {
                return;
            }

            // 앞 검사가 바꿔 둔 값. 창 모드는 창, 연출 속도는 스킵, 언어는 English 다.
            settings.SelectTab(0);
            float screenBefore = settings.Values.GetRaw("screenMode", -1f);
            Report("되돌리기 전 창 모드가 기본값이 아니다", screenBefore == 0f, (int)screenBefore);

            restore.onClick.Invoke();
            AwakenNew();

            Report("기본값 복원을 누르면 확인 팝업이 뜬다", popup.IsOpen);
            Report("확인 팝업 제목", popup.Current != null && popup.Current.Title == "기본값으로 되돌리시겠습니까?");
            Report("확인 팝업에 그대로 두는 언어와 창 모드가 적힌다",
                popup.Current != null && popup.Current.Body.Contains("언어, 창 모드"));

            Invoke(flow, "RefreshSettingsInput");
            Report("팝업이 떠 있는 동안 설정은 키를 받지 않는다", settings.InputBlocked);

            PressPopupButton(popup, "cancel");
            Report("취소를 누르면 아무것도 안 바뀐다",
                settings.Values.GetRaw("presentationSpeed", -1f) == 2f && settings.Values.GetRaw("language", -1f) == 1f);
            Report("취소 뒤에도 설정 화면은 열려 있다", settings.IsOpen);

            Invoke(flow, "RefreshSettingsInput");
            Report("팝업이 닫히면 설정이 다시 키를 받는다", !settings.InputBlocked);

            restore.onClick.Invoke();
            AwakenNew();
            PressPopupButton(popup, "confirm");
            settings.SelectTab(0);

            Report("되돌리면 팝업이 닫힌다", !popup.IsOpen);
            Report("되돌려도 창 모드는 그대로", settings.Values.GetRaw("screenMode", -1f) == screenBefore,
                (int)settings.Values.GetRaw("screenMode", -1f));
            Report("되돌려도 언어는 고른 English 그대로", settings.Values.GetRaw("language", -1f) == 1f);
            Report("연출 속도는 보통으로 돌아간다", settings.Values.GetRaw("presentationSpeed", -1f) == 0f);
            Report("기본값 복원이 소리 크기도 되돌린다", settings.Values.GetRaw("bgmVolume", -1f) == 100f,
                (int)settings.Values.GetRaw("bgmVolume", -1f));
            Report("기본값 복원이 음소거도 끈다", !settings.Values.GetBool("sfxVolumeMute", true));

            // 되돌린 값이 기기 설정 파일에도 들어갔는지. 언어와 창 모드는 고른 값 그대로 저장되어 있어야 한다.
            SettingsValues saved = save.Settings != null ? save.Settings.Values : null;
            Report("되돌린 값이 저장된다",
                saved != null && saved.GetRaw("presentationSpeed", -1f) == 0f && saved.GetRaw("bgmVolume", -1f) == 100f);
            Report("저장된 언어와 창 모드도 그대로",
                saved != null && saved.GetRaw("language", -1f) == 1f && saved.GetRaw("screenMode", -1f) == screenBefore);
        }

        /// <summary>언어 줄을 누르면 목록이 펼쳐지고 고른 값이 들어가는지 본다.</summary>
        private static void CheckSettingsDropdown(
            SettingsScreenController settings, SettingsScreenView view, List<SettingsRowView> rows)
        {
            SettingsRowView language = FindRow(rows, "language");
            Report("언어 줄이 있다", language != null);

            if (language == null)
            {
                return;
            }

            Button rowButton = language.GetComponent<Button>();
            Report("언어 줄에는 막대가 없다", language.Slider == null || !language.Slider.gameObject.activeSelf);
            Report("언어 값 뒤에 펼침 표시가 붙는다", language.ValueText.StartsWith("한국어") && language.ValueText.Length > 3);

            rowButton.onClick.Invoke();
            Report("언어를 누르면 드롭다운이 펼쳐진다", view.IsDropdownOpen);
            Report("누르기만 해서는 언어가 안 바뀐다", settings.Values.GetIndex("language", -1) == 0);

            // 펼치며 만든 선택지를 깨운다.
            AwakenNew();
            List<SettingsOptionView> options = view.GetOpenOptions();
            Report("선택지가 둘", options.Count, 2);

            if (options.Count < 2)
            {
                view.CloseDropdown();
                return;
            }

            Report("선택지 글이 한국어와 English", options[0].Text == "한국어" && options[1].Text == "English");
            Report("지금 고른 한국어만 칠해져 있다", options[0].IsSelected && !options[1].IsSelected);

            // 목록의 오른쪽 위가 줄의 오른쪽 아래에 붙는다.
            Vector3[] rowCorners = new Vector3[4];
            Vector3[] panelCorners = new Vector3[4];
            language.RectTransform.GetWorldCorners(rowCorners);
            ((RectTransform)options[0].transform.parent).GetWorldCorners(panelCorners);
            Report("드롭다운이 줄 오른쪽 아래에 붙는다",
                Vector3.Distance(rowCorners[3], panelCorners[2]) < 0.01f);

            UiSceneBuilder.SyncShadows();
            Canvas.ForceUpdateCanvases();
            CapturePreviewScenes.CaptureOpenScene("게임 설정 언어 펼침");

            options[1].Button.onClick.Invoke();
            Report("English 를 고르면 값이 바뀐다", settings.Values.GetIndex("language", -1) == 1);
            Report("고르면 드롭다운이 접힌다", !view.IsDropdownOpen);
            Report("줄에 English 가 적힌다", language.ValueText.StartsWith("English"));

            // 바깥을 누르면 값은 그대로 두고 접힌다.
            rowButton.onClick.Invoke();
            Button blocker = GetField<Button>(view, "_dropdownBlocker");
            Report("드롭다운 바깥 막이 있다", blocker != null);
            if (blocker != null)
            {
                blocker.onClick.Invoke();
            }

            Report("바깥을 누르면 접힌다", !view.IsDropdownOpen);
            Report("바깥을 눌러도 값은 그대로", settings.Values.GetIndex("language", -1) == 1);

            // 같은 줄을 다시 누르면 접힌다.
            rowButton.onClick.Invoke();
            rowButton.onClick.Invoke();
            Report("같은 줄을 다시 누르면 접힌다", !view.IsDropdownOpen);

            // 탭을 바꾸면 접힌다.
            rowButton.onClick.Invoke();
            settings.SelectTab(0);
            Report("탭을 다시 그리면 접힌다", !view.IsDropdownOpen);
        }

        /// <summary>창 모드와 연출 속도는 누를 때마다 다음 값으로 넘어간다.</summary>
        private static void CheckSettingsChoice(SettingsScreenController settings, List<SettingsRowView> rows)
        {
            SettingsRowView screenMode = FindRow(rows, "screenMode");
            SettingsRowView speed = FindRow(rows, "presentationSpeed");
            Report("창 모드와 연출 속도 줄이 있다", screenMode != null && speed != null);

            if (screenMode == null || speed == null)
            {
                return;
            }

            Report("창 모드는 전체 화면으로 시작", screenMode.ValueText == "전체 화면");
            screenMode.GetComponent<Button>().onClick.Invoke();
            Report("창 모드를 누르면 테두리 없는 창", screenMode.ValueText == "테두리 없는 창");
            screenMode.GetComponent<Button>().onClick.Invoke();
            Report("한 번 더 누르면 창", screenMode.ValueText == "창");

            Report("연출 속도는 보통으로 시작", speed.ValueText == "보통");
            speed.GetComponent<Button>().onClick.Invoke();
            Report("연출 속도를 누르면 빠름", speed.ValueText == "빠름");
            speed.GetComponent<Button>().onClick.Invoke();
            Report("한 번 더 누르면 스킵", speed.ValueText == "스킵");
        }

        /// <summary>사운드 탭의 막대와 음소거 버튼이 도는지 본다.</summary>
        private static void CheckSettingsSound(
            SettingsScreenController settings, SettingsScreenView view, SettingsCatalogConfig catalog)
        {
            SettingsTab sound = catalog.FindTab("sound");
            Report("사운드 탭이 있다", sound != null);

            if (sound == null)
            {
                return;
            }

            settings.SelectTab(catalog.Tabs.IndexOf(sound));
            AwakenNew();

            List<SettingsRowView> rows = view.GetShownRows();
            Report("사운드 탭 줄이 넷", rows.Count, 4);

            string[] names = { "전체", "배경음", "환경음", "효과음" };
            bool allSliders = rows.Count == 4;
            for (int i = 0; i < rows.Count && i < names.Length; i++)
            {
                SettingsRowView row = rows[i];
                allSliders &= row.Definition.DisplayName == names[i]
                              && row.Slider != null && row.Slider.gameObject.activeSelf
                              && row.MuteButton != null && row.MuteButton.gameObject.activeSelf
                              && row.Slider.minValue == 0f && row.Slider.maxValue == 100f
                              && row.Slider.wholeNumbers;
            }

            Report("네 줄 모두 0 ~ 100 막대와 음소거 버튼", allSliders);

            SettingsRowView bgm = FindRow(rows, "bgmVolume");
            if (bgm == null || bgm.Slider == null || bgm.MuteButton == null)
            {
                Report("배경음 줄이 있다", false);
                return;
            }

            // 줄 안의 순서. 이름, 막대, 숫자, 음소거 버튼이 왼쪽부터 늘어서고 줄 밖으로 나가지 않는다.
            Vector3[] rowBox = new Vector3[4];
            Vector3[] slider = new Vector3[4];
            Vector3[] mute = new Vector3[4];
            bgm.RectTransform.GetWorldCorners(rowBox);
            ((RectTransform)bgm.Slider.transform).GetWorldCorners(slider);
            ((RectTransform)bgm.MuteButton.transform).GetWorldCorners(mute);
            Report("막대가 음소거 버튼 왼쪽에 있다", slider[2].x < mute[0].x);
            Report("음소거 버튼이 줄 오른쪽 안에 있다", mute[2].x <= rowBox[2].x + 0.01f && mute[2].x > slider[2].x);
            Report("음소거 버튼이 줄 위아래 안에 있다", mute[0].y >= rowBox[0].y - 0.01f && mute[1].y <= rowBox[1].y + 0.01f);

            bgm.Slider.value = 40f;
            Report("막대를 옮기면 값이 바뀐다", settings.Values.GetRaw("bgmVolume", -1f) == 40f,
                (int)settings.Values.GetRaw("bgmVolume", -1f));
            Report("막대를 옮기면 숫자도 바뀐다", bgm.ValueText == "40");

            bgm.MuteButton.onClick.Invoke();
            Report("음소거를 누르면 켜진다", settings.Values.GetBool("bgmVolumeMute", false) && bgm.IsMuted);
            Report("음소거해도 크기는 그대로", settings.Values.GetRaw("bgmVolume", -1f) == 40f);

            bgm.MuteButton.onClick.Invoke();
            Report("다시 누르면 음소거가 꺼진다", !settings.Values.GetBool("bgmVolumeMute", true) && !bgm.IsMuted);

            // 화면을 남긴다. 효과음은 음소거한 모습으로 둔다.
            SettingsRowView sfx = FindRow(rows, "sfxVolume");
            if (sfx != null && sfx.MuteButton != null)
            {
                sfx.MuteButton.onClick.Invoke();
            }

            Canvas.ForceUpdateCanvases();
            CapturePreviewScenes.CaptureOpenScene("게임 설정 사운드");
        }

        private static SettingsRowView FindRow(List<SettingsRowView> rows, string id)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] != null && rows[i].Definition != null && rows[i].Definition.Id == id)
                {
                    return rows[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 빈 프로필 자리를 눌러 이름을 치고 만드는 길이 통하는지 본다.
        ///
        /// 카드에 이름 입력칸이 없으면 이름을 칠 수가 없고
        /// 빈 이름은 받아 주지 않으므로 프로필이 영영 안 생긴다.
        /// 게임을 시작할 길이 통째로 막히므로 따로 확인한다.
        /// </summary>
        private static void CheckProfileCreation(SaveService save)
        {
            ProfileCardView[] cards =
                Object.FindObjectsByType<ProfileCardView>(FindObjectsInactive.Include);

            Report("프로필 카드가 화면에 있다", cards.Length >= 3, cards.Length);

            // 화면이 실제로 쓰는 카드 가운데 **빈 자리**를 고른다.
            // 확인용 씬에서 넘어온 옛 카드는 아직 자료를 받지 않아 아무 반응이 없다.
            //
            // 채워진 자리를 잡으면 이름을 새로 만드는 대신 **고치는 길**로 가 버린다.
            // 그러면 만들기를 본 적이 없는데 검사가 통과하고,
            // 뒤에 오는 삭제 검사도 지울 프로필을 못 찾는다.
            ProfileList before = save.BuildProfileList();
            ProfileCardView empty = null;
            bool hasInput = false;

            for (int i = 0; i < cards.Length; i++)
            {
                if (GetField<TMP_InputField>(cards[i], "_nameInput") != null)
                {
                    hasInput = true;
                }

                if (empty != null
                    || !cards[i].gameObject.activeInHierarchy
                    || GetField<ProfileVisualConfig>(cards[i], "_visual") == null)
                {
                    continue;
                }

                if (before.Get(cards[i].Index).IsEmpty)
                {
                    empty = cards[i];
                }
            }

            Report("카드에 이름 입력칸이 있다", hasInput);
            Report("화면이 쓰는 빈 카드를 찾았다", empty != null);

            if (empty == null)
            {
                return;
            }

            // 빈 자리를 누르면 이름 고치기로 들어간다.
            empty.BeginEditName(string.Empty);
            TMP_InputField field = GetField<TMP_InputField>(empty, "_nameInput");

            Report("이름 칸이 켜진다", field.gameObject.activeSelf);

            // 이름을 치고 확인한다.
            field.text = "확인용";
            field.onSubmit.Invoke("확인용");

            ProfileList list = save.BuildProfileList();

            Report("친 이름으로 빈 자리에 프로필이 생긴다",
                list.Get(empty.Index).Name == "확인용", empty.Index);
            Report("앞에 있던 프로필은 그대로",
                list.Get(0).Name == before.Get(0).Name);
        }

        /// <summary>메서드를 부르고 돌려준 값을 받는다.</summary>
        private static object InvokeResult(object target, string methodName, params object[] args)
        {
            MethodInfo method = FindMethod(target, methodName, args);
            return method != null ? method.Invoke(target, args) : null;
        }

        /// <summary>
        /// 이름과 인자 수가 맞는 메서드를 찾는다.
        /// 이름만으로 찾으면 같은 이름의 메서드가 둘 이상일 때 반사가 어느 것인지 몰라 터진다.
        /// `GoToTitle()` 과 `GoToTitle(bool)` 처럼 겹치는 것이 생겨 인자 수로 가린다.
        /// </summary>
        private static MethodInfo FindMethod(object target, string methodName, object[] args)
        {
            int count = args != null ? args.Length : 0;
            MethodInfo[] methods = target.GetType().GetMethods(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == methodName && methods[i].GetParameters().Length == count)
                {
                    return methods[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 감춰진 필드를 꺼내 본다.
        ///
        /// **반사로 진짜 값을 읽는다.** `SerializedObject` 로 읽으면 안 된다.
        /// 화면이 돌면서 넣어 준 값은 파일에 저장되지 않아 그쪽으로는 늘 비어 보인다.
        /// </summary>
        private static T GetField<T>(object target, string fieldName) where T : class
        {
            FieldInfo field = target.GetType().GetField(
                fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            return field != null ? field.GetValue(target) as T : null;
        }

        /// <summary>그 종류의 방을 하나 찾아 들어갔다 나온다.</summary>
        private static void EnterOneOf(
            GameFlowController flow, SaveService save, MemorySaveStorage storage,
            MapScreenController map, RoomType roomType, GameScreen expected, string title)
        {
            MapNode node = null;

            for (int i = 0; i < map.Map.Nodes.Count; i++)
            {
                if (map.Map.Nodes[i].RoomType == roomType)
                {
                    node = map.Map.Nodes[i];
                    break;
                }
            }

            if (node == null)
            {
                Report(title + " 방이 맵에 있다", false);
                return;
            }

            int before = save.Run.Statistics.RoomsCleared;

            Invoke(flow, "EnterRoom", node);
            Report(title + " 방에 들어간다", flow.Screen == expected, (int)flow.Screen);

            // 방 화면 뒤로 맵이 비치면 안 된다. 예전에는 방 화면의 불투명한 바탕이 가리고 있었다.
            Report(title + " 방에 들어가면 맵이 닫힌다", !map.IsOpen);

            Report(title + " 방 진입이 저장된다",
                save.Run.CurrentNodeId == node.Id, save.Run.CurrentNodeId);
            Report(title + " 들른 방에 적힌다", save.Run.VisitedNodeIds.Contains(node.Id));

            // 들어간 방의 단계가 상단 표시줄에 적히는지.
            CheckTopBarStep(node, title);

            CapturePreviewScenes.CaptureOpenScene("게임 4 " + title + " 방");

            // 방 안에서 지도 버튼을 눌러 본다. 방이 끝나 버리면 안 된다.
            CheckMapButtonInRoom(flow, save, node, expected, title);

            // 성소에 들어가 있는 지금이 행상을 눌러 볼 수 있는 유일한 때다.
            if (expected == GameScreen.Sanctum)
            {
                CheckCamp();
                CheckMerchant();
            }

            // 같은 이벤트 방에 다시 들어가면 처음부터 시작해야 한다.
            if (expected == GameScreen.Event)
            {
                CheckEventStartsClean(flow, save, node);
            }

            // 전투는 팝업이 떠 기다린다.
            // 결과를 직접 넣지 않고 **팝업 버튼을 진짜로 눌러** 끝낸다.
            // 직접 넣으면 팝업이 열린 채로 남아 뒤에 이어지는 검사를 가린다.
            if (expected == GameScreen.Combat)
            {
                CheckCombatPopup();
            }
            else
            {
                Invoke(flow, "LeaveRoom");
            }

            // 보상이 있는 방은 나오면 보상 화면이 뜬다.
            // 지금 규칙으로는 전투 방만 보상을 준다.
            if (flow.Screen == GameScreen.Reward)
            {
                RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>();

                Report(title + " 방을 깨면 보상이 뜬다", reward != null && reward.Offer != null);

                // 보상은 반투명한 막 위에 뜬다. 뒤에 맵이나 방 화면이 열려 있으면 비쳐 보인다.
                Report(title + " 보상 뒤에 맵이 열려 있지 않다", !map.IsOpen);
                CapturePreviewScenes.CaptureOpenScene("게임 4 " + title + " 보상");

                // 보상 화면도 방 진행 중이다. 원재가 2026년 10월 5일에 정했다.
                // 지도를 띄웠다 돌아와도 보상을 놓치지 않아야 한다.
                TopBarButton rewardMapButton = FindTopBarButton(TopBarButtonKind.Map);
                if (rewardMapButton != null)
                {
                    CheckMapPeek(flow, save, map, rewardMapButton, title + " 보상");
                    Report(title + " 보상에서 지도를 보고 와도 보상이 남아 있다",
                        reward != null && reward.IsOpen && reward.Offer != null);
                }

                if (reward != null && reward.Offer != null)
                {
                    Report(title + " 보상에 골드가 있다",
                        reward.Offer.TotalGold > 0, reward.Offer.TotalGold);
                    Report(title + " 보상 카드가 나온다",
                        reward.Offer.Cards.Count > 0, reward.Offer.Cards.Count);

                    int ownedBefore = save.Run.Owned.TotalSymbolCount
                        + save.Run.Owned.Relics.Count + save.Run.Owned.CoinIds.Count;

                    CheckRewardSelection(reward, title);

                    int ownedAfter = save.Run.Owned.TotalSymbolCount
                        + save.Run.Owned.Relics.Count + save.Run.Owned.CoinIds.Count;

                    Report(title + " 고른 카드가 들어온다",
                        ownedAfter == ownedBefore + 1, ownedAfter - ownedBefore);
                }
            }

            Report(title + " 방을 나오면 맵으로 돌아간다", flow.Screen == GameScreen.Map);
            CheckMapButtonOnMap(flow, map);
            Report(title + " 방 완료가 세어진다",
                save.Run.Statistics.RoomsCleared == before + 1, save.Run.Statistics.RoomsCleared);
            Report(title + " 방 완료가 저장된다", save.Run.CurrentRoomCleared);
        }

        /// <summary>씬에 있는 우리 스크립트의 그 메서드를 직접 부른다.</summary>
        /// <summary>
        /// 새로 찍어 낸 것에 `Awake` 를 불러 준다.
        ///
        /// **편집 모드에서는 `Instantiate` 가 `Awake` 를 부르지 않는다.**
        /// 팝업 버튼처럼 화면이 돌면서 만들어지는 것은 이걸 빠뜨리면
        /// 버튼에 귀를 대지 못해 눌러도 아무 일이 없다.
        /// 플레이 모드에서는 유니티가 알아서 부르므로 이건 검사 쪽 사정이다.
        /// </summary>
        private static void AwakenNew()
        {
            // **꺼진 것까지 불러야 한다.**
            // 게임 씬은 화면을 다 꺼 둔 채로 시작하므로 꺼진 것을 빼면
            // 타이틀 안의 버튼처럼 아직 열리지 않은 화면의 것이 영영 깨어나지 않는다.
            // 플레이 모드에서는 유니티가 처음 켜질 때 Awake 를 부르니 그쪽과 결과가 같다.
            CallOn("Awake", _awakened, true);
        }

        private static void CallOn(string methodName, HashSet<MonoBehaviour> already)
        {
            CallOn(methodName, already, false);
        }

        /// <summary>
        /// 씬에 있는 우리 스크립트의 그 메서드를 한 번씩 부른다.
        ///
        /// `OnEnable` 은 꺼진 것에 부르지 않는다.
        /// 꺼진 화면이 바깥 사건에 귀를 대 버리면 플레이 모드와 달라진다.
        /// </summary>
        private static void CallOn(
            string methodName, HashSet<MonoBehaviour> already, bool includeInactive)
        {
            MonoBehaviour[] all = includeInactive
                ? Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                : Object.FindObjectsByType<MonoBehaviour>();

            for (int i = 0; i < all.Length; i++)
            {
                MonoBehaviour behaviour = all[i];
                if (behaviour == null || already.Contains(behaviour))
                {
                    continue;
                }

                already.Add(behaviour);

                System.Type type = behaviour.GetType();
                if (type.Namespace == null || !type.Namespace.StartsWith("SlotHero"))
                {
                    continue;
                }

                Invoke(behaviour, methodName);
            }
        }

        private static void Invoke(object target, string methodName, params object[] args)
        {
            if (target == null)
            {
                return;
            }

            MethodInfo method = FindMethod(target, methodName, args);

            if (method == null)
            {
                return;
            }

            method.Invoke(target, args);
        }

        /// <summary>
        /// 현재 빌드 버튼에 그림이 붙었고 기획서 자리에 있는지 본다.
        ///
        /// 그림을 안 물리면 빈 흰 네모가 그대로 나간다.
        /// `Hud/Art` 에 넉 장이 있었는데 `ImportArt` 가 그 폴더만 안 가져오고 있었다.
        /// </summary>
        private static void CheckCurrentBuildButton()
        {
            CurrentBuildButton button = Object.FindAnyObjectByType<CurrentBuildButton>(
                FindObjectsInactive.Include);

            Report("현재 빌드 버튼이 있다", button != null);

            if (button == null)
            {
                return;
            }

            Image background = GetField<Image>(button, "_background");
            Image icon = GetField<Image>(button, "_icon");

            Report("버튼 바탕에 그림이 붙었다", background != null && background.sprite != null);
            Report("버튼 아이콘에 그림이 붙었다", icon != null && icon.sprite != null);

            Report("누를 수 없을 때 쓸 바탕 그림이 있다",
                GetField<Sprite>(button, "_backgroundDisabledSprite") != null);
            Report("누를 수 없을 때 쓸 아이콘 그림이 있다",
                GetField<Sprite>(button, "_iconDisabledSprite") != null);

            // 기획서와 와이어프레임의 자리는 우측 하단이다.
            HudLayoutConfig layout = UiSceneBuilder.LoadConfig<HudLayoutConfig>("Hud", "HudLayoutConfig");
            Report("설정이 우측 하단으로 되어 있다", layout != null && !layout.OnLeftCorner);

            RectTransform rect = button.RectTransform;
            Report("버튼 기준점이 오른쪽에 있다",
                Mathf.Approximately(rect.anchorMin.x, 1f), Mathf.RoundToInt(rect.anchorMin.x));
            Report("버튼 기준점이 아래에 있다",
                Mathf.Approximately(rect.anchorMin.y, 0f), Mathf.RoundToInt(rect.anchorMin.y));
        }

        /// <summary>
        /// 상단 표시줄의 런 종료 버튼을 **진짜로 눌러** 본다.
        ///
        /// 여기까지 오는 길이 길다.
        /// 버튼 → 상단 표시줄 조종기 → 흐름 → 확인 팝업 → 고른 결과 → 런 종료.
        /// 지금까지는 `flow.EndRun` 을 직접 불러 확인해서 이 길을 한 번도 지나가지 않았다.
        ///
        /// 눌러 보고 나서 **취소를 골라 런을 그대로 둔다.**
        /// 여기서 런을 끝내 버리면 뒤에 이어지는 검사가 굴러가지 않는다.
        /// </summary>
        private static void CheckEndRunButton(GameFlowController flow, SaveService save)
        {
            TopBarButton endRun = null;
            TopBarButton[] buttons = Object.FindObjectsByType<TopBarButton>(FindObjectsInactive.Include);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].Kind == TopBarButtonKind.EndRun)
                {
                    endRun = buttons[i];
                    break;
                }
            }

            Report("런 종료 버튼이 있다", endRun != null);

            if (endRun == null)
            {
                return;
            }

            Report("런 종료 버튼이 화면에 보인다", endRun.gameObject.activeInHierarchy);
            Report("런 종료 버튼을 누를 수 있다", endRun.Interactable);

            Button button = GetField<Button>(endRun, "_button");
            Report("런 종료 버튼에 누름이 물려 있다", button != null);

            if (button == null)
            {
                return;
            }

            button.onClick.Invoke();

            PopupPresenter popup = Object.FindAnyObjectByType<PopupPresenter>(
                FindObjectsInactive.Include);

            Report("누르면 확인 팝업이 뜬다", popup != null && popup.IsOpen);

            AwakenNew();

            if (popup == null || !popup.IsOpen)
            {
                return;
            }

            if (popup.Current != null)
            {
                string ids = string.Empty;
                for (int i = 0; i < popup.Current.Buttons.Count; i++)
                {
                    ids += popup.Current.Buttons[i].Id + " ";
                }

                Debug.Log("  뜬 팝업: [" + popup.Current.Title + "] 버튼 " + ids);
            }

            Report("고를 것이 셋", popup.Current != null ? popup.Current.ButtonCount : 0, 3);

            // 세 버튼이 화면에 실제로 그려지는지.
            PopupButtonView[] shown = Object.FindObjectsByType<PopupButtonView>(FindObjectsInactive.Exclude);

            int visible = 0;
            PopupButtonView cancel = null;

            for (int i = 0; i < shown.Length; i++)
            {
                if (!shown[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                visible++;

                if (shown[i].Spec.Id == "cancel")
                {
                    cancel = shown[i];
                }
            }

            Report("버튼 셋이 화면에 보인다", visible, 3);
            Report("취소를 고를 수 있다", cancel != null);

            // 취소를 눌러 런을 그대로 둔다.
            if (cancel != null)
            {
                Button cancelButton = GetField<Button>(cancel, "_button");
                if (cancelButton != null)
                {
                    cancelButton.onClick.Invoke();
                }
            }
            else
            {
                popup.Dismiss();
            }

            Report("취소하면 팝업이 닫힌다", !popup.IsOpen);
            Report("취소하면 런이 그대로다", flow.Screen == GameScreen.Map, (int)flow.Screen);
            Report("취소하면 런 자료가 남아 있다", save.Run != null);
            Report("닫히면 어두운 막도 걷힌다", !DimIsShowing());
        }

        /// <summary>
        /// 팝업의 어두운 막이 지금 화면을 덮고 있는지.
        ///
        /// 막이 팝업 칸과 따로 놀면 팝업이 떠도 뒤가 안 막히거나,
        /// 팝업이 닫혀도 막이 남아 **화면 전체가 안 눌리게** 된다.
        /// </summary>
        private static bool DimIsShowing()
        {
            PopupView view = Object.FindAnyObjectByType<PopupView>(FindObjectsInactive.Include);
            if (view == null)
            {
                return false;
            }

            // PopupView 는 팝업 껍데기에 붙어 있고 막은 그 아래 PopupBody 안에 있다.
            Transform body = view.transform.Find("PopupBody");
            Transform dim = body != null ? body.Find("Dim") : null;

            return dim != null && dim.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 상단 표시줄의 런 종료를 누르고 팝업에서 그 버튼을 고른다.
        /// `cancel` 은 그대로 두고, `abandon` 은 런을 버리고, `save` 는 저장하고 타이틀로 간다.
        /// </summary>
        private static void PressEndRun(string buttonId)
        {
            TopBarButton endRun = null;
            TopBarButton[] buttons = Object.FindObjectsByType<TopBarButton>(FindObjectsInactive.Include);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].Kind == TopBarButtonKind.EndRun)
                {
                    endRun = buttons[i];
                    break;
                }
            }

            Button topBarButton = endRun != null ? GetField<Button>(endRun, "_button") : null;
            Report("런 종료를 누를 수 있다", topBarButton != null);

            if (topBarButton == null)
            {
                return;
            }

            topBarButton.onClick.Invoke();
            AwakenNew();

            PopupButtonView[] shown = Object.FindObjectsByType<PopupButtonView>(FindObjectsInactive.Exclude);

            for (int i = 0; i < shown.Length; i++)
            {
                if (!shown[i].gameObject.activeInHierarchy || shown[i].Spec.Id != buttonId)
                {
                    continue;
                }

                Button chosen = GetField<Button>(shown[i], "_button");
                if (chosen != null)
                {
                    chosen.onClick.Invoke();
                    Report("런 종료 팝업에서 " + buttonId + " 를 골랐다", true);
                    return;
                }
            }

            Report("런 종료 팝업에서 " + buttonId + " 를 골랐다", false);
        }

        /// <summary>
        /// 지도 전체가 한 화면에 들어오는지 본다. 스크롤해야 다 보이면 안 된다.
        ///
        /// 한 번 그랬다. 지도 내용을 담는 칸이 네 귀퉁이에 펼쳐져 있어서
        /// `sizeDelta` 가 크기가 아니라 **부모 바깥으로 더 나가는 여백**으로 읽혔다.
        /// 그래서 지도가 좌표 배율만큼 커져 노드가 사방으로 흩어졌다.
        /// </summary>
        private static void CheckMapFitsScreen()
        {
            MapScreenView view = Object.FindAnyObjectByType<MapScreenView>(
                FindObjectsInactive.Include);

            Report("맵 화면이 있다", view != null);

            if (view == null)
            {
                return;
            }

            RectTransform viewport = GetField<RectTransform>(view, "_viewport");
            RectTransform content = GetField<RectTransform>(view, "_content");

            Report("보이는 칸과 내용 칸이 있다", viewport != null && content != null);

            if (viewport == null || content == null)
            {
                return;
            }

            Report("내용이 보이는 칸보다 넓지 않다",
                content.rect.width <= viewport.rect.width + 1f, (int)content.rect.width);
            Report("내용이 보이는 칸보다 높지 않다",
                content.rect.height <= viewport.rect.height + 1f, (int)content.rect.height);

            // 보이는 칸도 화면 안에 들어가야 한다.
            Report("보이는 칸이 화면 폭을 넘지 않는다",
                viewport.rect.width <= UiSceneBuilder.ReferenceResolution.x + 1f,
                (int)viewport.rect.width);
            Report("보이는 칸이 화면 높이를 넘지 않는다",
                viewport.rect.height <= UiSceneBuilder.ReferenceResolution.y + 1f,
                (int)viewport.rect.height);
        }

        /// <summary>
        /// 런 중 화면 뒤에 돌벽이 보이는지.
        ///
        /// 공용 배경은 처음부터 맨 뒤에 깔려 있었는데 **한 번도 보인 적이 없다.**
        /// 맵과 이벤트와 런 종료 결과가 저마다 화면을 꽉 채우는 어두운 바탕을 깔아 덮고 있었다.
        /// 그것이 그 화면들의 까만 부분이었다.
        ///
        /// 화면마다 바로 아래 자식 가운데 꽉 채우고 불투명한 그림이 있으면 덮는 것으로 본다.
        /// </summary>
        private static void CheckBackdrops()
        {
            GameObject canvasGo = GameObject.Find("GameCanvas");
            Transform canvas = canvasGo != null ? canvasGo.transform : null;

            Report("게임 캔버스가 있다", canvas != null);

            if (canvas == null)
            {
                return;
            }

            Transform run = canvas.Find("RunBackground");
            Image runImage = run != null ? run.GetComponent<Image>() : null;

            Report("공용 배경이 있다", runImage != null);
            Report("공용 배경에 돌벽 그림이 붙어 있다", runImage != null && runImage.sprite != null);
            Report("공용 배경이 맨 뒤에 있다", run != null && run.GetSiblingIndex() == 0,
                run != null ? run.GetSiblingIndex() : -1);

            Transform shade = run != null ? run.Find("Shade") : null;
            Image shadeImage = shade != null ? shade.GetComponent<Image>() : null;
            Report("돌벽 위에 검정 50퍼센트가 얹혀 있다",
                shadeImage != null
                && Mathf.Abs(shadeImage.color.a - UiSceneBuilder.StoneBackdropShade) < 0.01f);

            // 돌벽이 보여야 하는 화면들. 와이어프레임에서 돌벽을 쓴 셋과,
            // 검정 50퍼센트 막만 얹어 뒤가 비쳐야 하는 보상 화면이다.
            // 보상은 예전에 불투명한 회색 막을 깔아 돌벽을 통째로 가렸다.
            string[] screens = { "MapScreen", "EventScreen", "RunResultScreen", "RewardScreen" };
            int covering = 0;

            for (int i = 0; i < screens.Length; i++)
            {
                Transform screen = canvas.Find(screens[i]);

                if (screen == null)
                {
                    Report(screens[i] + " 가 있다", false);
                    continue;
                }

                for (int c = 0; c < screen.childCount; c++)
                {
                    Transform child = screen.GetChild(c);
                    Image image = child.GetComponent<Image>();
                    RectTransform rect = child as RectTransform;

                    if (image == null || rect == null)
                    {
                        continue;
                    }

                    bool fills = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
                    bool opaque = image.color.a >= 0.99f;

                    if (fills && opaque)
                    {
                        covering++;
                        Debug.LogWarning("  배경  " + PathOf(child.gameObject) + " 가 공용 배경을 덮는다");
                    }
                }
            }

            Report("화면마다 깐 바탕이 공용 배경을 덮지 않는다", covering, 0);
        }

        /// <summary>
        /// 맵 뒤에 양 끝이 말린 양피지가 있는지.
        ///
        /// 와이어프레임 `10 맵 화면` 의 `지도 배경 · 양피지_세로확장2.png` 다.
        /// **뷰포트 안에 두면 말린 끝이 잘린다.** 뷰포트가 노드 영역 크기로 잘라 내기 때문이다.
        /// 노드 영역도 양피지 종이 몸통 안에 들어가야 하고 노드끼리 겹치면 안 된다.
        /// </summary>
        private static void CheckParchment(MapScreenController map)
        {
            MapScreenView view = Object.FindAnyObjectByType<MapScreenView>(FindObjectsInactive.Include);
            MapVisualConfig visual = view != null ? GetField<MapVisualConfig>(view, "_visual") : null;
            RectTransform viewport = view != null ? GetField<RectTransform>(view, "_viewport") : null;

            if (view == null || visual == null || viewport == null)
            {
                Report("맵 화면과 표시 설정이 있다", false);
                return;
            }

            Transform paper = view.transform.Find("Parchment");
            Image paperImage = paper != null ? paper.GetComponent<Image>() : null;

            Report("맵 뒤에 양피지가 있다", paperImage != null);
            Report("양피지에 그림이 붙어 있다", paperImage != null && paperImage.sprite != null);

            if (paperImage == null)
            {
                return;
            }

            Report("양피지가 뷰포트 밖에 있다. 안이면 말린 끝이 잘린다",
                paper.parent != viewport && !paper.IsChildOf(viewport));
            Report("양피지가 노드보다 뒤에 그려진다",
                paper.GetSiblingIndex() < viewport.GetSiblingIndex(),
                paper.GetSiblingIndex());

            RectTransform paperRect = (RectTransform)paper;
            Report("양피지가 와이어프레임 크기다",
                Vector2.Distance(paperRect.rect.size, visual.ParchmentSize) < 1f,
                (int)paperRect.rect.width);
            Report("노드 영역이 양피지 안에 들어간다", visual.MapFitsInsideParchment());

            // 이웃 노드의 누르는 영역이 어떤 지터에서도 겹치지 않아야 한다.
            // 아이콘 모양은 가장 넓은 이벤트 방 둘이 끝까지 쏠릴 때만 4 안쪽으로 닿는다. 그것은 받아들였다.
            MapGenerationConfig generation = GetField<MapGenerationConfig>(map, "_generationConfig");

            if (generation != null)
            {
                float closest = visual.ClosestNodeDistance(12, 5, generation.JitterX, generation.JitterY);
                Report("이웃 노드의 누르는 영역이 겹치지 않는다",
                    visual.ClickAreasNeverOverlap(12, 5, generation.JitterX, generation.JitterY),
                    (int)UiScale.ToWireframe(closest));
                Report("가장 넓은 아이콘 모양끼리 닿아도 4 안쪽이다",
                    visual.NodeSize * visual.WidestIconShapeRatio - closest < UiScale.Px(4f));
            }

            // 누르는 영역을 아이콘 모양 크기로 줄였는지.
            MapNodeView firstNode = Object.FindAnyObjectByType<MapNodeView>(FindObjectsInactive.Include);
            if (firstNode != null)
            {
                Image icon = GetField<Image>(firstNode, "_icon");
                Image check = GetField<Image>(firstNode, "_checkMark");
                float padding = MapNodeView.GetClickPadding(visual);
                Report("노드 아이콘의 누르는 영역을 줄였다",
                    icon != null && Mathf.Abs(icon.raycastPadding.x - padding) < 0.5f
                    && Mathf.Abs(icon.raycastPadding.w - padding) < 0.5f);
                Report("완료 체크는 커서를 받지 않는다", check != null && !check.raycastTarget);
                Report("완료 체크가 설정 크기다",
                    check != null && Vector2.Distance(check.rectTransform.sizeDelta, visual.CheckMarkSize) < 0.5f);
            }

            // 실제로 놓인 노드가 노드 영역 안에 다 들어가는지.
            MapNodeView[] nodes = Object.FindObjectsByType<MapNodeView>(FindObjectsInactive.Include);

            int outside = 0;
            float halfW = viewport.rect.width * 0.5f;
            float halfH = viewport.rect.height * 0.5f;

            for (int i = 0; i < nodes.Length; i++)
            {
                RectTransform rect = (RectTransform)nodes[i].transform;
                Vector2 p = rect.anchoredPosition;
                float half = visual.NodeSize * 0.5f;

                if (p.x - half < -halfW - 1f || p.x + half > halfW + 1f
                    || p.y - half < -halfH - 1f || p.y + half > halfH + 1f)
                {
                    outside++;
                }
            }

            Report("노드가 다 노드 영역 안에 놓인다", outside, 0);

            CheckNodeHoverKeepsHighlight(nodes, visual);
        }

        /// <summary>
        /// 고를 수 있는 노드가 마우스가 지나간 뒤에도 커진 채로 남는지.
        ///
        /// 노드는 고를 수 있으면 스스로 5퍼센트 커지고, 호버 확대도 크기를 만진다.
        /// 호버 확대가 처음 크기를 기준으로 삼던 때는 마우스가 지나가면
        /// **고를 수 있다는 강조가 사라졌다.**
        /// </summary>
        private static void CheckNodeHoverKeepsHighlight(MapNodeView[] nodes, MapVisualConfig visual)
        {
            // 노드는 맵을 만들 때 찍어 낸 것이라 편집 모드에서는 아직 Awake 를 받지 못했다.
            // 빠뜨리면 호버 확대가 키울 칸을 몰라 아무 일도 안 한다. 플레이 모드에서는 유니티가 부른다.
            AwakenNew();

            MapNodeView selectable = null;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].State == MapNodeDisplayState.Selectable)
                {
                    selectable = nodes[i];
                    break;
                }
            }

            if (selectable == null)
            {
                Report("고를 수 있는 노드가 있다", false);
                return;
            }

            HoverScale hover = selectable.GetComponent<HoverScale>();
            Report("노드에 호버 확대가 붙어 있다", hover != null);

            if (hover == null)
            {
                return;
            }

            float before = selectable.transform.localScale.x;
            Report("고를 수 있는 노드는 커져 있다",
                Mathf.Abs(before - visual.SelectableScale) < 0.001f, (int)(before * 1000f));

            hover.OnPointerEnter(null);
            float hovered = selectable.transform.localScale.x;

            hover.OnPointerExit(null);
            float after = selectable.transform.localScale.x;

            Report("마우스를 올리면 더 커진다", hovered > before + 0.001f, (int)(hovered * 1000f));
            Report("마우스가 지나가도 고를 수 있다는 강조가 남는다",
                Mathf.Abs(after - before) < 0.001f, (int)(after * 1000f));
        }

        /// <summary>
        /// 전투 방에 들어갔을 때 대역 팝업이 **실제로 화면에 보이는지** 본다.
        ///
        /// 전투 코드가 아직 없어 이 팝업이 유일한 진행 수단이다.
        /// 이게 안 보이면 전투 방에서 게임이 멈춰 버린다.
        ///
        /// 한 번 그랬다. 팝업 칸의 부모가 꺼져 있어 `Show` 가 칸을 켜도 화면에 안 나왔다.
        /// 그래서 켜졌는지만 보지 않고 **부모까지 따져 보는** `activeInHierarchy` 로 본다.
        /// </summary>
        private static void CheckCombatPopup()
        {
            PopupPresenter popup = Object.FindAnyObjectByType<PopupPresenter>(
                FindObjectsInactive.Include);

            Report("팝업 연출기가 있다", popup != null);

            if (popup == null)
            {
                return;
            }

            Report("전투 방에서 대역 팝업이 열려 있다", popup.IsOpen);
            Report("팝업이 뜨면 뒤가 막힌다", DimIsShowing());

            // 팝업 버튼은 방금 찍어 낸 것이라 아직 귀를 대지 못했다.
            AwakenNew();
            Report("팝업에 고를 것이 있다", popup.Current != null);

            if (popup.Current == null)
            {
                return;
            }

            Report("승리와 패배 둘을 고를 수 있다", popup.Current.ButtonCount, 2);

            // 버튼이 화면에 실제로 그려지는지까지 본다.
            PopupButtonView[] buttons = Object.FindObjectsByType<PopupButtonView>(FindObjectsInactive.Exclude);

            int shown = 0;
            PopupButtonView victory = null;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (!buttons[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                shown++;

                if (buttons[i].Spec.Id == "victory")
                {
                    victory = buttons[i];
                }
            }

            Report("버튼 둘이 화면에 보인다", shown, 2);
            Report("승리를 고를 수 있다", victory != null);

            // 승리를 눌러 전투를 끝낸다. 대역이 결과를 흐름에 넘긴다.
            if (victory == null)
            {
                return;
            }

            Button button = GetField<Button>(victory, "_button");
            if (button != null)
            {
                button.onClick.Invoke();
            }

            Report("승리를 누르면 팝업이 닫힌다", !popup.IsOpen);
        }

        /// <summary>
        /// 게임을 처음 켜면 첫 프로필이 이름을 달고 나와 바로 시작할 수 있는지 본다.
        ///
        /// 이게 없으면 세 칸이 모두 비어 있어 이름을 직접 쳐야 게임이 시작된다.
        /// **이름은 원래 스팀이나 스토브 닉네임을 받을 자리다.** 아직 연동이 없어 설정 값을 쓴다.
        /// </summary>
        private static void CheckFirstProfilePrefilled(SaveService save, MemorySaveStorage storage)
        {
            ProfileList list = save.BuildProfileList();

            Report("첫 프로필이 미리 만들어져 있다", storage.Exists("profile0_meta.json"));
            Report("첫 칸이 비어 있지 않다", !list.Get(0).IsEmpty);
            Report("첫 칸을 바로 누를 수 있다", list.Get(0).CanEnter);

            string expected = save.Config.FirstProfileName;
            Report("첫 칸 이름이 설정의 기본 닉네임과 같다", list.Get(0).Name == expected);

            // 나머지 칸은 비어 있어야 한다. 셋 다 채우면 새로 만들 자리가 없다.
            Report("둘째 칸은 비어 있다", list.Get(1).IsEmpty);
            Report("셋째 칸은 비어 있다", list.Get(2).IsEmpty);
        }

        /// <summary>
        /// 입력이 실제로 먹는 설정인지 본다.
        ///
        /// 이것 때문에 플레이 모드에서 **아무것도 작동하지 않은 적이 있다.**
        /// Universal 2D 템플릿은 `Active Input Handling` 을 새 입력 시스템 하나로 두고 나오는데,
        /// 그 상태에서는 씬의 `StandaloneInputModule` 이 죽어 버튼이 하나도 안 눌리고
        /// `Input.GetKeyDown` 은 부를 때마다 예외를 던진다.
        /// `GameFlowController.Update` 가 그걸 매 프레임 부르므로 콘솔이 예외로 덮인다.
        ///
        /// 검사가 이걸 못 잡았던 까닭은 여기서 버튼을 진짜로 누르지 않고
        /// `onClick` 을 직접 불러 왔기 때문이다. 입력 경로를 한 번도 지나가지 않았다.
        ///
        /// 설정은 프로젝트 설정 / 플레이어 / Active Input Handling 에서 바꾼다.
        /// 바꾸면 에디터를 다시 켜야 적용된다.
        /// </summary>
        private static void CheckInputHandling()
        {
            // 0 = 옛 입력만, 1 = 새 입력만, 2 = 둘 다.
            SerializedObject settings = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty handler = settings.FindProperty("activeInputHandler");

            int mode = handler != null ? handler.intValue : -1;
            Report("입력 처리 설정을 읽었다", handler != null, mode);

            bool usesOldInput = Object.FindAnyObjectByType<StandaloneInputModule>(
                FindObjectsInactive.Include) != null;

            if (usesOldInput)
            {
                Report("씬이 옛 입력 모듈을 쓰는데 설정이 그걸 켜 두었다", mode == 0 || mode == 2, mode);
            }

            // 코드가 `Input.GetKeyDown` 을 쓰므로 새 입력만으로 두면 예외가 난다.
            Report("Input.GetKeyDown 이 예외 없이 도는 설정", mode == 0 || mode == 2, mode);
        }

        /// <summary>
        /// 확인용 씬의 예시 칸이 따라 들어오지 않았는지 본다.
        ///
        /// 게임 씬은 화면을 확인용 씬에서 통째로 가져다 쓴다.
        /// 확인용 씬에는 선택지나 카드 같은 예시 칸이 만들어져 저장돼 있는데
        /// 그게 따라오면 게임에서 만든 진짜 칸 뒤에 겹쳐 남는다.
        /// 이벤트 방에서 `지나친다` 가 두 번 나온 적이 있다.
        ///
        /// 칸은 게임이 돌면서 만드는 것이므로 씬을 막 열었을 때는 하나도 없어야 맞다.
        /// </summary>
        private static void CheckNoDemoItems()
        {
            Report("예시 선택지가 남아 있지 않다", CountOf<EventChoiceView>(), 0);
            Report("예시 보상 카드가 남아 있지 않다", CountOf<RewardCardView>(), 0);
            Report("예시 프로필 카드가 남아 있지 않다", CountOf<ProfileCardView>(), 0);
            Report("예시 맵 노드가 남아 있지 않다", CountOf<MapNodeView>(), 0);
            Report("예시 맵 간선이 남아 있지 않다", CountOf<MapEdgeView>(), 0);
            Report("예시 팝업 버튼이 남아 있지 않다", CountOf<PopupButtonView>(), 0);
            Report("예시 설정 탭이 남아 있지 않다", CountOf<SettingsTabView>(), 0);
            Report("예시 설정 줄이 남아 있지 않다", CountOf<SettingsRowView>(), 0);
            Report("예시 타이틀 메뉴가 남아 있지 않다", CountOf<TitleMenuItemView>(), 0);
            Report("예시 빌드 칸이 남아 있지 않다", CountOf<BuildSlotView>(), 0);
            Report("예시 태그 줄이 남아 있지 않다", CountOf<BuildTagBarView>(), 0);
        }

        /// <summary>
        /// 성소 방에 들어간 채로 행상이 실제로 열리는지 본다.
        ///
        /// 행상은 오래 "버튼은 눌리는데 아무것도 안 열리는" 상태였다.
        /// `_merchantRoot` 와 `_merchantView` 가 비어 있었기 때문이다.
        /// 다시 그렇게 되지 않게 여기서 눌러 보고 진열까지 확인한다.
        /// </summary>
        private static void CheckMerchant()
        {
            SanctumController sanctum = Object.FindAnyObjectByType<SanctumController>();
            Report("성소 조종기가 있다", sanctum != null);

            if (sanctum == null)
            {
                return;
            }

            // 행상 칸은 지금 꺼져 있으므로 꺼진 것까지 찾는다.
            MerchantScreenView merchant = Object.FindAnyObjectByType<MerchantScreenView>(
                FindObjectsInactive.Include);
            Report("행상 화면이 씬에 있다", merchant != null);

            int slots = CountOf<MerchantSlotView>();
            Report("진열 자리가 아홉", slots, 9);

            if (merchant == null)
            {
                return;
            }

            Report("들어가기 전에는 행상이 닫혀 있다", !sanctum.IsMerchantOpen);

            sanctum.OpenMerchant();
            Report("행상 버튼을 누르면 열린다", sanctum.IsMerchantOpen);
            Report("열리면 행상 화면이 켜진다", merchant.gameObject.activeInHierarchy);

            // 진열이 채워져 물건 그림이 붙어야 한다.
            int withIcon = 0;
            MerchantSlotView[] all = Object.FindObjectsByType<MerchantSlotView>(FindObjectsInactive.Include);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Slot.HasItem)
                {
                    withIcon++;
                }
            }

            Report("진열에 물건이 찼다", withIcon > 0, withIcon);

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Slot.HasItem && all[i].gameObject.activeInHierarchy)
                {
                    CheckItemDetailHover("행상 진열", all[i], true);
                    break;
                }
            }

            CheckPurchase(sanctum, all);

            // 행상이 열려 있는 지금이 문양 변경을 눌러 볼 수 있는 때다.
            CheckSymbolChange(
                Object.FindAnyObjectByType<GameFlowController>(),
                Object.FindAnyObjectByType<SaveService>());

            sanctum.CloseMerchant();
            Report("돌아가기를 누르면 닫힌다", !sanctum.IsMerchantOpen);
        }

        /// <summary>
        /// 행상에서 하나를 사 본다. 값을 치르고 산 물건이 런에 들어와야 한다.
        ///
        /// 산 물건은 `SanctumState.TryBuy` 가 `RunCatalog.Acquire` 를 불러 런에 넣는다.
        /// `남은 일.md` 가 오래 "골드만 빠지고 물건이 안 들어간다" 고 적고 있었는데
        /// 게임 씬에서 실제로 사 보는 검사가 없어 그 말이 맞는지 아무도 확인하지 않았다.
        /// </summary>
        private static void CheckPurchase(SanctumController sanctum, MerchantSlotView[] slots)
        {
            SaveService save = Object.FindAnyObjectByType<SaveService>();
            if (save == null || save.Run == null)
            {
                Report("행상 구매를 볼 런이 있다", false);
                return;
            }

            GameFlowController flow = Object.FindAnyObjectByType<GameFlowController>();
            PopupPresenter popup = flow != null ? GetField<PopupPresenter>(flow, "_popup") : null;

            // 골드가 모자란 칸을 누르면 아무 일도 없어야 한다. 성소 기획서 06 골드 의 "소모 불가" 와 붉은 가격까지다.
            // 예전에는 "골드가 모자랍니다" 팝업이 떴다. 기획서에도 요청에도 없던 것이라 2026년 10월 6일에 뺐다.
            MerchantSlotView poorTarget = null;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Slot != null && slots[i].Slot.IsBuyable && slots[i].Slot.Price > 0)
                {
                    poorTarget = slots[i];
                    break;
                }
            }

            if (flow != null && flow.Context != null && poorTarget != null && popup != null)
            {
                flow.Context.Gold.TrySpend(flow.Context.Gold.Gold);
                int poorOwned = save.Run.Owned.TotalSymbolCount + save.Run.Owned.Relics.Count
                    + save.Run.Owned.CoinIds.Count;

                Invoke(sanctum, "HandleSlotClicked", poorTarget);

                Report("골드가 모자란 칸을 눌러도 팝업이 뜨지 않는다", !popup.IsOpen);
                Report("골드가 모자란 칸을 누르면 아무것도 들어오지 않는다",
                    save.Run.Owned.TotalSymbolCount + save.Run.Owned.Relics.Count + save.Run.Owned.CoinIds.Count == poorOwned);
                Report("골드가 모자란 칸은 팔리지 않는다", !poorTarget.Slot.Sold);
            }

            // 넉넉히 쥐여 주고 산다.
            if (flow != null && flow.Context != null)
            {
                flow.Context.GainGold(1000);
            }

            MerchantSlotView target = null;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Slot != null && slots[i].Slot.IsBuyable)
                {
                    target = slots[i];
                    break;
                }
            }

            if (target == null)
            {
                Report("살 수 있는 진열 자리가 있다", false);
                return;
            }

            MerchantSlot slot = target.Slot;
            int goldBefore = save.Run.Status.Gold;
            int ownedBefore = save.Run.Owned.TotalSymbolCount + save.Run.Owned.Relics.Count
                + save.Run.Owned.CoinIds.Count;

            Invoke(sanctum, "HandleSlotClicked", target);

            int ownedAfter = save.Run.Owned.TotalSymbolCount + save.Run.Owned.Relics.Count
                + save.Run.Owned.CoinIds.Count;

            Report("행상에서 사면 값만큼 골드가 준다",
                save.Run.Status.Gold == goldBefore - slot.Price, goldBefore - save.Run.Status.Gold);
            Report("행상에서 산 물건이 런에 들어온다", ownedAfter == ownedBefore + 1, ownedAfter - ownedBefore);
            Report("산 자리는 팔린 것으로 남는다", slot.Sold);

            CheckPurchaseWhenFull(sanctum, slots, save, flow);
        }

        /// <summary>
        /// 코인이 소지 한도까지 찼을 때 행상에서 코인을 사면 버릴 것을 고르게 하는지.
        /// 포기하면 사지 않아 골드도 그대로고, 고르면 그것을 버리고 산다. 2026년 10월 9일 원재가 정했다.
        /// </summary>
        private static void CheckPurchaseWhenFull(
            SanctumController sanctum, MerchantSlotView[] slots, SaveService save, GameFlowController flow)
        {
            ItemPickerScreenController picker = Object.FindAnyObjectByType<ItemPickerScreenController>(FindObjectsInactive.Include);
            MerchantSlotView coinSlot = null;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Slot != null && slots[i].Slot.IsBuyable && slots[i].Slot.Item.Kind == SanctumItemKind.Coin)
                {
                    coinSlot = slots[i];
                    break;
                }
            }

            if (picker == null || coinSlot == null || flow == null || flow.Context == null)
            {
                Report("가득 찬 채로 살 코인 칸과 고르기 화면이 있다", false);
                return;
            }

            RunOwnedData owned = save.Run.Owned;
            while (owned.CoinIds.Count < owned.CoinCapacity)
            {
                owned.AddCoin(coinSlot.Slot.Item.Id + "_가득");
            }

            flow.Context.GainGold(1000);
            int goldBefore = save.Run.Status.Gold;
            int coinsBefore = owned.CoinIds.Count;

            Invoke(sanctum, "HandleSlotClicked", coinSlot);
            AwakenNew();
            Report("코인이 가득이면 사기 전에 버릴 코인 고르기가 뜬다",
                picker.IsOpen && picker.Request != null && picker.Request.Title == "버릴 코인 선택", picker.IsOpen ? 1 : 0);
            Report("버릴 것 고르기는 포기할 수 있다", picker.CanCancel && picker.Request != null && picker.Request.CancelText == "포기");

            picker.Cancel();
            Report("포기하면 사지 않는다", !coinSlot.Slot.Sold && owned.CoinIds.Count == coinsBefore, owned.CoinIds.Count);
            Report("포기하면 골드가 그대로다", save.Run.Status.Gold == goldBefore, goldBefore - save.Run.Status.Gold);

            Invoke(sanctum, "HandleSlotClicked", coinSlot);
            AwakenNew();
            if (!picker.IsOpen || picker.Request == null || picker.Request.Candidates.Count == 0)
            {
                Report("다시 누르면 버릴 코인 고르기가 뜬다", false);
                return;
            }

            BuildEntry dropped = picker.Request.Candidates[0];
            int droppedBefore = CountCoin(owned, dropped.Id);
            picker.Toggle(dropped);
            picker.Confirm();

            Report("버리면 산다", coinSlot.Slot.Sold && owned.CoinIds.Contains(coinSlot.Slot.Item.Id));
            Report("버린 코인이 빠지고 한도를 넘지 않는다",
                CountCoin(owned, dropped.Id) == droppedBefore - (dropped.Id == coinSlot.Slot.Item.Id ? 0 : 1)
                && owned.CoinIds.Count == owned.CoinCapacity, owned.CoinIds.Count);
            Report("버리고 사면 값만큼 골드가 준다", save.Run.Status.Gold == goldBefore - coinSlot.Slot.Price,
                goldBefore - save.Run.Status.Gold);

            owned.CoinIds.RemoveAll(id => id.EndsWith("_가득"));
        }

        private static int CountCoin(RunOwnedData owned, string id)
        {
            int count = 0;
            for (int i = 0; i < owned.CoinIds.Count; i++)
            {
                count += owned.CoinIds[i] == id ? 1 : 0;
            }

            return count;
        }

        /// <summary>
        /// 타이틀 화면의 그림.
        ///
        /// 배경 요청 기획서가 "메인 화면 배경 · 고대 유적 슬롯"을 발주해 뒀고 아직 안 왔다.
        /// 그림이 없는 것 자체는 실패로 치지 않는다. 대신 자리가 비어 있지 않은지만 본다.
        /// 비어 있으면 검은 화면이 되어 그림이 빠진 것인지 깨진 것인지 가릴 수가 없다.
        /// `코드/Title/Art` 에 `배경_메인화면.png` 를 넣으면 이 검사가 그림을 찾는다.
        /// </summary>
        private static void CheckTitleArt()
        {
            TitleScreenView view = Object.FindAnyObjectByType<TitleScreenView>();
            Report("타이틀 화면이 있다", view != null);

            if (view == null)
            {
                return;
            }

            Image background = GetField<Image>(view, "_background");
            Image logo = GetField<Image>(view, "_logo");

            Report("타이틀 배경 칸이 있다", background != null);
            Report("타이틀 로고 칸이 있다", logo != null);

            if (background == null || logo == null)
            {
                return;
            }

            // 그림이 없으면 색이라도 깔려 있어야 한다.
            Report("타이틀 배경이 비어 있지 않다",
                background.sprite != null || background.color.a > 0.5f);

            // 로고는 그림이 있거나, 없으면 글자라도 보여야 한다.
            // 배경 그림 위에서는 와이어프레임처럼 칸 없이 글자만 둔다.
            TMP_Text logoLabel = logo.GetComponentInChildren<TMP_Text>(true);
            Report("타이틀 로고 자리가 보인다",
                logo.sprite != null
                || (logoLabel != null && !string.IsNullOrEmpty(logoLabel.text) && logoLabel.color.a > 0.5f));

            if (background.sprite != null && logo.sprite == null)
            {
                Report("배경 그림 위의 로고에는 칸을 얹지 않는다", logo.color.a < 0.01f,
                    (int)(logo.color.a * 100f));
            }

            if (background.sprite == null)
            {
                Debug.Log("  그림  타이틀 배경이 아직 없다. 배경 요청 기획서의 메인 화면 배경을 기다린다.");
            }
        }

        /// <summary>
        /// 타이틀 오른쪽 위의 현재 프로필 버튼.
        ///
        /// 버튼은 만들어 뒀는데 흐름이 그 `Clicked` 를 듣지 않아
        /// 타이틀에서 프로필 선택 화면으로 갈 길이 아예 없었다.
        /// 버튼을 진짜로 눌러 화면이 바뀌는지 본다.
        /// </summary>
        private static void CheckCurrentProfileButton(GameFlowController flow, SaveService save)
        {
            CurrentProfileButton button = GetField<CurrentProfileButton>(flow, "_currentProfileButton");
            Report("흐름이 현재 프로필 버튼을 물고 있다", button != null);

            if (button == null)
            {
                // 물리지 않았으면 눌러 볼 수가 없다. 화면을 직접 열어 뒤 검사를 이어 간다.
                flow.GoToProfileSelect();
                return;
            }

            TMP_Text label = GetField<TMP_Text>(button, "_label");
            Report("버튼에 지금 프로필 이름이 적힌다",
                label != null && label.text == save.BuildProfileList().CurrentName);

            Button inner = GetField<Button>(button, "_button");
            Report("버튼에 누를 수 있는 것이 붙어 있다", inner != null);

            if (inner == null)
            {
                flow.GoToProfileSelect();
                return;
            }

            inner.onClick.Invoke();
            Report("현재 프로필 버튼을 누르면 프로필 선택이 열린다",
                flow.Screen == GameScreen.ProfileSelect, (int)flow.Screen);

            if (flow.Screen != GameScreen.ProfileSelect)
            {
                flow.GoToProfileSelect();
            }
        }

        /// <summary>
        /// 프로필 삭제.
        ///
        /// 화면은 `DeleteRequested` 로 알리기만 하는데 흐름이 그것을 듣지 않아
        /// 삭제 버튼을 눌러도 아무 일이 없었다.
        /// 카드의 삭제 버튼을 진짜로 눌러 팝업이 뜨고 자리가 비는지까지 본다.
        /// </summary>
        private static void CheckProfileDelete(
            GameFlowController flow, SaveService save, MemorySaveStorage storage)
        {
            ProfileSelectScreenController screen =
                Object.FindAnyObjectByType<ProfileSelectScreenController>();
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");

            if (screen == null || screen.List == null || popup == null)
            {
                Report("프로필 선택 화면과 팝업이 있다", false);
                return;
            }

            // 지울 자리를 고른다. 0번은 뒤 검사가 쓰므로 건드리지 않는다.
            int target = -1;
            for (int i = 1; i < screen.List.Count; i++)
            {
                if (screen.List.Get(i).CanEnter)
                {
                    target = i;
                    break;
                }
            }

            if (target < 0)
            {
                Report("지울 프로필이 있다", false);
                return;
            }

            string fileName = "profile" + target + "_meta.json";
            Report("지우기 전에는 메타 파일이 있다", storage.Exists(fileName));

            ProfileCardView card = FindCardFor(target);
            Report("지울 카드를 찾았다", card != null);

            if (card == null)
            {
                return;
            }

            Button deleteButton = GetField<Button>(card, "_deleteButton");
            Report("카드에 삭제 버튼이 있다", deleteButton != null);

            if (deleteButton == null)
            {
                return;
            }

            deleteButton.onClick.Invoke();

            // 팝업이 방금 찍어 낸 버튼 칸은 아직 Awake 를 받지 못했다.
            // 빠뜨리면 버튼에 귀가 없어 눌러도 안 닫히고, 팝업이 열린 채 남아
            // 뒤에 오는 검사가 전부 "겹쳐 뜬 화면이 있다"로 읽힌다.
            AwakenNew();

            Report("삭제를 누르면 확인 팝업이 뜬다", popup.IsOpen);
            Report("확인 팝업에 버튼이 둘",
                popup.Current != null && popup.Current.ButtonCount == 2,
                popup.Current != null ? popup.Current.ButtonCount : 0);

            if (!popup.IsOpen)
            {
                return;
            }

            CheckPopupButtonsAreVisible(popup, "프로필 삭제 확인");

            // 확인을 누른다.
            PressPopupButton(popup, "confirm");

            Report("확인하면 팝업이 닫힌다", !popup.IsOpen);
            Report("그 자리가 빈 자리로 돌아간다",
                screen.List.Get(target).IsEmpty, target);
            Report("메타 파일이 지워졌다", !storage.Exists(fileName));
            Report("남은 프로필은 그대로", storage.Exists("profile0_meta.json"));
        }

        /// <summary>
        /// 방 안에서 지도 버튼을 눌러 본다.
        ///
        /// **방이 끝나면 안 된다.**
        /// 흐름이 화면을 치울 때 성소의 `Exit` 을 불렀더니 `Exited` 가 울려
        /// 방 완료가 저장되고 보상까지 열렸다. 깬 적도 없는 방이 깬 것이 됐다.
        /// 치우는 것은 `Close` 고 방을 끝내는 것은 나가기를 눌렀을 때뿐이다.
        ///
        /// 누르고 나서 그 방에 다시 들어가 보던 자리를 이어 가는지도 본다.
        /// 새로 만들면 성소 진열이 다시 굴려져 지도를 들락거리며 고를 수 있게 된다.
        /// </summary>
        private static void CheckMapButtonInRoom(
            GameFlowController flow, SaveService save, MapNode node,
            GameScreen expected, string title)
        {
            MapScreenController map = Object.FindAnyObjectByType<MapScreenController>(FindObjectsInactive.Include);
            TopBarButton mapButton = FindTopBarButton(TopBarButtonKind.Map);
            Report(title + " 방에서 지도 버튼을 찾았다", mapButton != null && map != null);

            if (mapButton == null || map == null)
            {
                return;
            }

            // 전투 대역은 팝업으로 떠 있다. 팝업이 떠 있으면 지도 버튼이 안 눌려야 한다.
            if (expected == GameScreen.Combat)
            {
                Report(title + " 방은 팝업이 떠 있어 지도 버튼이 꺼진다", !MapButtonUsable(flow, mapButton));
                return;
            }

            // 성소라면 지금 진열과 야영 기록을 적어 둔다. 지도를 보고 돌아와도 같은 것이어야 한다.
            SanctumController sanctum = Object.FindAnyObjectByType<SanctumController>();
            SanctumState stateBefore = expected == GameScreen.Sanctum && sanctum != null
                ? sanctum.State
                : null;

            CheckMapPeek(flow, save, map, mapButton, title + " 방");

            if (stateBefore != null && sanctum != null)
            {
                Report("성소에서 지도를 보고 돌아와도 보던 진열 그대로",
                    ReferenceEquals(sanctum.State, stateBefore));
            }

            // 방 진행 중 + 현재 빌드 열림. 지도를 누르면 현재 빌드를 닫고 지도를 띄운다.
            flow.OpenCurrentBuild();
            CurrentBuildScreenController build = Object.FindAnyObjectByType<CurrentBuildScreenController>();
            Report(title + " 방에서 현재 빌드를 열면 지도 버튼이 켜져 있다", MapButtonUsable(flow, mapButton));
            PressMapButton(mapButton);
            Report(title + " 방에서 현재 빌드 위로 지도를 누르면 현재 빌드가 닫힌다", build == null || !build.IsOpen);
            Report(title + " 방에서 현재 빌드 위로 지도를 누르면 지도가 뜬다", flow.IsMapPeeking && map.IsOpen);
            Report(title + " 방에서 지도를 띄워도 플레이 시간이 흐른다", !(bool)InvokeResult(flow, "IsClockPaused"));

            // 거울로 같다. 지도를 띄운 중에 현재 빌드를 누르면 지도를 걷고 현재 빌드를 연다. 2026년 10월 6일 원재가 정했다.
            flow.OpenCurrentBuild();
            Report(title + " 방에서 지도 위로 현재 빌드를 누르면 지도가 걷힌다", !flow.IsMapPeeking && !map.IsOpen);
            Report(title + " 방에서 지도 위로 현재 빌드를 누르면 현재 빌드가 뜬다", build != null && build.IsOpen);
            Report(title + " 방에서 현재 빌드를 열어도 플레이 시간이 흐른다", !(bool)InvokeResult(flow, "IsClockPaused"));

            if (build != null)
            {
                build.Close();
            }

            Report(title + " 방으로 돌아온다",
                !flow.IsMapPeeking && !map.IsOpen && flow.Screen == expected && (build == null || !build.IsOpen));

            // 설정이 떠 있으면 지도 버튼이 안 눌린다.
            SettingsScreenController settings = Object.FindAnyObjectByType<SettingsScreenController>(FindObjectsInactive.Include);
            flow.OpenSettings();
            Report(title + " 방에서 설정이 떠 있으면 지도 버튼이 꺼진다", !MapButtonUsable(flow, mapButton));
            if (settings != null)
            {
                settings.Close();
            }

            Report(title + " 방에서 설정을 닫으면 지도 버튼이 다시 켜진다", MapButtonUsable(flow, mapButton));
        }

        /// <summary>
        /// 방 안에서 저장하고 나간 런을 이어 한다.
        ///
        /// **그 방에 다시 들어가야 한다.** 저장은 방에 들어갈 때 하므로 방 안에서 나가면
        /// 지금 방을 아직 깨지 않은 채로 남는다. 예전에는 이어하면 맵만 열려
        /// 고를 수 있는 다음 방이 없고 그 방으로 돌아갈 길도 없어 런이 막혔다.
        /// </summary>
        /// <summary>
        /// 깨진 런은 팝업을 띄우고 지운다. 버튼은 확인 하나다. 2026년 10월 7일 원재가 정했다.
        /// 프로필을 고를 때 파일을 읽으므로 그 길을 밟는다.
        /// </summary>
        private static void CheckBrokenRunNotice(GameFlowController flow, SaveService save, MemorySaveStorage storage)
        {
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            if (popup == null)
            {
                Report("깨진 런을 볼 팝업이 있다", false);
                return;
            }

            while (popup.IsOpen)
            {
                popup.Dismiss();
            }

            flow.StartNewRun();
            flow.GoToTitle();
            Report("깨진 런을 볼 런 파일이 있다", storage.Exists("profile0_run.json"));

            storage.Put("profile0_run.json", "{ 망가진 런");
            Invoke(flow, "HandleProfileChosen", 0);

            PopupSpec shown = popup.Current;
            Report("깨진 런을 읽으면 팝업이 뜬다", popup.IsOpen);
            Report("팝업 제목이 런 데이터 손상이다",
                shown != null && shown.Title == "런 데이터 손상", shown != null ? shown.Title : "없음");
            Report("팝업 버튼은 확인 하나다",
                shown != null && shown.ButtonCount == 1 && shown.Buttons[0].Label == "확인",
                shown != null ? shown.ButtonCount : -1);
            Report("깨진 런 파일은 지워졌다", !storage.Exists("profile0_run.json"));
            Report("메타도 런이 없다고 적는다", save.Meta != null && !save.Meta.HasSavedRun);
            Report("타이틀의 이어하기가 꺼진다",
                save.GetSavedRunState() == SlotHero.Title.SavedRunState.None, (int)save.GetSavedRunState());

            PressPopupButton(popup, "confirm");
            Report("확인을 누르면 팝업이 닫히고 타이틀에 남는다",
                !popup.IsOpen && flow.Screen == GameScreen.Title, (int)flow.Screen);
        }

        /// <summary>
        /// 저장 파일을 잠시 못 읽으면 팝업을 띄우고 기다렸다 다시 읽는지. 2026년 10월 9일 원재가 정했다.
        ///   - 이어하기에서 메타를 못 읽으면 손상 알림 없이 기다리는 팝업이 뜨고, 읽히면 그대로 이어 한다
        ///   - 끝내 못 읽으면 백업과 런이 온전할 때 메타를 백업으로 되살려 이어 한다
        ///   - 읽지 못한 프로필 칸을 누르면 기다리는 팝업이 뜨고, 그만두기를 누르면 프로필 선택에 남는다
        /// 시간은 흐름의 `_readRetry` 에 직접 흘린다.
        /// </summary>
        private static void CheckReadRetry(GameFlowController flow, SaveService save, MemorySaveStorage storage)
        {
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            ReadRetry retry = GetField<ReadRetry>(flow, "_readRetry");
            if (popup == null || retry == null)
            {
                Report("다시 읽기를 볼 팝업과 타이머가 있다", false);
                return;
            }

            const string metaName = "profile0_meta.json";
            const string backupName = "profile0_meta.backup.json";
            const string waitingTitle = "저장 파일을 기다리는 중";
            float delay = save.Config.ReadRetryDelaySeconds;

            while (popup.IsOpen)
            {
                popup.Dismiss();
            }

            // 1. 이어하기에서 메타를 잠시 못 읽는다. 읽히면 그대로 이어 한다.
            flow.StartNewRun();
            Invoke(flow, "HandleEndRunConfirmClosed", "save");
            storage.Unreadable.Add(metaName);
            Invoke(flow, "ContinueRun");

            Report("메타를 못 읽으면 타이틀에서 기다린다", flow.Screen == GameScreen.Title && retry.IsWaiting, (int)flow.Screen);
            Report("기다리는 팝업이 뜬다", popup.IsOpen && popup.Current != null && popup.Current.Title == waitingTitle,
                popup.Current != null ? popup.Current.Title : "없음");
            Report("손상이 아니라 읽지 못함이다",
                save.GetSavedRunState() == SlotHero.Title.SavedRunState.Unreadable, (int)save.GetSavedRunState());

            storage.Unreadable.Clear();
            retry.Tick(delay);
            Report("읽히면 기다리는 팝업을 거두고 이어 한다",
                !retry.IsWaiting && flow.Context != null && flow.Screen != GameScreen.Title
                && (popup.Current == null || popup.Current.Title != waitingTitle), (int)flow.Screen);

            // 2. 끝내 못 읽는다. 백업과 런이 온전하므로 백업으로 되살려 이어 한다.
            while (popup.IsOpen)
            {
                popup.Dismiss();
            }

            Invoke(flow, "HandleEndRunConfirmClosed", "save");
            storage.Unreadable.Add(metaName);
            Invoke(flow, "ContinueRun");

            for (int i = 0; i < save.Config.ReadRetryCount + 1 && retry.IsWaiting; i++)
            {
                retry.Tick(delay);
            }

            Report("끝내 못 읽으면 백업으로 되살려 이어 한다",
                !retry.IsWaiting && flow.Context != null && flow.Screen != GameScreen.Title
                && save.Meta != null && save.Meta.HasSavedRun, (int)flow.Screen);
            storage.Unreadable.Clear();

            while (popup.IsOpen)
            {
                popup.Dismiss();
            }

            // 3. 읽지 못한 프로필 칸을 누른다. 그만두기를 누르면 프로필 선택에 남는다.
            flow.EndRun(false, "검사");
            flow.GoToProfileSelect();
            storage.Unreadable.Add(metaName);
            storage.Unreadable.Add(backupName);
            flow.GoToProfileSelect();
            Invoke(flow, "ShowProfileBrokenNotice", 0);

            Report("읽지 못한 칸을 누르면 기다리는 팝업이 뜬다",
                retry.IsWaiting && popup.Current != null && popup.Current.Title == waitingTitle,
                popup.Current != null ? popup.Current.Title : "없음");

            PressPopupButton(popup, "stop");
            Report("그만두기를 누르면 프로필 선택에 남는다",
                !retry.IsWaiting && flow.Screen == GameScreen.ProfileSelect && !popup.IsOpen, (int)flow.Screen);

            storage.Unreadable.Clear();

            // 4. 게임을 켤 때 런 파일이 잠겨 있다. 다 기다리고 포기하면 타이틀에서 다시 기다리지 않는다.
            if (save.SelectProfile(0))
            {
                flow.GoToTitle();
                flow.StartNewRun();
                Invoke(flow, "HandleEndRunConfirmClosed", "save");
            }

            storage.Unreadable.Add("profile0_run.json");
            Invoke(flow, "Start");
            Report("켤 때 런 파일이 잠겨 있으면 기다린다", retry.IsWaiting, 0);

            for (int i = 0; i < save.Config.ReadRetryCount + 1 && retry.IsWaiting; i++)
            {
                retry.Tick(delay);
            }

            Report("포기한 뒤 타이틀에서는 다시 기다리지 않는다",
                !retry.IsWaiting && flow.Screen == GameScreen.Title, (int)flow.Screen);

            while (popup.IsOpen)
            {
                popup.Dismiss();
            }

            storage.Unreadable.Clear();
            flow.GoToTitle();
        }

        private static void CheckContinueInsideRoom(GameFlowController flow, SaveService save, MapScreenController map)
        {
            MapNode eventNode = null;
            for (int i = 0; i < map.Map.Nodes.Count; i++)
            {
                if (map.Map.Nodes[i].RoomType == RoomType.Event)
                {
                    eventNode = map.Map.Nodes[i];
                    break;
                }
            }

            if (eventNode == null)
            {
                Report("이어하기를 볼 이벤트 방이 있다", false);
                return;
            }

            Invoke(flow, "EnterRoom", eventNode);

            // 방 안에서 무언가를 얻는다. 저장하고 나갔다 이어하면 이것은 남지 않아야 한다.
            // 남으면 방에 처음부터 다시 들어가 한 번 더 받을 수 있다.
            int goldAtEntry = save.Run.Status.Gold;
            flow.Context.GainGold(77);

            // 방 안에서 5분을 놀았다고 친다. 플레이 시간도 저장한 시점의 값을 따른다.
            // 2026년 10월 7일 원재가 "저장 후 진행한 부분은 따로 시간을 더하지 않는다" 고 정했다.
            int timeAtEntry = save.Run.Status.ElapsedSeconds;
            save.Run.Status.ElapsedSeconds = timeAtEntry + 300;

            // 런 종료 팝업의 "저장하고 나가기" 와 같은 길이다.
            Invoke(flow, "HandleEndRunConfirmClosed", "save");
            Report("방 안에서 저장하고 나가면 타이틀로 간다", flow.Screen == GameScreen.Title, (int)flow.Screen);

            save.SelectProfile(0);
            Report("방 안에서 나간 런은 아직 그 방을 깨지 않았다",
                save.Run != null && save.Run.CurrentNodeId == eventNode.Id && !save.Run.CurrentRoomCleared);
            Report("방 안에서 얻은 것은 방에 들어갈 때의 저장으로 돌아간다",
                save.Run != null && save.Run.Status.Gold == goldAtEntry,
                save.Run != null ? save.Run.Status.Gold - goldAtEntry : -1);
            Report("방 안에서 보낸 플레이 시간은 방에 들어갈 때의 값으로 돌아간다",
                save.Run != null && save.Run.Status.ElapsedSeconds == timeAtEntry,
                save.Run != null ? save.Run.Status.ElapsedSeconds - timeAtEntry : -1);

            flow.ContinueRun();
            Report("방 안에서 나간 런을 이어 하면 그 방에 다시 들어간다",
                flow.Screen == GameScreen.Event, (int)flow.Screen);

            RunClock clock = GetField<RunClock>(flow, "_clock");
            Report("이어 한 플레이 시간은 저장한 시점에서 다시 흐른다",
                clock != null && (int)clock.Seconds == timeAtEntry,
                clock != null ? (int)clock.Seconds - timeAtEntry : -1);

            EventScreenController events = Object.FindAnyObjectByType<EventScreenController>(FindObjectsInactive.Include);
            Report("다시 들어간 이벤트 방이 열려 있다", events != null && events.IsOpen);
            Report("다시 들어간 방 뒤로 맵이 닫혀 있다", !map.IsOpen);

            // 그 방을 끝내면 다음 방을 고를 수 있어야 한다. 막히지 않았는지가 핵심이다.
            Invoke(flow, "LeaveRoom");
            List<MapNode> next = new List<MapNode>();
            map.Progress.GetSelectableNodes(map.Map, next);
            Report("이어 한 방을 끝내면 다음 방을 고를 수 있다",
                flow.Screen == GameScreen.Map && next.Count > 0, next.Count);
        }

        /// <summary>
        /// 저장 후 이어하기를 해도 모든 상황이 같게 나오는지. 2026년 10월 6일 원재가 정한 조건이다.
        ///
        /// 방 안에서 "저장하고 나가기" 를 하고 **게임을 끄지 않고** 타이틀에서 바로 이어 하는 길을 밟는다.
        /// 위의 검사는 이어 하기 전에 프로필을 다시 골라 파일을 읽었기 때문에 이 길을 지나치고 있었다.
        /// 예전에는 이 길에서 메모리의 런으로 이어 해서 방 안에서 얻은 골드가 남았다.
        ///
        /// 성소와 이벤트 방마다 세 번을 견준다. 처음 들어갔을 때, 끄지 않고 바로 이어 했을 때,
        /// 파일을 다시 읽어 이어 했을 때다. 진열, 유물 순서 위치, 골드, 체력, 이벤트 첫 화면이 모두 같아야 한다.
        /// </summary>
        private static void CheckContinueRepeatsRoom(GameFlowController flow, SaveService save, MapScreenController map)
        {
            RoomType[] kinds = { RoomType.Sanctum, RoomType.Event };

            for (int k = 0; k < kinds.Length; k++)
            {
                MapNode node = FirstNodeOf(map, kinds[k]);
                string what = kinds[k] == RoomType.Sanctum ? "성소" : "이벤트";

                if (node == null)
                {
                    Report("이어하기 재현을 볼 " + what + " 방이 있다", false);
                    continue;
                }

                Invoke(flow, "EnterRoom", node);
                string first = DescribeRoom(flow);

                // 방 안에서 무언가를 바꾼다. 이어 하면 하나도 남지 않아야 한다.
                flow.Context.GainGold(77);
                if (kinds[k] == RoomType.Sanctum)
                {
                    SanctumController sanctum = GetField<SanctumController>(flow, "_sanctum");
                    if (sanctum != null)
                    {
                        sanctum.UseCamp();
                    }
                }

                Invoke(flow, "HandleEndRunConfirmClosed", "save");
                flow.ContinueRun();
                string resumed = DescribeRoom(flow);
                Report(what + " 방에서 저장하고 끄지 않고 이어 해도 처음과 같다", resumed == first, resumed + " / " + first);

                Invoke(flow, "HandleEndRunConfirmClosed", "save");
                save.SelectProfile(0);
                flow.ContinueRun();
                string reloaded = DescribeRoom(flow);
                Report(what + " 방에서 저장하고 껐다 켜서 이어 해도 처음과 같다", reloaded == first, reloaded + " / " + first);

                Invoke(flow, "LeaveRoom");

                // 보상 화면이 떴으면 닫고 맵으로 돌아간다. 다음 검사는 맵에서 시작한다.
                RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>(FindObjectsInactive.Include);
                if (flow.Screen == GameScreen.Reward && reward != null)
                {
                    reward.Skip();
                }
            }
        }

        /// <summary>맵에서 그 종류의 첫 방.</summary>
        private static MapNode FirstNodeOf(MapScreenController map, RoomType kind)
        {
            for (int i = 0; i < map.Map.Nodes.Count; i++)
            {
                if (map.Map.Nodes[i].RoomType == kind)
                {
                    return map.Map.Nodes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 지금 방의 상태를 글 한 줄로 적는다. 이어하기 전후를 견주는 데 쓴다.
        /// 골드, 체력, 유물 순서 위치와 성소 진열 또는 이벤트 첫 화면을 담는다.
        /// </summary>
        private static string DescribeRoom(GameFlowController flow)
        {
            List<string> parts = new List<string>();
            parts.Add("화면 " + flow.Screen);

            if (flow.Context != null)
            {
                parts.Add("골드 " + flow.Context.Run.Status.Gold);
                parts.Add("체력 " + flow.Context.Run.Status.Health);
            }

            // 2026년 10월 8일부터 유물 순서는 흐름에서 떼어 낸 `RunSession` 이 든다.
            RunSession session = GetField<RunSession>(flow, "_session");
            RunRelicPool pool = session != null ? session.RelicPool : null;
            parts.Add("유물 위치 " + (pool != null ? pool.TableIndex + "-" + pool.NextIndex : "없음"));

            SanctumController sanctum = GetField<SanctumController>(flow, "_sanctum");
            if (flow.Screen == GameScreen.Sanctum && sanctum != null && sanctum.State != null)
            {
                parts.Add("야영 " + sanctum.State.CampUsedCount);
                foreach (MerchantSlot slot in sanctum.State.Stock.AllSlots())
                {
                    parts.Add(slot.Item.Id + ":" + slot.Price);
                }
            }

            EventScreenController events = Object.FindAnyObjectByType<EventScreenController>(FindObjectsInactive.Include);
            if (flow.Screen == GameScreen.Event && events != null && events.Page != null)
            {
                parts.Add(events.Page.BodyText);
                for (int i = 0; i < events.Page.Choices.Count; i++)
                {
                    parts.Add(events.Page.Choices[i].Label);
                }
            }

            return string.Join(" | ", parts);
        }

        /// <summary>
        /// 보상 카드를 고르고, 바꾸고, 풀고, 아래 버튼으로 받는다. 2026년 10월 4일에 원재가 정한 방식이다.
        ///
        /// 카드를 누르는 것은 고르기만 한다. 화면은 닫히지 않는다.
        /// 아래 버튼 글은 고른 것이 없으면 "아이템 선택 건너뛰기", 있으면 "보상 받기" 다.
        /// 카드는 진짜 버튼을 눌러 본다. 조종기 함수를 직접 부르면 카드에 귀가 달렸는지를 지나친다.
        /// </summary>
        /// <summary>
        /// 물건에 커서를 올리면 아이템 상세 오버레이가 뜨고 벗어나면 사라지는지.
        /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 아이템 상세 팝업이다.
        ///
        /// 2026년 10월 5일 원재가 "아이템 설명 오버레이가 안 보인다" 고 짚었다.
        /// 오버레이 코드는 있었는데 **씬에 놓지도 않았고 아무도 부르지 않았다.**
        /// 그래서 칸의 커서 처리기를 직접 불러 진짜 길을 밟는다.
        /// </summary>
        private static void CheckItemDetailHover(string where, Component target, bool capture)
        {
            ItemDetailView detail = Object.FindAnyObjectByType<ItemDetailView>(FindObjectsInactive.Include);
            Report("아이템 상세 오버레이가 씬에 있다", detail != null);

            IPointerEnterHandler enter = target as IPointerEnterHandler;
            IPointerExitHandler exit = target as IPointerExitHandler;
            if (detail == null || enter == null || exit == null)
            {
                Report(where + " 에 커서 처리기가 있다", false);
                return;
            }

            AwakenNew();
            detail.Hide();

            PointerEventData data = new PointerEventData(EventSystem.current);
            enter.OnPointerEnter(data);

            TMP_Text name = GetField<TMP_Text>(detail, "_nameLabel");
            Report(where + " 에 커서를 올리면 아이템 상세가 뜬다", detail.IsShowing);
            Report(where + " 의 아이템 상세에 이름이 적힌다", name != null && !string.IsNullOrEmpty(name.text));

            if (capture && detail.IsShowing)
            {
                UiSceneBuilder.SyncShadows();
                Canvas.ForceUpdateCanvases();
                CapturePreviewScenes.CaptureOpenScene("게임 아이템 상세 " + where);
            }

            exit.OnPointerExit(data);
            Report(where + " 에서 커서가 벗어나면 아이템 상세가 사라진다", !detail.IsShowing);
        }

        /// <summary>
        /// 현재 빌드 화면에서 휠을 굴리면 문양 줄이 실제로 움직이는지.
        ///
        /// 2026년 10월 5일 원재가 문양이 세 줄을 넘으면 아래를 볼 수 없다고 짚었다.
        /// 스크롤의 휠 거리가 유니티 기본값 1 이었는데 그때 좌표 배율이 12 라 한 칸에 1/12 픽셀만 움직였다.
        /// 칸 위에서 휠을 굴린 셈 치고 이벤트를 위로 올려 보낸다. 칸 사이 빈 곳도 본다.
        /// </summary>
        private static void CheckBuildScroll(BuildSlotView[] slots)
        {
            ScrollRect[] scrolls = Object.FindObjectsByType<ScrollRect>(FindObjectsInactive.Include);
            int slow = 0;
            for (int i = 0; i < scrolls.Length; i++)
            {
                if (scrolls[i].scrollSensitivity < UiScale.Px(20f))
                {
                    slow++;
                    Debug.LogWarning("  스크롤  휠 거리가 너무 작다: " + scrolls[i].name + " " + scrolls[i].scrollSensitivity);
                }
            }

            Report("모든 스크롤의 휠 한 칸이 20 픽셀 이상 움직인다", slow, 0);

            BuildSlotView symbol = null;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Entry.IsValid && slots[i].gameObject.activeInHierarchy
                    && slots[i].transform.parent != null && slots[i].transform.parent.name == "SymbolLayer")
                {
                    symbol = slots[i];
                    break;
                }
            }

            ScrollRect scroll = symbol != null ? symbol.GetComponentInParent<ScrollRect>() : null;
            Report("현재 빌드 문양 칸이 스크롤 안에 있다", scroll != null);
            if (scroll == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            Report("문양이 많으면 스크롤 내용이 창보다 길다",
                scroll.content.rect.height > scroll.viewport.rect.height + 1f,
                Mathf.RoundToInt(scroll.content.rect.height / UiScale.Factor));

            float before = scroll.content.anchoredPosition.y;
            PointerEventData wheel = new PointerEventData(EventSystem.current);
            wheel.scrollDelta = new Vector2(0f, -1f);
            ExecuteEvents.ExecuteHierarchy(symbol.gameObject, wheel, ExecuteEvents.scrollHandler);
            float moved = scroll.content.anchoredPosition.y - before;
            Report("문양 칸 위에서 휠 한 칸이면 아래로 20 픽셀 넘게 내려간다",
                moved >= UiScale.Px(20f), Mathf.RoundToInt(moved / UiScale.Factor));

            Graphic catcher = scroll.viewport.GetComponent<Graphic>();
            Report("칸 사이 빈 곳도 휠을 받는다", catcher != null && catcher.raycastTarget);

            Report("문양 줄 이름도 스크롤과 함께 움직인다",
                GetField<TMP_Text>(Object.FindAnyObjectByType<CurrentBuildScreenView>(), "_symbolLabel") != null
                && GetField<TMP_Text>(Object.FindAnyObjectByType<CurrentBuildScreenView>(), "_symbolLabel").transform.IsChildOf(scroll.content));

            scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x, before);
        }

        private static void CheckRewardSelection(RewardScreenController reward, string title)
        {
            RewardScreenView view = GetField<RewardScreenView>(reward, "_view");
            RewardVisualConfig visual = view != null ? GetField<RewardVisualConfig>(view, "_visual") : null;

            if (view == null || visual == null || view.Cards.Count == 0)
            {
                Report(title + " 보상 화면의 카드를 찾았다", false);
                return;
            }

            // 카드 칸은 보상을 열 때 새로 찍어 낸다. 편집 모드에서는 그 `Awake` 가 불리지 않아
            // 버튼에 귀가 달리지 않는다. 플레이 모드에서는 유니티가 부른다.
            AwakenNew();

            CheckItemDetailHover(title + " 보상 카드", view.Cards[0], true);

            Report(title + " 처음에는 고른 것이 없다", reward.SelectedCard == null);
            Report(title + " 처음 아래 버튼은 아이템 선택 건너뛰기", view.ConfirmText == visual.SkipText, 0);

            RewardCardView first = view.Cards[0];
            PressCard(first);

            Report(title + " 카드를 눌러도 화면이 닫히지 않는다", reward.IsOpen);
            Report(title + " 누른 카드가 골라진다",
                reward.SelectedCard != null && reward.SelectedCard.Id == first.Card.Id);
            Report(title + " 고른 카드에 테두리가 생긴다", CardBorderShown(first));
            Report(title + " 고르면 아래 버튼이 보상 받기", view.ConfirmText == visual.ConfirmText, 0);
            CapturePreviewScenes.CaptureOpenScene("게임 4 " + title + " 보상 고름");

            if (view.Cards.Count > 1)
            {
                RewardCardView second = view.Cards[1];
                PressCard(second);

                Report(title + " 다른 카드를 누르면 고른 것이 바뀐다",
                    reward.SelectedCard != null && reward.SelectedCard.Id == second.Card.Id);
                Report(title + " 바뀌면 앞 카드의 테두리가 사라진다",
                    !CardBorderShown(first) && CardBorderShown(second));

                PressCard(second);
                Report(title + " 고른 카드를 다시 누르면 풀린다", reward.SelectedCard == null);
                Report(title + " 풀리면 테두리가 사라진다", !CardBorderShown(second));
                Report(title + " 풀리면 아래 버튼이 다시 아이템 선택 건너뛰기", view.ConfirmText == visual.SkipText, 0);
            }

            // 첫 카드를 골라 아래 버튼으로 받는다.
            PressCard(first);
            Button confirm = GetField<Button>(view, "_skipButton");
            if (confirm != null)
            {
                confirm.onClick.Invoke();
            }

            Report(title + " 아래 버튼을 누르면 화면이 닫힌다", !reward.IsOpen);
        }

        /// <summary>보상 카드의 진짜 버튼을 누른다.</summary>
        private static void PressCard(RewardCardView card)
        {
            Button button = GetField<Button>(card, "_button");
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }

        private static bool CardBorderShown(RewardCardView card)
        {
            Image border = GetField<Image>(card, "_border");
            return border != null && border.gameObject.activeSelf;
        }

        /// <summary>
        /// 보상 카드를 고르기 전에 저장하고 나간 런을 이어 한다.
        ///
        /// **보상 화면이 다시 떠야 한다.** 2026년 10월 5일에 원재가 정했다.
        /// 예전에는 방 완료만 저장되어 이어하면 맵이 열리고 카드는 사라졌다.
        /// **모든 보상은 받을 때 한 번에 받는다.** 2026년 10월 6일 원재가 정했다.
        /// 보상 화면을 여는 것만으로는 아무것도 들어오지 않고, 아래 버튼을 누를 때 골드, 오버킬, 카드가 함께 들어온다.
        /// </summary>
        private static void CheckContinueAtReward(GameFlowController flow, SaveService save, MapScreenController map)
        {
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>(FindObjectsInactive.Include);

            MapNode fight = null;
            for (int i = 0; i < map.Map.Nodes.Count; i++)
            {
                if (map.Map.Nodes[i].RoomType == RoomType.Normal)
                {
                    fight = map.Map.Nodes[i];
                    break;
                }
            }

            if (fight == null || popup == null || reward == null)
            {
                Report("보상 이어하기를 볼 전투 방과 화면이 있다", false);
                return;
            }

            // 오버킬이 난 전투로 만든다. 오버킬 골드는 시드로 다시 만들 수 없어 저장해야 한다. 2026년 10월 6일 원재가 정했다.
            const int overkill = 7;
            PlaceholderCombatEntry combat = Object.FindAnyObjectByType<PlaceholderCombatEntry>(FindObjectsInactive.Include);
            FieldInfo overkillField = typeof(PlaceholderCombatEntry).GetField(
                "_victoryOverkillGold", BindingFlags.NonPublic | BindingFlags.Instance);
            if (combat != null && overkillField != null)
            {
                overkillField.SetValue(combat, overkill);
            }

            // 전투 대역 팝업에서 이긴 것으로 고른다. 방을 깨고 보상 화면이 뜬다.
            int goldAtEntry = save.Run.Status.Gold;
            Invoke(flow, "EnterRoom", fight);
            AwakenNew();
            PressPopupButton(popup, "victory");
            AwakenNew();

            Report("전투를 이기면 보상 화면이 뜬다", flow.Screen == GameScreen.Reward, (int)flow.Screen);

            // 전투 골드는 이기는 순간 주지 않고 보상 기본 골드에 합친다. 2026년 10월 6일 원재가 정했다.
            FieldInfo victoryGoldField = typeof(PlaceholderCombatEntry).GetField(
                "_victoryGold", BindingFlags.NonPublic | BindingFlags.Instance);
            int combatGold = combat != null && victoryGoldField != null ? (int)victoryGoldField.GetValue(combat) : 0;
            // 2026년 10월 8일부터 보상 정산은 흐름에서 떼어 낸 `RoomRewardFlow` 가 한다.
            RoomRewardFlow rewards = GetField<RoomRewardFlow>(flow, "_rewards");
            RewardMaker maker = rewards != null ? rewards.Maker : null;
            RewardOffer ruleOnly = maker != null ? maker.Make(save.Run.Seed, fight, save.Run) : null;
            Report("전투를 이겨도 골드가 바로 들어오지 않는다",
                save.Run.Status.Gold == goldAtEntry, save.Run.Status.Gold - goldAtEntry);
            Report("전투 골드가 저장된다", combatGold > 0 && save.Run.RewardCombatGold == combatGold, save.Run.RewardCombatGold);
            Report("전투 골드가 보상 기본 골드에 합쳐진다",
                ruleOnly != null && reward.Offer != null && reward.Offer.BaseGold == ruleOnly.BaseGold + combatGold,
                reward.Offer != null ? reward.Offer.BaseGold : -1);
            Report("보상이 남아 있다고 적힌다", save.Run.RewardPending);
            Report("오버킬 골드가 보상 화면에 적힌다",
                reward.Offer != null && reward.Offer.OverkillGold == overkill,
                reward.Offer != null ? reward.Offer.OverkillGold : -1);
            Report("오버킬 골드가 저장된다", save.Run.RewardOverkillGold == overkill, save.Run.RewardOverkillGold);

            // 화면을 열었을 때의 골드. 전투 대역이 이기면서 준 골드까지 들어간 값이다. 보상 골드는 아직 아니다.
            int goldBefore = save.Run.Status.Gold;
            int rewardGold = reward.Offer != null ? reward.Offer.TotalGold : 0;
            int ownedBefore = save.Run.Owned.TotalSymbolCount + save.Run.Owned.Relics.Count + save.Run.Owned.CoinIds.Count;

            // 보상 화면에서 저장하고 나간다.
            Invoke(flow, "HandleEndRunConfirmClosed", "save");
            Report("보상 화면에서 저장하고 나가면 타이틀로 간다", flow.Screen == GameScreen.Title, (int)flow.Screen);

            save.SelectProfile(0);
            Report("다시 읽어도 보상이 남아 있다", save.Run != null && save.Run.RewardPending);
            Report("다시 읽어도 오버킬 골드가 남아 있다",
                save.Run != null && save.Run.RewardOverkillGold == overkill,
                save.Run != null ? save.Run.RewardOverkillGold : -1);

            flow.ContinueRun();
            RewardOffer remade = rewards != null ? rewards.Make(flow.Context, fight) : null;
            Report("이어 해서 다시 만든 보상에 오버킬 골드가 그대로 얹힌다",
                remade != null && remade.OverkillGold == overkill,
                remade != null ? remade.OverkillGold : -1);
            Report("이어 하면 보상 화면이 다시 뜬다",
                flow.Screen == GameScreen.Reward && reward.IsOpen, (int)flow.Screen);
            Report("다시 뜬 보상 뒤로 맵이 닫혀 있다", !map.IsOpen);
            Report("보상을 받기 전에는 골드가 들어오지 않는다",
                save.Run.Status.Gold == goldBefore, save.Run.Status.Gold - goldBefore);
            Report("다시 뜬 보상에 골드가 그대로 있다",
                reward.Offer != null && reward.Offer.TotalGold == rewardGold && rewardGold > 0,
                reward.Offer != null ? reward.Offer.TotalGold : -1);

            if (reward.Offer == null || !reward.Offer.HasCards)
            {
                Report("다시 뜬 보상에 고를 카드가 있다", false);
                return;
            }

            reward.Choose(reward.Offer.Cards[0].Id);

            int ownedAfter = save.Run.Owned.TotalSymbolCount + save.Run.Owned.Relics.Count + save.Run.Owned.CoinIds.Count;
            Report("다시 뜬 보상에서 고른 카드가 들어온다", ownedAfter == ownedBefore + 1, ownedAfter - ownedBefore);
            Report("받을 때 골드와 오버킬이 한 번에 들어온다",
                save.Run.Status.Gold == goldBefore + rewardGold, save.Run.Status.Gold - goldBefore);
            Report("고르고 나면 남은 보상이 없다", !save.Run.RewardPending);
            Report("고르고 나면 오버킬 골드가 비워진다", save.Run.RewardOverkillGold == 0, save.Run.RewardOverkillGold);
            Report("고르고 나면 전투 골드가 비워진다", save.Run.RewardCombatGold == 0, save.Run.RewardCombatGold);

            if (combat != null && overkillField != null)
            {
                overkillField.SetValue(combat, 0);
            }
            Report("고르고 나면 맵으로 간다", flow.Screen == GameScreen.Map, (int)flow.Screen);

            // 한 번 더 이어해도 보상이 다시 뜨지 않는다.
            Invoke(flow, "HandleEndRunConfirmClosed", "save");
            save.SelectProfile(0);
            flow.ContinueRun();
            Report("받은 뒤에 이어하면 보상이 다시 뜨지 않는다", flow.Screen == GameScreen.Map, (int)flow.Screen);

            // 골드만 있는 보상도 보상 화면이 뜨고 아래 버튼으로 받는다. 2026년 10월 6일 원재가 정했다.
            int goldOnlyBefore = save.Run.Status.Gold;
            Invoke(flow, "OpenReward", RewardOffer.Gold(12, 0));
            RewardScreenView rewardView = GetField<RewardScreenView>(reward, "_view");
            Report("골드만 있는 보상도 보상 화면이 뜬다", reward.IsOpen && flow.Screen == GameScreen.Reward, (int)flow.Screen);
            Report("골드만 있으면 아래 버튼이 보상 받기다", rewardView != null && rewardView.ConfirmText == "보상 받기");
            Report("골드만 있는 보상도 화면이 뜨기만 해서는 골드가 들어오지 않는다",
                save.Run.Status.Gold == goldOnlyBefore, save.Run.Status.Gold - goldOnlyBefore);
            reward.Confirm();
            Report("골드만 있는 보상을 받으면 골드가 들어온다",
                save.Run.Status.Gold == goldOnlyBefore + 12, save.Run.Status.Gold - goldOnlyBefore);
            Report("골드만 있는 보상을 받으면 맵으로 간다", flow.Screen == GameScreen.Map && !reward.IsOpen, (int)flow.Screen);
        }

        /// <summary>
        /// 35 도전자 를 실제 흐름으로 끝까지 해 본다. 이벤트 기획서 3장 도전 이벤트 공통 규칙.
        ///   화면 2 에서 고른 부정 효과가 전투에 넘어간다
        ///   이기면 이벤트 마무리 화면으로 돌아가고, 방을 나갈 때 보상을 두 번 고른다
        ///   두 번째 고르기는 카드만 있고, 그 앞에서 꺼졌다 이어도 그 고르기가 다시 뜬다
        /// 2026년 10월 8일 외부 검토가 부정 효과도 카드 두 번도 이어지지 않았다고 짚어 더했다.
        /// 이벤트 목록을 검사 동안만 도전자 하나로 줄이고 끝에 되돌린다.
        /// </summary>
        private static void CheckChallengeEvent(GameFlowController flow, SaveService save, MapScreenController map)
        {
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>(FindObjectsInactive.Include);
            RunEventConfig config = GetField<RunEventConfig>(flow, "_eventConfig");
            EventRoomFlow events = GetField<EventRoomFlow>(flow, "_events");
            MapNode node = FirstNodeOf(map, RoomType.Event);
            EventScript challenger = config != null ? config.Find("challenger") : null;

            if (popup == null || reward == null || events == null || node == null || challenger == null || save.Run == null)
            {
                Report("도전자 를 볼 이벤트 방과 화면이 있다", false);
                return;
            }

            List<EventScript> kept = new List<EventScript>(config.Events);
            config.Events.Clear();
            config.Events.Add(challenger);

            try
            {
                Invoke(flow, "EnterRoom", node);
                AwakenNew();
                Report("도전자 이벤트가 열린다", flow.Screen == GameScreen.Event, (int)flow.Screen);

                Invoke(flow, "HandleEventChoice", "challenger", "step");
                Invoke(flow, "HandleEventChoice", "challenger", "penalty_0");
                Invoke(flow, "HandleEventChoice", "challenger", "fight");
                AwakenNew();

                CombatOptions options = events.CombatOptions;
                bool known = false;
                List<EventCombatPenalty> list = config.CombatPenalties;
                for (int i = 0; i < list.Count; i++)
                {
                    known |= list[i].Id == options.PenaltyId && list[i].Text == options.PenaltyText;
                }

                Report("싸우면 전투가 열린다", flow.Screen == GameScreen.Combat, (int)flow.Screen);
                Report("도전자 전투에 고른 부정 효과가 넘어간다", options.HasPenalty && known, options.PenaltyId);
                Report("도전자 전투는 이벤트 전투이고 보상이 두 배다", options.FromEvent && options.DoubleReward);

                int goldBefore = save.Run.Status.Gold;
                PressPopupButton(popup, "victory");
                AwakenNew();

                Report("이기면 이벤트 마무리 화면으로 돌아간다", flow.Screen == GameScreen.Event, (int)flow.Screen);
                Report("이기면 보상 고르기 두 번이 쌓인다", save.Run.RewardCardRounds == 2, save.Run.RewardCardRounds);

                Invoke(flow, "HandleEventChoice", "challenger", "leave");
                AwakenNew();

                Report("이벤트를 마치면 첫 보상 화면이 뜬다",
                    flow.Screen == GameScreen.Reward && reward.IsOpen && reward.Offer != null && reward.Offer.HasCards,
                    (int)flow.Screen);

                if (reward.Offer == null || !reward.Offer.HasCards)
                {
                    return;
                }

                int firstGold = reward.Offer.TotalGold;
                reward.Choose(reward.Offer.Cards[0].Id);
                AwakenNew();

                Report("첫 보상에서 두 배 전투 골드를 받는다",
                    firstGold > 0 && save.Run.Status.Gold == goldBefore + firstGold, save.Run.Status.Gold - goldBefore);
                Report("두 번째 보상 화면이 이어 뜬다",
                    flow.Screen == GameScreen.Reward && reward.IsOpen && reward.Offer != null && reward.Offer.HasCards,
                    (int)flow.Screen);
                Report("두 번째 보상은 카드만 있다", reward.Offer != null && reward.Offer.TotalGold == 0,
                    reward.Offer != null ? reward.Offer.TotalGold : -1);
                Report("두 번째 보상 앞에서도 보상이 남아 있다고 적힌다", save.Run.RewardPending);

                string secondCards = string.Empty;
                for (int i = 0; reward.Offer != null && i < reward.Offer.Cards.Count; i++)
                {
                    secondCards += reward.Offer.Cards[i].Id + ",";
                }

                // 두 번째 고르기 앞에서 저장하고 나갔다 이어 한다.
                Invoke(flow, "HandleEndRunConfirmClosed", "save");
                save.SelectProfile(0);
                flow.ContinueRun();
                AwakenNew();

                string again = string.Empty;
                for (int i = 0; reward.Offer != null && i < reward.Offer.Cards.Count; i++)
                {
                    again += reward.Offer.Cards[i].Id + ",";
                }

                Report("두 번째 보상 앞에서 이어 하면 그 보상이 다시 뜬다",
                    flow.Screen == GameScreen.Reward && reward.IsOpen && again == secondCards && reward.Offer.TotalGold == 0,
                    (int)flow.Screen);

                if (reward.Offer == null || !reward.Offer.HasCards)
                {
                    return;
                }

                reward.Choose(reward.Offer.Cards[0].Id);
                AwakenNew();

                Report("두 번 고르면 맵으로 간다", flow.Screen == GameScreen.Map && !reward.IsOpen, (int)flow.Screen);
                Report("두 번 고르면 남은 보상이 없다",
                    !save.Run.RewardPending && save.Run.RewardCardRounds == 0, save.Run.RewardCardRounds);
            }
            finally
            {
                config.Events.Clear();
                config.Events.AddRange(kept);
            }
        }

        /// <summary>
        /// 유물이 소지 한도까지 찼을 때 보상에서 유물 카드를 고르면 버릴 것을 고르게 하는지.
        /// 포기하면 보상 화면에 남아 다른 카드나 건너뛰기를 고를 수 있고, 고르면 그것을 버리고 받는다. 2026년 10월 9일 원재가 정했다.
        /// </summary>
        private static void CheckRewardWhenFull(GameFlowController flow, SaveService save)
        {
            RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>(FindObjectsInactive.Include);
            ItemPickerScreenController picker = Object.FindAnyObjectByType<ItemPickerScreenController>(FindObjectsInactive.Include);
            RunCatalogConfig catalog = GetField<RunCatalogConfig>(flow, "_catalogConfig");

            if (reward == null || picker == null || catalog == null || flow.Context == null || catalog.Relics.Count < 2)
            {
                Report("가득 찬 채로 받을 보상 화면과 고르기 화면이 있다", false);
                return;
            }

            RunOwnedData owned = save.Run.Owned;
            List<OwnedRelic> keep = new List<OwnedRelic>(owned.Relics);
            int capacity = owned.RelicCapacity;

            // 유물 하나만 가질 수 있게 줄이고 하나를 쥐여 준다. 보상 카드는 가지지 않은 다른 유물이다.
            owned.Relics.Clear();
            owned.RelicCapacity = 1;
            owned.AddRelic(catalog.Relics[0].Id);
            ItemDefinition incoming = catalog.Relics[1];

            RewardOffer offer = RewardOffer.Gold(5, 0);
            offer.Add(RewardCard.Relic(incoming.Id, incoming.DisplayName));
            Invoke(flow, "OpenReward", offer);
            AwakenNew();

            int goldBefore = save.Run.Status.Gold;
            reward.Choose(incoming.Id);
            AwakenNew();

            Report("유물이 가득이면 받기 전에 버릴 유물 고르기가 뜬다",
                picker.IsOpen && picker.Request != null && picker.Request.Title == "버릴 유물 선택", picker.IsOpen ? 1 : 0);
            Report("보상을 받기 전에는 아무것도 들어오지 않는다",
                save.Run.Status.Gold == goldBefore && owned.Relics.Count == 1, save.Run.Status.Gold - goldBefore);

            if (picker.IsOpen && picker.Request != null && picker.Request.Candidates.Count > 0)
            {
                picker.Toggle(picker.Request.Candidates[0]);
                UiSceneBuilder.SyncShadows();
                Canvas.ForceUpdateCanvases();
                CapturePreviewScenes.CaptureOpenScene("게임 보상 가득 버릴 유물 고르기");
                picker.Toggle(picker.Request.Candidates[0]);
            }

            picker.Cancel();
            Report("포기하면 보상 화면에 남는다", reward.IsOpen && flow.Screen == GameScreen.Reward, (int)flow.Screen);
            Report("포기하면 가진 유물이 그대로다", owned.HasRelic(catalog.Relics[0].Id) && !owned.HasRelic(incoming.Id));

            reward.Choose(incoming.Id);
            AwakenNew();
            if (!picker.IsOpen || picker.Request == null || picker.Request.Candidates.Count == 0)
            {
                Report("다시 고르면 버릴 유물 고르기가 뜬다", false);
            }
            else
            {
                picker.Toggle(picker.Request.Candidates[0]);
                picker.Confirm();
                AwakenNew();

                Report("버리면 보상을 받는다", owned.HasRelic(incoming.Id) && !owned.HasRelic(catalog.Relics[0].Id));
                Report("버리고 받아도 한도를 넘지 않는다", owned.Relics.Count == 1, owned.Relics.Count);
                Report("버리고 받으면 골드도 함께 받는다", save.Run.Status.Gold == goldBefore + 5, save.Run.Status.Gold - goldBefore);
                Report("받으면 보상 화면이 닫힌다", !reward.IsOpen);
            }

            owned.Relics.Clear();
            owned.Relics.AddRange(keep);
            owned.RelicCapacity = capacity;
        }

        /// <summary>
        /// 소지 한도가 줄어 넘치면 넘친 만큼 바로 버리게 하는지. 이때는 닫을 수 없다. 2026년 10월 9일 원재가 정했다.
        /// 어떤 조건에서 줄어드는지는 아직 없어 한도를 직접 줄여 본다.
        /// </summary>
        private static void CheckOverflowDiscard(GameFlowController flow, SaveService save)
        {
            ItemPickerScreenController picker = Object.FindAnyObjectByType<ItemPickerScreenController>(FindObjectsInactive.Include);
            ItemPickerScreenView view = Object.FindAnyObjectByType<ItemPickerScreenView>(FindObjectsInactive.Include);
            if (picker == null || view == null || flow.Context == null)
            {
                Report("넘친 것을 버릴 고르기 화면이 있다", false);
                return;
            }

            RunOwnedData owned = save.Run.Owned;
            List<string> keep = new List<string>(owned.CoinIds);
            int capacity = owned.CoinCapacity;

            owned.CoinIds.Clear();
            owned.CoinIds.AddRange(new[] { "넘침_가", "넘침_나", "넘침_다" });
            owned.CoinCapacity = 3;
            flow.Context.ChangeCoinCapacity(-2);

            bool done = false;
            Invoke(flow, "ResolveOverflow", (System.Action)(() => done = true));
            AwakenNew();

            Report("한도가 줄어 넘치면 버릴 코인 고르기가 바로 뜬다", picker.IsOpen && !done, picker.IsOpen ? 1 : 0);
            Report("넘쳐 버릴 때는 닫을 수 없다", !picker.CanCancel);
            Report("넘쳐 버릴 때는 포기 버튼이 없다", !view.CanCancelShown);

            picker.Cancel();
            Report("닫으려 해도 그대로다", picker.IsOpen);

            for (int round = 0; round < 2 && picker.IsOpen && picker.Request != null && picker.Request.Candidates.Count > 0; round++)
            {
                picker.Toggle(picker.Request.Candidates[0]);
                picker.Confirm();
                AwakenNew();
            }

            Report("넘친 만큼 버리면 끝난다", done && !picker.IsOpen && owned.CoinIds.Count == 1, owned.CoinIds.Count);

            owned.CoinIds.Clear();
            owned.CoinIds.AddRange(keep);
            owned.CoinCapacity = capacity;
        }

        /// <summary>
        /// 방 종류마다 보상이 맞게 나오는지. 2026년 10월 6일 원재가 정했다.
        ///   성소는 보상이 없다
        ///   마지막 스테이지가 아닌 보스는 보상을 주고, 받은 뒤 다음 스테이지로 넘어간다
        ///   보스 보상을 받고 다음 스테이지로 넘어가기 전에 꺼져도 이어하면 마저 넘어간다
        ///   마지막 스테이지의 보스는 보상 없이 런이 끝난다
        /// 예전에는 보스면 스테이지와 상관없이 보상 없이 넘어갔다. **이 검사는 런을 끝낸다.**
        /// </summary>
        private static void CheckRewardByRoom(GameFlowController flow, SaveService save, MapScreenController map)
        {
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>(FindObjectsInactive.Include);
            FieldInfo stageCountField = typeof(GameFlowController).GetField(
                "_stageCount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (popup == null || reward == null || save.Run == null || stageCountField == null)
            {
                Report("방별 보상을 볼 화면과 런이 있다", false);
                return;
            }

            // 지금 게임은 2스테이지 이후가 정해지지 않아 스테이지가 하나뿐이다.
            // 마지막이 아닌 보스를 보려고 검사 동안만 셋으로 늘리고 끝에 되돌린다.
            int originalStageCount = (int)stageCountField.GetValue(flow);
            const int stageCount = 3;
            stageCountField.SetValue(flow, stageCount);

            CheckRewardByRoomInner(flow, save, map, popup, reward, stageCount);

            stageCountField.SetValue(flow, originalStageCount);
        }

        private static void CheckRewardByRoomInner(
            GameFlowController flow, SaveService save, MapScreenController map,
            PopupPresenter popup, RewardScreenController reward, int stageCount)
        {

            // 성소는 보상이 없다.
            MapNode sanctumNode = FirstNodeOf(map, RoomType.Sanctum);
            if (sanctumNode != null)
            {
                Invoke(flow, "EnterRoom", sanctumNode);
                Invoke(flow, "LeaveRoom");
                Report("성소를 나가면 보상 화면 없이 맵으로 간다", flow.Screen == GameScreen.Map, (int)flow.Screen);
                Report("성소를 나가면 남은 보상이 없다", !save.Run.RewardPending);
            }

            // 마지막이 아닌 스테이지의 보스.
            MapNode boss = FirstNodeOf(map, RoomType.Boss);
            int stage = save.Run.StageIndex;
            Report("보상을 볼 보스가 마지막 스테이지가 아니다", boss != null && stage < stageCount, stage);

            if (boss == null || stage >= stageCount)
            {
                return;
            }

            Invoke(flow, "EnterRoom", boss);
            AwakenNew();
            PressPopupButton(popup, "victory");
            AwakenNew();

            Report("마지막이 아닌 보스를 깨면 보상 화면이 뜬다",
                flow.Screen == GameScreen.Reward && reward.IsOpen && reward.Offer != null && reward.Offer.HasCards,
                (int)flow.Screen);
            Report("보스 보상을 받기 전에는 스테이지가 그대로다", save.Run.StageIndex == stage, save.Run.StageIndex);

            // 보스 보상 앞에서 저장하고 나갔다 이어 해도 보상 화면에서 시작한다.
            Invoke(flow, "HandleEndRunConfirmClosed", "save");
            save.SelectProfile(0);
            flow.ContinueRun();
            Report("보스 보상 앞에서 이어 하면 보상 화면에서 시작한다",
                flow.Screen == GameScreen.Reward && reward.IsOpen, (int)flow.Screen);

            // 받은 것은 저장했는데 다음 스테이지로 넘어가기 전에 꺼진 경우.
            Invoke(flow, "HandleRewardReceived", 0, null);
            save.SelectProfile(0);
            flow.ContinueRun();
            Report("보스 보상을 받고 꺼졌다 이어 하면 다음 스테이지로 넘어간다",
                save.Run.StageIndex == stage + 1 && flow.Screen == GameScreen.Map,
                save.Run.StageIndex);

            // 마지막 스테이지의 보스는 보상 없이 런이 끝난다.
            save.Run.StageIndex = stageCount;
            MapNode lastBoss = FirstNodeOf(map, RoomType.Boss);
            if (lastBoss == null)
            {
                Report("마지막 보스를 볼 보스 방이 있다", false);
                return;
            }

            Invoke(flow, "EnterRoom", lastBoss);
            AwakenNew();
            PressPopupButton(popup, "victory");
            AwakenNew();
            Report("마지막 스테이지의 보스를 깨면 보상 없이 결과 화면으로 간다",
                flow.Screen == GameScreen.RunResult && !reward.IsOpen, (int)flow.Screen);
        }

        /// <summary>
        /// 방 진행 중에 지도 버튼을 눌러 지도를 잠깐 띄우고, 한 번 더 눌러 돌아온다.
        ///
        /// **방이 끝나면 안 된다.** 예전에는 지도 버튼이 맵을 다시 여는 함수로 갔고
        /// 그 함수가 지금 방을 깬 것으로 쳐서 지도를 누르기만 해도 다음 방이 골라졌다.
        /// 그때 이 검사는 방에 다시 들어가는 것을 안쪽 함수로 해 버려 그것을 놓쳤다.
        /// 지금은 버튼만 눌러 보고, 맵 진행 상태까지 본다.
        /// </summary>
        private static void CheckMapPeek(
            GameFlowController flow, SaveService save, MapScreenController map,
            TopBarButton mapButton, string title)
        {
            GameScreen screen = flow.Screen;
            int clearedBefore = save.Run.Statistics.RoomsCleared;
            bool roomClearedBefore = map.Progress.CurrentRoomCleared;

            Report(title + "에서 지도 버튼을 누를 수 있다", MapButtonUsable(flow, mapButton));
            PressMapButton(mapButton);

            Report(title + "에서 지도를 누르면 지도가 뜬다", flow.IsMapPeeking && map.IsOpen && map.IsPeeking);
            Report(title + "에서 지도를 띄워도 지금 화면은 그대로다", flow.Screen == screen, (int)flow.Screen);
            Report(title + "에서 지도를 띄워도 화면이 닫히지 않는다", RoomScreenOpen(screen));
            Report(title + "에서 지도를 띄워도 맵이 방을 깬 것으로 치지 않는다",
                map.Progress.CurrentRoomCleared == roomClearedBefore);
            Report(title + "에서 지도를 띄워도 방 완료가 세어지지 않는다",
                save.Run.Statistics.RoomsCleared == clearedBefore, save.Run.Statistics.RoomsCleared);

            GameObject dim = GetField<GameObject>(map, "_peekDim");
            Report(title + "에서 지도 뒤에 검은 막이 깔린다", dim != null && dim.activeInHierarchy);
            Report(title + "에서 지도를 띄운 동안에도 지도 버튼이 켜져 있다", MapButtonUsable(flow, mapButton));

            // 띄운 지도에서는 노드를 고를 수 없다.
            MapNodeView[] nodes = Object.FindObjectsByType<MapNodeView>();
            int before = map.Progress.CurrentNodeId;
            for (int i = 0; i < nodes.Length; i++)
            {
                Invoke(map, "HandleNodeClicked", nodes[i].Node);
            }

            Report(title + "에서 띄운 지도의 노드를 눌러도 들어가지 않는다",
                map.Progress.CurrentNodeId == before && flow.Screen == screen);

            CapturePreviewScenes.CaptureOpenScene("게임 지도 잠깐 보기 " + title);

            PressMapButton(mapButton);
            Report(title + "에서 지도를 한 번 더 누르면 돌아온다", !flow.IsMapPeeking && !map.IsOpen);
            Report(title + "에서 돌아오면 보던 화면 그대로다",
                flow.Screen == screen && RoomScreenOpen(screen), (int)flow.Screen);
            Report(title + "에서 돌아오면 검은 막이 걷힌다", dim == null || !dim.activeInHierarchy);
        }

        /// <summary>그 화면이 열려 있는지.</summary>
        private static bool RoomScreenOpen(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.Sanctum:
                    SanctumController sanctum = Object.FindAnyObjectByType<SanctumController>(FindObjectsInactive.Include);
                    return sanctum != null && sanctum.IsOpen;
                case GameScreen.Event:
                    EventScreenController events = Object.FindAnyObjectByType<EventScreenController>(FindObjectsInactive.Include);
                    return events != null && events.IsOpen;
                case GameScreen.Reward:
                    RewardScreenController reward = Object.FindAnyObjectByType<RewardScreenController>(FindObjectsInactive.Include);
                    return reward != null && reward.IsOpen;
            }

            return true;
        }

        /// <summary>지도 버튼을 지금 누를 수 있는지. 흐름이 매 프레임 하는 갱신을 한 번 부르고 본다.</summary>
        private static bool MapButtonUsable(GameFlowController flow, TopBarButton mapButton)
        {
            Invoke(flow, "RefreshMapButton");
            Button inner = GetField<Button>(mapButton, "_button");
            return mapButton.Interactable && inner != null && inner.interactable
                && mapButton.gameObject.activeInHierarchy;
        }

        /// <summary>지도 버튼을 진짜로 누른다. 꺼져 있으면 누르지 않는다.</summary>
        private static void PressMapButton(TopBarButton mapButton)
        {
            Button inner = GetField<Button>(mapButton, "_button");
            if (inner != null && inner.interactable)
            {
                inner.onClick.Invoke();
            }
        }

        /// <summary>
        /// 다음 방을 고르는 맵에서 지도 버튼.
        /// 꺼져 있다가, 현재 빌드를 열면 켜지고, 누르면 현재 빌드만 닫힌다.
        /// </summary>
        private static void CheckMapButtonOnMap(GameFlowController flow, MapScreenController map)
        {
            TopBarButton mapButton = FindTopBarButton(TopBarButtonKind.Map);
            if (mapButton == null)
            {
                Report("맵에서 지도 버튼을 찾았다", false);
                return;
            }

            Report("다음 방을 고르는 맵에서는 지도 버튼이 꺼진다", !MapButtonUsable(flow, mapButton));

            flow.OpenCurrentBuild();
            CurrentBuildScreenController build = Object.FindAnyObjectByType<CurrentBuildScreenController>();
            Report("맵에서 현재 빌드를 열면 지도 버튼이 켜진다", MapButtonUsable(flow, mapButton));

            BuildSlotView filled = null;
            BuildSlotView[] buildSlots = Object.FindObjectsByType<BuildSlotView>();
            for (int i = 0; i < buildSlots.Length; i++)
            {
                if (buildSlots[i].Entry.IsValid && buildSlots[i].gameObject.activeInHierarchy)
                {
                    filled = buildSlots[i];
                    break;
                }
            }

            Report("현재 빌드에 물건이 놓인 칸이 있다", filled != null);
            if (filled != null)
            {
                CheckItemDetailHover("현재 빌드 칸", filled, true);
            }

            CheckBuildScroll(buildSlots);

            PressMapButton(mapButton);
            Report("맵에서 지도를 누르면 현재 빌드만 닫힌다",
                (build == null || !build.IsOpen) && map.IsOpen && !flow.IsMapPeeking
                && flow.Screen == GameScreen.Map);
            Report("현재 빌드를 닫으면 지도 버튼이 다시 꺼진다", !MapButtonUsable(flow, mapButton));
        }

        /// <summary>
        /// 눌리는 것마다 마우스를 올렸을 때 표시가 있는지.
        ///
        /// 04 화면 공통 규칙 은 호버를 밝기로만 정했는데
        /// **그림이 밝은 버튼은 밝기만으로는 올렸는지 티가 안 난다.**
        /// 성소의 모닥불과 행상인과 나가기, 행상의 버튼들이 그랬다.
        /// 그래서 `UiSceneBuilder.AddButton` 이 어느 버튼에나 `HoverScale` 을 붙인다.
        ///
        /// 밝기를 스스로 다루는 버튼은 크기를 안 써도 된다.
        /// 그쪽은 `IPointerEnterHandler` 를 들고 있으니 둘 중 하나만 있으면 통과로 본다.
        /// </summary>
        private static void CheckEveryButtonShowsHover()
        {
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);

            Report("씬에 버튼이 있다", buttons.Length > 0, buttons.Length);

            int silent = 0;

            for (int i = 0; i < buttons.Length; i++)
            {
                GameObject go = buttons[i].gameObject;

                if (go.GetComponent<HoverScale>() != null
                    || go.GetComponent<IPointerEnterHandler>() != null)
                {
                    continue;
                }

                silent++;
                Debug.LogWarning("  호버  " + PathOf(go) + " 는 올려도 아무 표시가 없다");
            }

            Report("올려도 표시가 없는 버튼이 없다", silent, 0);

            // 붙은 것이 실제로 커지는 값인지도 본다.
            HoverScale[] scales = Object.FindObjectsByType<HoverScale>(FindObjectsInactive.Include);

            int flat = 0;
            for (int i = 0; i < scales.Length; i++)
            {
                if (scales[i].Ratio <= 1.0001f)
                {
                    flat++;
                }
            }

            Report("호버 확대가 1보다 큰 값으로 들어 있다", flat, 0);
            Report("호버 확대가 버튼마다 붙어 있다", scales.Length > 0, scales.Length);

            // 누를 때 그림을 바꾸는 버튼은 스킨 칸이어야 한다.
            // 그림 자체가 버튼인 칸이 눌림 그림으로 바뀌면 누르는 동안 상자가 나온다. 성소의 모닥불과 행상인이 그랬다.
            int boxed = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button.transition != Selectable.Transition.SpriteSwap)
                {
                    continue;
                }

                Image target = button.targetGraphic as Image;
                if (target != null && UiSceneBuilder.IsSkinSprite(target.sprite))
                {
                    continue;
                }

                boxed++;
                Debug.LogWarning("  눌림  " + PathOf(button.gameObject) + " 는 누르는 동안 그림 대신 상자가 나온다");
            }

            Report("누를 때 그림 대신 상자가 나오는 버튼이 없다", boxed, 0);

            CheckHoverGrowsFromCenter();
        }

        /// <summary>
        /// 호버 확대가 칸 가운데를 기준으로 커지는지.
        /// 버튼 대부분이 피벗을 왼쪽 위에 두어 그대로 키우면 오른쪽 아래로만 커진다.
        /// </summary>
        private static void CheckHoverGrowsFromCenter()
        {
            GameObject go = new GameObject("HoverProbe", typeof(RectTransform));
            try
            {
                RectTransform rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(200f, 100f);
                rect.anchoredPosition = new Vector2(300f, -400f);

                HoverScale hover = go.AddComponent<HoverScale>();
                MethodInfo awake = typeof(HoverScale).GetMethod(
                    "Awake", BindingFlags.Instance | BindingFlags.NonPublic);
                awake.Invoke(hover, null);

                Vector3 before = rect.TransformPoint(rect.rect.center);
                Vector2 position = rect.anchoredPosition;

                hover.OnPointerEnter(null);
                Vector3 grown = rect.TransformPoint(rect.rect.center);
                Report("호버로 커져도 가운데가 제자리다",
                    Vector3.Distance(before, grown) < 0.01f,
                    (int)(Vector3.Distance(before, grown) * 100f));
                Report("호버로 실제로 커진다", rect.localScale.x > 1.0001f);

                hover.OnPointerExit(null);
                Report("마우스가 벗어나면 자리가 돌아온다",
                    Vector2.Distance(rect.anchoredPosition, position) < 0.01f
                    && Mathf.Abs(rect.localScale.x - 1f) < 0.0001f);

                // 커진 동안 다른 코드가 자리를 옮기면 그 자리를 새 기준으로 삼는다.
                hover.OnPointerEnter(null);
                rect.anchoredPosition = new Vector2(500f, -100f);
                hover.OnPointerExit(null);
                Report("커진 동안 다른 코드가 옮긴 자리를 벗어날 때도 지킨다",
                    Vector2.Distance(rect.anchoredPosition, new Vector2(500f, -100f)) < 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>씬에서의 자리를 글로 적는다.</summary>
        private static string PathOf(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        /// <summary>그 종류의 상단 표시줄 버튼을 찾는다.</summary>
        private static TopBarButton FindTopBarButton(TopBarButtonKind kind)
        {
            TopBarButton[] buttons = Object.FindObjectsByType<TopBarButton>(FindObjectsInactive.Include);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].Kind == kind)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 호버 풍선이 커서를 가로채지 않는지.
        ///
        /// 풍선은 버튼 바로 아래에 뜬다. 커서를 받으면 버튼이 벗어난 것으로 읽혀
        /// 풍선이 사라지고, 사라지면 다시 버튼에 올라간 것이 되어 또 뜬다.
        /// 그렇게 **점멸**하고 그 사이에 누른 클릭이 버튼에 닿지 않는다.
        ///
        /// 크기도 함께 본다. 크기를 안 잡으면 유니티가 넣어 주는 100 × 100 이 남아
        /// 글과 상관없는 칸이 버튼을 덮는다. 좌표 배율이 12 였을 때 표시줄 전체를 덮었다.
        /// </summary>
        private static void CheckTooltip()
        {
            TopBarTooltip tooltip = Object.FindAnyObjectByType<TopBarTooltip>(
                FindObjectsInactive.Include);

            Report("호버 풍선이 있다", tooltip != null);

            if (tooltip == null)
            {
                return;
            }

            RectTransform root = GetField<RectTransform>(tooltip, "_root");
            Image background = GetField<Image>(tooltip, "_background");
            TMP_Text label = GetField<TMP_Text>(tooltip, "_label");

            Report("풍선에 켜고 끄는 칸이 있다", root != null);
            Report("풍선 배경이 있다", background != null);
            Report("풍선 글자가 있다", label != null);

            if (root == null || background == null || label == null)
            {
                return;
            }

            // 껍데기와 켜고 끄는 칸이 달라야 한다. 같으면 자기 자신을 끈다.
            Report("풍선이 자기 자신을 끄지 않는다", root.gameObject != tooltip.gameObject);

            Report("풍선 배경이 커서를 받지 않는다", !background.raycastTarget);
            Report("풍선 글자가 커서를 받지 않는다", !label.raycastTarget);

            // 띄워 보고 크기와 자리를 본다.
            TopBarButton mapButton = FindTopBarButton(TopBarButtonKind.Map);

            if (mapButton == null)
            {
                return;
            }

            tooltip.Show(mapButton.RectTransform, "지도");

            Report("풍선이 뜬다", root.gameObject.activeInHierarchy);
            Report("풍선 기준점이 위쪽 가운데", root.pivot.y >= 0.99f && root.pivot.x > 0.4f);

            Rect rect = root.rect;
            Report("풍선이 기본 크기 그대로가 아니다",
                Mathf.Abs(rect.width - UiScale.Px(100f)) > 1f, (int)rect.width);
            Report("풍선이 표시줄보다 낮다", rect.height < UiScale.Px(200f), (int)rect.height);

            // 버튼 아래에 붙어야 한다. 겹치면 커서를 가로채는 자리다.
            Vector3 buttonBottom = mapButton.RectTransform.TransformPoint(
                new Vector3(0f, mapButton.RectTransform.rect.yMin, 0f));
            Vector3 tooltipTop = root.TransformPoint(new Vector3(0f, root.rect.yMax, 0f));

            Report("풍선이 버튼 아래에 붙는다", tooltipTop.y <= buttonBottom.y + 0.01f);

            tooltip.Hide();
            Report("풍선이 사라진다", !root.gameObject.activeInHierarchy);
        }

        /// <summary>
        /// 상단 표시줄의 위치에 **맵의 단계**가 적히는지.
        ///
        /// 스테이지 번호를 적고 있어 방을 옮겨도 늘 1 로 멈춰 있었다.
        /// 02 구성요소 의 위치는 "스테이지 아이콘 + 단계 + 방 아이콘"이고
        /// 05 정보 갱신 시점 이 "새로운 방 진입 시" 바뀌라고 정했다.
        /// </summary>
        private static void CheckTopBarStep(MapNode node, string title)
        {
            TopBarView view = Object.FindAnyObjectByType<TopBarView>(FindObjectsInactive.Include);

            if (view == null)
            {
                Report(title + " 상단 표시줄이 있다", false);
                return;
            }

            TMP_Text label = GetField<TMP_Text>(view, "_stageLabel");

            if (label == null)
            {
                Report(title + " 위치 칸에 숫자 글자가 있다", false);
                return;
            }

            Report(title + " 방의 단계가 위치에 적힌다",
                label.text == node.Stage.ToString(), node.Stage);

            Image stageIcon = GetField<Image>(view, "_stageIcon");
            Report(title + " 위치에 스테이지 아이콘이 붙어 있다",
                stageIcon != null && stageIcon.sprite != null);

            Image roomIcon = GetField<Image>(view, "_roomIcon");
            Report(title + " 위치에 방 아이콘이 붙어 있다",
                roomIcon != null && roomIcon.sprite != null);
        }

        /// <summary>
        /// 야영을 쓰면 모닥불이 꺼진 그림으로 바뀌는지.
        ///
        /// 뷰는 그림을 갈아 끼우고 있었는데 버튼의 스프라이트 전환이
        /// `overrideSprite` 를 비활성 그림으로 덮어써 화면에 나오지 않았다.
        /// 04 야영 의 사용 표시는 쓴 뒤에 아이콘을 회색으로 바꾸고 선택을 막는 것이다.
        /// </summary>
        private static void CheckCamp()
        {
            SanctumController sanctum = Object.FindAnyObjectByType<SanctumController>();
            SanctumScreenView view = Object.FindAnyObjectByType<SanctumScreenView>(
                FindObjectsInactive.Include);

            if (sanctum == null || view == null)
            {
                Report("성소 화면이 있다", false);
                return;
            }

            Image campImage = GetField<Image>(view, "_campImage");
            Button campButton = GetField<Button>(view, "_campButton");
            Sprite onSprite = GetField<Sprite>(view, "_campAvailableSprite");
            Sprite offSprite = GetField<Sprite>(view, "_campUsedSprite");

            Report("야영 그림 칸이 있다", campImage != null);
            Report("야영 버튼이 있다", campButton != null);
            Report("불 붙은 모닥불 그림이 있다", onSprite != null);
            Report("불 꺼진 모닥불 그림이 있다", offSprite != null);

            if (campImage == null || campButton == null || onSprite == null || offSprite == null)
            {
                return;
            }

            Report("쓰기 전에는 야영을 누를 수 있다", campButton.interactable);
            Report("쓰기 전에는 불이 붙어 있다", ShownSpriteOf(campImage) == onSprite);

            sanctum.UseCamp();

            Report("쓰면 야영을 누를 수 없다", !campButton.interactable);
            Report("쓰면 불 꺼진 모닥불로 바뀐다", ShownSpriteOf(campImage) == offSprite);

            SanctumConfig config = GetField<SanctumConfig>(sanctum, "_config");
            Report("성소당 한 번만 쓸 수 있다",
                config != null && sanctum.State != null && !sanctum.State.CanUseCamp(config));
        }

        /// <summary>
        /// 행상의 문양 변경을 실제로 눌러 본다.
        ///
        /// **보이는지부터 본다.** 소용돌이가 양탄자 그림에 있을 줄 알고
        /// 투명한 누르는 칸만 얹어 두어 **화면에 아무것도 없는 자리**였다.
        /// 그러면 누를 자리를 찾을 수가 없다.
        ///
        /// 누른 뒤도 본다. 바꿀 문양을 고르는 화면이 없어
        /// `SymbolChangeTargetRequested` 를 아무도 받지 않았고,
        /// 그래서 눌러도 아무 일이 일어나지 않았다.
        /// </summary>
        private static void CheckSymbolChange(GameFlowController flow, SaveService save)
        {
            SanctumController sanctum = Object.FindAnyObjectByType<SanctumController>();
            MerchantScreenView view = Object.FindAnyObjectByType<MerchantScreenView>(
                FindObjectsInactive.Include);
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");

            if (sanctum == null || view == null || popup == null || save.Run == null)
            {
                Report("행상 화면과 팝업이 있다", false);
                return;
            }

            Button change = GetField<Button>(view, "_symbolChangeButton");
            Report("문양 변경 버튼이 있다", change != null);

            if (change == null)
            {
                return;
            }

            Report("문양 변경 자리가 화면에 있다", change.gameObject.activeInHierarchy);

            // 눈에 보이는 자리여야 한다. 투명하기만 하면 누를 데를 찾을 수가 없다.
            Image mark = change.GetComponent<Image>();
            Report("문양 변경 자리에 보이는 것이 있다",
                mark != null && (mark.sprite != null || mark.color.a > 0.1f));

            // 값을 치를 수 있게 골드를 넉넉히 준다. 모자라면 버튼이 꺼져 눌리지 않는다.
            // **런 데이터의 숫자를 직접 고치지 않는다.** 성소는 `RunContext` 의 골드 상태를 따로 들고 있어
            // 숫자만 바꾸면 둘이 어긋난다. 예전에는 그렇게 해 놓고 우연히 통과하고 있었다.
            if (flow.Context != null && save.Run.Status.Gold < 500)
            {
                flow.Context.GainGold(500 - save.Run.Status.Gold);
            }

            int goldBefore = save.Run.Status.Gold;

            int symbolsBefore = save.Run.Owned.TotalSymbolCount;
            change.onClick.Invoke();
            AwakenNew();

            // 2026년 10월 5일부터 문양 변경은 고르기 화면에서 바꿀 문양을 직접 고른다. 예전에는 난수로 골랐다.
            ItemPickerScreenController picker = GetField<ItemPickerScreenController>(flow, "_picker");
            ItemPickerScreenView pickerView = picker != null ? GetField<ItemPickerScreenView>(picker, "_view") : null;
            Report("문양 변경을 누르면 고르기 화면이 열린다", picker != null && picker.IsOpen);

            if (picker == null || pickerView == null || !picker.IsOpen)
            {
                return;
            }

            List<BuildSlotView> choices = pickerView.GetShownSlots();
            Report("고르기 화면에 가진 문양 종류가 모두 나온다",
                choices.Count == save.Run.Owned.Symbols.Count, choices.Count);
            Report("문양을 고를 때 정렬과 태그 비중 칸이 나온다", pickerView.ShowsSymbolPanels);
            Report("아직 고르지 않으면 확인을 못 누른다", !pickerView.CanConfirm);
            // 2026년 10월 8일부터 지도 버튼은 흐름에서 떼어 낸 `MapPeekFlow` 가 맡는다.
            MapPeekFlow mapPeek = GetField<MapPeekFlow>(flow, "_mapPeek");
            Report("고르기 화면이 떠 있으면 지도 버튼이 안 눌린다", mapPeek != null && !mapPeek.CanUseButton());

            // 취소하면 값을 치르지 않는다.
            Button cancel = GetField<Button>(pickerView, "_cancelButton");
            cancel.onClick.Invoke();
            Report("취소하면 고르기 화면이 닫힌다", !picker.IsOpen);
            Report("취소하면 골드가 그대로", save.Run.Status.Gold == goldBefore, save.Run.Status.Gold);
            Report("취소하면 문양 수가 그대로",
                save.Run.Owned.TotalSymbolCount == symbolsBefore,
                save.Run.Owned.TotalSymbolCount);

            // 다시 열어 칸을 눌러 고르고 바꾼다.
            change.onClick.Invoke();
            AwakenNew();
            choices = pickerView.GetShownSlots();
            BuildSlotView target = choices.Count > 0 ? choices[0] : null;
            if (target == null)
            {
                Report("고를 문양 칸이 있다", false);
                return;
            }

            string targetId = target.Entry.Id;
            int targetCount = save.Run.Owned.GetSymbolCount(targetId);
            target.Press();
            Report("칸을 누르면 테두리로 골라진다", target.IsSelected && picker.SelectedId == targetId);
            Report("고르면 아래 줄에 고른 문양이 적힌다", pickerView.ResultText.Contains("[" + target.Entry.DisplayName + "]"));
            Report("고르면 확인을 누를 수 있다", pickerView.CanConfirm);

            CapturePreviewScenes.CaptureOpenScene("게임 성소 문양 변경 고르기");

            GetField<Button>(pickerView, "_confirmButton").onClick.Invoke();
            AwakenNew();

            Report("확인하면 고르기 화면이 닫힌다", !picker.IsOpen);
            Report("고른 문양이 한 장 줄어든다", save.Run.Owned.GetSymbolCount(targetId) == targetCount - 1,
                save.Run.Owned.GetSymbolCount(targetId));
            Report("바꾸면 골드가 줄어든다", save.Run.Status.Gold < goldBefore, save.Run.Status.Gold);
            Report("바꿔도 문양 장수는 그대로",
                save.Run.Owned.TotalSymbolCount == symbolsBefore,
                save.Run.Owned.TotalSymbolCount);
            Report("문양 변경 횟수가 올랐다",
                sanctum.State != null && sanctum.State.SymbolChangeCount == 1,
                sanctum.State != null ? sanctum.State.SymbolChangeCount : -1);

            // 바꿨다는 알림 팝업이 떠 있다. 닫아 둔다.
            if (popup.IsOpen)
            {
                PressPopupButton(popup, "confirm");
            }

            Report("알림까지 닫으면 팝업이 걷힌다", !popup.IsOpen);

            CheckBloodDealPicker(flow, picker, pickerView);
        }

        /// <summary>
        /// 08 피의 거래 가 여는 고르기 화면. 가진 것이 아니라 게임의 모든 문양을 늘어놓는다.
        /// 개수 배지는 붙이지 않고, 오른쪽 태그 비중은 지금 가진 문양으로 센다.
        /// </summary>
        private static void CheckBloodDealPicker(GameFlowController flow, ItemPickerScreenController picker, ItemPickerScreenView view)
        {
            RunCatalogConfig catalog = GetField<RunCatalogConfig>(flow, "_catalogConfig");
            // 2026년 10월 8일부터 이벤트 고르기 화면 내용은 흐름에서 떼어 낸 `EventRoomFlow` 가 만든다.
            EventRoomFlow events = GetField<EventRoomFlow>(flow, "_events");
            ItemPickRequest request = events != null && flow.Context != null
                ? events.MakePickRequest(EventPickSource.AllSymbols, flow.Context)
                : null;

            Report("08 고르기 화면 내용을 만든다", request != null && catalog != null);
            if (request == null || catalog == null)
            {
                return;
            }

            Report("08 은 모든 문양을 늘어놓는다", request.Candidates.Count == catalog.Symbols.Count, request.Candidates.Count);
            Report("08 은 개수 배지를 붙이지 않는다", !request.ShowCount);

            picker.Open(request, null);
            AwakenNew();
            Report("08 고르기 화면에 모든 문양 칸이 놓인다", view.GetShownSlots().Count == catalog.Symbols.Count,
                view.GetShownSlots().Count);

            ScrollRect scroll = GetField<ScrollRect>(view, "_scroll");
            Canvas.ForceUpdateCanvases();
            Report("08 문양 서른여섯은 스크롤로 본다",
                scroll != null && scroll.content.rect.height > scroll.viewport.rect.height + 1f);

            List<BuildSlotView> slots = view.GetShownSlots();
            if (slots.Count > 0)
            {
                slots[0].Press();
            }

            UiSceneBuilder.SyncShadows();
            Canvas.ForceUpdateCanvases();
            CapturePreviewScenes.CaptureOpenScene("게임 피의 거래 문양 고르기");
            picker.Close();
        }

        /// <summary>
        /// 그 칸이 화면에 실제로 내보내는 그림.
        /// `overrideSprite` 가 채워져 있으면 그쪽이 나오고 `sprite` 는 가려진다.
        /// </summary>
        private static Sprite ShownSpriteOf(Image image)
        {
            return image.overrideSprite != null ? image.overrideSprite : image.sprite;
        }

        /// <summary>
        /// 이벤트 화면이 설정 에셋의 자료를 직접 쓰고 있지 않은지.
        ///
        /// 에셋이 들고 있는 쪽을 그대로 넘겼더니 화면이 그것을 제자리에서 고쳐
        /// **에셋이 영구히 바뀌었다.** 그래서 같은 이벤트 방에 다시 들어가면
        /// 앞의 진행이 그대로 이어져 열렸고, 이벤트가 정상 진행되지 않았다.
        ///
        /// 여기서는 선택지를 눌러 보지 않는다.
        /// 누르면 이벤트가 끝나 방까지 함께 끝나 버려 뒤의 검사가 어긋난다.
        /// 고쳐 쓰는 바탕이 **에셋과 다른 객체인지**만 본다. 그것이 이 버그의 핵심이다.
        /// 실제로 고쳐도 원본이 그대로인지는 순수 로직 검사의 `OriginalStaysClean` 이 본다.
        ///
        /// 선택지마다 이어지는 쪽이 다 적혀 있는지도 함께 본다.
        /// 빠뜨리면 그 선택지를 고른 자리에서 이벤트가 끝나지 않아 런이 막힌다.
        /// </summary>
        private static void CheckEventStartsClean(
            GameFlowController flow, SaveService save, MapNode node)
        {
            EventScreenController events = Object.FindAnyObjectByType<EventScreenController>();
            RunEventConfig config = GetField<RunEventConfig>(flow, "_eventConfig");

            if (events == null || events.Page == null || config == null || save.Run == null)
            {
                Report("이벤트 화면과 목록 설정이 있다", false);
                return;
            }

            Report("처음 열면 고를 것이 있다", events.Page.HasSelectableChoice);
            Report("이벤트가 하나 이상 있다", config.Events.Count > 0, config.Events.Count);
            Report("이벤트 식별자가 겹치지 않는다", config.AllIdsAreUnique());

            // 흐름과 같은 방 번호로 찾는다. 스테이지를 섞은 번호다. `GameFlowController.RoomKey`
            // 흐름은 이 방의 이벤트를 만난 것으로 이미 적었다. 그 앞의 목록으로 뽑아야 같은 것이 나온다.
            List<string> seenBefore = new List<string>(save.Run.SeenEventIds);
            if (seenBefore.Count > 0)
            {
                seenBefore.RemoveAt(seenBefore.Count - 1);
            }

            EventScript script = config.Pick(save.Run.Seed, save.Run.StageIndex * 1000 + node.Id, save.Run.StageIndex, seenBefore);
            Report("들어간 이벤트를 만난 것으로 적는다",
                script != null && save.Run.SeenEventIds.Count > 0 && save.Run.SeenEventIds[save.Run.SeenEventIds.Count - 1] == script.EventId);
            Report("흐름이 연 이벤트와 같은 이벤트를 찾았다",
                script != null && events.Page != null && script.StartScreen != null
                && events.Page.BodyText == script.StartScreen.BodyText);
            Report("그 방에 나올 이벤트를 찾았다", script != null);

            if (script == null || script.StartScreen == null)
            {
                return;
            }

            // 화면이 들고 있는 것이 각본 자체면 고칠 때마다 에셋이 바뀐다.
            // 각본은 화면마다 쪽을 새로 만들어 주므로 늘 다른 객체여야 한다.
            Report("화면이 설정 에셋의 쪽을 그대로 쓰지 않는다",
                events.Page.Choices.Count > 0
                && script.StartScreen.Choices.Count > 0
                && !ReferenceEquals(events.Page.Choices[0], script.StartScreen.Choices[0]));

            // 각본 전부를 훑어 끊긴 데를 찾는다.
            // 없는 화면을 가리키거나 아무 데서도 갈 수 없는 화면이 있으면 런이 막힌다.
            List<string> broken = new List<string>();
            config.FindBrokenLinks(broken);

            for (int i = 0; i < broken.Count; i++)
            {
                Debug.LogWarning("  이벤트  " + broken[i]);
            }

            Report("이벤트 각본에 끊긴 데가 없다", broken.Count, 0);

            // 에디터에서 고친 이벤트도 화면당 선택지 수를 넘지 않는지. 2026년 10월 9일 원재가 5개로 정했다.
            List<string> crowded = new List<string>();
            config.FindTooManyChoices(crowded);
            for (int i = 0; i < crowded.Count; i++)
            {
                Debug.LogWarning("  이벤트  " + crowded[i]);
            }

            Report("이벤트 화면의 선택지가 화면당 최대 수를 넘지 않는다", crowded.Count, 0);
            Report("화면당 최대 선택지 수가 5개다", config.MaxChoicesPerScreen, 5);

            EventScreenView view = Object.FindAnyObjectByType<EventScreenView>(FindObjectsInactive.Include);
            Report("화면당 최대 선택지가 이벤트 화면 칸 수 안에 들어간다",
                view != null && config.MaxChoicesPerScreen <= view.MaxChoiceCount, view != null ? view.MaxChoiceCount : -1);

            CheckEventIllustrations(flow, config, events);
        }

        /// <summary>
        /// 이벤트 삽화가 붙는지.
        ///
        /// 02 ~ 06 다섯 이벤트에 임시 그림이 있다. 각본이 코드에서 다시 지어져
        /// 이벤트 목록 에셋에 들어갔는지도 함께 본다. 예전에는 처음 한 번만 만들어 옛 각본이 남았다.
        /// </summary>
        private static void CheckEventIllustrations(
            GameFlowController flow, RunEventConfig config, EventScreenController events)
        {
            EventIllustrationConfig illustrations = GetField<EventIllustrationConfig>(flow, "_eventIllustrations");
            Report("흐름에 이벤트 삽화 목록이 물려 있다", illustrations != null);

            if (illustrations == null)
            {
                return;
            }

            Report("이벤트 삽화가 열여섯 장 있다", illustrations.Illustrations.Count, 16);

            int missing = 0;
            string[] withArt = { "carve_power", "pandora", "reflection", "purification", "symbol_trade" };

            for (int i = 0; i < withArt.Length; i++)
            {
                EventScript script = config.Find(withArt[i]);
                if (script == null)
                {
                    missing++;
                    Debug.LogWarning("  이벤트  목록에 " + withArt[i] + " 이 없다");
                    continue;
                }

                for (int j = 0; j < script.Screens.Count; j++)
                {
                    if (illustrations.GetIllustration(script.Screens[j].IllustrationId) == null)
                    {
                        missing++;
                        Debug.LogWarning("  이벤트  삽화가 없다: " + script.Screens[j].IllustrationId);
                    }
                }
            }

            Report("02 ~ 06 이 이벤트 목록에 다 들어 있다", config.Find("reflection") != null);
            Report("02 ~ 06 의 화면마다 삽화가 붙는다", missing, 0);

            // 화면이 그림 없는 이벤트에서 앞 그림을 남기지 않는지.
            EventScreenView view = Object.FindAnyObjectByType<EventScreenView>(FindObjectsInactive.Include);
            if (view != null)
            {
                Sprite art = illustrations.GetIllustration(EventScripts.IllustrationIdOf("pandora", "1"));
                view.ShowIllustration(art);
                Report("삽화가 있으면 그 그림이 놓인다", art != null && view.CurrentIllustration == art);

                view.ShowIllustration(null);
                Report("삽화가 없으면 앞 그림을 지우고 바탕 칸으로 돌린다",
                    view.CurrentIllustration != art && view.CurrentIllustration != null);

                CheckEventBodiesFit(view, config);

                // 원래 보이던 쪽으로 되돌린다.
                events.Refresh();
            }
        }

        /// <summary>
        /// 모든 이벤트의 본문이 본문 칸 안에 들어가는지.
        ///
        /// 이벤트 기획서의 스토리가 길어 24픽셀로는 넘치는 화면이 있었다.
        /// 넘칠 때만 20픽셀까지 줄이게 했으니 그래도 넘치는 화면이 없어야 한다.
        /// 앞 화면의 작업 결과가 본문 위에 한 줄 붙으므로 그 줄까지 넣고 잰다.
        /// </summary>
        private static void CheckEventBodiesFit(EventScreenView view, RunEventConfig config)
        {
            TMP_Text label = GetField<TMP_Text>(view, "_bodyLabel");
            if (label == null)
            {
                Report("이벤트 본문 글자를 찾았다", false);
                return;
            }

            bool wasActive = view.gameObject.activeSelf;
            view.gameObject.SetActive(true);
            view.ApplyLayout();

            string before = label.text;
            int overflowing = 0;
            int shrunk = 0;

            for (int i = 0; i < config.Events.Count; i++)
            {
                EventScript script = config.Events[i];

                for (int j = 0; j < script.Screens.Count; j++)
                {
                    EventScreen screen = script.Screens[j];
                    string body = j == 0
                        ? screen.BodyText
                        : "스틱스강 이 난파선 이 되었다. 골드 100 을 얻었다.\n\n" + screen.BodyText;

                    label.text = body;
                    label.ForceMeshUpdate(true, true);

                    if (label.isTextOverflowing)
                    {
                        overflowing++;
                        Debug.LogWarning("  이벤트  본문이 칸을 넘친다: " + script.EventId + "/" + screen.ScreenId);
                    }
                    else if (label.fontSize < label.fontSizeMax - 0.5f)
                    {
                        shrunk++;
                    }
                }
            }

            CaptureEventColors(view, config);
            CheckEventChoicesFit(view, config);

            label.text = before;
            view.gameObject.SetActive(wasActive);

            Report("이벤트 본문이 칸을 넘치지 않는다", overflowing, 0);
            Debug.Log("  이벤트  글자를 줄여 맞춘 화면 " + shrunk + "개");
        }

        /// <summary>
        /// 모든 이벤트 화면의 선택지가 칸 안에 들어가는지.
        /// 2026년 10월 5일 원재가 글이 긴 선택지가 여럿이면 화면 아래로 넘친다고 짚었다.
        /// 넘치면 화면이 글자를 20픽셀까지 줄이므로 그래도 넘치는 화면이 없어야 한다.
        ///
        /// 채우는 선택지는 대상 이름을 꽤 긴 `불의 꽃` 으로 넣고 채울 수만큼 만든다.
        /// 앞 화면에서 고른 선택지 하나가 위에 남는 것까지 넣고 잰다.
        /// </summary>
        private static void CheckEventChoicesFit(EventScreenView view, RunEventConfig config)
        {
            EventLayoutConfig layout = GetField<EventLayoutConfig>(view, "_layout");
            if (layout == null)
            {
                Report("이벤트 자리 설정을 찾았다", false);
                return;
            }

            int overflowing = 0;
            int shrunk = 0;
            EventVisualConfig visual = GetField<EventVisualConfig>(view, "_visual");

            for (int i = 0; i < config.Events.Count; i++)
            {
                EventScript script = config.Events[i];
                for (int j = 0; j < script.Screens.Count; j++)
                {
                    EventScreen screen = script.Screens[j];
                    EventPage page = new EventPage();
                    page.BodyText = screen.BodyText;

                    if (j > 0)
                    {
                        EventChoice before = new EventChoice("before", "앞 화면에서 고른 선택지");
                        before.State = EventChoiceState.Chosen;
                        page.Choices.Add(before);
                    }

                    for (int k = 0; k < screen.Choices.Count; k++)
                    {
                        EventScreenChoice spec = screen.Choices[k];
                        int made = spec.Fill == EventChoiceFill.None ? 1 : spec.FillCount;

                        for (int n = 0; n < made; n++)
                        {
                            string label = spec.Fill == EventChoiceFill.None
                                ? spec.Label
                                : string.Format(spec.GetFillLabelFormat(n), "불의 꽃");
                            EventChoice choice = new EventChoice(spec.Id + "_" + n, SampleFill(label));
                            choice.ResultHidden = spec.ResultHidden;
                            for (int m = 0; m < spec.Lines.Count; m++)
                            {
                                EventEffectLine line = spec.Lines[m];
                                string text = SampleFill(line.Text);
                                choice.Lines.Add(new EventEffectLine(line.Kind, text, line.Met));
                            }

                            choice.ResolveState();
                            page.Choices.Add(choice);
                        }
                    }

                    view.Show(page, null);

                    List<float> heights = GetField<List<float>>(view, "_heights");
                    if (heights != null && heights.Count > 0 && !layout.ChoicesFitBody(heights))
                    {
                        overflowing++;
                        Debug.LogWarning("  이벤트  선택지가 칸을 넘친다: " + script.EventId + "/" + screen.ScreenId);
                    }

                    List<EventChoiceView> shown = GetField<List<EventChoiceView>>(view, "_shown");
                    if (visual != null && shown != null && shown.Count > 0 && shown[0].FontSize < visual.ChoiceFontSize - 0.5f)
                    {
                        shrunk++;
                    }
                }
            }

            Report("이벤트 선택지가 칸을 넘치지 않는다", overflowing, 0);
            Debug.Log("  이벤트  선택지 글자를 줄여 맞춘 화면 " + shrunk + "개");
        }

        /// <summary>선택지 글의 자리 표시를 길이 재기용 견본으로 채운다. 이름은 꽤 긴 `불의 꽃` 이다.</summary>
        private static string SampleFill(string text)
        {
            return text
                .Replace(EventRunner.HealthToken, "30")
                .Replace(EventRunner.MaxHealthCostToken, "30")
                .Replace(EventRunner.NameToken, "불의 꽃")
                .Replace(EventRunner.ResultToken(0), "불의 꽃")
                .Replace(EventRunner.ResultToken(1), "불의 꽃")
                .Replace(EventRunner.ResultsToken, "[불의 꽃], [불의 꽃]");
        }

        /// <summary>그림으로 뽑을 때만 쓰는 바깥 세상. 최대 체력 99 인 런처럼 굴고 아무것도 바꾸지 않는다.</summary>
        private class CaptureWorld : IEventWorld
        {
            public int Gold
            {
                get { return 100; }
            }

            public int Health
            {
                get { return 99; }
            }

            public int MaxHealth
            {
                get { return 99; }
            }

            /// 채우는 선택지에 넣을 견본 물건. 그림에 이름이 어떻게 나오는지만 본다.
            private static readonly string[] SampleNames = { "곡괭이", "날개", "낡은 책" };

            public void Collect(EventFillRequest request, EventRandom random, List<EventItem> into)
            {
                for (int i = 0; i < request.Count && i < SampleNames.Length; i++)
                {
                    into.Add(new EventItem("sample_" + i, SampleNames[i]));
                }
            }

            public bool NeedsRoom(EventAction action)
            {
                return false;
            }

            public bool CanDo(EventAction action, EventTarget target, out string reason)
            {
                reason = string.Empty;
                return true;
            }

            public void Decide(EventAction action, EventTarget target, EventRandom random, List<EventItem> into)
            {
                int count = action != null && action.Count > 1 ? action.Count : 1;
                for (int i = 0; i < count && i < SampleNames.Length; i++)
                {
                    into.Add(new EventItem("decided_" + i, SampleNames[i]));
                }
            }

            public string Apply(EventAction action, EventTarget target, EventRandom random)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 거래 이벤트 화면에서 잃는 것이 빨강, 얻는 것이 초록으로 그려지는지 본다.
        /// 2026년 10월 5일 원재의 요청이다. 29 생명력 거래 화면 2 의 선택지와,
        /// 앞 화면에서 체력을 내주고 골드를 받은 결과 한 줄을 본문 위에 얹어 그린다.
        /// </summary>
        private static void CaptureEventColors(EventScreenView view, RunEventConfig config)
        {
            EventScript trade = null;
            for (int i = 0; i < config.Events.Count; i++)
            {
                if (config.Events[i].EventId == "life_trade")
                {
                    trade = config.Events[i];
                }
            }

            EventScreen screen = trade != null ? trade.Find("2") : null;
            if (screen == null)
            {
                Report("29 생명력 거래 화면 2 가 있다", false);
                return;
            }

            // 선택지는 진짜 각본 실행기로 만든다. 체력 자리(`{hp}`)를 숫자로 바꾸는 것이 실행기다.
            // 최대 체력 99 인 런으로 본다. 10% 는 9.9 를 반올림해 10 이다.
            EventRunner runner = new EventRunner(trade, new CaptureWorld(), new EventRandom(1));
            runner.Begin();
            EventStep offer = runner.Choose("talk");

            EventPage page = new EventPage();
            page.IllustrationId = screen.IllustrationId;
            page.BodyText = EventChoiceText.MarkCost("체력 10") + " 을 잃었다. "
                            + EventChoiceText.MarkGain("골드 60") + " 을 받았다.\n\n" + screen.BodyText;

            if (offer.Result != null)
            {
                page.Choices.AddRange(offer.Result.NextChoices);
            }

            bool placeholderLeft = false;
            for (int i = 0; i < page.Choices.Count; i++)
            {
                placeholderLeft |= EventChoiceText.Build(page.Choices[i], EventChoiceTextFormat.Default()).Contains("{");
            }

            Report("29 생명력 거래 선택지에 체력 자리 표시가 남지 않는다", !placeholderLeft);

            view.Show(page, null);
            AwakenNew();

            TMP_Text label = GetField<TMP_Text>(view, "_bodyLabel");
            string shown = label != null ? label.text : string.Empty;
            EventVisualConfig visual = GetField<EventVisualConfig>(view, "_visual");
            EventChoiceTextFormat format = visual != null ? visual.GetTextFormat() : EventChoiceTextFormat.Default();
            Report("결과 문장의 잃은 것이 빨강으로 그려진다", shown.Contains("<color=#" + format.CostColorHex + ">체력 10</color>"));
            Report("결과 문장의 얻은 것이 초록으로 그려진다", shown.Contains("<color=#" + format.GainColorHex + ">골드 60</color>"));
            Report("결과 문장에 표시 글자가 남지 않는다", !shown.Contains("<cost>") && !shown.Contains("<gain>"));

            Canvas.ForceUpdateCanvases();
            CapturePreviewScenes.CaptureOpenScene("게임 이벤트 색 확인");

            // 18 주인 없는 유물 화면 2. 받는 물건은 "유물 — [곡괭이]" 꼴이다.
            EventScript relic = null;
            for (int i = 0; i < config.Events.Count; i++)
            {
                if (config.Events[i].EventId == "ownerless_relic")
                {
                    relic = config.Events[i];
                }
            }

            if (relic != null)
            {
                EventRunner relicRunner = new EventRunner(relic, new CaptureWorld(), new EventRandom(1));
                EventPage relicStart = relicRunner.Begin();
                EventStep pick = relicRunner.Choose("search");
                if (pick.Result != null)
                {
                    EventPage relicPage = new EventPage();
                    relicPage.IllustrationId = relicStart.IllustrationId;
                    relicPage.BodyText = pick.Result.BodyText;
                    relicPage.Choices.AddRange(pick.Result.NextChoices);
                    view.Show(relicPage, null);
                    AwakenNew();

                    string firstText = relicPage.Choices.Count > 0
                        ? EventChoiceText.Build(relicPage.Choices[0], format)
                        : string.Empty;
                    Report("18 유물 선택지가 유물 — [곡괭이] 꼴이다",
                        firstText.StartsWith("유물") && firstText.Contains("[곡괭이]"));

                    Canvas.ForceUpdateCanvases();
                    CapturePreviewScenes.CaptureOpenScene("게임 이벤트 유물 고르기");
                }
            }
        }

        /// <summary>
        /// 버튼 수가 다른 팝업을 번갈아 띄워 본다.
        ///
        /// **큰 팝업 → 작은 팝업 → 큰 팝업 순서여야 걸린다.**
        /// 버튼 칸은 한 번 만들면 다음 팝업에서도 그대로 쓰는데,
        /// 작은 팝업이 남는 칸을 꺼 두고 다시 켜 주는 데가 없었다.
        /// 그래서 셋 가운데 첫 하나만 보였다. 런 종료 확인 팝업이 그랬다.
        /// </summary>
        private static void CheckPopupButtonCountChanges(GameFlowController flow)
        {
            PopupPresenter popup = GetField<PopupPresenter>(flow, "_popup");
            Report("흐름이 팝업을 물고 있다", popup != null);

            if (popup == null)
            {
                return;
            }

            PopupSpec three = new PopupSpec("검사용 셋", "버튼 셋을 띄운다.");
            three.Add(PopupButtonSpec.Normal("a", "하나"));
            three.Add(PopupButtonSpec.Normal("b", "둘"));
            three.Add(PopupButtonSpec.Normal("c", "셋"));

            popup.Show(three);
            AwakenNew();
            CheckPopupButtonsAreVisible(popup, "처음 띄운 버튼 셋");
            popup.Dismiss();

            // 버튼 하나짜리를 끼운다. 여기서 남는 칸 둘이 꺼진다.
            popup.Show(PopupSpec.Notice("검사용 하나", "버튼 하나를 띄운다.", "확인"));
            AwakenNew();
            CheckPopupButtonsAreVisible(popup, "버튼 하나짜리");
            popup.Dismiss();

            // 다시 셋짜리. 꺼진 칸이 다시 켜져야 한다.
            popup.Show(three);
            AwakenNew();
            CheckPopupButtonsAreVisible(popup, "하나짜리 뒤의 버튼 셋");

            // 눌러서 닫히는지도 본다. 꺼진 칸은 눌릴 수가 없다.
            PressPopupButton(popup, "c");
            Report("되살아난 버튼이 눌린다", !popup.IsOpen);

            if (popup.IsOpen)
            {
                popup.Dismiss();
            }

            Report("검사용 팝업을 닫으면 어두운 막도 걷힌다", !DimIsShowing());

            // 팝업이 떠 있을 때 다른 팝업이 오면 덮어쓰지 않고 뒤에 선다.
            // 덮어쓰면 앞 팝업이 기다리던 대답이 오지 않는다. 전투 대역이 그렇게 멈출 수 있었다.
            string firstAnswer = null;
            popup.Show(PopupSpec.Notice("앞 팝업", "먼저 뜬다.", "확인"), id => firstAnswer = id);
            popup.Show(PopupSpec.Notice("뒤 팝업", "나중에 뜬다.", "확인"));
            AwakenNew();

            Report("떠 있는 팝업을 새 팝업이 덮어쓰지 않는다",
                popup.Current != null && popup.Current.Title == "앞 팝업" && popup.PendingCount == 1);

            PressPopupButton(popup, popup.Current != null && popup.Current.ButtonCount > 0
                ? popup.Current.Buttons[0].Id : "ok");
            AwakenNew();

            Report("앞 팝업의 대답이 온다", firstAnswer != null);
            Report("앞 팝업이 닫히면 뒤 팝업이 뜬다",
                popup.IsOpen && popup.Current != null && popup.Current.Title == "뒤 팝업");

            popup.Dismiss();
            Report("기다리던 팝업까지 닫으면 다 걷힌다", !popup.IsOpen && popup.PendingCount == 0);
        }

        /// <summary>그 자리를 맡고 있는 카드를 찾는다.</summary>
        private static ProfileCardView FindCardFor(int index)
        {
            ProfileCardView[] cards = Object.FindObjectsByType<ProfileCardView>(FindObjectsInactive.Include);

            for (int i = 0; i < cards.Length; i++)
            {
                if (!cards[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (cards[i].Index == index)
                {
                    return cards[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 떠 있는 팝업의 버튼 칸이 모두 켜져 있는지.
        ///
        /// 버튼 칸은 한 번 만들면 다음 팝업에서도 그대로 쓴다.
        /// 버튼이 적은 팝업을 띄우면 남는 칸을 꺼 두는데 다시 켜는 데가 없어서,
        /// 그 뒤에 버튼이 많은 팝업이 떠도 첫 하나만 보였다.
        /// </summary>
        private static void CheckPopupButtonsAreVisible(PopupPresenter popup, string title)
        {
            PopupView view = GetField<PopupView>(popup, "_view");

            if (view == null || popup.Current == null)
            {
                Report(title + " 팝업의 표시를 찾았다", false);
                return;
            }

            List<PopupButtonView> buttons = GetField<List<PopupButtonView>>(view, "_buttons");

            if (buttons == null)
            {
                Report(title + " 팝업의 버튼 목록을 찾았다", false);
                return;
            }

            int count = popup.Current.ButtonCount;
            int shown = 0;

            for (int i = 0; i < buttons.Count && i < count; i++)
            {
                if (buttons[i] != null && buttons[i].gameObject.activeSelf)
                {
                    shown++;
                }
            }

            Report(title + " 팝업의 버튼이 " + count + "개 다 보인다", shown == count, shown);
        }

        /// <summary>떠 있는 팝업의 그 버튼을 누른다.</summary>
        private static void PressPopupButton(PopupPresenter popup, string buttonId)
        {
            PopupView view = GetField<PopupView>(popup, "_view");

            if (view == null)
            {
                return;
            }

            List<PopupButtonView> buttons = GetField<List<PopupButtonView>>(view, "_buttons");

            if (buttons == null)
            {
                return;
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] == null || !buttons[i].gameObject.activeSelf)
                {
                    continue;
                }

                if (buttons[i].Spec.Id != buttonId)
                {
                    continue;
                }

                Button inner = GetField<Button>(buttons[i], "_button");

                if (inner != null)
                {
                    inner.onClick.Invoke();
                }

                return;
            }
        }

        private static int CountOf<T>() where T : Object
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include).Length;
        }

        /// <summary>센 수가 기대한 수와 같은지 본다.</summary>
        private static void Report(string title, int actual, int expected)
        {
            Report(title, actual == expected, actual);
        }

        private static void Report(string title, bool passed)
        {
            Report(title, passed, 0);
        }

        private static void Report(string title, bool passed, int actual)
        {
            _checked++;

            if (passed)
            {
                Debug.Log("  통과  " + title);
                return;
            }

            _failed++;
            Debug.LogError("  실패  " + title + "   (실제 " + actual + ")");
        }

        /// <summary>실제 값이 글일 때. 실패하면 그 글을 그대로 적어 무엇이 달랐는지 보이게 한다.</summary>
        private static void Report(string title, bool passed, string actual)
        {
            _checked++;

            if (passed)
            {
                Debug.Log("  통과  " + title);
                return;
            }

            _failed++;
            Debug.LogError("  실패  " + title + "   (실제 " + actual + ")");
        }

        private static void Done()
        {
            Debug.Log("게임 씬 검사 " + _checked + "건, 실패 " + _failed + "건");
        }
    }
}
