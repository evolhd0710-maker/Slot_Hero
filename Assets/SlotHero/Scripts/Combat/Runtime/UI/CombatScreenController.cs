using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 전투 화면의 흐름을 맡는다. 전투 UI 및 연출 초안 / 02 전투 UI 연출 흐름 을 따른다.
    ///
    ///   문양 회전 → 코인 선택 ⇄ 태그 선택 → 공격 예정(원판이 태그 색으로 찬다) → 공격 연출(한 번)
    ///   → 적 사망 확인 → 적 공격 → 쓴 문양만 재굴림 → 코인 선택 …
    ///
    /// 규칙은 `CombatBattle` 이 굴리고 여기서는 그 차례에 맞춰 연출을 튼다.
    /// 체력이 깎이면 `PlayerHealthLost` 로 알린다. 런에 적는 일은 듣는 쪽(흐름)이 한다.
    ///
    /// 플레이 중이 아니거나 `Instant` 를 켜면 연출 없이 한 번에 지나간다.
    /// 에디터 검사가 실제 화면을 그대로 눌러 볼 수 있게 하려는 것이다.
    /// </summary>
    public class CombatScreenController : MonoBehaviour
    {
        [Header("붙일 것")]
        [SerializeField] private CombatScreenView _view;
        [SerializeField] private CombatRuleConfig _rules;

        [Header("동작")]
        [Tooltip("켜면 Awake 에서 화면을 닫아 둔다. 다른 화면 조종기와 같은 까닭으로 기본은 끈다.")]
        [SerializeField] private bool _hideOnAwake;

        [Tooltip("켜면 연출 없이 한 번에 지나간다.")]
        [SerializeField] private bool _instant;

        private CombatBattle _battle;
        private CombatSetup _setup;
        private ICombatIconSource _icons;
        private Coroutine _running;
        private bool _busy;
        private bool _finished;

        /// <summary>플레이어 체력이 깎였다. 코인 소모값이나 적의 공격이다. 런에 적는 쪽이 듣는다.</summary>
        public event Action<int> PlayerHealthLost;

        /// <summary>전투가 끝났다. 한 번만 온다.</summary>
        public event Action<CombatOutcome> Finished;

        /// <summary>유물 고유 수치가 올랐다(12개의 금화). 런에 쌓인 횟수를 적는 쪽이 듣는다.</summary>
        public event Action<CombatRelic> RelicStackChanged;

        /// <summary>지금 굴리는 전투. 열지 않았으면 null 이다.</summary>
        public CombatBattle Battle
        {
            get { return _battle; }
        }

        /// <summary>화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _view != null && _view.gameObject.activeInHierarchy; }
        }

        /// <summary>연출이 도는 중이라 누르기를 받지 않는지.</summary>
        public bool IsBusy
        {
            get { return _busy; }
        }

        /// <summary>
        /// 연출 없이 지나갈지. 검사가 켠다.
        /// 플레이 중이 아니거나 꺼져 있으면 코루틴이 돌지 않으므로 늘 그렇다.
        /// </summary>
        public bool Instant
        {
            get { return _instant || !Application.isPlaying || !isActiveAndEnabled; }
            set { _instant = value; }
        }

        public CombatScreenView View
        {
            get { return _view; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                if (_view.CoinDial != null)
                {
                    _view.CoinDial.CoinClicked += HandleCoinClicked;
                    _view.CoinDial.CoinHoverChanged += HandleCoinHover;
                    _view.CoinDial.CenterClicked += HandleNoCoinClicked;
                    _view.CoinDial.CenterHoverChanged += HandleNoCoinHover;
                }

                if (_view.TagDial != null)
                {
                    _view.TagDial.TagClicked += HandleTagClicked;
                    _view.TagDial.TagHoverChanged += HandleTagHover;
                    _view.TagDial.VortexHoverChanged += HandleVortexHover;
                    _view.TagDial.CenterClicked += HandleTagCenterClicked;
                    _view.TagDial.VortexClicked += HandleVortexClicked;
                }
            }

            if (_hideOnAwake)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            if (_view == null)
            {
                return;
            }

            if (_view.CoinDial != null)
            {
                _view.CoinDial.CoinClicked -= HandleCoinClicked;
                _view.CoinDial.CoinHoverChanged -= HandleCoinHover;
                _view.CoinDial.CenterClicked -= HandleNoCoinClicked;
                _view.CoinDial.CenterHoverChanged -= HandleNoCoinHover;
            }

            if (_view.TagDial != null)
            {
                _view.TagDial.TagClicked -= HandleTagClicked;
                _view.TagDial.TagHoverChanged -= HandleTagHover;
                _view.TagDial.VortexHoverChanged -= HandleVortexHover;
                _view.TagDial.CenterClicked -= HandleTagCenterClicked;
                _view.TagDial.VortexClicked -= HandleVortexClicked;
            }
        }

        /// <summary>전투를 연다. 릴을 모두 돌린 뒤 코인 선택으로 간다.</summary>
        public void Open(CombatSetup setup, ICombatIconSource icons)
        {
            StopRunning();

            _setup = setup ?? new CombatSetup();
            _icons = icons;
            _finished = false;
            _busy = false;

            _battle = new CombatBattle(_setup, _rules);
            _battle.RelicStackChanged += HandleRelicStackChanged;

            if (_view != null)
            {
                _view.Show();
                _view.ShowEnemy(_battle.Enemy);
                _view.ShowRelics(_battle.Relics, _icons, _setup.RelicCapacity);
                _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, 0);
                _view.ClearReelHighlights();
                _view.SetDial(true);
            }

            _battle.Begin();
            Run(Intro());
        }

        /// <summary>화면을 닫는다. 알리지 않는다.</summary>
        public void Close()
        {
            StopRunning();
            _busy = false;

            if (_view != null)
            {
                _view.Hide();
            }
        }

        // ---- 누르기. 검사도 이 길로 누른다 ----

        /// <summary>그 자리의 코인을 고른다. 눌렀을 때와 같은 길이다.</summary>
        public void ChooseCoin(int offerIndex)
        {
            HandleCoinClicked(offerIndex);
        }

        /// <summary>코인 없이 태그를 고르러 간다. 가운데 원을 눌렀을 때와 같다.</summary>
        public void ChooseWithoutCoin()
        {
            HandleNoCoinClicked();
        }

        /// <summary>태그를 골라 공격한다. 칸을 눌렀을 때와 같다.</summary>
        public void ChooseTag(SymbolTagType tag)
        {
            HandleTagClicked(tag);
        }

        /// <summary>소용돌이 칸으로 공격한다. 사건의 지평선이 있을 때만 된다. 칸을 눌렀을 때와 같다.</summary>
        public void ChooseVortex()
        {
            HandleVortexClicked();
        }

        /// <summary>코인을 다시 고르러 돌아간다. 태그 UI 가운데를 눌렀을 때와 같다.</summary>
        public void ReturnToCoins()
        {
            HandleTagCenterClicked();
        }

        // ---- 차례 ----

        private IEnumerator Intro()
        {
            _busy = true;
            if (_view != null)
            {
                _view.SetDialInteractable(false);
            }

            yield return SpinReels(AllReels());
            ShowCoinPhase();

            _busy = false;
        }

        private IEnumerator AttackRoutine(Func<AttackResult> attackNow)
        {
            _busy = true;
            if (_view != null)
            {
                _view.SetDialInteractable(false);
            }

            AttackResult attack = attackNow();
            if (attack == null)
            {
                _busy = false;
                if (_view != null)
                {
                    _view.SetDialInteractable(true);
                }

                yield break;
            }

            if (attack.HealthPaid > 0)
            {
                LoseHealth(attack.HealthPaid);
            }

            Color color = _view != null ? _view.GetTagVisual(attack.Tag).BarColor : Color.white;

            if (_view != null)
            {
                _view.HighlightReels(attack.ReelIndices, color);
                _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, 0);
                _view.SetHint(string.Empty);

                if (_view.TagDial != null)
                {
                    _view.TagDial.SetCharge(true, color);
                }
            }

            yield return Wait(_view != null && _view.Visual != null ? _view.Visual.AttackChargeDuration : 0.45f);

            if (_view != null)
            {
                if (!Instant)
                {
                    yield return _view.FireProjectile(color);
                }

                _view.ShowEnemyHealth(_battle.Enemy.Health, _battle.Enemy.MaxHealth);

                if (!Instant)
                {
                    yield return _view.ShowEnemyHit(attack.Damage, _battle.Enemy.MaxHealth);
                }

                if (_view.TagDial != null)
                {
                    _view.TagDial.SetCharge(false, Color.clear);
                }
            }

            if (_battle.Phase == CombatPhase.Won)
            {
                yield return FinishRoutine();
                yield break;
            }

            // 적 공격. 초안 06 의 "기존 UI 로 복귀, 적 공격 애니메이션 실행".
            int damage = _battle.EnemyAttack();
            LoseHealth(damage);

            if (_view != null)
            {
                // 철제 방패처럼 적이 때릴 때 발동한 유물을 알린다.
                if (_battle.Notices.Count > 0)
                {
                    _view.SetHint(string.Join("  ·  ", _battle.Notices));
                }

                _view.ShowPlayerHealth(Mathf.Max(0, _battle.PlayerHealth), _battle.PlayerMaxHealth, 0);
                if (!Instant)
                {
                    yield return _view.ShowEnemyAttack(damage);
                }
            }

            if (_battle.Phase == CombatPhase.Lost)
            {
                yield return FinishRoutine();
                yield break;
            }

            // 쓴 문양만 다시 돈다. 초안 05 의 "적 공격이 진행되고 재굴림이 진행됨".
            List<int> spun = _battle.Reroll();

            if (_view != null)
            {
                _view.ClearReelHighlights();
            }

            yield return SpinReels(spun);
            yield return SwapDial(true);
            ShowCoinPhase();

            _busy = false;
        }

        private IEnumerator FinishRoutine()
        {
            _busy = true;

            CombatOutcome outcome = _battle.GetOutcome();

            if (_view != null && _view.Visual != null)
            {
                _view.ShowResult(outcome.Won ? _view.Visual.VictoryText : _view.Visual.DefeatText);
            }

            yield return Wait(_view != null && _view.Visual != null ? _view.Visual.EndDelay : 1f);

            if (_finished)
            {
                yield break;
            }

            _finished = true;
            _running = null;

            if (Finished != null)
            {
                Finished(outcome);
            }
        }

        private IEnumerator SpinReels(List<int> targets)
        {
            if (_view == null || targets == null || targets.Count == 0)
            {
                yield break;
            }

            if (Instant)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    _view.Land(targets[i], _battle.Reels, _icons);
                }

                yield break;
            }

            yield return _view.Spin(targets, _battle.Reels, _battle.Deck, _icons);
        }

        private IEnumerator SwapDial(bool toCoins)
        {
            if (_view == null)
            {
                yield break;
            }

            if (Instant)
            {
                _view.SetDial(toCoins);
                yield break;
            }

            yield return _view.SwapDial(toCoins);
        }

        private object Wait(float seconds)
        {
            return Instant ? null : new WaitForSeconds(seconds);
        }

        /// <summary>코인 UI 를 이번 턴에 꺼낸 코인으로 채운다.</summary>
        private void ShowCoinPhase()
        {
            if (_view == null)
            {
                return;
            }

            CoinDialView dial = _view.CoinDial;
            if (dial != null)
            {
                for (int i = 0; i < dial.SlotCount; i++)
                {
                    CombatCoin coin;
                    if (_battle.TryGetOffered(i, out coin))
                    {
                        dial.SetCoin(i, GetIcon(coin.Id), coin.DisplayName, _battle.CanUse(coin));
                    }
                    else
                    {
                        dial.SetEmpty(i);
                    }
                }

                dial.HidePopup();
            }

            _view.SetDial(true);
            _view.SetDialInteractable(true);
            _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, 0);
            _view.RefreshRelicCounters();

            // 릴이 멈출 때 발동한 유물(망치와 끌, 12개의 금화)이 있으면 그 소식을 먼저 보인다.
            _view.SetHint(_battle.Notices.Count > 0
                ? string.Join("  ·  ", _battle.Notices)
                : (_view.Visual != null ? _view.Visual.ChooseCoinHint : string.Empty));
        }

        /// <summary>태그 UI 를 고른 코인과 고를 수 있는 태그로 채운다.</summary>
        private void ShowTagPhase()
        {
            if (_view == null)
            {
                return;
            }

            TagDialView dial = _view.TagDial;
            if (dial != null)
            {
                CombatCoin coin;
                if (_battle.TryGetSelectedCoin(out coin))
                {
                    dial.SetCenterCoin(GetIcon(coin.Id), coin.DisplayName);
                }
                else
                {
                    dial.SetCenterCoin(null, _view.Visual != null ? _view.Visual.NoCoinText : string.Empty);
                }

                dial.SetAvailable(tag => _battle.FindReels(tag).Count > 0, _battle.CanUseVortex());
                dial.SetCharge(false, Color.clear);
            }

            CombatCoin selected;
            int cost = _battle.TryGetSelectedCoin(out selected) ? _battle.GetCoinCost(selected) : 0;
            _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, cost);
            _view.SetHint(_view.Visual != null ? _view.Visual.ChooseTagHint : string.Empty);
        }

        private IEnumerator EnterTagPhase()
        {
            _busy = true;
            if (_view != null)
            {
                _view.SetDialInteractable(false);
                if (_view.CoinDial != null)
                {
                    _view.CoinDial.HidePopup();
                }
            }

            ShowTagPhase();
            yield return SwapDial(false);

            if (_view != null)
            {
                _view.SetDialInteractable(true);
            }

            _busy = false;
        }

        private IEnumerator EnterCoinPhase()
        {
            _busy = true;
            if (_view != null)
            {
                _view.SetDialInteractable(false);
                _view.ClearReelHighlights();
            }

            yield return SwapDial(true);
            ShowCoinPhase();

            _busy = false;
        }

        // ---- 사건 ----

        private void HandleCoinClicked(int offerIndex)
        {
            if (!CanAct() || _battle.Phase != CombatPhase.ChoosingCoin)
            {
                return;
            }

            if (_battle.SelectCoin(offerIndex))
            {
                Run(EnterTagPhase());
            }
        }

        private void HandleNoCoinClicked()
        {
            if (!CanAct() || _battle.Phase != CombatPhase.ChoosingCoin)
            {
                return;
            }

            if (_battle.SelectWithoutCoin())
            {
                Run(EnterTagPhase());
            }
        }

        private void HandleTagCenterClicked()
        {
            if (!CanAct() || _battle.Phase != CombatPhase.ChoosingTag)
            {
                return;
            }

            if (_battle.BackToCoins())
            {
                Run(EnterCoinPhase());
            }
        }

        private void HandleTagClicked(SymbolTagType tag)
        {
            if (!CanAct() || !_battle.CanAttackWith(tag))
            {
                return;
            }

            Run(AttackRoutine(() => _battle.Attack(tag)));
        }

        private void HandleVortexClicked()
        {
            if (!CanAct() || !_battle.CanUseVortex())
            {
                return;
            }

            Run(AttackRoutine(() => _battle.AttackWithVortex()));
        }

        private void HandleRelicStackChanged(CombatRelic relic)
        {
            if (_view != null)
            {
                _view.RefreshRelicCounters();
            }

            if (RelicStackChanged != null)
            {
                RelicStackChanged(relic);
            }
        }

        private void HandleCoinHover(int offerIndex, bool hovering)
        {
            if (_view == null || _battle == null || _battle.Phase != CombatPhase.ChoosingCoin)
            {
                return;
            }

            CombatCoin coin;
            if (!hovering || !_battle.TryGetOffered(offerIndex, out coin))
            {
                if (_view.CoinDial != null)
                {
                    _view.CoinDial.HidePopup();
                }

                _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, 0);
                return;
            }

            CombatVisualConfig visual = _view.Visual;
            bool usable = _battle.CanUse(coin);

            // 유물이 바꾼 값(흑요석 단검, 12개의 금화)까지 넣어 적는다.
            int cost = _battle.GetCoinCost(coin);
            int weight = _battle.GetCoinWeight(coin);
            string weightText = weight >= 0 ? "+" + weight : weight.ToString();

            string stats = visual != null
                ? visual.CostLabel + " " + cost + "\n" + visual.WeightLabel + " " + weightText
                : cost + " / " + weightText;

            string description = coin.Description;
            if (!usable && visual != null)
            {
                description = visual.NotEnoughHealthText + "\n" + description;
            }

            if (_view.CoinDial != null)
            {
                _view.CoinDial.ShowPopup(offerIndex, GetIcon(coin.Id), coin.DisplayName, stats, description);
            }

            // 1.0.b 초안 05 처럼 소모값만큼 깎일 체력을 미리 보인다.
            _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, cost);
        }

        private void HandleNoCoinHover(bool hovering)
        {
            if (_view == null || _view.Visual == null || _battle == null || _battle.Phase != CombatPhase.ChoosingCoin)
            {
                return;
            }

            _view.SetHint(hovering ? _view.Visual.NoCoinHint : _view.Visual.ChooseCoinHint);
        }

        private void HandleTagHover(SymbolTagType tag, bool hovering)
        {
            if (_view == null || _battle == null || _battle.Phase != CombatPhase.ChoosingTag || _busy)
            {
                return;
            }

            // 초안 06 의 "태그 UI 의 각 태그 위에 마우스를 올릴 경우 태그가 포함된 문양을 강조 표시".
            if (hovering)
            {
                _view.HighlightReels(_battle.FindReels(tag), _view.GetTagVisual(tag).BarColor);
            }
            else
            {
                _view.ClearReelHighlights();
            }
        }

        private void HandleVortexHover(bool hovering)
        {
            if (_view == null || _view.Visual == null || _battle == null || _battle.Phase != CombatPhase.ChoosingTag)
            {
                return;
            }

            string vortex = _battle.CanUseVortex() ? _view.Visual.VortexRelicHint : _view.Visual.VortexHint;
            _view.SetHint(hovering ? vortex : _view.Visual.ChooseTagHint);
        }

        private bool CanAct()
        {
            return _battle != null && !_busy && !_finished;
        }

        private void LoseHealth(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (PlayerHealthLost != null)
            {
                PlayerHealthLost(amount);
            }
        }

        private Sprite GetIcon(string id)
        {
            return _icons != null ? _icons.GetIcon(id) : null;
        }

        private List<int> AllReels()
        {
            List<int> all = new List<int>();
            for (int i = 0; i < _battle.Reels.Count; i++)
            {
                all.Add(i);
            }

            return all;
        }

        /// <summary>
        /// 차례 하나를 튼다. 플레이 중이면 코루틴으로, 아니면 그 자리에서 끝까지 돌린다.
        /// 연출을 건너뛰므로 그 자리에서 돌려도 기다리는 곳이 없다.
        /// </summary>
        private void Run(IEnumerator routine)
        {
            if (Instant)
            {
                Drain(routine);
                return;
            }

            StopRunning();
            _running = StartCoroutine(routine);
        }

        private static void Drain(IEnumerator routine)
        {
            Stack<IEnumerator> stack = new Stack<IEnumerator>();
            stack.Push(routine);

            while (stack.Count > 0)
            {
                IEnumerator top = stack.Peek();
                if (!top.MoveNext())
                {
                    stack.Pop();
                    continue;
                }

                IEnumerator nested = top.Current as IEnumerator;
                if (nested != null)
                {
                    stack.Push(nested);
                }
            }
        }

        private void StopRunning()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
        }
    }
}
