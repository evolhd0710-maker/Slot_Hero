using System;

namespace SlotHero.Combat
{
    /// <summary>
    /// 적의 패턴. `slot hero 테스트 몬스터.pptx`(Stage 1 몬스터 기획서)의 검투사와
    /// 장군 패턴 재설계(2026년 10월 29일)의 장군을 옮겼다.
    ///
    /// 공통 행동 방식.
    ///   - 모든 적이 턴 시작에 다음 행동을 미리 골라 정보 칸에 보이고, 적 차례에 그 행동을 한다
    ///   - 장군은 고르기 전에 방패병을 소환한다
    ///   - 방어도는 그 적이 행동하기 직전에 사라진다. 얻은 방어도는 다음 영웅 공격을 막는다
    /// </summary>
    public partial class CombatBattle
    {
        private const int Charge = 0;
        private const int Formation = 1;
        private const int ShieldWall = 2;
        private const int AllOut = 3;

        private GladiatorRule GladiatorRules
        {
            get { return _rules != null && _rules.Gladiator != null ? _rules.Gladiator : new GladiatorRule(); }
        }

        private GeneralRule GeneralRules
        {
            get { return _rules != null && _rules.General != null ? _rules.General : new GeneralRule(); }
        }

        /// <summary>
        /// 턴 시작. 플레이어가 코인을 고르기 전이다. 전투 시작과 재굴림 뒤에 부른다.
        /// 장군은 방패병을 소환한다. 그 뒤 모든 적이 이번 턴 행동을 고른다.
        /// </summary>
        private void StartEnemyTurns()
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                CombatEnemy enemy = _enemies[i];
                if (enemy.IsDead)
                {
                    continue;
                }

                if (enemy.Monster == MonsterKind.General)
                {
                    GeneralRule rule = GeneralRules;
                    int before = enemy.Shieldmen;
                    enemy.Shieldmen = Math.Min(rule.MaxShieldmen, enemy.Shieldmen + rule.SummonPerTurn);
                    if (enemy.Shieldmen > before)
                    {
                        _notices.Add(enemy.Name + " · 방패병 소환 (" + enemy.Shieldmen + ")");
                    }

                    RefreshDescription(enemy);
                }

                enemy.PlannedPick = PickPattern(enemy);
            }

