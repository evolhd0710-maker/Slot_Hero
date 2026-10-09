using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상의 진열을 만든다.
    /// 성소 기획서 v0.2 / 05 행상 구성 의 문양 4, 코인 2, 유물 3 과
    /// "서로 다른 문양", "보유하지 않은 코인", "보유하지 않은 유물"을 지키는 자리다.
    ///
    /// 문양과 코인은 진열할 때마다 후보에서 뽑고,
    /// 유물은 런을 시작할 때 미리 정해 둔 <see cref="RunRelicPool"/>의 순서대로 꺼낸다.
    /// 보유 여부는 카탈로그가 후보를 모아 줄 때 이미 걸러 둔 것으로 본다.
    /// </summary>
    public static class MerchantStockBuilder
    {
        /// <summary>성소에 처음 들어갔을 때의 진열을 만든다.</summary>
        public static MerchantStock Build(
            SanctumConfig config,
            SanctumPriceConfig prices,
            IMerchantCatalog catalog,
            SanctumRandom random,
            RunRelicPool relicPool)
        {
            MerchantStock stock = MerchantStock.CreateEmpty(
                config != null ? config.SymbolSlotCount : 0,
                config != null ? config.CoinSlotCount : 0,
                config != null ? config.RelicSlotCount : 0);

            if (config == null || catalog == null || random == null)
            {
                return stock;
            }

            List<MerchantItem> candidates = new List<MerchantItem>();

            // 그 스테이지의 희귀도 분포. 없으면 null 이고 예전처럼 고르게 뽑는다. 문양은 등급이 없어 쓰지 않는다.
            StageRarityWeights weights = config.GetRarityWeights(catalog.Stage);

            candidates.Clear();
            catalog.CollectSymbolCandidates(candidates);
            FillSlots(stock.SymbolSlots, candidates, prices, random, false, null);

            candidates.Clear();
            catalog.CollectCoinCandidates(candidates);
            FillSlots(stock.CoinSlots, candidates, prices, random, false, weights);

            FillRelicSlots(stock.RelicSlots, relicPool, catalog, prices, false, weights);

            return stock;
        }

        /// <summary>
        /// 유물 자리를 모두 새 유물로 바꾼다.
        /// 05 행상 구성 의 "유물 3자리 모두 보유하지 않은 다른 유물로 모두 교체한다".
        /// 미리 정해 둔 순서에서 다음 것을 꺼내므로 앞에 나왔던 유물은 다시 나오지 않는다.
        /// 채운 자리 수를 돌려준다.
        /// </summary>
        public static int RefreshRelics(
            MerchantStock stock,
            SanctumConfig config,
            SanctumPriceConfig prices,
            IMerchantCatalog catalog,
            RunRelicPool relicPool)
        {
            if (stock == null || config == null || catalog == null)
            {
                return 0;
            }

            return FillRelicSlots(
                stock.RelicSlots,
                relicPool,
                catalog,
                prices,
                !config.RefreshRefillsSoldSlots,
                config.GetRarityWeights(catalog.Stage));
        }

        /// <summary>미리 정해 둔 순서에서 다음 유물을 꺼내 자리를 채운다. 희귀도 분포가 있으면 등급부터 굴린다(`RunRelicPool.Draw`).</summary>
        private static int FillRelicSlots(
            List<MerchantSlot> slots,
            RunRelicPool relicPool,
            IMerchantCatalog catalog,
            SanctumPriceConfig prices,
            bool keepSold,
            StageRarityWeights weights)
        {
            if (slots == null)
            {
                return 0;
            }

            int need = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = new MerchantSlot();
                }

                if (keepSold && slots[i].Sold)
                {
                    continue;
                }

                need++;
            }

            List<MerchantItem> drawn = new List<MerchantItem>();
            if (relicPool != null && need > 0)
            {
                relicPool.Draw(catalog, need, drawn, weights);
            }

            int drawnIndex = 0;
            int filled = 0;

            for (int i = 0; i < slots.Count; i++)
            {
                MerchantSlot slot = slots[i];

                if (keepSold && slot.Sold)
                {
                    continue;
                }

                if (drawnIndex >= drawn.Count)
                {
                    slot.Clear();
                    continue;
                }

                MerchantItem item = drawn[drawnIndex];
                drawnIndex++;

                slot.Set(item, prices != null ? prices.GetItemPrice(item) : 0);
                filled++;
            }

            return filled;
        }

        /// <summary>
        /// 문양 변경으로 받을 새 문양을 고른다.
        /// 어떤 문양이 나올지는 시드 난수로 정하므로 같은 시드면 같은 결과가 나온다.
        /// </summary>
        public static bool TryPickReplacementSymbol(
            IMerchantCatalog catalog,
            SanctumConfig config,
            string sourceSymbolId,
            SanctumRandom random,
            out MerchantItem picked)
        {
            picked = new MerchantItem();

            if (catalog == null || config == null || random == null)
            {
                return false;
            }

            List<MerchantItem> candidates = new List<MerchantItem>();
            catalog.CollectSymbolCandidates(candidates);
            if (candidates.Count == 0)
            {
                return false;
            }

            List<string> avoid = new List<string>();
            if (config.SymbolChangeExcludesOwned)
            {
                List<MerchantItem> owned = new List<MerchantItem>();
                catalog.CollectOwnedSymbols(owned);
                for (int i = 0; i < owned.Count; i++)
                {
                    avoid.Add(owned[i].Id);
                }
            }
            else if (config.SymbolChangeExcludesSource && !string.IsNullOrEmpty(sourceSymbolId))
            {
                avoid.Add(sourceSymbolId);
            }

            List<MerchantItem> allowed = new List<MerchantItem>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!Contains(avoid, candidates[i].Id))
                {
                    allowed.Add(candidates[i]);
                }
            }

            // 제한을 다 지키면 뽑을 것이 없는 경우가 있다.
            // 바꾸기 전과 같은 문양만 막고 다시 고른다.
            if (allowed.Count == 0 && config.SymbolChangeExcludesSource && !string.IsNullOrEmpty(sourceSymbolId))
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].Id != sourceSymbolId)
                    {
                        allowed.Add(candidates[i]);
                    }
                }
            }

            // 그래도 없으면 후보 전체에서 고른다.
            if (allowed.Count == 0)
            {
                allowed.AddRange(candidates);
            }

            picked = allowed[random.Range(0, allowed.Count)];
            return picked.IsValid;
        }

        /// <summary>
        /// 자리 목록을 후보로 채운다. 한 목록 안에서 같은 물건이 두 번 나오지 않는다.
        /// 후보가 모자라면 남는 자리는 비워 둔다.
        /// 희귀도 분포(weights)가 있으면 자리마다 등급부터 굴려 남은 후보에서 뽑는다. 없으면 섞어서 앞에서부터 쓴다.
        /// </summary>
        private static int FillSlots(
            List<MerchantSlot> slots,
            List<MerchantItem> candidates,
            SanctumPriceConfig prices,
            SanctumRandom random,
            bool keepSold,
            StageRarityWeights weights)
        {
            if (slots == null)
            {
                return 0;
            }

            List<MerchantItem> pool = new List<MerchantItem>();

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].IsValid)
                    {
                        pool.Add(candidates[i]);
                    }
                }
            }

            if (weights != null)
            {
                // 자리마다 등급부터 굴려 뽑은 것을 앞으로 모은다. 아래는 앞에서부터 쓰므로 그대로 이어진다.
                List<MerchantItem> ordered = new List<MerchantItem>();
                while (pool.Count > 0)
                {
                    int index = RarityPicker.PickIndex(pool, item => item.Rarity, weights, n => random.Range(0, n));
                    ordered.Add(pool[index]);
                    pool.RemoveAt(index);
                }

                pool = ordered;
            }
            else
            {
                random.Shuffle(pool);
            }

            int poolIndex = 0;
            int filled = 0;

            for (int i = 0; i < slots.Count; i++)
            {
                MerchantSlot slot = slots[i];
                if (slot == null)
                {
                    slot = new MerchantSlot();
                    slots[i] = slot;
                }

                if (keepSold && slot.Sold)
                {
                    continue;
                }

                if (poolIndex >= pool.Count)
                {
                    slot.Clear();
                    continue;
                }

                MerchantItem item = pool[poolIndex];
                poolIndex++;

                slot.Set(item, prices != null ? prices.GetItemPrice(item) : 0);
                filled++;
            }

            return filled;
        }

        private static bool Contains(List<string> ids, string id)
        {
            if (ids == null || string.IsNullOrEmpty(id))
            {
                return false;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
