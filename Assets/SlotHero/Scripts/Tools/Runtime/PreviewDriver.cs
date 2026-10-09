using SlotHero.CurrentBuild;
using SlotHero.CurrentBuild.UI;
using SlotHero.Events;
using SlotHero.Events.UI;
using SlotHero.Map.UI;
using SlotHero.Popup;
using SlotHero.Popup.UI;
using SlotHero.Profile;
using SlotHero.Profile.UI;
using SlotHero.RunResult;
using SlotHero.RunResult.UI;
using SlotHero.Settings;
using SlotHero.Settings.UI;
using SlotHero.Sanctum;
using SlotHero.Sanctum.UI;
using SlotHero.Title;
using SlotHero.Title.UI;
using SlotHero.TopBar.UI;
using UnityEngine;

namespace SlotHero.Tools
{
    /// <summary>
    /// 확인용 씬을 눌러 볼 수 있게 굴린다.
    /// 씬에 있는 화면 조종기를 찾아 보기 자료로 열고, 눌렀을 때 일어난 일을 기록에 남긴다.
    ///
    /// 플레이를 눌러야 움직인다. 눌린 결과는 콘솔 창에서 본다.
    /// **게임 코드가 아니다.** 실제 흐름이 생기면 이 파일은 지운다.
    /// </summary>
    public class PreviewDriver : MonoBehaviour
    {
        [Header("설정")]
        [Tooltip("런 종료 결과의 최종 구성을 만들 때 태그 이름을 여기서 가져온다.")]
        [SerializeField] private CurrentBuildVisualConfig _buildVisual;

        [SerializeField] private RunResultVisualConfig _runResultVisual;

        private void Start()
        {
            Run();
        }

        /// <summary>
        /// 씬에 있는 화면 조종기를 찾아 보기 자료로 연다.
        /// 플레이를 누르면 저절로 돌고, 에디터에서 확인할 때는 곧바로 부른다.
        /// </summary>
        public void Run()
        {
            DriveTitle();
            DriveProfile();
            DriveSettings();
            DriveEvent();
            DriveCurrentBuild();
            DriveRunResult();
            DrivePopup();
            DriveTopBar();
            DriveSanctum();
            DriveMap();
        }

        private void DriveMap()
        {
            MapScreenController map = FindAnyObjectByType<MapScreenController>();
            if (map == null)
            {
                return;
            }

            map.RoomEntered += node => Say("맵 · " + node.Stage + "단계 " + node.RoomType + " 방으로 들어간다");
            map.ScreenVisibilityChanged += visible => Say("맵 화면 " + (visible ? "열림" : "닫힘"));

            // 같은 시드는 어느 기기에서든 같은 지도가 나온다.
            map.StartNewStage(20260921, 1);
            map.Open();
        }

        private void DriveTopBar()
        {
            TopBarController topBar = FindAnyObjectByType<TopBarController>();
            if (topBar == null)
            {
                return;
            }

            topBar.ButtonClicked += kind => Say("상단 표시줄 · " + kind + " 를 눌렀다");

            topBar.SetElapsedSeconds(3580f);
            topBar.SetHealth(48, 99, false);
            topBar.SetGold(137, false);
        }

        private void DriveSanctum()
        {
            SanctumController sanctum = FindAnyObjectByType<SanctumController>();
            if (sanctum == null)
            {
                return;
            }

            sanctum.CampUsed += healed => Say("야영으로 체력 " + healed + " 을 회복했다");
            sanctum.Exited += () => Say("성소를 떠났다");
            sanctum.ActionFailed += result => Say("할 수 없다 · " + result);

            // 야영을 누르려면 성소에 들어와 있어야 한다.
            // 행상 목록과 체력은 성소 폴더의 보기용 구현을 쓴다.
            SamplePlayerVitals vitals = new SamplePlayerVitals(48, 99);
            RunGoldState gold = new RunGoldState();
            gold.ResetForNewRun(137);

            sanctum.Enter(20260921, 1, new SampleMerchantCatalog(), vitals, gold, new RunRelicPool());
            sanctum.Open();
        }

