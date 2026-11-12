using System.Collections.Generic;
using System.IO;
using SlotHero.Audio;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 고른 소리 파일을 프로젝트로 가져와 효과음 설정에 물린다.
    /// 메뉴 Slot Hero / 소리 가져오기 에서 실행한다.
    ///
    /// 가져오는 것은 `코드/Audio/Sound` 의 WAV 다. 에셋 스토어 무료 묶음에서 원재가 들어 보고 고른 것만 넣어 둔다.
    /// 소리마다 쓰는 파일은 `효과음과 이펙트 목록.md` 6.0 받은 것 의 표다.
    /// </summary>
    public static class ImportSounds
    {
        private const string SoundFolder = "Assets/SlotHero/Art/Audio";

        /// <summary>
        /// 소리마다 쓰는 파일 이름. 2026년 10월 29일 원재가 골랐다.
        /// 행상 구매는 Shapeforms Audio Free Sound Effects 의 동전 소리 셋,
        /// 야영은 Timbeaux's 2D Fantasy SFX Pack 의 모닥불 소리다.
        ///
        /// 행상 구매의 파일 차례는 치른 골드의 구간 차례다(`SoundConfig.PurchaseGoldSteps`, 2026년 10월 31일 원재).
        /// 30 미만은 동전 한 닢, 80 미만은 부드러운 바닥에 떨어진 자루, 80 이상은 나무 바닥에 떨어진 자루다.
        /// </summary>
        private static readonly Dictionary<SoundCue, string[]> Files = new Dictionary<SoundCue, string[]>
        {
            {
                SoundCue.MerchantPurchase, new[]
                {
                    "Coin Dropped on Wood Rattle",
                    "Coins in Sack Dropped on Soft Surface",
                    "Coins In Sack Dropped on Wood",
                }
            },
            { SoundCue.Camp, new[] { "CampfireEcho" } },

            // 2026년 11월 13일 원재가 효과음 후보 듣기 씬에서 골랐다.
            // S01 은 Free UI Click, S06 · S19 · S20 은 Whoosh Lite, S09 는 Timbeaux, S12 와 S15 는 Shapeforms 의 소리다.
            { SoundCue.ButtonPress, new[] { "SFX_UI_Button_Organic_Plastic_Thin_Select_1" } },
            { SoundCue.CurrentBuildToggle, new[] { "MOVEActv_Medium_HA_WHOOSH_04" } },
            { SoundCue.GoldGained, new[] { "Coin 1" } },
            { SoundCue.HealthLost, new[] { "PUNCH_PERCUSSIVE_HEAVY_08" } },
            { SoundCue.CoinGained, new[] { "Coin Flung" } },
            { SoundCue.RunStart, new[] { "MOVEActv_LongHeavy_HA_WHOOSH_04" } },
            { SoundCue.StageTransition, new[] { "MOVEActv_LongHeavy_HA_WHOOSH_09" } },

            // 같은 날 원재가 더 골랐다. S07 은 Kenney RPG Audio 의 한 걸음 소리 다섯을 0.3초 간격으로 이은 것,
            // S25 · S26 은 원재가 받아 둔 Atelier Magicae Fantasy UI Sound Effects 의 두 소리다. 이 묶음은 파일을 다시 나눠 주는 것을 막는다.
            { SoundCue.NodeSelect, new[] { "Kenney 다섯 걸음 빠름" } },
            { SoundCue.SettingStepUp, new[] { "Fantasy_UI (10)" } },
            { SoundCue.SettingStepDown, new[] { "Fantasy_UI (11)" } },
        };

        [MenuItem("Slot Hero/소리 가져오기")]
        public static void Import()
        {
            CopyFolder(Path.Combine(ImportArt.SourceRoot, @"Audio\Sound"), SoundFolder);
            CopyCandidates();
            AssetDatabase.Refresh();

            LinkClips();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>효과음 후보 듣기 씬이 쓰는 후보 소리의 자리. 소리마다 하위 폴더 하나다.</summary>
        public const string CandidateFolder = SoundFolder + "/후보";

        /// <summary>
        /// 효과음 후보를 소리마다 하위 폴더로 옮긴다(2026년 11월 13일 원재). 효과음 후보 듣기 씬이 이것을 늘어놓는다.
        /// `코드/Audio/후보 효과음 20261113` 의 소리별 폴더와 S07 의 후보 폴더다.
        /// 같은 날 원재가 씬을 정리하라고 해, 파일을 정했거나 후보를 쓰지 않기로 한 소리의 폴더는 지웠다. 지금은 아직 고를 S07 과 S08 만 남았다.
        /// 내용이 같은 파일은 덮지 않는다.
        /// </summary>
        private static void CopyCandidates()
        {
            string root = Path.Combine(ImportArt.SourceRoot, "Audio");
            Dictionary<string, string> folders = new Dictionary<string, string>();

            // 원본 폴더가 없는 자리(다른 PC)에서는 프로젝트의 후보를 건드리지 않는다. 없는 것을 빈 목록으로 보면 다 지운다(2026년 11월 13일 코덱스 점검).
            string gathered = Path.Combine(root, "후보 효과음 20261113");
            if (!Directory.Exists(gathered))
            {
                Debug.Log("효과음 후보 폴더가 없어 후보 옮기기를 건너뛴다: " + gathered);
                return;
            }

            string[] rows = Directory.GetDirectories(gathered);
            for (int i = 0; i < rows.Length; i++)
            {
                folders[Path.GetFileName(rows[i])] = rows[i];
            }

            UiSceneBuilder.EnsureFolder(CandidateFolder);

            // 원본에서 없어진 소리 폴더는 프로젝트에서도 지운다. S08 은 2026년 11월 13일 펼치기와 접기 두 줄로 나눴다.
            string[] existing = AssetDatabase.GetSubFolders(CandidateFolder);
            for (int i = 0; i < existing.Length; i++)
            {
                if (!folders.ContainsKey(Path.GetFileName(existing[i])))
                {
                    AssetDatabase.DeleteAsset(existing[i]);
                }
            }

            foreach (KeyValuePair<string, string> pair in folders)
            {
                string target = CandidateFolder + "/" + pair.Key;
                UiSceneBuilder.EnsureFolder(target);
                if (!Directory.Exists(pair.Value))
                {
                    continue;
                }

                // Kenney 의 소리는 OGG 라 둘 다 옮긴다(2026년 11월 13일 S08 후보).
                string[] files = SoundFiles(pair.Value);
                HashSet<string> names = new HashSet<string>();
                for (int i = 0; i < files.Length; i++)
                {
                    names.Add(Path.GetFileName(files[i]));
                    string destination = Path.Combine(target, Path.GetFileName(files[i]));
                    // 크기가 같아도 소리를 고쳤을 수 있어 내용을 견준다(2026년 11월 13일 코덱스 점검).
                    if (File.Exists(destination) && ImportArt.SameBytes(files[i], destination))
                    {
                        continue;
                    }

                    File.Copy(files[i], destination, true);
                }

                // 원본에서 빠진 후보는 프로젝트에서도 지운다. S07 은 2026년 11월 13일 원재가 목록을 비우고 발걸음으로 다시 채웠다.
                string[] old = SoundFiles(target);
                for (int i = 0; i < old.Length; i++)
                {
                    if (!names.Contains(Path.GetFileName(old[i])))
                    {
                        AssetDatabase.DeleteAsset(target + "/" + Path.GetFileName(old[i]));
                    }
                }
            }
        }

        /// <summary>폴더의 WAV 와 OGG.</summary>
        private static string[] SoundFiles(string folder)
        {
            List<string> files = new List<string>(Directory.GetFiles(folder, "*.wav"));
            files.AddRange(Directory.GetFiles(folder, "*.ogg"));
            return files.ToArray();
        }

        /// <summary>폴더의 WAV 를 프로젝트로 옮긴다.</summary>
        private static void CopyFolder(string source, string target)
        {
            if (!Directory.Exists(source))
            {
                Debug.Log("소리 폴더가 없어 건너뛴다: " + source);
                return;
            }

            UiSceneBuilder.EnsureFolder(target);

            string[] files = Directory.GetFiles(source, "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                string destination = Path.Combine(target, Path.GetFileName(files[i]));
                File.Copy(files[i], destination, true);
            }

            Debug.Log(files.Length + "개 소리를 옮겼다: " + target);
        }

        /// <summary>가져온 소리를 효과음 설정의 소리마다 물린다. 없는 파일은 건너뛴다.</summary>
        private static void LinkClips()
        {
            SoundConfig config = UiSceneBuilder.LoadConfig<SoundConfig>("Audio", "SoundConfig");
            if (config == null)
            {
                Debug.LogWarning("효과음 설정을 찾지 못했다.");
                return;
            }

            // 새로 붙인 소리(S12 체력 잃음 등)의 칸이 이미 만든 설정 에셋에도 생기게 한다.
            int addedCues = config.AddMissingDefaults();
            if (addedCues > 0)
            {
                Debug.Log("효과음 설정에 소리 칸 " + addedCues + "개를 더했다.");
            }

            int linked = 0;

            for (int i = 0; i < config.Cues.Count; i++)
            {
                SoundCueEntry entry = config.Cues[i];

                string[] names;
                if (!Files.TryGetValue(entry.Cue, out names))
                {
                    continue;
                }

                List<AudioClip> clips = new List<AudioClip>();
                for (int n = 0; n < names.Length; n++)
                {
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SoundFolder + "/" + names[n] + ".wav");
                    if (clip != null)
                    {
                        clips.Add(clip);
                    }
                }

                entry.Clips = clips.ToArray();
                config.Cues[i] = entry;
                linked += clips.Count;
            }

            EditorUtility.SetDirty(config);
            Debug.Log("효과음 파일 " + linked + "개를 물렸다.");
        }
    }
}
