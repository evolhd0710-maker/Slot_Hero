using System;
using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 성소 한 곳의 상태와 규칙.
    /// 성소 기획서 v0.2 의 03 진입 흐름, 04 야영, 05 행상 구성, 07 가격 을 한 자리에 모은 것이다.
    ///
    /// 값만 들고 있어 저장 시스템 기획서의 런 데이터에 그대로 실린다.
    /// 화면과 연출은 UI 쪽이 맡고 여기서는 무엇이 가능하고 무엇이 바뀌는지만 정한다.
    ///
    /// 문양 변경과 유물 새로고침의 사용 횟수는 성소마다 다시 0에서 시작한다.
    /// </summary>
    [Serializable]
    public class SanctumState
    {
        /// <summary>
        /// 이 성소를 가리키는 번호. 시드를 섞는 데 쓴다.
        /// 흐름이 스테이지와 노드 번호로 만든 방 번호(`GameFlowController.RoomKey`)를 넣는다.
        /// 몇 번째 성소인지로 세면 이어하기를 할 때마다 처음부터 다시 세어 진열이 달라진다.
        /// </summary>
        public int SanctumIndex;

        /// <summary>이어서 굴릴 난수 상태. 새로고침을 거쳐도 같은 시드면 같은 결과가 나오게 한다.</summary>
        public uint RandomState;

        /// <summary>야영을 쓴 횟수. 04 야영 의 사용 횟수 1회.</summary>
        public int CampUsedCount;

        /// <summary>이번 성소에서 문양 변경을 쓴 횟수. 다음 가격을 구하는 데 쓴다.</summary>
        public int SymbolChangeCount;

        /// <summary>이번 성소에서 유물 새로고침을 쓴 횟수. 다음 가격을 구하는 데 쓴다.</summary>
        public int RelicRefreshCount;

        /// <summary>행상 진열.</summary>
        public MerchantStock Stock = new MerchantStock();

        /// <summary>
        /// 성소에 들어와 진열을 만든다. 03 진입 흐름 의 성소 진입에 해당한다.
        /// 유물은 런을 시작할 때 만들어 둔 <paramref name="relicPool"/>의 순서대로 나온다.
        /// </summary>
        public void Begin(
            int runSeed,
            int sanctumIndex,
            SanctumConfig config,
            SanctumPriceConfig prices,
            IMerchantCatalog catalog,
            RunRelicPool relicPool)
        {
            SanctumIndex = sanctumIndex;
            CampUsedCount = 0;
            SymbolChangeCount = 0;
            RelicRefreshCount = 0;

            SanctumRandom random = SanctumRandom.ForSanctum(runSeed, sanctumIndex);
            Stock = MerchantStockBuilder.Build(config, prices, catalog, random, relicPool);
            RandomState = random.State;
        }

        /// <summary>저장에서 불러온 뒤 자리 수를 지금 설정에 맞춘다.</summary>
        public void RestoreAfterLoad(SanctumConfig config)
        {
            if (Stock == null)
            {
                Stock = new MerchantStock();
            }

            if (config != null)
            {
                Stock.EnsureSlots(config.SymbolSlotCount, config.CoinSlotCount, config.RelicSlotCount);
            }
        }

        // ── 야영 ────────────────────────────────────────────────

        /// <summary>야영을 아직 쓸 수 있는지. 04 야영 의 사용 표시 비활성 판정에 쓴다.</summary>
        public bool CanUseCamp(SanctumConfig config)
        {
            if (config == null)
            {
                return false;
            }

            return CampUsedCount < config.CampUseLimit;
        }

        /// <summary>
        /// 야영을 쓴다. 04 야영 의 즉시 사용, 성소당 1회, 체력 40 회복, 최대 체력 초과 금지.
        /// 실제로 회복한 양을 healedAmount 로 돌려준다.
        /// 체력이 이미 가득이면 회복량 0으로 성공한다.
        /// </summary>
        public PurchaseResult TryUseCamp(
            SanctumConfig config,
            IPlayerVitals vitals,
            RunGoldState gold,
            out int healedAmount)
        {
            healedAmount = 0;

            if (config == null || vitals == null)
            {
                return PurchaseResult.Unavailable;
            }

            if (!CanUseCamp(config))
            {
                return PurchaseResult.Unavailable;
            }

            if (config.CampCost > 0)
            {
                if (gold == null || !gold.CanSpend(config.CampCost))
                {
                    return PurchaseResult.NotEnoughGold;
                }

                gold.TrySpend(config.CampCost);
            }

            int missing = vitals.MaxHealth - vitals.Health;
            if (missing < 0)
            {
                missing = 0;
            }

            healedAmount = config.CampHealAmount < missing ? config.CampHealAmount : missing;
            if (healedAmount > 0)
            {
                vitals.Heal(healedAmount);
            }

            CampUsedCount++;
            return PurchaseResult.Success;
        }

        // ── 구매 ────────────────────────────────────────────────

        /// <summary>
        /// 진열된 물건을 산다. 08 화면 구성 의 "클릭 시 즉시 구매한다".
        /// 06 재화 에 따라 보유 골드를 넘겨 소모할 수 없으므로 모자라면 아무것도 하지 않는다.
        /// </summary>
        public PurchaseResult TryBuy(MerchantSlot slot, RunGoldState gold, IMerchantCatalog catalog)
        {
            if (slot == null || !slot.HasItem)
            {
                return PurchaseResult.EmptySlot;
            }

            if (slot.Sold)
            {
                return PurchaseResult.AlreadySold;
            }

            if (gold == null || catalog == null)
            {
                return PurchaseResult.Unavailable;
            }

            if (!gold.CanSpend(slot.Price))
            {
                return PurchaseResult.NotEnoughGold;
            }

            // 소지 한도까지 찼으면 값을 치르지 않고 알린다. 골드가 모자라면 그쪽이 먼저다. 버려도 살 수 없기 때문이다.
            if (!catalog.HasRoomFor(slot.Item))
            {
                return PurchaseResult.NoRoom;
            }

            gold.TrySpend(slot.Price);
            catalog.Acquire(slot.Item);
            slot.Sold = true;
            return PurchaseResult.Success;
        }

        // ── 문양 변경 ───────────────────────────────────────────

        /// <summary>다음 문양 변경에 드는 골드.</summary>
        public int GetSymbolChangePrice(SanctumPriceConfig prices)
        {
            return prices != null ? prices.GetSymbolChangePrice(SymbolChangeCount) : 0;
        }

        /// <summary>
        /// 가지고 있는 문양 하나를 다른 문양으로 바꾼다.
        /// 05 행상 구성 의 문양 변경으로, 횟수 제한 없이 쓸 수 있고 쓸 때마다 값이 오른다.
        /// 어떤 문양으로 바뀌는지는 시드 난수가 정해 newSymbol 로 돌아온다.
        /// </summary>
        public PurchaseResult TryChangeSymbol(
            string ownedSymbolId,
            SanctumConfig config,
            SanctumPriceConfig prices,
            RunGoldState gold,
            IMerchantCatalog catalog,
            out MerchantItem newSymbol)
        {
            newSymbol = new MerchantItem();

            if (config == null || prices == null || gold == null || catalog == null)
            {
                return PurchaseResult.Unavailable;
            }

            if (string.IsNullOrEmpty(ownedSymbolId))
            {
                return PurchaseResult.Unavailable;
            }

            int price = GetSymbolChangePrice(prices);
            if (!gold.CanSpend(price))
            {
                return PurchaseResult.NotEnoughGold;
            }

            SanctumRandom random = SanctumRandom.FromState(RandomState);
            if (!MerchantStockBuilder.TryPickReplacementSymbol(catalog, config, ownedSymbolId, random, out newSymbol))
            {
                return PurchaseResult.NoCandidate;
            }

            if (!catalog.TryReplaceSymbol(ownedSymbolId, newSymbol))
            {
                return PurchaseResult.Unavailable;
            }

            gold.TrySpend(price);
            SymbolChangeCount++;
            RandomState = random.State;
            return PurchaseResult.Success;
        }

        // ── 유물 새로고침 ───────────────────────────────────────

        /// <summary>다음 유물 새로고침에 드는 골드.</summary>
        public int GetRelicRefreshPrice(SanctumPriceConfig prices)
        {
            return prices != null ? prices.GetRelicRefreshPrice(RelicRefreshCount) : 0;
        }

        /// <summary>
        /// 유물 자리를 미리 정해 둔 순서의 다음 유물로 바꾼다.
        /// 순서를 다 쓰면 다음 테이블을 준비해 이어 가므로 새로고침이 끊기지 않는다.
        /// 가지고 있지 않은 유물이 아예 없을 때만 값을 받지 않고 진열도 건드리지 않는다.
        /// </summary>
        public PurchaseResult TryRefreshRelics(
            SanctumConfig config,
            SanctumPriceConfig prices,
            RunGoldState gold,
            IMerchantCatalog catalog,
            RunRelicPool relicPool)
        {
            if (config == null || prices == null || gold == null || catalog == null || Stock == null)
            {
                return PurchaseResult.Unavailable;
            }

            if (relicPool == null)
            {
                return PurchaseResult.Unavailable;
            }

            int price = GetRelicRefreshPrice(prices);
            if (!gold.CanSpend(price))
            {
                return PurchaseResult.NotEnoughGold;
            }

            // 지금 나올 수 있는 유물이 하나라도 있는지 먼저 본다.
            // 값을 받아 놓고 진열이 비는 일이 없어야 하기 때문이다.
            // 테이블에 남은 자리가 모자라도 다음 테이블을 준비해 이어 가므로
            // 여기서는 보유하지 않은 유물이 있는지만 보면 된다.
            List<MerchantItem> available = new List<MerchantItem>();
            catalog.CollectRelicCandidates(available);
            if (available.Count == 0)
            {
                return PurchaseResult.NoCandidate;
            }

            int filled = MerchantStockBuilder.RefreshRelics(Stock, config, prices, catalog, relicPool);
            if (filled == 0)
            {
                return PurchaseResult.NoCandidate;
            }

            gold.TrySpend(price);
            RelicRefreshCount++;
            return PurchaseResult.Success;
        }
    }
}
