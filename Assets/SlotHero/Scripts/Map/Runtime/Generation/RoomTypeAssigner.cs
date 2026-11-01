using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Map
{
    /// <summary>
    /// 맵 생성 규칙 기획서 v0.2 / 06 배치 규칙 의 절차 ① ~ ⑫를 수행한다.
    /// 노드와 간선이 모두 만들어진 뒤에 호출한다.
    /// </summary>
    public static class RoomTypeAssigner
    {
        public static void Assign(StageMap map, MapGenerationConfig config, MapRandom random)
        {
            if (map == null || config == null || random == null)
            {
                return;
            }

            // ① 고정 단계 배치
            AssignFixedStages(map, config);

            // ② ~ ⑥ 중간 성소 배치
            PlaceMidSanctums(map, config, random);

            // ⑦ 배분 대상 노드 개수 확인
            List<MapNode> targets = CollectUnassigned(map);
            int targetCount = targets.Count;
            if (targetCount == 0)
            {
                return;
            }

            // ⑧ 토큰 수 계산
            int tokenCount = ChooseTokenCount(targetCount, config);

            // ⑨ 타입별 토큰 분배
            Dictionary<RoomType, int> shares = DistributeShares(tokenCount, config);

            // ⑩ 연속 불가 타입 우선 배치
            Dictionary<RoomType, int> preplaced = PlaceNonConsecutiveFirst(map, config, random, targets, shares);

            // ⑨ 나머지 토큰을 풀에 넣고 섞는다
            List<RoomType> pool = BuildPool(config, shares, preplaced);
            random.Shuffle(pool);

            // ⑪ 나머지 노드 배치
            FillRemaining(map, config, random, targets, pool);

            // ⑫ 풀 초기화
            pool.Clear();
        }

        /// <summary>① 1, 2단계는 일반 몬스터, 11단계는 성소, 12단계는 보스로 고정한다.</summary>
        private static void AssignFixedStages(StageMap map, MapGenerationConfig config)
        {
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];
                if (config.IsFixedNormalStage(node.Stage))
                {
                    node.RoomType = RoomType.Normal;
                }
                else if (node.Stage == config.FixedSanctumStage)
                {
                    node.RoomType = RoomType.Sanctum;
                }
                else if (node.Stage == config.BossStage)
                {
                    node.RoomType = RoomType.Boss;
                }
                else
                {
                    node.RoomType = RoomType.None;
                }
            }
        }

        /// <summary>
        /// ② ~ ⑥ 중간 성소 배치.
        /// 구간 안에서 간선이 가장 많은 노드부터 성소를 놓고,
        /// 그 성소를 지나는 경로 위의 노드는 후보에서 빼며 후보가 없어질 때까지 반복한다.
        /// </summary>
        private static void PlaceMidSanctums(StageMap map, MapGenerationConfig config, MapRandom random)
        {
            // ② 구간의 노드를 후보로 모은다
            List<MapNode> candidates = new List<MapNode>();
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];
                if (node.RoomType != RoomType.None)
                {
                    continue;
                }

                if (node.Stage >= config.MidSanctumStageMin && node.Stage <= config.MidSanctumStageMax)
                {
                    candidates.Add(node);
                }
            }

            HashSet<int> stagesWithSanctum = new HashSet<int>();
            List<MapNode> tied = new List<MapNode>();

            while (candidates.Count > 0)
            {
                // ③ 첫 성소는 간선이 가장 많은 노드에, ⑤ 이후로는 성소가 없는 단계를 먼저 본다
                MapNode chosen = SelectSanctumTarget(candidates, stagesWithSanctum, random, tied);
                if (chosen == null)
                {
                    break;
                }

                chosen.RoomType = RoomType.Sanctum;
                stagesWithSanctum.Add(chosen.Stage);

                // ④ 배치한 성소를 지나는 경로 위의 노드를 후보에서 제거한다
                HashSet<int> onPath = CollectNodesOnPathsThrough(map, chosen);
                for (int i = candidates.Count - 1; i >= 0; i--)
                {
                    if (onPath.Contains(candidates[i].Id))
                    {
                        candidates.RemoveAt(i);
                    }
                }
            }
        }

        private static MapNode SelectSanctumTarget(
            List<MapNode> candidates,
            HashSet<int> stagesWithSanctum,
            MapRandom random,
            List<MapNode> tied)
        {
            tied.Clear();

            // 우선순위 1. 성소가 아직 배치되지 않은 단계
            bool preferEmptyStage = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!stagesWithSanctum.Contains(candidates[i].Stage))
                {
                    preferEmptyStage = true;
                    break;
                }
            }

            // 우선순위 2. 간선이 많은 노드
            int bestEdgeCount = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                MapNode node = candidates[i];
                if (preferEmptyStage && stagesWithSanctum.Contains(node.Stage))
                {
                    continue;
                }

                if (node.EdgeCount > bestEdgeCount)
                {
                    bestEdgeCount = node.EdgeCount;
                    tied.Clear();
                    tied.Add(node);
                }
                else if (node.EdgeCount == bestEdgeCount)
                {
                    tied.Add(node);
                }
            }

            if (tied.Count == 0)
            {
                return null;
            }

            // 간선 수가 같으면 그중 하나를 시드 난수로 고른다
            return tied[random.Range(0, tied.Count)];
        }

        /// <summary>해당 노드를 지나는 모든 경로 위의 노드 번호를 모은다. 자기 자신도 포함한다.</summary>
        private static HashSet<int> CollectNodesOnPathsThrough(StageMap map, MapNode node)
        {
            HashSet<int> result = new HashSet<int> { node.Id };
            Queue<int> queue = new Queue<int>();

            // 앞쪽으로
            queue.Enqueue(node.Id);
            while (queue.Count > 0)
            {
                MapNode current = map.GetNode(queue.Dequeue());
                if (current == null)
                {
                    continue;
                }

                for (int i = 0; i < current.NextNodeIds.Count; i++)
                {
                    int nextId = current.NextNodeIds[i];
                    if (result.Add(nextId))
                    {
                        queue.Enqueue(nextId);
                    }
                }
            }

            // 뒤쪽으로
            queue.Enqueue(node.Id);
            while (queue.Count > 0)
            {
                MapNode current = map.GetNode(queue.Dequeue());
                if (current == null)
                {
                    continue;
                }

                for (int i = 0; i < current.PrevNodeIds.Count; i++)
                {
                    int prevId = current.PrevNodeIds[i];
                    if (result.Add(prevId))
                    {
                        queue.Enqueue(prevId);
                    }
                }
            }

            return result;
        }

        /// <summary>⑦ 아직 방 타입이 정해지지 않은 노드를 모은다.</summary>
        private static List<MapNode> CollectUnassigned(StageMap map)
        {
            List<MapNode> result = new List<MapNode>();
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                if (map.Nodes[i].RoomType == RoomType.None)
                {
                    result.Add(map.Nodes[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// ⑧ 토큰 수 계산.
        /// N의 지정 배수 범위에서 각 타입의 몫이 정수가 되는 가장 작은 수를 고르고,
        /// 그런 수가 없으면 소수점 아래의 합이 가장 작은 수를 고른다.
        /// </summary>
        public static int ChooseTokenCount(int nodeCount, MapGenerationConfig config)
        {
            if (nodeCount <= 0)
            {
                return 0;
            }

            int totalWeight = config.TotalWeight;
            if (totalWeight <= 0)
            {
                return nodeCount;
            }

            int min = Mathf.CeilToInt(nodeCount * config.TokenMultiplierMin);
            int max = Mathf.FloorToInt(nodeCount * config.TokenMultiplierMax);
            if (max < min)
            {
                max = min;
            }

            int best = min;
            float bestFractionSum = float.MaxValue;

            for (int tokens = min; tokens <= max; tokens++)
            {
                float fractionSum = 0f;
                for (int i = 0; i < config.RoomTypeWeights.Count; i++)
                {
                    int weight = Mathf.Max(0, config.RoomTypeWeights[i].Weight);
                    if (weight == 0)
                    {
                        continue;
                    }

                    float exact = tokens * weight / (float)totalWeight;
                    fractionSum += exact - Mathf.Floor(exact);
                }

                // 모든 몫이 정수인 가장 작은 값을 바로 고른다
                if (fractionSum < 0.0001f)
                {
                    return tokens;
                }

                if (fractionSum < bestFractionSum - 0.0001f)
                {
                    bestFractionSum = fractionSum;
                    best = tokens;
                }
            }

            return best;
        }

        /// <summary>⑨ 가중치에 비례해 토큰을 나눈다. 나머지는 소수점 아래가 큰 타입부터 준다.</summary>
        public static Dictionary<RoomType, int> DistributeShares(int tokenCount, MapGenerationConfig config)
        {
            Dictionary<RoomType, int> shares = new Dictionary<RoomType, int>();
            int totalWeight = config.TotalWeight;
            if (tokenCount <= 0 || totalWeight <= 0)
            {
                return shares;
            }

            List<RoomTypeWeight> weights = config.RoomTypeWeights;
            int assigned = 0;
            List<int> order = new List<int>(weights.Count);

            for (int i = 0; i < weights.Count; i++)
            {
                int weight = Mathf.Max(0, weights[i].Weight);
                int share = Mathf.FloorToInt(tokenCount * weight / (float)totalWeight);
                shares[weights[i].RoomType] = share;
                assigned += share;
                order.Add(i);
            }

            // 소수점 아래가 큰 순서, 같으면 가중치가 큰 순서, 그래도 같으면 목록 순서
            order.Sort((a, b) =>
            {
                float fractionA = Fraction(tokenCount, Mathf.Max(0, weights[a].Weight), totalWeight);
                float fractionB = Fraction(tokenCount, Mathf.Max(0, weights[b].Weight), totalWeight);
                int byFraction = fractionB.CompareTo(fractionA);
                if (byFraction != 0)
                {
                    return byFraction;
                }

                int byWeight = weights[b].Weight.CompareTo(weights[a].Weight);
                return byWeight != 0 ? byWeight : a.CompareTo(b);
            });

            int remainder = tokenCount - assigned;
            for (int i = 0; i < order.Count && remainder > 0; i++)
            {
                RoomTypeWeight entry = weights[order[i]];
                if (entry.Weight <= 0)
                {
                    continue;
                }

                shares[entry.RoomType] = shares[entry.RoomType] + 1;
                remainder--;
            }

            return shares;
        }

        private static float Fraction(int tokenCount, int weight, int totalWeight)
        {
            float exact = tokenCount * weight / (float)totalWeight;
            return exact - Mathf.Floor(exact);
        }

        /// <summary>
        /// ⑩ 연속 배치가 불가능한 타입을 먼저 배치한다.
        /// 몫에서 예약분을 뺀 만큼만 놓고, 예약분은 ⑪의 풀에 남긴다.
        /// 반환값은 타입별로 실제로 미리 배치한 개수이다.
        /// </summary>
        private static Dictionary<RoomType, int> PlaceNonConsecutiveFirst(
            StageMap map,
            MapGenerationConfig config,
            MapRandom random,
            List<MapNode> targets,
            Dictionary<RoomType, int> shares)
        {
            Dictionary<RoomType, int> placedCounts = new Dictionary<RoomType, int>();
            if (config.NonConsecutiveTypes == null)
            {
                return placedCounts;
            }

            List<MapNode> available = new List<MapNode>();

            // 목록 순서대로 배치한다. 기본값은 성소 먼저, 그다음 엘리트이다.
            for (int t = 0; t < config.NonConsecutiveTypes.Length; t++)
            {
                RoomType roomType = config.NonConsecutiveTypes[t];
                int share;
                if (!shares.TryGetValue(roomType, out share))
                {
                    continue;
                }

                int reserved = Mathf.Min(share, config.ReservedTokensPerNonConsecutiveType);
                int toPlace = share - reserved;
                int placed = 0;

                while (placed < toPlace)
                {
                    available.Clear();
                    for (int i = 0; i < targets.Count; i++)
                    {
                        MapNode node = targets[i];
                        if (node.RoomType == RoomType.None && !HasNeighborOfType(map, node, roomType))
                        {
                            available.Add(node);
                        }
                    }

                    if (available.Count == 0)
                    {
                        break;
                    }

                    MapNode chosen = available[random.Range(0, available.Count)];
                    chosen.RoomType = roomType;
                    placed++;
                }

                placedCounts[roomType] = placed;
            }

            return placedCounts;
        }

        /// <summary>⑨ 미리 배치하고 남은 토큰을 모두 풀에 담는다.</summary>
        private static List<RoomType> BuildPool(
            MapGenerationConfig config,
            Dictionary<RoomType, int> shares,
            Dictionary<RoomType, int> preplaced)
        {
            // 재현성을 위해 사전이 아니라 설정의 목록 순서대로 담는다.
            List<RoomType> pool = new List<RoomType>();
            for (int i = 0; i < config.RoomTypeWeights.Count; i++)
            {
                RoomType roomType = config.RoomTypeWeights[i].RoomType;

                int share;
                if (!shares.TryGetValue(roomType, out share))
                {
                    continue;
                }

                int alreadyPlaced;
                preplaced.TryGetValue(roomType, out alreadyPlaced);

                int remaining = share - alreadyPlaced;
                for (int t = 0; t < remaining; t++)
                {
                    pool.Add(roomType);
                }
            }

            return pool;
        }

        /// <summary>
        /// ⑪ 남은 노드에 풀에서 토큰을 꺼내 배치한다.
        /// 연속 불가 타입이 걸리면 풀에 되돌리고 다음 토큰을 꺼내며, 배치 후 풀을 다시 섞는다.
        /// </summary>
        private static void FillRemaining(
            StageMap map,
            MapGenerationConfig config,
            MapRandom random,
            List<MapNode> targets,
            List<RoomType> pool)
        {
            List<RoomType> returned = new List<RoomType>();

            for (int i = 0; i < targets.Count; i++)
            {
                MapNode node = targets[i];
                if (node.RoomType != RoomType.None)
                {
                    continue;
                }

                RoomType chosen = RoomType.None;
                returned.Clear();

                while (pool.Count > 0)
                {
                    int last = pool.Count - 1;
                    RoomType token = pool[last];
                    pool.RemoveAt(last);

                    if (config.IsNonConsecutive(token) && HasNeighborOfType(map, node, token))
                    {
                        returned.Add(token);
                        continue;
                    }

                    chosen = token;
                    break;
                }

                pool.AddRange(returned);

                if (chosen == RoomType.None)
                {
                    // 풀이 비었거나 남은 토큰이 모두 연속 불가로 막힌 경우의 대비책이다.
                    chosen = RoomType.Normal;
                    Debug.LogWarning(string.Format(
                        "{0}에 배치할 토큰을 찾지 못해 일반 몬스터로 채웠다.", node));
                }

                node.RoomType = chosen;
                random.Shuffle(pool);
            }
        }

        /// <summary>간선으로 이어진 노드 중에 같은 타입이 있는지 확인한다.</summary>
        public static bool HasNeighborOfType(StageMap map, MapNode node, RoomType roomType)
        {
            for (int i = 0; i < node.PrevNodeIds.Count; i++)
            {
                MapNode neighbor = map.GetNode(node.PrevNodeIds[i]);
                if (neighbor != null && neighbor.RoomType == roomType)
                {
                    return true;
                }
            }

            for (int i = 0; i < node.NextNodeIds.Count; i++)
            {
                MapNode neighbor = map.GetNode(node.NextNodeIds[i]);
                if (neighbor != null && neighbor.RoomType == roomType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
