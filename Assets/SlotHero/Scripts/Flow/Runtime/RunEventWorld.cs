using System;
using System.Collections.Generic;
using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.Events;
using SlotHero.Sanctum;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 이벤트가 묻고 시키는 것을 진행 중인 런에 대고 처리한다.
    ///
    /// 이벤트 코드는 런 데이터도 저장도 모른다. 무엇을 할지만 적어 보내고
    /// 실제로 세고 바꾸는 일은 여기서 한다.
    /// 이벤트 기획서가 문양과 코인과 유물의 목록을 다른 문서로 넘겼기 때문이다.
    ///
    /// 체력 수치는 거의 최대 체력의 비율로 적혀 있어 여기서 정수로 바꾼다.
    /// 올림이 아니라 반올림으로 하고 최소 1 은 깎거나 채우게 한다.
    /// 최대 체력이 작을 때 0 이 되어 아무 일도 안 일어나는 것을 막으려는 것이다.
    /// </summary>
    public class RunEventWorld : IEventWorld
    {
        private readonly RunContext _context;
        private readonly RunCatalogConfig _config;

        /// 태그 이름을 찾는 곳. 현재 빌드 화면과 같은 이름을 쓴다. 없으면 영어 이름이 나온다.
        private readonly CurrentBuildVisualConfig _tagNames;

        /// 이번 전투에 보상을 두 배로 줄지. 전투를 여는 선택지가 켜 둔다.
        private bool _doubleCombatReward;

        /// 도전 이벤트가 전투에 거는 부정 효과 목록. 이벤트 목록 에셋(`RunEventConfig`)에서 받는다.
        /// 받지 않으면 3장 의 여섯 가지를 쓴다.
        private List<EventCombatPenalty> _combatPenalties = EventCombatPenalty.SpecDefaults();

        public RunEventWorld(RunContext context, RunCatalogConfig config)
            : this(context, config, null)
        {
        }

        public RunEventWorld(RunContext context, RunCatalogConfig config, CurrentBuildVisualConfig tagNames)
        {
            _context = context;
            _config = config;
            _tagNames = tagNames;
        }

        /// <summary>
        /// 지금 굴리는 이벤트의 식별자. 이벤트에서만 나오는 코인과 유물을 가리는 데 쓴다(`ItemDefinition.OnlyInEvents`).
        /// 무작위로 주거나 바꿔 줄 후보에서 이벤트 경로가 막힌 것을 뺀다(`ItemDefinition.CanComeFrom`). 2026년 10월 9일 원재.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>이벤트가 그 코인이나 유물을 무작위로 줄 수 있는지.</summary>
        private bool EventMayGive(ItemDefinition item)
        {
            return item.CanComeFrom(ItemSource.Event, EventId);
        }

        /// <summary>이번 전투 보상을 두 배로 줄지. 3장 도전 이벤트 공통 규칙.</summary>
        public bool DoubleCombatReward
        {
            get { return _doubleCombatReward; }
            set { _doubleCombatReward = value; }
        }

        /// <summary>도전 이벤트가 전투에 거는 부정 효과 목록. 3장 도전 이벤트 공통 규칙. 비우면 3장 의 여섯 가지로 돌아간다.</summary>
        public List<EventCombatPenalty> CombatPenalties
        {
            get { return _combatPenalties; }
            set { _combatPenalties = value != null && value.Count > 0 ? value : EventCombatPenalty.SpecDefaults(); }
        }

        public int Gold
        {
            get { return _context != null ? _context.Gold.Gold : 0; }
        }

        public int Health
        {
            get { return _context != null ? _context.Health : 0; }
        }

        public int MaxHealth
        {
            get { return _context != null ? _context.MaxHealth : 0; }
        }

        /// <inheritdoc />
        public void Collect(EventFillRequest request, EventRandom random, List<EventItem> into)
        {
            int count = request.Count;

            if (into == null || _context == null || _config == null || count <= 0)
            {
                return;
            }

            List<EventItem> pool = new List<EventItem>();

            switch (request.Fill)
            {
                case EventChoiceFill.RandomTags:
                    CollectTags(SymbolTags.InOrder, request.ExcludeIds, pool);
                    break;

                case EventChoiceFill.OwnedTags:
                    CollectTags(GetOwnedTags(), request.ExcludeIds, pool);
                    break;

                case EventChoiceFill.TagsOfRemembered:
                    CollectTags(GetTagsOf(request.SourceId), request.ExcludeIds, pool);
                    break;

                case EventChoiceFill.RandomSymbols:
                    CollectAllSymbols(pool);
                    break;

                case EventChoiceFill.OwnedSymbols:
                    CollectOwnedSymbols(pool);
                    break;

                case EventChoiceFill.MostCommonTagSymbols:
                    CollectMostCommonTagSymbols(pool);
                    break;

                case EventChoiceFill.RandomCoins:
                    CollectItems(_config.Coins, pool, null);
                    break;

                case EventChoiceFill.OwnedCoins:
                    CollectOwnedCoins(pool);
                    break;

                case EventChoiceFill.RandomRelics:
                    CollectItems(_config.Relics, pool, _context.Run.Owned.Relics);
                    break;

                case EventChoiceFill.CombatPenalties:
                    for (int i = 0; i < _combatPenalties.Count; i++)
                    {
                        pool.Add(new EventItem(_combatPenalties[i].Id, _combatPenalties[i].Text));
                    }

                    break;
            }

            // 이벤트마다 정한 등급 조건. 18 주인 없는 유물 은 고급 이상, 27 축복 은 일반 또는 고급이다.
            // 2026년 10월 8일 외부 검토가 빠진 것을 짚어 더했다.
            FilterByRarity(pool, request.MinRarity, request.MaxRarity);

            if (pool.Count == 0)
            {
                return;
            }

            // 등급마다 비중이 있으면 등급부터 뽑는다. 26 반짝이는 것 의 일반 70%, 고급 30% 가 그렇다.
            if (request.RarityWeights != null && request.RarityWeights.Count > 0)
            {
                TakeByRarityWeights(pool, request.RarityWeights, count, random, into);
                return;
            }

            random.Shuffle(pool);

            int take = count < pool.Count ? count : pool.Count;
            for (int i = 0; i < take; i++)
            {
                into.Add(pool[i]);
            }
        }

        /// <summary>후보를 그 등급 사이로 줄인다. 등급이 없는 것(문양과 태그)은 건드리지 않는다. -1 은 따지지 않는다.</summary>
        private void FilterByRarity(List<EventItem> pool, int min, int max)
        {
            if (min < 0 && max < 0)
            {
                return;
            }

            for (int i = pool.Count - 1; i >= 0; i--)
            {
                int rarity = RarityOf(pool[i].Id);
                if (rarity < 0)
                {
                    continue;
                }

                if ((min >= 0 && rarity < min) || (max >= 0 && rarity > max))
                {
                    pool.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 하나를 뽑을 때마다 등급을 비중대로 먼저 굴리고 그 등급에서 하나를 고른다. 겹치지 않게 뺀다.
        /// 남은 후보에 없는 등급은 비중에서 빠진다. 비중이 있는 등급의 후보가 다 떨어지면 거기서 멈춘다.
        /// </summary>
        private void TakeByRarityWeights(
            List<EventItem> pool, List<int> weights, int count, EventRandom random, List<EventItem> into)
        {
            // 후보마다의 등급과 등급마다의 후보 수를 한 번만 세어 두고 뽑을 때마다 고쳐 쓴다.
            // 예전에는 등급마다 후보 전체를 다시 훑고 후보마다 목록에서 등급을 다시 찾았다. 2026년 10월 9일 외부 검토가 권했다.
            // 난수를 굴리는 차례와 후보 차례는 예전과 같다. 같은 시드면 같은 것이 나온다.
            List<int> rarities = new List<int>(pool.Count);
            int[] counts = new int[weights.Count];
            for (int i = 0; i < pool.Count; i++)
            {
                int rarity = RarityOf(pool[i].Id);
                rarities.Add(rarity);
                if (rarity >= 0 && rarity < counts.Length)
                {
                    counts[rarity]++;
                }
            }

            for (int n = 0; n < count; n++)
            {
                int total = 0;
                for (int r = 0; r < weights.Count; r++)
                {
                    if (weights[r] > 0 && counts[r] > 0)
                    {
                        total += weights[r];
                    }
                }

                if (total <= 0)
                {
                    return;
                }

                int roll = random.Range(0, total);
                int picked = -1;
                for (int r = 0; r < weights.Count; r++)
                {
                    if (weights[r] <= 0 || counts[r] == 0)
                    {
                        continue;
                    }

                    roll -= weights[r];
                    if (roll < 0)
                    {
                        picked = r;
                        break;
                    }
                }

                // 그 등급에서 nth 번째 후보. 예전처럼 등급이 같은 후보를 차례대로 세어 고른다.
                int nth = random.Range(0, counts[picked]);
                int chosen = -1;
                for (int i = 0; i < pool.Count; i++)
                {
                    if (rarities[i] != picked)
                    {
                        continue;
                    }

                    if (nth == 0)
                    {
                        chosen = i;
                        break;
                    }

                    nth--;
                }

                into.Add(pool[chosen]);
                pool.RemoveAt(chosen);
                rarities.RemoveAt(chosen);
                counts[picked]--;
            }
        }

        /// <summary>코인이나 유물의 등급. 0 일반 ~ 4 전설. 목록에 없으면(문양, 태그) -1 이다. 물건 목록의 색인으로 찾는다.</summary>
        private int RarityOf(string id)
        {
            ItemDefinition item;
            if (_config.TryGetItem(id, out item))
            {
                return (int)item.Rarity;
            }

            return -1;
        }

        /// <inheritdoc />
        public void Decide(EventAction action, EventTarget target, EventRandom random, List<EventItem> into)
        {
            if (action == null || into == null || _context == null || _config == null)
            {
                return;
            }

            switch (action.Kind)
            {
                case EventActionKind.GainSymbolByTags:
                    AddDecided(ResolveSymbolByTags(target.GetRecalled(0), target.Id, random), into);
                    break;

                case EventActionKind.ReshapeSymbol:
                    AddDecided(ResolveReshape(target.GetRecalled(0), target.GetRecalled(1), target.Id, random), into);
                    break;

                case EventActionKind.ChangeSymbol:
                    AddDecided(PickAnySymbolId(random), into);
                    break;

                // 문양 거래(06), 유물 교환(13) 과 같이 코인 환전(20)도 받을 코인을 미리 정해 보여 준다.
                // 2026년 10월 5일 원재가 문양, 유물, 코인 모두 같은 방식으로 하라고 했다.
                case EventActionKind.ChangeCoin:
                    if (!string.IsNullOrEmpty(target.Id))
                    {
                        AddDecided(PickCoinOfRarity((int)GetRarity(_config.Coins, target.Id), random), into);
                    }

                    break;

                case EventActionKind.GainRelic:
                    DecideRelics(action.Count - (string.IsNullOrEmpty(target.Id) ? 0 : 1), target.Id, random, into);
                    break;

                case EventActionKind.ChangeRelic:
                    // 13 유물 교환. 내줄 유물을 정하고, 그 등급에서 비중대로 몇 단계 위의 유물을 정한다.
                    string give = PickTradableRelic(action, random);
                    if (string.IsNullOrEmpty(give))
                    {
                        return;
                    }

                    AddDecided(give, into);
                    AddDecided(PickTradeRelic(action, RarityOf(give), random), into);
                    break;
            }
        }

        /// <summary>가지지 않은 유물을 겹치지 않게 count 개 정한다. 남은 것이 모자라면 있는 만큼이다.</summary>
        private void DecideRelics(int count, string skipId, EventRandom random, List<EventItem> into)
        {
            List<EventItem> pool = new List<EventItem>();
            CollectItems(_config.Relics, pool, _context.Run.Owned.Relics);

            for (int i = pool.Count - 1; i >= 0; i--)
            {
                if (pool[i].Id == skipId)
                {
                    pool.RemoveAt(i);
                }
            }

            for (int n = 0; n < count && pool.Count > 0; n++)
            {
                int index = random.Range(0, pool.Count);
                into.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        private void AddDecided(string id, List<EventItem> into)
        {
            if (!string.IsNullOrEmpty(id))
            {
                into.Add(new EventItem(id, _config.GetDisplayName(id)));
            }
        }

        /// <inheritdoc />
        public bool NeedsRoom(EventAction action)
        {
            return RoomShortfall(action) > 0;
        }

        /// <summary>
        /// 그 작업을 하려면 코인이나 유물을 몇 개 버려야 하는지. 자리가 넉넉하면 0 이다.
        /// 코인 받기는 하나, 유물 받기는 `Count` 개를 받는다. 바꾸는 작업은 하나를 내주고 하나를 받으므로 자리가 늘지 않는다.
        /// </summary>
        public int RoomShortfall(EventAction action)
        {
            if (action == null || _context == null)
            {
                return 0;
            }

            switch (action.Kind)
            {
                case EventActionKind.GainCoin:
                    return _context.Run.Owned.CoinRoom >= 1 ? 0 : 1;

                case EventActionKind.GainRelic:
                    int wanted = action.Count < 1 ? 1 : action.Count;
                    int room = _context.Run.Owned.RelicRoom;
                    return wanted > room ? wanted - room : 0;

                default:
                    return 0;
            }
        }

        /// <inheritdoc />
        public bool CanDo(EventAction action, EventTarget target, out string reason)
        {
            reason = string.Empty;

            if (action == null || _context == null)
            {
                return true;
            }

            // 값을 치르는 작업은 골드가 모자라면 못 한다.
            // 4장 공통 규칙 의 "조건을 채우지 못한 선택지는 비활성으로 표시하며
            // 부족한 조건을 함께 보여 준다"가 이것이다.
            if (action.GoldCost > 0 && Gold < action.GoldCost)
            {
                reason = "골드 " + action.GoldCost + " 필요";
                return false;
            }

            // 함께 깎는 최대 체력. 지금 체력이 아니라 **최대 체력**이 1 아래로 내려가는지만 본다.
            // 최대 체력을 잃어도 지금 체력은 그대로이고, 줄어든 최대를 넘을 때만 그 최대로 맞춘다(`RunContext.ChangeMaxHealth`).
            // 예전에는 지금 체력에서 뺐다. 그래서 체력 10 / 100 이면 12 잠든 보물 의 둘 다 가진다 가 막혔는데
            // 실제 결과는 10 / 90 이다. 2026년 10월 9일 외부 검토가 짚어 고쳤다.
            if (action.MaxHealthCostRatio > 0f && !CanLoseMaxHealth(ByMaxHealthRatio(action.MaxHealthCostRatio)))
            {
                reason = EventAction.NotEnoughMaxHealth;
                return false;
            }

            switch (action.Kind)
            {
                case EventActionKind.LoseGold:
                    if (action.Amount > 0 && Gold <= 0)
                    {
                        reason = "골드 없음";
                        return false;
                    }

                    break;

                case EventActionKind.GainGold:
                    break;

                case EventActionKind.LoseHealth:
                    // 4장 공통 규칙 · 체력 손실로 현재 체력이 1 미만이 되는 선택지는 비활성이다.
                    if (Health - ByRatio(action) < 1)
                    {
                        reason = EventAction.NotEnoughHealth;
                        return false;
                    }

                    break;

                case EventActionKind.LoseMaxHealth:
                    // 최대 체력 손실은 지금 체력을 1 아래로 떨어뜨리지 않는다. 최대 체력이 1 아래로 내려갈 때만 막는다.
                    if (!CanLoseMaxHealth(ByRatio(action)))
                    {
                        reason = EventAction.NotEnoughMaxHealth;
                        return false;
                    }

                    break;

                case EventActionKind.LoseSymbol:
                case EventActionKind.ChangeSymbol:
                case EventActionKind.ChangeAllSymbols:
                    if (_context.Run.Owned.Symbols.Count == 0)
                    {
                        reason = "문양 없음";
                        return false;
                    }

                    // 문양을 잃으면 덱이 줄어든다. 릴 수보다 적어지면 전투를 돌릴 수 없다(전투 시스템 기획서 04).
                    // 2026년 10월 8일 외부 검토가 엔드리스 모드처럼 런이 길어지면 10 정신 붕괴 를 거듭 만나 덱이 줄 수 있다고 짚었다.
                    if (action.Kind == EventActionKind.LoseSymbol && !CanLoseSymbol())
                    {
                        reason = "문양 " + _config.MinimumDeckSize + "장 유지";
                        return false;
                    }

                    break;

                case EventActionKind.LoseCoin:
                case EventActionKind.ChangeCoin:
                case EventActionKind.SellCoin:
                case EventActionKind.DowngradeCoin:
                    if (_context.Run.Owned.CoinIds.Count == 0)
                    {
                        reason = "코인 없음";
                        return false;
                    }

                    // 25 미치광이 대장장이 는 고급 이상 코인만 노린다. 그런 코인이 없으면 못 한다.
                    if (string.IsNullOrEmpty(target.Id) && action.TargetMinRarity >= 0
                        && string.IsNullOrEmpty(PickOwnedCoinId(null, action.TargetMinRarity)))
                    {
                        reason = ItemDetailTexts.GetRarityName((ItemRarity)action.TargetMinRarity) + " 이상 코인 없음";
                        return false;
                    }

                    break;

                case EventActionKind.GainRelic:
                    // 받을 유물이 모자라면 못 한다. 12 잠든 보물 의 둘 다 가진다 는 유물 둘과 함께 최대 체력을 치르는데,
                    // 예전에는 유물을 다 가진 상태에서 고르면 받는 것 없이 최대 체력만 잃었다. 2026년 10월 9일 외부 검토가 짚었다.
                    // 둘 다 가진다 는 기획서가 "유물 A와 B를 모두 획득한다" 라 둘이 없으면 막는다.
                    int wanted = action.Count < 1 ? 1 : action.Count;
                    int gainable = CountGainableRelics();
                    if (gainable < wanted)
                    {
                        reason = gainable == 0 ? "받을 유물 없음" : "받을 유물 부족";
                        return false;
                    }

                    break;

                case EventActionKind.ChangeRelic:
                    if (_context.Run.Owned.Relics.Count == 0)
                    {
                        reason = "유물 없음";
                        return false;
                    }

                    // 받을 유물이 내줄 유물보다 낮은 등급이면 안 되므로 바꿔 줄 유물이 있어야 한다.
                    if (string.IsNullOrEmpty(PickTradableRelic(action, null)))
                    {
                        reason = "바꿔 받을 유물 없음";
                        return false;
                    }

                    break;

                case EventActionKind.ReshapeSymbol:
                    // 바꿀 문양을 앞 화면에서 골랐는데 그사이 잃었으면 못 한다.
                    if (_context.Run.Owned.GetSymbolCount(target.GetRecalled(0)) <= 0)
                    {
                        reason = "문양 없음";
                        return false;
                    }

                    break;
            }

            return true;
        }

        /// <inheritdoc />
        public string Apply(EventAction action, EventTarget target, EventRandom random)
        {
            string targetId = target.Id;

            if (action == null || _context == null || _config == null)
            {
                return string.Empty;
            }

            // 값을 먼저 치르고 함께 받는 골드를 더한다.
            // 기획서가 "10G를 지불하고 교환한다", "체력을 잃고 00G를 획득한다" 처럼
            // 한 선택지에 둘을 함께 적어 두었다.
            string paid = string.Empty;

            if (action.GoldCost > 0)
            {
                _context.Gold.TrySpend(action.GoldCost);
                paid = Cost("골드 " + action.GoldCost) + " 을 치렀다. ";
            }

            if (action.GoldGain > 0)
            {
                _context.GainGold(action.GoldGain);
                paid += Gain("골드 " + action.GoldGain) + " 을 받았다. ";
            }

            if (action.MaxHealthCostRatio > 0f)
            {
                paid += ChangeMaxHealth(-ByMaxHealthRatio(action.MaxHealthCostRatio)) + " ";
            }

            return paid + ApplyMain(action, target, random);
        }

        private string ApplyMain(EventAction action, EventTarget target, EventRandom random)
        {
            string targetId = target.Id;

            switch (action.Kind)
            {
                case EventActionKind.GainSymbolByTags:
                    return GainSymbolByTags(target.GetRecalled(0), targetId, target.GetDecided(0), random);

                case EventActionKind.ReshapeSymbol:
                    return ReshapeSymbol(target.GetRecalled(0), target.GetRecalled(1), targetId, target.GetDecided(0), random);

                case EventActionKind.PurgeTag:
                    return PurgeTag(targetId, random);

                case EventActionKind.GainGold:
                    _context.GainGold(action.Amount);
                    return Gain("골드 " + action.Amount) + " 을 얻었다.";

                case EventActionKind.LoseGold:
                    return LoseGold(action.Amount);

                case EventActionKind.Heal:
                    return HealBy(ByRatio(action));

                case EventActionKind.LoseHealth:
                    return Hurt(ByRatio(action));

                case EventActionKind.GainMaxHealth:
                    return ChangeMaxHealth(ByRatio(action));

                case EventActionKind.LoseMaxHealth:
                    return ChangeMaxHealth(-ByRatio(action));

                case EventActionKind.ChangeCoinCapacity:
                    return ChangeCapacity("코인", _context.ChangeCoinCapacity(action.Amount));

                case EventActionKind.ChangeRelicCapacity:
                    return ChangeCapacity("유물", _context.ChangeRelicCapacity(action.Amount));

                case EventActionKind.GainSymbol:
                    return GainSymbol(targetId, random);

                case EventActionKind.LoseSymbol:
                    return LoseSymbol(targetId, random);

                case EventActionKind.ChangeSymbol:
                    return ChangeSymbol(targetId, target.GetDecided(0), random);

                case EventActionKind.ChangeAllSymbols:
                    return ChangeAllSymbols(random);

                case EventActionKind.GainRelic:
                    return GainRelics(targetId, action.Count, target.Decided, random);

                case EventActionKind.ChangeRelic:
                    return ChangeRelic(action, target.GetDecided(0), target.GetDecided(1), random);

                case EventActionKind.GainCoin:
                    return GainCoin(targetId, random);

                case EventActionKind.LoseCoin:
                    return LoseCoin(targetId, random);

                case EventActionKind.ChangeCoin:
                    return ChangeCoin(targetId, random, 0, target.GetDecided(0), -1);

                case EventActionKind.DowngradeCoin:
                    return ChangeCoin(targetId, random, -1, target.GetDecided(0), action.TargetMinRarity);

                case EventActionKind.UpgradeCoin:
                    return ChangeCoin(targetId, random, 1, target.GetDecided(0), -1);

                case EventActionKind.SellCoin:
                    return SellCoin(targetId, random);
            }

            return string.Empty;
        }

        // ---- 체력과 골드 ----

        /// <summary>
        /// 최대 체력의 비율을 정수로 바꾼다. 비율이 0 이면 고정 수치를 쓴다.
        /// 선택지 칸에 적는 숫자와 같아야 하므로 규칙은 `EventAction.HealthAmount` 한 곳에 있다.
        /// </summary>
        private int ByRatio(EventAction action)
        {
            return action.HealthAmount(MaxHealth);
        }

        /// <summary>최대 체력의 비율을 정수로 바꾼다. 반올림하고 최소 1 이다.</summary>
        private int ByMaxHealthRatio(float ratio)
        {
            return EventAction.ByRatio(MaxHealth, ratio);
        }

        private string LoseGold(int amount)
        {
            int taken = amount < Gold ? amount : Gold;

            if (taken <= 0)
            {
                return "내줄 골드가 없었다.";
            }

            _context.Gold.TrySpend(taken);
            return Cost("골드 " + taken) + " 을 잃었다.";
        }

        private string HealBy(int amount)
        {
            int before = Health;
            _context.Heal(amount);
            int healed = Health - before;

            return healed > 0 ? Gain("체력 " + healed) + " 을 회복했다." : "더 회복할 체력이 없었다.";
        }

        private string Hurt(int amount)
        {
            _context.TakeDamage(amount);
            return Cost("체력 " + amount) + " 을 잃었다.";
        }

        /// <summary>소지 한도가 바뀐 결과 문장.</summary>
        private static string ChangeCapacity(string noun, int changed)
        {
            if (changed > 0)
            {
                return Gain("최대 " + noun + " 수 " + changed) + " 이 늘었다.";
            }

            return changed < 0
                ? Cost("최대 " + noun + " 수 " + (-changed)) + " 이 줄었다."
                : "최대 " + noun + " 수가 그대로다.";
        }

        /// <summary>최대 체력을 그만큼 잃어도 1 이상 남는지.</summary>
        private bool CanLoseMaxHealth(int amount)
        {
            return MaxHealth - amount >= 1;
        }

        /// <summary>
        /// 최대 체력을 영구히 올리거나 내린다. 규칙은 `RunContext.ChangeMaxHealth` 에 있다.
        /// 올릴 때는 지금 체력도 같은 만큼 오른다.
        /// </summary>
        private string ChangeMaxHealth(int amount)
        {
            int changed = _context.ChangeMaxHealth(amount);

            if (changed > 0)
            {
                return Gain("최대 체력과 체력 " + changed) + " 이 늘었다.";
            }

            return changed < 0
                ? Cost("최대 체력 " + (-changed)) + " 이 줄었다."
                : "최대 체력이 그대로다.";
        }

        // ---- 문양 ----

        private string GainSymbol(string targetId, EventRandom random)
        {
            string id = !string.IsNullOrEmpty(targetId) ? targetId : PickAnySymbolId(random);

            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            _context.Run.Owned.AddSymbol(id, 1);
            return Gain(_config.GetDisplayName(id)) + " 을 얻었다.";
        }

        private string LoseSymbol(string targetId, EventRandom random)
        {
            if (!CanLoseSymbol())
            {
                return "문양을 " + _config.MinimumDeckSize + "장 아래로 줄일 수 없었다.";
            }

            string id = !string.IsNullOrEmpty(targetId) ? targetId : PickOwnedSymbolId(random);

            if (string.IsNullOrEmpty(id) || _context.Run.Owned.RemoveSymbol(id, 1) <= 0)
            {
                return "잃을 문양이 없었다.";
            }

            return Cost(_config.GetDisplayName(id)) + " 을 잃었다.";
        }

        /// <summary>
        /// 문양 한 장을 잃어도 덱이 가장 작은 장수 이상인지.
        /// 전투 시스템 기획서 04 의 "문양 풀이 슬롯 릴 갯수보다 적은 경우는 존재하지 않는다" 를 지키는 것이다.
        /// </summary>
        private bool CanLoseSymbol()
        {
            return _context.Run.Owned.TotalSymbolCount - 1 >= _config.MinimumDeckSize;
        }

        private string ChangeSymbol(string targetId, string decidedTo, EventRandom random)
        {
            string from = !string.IsNullOrEmpty(targetId) ? targetId : PickOwnedSymbolId(random);

            if (string.IsNullOrEmpty(from) || _context.Run.Owned.RemoveSymbol(from, 1) <= 0)
            {
                return "바꿀 문양이 없었다.";
            }

            // 칸에 이름을 적어 둔 것이 있으면 그것을 받는다. 06 문양 거래 가 그렇다.
            string to = !string.IsNullOrEmpty(decidedTo) ? decidedTo : PickAnySymbolId(random);
            _context.Run.Owned.AddSymbol(to, 1);

            return Cost(_config.GetDisplayName(from)) + " 이 " + Gain(_config.GetDisplayName(to)) + " 이 되었다.";
        }

        /// <summary>
        /// 가진 문양을 모두 무작위 문양으로 바꾼다. 장수는 그대로다.
        /// 03 판도라의 상자 가 이것이다.
        /// </summary>
        private string ChangeAllSymbols(EventRandom random)
        {
            RunOwnedData owned = _context.Run.Owned;
            int total = owned.TotalSymbolCount;

            if (total <= 0)
            {
                return "바꿀 문양이 없었다.";
            }

            owned.Symbols.Clear();

            for (int i = 0; i < total; i++)
            {
                owned.AddSymbol(PickAnySymbolId(random), 1);
            }

            return Cost("문양 " + total + "장") + " 이 모두 " + Gain("무작위 문양") + " 으로 뒤바뀌었다.";
        }

        private string PickAnySymbolId(EventRandom random)
        {
            if (_config.Symbols.Count == 0)
            {
                return string.Empty;
            }

            return _config.Symbols[random.Range(0, _config.Symbols.Count)].Id;
        }

        private string PickOwnedSymbolId(EventRandom random)
        {
            List<EventItem> owned = new List<EventItem>();
            CollectOwnedSymbols(owned);

            if (owned.Count == 0)
            {
                return string.Empty;
            }

            return owned[random.Range(0, owned.Count)].Id;
        }

        private void CollectAllSymbols(List<EventItem> into)
        {
            for (int i = 0; i < _config.Symbols.Count; i++)
            {
                into.Add(new EventItem(_config.Symbols[i].Id, _config.Symbols[i].DisplayName));
            }
        }

        /// <summary>가진 문양을 모은다. 같은 문양이 여러 장이면 한 번만 넣는다.</summary>
        private void CollectOwnedSymbols(List<EventItem> into)
        {
            List<OwnedSymbol> symbols = _context.Run.Owned.Symbols;

            for (int i = 0; i < symbols.Count; i++)
            {
                into.Add(new EventItem(symbols[i].SymbolId, _config.GetDisplayName(symbols[i].SymbolId)));
            }
        }

        /// <summary>
        /// 가장 많이 가진 태그를 가진 보유 문양을 장마다 하나씩 모은다. 10 정신 붕괴.
        /// 태그 수를 세고 가장 큰 수를 고른 뒤 그 태그를 가진 문양만 남긴다.
        /// 장마다 넣으므로 섞어 뽑으면 같은 문양이 여러 번 나올 수 있다. 기획서 비고 그대로다.
        /// </summary>
        private void CollectMostCommonTagSymbols(List<EventItem> into)
        {
            List<SymbolTagType> top = GetMostCommonTags();
            if (top.Count == 0)
            {
                return;
            }

            List<OwnedSymbol> symbols = _context.Run.Owned.Symbols;

            for (int i = 0; i < symbols.Count; i++)
            {
                bool hasTop = false;
                for (int t = 0; t < top.Count; t++)
                {
                    if (SymbolHasTag(symbols[i].SymbolId, top[t]))
                    {
                        hasTop = true;
                        break;
                    }
                }

                if (!hasTop)
                {
                    continue;
                }

                for (int c = 0; c < symbols[i].Count; c++)
                {
                    into.Add(new EventItem(symbols[i].SymbolId, _config.GetDisplayName(symbols[i].SymbolId)));
                }
            }
        }

        /// <summary>가진 문양에서 가장 많이 나온 태그. 장수대로 센다. 같은 수면 모두 돌려준다.</summary>
        public List<SymbolTagType> GetMostCommonTags()
        {
            IReadOnlyList<SymbolTagType> all = SymbolTags.InOrder;
            int[] counts = new int[all.Count];
            List<OwnedSymbol> symbols = _context != null ? _context.Run.Owned.Symbols : new List<OwnedSymbol>();

            for (int i = 0; i < symbols.Count; i++)
            {
                for (int t = 0; t < all.Count; t++)
                {
                    if (SymbolHasTag(symbols[i].SymbolId, all[t]))
                    {
                        counts[t] += symbols[i].Count;
                    }
                }
            }

            int best = 0;
            for (int t = 0; t < counts.Length; t++)
            {
                if (counts[t] > best)
                {
                    best = counts[t];
                }
            }

            List<SymbolTagType> top = new List<SymbolTagType>();
            if (best <= 0)
            {
                return top;
            }

            for (int t = 0; t < counts.Length; t++)
            {
                if (counts[t] == best)
                {
                    top.Add(all[t]);
                }
            }

            return top;
        }

        // ---- 태그 ----
        //
        // 태그는 식별자로 영어 이름을 쓴다. `SymbolTagType` 의 이름 그대로다.
        // 화면에는 현재 빌드 화면과 같은 한국어 이름이 나온다.

        /// <summary>
        /// 두 태그를 모두 가진 문양 하나를 받는다. 02 힘을 새기는 것.
        /// 문양은 태그를 둘씩 가지고 모든 짝에 문양이 하나씩 있어 하나로 정해진다.
        /// 짝이 없으면 지목한 태그를 가진 무작위 문양을 준다. 이벤트가 빈손으로 끝나지 않게 하려는 것이다.
        /// </summary>
        private string GainSymbolByTags(string firstTagId, string secondTagId, string decidedId, EventRandom random)
        {
            string id = !string.IsNullOrEmpty(decidedId) ? decidedId : ResolveSymbolByTags(firstTagId, secondTagId, random);

            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            _context.Run.Owned.AddSymbol(id, 1);
            return Gain(_config.GetDisplayName(id)) + " 을 얻었다.";
        }

        /// <summary>두 태그를 모두 가진 문양. 짝이 없으면 둘째 태그를 가진 무작위 문양이다.</summary>
        private string ResolveSymbolByTags(string firstTagId, string secondTagId, EventRandom random)
        {
            SymbolTagType first;
            SymbolTagType second;
            bool hasFirst = TryParseTag(firstTagId, out first);
            bool hasSecond = TryParseTag(secondTagId, out second);

            string id = hasFirst && hasSecond ? FindSymbolWithTags(first, second) : string.Empty;

            if (string.IsNullOrEmpty(id))
            {
                id = hasSecond ? PickSymbolWithTag(second, random) : PickAnySymbolId(random);
            }

            return id;
        }

        /// <summary>
        /// 04 닮은 모습 에서 문양이 바뀔 모습. 버릴 태그가 아닌 쪽을 남기고 새 태그를 붙인 문양이다.
        /// 버릴 태그를 못 읽으면 첫째를 버리고 둘째를 남긴다. 못 찾으면 빈 글이다.
        /// </summary>
        private string ResolveReshape(string symbolId, string dropTagId, string newTagId, EventRandom random)
        {
            SymbolTagType first;
            SymbolTagType second;
            SymbolTagType drop;
            SymbolTagType added;

            if (!_config.TryGetSymbolTags(symbolId, out first, out second) || !TryParseTag(newTagId, out added))
            {
                return string.Empty;
            }

            SymbolTagType kept = second;
            if (TryParseTag(dropTagId, out drop))
            {
                kept = drop == first ? second : first;
            }

            string to = FindSymbolWithTags(kept, added);
            return string.IsNullOrEmpty(to) ? PickSymbolWithTag(added, random) : to;
        }

        /// <summary>
        /// 문양의 태그 하나를 바꾼다. 04 닮은 모습.
        /// 버릴 태그가 아닌 쪽을 남기고, 그것과 새 태그를 가진 문양으로 바뀐다.
        /// </summary>
        private string ReshapeSymbol(string symbolId, string dropTagId, string newTagId, string decidedTo, EventRandom random)
        {
            SymbolTagType first;
            SymbolTagType second;
            SymbolTagType added;

            if (_context.Run.Owned.GetSymbolCount(symbolId) <= 0
                || !_config.TryGetSymbolTags(symbolId, out first, out second))
            {
                return "바꿀 문양이 없었다.";
            }

            if (!TryParseTag(newTagId, out added))
            {
                return "바뀔 모습을 찾지 못했다.";
            }

            string to = !string.IsNullOrEmpty(decidedTo) ? decidedTo : ResolveReshape(symbolId, dropTagId, newTagId, random);

            if (string.IsNullOrEmpty(to) || _context.Run.Owned.RemoveSymbol(symbolId, 1) <= 0)
            {
                return "바꿀 문양이 없었다.";
            }

            _context.Run.Owned.AddSymbol(to, 1);
            return Cost(_config.GetDisplayName(symbolId)) + " 이 " + Gain(_config.GetDisplayName(to)) + " 이 되었다.";
        }

        /// <summary>
        /// 그 태그를 가진 문양을 모두 그 태그가 없는 무작위 문양으로 바꾼다. 05 정화.
        /// 한 장씩 따로 뽑는다. 같은 문양 두 장이 같은 문양으로 바뀌지 않아도 된다.
        /// </summary>
        private string PurgeTag(string tagId, EventRandom random)
        {
            SymbolTagType tag;
            if (!TryParseTag(tagId, out tag))
            {
                return "지울 태그를 찾지 못했다.";
            }

            RunOwnedData owned = _context.Run.Owned;

            // 바꾸는 도중에 목록이 바뀌므로 바꿀 것을 먼저 모은다.
            List<OwnedSymbol> targets = new List<OwnedSymbol>();
            for (int i = 0; i < owned.Symbols.Count; i++)
            {
                if (SymbolHasTag(owned.Symbols[i].SymbolId, tag))
                {
                    targets.Add(owned.Symbols[i]);
                }
            }

            int changed = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                string id = targets[i].SymbolId;
                int count = targets[i].Count;

                int removed = owned.RemoveSymbol(id, count);

                for (int j = 0; j < removed; j++)
                {
                    string to = PickSymbolWithoutTag(tag, random);
                    if (string.IsNullOrEmpty(to))
                    {
                        continue;
                    }

                    owned.AddSymbol(to, 1);
                    changed++;
                }
            }

            if (changed == 0)
            {
                return GetTagName(tag) + " 을 가진 문양이 없었다.";
            }

            return Cost(GetTagName(tag) + " 을 가진 문양 " + changed + "장") + " 이 " + Gain("다른 문양") + " 으로 바뀌었다.";
        }

        /// <summary>태그 목록을 후보로 넣는다. 뺄 것은 뺀다.</summary>
        private void CollectTags(IReadOnlyList<SymbolTagType> tags, List<string> excludeIds, List<EventItem> into)
        {
            List<SymbolTagType> excluded = ExpandExcludedTags(excludeIds);

            for (int i = 0; i < tags.Count; i++)
            {
                if (excluded.Contains(tags[i]))
                {
                    continue;
                }

                into.Add(new EventItem(tags[i].ToString(), GetTagName(tags[i])));
            }
        }

        /// <summary>
        /// 뺄 것을 태그로 푼다. 태그는 그대로, 문양은 그 문양의 태그 둘이 된다.
        /// 04 닮은 모습 이 원래 문양의 두 태그를 모두 빼는 데 쓴다.
        /// </summary>
        private List<SymbolTagType> ExpandExcludedTags(List<string> excludeIds)
        {
            List<SymbolTagType> excluded = new List<SymbolTagType>();

            if (excludeIds == null)
            {
                return excluded;
            }

            for (int i = 0; i < excludeIds.Count; i++)
            {
                SymbolTagType tag;
                SymbolTagType second;

                if (TryParseTag(excludeIds[i], out tag))
                {
                    excluded.Add(tag);
                }
                else if (_config.TryGetSymbolTags(excludeIds[i], out tag, out second))
                {
                    excluded.Add(tag);
                    excluded.Add(second);
                }
            }

            return excluded;
        }

        /// <summary>가진 문양에 붙은 태그. 겹치지 않게 태양계 차례로 모은다.</summary>
        private List<SymbolTagType> GetOwnedTags()
        {
            List<SymbolTagType> found = new List<SymbolTagType>();
            IReadOnlyList<SymbolTagType> all = SymbolTags.InOrder;

            for (int i = 0; i < all.Count; i++)
            {
                for (int j = 0; j < _context.Run.Owned.Symbols.Count; j++)
                {
                    if (SymbolHasTag(_context.Run.Owned.Symbols[j].SymbolId, all[i]))
                    {
                        found.Add(all[i]);
                        break;
                    }
                }
            }

            return found;
        }

        /// <summary>그 문양의 태그 둘. 문양을 못 찾으면 빈 목록이다.</summary>
        private List<SymbolTagType> GetTagsOf(string symbolId)
        {
            List<SymbolTagType> tags = new List<SymbolTagType>();
            SymbolTagType first;
            SymbolTagType second;

            if (_config.TryGetSymbolTags(symbolId, out first, out second))
            {
                tags.Add(first);
                if (second != first)
                {
                    tags.Add(second);
                }
            }

            return tags;
        }

        private bool SymbolHasTag(string symbolId, SymbolTagType tag)
        {
            SymbolTagType first;
            SymbolTagType second;
            return _config.TryGetSymbolTags(symbolId, out first, out second)
                && (first == tag || second == tag);
        }

        /// <summary>두 태그를 모두 가진 문양. 없으면 빈 글이다.</summary>
        private string FindSymbolWithTags(SymbolTagType a, SymbolTagType b)
        {
            if (a == b)
            {
                return string.Empty;
            }

            for (int i = 0; i < _config.Symbols.Count; i++)
            {
                SymbolTagType first = _config.Symbols[i].FirstTag;
                SymbolTagType second = _config.Symbols[i].SecondTag;

                if ((first == a && second == b) || (first == b && second == a))
                {
                    return _config.Symbols[i].Id;
                }
            }

            return string.Empty;
        }

        private string PickSymbolWithTag(SymbolTagType tag, EventRandom random)
        {
            return PickSymbol(random, tag, true);
        }

        private string PickSymbolWithoutTag(SymbolTagType tag, EventRandom random)
        {
            return PickSymbol(random, tag, false);
        }

        private string PickSymbol(EventRandom random, SymbolTagType tag, bool withTag)
        {
            List<string> pool = new List<string>();

            for (int i = 0; i < _config.Symbols.Count; i++)
            {
                bool has = _config.Symbols[i].FirstTag == tag || _config.Symbols[i].SecondTag == tag;
                if (has == withTag)
                {
                    pool.Add(_config.Symbols[i].Id);
                }
            }

            return pool.Count == 0 ? string.Empty : pool[random.Range(0, pool.Count)];
        }

        private static bool TryParseTag(string id, out SymbolTagType tag)
        {
            tag = SymbolTagType.Mercury;
            return !string.IsNullOrEmpty(id) && Enum.TryParse(id, out tag)
                && Enum.IsDefined(typeof(SymbolTagType), tag);
        }

        /// <summary>현재 빌드 화면과 같은 태그 이름. 설정이 없으면 영어 이름이다.</summary>
        private string GetTagName(SymbolTagType tag)
        {
            if (_tagNames == null)
            {
                return tag.ToString();
            }

            string name = _tagNames.GetTagVisual(tag).DisplayName;
            return string.IsNullOrEmpty(name) ? tag.ToString() : name;
        }

        // ---- 유물 ----

        /// <summary>
        /// 유물을 count 개 받는다.
        ///
        /// 지목된 것이 있으면 그것을 먼저 받고 모자란 만큼 무작위로 채운다.
        /// 12 잠든 보물 의 "둘 다 가진다" 가 두 개를 한꺼번에 받는다.
        /// 가질 수 있는 유물이 모자라면 있는 만큼만 받는다.
        /// </summary>
        private string GainRelics(string targetId, int count, List<string> decided, EventRandom random)
        {
            int wanted = count < 1 ? 1 : count;
            List<string> got = new List<string>();

            // 소지 한도는 흐름이 고르기 전에 버릴 것을 골라 맞춘다. 여기서도 한도를 넘기지 않게 한 번 더 막는다.
            int room = _context.Run.Owned.RelicRoom;
            if (room <= 0)
            {
                return "유물을 더 가질 자리가 없어 받지 못했다.";
            }

            wanted = wanted < room ? wanted : room;

            if (!string.IsNullOrEmpty(targetId) && _context.Run.Owned.AddRelic(targetId))
            {
                got.Add(targetId);
            }

            // 칸에 이름을 적어 둔 것을 먼저 받는다. 12 잠든 보물 의 둘 다 가진다, 19 두 개의 상자 가 그렇다.
            if (decided != null)
            {
                for (int i = 0; i < decided.Count && got.Count < wanted; i++)
                {
                    if (!string.IsNullOrEmpty(decided[i]) && _context.Run.Owned.AddRelic(decided[i]))
                    {
                        got.Add(decided[i]);
                    }
                }
            }

            while (got.Count < wanted)
            {
                List<EventItem> pool = new List<EventItem>();
                CollectItems(_config.Relics, pool, _context.Run.Owned.Relics);

                if (pool.Count == 0)
                {
                    break;
                }

                string id = pool[random.Range(0, pool.Count)].Id;
                _context.Run.Owned.AddRelic(id);
                got.Add(id);
            }

            if (got.Count == 0)
            {
                return "더 가질 유물이 없었다.";
            }

            string names = string.Empty;
            for (int i = 0; i < got.Count; i++)
            {
                if (i > 0)
                {
                    names += " 와 ";
                }

                names += _config.GetDisplayName(got[i]);
            }

            return Gain(names) + " 을 얻었다.";
        }

        /// <summary>아직 가지지 않아 받을 수 있는 유물 수.</summary>
        private int CountGainableRelics()
        {
            List<EventItem> pool = new List<EventItem>();
            CollectItems(_config.Relics, pool, _context.Run.Owned.Relics);
            return pool.Count;
        }

        /// <summary>
        /// 보유 유물 하나를 내주고 다른 유물 하나를 받는다. 13 유물 교환.
        /// 칸에 이름을 적어 둔 것이 있으면 그대로 바꾼다. 받을 유물이 없으면 내주지도 않는다.
        /// </summary>
        private string ChangeRelic(EventAction action, string decidedFrom, string decidedTo, EventRandom random)
        {
            List<OwnedRelic> relics = _context.Run.Owned.Relics;

            if (relics.Count == 0)
            {
                return "바꿀 유물이 없었다.";
            }

            string from = !string.IsNullOrEmpty(decidedFrom) && HasRelic(relics, decidedFrom)
                ? decidedFrom
                : PickTradableRelic(action, random);

            if (string.IsNullOrEmpty(from))
            {
                return "바꿔 받을 유물이 없었다.";
            }

            string to = !string.IsNullOrEmpty(decidedTo) && !HasRelic(relics, decidedTo)
                ? decidedTo
                : PickTradeRelic(action, RarityOf(from), random);

            if (string.IsNullOrEmpty(to))
            {
                return "바꿔 받을 유물이 없었다.";
            }

            _context.Run.Owned.RemoveRelic(from);
            _context.Run.Owned.AddRelic(to);

            return Cost(_config.GetDisplayName(from)) + " 을 내주고 " + Gain(_config.GetDisplayName(to)) + " 을 받았다.";
        }

        /// <summary>
        /// 내줄 수 있는 보유 유물 하나. 바꿔 받을 유물이 있는 것만 든다. random 이 null 이면 있는지만 본다.
        /// </summary>
        private string PickTradableRelic(EventAction action, EventRandom random)
        {
            List<string> tradable = new List<string>();
            List<OwnedRelic> relics = _context.Run.Owned.Relics;

            for (int i = 0; i < relics.Count; i++)
            {
                if (HasTradeTarget(action, RarityOf(relics[i].RelicId)))
                {
                    tradable.Add(relics[i].RelicId);
                }
            }

            if (tradable.Count == 0)
            {
                return string.Empty;
            }

            return random == null ? tradable[0] : tradable[random.Range(0, tradable.Count)];
        }

        /// <summary>그 등급의 유물을 내주면 받을 유물이 있는지.</summary>
        private bool HasTradeTarget(EventAction action, int fromRarity)
        {
            bool stepped = action != null && action.RarityStepWeights != null && action.RarityStepWeights.Count > 0;

            for (int i = 0; i < _config.Relics.Count; i++)
            {
                if (HasRelic(_context.Run.Owned.Relics, _config.Relics[i].Id) || !EventMayGive(_config.Relics[i]))
                {
                    continue;
                }

                if (!stepped || (int)_config.Relics[i].Rarity >= fromRarity)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 내준 유물 대신 받을 유물. 13 유물 교환 의 등급 규칙을 따른다.
        ///
        /// 비중(`EventAction.RarityStepWeights`)대로 몇 단계 위인지 굴린다. 50 · 40 · 10 이면
        /// 같은 등급 50%, 한 단계 위 40%, 두 단계 위 10% 다. 가장 높은 등급 위로는 가지 않는다.
        /// 그 등급에 가질 수 있는 유물이 없으면 한 단계씩 내려오되 **내준 유물보다 낮은 등급은 주지 않는다.**
        /// 예전에는 남은 유물에서 아무거나 뽑아 희귀 유물을 내주고 일반 유물을 받을 수 있었다.
        /// 2026년 10월 8일 외부 검토가 짚어 고쳤다.
        ///
        /// 비중이 비어 있으면 등급을 따지지 않고 가지지 않은 유물에서 고르게 뽑는다.
        /// </summary>
        private string PickTradeRelic(EventAction action, int fromRarity, EventRandom random)
        {
            List<EventItem> pool = new List<EventItem>();
            CollectItems(_config.Relics, pool, _context.Run.Owned.Relics);

            if (pool.Count == 0)
            {
                return string.Empty;
            }

            List<int> weights = action != null ? action.RarityStepWeights : null;
            if (weights == null || weights.Count == 0)
            {
                return pool[random.Range(0, pool.Count)].Id;
            }

            int total = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                total += weights[i] > 0 ? weights[i] : 0;
            }

            int step = 0;
            if (total > 0)
            {
                int roll = random.Range(0, total);
                for (int i = 0; i < weights.Count; i++)
                {
                    if (weights[i] <= 0)
                    {
                        continue;
                    }

                    roll -= weights[i];
                    if (roll < 0)
                    {
                        step = i;
                        break;
                    }
                }
            }

            int highest = -1;
            for (int i = 0; i < _config.Relics.Count; i++)
            {
                int rarity = (int)_config.Relics[i].Rarity;
                highest = rarity > highest ? rarity : highest;
            }

            int from = fromRarity < 0 ? 0 : fromRarity;
            int wanted = from + step > highest ? highest : from + step;

            for (int rarity = wanted; rarity >= from; rarity--)
            {
                List<EventItem> tier = new List<EventItem>();
                for (int i = 0; i < pool.Count; i++)
                {
                    if (RarityOf(pool[i].Id) == rarity)
                    {
                        tier.Add(pool[i]);
                    }
                }

                if (tier.Count > 0)
                {
                    return tier[random.Range(0, tier.Count)].Id;
                }
            }

            // 위 등급이 비어 있으면 그보다 더 위에서라도 준다. 내준 것보다 낮아지지만 않으면 된다.
            for (int rarity = wanted + 1; rarity <= highest; rarity++)
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    if (RarityOf(pool[i].Id) == rarity)
                    {
                        return pool[i].Id;
                    }
                }
            }

            return string.Empty;
        }

        // ---- 코인 ----

        private string GainCoin(string targetId, EventRandom random)
        {
            // 소지 한도는 흐름이 고르기 전에 버릴 것을 골라 맞춘다. 여기서도 한도를 넘기지 않게 한 번 더 막는다.
            if (_context.Run.Owned.CoinRoom <= 0)
            {
                return "코인을 더 가질 자리가 없어 받지 못했다.";
            }

            string id = !string.IsNullOrEmpty(targetId) ? targetId : PickCoinOfRarity(-1, random);

            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            _context.Run.Owned.AddCoin(id);
            return Gain(_config.GetDisplayName(id)) + " 을 얻었다.";
        }

        private string LoseCoin(string targetId, EventRandom random)
        {
            string id = !string.IsNullOrEmpty(targetId) ? targetId : PickOwnedCoinId(random, -1);

            if (string.IsNullOrEmpty(id) || !_context.Run.Owned.RemoveCoin(id))
            {
                return "잃을 코인이 없었다.";
            }

            return Cost(_config.GetDisplayName(id)) + " 을 잃었다.";
        }

        /// <summary>코인을 같은 등급이나 한 단계 다른 등급의 다른 코인으로 바꾼다.</summary>
        private string ChangeCoin(string targetId, EventRandom random, int rarityStep, string decidedTo, int minRarity)
        {
            string from = !string.IsNullOrEmpty(targetId) ? targetId : PickOwnedCoinId(random, minRarity);

            if (string.IsNullOrEmpty(from))
            {
                return "바꿀 코인이 없었다.";
            }

            int fromRarity = (int)GetRarity(_config.Coins, from);

            // 칸에 이름을 적어 둔 것이 있으면 그것을 받는다. 20 환전 이 그렇다.
            string to = !string.IsNullOrEmpty(decidedTo)
                ? decidedTo
                : PickCoinStepped(fromRarity, rarityStep, random);

            if (string.IsNullOrEmpty(to) || !_context.Run.Owned.RemoveCoin(from))
            {
                return "바꿀 수 없었다.";
            }

            _context.Run.Owned.AddCoin(to);

            // 등급이 내려간 코인은 얻은 것이 아니라 잃은 것이다. 25 미치광이 대장장이 가 그렇다.
            string toName = rarityStep < 0
                ? Cost(_config.GetDisplayName(to))
                : Gain(_config.GetDisplayName(to));

            return Cost(_config.GetDisplayName(from)) + " 이 " + toName + " 이 되었다.";
        }

        /// <summary>코인을 팔고 등급에 따른 골드를 받는다.</summary>
        private string SellCoin(string targetId, EventRandom random)
        {
            string id = !string.IsNullOrEmpty(targetId) ? targetId : PickOwnedCoinId(random, -1);

            if (string.IsNullOrEmpty(id) || !_context.Run.Owned.RemoveCoin(id))
            {
                return "팔 코인이 없었다.";
            }

            // 등급마다 받는 골드는 물건 목록 설정에 있다(RunCatalogConfig.CoinSellPrices).
            int gold = _config.GetCoinSellPrice(GetRarity(_config.Coins, id));

            _context.GainGold(gold);
            return Cost(_config.GetDisplayName(id)) + " 을 팔아 " + Gain("골드 " + gold) + " 을 받았다.";
        }

        /// <summary>
        /// 보유 코인 하나. minRarity 가 0 이상이면 그 등급 이상만 든다(25 미치광이 대장장이 의 "고급 이상").
        /// random 이 null 이면 있는지만 보고 처음 것을 돌려준다. 없으면 빈 글이다.
        /// </summary>
        private string PickOwnedCoinId(EventRandom random, int minRarity)
        {
            List<string> coins = new List<string>();
            List<string> owned = _context.Run.Owned.CoinIds;

            for (int i = 0; i < owned.Count; i++)
            {
                if (minRarity < 0 || (int)GetRarity(_config.Coins, owned[i]) >= minRarity)
                {
                    coins.Add(owned[i]);
                }
            }

            if (coins.Count == 0)
            {
                return string.Empty;
            }

            return random == null ? coins[0] : coins[random.Range(0, coins.Count)];
        }

        /// <summary>
        /// 등급을 step 만큼 옮긴 코인 하나. 25 미치광이 대장장이 가 내리고 27 축복 이 올린다.
        ///
        /// **등급은 가장 낮은 등급 아래로, 가장 높은 등급 위로 가지 않는다.**
        /// 이미 가장 높은 코인을 올리면 같은 등급에서 뽑는다. 2026년 10월 8일 원재가 정했다.
        /// 예전에는 그 위 등급에 코인이 없어 아무 코인이나 뽑아, 전설 코인이 축복을 받고 일반 코인이 될 수 있었다.
        ///
        /// 가장 낮고 높은 등급은 목록에 실제로 있는 코인으로 정한다.
        /// 옮긴 등급에 코인이 비어 있으면 원래 등급 쪽으로 한 칸씩 물러난다.
        /// 그래서 올리기가 원래보다 낮은 코인을, 내리기가 원래보다 높은 코인을 주는 일이 없다.
        /// </summary>
        private string PickCoinStepped(int fromRarity, int step, EventRandom random)
        {
            int lowest = int.MaxValue;
            int highest = int.MinValue;

            for (int i = 0; i < _config.Coins.Count; i++)
            {
                int rarity = (int)_config.Coins[i].Rarity;
                lowest = rarity < lowest ? rarity : lowest;
                highest = rarity > highest ? rarity : highest;
            }

            if (lowest > highest)
            {
                return string.Empty;
            }

            int wanted = fromRarity + step;
            wanted = wanted < lowest ? lowest : (wanted > highest ? highest : wanted);

            // 원래 등급을 넘어 반대쪽으로는 가지 않는다. 원래 등급이 범위 밖이면 범위 끝에서 멈춘다.
            int stop = fromRarity < lowest ? lowest : (fromRarity > highest ? highest : fromRarity);
            int back = wanted > stop ? -1 : 1;

            for (int rarity = wanted; ; rarity += back)
            {
                List<string> pool = CoinsOfRarity(rarity);
                if (pool.Count > 0)
                {
                    return pool[random.Range(0, pool.Count)];
                }

                if (rarity == stop)
                {
                    break;
                }
            }

            // 원래 등급까지 모두 비어 있다. 이벤트가 멈추지 않게 아무 코인이나 내준다.
            return PickCoinOfRarity(-1, random);
        }

        /// <summary>그 등급의 코인 식별자.</summary>
        private List<string> CoinsOfRarity(int rarity)
        {
            List<string> pool = new List<string>();

            for (int i = 0; i < _config.Coins.Count; i++)
            {
                if ((int)_config.Coins[i].Rarity == rarity && EventMayGive(_config.Coins[i]))
                {
                    pool.Add(_config.Coins[i].Id);
                }
            }

            return pool;
        }

        /// <summary>그 등급의 코인 하나. 등급이 음수면 아무 코인이나 뽑는다.</summary>
        private string PickCoinOfRarity(int rarity, EventRandom random)
        {
            List<string> pool = new List<string>();

            for (int i = 0; i < _config.Coins.Count; i++)
            {
                if ((rarity < 0 || (int)_config.Coins[i].Rarity == rarity) && EventMayGive(_config.Coins[i]))
                {
                    pool.Add(_config.Coins[i].Id);
                }
            }

            // 그 등급에 코인이 없으면 아무 코인이나 내준다. 이벤트로 나오지 않는 코인은 여기서도 뺀다.
            // 등급별 코인이 다 채워지기 전에도 이벤트가 멈추지 않게 하려는 것이다.
            if (pool.Count == 0)
            {
                for (int i = 0; i < _config.Coins.Count; i++)
                {
                    if (EventMayGive(_config.Coins[i]))
                    {
                        pool.Add(_config.Coins[i].Id);
                    }
                }
            }

            return pool.Count == 0 ? string.Empty : pool[random.Range(0, pool.Count)];
        }

        private void CollectOwnedCoins(List<EventItem> into)
        {
            List<string> coins = _context.Run.Owned.CoinIds;

            for (int i = 0; i < coins.Count; i++)
            {
                into.Add(new EventItem(coins[i], _config.GetDisplayName(coins[i])));
            }
        }

        // ---- 공통 ----

        /// <summary>
        /// 결과 문장에서 얻은 것. 화면이 초록으로 칠한다.
        /// 2026년 10월 5일 원재가 대가와 거래 이벤트에서 잃은 것은 빨강, 얻은 것은 초록으로 보이게 하라고 했다.
        /// </summary>
        private static string Gain(string text)
        {
            return EventChoiceText.MarkGain(text);
        }

        /// <summary>결과 문장에서 잃거나 치른 것. 화면이 빨강으로 칠한다.</summary>
        private static string Cost(string text)
        {
            return EventChoiceText.MarkCost(text);
        }

        /// <summary>목록을 모은다. 이미 가진 유물과 이벤트로는 나오지 않는 것은 뺀다.</summary>
        private void CollectItems(
            List<ItemDefinition> source, List<EventItem> into, List<OwnedRelic> exclude)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if ((exclude != null && HasRelic(exclude, source[i].Id)) || !EventMayGive(source[i]))
                {
                    continue;
                }

                into.Add(new EventItem(source[i].Id, source[i].DisplayName));
            }
        }

        private static bool HasRelic(List<OwnedRelic> relics, string id)
        {
            for (int i = 0; i < relics.Count; i++)
            {
                if (relics[i].RelicId == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static ItemRarity GetRarity(List<ItemDefinition> source, string id)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].Id == id)
                {
                    return source[i].Rarity;
                }
            }

            return ItemRarity.Common;
        }
    }
}
