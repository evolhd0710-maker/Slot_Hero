using System.Collections.Generic;
using UnityEngine;
using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.CurrentBuild.UI;
using SlotHero.Reward;
using SlotHero.Reward.UI;
using SlotHero.Sanctum;
using SlotHero.Sanctum.UI;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 행상이 무엇을 올릴 수 있는지 알려 주고 산 것을 런에 넣어 주는 쪽.
    /// 성소 기획서 v0.2 / 05 행상 구성 이 "보유하지 않은 코인 2개, 보유하지 않은 유물 3개"로
    /// 정했으므로 후보를 모을 때 이미 가진 것을 빼고 넘긴다. 문양은 보유 여부를 따지지 않는다.
    ///
    /// 물건 그림을 내주는 일도 여기서 한다.
    /// 현재 빌드 화면과 행상이 각각 다른 창구로 그림을 받는데 둘 다 같은 목록을 보아야 하므로
    /// 한 곳에서 내준다.
    ///
    /// 목록은 설정 에셋에서, 보유 여부는 런 데이터에서 가져온다.
    /// </summary>
    public class RunCatalog : IMerchantCatalog, IBuildIconSource, IMerchantIconSource, IRewardIconSource
    {
        private readonly RunCatalogConfig _config;
        private RunSaveData _run;

        /// <summary>목록을 물려 만든다. 런은 나중에 붙인다.</summary>
        public RunCatalog(RunCatalogConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 어떤 런을 볼지 정한다.
        /// 코인과 유물의 소지 한도가 아직 정해지지 않은 런이면 설정의 시작 값으로 채운다.
        /// 새 런과, 한도 칸이 생기기 전에 저장한 런이 그렇다.
        /// </summary>
        public void Attach(RunSaveData run)
        {
            _run = run;

            if (run == null || _config == null)
            {
                return;
            }

            if (run.Owned.CoinCapacity < 0)
            {
                run.Owned.CoinCapacity = _config.StartingCoinCapacity;
            }

            if (run.Owned.RelicCapacity < 0)
            {
                run.Owned.RelicCapacity = _config.StartingRelicCapacity;
            }
        }

        /// <summary>그 물건을 더 가질 자리가 있는지. 문양은 한도가 없다.</summary>
        public bool HasRoomFor(MerchantItem item)
        {
            if (_run == null || !item.IsValid)
            {
                return true;
            }

            switch (item.Kind)
            {
                case SanctumItemKind.Coin:
                    return _run.Owned.CoinRoom > 0;

                case SanctumItemKind.Relic:
                    return _run.Owned.RelicRoom > 0;

                default:
                    return true;
            }
        }

        /// <summary>그 식별자의 이름.</summary>
        public string GetDisplayName(string id)
        {
            return _config != null ? _config.GetDisplayName(id) : id;
        }

        /// <summary>그 문양의 태그 둘.</summary>
        public void GetSymbolTags(
            string id, out SymbolTagType firstTag, out SymbolTagType secondTag)
        {
            if (_config == null || !_config.TryGetSymbolTags(id, out firstTag, out secondTag))
            {
                firstTag = SymbolTagType.Mercury;
                secondTag = SymbolTagType.Mercury;
            }
        }

        /// <summary>현재 빌드 화면이 칸에 놓을 그림을 내준다.</summary>
        public Sprite GetIcon(BuildEntry entry)
        {
            return _config != null ? _config.GetIcon(entry.Id) : null;
        }

        /// <summary>행상이 진열에 놓을 그림을 내준다.</summary>
        public Sprite GetIcon(MerchantItem item)
        {
            return _config != null ? _config.GetIcon(item.Id) : null;
        }

        /// <summary>보상 카드에 놓을 그림을 내준다.</summary>
        public Sprite GetIcon(RewardCard card)
        {
            if (_config == null || card == null)
            {
                return null;
            }

            return _config.GetIcon(card.Id);
        }

        /// <summary>런을 시작할 때 주는 것들을 넣어 준다.</summary>
        public void GiveStartingItems(RunContext context)
        {
            if (context == null || _config == null)
            {
                return;
            }

            context.GiveStartingItems(
                _config.StartingSymbolIds,
                _config.StartingCoinIds,
                _config.StartingRelicIds,
                _config.StartingGold);
        }

        // ---- IMerchantCatalog ----

        /// <inheritdoc />
        public void CollectSymbolCandidates(List<MerchantItem> into)
        {
            if (into == null || _config == null)
            {
                return;
            }

            for (int i = 0; i < _config.Symbols.Count; i++)
            {
                // 지금 스테이지에 나올 수 없는 문양은 뺀다(`SymbolDefinition.AppearsIn`). 문양 변경도 이 후보를 쓴다.
                if (!_config.Symbols[i].AppearsIn(Stage))
                {
                    continue;
                }

                into.Add(_config.ToMerchantItem(_config.Symbols[i]));
            }
        }

        /// <inheritdoc />
        public void CollectCoinCandidates(List<MerchantItem> into)
        {
            if (into == null || _config == null)
            {
                return;
            }

            for (int i = 0; i < _config.Coins.Count; i++)
            {
                ItemDefinition coin = _config.Coins[i];

                if ((_run != null && _run.Owned.CoinIds.Contains(coin.Id)) || !coin.AppearsIn(Stage)
                    || !coin.CanComeFrom(ItemSource.Merchant, null))
                {
                    continue;
                }

                into.Add(MerchantItem.Coin(coin.Id, coin.DisplayName, coin.Rarity));
            }
        }

        /// <inheritdoc />
        public void CollectRelicCandidates(List<MerchantItem> into)
        {
            if (into == null || _config == null)
            {
                return;
            }

            for (int i = 0; i < _config.Relics.Count; i++)
            {
                ItemDefinition relic = _config.Relics[i];

                // 지금 스테이지에 나올 수 없는 유물도 뺀다. 유물 순서 테이블에서는 가진 유물처럼 지나간다(`RunRelicPool.Draw`).
                if ((_run != null && _run.Owned.HasRelic(relic.Id)) || !relic.AppearsIn(Stage)
                    || !relic.CanComeFrom(ItemSource.Merchant, null))
                {
                    continue;
                }

                into.Add(MerchantItem.Relic(relic.Id, relic.DisplayName, relic.Rarity));
            }
        }

        /// <inheritdoc />
        public int Stage
        {
            get { return _run != null && _run.StageIndex > 0 ? _run.StageIndex : 1; }
        }

        /// <inheritdoc />
        public void CollectAllRelics(List<MerchantItem> into)
        {
            if (into == null || _config == null)
            {
                return;
            }

            for (int i = 0; i < _config.Relics.Count; i++)
            {
                ItemDefinition relic = _config.Relics[i];
                into.Add(MerchantItem.Relic(relic.Id, relic.DisplayName, relic.Rarity));
            }
        }

        /// <inheritdoc />
        public void CollectOwnedSymbols(List<MerchantItem> into)
        {
            if (into == null || _run == null || _config == null)
            {
                return;
            }

            for (int i = 0; i < _run.Owned.Symbols.Count; i++)
            {
                string id = _run.Owned.Symbols[i].SymbolId;

                for (int j = 0; j < _config.Symbols.Count; j++)
                {
                    if (_config.Symbols[j].Id == id)
                    {
                        into.Add(_config.ToMerchantItem(_config.Symbols[j]));
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Acquire(MerchantItem item)
        {
            if (_run == null || !item.IsValid)
            {
                return;
            }

            switch (item.Kind)
            {
                case SanctumItemKind.Symbol:
                    _run.Owned.AddSymbol(item.Id, 1);
                    break;

                // 소지 한도는 사기 전에 본다(`HasRoomFor`). 여기서도 한도를 넘기지 않게 한 번 더 막는다.
                case SanctumItemKind.Coin:
                    if (_run.Owned.CoinRoom > 0)
                    {
                        _run.Owned.AddCoin(item.Id);
                    }

                    break;

                case SanctumItemKind.Relic:
                    if (_run.Owned.RelicRoom > 0)
                    {
                        _run.Owned.AddRelic(item.Id);
                    }

                    break;
            }
        }

        /// <inheritdoc />
        public bool TryReplaceSymbol(string ownedSymbolId, MerchantItem newSymbol)
        {
            if (_run == null || !newSymbol.IsValid)
            {
                return false;
            }

            if (_run.Owned.RemoveSymbol(ownedSymbolId, 1) <= 0)
            {
                return false;
            }

            _run.Owned.AddSymbol(newSymbol.Id, 1);
            return true;
        }
    }
}