            UpdateNextActions();
        }

        /// <summary>
        /// 이번 턴 패턴 칸을 고른다.
        ///   검투사 0 타격 · 1 방어 · 2 함성. 첫 턴은 타격, 그 뒤로 50 · 30 · 20
        ///   장군   0 돌격 · 1 진형 형성 · 2 방패 진형 · 3 총공세(`PickGeneral`)
        /// </summary>
        private int PickPattern(CombatEnemy enemy)
        {
            switch (enemy.Monster)
            {
                case MonsterKind.Gladiator:
                    GladiatorRule rule = GladiatorRules;
                    return enemy.Actions == 0 ? 0 : PickWeighted(rule.StrikeWeight, rule.GuardWeight, rule.ShoutWeight);
                case MonsterKind.General:
                    return PickGeneral(enemy.Shieldmen);
                default:
                    return 0;
            }
        }

        /// <summary>살아 있는 적마다 고른 패턴의 미리보기를 지금 값으로 다시 셈한다. 고르지 않았으면 비운다.</summary>
        private void UpdateNextActions()
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                CombatEnemy enemy = _enemies[i];
                enemy.NextAction = enemy.IsDead || enemy.PlannedPick < 0 ? null : Preview(enemy, enemy.PlannedPick);
            }
        }

        /// <summary>그 패턴을 쓰면 어떻게 될지와 그 설명. 아무것도 바꾸지 않는다.</summary>
        private EnemyAction Preview(CombatEnemy actor, int pick)
        {
            switch (actor.Monster)
            {
                case MonsterKind.Gladiator:
                    GladiatorRule gladiator = GladiatorRules;
                    switch (pick)
                    {
                        case 1:
                            return Planned(actor, "방어", PatternKind.Defend, 0, gladiator.GuardBlock,
                                Format(gladiator.GuardText, gladiator.GuardBlock));
                        case 2:
                            return Planned(actor, "함성", PatternKind.Buff, 0, 0,
                                Format(gladiator.ShoutText, gladiator.ShoutMomentum));
                        default:
                            int strike = DamageToPlayer(actor, gladiator.StrikeDamage, false);
                            return Planned(actor, "타격", PatternKind.Attack, strike, 0, Format(gladiator.StrikeText, strike));
                    }
                case MonsterKind.General:
                    GeneralRule general = GeneralRules;
                    int shieldmen = actor.Shieldmen;
                    switch (pick)
                    {
                        case Formation:
                            return Planned(actor, "진형 형성", PatternKind.Summon, 0, 0,
                                Format(general.FormationText, general.FormationSummon, general.MaxShieldmen));
                        case ShieldWall:
                            int wall = DamageToPlayer(actor, general.ShieldWallDamage * (shieldmen + 1), false);
                            return Planned(actor, "방패 진형", PatternKind.Attack, wall, 0,
                                Format(general.ShieldWallText, wall, general.ShieldWallDamage, shieldmen));
                        case AllOut:
                            int allOut = DamageToPlayer(actor, general.AllOutDamage * shieldmen, false);
                            return Planned(actor, "총공세", PatternKind.Attack, allOut, 0,
                                Format(general.AllOutText, allOut, general.AllOutDamage, shieldmen));
                        default:
                            int charge = DamageToPlayer(actor, general.ChargeDamage * (shieldmen + 1), false);
                            return Planned(actor, "돌격", PatternKind.Attack, charge, 0,
                                Format(general.ChargeText, charge, general.ChargeDamage, shieldmen));
                    }
                default:
                    int hit = DamageToPlayer(actor, actor.Attack, false);
                    return Planned(actor, "공격", PatternKind.Attack, hit, 0,
                        Format(_rules != null ? _rules.PlainAttackText : "영웅에게 {0} 피해를 준다.", hit));
            }
        }

        private static EnemyAction Planned(CombatEnemy actor, string pattern, PatternKind kind, int damage, int block, string description)
        {
            EnemyAction action = new EnemyAction();
            action.Actor = actor;
            action.Pattern = pattern;
            action.Kind = kind;
            action.Damage = damage;
            action.BlockGained = block;
            action.Description = description;
            return action;
        }

        /// <summary>설명 글에 수를 넣는다. 인스펙터에 적은 "\n" 은 줄바꿈으로 바꾼다. 글이 틀려도 전투를 멈추지 않는다.</summary>
        private static string Format(string text, params object[] values)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            try
            {
                return string.Format(text.Replace("\\n", "\n"), values);
            }
            catch (FormatException)
            {
                return text;
            }
        }

        /// <summary>장군 설명에 지금 방패병 수를 적는다. 다른 적은 그대로 둔다.</summary>
        private void RefreshDescription(CombatEnemy enemy)
        {
            if (enemy.Monster == MonsterKind.General)
            {
                enemy.Description = GeneralRules.Describe(enemy.Shieldmen);
            }
        }

        /// <summary>
        /// 방패병이 줄이는 피해. 영웅 피해 계산이 모두 끝난 값으로 본다.
        /// 장군이 방패병을 가졌고 피해가 문턱 이상이면 한 번 줄인다.
        /// </summary>
        private int GetShieldmanReduction(CombatEnemy target, int damage)
        {
            if (target == null || target.Monster != MonsterKind.General || target.Shieldmen <= 0)
            {
                return 0;
            }

            GeneralRule rule = GeneralRules;
            return damage >= rule.GuardThreshold ? Math.Min(rule.GuardReduction, damage) : 0;
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
                default:
                    actor.PlannedPick = -1;
                    return Strike(actor, "공격", actor.Attack);
            }
        }

        /// <summary>검투사. 턴 시작에 고른 패턴을 쓴다. 0 타격 · 1 방어 · 2 함성.</summary>
        private EnemyAction ActGladiator(CombatEnemy actor)
        {
            GladiatorRule rule = GladiatorRules;

            int pick = actor.PlannedPick >= 0 ? actor.PlannedPick : PickPattern(actor);
            actor.PlannedPick = -1;

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
        /// 장군 기술 고르기. 방패병 수로 가중치가 갈린다.
        ///   돌격 1 ~ 2기 50 · 진형 형성 1기 20 · 방패 진형 2 ~ 3기 70 · 총공세 3기 130
        /// 방패병이 없으면 돌격이다.
        /// </summary>
        private int PickGeneral(int shieldmen)
        {
            GeneralRule rule = GeneralRules;
            return PickWeighted(
                shieldmen >= 1 && shieldmen <= 2 ? rule.ChargeWeight : 0,
                shieldmen == 1 ? rule.FormationWeight : 0,
                shieldmen >= 2 ? rule.ShieldWallWeight : 0,
                shieldmen >= 3 ? rule.AllOutWeight : 0);
        }

        /// <summary>
        /// 장군. 턴 시작에 고른 기술을 쓴다. 위력은 지금 방패병 수로 셈한다.
        /// 영웅 공격에 방패병이 줄었으면 그만큼 약해진다.
        /// </summary>
        private EnemyAction ActGeneral(CombatEnemy actor)
        {
            GeneralRule rule = GeneralRules;
            int pick = actor.PlannedPick >= 0 ? actor.PlannedPick : PickGeneral(actor.Shieldmen);
            actor.PlannedPick = -1;
            int shieldmen = actor.Shieldmen;

            switch (pick)
            {
                case Formation:
                    actor.Shieldmen = Math.Min(rule.MaxShieldmen, shieldmen + rule.FormationSummon);
                    RefreshDescription(actor);

                    EnemyAction action = new EnemyAction();
                    action.Actor = actor;
                    action.Pattern = "진형 형성";
                    action.Kind = PatternKind.Summon;
                    return action;
                case ShieldWall:
                    return Strike(actor, "방패 진형", rule.ShieldWallDamage * (shieldmen + 1));
                case AllOut:
                    return Strike(actor, "총공세", rule.AllOutDamage * shieldmen);
                default:
                    return Strike(actor, "돌격", rule.ChargeDamage * (shieldmen + 1));
            }
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
