using SlotHero.Combat;
using SlotHero.Combat.UI;
using SlotHero.CurrentBuild;
using SlotHero.Flow;
using SlotHero.Tools;
using SlotHero.Ui;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 전투 화면의 확인용 씬. 전투 UI 및 연출 초안 / 03 · 04 기초 화면 정보 를 짓는다.
    /// 화면이 커서 다른 화면과 따로 이 파일에 둔다.
    /// </summary>
    public static partial class BuildPreviewScenes
    {
        /// 전투 확인용 씬 이름. 게임 씬이 여기서 전투 화면을 가져간다.
        public const string CombatSceneName = "14 전투 화면";

        [MenuItem("Slot Hero/확인용 씬 만들기/전투 화면")]
        public static void BuildCombat()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("CombatCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            CombatLayoutConfig layout = UiSceneBuilder.LoadConfig<CombatLayoutConfig>("Combat", "CombatLayoutConfig");
            CombatVisualConfig visual = UiSceneBuilder.LoadConfig<CombatVisualConfig>("Combat", "CombatVisualConfig");
            CombatRuleConfig rules = UiSceneBuilder.LoadConfig<CombatRuleConfig>("Combat", "CombatRuleConfig");
            CurrentBuildVisualConfig tags =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");
            RunCatalogConfig catalog = UiSceneBuilder.LoadConfig<RunCatalogConfig>("Flow", "RunCatalogConfig");

            if (layout == null || visual == null || rules == null)
            {
                Debug.LogWarning("전투 설정 에셋이 없다. 먼저 `설정 에셋 한 번에 만들기` 를 누른다.");
                return;
            }

            Sprite circle = UiSceneBuilder.LoadSkin("원");

            RectTransform root = UiSceneBuilder.NewRect("CombatScreen", canvas.transform);
            UiSceneBuilder.Stretch(root);

            // 확인용 씬에서만 까는 바탕. 게임 씬으로 옮길 때 지워지고 공용 배경이 보인다.
            UiSceneBuilder.NewStoneBackdrop("BehindScreen", root);

            // ---- 문양 ----
            CombatReelView[] reels = new CombatReelView[rules.ReelCount];
            for (int i = 0; i < reels.Length; i++)
            {
                reels[i] = BuildReel("Reel" + i, root, layout, visual, circle);
            }

            // ---- 유물 ----
            Image relicPanel = UiSceneBuilder.NewImage("RelicPanel", root, visual.PanelColor);
            relicPanel.raycastTarget = false;
            UiSceneBuilder.AddShadowPx(relicPanel, 10f, 6f);

            const int relicSlotCount = 6;
            Image[] relicSlots = new Image[relicSlotCount];
            Image[] relicIcons = new Image[relicSlotCount];
            TMP_Text[] relicLabels = new TMP_Text[relicSlotCount];
            TMP_Text[] relicCounters = new TMP_Text[relicSlotCount];
            PointerRelay[] relicHovers = new PointerRelay[relicSlotCount];
            for (int i = 0; i < relicSlotCount; i++)
            {
                // 칸이 마우스를 받아 유물 정보 팝업을 띄운다.
                relicSlots[i] = UiSceneBuilder.NewImage("RelicSlot" + i, root, visual.EmptyColor);
                relicIcons[i] = NewIcon("Icon", relicSlots[i].transform, UiScale.Px(10f));

                // 그림이 없을 때 이름을 적는다.
                relicLabels[i] = NewLabel("NameLabel", relicSlots[i].transform, visual.BodyFontSize * 0.85f, visual.TextColor, true);
                UiSceneBuilder.Stretch(relicLabels[i].rectTransform);
                relicLabels[i].rectTransform.offsetMin = new Vector2(UiScale.Px(6f), UiScale.Px(6f));
                relicLabels[i].rectTransform.offsetMax = new Vector2(-UiScale.Px(6f), -UiScale.Px(6f));

                // 고유 수치(12개의 금화). 오른쪽 아래에 붙인다.
                relicCounters[i] = NewLabel("CounterLabel", relicSlots[i].transform, visual.BodyFontSize, visual.CostColor, true);
                RectTransform counter = relicCounters[i].rectTransform;
                counter.anchorMin = new Vector2(1f, 0f);
                counter.anchorMax = new Vector2(1f, 0f);
                counter.pivot = new Vector2(1f, 0f);
                counter.sizeDelta = layout.RelicCounterSize;
                counter.anchoredPosition = Vector2.zero;
                relicCounters[i].alignment = TextAlignmentOptions.BottomRight;
                relicCounters[i].gameObject.SetActive(false);

                relicHovers[i] = relicSlots[i].gameObject.AddComponent<PointerRelay>();
            }

            // 유물 정보 팝업. 유물 목록 오른쪽에 뜬다.
            Image relicPopup = UiSceneBuilder.NewImage("RelicPopup", root, visual.PopupColor);
            relicPopup.raycastTarget = false;
            TMP_Text relicPopupName = NewLabel("NameLabel", relicPopup.transform, visual.NameFontSize * 0.85f, visual.DarkTextColor, true);
            AnchorBand(relicPopupName.rectTransform, 0.78f, 1f, UiScale.Px(14f));
            relicPopupName.alignment = TextAlignmentOptions.Left;
            TMP_Text relicPopupBody = NewLabel("BodyLabel", relicPopup.transform, visual.BodyFontSize, visual.DarkTextColor, false);
            AnchorBand(relicPopupBody.rectTransform, 0.3f, 0.78f, UiScale.Px(14f));
            relicPopupBody.alignment = TextAlignmentOptions.TopLeft;
            TMP_Text relicPopupFlavor = NewLabel("FlavorLabel", relicPopup.transform, visual.BodyFontSize * 0.85f, visual.SubTextColor, false);
            AnchorBand(relicPopupFlavor.rectTransform, 0.04f, 0.3f, UiScale.Px(14f));
            relicPopupFlavor.alignment = TextAlignmentOptions.BottomLeft;
            relicPopupFlavor.fontStyle = FontStyles.Italic;
            relicPopup.gameObject.SetActive(false);

            // ---- 플레이어 ----
            Image playerPanel;
            Image playerBar;
            TMP_Text playerLabel;
            BuildHealthPanel("PlayerHealth", root, visual, visual.PlayerBarColor, out playerPanel, out playerBar, out playerLabel);

            // ---- 몬스터 ----
            Image enemyPanel;
            Image enemyBar;
            TMP_Text enemyLabel;
            BuildHealthPanel("EnemyHealth", root, visual, visual.EnemyBarColor, out enemyPanel, out enemyBar, out enemyLabel);

            // 몬스터 그림이 아직 없어 이름을 적은 칸으로 둔다.
            Image enemyArt = UiSceneBuilder.NewImage("EnemyArt", root, visual.EnemyArtStandIn);
            enemyArt.raycastTarget = false;
            TMP_Text enemyArtLabel = NewLabel("NameLabel", enemyArt.transform, visual.NameFontSize, visual.TextColor, true);
            UiSceneBuilder.Stretch(enemyArtLabel.rectTransform);

            Image enemyInfo = UiSceneBuilder.NewImage("EnemyInfo", root, visual.PanelColor);
            enemyInfo.raycastTarget = false;
            TMP_Text enemyName = NewLabel("NameLabel", enemyInfo.transform, visual.NameFontSize, visual.TextColor, true);
            AnchorBand(enemyName.rectTransform, 0.78f, 1f, UiScale.Px(16f));
            TMP_Text enemyInfoLabel = NewLabel("InfoLabel", enemyInfo.transform, visual.BodyFontSize, visual.SubTextColor, false);
            AnchorBand(enemyInfoLabel.rectTransform, 0f, 0.78f, UiScale.Px(20f));
            enemyInfoLabel.alignment = TextAlignmentOptions.TopLeft;

            // 소환수 칸. 장군의 방패병 자리다. 소환되기 전에는 꺼 둔다.
            int summonCount = rules.General != null ? rules.General.MaxShieldmen : 3;
            Image[] summonPanels = new Image[summonCount];
            Image[] summonBars = new Image[summonCount];
            TMP_Text[] summonNames = new TMP_Text[summonCount];
            TMP_Text[] summonInfos = new TMP_Text[summonCount];
            for (int i = 0; i < summonCount; i++)
            {
                summonPanels[i] = UiSceneBuilder.NewImage("Summon" + i, root, visual.PanelColor);
                summonPanels[i].raycastTarget = false;
                UiSceneBuilder.AddShadowPx(summonPanels[i], 6f, 4f);

                summonNames[i] = NewLabel("NameLabel", summonPanels[i].transform, visual.BodyFontSize, visual.TextColor, true);
                AnchorBand(summonNames[i].rectTransform, 0.8f, 1f, UiScale.Px(4f));

                Image back = UiSceneBuilder.NewFlatImage("BarBack", summonPanels[i].transform, visual.BarBackColor);
                back.raycastTarget = false;
                AnchorBand(back.rectTransform, 0.7f, 0.77f, UiScale.Px(8f));

                summonBars[i] = UiSceneBuilder.NewFlatImage("Bar", back.transform, visual.EnemyBarColor);
                summonBars[i].raycastTarget = false;
                summonBars[i].sprite = UiSceneBuilder.LoadSkin("패널");
                summonBars[i].type = Image.Type.Filled;
                summonBars[i].fillMethod = Image.FillMethod.Horizontal;
                UiSceneBuilder.Stretch(summonBars[i].rectTransform);

                summonInfos[i] = NewLabel("InfoLabel", summonPanels[i].transform, visual.BodyFontSize * 0.8f, visual.SubTextColor, false);
                AnchorBand(summonInfos[i].rectTransform, 0.04f, 0.66f, UiScale.Px(4f));
                summonInfos[i].alignment = TextAlignmentOptions.Top;

                summonPanels[i].gameObject.SetActive(false);
            }

            // ---- 원판 ----
            TMP_Text hint = NewLabel("HintLabel", root, visual.HintFontSize, visual.TextColor, true);

            Image dial = UiSceneBuilder.NewFlatImage("Dial", root, visual.DialColor);
            dial.sprite = circle;
            dial.raycastTarget = false;

            CanvasGroup coinGroup;
            CoinDialView coinDial = BuildCoinDial(dial.transform, layout, visual, circle, out coinGroup);

            CanvasGroup tagGroup;
            TagDialView tagDial = BuildTagDial(dial.transform, visual, circle, out tagGroup);

            // ---- 연출 ----
            Image projectile = UiSceneBuilder.NewFlatImage("Projectile", root, Color.white);
            projectile.sprite = circle;
            projectile.raycastTarget = false;

            TMP_Text enemyDamage = NewLabel("EnemyDamageLabel", root, visual.DamageFontSize, Color.white, true);
            TMP_Text playerDamage = NewLabel("PlayerDamageLabel", root, visual.DamageFontSize, visual.CostColor, true);

            TMP_Text result = NewLabel("ResultLabel", root, visual.DamageFontSize * 1.4f, visual.TextColor, true);
            UiSceneBuilder.Stretch(result.rectTransform);

            // ---- 붙이기 ----
            CombatScreenView view = root.gameObject.AddComponent<CombatScreenView>();
            UiSceneBuilder.SetField(view, "_layout", layout);
            UiSceneBuilder.SetField(view, "_visual", visual);
            UiSceneBuilder.SetField(view, "_tagVisuals", tags);
            UiSceneBuilder.SetArrayField(view, "_reels", reels);
            UiSceneBuilder.SetField(view, "_relicPanel", relicPanel);
            UiSceneBuilder.SetArrayField(view, "_relicSlots", relicSlots);
            UiSceneBuilder.SetArrayField(view, "_relicIcons", relicIcons);
            UiSceneBuilder.SetArrayField(view, "_relicLabels", relicLabels);
            UiSceneBuilder.SetArrayField(view, "_relicCounters", relicCounters);
            UiSceneBuilder.SetArrayField(view, "_relicHovers", relicHovers);
            UiSceneBuilder.SetField(view, "_relicPopup", relicPopup);
            UiSceneBuilder.SetField(view, "_relicPopupName", relicPopupName);
            UiSceneBuilder.SetField(view, "_relicPopupBody", relicPopupBody);
            UiSceneBuilder.SetField(view, "_relicPopupFlavor", relicPopupFlavor);
            UiSceneBuilder.SetField(view, "_playerHealthPanel", playerPanel);
            UiSceneBuilder.SetField(view, "_playerHealthBar", playerBar);
            UiSceneBuilder.SetField(view, "_playerHealthLabel", playerLabel);
            UiSceneBuilder.SetField(view, "_playerDamageLabel", playerDamage);
            UiSceneBuilder.SetField(view, "_enemyHealthPanel", enemyPanel);
            UiSceneBuilder.SetField(view, "_enemyHealthBar", enemyBar);
            UiSceneBuilder.SetField(view, "_enemyHealthLabel", enemyLabel);
            UiSceneBuilder.SetField(view, "_enemyArt", enemyArt);
            UiSceneBuilder.SetField(view, "_enemyArtLabel", enemyArtLabel);
            UiSceneBuilder.SetField(view, "_enemyInfo", enemyInfo);
            UiSceneBuilder.SetField(view, "_enemyNameLabel", enemyName);
            UiSceneBuilder.SetField(view, "_enemyInfoLabel", enemyInfoLabel);
            UiSceneBuilder.SetField(view, "_enemyDamageLabel", enemyDamage);
            UiSceneBuilder.SetArrayField(view, "_summonPanels", summonPanels);
            UiSceneBuilder.SetArrayField(view, "_summonBars", summonBars);
            UiSceneBuilder.SetArrayField(view, "_summonNames", summonNames);
            UiSceneBuilder.SetArrayField(view, "_summonInfos", summonInfos);
            UiSceneBuilder.SetField(view, "_dial", dial);
            UiSceneBuilder.SetField(view, "_coinGroup", coinGroup);
            UiSceneBuilder.SetField(view, "_coinDial", coinDial);
            UiSceneBuilder.SetField(view, "_tagGroup", tagGroup);
            UiSceneBuilder.SetField(view, "_tagDial", tagDial);
            UiSceneBuilder.SetField(view, "_hintLabel", hint);
            UiSceneBuilder.SetField(view, "_projectile", projectile);
            UiSceneBuilder.SetField(view, "_resultLabel", result);

            CombatScreenController controller = root.gameObject.AddComponent<CombatScreenController>();
            UiSceneBuilder.SetField(controller, "_view", view);
            UiSceneBuilder.SetField(controller, "_rules", rules);
            UiSceneBuilder.SetBoolField(controller, "_hideOnAwake", false);

            // 보기 자료로 한 번 열어 둔다. 플레이 중이 아니라 연출 없이 코인 선택까지 바로 간다.
            controller.Open(PreviewSamples.Combat(), new CatalogIconSource(catalog));

            AddDriver();
            SaveScene(scene, CombatSceneName);
        }

        /// <summary>릴 한 칸. 강조 테두리, 바탕, 문양 그림, 모서리 태그 둘.</summary>
        private static CombatReelView BuildReel(
            string name, Transform parent, CombatLayoutConfig layout, CombatVisualConfig visual, Sprite circle)
        {
            RectTransform rect = UiSceneBuilder.NewRect(name, parent);
            rect.sizeDelta = layout.ReelSize;

            // 테두리는 바탕보다 뒤에 두고 바깥으로 삐져나오게 한다.
            // 펼친 칸에 크기를 넣으면 구조 감사가 잡는다. 가운데에 고정 크기로 둔다.
            Image highlight = UiSceneBuilder.NewImage("Highlight", rect, Color.white);
            float spread = layout.ReelHighlightThickness * 2f;
            highlight.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            highlight.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            highlight.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            highlight.rectTransform.sizeDelta = layout.ReelSize + new Vector2(spread, spread);
            highlight.rectTransform.anchoredPosition = Vector2.zero;
            highlight.raycastTarget = false;
            highlight.gameObject.SetActive(false);

            Image background = UiSceneBuilder.NewImage("Background", rect, visual.PanelColor);
            UiSceneBuilder.Stretch(background.rectTransform);
            background.raycastTarget = false;

            Image icon = UiSceneBuilder.NewFlatImage("Icon", rect, Color.white);
            icon.raycastTarget = false;
            Center(icon.rectTransform, Vector2.zero, layout.ReelIconSize);

            TMP_Text nameLabel = NewLabel("NameLabel", rect, visual.BodyFontSize, visual.TextColor, true);
            UiSceneBuilder.Stretch(nameLabel.rectTransform);

            // 초안 08 의 태그 이미지 자리. 오른쪽 위와 왼쪽 아래 모서리다.
            float tag = layout.ReelTagSize;
            float edge = layout.ReelSize.x * 0.5f - tag * 0.5f - UiScale.Px(6f);

            Image firstBack;
            Image firstIcon;
            TMP_Text firstLabel;
            BuildTagBadge("FirstTag", rect, circle, tag, new Vector2(edge, edge), visual, out firstBack, out firstIcon, out firstLabel);

            Image secondBack;
            Image secondIcon;
            TMP_Text secondLabel;
            BuildTagBadge("SecondTag", rect, circle, tag, new Vector2(-edge, -edge), visual, out secondBack, out secondIcon, out secondLabel);

            CombatReelView view = rect.gameObject.AddComponent<CombatReelView>();
            UiSceneBuilder.SetField(view, "_highlight", highlight);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_icon", icon);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_firstTagBack", firstBack);
            UiSceneBuilder.SetField(view, "_firstTagIcon", firstIcon);
            UiSceneBuilder.SetField(view, "_firstTagLabel", firstLabel);
            UiSceneBuilder.SetField(view, "_secondTagBack", secondBack);
            UiSceneBuilder.SetField(view, "_secondTagIcon", secondIcon);
            UiSceneBuilder.SetField(view, "_secondTagLabel", secondLabel);
            return view;
        }

        private static void BuildTagBadge(
            string name, Transform parent, Sprite circle, float size, Vector2 position, CombatVisualConfig visual,
            out Image back, out Image icon, out TMP_Text label)
        {
            back = UiSceneBuilder.NewFlatImage(name, parent, Color.white);
            back.sprite = circle;
            back.raycastTarget = false;
            Center(back.rectTransform, position, size);

            icon = NewIcon("Icon", back.transform, UiScale.Px(6f));

            label = NewLabel("Label", back.transform, size * 0.6f, visual.DarkTextColor, true);
            UiSceneBuilder.Stretch(label.rectTransform);
        }

        /// <summary>체력 칸. 위에 글, 아래에 막대.</summary>
        private static void BuildHealthPanel(
            string name, Transform parent, CombatVisualConfig visual, Color barColor,
            out Image panel, out Image bar, out TMP_Text label)
        {
            panel = UiSceneBuilder.NewImage(name, parent, visual.PanelColor);
            panel.raycastTarget = false;
            UiSceneBuilder.AddShadowPx(panel, 8f, 5f);

            label = NewLabel("Label", panel.transform, visual.HealthFontSize, visual.TextColor, true);
            AnchorBand(label.rectTransform, 0.35f, 1f, UiScale.Px(10f));

            Image back = UiSceneBuilder.NewFlatImage("BarBack", panel.transform, visual.BarBackColor);
            back.raycastTarget = false;
            AnchorBand(back.rectTransform, 0.12f, 0.32f, UiScale.Px(14f));

            bar = UiSceneBuilder.NewFlatImage("Bar", back.transform, barColor);
            bar.raycastTarget = false;
            // 그림이 없으면 유니티가 채우기를 무시하고 통째로 그린다. 막대가 줄지 않는다.
            bar.sprite = UiSceneBuilder.LoadSkin("패널");
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            UiSceneBuilder.Stretch(bar.rectTransform);
        }

        /// <summary>코인 UI. 코인 셋, 가운데 "코인 없이", 정보 팝업.</summary>
        private static CoinDialView BuildCoinDial(
            Transform dial, CombatLayoutConfig layout, CombatVisualConfig visual, Sprite circle, out CanvasGroup group)
        {
            RectTransform root = UiSceneBuilder.NewRect("CoinDial", dial);
            UiSceneBuilder.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            int count = 3;
            Button[] buttons = new Button[count];
            Image[] backs = new Image[count];
            Image[] icons = new Image[count];
            TMP_Text[] labels = new TMP_Text[count];
            PointerRelay[] hovers = new PointerRelay[count];

            for (int i = 0; i < count; i++)
            {
                RectTransform coin = UiSceneBuilder.NewRect("Coin" + i, root);

                backs[i] = UiSceneBuilder.NewFlatImage("Back", coin, visual.CoinColor);
                backs[i].sprite = circle;
                UiSceneBuilder.Stretch(backs[i].rectTransform);

                icons[i] = NewIcon("Icon", coin, 0f);

                labels[i] = NewLabel("Label", coin, visual.CoinLabelFontSize, visual.DarkTextColor, true);
                UiSceneBuilder.Stretch(labels[i].rectTransform);

                buttons[i] = UiSceneBuilder.AddButton(coin.gameObject, backs[i]);
                hovers[i] = coin.gameObject.AddComponent<PointerRelay>();
            }

            RectTransform center = UiSceneBuilder.NewRect("NoCoin", root);
            Image centerBack = UiSceneBuilder.NewFlatImage("Back", center, visual.EmptyColor);
            centerBack.sprite = circle;
            UiSceneBuilder.Stretch(centerBack.rectTransform);
            TMP_Text centerLabel = NewLabel("Label", center, visual.CoinLabelFontSize, visual.TextColor, true);
            UiSceneBuilder.Stretch(centerLabel.rectTransform);
            Button centerButton = UiSceneBuilder.AddButton(center.gameObject, centerBack);
            PointerRelay centerHover = center.gameObject.AddComponent<PointerRelay>();

            // 정보 팝업. 초안 03 의 200x300.
            Image popup = UiSceneBuilder.NewImage("Popup", root, visual.PopupColor);
            popup.raycastTarget = false;
            Image popupIcon = UiSceneBuilder.NewFlatImage("Icon", popup.transform, Color.white);
            popupIcon.raycastTarget = false;
            PlaceInPopup(popupIcon.rectTransform, layout, 0f, UiScale.Px(14f), UiScale.Px(90f), UiScale.Px(90f), true);

            TMP_Text popupName = NewLabel("NameLabel", popup.transform, visual.CoinLabelFontSize * 1.2f, visual.DarkTextColor, true);
            PlaceInPopup(popupName.rectTransform, layout, 0f, UiScale.Px(110f), layout.CoinPopupSize.x, UiScale.Px(36f), false);

            TMP_Text popupStats = NewLabel("StatsLabel", popup.transform, visual.CoinLabelFontSize, visual.DarkTextColor, false);
            PlaceInPopup(popupStats.rectTransform, layout, 0f, UiScale.Px(150f), layout.CoinPopupSize.x, UiScale.Px(60f), false);

            TMP_Text popupDescription = NewLabel("DescriptionLabel", popup.transform, visual.CoinLabelFontSize * 0.85f, visual.DarkTextColor, false);
            PlaceInPopup(popupDescription.rectTransform, layout, UiScale.Px(12f), UiScale.Px(215f),
                layout.CoinPopupSize.x - UiScale.Px(24f), UiScale.Px(80f), false);
            popupDescription.alignment = TextAlignmentOptions.Top;

            CoinDialView view = root.gameObject.AddComponent<CoinDialView>();
            UiSceneBuilder.SetArrayField(view, "_coinButtons", buttons);
            UiSceneBuilder.SetArrayField(view, "_coinBacks", backs);
            UiSceneBuilder.SetArrayField(view, "_coinIcons", icons);
            UiSceneBuilder.SetArrayField(view, "_coinLabels", labels);
            UiSceneBuilder.SetArrayField(view, "_coinHovers", hovers);
            UiSceneBuilder.SetField(view, "_centerButton", centerButton);
            UiSceneBuilder.SetField(view, "_centerBack", centerBack);
            UiSceneBuilder.SetField(view, "_centerLabel", centerLabel);
            UiSceneBuilder.SetField(view, "_centerHover", centerHover);
            UiSceneBuilder.SetField(view, "_popup", popup);
            UiSceneBuilder.SetField(view, "_popupIcon", popupIcon);
            UiSceneBuilder.SetField(view, "_popupName", popupName);
            UiSceneBuilder.SetField(view, "_popupStats", popupStats);
            UiSceneBuilder.SetField(view, "_popupDescription", popupDescription);
            return view;
        }

        /// <summary>태그 UI. 바깥 열 칸, 가운데 코인, 공격 예정 원.</summary>
        private static TagDialView BuildTagDial(
            Transform dial, CombatVisualConfig visual, Sprite circle, out CanvasGroup group)
        {
            RectTransform root = UiSceneBuilder.NewRect("TagDial", dial);
            UiSceneBuilder.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            RectTransform sectorLayer = UiSceneBuilder.NewRect("Sectors", root);
            UiSceneBuilder.Stretch(sectorLayer);

            TagSectorView[] sectors = new TagSectorView[TagDialView.SectorCount];
            for (int i = 0; i < sectors.Length; i++)
            {
                RectTransform sector = UiSceneBuilder.NewRect("Sector" + i, sectorLayer);
                UiSceneBuilder.Stretch(sector);

                Image fill = UiSceneBuilder.NewFlatImage("Fill", sector, Color.white);
                fill.sprite = circle;
                UiSceneBuilder.Stretch(fill.rectTransform);

                Image icon = UiSceneBuilder.NewFlatImage("Icon", sector, Color.white);
                icon.raycastTarget = false;

                TMP_Text label = NewLabel("Label", sector, visual.TagSymbolFontSize, visual.DarkTextColor, true);

                TagSectorView view = sector.gameObject.AddComponent<TagSectorView>();
                UiSceneBuilder.SetField(view, "_fill", fill);
                UiSceneBuilder.SetField(view, "_icon", icon);
                UiSceneBuilder.SetField(view, "_label", label);
                sectors[i] = view;
            }

            RectTransform center = UiSceneBuilder.NewRect("Center", root);
            Image centerBack = UiSceneBuilder.NewFlatImage("Back", center, visual.DialCenterColor);
            centerBack.sprite = circle;
            UiSceneBuilder.Stretch(centerBack.rectTransform);
            Image centerCoin = NewIcon("Coin", center, UiScale.Px(30f));
            TMP_Text centerLabel = NewLabel("Label", center, visual.CoinLabelFontSize, visual.DarkTextColor, true);
            UiSceneBuilder.Stretch(centerLabel.rectTransform);
            Button centerButton = UiSceneBuilder.AddButton(center.gameObject, centerBack);

            Image charge = UiSceneBuilder.NewFlatImage("Charge", root, Color.white);
            charge.sprite = circle;
            charge.raycastTarget = false;
            UiSceneBuilder.Stretch(charge.rectTransform);
            charge.gameObject.SetActive(false);

            TagDialView dialView = root.gameObject.AddComponent<TagDialView>();
            UiSceneBuilder.SetArrayField(dialView, "_sectors", sectors);
            UiSceneBuilder.SetField(dialView, "_centerButton", centerButton);
            UiSceneBuilder.SetField(dialView, "_centerBack", centerBack);
            UiSceneBuilder.SetField(dialView, "_centerCoin", centerCoin);
            UiSceneBuilder.SetField(dialView, "_centerLabel", centerLabel);
            UiSceneBuilder.SetField(dialView, "_charge", charge);
            return dialView;
        }

        /// <summary>부모를 안쪽 여백만 남기고 채우는 그림. 누르기를 받지 않는다.</summary>
        private static Image NewIcon(string name, Transform parent, float inset)
        {
            Image icon = UiSceneBuilder.NewFlatImage(name, parent, Color.white);
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            UiSceneBuilder.Stretch(icon.rectTransform);
            icon.rectTransform.offsetMin = new Vector2(inset, inset);
            icon.rectTransform.offsetMax = new Vector2(-inset, -inset);
            icon.enabled = false;
            return icon;
        }

        /// <summary>가운데 맞춤 글. 누르기를 받지 않는다.</summary>
        private static TMP_Text NewLabel(string name, Transform parent, float size, Color color, bool bold)
        {
            TMP_Text label = UiSceneBuilder.NewText(name, parent, string.Empty, size, color, TextAlignmentOptions.Center, bold);
            label.raycastTarget = false;
            return label;
        }

        /// <summary>부모의 세로 구간 [bottom, top] 을 가로로 꽉 채운다. 좌우 여백을 둔다.</summary>
        private static void AnchorBand(RectTransform rect, float bottom, float top, float padding)
        {
            rect.anchorMin = new Vector2(0f, bottom);
            rect.anchorMax = new Vector2(1f, top);
            rect.offsetMin = new Vector2(padding, 0f);
            rect.offsetMax = new Vector2(-padding, 0f);
        }

        private static void Center(RectTransform rect, Vector2 position, float size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = position;
        }

        /// <summary>팝업 안에서 왼쪽 위를 기준으로 자리를 잡는다. 가운데 맞춤이면 x 는 무시하고 가로 가운데에 둔다.</summary>
        private static void PlaceInPopup(
            RectTransform rect, CombatLayoutConfig layout, float x, float y, float width, float height, bool centered)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            float left = centered ? (layout.CoinPopupSize.x - width) * 0.5f : x;
            rect.anchoredPosition = new Vector2(left, -y);
        }

        /// <summary>물건 목록에서 그림을 꺼내 전투 화면에 내준다. 확인용 씬을 지을 때 쓴다.</summary>
        private sealed class CatalogIconSource : ICombatIconSource
        {
            private readonly RunCatalogConfig _catalog;

            public CatalogIconSource(RunCatalogConfig catalog)
            {
                _catalog = catalog;
            }

            public Sprite GetIcon(string id)
            {
                return _catalog != null ? _catalog.GetIcon(id) : null;
            }
        }
    }
}
