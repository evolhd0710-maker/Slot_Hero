using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Combat;
using SlotHero.Combat.UI;
using SlotHero.Flow;
using SlotHero.Map;
using SlotHero.Save;
using Spine.Unity;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 전투를 검사한다. 메뉴 Slot Hero / 전투 검사 에서 실행한다.
    ///
    /// 두 갈래다.
    ///   - 규칙. `CombatBattle` 을 유니티 없이 굴려 계산식, 재굴림, 코인 뽑기, 체력, 승패를 본다
    ///   - 화면. 게임 씬을 열어 전투 창구가 진짜 전투에 물렸는지 보고, 런 하나로 전투를 끝까지 눌러 본다
    ///
    /// 화면은 플레이 중이 아니라 연출 없이 지나간다(`CombatScreenController.Instant`).
    /// 씬은 저장하지 않는다.
    /// </summary>
    public static class CheckCombat
    {
        private static int _checked;
        private static int _failed;

        [MenuItem("Slot Hero/전투 검사", priority = 2)]
        public static void Check()
        {
            _checked = 0;
            _failed = 0;

            CombatRuleConfig rules = ScriptableObject.CreateInstance<CombatRuleConfig>();

            CheckFormula(rules);
            CheckWithoutCoin(rules);
            CheckEmptyTag(rules);
            CheckReroll(rules);
            CheckCoinOffer(rules);
            CheckCoinCost(rules);
            CheckEnemyTurn(rules);
            CheckVictory(rules);
            CheckSeed(rules);
            CheckRules(rules);
            CheckSectors();
            CheckRelics(rules);
            CheckGladiator(rules);
            CheckGeneral(rules);
            CheckMonsterRules(rules);

            Object.DestroyImmediate(rules);

            CheckRelicCatalog();

            CheckFontPinned();
            CheckGameScene();

            Debug.Log("전투 검사 " + _checked + "건, 실패 " + _failed + "건");
        }

        // ---- 규칙 ----

        /// <summary>1.1.a 작동 예시. 해왕성에 소모 10 · 가중치 5 코인을 걸면 (7 + 9) × (2 + 5) = 112.</summary>
        private static void CheckFormula(CombatRuleConfig rules)
        {
            CombatBattle battle = new CombatBattle(ExampleSetup(99, 500), rules);
            battle.Begin();

            Report("시작하면 코인을 고른다", battle.Phase == CombatPhase.ChoosingCoin, (int)battle.Phase);
            Report("릴이 여섯이다", battle.Reels.Count, 6);
            Report("덱 여섯이 릴에 모두 나온다(비복원)", Distinct(battle.Reels), 6);

            Report("코인을 고르면 태그를 고른다", battle.SelectCoin(0) && battle.Phase == CombatPhase.ChoosingTag);

            AttackResult attack = battle.Attack(SymbolTagType.Neptune);
            Report("해왕성으로 때릴 수 있다", attack != null);
            if (attack == null)
            {
                return;
            }

            Report("해왕성 문양은 섬과 난파선 둘이다", attack.DuplicateCount, 2);
            Report("문양 가치 합은 7 + 9", attack.ValueSum, 16);
            Report("중복 배수는 2", attack.DuplicateMultiplier, 2);
            Report("코인 가중치 5 가 배수에 더해진다", attack.Multiplier, 7);
            Report("공격력 (7 + 9) × (2 + 5) = 112", attack.Damage, 112);
            Report("코인 소모값만큼 체력을 낸다", battle.PlayerHealth, 89);
            Report("적 체력이 깎인다", battle.Enemy.Health, 500 - 112);
            Report("적이 살아 있으면 적 차례다", battle.Phase == CombatPhase.EnemyTurn, (int)battle.Phase);
            Report("계산에 쓴 두 자리만 다시 돈다", battle.RerollTargets.Count, 2);

            bool targetsAreNeptune = true;
            for (int i = 0; i < battle.RerollTargets.Count; i++)
            {
                targetsAreNeptune &= battle.Reels[battle.RerollTargets[i]].Has(SymbolTagType.Neptune);
            }

            Report("다시 돌 자리는 해왕성 문양 자리다", targetsAreNeptune);
            Report("가장 큰 피해가 남는다", battle.BiggestHit, 112);
        }

        /// <summary>코인 없이 고르면 가중치도 소모도 없다.</summary>
        private static void CheckWithoutCoin(CombatRuleConfig rules)
        {
            CombatBattle battle = new CombatBattle(ExampleSetup(99, 500), rules);
            battle.Begin();
            battle.SelectWithoutCoin();

            AttackResult attack = battle.Attack(SymbolTagType.Neptune);
            Report("코인 없이 해왕성은 16 × 2 = 32", attack != null ? attack.Damage : -1, 32);
            Report("코인 없이는 체력을 내지 않는다", battle.PlayerHealth, 99);
        }

        /// <summary>그 태그 문양이 없으면 그 태그로 때릴 수 없다.</summary>
        private static void CheckEmptyTag(CombatRuleConfig rules)
        {
            CombatBattle battle = new CombatBattle(ExampleSetup(99, 500), rules);
            battle.Begin();
            battle.SelectCoin(0);

            Report("문양이 없는 태그(천왕성)로는 때릴 수 없다", !battle.CanAttackWith(SymbolTagType.Uranus));
            Report("때리지 못하면 아무것도 바뀌지 않는다", battle.Attack(SymbolTagType.Uranus) == null && battle.Phase == CombatPhase.ChoosingTag);

            Report("태그 UI 가운데를 누르면 코인으로 돌아간다", battle.BackToCoins() && battle.Phase == CombatPhase.ChoosingCoin);
            Report("돌아가면 고른 코인이 풀린다", !battle.HasChosenCoin);
        }

        /// <summary>적이 때린 뒤 쓴 문양만 다시 돈다. 나머지 자리는 그대로다.</summary>
        private static void CheckReroll(CombatRuleConfig rules)
        {
            CombatSetup setup = ExampleSetup(99, 5000);

            // 덱을 늘려 다시 돌 때 다른 문양이 나올 수 있게 한다.
            setup.Deck.Add(new ReelSymbol("Coffin", "관", SymbolTagType.Saturn, SymbolTagType.Pluto, 8));
            setup.Deck.Add(new ReelSymbol("Whale", "고래", SymbolTagType.Neptune, SymbolTagType.Jupiter, 7));
            setup.Deck.Add(new ReelSymbol("Volcano", "화산", SymbolTagType.Mars, SymbolTagType.Jupiter, 5));
            setup.Deck.Add(new ReelSymbol("Fullmoon", "보름달", SymbolTagType.Earth, SymbolTagType.Uranus, 6));

            CombatBattle battle = new CombatBattle(setup, rules);
            battle.Begin();

            bool stable = true;
            bool noDuplicates = true;

            for (int turn = 0; turn < 12; turn++)
            {
                battle.SelectWithoutCoin();

                SymbolTagType tag = FirstUsableTag(battle);
                battle.Attack(tag);

                List<int> used = new List<int>(battle.RerollTargets);
                List<string> before = Ids(battle.Reels);

                battle.EnemyAttack();
                if (battle.Phase != CombatPhase.Rerolling)
                {
                    break;
                }

                battle.Reroll();
                List<string> after = Ids(battle.Reels);

                for (int i = 0; i < before.Count; i++)
                {
                    if (!used.Contains(i) && before[i] != after[i])
                    {
                        stable = false;
                    }
                }

                noDuplicates &= Distinct(battle.Reels) == battle.Reels.Count;
            }

            Report("쓰지 않은 문양은 다시 돌아도 그대로다", stable);
            Report("다시 돌아도 한 장뿐인 문양이 두 자리에 나오지 않는다", noDuplicates);
            Report("다시 돈 뒤 코인 선택으로 돌아온다", battle.Phase == CombatPhase.ChoosingCoin, (int)battle.Phase);
        }

        /// <summary>코인은 매 턴 셋을 겹치지 않게 꺼낸다. 가진 것이 적으면 그만큼만 꺼낸다.</summary>
        private static void CheckCoinOffer(CombatRuleConfig rules)
        {
            CombatSetup setup = ExampleSetup(99, 5000);
            setup.Coins.Clear();
            for (int i = 0; i < 5; i++)
            {
                setup.Coins.Add(new CombatCoin("coin_" + i, "코인" + i, 1, 1, string.Empty));
            }

            CombatBattle battle = new CombatBattle(setup, rules);
            battle.Begin();

            Report("코인 다섯 중 셋을 꺼낸다", battle.OfferedCount, 3);

            HashSet<string> offered = new HashSet<string>();
            for (int i = 0; i < battle.OfferedCount; i++)
            {
                CombatCoin coin;
                if (battle.TryGetOffered(i, out coin))
                {
                    offered.Add(coin.Id);
                }
            }

            Report("한 턴에 꺼낸 셋은 서로 다르다", offered.Count, 3);

            CombatSetup few = ExampleSetup(99, 5000);
            CombatBattle fewBattle = new CombatBattle(few, rules);
            fewBattle.Begin();
            Report("코인이 하나면 하나만 꺼낸다", fewBattle.OfferedCount, 1);

            CombatSetup none = ExampleSetup(99, 5000);
            none.Coins.Clear();
            CombatBattle noneBattle = new CombatBattle(none, rules);
            noneBattle.Begin();
            Report("코인이 없으면 꺼내지 않는다", noneBattle.OfferedCount, 0);
            Report("코인이 없어도 코인 없이 태그를 고를 수 있다", noneBattle.SelectWithoutCoin());
        }

        /// <summary>소모값을 내고도 1 이 남아야 쓸 수 있다.</summary>
        private static void CheckCoinCost(CombatRuleConfig rules)
        {
            CombatBattle low = new CombatBattle(ExampleSetup(10, 500), rules);
            low.Begin();
            Report("체력 10 으로 소모 10 코인은 쓸 수 없다", !low.CanUseOffered(0));
            Report("쓸 수 없는 코인은 고를 수 없다", !low.SelectCoin(0) && low.Phase == CombatPhase.ChoosingCoin);

            CombatBattle enough = new CombatBattle(ExampleSetup(11, 500), rules);
            enough.Begin();
            Report("체력 11 이면 소모 10 코인을 쓸 수 있다", enough.CanUseOffered(0));
        }

        /// <summary>적이 때려 체력이 0 이하면 진다.</summary>
        private static void CheckEnemyTurn(CombatRuleConfig rules)
        {
            CombatSetup setup = ExampleSetup(5, 5000);
            setup.Enemy = new CombatEnemy("시험 적", string.Empty, 5000, 6);

            CombatBattle battle = new CombatBattle(setup, rules);
            battle.Begin();
            battle.SelectWithoutCoin();
            battle.Attack(SymbolTagType.Neptune);

            int damage = battle.EnemyAttack();
            Report("적은 공격력만큼 때린다", damage, 6);
            Report("체력이 0 아래로 내려가면 진다", battle.Phase == CombatPhase.Lost, (int)battle.Phase);
            Report("진 결과", !battle.GetOutcome().Won);
            Report("진 뒤에는 다시 돌지 않는다", battle.Reroll().Count, 0);
        }

        /// <summary>적을 쓰러뜨리면 이기고 넘친 피해가 오버킬로 남는다.</summary>
        private static void CheckVictory(CombatRuleConfig rules)
        {
            CombatBattle battle = new CombatBattle(ExampleSetup(99, 50), rules);
            battle.Begin();
            battle.SelectCoin(0);
            battle.Attack(SymbolTagType.Neptune);

            CombatOutcome outcome = battle.GetOutcome();
            Report("적을 쓰러뜨리면 이긴다", battle.Phase == CombatPhase.Won, (int)battle.Phase);
            Report("적 체력이 음수로 남는다(오버킬 표시)", battle.Enemy.Health, 50 - 112);
            Report("오버킬은 넘친 피해", outcome.Overkill, 62);
            Report("이긴 뒤에는 적이 때리지 않는다", battle.EnemyAttack(), 0);
            Report("오버킬 골드는 10 마다 1", rules.GetOverkillGold(outcome.Overkill), 6);
            Report("오버킬 회복은 10 마다 체력 1", rules.GetOverkillHeal(outcome.Overkill), 6);
        }

        /// <summary>같은 시드면 같은 릴이 나온다.</summary>
        private static void CheckSeed(CombatRuleConfig rules)
        {
            CombatSetup a = ExampleSetup(99, 500);
            a.Deck.Add(new ReelSymbol("Coffin", "관", SymbolTagType.Saturn, SymbolTagType.Pluto, 8));
            a.Deck.Add(new ReelSymbol("Whale", "고래", SymbolTagType.Neptune, SymbolTagType.Jupiter, 7));

            CombatSetup b = ExampleSetup(99, 500);
            b.Deck.AddRange(new[] { a.Deck[6], a.Deck[7] });

            CombatBattle first = new CombatBattle(a, rules);
            CombatBattle second = new CombatBattle(b, rules);
            first.Begin();
            second.Begin();

            Report("같은 시드면 같은 릴", string.Join(",", Ids(first.Reels)) == string.Join(",", Ids(second.Reels)));

            Report("시드를 섞으면 방마다 다르다",
                CombatRandom.MixSeed(1, 1, 3) != CombatRandom.MixSeed(1, 1, 4)
                && CombatRandom.MixSeed(1, 1, 3) != CombatRandom.MixSeed(1, 2, 3));
        }

        /// <summary>규칙 설정의 기본값이 1.1.a 와 맞는지.</summary>
        private static void CheckRules(CombatRuleConfig rules)
        {
            Report("금화는 소모 10 · 가중치 5", rules.GetCoinRule("coin_gold").HealthCost == 10 && rules.GetCoinRule("coin_gold").Weight == 5);
            Report("은화는 소모 20 · 가중치 15", rules.GetCoinRule("coin_silver").HealthCost == 20 && rules.GetCoinRule("coin_silver").Weight == 15);
            Report("모르는 코인은 기본값", rules.GetCoinRule("없는코인").HealthCost, rules.DefaultCoinCost);
            Report("문양 0 개는 배수 0", rules.GetDuplicateMultiplier(0), 0);
            Report("표에 없는 개수는 개수 그대로", rules.GetDuplicateMultiplier(9), 9);

            CombatEnemy first = rules.MakeEnemy(CombatEnemyKind.Normal, 1);
            CombatEnemy second = rules.MakeEnemy(CombatEnemyKind.Normal, 2);
            Report("스테이지가 오르면 적 체력이 오른다", second.MaxHealth > first.MaxHealth);
            Report("스테이지가 오르면 적 공격력이 오른다", second.Attack > first.Attack);
            Report("보스가 일반보다 세다", rules.MakeEnemy(CombatEnemyKind.Boss, 1).MaxHealth > first.MaxHealth);
        }

        /// <summary>
        /// 태그 UI 칸의 차례와 누르기 범위. 맨 위가 소용돌이, 시계 방향으로 금성부터 수성까지다.
        /// </summary>
        private static void CheckSectors()
        {
            Report("맨 위 칸은 소용돌이", !TagDialView.TryGetTag(0, out _));

            SymbolTagType tag;
            Report("다음 칸은 수성", TagDialView.TryGetTag(1, out tag) && tag == SymbolTagType.Mercury);
            Report("마지막 칸은 명왕성", TagDialView.TryGetTag(9, out tag) && tag == SymbolTagType.Pluto);
            Report("태그 칸 자리를 거꾸로 찾는다", TagDialView.GetSectorIndex(SymbolTagType.Neptune), 8);

            GameObject go = new GameObject("SectorCheck", typeof(RectTransform));
            TagSectorView top = go.AddComponent<TagSectorView>();
            top.Setup(-18f, 36f, 1f, 0.37f, 200f, 60f, Color.white, null, "@");

            Report("맨 위 칸은 12시를 받는다", top.Contains(new Vector2(0f, 250f), 300f));
            Report("맨 위 칸은 3시를 받지 않는다", !top.Contains(new Vector2(250f, 0f), 300f));
            Report("가운데 코인 자리는 칸이 받지 않는다", !top.Contains(new Vector2(0f, 50f), 300f));
            Report("원 밖은 받지 않는다", !top.Contains(new Vector2(0f, 320f), 300f));

            TagSectorView right = go.AddComponent<TagSectorView>();
            right.Setup(3 * 36f - 18f, 36f, 1f, 0.37f, 200f, 60f, Color.white, null, "@");
            Report("세 번째 태그 칸(화성)이 3시를 받는다", right.Contains(new Vector2(250f, 0f), 300f));

            Object.DestroyImmediate(go);
        }

        // ---- 유물 ----

        /// <summary>
        /// 유물 정보시트의 열 개. 1.1.a 예시 덱(불사조, 간헐천, 용광로, 섬, 난파선, 수은)에 하나씩 끼워 본다.
        /// 예시 덱은 화성 문양이 셋(불사조, 간헐천, 용광로)이라 "같은 태그 셋" 조건이 처음부터 맞는다.
        /// </summary>
        private static void CheckRelics(CombatRuleConfig rules)
        {
            // 제국 군기. 화성 태그 가치 +5. 불사조(화성/지구) 4 → 9.
            CombatBattle aquila = Begin(WithRelic(ExampleSetup(99, 500), 12001, 0), rules);
            Report("제국 군기 · 화성 문양 가치 +5", aquila.GetSymbolValue(Find(aquila, "Pheonix")), 9);
            Report("제국 군기 · 화성이 없는 문양은 그대로", aquila.GetSymbolValue(Find(aquila, "Island")), 7);

            // 흑요석 단검. 코인 소모값 +10, 코인 가치 +3. 금화(10 · 5) → 20 · 8. (7 + 9) × (2 + 8) = 160.
            CombatBattle knife = Begin(WithRelic(ExampleSetup(99, 500), 12002, 0), rules);
            knife.SelectCoin(0);
            AttackResult knifeHit = knife.Attack(SymbolTagType.Neptune);
            Report("흑요석 단검 · 공격력 16 × (2 + 8) = 160", knifeHit != null ? knifeHit.Damage : -1, 160);
            Report("흑요석 단검 · 체력 20 을 낸다", knife.PlayerHealth, 79);

            CombatBattle knifeLow = Begin(WithRelic(ExampleSetup(20, 500), 12002, 0), rules);
            Report("흑요석 단검 · 체력 20 으로는 소모 20 코인을 못 쓴다", !knifeLow.CanUseOffered(0));

            // 12개의 금화. -5 에서 시작해 같은 태그 셋이 보이면 +1. 화성이 셋이라 시작하자마자 -4.
            CombatSetup twelveSetup = WithRelic(ExampleSetup(99, 500), 12003, 0);
            CombatBattle twelve = new CombatBattle(twelveSetup, rules);
            int raised = 0;
            twelve.RelicStackChanged += relic => raised++;
            twelve.Begin();
            Report("12개의 금화 · 릴이 멈추면 같은 태그 셋에 +1 을 알린다", raised, 1);
            Report("12개의 금화 · 고유 수치 -5 + 1 = -4", twelve.Relics[0].Counter, -4);
            twelve.SelectCoin(0);
            AttackResult twelveHit = twelve.Attack(SymbolTagType.Neptune);
            Report("12개의 금화 · 코인 가치 5 - 4 = 1, 16 × (2 + 1) = 48", twelveHit != null ? twelveHit.Damage : -1, 48);
            Report("12개의 금화 · 소개문에 지금 값이 들어간다", twelve.Relics[0].Description.StartsWith("코인 가치 -4"));

            CombatBattle twelveSaved = Begin(WithRelic(ExampleSetup(99, 500), 12003, 7), rules);
            Report("12개의 금화 · 런에 쌓인 횟수에서 이어진다", twelveSaved.Relics[0].Counter, -5 + 7 + 1);

            // 망치와 끌. 첫 턴 가장 많은 태그(화성 셋)의 문양 3장을 이번 전투 덱에 더한다.
            CombatSetup hammerSetup = WithRelic(ExampleSetup(99, 500), 12004, 0);
            hammerSetup.SymbolPool.Add(new ReelSymbol("Volcano", "화산", SymbolTagType.Mars, SymbolTagType.Saturn, 5));
            hammerSetup.SymbolPool.Add(new ReelSymbol("Coral", "산호", SymbolTagType.Jupiter, SymbolTagType.Neptune, 7));
            CombatBattle hammer = Begin(hammerSetup, rules);
            Report("망치와 끌 · 첫 턴 덱이 3장 는다", hammer.Deck.Count, 9);
            bool allMars = true;
            for (int i = 6; i < hammer.Deck.Count; i++)
            {
                allMars &= hammer.Deck[i].Has(SymbolTagType.Mars);
            }

            Report("망치와 끌 · 더한 문양은 가장 많은 태그(화성)다", allMars);
            Report("망치와 끌 · 소식을 남긴다", hammer.Notices.Count, 1);
            hammer.SelectWithoutCoin();
            hammer.Attack(SymbolTagType.Neptune);
            hammer.EnemyAttack();
            hammer.Reroll();
            Report("망치와 끌 · 둘째 턴에는 더하지 않는다", hammer.Deck.Count, 9);

            // 철제 방패. 10 이상의 피해에 -5.
            CombatSetup shieldSetup = WithRelic(ExampleSetup(99, 5000), 12005, 0);
            shieldSetup.Enemy = new CombatEnemy("시험 적", string.Empty, 5000, 12);
            CombatBattle shield = Begin(shieldSetup, rules);
            shield.SelectWithoutCoin();
            shield.Attack(SymbolTagType.Neptune);
            Report("철제 방패 · 12 를 7 로 줄인다", shield.EnemyAttack(), 7);

            CombatBattle smallHit = Begin(WithRelic(ExampleSetup(99, 5000), 12005, 0), rules);
            smallHit.SelectWithoutCoin();
            smallHit.Attack(SymbolTagType.Neptune);
            Report("철제 방패 · 10 아래는 그대로", smallHit.EnemyAttack(), 6);

            // 사건의 지평선. 소용돌이로 가장 많은 태그(화성)로 공격한다.
            CombatBattle plain = Begin(ExampleSetup(99, 500), rules);
            plain.SelectWithoutCoin();
            Report("사건의 지평선 없이는 소용돌이를 못 쓴다", !plain.CanUseVortex());

            CombatBattle horizon = Begin(WithRelic(ExampleSetup(99, 500), 12006, 0), rules);
            Report("사건의 지평선 · 태그를 고르기 전에는 못 쓴다", !horizon.CanUseVortex());
            horizon.SelectWithoutCoin();
            Report("사건의 지평선 · 태그를 고를 때 쓸 수 있다", horizon.CanUseVortex());
            AttackResult horizonHit = horizon.AttackWithVortex();
            Report("사건의 지평선 · 화성으로 때린다", horizonHit != null && horizonHit.Tag == SymbolTagType.Mars && horizonHit.FromVortex);
            Report("사건의 지평선 · 화성 문양 셋 (4 + 3 + 3) × 3 = 30", horizonHit != null ? horizonHit.Damage : -1, 30);

            // 별의 저편. 천왕성 · 해왕성 · 명왕성 태그마다 한 번 더. 섬(해왕성) +1, 난파선(해왕성 · 명왕성) +2.
            CombatBattle beyond = Begin(WithRelic(ExampleSetup(99, 500), 12007, 0), rules);
            beyond.SelectWithoutCoin();
            AttackResult beyondHit = beyond.Attack(SymbolTagType.Neptune);
            Report("별의 저편 · 가치 7 × 2 + 9 × 3 = 41", beyondHit != null ? beyondHit.ValueSum : -1, 41);
            Report("별의 저편 · 재사용 셋", beyondHit != null ? beyondHit.ReuseCount : -1, 3);
            Report("별의 저편 · 중복 배수는 그대로 2", beyondHit != null ? beyondHit.DuplicateMultiplier : -1, 2);
            Report("별의 저편 · 41 × 2 = 82", beyondHit != null ? beyondHit.Damage : -1, 82);

            // 일식. 내행성 -3, 외행성 +3.
            CombatBattle eclipse = Begin(WithRelic(ExampleSetup(99, 500), 12008, 0), rules);
            Report("일식 · 섬(토성 · 해왕성) 7 + 6 = 13", eclipse.GetSymbolValue(Find(eclipse, "Island")), 13);
            // 문양 가치는 0 아래로 내려가지 않는다(2026년 11월 10일 원재).
            Report("일식 · 수은(수성 · 금성) 2 - 6 은 0 에서 멈춘다", eclipse.GetSymbolValue(Find(eclipse, "Quicksilver")), 0);
            Report("일식 · 불사조(화성 · 지구) 4 + 3 - 3 = 4", eclipse.GetSymbolValue(Find(eclipse, "Pheonix")), 4);
            eclipse.SelectWithoutCoin();
            AttackResult venus = eclipse.Attack(SymbolTagType.Venus);
            Report("일식 · 가치 합과 피해가 0 아래로 내려가지 않는다",
                venus != null && venus.ValueSum >= 0 && venus.Damage >= 0
                && eclipse.Enemy.Health == 500 - (venus.Damage - venus.Blocked));

            // 곰 가죽 망토. 체력 50% 이하면 1.2배. 코인 없이 해왕성 32 → 38.
            CombatBattle low = Begin(WithRelic(ExampleSetup(49, 500), 12009, 0), rules);
            low.SelectWithoutCoin();
            AttackResult lowHit = low.Attack(SymbolTagType.Neptune);
            Report("곰 가죽 망토 · 체력 49/99 면 32 × 1.2 = 38", lowHit != null ? lowHit.Damage : -1, 38);

            CombatBattle high = Begin(WithRelic(ExampleSetup(60, 500), 12009, 0), rules);
            high.SelectWithoutCoin();
            AttackResult highHit = high.Attack(SymbolTagType.Neptune);
            Report("곰 가죽 망토 · 체력 60/99 면 그대로 32", highHit != null ? highHit.Damage : -1, 32);

            CombatBattle paid = Begin(WithRelic(ExampleSetup(55, 500), 12009, 0), rules);
            paid.SelectCoin(0);
            AttackResult paidHit = paid.Attack(SymbolTagType.Neptune);
            Report("곰 가죽 망토 · 코인을 내고 45/99 가 되면 112 × 1.2 = 134", paidHit != null ? paidHit.Damage : -1, 134);

            // 속임수 주사위. 가장 왼쪽 문양 한 번 더.
            CombatBattle dice = Begin(WithRelic(ExampleSetup(99, 500), 12010, 0), rules);
            ReelSymbol left = dice.Reels[0];
            dice.SelectWithoutCoin();
            AttackResult withoutDice = dice.Calculate(left.FirstTag);
            int expected = 0;
            for (int i = 0; i < dice.Reels.Count; i++)
            {
                if (dice.Reels[i].Has(left.FirstTag))
                {
                    expected += dice.Reels[i].Value;
                }
            }

            Report("속임수 주사위 · 가장 왼쪽 문양 가치를 한 번 더 더한다", withoutDice.ValueSum, expected + left.Value);
            Report("속임수 주사위 · 재사용 하나", withoutDice.ReuseCount, 1);
        }

        // ---- 몬스터 ----

        /// <summary>
        /// 검투사. 첫 턴 타격, 그 뒤 타격 · 방어 · 함성. 함성의 기세만큼 타격이 세지고, 방어도는 영웅 공격을 막고 다음 행동 전에 사라진다.
        /// </summary>
        private static void CheckGladiator(CombatRuleConfig rules)
        {
            bool firstIsStrike = true;
            bool strikesMatchMomentum = true;
            bool blockAbsorbs = true;
            bool blockFades = true;
            bool momentumMatchesShouts = true;
            bool firstIntentIsStrike = true;
            bool intentMatches = true;
            bool intentDescribed = true;
            int strikes = 0;
            int guards = 0;
            int shouts = 0;

            // 확률은 표본 하나로 보면 흔들린다. 시드 스물로 30턴씩, 600번을 모아 본다.
            for (int seed = 0; seed < 20; seed++)
            {
                CombatSetup setup = ExampleSetup(5000, 5000);
                setup.PlayerMaxHealth = 5000;
                setup.Seed = 20261025 + seed * 7919;
                setup.Enemy = new CombatEnemy("검투사", string.Empty, 5000, 6, MonsterKind.Gladiator);

                CombatBattle battle = Begin(setup, rules);
                int battleShouts = 0;

                EnemyAction firstIntent = battle.Enemy.NextAction;
                firstIntentIsStrike &= firstIntent != null && firstIntent.Pattern == "타격" && firstIntent.Damage == 6
                    && firstIntent.Description == "영웅에게 6 피해를 준다.";

                for (int turn = 0; turn < 30 && battle.Phase == CombatPhase.ChoosingCoin; turn++)
                {
                    battle.SelectWithoutCoin();

                    int blockBefore = battle.Enemy.Block;
                    int healthBefore = battle.Enemy.Health;
                    AttackResult hit = battle.Attack(FirstUsableTag(battle));
                    if (hit != null)
                    {
                        int blocked = Mathf.Min(blockBefore, hit.Damage);
                        blockAbsorbs &= hit.Blocked == blocked && healthBefore - battle.Enemy.Health == hit.Damage - blocked;
                    }

                    int momentum = battle.Enemy.Momentum;
                    EnemyAction intent = battle.Enemy.NextAction;
                    battle.EnemyAttack();
                    EnemyAction action = battle.EnemyActions.Count > 0 ? battle.EnemyActions[0] : null;
                    if (action == null)
                    {
                        break;
                    }

                    intentMatches &= intent != null && intent.Pattern == action.Pattern && intent.Damage == action.Damage
                        && intent.BlockGained == action.BlockGained;
                    intentDescribed &= intent != null && !string.IsNullOrEmpty(intent.Description) && !intent.Description.Contains("{");

                    if (turn == 0)
                    {
                        firstIsStrike &= action.Pattern == "타격" && action.Damage == 6;
                    }

                    switch (action.Kind)
                    {
                        case PatternKind.Attack:
                            strikes++;
                            int expected = Mathf.RoundToInt(6 * (100 + momentum) / 100f);
                            strikesMatchMomentum &= action.Damage == expected;
                            blockFades &= battle.Enemy.Block == 0;
                            break;
                        case PatternKind.Defend:
                            guards++;
                            blockFades &= battle.Enemy.Block == 8;
                            break;
                        case PatternKind.Buff:
                            shouts++;
                            battleShouts++;
                            blockFades &= battle.Enemy.Block == 0;
                            break;
                    }

                    battle.Reroll();
                }

                momentumMatchesShouts &= battle.Enemy.Momentum == battleShouts * 50;
            }

            Report("검투사 · 첫 턴은 타격 6", firstIsStrike);
            Report("다음 행동 · 전투 시작부터 검투사의 타격 6 과 설명을 보인다", firstIntentIsStrike);
            Report("다음 행동 · 검투사가 보인 행동(패턴, 피해, 방어도)을 그대로 한다", intentMatches);
            Report("다음 행동 · 검투사의 행동마다 설명이 붙는다", intentDescribed);
            Report("검투사 · 타격 · 방어 · 함성을 모두 쓴다", strikes > 0 && guards > 0 && shouts > 0, strikes * 100 + guards * 10 + shouts);
            Report("검투사 · 타격이 가장 잦다", strikes > guards && strikes > shouts, strikes);
            Report("검투사 · 방어가 함성보다 잦다(30 : 20)", guards > shouts, guards);
            Report("검투사 · 함성마다 기세 50", momentumMatchesShouts);
            Report("검투사 · 타격은 6 × (1 + 기세%)", strikesMatchMomentum);
            Report("검투사 · 방어도가 영웅 공격을 먼저 막는다", blockAbsorbs);
            Report("검투사 · 방어도는 다음 행동 전에 사라진다", blockFades);
        }

        /// <summary>
        /// 장군 패턴 재설계(2026년 10월 29일). 방패병은 장군이 가진 수다.
        /// 턴 시작마다 1 소환(최대 3), 그 수로 기술을 고르고, 위력은 쓰는 순간의 수로 셈한다.
        /// 50 이상 피해는 방패병 1기를 써서 10 줄인다. 공격 한 번에 한 번이다.
        /// </summary>
        private static void CheckGeneral(CombatRuleConfig rules)
        {
            // 시작. 첫 턴 시작에 방패병 1.
            CombatSetup startSetup = ExampleSetup(5000, 5000);
            startSetup.PlayerMaxHealth = 5000;
            startSetup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);
            CombatBattle start = Begin(startSetup, rules);
            Report("장군 · 첫 턴 시작에 방패병 1", start.Enemy.Shieldmen, 1);
            Report("장군 · 방패병은 따로 서는 적이 아니다", start.Enemies.Count, 1);
            Report("장군 · 설명에 지금 방패병 수를 적는다", start.Enemy.Description.Contains("(현재 방패병 수:1)"));
            Report("장군 · 설명에 피해 감소를 적는다", start.Enemy.Description.Contains("(50) 이상 데미지를 받을 때마다 데미지를 (10) 감소"));

            // 피해 감소. 코인을 걸어 50 을 넘긴다.
            start.SelectCoin(0);
            int healthBefore = start.Enemy.Health;
            AttackResult cut = start.Attack(SymbolTagType.Neptune);
            int raw = cut != null ? cut.ValueSum * cut.Multiplier : -1;
            Report("방패병 · 50 이상이면 10 줄인다", cut != null ? cut.ShieldmanReduced : -1, 10);
            Report("방패병 · 줄인 피해로 때린다", cut != null ? cut.Damage : -1, raw - 10);
            Report("방패병 · 장군 체력은 줄인 만큼만 깎인다", healthBefore - start.Enemy.Health, raw - 10);
            Report("방패병 · 1기가 사라진다", start.Enemy.Shieldmen, 0);
            Report("방패병 · 설명의 수도 줄어든다", start.Enemy.Description.Contains("(현재 방패병 수:0)"));
            Report("방패병 · 대상을 고르지 않고 장군이 맞는다", cut != null && cut.Target == start.Enemy);

            // 방패병이 없으면 줄이지 않는다. 다음 턴 시작에 다시 1.
            start.EnemyAttack();
            start.Reroll();
            Report("장군 · 턴 시작마다 방패병 1 소환", start.Enemy.Shieldmen >= 1);

            CombatSetup emptySetup = ExampleSetup(5000, 5000);
            emptySetup.PlayerMaxHealth = 5000;
            emptySetup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);
            CombatBattle empty = Begin(emptySetup, rules);
            empty.Enemy.Shieldmen = 0;
            empty.SelectCoin(0);
            AttackResult bare = empty.Attack(SymbolTagType.Neptune);
            Report("방패병 · 없으면 줄이지 않는다", bare != null ? bare.ShieldmanReduced : -1, 0);

            // 50 아래는 줄이지 않는다.
            CombatRuleConfig high = Object.Instantiate(rules);
            high.General = new GeneralRule();
            high.General.GuardThreshold = 100000;
            CombatSetup lowSetup = ExampleSetup(5000, 5000);
            lowSetup.PlayerMaxHealth = 5000;
            lowSetup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);
            CombatBattle low = Begin(lowSetup, high);
            low.SelectCoin(0);
            AttackResult small = low.Attack(SymbolTagType.Neptune);
            Report("방패병 · 문턱 아래 피해는 줄이지 않는다", small != null ? small.ShieldmanReduced : -1, 0);
            Report("방패병 · 줄이지 않으면 그대로 남는다", low.Enemy.Shieldmen, 1);
            Object.DestroyImmediate(high);

            // 기술. 피해 감소를 1 로 두어 공격마다 방패병 하나를 쓰게 한다.
            // 그러면 고를 때 수와 쓸 때 수가 달라 위력이 쓰는 순간의 수를 따르는지 함께 본다.
            CombatRuleConfig every = Object.Instantiate(rules);
            every.General = new GeneralRule();
            every.General.GuardThreshold = 0;
            every.General.GuardReduction = 1;

            bool neverOverMax = true;
            bool weightsFollowCount = true;
            bool powerUsesActCount = true;
            bool formationSummonsTwo = true;
            bool generalIntentMatches = true;
            bool generalIntentDescribed = true;
            bool intentFollowsCut = true;
            int charges = 0;
            int formations = 0;
            int walls = 0;
            int allOuts = 0;

            for (int seed = 0; seed < 20; seed++)
            {
                CombatSetup setup = ExampleSetup(1000000, 1000000);
                setup.PlayerMaxHealth = 1000000;
                setup.Seed = 20261029 + seed * 7919;
                setup.Enemy = new CombatEnemy("장군", string.Empty, 1000000, 15, MonsterKind.General);
                CombatBattle battle = Begin(setup, every);

                for (int turn = 0; turn < 30 && battle.Phase == CombatPhase.ChoosingCoin; turn++)
                {
                    int chosenWith = battle.Enemy.Shieldmen;
                    neverOverMax &= chosenWith >= 1 && chosenWith <= 3;

                    EnemyAction shownBefore = battle.Enemy.NextAction;
                    battle.SelectWithoutCoin();
                    battle.Attack(FirstUsableTag(battle));
                    int actWith = battle.Enemy.Shieldmen;
                    powerUsesActCount &= actWith == chosenWith - 1;

                    // 영웅 공격에 방패병이 줄면 같은 기술의 예상 피해를 다시 셈한다.
                    EnemyAction intent = battle.Enemy.NextAction;
                    intentFollowsCut &= shownBefore != null && intent != null && shownBefore.Pattern == intent.Pattern
                        && (intent.Kind != PatternKind.Attack || intent.Damage <= shownBefore.Damage);

                    battle.EnemyAttack();
                    EnemyAction action = battle.EnemyActions.Count > 0 ? battle.EnemyActions[0] : null;
                    if (action == null)
                    {
                        break;
                    }

                    generalIntentMatches &= intent.Pattern == action.Pattern && intent.Damage == action.Damage;
                    generalIntentDescribed &= !string.IsNullOrEmpty(intent.Description) && !intent.Description.Contains("{");
                    generalIntentDescribed &= battle.Enemy.NextAction == null;

                    switch (action.Pattern)
                    {
                        case "돌격":
                            charges++;
                            weightsFollowCount &= chosenWith >= 1 && chosenWith <= 2;
                            powerUsesActCount &= action.Damage == 10 * (actWith + 1);
                            break;
                        case "진형 형성":
                            formations++;
                            weightsFollowCount &= chosenWith == 1;
                            formationSummonsTwo &= battle.Enemy.Shieldmen == Mathf.Min(3, actWith + 2);
                            break;
                        case "방패 진형":
                            walls++;
                            weightsFollowCount &= chosenWith >= 2;
                            powerUsesActCount &= action.Damage == 15 * (actWith + 1);
                            break;
                        case "총공세":
                            allOuts++;
                            weightsFollowCount &= chosenWith == 3;
                            powerUsesActCount &= action.Damage == 30 * actWith;
                            break;
                        default:
                            weightsFollowCount = false;
                            break;
                    }

                    battle.Reroll();
                }
            }

            Object.DestroyImmediate(every);

            Report("장군 · 방패병은 1 ~ 3기다", neverOverMax);
            Report("장군 · 네 기술을 모두 쓴다", charges > 0 && formations > 0 && walls > 0 && allOuts > 0,
                charges * 1000 + formations * 100 + walls * 10 + allOuts);
            Report("장군 · 기술은 고를 때의 방패병 수 가중치를 따른다", weightsFollowCount);
            Report("장군 · 위력은 쓰는 순간의 방패병 수로 셈한다", powerUsesActCount);
            Report("장군 · 진형 형성은 방패병 2 소환(최대 3)", formationSummonsTwo);
            Report("다음 행동 · 장군이 보인 기술과 피해를 그대로 쓴다", generalIntentMatches);
            Report("다음 행동 · 장군 기술마다 설명이 붙고, 쓴 뒤에는 비운다", generalIntentDescribed);
            Report("다음 행동 · 방패병이 줄면 같은 기술의 예상 피해를 다시 셈한다", intentFollowsCut);

            // 본체가 쓰러지면 이긴다.
            CombatSetup lastSetup = ExampleSetup(5000, 5000);
            lastSetup.PlayerMaxHealth = 5000;
            lastSetup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);
            CombatBattle last = Begin(lastSetup, rules);
            last.Enemy.Health = 1;
            last.SelectWithoutCoin();
            last.Attack(FirstUsableTag(last));
            Report("장군 · 쓰러지면 이긴다", last.Phase == CombatPhase.Won, (int)last.Phase);
        }

        /// <summary>규칙 설정의 적이 몬스터 기획서와 맞는지.</summary>
        private static void CheckMonsterRules(CombatRuleConfig rules)
        {
            CombatEnemy normal = rules.MakeEnemy(CombatEnemyKind.Normal, 1);
            // 이름은 2026년 10월 31일 팀원 몬스터 그림에 맞춰 덤, 더머, 더스트로 바꿨다. 패턴은 그대로다.
            Report("일반 방은 검투사 패턴의 덤", normal.Monster == MonsterKind.Gladiator && normal.Name == "덤");
            Report("일반 방 몬스터는 방 종류를 안다", normal.Kind == CombatEnemyKind.Normal);
            Report("검투사 체력 30", normal.MaxHealth, 30);

            CombatEnemy elite = rules.MakeEnemy(CombatEnemyKind.Elite, 1);
            Report("엘리트 방은 장군 패턴의 더머", elite.Monster == MonsterKind.General && elite.Name == "더머");
            Report("보스 방은 더스트", rules.MakeEnemy(CombatEnemyKind.Boss, 1).Name == "더스트");
            Report("장군 체력 80", elite.MaxHealth, 80);

            Report("스테이지가 오르면 패턴 피해에 더한다", rules.MakeEnemy(CombatEnemyKind.Normal, 2).AttackBonus, rules.EnemyAttackGrowthPerStage);
            Report("Plain 적은 공격력에 이미 더해 따로 더하지 않는다", rules.MakeEnemy(CombatEnemyKind.Boss, 2).AttackBonus, 0);

            CombatRuleConfig asset = UiSceneBuilder.LoadConfig<CombatRuleConfig>("Combat", "CombatRuleConfig");
            Report("설정 에셋도 일반 방이 검투사", asset != null && asset.MakeEnemy(CombatEnemyKind.Normal, 1).Monster == MonsterKind.Gladiator);
            Report("설정 에셋도 엘리트 방이 장군", asset != null && asset.MakeEnemy(CombatEnemyKind.Elite, 1).Monster == MonsterKind.General);
        }

        /// <summary>유물 설정과 물건 목록이 시트의 열 개로 맞춰졌는지.</summary>
        private static void CheckRelicCatalog()
        {
            RelicConfig relics = UiSceneBuilder.LoadConfig<RelicConfig>("Combat", "RelicConfig");
            RunCatalogConfig catalog = UiSceneBuilder.LoadConfig<RunCatalogConfig>("Flow", "RunCatalogConfig");

            Report("유물 설정이 있다", relics != null);
            Report("물건 목록이 있다", catalog != null);
            if (relics == null || catalog == null)
            {
                return;
            }

            Report("유물 설정은 시트의 열 개", relics.Relics.Count, 10);
            // 물건 목록에는 시트 열 개 뒤에 그림만 있는 세계관 유물 스물이 더 있다(2026년 10월 26일 원재).
            Report("물건 목록의 유물은 시트 열과 세계관 스물로 서른", catalog.Relics.Count, 30);

            bool matches = true;
            for (int i = 0; i < relics.Relics.Count; i++)
            {
                ItemDefinition item;
                matches &= catalog.TryGetItem(relics.Relics[i].Id, out item)
                    && item.DisplayName == relics.Relics[i].DisplayName
                    && (int)item.Rarity == relics.Relics[i].Rarity - 1;
            }

            Report("물건 목록의 유물 이름과 희귀도가 시트와 같다", matches);

            ItemDefinition old;
            Report("임시 유물(낡은 책)은 빠졌다", !catalog.TryGetItem("relic_book", out old));
            Report("모든 유물에 효과가 있다", AllHaveEffects(relics));
        }

        private static bool AllHaveEffects(RelicConfig relics)
        {
            for (int i = 0; i < relics.Relics.Count; i++)
            {
                if (relics.Relics[i].Effects == null || relics.Relics[i].Effects.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static CombatBattle Begin(CombatSetup setup, CombatRuleConfig rules)
        {
            CombatBattle battle = new CombatBattle(setup, rules);
            battle.Begin();
            return battle;
        }

        private static CombatSetup WithRelic(CombatSetup setup, int number, int stack)
        {
            List<RelicDefinition> sheet = RelicConfig.SheetRelics();
            string id = RelicConfig.IdFor(number);
            for (int i = 0; i < sheet.Count; i++)
            {
                if (sheet[i].Id == id)
                {
                    setup.Relics.Add(new CombatRelic(id, sheet[i].DisplayName, sheet[i], stack));
                }
            }

            return setup;
        }

        private static ReelSymbol Find(CombatBattle battle, string id)
        {
            for (int i = 0; i < battle.Reels.Count; i++)
            {
                if (battle.Reels[i].Id == id)
                {
                    return battle.Reels[i];
                }
            }

            return new ReelSymbol();
        }

        // ---- 화면 ----

        private static void CheckGameScene()
        {
            string path = UiSceneBuilder.SceneFolder + "/게임.unity";
            if (!System.IO.File.Exists(path))
            {
                Report("게임 씬이 있다", false);
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            GameFlowController flow = Object.FindAnyObjectByType<GameFlowController>(FindObjectsInactive.Include);
            CombatEntry entry = Object.FindAnyObjectByType<CombatEntry>(FindObjectsInactive.Include);
            CombatScreenController screen = Object.FindAnyObjectByType<CombatScreenController>(FindObjectsInactive.Include);

            Report("게임 씬에 전투 창구가 있다", entry != null);
            Report("게임 씬에 전투 화면이 있다", screen != null);
            Report("게임 씬에 대역이 남아 있지 않다", Object.FindAnyObjectByType<PlaceholderCombatEntry>(FindObjectsInactive.Include) == null);

            if (flow == null || entry == null || screen == null)
            {
                return;
            }

            Report("흐름이 진짜 전투에 물려 있다", GetField<MonoBehaviour>(flow, "_combatEntryBehaviour") == entry);
            Report("창구가 전투 화면에 물려 있다", entry.Screen == screen);

            // 설정의 소리 크기가 레이저 발사음까지 간다(2026년 10월 31일 원재).
            entry.SetSoundVolume(0.25f);
            Report("창구가 넘긴 소리 크기가 전투 화면에 간다",
                screen.View != null && Mathf.Approximately(screen.View.SoundVolume, 0.25f));
            entry.SetSoundVolume(1f);
            Report("창구에 규칙이 있다", GetField<CombatRuleConfig>(entry, "_rules") != null);
            Report("창구에 물건 목록이 있다", GetField<RunCatalogConfig>(entry, "_catalog") != null);
            Report("전투 화면은 꺼진 채로 시작한다", !screen.gameObject.activeSelf);

            CombatScreenView view = screen.View;
            Report("릴이 여섯이다", view != null ? view.Reels.Count : -1, 6);
            Report("태그 칸이 열이다", view != null && view.TagDial != null ? view.TagDial.Sectors.Count : -1, TagDialView.SectorCount);
            Report("코인 자리가 셋이다", view != null && view.CoinDial != null ? view.CoinDial.SlotCount : -1, 3);
            CheckSpinOverlay(view);
            CheckEnemyInfo(view);
            CheckHitEffect(view);

            PlayRun(entry, screen);
        }

        /// <summary>
        /// 런 하나로 전투를 끝까지 눌러 본다. 매 턴 쓸 수 있는 첫 코인과 첫 태그를 고른다.
        /// 체력이 런에 적히고, 끝나면 결과가 흐름으로 돌아오는지 본다.
        /// </summary>
        private static void PlayRun(CombatEntry entry, CombatScreenController screen)
        {
            RunCatalogConfig catalogConfig = GetField<RunCatalogConfig>(entry, "_catalog");
            RunSaveData run = RunSaveData.StartNew(1, 20261025, 99);
            RunCatalog catalog = new RunCatalog(catalogConfig);
            catalog.Attach(run);

            RunContext context = new RunContext(run, catalog);
            context.GiveStartingItems(
                catalogConfig.StartingSymbolIds, catalogConfig.StartingCoinIds, catalogConfig.StartingRelicIds, 0);

            MapNode node = new MapNode();
            node.Id = 3;
            node.Stage = 1;
            node.RoomType = RoomType.Normal;

            // 12개의 금화를 들고 들어간다. 쌓인 횟수가 런에 다시 적히는지 본다.
            run.Owned.AddRelic(RelicConfig.IdFor(12003));

            // 임시 아이템 효과(2026년 11월 1일 원재). 켜면 시트에 없던 유물과 코인도 효과를 갖고, 끄면 예전처럼 없다.
            CheckTemporaryItemEffects(entry, context, node);

            CombatSetup setup = entry.MakeSetup(context, node);
            Report("유물이 효과와 함께 넘어간다", setup.Relics.Count == 1 && setup.Relics[0].Definition != null);
            Report("모든 문양이 망치와 끌의 풀로 넘어간다", setup.SymbolPool.Count, catalogConfig.Symbols.Count);
            Report("덱이 런의 문양 수만큼이다", setup.Deck.Count, run.Owned.TotalSymbolCount);
            Report("문양 가치는 태그 둘의 값을 더한 것", setup.Deck.Count > 0 && setup.Deck[0].Value > 0);
            Report("코인이 런의 코인 수만큼이다", setup.Coins.Count, run.Owned.CoinIds.Count);
            Report("체력이 런의 체력이다", setup.PlayerHealth, 99);

            bool finished = false;
            CombatResult result = new CombatResult();

            entry.Enter(context, node, CombatOptions.None, r =>
            {
                finished = true;
                result = r;
            });

            Report("전투에 들어가면 화면이 켜진다", screen.gameObject.activeSelf);
            Report("들어가면 코인을 고른다", screen.Battle != null && screen.Battle.Phase == CombatPhase.ChoosingCoin);
            Report("꺼낸 코인은 마우스를 받는다", CoinReceivesPointer(screen, 0));

            // 몬스터 그림(2026년 10월 31일 팀원 `c887667`). 일반 방은 덤이고 그림이 일러스트 자리에 그려진다.
            CombatVisualConfig enemyVisual = screen.View != null ? screen.View.Visual : null;
            Report("일반 방 몬스터 이름은 덤", screen.Battle != null && screen.Battle.Enemy.Name == "덤");
            Report("몬스터 그림 셋이 표시 설정에 물려 있다", enemyVisual != null
                && enemyVisual.NormalEnemyArt != null && enemyVisual.EliteEnemyArt != null && enemyVisual.BossEnemyArt != null);
            Report("일반 방 몬스터 자리에 덤 그림을 그린다", enemyVisual != null && screen.View.EnemySprite != null
                && screen.View.EnemySprite == enemyVisual.NormalEnemyArt);
            // 9조각 늘여 그리기면 비율이 무시돼 세로로 늘어났다(2026년 11월 1일 원재).
            Report("몬스터 그림이 늘지 않고 제 비율로 그려진다", screen.View != null && screen.View.EnemyArtKeepsAspect);
            CheckEnemySpine(entry, screen, enemyVisual);
            CheckDamageAboveEffects(screen);

            int healthBefore = context.Health;
            int turns = 0;

            while (!finished && turns < 100 && screen.Battle != null)
            {
                CombatBattle battle = screen.Battle;

                if (battle.CanUseOffered(0))
                {
                    screen.ChooseCoin(0);
                }
                else
                {
                    screen.ChooseWithoutCoin();
                }

                if (turns == 0)
                {
                    CheckTagPhaseHighlights(screen, battle);
                }

                screen.ChooseTag(FirstUsableTag(battle));
                turns++;
            }

            Report("전투가 끝난다", finished, turns);
            Report("런의 체력이 전투에서 깎인 만큼 줄었다",
                screen.Battle != null && context.Health == Mathf.Max(0, screen.Battle.PlayerHealth),
                context.Health);
            Report("이기면 이긴 결과가 돌아온다", !result.Won || (screen.Battle != null && screen.Battle.Phase == CombatPhase.Won));
            Report("이긴 결과의 가장 큰 피해", !result.Won || result.BiggestHit > 0, result.BiggestHit);
            Report("체력이 줄었다", context.Health < healthBefore || result.Won, context.Health);
            Report("끝나면 전투 화면이 닫힌다", !screen.gameObject.activeSelf);

            int stack = run.Owned.Relics.Count > 0 ? run.Owned.Relics[0].Stack : -1;
            int battleStack = screen.Battle != null && screen.Battle.Relics.Count > 0 ? screen.Battle.Relics[0].Stack : -2;
            Report("12개의 금화가 쌓인 횟수가 런에 적힌다", stack == battleStack, stack);

            entry.Leave();

            // 새 런을 시작하면 앞 런의 전투를 모두 비운다. 다음 전투를 열 때 앞 런의 릴과 코인이 잠깐 보였다(2026년 10월 26일 원재).
            entry.ResetForNewRun();
            Report("새 런이면 전투 진행을 비운다", screen.Battle == null);
            Report("새 런이면 릴 그림을 비운다", !AnyReelIconShown(screen));
            Report("새 런이면 전투 화면이 닫혀 있다", !screen.gameObject.activeSelf);
        }

        /// <summary>
        /// 태그를 고르는 동안의 강조 셋(2026년 10월 31일 원재).
        /// 태그에 마우스를 올리면 그 태그 문양 릴의 무채색 틀에 태그 색을 입힌다. 바깥 사각 테두리는 없다.
        /// 마우스를 올린 칸은 제 회색 바퀴 선을 감추고 붉은 테두리만 보인다.
        /// 가운데 코인에 마우스를 올리면 코인 둘레가 붉게 빛난다.
        /// </summary>
        private static void CheckTagPhaseHighlights(CombatScreenController screen, CombatBattle battle)
        {
            CombatScreenView view = screen.View;
            TagDialView dial = view != null ? view.TagDial : null;
            Report("태그 고르기 단계다", battle.Phase == CombatPhase.ChoosingTag);
            if (dial == null || battle.Phase != CombatPhase.ChoosingTag)
            {
                return;
            }

            Report("릴에 바깥 사각 강조 테두리가 없다", view.Reels.Count > 0 && view.Reels[0].transform.Find("Highlight") == null);

            SymbolTagType tag = FirstUsableTag(battle);
            int index = TagDialView.GetSectorIndex(tag);
            TagSectorView sector = dial.Sectors[index];
            IList<int> reels = battle.FindReels(tag);
            Color tagColor = view.GetTagVisual(tag).BarColor;

            sector.OnPointerEnter(null);
            InvokePrivate(screen, "HandleTagHover", tag, true);

            bool tinted = reels.Count > 0;
            bool othersPlain = true;
            for (int i = 0; i < view.Reels.Count; i++)
            {
                Color frame = view.Reels[i].FrameColor;
                if (reels.Contains(i))
                {
                    tinted &= Mathf.Approximately(frame.r, tagColor.r) && Mathf.Approximately(frame.g, tagColor.g)
                        && Mathf.Approximately(frame.b, tagColor.b);
                }
                else
                {
                    othersPlain &= frame == view.Reels[i].IdleFrameColor && !view.Reels[i].GlowVisible && !view.Reels[i].ShineVisible;
                }
            }

            Report("태그에 마우스를 올리면 그 태그 문양 릴의 틀이 태그 색이다", tinted, reels.Count);
            Report("다른 릴의 틀은 기본 틀 색 그대로이고 빛이 없다", othersPlain);

            // 기본 틀은 흰색보다 어둡고, 강조한 틀 둘레의 빛은 태그 색으로 밝기가 오르내린다(2026년 11월 1일 원재).
            CombatVisualConfig frameVisual = view.Visual;
            Report("기본 릴 틀은 흰색보다 어둡다", frameVisual != null && frameVisual.ReelFrameIdleColor.r < 1f
                && view.Reels[0].IdleFrameColor == frameVisual.ReelFrameIdleColor);
            if (reels.Count > 0 && frameVisual != null)
            {
                CombatReelView lit = view.Reels[reels[0]];
                Color glowNow = lit.GlowColor;
                Color bright = frameVisual.GetReelGlowColor(tagColor);
                Report("강조한 릴 틀 둘레에 태그 색을 밝힌 빛이 켜진다", lit.GlowVisible
                    && Mathf.Approximately(glowNow.r, bright.r) && Mathf.Approximately(glowNow.g, bright.g)
                    && Mathf.Approximately(glowNow.b, bright.b));
                Report("강조 빛 색은 태그 색보다 밝다", bright.r + bright.g + bright.b > tagColor.r + tagColor.g + tagColor.b);
                Report("강조한 릴 틀 위에 밝은 덧칠이 켜진다", lit.ShineVisible);
                float brightest = glowNow.a;
                lit.AdvanceGlow(frameVisual.ReelGlowPulseSeconds * 0.5f);
                float dimmest = lit.GlowColor.a;
                Report("강조 빛이 밝아졌다 어두워지며 반짝인다",
                    Mathf.Approximately(brightest, frameVisual.ReelGlowMaxAlpha)
                    && Mathf.Approximately(dimmest, frameVisual.ReelGlowMinAlpha), (int)(dimmest * 100f));
            }
            Report("마우스를 올린 칸은 붉은 테두리가 켜진다", sector.OutlineVisible);
            Report("마우스를 올린 칸은 회색 바퀴 선을 감춘다", !sector.LineVisible);

            TagSectorView neighbour = dial.Sectors[(index + 1) % dial.Sectors.Count];
            Report("이웃 칸의 회색 바퀴 선은 그대로 보인다", neighbour.LineVisible);

            sector.OnPointerExit(null);
            InvokePrivate(screen, "HandleTagHover", tag, false);
            Report("마우스가 벗어나면 회색 바퀴 선이 돌아온다", sector.LineVisible && !sector.OutlineVisible);
            Report("마우스가 벗어나면 릴 틀이 기본 색으로 돌아오고 빛이 꺼진다", reels.Count == 0
                || (view.Reels[reels[0]].FrameColor == view.Reels[reels[0]].IdleFrameColor && !view.Reels[reels[0]].GlowVisible
                    && !view.Reels[reels[0]].ShineVisible));
            Report("릴 틀 여섯이 모두 같은 그림이다", SameReelFrames(view));
            // 같은 그림인지만 보면 엉뚱한 그림 여섯도 통과한다. 그림 이름까지 본다(2026년 11월 1일 코덱스 검토).
            Report("릴 틀 그림은 회색 릴틀_3, 빛은 릴틀_빛, 덧칠은 릴틀_흰이다", ReelArtNamed(view));

            InvokePrivate(dial, "HandleCenterHover", true);
            Report("가운데 코인에 마우스를 올리면 붉게 빛난다", dial.CenterGlowVisible);
            InvokePrivate(dial, "HandleCenterHover", false);
            Report("가운데 코인에서 마우스가 벗어나면 빛이 꺼진다", !dial.CenterGlowVisible);
        }

        /// <summary>
        /// 임시 아이템 효과를 켜고 끄며 전투 준비물을 만들어 본다. 녹슨 단검은 화성 가치 +2, 늑대 동전은 소모 5 · 가중치 1 이다.
        /// 런에 넣은 것은 다시 뺀다.
        /// </summary>
        private static void CheckTemporaryItemEffects(CombatEntry entry, RunContext context, MapNode node)
        {
            RunSaveData run = context.Run;
            int coinsBefore = run.Owned.CoinIds.Count;
            run.Owned.AddRelic("relic_rusted_gladius");
            run.Owned.CoinIds.Add("coin_bronze_wolf");

            entry.SetTemporaryItemEffects(true);
            CombatSetup on = entry.MakeSetup(context, node);
            CombatRelic gladius = on.Relics.Find(r => r.Id == "relic_rusted_gladius");
            int wolfIndex = on.Coins.FindIndex(c => c.Id == "coin_bronze_wolf");
            CombatCoin wolf = wolfIndex >= 0 ? on.Coins[wolfIndex] : default(CombatCoin);
            Report("임시 효과를 켜면 녹슨 단검이 화성 가치 +2 효과를 갖는다", gladius != null && gladius.Definition != null
                && gladius.Definition.Effects.Count == 1 && gladius.Definition.Effects[0].Kind == RelicEffectKind.TagValueAdd
                && gladius.Definition.Effects[0].Amount == 2 && gladius.Definition.Effects[0].Targets(SymbolTagType.Mars));
            Report("임시 효과를 켜면 늑대 동전은 소모 5 · 가중치 1", wolfIndex >= 0 && wolf.HealthCost == 5 && wolf.Weight == 1);

            entry.SetTemporaryItemEffects(false);
            CombatSetup off = entry.MakeSetup(context, node);
            CombatRelic plain = off.Relics.Find(r => r.Id == "relic_rusted_gladius");
            int plainIndex = off.Coins.FindIndex(c => c.Id == "coin_bronze_wolf");
            CombatRuleConfig rules = GetField<CombatRuleConfig>(entry, "_rules");
            Report("임시 효과를 끄면 녹슨 단검은 효과가 없다", plain != null && plain.Definition == null);
            Report("임시 효과를 끄면 늑대 동전은 기본 소모값이다",
                plainIndex >= 0 && rules != null && off.Coins[plainIndex].HealthCost == rules.DefaultCoinCost);

            entry.SetTemporaryItemEffects(true);
            run.Owned.RemoveRelic("relic_rusted_gladius");
            run.Owned.CoinIds.RemoveAt(run.Owned.CoinIds.Count - 1);
            Report("임시 효과 검사가 넣은 물건을 다시 뺀다", run.Owned.CoinIds.Count == coinsBefore
                && !run.Owned.Relics.Exists(r => r.RelicId == "relic_rusted_gladius"));
        }

        /// <summary>릴 틀이 모두 같은 한 장의 그림인지. 여섯 색 그림을 2026년 11월 1일 회색 한 장으로 모았다.</summary>
        private static bool SameReelFrames(CombatScreenView view)
        {
            if (view.Reels.Count == 0 || view.Reels[0].FrameSprite == null)
            {
                return false;
            }

            for (int i = 1; i < view.Reels.Count; i++)
            {
                if (view.Reels[i].FrameSprite != view.Reels[0].FrameSprite)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>릴마다 틀, 빛, 덧칠에 정한 그림이 물려 있는지.</summary>
        private static bool ReelArtNamed(CombatScreenView view)
        {
            if (view.Reels.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < view.Reels.Count; i++)
            {
                CombatReelView reel = view.Reels[i];
                if (!HasSprite(reel.Frame, "릴틀_3") || !HasSprite(reel.Glow, "릴틀_빛") || !HasSprite(reel.Shine, "릴틀_흰"))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSprite(Image image, string spriteName)
        {
            return image != null && image.sprite != null && image.sprite.name == spriteName;
        }

        private static void InvokePrivate(object target, string method, params object[] arguments)
        {
            System.Reflection.MethodInfo info = target.GetType().GetMethod(
                method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (info == null)
            {
                Report("처리기가 있다: " + target.GetType().Name + "." + method, false);
                return;
            }

            info.Invoke(target, arguments);
        }

        /// <summary>릴 가운데 그림이 켜진 칸이 있는지. 새 런으로 비웠으면 하나도 없어야 한다.</summary>
        private static bool AnyReelIconShown(CombatScreenController screen)
        {
            if (screen.View == null)
            {
                return false;
            }

            for (int i = 0; i < screen.View.Reels.Count; i++)
            {
                Transform icon = screen.View.Reels[i].transform.Find("Content/Icon");
                UnityEngine.UI.Image image = icon != null ? icon.GetComponent<UnityEngine.UI.Image>() : null;
                if (image != null && image.enabled)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 그 자리의 코인 아래에 켜져 있고 마우스를 받는 그림이 있는지.
        /// 버튼의 클릭 함수를 직접 부르는 검사로는 못 잡는다. 그림이 있는 코인의 바탕 원을 꺼서
        /// 실제로는 누를 수도 올릴 수도 없었던 적이 있다(2026년 10월 25일).
        /// </summary>
        private static bool CoinReceivesPointer(CombatScreenController screen, int index)
        {
            if (screen.View == null || screen.View.CoinDial == null)
            {
                return false;
            }

            Transform coin = screen.View.CoinDial.transform.Find("Coin" + index);
            if (coin == null)
            {
                return false;
            }

            UnityEngine.UI.Graphic[] graphics = coin.GetComponentsInChildren<UnityEngine.UI.Graphic>(false);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i].enabled && graphics[i].raycastTarget)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 회전 연출용 큰 슬롯(2026년 10월 27일). 꺼진 채로 있고, 6열 × 5행이며,
        /// 멈추면 가운데 행에 결과가 온다. 재굴림처럼 몇 열만 돌 때 서 있는 열도 가운데가 결과다.
        /// </summary>
        /// <summary>공격 레이저와 발사음이 전투 화면에 물려 있는지. 레이저는 꺼진 채로 시작한다.</summary>
        private static void CheckHitEffect(CombatScreenView view)
        {
            Image laser = view != null ? GetField<Image>(view, "_laser") : null;
            AudioSource audio = view != null ? GetField<AudioSource>(view, "_sfxAudio") : null;
            Report("타격 · 레이저 칸에 LaserBeam 그림이 있다", laser != null && laser.sprite != null && laser.sprite.name == "LaserBeam");
            Report("타격 · 레이저는 꺼진 채로 시작한다", laser != null && !laser.gameObject.activeSelf);
            Report("타격 · 효과음 칸이 있다", audio != null);
            Report("타격 · 효과음은 저절로 나지 않는다", audio != null && !audio.playOnAwake && audio.clip == null);
            AudioClip laserClip = view != null ? GetField<AudioClip>(view, "_laserClip") : null;
            Report("타격 · 레이저 발사음 LaserSound 가 있다", laserClip != null && laserClip.name == "LaserSound");
        }

        /// <summary>몬스터 정보 칸이 몬스터 그림만큼 크고, 넘칠 글은 글자를 줄여 칸 안에 담는지.</summary>
        private static void CheckEnemyInfo(CombatScreenView view)
        {
            CombatLayoutConfig layout = UiSceneBuilder.LoadConfig<CombatLayoutConfig>("Combat", "CombatLayoutConfig");
            Report("정보 칸 · 몬스터 그림과 크기가 같다", layout != null && layout.EnemyInfoSize == layout.EnemyArtSize);

            TMP_Text info = view != null ? GetField<TMP_Text>(view, "_enemyInfoLabel") : null;
            Report("정보 칸 · 글 칸이 있다", info != null);
            if (info == null)
            {
                return;
            }

            view.ApplyStyle();
            Report("정보 칸 · 넘칠 것 같으면 글자를 줄인다", info.enableAutoSizing && info.fontSizeMin < info.fontSizeMax);

            // 장군의 가장 긴 설명을 넣어 본다.
            CombatSetup setup = ExampleSetup(99, 500);
            setup.Enemy = new CombatEnemy("장군", string.Empty, 500, 15, MonsterKind.General);
            CombatBattle battle = Begin(setup, UiSceneBuilder.LoadConfig<CombatRuleConfig>("Combat", "CombatRuleConfig"));
            battle.Enemy.Block = 8;
            battle.Enemy.Momentum = 50;
            view.ShowEnemy(battle.Enemy);
            info.ForceMeshUpdate(true, true);
            Vector2 room = info.rectTransform.rect.size;
            Report("정보 칸 · 장군 설명이 칸 안에 들어간다", info.textBounds.size.y <= room.y + 1f,
                Mathf.RoundToInt(info.textBounds.size.y));
        }

        private static void CheckSpinOverlay(CombatScreenView view)
        {
            SlotSpinOverlay overlay = view != null ? view.SpinOverlay : null;
            Report("큰 슬롯이 있다", overlay != null);
            if (overlay == null)
            {
                return;
            }

            Report("큰 슬롯은 꺼진 채로 시작한다", !overlay.gameObject.activeSelf);
            Report("큰 슬롯은 릴 수만큼 열이 있다", overlay.ColumnCount, 6);

            CombatSetup setup = ExampleSetup(99, 500);
            List<ReelSymbol> result = new List<ReelSymbol>(setup.Deck);

            overlay.Prepare(result, new List<int> { 0, 1, 2, 3, 4, 5 }, setup.Deck, null);
            overlay.SnapToEnd();

            bool middle = true;
            for (int i = 0; i < result.Count; i++)
            {
                middle &= overlay.GetShown(i, SlotSpinOverlay.ResultRow).Id == result[i].Id;
            }

            Report("큰 슬롯이 멈추면 가운데 행이 결과다", middle);

            overlay.Prepare(result, new List<int> { 1, 4 }, setup.Deck, null);
            Report("재굴림 때 서 있는 열은 처음부터 가운데가 결과다",
                overlay.GetShown(0, SlotSpinOverlay.ResultRow).Id == result[0].Id
                && overlay.GetShown(5, SlotSpinOverlay.ResultRow).Id == result[5].Id);
            overlay.SnapToEnd();
            Report("재굴림 때 도는 열도 멈추면 가운데가 결과다",
                overlay.GetShown(1, SlotSpinOverlay.ResultRow).Id == result[1].Id
                && overlay.GetShown(4, SlotSpinOverlay.ResultRow).Id == result[4].Id);

            overlay.Hide();
            Report("걷으면 꺼진다", !overlay.gameObject.activeSelf);

            // 넘어갔다 돌아오기(2026년 10월 27일). 20 칸을 1.6 초 돌고 0.35 칸 지나쳤다가 0.18 초에 되돌아온다.
            float peak = SlotSpinOverlay.PositionAt(1.6f, 1.6f, 0.18f, 20f, 0.35f, 3.2f);
            float end = SlotSpinOverlay.PositionAt(1.78f, 1.6f, 0.18f, 20f, 0.35f, 3.2f);
            float mid = SlotSpinOverlay.PositionAt(1.69f, 1.6f, 0.18f, 20f, 0.35f, 3.2f);
            Report("굴러가기 끝에서 결과 칸을 지나친다", Mathf.Abs(peak - 20.35f) < 0.001f);
            Report("되돌아오는 중에는 지나친 자리와 결과 칸 사이다", mid < peak && mid > 20f);
            Report("되돌아와 결과 칸에 선다", Mathf.Abs(end - 20f) < 0.001f);
            Report("처음에는 빠르다(첫 10% 시간에 4분의 1 넘게 간다)", SlotSpinOverlay.PositionAt(0.16f, 1.6f, 0.18f, 20f, 0.35f, 3.2f) > 5f);
        }

        /// <summary>
        /// 한글 글꼴 에셋의 GUID 가 저장소 값(`PrepareTextResources.FontGuid`)과 같은지.
        /// 다르면 이 컴퓨터에서 지은 씬이 다른 컴퓨터에서 없는 글꼴을 가리킨다.
        /// 2026년 10월 27일 맞추기 전에 만든 글꼴로 씬을 지어 올려 팀원 컴퓨터에서 글자가 깨졌다.
        /// </summary>
        private static void CheckFontPinned()
        {
            TMPro.TMP_FontAsset font = PrepareTextResources.LoadKoreanFont();
            Report("한글 글꼴 에셋이 있다", font != null);
            if (font == null)
            {
                return;
            }

            string guid = AssetDatabase.GetAssetPath(font);
            guid = AssetDatabase.AssetPathToGUID(guid);
            Report("한글 글꼴 GUID 가 저장소 값과 같다(다르면 글자 준비하기를 다시 한다)", guid == PrepareTextResources.FontGuid);
        }

        // ---- 도우미 ----

        /// <summary>
        /// 1.1.a 작동 예시의 덱. 불사조, 간헐천, 용광로, 섬, 난파선에 하나를 더해 여섯이다.
        /// 덱이 릴 수와 같아 여섯이 모두 릴에 나온다. 코인은 금화 하나다.
        /// </summary>
        private static CombatSetup ExampleSetup(int playerHealth, int enemyHealth)
        {
            CombatSetup setup = new CombatSetup();
            setup.Deck.Add(new ReelSymbol("Pheonix", "불사조", SymbolTagType.Mars, SymbolTagType.Earth, 4));
            setup.Deck.Add(new ReelSymbol("Geyser", "간헐천", SymbolTagType.Mercury, SymbolTagType.Mars, 3));
            setup.Deck.Add(new ReelSymbol("Furnace", "용광로", SymbolTagType.Mars, SymbolTagType.Venus, 3));
            setup.Deck.Add(new ReelSymbol("Island", "섬", SymbolTagType.Neptune, SymbolTagType.Saturn, 7));
            setup.Deck.Add(new ReelSymbol("Shipwerck", "난파선", SymbolTagType.Neptune, SymbolTagType.Pluto, 9));
            setup.Deck.Add(new ReelSymbol("Quicksilver", "수은", SymbolTagType.Mercury, SymbolTagType.Venus, 2));

            setup.Coins.Add(new CombatCoin("coin_gold", "금화", 10, 5, string.Empty));

            setup.Enemy = new CombatEnemy("시험 적", string.Empty, enemyHealth, 6);
            setup.PlayerHealth = playerHealth;
            setup.PlayerMaxHealth = 99;
            setup.Seed = 20261025;
            return setup;
        }

        private static SymbolTagType FirstUsableTag(CombatBattle battle)
        {
            for (int i = 0; i < 9; i++)
            {
                SymbolTagType tag = (SymbolTagType)i;
                if (battle.FindReels(tag).Count > 0)
                {
                    return tag;
                }
            }

            return SymbolTagType.Mercury;
        }

        private static int Distinct(IReadOnlyList<ReelSymbol> reels)
        {
            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < reels.Count; i++)
            {
                ids.Add(reels[i].Id);
            }

            return ids.Count;
        }

        private static List<string> Ids(IReadOnlyList<ReelSymbol> reels)
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < reels.Count; i++)
            {
                ids.Add(reels[i].Id);
            }

            return ids;
        }

        /// <summary>
        /// 몬스터 스파인(2026년 11월 13일 원재). 덤과 더머는 그림 대신 스파인이 움직이고, 더스트는 그림을 그린다.
        /// 엘리트와 보스는 몬스터 자리만 바꿔 보고 지금 전투의 몬스터로 되돌린다.
        /// </summary>
        private static void CheckEnemySpine(CombatEntry entry, CombatScreenController screen, CombatVisualConfig visual)
        {
            CombatScreenView view = screen.View;
            Report("몬스터 스파인 둘이 표시 설정에 물려 있다", visual != null
                && visual.NormalEnemySpine is SkeletonDataAsset && visual.EliteEnemySpine is SkeletonDataAsset);
            if (view == null || visual == null || view.EnemySpine == null)
            {
                Report("전투 화면에 몬스터 스파인 자리가 있다", false);
                return;
            }

            Image art = GetField<Image>(view, "_enemyArt");
            Report("일반 방은 덤 스파인이 켜진다", view.EnemySpineData != null
                && view.EnemySpineData == visual.NormalEnemySpine && view.EnemySpine.gameObject.activeSelf);
            Report("덤 스파인이 설정의 애니메이션을 되풀이한다", view.EnemySpineAnimationName == visual.EnemySpineAnimation);
            Report("스파인이 켜지면 그림 칸은 비운다", art != null && art.color.a == 0f);
            Report("스파인은 마우스를 받지 않는다", !view.EnemySpine.raycastTarget);
            // 스파인 실행기 기본 설정이 sRGB 를 꺼 몬스터가 밝고 흐리게 보였다(2026년 11월 13일 원재).
            Report("스파인 그림은 다른 그림처럼 sRGB 로 읽는다",
                SpineTextureIsSrgb(visual.NormalEnemySpine as SkeletonDataAsset) && SpineTextureIsSrgb(visual.EliteEnemySpine as SkeletonDataAsset));
            CapturePreviewScenes.CaptureOpenScene("게임 전투 덤 스파인");

            CombatRuleConfig rules = GetField<CombatRuleConfig>(entry, "_rules");
            if (rules == null || screen.Battle == null)
            {
                return;
            }

            view.ShowEnemy(rules.MakeEnemy(CombatEnemyKind.Elite, 1));
            Report("엘리트 방은 더머 스파인이 켜진다", view.EnemySpineData == visual.EliteEnemySpine
                && view.EnemySpine.gameObject.activeSelf && view.EnemySpineAnimationName == visual.EnemySpineAnimation);
            CapturePreviewScenes.CaptureOpenScene("게임 전투 더머 스파인");

            view.ShowEnemy(rules.MakeEnemy(CombatEnemyKind.Boss, 1));
            Report("보스 방은 스파인을 끄고 더스트 그림을 그린다", view.EnemySpineData == null
                && !view.EnemySpine.gameObject.activeSelf && view.EnemySprite == visual.BossEnemyArt
                && art != null && art.color.a > 0f);

            view.ShowEnemy(screen.Battle.Enemy);
        }

        /// <summary>
        /// 몬스터 피해 숫자가 태그 공격 파티클 위에 그려지는지(2026년 11월 13일 원재).
        /// 파티클은 캔버스 차례 + 설정의 차례 + 프리팹의 차례로 그려진다. 프리팹의 차례는 지금 가장 큰 것이 4 다.
        /// </summary>
        private static void CheckDamageAboveEffects(CombatScreenController screen)
        {
            CombatScreenView view = screen.View;
            SlotHero.TagAttack.UI.TagAttackFxPlayer fx = view != null
                ? GetField<SlotHero.TagAttack.UI.TagAttackFxPlayer>(view, "_tagAttackFx")
                : null;
            SlotHero.TagAttack.TagAttackFxConfig fxConfig = fx != null
                ? GetField<SlotHero.TagAttack.TagAttackFxConfig>(fx, "_config")
                : null;
            DamageNumberView number = view != null ? GetField<DamageNumberView>(view, "_enemyDamageNumber") : null;
            if (view == null || fxConfig == null || number == null)
            {
                Report("피해 숫자와 태그 공격 이펙트가 전투 화면에 있다", false);
                return;
            }

            MethodInfo keep = typeof(CombatScreenView).GetMethod("KeepAboveEffects", BindingFlags.Instance | BindingFlags.NonPublic);
            if (keep != null)
            {
                keep.Invoke(view, new object[] { number.gameObject });
            }

            // 꺼진 피해 숫자의 제 캔버스는 자신을 바깥 캔버스로 알려 주므로 부모에서 찾는다.
            Canvas root = number.transform.parent.GetComponentInParent<Canvas>(true).rootCanvas;
            int particleTop = root.sortingOrder + fxConfig.SortingOffset + MaxEffectSortingOrder(fxConfig);
            Report("몬스터 피해 숫자가 태그 공격 이펙트보다 위에 그려진다", view.EnemyDamageSortingOrder > particleTop, view.EnemyDamageSortingOrder);
        }

        /// <summary>태그 공격 파티클 프리팹들의 가장 큰 그리는 차례.</summary>
        private static int MaxEffectSortingOrder(SlotHero.TagAttack.TagAttackFxConfig config)
        {
            int best = 0;
            foreach (SymbolTagType tag in System.Enum.GetValues(typeof(SymbolTagType)))
            {
                SlotHero.TagAttack.TagAttackFxEntry entry = config.GetEntry(tag);
                if (entry == null || entry.Effect == null)
                {
                    continue;
                }

                ParticleSystemRenderer[] renderers = entry.Effect.GetComponentsInChildren<ParticleSystemRenderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    best = Mathf.Max(best, renderers[i].sortingOrder);
                }
            }

            return best;
        }

        private static bool SpineTextureIsSrgb(SkeletonDataAsset data)
        {
            if (data == null || data.atlasAssets == null || data.atlasAssets.Length == 0 || data.atlasAssets[0] == null
                || data.atlasAssets[0].PrimaryMaterial == null)
            {
                return false;
            }

            Texture texture = data.atlasAssets[0].PrimaryMaterial.mainTexture;
            TextureImporter importer = texture != null
                ? AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter
                : null;
            return importer != null && importer.sRGBTexture;
        }

        private static T GetField<T>(object target, string name) where T : class
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null ? field.GetValue(target) as T : null;
        }

        private static void Report(string title, int actual, int expected)
        {
            Report(title, actual == expected, actual);
        }

        private static void Report(string title, bool passed)
        {
            Report(title, passed, 0);
        }

        private static void Report(string title, bool passed, int actual)
        {
            _checked++;
            LocalizationAudit.ScanOpenScenes();

            if (passed)
            {
                Debug.Log("  통과  " + title);
                return;
            }

            _failed++;
            Debug.LogError("  실패  " + title + "   (실제 " + actual + ")");
        }
    }
}
