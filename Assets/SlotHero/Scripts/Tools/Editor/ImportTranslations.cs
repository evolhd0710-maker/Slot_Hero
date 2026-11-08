using System.Collections.Generic;
using System.IO;
using System.Text;
using SlotHero.Localization;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 영어 번역표를 TSV 에서 설정 에셋으로 가져온다(2026년 11월 8일 원재).
    /// 메뉴 Slot Hero / 번역 가져오기 에서 실행한다.
    ///
    /// 원본은 `코드/Localization/Text/영어.tsv` 다. 원본 폴더가 없는 컴퓨터에서는 건너뛰고 에셋에 담긴 표를 그대로 쓴다.
    /// </summary>
    public static class ImportTranslations
    {
        /// <summary>원본 TSV 자리.</summary>
        public static string SourcePath
        {
            get { return Path.Combine(ImportArt.SourceRoot, @"Localization\Text\영어.tsv"); }
        }

        [MenuItem("Slot Hero/번역 가져오기")]
        public static void Import()
        {
            LocalizationConfig config = UiSceneBuilder.LoadConfig<LocalizationConfig>("Localization", "LocalizationConfig");
            if (config == null)
            {
                Debug.LogWarning("번역표 설정 에셋이 없다. 설정 에셋 만들기를 먼저 돌린다.");
                return;
            }

            if (!File.Exists(SourcePath))
            {
                Debug.Log("번역 원본이 없어 건너뛴다: " + SourcePath);
                return;
            }

            List<TranslationEntry> entries = LocalizationConfig.ParseTsv(File.ReadAllText(SourcePath, Encoding.UTF8));
            config.English = entries;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("번역 " + entries.Count + "줄을 가져왔다.");
        }
    }
}
