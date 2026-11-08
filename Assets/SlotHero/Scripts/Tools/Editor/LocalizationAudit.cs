using System.Collections.Generic;
using System.IO;
using System.Text;
using SlotHero.Localization;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 영어 번역이 빠진 한국어 글자를 찾는다(2026년 11월 8일 원재).
    ///
    /// 게임 씬 검사와 전투 검사가 화면을 바꿀 때마다 떠 있는 글상자를 훑고(`ScanOpenScenes`),
    /// 끝에 설정 에셋과 프리팹의 글자를 훑는다(`Finish`). 번역표로 다 바꾸지 못한 글자를
    /// `코드/Localization/Text/빠진 번역.tsv` 에 어디서 봤는지와 함께 적는다.
    ///
    /// 검사는 한국어 그대로 돈다. 여기서는 번역표만 따로 읽어 글자를 맞춰 볼 뿐 게임의 언어를 바꾸지 않는다.
    /// </summary>
    public static class LocalizationAudit
    {
        private static Translator _translator;
        private static readonly Dictionary<string, string> Seen = new Dictionary<string, string>();

        /// <summary>빠진 번역을 적는 자리.</summary>
        public static string MissingPath
        {
            get { return Path.Combine(ImportArt.SourceRoot, @"Localization\Text\빠진 번역.tsv"); }
        }

        /// <summary>번역표를 읽고 기록을 비운다. 게임 씬 검사 앞에 부른다.</summary>
        public static void Begin()
        {
            LocalizationConfig config = UiSceneBuilder.LoadConfig<LocalizationConfig>("Localization", "LocalizationConfig");
            _translator = new Translator();
            _translator.Load(config != null ? config.English : null);
            Seen.Clear();
        }

        /// <summary>열린 씬의 글상자를 모두 맞춰 본다. 꺼진 글상자도 본다.</summary>
        public static void ScanOpenScenes()
        {
            if (_translator == null)
            {
                return;
            }

            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    Try(texts[i].text, PathOf(texts[i].transform));
                }
            }
        }

        /// <summary>설정 에셋과 프리팹의 글자를 맞춰 보고 빠진 번역을 파일로 적는다.</summary>
        public static void Finish()
        {
            if (_translator == null)
            {
                Begin();
            }

            ScanConfigs();
            ScanPrefabs();

            List<string> missing = new List<string>(Seen.Keys);
            missing.Sort(System.StringComparer.Ordinal);

            StringBuilder file = new StringBuilder();
            file.AppendLine("# 영어 번역이 빠진 글자. 번역 빠진 글자 찾기가 만든다. 한국어, 탭, 본 곳이다.");
            for (int i = 0; i < missing.Count; i++)
            {
                file.Append(Escape(missing[i])).Append('\t').AppendLine(Seen[missing[i]]);
            }

            string folder = Path.GetDirectoryName(MissingPath);
            if (Directory.Exists(folder))
            {
                File.WriteAllText(MissingPath, file.ToString(), new UTF8Encoding(false));
            }

            Debug.Log("번역 빠진 글자 " + missing.Count + "건");
            _translator = null;
        }

        private static void ScanConfigs()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/SlotHero/Configs" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null || asset is LocalizationConfig)
                {
                    continue;
                }

                SerializedObject serialized = new SerializedObject(asset);
                SerializedProperty property = serialized.GetIterator();
                while (property.Next(true))
                {
                    // 식별자(_id, _nextScreenId 같은 것)와 삽화 분위기 설명(_mood)은 화면에 나오지 않는다.
                    if (property.propertyType == SerializedPropertyType.String
                        && !property.name.EndsWith("id", System.StringComparison.OrdinalIgnoreCase)
                        && property.name != "_mood")
                    {
                        Try(property.stringValue, asset.name + "." + property.propertyPath);
                    }
                }
            }
        }

        private static void ScanPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { UiSceneBuilder.PrefabFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (prefab == null)
                {
                    continue;
                }

                TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);
                for (int t = 0; t < texts.Length; t++)
                {
                    Try(texts[t].text, prefab.name + "/" + PathOf(texts[t].transform));
                }
            }
        }

        private static void Try(string text, string where)
        {
            if (!Translator.HasHangul(text) || Seen.ContainsKey(text))
            {
                return;
            }

            // 화면에 나오지 않는다고 표시한 글자는 한글이 남아도 빠진 것이 아니다. 못 찾은 글자 기록이 늘었는지로 본다.
            int before = _translator.Missing.Count;
            _translator.Translate(text);
            if (_translator.Missing.Count > before)
            {
                Seen[text] = where;
            }
        }

        private static string PathOf(Transform transform)
        {
            string path = transform.name;
            Transform parent = transform.parent;
            int depth = 0;
            while (parent != null && depth < 3)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
                depth++;
            }

            return path;
        }

        private static string Escape(string text)
        {
            return text.Replace("\r", string.Empty).Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}
