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
        private const string ArtFolder = "Assets/SlotHero/Art";

        /// 그림이 있는 원래 자리. 유니티 프로젝트 밖이다.
        public const string SourceRoot = @"C:\Users\wonja\Downloads\slot hero\코드";

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

            // 전투 화면 그림. 피그마 `Jackpot 아트웍 (복사)` 의 전투 화면에서 뽑은 것과 코덱스가 그린 피해 숫자다(2026년 10월 26일).
            // 피해 숫자는 하위 폴더에 원본과 함께 있어 그 폴더도 같은 곳으로 옮긴다. 원본 폴더는 옮기지 않는다.
            CopyFolder(Path.Combine(SourceRoot, @"Combat\Art"), ArtFolder + "/Combat");
            CopyFolder(Path.Combine(SourceRoot, @"Combat\Art\피해숫자"), ArtFolder + "/Combat");

            // 타이틀 그림은 아직 없다. 배경 요청 기획서의 "메인 화면 배경"을 발주해 둔 상태다.
            // 폴더가 없으면 조용히 넘어가므로 그림이 오면 넣기만 하면 붙는다.
            CopyFolder(Path.Combine(SourceRoot, @"Title\Art"), ArtFolder + "/Title");

            // 이벤트 삽화. 파일 이름이 곧 삽화 식별자다. `이벤트식별자_화면식별자.png`.
            CopyFolder(Path.Combine(SourceRoot, @"Events\Art"), ArtFolder + "/Events");

            // 문양 임시 그림. 저장소 그림을 받기 전까지 쓰려고 코덱스로 만들었다(2026년 10월 19일 원재).
            // 저장소 그림을 아래에서 같은 이름으로 다시 덮으므로 저장소를 받으면 그쪽이 쓰인다.
            CopyFolder(Path.Combine(SourceRoot, @"Flow\Art\Symbols"), ArtFolder + "/Symbols");

            // 문양 그림은 저장소에 있다. 받아 두었으면 그것도 가져온다.
            // 2026년 10월 25일 저장소 원본 코드를 폐기하면서 `Assets/SymbolImage` 를 `Art/Symbols` 로 옮겨 두었다.
            // 받아 둔 원본이 없으면 복사만 건너뛰고 옮겨 둔 그림으로 잇는다.
            CopyFolder(Path.Combine(RepoRoot, @"Assets\SymbolImage"), ArtFolder + "/Symbols");

            AssetDatabase.Refresh();

            MakeSprites(ArtFolder + "/Map", true);
            MakeSprites(ArtFolder + "/CurrentBuild");
            MakeSprites(ArtFolder + "/TopBar");
            MakeSprites(ArtFolder + "/Items");
            MakeSprites(ArtFolder + "/Sanctum");
            MakeSprites(ArtFolder + "/Hud");

            // 원판 그림(1254)과 피해 숫자(256)를 절반 넘게 줄여 그리므로 밉맵을 만든다.
            MakeSprites(ArtFolder + "/Combat", true);
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
            MakeSprites(folder, false);
        }

        /// <summary>
        /// 가져온 그림을 스프라이트로 바꾼다. mipmaps 를 켜면 작게 줄여 그릴 때 계단이 지지 않게 밉맵을 만든다.
        /// 지도 노드 아이콘은 256 그림을 지도에서 70, 상단 표시줄에서 50 으로 줄여 그리므로 켠다(2026년 10월 25일).
        /// </summary>
        private static void MakeSprites(string folder, bool mipmaps)
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
                importer.mipmapEnabled = mipmaps;
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

                // 안쪽 무늬는 `<이름>_무늬.png` 다. 없으면 비워 두어 얹지 않는다(2026년 10월 25일 원재).
                entry.DetailIcon = LoadSprite(ArtFolder + "/Map/" + fileName + "_무늬.png");
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

                // 문양 칸의 태그 테두리. `태그테두리_<이름>_우상.png`, `_좌하.png` 다(2026년 10월 19일 원재).
                string tagName = fileName.Substring("태그_".Length);
                entry.CornerTopRight = LoadSprite(ArtFolder + "/CurrentBuild/태그테두리_" + tagName + "_우상.png");
                entry.CornerBottomLeft = LoadSprite(ArtFolder + "/CurrentBuild/태그테두리_" + tagName + "_좌하.png");

                visual.TagVisuals[i] = entry;
                linked++;
            }

            // 문양 뒤에 까는 흑요석 판. 행상 진열과 같은 `문양_바닥` 이다(2026년 10월 20일 원재).
            // 예전에는 금빛 금과 균열이 조금씩 다른 넉 장이었다. 2026년 10월 25일 원재가 가장 깔끔한 한 장으로 모으라고 해
            // 금빛 금이 없는 옛 `문양_바닥_c` 를 `문양_바닥` 으로 두고 나머지는 `Flow/Art/이전 그림 20261025` 로 옮겼다.
            string[] plateNames = { "문양_바닥" };
            List<Sprite> plates = new List<Sprite>();
            for (int i = 0; i < plateNames.Length; i++)
            {
                Sprite plate = LoadItemArt(plateNames[i]);
                if (plate != null)
                {
                    plates.Add(plate);
                }
            }

            visual.SymbolPlates = plates.ToArray();
            Debug.Log("문양 판 그림 " + plates.Count + "장을 물렸다.");

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

        /// <summary>문양 그림 하나. 이름은 문양 식별자와 같다(`Whale`, `Raven` 등).</summary>
        public static Sprite LoadSymbol(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Symbols/" + name + ".png");
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
        /// 전투 화면 그림 하나. 릴 틀, 유물 칸, 원판, 체력 틀, 주인공 초상, 피해 숫자다.
        /// 이름은 `코드/Combat/Art` 의 파일 이름이다. 무엇을 어디서 가져왔는지는 `Combat/README.md` 에 있다.
        /// </summary>
        public static Sprite LoadCombat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + "/Combat/" + name + ".png");
        }

        /// <summary>전투 효과음 하나. Art/Combat 의 wav 다. 레이저 발사음(LaserSound)은 Unity AI 로 만들었다.</summary>
        public static AudioClip LoadCombatSound(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(ArtFolder + "/Combat/" + name + ".wav");
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
