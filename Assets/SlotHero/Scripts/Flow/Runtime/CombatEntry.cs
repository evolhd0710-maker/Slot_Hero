using System;
using UnityEngine;
using SlotHero.Combat;
using SlotHero.Combat.UI;
using SlotHero.Map;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 전투 창구(`ICombatEntry`)의 진짜 구현. 흐름과 전투 화면을 잇는다.
    ///
    /// 런 데이터에서 덱과 코인과 유물과 체력을 꺼내 `CombatSetup` 으로 바꿔 전투 화면에 넘기고,
    /// 전투가 깎은 체력은 바로 런에 적고, 끝난 결과는 `CombatResult` 로 바꿔 흐름에 돌려준다.
    /// 전투 화면은 런 데이터와 물건 목록을 모른다. 영역끼리 서로를 모르게 하려는 것이다.
    ///
    /// 2026년 10월 25일 전투 대역(`PlaceholderCombatEntry`)을 떼고 이것을 붙였다.
    /// 대역은 흐름 검사(`CheckGameScene`)가 승패를 바로 고르는 데만 쓴다.
    /// </summary>
    public class CombatEntry : MonoBehaviour, ICombatEntry, ICombatIconSource
    {
        [Header("붙일 것")]
        [SerializeField] private CombatScreenController _screen;
        [SerializeField] private CombatRuleConfig _rules;

        [Tooltip("문양의 이름과 태그와 가치, 코인 이름, 그림을 가져오는 곳.")]
        [SerializeField] private RunCatalogConfig _catalog;

        [Tooltip("유물 효과. 유물 정보시트를 옮긴 것이다.")]
        [SerializeField] private RelicConfig _relics;

        private RunContext _context;
        private Action<CombatResult> _onFinished;

        /// <summary>전투 화면. 검사가 직접 누르는 데 쓴다.</summary>
        public CombatScreenController Screen
        {
            get { return _screen; }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <inheritdoc />
        public void Enter(RunContext context, MapNode node, CombatOptions options, Action<CombatResult> onFinished)
        {
            _context = context;
            _onFinished = onFinished;

            if (_screen == null)
            {
                // 화면이 없으면 싸울 수 없다. 흐름이 멈추지 않게 이긴 것으로 넘긴다.
                Debug.LogWarning("전투 화면이 없어 이긴 것으로 넘긴다.", this);
                Finish(CombatResult.Victory(0, 0));
                return;
            }

            // 이벤트가 건 부정 효과(D1 ~ D6)는 아직 걸지 않는다. 얼마나 세게 걸지 정해지지 않았다.
            if (options.HasPenalty)
            {
                Debug.Log("전투에 걸린 부정 효과 " + options.PenaltyId + " 는 아직 적용하지 않는다.", this);
            }

            Unsubscribe();
            _screen.PlayerHealthLost += HandleHealthLost;
            _screen.Finished += HandleFinished;
            _screen.RelicStackChanged += HandleRelicStackChanged;
            _screen.Open(MakeSetup(context, node), this);
        }

        /// <inheritdoc />
        public void Leave()
        {
            Unsubscribe();

            if (_screen != null)
            {
                _screen.Close();
            }

            _context = null;
            _onFinished = null;
        }

        /// <inheritdoc />
        public void ResetForNewRun()
        {
            Leave();

            if (_screen != null)
            {
                _screen.ResetForNewRun();
            }
        }

        /// <inheritdoc />
        public Sprite GetIcon(string id)
        {
            return _catalog != null ? _catalog.GetIcon(id) : null;
        }

        /// <summary>
        /// 런 데이터에서 전투 준비물을 만든다.
        /// 시드는 런 시드, 스테이지, 방 번호를 섞어 같은 방이면 늘 같은 릴이 나오게 한다.
        /// </summary>
        public CombatSetup MakeSetup(RunContext context, MapNode node)
        {
            CombatSetup setup = new CombatSetup();
            RunSaveData run = context != null ? context.Run : null;

            if (run == null)
            {
                setup.Enemy = _rules != null ? _rules.MakeEnemy(CombatEnemyKind.Normal, 1) : null;
                return setup;
            }

            for (int i = 0; i < run.Owned.Symbols.Count; i++)
            {
                OwnedSymbol owned = run.Owned.Symbols[i];
                ReelSymbol symbol = MakeSymbol(owned.SymbolId);

                for (int n = 0; n < owned.Count; n++)
                {
                    setup.Deck.Add(symbol);
                }
            }

            for (int i = 0; i < run.Owned.CoinIds.Count; i++)
            {
                setup.Coins.Add(MakeCoin(run.Owned.CoinIds[i]));
            }

            for (int i = 0; i < run.Owned.Relics.Count; i++)
            {
                OwnedRelic owned = run.Owned.Relics[i];
                RelicDefinition definition = _relics != null ? _relics.Find(owned.RelicId) : null;
                string name = _catalog != null ? _catalog.GetDisplayName(owned.RelicId) : owned.RelicId;
                CombatRelic relic = new CombatRelic(owned.RelicId, name, definition, owned.Stack);

                // 전투 화면 유물 칸 색은 물건 목록의 등급을 따른다.
                ItemDefinition item;
                if (_catalog != null && _catalog.TryGetItem(owned.RelicId, out item))
                {
                    relic.Rarity = (int)item.Rarity;
                }

                setup.Relics.Add(relic);
            }

            // 망치와 끌이 "그 태그의 랜덤 문양" 을 고르는 풀. 게임에 있는 모든 문양이다.
            if (_catalog != null)
            {
                for (int i = 0; i < _catalog.Symbols.Count; i++)
                {
                    setup.SymbolPool.Add(MakeSymbol(_catalog.Symbols[i].Id));
                }
            }

            setup.RelicCapacity = run.Owned.RelicCapacity >= 0 ? run.Owned.RelicCapacity : 6;
            setup.PlayerHealth = context.Health;
            setup.PlayerMaxHealth = context.MaxHealth;

            int stage = run.StageIndex;
            setup.Enemy = _rules != null
                ? _rules.MakeEnemy(ToEnemyKind(node), stage)
                : new CombatEnemy("적", string.Empty, 30, 5);
            setup.Seed = CombatRandom.MixSeed(run.Seed, stage, node != null ? node.Id : 0);

            return setup;
        }

        /// <summary>방 종류에 맞는 적 종류. 이벤트가 연 전투처럼 전투 방이 아니면 일반이다.</summary>
        public static CombatEnemyKind ToEnemyKind(MapNode node)
        {
            if (node == null)
            {
                return CombatEnemyKind.Normal;
            }

            switch (node.RoomType)
            {
                case RoomType.Elite:
                    return CombatEnemyKind.Elite;
                case RoomType.Boss:
                    return CombatEnemyKind.Boss;
                default:
                    return CombatEnemyKind.Normal;
            }
        }

        private ReelSymbol MakeSymbol(string id)
        {
            SymbolTagType first = SymbolTagType.Mercury;
            SymbolTagType second = SymbolTagType.Mercury;
            int value = 0;
            string name = id;

            if (_catalog != null && _catalog.TryGetSymbolTags(id, out first, out second))
            {
                value = _catalog.GetTagValue(first) + _catalog.GetTagValue(second);
                name = _catalog.GetDisplayName(id);
            }

            return new ReelSymbol(id, name, first, second, value);
        }

        private CombatCoin MakeCoin(string id)
        {
            CoinRule rule = _rules != null
                ? _rules.GetCoinRule(id)
                : new CoinRule(id, 10, 5, string.Empty);
            string name = _catalog != null ? _catalog.GetDisplayName(id) : id;

            return new CombatCoin(id, name, rule.HealthCost, rule.Weight, rule.Description);
        }

        /// <summary>12개의 금화처럼 영구히 오르는 유물. 쌓인 횟수를 런에 적어 다음 전투로 이어지게 한다.</summary>
        private void HandleRelicStackChanged(CombatRelic relic)
        {
            if (_context != null && _context.Run != null && relic != null)
            {
                _context.Run.Owned.SetRelicStack(relic.Id, relic.Stack);
            }
        }

        private void HandleHealthLost(int amount)
        {
            if (_context != null)
            {
                _context.TakeDamage(amount);
            }
        }

        private void HandleFinished(CombatOutcome outcome)
        {
            Unsubscribe();

            CombatResult result = outcome.Won
                ? CombatResult.Victory(
                    outcome.BiggestHit, 0,
                    _rules != null ? _rules.GetOverkillGold(outcome.Overkill) : 0,
                    _rules != null ? _rules.GetOverkillHeal(outcome.Overkill) : 0)
                : CombatResult.Defeat();

            // 끝나면 화면부터 닫는다. 이벤트가 연 전투는 흐름이 `Leave` 를 부르지 않고 이벤트 화면으로 돌아가는데,
            // 이벤트 화면은 바탕이 비쳐 전투 화면이 뒤에 남아 보인다.
            if (_screen != null)
            {
                _screen.Close();
            }

            // 진 결과는 체력이 이미 0 이다. 흐름이 런을 끝낸다.
            Finish(result);
        }

        private void Finish(CombatResult result)
        {
            Action<CombatResult> callback = _onFinished;
            _onFinished = null;

            if (callback != null)
            {
                callback(result);
            }
        }

        private void Unsubscribe()
        {
            if (_screen != null)
            {
                _screen.PlayerHealthLost -= HandleHealthLost;
                _screen.Finished -= HandleFinished;
                _screen.RelicStackChanged -= HandleRelicStackChanged;
            }
        }
    }
}
