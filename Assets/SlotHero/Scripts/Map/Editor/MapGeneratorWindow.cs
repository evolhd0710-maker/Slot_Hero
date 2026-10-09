using System.Collections.Generic;
using System.Text;
using SlotHero.Map;
using UnityEditor;
using UnityEngine;

namespace SlotHero.MapEditor
{
    /// <summary>
    /// 에디터에서 맵 생성 결과를 눈으로 확인하고 여러 시드를 한 번에 검증하는 창.
    /// 메뉴 Slot Hero / 맵 생성 미리보기 에서 연다.
    /// </summary>
    public class MapGeneratorWindow : EditorWindow
    {
        private MapGenerationConfig _config;
        private int _seed = 12345;
        private int _stageIndex = 1;
        private int _batchCount = 500;

        private StageMap _map;
        private string _validationMessage;
        private bool _validationPassed;
        private Vector2 _scroll;
        private string _batchReport;

        private static readonly Dictionary<RoomType, Color> PreviewColors = new Dictionary<RoomType, Color>
        {
            { RoomType.Normal, new Color(0.95f, 0.78f, 0.20f) },
            { RoomType.Elite, new Color(0.78f, 0.22f, 0.20f) },
            { RoomType.Sanctum, new Color(0.25f, 0.65f, 0.38f) },
            { RoomType.Event, new Color(0.60f, 0.58f, 0.55f) },
            { RoomType.Etc, new Color(0.40f, 0.45f, 0.70f) },
            { RoomType.Boss, new Color(0.15f, 0.14f, 0.12f) },
            { RoomType.None, new Color(1f, 0f, 1f) },
        };

