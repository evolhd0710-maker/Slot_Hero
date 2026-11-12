using System.Collections.Generic;
using System.IO;
using System.Text;
using SlotHero.Localization;
using SlotHero.Localization.UI;
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
    ///
    /// 2026년 11월 11일 원재가 영어로 문제 될 곳을 전부 보라고 해 **넘침 검사**를 더했다.
    /// 화면에 켜진 글상자마다 한국어와 영어로 한 번씩 그려 글이 칸을 벗어나는지 잰다. 한국어는 칸에 들어가는데
    /// 영어만 벗어나는 글상자를 `코드/Localization/Text/영어 넘침.tsv` 에 적는다. 영어로 글자가 많이 줄어든 글상자(자동 크기)도 함께 적는다.
    /// 확인용 씬을 영어로 뽑을 때(`CapturePreviewScenes.CaptureAllEnglish`)도 넘침만 잰다.
    /// </summary>
    public static class LocalizationAudit
    {
        private static Translator _translator;
        private static readonly Dictionary<string, string> Seen = new Dictionary<string, string>();

        /// 넘침을 잰 글상자(글, 자리, 칸 크기). 같은 것은 다시 재지 않는다.
        private static readonly HashSet<string> Measured = new HashSet<string>();

        /// 영어로 넘치는 글상자. 열쇠는 글과 자리, 값은 적을 줄이다.
        private static readonly Dictionary<string, string> Overflows = new Dictionary<string, string>();

        /// 넘침으로 치는 여유. 글 테두리와 반올림 때문에 1 ~ 2 픽셀은 벗어난다.
        private const float OverflowTolerance = 3f;

        /// 영어로 자동 크기가 이 비율 아래로 줄면 적는다.
        private const float ShrinkWarnRatio = 0.75f;

        /// <summary>영어로 넘치는 글상자를 적는 자리.</summary>
        public static string OverflowPath
        {
            get { return Path.Combine(ImportArt.SourceRoot, @"Localization\Text\영어 넘침.tsv"); }
        }

        /// <summary>빠진 번역을 적는 자리.</summary>
        public static string MissingPath
        {
            get { return Path.Combine(ImportArt.SourceRoot, @"Localization\Text\빠진 번역.tsv"); }
        }

        /// <summary>번역표를 읽어 검사 중인지. 확인용 씬 영어 뽑기를 따로 돌릴 때 스스로 시작하고 적는다.</summary>
        public static bool Running
        {
            get { return _translator != null; }
        }

        /// <summary>번역표를 읽고 기록을 비운다. 게임 씬 검사 앞에 부른다.</summary>
        public static void Begin()
        {
            LocalizationConfig config = UiSceneBuilder.LoadConfig<LocalizationConfig>("Localization", "LocalizationConfig");
            _translator = new Translator();
            _translator.Load(config != null ? config.English : null);
            Seen.Clear();
            Measured.Clear();
            Overflows.Clear();
        }

        /// <summary>열린 씬의 글상자를 모두 맞춰 본다. 꺼진 글상자도 본다. 켜진 글상자는 영어로 넘치는지도 잰다.</summary>
        public static void ScanOpenScenes()
        {
            ScanOpenScenes(true);
        }

        /// <summary>
        /// 열린 씬의 글상자를 훑는다. countMissing 이 거짓이면 넘침만 잰다.
        /// 확인용 씬은 견본 글자가 번역표에 없어 빠진 번역으로 세지 않는다.
        /// </summary>
        public static void ScanOpenScenes(bool countMissing)
        {
            if (_translator == null)
            {
                return;
            }

            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null)
                {
                    continue;
                }

                if (countMissing)
                {
                    Try(texts[i].text, PathOf(texts[i].transform));
                }

                MeasureOverflow(texts[i]);
            }
        }

        /// <summary>
        /// 글상자 하나를 한국어와 영어로 그려 칸을 벗어나는지 잰다. 영어만 벗어나면 적는다.
        /// 게임의 언어는 바꾸지 않는다. 그 글상자의 전처리만 잠깐 바꿔 그리고 되돌린다.
        /// </summary>
        private static void MeasureOverflow(TMP_Text text)
        {
            if (!text.isActiveAndEnabled || !text.gameObject.activeInHierarchy || !Translator.HasHangul(text.text))
            {
                return;
            }

            RectTransform rect = text.rectTransform;
            Vector4 margin = text.margin;
            float width = rect.rect.width - margin.x - margin.z;
            float height = rect.rect.height - margin.y - margin.w;
            if (width < 1f || height < 1f)
            {
                return;
            }

            string where = PathOf(text.transform);
            string key = text.text + "\u0001" + where + "\u0001" + Mathf.RoundToInt(width) + "x" + Mathf.RoundToInt(height);
            if (!Measured.Add(key))
            {
                return;
            }

            TextLocalizer localizer = text.GetComponent<TextLocalizer>();
            string context = localizer != null ? localizer.Context : null;
            string english = string.IsNullOrEmpty(context) ? _translator.Translate(text.text) : _translator.Translate(text.text, context);

            ITextPreprocessor original = text.textPreprocessor;
            Vector2 korean;
            float koreanSize;
            Vector2 englishSize;
            float englishFont;
            try
            {
                text.textPreprocessor = null;
                korean = Draw(text, out koreanSize);
                text.textPreprocessor = new Fixed(english);
                englishSize = Draw(text, out englishFont);
            }
            finally
            {
                text.textPreprocessor = original;
                text.ForceMeshUpdate(true, true);
            }

            // 가로와 세로를 따로 본다. 칸을 벗어나고, 그 방향으로 한국어보다도 커야 영어 탓이다.
            // 한 줄 글은 글자 높이가 칸보다 조금 큰 것이 흔해 한국어와 영어가 함께 넘으면 세지 않는다.
            // 한국어가 그 방향으로 칸에 들어가면 영어만의 넘침이다. 한국어도 이미 넘치면 공통 배치 문제라 따로 적는다(2026년 11월 11일 코덱스 점검).
            bool wider = englishSize.x > width + OverflowTolerance && englishSize.x > korean.x + OverflowTolerance;
            bool taller = englishSize.y > height + OverflowTolerance && englishSize.y > korean.y + OverflowTolerance;
            bool koreanFitsWide = korean.x <= width + OverflowTolerance;
            bool koreanFitsTall = korean.y <= height + OverflowTolerance;
            bool shrunk = text.enableAutoSizing && koreanSize > 0f && englishFont < koreanSize * ShrinkWarnRatio;

            string problem = null;
            if ((wider && koreanFitsWide) || (taller && koreanFitsTall))
            {
                problem = "넘침";
            }
            else if (wider || taller)
            {
                problem = "한국어도 넘침";
            }
            else if (shrunk)
            {
                problem = "작아짐";
            }

            if (problem == null)
            {
                return;
            }

            string overflowKey = text.text + "\u0001" + where;
            if (Overflows.ContainsKey(overflowKey))
            {
                return;
            }

            Overflows[overflowKey] = problem + "\t" + Escape(text.text) + "\t" + Escape(english) + "\t" + where
                + "\t칸 " + Mathf.RoundToInt(width) + " × " + Mathf.RoundToInt(height)
                + "\t영어 " + Mathf.RoundToInt(englishSize.x) + " × " + Mathf.RoundToInt(englishSize.y)
                + " · 글자 " + englishFont.ToString("0")
                + "\t한국어 " + Mathf.RoundToInt(korean.x) + " × " + Mathf.RoundToInt(korean.y)
                + " · 글자 " + koreanSize.ToString("0");
        }

        /// <summary>지금 전처리로 그린 글의 크기와 글자 크기(자동 크기면 줄어든 크기).</summary>
        private static Vector2 Draw(TMP_Text text, out float fontSize)
        {
            text.ForceMeshUpdate(true, true);
            Bounds bounds = text.textBounds;
            fontSize = text.fontSize;
            return new Vector2(bounds.size.x, bounds.size.y);
        }

        /// <summary>정해 둔 글로 바꿔 그리는 전처리. 넘침을 잴 때만 잠깐 건다.</summary>
        private class Fixed : ITextPreprocessor
        {
            private readonly string _shown;

            public Fixed(string shown)
            {
                _shown = shown;
            }

            public string PreprocessText(string text)
            {
                return _shown;
            }
        }

        /// <summary>영어 넘침을 파일로 적는다. 확인용 씬 영어 그림까지 뽑은 뒤 부른다.</summary>
        public static void WriteOverflows()
        {
            List<string> lines = new List<string>(Overflows.Values);
            lines.Sort(System.StringComparer.Ordinal);

            StringBuilder file = new StringBuilder();
            file.AppendLine("# 영어로만 칸을 벗어나거나 많이 작아지는 글상자. 번역 검사가 만든다.");
            file.AppendLine("# 문제, 한국어, 영어, 자리, 칸 크기, 영어로 그린 크기, 한국어로 그린 크기");
            for (int i = 0; i < lines.Count; i++)
            {
                file.AppendLine(lines[i]);
            }

            string folder = Path.GetDirectoryName(OverflowPath);
            if (Directory.Exists(folder))
            {
                File.WriteAllText(OverflowPath, file.ToString(), new UTF8Encoding(false));
            }

            int over = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("넘침"))
                {
                    over++;
                }
            }

            Debug.Log("영어로 넘치는 글상자 " + over + "건, 한국어도 넘치거나 많이 작아지는 글상자 " + (lines.Count - over) + "건");

            // 검사를 마친다. 다음에 따로 돌리면 번역표를 새로 읽는다.
            _translator = null;
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

            // 번역표는 남겨 둔다. 뒤이어 확인용 씬을 영어로 뽑으며 넘침을 더 잰다(`WriteOverflows`).
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
