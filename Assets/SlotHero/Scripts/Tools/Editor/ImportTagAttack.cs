using System.Collections.Generic;
using System.IO;
using SlotHero.Combat;
using SlotHero.TagAttack;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 태그별 공격 이펙트와 소리를 게임에 쓸 수 있게 만들어 설정에 물린다.
    /// 메뉴 Slot Hero / 태그 공격 이펙트 가져오기 에서 실행한다. 한 바퀴 다 돌리기가 소리 가져오기 뒤에 부른다.
    ///
    /// 원재가 확인용 씬(`Scenes/태그 공격 확인`)에서 고른 것이다(2026년 11월 1일). 고른 내용은 `효과음과 이펙트 목록.md` 7장에 있다.
    ///
    /// 하는 일은 셋이다.
    /// 1. 소리와 그림: `코드/TagAttack/Art` 의 구운 소리(`Sound/태그공격_*.wav`)와 목성 뿌리 그림을 `Assets/SlotHero/Art/TagAttack` 으로 옮긴다
    /// 2. 이펙트: 받아 둔 묶음(Assets 맨 위의 Vefects, VFX Fantasy DEMO)에서 고른 파티클을 태그마다 프리팹 하나로 묶는다.
    ///    재질, 그림, 셰이더를 `Assets/SlotHero/Art/TagAttack` 아래로 복사해 묶음을 지워도 그대로 돈다.
    ///    Vefects 셰이더는 2D 렌더러가 그리는 단계가 없어 단계 이름을 `Universal2D` 로 바꾼 사본을 쓴다
    /// 3. 설정(`TagAttackFxConfig`)의 태그마다 프리팹, 소리, 그림을 물린다
    ///
    /// 묶음이 없는 컴퓨터에서는 2를 건너뛰고 이미 만든 프리팹을 그대로 물린다.
    /// </summary>
    public static class ImportTagAttack
    {
        private const string Folder = "Assets/SlotHero/Art/TagAttack";
        private const string SoundFolder = Folder + "/Sound";
        private const string MaterialFolder = Folder + "/Materials";
        private const string TextureFolder = Folder + "/Textures";
        private const string ShaderFolder = Folder + "/Shaders";
        private const string MeshFolder = Folder + "/Meshes";
        private const string RootsSprite = Folder + "/목성_뿌리.png";

        private const string Vefects = "Assets/Vefects/Flipbook VFX Bundle Lite URP/Flipbook VFX URP/_ Lite/Prefabs/";
        private const string Fantasy = "Assets/VFXFantasyPackDEMO/Content/URP/Prefabs/";

        private static readonly Color NoTint = new Color(1f, 1f, 1f, 0f);

        /// 이펙트 한 조각. 자리는 적 가운데 기준 픽셀이다(1 단위 = 100 픽셀).
        private struct Part
        {
            public string Path;
            public Color Tint;
            public float Scale;
            public bool FlipX;
            public bool FlipY;
            public float Speed;
            public Vector2 Offset;
            public float Angle;
            public float Delay;
            public int Order;
        }

        private static Part P(string path, Color tint, float scale, float speed, Vector2 offset)
        {
            Part part = new Part();
            part.Path = path;
            part.Tint = tint;
            part.Scale = scale;
            part.Speed = speed;
            part.Offset = offset;
            return part;
        }

        /// <summary>
        /// 태그마다 고른 이펙트. 확인용 씬의 안을 원재가 고치라고 한 대로 옮겼다(2026년 11월 1일).
        /// 목성은 그림 이펙트(뿌리)만 쓴다.
        /// </summary>
        private static Dictionary<SymbolTagType, Part[]> Parts()
        {
            Color life = new Color(0.45f, 1f, 0.45f, 1f);
            Color yellowDust = new Color(0.86f, 0.7f, 0.42f, 1f);
            Color air = new Color(0.75f, 0.95f, 1f, 1f);
            Color death = new Color(0.35f, 0.12f, 0.45f, 1f);

            // 수성. 물 튀김을 위아래로 뒤집어 머리 위에서 쏟아지게 한다. 확인용 씬보다 33% 작게(2.2 → 1.47).
            Part mercury = P(Vefects + "Lite_VFX_Water_Ground_Splash_01.prefab", NoTint, 1.47f, 1f, new Vector2(0f, -134f));
            mercury.FlipY = true;

            // 화성. 같은 불꽃 셋. 앞에 하나, 뒤에 조금 높게 좌우로 둘. 1.5배, 두 배 느리게.
            Part marsFront = P(Vefects + "Lite_VFX_Flame_Burst_01.prefab", NoTint, 1.5f, 0.5f, new Vector2(0f, -160f));
            marsFront.Order = 1;
            Part marsLeft = P(Vefects + "Lite_VFX_Flame_Burst_01.prefab", NoTint, 1.5f, 0.5f, new Vector2(-120f, -110f));
            Part marsRight = P(Vefects + "Lite_VFX_Flame_Burst_01.prefab", NoTint, 1.5f, 0.5f, new Vector2(120f, -110f));

            // 토성. 흙먼지를 원판 쪽에서 왼쪽으로 눕혀 가운데 두 줄로 몰아친다. 1초 뒤는 설정에서 자른다.
            Part saturnA = P(Vefects + "Lite_VFX_Dust_Directional_01.prefab", yellowDust, 2f, 0.6f, new Vector2(300f, -40f));
            saturnA.Angle = 90f;
            Part saturnB = P(Vefects + "Lite_VFX_Dust_Directional_01.prefab", yellowDust, 2f, 0.6f, new Vector2(260f, 45f));
            saturnB.Angle = 90f;
            saturnB.Delay = 0.06f;

            // 해왕성. 확인용 씬 안 3(2026년 11월 1일 원재). 큰 물 튀김 넷이 원판 쪽에서 차례로 덮치고 그 위로 물살 줄기 셋이 지난다.
            List<Part> neptune = new List<Part>();
            for (int i = 0; i < 4; i++)
            {
                Part splash = P(Vefects + "Lite_VFX_Water_Ground_Splash_01.prefab", new Color(0.3f, 0.5f, 1f, 1f), 2.2f, 0.8f,
                    new Vector2(260f - 150f * i, -60f + 15f * i));
                splash.FlipX = true;
                splash.Delay = 0.07f * i;
                neptune.Add(splash);
            }

            for (int i = 0; i < 3; i++)
            {
                Part rush = P(Fantasy + "Projectile/FX_Projectile_Blue.prefab", NoTint, 1.6f, 1f, new Vector2(300f, -80f + 80f * i));
                rush.Delay = 0.04f * i;
                neptune.Add(rush);
            }

            // 명왕성. 보라 구름 1.5배와 보라 줄기. 둘 다 두 배 느리게 해 연출을 길게 늘인다.
            return new Dictionary<SymbolTagType, Part[]>
            {
                { SymbolTagType.Mercury, new[] { mercury } },
                { SymbolTagType.Venus, new[] { P(Vefects + "Lite_VFX_Impact_01.prefab", NoTint, 3f, 1f, Vector2.zero) } },
                { SymbolTagType.Earth, new[] { P(Fantasy + "UI/FX_NotificationPulse_White.prefab", life, 1f, 1f, Vector2.zero) } },
                { SymbolTagType.Mars, new[] { marsLeft, marsRight, marsFront } },
                { SymbolTagType.Saturn, new[] { saturnA, saturnB } },
                { SymbolTagType.Uranus, new[] { P(Fantasy + "Slash/FX_SlashLight_White.prefab", air, 1f, 1f, Vector2.zero) } },
                { SymbolTagType.Neptune, neptune.ToArray() },
                {
                    SymbolTagType.Pluto, new[]
                    {
                        P(Vefects + "Lite_VFX_Blood_Cloud_Blur_01.prefab", death, 1.5f, 0.5f, Vector2.zero),
                        P(Fantasy + "Cast/FX_Shoot_White.prefab", death, 1f, 0.5f, Vector2.zero),
                    }
                },
            };
        }

        /// 태그의 파일 이름 꼬리. 소리는 `태그공격_수성.wav` 처럼 이 이름으로 굽는다(`코드/TagAttack/소리 굽기.py`).
        private static readonly Dictionary<SymbolTagType, string> Names = new Dictionary<SymbolTagType, string>
        {
            { SymbolTagType.Mercury, "수성" },
            { SymbolTagType.Venus, "금성" },
            { SymbolTagType.Earth, "지구" },
            { SymbolTagType.Mars, "화성" },
            { SymbolTagType.Jupiter, "목성" },
            { SymbolTagType.Saturn, "토성" },
            { SymbolTagType.Uranus, "천왕성" },
            { SymbolTagType.Neptune, "해왕성" },
            { SymbolTagType.Pluto, "명왕성" },
        };

        [MenuItem("Slot Hero/태그 공격 이펙트 가져오기")]
        public static void Import()
        {
            UiSceneBuilder.EnsureFolder(SoundFolder);
            CopyFiles(Path.Combine(ImportArt.SourceRoot, @"TagAttack\Art\Sound"), SoundFolder, "*.wav");
            CopyFiles(Path.Combine(ImportArt.SourceRoot, @"TagAttack\Art"), Folder, "*.png");
            AssetDatabase.Refresh();
            MakeSprite(RootsSprite);

            Dictionary<SymbolTagType, Part[]> parts = Parts();
            foreach (KeyValuePair<SymbolTagType, Part[]> pair in parts)
            {
                BuildEffect(Names[pair.Key], pair.Value);
            }

            TagAttackFxConfig config = UiSceneBuilder.LoadConfig<TagAttackFxConfig>("TagAttack", "TagAttackFxConfig");
            if (config == null)
            {
                Debug.LogWarning("태그 공격 설정이 없다. 먼저 `설정 에셋 한 번에 만들기` 를 누른다.");
                return;
            }

            int linked = 0;
            foreach (KeyValuePair<SymbolTagType, string> pair in Names)
            {
                TagAttackFxEntry entry = config.GetEntry(pair.Key);
                if (entry == null)
                {
                    entry = new TagAttackFxEntry(pair.Key);
                    config.Entries.Add(entry);
                }

                entry.Effect = parts.ContainsKey(pair.Key)
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/태그공격_" + pair.Value + ".prefab")
                    : null;
                entry.Sound = AssetDatabase.LoadAssetAtPath<AudioClip>(SoundFolder + "/태그공격_" + pair.Value + ".wav");
                entry.Sprite = pair.Key == SymbolTagType.Jupiter ? AssetDatabase.LoadAssetAtPath<Sprite>(RootsSprite) : null;

                if (entry.Sound != null)
                {
                    linked++;
                }
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("태그 공격 소리 " + linked + "개와 이펙트를 설정에 물렸다.");
        }

        private static void CopyFiles(string source, string target, string pattern)
        {
            if (!Directory.Exists(source))
            {
                Debug.Log("태그 공격 폴더가 없어 건너뛴다: " + source);
                return;
            }

            UiSceneBuilder.EnsureFolder(target);
            string[] files = Directory.GetFiles(source, pattern);
            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i], Path.Combine(target, Path.GetFileName(files[i])), true);
            }
        }

        private static void MakeSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// 태그의 이펙트 조각들을 프리팹 하나로 묶는다. 조각의 자리, 크기, 뒤집기, 빠르기, 늦음을 파티클에 굳혀 넣는다.
        /// 원본 묶음이 없으면 이미 만든 프리팹을 그대로 둔다.
        /// </summary>
        private static void BuildEffect(string name, Part[] parts)
        {
            string path = Folder + "/태그공격_" + name + ".prefab";

            for (int i = 0; i < parts.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(parts[i].Path) == null)
                {
                    Debug.Log("받아 둔 묶음이 없어 " + name + " 이펙트를 다시 만들지 않는다: " + parts[i].Path);
                    return;
                }
            }

            UiSceneBuilder.EnsureFolder(MaterialFolder);
            UiSceneBuilder.EnsureFolder(TextureFolder);
            UiSceneBuilder.EnsureFolder(ShaderFolder);

            GameObject root = new GameObject("태그공격_" + name);

            for (int i = 0; i < parts.Length; i++)
            {
                Part part = parts[i];
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(part.Path);
                GameObject piece = (GameObject)PrefabUtility.InstantiatePrefab(source);
                PrefabUtility.UnpackPrefabInstance(piece, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                piece.transform.SetParent(root.transform, false);
                piece.transform.localPosition = new Vector3(part.Offset.x / 100f, part.Offset.y / 100f, 0f);
                piece.transform.localRotation = Quaternion.Euler(0f, 0f, part.Angle);
                piece.transform.localScale = new Vector3(
                    part.FlipX ? -part.Scale : part.Scale, part.FlipY ? -part.Scale : part.Scale, part.Scale);

                Settle(piece, part);
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            ReportOutsideDependencies(path);
        }

        /// <summary>조각의 파티클을 한 번만 돌게 하고, 빠르기와 늦음과 물들임을 굳히고, 재질을 우리 폴더의 사본으로 바꾼다.</summary>
        private static void Settle(GameObject piece, Part part)
        {
            // 묶음의 이펙트 프리팹 가운데 자기 소리를 내는 소리 칸이 든 것이 있다(Vefects `Impact_01`).
            // 태그 소리와 겹쳐 다른 효과음이 섞여 들리므로 뺀다(2026년 11월 1일 원재가 금성에서 들었다).
            AudioSource[] sources = piece.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                Object.DestroyImmediate(sources[i]);
            }

            ParticleSystem[] systems = piece.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                main.loop = false;
                main.prewarm = false;
                main.playOnAwake = true;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.simulationSpeed = main.simulationSpeed * part.Speed;

                if (part.Delay > 0f)
                {
                    main.startDelay = main.startDelay.constantMax + part.Delay;
                }

                ParticleSystemRenderer renderer = systems[i].GetComponent<ParticleSystemRenderer>();
                if (renderer == null)
                {
                    continue;
                }

                renderer.sortingOrder = renderer.sortingOrder + part.Order;

                // 바닥에 눕혀 그리는 파티클은 정면 카메라에서 납작해진다. 카메라를 보게 세운다.
                if (renderer.renderMode == ParticleSystemRenderMode.HorizontalBillboard)
                {
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                }

                // 늘 카메라를 보는 파티클은 돌려도 그림이 돌지 않는다. 돌릴 때는 오브젝트 방향을 따르게 한다.
                if (!Mathf.Approximately(part.Angle, 0f) && renderer.alignment == ParticleSystemRenderSpace.View)
                {
                    renderer.alignment = ParticleSystemRenderSpace.Local;
                }

                if (part.FlipX || part.FlipY)
                {
                    renderer.flip = new Vector3(part.FlipX ? 1f : renderer.flip.x, part.FlipY ? 1f : renderer.flip.y, renderer.flip.z);
                }

                bool vefects = renderer.sharedMaterial != null && renderer.sharedMaterial.shader.name.StartsWith("Vefects/");

                // Vefects 셰이더는 파티클 색을 쓰지 않고 재질 색으로 칠한다. 그쪽은 재질 사본에서 물들인다.
                if (part.Tint.a > 0f && !vefects)
                {
                    main.startColor = Multiply(main.startColor, part.Tint);
                }

                if (renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial = CopyMaterial(renderer.sharedMaterial, vefects ? part.Tint : NoTint);
                }

                if (renderer.trailMaterial != null)
                {
                    renderer.trailMaterial = CopyMaterial(renderer.trailMaterial, NoTint);
                }

                CopyMeshes(renderer);
                CopySheetSprites(systems[i]);
                CopyShapeArt(systems[i]);
            }
        }

        /// <summary>파티클이 메시 모양으로 그릴 때 쓰는 메시를 우리 폴더로 복사해 바꿔 끼운다(수성 물 튀김의 판).</summary>
        private static void CopyMeshes(ParticleSystemRenderer renderer)
        {
            Mesh[] meshes = new Mesh[renderer.meshCount];
            renderer.GetMeshes(meshes);
            bool changed = false;

            for (int i = 0; i < meshes.Length; i++)
            {
                Object copy = CopySubAsset(meshes[i], MeshFolder);
                if (copy is Mesh && copy != meshes[i])
                {
                    meshes[i] = (Mesh)copy;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.SetMeshes(meshes);
            }
        }

        /// <summary>연속 그림을 스프라이트로 돌리는 파티클(VFX Fantasy)의 스프라이트를 우리 폴더로 복사해 바꿔 끼운다.</summary>
        private static void CopySheetSprites(ParticleSystem system)
        {
            ParticleSystem.TextureSheetAnimationModule sheet = system.textureSheetAnimation;
            // 꺼진 칸에도 스프라이트가 남아 있으면 묶음 파일을 붙든다. 켜짐과 상관없이 복사한다.
            if (sheet.mode != ParticleSystemAnimationMode.Sprites)
            {
                return;
            }

            for (int i = 0; i < sheet.spriteCount; i++)
            {
                Object copy = CopySubAsset(sheet.GetSprite(i), TextureFolder);
                if (copy is Sprite)
                {
                    sheet.SetSprite(i, (Sprite)copy);
                }
            }
        }

        /// <summary>파티클을 뿜는 모양에 쓰는 그림(스프라이트, 텍스처)을 우리 폴더로 복사해 바꿔 끼운다.</summary>
        private static void CopyShapeArt(ParticleSystem system)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            if (shape.sprite != null)
            {
                Object copy = CopySubAsset(shape.sprite, TextureFolder);
                if (copy is Sprite)
                {
                    shape.sprite = (Sprite)copy;
                }
            }

            if (shape.texture != null)
            {
                Object copy = CopySubAsset(shape.texture, TextureFolder);
                if (copy is Texture2D)
                {
                    shape.texture = (Texture2D)copy;
                }
            }
        }

        /// <summary>
        /// 그 오브젝트가 든 파일(그림, fbx)을 우리 폴더로 복사하고, 복사본 안에서 같은 이름의 오브젝트를 찾아 돌려준다.
        /// 이미 우리 폴더나 유니티 패키지에 있으면 그대로 돌려준다.
        /// </summary>
        private static Object CopySubAsset(Object asset, string folder)
        {
            if (asset == null)
            {
                return null;
            }

            string from = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(from) || !from.StartsWith("Assets/") || from.StartsWith("Assets/SlotHero/"))
            {
                return asset;
            }

            UiSceneBuilder.EnsureFolder(folder);
            string to = folder + "/" + Path.GetFileName(from);
            if (AssetDatabase.LoadMainAssetAtPath(to) == null)
            {
                AssetDatabase.CopyAsset(from, to);
            }

            Object[] all = AssetDatabase.LoadAllAssetsAtPath(to);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == asset.name && all[i].GetType() == asset.GetType())
                {
                    return all[i];
                }
            }

            return asset;
        }

        /// <summary>파티클 시작 색에 태그 색을 곱한다. 흰 이펙트는 태그 색 그대로가 된다.</summary>
        private static ParticleSystem.MinMaxGradient Multiply(ParticleSystem.MinMaxGradient source, Color tint)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(source.color * tint);
                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(source.colorMin * tint, source.colorMax * tint);
                default:
                    return new ParticleSystem.MinMaxGradient(tint);
            }
        }

        /// <summary>재질을 우리 폴더로 복사한다. 그림도 복사해 바꿔 끼우고, Vefects 셰이더는 2D 렌더러용 사본으로 바꾼다.</summary>
        private static Material CopyMaterial(Material source, Color tint)
        {
            string suffix = tint.a > 0f ? "_" + ColorUtility.ToHtmlStringRGB(tint) : string.Empty;
            string path = MaterialFolder + "/" + source.name + suffix + ".mat";

            Material copy = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool created = copy == null;
            if (created)
            {
                copy = new Material(source);
            }
            else
            {
                copy.shader = source.shader;
                copy.CopyPropertiesFromMaterial(source);
            }

            if (source.shader.name.StartsWith("Vefects/"))
            {
                copy.shader = Shader2D(source.shader);
            }

            string[] textures = copy.GetTexturePropertyNames();
            for (int i = 0; i < textures.Length; i++)
            {
                Texture texture = copy.GetTexture(textures[i]);
                if (texture != null)
                {
                    copy.SetTexture(textures[i], CopyTexture(texture));
                }
            }

            if (tint.a > 0f)
            {
                Tint(copy, tint);
            }

            if (created)
            {
                AssetDatabase.CreateAsset(copy, path);
            }
            else
            {
                EditorUtility.SetDirty(copy);
            }

            return copy;
        }

        private static Texture CopyTexture(Texture texture)
        {
            string from = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(from) || !from.StartsWith("Assets/") || from.StartsWith("Assets/SlotHero/"))
            {
                return texture;
            }

            string to = TextureFolder + "/" + Path.GetFileName(from);
            if (AssetDatabase.LoadAssetAtPath<Texture>(to) == null)
            {
                AssetDatabase.CopyAsset(from, to);
            }

            Texture copy = AssetDatabase.LoadAssetAtPath<Texture>(to);
            return copy != null ? copy : texture;
        }

        /// <summary>
        /// Vefects 셰이더 사본. 이름과 그리는 단계 이름만 바꾼다.
        /// 2D 렌더러는 `Universal2D` 와 단계 이름이 없는 단계만 그리고, Vefects Lite 셰이더는 `UniversalForwardOnly` 단계만 있다.
        /// </summary>
        private static Shader Shader2D(Shader source)
        {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            string path = ShaderFolder + "/" + Path.GetFileNameWithoutExtension(sourcePath) + "_2D.shader";

            string text = File.ReadAllText(sourcePath);
            text = new System.Text.RegularExpressions.Regex("Shader \"[^\"]+\"")
                .Replace(text, "Shader \"SlotHero/TagAttack/" + source.name.Replace("Vefects/", string.Empty) + "\"", 1);
            text = text.Replace("\"LightMode\"=\"UniversalForwardOnly\"", "\"LightMode\"=\"Universal2D\"");

            string full = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!File.Exists(full) || File.ReadAllText(full) != text)
            {
                File.WriteAllText(full, text);
                AssetDatabase.ImportAsset(path);
            }

            return AssetDatabase.LoadAssetAtPath<Shader>(path);
        }

        /// Vefects 재질이 그림의 빨강 · 초록 · 파랑 칸마다 칠하는 색 칸과 테두리 색 칸.
        private static readonly string[] VefectsColors =
        {
            "_R", "_G", "_B", "_Outline", "_Color01", "_Color02", "_Color03", "_Color04",
        };

        /// <summary>재질 색 칸을 태그 색으로 바꾼다. 원래 색의 밝기는 남겨 무늬가 그대로 보이게 한다.</summary>
        private static void Tint(Material material, Color tint)
        {
            for (int i = 0; i < VefectsColors.Length; i++)
            {
                if (!material.HasProperty(VefectsColors[i]))
                {
                    continue;
                }

                Color original = material.GetColor(VefectsColors[i]);
                float bright = Mathf.Max(original.r, Mathf.Max(original.g, original.b));
                material.SetColor(VefectsColors[i], new Color(tint.r * bright, tint.g * bright, tint.b * bright, original.a));
            }
        }

        /// <summary>프리팹이 우리 폴더와 유니티 패키지 밖의 파일을 붙들고 있으면 알린다. 묶음을 지우면 그 파일이 빠진다.</summary>
        private static void ReportOutsideDependencies(string path)
        {
            string[] dependencies = AssetDatabase.GetDependencies(path, true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i];
                if (dependency.StartsWith("Assets/SlotHero/") || dependency.StartsWith("Packages/"))
                {
                    continue;
                }

                Debug.LogWarning(Path.GetFileName(path) + " 이 묶음 안의 파일을 쓴다: " + dependency);
            }
        }
    }
}
