using System.Collections.Generic;
using SlotHero.Combat;
using SlotHero.Map;
using SlotHero.Reward;
using SlotHero.Sanctum;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 방을 깼을 때 받을 보상을 정한다.
    ///
    /// 런 시드와 방 번호로만 정한다. 그래서 저장하지 않는다.
    /// 저장 시스템 기획서 v0.1 / 10쪽 의 "보상 후보는 시드에서 다시 만드는 값"이 그렇게 정했다.
    /// 같은 방에 다시 들어가면 같은 보상이 나온다.
    ///
    /// 골드 액수와 카드 장수는 설정 에셋에서 온다.
    /// </summary>
    public class RewardMaker
    {
        private readonly RewardRuleConfig _rules;
        private readonly RunCatalogConfig _catalog;

        /// <summary>규칙과 물건 목록을 물려 만든다.</summary>
        public RewardMaker(RewardRuleConfig rules, RunCatalogConfig catalog)
        {
            _rules = rules;
            _catalog = catalog;
        }

        /// <summary>그 방의 보상을 만든다.</summary>
        public RewardOffer Make(int runSeed, MapNode node, RunSaveData run)
        {
            if (_rules == null || node == null)
            {
                return new RewardOffer();
            }

            // 방마다 다른 난수를 쓴다. 같은 방이면 늘 같은 값이 나온다.
            // 노드 번호는 스테이지마다 0 부터 다시 매겨지므로 스테이지도 섞는다.
            // 안 섞으면 2스테이지의 같은 번호 방에서 1스테이지와 같은 보상이 나온다.
            int stage = run != null ? run.StageIndex : 1;
            MapRandom random = new MapRandom(runSeed ^ (node.Id * 6971) ^ (stage * 104729));

            RewardOffer offer = new RewardOffer();
            offer.BaseGold = _rules.GetBaseGold(node.RoomType, random);
            offer.OverkillGold = 0;

            int cardCount = _rules.GetCardCount(node.RoomType);
            if (cardCount <= 0 || _catalog == null)
            {
                return offer;
            }

            AddCards(offer, random, cardCount, node.RoomType, run);
            return offer;
        }

        /// <summary>
        /// 이벤트가 연 전투의 보상 카드를 넣는다. `RewardRuleConfig.EventCombatCardsLike` 방의 카드 규칙을 쓴다.
        /// round 는 몇 번째 고르기인지다. 35 도전자 처럼 두 번 고를 때 두 번째에 다른 카드가 나오게 섞는다.
        /// 시드와 방과 round 로만 정하므로 고르기 사이에 꺼졌다 이어도 같은 카드가 나온다.
        /// </summary>
        public void AddEventCombatCards(RewardOffer offer, int runSeed, MapNode node, RunSaveData run, int round)
        {
            if (offer == null || _rules == null || _catalog == null || node == null)
            {
                return;
            }

            RoomType like = _rules.EventCombatCardsLike;
            int cardCount = _rules.GetCardCount(like);
            if (cardCount <= 0)
            {
                return;
            }

            int stage = run != null ? run.StageIndex : 1;
            MapRandom random = new MapRandom(
                runSeed ^ (node.Id * 6971) ^ (stage * 104729) ^ ((round + 1) * 15485863));

            AddCards(offer, random, cardCount, like, run);
        }

        /// <summary>
        /// 카드를 뽑아 넣는다.
        /// 같은 것이 두 번 나오지 않게 뽑은 것을 빼 가며 고른다.
        /// </summary>
        private void AddCards(
            RewardOffer offer, MapRandom random, int cardCount, RoomType roomType, RunSaveData run)
        {
            // **후보는 물건 목록의 자리로만 담고 카드는 고른 것만 만든다.**
            // 예전에는 후보 전부를 카드로 만들고 등급 사전까지 지어 보상 한 번에 후보 수만큼 할당했다. 2026년 10월 9일 외부 검토가 권했다.
            // 후보 차례와 섞는 난수는 예전과 같아 같은 시드면 같은 보상이 나온다.
            List<RewardCandidate> pool = new List<RewardCandidate>();

            // 지금 스테이지에 나올 수 없는 것은 뺀다(`ItemDefinition.AppearsIn`). 2026년 10월 9일에 칸만 만들었다.
            int stage = run != null && run.StageIndex > 0 ? run.StageIndex : 1;

            // 엘리트와 보스는 유물이 나오고 일반 방은 문양이 나온다.
            // 기획서가 방마다 무엇이 나오는지 아직 정하지 않아 설정 에셋에서 고르게 두었다.
            if (_rules.GivesRelics(roomType))
            {
                for (int i = 0; i < _catalog.Relics.Count; i++)
                {
                    ItemDefinition relic = _catalog.Relics[i];

                    if ((run != null && run.Owned.HasRelic(relic.Id)) || !relic.AppearsIn(stage)
                        || !relic.CanComeFrom(ItemSource.Reward, null))
                    {
                        continue;
                    }

                    pool.Add(new RewardCandidate(RewardKind.Relic, i, relic.Rarity));
                }
            }

            if (_rules.GivesCoins(roomType))
            {
                for (int i = 0; i < _catalog.Coins.Count; i++)
                {
                    ItemDefinition coin = _catalog.Coins[i];

                    if ((run != null && run.Owned.CoinIds.Contains(coin.Id)) || !coin.AppearsIn(stage)
                        || !coin.CanComeFrom(ItemSource.Reward, null))
                    {
                        continue;
                    }

                    pool.Add(new RewardCandidate(RewardKind.Coin, i, coin.Rarity));
                }
            }

            bool hasSymbols = false;
            if (_rules.GivesSymbols(roomType))
            {
                for (int i = 0; i < _catalog.Symbols.Count; i++)
                {
                    if (!_catalog.Symbols[i].AppearsIn(stage))
                    {
                        continue;
                    }

                    pool.Add(new RewardCandidate(RewardKind.Symbol, i, ItemRarity.Common));
                    hasSymbols = true;
                }
            }

            // 희귀도 분포가 있고 카드가 모두 코인이나 유물이면 장마다 등급부터 굴린다.
            // 문양은 등급이 없어 섞이면 예전처럼 섞어서 앞에서부터 쓴다. 분포가 없을 때도 예전과 같다.
            StageRarityWeights weights = _rules.GetRarityWeights(stage);
            if (weights != null && !hasSymbols)
            {
                for (int n = 0; n < cardCount && pool.Count > 0; n++)
                {
                    int index = RarityPicker.PickIndex(pool, candidate => candidate.Rarity, weights, max => random.Range(0, max));
                    offer.Add(MakeCard(pool[index]));
                    pool.RemoveAt(index);
                }

                return;
            }

            random.Shuffle(pool);

            int take = cardCount < pool.Count ? cardCount : pool.Count;
            for (int i = 0; i < take; i++)
            {
                offer.Add(MakeCard(pool[i]));
            }
        }

        /// <summary>고른 후보를 카드로 만든다.</summary>
        private RewardCard MakeCard(RewardCandidate candidate)
        {
            switch (candidate.Kind)
            {
                case RewardKind.Relic:
                    ItemDefinition relic = _catalog.Relics[candidate.Index];
                    return RewardCard.Relic(relic.Id, relic.DisplayName);

                case RewardKind.Coin:
                    ItemDefinition coin = _catalog.Coins[candidate.Index];
                    return RewardCard.Coin(coin.Id, coin.DisplayName);

                default:
                    SymbolDefinition symbol = _catalog.Symbols[candidate.Index];
                    return RewardCard.Symbol(symbol.Id, symbol.DisplayName, symbol.FirstTag, symbol.SecondTag);
            }
        }

        /// <summary>보상 후보 하나. 물건 목록의 어느 목록 몇 번째인지와 등급만 담는다. 문양의 등급은 쓰지 않는다.</summary>
        private struct RewardCandidate
        {
            public readonly RewardKind Kind;
            public readonly int Index;
            public readonly ItemRarity Rarity;

            public RewardCandidate(RewardKind kind, int index, ItemRarity rarity)
            {
                Kind = kind;
                Index = index;
                Rarity = rarity;
            }
        }
    }
}
