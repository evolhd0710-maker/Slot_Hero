using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.Tools;
using SlotHero.Ui;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 태그별 공격 이펙트와 소리를 눌러 보는 확인용 씬. 태그마다 씬 하나다.
    /// 원재가 2026년 11월 1일 태그마다 연상되는 것을 정했다.
    /// 수성 물, 금성 금속, 지구 생명, 화성 불, 목성 나무, 토성 모래먼지, 천왕성 바람, 해왕성 바다, 명왕성 검은색과 죽음.
    ///
    /// 첫 후보를 원재가 씬에서 보고 고른 뒤(2026년 11월 1일) 고칠 것을 받아 안을 다시 짰다. 태그마다 안이 하나에서 셋이다.
    /// 후보는 원재가 받아 둔 묶음(Assets 맨 위)에서 골랐다. 확인받기 전이라 게임에는 붙이지 않는다.
    /// Vefects 이펙트는 셰이더에 2D 렌더러가 그리는 단계가 없어, 단계 이름만 바꾼 셰이더 사본과
    /// 그 셰이더를 쓰는 재질과 프리팹 사본을 Assets/SlotHero/Art/TagFx 에 만들어 쓴다. 원본 묶음은 건드리지 않는다.
    /// </summary>
    public static partial class BuildPreviewScenes
    {
        /// 태그 공격 확인용 씬을 두는 폴더. 확인용 씬 폴더 아래다.
        private const string TagAttackSceneFolder = "태그 공격 확인";

        /// 2D 렌더러에서 그리게 고친 이펙트 사본을 두는 곳.
        private const string TagFxFolder = "Assets/SlotHero/Art/TagFx";

        private const string VefectsPrefabs = "Assets/Vefects/Flipbook VFX Bundle Lite URP/Flipbook VFX URP/_ Lite/Prefabs/";
        private const string FantasyPrefabs = "Assets/VFXFantasyPackDEMO/Content/URP/Prefabs/";

        private const string WaterSplash = VefectsPrefabs + "Lite_VFX_Water_Ground_Splash_01.prefab";
        private const string MetalImpact = VefectsPrefabs + "Lite_VFX_Impact_01.prefab";
        private const string FlameBurst = VefectsPrefabs + "Lite_VFX_Flame_Burst_01.prefab";
        private const string DustBig = VefectsPrefabs + "Lite_VFX_Dust_Directional_01.prefab";
        private const string DustSmall = VefectsPrefabs + "Lite_VFX_Dust_Directional_Small_01.prefab";
        private const string DarkCloud = VefectsPrefabs + "Lite_VFX_Blood_Cloud_Blur_01.prefab";
        private const string LifePulse = FantasyPrefabs + "UI/FX_NotificationPulse_White.prefab";
        private const string Slash = FantasyPrefabs + "Slash/FX_SlashLight_White.prefab";
        private const string Shoot = FantasyPrefabs + "Cast/FX_Shoot_White.prefab";

        /// 해왕성 파도 시도 6. 2026년 11월 8일 원재가 골라 게임에 붙였다. 확인용 씬도 게임의 프리팹을 그대로 쓴다.
        /// Unity AI 가 만든 파도 그림을 평면 색 셋으로 단순화했다. 반투명한 파도(가로 7.56, 세로 11)가 0.75초 동안 몬스터 오른쪽에서 왼쪽으로 지나가며 덮치고,
        /// 거품은 1.5초까지 남는다.
        private const string NeptuneWave = "Assets/SlotHero/Art/TagAttack/태그공격_해왕성.prefab";

        /// 목성 고목 뿌리 묶기. 2026년 11월 9일 원재가 골라 게임에 붙였다. 확인용 씬도 게임의 프리팹을 그대로 쓴다.
        /// 내가 평면 색으로 그린 고목 뿌리 일곱이 몬스터 가운데(-1.62 ~ 1.62)에서 0 ~ 0.05초 안에 거의 동시에 솟고,
        /// 0.58초에 밑동을 축으로 가로 0.82 배로 조였다가 0.9 배로 돌아오고, 1.2초부터 땅으로 거둬져 1.5초에 끝난다.
        private const string JupiterRootsTree = "Assets/SlotHero/Art/TagAttack/태그공격_목성.prefab";

        /// 그림으로 뽑을 때 파티클이 시작한 뒤 미리 돌리는 시간. 수명이 0.1 ~ 0.5초로 짧은 것이 많아 터진 직후를 찍는다.
        private const float TagAttackCaptureSeconds = 0.12f;

        /// 태그 공격 확인용 씬을 그림으로 뽑는 폴더. 프로젝트 폴더 기준이다.
        private const string TagAttackCaptureFolder = "화면 확인/태그 공격";

        private static readonly Color NoTint = new Color(1f, 1f, 1f, 0f);

        /// 이펙트 하나. 같은 것을 늘어놓을 때는 Row 로 개수와 간격을 준다.
        private struct FxSpec
        {
            public string Path;
            public Color Tint;
            public float Scale;
            public bool FlipX;
            public bool FlipY;
            public float Speed;
            public Vector2 Offset;
            public float Angle;
            public int Count;
            public Vector2 Step;
            public float StepAngle;
            public float StepDelay;

            /// 위아래로 뒤집는다.
            public FxSpec UpsideDown()
            {
                FxSpec copy = this;
                copy.FlipY = true;
                return copy;
            }

            /// 이펙트를 돌린다. 0 이면 위, 90 이면 왼쪽을 향한다.
            public FxSpec Turn(float angle)
            {
                FxSpec copy = this;
                copy.Angle = angle;
                return copy;
            }

            public FxSpec Row(int count, Vector2 offset, Vector2 step, float stepAngle, float stepDelay)
            {
                FxSpec copy = this;
                copy.Count = count;
                copy.Offset = offset;
                copy.Step = step;
                copy.StepAngle = stepAngle;
                copy.StepDelay = stepDelay;
                return copy;
            }
        }

        /// 소리 한 구간. 길이 0 은 파일 끝까지(1.5초가 넘으면 1.5초에서 줄인다).
        private struct SoundSpec
        {
            public string File;
            public float Start;
            public float Seconds;
            public float Volume;
            public float Delay;
            public float Fade;

            /// 끝을 줄이는 시간을 정한다. 바로 뒤에 다른 소리가 붙은 파일은 짧게 줄인다.
            public SoundSpec FadeOut(float seconds)
            {
                SoundSpec copy = this;
                copy.Fade = seconds;
                return copy;
            }
        }

        /// 그림 한 장으로 도는 이펙트. 그림이 아직 없으면 Fallback 의 파티클을 쓴다.
        private struct SpriteSpec
        {
            public string Path;
            public Color Color;
            public Vector2 Size;
            public Vector2 Pivot;
            public Vector2 From;
            public Vector2 To;
            public Vector2 FromScale;
            public Vector2 ToScale;
            public float Squeeze;
            public float In;
            public float Hold;
            public float Out;
        }

        private struct PlanSpec
        {
            public string Name;
            public FxSpec[] Effects;
            public SoundSpec[] Sounds;
            public SpriteSpec[] Sprites;

            /// 그림 이펙트를 붙인다. 그림이 있으면 파티클 대역(Effects)은 쓰지 않는다.
            public PlanSpec WithSprite(SpriteSpec sprite)
            {
                PlanSpec copy = this;
                copy.Sprites = new[] { sprite };
                return copy;
            }
        }

        private struct TagAttackCandidates
        {
            public SymbolTagType Tag;
            public string Look;
            public PlanSpec[] Plans;
        }

        private static FxSpec Fx(string path, Color tint, float scale = 1f, bool flipX = false, float speed = 1f)
        {
            FxSpec fx = new FxSpec();
            fx.Path = path;
            fx.Tint = tint;
            fx.Scale = scale;
            fx.FlipX = flipX;
            fx.Speed = speed;
            fx.Count = 1;
            return fx;
        }

        private static SoundSpec Snd(string file, float start = 0f, float seconds = 0f, float volume = 1f, float delay = 0f)
        {
            SoundSpec sound = new SoundSpec();
            sound.File = file;
            sound.Start = start;
            sound.Seconds = seconds;
            sound.Volume = volume;
            sound.Delay = delay;
            return sound;
        }

        private static PlanSpec Plan(string name, FxSpec[] effects, params SoundSpec[] sounds)
        {
            PlanSpec plan = new PlanSpec();
            plan.Name = name;
            plan.Effects = effects;
            plan.Sounds = sounds;
            plan.Sprites = new SpriteSpec[0];
            return plan;
        }

        private static SpriteSpec SpriteFx(
            string path, Vector2 size, Vector2 pivot, Vector2 from, Vector2 to, Vector2 fromScale, Vector2 toScale,
            float squeeze, float inSeconds, float holdSeconds, float outSeconds)
        {
            SpriteSpec sprite = new SpriteSpec();
            sprite.Path = path;
            sprite.Color = Color.white;
            sprite.Size = size;
            sprite.Pivot = pivot;
            sprite.From = from;
            sprite.To = to;
            sprite.FromScale = fromScale;
            sprite.ToScale = toScale;
            sprite.Squeeze = squeeze;
            sprite.In = inSeconds;
            sprite.Hold = holdSeconds;
            sprite.Out = outSeconds;
            return sprite;
        }

        private static TagAttackCandidates Tag(SymbolTagType tag, string look, params PlanSpec[] plans)
        {
            TagAttackCandidates candidates = new TagAttackCandidates();
            candidates.Tag = tag;
            candidates.Look = look;
            candidates.Plans = plans;
            return candidates;
        }

        /// <summary>
        /// 태그마다 안. 2026년 11월 1일 원재가 고칠 것을 준 대로 짰다.
        /// 수성 뒤집기, 금성 두 번째 이펙트, 지구 첫 이펙트에 두 번째 소리 앞부분, 화성 1.5배 크게 두 배 느리게,
        /// 목성 묶고 조이는 느낌, 토성 황사 크기, 천왕성 더 크고 긴 바람 소리, 해왕성 파도, 명왕성 이펙트 둘 다와 두 번째 소리.
        /// </summary>
        private static TagAttackCandidates[] GetTagAttackCandidates()
        {
            Color life = new Color(0.45f, 1f, 0.45f, 1f);
            Color yellowDust = new Color(0.86f, 0.7f, 0.42f, 1f);
            Color air = new Color(0.75f, 0.95f, 1f, 1f);
            Color death = new Color(0.35f, 0.12f, 0.45f, 1f);

            // 수성. 물 튀김을 위아래로 뒤집어 머리 위에서 쏟아져 몸을 덮으며 아래로 퍼지게 한다.
            // 파티클 그림은 뒤집어도 기준점 위쪽에 그려져, 기준점을 몸 아래에 두어야 그림이 몸을 덮는다.
            FxSpec[] mercury = { Fx(WaterSplash, NoTint, 2.2f).UpsideDown().Row(1, new Vector2(0f, -200f), Vector2.zero, 0f, 0f) };
            FxSpec[] venus = { Fx(MetalImpact, NoTint, 3f) };
            FxSpec[] earth = { Fx(LifePulse, life) };

            // 화성. 불꽃의 밑이 아니라 가운데가 레이저가 닿는 자리에 오게 내린다.
            FxSpec[] mars = { Fx(FlameBurst, NoTint, 1.5f, false, 0.5f).Row(1, new Vector2(0f, -160f), Vector2.zero, 0f, 0f) };

            // 목성. 고목처럼 갈라지는 뿌리 일곱이 발밑에서 솟아 몬스터를 덮고 조인다(2026년 11월 9일 원재).
            FxSpec[] jupiterRootsTree = { Fx(JupiterRootsTree, NoTint) };

            // 토성. 흙먼지를 원판 쪽(오른쪽)에서 왼쪽으로 눕혀 몰아친다. 위아래 줄을 빼고 가운데 둘만 남겼다(다섯에서 둘).
            FxSpec[] saturn =
            {
                Fx(DustBig, yellowDust, 2f, false, 0.6f).Turn(90f).Row(2, new Vector2(300f, -40f), new Vector2(-40f, 85f), 0f, 0.06f),
            };

            FxSpec[] uranus = { Fx(Slash, air) };

            // 해왕성. 게임에 붙인 파도 시도 6(2026년 11월 8일 원재). 파도 시트를 한 장으로 넘기고 물보라와 거품을 얹은 파티클 프리팹이다.
            FxSpec[] neptuneWave = { Fx(NeptuneWave, NoTint) };

            FxSpec[] pluto = { Fx(DarkCloud, death, 1.5f), Fx(Shoot, death) };

            return new[]
            {
                Tag(SymbolTagType.Mercury, "물",
                    Plan("뒤집은 물 튀김 + AMP_Water_Impact_02", mercury, Snd("AMP_Water_Impact_02.wav")),
                    Plan("뒤집은 물 튀김 + Liquid_Hit_02", mercury, Snd("Vefects_SFX_Liquid_Hit_02.wav"))),
                Tag(SymbolTagType.Venus, "금속",
                    Plan("Impact_01 3배 + Metal Impact 7 첫 0.3초만", venus, Snd("Metal Impact 7.wav", 0f, 0.3f).FadeOut(0.08f))),
                Tag(SymbolTagType.Earth, "생명",
                    Plan("초록 맥동 + AMP_Buff_04 앞 0.7초", earth, Snd("AMP_Buff_04.wav", 0f, 0.7f))),
                Tag(SymbolTagType.Mars, "불",
                    Plan("불꽃 1.5배 · 두 배 느리게 + Fire_Burst_01", mars, Snd("Vefects_SFX_Fire_Burst_01.wav"))),
                Tag(SymbolTagType.Jupiter, "나무, 묶고 조이기",
                    // 소리는 게임과 같은 삐걱과 찢김이고, 찢김은 조이는 0.58초에 맞춰 0.5초 늦게 낸다.
                    Plan("가운데로 모인 고목 가지 + 나무 삐걱 · 찢김", jupiterRootsTree,
                        Snd("Gregor Quendel - Designed Plastic - Elements - Creaking 04.wav", 0.15f, 1.2f),
                        Snd("Gregor Quendel - Designed Plastic - Elements - Ripping 03.wav", 0f, 0.8f, 0.6f, 0.5f))),
                Tag(SymbolTagType.Saturn, "흙, 황사",
                    Plan("황사 둘 + 바람 · 흙 터짐", saturn,
                        Snd("Gregor Quendel - Designed Atmospheres - Wind - 001 - II.wav", 0.8f, 2f),
                        Snd("Vefects_SFX_Dust_Puff_01.wav"))),
                Tag(SymbolTagType.Uranus, "바람",
                    Plan("바람 베기 + Simple Swoosh_6 (크고 긴 휙)", uranus, Snd("Magic Spell_Simple Swoosh_6.wav"))),
                Tag(SymbolTagType.Neptune, "바다, 파도",
                    // 해일 소리는 점점 커지는 소리다. 가장 큰 부분(13.55초 ~ 14.4초)이 파도가 몬스터에 닿는 0.24초 무렵에 오게 13.3초부터 1.5초를 틀고,
                    // 마지막 0.4초를 줄여 거품이 사라지는 1.5초에 함께 끝낸다.
                    Plan("시도 6 · 파도 0.75초, 거품 1.5초 + 해일 절정 1.5초", neptuneWave, Snd("Tsunami hitting a Large City.wav", 13.3f, 1.5f).FadeOut(0.4f))),
                Tag(SymbolTagType.Pluto, "검은색, 죽음",
                    Plan("보라 구름 1.5배와 보라 줄기 + AMP_DeBuff_09", pluto, Snd("AMP_DeBuff_09.wav"))),
            };
        }

        [MenuItem("Slot Hero/확인용 씬 만들기/태그 공격 이펙트와 소리")]
        public static void BuildTagAttack()
        {
            CurrentBuildVisualConfig tags =
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig");
            CombatLayoutConfig layout = UiSceneBuilder.LoadConfig<CombatLayoutConfig>("Combat", "CombatLayoutConfig");

            if (tags == null || layout == null)
            {
                Debug.LogWarning("현재 빌드나 전투 설정 에셋이 없다. 먼저 `설정 에셋 한 번에 만들기` 를 누른다.");
                return;
            }

            UiSceneBuilder.EnsureFolder(UiSceneBuilder.SceneFolder + "/" + TagAttackSceneFolder);

            // 후보를 바꾸면 옛 사본이 남는다. 매번 지우고 다시 만든다.
            AssetDatabase.DeleteAsset(TagFxFolder);

            TagAttackCandidates[] all = GetTagAttackCandidates();
            Dictionary<string, GameObject> converted = new Dictionary<string, GameObject>();

            for (int i = 0; i < all.Length; i++)
            {
                BuildTagAttackScene(i + 1, all[i], tags, layout, converted);
            }

            Debug.Log("태그 공격 확인용 씬 " + all.Length + "개를 만들었다.");
        }

        private static void BuildTagAttackScene(
            int number,
            TagAttackCandidates candidates,
            CurrentBuildVisualConfig tags,
            CombatLayoutConfig layout,
            Dictionary<string, GameObject> converted)
        {
            BuildTagVisual tagVisual = tags.GetTagVisual(candidates.Tag);
            string tagName = string.IsNullOrEmpty(tagVisual.DisplayName) ? candidates.Tag.ToString() : tagVisual.DisplayName;
            Color tagColor = tagVisual.BarColor;

            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("TagAttackCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            RectTransform root = UiSceneBuilder.NewRect("TagAttack", canvas.transform);
            UiSceneBuilder.Stretch(root);

            if (UiSceneBuilder.NewBattleBackdrop("BehindScreen", root) == null)
            {
                UiSceneBuilder.NewStoneBackdrop("BehindScreen", root);
            }

            // 전투 화면과 같은 자리에 원판과 적을 둔다. 레이저가 같은 길이로 뻗는다.
            Image dial = UiSceneBuilder.NewFlatImage("Dial", root, Color.white);
            dial.sprite = ImportArt.LoadCombat("원판_테두리");
            dial.raycastTarget = false;
            PlaceCentered(dial.rectTransform, layout.DialPosition, new Vector2(layout.DialSize, layout.DialSize));

            Image enemy = UiSceneBuilder.NewFlatImage("Enemy", root, Color.white);
            enemy.sprite = ImportArt.LoadCombat("몬스터_덤");
            enemy.preserveAspect = true;
            enemy.raycastTarget = false;
            PlaceCentered(enemy.rectTransform, layout.EnemyArtPosition, layout.EnemyArtSize);

            Image laser = UiSceneBuilder.NewFlatImage("Laser", root, Color.white);
            laser.sprite = ImportArt.LoadCombat("LaserBeam");
            laser.raycastTarget = false;
            RectTransform laserRect = laser.rectTransform;
            laserRect.anchorMin = new Vector2(0f, 1f);
            laserRect.anchorMax = new Vector2(0f, 1f);
            laserRect.pivot = new Vector2(0f, 0.5f);
            laser.gameObject.SetActive(false);

            TMP_Text title = UiSceneBuilder.NewText(
                "Title", root, "태그 공격 확인 · " + tagName + " · " + candidates.Look,
                44f, Color.white, TextAlignmentOptions.TopLeft, true);
            UiSceneBuilder.PlaceTopLeft(title.rectTransform, new Vector2(60f, 40f), new Vector2(1800f, 64f));

            TMP_Text guide = UiSceneBuilder.NewText(
                "Guide", root,
                "플레이를 누른 뒤 버튼을 누른다. 레이저가 적에게 닿는 순간 이펙트가 터지고 소리가 난다.",
                24f, new Color(1f, 1f, 1f, 0.8f), TextAlignmentOptions.TopLeft);
            UiSceneBuilder.PlaceTopLeft(guide.rectTransform, new Vector2(60f, 112f), new Vector2(1800f, 40f));

            GameObject previewGo = new GameObject("TagAttackPreview");
            TagAttackPreview preview = previewGo.AddComponent<TagAttackPreview>();

            TagAttackPlan[] plans = new TagAttackPlan[candidates.Plans.Length];
            for (int i = 0; i < plans.Length; i++)
            {
                plans[i] = MakePlan(candidates.Plans[i], converted);
            }

            preview.SetPlans(plans);

            SerializedObject so = new SerializedObject(preview);
            so.FindProperty("_tagColor").colorValue = tagColor;
            so.FindProperty("_camera").objectReferenceValue = _camera;
            so.FindProperty("_laser").objectReferenceValue = laser;
            so.FindProperty("_dial").objectReferenceValue = dial.rectTransform;
            so.FindProperty("_enemy").objectReferenceValue = enemy.rectTransform;
            so.FindProperty("_laserThickness").floatValue = layout.LaserThickness;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(preview);

            // 안마다 공격, 이펙트만, 소리만 버튼 셋을 왼쪽에 늘어놓는다. 이펙트만과 소리만은 한 줄에 나란히 두어 안 여섯 개까지 화면에 들어가게 한다.
            float y = 180f;
            for (int i = 0; i < plans.Length; i++)
            {
                NewPreviewButton(root, "Attack" + (i + 1), "안 " + (i + 1) + " · " + plans[i].Name, ref y, preview.Attack, i, true, 60f);
                float row = y;
                NewPreviewButton(root, "Effects" + (i + 1), "    이펙트만", ref row, preview.PlayEffects, i, false, 60f);
                NewPreviewButton(root, "Sounds" + (i + 1), "    소리만", ref y, preview.PlaySounds, i, false, 376f);
                y += 16f;
            }

            AddDriver();
            SaveScene(scene, TagAttackSceneFolder + "/태그 공격 " + number + " " + tagName);
        }

        /// <summary>안 하나를 씬에 넣을 자료로 바꾼다. 이펙트는 2D 렌더러용 사본을, 소리는 받은 묶음의 파일을 찾아 물린다.</summary>
        private static TagAttackPlan MakePlan(PlanSpec spec, Dictionary<string, GameObject> converted)
        {
            TagAttackPlan plan = new TagAttackPlan();
            plan.Name = spec.Name;

            // 그림 이펙트의 그림이 있으면 그것을 쓰고 파티클 대역은 뺀다.
            List<TagSpriteLayer> sprites = new List<TagSpriteLayer>();
            SpriteSpec[] spriteSpecs = spec.Sprites ?? new SpriteSpec[0];
            for (int i = 0; i < spriteSpecs.Length; i++)
            {
                Sprite art = LoadTagFxSprite(spriteSpecs[i].Path);
                if (art == null)
                {
                    continue;
                }

                TagSpriteLayer layer = new TagSpriteLayer();
                layer.Sprite = art;
                layer.Color = spriteSpecs[i].Color;
                layer.Size = spriteSpecs[i].Size;
                layer.Pivot = spriteSpecs[i].Pivot;
                layer.From = spriteSpecs[i].From;
                layer.To = spriteSpecs[i].To;
                layer.FromScale = spriteSpecs[i].FromScale;
                layer.ToScale = spriteSpecs[i].ToScale;
                layer.Squeeze = spriteSpecs[i].Squeeze;
                layer.InSeconds = spriteSpecs[i].In;
                layer.HoldSeconds = spriteSpecs[i].Hold;
                layer.OutSeconds = spriteSpecs[i].Out;
                sprites.Add(layer);
            }

            plan.Sprites = sprites.ToArray();
            FxSpec[] effectSpecs = sprites.Count > 0 ? new FxSpec[0] : spec.Effects;

            TagFxLayer[] effects = new TagFxLayer[effectSpecs.Length];
            for (int i = 0; i < effects.Length; i++)
            {
                FxSpec fx = effectSpecs[i];
                bool tintedInMaterial;

                TagFxLayer layer = new TagFxLayer();
                layer.Prefab = LoadTagFx(fx.Path, fx.Tint, converted, out tintedInMaterial);
                layer.Tint = tintedInMaterial ? NoTint : fx.Tint;
                layer.Scale = fx.Scale;
                layer.FlipX = fx.FlipX;
                layer.FlipY = fx.FlipY;
                layer.Speed = fx.Speed;
                layer.Offset = fx.Offset;
                layer.Angle = fx.Angle;
                layer.Count = fx.Count;
                layer.Step = fx.Step;
                layer.StepAngle = fx.StepAngle;
                layer.StepDelay = fx.StepDelay;
                effects[i] = layer;
            }

            TagSoundLayer[] sounds = new TagSoundLayer[spec.Sounds.Length];
            for (int i = 0; i < sounds.Length; i++)
            {
                SoundSpec sound = spec.Sounds[i];
                TagSoundLayer layer = new TagSoundLayer();
                layer.Clip = FindClip(sound.File);
                layer.Start = sound.Start;
                layer.Seconds = sound.Seconds;
                layer.Volume = sound.Volume;
                layer.Delay = sound.Delay;
                layer.FadeSeconds = sound.Fade;
                sounds[i] = layer;
            }

            plan.Effects = effects;
            plan.Sounds = sounds;
            return plan;
        }

        /// <summary>Unity AI 로 만든 그림 이펙트를 읽는다. 스프라이트로 가져오게 한 뒤 읽는다.</summary>
        private static Sprite LoadTagFxSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return null;
            }

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>
        /// 원본 이펙트 프리팹을 읽는다. Vefects 셰이더를 쓰면 2D 렌더러에서 그리는 사본을 만들어 돌려준다.
        /// Vefects 셰이더는 파티클 색을 쓰지 않고 재질의 색 칸(`_R`, `_G`, `_B` 등)으로 칠한다.
        /// 그래서 물들이는 후보는 재질 색을 바꾼 사본을 따로 만든다. 그때 tintedInMaterial 이 참이다.
        /// </summary>
        private static GameObject LoadTagFx(string path, Color tint, Dictionary<string, GameObject> converted, out bool tintedInMaterial)
        {
            tintedInMaterial = false;

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null)
            {
                Debug.LogWarning("이펙트 후보가 없다: " + path);
                return null;
            }

            if (!UsesVefectsShader(source))
            {
                return source;
            }

            bool tinted = tint.a > 0f;
            string suffix = tinted ? "_" + ColorUtility.ToHtmlStringRGB(tint) : string.Empty;
            tintedInMaterial = tinted;

            GameObject copy;
            if (converted.TryGetValue(path + suffix, out copy))
            {
                return copy;
            }

            copy = MakeFlipbook2DCopy(source, tinted ? tint : NoTint, suffix);
            converted[path + suffix] = copy;
            return copy;
        }

        private static bool UsesVefectsShader(GameObject prefab)
        {
            ParticleSystemRenderer[] renderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                if (material != null && material.shader != null && material.shader.name.StartsWith("Vefects/"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Vefects 프리팹의 사본을 만든다. 재질은 2D 렌더러가 그리는 셰이더 사본으로 바꾼다.
        /// 프로젝트는 URP 2D 렌더러를 쓰고, 2D 렌더러는 `Universal2D` 와 단계 이름이 없는 단계만 그린다.
        /// Vefects Lite 셰이더는 `UniversalForwardOnly` 단계만 있어 그대로는 화면에 나오지 않는다.
        /// </summary>
        private static GameObject MakeFlipbook2DCopy(GameObject source, Color tint, string suffix)
        {
            UiSceneBuilder.EnsureFolder(TagFxFolder + "/Materials");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            ParticleSystemRenderer[] renderers = instance.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                if (material == null || material.shader == null || !material.shader.name.StartsWith("Vefects/"))
                {
                    continue;
                }

                renderers[i].sharedMaterial = MakeMaterial2D(material, tint, suffix);
            }

            string path = TagFxFolder + "/" + source.name + "_2D" + suffix + ".prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return saved;
        }

        private static Material MakeMaterial2D(Material source, Color tint, string suffix)
        {
            string path = TagFxFolder + "/Materials/" + source.name + "_2D" + suffix + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = MakeShader2D(source.shader);
            bool created = material == null;

            if (created)
            {
                material = new Material(source);
            }
            else
            {
                material.CopyPropertiesFromMaterial(source);
            }

            material.shader = shader;

            if (tint.a > 0f)
            {
                TintMaterial(material, tint);
            }

            if (created)
            {
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        /// Vefects 재질이 그림의 빨강 · 초록 · 파랑 칸마다 칠하는 색 칸과 테두리 색 칸.
        /// 물 튀김의 하늘색, 파편의 흰 불빛은 테두리 칸(`_Outline`)에서 나온다.
        private static readonly string[] VefectsColorProperties =
        {
            "_R", "_G", "_B", "_Outline", "_Color01", "_Color02", "_Color03", "_Color04",
        };

        /// <summary>재질 색 칸을 태그 색으로 바꾼다. 원래 색의 밝기는 남겨 무늬가 그대로 보이게 한다.</summary>
        private static void TintMaterial(Material material, Color tint)
        {
            for (int i = 0; i < VefectsColorProperties.Length; i++)
            {
                string name = VefectsColorProperties[i];
                if (!material.HasProperty(name))
                {
                    continue;
                }

                Color original = material.GetColor(name);
                float bright = Mathf.Max(original.r, Mathf.Max(original.g, original.b));
                material.SetColor(name, new Color(tint.r * bright, tint.g * bright, tint.b * bright, original.a));
            }
        }

        /// <summary>
        /// 셰이더 사본을 만든다. 이름과 그리는 단계 이름만 바꾼다.
        /// `UniversalForwardOnly` 를 `Universal2D` 로 바꾸면 2D 렌더러가 같은 단계를 그린다.
        /// </summary>
        private static Shader MakeShader2D(Shader source)
        {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            string fileName = Path.GetFileNameWithoutExtension(sourcePath) + "_2D.shader";
            string path = TagFxFolder + "/" + fileName;

            string text = File.ReadAllText(sourcePath);
            text = new Regex("Shader \"[^\"]+\"").Replace(text, "Shader \"SlotHero/2D/" + source.name + "\"", 1);
            text = text.Replace("\"LightMode\"=\"UniversalForwardOnly\"", "\"LightMode\"=\"Universal2D\"");

            string full = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!File.Exists(full) || File.ReadAllText(full) != text)
            {
                File.WriteAllText(full, text);
                AssetDatabase.ImportAsset(path);
            }

            return AssetDatabase.LoadAssetAtPath<Shader>(path);
        }

        /// <summary>받아 둔 묶음에서 파일 이름으로 소리를 찾는다.</summary>
        private static AudioClip FindClip(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string[] guids = AssetDatabase.FindAssets(name + " t:AudioClip");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Path.GetFileName(path) == fileName)
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }

            Debug.LogWarning("소리 후보가 없다: " + fileName);
            return null;
        }

        private static void NewPreviewButton(
            RectTransform root, string name, string label, ref float y, UnityAction<int> action, int argument, bool strong, float x)
        {
            Color color = strong ? new Color(0.545f, 0.412f, 0.212f, 1f) : new Color(0.290f, 0.227f, 0.157f, 1f);
            float height = strong ? 60f : 46f;
            Image image = UiSceneBuilder.NewButtonImage(name, root, color);
            UiSceneBuilder.PlaceTopLeft(image.rectTransform, new Vector2(x, y), new Vector2(strong ? 700f : 300f, height));

            Button button = UiSceneBuilder.AddButton(image.gameObject);
            UnityEventTools.AddIntPersistentListener(button.onClick, action, argument);

            TMP_Text text = UiSceneBuilder.NewText(
                "Label", image.transform, label, strong ? 24f : 22f, Color.white, TextAlignmentOptions.MidlineLeft, strong, true);
            RectTransform rect = text.rectTransform;
            UiSceneBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(20f, 0f);
            rect.offsetMax = new Vector2(-12f, 0f);

            y += height + 8f;
        }

        /// <summary>왼쪽 위 좌표로 자리를 잡되 가운데를 기준점으로 둔다. 레이저가 가운데에서 가운데로 뻗는다.</summary>
        private static void PlaceCentered(RectTransform rect, Vector2 topLeft, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(topLeft.x + size.x * 0.5f, -(topLeft.y + size.y * 0.5f));
        }

        /// <summary>
        /// 태그 공격 확인용 씬마다 안의 이펙트를 터뜨린 순간을 그림으로 뽑는다.
        /// 편집 모드라 파티클을 미리 돌려 찍는다. 그림은 프로젝트 폴더의 `화면 확인/태그 공격` 에 들어간다.
        /// </summary>
        [MenuItem("Slot Hero/확인용 씬 만들기/태그 공격 그림 뽑기")]
        public static void CaptureTagAttack()
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), TagAttackCaptureFolder);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            Directory.CreateDirectory(folder);

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { UiSceneBuilder.SceneFolder + "/" + TagAttackSceneFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                TagAttackPreview preview = Object.FindAnyObjectByType<TagAttackPreview>();
                Camera camera = null;
                Camera[] cameras = Object.FindObjectsByType<Camera>();
                for (int c = 0; c < cameras.Length; c++)
                {
                    if (cameras[c].cullingMask != 0)
                    {
                        camera = cameras[c];
                    }
                }

                if (preview == null || camera == null)
                {
                    continue;
                }

                string name = Path.GetFileNameWithoutExtension(scenePath);
                for (int p = 0; p < preview.PlanCount; p++)
                {
                    // 같은 이펙트를 여러 안이 쓰면 그림이 같으므로, 앞 안과 이펙트가 같으면 건너뛴다.
                    if (p > 0 && SameEffects(preview, p - 1, p))
                    {
                        continue;
                    }

                    List<KeyValuePair<GameObject, float>> spawned = preview.SpawnPlanEffects(p);
                    float latest = 0f;
                    for (int s = 0; s < spawned.Count; s++)
                    {
                        latest = Mathf.Max(latest, spawned[s].Value);
                    }

                    // 늦게 나오는 것은 그만큼 덜 돌려 차례로 덮치는 순간을 찍는다.
                    // 파티클마다 시작 지연 뒤, 수명의 절반 안쪽을 찍는다(명왕성 구름은 0.15초 늦게 시작하고 금성 파편은 0.1초만 산다).
                    for (int s = 0; s < spawned.Count; s++)
                    {
                        ParticleSystem[] systems = spawned[s].Key.GetComponentsInChildren<ParticleSystem>(true);
                        for (int k = 0; k < systems.Length; k++)
                        {
                            ParticleSystem.MainModule main = systems[k].main;
                            float at = main.startDelay.constantMax
                                + Mathf.Min(TagAttackCaptureSeconds, main.startLifetime.constantMax * 0.5f)
                                + (latest - spawned[s].Value) * 0.5f;
                            systems[k].Simulate(at, false, true);
                        }
                    }

                    List<GameObject> sprites = preview.SpawnPlanSprites(p);

                    ShootCamera(camera, Path.Combine(folder, name + " 안 " + (p + 1) + ".png"));

                    for (int s = 0; s < spawned.Count; s++)
                    {
                        Object.DestroyImmediate(spawned[s].Key);
                    }

                    for (int s = 0; s < sprites.Count; s++)
                    {
                        Object.DestroyImmediate(sprites[s]);
                    }
                }
            }

            Debug.Log("태그 공격 그림을 뽑았다: " + folder);
        }

        /// <summary>
        /// 게임에 붙인 태그 공격 이펙트를 전투 확인용 씬의 적 자리에 터뜨려 그림으로 뽑는다.
        /// 설정(`TagAttackFxConfig`)의 프리팹과 그림을 그대로 쓰므로 게임에서 보이는 모양과 같다.
        /// 그림은 프로젝트 폴더의 `화면 확인/태그 공격 게임` 에 들어간다.
        /// </summary>
        [MenuItem("Slot Hero/확인용 씬 만들기/태그 공격 게임 그림 뽑기")]
        public static void CaptureTagAttackInCombat()
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), TagAttackCaptureFolder + " 게임");
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            Directory.CreateDirectory(folder);

            EditorSceneManager.OpenScene(UiSceneBuilder.SceneFolder + "/" + CombatSceneName + ".unity", OpenSceneMode.Single);
            SlotHero.Combat.UI.CombatScreenView view = Object.FindAnyObjectByType<SlotHero.Combat.UI.CombatScreenView>();
            SlotHero.TagAttack.TagAttackFxConfig config =
                UiSceneBuilder.LoadConfig<SlotHero.TagAttack.TagAttackFxConfig>("TagAttack", "TagAttackFxConfig");
            Camera camera = null;
            Camera[] cameras = Object.FindObjectsByType<Camera>();
            for (int c = 0; c < cameras.Length; c++)
            {
                if (cameras[c].cullingMask != 0)
                {
                    camera = cameras[c];
                }
            }

            Image enemyArt = view != null ? new SerializedObject(view).FindProperty("_enemyArt").objectReferenceValue as Image : null;
            if (config == null || camera == null || enemyArt == null)
            {
                Debug.LogWarning("전투 확인용 씬이나 태그 공격 설정이 없어 그림을 뽑지 못했다.");
                return;
            }

            enemyArt.gameObject.SetActive(true);
            RectTransform target = enemyArt.rectTransform;
            Canvas root = target.GetComponentInParent<Canvas>().rootCanvas;

            for (int i = 0; i < config.Entries.Count; i++)
            {
                SlotHero.TagAttack.TagAttackFxEntry entry = config.Entries[i];
                List<GameObject> spawned = new List<GameObject>();

                if (entry.Effect != null)
                {
                    Vector3 at = target.TransformPoint(target.rect.center);
                    at.z = camera.transform.position.z + 10f;
                    GameObject fx = (GameObject)Object.Instantiate(entry.Effect, at, entry.Effect.transform.rotation);
                    spawned.Add(fx);

                    ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>(true);
                    for (int k = 0; k < systems.Length; k++)
                    {
                        ParticleSystem.MainModule main = systems[k].main;
                        ParticleSystemRenderer renderer = systems[k].GetComponent<ParticleSystemRenderer>();
                        if (renderer != null)
                        {
                            renderer.sortingOrder = root.sortingOrder + config.SortingOffset + renderer.sortingOrder;
                        }

                        // 미리 돌리기는 재생 빠르기를 따른다. 느리게 만든 이펙트(토성, 명왕성)는 그만큼 더 돌려야 같은 순간이 찍힌다.
                        float speed = Mathf.Max(0.05f, main.simulationSpeed);
                        systems[k].Simulate((main.startDelay.constantMax
                            + Mathf.Min(TagAttackCaptureSeconds, main.startLifetime.constantMax * 0.5f)) / speed, false, true);
                    }
                }

                if (entry.Sprite != null)
                {
                    GameObject go = new GameObject("TagAttackSprite", typeof(RectTransform), typeof(Image));
                    RectTransform rect = (RectTransform)go.transform;
                    rect.SetParent(target.parent, false);
                    rect.SetSiblingIndex(target.GetSiblingIndex() + 1);
                    rect.anchorMin = target.anchorMin;
                    rect.anchorMax = target.anchorMax;
                    rect.pivot = entry.SpritePivot;
                    rect.sizeDelta = entry.SpriteSize;
                    Vector2 center = target.anchoredPosition + Vector2.Scale(new Vector2(0.5f, 0.5f) - target.pivot, target.rect.size);
                    rect.anchoredPosition = center + entry.SpriteOffset;
                    Image image = go.GetComponent<Image>();
                    image.sprite = entry.Sprite;
                    image.preserveAspect = true;
                    spawned.Add(go);
                }

                if (spawned.Count > 0)
                {
                    ShootCamera(camera, Path.Combine(folder, "태그 공격 게임 " + (i + 1) + " " + entry.Tag + ".png"));
                }

                for (int s = 0; s < spawned.Count; s++)
                {
                    Object.DestroyImmediate(spawned[s]);
                }
            }

            Debug.Log("태그 공격 게임 그림을 뽑았다: " + folder);
        }

        private static bool SameEffects(TagAttackPreview preview, int a, int b)
        {
            SerializedObject so = new SerializedObject(preview);
            SerializedProperty plans = so.FindProperty("_plans");
            SerializedProperty first = plans.GetArrayElementAtIndex(a).FindPropertyRelative("_effects");
            SerializedProperty second = plans.GetArrayElementAtIndex(b).FindPropertyRelative("_effects");
            return SerializedProperty.DataEquals(first, second);
        }

        private static void ShootCamera(Camera camera, string file)
        {
            Canvas.ForceUpdateCanvases();

            RenderTexture texture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = texture;

            CameraFrame frame = camera.GetComponent<CameraFrame>();
            if (frame != null)
            {
                frame.Apply();
            }

            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            Texture2D shot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            shot.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;

            File.WriteAllBytes(file, shot.EncodeToPNG());

            Object.DestroyImmediate(shot);
            texture.Release();
            Object.DestroyImmediate(texture);
        }
    }
}
