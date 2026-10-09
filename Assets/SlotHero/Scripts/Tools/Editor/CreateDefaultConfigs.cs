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
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 여덟 영역의 설정 에셋을 기본값으로 한꺼번에 만든다.
    /// 메뉴 Slot Hero / 설정 에셋 한 번에 만들기 에서 실행한다.
    ///
    /// 각 설정 에셋의 기본값은 그 영역 기획서의 수치를 그대로 적어 둔 것이라
    /// 이렇게 만들기만 해도 기획서대로 된 값이 들어간다.
    /// 이미 있는 에셋은 건드리지 않으므로 여러 번 눌러도 손으로 고친 값이 지워지지 않는다.
    /// </summary>
    public static class CreateDefaultConfigs
    {
        /// 에셋을 모아 두는 자리
        private const string Root = "Assets/SlotHeroConfigs";

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
        /// 문양 서른여섯과 태그는 저장소의 SymbolTagAutoAssigner 에 적힌 것을 그대로 옮겼다.
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
            if (File.Exists(path))
            {
                return 0;
            }

            RunCatalogConfig config = ScriptableObject.CreateInstance<RunCatalogConfig>();

            // 태그 하나의 값. 행상이 문양 값을 매길 때 가진 태그 둘의 값을 더해 쓴다.
            // 전투 시스템 기획서 10c / 04 문양·태그 표 의 값이다. 예전에는 태양계 차례대로 1 ~ 9 를 넣었다.
            config.TagValues.AddRange(RunCatalogConfig.SpecTagValues());

            // 문양 서른여섯. 저장소 SymbolTagAutoAssigner 에 적힌 것을 그대로 옮겼다.
            // 아홉 행성에서 둘을 고르는 모든 짝이 한 번씩 나온다.
            AddSymbol(config, "QuickSilver", "수은", SymbolTagType.Mercury, SymbolTagType.Venus);
            AddSymbol(config, "Fish", "물고기", SymbolTagType.Mercury, SymbolTagType.Earth);
            AddSymbol(config, "Geyser", "간헐천", SymbolTagType.Mercury, SymbolTagType.Mars);
            AddSymbol(config, "Lotus", "연꽃", SymbolTagType.Mercury, SymbolTagType.Jupiter);
            AddSymbol(config, "Mud", "진흙", SymbolTagType.Mercury, SymbolTagType.Saturn);
            AddSymbol(config, "DarkCloud", "먹구름", SymbolTagType.Mercury, SymbolTagType.Uranus);
            AddSymbol(config, "Tsunami", "해일", SymbolTagType.Mercury, SymbolTagType.Neptune);
            AddSymbol(config, "Styx", "스틱스강", SymbolTagType.Mercury, SymbolTagType.Pluto);
            AddSymbol(config, "Chariot", "전차", SymbolTagType.Venus, SymbolTagType.Earth);
            AddSymbol(config, "Furnace", "용광로", SymbolTagType.Venus, SymbolTagType.Mars);
            AddSymbol(config, "GoldLaurel", "금빛계관", SymbolTagType.Venus, SymbolTagType.Jupiter);
            AddSymbol(config, "Gem", "보석", SymbolTagType.Venus, SymbolTagType.Saturn);
            AddSymbol(config, "Meteorite", "운석", SymbolTagType.Venus, SymbolTagType.Uranus);
            AddSymbol(config, "Anchor", "닻", SymbolTagType.Venus, SymbolTagType.Neptune);
            AddSymbol(config, "Scythe", "낫", SymbolTagType.Venus, SymbolTagType.Pluto);
            AddSymbol(config, "Phoenix", "불사조", SymbolTagType.Earth, SymbolTagType.Mars);
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
            AddSymbol(config, "Shipwreck", "난파선", SymbolTagType.Neptune, SymbolTagType.Pluto);

            AddItem(config.Coins, "coin_gold", "금화", ItemRarity.Common);
            AddItem(config.Coins, "coin_silver", "은화", ItemRarity.Common);

            AddItem(config.Relics, "relic_book", "낡은 책", ItemRarity.Common);
            AddItem(config.Relics, "relic_wing", "날개", ItemRarity.Uncommon);
            AddItem(config.Relics, "relic_pick", "곡괭이", ItemRarity.Rare);

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

        /// <summary>런 시작 덱에 그 문양을 몇 장 넣는다.</summary>
        private static void AddStarting(RunCatalogConfig config, string id, int count)
        {
            for (int i = 0; i < count; i++)
            {
                config.StartingSymbolIds.Add(id);
            }
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
