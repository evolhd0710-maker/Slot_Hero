using System.IO;
using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.Events;
using SlotHero.Flow;
using SlotHero.Hud;
using SlotHero.Map;
using SlotHero.Popup;
using SlotHero.Profile;
using SlotHero.Reward;
using SlotHero.RunResult;
using SlotHero.Sanctum;
using SlotHero.Save;
using SlotHero.Settings;
using SlotHero.Title;
using SlotHero.TopBar;
using SlotHero.Ui;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 영역들의 설정 에셋을 기본값으로 한꺼번에 만든다.
    /// 메뉴 Slot Hero / 설정 에셋 한 번에 만들기 에서 실행한다.
    ///
    /// 각 설정 에셋의 기본값은 그 영역 기획서의 수치를 그대로 적어 둔 것이라
    /// 이렇게 만들기만 해도 기획서대로 된 값이 들어간다.
    /// 이미 있는 에셋은 건드리지 않으므로 여러 번 눌러도 손으로 고친 값이 지워지지 않는다.
    /// </summary>
    public static class CreateDefaultConfigs
    {
        /// 에셋을 모아 두는 자리
        private const string Root = "Assets/SlotHero/Configs";

        [MenuItem("Slot Hero/설정 에셋 한 번에 만들기")]
        public static void Create()
        {
            int made = 0;

            made += Make<MapGenerationConfig>("Map", "MapGenerationConfig");
            made += Make<MapVisualConfig>("Map", "MapVisualConfig");

            made += Make<SanctumConfig>("Sanctum", "SanctumConfig");
            made += Make<SanctumPriceConfig>("Sanctum", "SanctumPriceConfig");
            made += Make<SanctumVisualConfig>("Sanctum", "SanctumVisualConfig");
            made += Make<SanctumLayoutConfig>("Sanctum", "SanctumLayoutConfig");
            made += Make<MerchantLayoutConfig>("Sanctum", "MerchantLayoutConfig");

            made += Make<HudLayoutConfig>("Hud", "HudLayoutConfig");
            made += Make<HudVisualConfig>("Hud", "HudVisualConfig");

            made += Make<TopBarLayoutConfig>("TopBar", "TopBarLayoutConfig");
            made += Make<TopBarVisualConfig>("TopBar", "TopBarVisualConfig");

            made += Make<EventLayoutConfig>("Events", "EventLayoutConfig");
            made += Make<EventVisualConfig>("Events", "EventVisualConfig");
            made += Make<EventIllustrationConfig>("Events", "EventIllustrationConfig");

            made += Make<CurrentBuildLayoutConfig>("CurrentBuild", "CurrentBuildLayoutConfig");
            made += Make<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            made += Make<PopupLayoutConfig>("Popup", "PopupLayoutConfig");
            made += Make<PopupVisualConfig>("Popup", "PopupVisualConfig");

            made += Make<SettingsCatalogConfig>("Settings", "SettingsCatalogConfig");

            // 설정 목록은 코드가 정본이다. 이벤트 목록처럼 이미 있어도 코드의 목록으로 갈아 끼운다.
            // 그러지 않으면 선택지를 바꿔도 옛 에셋의 선택지가 그대로 나온다.
            Reset<SettingsCatalogConfig>("Settings", "SettingsCatalogConfig");

            made += Make<SettingsLayoutConfig>("Settings", "SettingsLayoutConfig");
            made += Make<SettingsVisualConfig>("Settings", "SettingsVisualConfig");

            made += Make<RunResultLayoutConfig>("RunResult", "RunResultLayoutConfig");
            made += Make<RunResultVisualConfig>("RunResult", "RunResultVisualConfig");

            made += Make<ProfileLayoutConfig>("Profile", "ProfileLayoutConfig");
            made += Make<ProfileVisualConfig>("Profile", "ProfileVisualConfig");

            made += Make<TitleLayoutConfig>("Title", "TitleLayoutConfig");
            made += Make<TitleVisualConfig>("Title", "TitleVisualConfig");

            made += Make<RewardLayoutConfig>("Reward", "RewardLayoutConfig");
            made += Make<RewardVisualConfig>("Reward", "RewardVisualConfig");

            made += Make<SaveConfig>("Save", "SaveConfig");

            // 여러 화면이 함께 쓰는 스크롤 휠 거리와 스크롤 바 생김새(2026년 10월 25일 원재).
            made += Make<ScrollVisualConfig>("Ui", "ScrollVisualConfig");

            made += Make<CombatRuleConfig>("Combat", "CombatRuleConfig");
            made += Make<CombatLayoutConfig>("Combat", "CombatLayoutConfig");
            made += Make<CombatVisualConfig>("Combat", "CombatVisualConfig");
            made += Make<RelicConfig>("Combat", "RelicConfig");

            // 유물 목록은 코드가 정본이다. 이미 있어도 시트의 목록으로 갈아 끼우고 물건 목록도 맞춘다.
            Reset<RelicConfig>("Combat", "RelicConfig");
            RunCatalogConfig catalog = AssetDatabase.LoadAssetAtPath<RunCatalogConfig>(Root + "/Flow/RunCatalogConfig.asset");
            if (catalog != null)
            {
                SyncRelics(catalog);
                EditorUtility.SetDirty(catalog);
            }

            made += MakeCatalog();

            // 이벤트 목록은 임시다. 이벤트 기획서의 실제 이벤트가 정해지면 갈아 끼운다.
            made += MakeEvents();
            made += Make<RewardRuleConfig>("Flow", "RewardRuleConfig");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("설정 에셋 " + made + "개를 새로 만들었다. 자리: " + Root);
        }

        /// <summary>
        /// 자리와 표시 설정 에셋을 기본값으로 되돌린다.
        ///
        /// 좌표 배율(UiScale.Factor)을 바꾸면 코드의 기본값만 달라지고
        /// 이미 만들어 둔 .asset 안에는 옛 값이 그대로 남는다. 그걸 갈아 준다.
        ///
        /// 지우고 다시 만들지 않고 `CopySerialized` 로 같은 파일 안을 덮어쓴다.
        /// 지우면 GUID 가 바뀌어 씬과 프리팹이 붙들고 있던 참조가 전부 끊긴다.
        ///
        /// 물건 목록과 이벤트 목록은 손대지 않는다. 자리 값이 없고, 되돌리면 목록이 비어 버린다.
        ///
        /// **되돌리면 설정 안에 이어 둔 그림도 함께 지워진다.**
        /// 방 종류 아이콘과 태그 그림이 그렇다. 되돌린 뒤에는 `ImportArt.Import` 를 다시 돌린다.
        /// `RunEverything.All` 은 바로 뒤에서 그렇게 한다.
        ///
        /// 손으로 고친 값도 함께 사라진다. 배율을 바꿨을 때만 부른다.
        /// </summary>
        [MenuItem("Slot Hero/설정 에셋 기본값으로 되돌리기")]
        public static void ResetLayoutAndVisual()
        {
            int reset = 0;

            reset += Reset<MapVisualConfig>("Map", "MapVisualConfig");

            reset += Reset<HudLayoutConfig>("Hud", "HudLayoutConfig");
            reset += Reset<HudVisualConfig>("Hud", "HudVisualConfig");

            reset += Reset<TopBarLayoutConfig>("TopBar", "TopBarLayoutConfig");
            reset += Reset<TopBarVisualConfig>("TopBar", "TopBarVisualConfig");

            reset += Reset<EventLayoutConfig>("Events", "EventLayoutConfig");
            reset += Reset<EventVisualConfig>("Events", "EventVisualConfig");

            reset += Reset<CurrentBuildLayoutConfig>("CurrentBuild", "CurrentBuildLayoutConfig");
            reset += Reset<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            reset += Reset<PopupLayoutConfig>("Popup", "PopupLayoutConfig");
            reset += Reset<PopupVisualConfig>("Popup", "PopupVisualConfig");

            reset += Reset<SettingsLayoutConfig>("Settings", "SettingsLayoutConfig");
            reset += Reset<SettingsVisualConfig>("Settings", "SettingsVisualConfig");

            reset += Reset<RunResultLayoutConfig>("RunResult", "RunResultLayoutConfig");
            reset += Reset<RunResultVisualConfig>("RunResult", "RunResultVisualConfig");

            reset += Reset<ProfileLayoutConfig>("Profile", "ProfileLayoutConfig");
            reset += Reset<ProfileVisualConfig>("Profile", "ProfileVisualConfig");

            reset += Reset<TitleLayoutConfig>("Title", "TitleLayoutConfig");
            reset += Reset<TitleVisualConfig>("Title", "TitleVisualConfig");

            reset += Reset<RewardLayoutConfig>("Reward", "RewardLayoutConfig");
            reset += Reset<RewardVisualConfig>("Reward", "RewardVisualConfig");

            reset += Reset<SanctumVisualConfig>("Sanctum", "SanctumVisualConfig");
            reset += Reset<SanctumLayoutConfig>("Sanctum", "SanctumLayoutConfig");
            reset += Reset<MerchantLayoutConfig>("Sanctum", "MerchantLayoutConfig");

            reset += Reset<ScrollVisualConfig>("Ui", "ScrollVisualConfig");

            reset += Reset<CombatLayoutConfig>("Combat", "CombatLayoutConfig");
            reset += Reset<CombatVisualConfig>("Combat", "CombatVisualConfig");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("설정 에셋 " + reset + "개를 기본값으로 되돌렸다. 좌표 배율 "
                + SlotHero.Ui.UiScale.Factor + "배가 들어갔다.");
        }

        /// <summary>
        /// 에셋 하나의 값을 코드의 기본값으로 덮어쓴다. GUID 는 그대로 둔다.
        /// 되돌린 것은 1을, 파일이 없어 넘어간 것은 0을 돌려준다.
        /// </summary>
        private static int Reset<T>(string area, string name) where T : ScriptableObject
        {
            string path = Root + "/" + area + "/" + name + ".asset";

            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                return 0;
            }

            T fresh = ScriptableObject.CreateInstance<T>();
            EditorUtility.CopySerialized(fresh, existing);
            Object.DestroyImmediate(fresh);

            EditorUtility.SetDirty(existing);
            return 1;
        }

        /// <summary>
        /// 물건 목록을 만든다.
        ///
        /// 문양 서른여섯과 태그는 전투 시스템 기획서 10c 의 04 문양/태그 표와 영문명 표를 따른다(2026년 10월 19일 원재).
        /// 아홉 행성에서 둘을 고르는 모든 짝이 한 번씩 나온다.
        ///
        /// 유물과 코인은 그림이 있는 것만 담았다.
        /// 저장소의 RelicData 와 BetCoinData 에셋이 오면 그쪽 이름과 등급으로 맞춘다.
        /// </summary>
        private static int MakeCatalog()
        {
            string path = Root + "/Flow/RunCatalogConfig.asset";
            EnsureFolder(Root + "/Flow");

            // 이미 있으면 손대지 않는다. 에디터에서 조정한 값이 원본이다(2026년 10월 9일 원재).
            // 기획서 수치로 되돌리려면 메뉴 "물건 목록 수치를 기획서 값으로 되돌리기" 를 쓴다.
            // 문양 이름을 기획서에 맞춘 것만은 이미 있는 에셋에도 옮긴다. 바뀐 칸 말고는 건드리지 않는다.
            if (File.Exists(path))
            {
                RunCatalogConfig existing = AssetDatabase.LoadAssetAtPath<RunCatalogConfig>(path);
                RenameCatalogSymbols(existing);

                // 세계관 유물과 코인은 이미 있는 에셋에도 빠진 것만 더한다. 있던 칸은 건드리지 않는다.
                if (AddWorldItems(existing) > 0)
                {
                    EditorUtility.SetDirty(existing);
                }

                return 0;
            }

            RunCatalogConfig config = ScriptableObject.CreateInstance<RunCatalogConfig>();

            // 태그 하나의 값. 행상이 문양 값을 매길 때 가진 태그 둘의 값을 더해 쓴다.
            // 전투 시스템 기획서 10c / 04 문양·태그 표 의 값이다. 예전에는 태양계 차례대로 1 ~ 9 를 넣었다.
            config.TagValues.AddRange(RunCatalogConfig.SpecTagValues());

            // 문양 서른여섯. 전투 시스템 기획서 10c 의 04 문양/태그 표와 영문명 표를 따른다(2026년 10월 19일 원재).
            // `Pheonix`, `Shipwerck` 는 영어 철자로는 틀렸지만 기획서 표기를 그대로 쓴다.
            // 아홉 행성에서 둘을 고르는 모든 짝이 한 번씩 나온다.
            AddSymbol(config, "Quicksilver", "수은", SymbolTagType.Mercury, SymbolTagType.Venus);
            AddSymbol(config, "Fish", "물고기", SymbolTagType.Mercury, SymbolTagType.Earth);
            AddSymbol(config, "Geyser", "간헐천", SymbolTagType.Mercury, SymbolTagType.Mars);
            AddSymbol(config, "Lotus", "연꽃", SymbolTagType.Mercury, SymbolTagType.Jupiter);
            AddSymbol(config, "Mud", "진흙", SymbolTagType.Mercury, SymbolTagType.Saturn);
            AddSymbol(config, "DarkCloud", "먹구름", SymbolTagType.Mercury, SymbolTagType.Uranus);
            AddSymbol(config, "Tsunami", "해일", SymbolTagType.Mercury, SymbolTagType.Neptune);
            AddSymbol(config, "Styx", "스틱스강", SymbolTagType.Mercury, SymbolTagType.Pluto);
            AddSymbol(config, "Chariot", "전차", SymbolTagType.Venus, SymbolTagType.Earth);
            AddSymbol(config, "Furnace", "용광로", SymbolTagType.Venus, SymbolTagType.Mars);
            AddSymbol(config, "GoldLaurel", "금월계관", SymbolTagType.Venus, SymbolTagType.Jupiter);
            AddSymbol(config, "Gem", "보석", SymbolTagType.Venus, SymbolTagType.Saturn);
            AddSymbol(config, "Meteorite", "운석", SymbolTagType.Venus, SymbolTagType.Uranus);
            AddSymbol(config, "Anchor", "닻", SymbolTagType.Venus, SymbolTagType.Neptune);
            AddSymbol(config, "Scythe", "낫", SymbolTagType.Venus, SymbolTagType.Pluto);
            AddSymbol(config, "Pheonix", "불사조", SymbolTagType.Earth, SymbolTagType.Mars);
            AddSymbol(config, "Druid", "드루이드", SymbolTagType.Earth, SymbolTagType.Jupiter);
            AddSymbol(config, "Golem", "골렘", SymbolTagType.Earth, SymbolTagType.Saturn);
            AddSymbol(config, "Bird", "새", SymbolTagType.Earth, SymbolTagType.Uranus);
            AddSymbol(config, "Whale", "고래", SymbolTagType.Earth, SymbolTagType.Neptune);
            AddSymbol(config, "Raven", "까마귀", SymbolTagType.Earth, SymbolTagType.Pluto);
            AddSymbol(config, "FireFlower", "불의 꽃", SymbolTagType.Mars, SymbolTagType.Jupiter);
            AddSymbol(config, "Volcano", "화산", SymbolTagType.Mars, SymbolTagType.Saturn);
            AddSymbol(config, "Sunrise", "일출", SymbolTagType.Mars, SymbolTagType.Uranus);
            AddSymbol(config, "LightHouse", "등대", SymbolTagType.Mars, SymbolTagType.Neptune);
            AddSymbol(config, "Fullmoon", "만월", SymbolTagType.Mars, SymbolTagType.Pluto);
            AddSymbol(config, "Forest", "숲", SymbolTagType.Jupiter, SymbolTagType.Saturn);
            AddSymbol(config, "Yggdrasil", "세계수", SymbolTagType.Jupiter, SymbolTagType.Uranus);
            AddSymbol(config, "Coral", "산호", SymbolTagType.Jupiter, SymbolTagType.Neptune);
            AddSymbol(config, "Coffin", "관", SymbolTagType.Jupiter, SymbolTagType.Pluto);
            AddSymbol(config, "Desert", "사막", SymbolTagType.Saturn, SymbolTagType.Uranus);
            AddSymbol(config, "Island", "섬", SymbolTagType.Saturn, SymbolTagType.Neptune);
            AddSymbol(config, "Grave", "무덤", SymbolTagType.Saturn, SymbolTagType.Pluto);
            AddSymbol(config, "Hail", "우박", SymbolTagType.Uranus, SymbolTagType.Neptune);
            AddSymbol(config, "NightSky", "밤하늘", SymbolTagType.Uranus, SymbolTagType.Pluto);
            AddSymbol(config, "Shipwerck", "난파선", SymbolTagType.Neptune, SymbolTagType.Pluto);

            AddItem(config.Coins, "coin_gold", "금화", ItemRarity.Common);
            AddItem(config.Coins, "coin_silver", "은화", ItemRarity.Common);

            // 유물은 유물 정보시트의 열 개다. 2026년 10월 25일 임시 셋(낡은 책, 날개, 곡괭이)을 뺐다.
            SyncRelics(config);

            AddWorldItems(config);

            // 런 시작 덱. **모든 문양을 한 장씩 넣는다.**
            //
            // 2026년 10월 2일에 원재가 그렇게 정했다. 서른여섯 장 서른여섯 종이다.
            // 14 현재 빌드 화면 와이어프레임의 예시 덱은 넷뿐이었는데,
            // 그러면 성소와 이벤트에서 문양을 다루는 길이 넷 안에서만 돌아
            // 바꾸고 잃고 받는 것이 제대로 보이지 않는다.
            //
            // 실제 시작 덱은 런 시작 선택 화면이 정할 몫이다. 그 화면이 오면 이 줄을 지운다.
            for (int i = 0; i < config.Symbols.Count; i++)
            {
                AddStarting(config, config.Symbols[i].Id, 1);
            }

            config.StartingCoinIds.Add("coin_gold");

            // 성소 기획서 06 재화 의 "시작 수치 0". 예전에는 100 이었다.
            config.StartingGold = 0;
            AssetDatabase.CreateAsset(config, path);
            return 1;
        }

        /// <summary>
        /// 물건 목록 에셋의 기획서 수치를 기획서 값으로 되돌린다.
        /// 태그 가치, 시작 골드, 덱 최소 장수, 코인과 유물 소지 한도다.
        /// 문양과 코인과 유물 목록, 그림 연결은 건드리지 않는다.
        ///
        /// **손으로 누를 때만 돈다.** 예전에는 설정 에셋을 만들 때마다 불려 에디터에서 조정한 값을 덮었다.
        /// 2026년 10월 9일 외부 검토가 짚었고 원재가 에디터 값을 원본으로 두기로 정했다.
        /// </summary>
        [MenuItem("Slot Hero/물건 목록 수치를 기획서 값으로 되돌리기")]
        public static void ResetCatalogNumbers()
        {
            RunCatalogConfig config = AssetDatabase.LoadAssetAtPath<RunCatalogConfig>(Root + "/Flow/RunCatalogConfig.asset");
            if (config == null)
            {
                Debug.LogWarning("물건 목록 에셋이 없다. 먼저 설정 에셋 한 번에 만들기 를 누른다.");
                return;
            }

            config.TagValues = RunCatalogConfig.SpecTagValues();
            config.StartingGold = 0;
            config.MinimumDeckSize = 6;
            config.StartingCoinCapacity = 5;
            config.StartingRelicCapacity = 6;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("물건 목록 수치를 기획서 값으로 되돌렸다.");
        }

        /// <summary>
        /// 이벤트 목록을 만든다. **없을 때 한 번만** 코드의 각본(`EventScripts.BuildAll`)으로 채운다.
        ///
        /// 각본은 `이벤트 기획서 초안` 6장 이벤트 상세 를 옮긴 것이다. 본문과 비워 둔 수치는 임시다. 자세한 것은 `Events/README.md` 를 본다.
        ///
        /// **이미 있으면 이벤트를 건드리지 않는다.** 만든 뒤에는 에디터의 에셋이 원본이다.
        /// 예전에는 실행할 때마다 코드의 각본으로 갈아 끼워 에디터에서 고친 이벤트가 사라졌다.
        /// 2026년 10월 9일 외부 검토가 짚었고 원재가 에디터 에셋을 원본으로 두기로 정했다.
        /// 코드의 각본으로 되돌리려면 메뉴 "이벤트 목록을 코드 각본으로 되돌리기" 를 쓴다.
        /// </summary>
        private static int MakeEvents()
        {
            string path = Root + "/Flow/RunEventConfig.asset";
            EnsureFolder(Root + "/Flow");

            RunEventConfig existing = AssetDatabase.LoadAssetAtPath<RunEventConfig>(path);
            if (existing != null)
            {
                // 부정 효과 목록은 2026년 10월 8일에 더했다. 그 전에 만든 에셋은 비어 있을 수 있어 그때만 채운다.
                if (existing.CombatPenalties == null || existing.CombatPenalties.Count == 0)
                {
                    existing.CombatPenalties = SlotHero.Events.EventCombatPenalty.SpecDefaults();
                    EditorUtility.SetDirty(existing);
                }

                return 0;
            }

            RunEventConfig config = ScriptableObject.CreateInstance<RunEventConfig>();
            config.Events.AddRange(EventScripts.BuildAll());

            AssetDatabase.CreateAsset(config, path);
            return 1;
        }

        /// <summary>
        /// 이벤트 목록을 코드의 각본으로 갈아 끼운다. **에디터에서 고친 이벤트는 사라진다.**
        /// 코드에서 각본을 고쳐 게임에 넣고 싶을 때만 누른다. 파일을 지우지 않고 같은 에셋 안을 고치므로 씬이 붙든 참조는 끊기지 않는다.
        /// </summary>
        [MenuItem("Slot Hero/이벤트 목록을 코드 각본으로 되돌리기")]
        public static void ResetEventsToCode()
        {
            RunEventConfig config = AssetDatabase.LoadAssetAtPath<RunEventConfig>(Root + "/Flow/RunEventConfig.asset");
            if (config == null)
            {
                Debug.LogWarning("이벤트 목록 에셋이 없다. 먼저 설정 에셋 한 번에 만들기 를 누른다.");
                return;
            }

            config.Events.Clear();
            config.Events.AddRange(EventScripts.BuildAll());
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("이벤트 목록을 코드 각본으로 되돌렸다. 이벤트 " + config.Events.Count + "개.");
        }

        private static void AddSymbol(
            RunCatalogConfig config, string id, string name,
            SymbolTagType first, SymbolTagType second)
        {
            SymbolDefinition symbol = new SymbolDefinition();
            symbol.Id = id;
            symbol.DisplayName = name;
            symbol.FirstTag = first;
            symbol.SecondTag = second;
            config.Symbols.Add(symbol);
        }

        /// <summary>
        /// 이미 만든 물건 목록의 문양 이름을 기획서에 맞춘다. 2026년 10월 19일 원재가 정했다.
        ///
        /// 바뀐 식별자(`SymbolIdRenames`)와 금빛계관 → 금월계관 이름만 고치고 나머지 칸은 그대로 둔다.
        /// 에셋을 지우고 다시 만들면 에디터에서 조정한 값이 날아가기 때문이다.
        /// 이미 고친 에셋이면 바꿀 것이 남지 않아 몇 번 불러도 같다.
        /// </summary>
        public static int RenameCatalogSymbols(RunCatalogConfig config)
        {
            if (config == null)
            {
                return 0;
            }

            int changed = 0;

            for (int i = 0; i < config.Symbols.Count; i++)
            {
                SymbolDefinition symbol = config.Symbols[i];
                string id = SymbolIdRenames.Current(symbol.Id);
                string name = symbol.Id == "GoldLaurel" && symbol.DisplayName == "금빛계관" ? "금월계관" : symbol.DisplayName;

                if (id != symbol.Id || name != symbol.DisplayName)
                {
                    symbol.Id = id;
                    symbol.DisplayName = name;
                    config.Symbols[i] = symbol;
                    changed++;
                }
            }

            for (int i = 0; i < config.StartingSymbolIds.Count; i++)
            {
                string id = SymbolIdRenames.Current(config.StartingSymbolIds[i]);
                if (id != config.StartingSymbolIds[i])
                {
                    config.StartingSymbolIds[i] = id;
                    changed++;
                }
            }

            if (changed > 0)
            {
                config.InvalidateIndex();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Debug.Log("물건 목록의 문양 이름을 기획서에 맞췄다: " + changed + "칸");
            }

            return changed;
        }

        /// <summary>런 시작 덱에 그 문양을 몇 장 넣는다.</summary>
        private static void AddStarting(RunCatalogConfig config, string id, int count)
        {
            for (int i = 0; i < count; i++)
            {
                config.StartingSymbolIds.Add(id);
            }
        }

        /// <summary>
        /// 세계관에 맞춘 유물 스물과 코인 서른(2026년 10월 22일 원재). 목록에 없는 것만 더하고 더한 수를 돌려준다.
        ///
        /// 유물은 다섯 지역(쇠락한 제국, 신전, 바다, 사막, 유적)마다 넷이고 지역마다 일반 · 고급 · 희귀 · 영웅이 하나씩이다.
        /// 등급은 내가 나눴다. 코인은 금 · 은 · 동 열씩이고 동은 고급, 은은 희귀, 금은 영웅이다(원재가 정했다).
        /// 옛 유물 셋과 코인 둘은 그대로 둔다(원재가 정했다). 그림은 `Flow/Art` 의 식별자 이름 그림이 붙는다.
        /// 효과는 전투 시스템이 정할 몫이라 이름과 등급만 있다.
        /// </summary>
        private static int AddWorldItems(RunCatalogConfig config)
        {
            int added = 0;

            // 쇠락한 제국
            added += AddItemIfMissing(config.Relics, "relic_rusted_gladius", "녹슨 단검", ItemRarity.Common);
            added += AddItemIfMissing(config.Relics, "relic_senate_ring", "원로원 인장 반지", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Relics, "relic_legion_eagle", "군단 독수리", ItemRarity.Rare);
            added += AddItemIfMissing(config.Relics, "relic_cracked_crown", "금 간 왕관", ItemRarity.Epic);

            // 신전
            added += AddItemIfMissing(config.Relics, "relic_owl_lamp", "부엉이 등잔", ItemRarity.Common);
            added += AddItemIfMissing(config.Relics, "relic_marble_mask", "대리석 가면", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Relics, "relic_laurel_lyre", "월계수 리라", ItemRarity.Rare);
            added += AddItemIfMissing(config.Relics, "relic_oracle_tripod", "신탁의 삼발이", ItemRarity.Epic);

            // 바다
            added += AddItemIfMissing(config.Relics, "relic_sealed_amphora", "봉인된 암포라", ItemRarity.Common);
            added += AddItemIfMissing(config.Relics, "relic_siren_shell", "세이렌의 소라", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Relics, "relic_star_disc", "별자리 원반", ItemRarity.Rare);
            added += AddItemIfMissing(config.Relics, "relic_bronze_trident", "청동 삼지창", ItemRarity.Epic);

            // 사막
            added += AddItemIfMissing(config.Relics, "relic_canopic_jar", "카노푸스 단지", ItemRarity.Common);
            added += AddItemIfMissing(config.Relics, "relic_eye_pendant", "호루스의 눈 목걸이", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Relics, "relic_golden_scarab", "황금 스카라브", ItemRarity.Rare);
            added += AddItemIfMissing(config.Relics, "relic_ankh", "생명의 앙크", ItemRarity.Epic);

            // 유적
            added += AddItemIfMissing(config.Relics, "relic_clay_tablet", "쐐기문자 점토판", ItemRarity.Common);
            added += AddItemIfMissing(config.Relics, "relic_cylinder_seal", "원통 인장", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Relics, "relic_lapis_harp", "청금석 하프", ItemRarity.Rare);
            added += AddItemIfMissing(config.Relics, "relic_lamassu_idol", "라마수 조각", ItemRarity.Epic);

            // 금화. 영웅
            added += AddItemIfMissing(config.Coins, "coin_gold_emperor", "황제 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_arch", "개선문 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_athena", "아테나 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_sunchariot", "태양 전차 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_poseidon", "포세이돈 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_galley", "황금 함선 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_sundisk", "태양 원반 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_pharaoh", "파라오 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_ishtar", "이슈타르 금화", ItemRarity.Epic);
            added += AddItemIfMissing(config.Coins, "coin_gold_lamassu", "라마수 금화", ItemRarity.Epic);

            // 은화. 희귀
            added += AddItemIfMissing(config.Coins, "coin_silver_eagle", "독수리 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_laurel", "월계관 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_owl", "부엉이 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_column", "신전 기둥 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_dolphin", "돌고래 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_trireme", "삼단노선 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_scarab", "스카라브 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_moon", "초승달 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_wedge", "쐐기문자 은화", ItemRarity.Rare);
            added += AddItemIfMissing(config.Coins, "coin_silver_ziggurat", "지구라트 은화", ItemRarity.Rare);

            // 동전. 고급
            added += AddItemIfMissing(config.Coins, "coin_bronze_wolf", "늑대 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_shield", "방패 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_lyre", "리라 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_amphora", "암포라 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_octopus", "문어 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_anchor", "닻 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_pyramid", "피라미드 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_camel", "낙타 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_lion", "사자 동전", ItemRarity.Uncommon);
            added += AddItemIfMissing(config.Coins, "coin_bronze_palm", "대추야자 동전", ItemRarity.Uncommon);

            return added;
        }

        /// <summary>그 식별자가 목록에 없을 때만 더한다. 더했으면 1 이다.</summary>
        private static int AddItemIfMissing(
            System.Collections.Generic.List<ItemDefinition> into,
            string id, string name, ItemRarity rarity)
        {
            for (int i = 0; i < into.Count; i++)
            {
                if (into[i].Id == id)
                {
                    return 0;
                }
            }

            AddItem(into, id, name, rarity);
            return 1;
        }

        private static void AddItem(
            System.Collections.Generic.List<ItemDefinition> into,
            string id, string name, ItemRarity rarity)
        {
            ItemDefinition item = new ItemDefinition();
            item.Id = id;
            item.DisplayName = name;
            item.Rarity = rarity;
            into.Add(item);
        }

        /// 에셋 하나를 만든다. 이미 있으면 0을, 새로 만들면 1을 돌려준다
        private static int Make<T>(string area, string name) where T : ScriptableObject
        {
            string folder = Root + "/" + area;
            EnsureFolder(folder);

            string path = folder + "/" + name + ".asset";
            if (File.Exists(path))
            {
                return 0;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return 1;
        }

        /// <summary>
        /// 물건 목록의 유물을 유물 정보시트의 열 개로 맞춘다.
        /// 시트에 있는 유물은 이름과 희귀도를 시트대로 고친다. 이어 둔 그림과 등장 조건은 그대로 둔다.
        /// 임시 셋(낡은 책, 날개, 곡괭이)은 뺀다. 세계관 유물 스물(`AddWorldItems`)은 시트 열 개 뒤에 그대로 둔다.
        /// 2026년 10월 26일 원재가 시트 열 개에 그림만 있는 세계관 유물을 더해 서른으로 두라고 했다.
        /// 희귀도는 시트의 1 ~ 5 가 일반 ~ 전설이다.
        /// </summary>
        public static void SyncRelics(RunCatalogConfig config)
        {
            System.Collections.Generic.List<RelicDefinition> sheet = RelicConfig.SheetRelics();
            System.Collections.Generic.List<ItemDefinition> synced = new System.Collections.Generic.List<ItemDefinition>();

            for (int i = 0; i < sheet.Count; i++)
            {
                ItemDefinition item = new ItemDefinition();
                bool found = false;

                for (int j = 0; j < config.Relics.Count; j++)
                {
                    if (config.Relics[j].Id == sheet[i].Id)
                    {
                        item = config.Relics[j];
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    item.Id = sheet[i].Id;
                }

                item.DisplayName = sheet[i].DisplayName;
                item.Rarity = (ItemRarity)Mathf.Clamp(sheet[i].Rarity - 1, 0, 4);
                synced.Add(item);
            }

            string[] removed = { "relic_book", "relic_wing", "relic_pick" };
            for (int j = 0; j < config.Relics.Count; j++)
            {
                ItemDefinition other = config.Relics[j];
                bool inSheet = false;
                for (int i = 0; i < synced.Count; i++)
                {
                    inSheet |= synced[i].Id == other.Id;
                }

                if (!inSheet && System.Array.IndexOf(removed, other.Id) < 0)
                {
                    synced.Add(other);
                }
            }

            config.Relics = synced;
            config.InvalidateIndex();
        }

        /// 폴더가 없으면 위에서부터 차례로 만든다
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