        [MenuItem("Slot Hero/맵 생성 미리보기")]
        public static void Open()
        {
            MapGeneratorWindow window = GetWindow<MapGeneratorWindow>();
            window.titleContent = new GUIContent("맵 생성 미리보기");
            window.minSize = new Vector2(760f, 560f);
            window.Show();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawSettings();
            EditorGUILayout.Space();
            DrawPreview();
            EditorGUILayout.Space();
            DrawBatch();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("생성 설정", EditorStyles.boldLabel);
            _config = (MapGenerationConfig)EditorGUILayout.ObjectField(
                "맵 생성 설정", _config, typeof(MapGenerationConfig), false);
            _seed = EditorGUILayout.IntField("시드", _seed);
            _stageIndex = EditorGUILayout.IntField("스테이지", _stageIndex);

            using (new EditorGUI.DisabledScope(_config == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("생성"))
                {
                    GenerateOnce(_seed);
                }

                if (GUILayout.Button("시드 바꿔 생성"))
                {
                    _seed = Random.Range(int.MinValue, int.MaxValue);
                    GenerateOnce(_seed);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (!string.IsNullOrEmpty(_validationMessage))
            {
                EditorGUILayout.HelpBox(
                    _validationMessage,
                    _validationPassed ? MessageType.Info : MessageType.Error);
            }
        }

        private void GenerateOnce(int seed)
        {
            _map = MapGenerator.Generate(seed, _stageIndex, _config);

            string error;
            _validationPassed = MapValidator.ValidateAll(_map, _config, out error);
            _validationMessage = _validationPassed
                ? BuildSummary(_map)
                : error;

            Repaint();
        }

        private string BuildSummary(StageMap map)
        {
            if (map == null)
            {
                return "맵이 없다.";
            }

            Dictionary<RoomType, int> counts = new Dictionary<RoomType, int>();
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                RoomType roomType = map.Nodes[i].RoomType;
                int count;
                counts.TryGetValue(roomType, out count);
                counts[roomType] = count + 1;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("검증 통과. 노드 {0}개, 간선 {1}개, 시작 노드 {2}개",
                map.Nodes.Count, map.Edges.Count, map.GetStartNodes().Count);
            builder.Append("\n방 타입: ");

            foreach (KeyValuePair<RoomType, int> pair in counts)
            {
                builder.AppendFormat("{0} {1}개  ", pair.Key, pair.Value);
            }

            return builder.ToString();
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);

            Rect area = GUILayoutUtility.GetRect(720f, 340f);
            EditorGUI.DrawRect(area, new Color(0.16f, 0.16f, 0.16f));

            if (_map == null || _config == null)
            {
                EditorGUI.LabelField(area, "생성 버튼을 누르면 결과가 여기에 나온다.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            const float padding = 30f;
            float nodeSize = 16f;
            float columnSpacing = (area.width - padding * 2f) / Mathf.Max(1, _map.StageCount - 1);
            float rowSpacing = (area.height - padding * 2f) / Mathf.Max(1, _map.RowCount - 1);

            // 간선 먼저 그린다
            Handles.BeginGUI();
            Handles.color = new Color(0.65f, 0.65f, 0.65f);
            for (int i = 0; i < _map.Edges.Count; i++)
            {
                MapNode from = _map.GetNode(_map.Edges[i].FromNodeId);
                MapNode to = _map.GetNode(_map.Edges[i].ToNodeId);
                if (from == null || to == null)
                {
                    continue;
                }

                Handles.DrawLine(
                    NodeCenter(from, area, padding, columnSpacing, rowSpacing),
                    NodeCenter(to, area, padding, columnSpacing, rowSpacing));
            }

            Handles.EndGUI();

            for (int i = 0; i < _map.Nodes.Count; i++)
            {
                MapNode node = _map.Nodes[i];
                Vector2 center = NodeCenter(node, area, padding, columnSpacing, rowSpacing);
                Rect nodeRect = new Rect(
                    center.x - nodeSize * 0.5f,
                    center.y - nodeSize * 0.5f,
                    nodeSize,
                    nodeSize);

                Color color;
                if (!PreviewColors.TryGetValue(node.RoomType, out color))
                {
                    color = Color.white;
                }

                EditorGUI.DrawRect(nodeRect, color);
            }

            // 범례
            Rect legend = new Rect(area.x + 8f, area.yMax - 22f, area.width - 16f, 18f);
            EditorGUI.LabelField(legend,
                "노랑 일반 · 빨강 엘리트 · 초록 성소 · 회색 이벤트 · 검정 보스",
                EditorStyles.miniLabel);
        }

        private Vector2 NodeCenter(
            MapNode node, Rect area, float padding, float columnSpacing, float rowSpacing)
        {
            float x = area.x + padding + (node.Stage - 1) * columnSpacing + node.JitterX * columnSpacing;
            float y = area.y + padding + node.Row * rowSpacing + node.JitterY * rowSpacing;
            return new Vector2(x, y);
        }

        private void DrawBatch()
        {
            EditorGUILayout.LabelField("일괄 검증", EditorStyles.boldLabel);
            _batchCount = EditorGUILayout.IntSlider("시드 개수", _batchCount, 10, 5000);

            using (new EditorGUI.DisabledScope(_config == null))
            {
                if (GUILayout.Button("여러 시드로 검증"))
                {
                    RunBatch();
                }
            }

            if (!string.IsNullOrEmpty(_batchReport))
            {
                EditorGUILayout.HelpBox(_batchReport, MessageType.None);
            }
        }

        private void RunBatch()
        {
            int failed = 0;
            int minNodes = int.MaxValue;
            int maxNodes = 0;
            int totalNodes = 0;
            Dictionary<RoomType, int> typeCounts = new Dictionary<RoomType, int>();
            StringBuilder failures = new StringBuilder();

            for (int i = 0; i < _batchCount; i++)
            {
                int seed = _seed + i;
                StageMap map = MapGenerator.Generate(seed, _stageIndex, _config);

                string error;
                if (!MapValidator.ValidateAll(map, _config, out error))
                {
                    failed++;
                    if (failed <= 5)
                    {
                        failures.AppendFormat("\n시드 {0}: {1}", seed, error.Replace("\n", " / "));
                    }

                    continue;
                }

                totalNodes += map.Nodes.Count;
                minNodes = Mathf.Min(minNodes, map.Nodes.Count);
                maxNodes = Mathf.Max(maxNodes, map.Nodes.Count);

                for (int n = 0; n < map.Nodes.Count; n++)
                {
                    RoomType roomType = map.Nodes[n].RoomType;
                    int count;
                    typeCounts.TryGetValue(roomType, out count);
                    typeCounts[roomType] = count + 1;
                }
            }

            int passed = _batchCount - failed;
            StringBuilder report = new StringBuilder();
            report.AppendFormat("시드 {0}개 중 {1}개 통과, {2}개 실패", _batchCount, passed, failed);

            if (passed > 0)
            {
                report.AppendFormat("\n노드 수 최소 {0} · 최대 {1} · 평균 {2:0.0}",
                    minNodes, maxNodes, totalNodes / (float)passed);
                report.Append("\n맵 하나당 평균 방 타입: ");
                foreach (KeyValuePair<RoomType, int> pair in typeCounts)
                {
                    report.AppendFormat("{0} {1:0.0}개  ", pair.Key, pair.Value / (float)passed);
                }
            }

            if (failures.Length > 0)
            {
                report.Append("\n실패 예시:");
                report.Append(failures);
            }

            _batchReport = report.ToString();
            Repaint();
        }
    }
}
