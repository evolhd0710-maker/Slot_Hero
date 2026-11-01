using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Map
{
    /// <summary>
    /// 맵 생성 규칙 기획서 v0.2 / 03 맵 구조 의 절차 ① ~ ⑧을 수행한다.
    /// 노드와 간선을 만든 뒤 <see cref="RoomTypeAssigner"/>로 방 타입을 배분한다.
    /// </summary>
    public static class MapGenerator
    {
        /// <summary>격자 위의 간선. 출발 단계와 두 줄 번호로 나타낸다.</summary>
        private struct GridEdge
        {
            public int Stage;
            public int FromRow;
            public int ToRow;

            public GridEdge(int stage, int fromRow, int toRow)
            {
                Stage = stage;
                FromRow = fromRow;
                ToRow = toRow;
            }
        }

        /// <summary>
        /// 시드와 설정으로 스테이지 맵 하나를 만든다.
        /// 같은 값을 넣으면 항상 같은 맵이 나온다.
        /// </summary>
        public static StageMap Generate(int runSeed, int stageIndex, MapGenerationConfig config)
        {
            if (config == null)
            {
                Debug.LogError("맵 생성 설정이 없다.");
                return null;
            }

            StageMap best = null;
            string lastError = null;

            for (int attempt = 0; attempt < config.MaxGenerationAttempts; attempt++)
            {
                // 재시도도 시드에서 결정되게 해 재현성을 유지한다.
                MapRandom random = MapRandom.ForStage(runSeed, stageIndex * 1000 + attempt);
                StageMap map = GenerateOnce(runSeed, stageIndex, config, random);

                string error;
                if (MapValidator.Validate(map, config, out error))
                {
                    return map;
                }

                lastError = error;
                best = map;
            }

            Debug.LogWarning(string.Format(
                "맵 생성이 {0}회 안에 보장 조건을 채우지 못했다. 마지막 결과를 그대로 쓴다. 사유: {1}",
                config.MaxGenerationAttempts, lastError));
            return best;
        }

        private static StageMap GenerateOnce(
            int runSeed, int stageIndex, MapGenerationConfig config, MapRandom random)
        {
            int stageCount = config.StageCount;
            int rowCount = config.RowCount;
            int pathEndStage = Mathf.Clamp(config.PathEndStage, 2, stageCount);

            // ① 격자 준비
            bool[,] occupied = new bool[stageCount + 1, rowCount];
            HashSet<GridEdge> gridEdges = new HashSet<GridEdge>(new GridEdgeComparer());

            // ② ~ ⑤ 경로 그리기
            DrawPaths(config, random, pathEndStage, occupied, gridEdges);

            // ⑦ 교차 정리
            if (config.RemoveEdgeCrossings)
            {
                RemoveCrossings(pathEndStage, gridEdges);
            }

            // ⑥ 노드 확정
            StageMap map = new StageMap
            {
                Seed = runSeed,
                StageIndex = stageIndex,
                StageCount = stageCount,
                RowCount = rowCount,
            };

            int[,] nodeIdByGrid = new int[stageCount + 1, rowCount];
            for (int s = 0; s <= stageCount; s++)
            {
                for (int r = 0; r < rowCount; r++)
                {
                    nodeIdByGrid[s, r] = -1;
                }
            }

            int nextId = 0;
            for (int stage = 1; stage <= pathEndStage; stage++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    if (!occupied[stage, row])
                    {
                        continue;
                    }

                    MapNode node = new MapNode
                    {
                        Id = nextId++,
                        Stage = stage,
                        Row = row,
                        RoomType = RoomType.None,
                    };
                    map.Nodes.Add(node);
                    nodeIdByGrid[stage, row] = node.Id;
                }
            }

            map.RebuildIndex();

            foreach (GridEdge gridEdge in gridEdges)
            {
                int fromId = nodeIdByGrid[gridEdge.Stage, gridEdge.FromRow];
                int toId = nodeIdByGrid[gridEdge.Stage + 1, gridEdge.ToRow];
                if (fromId < 0 || toId < 0)
                {
                    continue;
                }

                AddEdge(map, fromId, toId);
            }

            // 경로 생성 구간 뒤의 고정 단일 노드. 기본값에서는 11단계 성소와 12단계 보스이다.
            int centerRow = rowCount / 2;
            for (int stage = pathEndStage + 1; stage <= stageCount; stage++)
            {
                MapNode node = new MapNode
                {
                    Id = nextId++,
                    Stage = stage,
                    Row = centerRow,
                    RoomType = RoomType.None,
                };
                map.Nodes.Add(node);
                nodeIdByGrid[stage, centerRow] = node.Id;
                map.RebuildIndex();

                IReadOnlyList<MapNode> previous = map.GetNodesOfStage(stage - 1);
                for (int i = 0; i < previous.Count; i++)
                {
                    AddEdge(map, previous[i].Id, node.Id);
                }
            }

            map.RebuildIndex();
            SortEdgeLists(map);

            // ⑧ 노드 지터
            if (config.ApplyJitter)
            {
                ApplyJitter(map, config, random);
            }

            // 06 배치 규칙 ① ~ ⑫
            RoomTypeAssigner.Assign(map, config, random);

            return map;
        }

        /// <summary>② ~ ⑤ 경로 그리기. 1회차부터 지정된 횟수만큼 왼쪽에서 오른쪽으로 걷는다.</summary>
        private static void DrawPaths(
            MapGenerationConfig config,
            MapRandom random,
            int pathEndStage,
            bool[,] occupied,
            HashSet<GridEdge> gridEdges)
        {
            int rowCount = config.RowCount;
            List<int> usedStartRows = new List<int>();
            HashSet<int> firstPathPoints = new HashSet<int>();
            List<int> candidates = new List<int>(rowCount);
            List<int> filtered = new List<int>(rowCount);

            for (int run = 0; run < config.PathCount; run++)
            {
                // ② 1회차는 아무 줄에서나, ③ ④ 2 ~ 3회차는 아직 쓰지 않은 시작 줄에서 시작한다.
                bool needsFreshStart = run == 1 || run == 2;
                int startRow = PickStartRow(random, rowCount, usedStartRows, needsFreshStart);
                if (!usedStartRows.Contains(startRow))
                {
                    usedStartRows.Add(startRow);
                }

                // ③ 2회차는 1회차가 지난 노드를 피해 각 단계 최소 2개 노드를 보장한다.
                bool avoidFirstPath = run == 1 && config.SecondPathAvoidsFirst;

                int row = startRow;
                occupied[1, row] = true;
                if (run == 0)
                {
                    firstPathPoints.Add(PointKey(1, row, rowCount));
                }

                for (int stage = 1; stage < pathEndStage; stage++)
                {
                    candidates.Clear();
                    int minRow = Mathf.Max(0, row - config.MaxRowStep);
                    int maxRow = Mathf.Min(rowCount - 1, row + config.MaxRowStep);
                    for (int r = minRow; r <= maxRow; r++)
                    {
                        candidates.Add(r);
                    }

                    List<int> pickFrom = candidates;
                    if (avoidFirstPath)
                    {
                        filtered.Clear();
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            if (!firstPathPoints.Contains(PointKey(stage + 1, candidates[i], rowCount)))
                            {
                                filtered.Add(candidates[i]);
                            }
                        }

                        // 피할 수 없는 상황에서는 제약을 풀어 경로가 끊기지 않게 한다.
                        if (filtered.Count > 0)
                        {
                            pickFrom = filtered;
                        }
                    }

                    int nextRow = pickFrom[random.Range(0, pickFrom.Count)];
                    gridEdges.Add(new GridEdge(stage, row, nextRow));
                    occupied[stage + 1, nextRow] = true;
                    if (run == 0)
                    {
                        firstPathPoints.Add(PointKey(stage + 1, nextRow, rowCount));
                    }

                    row = nextRow;
                }
            }
        }

        private static int PickStartRow(
            MapRandom random, int rowCount, List<int> usedStartRows, bool needsFreshStart)
        {
            if (!needsFreshStart)
            {
                return random.Range(0, rowCount);
            }

            List<int> free = new List<int>(rowCount);
            for (int r = 0; r < rowCount; r++)
            {
                if (!usedStartRows.Contains(r))
                {
                    free.Add(r);
                }
            }

            if (free.Count == 0)
            {
                return random.Range(0, rowCount);
            }

            return free[random.Range(0, free.Count)];
        }

        /// <summary>
        /// ⑦ 교차 정리. 각 단계에서 출발 줄과 도착 줄을 각각 정렬한 뒤 같은 차례끼리 다시 잇는다.
        /// 출발 줄과 도착 줄의 구성이 그대로이므로 노드는 하나도 사라지지 않는다.
        /// </summary>
        private static void RemoveCrossings(int pathEndStage, HashSet<GridEdge> gridEdges)
        {
            List<GridEdge> all = new List<GridEdge>(gridEdges);
            gridEdges.Clear();

            List<int> fromRows = new List<int>();
            List<int> toRows = new List<int>();

            for (int stage = 1; stage < pathEndStage; stage++)
            {
                fromRows.Clear();
                toRows.Clear();

                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].Stage != stage)
                    {
                        continue;
                    }

                    fromRows.Add(all[i].FromRow);
                    toRows.Add(all[i].ToRow);
                }

                fromRows.Sort();
                toRows.Sort();

                for (int i = 0; i < fromRows.Count; i++)
                {
                    gridEdges.Add(new GridEdge(stage, fromRows[i], toRows[i]));
                }
            }
        }

        /// <summary>⑧ 노드 지터. 격자 간격을 1로 본 상대값으로 저장한다.</summary>
        private static void ApplyJitter(StageMap map, MapGenerationConfig config, MapRandom random)
        {
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];
                node.JitterX = random.NextSignedFloat() * config.JitterX;
                node.JitterY = random.NextSignedFloat() * config.JitterY;
            }
        }

        private static void AddEdge(StageMap map, int fromId, int toId)
        {
            MapEdge edge = new MapEdge(fromId, toId);
            if (map.Edges.Contains(edge))
            {
                return;
            }

            map.Edges.Add(edge);

            MapNode from = map.GetNode(fromId);
            MapNode to = map.GetNode(toId);
            if (from != null && !from.NextNodeIds.Contains(toId))
            {
                from.NextNodeIds.Add(toId);
            }

            if (to != null && !to.PrevNodeIds.Contains(fromId))
            {
                to.PrevNodeIds.Add(fromId);
            }
        }

        /// <summary>간선 목록을 줄 순서로 정렬해 표시와 순회 순서를 항상 같게 만든다.</summary>
        private static void SortEdgeLists(StageMap map)
        {
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                MapNode node = map.Nodes[i];
                node.NextNodeIds.Sort((a, b) => CompareByRow(map, a, b));
                node.PrevNodeIds.Sort((a, b) => CompareByRow(map, a, b));
            }

            map.Edges.Sort((a, b) =>
            {
                int byFrom = a.FromNodeId.CompareTo(b.FromNodeId);
                return byFrom != 0 ? byFrom : a.ToNodeId.CompareTo(b.ToNodeId);
            });
        }

        private static int CompareByRow(StageMap map, int idA, int idB)
        {
            MapNode a = map.GetNode(idA);
            MapNode b = map.GetNode(idB);
            if (a == null || b == null)
            {
                return idA.CompareTo(idB);
            }

            int byRow = a.Row.CompareTo(b.Row);
            return byRow != 0 ? byRow : idA.CompareTo(idB);
        }

        private static int PointKey(int stage, int row, int rowCount)
        {
            return stage * rowCount + row;
        }

        private sealed class GridEdgeComparer : IEqualityComparer<GridEdge>
        {
            public bool Equals(GridEdge a, GridEdge b)
            {
                return a.Stage == b.Stage && a.FromRow == b.FromRow && a.ToRow == b.ToRow;
            }

            public int GetHashCode(GridEdge edge)
            {
                unchecked
                {
                    int hash = edge.Stage;
                    hash = (hash * 397) ^ edge.FromRow;
                    hash = (hash * 397) ^ edge.ToRow;
                    return hash;
                }
            }
        }
    }
}
