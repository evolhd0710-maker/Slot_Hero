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
    ///
    /// 그림은 피그마 `Jackpot 아트웍 (복사)` 의 전투 화면 디자인이다(2026년 10월 26일 원재).
    /// 설정 에셋은 검사 때마다 기본값으로 되돌아가므로 그림은 설정이 아니라 여기서 그림 칸에 바로 물린다.
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
                // 피그마 릴 틀은 여섯 색이다. 왼쪽부터 청록, 빨강, 금, 회색, 녹청, 보라.
                Sprite frame = ImportArt.LoadCombat("릴틀_" + (i % ReelFrameCount));
                reels[i] = BuildReel("Reel" + i, root, layout, visual, frame);
            }

            // ---- 유물 ----
            // 피그마는 목록 바탕 없이 칸 틀만 세로로 늘어놓았다.
            Image relicPanel = UiSceneBuilder.NewFlatImage("RelicPanel", root, visual.RelicPanelColor);
            relicPanel.raycastTarget = false;

            const int relicSlotCount = 6;
            Image[] relicSlots = new Image[relicSlotCount];
            Image[] relicIcons = new Image[relicSlotCount];
            TMP_Text[] relicLabels = new TMP_Text[relicSlotCount];
            TMP_Text[] relicCounters = new TMP_Text[relicSlotCount];
            PointerRelay[] relicHovers = new PointerRelay[relicSlotCount];
            for (int i = 0; i < relicSlotCount; i++)
            {
                // 칸이 마우스를 받아 유물 정보 팝업을 띄운다. 칸 그림은 피그마 유물 칸 틀의 색을 뺀 흰 틀이고
                // 색은 뷰가 유물 등급에 맞춰 곱한다.
                relicSlots[i] = UiSceneBuilder.NewFlatImage("RelicSlot" + i, root, visual.RelicEmptyFrameColor);
                relicSlots[i].sprite = ImportArt.LoadCombat("유물칸_흰");
                relicIcons[i] = NewIcon("Icon", relicSlots[i].transform, layout.RelicIconInset);

                // 그림이 없을 때 이름을 적는다.
                relicLabels[i] = NewLabel("NameLabel", relicSlots[i].transform, visual.BodyFontSize * 0.85f, visual.TextColor, true);
                UiSceneBuilder.Stretch(relicLabels[i].rectTransform);
                relicLabels[i].rectTransform.offsetMin = new Vector2(UiScale.Px(10f), UiScale.Px(10f));
                relicLabels[i].rectTransform.offsetMax = new Vector2(-UiScale.Px(10f), -UiScale.Px(10f));

                // 고유 수치(12개의 금화). 오른쪽 아래에 붙인다.
                relicCounters[i] = NewLabel("CounterLabel", relicSlots[i].transform, visual.BodyFontSize, visual.CostColor, true);
                RectTransform counter = relicCounters[i].rectTransform;
                counter.anchorMin = new Vector2(1f, 0f);
                counter.anchorMax = new Vector2(1f, 0f);
                counter.pivot = new Vector2(1f, 0f);
                counter.sizeDelta = layout.RelicCounterSize;
                // 칸 틀 안쪽에 들어오게 틀 두께만큼 들인다.
                counter.anchoredPosition = new Vector2(-layout.RelicIconInset * 0.5f, layout.RelicIconInset * 0.5f);
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
            // 피그마 주인공 깃발과 초상. 두 그림을 한 장으로 합쳐 두었다.
            Image playerPortrait = UiSceneBuilder.NewFlatImage("PlayerPortrait", root, Color.white);
            UseArt(playerPortrait, ImportArt.LoadCombat("주인공_깃발초상"));
            playerPortrait.preserveAspect = true;
            playerPortrait.raycastTarget = false;

            Image playerPanel;
            Image playerBar;
            TMP_Text playerLabel;
            BuildHealthPanel("PlayerHealth", root, visual, visual.PlayerBarColor, ImportArt.LoadCombat("주인공_체력틀"),
                layout.PlayerFrameArea, layout.PlayerBarArea, layout.PlayerHeartArea, layout.PlayerNumberArea,
                out playerPanel, out playerBar, out playerLabel);

            // ---- 몬스터 ----
            Image enemyPanel;
            Image enemyBar;
            TMP_Text enemyLabel;
            BuildHealthPanel("EnemyHealth", root, visual, visual.EnemyBarColor, ImportArt.LoadCombat("적_체력_틀"),
                new Rect(0f, 0f, 1f, 1f), layout.EnemyBarArea, layout.EnemyHeartArea, layout.EnemyNumberArea,
                out enemyPanel, out enemyBar, out enemyLabel);

            // 몬스터 그림이 아직 없어 이름을 적은 칸으로 둔다.
            // 태그를 건 뒤 누르면 본체를 때린다.
            Image enemyArt = UiSceneBuilder.NewImage("EnemyArt", root, visual.EnemyArtStandIn);
            Button enemyArtButton = UiSceneBuilder.AddButton(enemyArt.gameObject, enemyArt);
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
            Button[] summonButtons = new Button[summonCount];
            for (int i = 0; i < summonCount; i++)
            {
                // 태그를 건 뒤 누르면 그 소환수를 때린다.
                summonPanels[i] = UiSceneBuilder.NewImage("Summon" + i, root, visual.PanelColor);
                summonButtons[i] = UiSceneBuilder.AddButton(summonPanels[i].gameObject, summonPanels[i]);
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

            // 두 UI 가 저마다 피그마 바탕 그림을 깔아 공용 원판 칸은 자리만 잡는다.
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

            DamageNumberView enemyDamage = BuildDamageNumber("EnemyDamageNumber", root, visual);

            // 숫자 그림으로 적을 수 없을 때(그림이 빠졌거나 자리 수를 넘을 때) 쓰는 글자 피해 숫자.
            TMP_Text enemyDamageText = NewLabel("EnemyDamageLabel", root, visual.DamageFontSize, Color.white, true);
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
            UiSceneBuilder.SetField(view, "_playerPortrait", playerPortrait);
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
            UiSceneBuilder.SetField(view, "_enemyDamageNumber", enemyDamage);
            UiSceneBuilder.SetField(view, "_enemyDamageLabel", enemyDamageText);
            UiSceneBuilder.SetArrayField(view, "_summonPanels", summonPanels);
            UiSceneBuilder.SetArrayField(view, "_summonBars", summonBars);
            UiSceneBuilder.SetArrayField(view, "_summonNames", summonNames);
            UiSceneBuilder.SetArrayField(view, "_summonInfos", summonInfos);
            UiSceneBuilder.SetField(view, "_enemyArtButton", enemyArtButton);
            UiSceneBuilder.SetArrayField(view, "_summonButtons", summonButtons);
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
            OpenCombatSample(controller);

            AddDriver();
            SaveScene(scene, CombatSceneName);
        }

        /// 피그마 릴 틀 그림 수. 릴이 더 많으면 처음 색부터 다시 쓴다.
        private const int ReelFrameCount = 6;

        /// 피해 숫자 자리 수. 백만 단위까지 적는다.
        private const int DamageDigitCount = 7;

        /// <summary>릴 한 칸. 강조 테두리, 바탕, 흑요석 판, 문양 그림, 태그 테두리, 피그마 릴 틀.</summary>
        private static CombatReelView BuildReel(
            string name, Transform parent, CombatLayoutConfig layout, CombatVisualConfig visual, Sprite frame)
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

            // 바탕은 틀 안쪽만 검게 메운다. 틀 바깥 깨진 가장자리로 비치지 않게 들인다.
            Image background = UiSceneBuilder.NewFlatImage("Background", rect, visual.ReelBackColor);
            UiSceneBuilder.Stretch(background.rectTransform);
            background.rectTransform.offsetMin = new Vector2(layout.ReelBackInset, layout.ReelBackInset);
            background.rectTransform.offsetMax = new Vector2(-layout.ReelBackInset, -layout.ReelBackInset);
            background.raycastTarget = false;

            // 릴 틀 안쪽 자리. 다른 화면의 문양 칸처럼 흑요석 판, 문양 그림, 태그 테두리를 차례로 쌓는다(2026년 10월 26일 원재).
            // 태그는 테두리 장식과 장식 모서리의 행성 기호가 보인다. 초안 08 의 모서리 태그 원은 이것으로 바꿨다.
            RectTransform content = UiSceneBuilder.NewRect("Content", rect);
            UiSceneBuilder.Stretch(content);
            content.offsetMin = new Vector2(layout.ReelContentInset, layout.ReelContentInset);
            content.offsetMax = new Vector2(-layout.ReelContentInset, -layout.ReelContentInset);

            Image icon = UiSceneBuilder.NewFlatImage("Icon", content, Color.white);
            icon.raycastTarget = false;
            Center(icon.rectTransform, Vector2.zero, layout.ReelIconSize);

            TagCorners corners = UiSceneBuilder.NewTagCorners(content, icon.rectTransform);
            Image plate = UiSceneBuilder.NewSymbolPlate(content, corners);
            plate.rectTransform.SetSiblingIndex(0);

            TMP_Text nameLabel = NewLabel("NameLabel", content, visual.BodyFontSize, visual.TextColor, true);
            UiSceneBuilder.Stretch(nameLabel.rectTransform);

            Image frameImage = UiSceneBuilder.NewFlatImage("Frame", rect, Color.white);
            UseArt(frameImage, frame);
            frameImage.raycastTarget = false;
            UiSceneBuilder.Stretch(frameImage.rectTransform);

            CombatReelView view = rect.gameObject.AddComponent<CombatReelView>();
            UiSceneBuilder.SetField(view, "_highlight", highlight);
            UiSceneBuilder.SetField(view, "_background", background);
            UiSceneBuilder.SetField(view, "_icon", icon);
            UiSceneBuilder.SetField(view, "_nameLabel", nameLabel);
            UiSceneBuilder.SetField(view, "_tagCorners", corners);
            return view;
        }

        /// <summary>
        /// 체력 칸. 피그마 체력 틀 그림 안에 막대를 넣고, 하트와 "지금/최대" 숫자를 얹는다.
        /// 칸 자체는 투명한 묶음이고 안의 자리는 칸 왼쪽 위 기준 비율(area)로 잡는다.
        /// </summary>
        private static void BuildHealthPanel(
            string name, Transform parent, CombatVisualConfig visual, Color barColor, Sprite frameSprite,
            Rect frameArea, Rect barArea, Rect heartArea, Rect numberArea,
            out Image panel, out Image bar, out TMP_Text label)
        {
            panel = UiSceneBuilder.NewFlatImage(name, parent, Color.clear);
            panel.raycastTarget = false;

            Image back = UiSceneBuilder.NewFlatImage("BarBack", panel.transform, visual.BarBackColor);
            back.raycastTarget = false;
            PlaceArea(back.rectTransform, barArea);

            bar = UiSceneBuilder.NewFlatImage("Bar", back.transform, barColor);
            bar.raycastTarget = false;
            // 그림이 없으면 유니티가 채우기를 무시하고 통째로 그린다. 막대가 줄지 않는다.
            bar.sprite = ImportArt.LoadCombat("흰칸");
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            UiSceneBuilder.Stretch(bar.rectTransform);

            // 틀은 막대 위에 덮어 막대 끝이 틀 안쪽 선 아래로 들어가게 한다.
            Image frame = UiSceneBuilder.NewFlatImage("Frame", panel.transform, Color.white);
            UseArt(frame, frameSprite);
            frame.raycastTarget = false;
            PlaceArea(frame.rectTransform, frameArea);

            Image heart = UiSceneBuilder.NewFlatImage("Heart", panel.transform, Color.white);
            UseArt(heart, ImportArt.LoadCombat("하트"));
            heart.preserveAspect = true;
            heart.raycastTarget = false;
            PlaceArea(heart.rectTransform, heartArea);

            label = NewLabel("Label", panel.transform, visual.HealthFontSize, visual.TextColor, false);
            label.alignment = TextAlignmentOptions.Left;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            PlaceArea(label.rectTransform, numberArea);
        }

        /// <summary>부모 안의 자리를 왼쪽 위 기준 비율로 잡는다. 크기는 부모를 따라간다.</summary>
        private static void PlaceArea(RectTransform rect, Rect area)
        {
            rect.anchorMin = new Vector2(area.xMin, 1f - area.yMax);
            rect.anchorMax = new Vector2(area.xMax, 1f - area.yMin);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 코인 UI. 피그마 원판 바탕, 코인 셋과 고름 강조, 가운데 "코인 없이", 설명 풍선 정보 팝업.
        /// </summary>
        private static CoinDialView BuildCoinDial(
            Transform dial, CombatLayoutConfig layout, CombatVisualConfig visual, Sprite circle, out CanvasGroup group)
        {
            RectTransform root = UiSceneBuilder.NewRect("CoinDial", dial);
            UiSceneBuilder.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            // 원판 테두리와 별자리 바탕, 빈 코인 자리 셋이 그려진 한 장이다.
            Image dialBase = UiSceneBuilder.NewFlatImage("Base", root, Color.white);
            UseArt(dialBase, ImportArt.LoadCombat("원판_코인바탕"));
            dialBase.raycastTarget = false;
            UiSceneBuilder.Stretch(dialBase.rectTransform);

            Sprite glowSprite = ImportArt.LoadCombat("코인_고름_강조");

            int count = 3;
            Button[] buttons = new Button[count];
            Image[] backs = new Image[count];
            Image[] icons = new Image[count];
            TMP_Text[] labels = new TMP_Text[count];
            PointerRelay[] hovers = new PointerRelay[count];
            Image[] glows = new Image[count];

            for (int i = 0; i < count; i++)
            {
                RectTransform coin = UiSceneBuilder.NewRect("Coin" + i, root);

                backs[i] = UiSceneBuilder.NewFlatImage("Back", coin, visual.CoinColor);
                backs[i].sprite = circle;
                UiSceneBuilder.Stretch(backs[i].rectTransform);

                icons[i] = NewIcon("Icon", coin, 0f);

                labels[i] = NewLabel("Label", coin, visual.CoinLabelFontSize, visual.DarkTextColor, true);
                UiSceneBuilder.Stretch(labels[i].rectTransform);

                // 고름 강조는 코인 가장자리에 겹쳐 빛나게 코인 그림 위에 둔다.
                glows[i] = UiSceneBuilder.NewFlatImage("Glow", coin, Color.white);
                UseArt(glows[i], glowSprite);
                glows[i].raycastTarget = false;
                Center(glows[i].rectTransform, Vector2.zero, layout.CoinSize * layout.CoinGlowScale);
                glows[i].gameObject.SetActive(false);

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

            // 정보 팝업. 피그마 Frame 2 의 설명 풍선(314 × 183)이다. 왼쪽에 코인 그림, 오른쪽에 이름과 수치와 설명.
            // 자리는 풍선 그림 왼쪽 위에서 잰 피그마 값이고 자리 설정의 `CoinPopup...Area` 에 있다.
            Image popup = UiSceneBuilder.NewFlatImage("Popup", root, Color.white);
            UseArt(popup, ImportArt.LoadCombat("설명_풍선"));
            popup.raycastTarget = false;
            Image popupIcon = UiSceneBuilder.NewFlatImage("Icon", popup.transform, Color.white);
            popupIcon.raycastTarget = false;
            PlaceInPopup(popupIcon.rectTransform, layout, layout.CoinPopupIconArea);

            Color popupText = visual.CoinPopupTextColor;

            TMP_Text popupName = NewLabel("NameLabel", popup.transform, visual.CoinPopupNameFontSize, popupText, true);
            PlaceInPopup(popupName.rectTransform, layout, layout.CoinPopupNameArea);
            popupName.alignment = TextAlignmentOptions.Left;

            Image popupLine = UiSceneBuilder.NewFlatImage("Line", popup.transform, visual.CoinPopupLineColor);
            popupLine.raycastTarget = false;
            PlaceInPopup(popupLine.rectTransform, layout, layout.CoinPopupLineArea);

            TMP_Text popupStats = NewLabel("StatsLabel", popup.transform, visual.CoinPopupBodyFontSize, popupText, false);
            PlaceInPopup(popupStats.rectTransform, layout, layout.CoinPopupStatsArea);
            popupStats.alignment = TextAlignmentOptions.TopLeft;

            // 체력이 모자라면 설명 앞에 한 줄이 더 붙는다. 칸을 넘지 않게 줄여 넣는다.
            TMP_Text popupDescription = NewLabel("DescriptionLabel", popup.transform, visual.CoinPopupBodyFontSize, popupText, false);
            PlaceInPopup(popupDescription.rectTransform, layout, layout.CoinPopupBodyArea);
            popupDescription.alignment = TextAlignmentOptions.TopLeft;
            popupDescription.enableAutoSizing = true;
            popupDescription.fontSizeMax = visual.CoinPopupBodyFontSize;
            popupDescription.fontSizeMin = visual.CoinPopupBodyFontSize * 0.7f;

            CoinDialView view = root.gameObject.AddComponent<CoinDialView>();
            UiSceneBuilder.SetArrayField(view, "_coinButtons", buttons);
            UiSceneBuilder.SetArrayField(view, "_coinBacks", backs);
            UiSceneBuilder.SetArrayField(view, "_coinIcons", icons);
            UiSceneBuilder.SetArrayField(view, "_coinLabels", labels);
            UiSceneBuilder.SetArrayField(view, "_coinHovers", hovers);
            UiSceneBuilder.SetArrayField(view, "_coinGlows", glows);
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

        /// <summary>
        /// 태그 UI. 피그마 Frame 3 · 4 를 따라 어두운 원, 원판 테두리, 열 칸 바퀴, 칸 열 개, 가운데 메달과 코인을 차례로 쌓는다.
        /// 크기는 `TagDialView.Configure` 가 자리 설정에서 잡는다.
        /// </summary>
        private static TagDialView BuildTagDial(
            Transform dial, CombatVisualConfig visual, Sprite circle, out CanvasGroup group)
        {
            RectTransform root = UiSceneBuilder.NewRect("TagDial", dial);
            UiSceneBuilder.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            Image wheelBack = UiSceneBuilder.NewFlatImage("WheelBack", root, visual.TagWheelBackColor);
            wheelBack.sprite = circle;
            wheelBack.raycastTarget = false;

            Image ring = UiSceneBuilder.NewFlatImage("Ring", root, Color.white);
            UseArt(ring, ImportArt.LoadCombat("원판_테두리"));
            ring.raycastTarget = false;
            UiSceneBuilder.Stretch(ring.rectTransform);

            Image wheel = UiSceneBuilder.NewFlatImage("Wheel", root, Color.white);
            UseArt(wheel, ImportArt.LoadCombat("원판_열칸"));
            wheel.raycastTarget = false;

            RectTransform sectorLayer = UiSceneBuilder.NewRect("Sectors", root);
            Sprite outlineSprite = ImportArt.LoadCombat("원판_칸_강조_열칸");

            TagSectorView[] sectors = new TagSectorView[TagDialView.SectorCount];
            for (int i = 0; i < sectors.Length; i++)
            {
                RectTransform sector = UiSceneBuilder.NewRect("Sector" + i, sectorLayer);
                UiSceneBuilder.Stretch(sector);

                // 칸 그림은 누르기를 받는 자리이자 공격 예정 때 태그 색으로 차는 자리다. 평소에는 투명하다.
                Image fill = UiSceneBuilder.NewFlatImage("Fill", sector, Color.clear);
                fill.sprite = circle;
                UiSceneBuilder.Stretch(fill.rectTransform);

                Image outline = UiSceneBuilder.NewFlatImage("Outline", sector, Color.white);
                UseArt(outline, outlineSprite);
                outline.raycastTarget = false;
                outline.gameObject.SetActive(false);

                Image icon = UiSceneBuilder.NewFlatImage("Icon", sector, Color.white);
                icon.raycastTarget = false;

                TMP_Text label = NewLabel("Label", sector, visual.TagSymbolFontSize, visual.TextColor, true);

                TagSectorView view = sector.gameObject.AddComponent<TagSectorView>();
                UiSceneBuilder.SetField(view, "_fill", fill);
                UiSceneBuilder.SetField(view, "_icon", icon);
                UiSceneBuilder.SetField(view, "_label", label);
                UiSceneBuilder.SetField(view, "_outline", outline);
                sectors[i] = view;
            }

            // 누르는 칸은 투명한 원이고 그 위에 메달과 고른 코인을 얹는다.
            RectTransform center = UiSceneBuilder.NewRect("Center", root);
            Image centerBack = UiSceneBuilder.NewFlatImage("Back", center, visual.DialCenterColor);
            centerBack.sprite = circle;
            UiSceneBuilder.Stretch(centerBack.rectTransform);

            Image medal = UiSceneBuilder.NewFlatImage("Medal", center, Color.white);
            UseArt(medal, ImportArt.LoadCombat("원판_가운데_메달"));
            medal.raycastTarget = false;

            Image centerCoin = UiSceneBuilder.NewFlatImage("Coin", center, Color.white);
            centerCoin.raycastTarget = false;
            centerCoin.preserveAspect = true;
            centerCoin.enabled = false;

            TMP_Text centerLabel = NewLabel("Label", center, visual.CoinLabelFontSize, visual.DarkTextColor, true);
            UiSceneBuilder.Stretch(centerLabel.rectTransform);
            Button centerButton = UiSceneBuilder.AddButton(center.gameObject, centerBack);

            TagDialView dialView = root.gameObject.AddComponent<TagDialView>();
            UiSceneBuilder.SetArrayField(dialView, "_sectors", sectors);
            UiSceneBuilder.SetField(dialView, "_sectorLayer", sectorLayer);
            UiSceneBuilder.SetField(dialView, "_wheelBack", wheelBack);
            UiSceneBuilder.SetField(dialView, "_wheel", wheel);
            UiSceneBuilder.SetField(dialView, "_centerButton", centerButton);
            UiSceneBuilder.SetField(dialView, "_centerBack", centerBack);
            UiSceneBuilder.SetField(dialView, "_medal", medal);
            UiSceneBuilder.SetField(dialView, "_centerCoin", centerCoin);
            UiSceneBuilder.SetField(dialView, "_centerLabel", centerLabel);
            return dialView;
        }

        /// <summary>피해 숫자. 번개 그림 위에 0 ~ 9 숫자 그림 자리를 늘어놓고 막은 몫을 적을 글을 둔다.</summary>
        private static DamageNumberView BuildDamageNumber(string name, Transform parent, CombatVisualConfig visual)
        {
            RectTransform root = UiSceneBuilder.NewRect(name, parent);
            CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            Image spark = UiSceneBuilder.NewFlatImage("Spark", root, Color.white);
            UseArt(spark, ImportArt.LoadCombat("번개"));
            spark.raycastTarget = false;

            Image[] digits = new Image[DamageDigitCount];
            for (int i = 0; i < digits.Length; i++)
            {
                digits[i] = UiSceneBuilder.NewFlatImage("Digit" + i, root, Color.white);
                digits[i].raycastTarget = false;
                digits[i].preserveAspect = true;
            }

            Sprite[] digitSprites = new Sprite[10];
            for (int i = 0; i < digitSprites.Length; i++)
            {
                digitSprites[i] = ImportArt.LoadCombat("숫자_" + i);
            }

            TMP_Text note = NewLabel("NoteLabel", root, visual.DamageNoteFontSize, visual.TextColor, true);

            DamageNumberView view = root.gameObject.AddComponent<DamageNumberView>();
            UiSceneBuilder.SetField(view, "_group", group);
            UiSceneBuilder.SetField(view, "_spark", spark);
            UiSceneBuilder.SetArrayField(view, "_digits", digits);
            UiSceneBuilder.SetArrayField(view, "_digitSprites", digitSprites);
            UiSceneBuilder.SetField(view, "_note", note);
            root.gameObject.SetActive(false);
            return view;
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
        /// <summary>팝업 안 자리(왼쪽 위 x, y, 너비, 높이)에 놓는다.</summary>
        private static void PlaceInPopup(RectTransform rect, CombatLayoutConfig layout, Rect area)
        {
            PlaceInPopup(rect, layout, area.x, area.y, area.width, area.height, false);
        }

        /// <summary>
        /// 꾸밈 그림을 물린다. 그림을 못 찾으면 칸을 끈다. 그림 없는 Image 는 흰 사각형으로 그려져 문양이나 글을 가린다(2026년 10월 26일 코덱스 점검).
        /// </summary>
        private static void UseArt(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
            if (sprite == null)
            {
                Debug.LogWarning("전투 화면 그림이 없어 칸을 끈다: " + image.name + ". `그림 가져오기` 를 먼저 돌린다.");
            }
        }

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

        /// <summary>
        /// 전투 화면을 보기 자료로 연다. 전투 진행은 씬에 저장되지 않아
        /// 저장된 씬을 다시 열어 그림을 뽑을 때도 이걸 먼저 부른다.
        /// </summary>
        public static void OpenCombatSample(CombatScreenController controller)
        {
            RunCatalogConfig catalog = UiSceneBuilder.LoadConfig<RunCatalogConfig>("Flow", "RunCatalogConfig");
            controller.Open(PreviewSamples.Combat(), new CatalogIconSource(catalog));
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
