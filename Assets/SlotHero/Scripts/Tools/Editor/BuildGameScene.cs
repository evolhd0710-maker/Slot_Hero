using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
using SlotHero.Popup.UI;
using SlotHero.Profile.UI;
using SlotHero.Reward.UI;
using SlotHero.RunResult;
using SlotHero.RunResult.UI;
using SlotHero.Sanctum.UI;
using SlotHero.Save;
using SlotHero.Settings.UI;
using SlotHero.Title.UI;
using SlotHero.TopBar.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 실제로 돌아가는 게임 씬을 만든다.
    /// 메뉴 Slot Hero / 게임 씬 만들기 에서 실행한다.
    ///
    /// 화면을 새로 짓지 않는다. 확인용 씬에서 이미 만들어 둔 화면을 그대로 옮겨 온다.
    /// 화면 만드는 코드를 두 벌 두면 한쪽만 고쳐져 어긋나기 때문이다.
    /// 그러므로 **확인용 씬을 먼저 만들어 두어야 한다.**
    ///
    /// 옮겨 온 화면들을 캔버스 하나 아래 모으고 흐름 조종기에 물린다.
    /// 씬을 갈아 끼우지 않고 한 씬에서 켜고 끄는 방식이다.
    /// 설정과 팝업과 현재 빌드가 다른 화면 위에 겹쳐 떠야 하기 때문이다.
    /// </summary>
    public static class BuildGameScene
    {
        private const string SceneName = "게임";

        /// 배경 그림이 없을 때 쓰는 색. 확인용 씬과 같은 값이다.
        private static readonly Color DarkBackground = new Color(0.125f, 0.114f, 0.102f, 1f);

        [MenuItem("Slot Hero/게임 씬 만들기", priority = 1)]
        public static void Build()
        {
            Scene game = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 카메라가 없으면 유니티가 아무것도 그리지 않는다고 알린다.
            // 나중에 전투를 뒤에 얹을 때도 UI 와 세계가 같은 카메라를 봐야 앞뒤가 맞는다.
            Camera camera = UiSceneBuilder.CreateCamera("Main Camera", DarkBackground, true);

            Canvas canvas = UiSceneBuilder.CreateCanvas("GameCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, camera);
            UiSceneBuilder.CreateEventSystem();

            // 맨 뒤에 깔리는 배경. 늘 켜 두고 화면들이 그 위에 그려진다.
            BuildBackground(canvas);

            // 확인용 씬에서 화면을 통째로 가져온다. 여는 차례가 곧 그리는 차례다.
            // 뒤에 오는 것이 위에 그려지므로 겹쳐 뜨는 것을 뒤에 둔다.
            SanctumController sanctum = Take<SanctumController>(canvas, "성소 화면");
            EventScreenController events = Take<EventScreenController>(canvas, "11 이벤트 화면");
            RewardScreenController reward = Take<RewardScreenController>(canvas, "10 보상 화면");

            // **맵은 방 화면과 보상 화면보다 위에 둔다.**
            // 방 진행 중에 지도 버튼을 누르면 방 화면을 닫지 않고 그 위에 반투명 막과 지도를 띄운다.
            // 상단 표시줄과 현재 빌드와 설정과 팝업보다는 아래라 지도 위에서도 그것들이 눌린다.
            // 평소 맵 화면이 열릴 때는 방과 보상이 닫혀 있어 차례가 바뀌어도 보이는 것은 같다.
            MapScreenController map = Take<MapScreenController>(canvas, "09 맵 화면");

            RunResultScreenController runResult = Take<RunResultScreenController>(canvas, "12 런 종료 결과 화면");
            ProfileSelectScreenController profile = Take<ProfileSelectScreenController>(canvas, "06 프로필 선택 화면");
            TitleScreenController title = Take<TitleScreenController>(canvas, "05 타이틀 화면");

            TopBarController topBar = Take<TopBarController>(canvas, "03 상단 표시줄");
            RunHudController hud = BuildHud(canvas);

            CurrentBuildScreenController currentBuild =
                Take<CurrentBuildScreenController>(canvas, "13 현재 빌드 화면");

            // 고르기 화면은 방 화면과 현재 빌드 위, 아이템 상세 아래다. 고르는 칸에도 아이템 상세가 떠야 한다.
            ItemPickerScreenController picker = Take<ItemPickerScreenController>(canvas, "13 고르기 화면");

            // 아이템 상세는 현재 빌드 위에 뜬다. 설정과 팝업보다는 아래다. 그 둘이 열려 있으면 물건에 커서가 닿지 않는다.
            ItemDetailView itemDetail = Take<ItemDetailView>(canvas, "08 아이템 상세");

            SettingsScreenController settings = Take<SettingsScreenController>(canvas, "07 설정 화면");
            PopupPresenter popup = Take<PopupPresenter>(canvas, "08 화면 팝업");

            // 흐름을 맡는 오브젝트. 저장과 전투 대역도 여기 붙인다.
            GameObject flowGo = new GameObject("GameFlow");
            SceneManager.MoveGameObjectToScene(flowGo, game);

            SaveService save = flowGo.AddComponent<SaveService>();
            UiSceneBuilder.SetField(save, "_config",
                UiSceneBuilder.LoadConfig<SaveConfig>("Save", "SaveConfig"));

            PlaceholderCombatEntry combat = flowGo.AddComponent<PlaceholderCombatEntry>();
            UiSceneBuilder.SetField(combat, "_popup", popup);

            GameFlowController flow = flowGo.AddComponent<GameFlowController>();

            UiSceneBuilder.SetField(flow, "_save", save);
            UiSceneBuilder.SetField(flow, "_title", title);
            UiSceneBuilder.SetField(flow, "_profileSelect", profile);

            // 타이틀 화면 안에 들어 있는 현재 프로필 버튼.
            // 타이틀 씬에서 함께 넘어오므로 거기서 찾아 물린다.
            // 안 물리면 타이틀에서 프로필 선택 화면으로 갈 길이 없다.
            UiSceneBuilder.SetField(
                flow, "_currentProfileButton",
                title != null ? title.GetComponentInChildren<CurrentProfileButton>(true) : null);
            UiSceneBuilder.SetField(flow, "_map", map);
            UiSceneBuilder.SetField(flow, "_sanctum", sanctum);
            UiSceneBuilder.SetField(flow, "_event", events);
            UiSceneBuilder.SetField(flow, "_reward", reward);
            UiSceneBuilder.SetField(flow, "_runResult", runResult);
            UiSceneBuilder.SetField(flow, "_settings", settings);
            UiSceneBuilder.SetField(flow, "_currentBuild", currentBuild);
            UiSceneBuilder.SetField(flow, "_popup", popup);
            UiSceneBuilder.SetField(flow, "_itemDetail", itemDetail);
            UiSceneBuilder.SetField(flow, "_picker", picker);
            UiSceneBuilder.SetField(flow, "_topBar", topBar);
            UiSceneBuilder.SetField(flow, "_hud", hud);
            UiSceneBuilder.SetField(flow, "_combatEntryBehaviour", combat);

            UiSceneBuilder.SetField(flow, "_catalogConfig",
                UiSceneBuilder.LoadConfig<RunCatalogConfig>("Flow", "RunCatalogConfig"));
            UiSceneBuilder.SetField(flow, "_eventConfig",
                UiSceneBuilder.LoadConfig<RunEventConfig>("Flow", "RunEventConfig"));
            UiSceneBuilder.SetField(flow, "_eventIllustrations",
                UiSceneBuilder.LoadConfig<EventIllustrationConfig>("Events", "EventIllustrationConfig"));
            UiSceneBuilder.SetField(flow, "_rewardRules",
                UiSceneBuilder.LoadConfig<RewardRuleConfig>("Flow", "RewardRuleConfig"));
            UiSceneBuilder.SetField(flow, "_mapVisual",
                UiSceneBuilder.LoadConfig<MapVisualConfig>("Map", "MapVisualConfig"));
            UiSceneBuilder.SetField(flow, "_buildVisual",
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig"));
            UiSceneBuilder.SetField(flow, "_resultVisual",
                UiSceneBuilder.LoadConfig<RunResultVisualConfig>("RunResult", "RunResultVisualConfig"));

            // 화면을 다 닫아 둔다. 확인용 씬에서 열린 채로 넘어왔기 때문이다.
            // 플레이를 누르면 흐름 조종기가 프로필 선택부터 연다.
            CloseAll(canvas);

            // 칸을 다 닫기 전 자리 그대로 그림자를 맞춘다.
            UiSceneBuilder.SyncShadows();

            string path = UiSceneBuilder.SceneFolder + "/" + SceneName + ".unity";
            UiSceneBuilder.EnsureFolder(UiSceneBuilder.SceneFolder);
            EditorSceneManager.SaveScene(game, path);

            AddToBuildSettings(path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("게임 씬을 만들었다: " + path);
        }

        /// <summary>
        /// 확인용 씬을 잠깐 열어 그 안의 화면을 통째로 가져온다.
        ///
        /// 캔버스 아래 있는 것을 모두 옮긴다. 화면 하나가 오브젝트 여럿일 수 있기 때문이다.
        /// 상단 표시줄은 표시줄과 풍선이 따로 서 있다.
        /// 옮긴 뒤 원래 씬은 닫는다. 확인용 씬 자체는 그대로 남는다.
        /// </summary>
        /// <summary>
        /// 확인용 씬에 남아 있는 **예시 칸**을 치운다.
        ///
        /// 확인용 씬은 만들 때 `Show(PreviewSamples...)` 를 한 번 불러
        /// 선택지, 카드, 노드 같은 칸을 진짜로 만들어 씬에 저장해 둔다.
        /// 그대로 가져오면 게임에서 화면이 자기 칸을 새로 만들 때
        /// 예시 칸이 지워지지 않고 남아 **새 칸 뒤에 겹쳐 보인다.**
        ///
        /// 이벤트 방에서 `지나친다` 가 두 번 나온 것이 이것이었다.
        /// 화면이 들고 있는 목록은 저장되지 않으므로 화면 쪽에서는 예시 칸을 알 수가 없다.
        /// 칸 수가 우연히 같으면 딱 겹쳐 티가 안 날 뿐 어느 화면에나 있던 문제다.
        /// </summary>
        private static void ClearDemoItems(Transform root)
        {
            ClearAll<EventChoiceView>(root);
            ClearAll<RewardCardView>(root);
            ClearAll<ProfileCardView>(root);
            ClearAll<MapNodeView>(root);
            ClearAll<MapEdgeView>(root);
            ClearAll<PopupButtonView>(root);
            ClearAll<SettingsTabView>(root);
            ClearAll<SettingsRowView>(root);
            ClearAll<SettingsOptionView>(root);
            ClearAll<TitleMenuItemView>(root);
            ClearAll<BuildSlotView>(root);
            ClearAll<BuildTagBarView>(root);

            // 행상 진열 자리는 여기 넣지 않는다.
            // 다른 화면과 달리 자리 수가 설정에서 와 씬에 고정으로 박혀 있고,
            // `MerchantScreenView` 가 배열로 붙들고 있어 지우면 참조가 끊긴다.
            // 진열 내용은 성소에 들어갈 때 흐름이 진짜 목록으로 다시 채운다.
        }

        /// <summary>그 아래에 있는 칸을 전부 지운다.</summary>
        private static void ClearAll<T>(Transform root) where T : Component
        {
            T[] items = root.GetComponentsInChildren<T>(true);
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    Object.DestroyImmediate(items[i].gameObject);
                }
            }
        }

        private static T Take<T>(Canvas canvas, string sceneName) where T : Component
        {
            string path = UiSceneBuilder.SceneFolder + "/" + sceneName + ".unity";

            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning("확인용 씬이 없다. 먼저 만들어야 한다: " + path);
                return null;
            }

            Scene source = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            List<Transform> moved = new List<Transform>();
            GameObject[] roots = source.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                Canvas sourceCanvas = roots[i].GetComponent<Canvas>();
                if (sourceCanvas == null)
                {
                    // EventSystem 과 PreviewDriver 는 가져오지 않는다.
                    // 게임 씬에는 이미 하나씩 있거나 필요 없다.
                    continue;
                }

                Transform sourceRoot = sourceCanvas.transform;
                for (int j = sourceRoot.childCount - 1; j >= 0; j--)
                {
                    moved.Add(sourceRoot.GetChild(j));
                }
            }

            // 부모를 바꾸면 게임 씬으로 따라온다.
            // 뒤에서부터 모았으므로 다시 뒤집어 원래 차례를 지킨다.
            for (int i = moved.Count - 1; i >= 0; i--)
            {
                moved[i].SetParent(canvas.transform, false);
            }

            EditorSceneManager.CloseScene(source, true);

            // 확인용 씬에만 있던 배경을 치운다. 게임 씬에서는 화면 전체를 덮어 버린다.
            for (int i = 0; i < moved.Count; i++)
            {
                if (moved[i] == null)
                {
                    continue;
                }

                Transform backdrop = moved[i].Find("BehindScreen");
                if (backdrop != null)
                {
                    Object.DestroyImmediate(backdrop.gameObject);
                }

                ClearDemoItems(moved[i]);
            }

            T found = null;
            for (int i = 0; i < moved.Count; i++)
            {
                if (moved[i] == null)
                {
                    continue;
                }

                T candidate = moved[i].GetComponentInChildren<T>(true);
                if (candidate != null)
                {
                    found = candidate;
                    break;
                }
            }

            if (found == null)
            {
                Debug.LogWarning(sceneName + " 에서 " + typeof(T).Name + " 을 찾지 못했다.");
                return null;
            }

            Flatten(canvas, found.transform);
            return found;
        }

        /// <summary>
        /// 조종기가 붙은 오브젝트를 캔버스 바로 아래로 끌어올린다.
        ///
        /// 흐름 조종기는 화면을 `조종기.gameObject.SetActive` 로 켜고 끈다.
        /// 조종기가 중간 오브젝트 아래에 있으면 그 중간이 꺼져 있을 때 켜도 보이지 않는다.
        /// 상단 표시줄이 그렇다. TopBarScreen 아래에 TopBar 가 있고 조종기는 TopBar 에 붙어 있다.
        ///
        /// 중간 오브젝트는 캔버스에 꽉 차게 펼쳐져 있으므로 끌어올려도 자리가 그대로다.
        /// 중간에 남은 형제도 함께 올리고 빈 중간은 지운다.
        /// </summary>
        private static void Flatten(Canvas canvas, Transform target)
        {
            Transform middle = target.parent;

            if (middle == null || middle == canvas.transform)
            {
                return;
            }

            // 캔버스 바로 아래까지만 끌어올린다.
            if (middle.parent != canvas.transform)
            {
                return;
            }

            List<Transform> children = new List<Transform>();
            for (int i = 0; i < middle.childCount; i++)
            {
                children.Add(middle.GetChild(i));
            }

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetParent(canvas.transform, false);
            }

            Object.DestroyImmediate(middle.gameObject);
        }

        /// <summary>
        /// 현재 빌드 버튼과 자동 저장 표시를 만든다.
        /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 오른쪽 아래 고정 자리다.
        ///
        /// 이것만 확인용 씬이 따로 없어 여기서 짓는다.
        /// </summary>
        private static RunHudController BuildHud(Canvas canvas)
        {
            HudLayoutConfig layout = UiSceneBuilder.LoadConfig<HudLayoutConfig>("Hud", "HudLayoutConfig");
            HudVisualConfig visual = UiSceneBuilder.LoadConfig<HudVisualConfig>("Hud", "HudVisualConfig");

            RectTransform root = UiSceneBuilder.NewRect("RunHud", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 버튼. 자리는 RunHudController.ApplyLayout 이 잡는다.
            //
            // 뿌리와 배경을 같은 오브젝트에 두면 안 된다.
            // ApplySize 가 배경을 가운데로 맞추는데 그것이 곧 뿌리의 자리를 덮어써
            // 버튼이 화면 한가운데로 가 버린다.
            RectTransform buttonRoot = UiSceneBuilder.NewRect("CurrentBuildButton", root);
            RectTransform content = UiSceneBuilder.NewRect("Content", buttonRoot);

            Image background = UiSceneBuilder.NewButtonImage("Background", content, Color.white);
            Button button = UiSceneBuilder.AddButton(buttonRoot.gameObject, background);
            Image icon = UiSceneBuilder.NewFlatImage("Icon", content, Color.white);

            // 우주 바탕 원과 혼천의. 누를 수 없을 때는 채도를 뺀 것으로 바꾼다.
            // 이걸 안 물리면 9조각 버튼 그림과 빈 흰 네모가 그대로 나온다.
            Sprite backgroundSprite = ImportArt.LoadHud("현재빌드_버튼배경");
            Sprite iconSprite = ImportArt.LoadHud("현재빌드_아이콘");

            if (backgroundSprite != null)
            {
                background.sprite = backgroundSprite;
                background.type = Image.Type.Simple;
            }

            if (iconSprite != null)
            {
                icon.sprite = iconSprite;
            }

            icon.preserveAspect = true;

            CurrentBuildButton buildButton = buttonRoot.gameObject.AddComponent<CurrentBuildButton>();
            UiSceneBuilder.SetField(buildButton, "_button", button);
            UiSceneBuilder.SetField(buildButton, "_content", content);
            UiSceneBuilder.SetField(buildButton, "_background", background);
            UiSceneBuilder.SetField(buildButton, "_icon", icon);
            UiSceneBuilder.SetField(buildButton, "_backgroundSprite", backgroundSprite);
            UiSceneBuilder.SetField(buildButton, "_backgroundDisabledSprite",
                ImportArt.LoadHud("현재빌드_버튼배경_비활성"));
            UiSceneBuilder.SetField(buildButton, "_iconSprite", iconSprite);
            UiSceneBuilder.SetField(buildButton, "_iconDisabledSprite",
                ImportArt.LoadHud("현재빌드_아이콘_비활성"));
            UiSceneBuilder.SetField(buildButton, "_visual", visual);

            // 자동 저장 표시.
            RectTransform noticeRect = UiSceneBuilder.NewRect("AutoSaveNotice", root);
            CanvasGroup group = noticeRect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            TMP_Text noticeLabel = UiSceneBuilder.NewText(
                "Label", noticeRect, visual.AutoSaveText, visual.AutoSaveFontSize,
                visual.AutoSaveColor, TextAlignmentOptions.Right, false, true);
            UiSceneBuilder.Stretch(noticeLabel.rectTransform);

            AutoSaveNotice notice = noticeRect.gameObject.AddComponent<AutoSaveNotice>();
            UiSceneBuilder.SetField(notice, "_group", group);
            UiSceneBuilder.SetField(notice, "_label", noticeLabel);
            UiSceneBuilder.SetField(notice, "_visual", visual);

            RunHudController controller = root.gameObject.AddComponent<RunHudController>();
            UiSceneBuilder.SetField(controller, "_layout", layout);
            UiSceneBuilder.SetField(controller, "_currentBuildButton", buildButton);
            UiSceneBuilder.SetField(controller, "_autoSaveNotice", notice);

            controller.ApplyLayout();
            return controller;
        }

        /// <summary>
        /// 맨 뒤 배경을 깐다. 돌벽 위에 검정 50퍼센트다.
        ///
        /// 와이어프레임 `10 맵 화면`, `12 이벤트`, `13 런 종료 결과` 가 이것을 쓴다.
        /// 예전에는 돌바닥을 깔았는데 **화면마다 어두운 바탕을 따로 깔아 덮고 있어
        /// 그림이 한 번도 보인 적이 없다.** 그것이 맵과 이벤트와 결과 화면의 까만 부분이었다.
        /// 이제 확인용 씬의 그 바탕은 `BehindScreen` 이라 옮길 때 지워지고 이것이 보인다.
        /// </summary>
        private static void BuildBackground(Canvas canvas)
        {
            UiSceneBuilder.NewStoneBackdrop("RunBackground", canvas.transform);
        }

        /// <summary>캔버스 아래 화면을 전부 끈다. 맨 뒤 배경만 남긴다.</summary>
        private static void CloseAll(Canvas canvas)
        {
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform child = canvas.transform.GetChild(i);

                if (child.name == "RunBackground")
                {
                    continue;
                }

                // 껍데기만 늘 켜 두고 그 안을 조종기가 켜고 끄는 화면들이다.
                // 껍데기까지 끄면 조종기가 켜는 안쪽 칸이 꺼진 부모 밑에 남아
                // **아무리 켜도 화면에 나오지 않는다.**
                // 성소는 성소 칸과 행상 칸을, 팝업은 팝업 칸을,
                // 호버 풍선은 글자 칸을 그렇게 켜고 끈다.
                //
                // 빠뜨린 것이 있는지는 `게임 씬 구조 감사` 가 찾아 준다.
                if (IsShell(child.name))
                {
                    CloseChildren(child);
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        /// 안쪽 칸을 조종기가 켜고 끄므로 껍데기는 늘 켜 둬야 하는 것들.
        private static readonly string[] Shells = { "SanctumScreen", "Popup", "Tooltip", "ItemDetail" };

        /// <summary>껍데기를 켜 둔 채로 안쪽만 꺼야 하는 화면인지.</summary>
        private static bool IsShell(string name)
        {
            for (int i = 0; i < Shells.Length; i++)
            {
                if (Shells[i] == name)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>그 아래 칸만 모두 끈다. 껍데기는 그대로 둔다.</summary>
        private static void CloseChildren(Transform root)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                root.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>빌드 설정에 게임 씬을 첫 자리로 넣는다.</summary>
        private static void AddToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene(path, true));

            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i].path != path)
                {
                    scenes.Add(current[i]);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
