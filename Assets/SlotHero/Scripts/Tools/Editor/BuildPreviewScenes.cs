using System.Collections.Generic;
using SlotHero.CurrentBuild;
using SlotHero.CurrentBuild.UI;
using SlotHero.Events;
using SlotHero.Events.UI;
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
using SlotHero.Settings;
using SlotHero.Settings.UI;
using SlotHero.Title;
using SlotHero.Title.UI;
using SlotHero.Tools;
using SlotHero.TopBar;
using SlotHero.TopBar.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SlotHero.Ui;
using SlotHero.Combat;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 화면을 눈으로 확인하려고 씬을 만든다.
    /// 메뉴 Slot Hero / 확인용 씬 만들기 에서 실행한다.
    ///
    /// 각 영역 README 의 씬 구성을 그대로 옮긴 것이다.
    /// 만든 씬은 Assets/SlotHeroScenes 에 들어간다.
    ///
    /// 그림이 아직 없어 로고와 배경은 단색으로 둔다.
    /// 자리와 크기, 글자가 기획서대로 나오는지 보는 데 쓴다.
    /// </summary>
    public static class BuildPreviewScenes
    {
        /// 배경이 없을 때 까는 색. 와이어프레임의 어두운 바탕을 흉내 낸 것이다.
        private static readonly Color DarkBackground = new Color(0.125f, 0.114f, 0.102f, 1f);

        /// 타이틀 배경 대신 까는 색. 배경 요청 기획서의 지구라트 딥 브라운 `#4A3A28` 이다.
        /// 발주한 그림이 오면 이 색은 쓰이지 않는다.
        private static readonly Color TitleStandInBackground =
            new Color(0.290f, 0.227f, 0.157f, 1f);

        /// 로고 자리에 까는 칸. 위 색보다 조금 밝게 해 자리가 보이게 한다.
        private static readonly Color TitleStandInLogoPanel =
            new Color(0.290f, 0.227f, 0.157f, 0.85f);

        /// 같은 기획서의 틈새 빛 앰버 골드 `#E8C87A`. 로고 글자에 쓴다.
        private static readonly Color TitleStandInAccent =
            new Color(0.910f, 0.784f, 0.478f, 1f);

        /// 타이틀 임시 배경 위에 얹는 어둡게 하기. 와이어프레임 `06 타이틀 화면` 의 `배경 · 검정 35%` 다.
        private const float TitleTemporaryShade = 0.35f;

        /// 행상의 문양 변경 자리에 까는 색. `행상 예시.psd` 의 보라 소용돌이에서 잰 `#6B3FA0` 이다.
        /// 양탄자에 그려진 무늬가 살짝 비치게 조금 투명하게 둔다.
        /// 그림이 오면 이 색은 쓰이지 않는다.
        private static readonly Color SymbolChangeStandIn =
            new Color(0.420f, 0.247f, 0.627f, 0.88f);

        [MenuItem("Slot Hero/확인용 씬 만들기/모두 다시 만들기", priority = 0)]
        public static void BuildAll()
        {
            BuildTopBar();
            BuildTitle();
            BuildProfileSelect();
            BuildSettings();
            BuildPopup();
            BuildItemDetail();
            BuildMap();
            BuildEvent();
            BuildReward();
            BuildRunResult();
            BuildCurrentBuild();
            BuildItemPicker();
            BuildSanctum();
            BuildMerchant();

            Debug.Log("확인용 씬을 모두 다시 만들었다.");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/타이틀 화면")]
        public static void BuildTitle()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("TitleCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            TitleLayoutConfig layout =
                UiSceneBuilder.LoadConfig<TitleLayoutConfig>("Title", "TitleLayoutConfig");
            TitleVisualConfig visual =
                UiSceneBuilder.LoadConfig<TitleVisualConfig>("Title", "TitleVisualConfig");

            GameObject menuPrefab = BuildTitleMenuItemPrefab(visual);

            RectTransform root = UiSceneBuilder.NewRect("TitleScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 배경은 세 갈래로 고른다.
            //
            // 1. 배경 요청 기획서로 발주한 "메인 화면 배경 · 고대 유적 슬롯". 아직 안 왔다
            // 2. 와이어프레임 `06 타이틀 화면` 이 쓴 임시 그림 `배경 · 폐허.png`.
            //    그 위에 `배경 · 검정 35%` 를 얹는다. 와이어프레임이 그렇게 했다
            // 3. 둘 다 없으면 배경 요청 기획서가 정한 딥 브라운 한 색
            //
            // 검은 화면으로 두면 그림이 빠진 것인지 깨진 것인지 가릴 수가 없다.
            Sprite titleBackground = ImportArt.LoadTitle("배경_메인화면");
            bool temporary = false;

            if (titleBackground == null)
            {
                titleBackground = ImportArt.LoadTitle("배경_임시_폐허");
                temporary = titleBackground != null;
            }

            Image background = titleBackground != null
                ? UiSceneBuilder.NewFlatImage("Background", root, Color.white)
                : UiSceneBuilder.NewFlatImage("Background", root, TitleStandInBackground);
            background.sprite = titleBackground;
            background.type = Image.Type.Simple;
            UiSceneBuilder.Stretch(background.rectTransform);

            if (temporary)
            {
                Image shade = UiSceneBuilder.NewFlatImage(
                    "Shade", background.transform, new Color(0f, 0f, 0f, TitleTemporaryShade));
                UiSceneBuilder.Stretch(shade.rectTransform);
                shade.raycastTarget = false;
            }

            // 로고 그림도 없다. 글자로 자리를 채운다.
            //
            // 배경 그림이 깔려 있으면 와이어프레임처럼 칸 없이 흰 글자만 둔다.
            // 그림 위에 칸을 얹으면 덧댄 판처럼 떠 보인다.
            // 배경이 한 색일 때만 발주 색의 칸과 앰버 글자로 자리를 알아보게 한다.
            Sprite titleLogo = ImportArt.LoadTitle("로고");
            bool onPicture = titleBackground != null;

            Image logo;

            if (titleLogo != null)
            {
                logo = UiSceneBuilder.NewFlatImage("Logo", root, Color.white);
            }
            else if (onPicture)
            {
                logo = UiSceneBuilder.NewFlatImage("Logo", root, new Color(1f, 1f, 1f, 0f));
            }
            else
            {
                logo = UiSceneBuilder.NewImage("Logo", root, TitleStandInLogoPanel);
            }

            logo.sprite = titleLogo;
            logo.raycastTarget = false;

            if (titleLogo == null)
            {
                TMP_Text logoLabel = UiSceneBuilder.NewText(
                    "LogoLabel", logo.transform, "SLOT HERO", UiScale.Px(64f),
                    onPicture ? Color.white : TitleStandInAccent, TextAlignmentOptions.Left, true);
                UiSceneBuilder.Stretch(logoLabel.rectTransform);
            }

            RectTransform menuLayer = UiSceneBuilder.NewRect("MenuLayer", root);
            UiSceneBuilder.Stretch(menuLayer);

            TMP_Text version = UiSceneBuilder.NewText(
                "VersionLabel", root, "v0.1", UiScale.Px(20f), Color.white, TextAlignmentOptions.Right);

            TitleScreenView view = root.gameObject.AddComponent<TitleScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_logo", logo);
            UiSceneBuilder.SetField(view, "_menuLayer", menuLayer);
            UiSceneBuilder.SetField(view, "_menuPrefab", menuPrefab.GetComponent<TitleMenuItemView>());
            UiSceneBuilder.SetField(view, "_versionLabel", version);

            TitleScreenController controller = root.gameObject.AddComponent<TitleScreenController>();
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetField(controller, "_visual", visual);
            UiSceneBuilder.SetBoolField(controller, "_hideOnAwake", false);

            // 오른쪽 위의 현재 프로필 버튼은 Profile 폴더가 맡는다.
            BuildCurrentProfileButton(root, "프로필1");

            view.ShowMenu();
            view.ApplyLayout();
            view.SetVersion("0.1");

            AddDriver();
            SaveScene(scene, "05 타이틀 화면");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/프로필 선택 화면")]
        public static void BuildProfileSelect()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("ProfileCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            ProfileLayoutConfig layout =
                UiSceneBuilder.LoadConfig<ProfileLayoutConfig>("Profile", "ProfileLayoutConfig");
            ProfileVisualConfig visual =
                UiSceneBuilder.LoadConfig<ProfileVisualConfig>("Profile", "ProfileVisualConfig");

            GameObject cardPrefab = BuildProfileCardPrefab(layout, visual);

            RectTransform root = UiSceneBuilder.NewRect("ProfileSelectScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            Image background = UiSceneBuilder.NewFlatImage("Background", root, visual.BackgroundColor);
            UiSceneBuilder.Stretch(background.rectTransform);

            Image titlePanel = UiSceneBuilder.NewImage("TitlePanel", root, visual.PanelColor);
            TMP_Text titleLabel = UiSceneBuilder.NewText(
                "TitleLabel", titlePanel.transform, visual.TitleText,
                visual.TitleFontSize, visual.NameColor, TextAlignmentOptions.Left, true);
            InsetText(titleLabel.rectTransform, UiScale.Px(30f));

            RectTransform cardLayer = UiSceneBuilder.NewRect("CardLayer", root);
            UiSceneBuilder.Stretch(cardLayer);

            Image backPanel = UiSceneBuilder.NewButtonImage("BackPanel", root, visual.PanelColor);
            UiSceneBuilder.AddButton(backPanel.gameObject);
            TMP_Text backLabel = UiSceneBuilder.NewText(
                "BackLabel", backPanel.transform, visual.BackText,
                visual.BackFontSize, visual.NameColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(backLabel.rectTransform);

            ProfileSelectScreenView view = root.gameObject.AddComponent<ProfileSelectScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_titlePanel", titlePanel);
            UiSceneBuilder.SetField(view, "_titleLabel", titleLabel);
            UiSceneBuilder.SetField(view, "_cardLayer", cardLayer);
            UiSceneBuilder.SetField(view, "_cardPrefab", cardPrefab.GetComponent<ProfileCardView>());
            UiSceneBuilder.SetField(view, "_backButton", backPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_backPanel", backPanel);
            UiSceneBuilder.SetField(view, "_backLabel", backLabel);

            ProfileSelectScreenController controller =
                root.gameObject.AddComponent<ProfileSelectScreenController>();
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetBoolField(controller, "_hideOnAwake", false);

            view.ApplyLayout();
            view.Show(PreviewSamples.Profiles());

            AddDriver();
            SaveScene(scene, "06 프로필 선택 화면");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/설정 화면")]
        public static void BuildSettings()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("SettingsCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            SettingsLayoutConfig layout =
                UiSceneBuilder.LoadConfig<SettingsLayoutConfig>("Settings", "SettingsLayoutConfig");
            SettingsVisualConfig visual =
                UiSceneBuilder.LoadConfig<SettingsVisualConfig>("Settings", "SettingsVisualConfig");
            SettingsCatalogConfig catalog =
                UiSceneBuilder.LoadConfig<SettingsCatalogConfig>("Settings", "SettingsCatalogConfig");

            GameObject tabPrefab = BuildSettingsTabPrefab(visual);
            GameObject rowPrefab = BuildSettingsRowPrefab(layout, visual);
            GameObject optionPrefab = BuildSettingsOptionPrefab(visual);

            RectTransform root = UiSceneBuilder.NewRect("SettingsScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 설정은 다른 화면 위에 열리므로 뒤에 깔릴 화면을 흉내 낸 바탕을 둔다.
            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", root, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);

            Image dim = UiSceneBuilder.NewFlatImage("Dim", root, visual.DimColor);
            UiSceneBuilder.Stretch(dim.rectTransform);

            Image titlePanel = UiSceneBuilder.NewImage("TitlePanel", root, visual.PanelColor);
            TMP_Text titleLabel = UiSceneBuilder.NewText(
                "TitleLabel", titlePanel.transform, visual.TitleText,
                visual.TitleFontSize, visual.ItemTextColor, TextAlignmentOptions.Left, true);
            InsetText(titleLabel.rectTransform, UiScale.Px(30f));

            Image tabPanel = UiSceneBuilder.NewImage("TabPanel", root, visual.PanelColor);
            RectTransform tabLayer = UiSceneBuilder.NewRect("TabLayer", root);
            UiSceneBuilder.Stretch(tabLayer);

            Image listPanel = UiSceneBuilder.NewImage("ListPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(listPanel, 14f, 8f);
            RectTransform listViewport = UiSceneBuilder.NewRect("ListViewport", root);
            listViewport.gameObject.AddComponent<RectMask2D>();
            RectTransform listContent = UiSceneBuilder.NewRect("ListContent", listViewport);
            UiSceneBuilder.Stretch(listContent);

            ScrollRect listScroll = UiSceneBuilder.AddScroll(listViewport, listContent);

            Image restorePanel = UiSceneBuilder.NewButtonImage("RestorePanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(restorePanel, 8f, 5f);
            UiSceneBuilder.AddButton(restorePanel.gameObject);
            TMP_Text restoreLabel = UiSceneBuilder.NewText(
                "RestoreLabel", restorePanel.transform, visual.RestoreText,
                visual.ButtonFontSize, visual.ItemTextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(restoreLabel.rectTransform);

            Image closePanel = UiSceneBuilder.NewButtonImage("ClosePanel", root, visual.PanelColor);
            UiSceneBuilder.AddButton(closePanel.gameObject);
            TMP_Text closeLabel = UiSceneBuilder.NewText(
                "CloseLabel", closePanel.transform, visual.CloseText,
                visual.ButtonFontSize, visual.ItemTextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(closeLabel.rectTransform);

            // 드롭다운 층은 맨 마지막에 두어 다른 칸보다 위에 그린다.
            // 바깥을 누르면 접히도록 투명한 막이 층 전체를 덮는다.
            RectTransform dropdownLayer = UiSceneBuilder.NewRect("DropdownLayer", root);
            UiSceneBuilder.Stretch(dropdownLayer);
            Image dropdownBlockerImage = UiSceneBuilder.NewFlatImage(
                "DropdownBlocker", dropdownLayer, new Color(0f, 0f, 0f, 0f));
            UiSceneBuilder.Stretch(dropdownBlockerImage.rectTransform);
            Button dropdownBlocker = dropdownBlockerImage.gameObject.AddComponent<Button>();
            dropdownBlocker.transition = Selectable.Transition.None;
            Image dropdownPanel = UiSceneBuilder.NewImage("DropdownPanel", dropdownLayer, visual.DropdownPanelColor);
            UiSceneBuilder.AddShadowPx(dropdownPanel, 8f, 5f);
            dropdownPanel.gameObject.AddComponent<RectMask2D>();

            SettingsScreenView view = root.gameObject.AddComponent<SettingsScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_dim", dim);
            UiSceneBuilder.SetField(view, "_titlePanel", titlePanel);
            UiSceneBuilder.SetField(view, "_titleLabel", titleLabel);
            UiSceneBuilder.SetField(view, "_tabPanel", tabPanel);
            UiSceneBuilder.SetField(view, "_tabLayer", tabLayer);
            UiSceneBuilder.SetField(view, "_tabPrefab", tabPrefab.GetComponent<SettingsTabView>());
            UiSceneBuilder.SetField(view, "_listPanel", listPanel);
            UiSceneBuilder.SetField(view, "_listScroll", listScroll);
            UiSceneBuilder.SetField(view, "_listViewport", listViewport);
            UiSceneBuilder.SetField(view, "_listContent", listContent);
            UiSceneBuilder.SetField(view, "_rowPrefab", rowPrefab.GetComponent<SettingsRowView>());
            UiSceneBuilder.SetField(view, "_dropdownLayer", dropdownLayer.gameObject);
            UiSceneBuilder.SetField(view, "_dropdownBlocker", dropdownBlocker);
            UiSceneBuilder.SetField(view, "_dropdownPanel", dropdownPanel);
            UiSceneBuilder.SetField(view, "_optionPrefab", optionPrefab.GetComponent<SettingsOptionView>());
            UiSceneBuilder.SetField(view, "_restoreButton", restorePanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_restorePanel", restorePanel);
            UiSceneBuilder.SetField(view, "_restoreLabel", restoreLabel);
            UiSceneBuilder.SetField(view, "_closeButton", closePanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_closePanel", closePanel);
            UiSceneBuilder.SetField(view, "_closeLabel", closeLabel);

            SettingsScreenController controller = root.gameObject.AddComponent<SettingsScreenController>();
            UiSceneBuilder.SetField(controller, "_screenRoot", root.gameObject);
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetField(controller, "_catalog", catalog);

            view.ApplyLayout();
            dropdownLayer.gameObject.SetActive(false);

            if (catalog != null && catalog.TabCount > 0)
            {
                SettingsValues values = new SettingsValues();
                values.FillMissing(catalog.GetTab(0).Items);

                view.ShowTabs(catalog, 0);
                view.ShowItems(catalog.GetTab(0), values);
            }

            AddDriver();
            SaveScene(scene, "07 설정 화면");

            // 사운드 탭 모습도 따로 남긴다. 막대와 음소거 버튼을 눈으로 확인하는 용도다.
            SettingsTab sound = catalog != null ? catalog.FindTab("sound") : null;
            if (sound != null)
            {
                SettingsValues soundValues = new SettingsValues();
                soundValues.FillMissing(catalog.CollectAllItems());
                soundValues.Set(catalog.FindItem("bgmVolume"), 60f);
                soundValues.Set(catalog.FindItem("ambienceVolume"), 35f);
                soundValues.Set(catalog.FindItem("sfxVolumeMute"), 1f);

                view.ShowTabs(catalog, catalog.Tabs.IndexOf(sound));
                view.ShowItems(sound, soundValues);
                SaveScene(scene, "07 설정 화면 사운드");
            }
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/런 종료 결과 화면")]
        public static void BuildRunResult()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("RunResultCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            RunResultLayoutConfig layout =
                UiSceneBuilder.LoadConfig<RunResultLayoutConfig>("RunResult", "RunResultLayoutConfig");
            RunResultVisualConfig visual =
                UiSceneBuilder.LoadConfig<RunResultVisualConfig>("RunResult", "RunResultVisualConfig");

            RectTransform root = UiSceneBuilder.NewRect("RunResultScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 확인용 씬에서만 보이는 돌벽. 게임 씬에서는 지워지고 공용 배경이 보인다.
            // 와이어프레임 `13 런 종료 결과` 가 돌벽 위에 검정 50퍼센트를 쓴다.
            // 뷰의 `_background` 에는 물리지 않는다. 물리면 뷰가 설정의 색으로 덮어 칠한다.
            UiSceneBuilder.NewStoneBackdrop("BehindScreen", root);

            Image outcomePanel = UiSceneBuilder.NewImage("OutcomePanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(outcomePanel, 8f, 5f);
            TMP_Text outcomeLabel = UiSceneBuilder.NewText(
                "OutcomeLabel", outcomePanel.transform, visual.DefeatText,
                visual.OutcomeFontSize, visual.DefeatColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(outcomeLabel.rectTransform);

            Image locationPanel = UiSceneBuilder.NewImage("LocationPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(locationPanel, 8f, 5f);
            TMP_Text locationLabel = UiSceneBuilder.NewText(
                "LocationLabel", locationPanel.transform, string.Empty,
                visual.LocationFontSize, visual.TextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(locationLabel.rectTransform);

            RunResultPanelView finalBuild = BuildRunResultPanel("FinalBuildPanel", root, visual);
            RunResultPanelView runRecord = BuildRunResultPanel("RunRecordPanel", root, visual);
            RunResultPanelView unlock = BuildRunResultPanel("UnlockPanel", root, visual);

            Image titleButtonPanel = UiSceneBuilder.NewButtonImage("TitleButtonPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(titleButtonPanel, 8f, 5f);
            UiSceneBuilder.AddButton(titleButtonPanel.gameObject);
            TMP_Text titleButtonLabel = UiSceneBuilder.NewText(
                "TitleButtonLabel", titleButtonPanel.transform, visual.TitleButtonText,
                visual.ButtonFontSize, visual.TextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(titleButtonLabel.rectTransform);

            RunResultScreenView view = root.gameObject.AddComponent<RunResultScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_outcomePanel", outcomePanel);
            UiSceneBuilder.SetField(view, "_outcomeLabel", outcomeLabel);
            UiSceneBuilder.SetField(view, "_locationPanel", locationPanel);
            UiSceneBuilder.SetField(view, "_locationLabel", locationLabel);
            UiSceneBuilder.SetField(view, "_finalBuildPanel", finalBuild);
            UiSceneBuilder.SetField(view, "_runRecordPanel", runRecord);
            UiSceneBuilder.SetField(view, "_unlockPanel", unlock);
            UiSceneBuilder.SetField(view, "_titleButton", titleButtonPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_titleButtonPanel", titleButtonPanel);
            UiSceneBuilder.SetField(view, "_titleButtonLabel", titleButtonLabel);

            RunResultScreenController controller =
                root.gameObject.AddComponent<RunResultScreenController>();
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetBoolField(controller, "_hideOnAwake", false);

            CurrentBuildVisualConfig buildVisual =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            view.ApplyLayout();
            view.Show(PreviewSamples.Result(buildVisual, visual));

            AddDriver();
            SaveScene(scene, "12 런 종료 결과 화면");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/보상 화면")]
        public static void BuildReward()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("RewardCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            RewardLayoutConfig layout =
                UiSceneBuilder.LoadConfig<RewardLayoutConfig>("Reward", "RewardLayoutConfig");
            RewardVisualConfig visual =
                UiSceneBuilder.LoadConfig<RewardVisualConfig>("Reward", "RewardVisualConfig");
            CurrentBuildVisualConfig buildVisual =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            GameObject cardPrefab = BuildRewardCardPrefab(layout, visual);

            RectTransform root = UiSceneBuilder.NewRect("RewardScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            Image dim = UiSceneBuilder.NewFlatImage("Dim", root, visual.DimColor);
            UiSceneBuilder.Stretch(dim.rectTransform);

            Image box = UiSceneBuilder.NewImage("RewardBox", root, visual.BoxColor);
            UiSceneBuilder.AddShadowPx(box, 14f, 8f);

            TMP_Text titleLabel = UiSceneBuilder.NewText(
                "TitleLabel", root, visual.TitleText,
                visual.TitleFontSize, visual.TextColor, TextAlignmentOptions.Left, true);

            Image baseRow = UiSceneBuilder.NewImage("BaseGoldRow", root, visual.GoldRowColor);
            TMP_Text baseLabel = UiSceneBuilder.NewText(
                "BaseGoldLabel", baseRow.transform, visual.BaseGoldText,
                visual.GoldLabelFontSize, visual.TextColor, TextAlignmentOptions.Left, true);
            Image baseIcon = UiSceneBuilder.NewFlatImage(
                "BaseGoldIcon", baseRow.transform, Color.white);
            baseIcon.sprite = ImportArt.LoadTopBar("아이콘_골드");
            TMP_Text baseValue = UiSceneBuilder.NewText(
                "BaseGoldValue", baseRow.transform, "0",
                visual.GoldValueFontSize, visual.TextColor, TextAlignmentOptions.Left, true);

            Image overkillRow = UiSceneBuilder.NewImage("OverkillGoldRow", root, visual.GoldRowColor);
            TMP_Text overkillLabel = UiSceneBuilder.NewText(
                "OverkillGoldLabel", overkillRow.transform, visual.OverkillGoldText,
                visual.GoldLabelFontSize, visual.TextColor, TextAlignmentOptions.Left, true);
            Image overkillIcon = UiSceneBuilder.NewFlatImage(
                "OverkillGoldIcon", overkillRow.transform, Color.white);
            overkillIcon.sprite = ImportArt.LoadTopBar("아이콘_골드");
            TMP_Text overkillValue = UiSceneBuilder.NewText(
                "OverkillGoldValue", overkillRow.transform, "0",
                visual.GoldValueFontSize, visual.TextColor, TextAlignmentOptions.Left, true);

            RectTransform cardLayer = UiSceneBuilder.NewRect("CardLayer", root);
            UiSceneBuilder.Stretch(cardLayer);

            Image skipPanel = UiSceneBuilder.NewButtonImage("SkipPanel", root, visual.SkipColor);
            UiSceneBuilder.AddShadowPx(skipPanel, 8f, 5f);
            Button skipButton = UiSceneBuilder.AddButton(skipPanel.gameObject);
            TMP_Text skipLabel = UiSceneBuilder.NewText(
                "SkipLabel", skipPanel.transform, visual.SkipText,
                visual.SkipFontSize, visual.TextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(skipLabel.rectTransform);

            RewardScreenView view = root.gameObject.AddComponent<RewardScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_buildVisual", buildVisual);
            UiSceneBuilder.SetField(view, "_dim", dim);
            UiSceneBuilder.SetField(view, "_box", box);
            UiSceneBuilder.SetField(view, "_titleLabel", titleLabel);
            UiSceneBuilder.SetField(view, "_baseGoldRow", baseRow);
            UiSceneBuilder.SetField(view, "_baseGoldLabel", baseLabel);
            UiSceneBuilder.SetField(view, "_baseGoldValue", baseValue);
            UiSceneBuilder.SetField(view, "_baseGoldIcon", baseIcon);
            UiSceneBuilder.SetField(view, "_overkillGoldRow", overkillRow);
            UiSceneBuilder.SetField(view, "_overkillGoldLabel", overkillLabel);
            UiSceneBuilder.SetField(view, "_overkillGoldValue", overkillValue);
            UiSceneBuilder.SetField(view, "_overkillGoldIcon", overkillIcon);
            UiSceneBuilder.SetField(view, "_cardLayer", cardLayer);
            UiSceneBuilder.SetField(view, "_cardPrefab", cardPrefab.GetComponent<RewardCardView>());
            UiSceneBuilder.SetField(view, "_skipPanel", skipPanel);
            UiSceneBuilder.SetField(view, "_skipButton", skipButton);
            UiSceneBuilder.SetField(view, "_skipLabel", skipLabel);

            RewardScreenController controller = root.gameObject.AddComponent<RewardScreenController>();
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetBoolField(controller, "_hideOnAwake", false);

            view.Show(PreviewSamples.Reward(), null);

            AddDriver();
            SaveScene(scene, "10 보상 화면");
        }

        /// <summary>보상 카드 프리팹.</summary>
        private static GameObject BuildRewardCardPrefab(
            RewardLayoutConfig layout, RewardVisualConfig visual)
        {
            GameObject go = new GameObject("RewardCard", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            RectTransform rect = (RectTransform)go.transform;
            rect.sizeDelta = layout.CardSize;

            // 테두리가 배경보다 뒤에 있어야 고른 카드에서 테두리만 삐져나와 보인다.
            Image border = UiSceneBuilder.NewImage("Border", go.transform, visual.SelectedBorderColor);
            UiSceneBuilder.Stretch(border.rectTransform);

            Image background = UiSceneBuilder.NewButtonImage("Background", go.transform, visual.CardColor);
            RectTransform backgroundRect = background.rectTransform;
            UiSceneBuilder.Stretch(backgroundRect);
            float inset = visual.SelectedBorderThickness;
            backgroundRect.offsetMin = new Vector2(inset, inset);
            backgroundRect.offsetMax = new Vector2(-inset, -inset);

            Button button = UiSceneBuilder.AddButton(go, background);

            Image icon = UiSceneBuilder.NewFlatImage("Icon", go.transform, Color.white);
            PlaceTopLeftInCard(
                icon.rectTransform, layout,
                (layout.CardSize.x - layout.CardIconSize) * 0.5f, layout.CardIconTop,
                layout.CardIconSize, layout.CardIconSize);

            TMP_Text nameLabel = UiSceneBuilder.NewText(
                "NameLabel", go.transform, "이름", visual.CardNameFontSize,
                visual.CardTextColor, TextAlignmentOptions.Center, true);
            PlaceTopLeftInCard(
                nameLabel.rectTransform, layout,
                0f, layout.CardIconTop + layout.CardIconSize + UiScale.Px(10f), layout.CardSize.x, UiScale.Px(40f));

            TMP_Text tagLabel = UiSceneBuilder.NewText(
                "TagLabel", go.transform, "태그", visual.CardTagFontSize,
                visual.CardTextColor, TextAlignmentOptions.Center);
            PlaceTopLeftInCard(
                tagLabel.rectTransform, layout,
                0f, layout.CardIconTop + layout.CardIconSize + UiScale.Px(50f), layout.CardSize.x, UiScale.Px(36f));

            RewardCardView view = go.AddComponent<RewardCardView>();
            UiSceneBuilder.SetField(view, "_button", button);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_border", border);
            UiSceneBuilder.SetField(view, "_icon", icon);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_tagLabel", tagLabel);

            return UiSceneBuilder.SavePrefab(go, "RewardCard");
        }

        /// <summary>카드 안에서 왼쪽 위를 기준으로 자리를 잡는다.</summary>
        private static void PlaceTopLeftInCard(
            RectTransform rect, RewardLayoutConfig layout, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, -y);
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/이벤트 화면")]
        public static void BuildEvent()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("EventCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            EventLayoutConfig layout =
                UiSceneBuilder.LoadConfig<EventLayoutConfig>("Events", "EventLayoutConfig");
            EventVisualConfig visual =
                UiSceneBuilder.LoadConfig<EventVisualConfig>("Events", "EventVisualConfig");

            GameObject choicePrefab = BuildEventChoicePrefab(visual);

            RectTransform root = UiSceneBuilder.NewRect("EventScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 확인용 씬에서만 보이는 돌벽. 게임 씬에서는 지워지고 공용 배경이 보인다.
            // 와이어프레임 `12 이벤트` 가 돌벽 위에 검정 50퍼센트를 쓴다.
            UiSceneBuilder.NewStoneBackdrop("BehindScreen", root);

            // 삽화 그림이 없는 화면은 자리를 알아볼 수 있게 옅은 칸을 둔다.
            Image illustration = UiSceneBuilder.NewImage(
                "Illustration", root, visual.IllustrationPlaceholderColor);
            illustration.raycastTarget = false;

            Image bodyPanel = UiSceneBuilder.NewImage("BodyPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(bodyPanel, 14f, 8f);
            TMP_Text bodyLabel = UiSceneBuilder.NewText(
                "BodyLabel", bodyPanel.transform, string.Empty,
                visual.BodyFontSize, visual.TextColor, TextAlignmentOptions.TopLeft);
            UiSceneBuilder.Stretch(bodyLabel.rectTransform);

            RectTransform choiceLayer = UiSceneBuilder.NewRect("ChoiceLayer", root);
            UiSceneBuilder.Stretch(choiceLayer);

            EventScreenView view = root.gameObject.AddComponent<EventScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_illustration", illustration);
            UiSceneBuilder.SetField(view, "_illustrationPlaceholder", illustration.sprite);
            UiSceneBuilder.SetField(view, "_bodyPanel", bodyPanel);
            UiSceneBuilder.SetField(view, "_bodyLabel", bodyLabel);
            UiSceneBuilder.SetField(view, "_choiceLayer", choiceLayer);
            UiSceneBuilder.SetField(view, "_choicePrefab", choicePrefab.GetComponent<EventChoiceView>());

            EventScreenController controller = root.gameObject.AddComponent<EventScreenController>();
            UiSceneBuilder.SetField(controller, "_screenRoot", root.gameObject);
            UiSceneBuilder.SetField(controller, "_view", view);

            view.ApplyLayout();
            view.Show(PreviewSamples.Event(), null);

            // 삽화가 칸에 어떻게 들어가는지 보이게 임시 그림 하나를 얹는다. 02 힘을 새기는 것 의 첫 화면이다.
            view.ShowIllustration(ImportArt.LoadEventIllustration(
                SlotHero.Flow.EventScripts.IllustrationIdOf("carve_power", "1")));

            AddDriver();
            SaveScene(scene, "11 이벤트 화면");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/팝업")]
        public static void BuildPopup()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("PopupCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            PopupLayoutConfig layout =
                UiSceneBuilder.LoadConfig<PopupLayoutConfig>("Popup", "PopupLayoutConfig");
            PopupVisualConfig visual =
                UiSceneBuilder.LoadConfig<PopupVisualConfig>("Popup", "PopupVisualConfig");

            GameObject buttonPrefab = BuildPopupButtonPrefab(visual);

            RectTransform root = UiSceneBuilder.NewRect("Popup", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 팝업은 다른 화면 위에 열리므로 뒤에 깔릴 화면을 흉내 낸 바탕을 둔다.
            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", root, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);

            // 어두운 막과 팝업 칸을 한 칸에 묶는다.
            // 따로 두면 연출기가 팝업 칸만 켜고 꺼서 **막이 영영 꺼진 채로 남는다.**
            // 그러면 팝업이 떠 있어도 뒤가 막히지 않아 뒤 화면이 그대로 눌린다.
            RectTransform body = UiSceneBuilder.NewRect("PopupBody", root);
            UiSceneBuilder.Stretch(body);

            Image dim = UiSceneBuilder.NewFlatImage("Dim", body, visual.DimColor);
            UiSceneBuilder.Stretch(dim.rectTransform);

            Image popupPanel = UiSceneBuilder.NewImage("PopupPanel", body, visual.PopupColor);
            UiSceneBuilder.AddShadowPx(popupPanel, 14f, 8f);
            popupPanel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            popupPanel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            popupPanel.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Image titlePanel = UiSceneBuilder.NewImage("TitlePanel", popupPanel.transform, visual.TitlePanelColor);
            TMP_Text titleLabel = UiSceneBuilder.NewText(
                "TitleLabel", titlePanel.transform, string.Empty,
                visual.TitleFontSize, visual.TextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(titleLabel.rectTransform);

            Image bodyPanel = UiSceneBuilder.NewImage("BodyPanel", popupPanel.transform, visual.BodyPanelColor);
            RectTransform bodyContent = UiSceneBuilder.NewRect("BodyContent", bodyPanel.transform);
            UiSceneBuilder.Stretch(bodyContent);
            bodyContent.gameObject.AddComponent<RectMask2D>();

            TMP_Text bodyLabel = UiSceneBuilder.NewText(
                "BodyLabel", bodyContent, string.Empty,
                visual.BodyFontSize, visual.TextColor, TextAlignmentOptions.TopLeft);
            InsetText(bodyLabel.rectTransform, layout.BodyPadding);

            ScrollRect bodyScroll = UiSceneBuilder.AddScroll(bodyContent, bodyLabel.rectTransform);

            RectTransform buttonLayer = UiSceneBuilder.NewRect("ButtonLayer", popupPanel.transform);
            UiSceneBuilder.Stretch(buttonLayer);

            PopupView view = root.gameObject.AddComponent<PopupView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_dim", dim);
            UiSceneBuilder.SetField(view, "_popupPanel", popupPanel);
            UiSceneBuilder.SetField(view, "_titlePanel", titlePanel);
            UiSceneBuilder.SetField(view, "_titleLabel", titleLabel);
            UiSceneBuilder.SetField(view, "_bodyPanel", bodyPanel);
            UiSceneBuilder.SetField(view, "_bodyLabel", bodyLabel);
            UiSceneBuilder.SetField(view, "_bodyScroll", bodyScroll);
            UiSceneBuilder.SetField(view, "_bodyContent", bodyContent);
            UiSceneBuilder.SetField(view, "_buttonLayer", buttonLayer);
            UiSceneBuilder.SetField(view, "_buttonPrefab", buttonPrefab.GetComponent<PopupButtonView>());

            PopupPresenter presenter = root.gameObject.AddComponent<PopupPresenter>();
            UiSceneBuilder.SetField(presenter, "_popupRoot", body.gameObject);
            UiSceneBuilder.SetField(presenter, "_view", view);

            // 08장 예시 1 · 새 게임 확인. 되돌릴 수 없는 조작이라 오른쪽에 강조색이 붙는다.
            view.ApplyStyle();
            view.Show(PreviewSamples.NewGamePopup());

            AddDriver();
            SaveScene(scene, "08 화면 팝업");
        }

        /// <summary>
        /// 아이템 상세 오버레이. 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 아이템 상세 팝업이다.
        /// 이름은 좌상단, 가치 값은 우상단, 내용은 그 아래 안쪽 칸, 플레이버 텍스트는 하단이다.
        /// 안쪽 자리는 기획서에 없어 500 × 380 안에 여백 24 로 내가 나눴다.
        ///
        /// 오버레이라 아래 레이어를 막지 않는다. 그림과 글자 모두 커서를 받지 않는다.
        /// 받으면 커서가 오버레이로 넘어가 물건에서 벗어난 것으로 읽혀 깜빡인다.
        /// </summary>
        [MenuItem("Slot Hero/확인용 씬 만들기/아이템 상세")]
        public static void BuildItemDetail()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("ItemDetailCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            PopupLayoutConfig layout =
                UiSceneBuilder.LoadConfig<PopupLayoutConfig>("Popup", "PopupLayoutConfig");
            PopupVisualConfig visual =
                UiSceneBuilder.LoadConfig<PopupVisualConfig>("Popup", "PopupVisualConfig");

            // 껍데기. 캔버스를 꽉 채운다. 이 칸의 왼쪽 위가 오버레이 자리의 기준이다.
            RectTransform shell = UiSceneBuilder.NewRect("ItemDetail", canvas.transform);
            UiSceneBuilder.Stretch(shell);

            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", shell, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);
            behind.raycastTarget = false;

            // 켜고 끄고 옮기는 칸. 그림자가 본체와 함께 켜지고 꺼지도록 둘을 이 안에 둔다.
            RectTransform root = UiSceneBuilder.NewRect("Root", shell);
            float pad = UiScale.Px(24f);
            Vector2 size = layout.ItemDetailSize;

            // 본체는 펼치지 않고 오버레이 크기 그대로 놓는다.
            // 펼친 칸에 그림자를 달면 그림자가 여백만큼 크기를 받아 펼친 칸 감사에 걸린다.
            Image background = UiSceneBuilder.NewImage("Background", root, visual.ItemDetailColor);
            UiSceneBuilder.PlaceTopLeft(background.rectTransform, Vector2.zero, size);
            background.raycastTarget = false;
            Image shadow = UiSceneBuilder.AddShadowPx(background, 10f, 6f);
            if (shadow != null)
            {
                shadow.raycastTarget = false;
            }

            TMP_Text nameLabel = UiSceneBuilder.NewText(
                "NameLabel", root, "이름", visual.ItemNameFontSize, visual.TextColor,
                TextAlignmentOptions.Left, true, true);
            UiSceneBuilder.PlaceTopLeft(nameLabel.rectTransform, new Vector2(pad, pad), new Vector2(size.x * 0.65f - pad, UiScale.Px(44f)));

            TMP_Text valueLabel = UiSceneBuilder.NewText(
                "ValueLabel", root, "가치", visual.ItemValueFontSize, visual.TextColor,
                TextAlignmentOptions.Right, true, true);
            UiSceneBuilder.PlaceTopLeft(valueLabel.rectTransform, new Vector2(size.x * 0.65f, pad), new Vector2(size.x * 0.35f - pad, UiScale.Px(44f)));

            Image panel = UiSceneBuilder.NewImage("BodyPanel", root, visual.ItemDetailPanelColor);
            panel.raycastTarget = false;
            UiSceneBuilder.PlaceTopLeft(panel.rectTransform, new Vector2(pad, UiScale.Px(84f)), new Vector2(size.x - pad * 2f, UiScale.Px(200f)));

            TMP_Text bodyLabel = UiSceneBuilder.NewText(
                "BodyLabel", panel.transform, "내용", visual.ItemBodyFontSize, visual.TextColor,
                TextAlignmentOptions.TopLeft);
            UiSceneBuilder.Stretch(bodyLabel.rectTransform);
            bodyLabel.rectTransform.offsetMin = UiScale.V(16f, 12f);
            bodyLabel.rectTransform.offsetMax = UiScale.V(-16f, -12f);

            TMP_Text flavorLabel = UiSceneBuilder.NewText(
                "FlavorLabel", root, "플레이버", visual.ItemFlavorFontSize, visual.ItemFlavorColor,
                TextAlignmentOptions.Left);
            UiSceneBuilder.PlaceTopLeft(flavorLabel.rectTransform, new Vector2(pad, UiScale.Px(300f)), new Vector2(size.x - pad * 2f, UiScale.Px(56f)));

            TMP_Text[] labels = { nameLabel, valueLabel, bodyLabel, flavorLabel };
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].raycastTarget = false;
            }

            ItemDetailView view = shell.gameObject.AddComponent<ItemDetailView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_root", root);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_valueLabel", valueLabel);
            UiSceneBuilder.SetField(view, "_bodyLabel", bodyLabel);
            UiSceneBuilder.SetField(view, "_flavorLabel", flavorLabel);

            view.ApplyStyle();
            view.Show(
                new ItemDetailSpec("바다 은화", "일반", "체력 10\n예측 성공 시 +5", "은은 파도에도 쉽게 녹슬지 않는다."),
                UiScale.V(900f, 400f));

            AddDriver();
            SaveScene(scene, "08 아이템 상세");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/현재 빌드 화면")]
        public static void BuildCurrentBuild()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("CurrentBuildCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            CurrentBuildLayoutConfig layout =
                UiSceneBuilder.LoadConfig<CurrentBuildLayoutConfig>("CurrentBuild", "CurrentBuildLayoutConfig");
            CurrentBuildVisualConfig visual =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            GameObject slotPrefab = BuildSlotPrefab(visual);
            GameObject tagBarPrefab = BuildTagBarPrefab(visual);

            RectTransform root = UiSceneBuilder.NewRect("CurrentBuildScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", root, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);

            Image dim = UiSceneBuilder.NewFlatImage("Dim", root, visual.DimColor);
            UiSceneBuilder.Stretch(dim.rectTransform);

            RectTransform scrollViewport = UiSceneBuilder.NewRect("ScrollViewport", root);
            UiSceneBuilder.Stretch(scrollViewport);
            RectTransform scrollContent = UiSceneBuilder.NewRect("ScrollContent", scrollViewport);
            UiSceneBuilder.Stretch(scrollContent);

            // 줄 이름도 스크롤 안에 둔다. 밖에 두면 칸만 올라가고 `문양` 글자는 제자리에 남는다.
            TMP_Text relicLabel = UiSceneBuilder.NewText(
                "RelicLabel", scrollContent, "유물", visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            TMP_Text coinLabel = UiSceneBuilder.NewText(
                "CoinLabel", scrollContent, "코인", visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            TMP_Text symbolLabel = UiSceneBuilder.NewText(
                "SymbolLabel", scrollContent, "문양", visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);

            ScrollRect scroll = UiSceneBuilder.AddScroll(scrollViewport, scrollContent);

            // 칸 사이 빈 곳에서도 휠이 먹도록 창에 투명한 바탕을 깐다.
            // 바탕이 없으면 칸 위에서만 휠이 들고 빈 곳에서는 뒤의 어두운 막이 받아 아무 일도 없다.
            Image scrollCatcher = scrollViewport.gameObject.AddComponent<Image>();
            scrollCatcher.color = new Color(0f, 0f, 0f, 0f);

            RectTransform relicLayer = UiSceneBuilder.NewRect("RelicLayer", scrollContent);
            UiSceneBuilder.Stretch(relicLayer);
            RectTransform coinLayer = UiSceneBuilder.NewRect("CoinLayer", scrollContent);
            UiSceneBuilder.Stretch(coinLayer);
            RectTransform symbolLayer = UiSceneBuilder.NewRect("SymbolLayer", scrollContent);
            UiSceneBuilder.Stretch(symbolLayer);

            Image sortPanel = UiSceneBuilder.NewButtonImage("SortPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(sortPanel, 8f, 5f);
            UiSceneBuilder.AddButton(sortPanel.gameObject);
            TMP_Text sortLabel = UiSceneBuilder.NewText(
                "SortLabel", sortPanel.transform, "정렬", visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            TMP_Text sortValueLabel = UiSceneBuilder.NewText(
                "SortValueLabel", sortPanel.transform, string.Empty, visual.PanelTitleFontSize,
                visual.SubTextColor, TextAlignmentOptions.Right, true);

            // 둘 다 칸을 가득 채운다. 하나는 왼쪽, 하나는 오른쪽에 붙어 서로 비껴간다.
            // 이걸 안 하면 둘 다 칸 한가운데에 겹쳐 `정렬` 위에 `개수 순` 이 올라탄다.
            InsetText(sortLabel.rectTransform, UiScale.Px(20f));
            InsetText(sortValueLabel.rectTransform, UiScale.Px(20f));

            Image tagPanel = UiSceneBuilder.NewImage("TagPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(tagPanel, 14f, 8f);
            TMP_Text symbolCountLabel = UiSceneBuilder.NewText(
                "SymbolCountLabel", root, string.Empty, visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            TMP_Text tagTitleLabel = UiSceneBuilder.NewText(
                "TagTitleLabel", root, "태그 비중", visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            RectTransform tagLayer = UiSceneBuilder.NewRect("TagLayer", root);
            UiSceneBuilder.Stretch(tagLayer);

            Image closePanel = UiSceneBuilder.NewButtonImage("ClosePanel", root, visual.PanelColor);
            UiSceneBuilder.AddButton(closePanel.gameObject);
            TMP_Text closeLabel = UiSceneBuilder.NewText(
                "CloseLabel", closePanel.transform, "닫기", visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(closeLabel.rectTransform);

            // 단축키 안내는 닫기 글자와 겹치지 않게 오른쪽 끝에 붙인다.
            TMP_Text closeShortcutLabel = UiSceneBuilder.NewText(
                "CloseShortcutLabel", closePanel.transform, visual.CloseShortcutText,
                visual.CountFontSize, visual.SubTextColor, TextAlignmentOptions.Right);
            RectTransform shortcutRect = closeShortcutLabel.rectTransform;
            shortcutRect.anchorMin = new Vector2(1f, 0f);
            shortcutRect.anchorMax = new Vector2(1f, 1f);
            shortcutRect.pivot = new Vector2(1f, 0.5f);
            shortcutRect.sizeDelta = new Vector2(UiScale.Px(120f), 0f);
            shortcutRect.anchoredPosition = new Vector2(UiScale.Px(-20f), 0f);

            CurrentBuildScreenView view = root.gameObject.AddComponent<CurrentBuildScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_dim", dim);
            UiSceneBuilder.SetField(view, "_relicLabel", relicLabel);
            UiSceneBuilder.SetField(view, "_coinLabel", coinLabel);
            UiSceneBuilder.SetField(view, "_symbolLabel", symbolLabel);
            UiSceneBuilder.SetField(view, "_scroll", scroll);
            UiSceneBuilder.SetField(view, "_scrollViewport", scrollViewport);
            UiSceneBuilder.SetField(view, "_scrollContent", scrollContent);
            UiSceneBuilder.SetField(view, "_relicLayer", relicLayer);
            UiSceneBuilder.SetField(view, "_coinLayer", coinLayer);
            UiSceneBuilder.SetField(view, "_symbolLayer", symbolLayer);
            UiSceneBuilder.SetField(view, "_slotPrefab", slotPrefab.GetComponent<BuildSlotView>());
            UiSceneBuilder.SetField(view, "_sortButton", sortPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_sortPanel", sortPanel);
            UiSceneBuilder.SetField(view, "_sortLabel", sortLabel);
            UiSceneBuilder.SetField(view, "_sortValueLabel", sortValueLabel);
            UiSceneBuilder.SetField(view, "_tagPanel", tagPanel);
            UiSceneBuilder.SetField(view, "_symbolCountLabel", symbolCountLabel);
            UiSceneBuilder.SetField(view, "_tagTitleLabel", tagTitleLabel);
            UiSceneBuilder.SetField(view, "_tagLayer", tagLayer);
            UiSceneBuilder.SetField(view, "_tagBarPrefab", tagBarPrefab.GetComponent<BuildTagBarView>());
            UiSceneBuilder.SetField(view, "_closeButton", closePanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_closePanel", closePanel);
            UiSceneBuilder.SetField(view, "_closeLabel", closeLabel);
            UiSceneBuilder.SetField(view, "_closeShortcutLabel", closeShortcutLabel);

            CurrentBuildScreenController controller =
                root.gameObject.AddComponent<CurrentBuildScreenController>();
            UiSceneBuilder.SetField(controller, "_screenRoot", root.gameObject);
            UiSceneBuilder.SetField(controller, "_view", view);

            view.ApplyLayout();
            view.Show(PreviewSamples.Build(), BuildSortOrder.Count, null);

            AddDriver();
            SaveScene(scene, "13 현재 빌드 화면");
        }

        /// <summary>
        /// 고르기 화면. 현재 빌드 화면과 같은 칸과 오른쪽 칸을 쓰고, 아래에 고른 것과 확인 · 취소를 둔다.
        /// 성소 문양 변경과 08 피의 거래 가 연다. 2026년 10월 5일 원재가 정했다.
        /// </summary>
        [MenuItem("Slot Hero/확인용 씬 만들기/고르기 화면")]
        public static void BuildItemPicker()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("ItemPickerCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            CurrentBuildLayoutConfig layout =
                UiSceneBuilder.LoadConfig<CurrentBuildLayoutConfig>("CurrentBuild", "CurrentBuildLayoutConfig");
            CurrentBuildVisualConfig visual =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            GameObject slotPrefab = BuildSlotPrefab(visual);
            GameObject tagBarPrefab = BuildTagBarPrefab(visual);

            RectTransform root = UiSceneBuilder.NewRect("ItemPickerScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", root, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);

            // 아래를 막는다. 고르기 화면은 팝업처럼 고르거나 취소해야 닫힌다.
            Image dim = UiSceneBuilder.NewFlatImage("Dim", root, visual.DimColor);
            UiSceneBuilder.Stretch(dim.rectTransform);

            RectTransform viewport = UiSceneBuilder.NewRect("ScrollViewport", root);
            viewport.gameObject.AddComponent<RectMask2D>();
            Image catcher = viewport.gameObject.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            RectTransform content = UiSceneBuilder.NewRect("ScrollContent", viewport);
            UiSceneBuilder.Stretch(content);
            ScrollRect scroll = UiSceneBuilder.AddScroll(viewport, content);

            TMP_Text titleLabel = UiSceneBuilder.NewText(
                "TitleLabel", content, "문양 선택", visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            RectTransform slotLayer = UiSceneBuilder.NewRect("SlotLayer", content);
            UiSceneBuilder.Stretch(slotLayer);

            Image sortPanel = UiSceneBuilder.NewButtonImage("SortPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(sortPanel, 8f, 5f);
            UiSceneBuilder.AddButton(sortPanel.gameObject);
            TMP_Text sortLabel = UiSceneBuilder.NewText(
                "SortLabel", sortPanel.transform, "정렬", visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            TMP_Text sortValueLabel = UiSceneBuilder.NewText(
                "SortValueLabel", sortPanel.transform, string.Empty, visual.PanelTitleFontSize,
                visual.SubTextColor, TextAlignmentOptions.Right, true);
            InsetText(sortLabel.rectTransform, UiScale.Px(20f));
            InsetText(sortValueLabel.rectTransform, UiScale.Px(20f));

            Image tagPanel = UiSceneBuilder.NewImage("TagPanel", root, visual.PanelColor);
            UiSceneBuilder.AddShadowPx(tagPanel, 14f, 8f);
            TMP_Text symbolCountLabel = UiSceneBuilder.NewText(
                "SymbolCountLabel", root, string.Empty, visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            TMP_Text tagTitleLabel = UiSceneBuilder.NewText(
                "TagTitleLabel", root, "태그 비중", visual.PanelTitleFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true);
            RectTransform tagLayer = UiSceneBuilder.NewRect("TagLayer", root);
            UiSceneBuilder.Stretch(tagLayer);

            Image resultPanel = UiSceneBuilder.NewImage("ResultPanel", root, visual.PanelColor);
            TMP_Text resultLabel = UiSceneBuilder.NewText(
                "ResultLabel", resultPanel.transform, string.Empty, visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Left, true, true);
            InsetText(resultLabel.rectTransform, UiScale.Px(20f));

            Image confirmPanel = UiSceneBuilder.NewButtonImage("ConfirmPanel", root, visual.PanelColor);
            UiSceneBuilder.AddButton(confirmPanel.gameObject);
            TMP_Text confirmLabel = UiSceneBuilder.NewText(
                "ConfirmLabel", confirmPanel.transform, visual.PickConfirmText, visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Center, true, true);
            UiSceneBuilder.Stretch(confirmLabel.rectTransform);

            Image cancelPanel = UiSceneBuilder.NewButtonImage("CancelPanel", root, visual.PanelColor);
            UiSceneBuilder.AddButton(cancelPanel.gameObject);
            TMP_Text cancelLabel = UiSceneBuilder.NewText(
                "CancelLabel", cancelPanel.transform, visual.PickCancelText, visual.SectionFontSize,
                visual.TextColor, TextAlignmentOptions.Center, true, true);
            UiSceneBuilder.Stretch(cancelLabel.rectTransform);

            ItemPickerScreenView view = root.gameObject.AddComponent<ItemPickerScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_dim", dim);
            UiSceneBuilder.SetField(view, "_titleLabel", titleLabel);
            UiSceneBuilder.SetField(view, "_scroll", scroll);
            UiSceneBuilder.SetField(view, "_scrollViewport", viewport);
            UiSceneBuilder.SetField(view, "_scrollContent", content);
            UiSceneBuilder.SetField(view, "_slotLayer", slotLayer);
            UiSceneBuilder.SetField(view, "_slotPrefab", slotPrefab.GetComponent<BuildSlotView>());
            UiSceneBuilder.SetField(view, "_sortButton", sortPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_sortPanel", sortPanel);
            UiSceneBuilder.SetField(view, "_sortLabel", sortLabel);
            UiSceneBuilder.SetField(view, "_sortValueLabel", sortValueLabel);
            UiSceneBuilder.SetField(view, "_tagPanel", tagPanel);
            UiSceneBuilder.SetField(view, "_symbolCountLabel", symbolCountLabel);
            UiSceneBuilder.SetField(view, "_tagTitleLabel", tagTitleLabel);
            UiSceneBuilder.SetField(view, "_tagLayer", tagLayer);
            UiSceneBuilder.SetField(view, "_tagBarPrefab", tagBarPrefab.GetComponent<BuildTagBarView>());
            UiSceneBuilder.SetField(view, "_resultPanel", resultPanel);
            UiSceneBuilder.SetField(view, "_resultLabel", resultLabel);
            UiSceneBuilder.SetField(view, "_confirmButton", confirmPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_confirmPanel", confirmPanel);
            UiSceneBuilder.SetField(view, "_confirmLabel", confirmLabel);
            UiSceneBuilder.SetField(view, "_cancelButton", cancelPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_cancelPanel", cancelPanel);
            UiSceneBuilder.SetField(view, "_cancelLabel", cancelLabel);

            ItemPickerScreenController controller = root.gameObject.AddComponent<ItemPickerScreenController>();
            UiSceneBuilder.SetField(controller, "_screenRoot", root.gameObject);
            UiSceneBuilder.SetField(controller, "_view", view);

            // 견본. 성소 문양 변경처럼 가진 문양에서 하나를 고른 모습이다.
            CurrentBuildSnapshot build = PreviewSamples.Build();
            ItemPickRequest request = new ItemPickRequest();
            request.Kind = ItemPickKind.Symbol;
            request.Title = "바꿀 문양 선택";
            request.Candidates.AddRange(build.Symbols);
            request.Build = build;
            request.ResultFormat = "[{0}] 을 무작위 문양으로 바꾼다";
            request.ConfirmText = "바꾸기";

            view.ApplyLayout();
            view.Show(request, BuildSortOrder.Count, null, build.Symbols.Count > 0 ? build.Symbols[0].Id : string.Empty);

            AddDriver();
            SaveScene(scene, "13 고르기 화면");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/맵 화면")]
        public static void BuildMap()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("MapCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            MapGenerationConfig generation =
                UiSceneBuilder.LoadConfig<MapGenerationConfig>("Map", "MapGenerationConfig");
            MapVisualConfig visual =
                UiSceneBuilder.LoadConfig<MapVisualConfig>("Map", "MapVisualConfig");

            GameObject nodePrefab = BuildMapNodePrefab();
            GameObject edgePrefab = BuildMapEdgePrefab();

            RectTransform root = UiSceneBuilder.NewRect("MapScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 확인용 씬에서만 보이는 돌벽. 게임 씬으로 옮길 때 지워지고 공용 배경이 대신 보인다.
            // 이름을 `Background` 로 두면 남아서 공용 배경을 덮는다. 맵이 까맣던 까닭이다.
            UiSceneBuilder.NewStoneBackdrop("BehindScreen", root);

            // 지도를 잠깐 띄울 때 지도 뒤에 까는 반투명 검은 막. 평소에는 꺼 둔다.
            // 화면을 꽉 채우고 커서를 받아 아래 방 화면이 눌리지 않게 한다.
            // 상단 표시줄과 현재 빌드 버튼은 게임 씬에서 맵보다 위에 있어 막에 가리지 않는다.
            Image peekDim = UiSceneBuilder.NewFlatImage("PeekDim", root, visual.PeekDimColor);
            UiSceneBuilder.Stretch(peekDim.rectTransform);
            peekDim.raycastTarget = true;
            peekDim.gameObject.SetActive(false);

            // 양 끝이 말린 양피지. 와이어프레임 `10 맵 화면` 의 `지도 배경 · 양피지_세로확장2.png` 다.
            //
            // **뷰포트 밖에 둔다.** 뷰포트는 노드가 지도 밖으로 안 나가게 잘라 내는데
            // 양피지는 노드 영역보다 훨씬 커서 그 안에 두면 말린 끝이 잘린다.
            // 그림이 없으면 예전처럼 베이지 한 색 칸을 노드 영역 크기로 깐다.
            Sprite parchment = ImportArt.LoadMap("지도_양피지");

            Image paper = UiSceneBuilder.NewFlatImage(
                "Parchment", root,
                parchment != null ? Color.white : new Color(0.851f, 0.796f, 0.690f, 1f));
            RectTransform paperRect = paper.rectTransform;
            paperRect.anchorMin = new Vector2(0.5f, 0.5f);
            paperRect.anchorMax = new Vector2(0.5f, 0.5f);
            paperRect.pivot = new Vector2(0.5f, 0.5f);
            paperRect.anchoredPosition = Vector2.zero;
            paperRect.sizeDelta = parchment != null ? visual.ParchmentSize : visual.MapSize;
            paper.sprite = parchment;
            paper.type = Image.Type.Simple;
            paper.raycastTarget = false;

            // 노드 영역을 화면 가운데에 둔다. 양피지의 종이 몸통 안이다.
            RectTransform viewport = UiSceneBuilder.NewRect("Viewport", root);
            viewport.anchorMin = new Vector2(0.5f, 0.5f);
            viewport.anchorMax = new Vector2(0.5f, 0.5f);
            viewport.pivot = new Vector2(0.5f, 0.5f);
            viewport.sizeDelta = visual.MapSize;
            viewport.anchoredPosition = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = UiSceneBuilder.NewRect("Content", viewport);
            UiSceneBuilder.Stretch(content);

            RectTransform edgeLayer = UiSceneBuilder.NewRect("EdgeLayer", content);
            UiSceneBuilder.Stretch(edgeLayer);
            RectTransform nodeLayer = UiSceneBuilder.NewRect("NodeLayer", content);
            UiSceneBuilder.Stretch(nodeLayer);

            MapScreenView view = root.gameObject.AddComponent<MapScreenView>();
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_viewport", viewport);
            UiSceneBuilder.SetField(view, "_content", content);
            UiSceneBuilder.SetField(view, "_edgeLayer", edgeLayer);
            UiSceneBuilder.SetField(view, "_nodeLayer", nodeLayer);
            UiSceneBuilder.SetField(view, "_nodePrefab", nodePrefab.GetComponent<MapNodeView>());
            UiSceneBuilder.SetField(view, "_edgePrefab", edgePrefab.GetComponent<MapEdgeView>());

            // 같은 시드는 어느 기기에서든 같은 지도가 나온다.
            StageMap map = MapGenerator.Generate(20260921, 1, generation);

            // 몇 방을 지나온 상태로 둔다. 지나온 방과 지금 방, 고를 수 있는 방이 갈려 보인다.
            MapProgress progress = new MapProgress();
            WalkThreeSteps(map, progress);

            MapScreenController controller = root.gameObject.AddComponent<MapScreenController>();
            UiSceneBuilder.SetField(controller, "_generationConfig", generation);
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetField(controller, "_screenRoot", root.gameObject);
            UiSceneBuilder.SetField(controller, "_peekDim", peekDim.gameObject);

            // 지도가 화면보다 넓어 좌우로 움직인다. 처음에는 왼쪽 끝부터 보이게 둔다.
            view.Build(map, progress);

            AddDriver();
            SaveScene(scene, "09 맵 화면");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/상단 표시줄")]
        public static void BuildTopBar()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("TopBarCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            TopBarLayoutConfig layout =
                UiSceneBuilder.LoadConfig<TopBarLayoutConfig>("TopBar", "TopBarLayoutConfig");
            TopBarVisualConfig visual =
                UiSceneBuilder.LoadConfig<TopBarVisualConfig>("TopBar", "TopBarVisualConfig");

            RectTransform root = UiSceneBuilder.NewRect("TopBarScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 표시줄은 런 진행 화면 위에 얹히므로 아래에 깔릴 화면을 흉내 낸다.
            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", root, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);

            RectTransform bar = UiSceneBuilder.NewRect("TopBar", root);
            UiSceneBuilder.Stretch(bar);

            // 표시줄은 화면 맨 위에 1920 × 64 로 붙는다.
            Image background = UiSceneBuilder.NewFlatImage("Background", bar, Color.white);
            background.sprite = ImportArt.LoadTopBar("표시줄_바탕");
            UiSceneBuilder.PlaceTopLeft(
                background.rectTransform, Vector2.zero, new Vector2(UiScale.Px(1920f), layout.BarHeight));

            // 묶음의 원점이 왼쪽 가운데라 자식은 그 오른쪽으로 늘어놓는다.
            // 사이 간격은 예시 이미지에서 잰 값이다.
            RectTransform playTimeRoot = UiSceneBuilder.NewRect("PlayTimeRoot", bar);
            TMP_Text playTimeLabel = UiSceneBuilder.NewText(
                "PlayTimeLabel", playTimeRoot, "0:00", visual.PlayTimeFontSize,
                visual.PlayTimeColor, TextAlignmentOptions.MidlineLeft, true, true);
            PlaceInRow(playTimeLabel.rectTransform, 0f, 0f, UiScale.Px(240f), UiScale.Px(64f));

            RectTransform locationRoot = UiSceneBuilder.NewRect("LocationRoot", bar);
            Image stageIcon = UiSceneBuilder.NewFlatImage("StageIcon", locationRoot, Color.white);
            PlaceInRow(stageIcon.rectTransform, 0f, 0f, UiScale.Px(40f), UiScale.Px(40f));

            TMP_Text stageLabel = UiSceneBuilder.NewText(
                "StageLabel", locationRoot, "5", visual.StageFontSize,
                visual.StageColor, TextAlignmentOptions.Midline, true, true);
            PlaceInRow(stageLabel.rectTransform, UiScale.Px(66f), 0f, UiScale.Px(30f), UiScale.Px(64f));

            Image roomIcon = UiSceneBuilder.NewFlatImage("RoomIcon", locationRoot, Color.white);
            PlaceInRow(roomIcon.rectTransform, UiScale.Px(100f), 0f, UiScale.Px(40f), UiScale.Px(40f));

            locationRoot.gameObject.AddComponent<TopBarHoverArea>();

            TopBarField healthField = BuildTopBarField("HealthField", bar, visual,
                visual.HealthFontSize, visual.HealthColor);
            TopBarField goldField = BuildTopBarField("GoldField", bar, visual,
                visual.GoldFontSize, visual.GoldColor);

            TopBarButton[] buttons = new TopBarButton[3];
            buttons[0] = BuildTopBarButton(bar, visual, TopBarButtonKind.Map, "지도", "버튼_지도");
            buttons[1] = BuildTopBarButton(bar, visual, TopBarButtonKind.Settings, "설정", "버튼_설정");
            buttons[2] = BuildTopBarButton(bar, visual, TopBarButtonKind.EndRun, "런 종료", "버튼_런종료");

            TopBarTooltip tooltip = BuildTopBarTooltip(root, visual);

            TopBarView view = bar.gameObject.AddComponent<TopBarView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_playTimeRoot", playTimeRoot);
            UiSceneBuilder.SetField(view, "_playTimeLabel", playTimeLabel);
            UiSceneBuilder.SetField(view, "_locationRoot", locationRoot);
            UiSceneBuilder.SetField(view, "_stageIcon", stageIcon);
            UiSceneBuilder.SetField(view, "_stageLabel", stageLabel);
            UiSceneBuilder.SetField(view, "_roomIcon", roomIcon);
            UiSceneBuilder.SetField(view, "_locationHover", locationRoot.GetComponent<TopBarHoverArea>());
            UiSceneBuilder.SetField(view, "_healthField", healthField);
            UiSceneBuilder.SetField(view, "_goldField", goldField);
            UiSceneBuilder.SetField(view, "_tooltip", tooltip);
            SetButtonArray(view, buttons);

            TopBarController controller = bar.gameObject.AddComponent<TopBarController>();
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetField(controller, "_visual", visual);
            UiSceneBuilder.SetField(controller, "_stageIcon", ImportArt.LoadTopBar("아이콘_깃발"));
            UiSceneBuilder.SetField(controller, "_healthIcon", ImportArt.LoadTopBar("아이콘_체력"));
            UiSceneBuilder.SetField(controller, "_goldIcon", ImportArt.LoadTopBar("아이콘_골드"));

            view.ApplyLayout();

            // 예시 이미지와 같은 값을 넣는다.
            view.SetPlayTime("59:40");
            view.SetLocation(
                ImportArt.LoadTopBar("아이콘_깃발"), "5", ImportArt.LoadTopBar("아이콘_방_성소"));
            view.SetHealthIcon(ImportArt.LoadTopBar("아이콘_체력"));
            view.SetHealthText("48/99");
            view.SetGoldIcon(ImportArt.LoadTopBar("아이콘_골드"));
            view.SetGoldText("137");

            AddDriver();
            SaveScene(scene, "03 상단 표시줄");
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/성소 화면")]
        public static void BuildSanctum()
        {
            BuildSanctumScene(false);
        }

        /// <summary>
        /// 행상 칸을 펴 둔 성소 씬.
        /// 한 씬 안에서 성소 칸과 행상 칸을 켜고 끄므로
        /// 행상을 그림으로 뽑으려면 행상 쪽을 켠 씬이 따로 있어야 한다.
        /// </summary>
        [MenuItem("Slot Hero/확인용 씬 만들기/행상 화면")]
        public static void BuildMerchant()
        {
            BuildSanctumScene(true);
        }

        private static void BuildSanctumScene(bool showMerchant)
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas(showMerchant ? "MerchantCanvas" : "SanctumCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            SanctumConfig config = UiSceneBuilder.LoadConfig<SanctumConfig>("Sanctum", "SanctumConfig");
            SanctumVisualConfig visual =
                UiSceneBuilder.LoadConfig<SanctumVisualConfig>("Sanctum", "SanctumVisualConfig");

            SanctumLayoutConfig layout =
                UiSceneBuilder.LoadConfig<SanctumLayoutConfig>("Sanctum", "SanctumLayoutConfig");
            MerchantLayoutConfig merchantLayout =
                UiSceneBuilder.LoadConfig<MerchantLayoutConfig>("Sanctum", "MerchantLayoutConfig");
            SanctumPriceConfig prices =
                UiSceneBuilder.LoadConfig<SanctumPriceConfig>("Sanctum", "SanctumPriceConfig");

            // 뿌리는 조종기만 들고, 성소 칸과 행상 칸을 형제로 둔다.
            // 성소 칸에 조종기를 같이 두면 행상으로 들어갈 때 조종기까지 꺼진다.
            RectTransform root = UiSceneBuilder.NewRect("SanctumScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            RectTransform sanctumPanel = UiSceneBuilder.NewRect("SanctumPanel", root);
            UiSceneBuilder.Stretch(sanctumPanel);

            SanctumScreenView view = BuildSanctumPanel(sanctumPanel, layout);

            RectTransform merchantPanel = UiSceneBuilder.NewRect("MerchantPanel", root);
            UiSceneBuilder.Stretch(merchantPanel);

            MerchantScreenView merchantView = BuildMerchantPanel(merchantPanel, config, merchantLayout);

            SanctumController controller = root.gameObject.AddComponent<SanctumController>();
            UiSceneBuilder.SetField(controller, "_config", config);
            UiSceneBuilder.SetField(controller, "_priceConfig", prices);
            UiSceneBuilder.SetField(controller, "_visualConfig", visual);
            UiSceneBuilder.SetField(controller, "_sanctumRoot", sanctumPanel.gameObject);
            UiSceneBuilder.SetField(controller, "_sanctumView", view);
            UiSceneBuilder.SetField(controller, "_merchantRoot", merchantPanel.gameObject);
            UiSceneBuilder.SetField(controller, "_merchantView", merchantView);

            // 야영을 아직 쓰지 않은 성소다.
            SanctumState state = new SanctumState();
            view.Refresh(state, config, visual);

            // 진열을 한 번 채워 두어야 확인용 씬에서 물건이 보인다.
            // 실제 게임에서는 흐름이 진짜 목록으로 다시 채운다.
            SampleMerchantCatalog sample = new SampleMerchantCatalog();
            state.Begin(1234, 0, config, prices, sample, RunRelicPool.Build(1234, sample));
            merchantView.Bind(state, prices, visual, new SanctumPreviewIcons(), 137);

            // 한 씬에 둘이 들어 있으므로 보고 싶은 쪽만 켠다.
            // 게임에서는 조종기가 버튼에 따라 켜고 끈다.
            sanctumPanel.gameObject.SetActive(!showMerchant);
            merchantPanel.gameObject.SetActive(showMerchant);

            AddDriver();
            SaveScene(scene, showMerchant ? "성소 행상 화면" : "성소 화면");
        }

        /// <summary>
        /// 성소 화면. 그림 석 장이 전부다.
        /// 모닥불이 야영, 행상인이 행상, 오른쪽 아래 화살표가 나가기다.
        /// 누르는 영역이 곧 그림이라 따로 칸을 두지 않는다.
        /// </summary>
        private static SanctumScreenView BuildSanctumPanel(
            RectTransform panel, SanctumLayoutConfig layout)
        {
            Image background = UiSceneBuilder.NewFlatImage("Background", panel, Color.white);
            UiSceneBuilder.Stretch(background.rectTransform);
            background.sprite = ImportArt.LoadSanctum("배경_황무지");
            background.type = Image.Type.Simple;

            Sprite campOn = ImportArt.LoadSanctum("야영_모닥불");
            Sprite campOff = ImportArt.LoadSanctum("야영_모닥불_꺼짐");

            Image camp = NewPicture("CampButton", panel, campOn, layout.CampPosition, layout.CampSize);
            UiSceneBuilder.AddButton(camp.gameObject);

            Image merchant = NewPicture(
                "MerchantButton", panel, ImportArt.LoadSanctum("행상_행상인"),
                layout.MerchantPosition, layout.MerchantSize);
            UiSceneBuilder.AddButton(merchant.gameObject);

            Image exit = NewPicture(
                "ExitButton", panel, ImportArt.LoadSanctum("버튼_나가기_오른쪽"),
                layout.ExitPosition, layout.ExitSize);
            UiSceneBuilder.AddButton(exit.gameObject);

            // 글자는 화살표 머리를 뺀 왼쪽 몸통에 놓는다. 머리에 걸치면 읽기 어렵다.
            TMP_Text exitLabel = UiSceneBuilder.NewText(
                "ExitLabel", exit.transform, "나가기", layout.ExitFontSize,
                new Color(0.16f, 0.13f, 0.05f), TextAlignmentOptions.Center, true);
            RectTransform exitLabelRect = exitLabel.rectTransform;
            exitLabelRect.anchorMin = new Vector2(0f, 0f);
            exitLabelRect.anchorMax = new Vector2(0f, 1f);
            exitLabelRect.pivot = new Vector2(0f, 0.5f);
            exitLabelRect.sizeDelta = new Vector2(layout.GetExitLabelSize().x, 0f);
            exitLabelRect.anchoredPosition = Vector2.zero;

            SanctumScreenView view = panel.gameObject.AddComponent<SanctumScreenView>();
            UiSceneBuilder.SetField(view, "_campButton", camp.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_campImage", camp);
            UiSceneBuilder.SetField(view, "_campAvailableSprite", campOn);
            UiSceneBuilder.SetField(view, "_campUsedSprite", campOff);
            UiSceneBuilder.SetField(view, "_merchantButton", merchant.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_exitButton", exit.GetComponent<Button>());
            return view;
        }

        /// <summary>
        /// 행상 화면. 왼쪽 돗자리에 문양 넷과 코인 둘, 오른쪽 양탄자에 문양 변경과 유물 셋이다.
        /// 문양 변경 칸과 유물 칸의 테두리는 양탄자 그림에 이미 그려져 있어 따로 두지 않는다.
        /// </summary>
        private static MerchantScreenView BuildMerchantPanel(
            RectTransform panel, SanctumConfig config, MerchantLayoutConfig layout)
        {
            Image background = UiSceneBuilder.NewFlatImage("Background", panel, Color.white);
            UiSceneBuilder.Stretch(background.rectTransform);
            background.sprite = ImportArt.LoadItemArt("배경_돌바닥");
            background.type = Image.Type.Simple;

            NewPicture("LeftMat", panel, ImportArt.LoadSanctum("행상_돗자리"),
                layout.LeftMatPosition, layout.LeftMatSize);
            NewPicture("RightMat", panel, ImportArt.LoadSanctum("행상_양탄자"),
                layout.RightMatPosition, layout.RightMatSize);

            // 문양 바닥은 자리마다 조금씩 다른 넉 장이다. PSD 도 그렇게 되어 있다.
            string[] symbolBaseNames = { "문양_바닥", "문양_바닥_b", "문양_바닥_c", "문양_바닥_d" };
            Sprite coinBase = ImportArt.LoadSanctum("행상_코인받침");

            MerchantSlotView[] symbolSlots = new MerchantSlotView[config.SymbolSlotCount];
            for (int i = 0; i < symbolSlots.Length; i++)
            {
                Sprite symbolBase = ImportArt.LoadItemArt(symbolBaseNames[i % symbolBaseNames.Length]);
                symbolSlots[i] = BuildMerchantSlot(
                    "SymbolSlot" + i, panel, layout, symbolBase,
                    layout.GetSymbolPosition(i), layout.SymbolSlotSize, layout.SymbolPriceOffsetY);
            }

            MerchantSlotView[] coinSlots = new MerchantSlotView[config.CoinSlotCount];
            for (int i = 0; i < coinSlots.Length; i++)
            {
                coinSlots[i] = BuildMerchantSlot(
                    "CoinSlot" + i, panel, layout, coinBase,
                    layout.GetCoinPosition(i), layout.CoinSlotSize, layout.CoinPriceOffsetY);
            }

            MerchantSlotView[] relicSlots = new MerchantSlotView[config.RelicSlotCount];
            for (int i = 0; i < relicSlots.Length; i++)
            {
                relicSlots[i] = BuildMerchantSlot(
                    "RelicSlot" + i, panel, layout, null,
                    layout.GetRelicPosition(i), layout.RelicSlotSize, layout.RelicPriceOffsetY);
            }

            // 문양 변경.
            //
            // **그림이 아직 없다.** 보라 소용돌이가 양탄자에 그려져 있을 줄 알고
            // 투명한 누르는 칸만 얹어 두었는데, 잘라 온 양탄자에는 그것이 없다.
            // 그래서 화면에 아무것도 없는 자리를 눌러야 하는 꼴이 됐다.
            //
            // 그림이 올 때까지 PSD 에서 잰 보라색으로 자리를 그려 둔다.
            // `행상_문양변경.png` 를 `코드/Sanctum/Art` 에 넣으면 그쪽을 쓴다.
            Sprite changeSprite = ImportArt.LoadSanctum("행상_문양변경");

            Image changeHit = changeSprite != null
                ? NewPicture("SymbolChangeButton", panel, changeSprite,
                    layout.SymbolChangePosition, layout.SymbolChangeSize)
                : UiSceneBuilder.NewImage("SymbolChangeButton", panel, SymbolChangeStandIn);

            RectTransform change = changeHit.rectTransform;
            UiSceneBuilder.PlaceTopLeft(change, layout.SymbolChangePosition, layout.SymbolChangeSize);
            UiSceneBuilder.AddButton(change.gameObject);

            // 그림이 없을 때만 무엇을 누르는 자리인지 글로 알려 준다.
            if (changeSprite == null)
            {
                TMP_Text changeLabel = UiSceneBuilder.NewText(
                    "SymbolChangeLabel", change, "문양\n무작위 변경", layout.PriceFontSize,
                    Color.white, TextAlignmentOptions.Top, true);
                RectTransform changeLabelRect = changeLabel.rectTransform;
                changeLabelRect.anchorMin = new Vector2(0f, 0f);
                changeLabelRect.anchorMax = new Vector2(1f, 1f);
                changeLabelRect.offsetMin = UiScale.V(10f, 10f);
                changeLabelRect.offsetMax = UiScale.V(-10f, -10f);
            }

            TMP_Text changePrice = NewPriceLabel(
                "SymbolChangePrice", panel, layout, layout.GetSymbolChangePricePosition());

            // 유물 새로고침. 구슬 그림 위에 표시만 얹는다.
            Image refresh = NewPicture(
                "RelicRefreshButton", panel, ImportArt.LoadSanctum("행상_새로고침"),
                layout.RefreshPosition, layout.RefreshSize);
            UiSceneBuilder.AddButton(refresh.gameObject);
            TMP_Text refreshPrice = NewPriceLabel(
                "RelicRefreshPrice", panel, layout,
                layout.GetPricePosition(layout.RefreshPosition, layout.RefreshSize, layout.RefreshPriceOffsetY));

            Image back = NewPicture(
                "BackButton", panel, ImportArt.LoadSanctum("버튼_나가기_아래"),
                layout.BackPosition, layout.BackSize);
            UiSceneBuilder.AddButton(back.gameObject);

            // 글자는 화살표 머리를 뺀 위쪽 몸통에 놓는다.
            TMP_Text backLabel = UiSceneBuilder.NewText(
                "BackLabel", back.transform, "나가기", layout.BackFontSize,
                new Color(0.16f, 0.13f, 0.05f), TextAlignmentOptions.Center, true);
            RectTransform backLabelRect = backLabel.rectTransform;
            backLabelRect.anchorMin = new Vector2(0f, 1f);
            backLabelRect.anchorMax = new Vector2(1f, 1f);
            backLabelRect.pivot = new Vector2(0.5f, 1f);
            backLabelRect.sizeDelta = new Vector2(0f, layout.GetBackLabelSize().y);
            backLabelRect.anchoredPosition = Vector2.zero;

            MerchantScreenView view = panel.gameObject.AddComponent<MerchantScreenView>();
            UiSceneBuilder.SetArrayField(view, "_symbolSlots", symbolSlots);
            UiSceneBuilder.SetArrayField(view, "_coinSlots", coinSlots);
            UiSceneBuilder.SetArrayField(view, "_relicSlots", relicSlots);
            UiSceneBuilder.SetField(view, "_symbolChangeButton", change.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_symbolChangePriceLabel", changePrice);
            UiSceneBuilder.SetField(view, "_relicRefreshButton", refresh.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_relicRefreshPriceLabel", refreshPrice);
            UiSceneBuilder.SetField(view, "_backButton", back.GetComponent<Button>());
            return view;
        }

        /// <summary>진열 자리 하나. 바닥 그림과 물건 그림과 가격 줄을 둔다.</summary>
        private static MerchantSlotView BuildMerchantSlot(
            string name, RectTransform parent, MerchantLayoutConfig layout,
            Sprite baseSprite, Vector2 position, Vector2 size, float priceOffsetY)
        {
            RectTransform slot = UiSceneBuilder.NewRect(name, parent);
            UiSceneBuilder.PlaceTopLeft(slot, position, size);

            // 바닥 그림. 유물은 양탄자에 칸이 그려져 있어 바닥을 두지 않는다.
            Image slotImage = slot.gameObject.AddComponent<Image>();
            slotImage.sprite = baseSprite;
            slotImage.type = Image.Type.Simple;
            slotImage.color = baseSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);

            UiSceneBuilder.AddButton(slot.gameObject);

            // 물건 그림은 바닥 안쪽에 넉넉히 넣는다. 그림마다 비율이 달라 가운데 맞춤으로 둔다.
            Image icon = UiSceneBuilder.NewFlatImage("Icon", slot, Color.white);
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            icon.preserveAspect = true;
            icon.sprite = null;

            // 가격 줄은 자리 바깥이라 자리의 형제로 둔다. 자리를 감추면 가격도 함께 감춘다.
            RectTransform priceRoot = UiSceneBuilder.NewRect(name + "Price", parent);
            UiSceneBuilder.PlaceTopLeft(
                priceRoot, layout.GetPricePosition(position, size, priceOffsetY), layout.PriceSize);

            Image goldIcon = UiSceneBuilder.NewFlatImage("GoldIcon", priceRoot, Color.white);
            goldIcon.sprite = ImportArt.LoadItemArt("아이콘_골드");
            goldIcon.preserveAspect = true;
            RectTransform goldRect = goldIcon.rectTransform;
            goldRect.anchorMin = new Vector2(0.5f, 0.5f);
            goldRect.anchorMax = new Vector2(0.5f, 0.5f);
            goldRect.pivot = new Vector2(1f, 0.5f);
            goldRect.sizeDelta = layout.PriceIconSize;
            goldRect.anchoredPosition = new Vector2(-layout.PriceIconGap * 0.5f, 0f);

            TMP_Text priceLabel = UiSceneBuilder.NewText(
                "PriceLabel", priceRoot, "0", layout.PriceFontSize,
                new Color(1f, 0.85f, 0.25f), TextAlignmentOptions.MidlineLeft, true);
            RectTransform labelRect = priceLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(layout.PriceSize.x * 0.5f, layout.PriceSize.y);
            labelRect.anchoredPosition = new Vector2(layout.PriceIconGap * 0.5f, 0f);

            MerchantSlotView view = slot.gameObject.AddComponent<MerchantSlotView>();
            UiSceneBuilder.SetField(view, "_icon", icon);
            UiSceneBuilder.SetField(view, "_button", slot.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_priceRoot", priceRoot.gameObject);
            UiSceneBuilder.SetField(view, "_priceLabel", priceLabel);
            return view;
        }

        /// <summary>문양 변경과 유물 새로고침이 쓰는 가격 줄. 자리에 딸리지 않은 낱개다.</summary>
        private static TMP_Text NewPriceLabel(
            string name, RectTransform parent, MerchantLayoutConfig layout, Vector2 position)
        {
            RectTransform root = UiSceneBuilder.NewRect(name, parent);
            UiSceneBuilder.PlaceTopLeft(root, position, layout.PriceSize);

            Image goldIcon = UiSceneBuilder.NewFlatImage("GoldIcon", root, Color.white);
            goldIcon.sprite = ImportArt.LoadItemArt("아이콘_골드");
            goldIcon.preserveAspect = true;
            RectTransform goldRect = goldIcon.rectTransform;
            goldRect.anchorMin = new Vector2(0.5f, 0.5f);
            goldRect.anchorMax = new Vector2(0.5f, 0.5f);
            goldRect.pivot = new Vector2(1f, 0.5f);
            goldRect.sizeDelta = layout.PriceIconSize;
            goldRect.anchoredPosition = new Vector2(-layout.PriceIconGap * 0.5f, 0f);

            TMP_Text label = UiSceneBuilder.NewText(
                name + "Label", root, "0", layout.PriceFontSize,
                new Color(1f, 0.85f, 0.25f), TextAlignmentOptions.MidlineLeft, true);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(layout.PriceSize.x * 0.5f, layout.PriceSize.y);
            labelRect.anchoredPosition = new Vector2(layout.PriceIconGap * 0.5f, 0f);
            return label;
        }

        /// <summary>그림 한 장을 기획서 자리에 놓는다. 9조각이 아니라 통 그림이다.</summary>
        private static Image NewPicture(
            string name, RectTransform parent, Sprite sprite, Vector2 position, Vector2 size)
        {
            Image image = UiSceneBuilder.NewFlatImage(name, parent, Color.white);
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            UiSceneBuilder.PlaceTopLeft(image.rectTransform, position, size);
            return image;
        }

        /// <summary>
        /// 런 종료 결과 화면의 칸 하나.
        /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의
        /// 최종 구성, 이번 런 기록, 해금 및 도전과제 세 칸이 모두 같은 생김새다.
        ///
        /// 배경과 보이는 칸은 RunResultScreenView 가 화면 좌표로 자리를 잡으므로
        /// 둘 다 화면 전체에 펼친 홀더 아래 그대로 둔다. 한 겹 더 넣으면 자리가 두 번 밀린다.
        /// 글만 보이는 칸 안에 넣어 칸을 넘치면 잘리고 그 안에서만 스크롤되게 한다.
        /// </summary>
        private static RunResultPanelView BuildRunResultPanel(
            string name, RectTransform parent, RunResultVisualConfig visual)
        {
            RectTransform holder = UiSceneBuilder.NewRect(name, parent);
            UiSceneBuilder.Stretch(holder);

            Image panel = UiSceneBuilder.NewImage("Panel", holder, visual.PanelColor);

            RectTransform viewport = UiSceneBuilder.NewRect("Content", holder);
            viewport.gameObject.AddComponent<RectMask2D>();

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", viewport, string.Empty,
                visual.BodyFontSize, visual.TextColor, TextAlignmentOptions.TopLeft);

            // 글은 보이는 칸 위쪽에 붙어 아래로 자란다. 높이는 줄 수에 맞춰 뷰가 잡는다.
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;

            ScrollRect scroll = UiSceneBuilder.AddScroll(viewport, labelRect);
            scroll.movementType = ScrollRect.MovementType.Clamped;

            RunResultPanelView view = holder.gameObject.AddComponent<RunResultPanelView>();
            UiSceneBuilder.SetField(view, "_panel", panel);
            UiSceneBuilder.SetField(view, "_scroll", scroll);
            UiSceneBuilder.SetField(view, "_content", viewport);
            UiSceneBuilder.SetField(view, "_label", label);

            return view;
        }

        /// <summary>상단 표시줄의 아이콘 한 벌. 체력과 골드가 같은 모양을 쓴다.</summary>
        private static TopBarField BuildTopBarField(
            string name, RectTransform parent, TopBarVisualConfig visual, float fontSize, Color color)
        {
            RectTransform holder = UiSceneBuilder.NewRect(name, parent);

            // 아이콘이 왼쪽 끝, 글자는 그 오른쪽 54 부터다. 예시 이미지에서 잰 간격이다.
            Image icon = UiSceneBuilder.NewFlatImage("Icon", holder, Color.white);
            PlaceInRow(icon.rectTransform, 0f, 0f, UiScale.Px(40f), UiScale.Px(40f));

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", holder, string.Empty, fontSize, color,
                TextAlignmentOptions.MidlineLeft, true, true);
            PlaceInRow(label.rectTransform, UiScale.Px(66f), 0f, UiScale.Px(220f), UiScale.Px(64f));

            TopBarField field = holder.gameObject.AddComponent<TopBarField>();
            UiSceneBuilder.SetField(field, "_icon", icon);
            UiSceneBuilder.SetField(field, "_label", label);

            return field;
        }

        /// <summary>상단 표시줄의 버튼 하나.</summary>
        private static TopBarButton BuildTopBarButton(
            RectTransform parent, TopBarVisualConfig visual,
            TopBarButtonKind kind, string tooltip, string iconName)
        {
            RectTransform holder = UiSceneBuilder.NewRect(kind.ToString() + "Button", parent);
            UiSceneBuilder.AddButton(holder.gameObject);

            Image icon = UiSceneBuilder.NewFlatImage("Icon", holder, Color.white);
            icon.sprite = ImportArt.LoadTopBar(iconName);
            UiSceneBuilder.Stretch(icon.rectTransform);

            TopBarButton button = holder.gameObject.AddComponent<TopBarButton>();
            UiSceneBuilder.SetField(button, "_button", holder.GetComponent<Button>());
            UiSceneBuilder.SetField(button, "_icon", icon);
            UiSceneBuilder.SetField(button, "_visual", visual);

            SerializedObject so = new SerializedObject(button);
            so.FindProperty("_kind").enumValueIndex = (int)kind;
            so.FindProperty("_tooltipText").stringValue = tooltip;
            so.ApplyModifiedPropertiesWithoutUndo();

            return button;
        }

        /// <summary>
        /// 현재 방 설명이 뜨는 자리.
        ///
        /// **껍데기와 켜고 끄는 칸을 나눈다.**
        /// 스크립트를 켜고 끄는 칸에 바로 붙이면 자기 자신을 꺼 버린다.
        /// 껍데기는 늘 켜져 있고 그 아래 `Root` 만 켜고 끈다.
        ///
        /// 글자와 배경은 커서를 받지 않는다.
        /// 받으면 버튼 바로 아래에서 커서를 가로채 풍선이 점멸하고 클릭도 먹는다.
        /// </summary>
        private static TopBarTooltip BuildTopBarTooltip(RectTransform parent, TopBarVisualConfig visual)
        {
            RectTransform holder = UiSceneBuilder.NewRect("Tooltip", parent);
            UiSceneBuilder.Stretch(holder);

            // 기준점은 부모 한가운데, 피벗은 위쪽 가운데다.
            // 자리 계산이 부모 한가운데를 0 으로 보기 때문이다. `TopBarTooltip.Resize` 를 본다.
            RectTransform root = UiSceneBuilder.NewRect("Root", holder);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 1f);

            Image background = UiSceneBuilder.NewImage("Background", root, visual.TooltipBackgroundColor);
            UiSceneBuilder.Stretch(background.rectTransform);
            background.raycastTarget = false;

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", root, string.Empty, visual.TooltipFontSize,
                visual.TooltipTextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(label.rectTransform);
            label.raycastTarget = false;

            TopBarTooltip tooltip = holder.gameObject.AddComponent<TopBarTooltip>();
            UiSceneBuilder.SetField(tooltip, "_root", root);
            UiSceneBuilder.SetField(tooltip, "_background", background);
            UiSceneBuilder.SetField(tooltip, "_label", label);
            UiSceneBuilder.SetField(tooltip, "_visual", visual);

            root.gameObject.SetActive(false);
            return tooltip;
        }

        /// <summary>버튼 배열은 낱개가 아니라 줄로 넣어야 해서 따로 다룬다.</summary>
        private static void SetButtonArray(TopBarView view, TopBarButton[] buttons)
        {
            SerializedObject so = new SerializedObject(view);
            SerializedProperty property = so.FindProperty("_buttons");

            if (property == null)
            {
                return;
            }

            property.arraySize = buttons.Length;
            for (int i = 0; i < buttons.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>고를 수 있는 방을 따라 세 걸음 걷는다. 진행 상태를 눈으로 보려는 것이다.</summary>
        private static void WalkThreeSteps(StageMap map, MapProgress progress)
        {
            List<MapNode> selectable = new List<MapNode>();

            for (int step = 0; step < 3; step++)
            {
                selectable.Clear();
                progress.GetSelectableNodes(map, selectable);

                if (selectable.Count == 0)
                {
                    break;
                }

                progress.EnterNode(map, selectable[0].Id);

                // 마지막 방은 아직 깨지 않은 채로 둔다. 지금 머무는 방으로 보인다.
                if (step < 2)
                {
                    progress.ClearCurrentRoom();
                }
            }
        }

        /// <summary>맵 노드 프리팹.</summary>
        private static GameObject BuildMapNodePrefab()
        {
            GameObject go = new GameObject("MapNode", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            Image icon = UiSceneBuilder.NewFlatImage("Icon", go.transform, Color.white);
            UiSceneBuilder.Stretch(icon.rectTransform);

            // 완료 체크는 노드 오른쪽 위에 겹쳐 놓는다. 크기와 자리는 MapNodeView.Bind 가 MapVisualConfig 에서 읽어 다시 맞춘다.
            MapVisualConfig visualDefaults = ScriptableObject.CreateInstance<MapVisualConfig>();
            Image check = UiSceneBuilder.NewFlatImage("CheckMark", go.transform, Color.white);
            check.sprite = ImportArt.LoadCheckMark();
            check.raycastTarget = false;
            RectTransform checkRect = check.rectTransform;
            checkRect.anchorMin = new Vector2(1f, 1f);
            checkRect.anchorMax = new Vector2(1f, 1f);
            checkRect.pivot = new Vector2(1f, 1f);
            checkRect.sizeDelta = visualDefaults.CheckMarkSize;
            checkRect.anchoredPosition = visualDefaults.CheckMarkOffset;
            UnityEngine.Object.DestroyImmediate(visualDefaults);

            MapNodeView view = go.AddComponent<MapNodeView>();
            UiSceneBuilder.SetField(view, "_icon", icon);
            UiSceneBuilder.SetField(view, "_checkMark", check);
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());

            return UiSceneBuilder.SavePrefab(go, "MapNode");
        }

        /// <summary>맵 간선 프리팹.</summary>
        private static GameObject BuildMapEdgePrefab()
        {
            GameObject go = new GameObject("MapEdge", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);

            Image line = UiSceneBuilder.NewFlatImage("Line", go.transform, Color.white);
            UiSceneBuilder.Stretch(line.rectTransform);

            MapEdgeView view = go.AddComponent<MapEdgeView>();
            UiSceneBuilder.SetField(view, "_line", line);

            return UiSceneBuilder.SavePrefab(go, "MapEdge");
        }

        /// <summary>이벤트 선택지 프리팹.</summary>
        private static GameObject BuildEventChoicePrefab(EventVisualConfig visual)
        {
            GameObject go = new GameObject("EventChoice", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            Image background = UiSceneBuilder.NewButtonImage("Background", go.transform, visual.PanelColor);
            UiSceneBuilder.AddButton(go, background);
            UiSceneBuilder.Stretch(background.rectTransform);

            // 줄바꿈을 켠다. 칸이 글 높이에 맞춰 늘어나므로 긴 선택지도 잘리지 않는다.
            // `EventChoiceView` 가 글을 넣으면서 높이를 재고 칸을 그만큼 키운다.
            TMP_Text label = UiSceneBuilder.NewText(
                "Label", go.transform, "선택지", visual.ChoiceFontSize,
                visual.TextColor, TextAlignmentOptions.MidlineLeft);
            InsetText(label.rectTransform, visual.ChoicePadding);

            EventChoiceView view = go.AddComponent<EventChoiceView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_label", label);

            return UiSceneBuilder.SavePrefab(go, "EventChoice");
        }

        /// <summary>팝업 버튼 프리팹.</summary>
        private static GameObject BuildPopupButtonPrefab(PopupVisualConfig visual)
        {
            GameObject go = new GameObject("PopupButton", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            Image background = UiSceneBuilder.NewButtonImage("Background", go.transform, visual.ButtonColor);
            UiSceneBuilder.AddButton(go, background);
            UiSceneBuilder.Stretch(background.rectTransform);

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", go.transform, "버튼", visual.ButtonFontSize,
                visual.ButtonTextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(label.rectTransform);

            PopupButtonView view = go.AddComponent<PopupButtonView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_label", label);

            return UiSceneBuilder.SavePrefab(go, "PopupButton");
        }

        /// <summary>현재 빌드의 칸 프리팹.</summary>
        private static GameObject BuildSlotPrefab(CurrentBuildVisualConfig visual)
        {
            GameObject go = new GameObject("BuildSlot", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);

            // 고른 칸의 테두리. 배경보다 먼저 두어 배경 뒤에 깔리고 바깥으로만 삐져나온다.
            // 고르기 화면에서만 켠다. 현재 빌드 화면에서는 늘 꺼져 있다.
            Image selectedBorder = UiSceneBuilder.NewImage("SelectedBorder", go.transform, visual.PickSelectedBorderColor);
            UiSceneBuilder.Stretch(selectedBorder.rectTransform);
            selectedBorder.raycastTarget = false;
            selectedBorder.gameObject.SetActive(false);

            Image background = UiSceneBuilder.NewButtonImage("Background", go.transform, visual.SlotColor);
            UiSceneBuilder.Stretch(background.rectTransform);

            // 물건 그림이 아직 없어 자리를 알아볼 수 있게 옅은 칸을 둔다.
            Image icon = UiSceneBuilder.NewImage("Icon", go.transform, new Color(1f, 1f, 1f, 0.2f));
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = new Vector2(0.15f, 0.15f);
            iconRect.anchorMax = new Vector2(0.85f, 0.85f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image countBackground = UiSceneBuilder.NewImage(
                "CountBackground", go.transform, visual.CountBadgeColor);
            TMP_Text countLabel = UiSceneBuilder.NewText(
                "CountLabel", countBackground.transform, "×1", visual.CountFontSize,
                visual.TextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(countLabel.rectTransform);

            BuildSlotView view = go.AddComponent<BuildSlotView>();
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_icon", icon);
            UiSceneBuilder.SetField(view, "_countRoot", countBackground.gameObject);
            UiSceneBuilder.SetField(view, "_countBackground", countBackground);
            UiSceneBuilder.SetField(view, "_countLabel", countLabel);
            UiSceneBuilder.SetField(view, "_selectedBorder", selectedBorder);

            return UiSceneBuilder.SavePrefab(go, "BuildSlot");
        }

        /// <summary>태그 막대 프리팹.</summary>
        private static GameObject BuildTagBarPrefab(CurrentBuildVisualConfig visual)
        {
            GameObject go = new GameObject("BuildTagBar", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);

            // 줄 하나의 크기와 자리는 CurrentBuildScreenView.PlaceTagBars 가 잡는다.
            // 줄의 왼쪽 위가 TagSymbolX 이고 높이가 TagRowHeight 이므로
            // PlaceInRow 가 기준으로 삼는 왼쪽 가운데는 막대 위쪽 끝에서 20 위다.
            //
            // 그래서 줄 안의 자리는 이렇게 나온다.
            //   x  막대는 TagBarX - TagSymbolX = 80 부터. 이름과 장수도 막대에 맞춘다.
            //   y  이름과 장수는 0, 막대 가운데는 -30, 기호는 그 둘의 가운데인 -10
            //
            // 14 현재 빌드 화면 와이어프레임의 태그 줄을 그대로 옮긴 것이다.
            float BarX = UiScale.Px(80f);
            float BarWidth = UiScale.Px(300f);

            // 행성 기호는 막대 왼쪽 칸에 크게 하나 선다.
            // 맑은 고딕에 글리프가 없어 글자로 두면 네모로 나오므로 그림을 먼저 쓰고
            // 그림이 없을 때만 글자가 대신 나온다.
            Image symbolIcon = UiSceneBuilder.NewFlatImage("SymbolIcon", go.transform, Color.white);
            PlaceInRow(symbolIcon.rectTransform, 0f, UiScale.Px(-10f), UiScale.Px(60f), UiScale.Px(60f));

            TMP_Text symbolLabel = UiSceneBuilder.NewText(
                "SymbolLabel", go.transform, "☿", visual.TagSymbolFontSize,
                visual.TextColor, TextAlignmentOptions.Center);
            PlaceInRow(symbolLabel.rectTransform, 0f, UiScale.Px(-10f), UiScale.Px(60f), UiScale.Px(60f));

            TMP_Text nameLabel = UiSceneBuilder.NewText(
                "NameLabel", go.transform, "태그", visual.TagFontSize,
                visual.SubTextColor, TextAlignmentOptions.Left);
            PlaceInRow(nameLabel.rectTransform, BarX, 0f, UiScale.Px(150f), UiScale.Px(30f));

            TMP_Text countLabel = UiSceneBuilder.NewText(
                "CountLabel", go.transform, "0장", visual.TagFontSize,
                visual.SubTextColor, TextAlignmentOptions.Right);
            PlaceInRow(countLabel.rectTransform, BarX + UiScale.Px(150f), 0f, UiScale.Px(150f), UiScale.Px(30f));

            Image barBackground = UiSceneBuilder.NewImage(
                "BarBackground", go.transform, visual.TagBarBackColor);
            PlaceInRow(barBackground.rectTransform, BarX, UiScale.Px(-30f), BarWidth, UiScale.Px(20f));

            // 채워지는 막대는 배경 막대 안에 둔다.
            // Bind 가 자리를 0 으로 놓으므로 바깥에 두면 줄 왼쪽 끝에서 시작해 버린다.
            Image bar = UiSceneBuilder.NewImage("Bar", barBackground.transform, Color.white);

            BuildTagBarView view = go.AddComponent<BuildTagBarView>();
            UiSceneBuilder.SetField(view, "_symbolIcon", symbolIcon);
            UiSceneBuilder.SetField(view, "_symbolLabel", symbolLabel);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_countLabel", countLabel);
            UiSceneBuilder.SetField(view, "_barBackground", barBackground);
            UiSceneBuilder.SetField(view, "_bar", bar);

            return UiSceneBuilder.SavePrefab(go, "BuildTagBar");
        }


        /// <summary>설정 분류 탭 프리팹.</summary>
        private static GameObject BuildSettingsTabPrefab(SettingsVisualConfig visual)
        {
            GameObject go = new GameObject("SettingsTab", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            Image background = UiSceneBuilder.NewButtonImage("Background", go.transform, visual.TabColor);
            UiSceneBuilder.AddButton(go, background);
            UiSceneBuilder.Stretch(background.rectTransform);

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", go.transform, "분류", visual.ButtonFontSize,
                visual.TabTextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(label.rectTransform);

            SettingsTabView view = go.AddComponent<SettingsTabView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_label", label);

            return UiSceneBuilder.SavePrefab(go, "SettingsTab");
        }

        /// <summary>
        /// 설정 항목 프리팹. 이름은 왼쪽, 값은 오른쪽이다.
        /// 소리 크기 줄에 쓰는 막대와 음소거 버튼도 함께 넣어 두고 줄 종류에 따라 켜고 끈다.
        /// </summary>
        private static GameObject BuildSettingsRowPrefab(SettingsLayoutConfig layout, SettingsVisualConfig visual)
        {
            GameObject go = new GameObject("SettingsRow", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            Image background = UiSceneBuilder.NewButtonImage("Background", go.transform, visual.ItemColor);
            UiSceneBuilder.AddButton(go, background);
            UiSceneBuilder.Stretch(background.rectTransform);

            TMP_Text nameLabel = UiSceneBuilder.NewText(
                "NameLabel", go.transform, "항목", visual.ItemFontSize,
                visual.ItemTextColor, TextAlignmentOptions.Left, true);

            TMP_Text valueLabel = UiSceneBuilder.NewText(
                "ValueLabel", go.transform, "값", visual.ItemFontSize,
                visual.ItemTextColor, TextAlignmentOptions.Right, true, true);

            // 막대. 바탕, 찬 부분, 손잡이 순서로 그린다.
            RectTransform sliderRect = UiSceneBuilder.NewRect("Slider", go.transform);
            Image track = NewRoundedBar("Track", sliderRect, visual.SliderTrackColor);
            track.raycastTarget = true;

            RectTransform fillArea = UiSceneBuilder.NewRect("FillArea", sliderRect);
            Image fill = NewRoundedBar("Fill", fillArea, visual.SliderFillColor);
            UiSceneBuilder.Stretch(fill.rectTransform);

            // 손잡이가 막대 양 끝에서 반만 나가도록 손잡이 폭의 반만큼 안쪽에서 움직인다.
            RectTransform handleArea = UiSceneBuilder.NewRect("HandleArea", sliderRect);
            UiSceneBuilder.Stretch(handleArea);
            handleArea.offsetMin = new Vector2(layout.SliderHandleSize * 0.5f, 0f);
            handleArea.offsetMax = new Vector2(-layout.SliderHandleSize * 0.5f, 0f);

            // 손잡이에는 그림자를 달지 않는다. 그림자는 손잡이를 끌 때 따라오지 못하고 줄 가운데에 남는다.
            Image handle = NewRoundedBar("Handle", handleArea, visual.SliderHandleColor);
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 1f);
            handleRect.sizeDelta = new Vector2(layout.SliderHandleSize, 0f);

            Slider slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.targetGraphic = handle;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;

            // 음소거 버튼. 줄 오른쪽 끝에 붙는다.
            Image muteBackground = UiSceneBuilder.NewButtonImage("MuteButton", go.transform, visual.MuteOffColor);
            Button muteButton = UiSceneBuilder.AddButton(muteBackground.gameObject);
            TMP_Text muteLabel = UiSceneBuilder.NewText(
                "MuteLabel", muteBackground.transform, visual.MuteText, visual.ItemFontSize,
                visual.ItemTextColor, TextAlignmentOptions.Center, true, true);
            UiSceneBuilder.Stretch(muteLabel.rectTransform);

            SettingsRowView view = go.AddComponent<SettingsRowView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_valueLabel", valueLabel);
            UiSceneBuilder.SetField(view, "_slider", slider);
            UiSceneBuilder.SetField(view, "_sliderTrack", track);
            UiSceneBuilder.SetField(view, "_sliderFill", fill);
            UiSceneBuilder.SetField(view, "_sliderHandle", handle);
            UiSceneBuilder.SetField(view, "_muteButton", muteButton);
            UiSceneBuilder.SetField(view, "_muteBackground", muteBackground);
            UiSceneBuilder.SetField(view, "_muteLabel", muteLabel);

            // 고르는 줄이 기본이다. 막대와 음소거는 소리 크기 줄에서만 켠다.
            sliderRect.gameObject.SetActive(false);
            muteBackground.gameObject.SetActive(false);

            return UiSceneBuilder.SavePrefab(go, "SettingsRow");
        }

        /// <summary>막대에 쓰는 둥근 칸. 스킨 그림은 두께가 있어 얇은 막대에 맞지 않으므로 기본 둥근 그림을 쓴다.</summary>
        private static Image NewRoundedBar(string name, Transform parent, Color color)
        {
            Image image = UiSceneBuilder.NewFlatImage(name, parent, color);
            image.sprite = UiSceneBuilder.RoundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 0.1f;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>드롭다운 선택지 프리팹. 탭 프리팹과 구성이 같다.</summary>
        private static GameObject BuildSettingsOptionPrefab(SettingsVisualConfig visual)
        {
            GameObject go = new GameObject("SettingsOption", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            Image background = UiSceneBuilder.NewFlatImage("Background", go.transform, visual.ItemColor);
            UiSceneBuilder.Stretch(background.rectTransform);

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", go.transform, "선택지", visual.ItemFontSize,
                visual.ItemTextColor, TextAlignmentOptions.Center, true, true);
            UiSceneBuilder.Stretch(label.rectTransform);

            SettingsOptionView view = go.AddComponent<SettingsOptionView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_label", label);

            return UiSceneBuilder.SavePrefab(go, "SettingsOption");
        }

        /// <summary>메뉴 항목 프리팹. 루트에 Button 과 항목 스크립트, 그 아래에 글자를 둔다.</summary>
        private static GameObject BuildTitleMenuItemPrefab(TitleVisualConfig visual)
        {
            GameObject go = new GameObject("TitleMenuItem", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            RectTransform content = UiSceneBuilder.NewRect("Content", go.transform);
            UiSceneBuilder.Stretch(content);

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", content, "메뉴",
                visual != null ? visual.MenuFontSize : UiScale.Px(40f),
                Color.white,
                TextAlignmentOptions.Left,
                true);
            UiSceneBuilder.Stretch(label.rectTransform);

            TitleMenuItemView view = go.AddComponent<TitleMenuItemView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_content", content);
            UiSceneBuilder.SetField(view, "_label", label);

            return UiSceneBuilder.SavePrefab(go, "TitleMenuItem");
        }

        /// <summary>프로필 카드 프리팹.</summary>
        private static GameObject BuildProfileCardPrefab(
            ProfileLayoutConfig layout, ProfileVisualConfig visual)
        {
            GameObject go = new GameObject("ProfileCard", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            UiSceneBuilder.AddButton(go);

            RectTransform content = UiSceneBuilder.NewRect("Content", go.transform);
            UiSceneBuilder.Stretch(content);

            Image background = UiSceneBuilder.NewButtonImage("Background", content, Color.white);
            UiSceneBuilder.AddButton(go, background);
            UiSceneBuilder.Stretch(background.rectTransform);

            TMP_Text nameLabel = UiSceneBuilder.NewText(
                "NameLabel", content, "프로필", visual.NameFontSize,
                visual.NameColor, TextAlignmentOptions.Left, true);

            TMP_Text playTime = UiSceneBuilder.NewText(
                "PlayTimeLabel", content, "플레이 시간", visual.InfoFontSize,
                visual.InfoColor, TextAlignmentOptions.Left);

            TMP_Text lastPlayed = UiSceneBuilder.NewText(
                "LastPlayedLabel", content, "마지막 플레이", visual.InfoFontSize,
                visual.InfoColor, TextAlignmentOptions.Left);

            TMP_Text emptyLabel = UiSceneBuilder.NewText(
                "EmptyLabel", content, visual.EmptyText, visual.EmptyFontSize,
                visual.EmptyTextColor, TextAlignmentOptions.Center, true);
            UiSceneBuilder.Stretch(emptyLabel.rectTransform);

            Image editPanel = UiSceneBuilder.NewButtonImage("EditPanel", content, visual.ActionButtonColor);
            UiSceneBuilder.AddButton(editPanel.gameObject);
            TMP_Text editLabel = UiSceneBuilder.NewText(
                "EditLabel", editPanel.transform, visual.EditText, visual.ActionFontSize,
                visual.EditTextColor, TextAlignmentOptions.Center);
            UiSceneBuilder.Stretch(editLabel.rectTransform);

            Image deletePanel = UiSceneBuilder.NewButtonImage("DeletePanel", content, visual.ActionButtonColor);
            UiSceneBuilder.AddButton(deletePanel.gameObject);
            TMP_Text deleteLabel = UiSceneBuilder.NewText(
                "DeleteLabel", deletePanel.transform, visual.DeleteText, visual.ActionFontSize,
                visual.DeleteTextColor, TextAlignmentOptions.Center);
            UiSceneBuilder.Stretch(deleteLabel.rectTransform);

            // 이름을 치는 칸. 빈 자리를 누르면 이것이 켜진다.
            // 이게 없으면 프로필을 만들 수가 없어 게임을 시작할 길이 막힌다.
            TMP_InputField nameInput = UiSceneBuilder.NewInputField(
                "NameInput", content, visual.NameFontSize, visual.NameColor);

            ProfileCardView view = go.AddComponent<ProfileCardView>();
            UiSceneBuilder.SetField(view, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_content", content);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_nameInput", nameInput);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_playTimeLabel", playTime);
            UiSceneBuilder.SetField(view, "_lastPlayedLabel", lastPlayed);
            UiSceneBuilder.SetField(view, "_emptyLabel", emptyLabel);
            UiSceneBuilder.SetField(view, "_editButton", editPanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_editPanel", editPanel);
            UiSceneBuilder.SetField(view, "_editLabel", editLabel);
            UiSceneBuilder.SetField(view, "_deleteButton", deletePanel.GetComponent<Button>());
            UiSceneBuilder.SetField(view, "_deletePanel", deletePanel);
            UiSceneBuilder.SetField(view, "_deleteLabel", deleteLabel);

            return UiSceneBuilder.SavePrefab(go, "ProfileCard");
        }

        /// <summary>타이틀 오른쪽 위의 현재 프로필 버튼.</summary>
        private static void BuildCurrentProfileButton(RectTransform parent, string profileName)
        {
            ProfileLayoutConfig layout =
                UiSceneBuilder.LoadConfig<ProfileLayoutConfig>("Profile", "ProfileLayoutConfig");
            ProfileVisualConfig visual =
                UiSceneBuilder.LoadConfig<ProfileVisualConfig>("Profile", "ProfileVisualConfig");

            GameObject go = new GameObject("CurrentProfileButton", typeof(RectTransform));
            UiSceneBuilder.SizeDefault((RectTransform)go.transform);
            go.transform.SetParent(parent, false);
            UiSceneBuilder.AddButton(go);

            Image fill = UiSceneBuilder.NewButtonImage("Fill", go.transform, visual.CurrentButtonFillColor);
            UiSceneBuilder.AddButton(go, fill);
            UiSceneBuilder.Stretch(fill.rectTransform);

            Image border = UiSceneBuilder.NewImage(
                "Border", go.transform, visual.CurrentButtonBorderColor);
            UiSceneBuilder.Stretch(border.rectTransform);

            // 테두리만 그리는 그림이 아직 없어, 안쪽을 바탕색으로 덮어 테두리처럼 보이게 한다.
            // 그림이 생기면 이 Inner 를 지우고 Border 에 그 그림을 물리면 된다.
            Image inner = UiSceneBuilder.NewImage("Inner", border.transform, DarkBackground);
            RectTransform innerRect = inner.rectTransform;
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = UiScale.V(2f, 2f);
            innerRect.offsetMax = UiScale.V(-2f, -2f);

            TMP_Text label = UiSceneBuilder.NewText(
                "Label", go.transform, profileName, visual.CurrentButtonFontSize,
                visual.CurrentButtonTextColor, TextAlignmentOptions.Left, true);

            CurrentProfileButton button = go.AddComponent<CurrentProfileButton>();
            UiSceneBuilder.SetField(button, "_button", go.GetComponent<Button>());
            UiSceneBuilder.SetField(button, "_fill", fill);
            UiSceneBuilder.SetField(button, "_border", border);
            UiSceneBuilder.SetField(button, "_label", label);
            UiSceneBuilder.SetField(button, "_layout", layout);
            UiSceneBuilder.SetField(button, "_visual", visual);

            button.ApplyLayout();
            button.SetName(profileName);
        }

        /// <summary>태그 줄 안에서 왼쪽 가운데를 기준으로 자리를 잡는다.</summary>
        private static void PlaceInRow(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>글자를 칸 안쪽으로 들여 놓는다.</summary>
        private static void InsetText(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, 0f);
            rect.offsetMax = new Vector2(-padding, 0f);
        }

        private static Scene NewScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 이게 없으면 버튼이 눌리지 않는다. 모든 확인용 씬에 하나씩 둔다.
            UiSceneBuilder.CreateEventSystem();

            // 카메라가 없으면 유니티가 아무것도 그리지 않는다고 알린다.
            // 확인용 씬도 게임 씬과 같은 카메라를 두어 보이는 것이 같게 한다.
            _camera = UiSceneBuilder.CreateCamera("Main Camera", DarkBackground, true);

            return scene;
        }

        /// 지금 만들고 있는 씬의 카메라. 캔버스를 여기에 물린다
        private static Camera _camera;

        /// <summary>
        /// 씬을 눌러 볼 수 있게 굴리는 스크립트를 놓는다.
        /// 플레이를 누르면 화면 조종기를 찾아 보기 자료로 연다.
        /// </summary>
        private static void AddDriver()
        {
            GameObject go = new GameObject("PreviewDriver");
            PreviewDriver driver = go.AddComponent<PreviewDriver>();

            UiSceneBuilder.SetField(driver, "_buildVisual",
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig"));
            UiSceneBuilder.SetField(driver, "_runResultVisual",
                UiSceneBuilder.LoadConfig<RunResultVisualConfig>("RunResult", "RunResultVisualConfig"));
        }

        private static void SaveScene(Scene scene, string name)
        {
            // 칸 자리가 다 잡힌 뒤라야 그림자가 제자리에 깔린다.
            UiSceneBuilder.SyncShadows();

            UiSceneBuilder.EnsureFolder(UiSceneBuilder.SceneFolder);

            string path = UiSceneBuilder.SceneFolder + "/" + name + ".unity";
            EditorSceneManager.SaveScene(scene, path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("확인용 씬을 만들었다: " + path);
        }
    }
}
