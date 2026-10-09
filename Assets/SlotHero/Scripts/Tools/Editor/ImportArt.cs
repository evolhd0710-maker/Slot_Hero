using System.Collections.Generic;
using System.IO;
using SlotHero.CurrentBuild;
using SlotHero.Events;
using SlotHero.Flow;
using SlotHero.Map;
using UnityEditor;
using UnityEngine;
using SlotHero.Combat;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 그려 둔 그림을 프로젝트로 가져와 스프라이트로 바꾸고 설정 에셋에 물린다.
    /// 메뉴 Slot Hero / 그림 가져오기 에서 실행한다.
    ///
    /// 가져오는 것은 `코드` 폴더 아래의 `Art` 들이다.
    /// 맵 노드 아이콘 다섯과 완료 체크, 태그 기호 아홉이다.
    /// </summary>
    public static class ImportArt
    {
        private const string ArtFolder = "Assets/SlotHeroArt";

        /// 그림이 있는 원래 자리. 유니티 프로젝트 밖이다.
        private const string SourceRoot = @"C:\Users\wonja\Downloads\slot hero\코드";

        /// <summary>
        /// 저장소를 받아 둔 자리. 문양 그림 서른여섯 장이 `Assets/SymbolImage` 에 있다.
        /// 없으면 문양 그림 없이 넘어간다. 그 자리는 빈 칸으로 나온다.
        /// </summary>
        private const string RepoRoot = @"C:\Users\wonja\Downloads\slot hero\Slot_Hero";

        [MenuItem("Slot Hero/그림 가져오기")]
        public static void Import()
        {
            CopyFolder(Path.Combine(SourceRoot, @"Map\Art"), ArtFolder + "/Map");
            CopyFolder(Path.Combine(SourceRoot, @"CurrentBuild\Art"), ArtFolder + "/CurrentBuild");
            CopyFolder(Path.Combine(SourceRoot, @"TopBar\Art"), ArtFolder + "/TopBar");
            CopyFolder(Path.Combine(SourceRoot, @"Flow\Art"), ArtFolder + "/Items");
            CopyFolder(Path.Combine(SourceRoot, @"Sanctum\Art"), ArtFolder + "/Sanctum");
            CopyFolder(Path.Combine(SourceRoot, @"Hud\Art"), ArtFolder + "/Hud");

            // 타이틀 그림은 아직 없다. 배경 요청 기획서의 "메인 화면 배경"을 발주해 둔 상태다.
            // 폴더가 없으면 조용히 넘어가므로 그림이 오면 넣기만 하면 붙는다.
            CopyFolder(Path.Combine(SourceRoot, @"Title\Art"), ArtFolder + "/Title");

            // 이벤트 삽화. 파일 이름이 곧 삽화 식별자다. `이벤트식별자_화면식별자.png`.
            CopyFolder(Path.Combine(SourceRoot, @"Events\Art"), ArtFolder + "/Events");

            // 문양 그림은 저장소에 있다. 받아 두었으면 그것도 가져온다.
            CopyFolder(Path.Combine(RepoRoot, @"Assets\SymbolImage"), ArtFolder + "/Symbols");

            AssetDatabase.Refresh();

            MakeSprites(ArtFolder + "/Map");
            MakeSprites(ArtFolder + "/CurrentBuild");
            MakeSprites(ArtFolder + "/TopBar");
            MakeSprites(ArtFolder + "/Items");
            MakeSprites(ArtFolder + "/Sanctum");
            MakeSprites(ArtFolder + "/Hud");
            MakeSprites(ArtFolder + "/Title");
            MakeSprites(ArtFolder + "/Events");
            MakeSprites(ArtFolder + "/Symbols");

            AssetDatabase.Refresh();

            LinkMapIcons();
            LinkTagIcons();
            LinkCatalogIcons();
            LinkEventIllustrations();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>폴더의 PNG 를 프로젝트로 옮긴다.</summary>
        private static void CopyFolder(string source, string target)
        {
            if (!Directory.Exists(source))
            {
                // 저장소 문양 그림처럼 없을 수도 있는 폴더가 있어 알리기만 하고 넘어간다.
                Debug.Log("그림 폴더가 없어 건너뛴다: " + source);
                return;
            }

            UiSceneBuilder.EnsureFolder(target);

            string[] files = Directory.GetFiles(source, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                string destination = Path.Combine(target, Path.GetFileName(files[i]));
                File.Copy(files[i], destination, true);
            }

            Debug.Log(files.Length + "개 그림을 옮겼다: " + target);
        }

        /// <summary>가져온 그림을 스프라이트로 바꾼다.</summary>
        private static void MakeSprites(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;

                // UI 그림은 압축하지 않는다.
                // 화면에 그대로 붙는 그림이라 압축 자국이 눈에 띄고,
                // 배치 모드에서 그림으로 뽑을 때 압축이 늦게 끝나 회색으로 나온다.
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                importer.SaveAndReimport();
            }
        }

        /// <summary>맵 노드 아이콘을 맵 표시 설정에 물린다.</summary>
        private static void LinkMapIcons()
        {
            MapVisualConfig visual = UiSceneBuilder.LoadConfig<MapVisualConfig>("Map", "MapVisualConfig");
            if (visual == null)
            {
                Debug.LogWarning("맵 표시 설정을 찾지 못했다.");
                return;
            }

            Dictionary<RoomType, string> names = new Dictionary<RoomType, string>
            {
                { RoomType.Normal, "노드_일반" },
                { RoomType.Elite, "노드_엘리트" },
                { RoomType.Sanctum, "노드_성소" },
                { RoomType.Event, "노드_이벤트" },
                { RoomType.Boss, "노드_보스" },
            };

            int linked = 0;

            for (int i = 0; i < visual.RoomVisuals.Count; i++)
            {
                RoomTypeVisual entry = visual.RoomVisuals[i];

                string fileName;
                if (!names.TryGetValue(entry.RoomType, out fileName))
                {
                    continue;
                }

                Sprite sprite = LoadSprite(ArtFolder + "/Map/" + fileName + ".png");
                if (sprite == null)
                {
                    continue;
                }

                entry.Icon = sprite;
                visual.RoomVisuals[i] = entry;
                linked++;
            }

            EditorUtility.SetDirty(visual);
            Debug.Log("맵 노드 아이콘 " + linked + "개를 물렸다.");
        }

        /// <summary>태그 기호 그림을 현재 빌드 표시 설정에 물린다.</summary>
        private static void LinkTagIcons()
        {
            CurrentBuildVisualConfig visual =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");

            if (visual == null)
            {
                Debug.LogWarning("현재 빌드 표시 설정을 찾지 못했다.");
                return;
            }

            Dictionary<SymbolTagType, string> names = new Dictionary<SymbolTagType, string>
            {
                { SymbolTagType.Mercury, "태그_수성" },
                { SymbolTagType.Venus, "태그_금성" },
                { SymbolTagType.Earth, "태그_지구" },
                { SymbolTagType.Mars, "태그_화성" },
                { SymbolTagType.Jupiter, "태그_목성" },
                { SymbolTagType.Saturn, "태그_토성" },
                { SymbolTagType.Uranus, "태그_천왕성" },
                { SymbolTagType.Neptune, "태그_해왕성" },
                { SymbolTagType.Pluto, "태그_명왕성" },
            };

            int linked = 0;

            for (int i = 0; i < visual.TagVisuals.Count; i++)
            {
                BuildTagVisual entry = visual.TagVisuals[i];

                string fileName;
                if (!names.TryGetValue(entry.Tag, out fileName))
                {
                    continue;
                }

                Sprite sprite = LoadSprite(ArtFolder + "/CurrentBuild/" + fileName + ".png");
                if (sprite == null)
                {
                    continue;
                }

                entry.Icon = sprite;
                visual.TagVisuals[i] = entry;
                linked++;
            }

            EditorUtility.SetDirty(visual);
            Debug.Log("태그 기호 그림 " + linked + "개를 물렸다.");
        }

        /// <summary>
        /// 물건 그림을 목록 설정에 물린다.
        ///
        /// 파일 이름이 곧 식별자다.
        /// 문양은 `Symbols/<식별자>.png`, 유물과 코인은 `Items/<식별자>.png` 를 찾는다.
        /// 저장소의 문양 그림 파일 이름이 그대로 식별자라 따로 표를 두지 않는다.
        /// 없는 것은 건너뛴다. 그림이 아직 없어도 게임은 돈다.
        /// </summary>
        private static void LinkCatalogIcons()
        {
            RunCatalogConfig catalog =
                UiSceneBuilder.LoadConfig<RunCatalogConfig>("Flow", "RunCatalogConfig");

            if (catalog == null)
            {
                Debug.LogWarning("물건 목록 설정을 찾지 못했다.");
                return;
            }

            int symbols = 0;

            for (int i = 0; i < catalog.Symbols.Count; i++)
            {
                SymbolDefinition symbol = catalog.Symbols[i];
                Sprite sprite = FindSprite(ArtFolder + "/Symbols/" + symbol.Id + ".png");

                if (sprite == null)
                {
                    continue;
                }

                symbol.Icon = sprite;
                catalog.Symbols[i] = symbol;
                symbols++;
            }

            int items = LinkItemList(catalog.Coins) + LinkItemList(catalog.Relics);

            EditorUtility.SetDirty(catalog);
            Debug.Log("문양 그림 " + symbols + "개, 유물과 코인 그림 " + items + "개를 물렸다.");

            if (symbols == 0)
            {
                Debug.Log(
                    "문양 그림이 하나도 없다. 저장소를 " + RepoRoot + " 에 받아 두면 서른여섯 장이 붙는다.");
            }
        }

        /// <summary>
        /// 이벤트 삽화를 삽화 목록에 물린다. 파일 이름이 곧 삽화 식별자다.
        /// 각본에 없는 이름의 그림도 넣어 둔다. 남는 그림은 쓰이지 않을 뿐 해가 없다.
        /// </summary>
        private static void LinkEventIllustrations()
        {
            EventIllustrationConfig config =
                UiSceneBuilder.LoadConfig<EventIllustrationConfig>("Events", "EventIllustrationConfig");

            if (config == null)
            {
                Debug.LogWarning("이벤트 삽화 목록을 찾지 못했다.");
                return;
            }

            string folder = ArtFolder + "/Events";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            config.Illustrations.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null)
                {
                    config.Set(Path.GetFileNameWithoutExtension(path), sprite);
                }
            }

            EditorUtility.SetDirty(config);
            Debug.Log("이벤트 삽화 " + config.Illustrations.Count + "장을 물렸다.");
        }

        /// <summary>이벤트 삽화 하나. 확인용 씬이 쓴다.</summary>
        public static Sprite LoadEventIllustration(string illustrationId)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Events/" + illustrationId + ".png");
        }

        private static int LinkItemList(System.Collections.Generic.List<ItemDefinition> list)
        {
            int linked = 0;

            for (int i = 0; i < list.Count; i++)
            {
                ItemDefinition item = list[i];
                Sprite sprite = FindSprite(ArtFolder + "/Items/" + item.Id + ".png");

                if (sprite == null)
                {
                    continue;
                }

                item.Icon = sprite;
                list[i] = item;
                linked++;
            }

            return linked;
        }

        /// <summary>있으면 돌려주고 없으면 조용히 null 을 돌려준다.</summary>
        private static Sprite FindSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>런 배경 그림. 게임 씬이 쓴다.</summary>
        public static Sprite LoadItemArt(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Items/" + name + ".png");
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogWarning("스프라이트를 찾지 못했다: " + path);
            }

            return sprite;
        }

        /// <summary>완료 체크 그림. 맵 노드 프리팹이 쓴다.</summary>
        public static Sprite LoadCheckMark()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Map/노드_완료체크.png");
        }

        /// <summary>
        /// 맵 화면 그림 하나.
        /// `지도_양피지` 는 와이어프레임 `10 맵 화면` 의 `양피지_세로확장2.png` 원본이다.
        /// 배경을 지울 때 남은 거의 투명한 붉은 찌꺼기만 털어 냈다.
        /// </summary>
        public static Sprite LoadMap(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Map/" + name + ".png");
        }

        /// <summary>상단 표시줄 그림 하나. 이름은 `상단 표시줄 예시.psd` 의 레이어에서 잘라 낸 것이다.</summary>
        public static Sprite LoadTopBar(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/TopBar/" + name + ".png");
        }

        /// <summary>현재 빌드 버튼 그림 하나. 우주 원 바탕과 혼천의 아이콘이다.</summary>
        public static Sprite LoadHud(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Hud/" + name + ".png");
        }

        /// <summary>
        /// 성소와 행상 그림 하나.
        /// 이름은 `성소 이미지 예시.psd` 와 `행상 예시.psd` 의 레이어에서 잘라 낸 것이다.
        /// </summary>
        public static Sprite LoadSanctum(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Sanctum/" + name + ".png");
        }

        /// <summary>
        /// 타이틀 화면 그림 하나. **아직 한 장도 없다.**
        /// 배경 요청 기획서의 "메인 화면 배경 · 고대 유적 슬롯"을 발주해 둔 상태다.
        /// `코드/Title/Art` 에 `배경_메인화면.png` 와 `로고.png` 를 넣으면 바로 붙는다.
        /// 없으면 null 이 와서 확인용 씬이 자리만 알아볼 수 있게 둔다.
        /// </summary>
        public static Sprite LoadTitle(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Title/" + name + ".png");
        }
    }
}
