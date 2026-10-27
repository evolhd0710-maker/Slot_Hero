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

                _view.EnemyClicked += HandleEnemyClicked;
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

            _view.EnemyClicked -= HandleEnemyClicked;
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
                RefreshEnemies();
                _view.ShowRelics(_battle.Relics, _icons, _setup.RelicCapacity);
                _view.ShowPlayerHealth(_battle.PlayerHealth, _battle.PlayerMaxHealth, 0);
                _view.ClearReelHighlights();
                _view.SetDial(true);
            }

            _battle.Begin();
            Run(Intro());
        }

        /// <summary>
        /// 새 런을 시작하거나 다른 런을 이어 할 때 앞 런의 전투를 모두 비운다.
        /// 전투 진행과 화면에 남은 릴, 코인, 유물, 체력, 적, 안내 글을 지운다.
        /// 앞 런의 데이터가 다음 전투를 열 때 잠깐 보이지 않게 한다(2026년 10월 26일 원재).
        /// </summary>
        public void ResetForNewRun()
        {
            StopRunning();

            if (_battle != null)
            {
                _battle.RelicStackChanged -= HandleRelicStackChanged;
            }

            _battle = null;
            _setup = null;
            _finished = false;
            _busy = false;

            if (_view != null)
            {
                _view.ClearRunData();
                _view.Hide();
            }
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

        /// <summary>
        /// 태그를 건 뒤 그 자리의 적을 때린다. 적을 눌렀을 때와 같다. 대상을 고르는 중이 아니면 아무것도 하지 않는다.
        /// 소환수는 자리(0 이 가장 왼쪽), 본체는 `CombatScreenView.MainEnemySlot` 이다.
        /// </summary>
        public void ChooseTarget(int slot)
        {
            HandleEnemyClicked(slot);
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
                // 방패병이 있으면 방패병이 장군 대신 맞는다. 구슬도 그 칸으로 간다.
                if (!Instant)
                {
                    yield return _view.FireProjectile(color, attack.Target);
                    yield return _view.ShowEnemyHit(attack.Damage, attack.Target.MaxHealth, attack.Target, attack.Blocked);
                }

                RefreshEnemies();

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

            // 적 차례. 초안 06 의 "기존 UI 로 복귀, 적 공격 애니메이션 실행".
            // 왼쪽 적부터 한 번씩 행동한다. 행동마다 안내 글에 패턴을 적고 연출을 튼다.
            int healthBefore = _battle.PlayerHealth;
            int damage = _battle.EnemyAttack();
            LoseHealth(damage);

            if (_view != null)
            {
                RefreshEnemies();

                int shown = healthBefore;
                IReadOnlyList<EnemyAction> actions = _battle.EnemyActions;
                for (int i = 0; i < actions.Count; i++)
                {
                    shown -= actions[i].Damage;
                    _view.SetHint(actions[i].Summary);
                    _view.ShowPlayerHealth(Mathf.Max(0, shown), _battle.PlayerMaxHealth, 0);

                    if (!Instant)
                    {
                        yield return _view.ShowEnemyAttack(actions[i].Damage, actions[i].Actor);
                    }
                }

                // 철제 방패처럼 적이 때릴 때 발동한 유물을 알린다.
                if (_battle.Notices.Count > 0)
                {
                    _view.SetHint(string.Join("  ·  ", _battle.Notices));
                }

                _view.ShowPlayerHealth(Mathf.Max(0, _battle.PlayerHealth), _battle.PlayerMaxHealth, 0);
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

        /// <summary>
        /// 태그를 걸고 때릴 적을 고르는 동안의 화면. 건 칸과 그 태그 문양을 태그 색으로 칠해 두고 안내 글을 바꾼다.
        /// 원판은 그대로 누를 수 있다. 가운데를 누르면 태그 선택으로, 다른 태그를 누르면 그 태그로 다시 건다.
        /// </summary>
        private void ShowTargetPhase(Color color, List<int> reels)
        {
            if (_view == null)
            {
                return;
            }

            _view.HighlightReels(reels, color);
            if (_view.TagDial != null)
            {
                _view.TagDial.SetCharge(true, color);
            }

            _view.SetHint(_view.Visual != null ? _view.Visual.ChooseTargetHint : string.Empty);
            RefreshEnemies();
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
            if (!CanAct())
            {
                return;
            }

            // 대상을 고르는 중이면 건 태그를 풀고 태그 선택으로 돌아간다.
            if (_battle.Phase == CombatPhase.ChoosingTarget)
            {
                CancelTarget();
                return;
            }

            if (_battle.Phase == CombatPhase.ChoosingTag && _battle.BackToCoins())
            {
                Run(EnterCoinPhase());
            }
        }

        /// <summary>대상 선택을 그만두고 태그 선택 화면으로 되돌린다.</summary>
        private void CancelTarget()
        {
            if (_battle.CancelTarget() && _view != null)
            {
                _view.ClearReelHighlights();
                ShowTagPhase();
            }
        }

        /// <summary>
        /// 태그를 눌렀다. 적이 하나면 바로 때린다. 둘 이상이면 태그를 걸고 때릴 적을 고르게 한다.
        /// 대상을 고르는 중에 다른 태그를 누르면 그 태그로 다시 건다.
        /// </summary>
        private void HandleTagClicked(SymbolTagType tag)
        {
            if (!CanAct())
            {
                return;
            }

            if (_battle.Phase == CombatPhase.ChoosingTarget)
            {
                CancelTarget();
            }

            if (!_battle.CanAttackWith(tag))
            {
                return;
            }

            MarkChosen(TagDialView.GetSectorIndex(tag));

            if (_battle.NeedsTargetChoice)
            {
                if (_battle.BetTag(tag))
                {
                    ShowTargetPhase(TagColor(tag), _battle.FindReels(tag));
                }

                return;
            }

            Run(AttackRoutine(() => _battle.Attack(tag)));
        }

        private Color TagColor(SymbolTagType tag)
        {
            return _view != null ? _view.GetTagVisual(tag).BarColor : Color.white;
        }

        /// <summary>공격 예정 표시를 켤 칸을 원판에 알린다. 피그마 디자인은 원판 전체 대신 고른 칸만 빛낸다.</summary>
        private void MarkChosen(int sectorIndex)
        {
            if (_view != null && _view.TagDial != null)
            {
                _view.TagDial.MarkChosen(sectorIndex);
            }
        }

        private void HandleVortexClicked()
        {
            if (!CanAct())
            {
                return;
            }

            if (_battle.Phase == CombatPhase.ChoosingTarget)
            {
                CancelTarget();
            }

            if (!_battle.CanUseVortex())
            {
                return;
            }

            MarkChosen(TagDialView.VortexIndex);

            if (_battle.NeedsTargetChoice)
            {
                SymbolTagType top;
                int count;
                if (_battle.TryGetTopTag(out top, out count) && _battle.BetVortex())
                {
                    ShowTargetPhase(TagColor(top), _battle.FindReels(top));
                }

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

        /// <summary>
        /// 적을 눌렀다. 태그를 걸고 대상을 고르는 중이면 그 적을 때린다.
        /// 그 밖의 때나 연출 중, 쓰러진 적이면 무시한다.
        /// </summary>
        private void HandleEnemyClicked(int slot)
        {
            if (!CanAct() || _battle.Phase != CombatPhase.ChoosingTarget)
            {
                return;
            }

            CombatEnemy enemy = FindEnemyAt(slot);
            if (!_battle.CanTarget(enemy))
            {
                return;
            }

            Run(AttackRoutine(() => _battle.AttackTarget(enemy)));
        }

        /// <summary>그 자리의 살아 있는 적. 본체 자리 값이면 본체다. 없으면 null 이다.</summary>
        private CombatEnemy FindEnemyAt(int slot)
        {
            if (slot == CombatScreenView.MainEnemySlot)
            {
                return _battle.Enemy;
            }

            IReadOnlyList<CombatEnemy> enemies = _battle.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].IsSummon && enemies[i].Slot == slot)
                {
                    return enemies[i];
                }
            }

            return null;
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

        /// <summary>본체와 소환수의 체력, 방어도, 버프를 지금 값으로 다시 그린다.</summary>
        private void RefreshEnemies()
        {
            if (_view != null && _battle != null)
            {
                // 대상은 태그를 건 뒤 누른 적이라 미리 칠해 둘 대상이 없다.
                _view.ShowEnemies(_battle.Enemy, _battle.Enemies, null);
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
