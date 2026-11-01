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
        };

        [MenuItem("Slot Hero/소리 가져오기")]
        public static void Import()
        {
            CopyFolder(Path.Combine(ImportArt.SourceRoot, @"Audio\Sound"), SoundFolder);
            AssetDatabase.Refresh();

            LinkClips();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