        private void DriveTitle()
        {
            TitleScreenController title = FindAnyObjectByType<TitleScreenController>();
            if (title == null)
            {
                return;
            }

            title.NewGameConfirmRequested += () => Say("새 게임 · 저장된 런이 있어 확인 팝업을 띄울 차례");
            title.NewGameStarted += () => Say("새 게임 · 바로 시작");
            title.ContinueRequested += () => Say("이어하기");
            title.SettingsRequested += () => Say("설정 화면으로");
            title.QuitConfirmRequested += () => Say("종료 · 확인 팝업을 띄울 차례");
            title.RunDataBrokenNotice += () => Say("런 데이터 손상 알림");

            title.MenuHoverChanged += (kind, hovering, reason) =>
            {
                if (hovering && !string.IsNullOrEmpty(reason))
                {
                    Say(kind + " 를 누를 수 없다 · " + reason);
                }
            };

            // 저장된 런이 있는 상태로 연다. 이어하기가 눌린다.
            title.Open(SavedRunState.Ready, "0.1");

            CurrentProfileButton profileButton = FindAnyObjectByType<CurrentProfileButton>();
            if (profileButton != null)
            {
                profileButton.Clicked += () => Say("현재 프로필 · 프로필 선택 화면으로");
            }
        }

        private void DriveProfile()
        {
            ProfileSelectScreenController profile = FindAnyObjectByType<ProfileSelectScreenController>();
            if (profile == null)
            {
                return;
            }

            profile.ProfileChosen += index => Say("프로필 " + index + " 번을 골랐다");
            profile.ProfileCreated += (index, name) => Say("프로필 " + index + " 번을 만들었다 · " + name);
            profile.ProfileRenamed += (index, name) => Say("프로필 " + index + " 번 이름을 고쳤다 · " + name);
            profile.ProfileDeleted += index => Say("프로필 " + index + " 번을 지웠다");
            profile.BackRequested += () => Say("뒤로 · 타이틀 화면으로");
            profile.BrokenProfileOpened += index => Say("프로필 " + index + " 번을 읽을 수 없다");

            // 삭제는 확인 팝업을 거치지만 여기서는 바로 지워 결과를 보인다.
            profile.DeleteRequested += index =>
            {
                Say("프로필 " + index + " 번 삭제 확인 팝업을 띄울 차례 · 여기서는 바로 지운다");
                profile.ConfirmDelete(index);
            };

            profile.Open(PreviewSamples.Profiles());
        }

        private void DriveSettings()
        {
            SettingsScreenController settings = FindAnyObjectByType<SettingsScreenController>();
            if (settings == null)
            {
                return;
            }

            settings.ValueChanged += (definition, value) =>
                Say("설정을 바꿨다 · " + definition.DisplayName + " = " + definition.FormatValue(value));
            settings.RestoreRequested += () =>
            {
                Say("기본값 복원 확인 팝업을 띄울 차례 · 여기서는 바로 되돌린다");
                settings.RestoreDefaults();
            };
            settings.Closed += () => Say("설정을 닫았다");

            settings.Open(new SettingsValues());
        }

        private void DriveEvent()
        {
            EventScreenController events = FindAnyObjectByType<EventScreenController>();
            if (events == null)
            {
                return;
            }

            events.ChoiceSelected += (eventId, choiceId) =>
            {
                Say("선택지를 골랐다 · " + choiceId);
                events.ApplyResult(PreviewSamples.EventResultFor(choiceId));
            };

            events.Open("preview", PreviewSamples.Event(), null);
        }

        private void DriveCurrentBuild()
        {
            CurrentBuildScreenController build = FindAnyObjectByType<CurrentBuildScreenController>();
            if (build == null)
            {
                return;
            }

            build.Opened += () => Say("현재 빌드를 열었다");
            build.Closed += () => Say("현재 빌드를 닫았다");

            build.Open(PreviewSamples.Build(), null);
        }

        private void DriveRunResult()
        {
            RunResultScreenController result = FindAnyObjectByType<RunResultScreenController>();
            if (result == null)
            {
                return;
            }

            result.RunDataDeleteRequested += () => Say("런 데이터를 지울 차례");
            result.TitleRequested += () => Say("타이틀로");

            result.Open(PreviewSamples.Result(_buildVisual, _runResultVisual));
        }

        private void DrivePopup()
        {
            PopupPresenter popup = FindAnyObjectByType<PopupPresenter>();
            if (popup == null)
            {
                return;
            }

            popup.Show(PreviewSamples.NewGamePopup(), id =>
            {
                Say("팝업에서 " + id + " 를 골랐다 · 다음은 런 종료 확인 팝업이다");

                // 버튼이 셋인 팝업도 눌러 볼 수 있게 이어서 띄운다.
                popup.Show(PreviewSamples.EndRunPopup(), second =>
                    Say("런 종료 확인에서 " + second + " 를 골랐다"));
            });
        }

        private static void Say(string message)
        {
            Debug.Log("[확인용] " + message);
        }
    }
}
