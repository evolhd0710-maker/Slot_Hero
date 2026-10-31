using System.Collections.Generic;
using System.Text;

namespace SlotHero.Map
{
    /// <summary>
    /// 생성된 맵이 기획서의 보장 조건을 지키는지 확인한다.
    /// 생성 중에는 재시도 판정에, 에디터에서는 일괄 검증에 쓴다.
    /// </summary>
    public static class MapValidator
    {
        /// <summary>구조 조건만 확인한다. 방 타입 배분 전에도 쓸 수 있다.</summary>
        public static bool Validate(StageMap map, MapGenerationConfig config, out string error)
        {
            List<string> errors = new List<string>();
            ValidateStructure(map, config, errors);

            error = Join(errors);
            return errors.Count == 0;
        }

        /// <summary>구조와 방 타입 배분을 모두 확인한다.</summary>
        public static bool ValidateAll(StageMap map, MapGenerationConfig config, out string error)
        {
            List<string> errors = new List<string>();
            ValidateStructure(map, config, errors);
            ValidateRoomTypes(map, config, errors);

            error = Join(errors);
            return errors.Count == 0;
        }

        private static void ValidateStructure(StageMap map, MapGenerationConfig config, List<string> errors)
        {
            if (map == null)
            {
                errors.Add("맵이 없다.");
                return;
            }

            if (map.Nodes.Count == 0)
            {
                errors.Add("노드가 하나도 없다.");
                return;
            }

            // 단계당 노드 수
            for (int stage = 1; stage <= config.PathEndStage; stage++)
            {
                int count = map.GetNodesOfStage(stage).Count;
                if (count < config.MinNodesPerStage)
                {
                    errors.Add(string.Format("{0}단계의 노드가 {1}개로 최소 {2}개에 못 미친다.",
                        stage, count, config.MinNodesPerStage));
                }

                if (count > config.MaxNodesPerStage)
                {
                    errors.Add(string.Format("{0}단계의 노드가 {1}개로 최대 {2}개를 넘는다.",
                        stage, count, config.MaxNodesPerStage));
                }
            }

            // 경로 생성 구간 뒤는 단일 노드
            for (int stage = config.PathEndStage + 1; stage <= config.StageCount; stage++)
            {
                int count = map.GetNodesOfStage(stage).Count;
                if (count != 1)
                {
                    errors.Add(string.Format("{0}단계는 고정 단일 노드여야 하는데 {1}개이다.", stage, count));
                }
            }

            // 시작 노드 수
            int startCount = map.GetStartNodes().Count;
            if (startCount < config.MinStartNodes || startCount > config.MaxStartNodes)
            {
                errors.Add(string.Format("시작 노드가 {0}개로 {1} ~ {2}개 범위를 벗어난다.",
                    startCount, config.MinStartNodes, config.MaxStartNodes));
            }

            // 모든 노드가 앞뒤로 이어져야 한다
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];
                if (node.Stage > 1 && node.PrevNodeIds.Count == 0)
                {
                    errors.Add(string.Format("{0}로 들어오는 간선이 없다.", node));
                }

                if (node.Stage < config.StageCount && node.NextNodeIds.Count == 0)
                {
                    errors.Add(string.Format("{0}에서 나가는 간선이 없다.", node));
                }

                // 간선은 항상 다음 단계로만 이어진다
                for (int n = 0; n < node.NextNodeIds.Count; n++)
                {
                    MapNode next = map.GetNode(node.NextNodeIds[n]);
                    if (next != null && next.Stage != node.Stage + 1)
                    {
                        errors.Add(string.Format("{0}의 간선이 다음 단계가 아닌 {1}로 이어진다.", node, next));
                    }
                }

                // 경로 생성 구간에서 이동 폭을 넘지 않아야 한다
                if (node.Stage < config.PathEndStage)
                {
                    for (int n = 0; n < node.NextNodeIds.Count; n++)
                    {
                        MapNode next = map.GetNode(node.NextNodeIds[n]);
                        if (next == null)
                        {
                            continue;
                        }

                        int delta = next.Row - node.Row;
                        if (delta < -config.MaxRowStep || delta > config.MaxRowStep)
                        {
                            errors.Add(string.Format("{0}에서 {1}로 가는 간선의 이동 폭이 {2}로 제한을 넘는다.",
                                node, next, delta));
                        }
                    }
                }
            }

