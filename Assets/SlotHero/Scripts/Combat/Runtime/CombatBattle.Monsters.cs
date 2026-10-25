using System.Collections.Generic;

namespace SlotHero.Combat
{
    /// <summary>
    /// 적의 패턴. `slot hero 테스트 몬스터.pptx`(Stage 1 몬스터 기획서)의 검투사, 장군, 방패병을 옮겼다.
    ///
    /// 공통 행동 방식.
    ///   - 매 턴 패턴 하나를 확률이나 조건으로 고른다. 행동하는 그 순간에 고른다
    ///   - 왼쪽 자리의 적부터 행동한다. 방패병은 장군 왼쪽이라 장군보다 먼저 행동한다
    ///   - 방어도는 그 적이 행동하기 직전에 사라진다. 얻은 방어도는 다음 영웅 공격을 막는다
    ///   - 소환된 턴에는 행동하지 않는다
    /// </summary>
    public partial class CombatBattle
    {
        private GladiatorRule GladiatorRules
        {
            get { return _rules != null && _rules.Gladiator != null ? _rules.Gladiator : new GladiatorRule(); }
        }

        private GeneralRule GeneralRules
        {
            get { return _rules != null && _rules.General != null ? _rules.General : new GeneralRule(); }
        }

        private ShieldmanRule ShieldmanRules
        {
            get { return _rules != null && _rules.Shieldman != null ? _rules.Shieldman : new ShieldmanRule(); }
        }

        /// <summary>살아 있는 소환수 수. 장군에게는 방패병 수다.</summary>
        public int SummonCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _enemies.Count; i++)
                {
                    if (_enemies[i].IsSummon && !_enemies[i].IsDead)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>적 하나가 한 번 행동한다.</summary>
        private EnemyAction Act(CombatEnemy actor)
        {
            // 방어도는 매 턴 소멸한다.
            actor.Block = 0;

            switch (actor.Monster)
            {
                case MonsterKind.Gladiator:
                    return ActGladiator(actor);
                case MonsterKind.General:
                    return ActGeneral(actor);
                case MonsterKind.Shieldman:
                    return ActShieldman(actor);
                default:
                    return Strike(actor, "공격", actor.Attack);
            }
        }

        /// <summary>
        /// 검투사. 첫 턴은 반드시 타격, 그 뒤로 타격 50 · 방어 30 · 함성 20.
        /// </summary>
        private EnemyAction ActGladiator(CombatEnemy actor)
        {
            GladiatorRule rule = GladiatorRules;

            int pick = actor.Actions == 0
                ? 0
                : PickWeighted(rule.StrikeWeight, rule.GuardWeight, rule.ShoutWeight);

            switch (pick)
            {
                case 1:
                    return Guard(actor, "방어", rule.GuardBlock);
                case 2:
                    actor.Momentum += rule.ShoutMomentum;
                    return Buff(actor, "함성");
                default:
                    return Strike(actor, "타격", rule.StrikeDamage);
            }
        }

        /// <summary>
        /// 장군. 방패병 수로 패턴이 갈린다.
        ///   0기    진형 갖추기
        ///   1~2기  진형 갖추기 30 · 기마병 돌격 40 · 투창 명령 30
        ///   3기    기마병 돌격 4/7 · 투창 명령 3/7 (진형 갖추기를 빼고 다시 나눈다)
        /// </summary>
        private EnemyAction ActGeneral(CombatEnemy actor)
        {
            GeneralRule rule = GeneralRules;
            int shieldmen = SummonCount;

            int pick;
            if (shieldmen == 0)
            {
                pick = 0;
            }
            else
            {
                int formation = shieldmen < rule.MaxShieldmen ? rule.FormationWeight : 0;
                pick = PickWeighted(formation, rule.ChargeWeight, rule.JavelinOrderWeight);
            }

            switch (pick)
            {
                case 1:
                    return Strike(actor, "기마병 돌격", rule.ChargeDamage);
                case 2:
                    for (int i = 0; i < _enemies.Count; i++)
                    {
                        if (_enemies[i].Monster == MonsterKind.Shieldman && !_enemies[i].IsDead)
                        {
                            _enemies[i].JavelinOrdered = true;
                        }
                    }

                    return Buff(actor, "투창 명령");
                default:
                    EnemyAction action = new EnemyAction();
                    action.Actor = actor;
                    action.Pattern = "진형 갖추기";
                    action.Kind = PatternKind.Summon;
                    action.Summoned = SummonShieldman(actor);
                    return action;
            }
        }

        /// <summary>방패병. '투창 명령' 이 있으면 투창, 없으면 방어 상태.</summary>
        private EnemyAction ActShieldman(CombatEnemy actor)
        {
            ShieldmanRule rule = ShieldmanRules;

            if (actor.JavelinOrdered)
            {
                actor.JavelinOrdered = false;
                return Strike(actor, "투창", rule.JavelinDamage);
            }

            return Guard(actor, "방어 상태", rule.GuardBlock);
        }

        /// <summary>
        /// 장군과 가장 가까운 빈자리(바로 왼쪽부터)에 방패병을 세운다. 자리가 없으면 null 이다.
        /// </summary>
        private CombatEnemy SummonShieldman(CombatEnemy owner)
        {
            for (int slot = owner.Slot - 1; slot >= 0; slot--)
            {
                if (IsSlotTaken(slot))
                {
                    continue;
                }

                CombatEnemy shieldman = _rules != null
                    ? _rules.MakeShieldman(owner.Stage)
                    : new CombatEnemy(ShieldmanRules.Name, ShieldmanRules.Description,
                        ShieldmanRules.MaxHealth, ShieldmanRules.JavelinDamage, MonsterKind.Shieldman);
                shieldman.IsSummon = true;
                shieldman.Slot = slot;
                shieldman.JustSummoned = true;

                _enemies.Add(shieldman);
                _enemies.Sort((a, b) => a.Slot.CompareTo(b.Slot));
                return shieldman;
            }

            return null;
        }

        private bool IsSlotTaken(int slot)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].Slot == slot && !_enemies[i].IsDead)
                {
                    return true;
                }
            }

            return false;
        }

        private EnemyAction Strike(CombatEnemy actor, string pattern, int damage)
        {
            EnemyAction action = new EnemyAction();
            action.Actor = actor;
            action.Pattern = pattern;
            action.Kind = PatternKind.Attack;
            action.Damage = HitPlayer(actor, damage);
            return action;
        }

        private static EnemyAction Guard(CombatEnemy actor, string pattern, int block)
        {
            actor.Block += block;

            EnemyAction action = new EnemyAction();
            action.Actor = actor;
            action.Pattern = pattern;
            action.Kind = PatternKind.Defend;
            action.BlockGained = block;
            return action;
        }

        private static EnemyAction Buff(CombatEnemy actor, string pattern)
        {
            EnemyAction action = new EnemyAction();
            action.Actor = actor;
            action.Pattern = pattern;
            action.Kind = PatternKind.Buff;
            return action;
        }

        /// <summary>가중치대로 하나를 고른다. 0 인 칸은 고르지 않는다. 모두 0 이면 0 이다.</summary>
        private int PickWeighted(params int[] weights)
        {
            int total = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                total += weights[i] > 0 ? weights[i] : 0;
            }

            if (total <= 0)
            {
                return 0;
            }

            int roll = _monsterRandom.Range(0, total);
            for (int i = 0; i < weights.Length; i++)
            {
                int weight = weights[i] > 0 ? weights[i] : 0;
                if (roll < weight)
                {
                    return i;
                }

                roll -= weight;
            }

            return 0;
        }
    }
}
