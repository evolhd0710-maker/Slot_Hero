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
            Report("제국 군기 · 화성 문양 가치 +5", aquila.GetSymbolValue(Find(aquila, "Phoenix")), 9);
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
            Report("일식 · 수은(수성 · 금성) 2 - 6 = -4", eclipse.GetSymbolValue(Find(eclipse, "QuickSilver")), -4);
            Report("일식 · 불사조(화성 · 지구) 4 + 3 - 3 = 4", eclipse.GetSymbolValue(Find(eclipse, "Phoenix")), 4);
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
            Report("물건 목록의 유물도 열 개", catalog.Relics.Count, 10);

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
            setup.Deck.Add(new ReelSymbol("Phoenix", "불사조", SymbolTagType.Mars, SymbolTagType.Earth, 4));
            setup.Deck.Add(new ReelSymbol("Geyser", "간헐천", SymbolTagType.Mercury, SymbolTagType.Mars, 3));
            setup.Deck.Add(new ReelSymbol("Furnace", "용광로", SymbolTagType.Mars, SymbolTagType.Venus, 3));
            setup.Deck.Add(new ReelSymbol("Island", "섬", SymbolTagType.Neptune, SymbolTagType.Saturn, 7));
            setup.Deck.Add(new ReelSymbol("Shipwreck", "난파선", SymbolTagType.Neptune, SymbolTagType.Pluto, 9));
            setup.Deck.Add(new ReelSymbol("QuickSilver", "수은", SymbolTagType.Mercury, SymbolTagType.Venus, 2));

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
