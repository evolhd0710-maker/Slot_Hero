using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SlotHero.Combat;
using SlotHero.Combat.UI;
using SlotHero.Flow;
using SlotHero.Map;
using SlotHero.Save;

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
            Report("다음 칸은 금성", TagDialView.TryGetTag(1, out tag) && tag == SymbolTagType.Venus);
            Report("마지막 칸은 수성", TagDialView.TryGetTag(9, out tag) && tag == SymbolTagType.Mercury);
            Report("태그 칸 자리를 거꾸로 찾는다", TagDialView.GetSectorIndex(SymbolTagType.Neptune), 7);

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
            Report("일식 · 수은(수성 · 금성) 2 - 6 = -4", eclipse.GetSymbolValue(Find(eclipse, "Quicksilver")), -4);
            Report("일식 · 불사조(화성 · 지구) 4 + 3 - 3 = 4", eclipse.GetSymbolValue(Find(eclipse, "Pheonix")), 4);
            eclipse.SelectWithoutCoin();
            AttackResult venus = eclipse.Attack(SymbolTagType.Venus);
            Report("일식 · 가치 합이 음수여도 적을 낫게 하지 않는다", venus != null && venus.Damage == 0 && eclipse.Enemy.Health == 500);

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
                    battle.EnemyAttack();
                    EnemyAction action = battle.EnemyActions.Count > 0 ? battle.EnemyActions[0] : null;
                    if (action == null)
                    {
                        break;
                    }

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
            Report("검투사 · 타격 · 방어 · 함성을 모두 쓴다", strikes > 0 && guards > 0 && shouts > 0, strikes * 100 + guards * 10 + shouts);
            Report("검투사 · 타격이 가장 잦다", strikes > guards && strikes > shouts, strikes);
            Report("검투사 · 방어가 함성보다 잦다(30 : 20)", guards > shouts, guards);
            Report("검투사 · 함성마다 기세 50", momentumMatchesShouts);
            Report("검투사 · 타격은 6 × (1 + 기세%)", strikesMatchMomentum);
            Report("검투사 · 방어도가 영웅 공격을 먼저 막는다", blockAbsorbs);
            Report("검투사 · 방어도는 다음 행동 전에 사라진다", blockFades);
        }

        /// <summary>
        /// 장군과 방패병. 방패병이 없으면 진형 갖추기, 셋이면 더 소환하지 않는다.
        /// 방패병이 장군 대신 맞고, 투창 명령 다음 턴에 방패병마다 투창 7 을 던진다.
        /// </summary>
        private static void CheckGeneral(CombatRuleConfig rules)
        {
            // 방패병이 영웅 공격에 쓰러지지 않게 체력을 크게 둔 규칙으로 패턴만 본다.
            CombatRuleConfig sturdy = Object.Instantiate(rules);
            sturdy.Shieldman = new ShieldmanRule();
            sturdy.Shieldman.MaxHealth = 100000;

            CombatSetup setup = ExampleSetup(100000, 5000);
            setup.PlayerMaxHealth = 100000;
            setup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);

            CombatBattle battle = Begin(setup, sturdy);
            battle.SelectWithoutCoin();
            AttackResult first = battle.Attack(FirstUsableTag(battle));
            Report("장군 · 방패병이 없으면 장군이 맞는다", first != null && first.Target == battle.Enemy);

            battle.EnemyAttack();
            EnemyAction formation = battle.EnemyActions.Count > 0 ? battle.EnemyActions[0] : null;
            Report("장군 · 방패병이 없으면 진형 갖추기", formation != null && formation.Kind == PatternKind.Summon);
            Report("장군 · 방패병 1기가 선다", battle.SummonCount, 1);
            Report("장군 · 첫 방패병은 장군 바로 왼쪽 자리", formation != null && formation.Summoned != null ? formation.Summoned.Slot : -1, battle.Enemy.Slot - 1);
            Report("장군 · 소환된 방패병은 그 턴에 행동하지 않는다", battle.EnemyActions.Count, 1);
            Report("장군 · 방패병 체력은 규칙의 방패병 체력", formation != null && formation.Summoned != null ? formation.Summoned.MaxHealth : -1, 100000);

            battle.Reroll();
            battle.SelectWithoutCoin();
            int generalBefore = battle.Enemy.Health;
            AttackResult guarded = battle.Attack(FirstUsableTag(battle));
            Report("장군 · 방패병이 장군 대신 맞는다", guarded != null && guarded.Target != battle.Enemy && guarded.Target.IsSummon);
            Report("장군 · 장군 체력은 그대로", battle.Enemy.Health, generalBefore);

            bool neverOverMax = true;
            bool noFormationAtMax = true;
            bool javelinFollowsOrder = true;
            bool guardWithoutOrder = true;
            bool chargeIs15 = true;
            int charges = 0;
            int orders = 0;
            int javelins = 0;
            bool ordered = false;
            int orderedCount = 0;

            for (int turn = 0; turn < 80 && battle.Phase == CombatPhase.EnemyTurn; turn++)
            {
                int shieldmenBefore = battle.SummonCount;
                battle.EnemyAttack();

                int thrown = 0;
                for (int i = 0; i < battle.EnemyActions.Count; i++)
                {
                    EnemyAction action = battle.EnemyActions[i];
                    if (action.Actor.Monster == MonsterKind.Shieldman)
                    {
                        if (action.Pattern == "투창")
                        {
                            thrown++;
                            javelins++;
                            javelinFollowsOrder &= ordered && action.Damage == 7;
                        }
                        else
                        {
                            guardWithoutOrder &= action.Kind == PatternKind.Defend && action.BlockGained == 8;
                        }

                        continue;
                    }

                    if (action.Kind == PatternKind.Summon)
                    {
                        noFormationAtMax &= shieldmenBefore < 3;
                    }
                    else if (action.Kind == PatternKind.Attack)
                    {
                        charges++;
                        chargeIs15 &= action.Damage == 15;
                    }
                    else if (action.Kind == PatternKind.Buff)
                    {
                        orders++;
                    }
                }

                if (ordered)
                {
                    javelinFollowsOrder &= thrown == orderedCount;
                }

                // 이번 턴에 명령을 받은 방패병 수. 다음 턴에 그만큼 던진다.
                ordered = false;
                orderedCount = 0;
                for (int i = 0; i < battle.Enemies.Count; i++)
                {
                    if (battle.Enemies[i].JavelinOrdered)
                    {
                        ordered = true;
                        orderedCount++;
                    }
                }

                neverOverMax &= battle.SummonCount <= 3;

                battle.Reroll();
                battle.SelectWithoutCoin();
                battle.Attack(FirstUsableTag(battle));
            }

            Report("장군 · 방패병은 3기를 넘지 않는다", neverOverMax);
            Report("장군 · 방패병이 3기면 진형 갖추기를 쓰지 않는다", noFormationAtMax);
            Report("장군 · 기마병 돌격과 투창 명령을 쓴다", charges > 0 && orders > 0, charges * 100 + orders);
            Report("장군 · 기마병 돌격은 15", chargeIs15);
            Report("방패병 · 투창 명령 다음 턴에 명령받은 방패병마다 투창 7", javelins > 0 && javelinFollowsOrder, javelins);
            Report("방패병 · 명령이 없으면 방어 상태 8", guardWithoutOrder);

            Object.DestroyImmediate(sturdy);

            // 기본 규칙(방패병 체력 15). 방패병을 쓰러뜨리면 다시 장군이 맞는다.
            CombatSetup plainSetup = ExampleSetup(5000, 5000);
            plainSetup.PlayerMaxHealth = 5000;
            plainSetup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);
            CombatBattle plain = Begin(plainSetup, rules);
            plain.SelectWithoutCoin();
            plain.Attack(FirstUsableTag(plain));
            plain.EnemyAttack();
            plain.Reroll();
            plain.SelectCoin(0);
            AttackResult kill = plain.Attack(SymbolTagType.Mars);
            Report("방패병 · 체력 15", kill != null && kill.Target.IsSummon ? kill.Target.MaxHealth : -1, 15);
            Report("방패병 · 쓰러지면 자리에서 빠진다", plain.SummonCount, 0);
            Report("방패병 · 쓰러지면 다시 장군이 맞는다", plain.Target == plain.Enemy);

            // 본체가 쓰러지면 소환수도 사라지고 이긴다.
            CombatSetup lastSetup = ExampleSetup(5000, 5000);
            lastSetup.PlayerMaxHealth = 5000;
            lastSetup.Enemy = new CombatEnemy("장군", string.Empty, 5000, 15, MonsterKind.General);
            CombatBattle last = Begin(lastSetup, rules);
            last.SelectWithoutCoin();
            last.Attack(FirstUsableTag(last));
            last.EnemyAttack();
            last.Reroll();
            last.Enemy.Health = 0;
            last.SelectWithoutCoin();
            last.Attack(FirstUsableTag(last));
            Report("장군 · 본체가 쓰러지면 방패병도 사라진다", last.SummonCount, 0);
            Report("장군 · 본체가 쓰러지면 이긴다", last.Phase == CombatPhase.Won, (int)last.Phase);
        }

        /// <summary>규칙 설정의 적이 몬스터 기획서와 맞는지.</summary>
        private static void CheckMonsterRules(CombatRuleConfig rules)
        {
            CombatEnemy normal = rules.MakeEnemy(CombatEnemyKind.Normal, 1);
            Report("일반 방은 검투사", normal.Monster == MonsterKind.Gladiator && normal.Name == "검투사");
            Report("검투사 체력 30", normal.MaxHealth, 30);

            CombatEnemy elite = rules.MakeEnemy(CombatEnemyKind.Elite, 1);
            Report("엘리트 방은 장군", elite.Monster == MonsterKind.General && elite.Name == "장군");
            Report("장군 체력 80", elite.MaxHealth, 80);

            Report("스테이지가 오르면 패턴 피해에 더한다", rules.MakeEnemy(CombatEnemyKind.Normal, 2).AttackBonus, rules.EnemyAttackGrowthPerStage);
            Report("Plain 적은 공격력에 이미 더해 따로 더하지 않는다", rules.MakeEnemy(CombatEnemyKind.Boss, 2).AttackBonus, 0);
            Report("방패병은 장군과 같은 스테이지로 오른다", rules.MakeShieldman(2).MaxHealth > rules.MakeShieldman(1).MaxHealth);

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
            Report("창구에 규칙이 있다", GetField<CombatRuleConfig>(entry, "_rules") != null);
            Report("창구에 물건 목록이 있다", GetField<RunCatalogConfig>(entry, "_catalog") != null);
            Report("전투 화면은 꺼진 채로 시작한다", !screen.gameObject.activeSelf);

            CombatScreenView view = screen.View;
            Report("릴이 여섯이다", view != null ? view.Reels.Count : -1, 6);
            Report("태그 칸이 열이다", view != null && view.TagDial != null ? view.TagDial.Sectors.Count : -1, TagDialView.SectorCount);
            Report("코인 자리가 셋이다", view != null && view.CoinDial != null ? view.CoinDial.SlotCount : -1, 3);

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
