using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.CurrentBuild;
using SlotHero.Ui;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 문양과 코인과 유물의 그림을 내주는 쪽. 전투는 물건 목록을 직접 갖지 않는다.
    /// </summary>
    public interface ICombatIconSource
    {
        /// <summary>그 식별자의 그림. 없으면 null 이다.</summary>
        Sprite GetIcon(string id);
    }

    /// <summary>
    /// 전투 화면을 그린다. 전투 UI 및 연출 초안 / 03 · 04 기초 화면 정보 를 옮긴 것이다.
    ///
    /// 표시와 연출만 맡는다. 무엇이 일어나는지는 조종기와 `CombatBattle` 이 정한다.
    /// </summary>
    public class CombatScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private CombatLayoutConfig _layout;
        [SerializeField] private CombatVisualConfig _visual;

        [Tooltip("태그 이름, 기호, 색, 그림. 현재 빌드 화면과 같은 것을 쓴다.")]
        [SerializeField] private CurrentBuildVisualConfig _tagVisuals;

        [Header("문양")]
        [SerializeField] private CombatReelView[] _reels = new CombatReelView[0];

        [Header("유물")]
        [SerializeField] private Image _relicPanel;
        [SerializeField] private Image[] _relicSlots = new Image[0];
        [SerializeField] private Image[] _relicIcons = new Image[0];

        [Tooltip("유물 그림이 없을 때 칸에 적는 이름.")]
        [SerializeField] private TMP_Text[] _relicLabels = new TMP_Text[0];

        [Tooltip("유물 고유 수치. 12개의 금화처럼 값이 바뀌는 유물만 적는다.")]
        [SerializeField] private TMP_Text[] _relicCounters = new TMP_Text[0];

        [SerializeField] private PointerRelay[] _relicHovers = new PointerRelay[0];

        [Header("유물 정보 팝업")]
        [SerializeField] private Image _relicPopup;
        [SerializeField] private TMP_Text _relicPopupName;
        [SerializeField] private TMP_Text _relicPopupBody;
        [SerializeField] private TMP_Text _relicPopupFlavor;

        [Header("플레이어")]
        [SerializeField] private Image _playerHealthPanel;
        [SerializeField] private Image _playerHealthBar;
        [SerializeField] private TMP_Text _playerHealthLabel;
        [SerializeField] private TMP_Text _playerDamageLabel;

        [Header("몬스터")]
        [SerializeField] private Image _enemyHealthPanel;
        [SerializeField] private Image _enemyHealthBar;
        [SerializeField] private TMP_Text _enemyHealthLabel;
        [SerializeField] private Image _enemyArt;
        [SerializeField] private TMP_Text _enemyArtLabel;
        [SerializeField] private Image _enemyInfo;
        [SerializeField] private TMP_Text _enemyNameLabel;
        [SerializeField] private TMP_Text _enemyInfoLabel;
        [SerializeField] private TMP_Text _enemyDamageLabel;

        [Header("소환수")]
        [Tooltip("소환수 칸. 자리 0 이 가장 왼쪽이다. 장군의 방패병이 장군 바로 왼쪽(마지막 칸)부터 들어선다.")]
        [SerializeField] private Image[] _summonPanels = new Image[0];
        [SerializeField] private Image[] _summonBars = new Image[0];
        [SerializeField] private TMP_Text[] _summonNames = new TMP_Text[0];
        [SerializeField] private TMP_Text[] _summonInfos = new TMP_Text[0];

        [Header("원판")]
        [SerializeField] private Image _dial;
        [SerializeField] private CanvasGroup _coinGroup;
        [SerializeField] private CoinDialView _coinDial;
        [SerializeField] private CanvasGroup _tagGroup;
        [SerializeField] private TagDialView _tagDial;
        [SerializeField] private TMP_Text _hintLabel;

        [Header("연출")]
        [SerializeField] private Image _projectile;
        [SerializeField] private TMP_Text _resultLabel;

        private readonly List<CombatRelic> _shownRelics = new List<CombatRelic>();
        private System.Action<bool>[] _relicHoverHandlers = new System.Action<bool>[0];

        private void Awake()
        {
            _relicHoverHandlers = new System.Action<bool>[_relicHovers.Length];
            for (int i = 0; i < _relicHovers.Length; i++)
            {
                int index = i;
                _relicHoverHandlers[i] = hovering => HandleRelicHover(index, hovering);
                if (_relicHovers[i] != null)
                {
                    _relicHovers[i].HoverChanged += _relicHoverHandlers[i];
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _relicHovers.Length && i < _relicHoverHandlers.Length; i++)
            {
                if (_relicHovers[i] != null)
                {
                    _relicHovers[i].HoverChanged -= _relicHoverHandlers[i];
                }
            }
        }

        public CombatLayoutConfig Layout
        {
            get { return _layout; }
        }

        public CombatVisualConfig Visual
        {
            get { return _visual; }
        }

        public CurrentBuildVisualConfig TagVisuals
        {
            get { return _tagVisuals; }
        }

        public CoinDialView CoinDial
        {
            get { return _coinDial; }
        }

        public TagDialView TagDial
        {
            get { return _tagDial; }
        }

        public IReadOnlyList<CombatReelView> Reels
        {
            get { return _reels; }
        }

        /// <summary>지금 보이는 원판. 코인 UI 면 true 다.</summary>
        public bool ShowingCoins
        {
            get { return _coinGroup != null && _coinGroup.gameObject.activeSelf; }
        }

        /// <summary>안내 글.</summary>
        public string Hint
        {
            get { return _hintLabel != null ? _hintLabel.text : string.Empty; }
        }

        /// <summary>화면을 켠다.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ApplyLayout();
            ApplyStyle();
        }

        /// <summary>화면을 끈다.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>태그 하나의 표시 자료.</summary>
        public BuildTagVisual GetTagVisual(SymbolTagType tag)
        {
            if (_tagVisuals != null)
            {
                return _tagVisuals.GetTagVisual(tag);
            }

            BuildTagVisual fallback = new BuildTagVisual();
            fallback.Tag = tag;
            fallback.DisplayName = tag.ToString();
            fallback.BarColor = Color.white;
            return fallback;
        }

        // ---- 문양 ----

        /// <summary>그 릴에 멈춘 문양을 그린다.</summary>
        public void ShowReel(int index, ReelSymbol symbol, Sprite icon)
        {
            if (index < 0 || index >= _reels.Length || _reels[index] == null)
            {
                return;
            }

            _reels[index].Show(symbol, icon, GetTagVisual(symbol.FirstTag), GetTagVisual(symbol.SecondTag));
        }

        /// <summary>릴 강조를 모두 걷는다.</summary>
        public void ClearReelHighlights()
        {
            for (int i = 0; i < _reels.Length; i++)
            {
                if (_reels[i] != null)
                {
                    _reels[i].SetHighlight(false, Color.clear);
                }
            }
        }

        /// <summary>그 자리들의 릴에 그 색 테두리를 두른다. 나머지는 걷는다.</summary>
        public void HighlightReels(IList<int> indices, Color color)
        {
            for (int i = 0; i < _reels.Length; i++)
            {
                if (_reels[i] != null)
                {
                    _reels[i].SetHighlight(indices != null && indices.Contains(i), color);
                }
            }
        }

        /// <summary>
        /// 그 자리들의 릴을 돌리다 왼쪽부터 차례로 멈춘다.
        /// 돌 때 보이는 그림은 덱을 차례로 넘긴 것이다. 결과는 이미 정해져 있다.
        /// </summary>
        public IEnumerator Spin(
            IList<int> targets, IReadOnlyList<ReelSymbol> result, IReadOnlyList<ReelSymbol> deck, ICombatIconSource icons)
        {
            if (targets == null || targets.Count == 0)
            {
                yield break;
            }

            float duration = _visual != null ? _visual.SpinDuration : 0.9f;
            float interval = _visual != null ? _visual.SpinStopInterval : 0.12f;
            float frame = _visual != null ? Mathf.Max(0.01f, _visual.SpinFrame) : 0.05f;

            List<int> order = new List<int>(targets);
            order.Sort();

            float[] stopAt = new float[order.Count];
            for (int i = 0; i < order.Count; i++)
            {
                stopAt[i] = duration + i * interval;
            }

            bool[] stopped = new bool[order.Count];
            float elapsed = 0f;
            float nextFrame = 0f;
            int tick = 0;
            int remaining = order.Count;

            while (remaining > 0)
            {
                if (elapsed >= nextFrame)
                {
                    nextFrame += frame;
                    tick++;

                    for (int i = 0; i < order.Count; i++)
                    {
                        if (stopped[i] || deck == null || deck.Count == 0)
                        {
                            continue;
                        }

                        ReelSymbol passing = deck[(tick * 7 + order[i] * 5) % deck.Count];
                        SpinFrame(order[i], passing, icons);
                    }
                }

                for (int i = 0; i < order.Count; i++)
                {
                    if (!stopped[i] && elapsed >= stopAt[i])
                    {
                        stopped[i] = true;
                        remaining--;
                        Land(order[i], result, icons);
                    }
                }

                yield return null;
                elapsed += Time.deltaTime;
            }
        }

        /// <summary>돌리지 않고 결과를 바로 놓는다.</summary>
        public void Land(int index, IReadOnlyList<ReelSymbol> result, ICombatIconSource icons)
        {
            if (result == null || index < 0 || index >= result.Count)
            {
                return;
            }

            ReelSymbol symbol = result[index];
            ShowReel(index, symbol, icons != null ? icons.GetIcon(symbol.Id) : null);
        }

        private void SpinFrame(int index, ReelSymbol passing, ICombatIconSource icons)
        {
            if (index < 0 || index >= _reels.Length || _reels[index] == null)
            {
                return;
            }

            _reels[index].ShowSpinning(icons != null ? icons.GetIcon(passing.Id) : null, passing.DisplayName);
        }

        // ---- 유물 ----

        /// <summary>왼쪽 유물 목록을 채운다. 칸 수를 넘는 유물은 그리지 않는다.</summary>
        public void ShowRelics(IReadOnlyList<CombatRelic> relics, ICombatIconSource icons, int capacity)
        {
            _shownRelics.Clear();
            if (relics != null)
            {
                for (int i = 0; i < relics.Count; i++)
                {
                    _shownRelics.Add(relics[i]);
                }
            }

            for (int i = 0; i < _relicSlots.Length; i++)
            {
                bool open = i < capacity;
                CombatRelic relic = open && i < _shownRelics.Count ? _shownRelics[i] : null;

                if (_relicSlots[i] != null)
                {
                    _relicSlots[i].gameObject.SetActive(open);
                    if (_visual != null)
                    {
                        _relicSlots[i].color = relic != null ? _visual.PanelColor : _visual.EmptyColor;
                    }
                }

                Sprite icon = relic != null && icons != null ? icons.GetIcon(relic.Id) : null;

                if (i < _relicIcons.Length && _relicIcons[i] != null)
                {
                    _relicIcons[i].sprite = icon;
                    _relicIcons[i].enabled = icon != null;
                    _relicIcons[i].preserveAspect = true;
                }

                // 그림(Relicimage_120NN)이 아직 없어 이름을 적는다.
                if (i < _relicLabels.Length && _relicLabels[i] != null)
                {
                    _relicLabels[i].text = relic != null && icon == null ? relic.DisplayName : string.Empty;
                }
            }

            RefreshRelicCounters();
            HideRelicPopup();
        }

        /// <summary>
        /// 유물 칸의 고유 수치를 지금 값으로 다시 적는다. 12개의 금화가 오를 때 부른다.
        /// 시트의 "-5 부분 UI 처리 필요, 실제 값이 변경될때마다 같이 변경" 이다.
        /// </summary>
        public void RefreshRelicCounters()
        {
            for (int i = 0; i < _relicCounters.Length; i++)
            {
                if (_relicCounters[i] == null)
                {
                    continue;
                }

                CombatRelic relic = i < _shownRelics.Count ? _shownRelics[i] : null;
                bool show = relic != null && relic.ShowsCounter;
                _relicCounters[i].gameObject.SetActive(show);

                if (show)
                {
                    int counter = relic.Counter;
                    _relicCounters[i].text = counter >= 0 ? "+" + counter : counter.ToString();
                }
            }
        }

        private void HandleRelicHover(int index, bool hovering)
        {
            CombatRelic relic = index < _shownRelics.Count ? _shownRelics[index] : null;
            if (!hovering || relic == null)
            {
                HideRelicPopup();
                return;
            }

            ShowRelicPopup(index, relic);
        }

        /// <summary>그 칸 오른쪽에 유물 정보를 띄운다. 이름, 지금 값을 채운 소개문, 플레이버 텍스트.</summary>
        public void ShowRelicPopup(int index, CombatRelic relic)
        {
            if (_relicPopup == null || relic == null)
            {
                return;
            }

            _relicPopup.gameObject.SetActive(true);
            _relicPopup.transform.SetAsLastSibling();

            SetText(_relicPopupName, relic.DisplayName);
            SetText(_relicPopupBody, relic.Description);
            SetText(_relicPopupFlavor, relic.Definition != null ? relic.Definition.Flavor : string.Empty);

            if (_layout != null)
            {
                Vector2 slot = _layout.GetRelicSlotPosition(index);
                Vector2 at = new Vector2(
                    _layout.RelicPanelPosition.x + _layout.RelicPanelSize.x + _layout.RelicPopupGap, slot.y);
                PlaceTopLeftRect(_relicPopup.rectTransform, at, _layout.RelicPopupSize);
            }
        }

        /// <summary>유물 정보를 걷는다.</summary>
        public void HideRelicPopup()
        {
            if (_relicPopup != null)
            {
                _relicPopup.gameObject.SetActive(false);
            }
        }

        // ---- 체력 ----

        /// <summary>
        /// 플레이어 체력을 적는다. 소모값을 넘기면 "100-10/100" 꼴로 깎일 몫을 빨갛게 적는다.
        /// 1.0.b 초안 05 의 표시를 그대로 옮겼다.
        /// </summary>
        public void ShowPlayerHealth(int health, int maxHealth, int pendingCost)
        {
            if (_playerHealthLabel != null)
            {
                string label = _visual != null ? _visual.PlayerHealthLabel + "  " : string.Empty;
                string cost = string.Empty;

                if (pendingCost > 0 && _visual != null)
                {
                    cost = "<color=#" + ColorUtility.ToHtmlStringRGB(_visual.CostColor) + ">-" + pendingCost + "</color>";
                }

                _playerHealthLabel.text = label + health + cost + " / " + maxHealth;
            }

            SetBar(_playerHealthBar, health - Mathf.Max(0, pendingCost), maxHealth);
        }

        /// <summary>적 체력을 적는다. 0 아래면 음수 그대로 적는다(오버킬 표시).</summary>
        public void ShowEnemyHealth(int health, int maxHealth)
        {
            if (_enemyHealthLabel != null)
            {
                _enemyHealthLabel.text = health + " / " + maxHealth;
            }

            SetBar(_enemyHealthBar, health, maxHealth);
        }

        /// <summary>몬스터 정보와 일러스트 자리를 채운다.</summary>
        public void ShowEnemy(CombatEnemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            SetText(_enemyArtLabel, enemy.Name);
            SetText(_enemyNameLabel, enemy.Name);
            SetText(_enemyInfoLabel, DescribeEnemy(enemy, true));

            ShowEnemyHealth(enemy.Health, enemy.MaxHealth);
        }

        /// <summary>
        /// 본체와 소환수를 모두 그린다. 영웅의 공격을 받을 소환수 칸은 색을 바꾼다.
        /// 소환수가 없는 자리의 칸은 끈다.
        /// </summary>
        public void ShowEnemies(CombatEnemy main, IReadOnlyList<CombatEnemy> field, CombatEnemy target)
        {
            ShowEnemy(main);

            for (int i = 0; i < _summonPanels.Length; i++)
            {
                if (_summonPanels[i] != null)
                {
                    _summonPanels[i].gameObject.SetActive(false);
                }
            }

            if (field == null)
            {
                return;
            }

            for (int i = 0; i < field.Count; i++)
            {
                CombatEnemy summon = field[i];
                int slot = summon != null ? summon.Slot : -1;
                if (summon == null || !summon.IsSummon || slot < 0 || slot >= _summonPanels.Length || _summonPanels[slot] == null)
                {
                    continue;
                }

                _summonPanels[slot].gameObject.SetActive(true);
                if (_visual != null)
                {
                    _summonPanels[slot].color = summon == target ? _visual.TargetColor : _visual.PanelColor;
                }

                if (slot < _summonNames.Length)
                {
                    SetText(_summonNames[slot], summon.Name);
                }

                if (slot < _summonInfos.Length)
                {
                    SetText(_summonInfos[slot], summon.Health + " / " + summon.MaxHealth + "\n" + DescribeEnemy(summon, false));
                }

                if (slot < _summonBars.Length)
                {
                    SetBar(_summonBars[slot], summon.Health, summon.MaxHealth);
                }
            }
        }

        /// <summary>
        /// 그 적의 가운데. 화면 왼쪽 위 기준이다. 소환수는 그 칸, 본체는 일러스트 가운데다.
        /// 공격 구슬과 피해 숫자가 여기로 간다.
        /// </summary>
        public Vector2 GetEnemyCenter(CombatEnemy enemy)
        {
            if (_layout == null)
            {
                return Vector2.zero;
            }

            if (enemy != null && enemy.IsSummon && enemy.Slot >= 0 && enemy.Slot < _summonPanels.Length)
            {
                Vector2 at = _layout.GetSummonPosition(enemy.Slot);
                return at + _layout.SummonSize * 0.5f;
            }

            return _layout.EnemyArtPosition + _layout.EnemyArtSize * 0.5f;
        }

        /// <summary>
        /// 정보 칸에 적는 글. 방어도, 버프, 지난 행동, 설명.
        /// full 이 아니면 소환수 칸에 들어가게 상태만 짧게 적는다.
        /// </summary>
        private string DescribeEnemy(CombatEnemy enemy, bool full)
        {
            List<string> lines = new List<string>();

            if (enemy.Monster == MonsterKind.Plain)
            {
                lines.Add((_visual != null ? _visual.AttackLabel : "공격력") + " " + enemy.Attack);
            }

            if (enemy.Block > 0)
            {
                lines.Add(Colored((_visual != null ? _visual.BlockLabel : "방어도") + " " + enemy.Block,
                    _visual != null ? _visual.BlockColor : Color.white));
            }

            if (enemy.Momentum != 0)
            {
                lines.Add(Colored((_visual != null ? _visual.MomentumLabel : "기세") + " " + enemy.Momentum,
                    _visual != null ? _visual.BuffColor : Color.white));
            }

            if (enemy.JavelinOrdered)
            {
                lines.Add(Colored(_visual != null ? _visual.JavelinOrderLabel : "투창 명령",
                    _visual != null ? _visual.BuffColor : Color.white));
            }

            if (full)
            {
                if (enemy.LastAction != null)
                {
                    lines.Add((_visual != null ? _visual.LastActionLabel : "지난 행동") + " · " + DescribeAction(enemy.LastAction));
                }

                if (lines.Count > 0)
                {
                    lines.Add(string.Empty);
                }

                lines.Add(enemy.Description);
            }

            return string.Join("\n", lines);
        }

        private static string DescribeAction(EnemyAction action)
        {
            switch (action.Kind)
            {
                case PatternKind.Attack:
                    return action.Pattern + " " + action.Damage;
                case PatternKind.Defend:
                    return action.Pattern + " " + action.BlockGained;
                default:
                    return action.Pattern;
            }
        }

        private static string Colored(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
        }

        private static void SetBar(Image bar, int value, int max)
        {
            if (bar == null)
            {
                return;
            }

            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.fillOrigin = (int)Image.OriginHorizontal.Left;
            bar.fillAmount = max > 0 ? Mathf.Clamp01(value / (float)max) : 0f;
        }

        // ---- 원판 ----

        /// <summary>안내 글을 바꾼다.</summary>
        public void SetHint(string text)
        {
            SetText(_hintLabel, text);
        }

        /// <summary>
        /// 원판을 코인 UI 나 태그 UI 로 바꾼다. 하나가 흐려지며 사라지고 다른 하나가 나타난다(초안 05).
        /// 세부 연출은 두 UI 그림이 정해진 뒤 정하기로 되어 있어 지금은 흐려지기만 한다.
        /// </summary>
        public IEnumerator SwapDial(bool toCoins)
        {
            CanvasGroup from = toCoins ? _tagGroup : _coinGroup;
            CanvasGroup to = toCoins ? _coinGroup : _tagGroup;
            float half = (_visual != null ? _visual.DialSwapDuration : 0.2f) * 0.5f;

            if (from != null && from.gameObject.activeSelf)
            {
                yield return Fade(from, 1f, 0f, half);
                from.gameObject.SetActive(false);
            }

            if (to != null)
            {
                to.gameObject.SetActive(true);
                yield return Fade(to, 0f, 1f, half);
            }
        }

        /// <summary>원판을 바로 바꾼다. 연출 없이 연다.</summary>
        public void SetDial(bool toCoins)
        {
            if (_coinGroup != null)
            {
                _coinGroup.gameObject.SetActive(toCoins);
                _coinGroup.alpha = 1f;
            }

            if (_tagGroup != null)
            {
                _tagGroup.gameObject.SetActive(!toCoins);
                _tagGroup.alpha = 1f;
            }
        }

        /// <summary>원판 누르기를 막거나 푼다. 연출 중에 막는다.</summary>
        public void SetDialInteractable(bool interactable)
        {
            if (_coinGroup != null)
            {
                _coinGroup.interactable = interactable;
                _coinGroup.blocksRaycasts = interactable;
            }

            if (_tagGroup != null)
            {
                _tagGroup.interactable = interactable;
                _tagGroup.blocksRaycasts = interactable;
            }
        }

        private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
                elapsed += Time.deltaTime;
            }

            group.alpha = to;
        }

        // ---- 연출 ----

        /// <summary>
        /// 원판에서 몬스터로 공격 구슬을 날린다. 초안 06 의 "태그 선택 UI 에서 공격 효과가 출발하여 적 캐릭터에 닿는 방식".
        /// 구체적인 공격 모션과 에셋은 논의가 필요하다고 적혀 있어 지금은 태그 색 구슬 하나다.
        /// </summary>
        public IEnumerator FireProjectile(Color color)
        {
            return FireProjectile(color, null);
        }

        /// <summary>그 적에게 공격 구슬을 날린다. 방패병이 맞으면 방패병 칸으로 간다.</summary>
        public IEnumerator FireProjectile(Color color, CombatEnemy target)
        {
            if (_projectile == null || _layout == null)
            {
                yield break;
            }

            RectTransform rect = _projectile.rectTransform;
            Vector2 from = TopLeftToAnchored(_layout.DialCenter);
            Vector2 to = TopLeftToAnchored(GetEnemyCenter(target));

            _projectile.color = color;
            _projectile.gameObject.SetActive(true);
            _projectile.transform.SetAsLastSibling();

            float duration = _visual != null ? _visual.ProjectileDuration : 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                rect.anchoredPosition = Vector2.Lerp(from, to, t * t);
                yield return null;
                elapsed += Time.deltaTime;
            }

            rect.anchoredPosition = to;
            _projectile.gameObject.SetActive(false);
        }

        /// <summary>몬스터 위에 피해 숫자를 띄우고 몬스터를 흔든다. 세기는 적 최대 체력 대비 비율로 정한다.</summary>
        public IEnumerator ShowEnemyHit(int damage, int enemyMaxHealth)
        {
            return ShowEnemyHit(damage, enemyMaxHealth, null, 0);
        }

        /// <summary>
        /// 맞은 적 위에 피해 숫자를 띄우고 그 적을 흔든다. 방어도가 막은 몫이 있으면 함께 적는다.
        /// </summary>
        public IEnumerator ShowEnemyHit(int damage, int enemyMaxHealth, CombatEnemy target, int blocked)
        {
            int tier = CombatVisualConfig.GetDamageTier(damage, enemyMaxHealth);
            float scale = _visual != null ? _visual.GetTierScale(tier) : 1f;
            float shake = _visual != null ? _visual.GetTierShake(tier) : 10f;

            RectTransform body = GetEnemyRect(target);
            Coroutine shaking = body != null
                ? StartCoroutine(Shake(body, shake, 0.3f))
                : null;

            if (_enemyDamageLabel != null && _layout != null)
            {
                Vector2 center = GetEnemyCenter(target);
                _enemyDamageLabel.rectTransform.anchoredPosition =
                    TopLeftToAnchored(new Vector2(center.x, center.y - UiScale.Px(40f)));
            }

            string text = (damage - blocked).ToString();
            if (blocked > 0)
            {
                string blockedText = _visual != null ? _visual.BlockedText : "막음";
                Color blockColor = _visual != null ? _visual.BlockColor : Color.white;
                text += "\n<size=45%>" + Colored(blockedText + " " + blocked, blockColor) + "</size>";
            }

            yield return Pop(_enemyDamageLabel, text, Color.white, scale);

            if (shaking != null)
            {
                yield return shaking;
            }
        }

        /// <summary>그 적을 그린 칸. 소환수는 그 칸, 본체는 일러스트다.</summary>
        private RectTransform GetEnemyRect(CombatEnemy enemy)
        {
            if (enemy != null && enemy.IsSummon && enemy.Slot >= 0 && enemy.Slot < _summonPanels.Length
                && _summonPanels[enemy.Slot] != null)
            {
                return _summonPanels[enemy.Slot].rectTransform;
            }

            return _enemyArt != null ? _enemyArt.rectTransform : null;
        }

        /// <summary>몬스터가 앞으로 나와 때리고 플레이어 체력 칸이 흔들린다.</summary>
        public IEnumerator ShowEnemyAttack(int damage)
        {
            return ShowEnemyAttack(damage, null);
        }

        /// <summary>
        /// 그 적이 앞으로 나온다. 피해가 있으면 플레이어 체력 칸이 흔들리고 피해 숫자가 뜬다.
        /// 방어나 버프처럼 때리지 않는 패턴은 나오기만 한다.
        /// </summary>
        public IEnumerator ShowEnemyAttack(int damage, CombatEnemy actor)
        {
            float duration = _visual != null ? _visual.EnemyAttackDuration : 0.5f;
            float lunge = _layout != null ? _layout.EnemyLunge : 40f;

            RectTransform body = GetEnemyRect(actor);
            if (body != null)
            {
                RectTransform rect = body;
                Vector2 home = rect.anchoredPosition;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    float t = elapsed / duration;
                    float push = Mathf.Sin(t * Mathf.PI) * lunge;
                    rect.anchoredPosition = home + new Vector2(-push, 0f);
                    yield return null;
                    elapsed += Time.deltaTime;
                }

                rect.anchoredPosition = home;
            }

            if (damage <= 0)
            {
                yield break;
            }

            Color red = _visual != null ? _visual.CostColor : Color.red;
            Coroutine shaking = _playerHealthPanel != null
                ? StartCoroutine(Shake(_playerHealthPanel.rectTransform, UiScale.Px(10f), 0.25f))
                : null;

            yield return Pop(_playerDamageLabel, "-" + damage, red, 1f);

            if (shaking != null)
            {
                yield return shaking;
            }
        }

        /// <summary>가운데에 결과 글을 띄운다. 비우면 걷는다.</summary>
        public void ShowResult(string text)
        {
            if (_resultLabel == null)
            {
                return;
            }

            _resultLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _resultLabel.text = text ?? string.Empty;
            _resultLabel.transform.SetAsLastSibling();
        }

        private IEnumerator Pop(TMP_Text label, string text, Color color, float scale)
        {
            if (label == null)
            {
                yield break;
            }

            RectTransform rect = label.rectTransform;
            Vector2 home = rect.anchoredPosition;
            float rise = _layout != null ? _layout.DamageRise : 90f;
            float duration = _visual != null ? _visual.DamageShowDuration : 0.8f;

            label.gameObject.SetActive(true);
            label.text = text;
            label.color = color;
            label.transform.SetAsLastSibling();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float grow = t < 0.2f ? Mathf.Lerp(0.6f, 1f, t / 0.2f) : 1f;
                rect.localScale = Vector3.one * scale * grow;
                rect.anchoredPosition = home + new Vector2(0f, rise * t);
                label.alpha = t < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                yield return null;
                elapsed += Time.deltaTime;
            }

            rect.anchoredPosition = home;
            rect.localScale = Vector3.one;
            label.gameObject.SetActive(false);
        }

        private static IEnumerator Shake(RectTransform rect, float magnitude, float duration)
        {
            Vector2 home = rect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float fade = 1f - elapsed / duration;
                float x = Mathf.Sin(elapsed * 90f) * magnitude * fade;
                rect.anchoredPosition = home + new Vector2(x, 0f);
                yield return null;
                elapsed += Time.deltaTime;
            }

            rect.anchoredPosition = home;
        }

        // ---- 자리와 색 ----

        /// <summary>
        /// 초안 03 · 04 의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            for (int i = 0; i < _reels.Length; i++)
            {
                if (_reels[i] != null)
                {
                    PlaceTopLeft(_reels[i].RectTransform, _layout.GetReelPosition(i), _layout.ReelSize);
                }
            }

            PlaceTopLeft(_relicPanel, _layout.RelicPanelPosition, _layout.RelicPanelSize);
            for (int i = 0; i < _relicSlots.Length; i++)
            {
                PlaceTopLeft(
                    _relicSlots[i], _layout.GetRelicSlotPosition(i),
                    new Vector2(_layout.RelicSlotSize, _layout.RelicSlotSize));
            }

            PlaceTopLeft(_playerHealthPanel, _layout.PlayerHealthPosition, _layout.PlayerHealthSize);
            PlaceTopLeft(_enemyHealthPanel, _layout.EnemyHealthPosition, _layout.EnemyHealthSize);
            PlaceTopLeft(_enemyArt, _layout.EnemyArtPosition, _layout.EnemyArtSize);
            PlaceTopLeft(_enemyInfo, _layout.EnemyInfoPosition, _layout.EnemyInfoSize);

            for (int i = 0; i < _summonPanels.Length; i++)
            {
                PlaceTopLeft(_summonPanels[i], _layout.GetSummonPosition(i), _layout.SummonSize);
            }

            PlaceTopLeft(_dial, _layout.DialPosition, new Vector2(_layout.DialSize, _layout.DialSize));

            if (_hintLabel != null)
            {
                PlaceTopLeftRect(_hintLabel.rectTransform, _layout.HintPosition, _layout.HintSize);
            }

            if (_projectile != null)
            {
                RectTransform rect = _projectile.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(_layout.ProjectileSize, _layout.ProjectileSize);
                _projectile.gameObject.SetActive(false);
            }

            // 피해 숫자는 몬스터 일러스트 가운데와 플레이어 체력 칸 위에 띄운다.
            PlaceCenteredOn(_enemyDamageLabel, new Vector2(
                _layout.EnemyArtPosition.x + _layout.EnemyArtSize.x * 0.5f,
                _layout.EnemyArtPosition.y + _layout.EnemyArtSize.y * 0.4f), new Vector2(_layout.EnemyArtSize.x, UiScale.Px(120f)));
            PlaceCenteredOn(_playerDamageLabel, new Vector2(
                _layout.PlayerHealthPosition.x + _layout.PlayerHealthSize.x * 0.5f,
                _layout.PlayerHealthPosition.y - UiScale.Px(50f)), new Vector2(_layout.PlayerHealthSize.x, UiScale.Px(100f)));

            if (_coinDial != null)
            {
                _coinDial.Configure(_layout, _visual);
            }

            if (_tagDial != null)
            {
                _tagDial.Configure(_layout, _visual, _tagVisuals);
            }
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            for (int i = 0; i < _reels.Length; i++)
            {
                if (_reels[i] != null)
                {
                    _reels[i].SetBackground(_visual.PanelColor);
                }
            }

            Paint(_relicPanel, _visual.PanelColor);
            Paint(_relicPopup, _visual.PopupColor);
            Paint(_playerHealthPanel, _visual.PanelColor);
            Paint(_enemyHealthPanel, _visual.PanelColor);
            Paint(_enemyInfo, _visual.PanelColor);
            Paint(_enemyArt, _visual.EnemyArtStandIn);
            Paint(_dial, _visual.DialColor);
            Paint(_playerHealthBar, _visual.PlayerBarColor);
            Paint(_enemyHealthBar, _visual.EnemyBarColor);

            Style(_playerHealthLabel, _visual.HealthFontSize, _visual.TextColor);
            Style(_enemyHealthLabel, _visual.HealthFontSize, _visual.TextColor);
            Style(_enemyArtLabel, _visual.NameFontSize, _visual.TextColor);
            Style(_enemyNameLabel, _visual.NameFontSize, _visual.TextColor);
            Style(_enemyInfoLabel, _visual.BodyFontSize, _visual.SubTextColor);
            Style(_hintLabel, _visual.HintFontSize, _visual.TextColor);
            Style(_enemyDamageLabel, _visual.DamageFontSize, Color.white);
            Style(_playerDamageLabel, _visual.DamageFontSize * 0.7f, _visual.CostColor);
            Style(_resultLabel, _visual.DamageFontSize * 1.4f, _visual.TextColor);

            for (int i = 0; i < _summonPanels.Length; i++)
            {
                Paint(_summonPanels[i], _visual.PanelColor);
                Paint(i < _summonBars.Length ? _summonBars[i] : null, _visual.EnemyBarColor);
                Style(i < _summonNames.Length ? _summonNames[i] : null, _visual.BodyFontSize, _visual.TextColor);
                Style(i < _summonInfos.Length ? _summonInfos[i] : null, _visual.BodyFontSize * 0.8f, _visual.SubTextColor);
            }

            if (_enemyDamageLabel != null)
            {
                _enemyDamageLabel.gameObject.SetActive(false);
            }

            if (_playerDamageLabel != null)
            {
                _playerDamageLabel.gameObject.SetActive(false);
            }

            ShowResult(null);
        }

        private static void Paint(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
        }

        private static void Style(TMP_Text label, float size, Color color)
        {
            if (label != null)
            {
                label.fontSize = size;
                label.color = color;
            }
        }

        private static void SetText(TMP_Text label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        /// <summary>왼쪽 위 기준 화면 좌표를 왼쪽 위 앵커의 anchoredPosition 으로 바꾼다.</summary>
        private static Vector2 TopLeftToAnchored(Vector2 topLeft)
        {
            return new Vector2(topLeft.x, -topLeft.y);
        }

        private static void PlaceCenteredOn(TMP_Text label, Vector2 topLeftCenter, Vector2 size)
        {
            if (label == null)
            {
                return;
            }

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = TopLeftToAnchored(topLeftCenter);
            label.alignment = TextAlignmentOptions.Center;
        }

        private static void PlaceTopLeft(Image image, Vector2 position, Vector2 size)
        {
            if (image != null)
            {
                PlaceTopLeftRect(image.rectTransform, position, size);
            }
        }

        private static void PlaceTopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            PlaceTopLeftRect(rect, position, size);
        }

        private static void PlaceTopLeftRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(position.x, -position.y);
        }
    }
}
