using System;
using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 런 하나에서 유물이 나올 순서.
    /// 런을 시작할 때 유물 전체를 시드 난수로 한 번 섞어 테이블을 만들고,
    /// 행상에 진열할 때마다 그 순서대로 앞에서부터 꺼낸다.
    /// 이미 가지고 있어 나올 수 없는 유물은 건너뛴다.
    ///
    /// 테이블을 다 쓰면 다음 테이블을 이어서 준비한다.
    /// 테이블 번호가 바뀌면 같은 런 시드라도 섞이는 순서가 달라지므로
    /// 새로고침을 아무리 많이 해도 진열이 끊기지 않는다.
    ///
    /// 진열할 때마다 새로 뽑지 않고 미리 정해 둔 순서를 쓰므로
    /// 같은 유물이 여러 성소에 거듭 나오지 않고 새로고침해도 앞의 것이 다시 나오지 않는다.
    ///
    /// 유지 단위가 런이라 저장 시스템 기획서의 런 데이터에 그대로 실린다.
    /// </summary>
    [Serializable]
    public class RunRelicPool
    {
        /// <summary>한 번 꺼낼 때 새로 준비할 수 있는 테이블 수의 상한. 끝없이 도는 것을 막는다.</summary>
        private const int MaxTablesPerDraw = 8;

        /// <summary>런 시드. 다음 테이블을 준비할 때 쓴다.</summary>
        public int RunSeed;

        /// <summary>지금 쓰고 있는 테이블 번호. 1부터 센다.</summary>
        public int TableIndex;

        /// <summary>지금 테이블에서 유물이 나올 순서. 유물 식별자를 섞어 담는다.</summary>
        public List<string> Order = new List<string>();

        /// <summary>다음에 꺼낼 자리. 여기까지는 이미 지나간 것이다.</summary>
        public int NextIndex;

        /// <summary>런을 시작할 때 첫 테이블을 뽑는다.</summary>
        public static RunRelicPool Build(int runSeed, IMerchantCatalog catalog)
        {
            RunRelicPool pool = new RunRelicPool();
            pool.RunSeed = runSeed;
            pool.TableIndex = 0;
            pool.PrepareNextTable(catalog);
            return pool;
        }

        /// <summary>
        /// 저장해 둔 자리에서 다시 이어 간다. 이어하기가 쓴다.
        ///
        /// 순서만 시드에서 다시 만들면 안 된다. 어디까지 꺼냈는지는 새로고침을 몇 번 했는지에 달려 있어
        /// 시드로는 알 수 없다. 처음부터 다시 꺼내면 이미 본 유물이 또 나온다.
        /// 저장된 것이 없으면 처음부터 만든다.
        /// </summary>
        public static RunRelicPool Restore(
            int runSeed, int tableIndex, int nextIndex, IList<string> order, IMerchantCatalog catalog)
        {
            if (tableIndex <= 0 || order == null || order.Count == 0)
            {
                return Build(runSeed, catalog);
            }

            RunRelicPool pool = new RunRelicPool();
            pool.RunSeed = runSeed;
            pool.TableIndex = tableIndex;
            pool.Order = new List<string>(order);
            pool.NextIndex = nextIndex < 0 ? 0 : (nextIndex > pool.Order.Count ? pool.Order.Count : nextIndex);
            return pool;
        }

        /// <summary>지금 테이블에서 아직 지나가지 않은 자리 수.</summary>
        public int RemainingInTable
        {
            get
            {
                int left = Order.Count - NextIndex;
                return left > 0 ? left : 0;
            }
        }

        /// <summary>
        /// 다음 테이블을 준비한다. 테이블 번호를 올리고 그 번호로 순서를 다시 섞는다.
        /// 올릴 유물이 하나도 없으면 아무것도 하지 않고 false 를 돌려준다.
        /// </summary>
        public bool PrepareNextTable(IMerchantCatalog catalog)
        {
            if (catalog == null)
            {
                return false;
            }

            List<MerchantItem> all = new List<MerchantItem>();
            catalog.CollectAllRelics(all);

            List<string> next = new List<string>();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].IsValid)
                {
                    next.Add(all[i].Id);
                }
            }

            if (next.Count == 0)
            {
                return false;
            }

            TableIndex++;

            SanctumRandom random = SanctumRandom.ForRelicPool(RunSeed, TableIndex);
            random.Shuffle(next);

            Order = next;
            NextIndex = 0;
            return true;
        }

        /// <summary>
        /// 순서대로 유물을 count 개 꺼낸다.
        /// 이미 가지고 있는 유물은 건너뛰고 그 자리도 지나간 것으로 친다.
        /// 지금 테이블을 다 쓰면 다음 테이블을 준비해 이어서 꺼낸다.
        ///
        /// 테이블이 넘어가 이번에 이미 담은 유물을 다시 만나면
        /// 그 유물은 지나간 것으로 치지 않고 테이블 맨 뒤로 미룬다.
        /// 한 진열에 같은 유물이 두 번 오르지 않게 하면서도 그 유물의 차례를 잃지 않기 위해서다.
        ///
        /// 가지고 있지 않은 유물이 아예 없으면 하나도 꺼내지 않는다.
        /// </summary>
        public int Draw(IMerchantCatalog catalog, int count, List<MerchantItem> into)
        {
            return Draw(catalog, count, into, null);
        }

        /// <summary>
        /// 순서대로 유물을 count 개 꺼낸다. 희귀도 분포(weights)가 있으면 자리마다 등급부터 굴린다.
        ///
        /// 분포가 있으면 지금 테이블에 남은 나올 수 있는 유물 가운데 굴린 등급의 것을 **순서가 가장 앞선 것부터** 꺼낸다.
        /// 꺼낸 것은 지나간 자리로 옮기고, 건너뛴 다른 등급의 유물은 차례를 잃지 않고 테이블에 남는다.
        /// 남은 것 가운데 나올 수 있는 것이 없으면 다음 테이블로 넘어간다.
        /// 난수는 런 시드와 테이블 번호와 꺼낸 자리로 만든다. 꺼낸 자리와 순서를 저장하므로 이어 해도 같은 것이 나온다.
        /// 분포가 없으면 예전과 똑같이 꺼낸다. 2026년 10월 9일 외부 검토가 권했고 원재가 틀만 만들기로 정했다.
        /// </summary>
        public int Draw(IMerchantCatalog catalog, int count, List<MerchantItem> into, StageRarityWeights weights)
        {
            if (weights != null)
            {
                return DrawWeighted(catalog, count, into, weights);
            }

            if (into == null)
            {
                return 0;
            }

            into.Clear();

            if (catalog == null || count <= 0)
            {
                return 0;
            }

            // 지금 나올 수 있는 유물만 추려 둔다. 보유한 것은 여기에 없다.
            List<MerchantItem> available = new List<MerchantItem>();
            catalog.CollectRelicCandidates(available);

            if (available.Count == 0)
            {
                return 0;
            }

            // 테이블을 넘겨 가며 꺼내므로 한 번에 담을 수 있는 수는
            // 지금 나올 수 있는 유물 수를 넘지 못한다.
            // 이 한계를 두지 않으면 후보가 적을 때 같은 유물이 여러 자리에 들어간다.
            int limit = count < available.Count ? count : available.Count;
            int preparedTables = 0;
            int deferredInRow = 0;

            while (into.Count < limit)
            {
                if (NextIndex >= Order.Count)
                {
                    if (preparedTables >= MaxTablesPerDraw || !PrepareNextTable(catalog))
                    {
                        break;
                    }

                    preparedTables++;
                    deferredInRow = 0;
                    continue;
                }

                string id = Order[NextIndex];

                // 테이블이 넘어가면 이번에 이미 담은 유물을 다시 만날 수 있다.
                // 그 유물은 지나간 것으로 치지 않고 테이블 맨 뒤로 미룬다.
                if (ContainsId(into, id))
                {
                    // 남은 자리를 한 바퀴 다 미뤘다면 이 테이블에는 담을 것이 없다.
                    if (deferredInRow >= Order.Count - NextIndex)
                    {
                        NextIndex = Order.Count;
                        deferredInRow = 0;
                        continue;
                    }

                    Order.RemoveAt(NextIndex);
                    Order.Add(id);
                    deferredInRow++;
                    continue;
                }

                NextIndex++;
                deferredInRow = 0;

                int found = IndexOf(available, id);
                if (found >= 0)
                {
                    into.Add(available[found]);
                }
            }

            return into.Count;
        }

        /// <summary>희귀도 분포대로 꺼낸다. 규칙은 `Draw(IMerchantCatalog, int, List, StageRarityWeights)` 에 적었다.</summary>
        private int DrawWeighted(IMerchantCatalog catalog, int count, List<MerchantItem> into, StageRarityWeights weights)
        {
            if (into == null)
            {
                return 0;
            }

            into.Clear();

            if (catalog == null || count <= 0)
            {
                return 0;
            }

            List<MerchantItem> available = new List<MerchantItem>();
            catalog.CollectRelicCandidates(available);

            if (available.Count == 0)
            {
                return 0;
            }

            int limit = count < available.Count ? count : available.Count;
            int preparedTables = 0;
            List<int> positions = new List<int>();

            while (into.Count < limit)
            {
                // 지금 테이블에 남은 자리 가운데 나올 수 있고 이번에 아직 담지 않은 것.
                positions.Clear();
                for (int p = NextIndex; p < Order.Count; p++)
                {
                    if (IndexOf(available, Order[p]) >= 0 && !ContainsId(into, Order[p]))
                    {
                        positions.Add(p);
                    }
                }

                if (positions.Count == 0)
                {
                    // 남은 것은 가졌거나 이번에 담은 것뿐이다. 이 테이블은 다 썼다.
                    NextIndex = Order.Count;
                    if (preparedTables >= MaxTablesPerDraw || !PrepareNextTable(catalog))
                    {
                        break;
                    }

                    preparedTables++;
                    continue;
                }

                // 등급만 굴리고 그 등급에서 차례가 가장 앞선 것을 꺼낸다. 같은 등급 안에서는 테이블 순서를 그대로 지킨다.
                // positions 는 테이블 차례대로 담겼다.
                SanctumRandom random = SanctumRandom.ForRelicPool(RunSeed, TableIndex * 7919 + NextIndex + 1);
                int pick = RarityPicker.PickFirstIndex(
                    positions,
                    p => available[IndexOf(available, Order[p])].Rarity,
                    weights,
                    n => random.Range(0, n));

                // 꺼낸 것을 지나간 자리로 옮긴다. 건너뛴 것들은 앞뒤 차례를 그대로 지킨다.
                int at = positions[pick];
                string id = Order[at];
                Order.RemoveAt(at);
                Order.Insert(NextIndex, id);
                NextIndex++;

                into.Add(available[IndexOf(available, id)]);
            }

            return into.Count;
        }

        private static bool ContainsId(List<MerchantItem> items, string id)
        {
            return IndexOf(items, id) >= 0;
        }

        private static int IndexOf(List<MerchantItem> items, string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