            // 모든 시작 노드에서 보스까지 닿아야 한다
            MapNode boss = map.GetBossNode();
            if (boss == null)
            {
                errors.Add("보스 노드가 없다.");
            }
            else
            {
                HashSet<int> reachable = CollectReachable(map);
                for (int i = 0; i < map.Nodes.Count; i++)
                {
                    if (!reachable.Contains(map.Nodes[i].Id))
                    {
                        errors.Add(string.Format("{0}에 시작 노드에서 닿을 수 없다.", map.Nodes[i]));
                    }
                }
            }
        }

        private static void ValidateRoomTypes(StageMap map, MapGenerationConfig config, List<string> errors)
        {
            if (map == null)
            {
                return;
            }

            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];

                if (node.RoomType == RoomType.None)
                {
                    errors.Add(string.Format("{0}의 방 타입이 정해지지 않았다.", node));
                    continue;
                }

                if (config.IsFixedNormalStage(node.Stage) && node.RoomType != RoomType.Normal)
                {
                    errors.Add(string.Format("{0}은 일반 몬스터 고정 단계인데 {1}이다.", node, node.RoomType));
                }

                if (node.Stage == config.FixedSanctumStage && node.RoomType != RoomType.Sanctum)
                {
                    errors.Add(string.Format("{0}은 성소 고정 단계인데 {1}이다.", node, node.RoomType));
                }

                if (node.Stage == config.BossStage && node.RoomType != RoomType.Boss)
                {
                    errors.Add(string.Format("{0}은 보스 단계인데 {1}이다.", node, node.RoomType));
                }

                // 연속 불가 타입 검사
                if (config.IsNonConsecutive(node.RoomType))
                {
                    for (int n = 0; n < node.NextNodeIds.Count; n++)
                    {
                        MapNode next = map.GetNode(node.NextNodeIds[n]);
                        if (next != null && next.RoomType == node.RoomType)
                        {
                            errors.Add(string.Format("{0}과 {1}이 같은 타입으로 이어져 있다.", node, next));
                        }
                    }
                }
            }

            // 중반 구간에 성소로 가는 경로가 하나는 있어야 한다
            bool hasMidSanctum = false;
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];
                if (node.RoomType == RoomType.Sanctum
                    && node.Stage >= config.MidSanctumStageMin
                    && node.Stage <= config.MidSanctumStageMax)
                {
                    hasMidSanctum = true;
                    break;
                }
            }

            if (!hasMidSanctum)
            {
                errors.Add(string.Format("{0} ~ {1}단계에 성소가 하나도 없다.",
                    config.MidSanctumStageMin, config.MidSanctumStageMax));
            }
        }

        /// <summary>시작 노드에서 앞으로 나아가며 닿을 수 있는 노드를 모은다.</summary>
        public static HashSet<int> CollectReachable(StageMap map)
        {
            HashSet<int> reachable = new HashSet<int>();
            Queue<int> queue = new Queue<int>();

            IReadOnlyList<MapNode> startNodes = map.GetStartNodes();
            for (int i = 0; i < startNodes.Count; i++)
            {
                if (reachable.Add(startNodes[i].Id))
                {
                    queue.Enqueue(startNodes[i].Id);
                }
            }

            while (queue.Count > 0)
            {
                MapNode node = map.GetNode(queue.Dequeue());
                if (node == null)
                {
                    continue;
                }

                for (int i = 0; i < node.NextNodeIds.Count; i++)
                {
                    if (reachable.Add(node.NextNodeIds[i]))
                    {
                        queue.Enqueue(node.NextNodeIds[i]);
                    }
                }
            }

            return reachable;
        }

        private static string Join(List<string> errors)
        {
            if (errors.Count == 0)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < errors.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(errors[i]);
            }

            return builder.ToString();
        }
    }
}
