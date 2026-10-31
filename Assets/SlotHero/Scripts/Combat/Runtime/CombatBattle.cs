using System;
using System.Collections.Generic;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투 하나의 규칙을 굴린다. 화면을 모르는 일반 클래스라 유니티 없이 돌려 볼 수 있다.
    ///
    /// 슬롯머신 작동 방식 1.1.a 의 "랜덤 → 행동" 차례다.
    ///   1. 처음에 릴을 모두 돌린다
    ///   2. 가진 코인 중 셋을 꺼내 보인다. 매 턴 따로 뽑고, 한 턴 안에서는 겹치지 않는다
    ///   3. 코인 하나를 골라 태그 하나에 건다. 코인 소모값만큼 체력을 낸다
    ///   4. 그 태그가 붙은 문양만으로 공격력을 내고 한 번 때린다
    ///   5. 적이 살아 있으면 적이 때린다
    ///   6. 계산에 쓴 문양만 다시 돌린다. 나머지는 그대로 남는다. 2로 돌아간다
    ///
    /// 유물은 `유물 정보시트.xlsx` 의 효과를 이 차례 사이사이에 건다(`RelicConfig`).
    ///   - 태그 가치(제국 군기, 일식)는 문양 가치에 들어간다
    ///   - 코인 소모값 · 가치(흑요석 단검, 12개의 금화)는 코인을 쓸 때 들어간다
    ///   - 문양 재사용(별의 저편, 속임수 주사위)은 그 문양의 가치를 한 번 더 더한다
    ///   - 체력 비율(곰 가죽 망토)은 마지막 공격력에 곱한다
    ///   - 릴이 멈출 때(처음 회전과 재굴림 뒤) 망치와 끌과 12개의 금화가 발동한다
    ///   - 적이 때리기 전에 철제 방패가 피해를 줄인다
    ///   - 사건의 지평선이 있으면 소용돌이 칸이 가장 많은 태그로 공격한다
    ///
    /// 적의 패턴(검투사, 장군, 방패병)은 `CombatBattle.Monsters.cs` 에 있다.
    ///
    /// 연출은 `CombatScreenController` 가 맡는다. 여기서는 값만 바꾼다.
    /// </summary>
    public partial class CombatBattle
    {
        private readonly CombatRuleConfig _rules;
        private readonly CombatRandom _random;

        /// 적 패턴 전용 난수. 릴과 따로 두어 적이 행동해도 릴 결과가 바뀌지 않게 한다.
        private readonly CombatRandom _monsterRandom;

        /// 자리 차례로 놓인 적. 살아 있는 적만 남는다.
        private readonly List<CombatEnemy> _enemies = new List<CombatEnemy>();
        private readonly List<EnemyAction> _enemyActions = new List<EnemyAction>();
        private readonly List<ReelSymbol> _deck;
        private readonly List<CombatCoin> _coins;
        private readonly List<ReelSymbol> _pool;
        private readonly List<CombatRelic> _relics;
        private readonly List<string> _notices = new List<string>();

        private readonly List<ReelSymbol> _reels = new List<ReelSymbol>();
        private readonly List<int> _offered = new List<int>();
        private readonly List<int> _rerollTargets = new List<int>();

        private int _selectedOffer = -1;
        private bool _withoutCoin;

        /// <summary>규칙과 전투 하나의 준비물로 만든다. 아직 릴을 돌리지 않는다. `Begin` 을 부른다.</summary>
        public CombatBattle(CombatSetup setup, CombatRuleConfig rules)
        {
            if (setup == null)
            {
                throw new ArgumentNullException("setup");
            }

            _rules = rules;
            _random = new CombatRandom(setup.Seed);
            _deck = new List<ReelSymbol>(setup.Deck);
            _coins = new List<CombatCoin>(setup.Coins);
            _pool = new List<ReelSymbol>(setup.SymbolPool);
            _relics = new List<CombatRelic>(setup.Relics);

            _monsterRandom = new CombatRandom(setup.Seed ^ 0x5A17E5);

            Enemy = setup.Enemy ?? new CombatEnemy("적", string.Empty, 30, 5);
            Enemy.Slot = GeneralRules.MaxShieldmen;
            _enemies.Add(Enemy);

            PlayerMaxHealth = setup.PlayerMaxHealth;
            PlayerHealth = setup.PlayerHealth;
            Phase = CombatPhase.None;
        }

        /// <summary>유물 고유 수치가 올랐다. 흐름이 쌓인 횟수를 런에 적는다.</summary>
        public event Action<CombatRelic> RelicStackChanged;

        /// <summary>지금 어디까지 왔는지.</summary>
        public CombatPhase Phase { get; private set; }

        /// <summary>들고 들어간 유물.</summary>
        public IReadOnlyList<CombatRelic> Relics
        {
            get { return _relics; }
        }

        /// <summary>
        /// 마지막 차례에 발동한 유물 소식. 화면이 안내 글로 보인다.
        /// 릴이 멈출 때와 적이 때릴 때 새로 채운다.
        /// </summary>
        public IReadOnlyList<string> Notices
        {
            get { return _notices; }
        }

        /// <summary>본체. 이 적이 쓰러지면 이긴다. 소환수는 함께 사라진다.</summary>
        public CombatEnemy Enemy { get; private set; }

        /// <summary>살아 있는 적. 왼쪽 자리부터다. 이 차례로 행동한다.</summary>
        public IReadOnlyList<CombatEnemy> Enemies
        {
            get { return _enemies; }
        }

        /// <summary>영웅의 공격을 받을 적. 가장 왼쪽에 살아 있는 적이다. 방패병이 있으면 방패병이다.</summary>
        public CombatEnemy Target
        {
            get
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    if (!_enemies[i].IsDead)
                    {
                        return _enemies[i];
                    }
                }

                return Enemy;
            }
        }

        /// <summary>마지막 적 차례에 적들이 한 행동. 행동한 차례대로다.</summary>
        public IReadOnlyList<EnemyAction> EnemyActions
        {
            get { return _enemyActions; }
        }

        public int PlayerHealth { get; private set; }
        public int PlayerMaxHealth { get; private set; }

        /// <summary>공격한 횟수.</summary>
        public int Turns { get; private set; }

        /// <summary>이 전투에서 준 가장 큰 피해.</summary>
        public int BiggestHit { get; private set; }

        /// <summary>마지막 공격.</summary>
        public AttackResult LastAttack { get; private set; }

        /// <summary>지금 릴에 놓인 문양. 왼쪽부터다.</summary>
        public IReadOnlyList<ReelSymbol> Reels
        {
            get { return _reels; }
        }

        /// <summary>덱. 회전 연출이 그림을 바꿔 보이는 데 쓴다.</summary>
        public IReadOnlyList<ReelSymbol> Deck
        {
            get { return _deck; }
        }

        /// <summary>이번 턴에 꺼낸 코인 수. 가진 코인이 적으면 셋보다 적다.</summary>
        public int OfferedCount
        {
            get { return _offered.Count; }
        }

        /// <summary>다음에 다시 돌 릴 자리. 마지막 공격에 쓴 자리다.</summary>
        public IReadOnlyList<int> RerollTargets
        {
            get { return _rerollTargets; }
        }

        /// <summary>코인을 골랐는지. 코인 없이 고른 것도 고른 것으로 친다.</summary>
        public bool HasChosenCoin
        {
            get { return _selectedOffer >= 0 || _withoutCoin; }
        }

        /// <summary>고른 코인. 코인 없이 골랐거나 아직 고르지 않았으면 false 다.</summary>
        public bool TryGetSelectedCoin(out CombatCoin coin)
        {
            if (_selectedOffer >= 0 && _selectedOffer < _offered.Count)
            {
                coin = _coins[_offered[_selectedOffer]];
                return true;
            }

            coin = new CombatCoin();
            return false;
        }

        /// <summary>그 자리에 꺼낸 코인. 자리가 비었으면 false 다.</summary>
        public bool TryGetOffered(int offerIndex, out CombatCoin coin)
        {
            if (offerIndex >= 0 && offerIndex < _offered.Count)
            {
                coin = _coins[_offered[offerIndex]];
                return true;
            }

            coin = new CombatCoin();
            return false;
        }

        /// <summary>전투를 시작한다. 릴을 모두 돌리고 코인을 꺼낸다.</summary>
        public void Begin()
        {
            if (Phase != CombatPhase.None)
            {
                return;
            }

            SpinAll();
            OnReelsStopped();
            OfferCoins();
            StartEnemyTurns();
            Phase = CombatPhase.ChoosingCoin;
        }

        /// <summary>
        /// 그 코인을 지금 쓸 수 있는지.
        /// 소모값을 내고도 `MinimumHealthAfterCoin` 만큼 남아야 한다.
        /// </summary>
        public bool CanUse(CombatCoin coin)
        {
            int keep = _rules != null ? _rules.MinimumHealthAfterCoin : 1;
            return PlayerHealth - GetCoinCost(coin) >= keep;
        }

        /// <summary>그 코인을 쓸 때 내는 체력. 유물이 더한 소모값까지 들어 있다(흑요석 단검).</summary>
        public int GetCoinCost(CombatCoin coin)
        {
            int cost = coin.HealthCost;
            ForEachEffect(RelicEffectKind.CoinCostAdd, (relic, effect) => cost += effect.Amount);
            return cost < 0 ? 0 : cost;
        }

        /// <summary>
        /// 그 코인이 배수에 더하는 값. 유물이 더한 코인 가치까지 들어 있다(흑요석 단검, 12개의 금화).
        /// 음수가 될 수 있다. 12개의 금화는 -5 에서 시작한다.
        /// </summary>
        public int GetCoinWeight(CombatCoin coin)
        {
            int weight = coin.Weight;
            ForEachEffect(RelicEffectKind.CoinWeightAdd, (relic, effect) => weight += effect.Amount);
            ForEachEffect(RelicEffectKind.CoinWeightFromCounter, (relic, effect) => weight += relic.Counter);
            return weight;
        }

        /// <summary>그 태그 가치에 유물이 더한 값(제국 군기, 일식).</summary>
        public int GetTagValueBonus(SymbolTagType tag)
        {
            int bonus = 0;
            ForEachEffect(RelicEffectKind.TagValueAdd, (relic, effect) =>
            {
                if (effect.Targets(tag))
                {
                    bonus += effect.Amount;
                }
            });
            return bonus;
        }

        /// <summary>문양 하나의 지금 가치. 태그 둘에 유물이 더한 값까지 들어 있다.</summary>
        public int GetSymbolValue(ReelSymbol symbol)
        {
            return symbol.Value + GetTagValueBonus(symbol.FirstTag) + GetTagValueBonus(symbol.SecondTag);
        }

        /// <summary>그 자리의 코인을 지금 쓸 수 있는지.</summary>
        public bool CanUseOffered(int offerIndex)
        {
            CombatCoin coin;
            return TryGetOffered(offerIndex, out coin) && CanUse(coin);
        }

        /// <summary>코인을 고른다. 태그 선택으로 넘어간다. 쓸 수 없는 코인이면 false 다.</summary>
        public bool SelectCoin(int offerIndex)
        {
            if (Phase != CombatPhase.ChoosingCoin || !CanUseOffered(offerIndex))
            {
                return false;
            }

            _selectedOffer = offerIndex;
            _withoutCoin = false;
            Phase = CombatPhase.ChoosingTag;
            return true;
        }

        /// <summary>
        /// 코인 없이 태그를 고르러 간다. 가중치 0, 소모 0 이다.
        ///
        /// 전투 UI 초안에는 이 길이 없다. 코인이 하나도 없거나 체력이 모자라 쓸 수 없으면
        /// 전투가 멈추므로 코인 UI 가운데 원에 두었다(2026년 10월 25일). 기획이 정해지면 바꾼다.
        /// </summary>
        public bool SelectWithoutCoin()
        {
            if (Phase != CombatPhase.ChoosingCoin)
            {
                return false;
            }

            _selectedOffer = -1;
            _withoutCoin = true;
            Phase = CombatPhase.ChoosingTag;
            return true;
        }

        /// <summary>코인을 다시 고르러 돌아간다. 태그 UI 의 가운데를 눌렀을 때다.</summary>
        public bool BackToCoins()
        {
            if (Phase != CombatPhase.ChoosingTag)
            {
                return false;
            }

            _selectedOffer = -1;
            _withoutCoin = false;
            Phase = CombatPhase.ChoosingCoin;
            return true;
        }

        /// <summary>그 태그가 붙은 릴 자리.</summary>
        public List<int> FindReels(SymbolTagType tag)
        {
            List<int> found = new List<int>();
            for (int i = 0; i < _reels.Count; i++)
            {
                if (_reels[i].Has(tag))
                {
                    found.Add(i);
                }
            }

            return found;
        }

        /// <summary>
        /// 가장 많은 태그. 같으면 행성 차례(수금지화목토천해명)로 앞선 것이다. 릴이 비었으면 false 다.
        /// 사건의 지평선과 망치와 끌의 "N번째로 많은 태그 선택" 이다.
        /// </summary>
        public bool TryGetTopTag(out SymbolTagType top, out int count)
        {
            top = SymbolTagType.Mercury;
            count = 0;

            for (int t = 0; t < 9; t++)
            {
                SymbolTagType tag = (SymbolTagType)t;
                int found = FindReels(tag).Count;
                if (found > count)
                {
                    top = tag;
                    count = found;
                }
            }

            return count > 0;
        }

        /// <summary>소용돌이 칸으로 때릴 수 있는지. 사건의 지평선이 있어야 한다.</summary>
        public bool CanUseVortex()
        {
            SymbolTagType top;
            int count;
            return Phase == CombatPhase.ChoosingTag
                && HasEffect(RelicEffectKind.VortexAttacksTopTag)
                && TryGetTopTag(out top, out count);
        }

        /// <summary>소용돌이 칸으로 때린다. 가장 많은 태그로 공격한다. 때릴 수 없으면 null 이다.</summary>
        public AttackResult AttackWithVortex()
        {
            SymbolTagType top;
            int count;
            if (!CanUseVortex() || !TryGetTopTag(out top, out count))
            {
                return null;
            }

            AttackResult result = Attack(top);
            if (result != null)
            {
                result.FromVortex = true;
            }

            return result;
        }

        /// <summary>그 태그로 때릴 수 있는지. 그 태그가 붙은 문양이 하나라도 있어야 한다.</summary>
        public bool CanAttackWith(SymbolTagType tag)
        {
            return Phase == CombatPhase.ChoosingTag && FindReels(tag).Count > 0;
        }

        /// <summary>
        /// 그 태그로 때리면 어떻게 되는지 값만 낸다. 아무것도 바꾸지 않는다.
        /// </summary>
        public AttackResult Calculate(SymbolTagType tag)
        {
            AttackResult result = new AttackResult();
            result.Tag = tag;
            result.ReelIndices = FindReels(tag);
            result.DuplicateCount = result.ReelIndices.Count;

            for (int i = 0; i < result.ReelIndices.Count; i++)
            {
                int index = result.ReelIndices[i];
                int value = GetSymbolValue(_reels[index]);
                int uses = 1 + CountReuses(index);

                result.ValueSum += value * uses;
                result.ReuseCount += uses - 1;
            }

            result.DuplicateMultiplier = _rules != null
                ? _rules.GetDuplicateMultiplier(result.DuplicateCount)
                : result.DuplicateCount;

            CombatCoin coin;
            if (TryGetSelectedCoin(out coin))
            {
                result.CoinId = coin.Id;
                result.CoinWeight = GetCoinWeight(coin);
                result.HealthPaid = GetCoinCost(coin);
            }

            // 문양이 하나도 없으면 배수도 0 이라 코인 가중치만으로 때리지 않는다.
            // 가치나 배수가 유물로 음수가 되어도 적을 낫게 하지는 않는다.
            int damage = result.DuplicateCount > 0 ? result.ValueSum * result.Multiplier : 0;

            // 곰 가죽 망토. 코인 소모값을 낸 뒤의 체력으로 본다. 그때가 공격하는 순간이다.
            float scale = 1f;
            int healthAtAttack = PlayerHealth - result.HealthPaid;
            ForEachEffect(RelicEffectKind.DamageMultiplierAtLowHealth, (relic, effect) =>
            {
                if (PlayerMaxHealth > 0 && healthAtAttack * 100 <= PlayerMaxHealth * effect.Threshold)
                {
                    scale *= effect.Multiplier;
                }
            });

            result.DamageScale = scale;
            result.Damage = damage > 0 ? (int)Math.Round(damage * scale, MidpointRounding.AwayFromZero) : 0;

            CombatEnemy target = Target;
            result.Target = target;

            // 장군의 방패병. 영웅 피해 계산이 모두 끝난 뒤에 본다.
            result.ShieldmanReduced = GetShieldmanReduction(target, result.Damage);
            result.Damage -= result.ShieldmanReduced;

            // 피해 계산 순서 ⑤ 방어도 → ⑥ 체력. 방어도가 먼저 막는다.
            result.Blocked = Math.Min(Math.Max(0, target.Block), result.Damage);
            result.EnemyHealthAfter = target.Health - (result.Damage - result.Blocked);
            return result;
        }

        /// <summary>
        /// 그 태그로 한 번 때린다. 코인 소모값을 먼저 낸다.
        /// 적이 쓰러지면 끝나고, 아니면 적 차례로 넘어간다. 때릴 수 없으면 null 이다.
        /// </summary>
        public AttackResult Attack(SymbolTagType tag)
        {
            if (!CanAttackWith(tag))
            {
                return null;
            }

            AttackResult result = Calculate(tag);

            PlayerHealth -= result.HealthPaid;
            if (result.ShieldmanReduced > 0)
            {
                result.Target.Shieldmen--;
                RefreshDescription(result.Target);
            }

            result.Target.Block -= result.Blocked;
            result.Target.Health = result.EnemyHealthAfter;
            RemoveDefeated();
            UpdateNextActions();

            Turns++;
            if (result.Damage > BiggestHit)
            {
                BiggestHit = result.Damage;
            }

            _rerollTargets.Clear();
            _rerollTargets.AddRange(result.ReelIndices);

            _selectedOffer = -1;
            _withoutCoin = false;
            LastAttack = result;

            Phase = Enemy.IsDead ? CombatPhase.Won : CombatPhase.EnemyTurn;
            return result;
        }

        /// <summary>
        /// 적 차례. 살아 있는 적이 왼쪽부터 한 번씩 행동한다. 영웅이 받은 피해 합을 돌려준다.
        /// 무엇을 했는지는 `EnemyActions` 에 남는다. 플레이어가 쓰러지면 끝난다.
        /// </summary>
        public int EnemyAttack()
        {
            if (Phase != CombatPhase.EnemyTurn)
            {
                return 0;
            }

            _notices.Clear();
            _enemyActions.Clear();

            int before = PlayerHealth;

            // 행동하는 동안 소환으로 목록이 바뀐다. 차례는 시작할 때의 자리로 정한다.
            List<CombatEnemy> order = new List<CombatEnemy>(_enemies);
            for (int i = 0; i < order.Count && PlayerHealth > 0; i++)
            {
                CombatEnemy actor = order[i];
                if (actor.IsDead || actor.JustSummoned)
                {
                    continue;
                }

                EnemyAction action = Act(actor);
                actor.Actions++;
                actor.LastAction = action;
                _enemyActions.Add(action);
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                _enemies[i].JustSummoned = false;
            }

            UpdateNextActions();

            Phase = PlayerHealth <= 0 ? CombatPhase.Lost : CombatPhase.Rerolling;
            return before - PlayerHealth;
        }

        /// <summary>
        /// 적이 영웅을 때린다. 영웅이 받은 피해를 돌려준다. 몬스터 기획서의 피해 계산 순서다.
        ///   ① 데미지 증감(고정)   스테이지 보정
        ///   ② 데미지 증감(퍼센트) 기세
        ///   ③ 받는 데미지 증감(퍼센트) 지금은 없다
        ///   ④ 받는 데미지 증감(고정) 철제 방패
        ///   ⑤ 방어도 영웅은 아직 방어도가 없다
        ///   ⑥ 체력
        /// </summary>
        private int HitPlayer(CombatEnemy actor, int baseDamage)
        {
            return DamageToPlayer(actor, baseDamage, true);
        }

        /// <summary>`HitPlayer` 의 셈. apply 가 아니면 체력을 깎지 않고 유물 소식도 남기지 않는다. 다음 행동 미리보기가 쓴다.</summary>
        private int DamageToPlayer(CombatEnemy actor, int baseDamage, bool apply)
        {
            int damage = baseDamage + actor.AttackBonus;
            if (actor.Momentum != 0)
            {
                damage = (int)Math.Round(damage * (100 + actor.Momentum) / 100.0, MidpointRounding.AwayFromZero);
            }

            damage = Math.Max(0, damage);

            // 철제 방패. 적이 공격하기 전에 그 피해가 문턱 이상이면 줄인다.
            ForEachEffect(RelicEffectKind.ReduceDamageTaken, (relic, effect) =>
            {
                if (damage >= effect.Threshold)
                {
                    damage = Math.Max(0, damage - effect.Amount);
                    if (apply)
                    {
                        _notices.Add(relic.DisplayName + " · 받는 피해 -" + effect.Amount);
                    }
                }
            });

            if (apply)
            {
                PlayerHealth -= damage;
            }

            return damage;
        }

        /// <summary>쓰러진 적을 자리에서 뺀다. 본체가 쓰러졌으면 소환수도 함께 사라진다.</summary>
        private void RemoveDefeated()
        {
            bool mainDown = Enemy.IsDead;
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                CombatEnemy enemy = _enemies[i];
                if (enemy == Enemy)
                {
                    continue;
                }

                if (enemy.IsDead || mainDown)
                {
                    _enemies.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 쓴 문양만 다시 돌리고 새 코인을 꺼낸다. 다시 돈 자리를 돌려준다.
        /// 1.1.a 의 "재굴림은 사용된 문양에 한정에서 이루어진다".
        /// </summary>
        public List<int> Reroll()
        {
            List<int> spun = new List<int>(_rerollTargets);

            if (Phase != CombatPhase.Rerolling)
            {
                return new List<int>();
            }

            Draw(spun);
            _rerollTargets.Clear();
            OnReelsStopped();

            OfferCoins();
            StartEnemyTurns();
            Phase = CombatPhase.ChoosingCoin;
            return spun;
        }

        /// <summary>끝난 전투의 결과.</summary>
        public CombatOutcome GetOutcome()
        {
            CombatOutcome outcome = new CombatOutcome();
            outcome.Won = Phase == CombatPhase.Won;
            outcome.BiggestHit = BiggestHit;
            outcome.Overkill = Enemy.Overkill;
            outcome.Turns = Turns;
            return outcome;
        }

        /// <summary>
        /// 문양 재사용 횟수. 그 자리의 문양이 계산에 쓰일 때 몇 번 더 쓰이는지.
        /// 속임수 주사위는 왼쪽에서 N번째 자리, 별의 저편은 천왕성 · 해왕성 · 명왕성 태그마다 한 번씩이다.
        /// 재사용으로는 다시 발동하지 않는다(시트 비고).
        /// </summary>
        private int CountReuses(int reelIndex)
        {
            ReelSymbol symbol = _reels[reelIndex];
            int reuses = 0;

            ForEachEffect(RelicEffectKind.ReuseReelAt, (relic, effect) =>
            {
                if (reelIndex == effect.Threshold - 1)
                {
                    reuses += effect.Amount;
                }
            });

            ForEachEffect(RelicEffectKind.ReuseSymbolsWithTag, (relic, effect) =>
            {
                if (effect.Targets(symbol.FirstTag) || effect.Targets(symbol.SecondTag))
                {
                    reuses += effect.Amount;
                }
            });

            return reuses;
        }

        /// <summary>
        /// 릴이 멈췄다. 처음 회전과 매 재굴림 뒤에 부른다. 시트의 `EndRoll` · `ConfirmCard` 시점이다.
        ///   - 망치와 끌. 그 턴이면 가장 많은 태그의 문양을 이번 전투 덱에 더한다
        ///   - 12개의 금화. 같은 태그 문양이 문턱 이상이면 고유 수치가 영구히 오른다
        /// </summary>
        private void OnReelsStopped()
        {
            _notices.Clear();
            int turn = Turns + 1;

            SymbolTagType top;
            int topCount;
            bool hasTop = TryGetTopTag(out top, out topCount);

            ForEachEffect(RelicEffectKind.AddTopTagSymbols, (relic, effect) =>
            {
                if (turn != effect.Threshold || !hasTop)
                {
                    return;
                }

                int added = AddRandomSymbols(top, effect.Amount);
                if (added > 0)
                {
                    _notices.Add(relic.DisplayName + " · " + TagNames.Get(top) + " 문양 " + added + "장 추가");
                }
            });

            ForEachEffect(RelicEffectKind.CoinWeightFromCounter, (relic, effect) =>
            {
                if (topCount < effect.Threshold)
                {
                    return;
                }

                relic.Stack += effect.Amount;
                _notices.Add(relic.DisplayName + " · 코인 가치 " + relic.Counter);

                if (RelicStackChanged != null)
                {
                    RelicStackChanged(relic);
                }
            });
        }

        /// <summary>그 태그를 가진 문양을 무작위로 골라 이번 전투 덱에 더한다. 더한 장수를 돌려준다.</summary>
        private int AddRandomSymbols(SymbolTagType tag, int count)
        {
            List<ReelSymbol> source = _pool.Count > 0 ? _pool : _deck;
            List<ReelSymbol> candidates = new List<ReelSymbol>();

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].Has(tag))
                {
                    candidates.Add(source[i]);
                }
            }

            if (candidates.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < count; i++)
            {
                _deck.Add(candidates[_random.Range(0, candidates.Count)]);
            }

            return count;
        }

        /// <summary>그 종류의 효과를 가진 유물이 있는지.</summary>
        private bool HasEffect(RelicEffectKind kind)
        {
            bool found = false;
            ForEachEffect(kind, (relic, effect) => found = true);
            return found;
        }

        /// <summary>들고 있는 유물의 그 종류 효과마다 한 번씩 부른다. 유물 차례, 효과 차례대로다.</summary>
        private void ForEachEffect(RelicEffectKind kind, Action<CombatRelic, RelicEffect> action)
        {
            for (int i = 0; i < _relics.Count; i++)
            {
                RelicDefinition definition = _relics[i] != null ? _relics[i].Definition : null;
                if (definition == null)
                {
                    continue;
                }

                for (int j = 0; j < definition.Effects.Count; j++)
                {
                    if (definition.Effects[j].Kind == kind)
                    {
                        action(_relics[i], definition.Effects[j]);
                    }
                }
            }
        }

        private void SpinAll()
        {
            int count = _rules != null ? _rules.ReelCount : 6;

            _reels.Clear();
            List<int> all = new List<int>();
            for (int i = 0; i < count; i++)
            {
                _reels.Add(new ReelSymbol());
                all.Add(i);
            }

            Draw(all);
        }

        /// <summary>
        /// 그 자리들에 덱에서 새 문양을 놓는다.
        ///
        /// 슬롯은 비복원 추출이다. 다른 자리에 이미 놓인 문양은 덱에서 빼고 뽑는다.
        /// 같은 문양을 두 장 가졌으면 두 자리에 함께 나올 수 있다.
        /// 덱이 릴 수보다 적으면 빼지 않고 다시 뽑는다. 흐름이 덱을 6장 아래로 줄이지 않으므로 보통은 없다.
        /// </summary>
        private void Draw(List<int> targets)
        {
            if (targets.Count == 0 || _deck.Count == 0)
            {
                return;
            }

            List<ReelSymbol> pool = new List<ReelSymbol>(_deck);

            for (int i = 0; i < _reels.Count; i++)
            {
                if (targets.Contains(i))
                {
                    continue;
                }

                int at = IndexOf(pool, _reels[i].Id);
                if (at >= 0)
                {
                    pool.RemoveAt(at);
                }
            }

            _random.Shuffle(pool);

            for (int i = 0; i < targets.Count; i++)
            {
                if (pool.Count == 0)
                {
                    pool.AddRange(_deck);
                    _random.Shuffle(pool);
                }

                _reels[targets[i]] = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
            }
        }

        private static int IndexOf(List<ReelSymbol> list, string id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>가진 코인 중 이번 턴에 보일 것을 뽑는다. 지난 턴과 상관없이 새로 뽑는다.</summary>
        private void OfferCoins()
        {
            _offered.Clear();
            _selectedOffer = -1;
            _withoutCoin = false;

            List<int> indices = new List<int>();
            for (int i = 0; i < _coins.Count; i++)
            {
                indices.Add(i);
            }

            _random.Shuffle(indices);

            int count = _rules != null ? _rules.CoinsOffered : 3;
            for (int i = 0; i < indices.Count && i < count; i++)
            {
                _offered.Add(indices[i]);
            }
        }
    }
}
