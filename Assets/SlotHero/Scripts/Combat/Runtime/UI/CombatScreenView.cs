using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.CurrentBuild;
using SlotHero.TagAttack.UI;
using SlotHero.Ui;
using Spine.Unity;

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

        [Tooltip("유물 정보 팝업 왼쪽의 유물 그림. 코인 정보 팝업과 같은 꼴이다(2026년 10월 31일 원재).")]
        [SerializeField] private Image _relicPopupIcon;

        [Header("플레이어")]
        [Tooltip("주인공 깃발과 초상. 피그마 그림 그대로다.")]
        [SerializeField] private Image _playerPortrait;
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
        private Sprite _enemySprite;

        [Tooltip("몬스터 스파인을 그리는 자리. 몬스터 그림 칸의 자식이라 앞으로 나오는 공격 연출을 함께 탄다(2026년 11월 13일 원재).")]
        [SerializeField] private SkeletonGraphic _enemySpine;
        [Tooltip("몬스터 스파인을 움직이는 쪽. 스파인 4.3 은 그리는 것과 움직이는 것이 따로다.")]
        [SerializeField] private SkeletonAnimation _enemySpineMotion;
        private SkeletonDataAsset _enemySpineData;
        [SerializeField] private Image _enemyInfo;
        [SerializeField] private TMP_Text _enemyNameLabel;
        [SerializeField] private TMP_Text _enemyInfoLabel;
        [SerializeField] private TMP_Text _enemyDamageLabel;

        [Tooltip("숫자 그림으로 적는 피해 숫자. 있으면 위의 글자 대신 이것을 띄운다.")]
        [SerializeField] private DamageNumberView _enemyDamageNumber;

        [Header("레이저 발사음")]
        [Tooltip("레이저 발사음. 레이저가 뻗기 시작할 때 소리 칸으로 낸다. 적에 닿을 때 나던 타격음은 2026년 10월 30일 뺐다.")]
        [SerializeField] private AudioClip _laserClip;

        [Tooltip("효과음을 내는 소리 칸.")]
        [SerializeField] private AudioSource _sfxAudio;

        [Tooltip("태그별 공격 이펙트와 소리(2026년 11월 1일 원재). 레이저가 적에 닿는 순간 낸다. 소리가 있는 태그는 레이저 발사음을 내지 않는다.")]
        [SerializeField] private TagAttackFxPlayer _tagAttackFx;

        private float _soundVolume = 1f;

        /// <summary>
        /// 설정의 소리 크기(전체 × 효과음, 0 ~ 1). 레이저 발사음에 곱한다.
        /// 흐름이 설정을 적용할 때 넣는다(인게임 화면 기획서 07 설정 화면의 사운드 탭, 2026년 10월 31일 원재).
        /// </summary>
        public float SoundVolume
        {
            get { return _soundVolume; }
            set { _soundVolume = Mathf.Clamp01(value); }
        }

        [Header("소환수")]
        [Tooltip("소환수 칸. 자리 0 이 가장 왼쪽이다. 지금은 소환수를 세우는 적이 없다. 장군의 방패병은 장군이 가진 수다(2026년 10월 29일).")]
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
        [Tooltip("원판에서 적까지 쏘는 공격 레이저. 흰 그림을 태그 색으로 물들인다.")]
        [SerializeField] private Image _laser;
        [SerializeField] private TMP_Text _resultLabel;

        [Tooltip("회전할 때 가운데에 크게 뜨는 6열 × 5행 슬롯. 없으면 원래 칸에서 돈다.")]
        [SerializeField] private SlotSpinOverlay _spinOverlay;

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

        /// <summary>큰 슬롯. 없으면 null 이다.</summary>
        public SlotSpinOverlay SpinOverlay
        {
            get { return _spinOverlay; }
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

            // 지난 전투가 도는 중에 닫혔으면 큰 슬롯이 남아 있다. 새로 열 때 걷는다.
            if (_spinOverlay != null)
            {
                _spinOverlay.Hide();
            }

            if (_laser != null)
            {
                _laser.gameObject.SetActive(false);
            }
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

            _reels[index].Show(symbol, icon, GetTagVisual(symbol.FirstTag), GetTagVisual(symbol.SecondTag), _tagVisuals);
        }

        /// <summary>
        /// 앞 런의 전투 표시를 모두 지운다. 새 런을 시작하거나 다른 런을 이어 할 때 조종기가 부른다(2026년 10월 26일 원재).
        /// 릴, 코인 원판, 태그 원판 가운데 코인과 공격 예정 칸, 유물 칸, 체력, 적 이름과 정보, 소환수, 안내 글, 피해 숫자를 비운다.
        /// </summary>
        public void ClearRunData()
        {
            for (int i = 0; i < _reels.Length; i++)
            {
                if (_reels[i] != null)
                {
                    _reels[i].ShowSpinning(null, string.Empty);
                    _reels[i].SetHighlight(false, Color.clear);
                }
            }

            if (_coinDial != null)
            {
                for (int i = 0; i < _coinDial.SlotCount; i++)
                {
                    _coinDial.SetEmpty(i);
                }

                _coinDial.HidePopup();
            }

            if (_tagDial != null)
            {
                _tagDial.SetCenterCoin(null, string.Empty);
                _tagDial.SetCharge(false, Color.clear);
            }

            ShowRelics(null, null, _relicSlots.Length);

            SetText(_playerHealthLabel, string.Empty);
            SetText(_enemyHealthLabel, string.Empty);
            SetBar(_playerHealthBar, 0, 0);
            SetBar(_enemyHealthBar, 0, 0);
            _enemySprite = null;
            PaintEnemyArt();
            SetText(_enemyArtLabel, string.Empty);
            SetText(_enemyNameLabel, string.Empty);
            SetText(_enemyInfoLabel, string.Empty);

            for (int i = 0; i < _summonPanels.Length; i++)
            {
                if (_summonPanels[i] != null)
                {
                    _summonPanels[i].gameObject.SetActive(false);
                }
            }

            SetHint(string.Empty);
            ShowResult(null);
            SetDial(true);

            if (_enemyDamageLabel != null)
            {
                _enemyDamageLabel.gameObject.SetActive(false);
            }

            if (_enemyDamageNumber != null)
            {
                _enemyDamageNumber.gameObject.SetActive(false);
            }

            if (_playerDamageLabel != null)
            {
                _playerDamageLabel.gameObject.SetActive(false);
            }
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

                // 칸은 흰 유물 칸 틀 그림이고 색은 그 유물 등급을 따른다. 유물이 없으면 회색이다.
                if (_relicSlots[i] != null)
                {
                    _relicSlots[i].gameObject.SetActive(open);
                    if (_visual != null)
                    {
                        _relicSlots[i].color = relic != null
                            ? _visual.GetRelicGradeColor(relic.Grade)
                            : _visual.RelicEmptyFrameColor;
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

            // 코인 정보 팝업처럼 왼쪽에 그 유물 그림을 둔다. 칸에 그려 둔 그림을 그대로 쓴다.
            if (_relicPopupIcon != null)
            {
                Sprite icon = index < _relicIcons.Length && _relicIcons[index] != null ? _relicIcons[index].sprite : null;
                _relicPopupIcon.sprite = icon;
                _relicPopupIcon.enabled = icon != null;
                _relicPopupIcon.preserveAspect = true;
            }

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
        /// 1.0.b 초안 05 의 표시를 옮겼다. "체력" 글자는 피그마 디자인의 하트 그림이 대신한다.
        /// </summary>
        public void ShowPlayerHealth(int health, int maxHealth, int pendingCost)
        {
            if (_playerHealthLabel != null)
            {
                string cost = string.Empty;

                if (pendingCost > 0 && _visual != null)
                {
                    cost = Colored("-" + pendingCost, _visual.CostColor);
                }

                _playerHealthLabel.text = health + cost + MaxPart(maxHealth);
            }

            SetBar(_playerHealthBar, health - Mathf.Max(0, pendingCost), maxHealth);
        }

        /// <summary>적 체력을 적는다. 0 아래면 음수 그대로 적는다(오버킬 표시).</summary>
        public void ShowEnemyHealth(int health, int maxHealth)
        {
            if (_enemyHealthLabel != null)
            {
                _enemyHealthLabel.text = health + MaxPart(maxHealth);
            }

            SetBar(_enemyHealthBar, health, maxHealth);
        }

        /// <summary>체력 숫자의 "/최대" 부분. 피그마처럼 회색으로 적는다.</summary>
        private string MaxPart(int maxHealth)
        {
            return _visual != null ? Colored("/" + maxHealth, _visual.HealthMaxColor) : "/" + maxHealth;
        }

        /// <summary>
        /// 몬스터 일러스트 자리를 칠한다. 그림이 있으면 흰색으로 비율을 지켜 그리고, 없으면 대신 색을 깐다.
        /// 자리는 9조각 칸 그림으로 만들어져 늘여 그리기(Sliced)라 비율 지키기가 먹지 않았다.
        /// 몬스터 그림이 세로로 늘어나 2026년 11월 1일 그림을 그대로 그리기(Simple)로 바꿨다.
        /// </summary>
        private void PaintEnemyArt()
        {
            if (_enemyArt == null)
            {
                return;
            }

            _enemyArt.sprite = _enemySprite;
            _enemyArt.type = Image.Type.Simple;
            _enemyArt.preserveAspect = true;
            // 스파인을 그릴 때는 그림 칸을 비워 두고 자리와 공격 연출만 쓴다.
            _enemyArt.color = _enemySpineData != null
                ? Color.clear
                : (_enemySprite != null
                    ? Color.white
                    : (_visual != null ? _visual.EnemyArtStandIn : _enemyArt.color));
        }

        /// <summary>지금 몬스터 자리에서 움직이는 스파인 자료. 그림을 그리면 null 이다.</summary>
        public SkeletonDataAsset EnemySpineData
        {
            get { return _enemySpineData; }
        }

        /// <summary>몬스터 스파인을 그리는 쪽. 검사가 켜졌는지 본다.</summary>
        public SkeletonGraphic EnemySpine
        {
            get { return _enemySpine; }
        }

        /// <summary>지금 되풀이하는 몬스터 스파인 애니메이션 이름. 없으면 빈 글이다.</summary>
        public string EnemySpineAnimationName
        {
            get
            {
                if (_enemySpineData == null || _enemySpineMotion == null || _enemySpineMotion.AnimationState == null)
                {
                    return string.Empty;
                }

                Spine.TrackEntry entry = _enemySpineMotion.AnimationState.GetTrack(0);
                return entry != null && entry.Animation != null ? entry.Animation.Name : string.Empty;
            }
        }

        /// <summary>
        /// 몬스터 스파인을 켜거나 끈다. 자료가 있으면 그 몬스터로 바꾸고 설정의 애니메이션을 되풀이한다.
        /// 덤과 더머는 팀원이 Spine 4.3 으로 만들었다(2026년 11월 13일 원재). 자료가 없으면 그림을 그린다.
        /// </summary>
        private void ShowEnemySpine(SkeletonDataAsset data)
        {
            if (_enemySpine == null || _enemySpineMotion == null || data == null)
            {
                _enemySpineData = null;
                if (_enemySpine != null)
                {
                    _enemySpine.gameObject.SetActive(false);
                }

                return;
            }

            _enemySpine.gameObject.SetActive(true);
            bool rebuilt = false;
            if (_enemySpine.skeletonDataAsset != data || !_enemySpine.IsValid || _enemySpineMotion.AnimationState == null)
            {
                _enemySpine.skeletonDataAsset = data;
                _enemySpineMotion.Initialize(true);
                rebuilt = true;
            }

            if (!_enemySpine.IsValid || _enemySpineMotion.AnimationState == null)
            {
                _enemySpine.gameObject.SetActive(false);
                _enemySpineData = null;
                return;
            }

            _enemySpineData = data;
            string animationName = _visual != null ? _visual.EnemySpineAnimation : "animation";
            Spine.SkeletonData skeletonData = data.GetSkeletonData(true);
            // 몬스터 정보는 턴마다 다시 그린다. 같은 몬스터에 같은 애니메이션이 돌고 있으면 처음부터 다시 틀지 않는다(2026년 11월 13일 코덱스 점검).
            Spine.TrackEntry current = _enemySpineMotion.AnimationState.GetTrack(0);
            bool playing = !rebuilt && current != null && current.Animation != null && current.Animation.Name == animationName;
            if (!playing && skeletonData != null && skeletonData.FindAnimation(animationName) != null)
            {
                _enemySpineMotion.AnimationState.SetAnimation(0, animationName, true);
            }

            FitEnemySpine(skeletonData);
        }

        /// <summary>
        /// 스파인의 기본 자세 상자(내보낸 파일의 skeleton x, y, width, height)를 몬스터 자리 가운데에 비율을 지켜 맞춘다.
        /// 그림 칸의 비율 지키기와 같은 방식이다. 숨 쉬듯 커지는 움직임이 자리를 넘지 않게 설정의 비율만큼 줄인다.
        /// </summary>
        private void FitEnemySpine(Spine.SkeletonData skeletonData)
        {
            if (skeletonData == null || skeletonData.Width <= 0f || skeletonData.Height <= 0f || _layout == null)
            {
                return;
            }

            // 스파인 좌표 1 이 화면 좌표로 얼마인지. 자료 배율에 캔버스의 기준 픽셀을 곱한 것이 스파인의 메시 배율이다.
            float unit = _enemySpineData.scale * (_enemySpine.canvas != null ? _enemySpine.canvas.referencePixelsPerUnit : 100f);
            float width = skeletonData.Width * unit;
            float height = skeletonData.Height * unit;
            float fit = _visual != null ? _visual.EnemySpineFit : 0.9f;
            float scale = Mathf.Min(_layout.EnemyArtSize.x / width, _layout.EnemyArtSize.y / height) * fit;

            RectTransform rect = _enemySpine.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            rect.localScale = new Vector3(scale, scale, 1f);
            float centerX = (skeletonData.X + skeletonData.Width * 0.5f) * unit;
            float centerY = (skeletonData.Y + skeletonData.Height * 0.5f) * unit;
            rect.anchoredPosition = new Vector2(-centerX * scale, -centerY * scale);
        }

        /// <summary>지금 몬스터 일러스트 자리에 그린 그림. 없으면 null 이다.</summary>
        public Sprite EnemySprite
        {
            get { return _enemySprite; }
        }

        /// <summary>몬스터 그림을 늘이지 않고 제 비율로 그리는지.</summary>
        public bool EnemyArtKeepsAspect
        {
            get { return _enemyArt != null && _enemyArt.type == Image.Type.Simple && _enemyArt.preserveAspect; }
        }

        /// <summary>몬스터 정보와 일러스트 자리를 채운다.</summary>
        public void ShowEnemy(CombatEnemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            // 몬스터 그림이 있으면 그림을, 없으면 색 칸에 이름을 적는다(2026년 10월 31일 팀원 그림 `c887667`).
            _enemySprite = _visual != null ? _visual.GetEnemyArt(enemy.Kind) : null;
            ShowEnemySpine(_visual != null ? _visual.GetEnemySpine(enemy.Kind) as SkeletonDataAsset : null);
            PaintEnemyArt();
            SetText(_enemyArtLabel, _enemySprite != null || _enemySpineData != null ? string.Empty : enemy.Name);
            // 정보 칸은 적 이름과 설명 대신 다음 턴 행동을 보인다(2026년 10월 29일).
            SetText(_enemyNameLabel, _visual != null ? _visual.NextActionTitle : "다음 행동");
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
        /// 정보 칸에 적는 글. 다음 턴 행동과 그 설명, 그 아래에 장군 설명(방패병), 방어도, 기세.
        /// full 이 아니면 소환수 칸에 들어가게 상태만 짧게 적는다.
        /// </summary>
        private string DescribeEnemy(CombatEnemy enemy, bool full)
        {
            List<string> lines = new List<string>();

            if (full && enemy.NextAction != null)
            {
                lines.Add(Colored(DescribeAction(enemy.NextAction), _visual != null ? _visual.NextActionColor : Color.white));
                if (!string.IsNullOrEmpty(enemy.NextAction.Description))
                {
                    lines.Add(enemy.NextAction.Description);
                }

                lines.Add(string.Empty);
            }

            // 장군은 방패병 수와 피해 감소를 적은 설명(GeneralRule.DescriptionFormat)을 함께 보인다.
            if (full && enemy.Monster == MonsterKind.General && !string.IsNullOrEmpty(enemy.Description))
            {
                lines.Add(Colored(enemy.Description, _visual != null ? _visual.BuffColor : Color.white));
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

            return string.Join("\n", lines).TrimEnd('\n');
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
        /// 원판에서 몬스터로 공격 레이저를 쏜다. 초안 06 의 "태그 선택 UI 에서 공격 효과가 출발하여 적 캐릭터에 닿는 방식".
        /// 처음에는 태그 색 구슬을 날렸고, 2026년 10월 30일 레이저로 바꿨다.
        /// </summary>
        public IEnumerator FireLaser(Color color)
        {
            return FireLaser(color, null);
        }

        /// <summary>
        /// 그 적에게 레이저를 쏜다. 원판 가운데에서 뻗어 적 가운데에 닿고, 잠시 머물다 가늘어지며 사라진다.
        /// 뻗기 시작할 때 발사음을 낸다.
        /// </summary>
        public IEnumerator FireLaser(Color color, CombatEnemy target)
        {
            return FireLaser(color, target, null);
        }

        /// <summary>
        /// 그 태그로 공격하는 레이저. 레이저가 적에 닿는 순간 태그 이펙트를 터뜨리고 태그 소리를 낸다(2026년 11월 1일 원재).
        /// 태그 소리가 있으면 레이저 발사음은 내지 않는다.
        /// </summary>
        public IEnumerator FireLaser(Color color, CombatEnemy target, SymbolTagType? tag)
        {
            if (_laser == null || _layout == null)
            {
                yield break;
            }

            RectTransform rect = _laser.rectTransform;
            Vector2 from = TopLeftToAnchored(_layout.DialCenter);
            Vector2 to = TopLeftToAnchored(GetEnemyCenter(target));
            Vector2 delta = to - from;
            float length = delta.magnitude;
            float thickness = _layout.LaserThickness;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = from;
            rect.localScale = Vector3.one;
            rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            float mix = _visual != null ? _visual.LaserWhiteMix : 0.3f;
            Color tint = Color.Lerp(color, Color.white, mix);
            tint.a = 1f;
            _laser.color = tint;
            _laser.raycastTarget = false;
            _laser.gameObject.SetActive(true);
            _laser.transform.SetAsLastSibling();

            bool tagSound = tag.HasValue && _tagAttackFx != null && _tagAttackFx.HasSound(tag.Value);
            if (!tagSound && _sfxAudio != null && _laserClip != null && _soundVolume > 0f)
            {
                _sfxAudio.PlayOneShot(_laserClip, (_visual != null ? _visual.LaserSoundVolume : 0.7f) * _soundVolume);
            }

            // 뻗는다. 빠르게 나가다 끝에서 느려진다.
            float grow = _visual != null ? Mathf.Max(0.01f, _visual.LaserGrowDuration) : 0.1f;
            for (float t = 0f; t < grow; t += Time.deltaTime)
            {
                float k = t / grow;
                float eased = 1f - (1f - k) * (1f - k);
                rect.sizeDelta = new Vector2(length * eased, thickness * Mathf.Lerp(0.5f, 1f, eased));
                yield return null;
            }

            rect.sizeDelta = new Vector2(length, thickness);

            // 닿는 순간 태그 이펙트와 소리.
            if (tag.HasValue && _tagAttackFx != null)
            {
                _tagAttackFx.Play(tag.Value, GetEnemyRect(target), _soundVolume);
            }

            // 닿은 채 머문다. 굵기가 살짝 떨린다.
            float hold = _visual != null ? _visual.LaserHoldDuration : 0.12f;
            for (float t = 0f; t < hold; t += Time.deltaTime)
            {
                float pulse = 1f + 0.12f * Mathf.Sin(t * 60f);
                rect.sizeDelta = new Vector2(length, thickness * pulse);
                yield return null;
            }

            // 가늘어지며 사라진다.
            float fade = _visual != null ? Mathf.Max(0.01f, _visual.LaserFadeDuration) : 0.18f;
            for (float t = 0f; t < fade; t += Time.deltaTime)
            {
                float k = t / fade;
                rect.sizeDelta = new Vector2(length, thickness * (1f - k));
                Color c = tint;
                c.a = 1f - k;
                _laser.color = c;
                yield return null;
            }

            _laser.gameObject.SetActive(false);
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

            Vector2 center = GetEnemyCenter(target);
            Vector2 at = TopLeftToAnchored(new Vector2(center.x, center.y - UiScale.Px(40f)));

            // 숫자 그림으로 적을 수 있으면 그것으로 띄운다. 막은 몫은 숫자 아래 작은 글이다.
            // 그림이 빠졌거나 자리 수를 넘으면 아래 글자 피해 숫자로 적는다.
            if (_enemyDamageNumber != null && _enemyDamageNumber.CanShow(damage - blocked))
            {
                string note = string.Empty;
                if (blocked > 0)
                {
                    string blockedLabel = _visual != null ? _visual.BlockedText : "막음";
                    Color blockedColor = _visual != null ? _visual.BlockColor : Color.white;
                    note = Colored(blockedLabel + " " + blocked, blockedColor);
                }

                _enemyDamageNumber.RectTransform.anchoredPosition = at;
                _enemyDamageNumber.Show(damage - blocked, note);
                KeepAboveEffects(_enemyDamageNumber.gameObject);
                yield return PopNumber(_enemyDamageNumber, scale);

                if (shaking != null)
                {
                    yield return shaking;
                }

                yield break;
            }

            if (_enemyDamageLabel != null && _layout != null)
            {
                _enemyDamageLabel.rectTransform.anchoredPosition = at;
            }

            string text = (damage - blocked).ToString();
            if (blocked > 0)
            {
                string blockedText = _visual != null ? _visual.BlockedText : "막음";
                Color blockColor = _visual != null ? _visual.BlockColor : Color.white;
                text += "\n<size=45%>" + Colored(blockedText + " " + blocked, blockColor) + "</size>";
            }

            if (_enemyDamageLabel != null)
            {
                KeepAboveEffects(_enemyDamageLabel.gameObject);
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

        /// <summary>숫자 그림 피해 숫자를 글자 피해 숫자(`Pop`)와 같은 움직임으로 띄운다.</summary>
        /// <summary>
        /// 몬스터 피해 숫자를 태그 공격 이펙트 위에 그린다(2026년 11월 13일 원재).
        /// 파티클 이펙트는 캔버스보다 앞 차례로 그려져 캔버스 안의 피해 숫자를 덮었다.
        /// 피해 숫자에 제 캔버스를 달아 차례를 따로 두고, 바깥 캔버스 차례에 설정의 차례(`DamageSortingOffset`)를 더한다.
        /// 게임 씬은 전투 화면을 다른 캔버스로 옮기므로 띄울 때마다 바깥 캔버스를 다시 찾는다.
        /// </summary>
        private void KeepAboveEffects(GameObject target)
        {
            if (target == null || target.transform.parent == null)
            {
                return;
            }

            Canvas outer = target.transform.parent.GetComponentInParent<Canvas>();
            Canvas root = outer != null ? outer.rootCanvas : null;
            if (root == null)
            {
                return;
            }

            Canvas own = target.GetComponent<Canvas>();
            if (own == null)
            {
                own = target.AddComponent<Canvas>();
            }

            own.overrideSorting = true;
            own.sortingLayerID = root.sortingLayerID;
            own.sortingOrder = root.sortingOrder + (_visual != null ? _visual.DamageSortingOffset : 50);
        }

        /// <summary>몬스터 피해 숫자가 그려지는 차례. 제 캔버스가 없으면 바깥 캔버스의 차례다. 검사가 공격 이펙트보다 위인지 본다.</summary>
        public int EnemyDamageSortingOrder
        {
            get
            {
                GameObject target = _enemyDamageNumber != null ? _enemyDamageNumber.gameObject : null;
                Canvas canvas = target != null ? target.GetComponentInParent<Canvas>(true) : null;
                return canvas != null ? (canvas.overrideSorting || canvas.isRootCanvas ? canvas.sortingOrder : canvas.rootCanvas.sortingOrder) : 0;
            }
        }

        private IEnumerator PopNumber(DamageNumberView number, float scale)
        {
            RectTransform rect = number.RectTransform;
            CanvasGroup group = number.Group;
            Vector2 home = rect.anchoredPosition;
            float rise = _layout != null ? _layout.DamageRise : 90f;
            float duration = _visual != null ? _visual.DamageShowDuration : 0.8f;

            number.gameObject.SetActive(true);
            number.transform.SetAsLastSibling();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float grow = t < 0.2f ? Mathf.Lerp(0.6f, 1f, t / 0.2f) : 1f;
                rect.localScale = Vector3.one * scale * grow;
                rect.anchoredPosition = home + new Vector2(0f, rise * t);
                if (group != null)
                {
                    group.alpha = t < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            rect.anchoredPosition = home;
            rect.localScale = Vector3.one;
            number.gameObject.SetActive(false);
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

            PlaceTopLeft(_playerPortrait, _layout.PlayerPortraitPosition, _layout.PlayerPortraitSize);
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

            if (_laser != null)
            {
                RectTransform rect = _laser.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(0f, _layout.LaserThickness);
                _laser.gameObject.SetActive(false);
            }

            // 피해 숫자는 몬스터 일러스트 가운데와 플레이어 체력 칸 위에 띄운다.
            PlaceCenteredOn(_enemyDamageLabel, new Vector2(
                _layout.EnemyArtPosition.x + _layout.EnemyArtSize.x * 0.5f,
                _layout.EnemyArtPosition.y + _layout.EnemyArtSize.y * 0.4f), new Vector2(_layout.EnemyArtSize.x, UiScale.Px(120f)));
            PlaceCenteredOn(_playerDamageLabel, new Vector2(
                _layout.PlayerHealthPosition.x + _layout.PlayerHealthSize.x * 0.5f,
                _layout.PlayerHealthPosition.y - UiScale.Px(50f)), new Vector2(_layout.PlayerHealthSize.x, UiScale.Px(100f)));

            if (_enemyDamageNumber != null)
            {
                RectTransform rect = _enemyDamageNumber.RectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                _enemyDamageNumber.Configure(_layout, _visual);
                _enemyDamageNumber.gameObject.SetActive(false);
            }

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
                    _reels[i].SetBackground(_visual.ReelBackColor);
                    _reels[i].SetFrameMaterial(_visual.ReelFrameMaterial);
                    _reels[i].SetFrameStyle(_visual);
                }
            }

            // 체력 칸은 피그마 체력 틀 그림이라 색을 입히지 않는다.
            Paint(_relicPanel, _visual.RelicPanelColor);
            Paint(_relicPopup, _visual.PopupColor);
            Paint(_enemyInfo, _visual.PanelColor);
            PaintEnemyArt();
            Paint(_dial, _visual.DialColor);
            Paint(_playerHealthBar, _visual.PlayerBarColor);
            Paint(_enemyHealthBar, _visual.EnemyBarColor);

            Style(_playerHealthLabel, _visual.HealthFontSize, _visual.TextColor);
            Style(_enemyHealthLabel, _visual.HealthFontSize, _visual.TextColor);
            Style(_enemyArtLabel, _visual.NameFontSize, _visual.TextColor);
            Style(_enemyNameLabel, _visual.NameFontSize, _visual.TextColor);
            Style(_enemyInfoLabel, _visual.BodyFontSize, _visual.SubTextColor);
            ShrinkToFit(_enemyInfoLabel, _visual.BodyFontSize, _visual.EnemyInfoMinFontSize);
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

        /// <summary>
        /// 글이 칸을 넘칠 것 같으면 글자를 줄인다. 넘치지 않으면 max 그대로다(TMP 자동 크기).
        /// 장군의 행동 설명과 방패병 설명이 함께 들어가 몬스터 정보 칸을 넘쳤다(2026년 10월 29일).
        /// </summary>
        private static void ShrinkToFit(TMP_Text label, float max, float min)
        {
            if (label == null)
            {
                return;
            }

            label.enableAutoSizing = true;
            label.fontSizeMax = max;
            label.fontSizeMin = Mathf.Min(min, max);
            label.overflowMode = TextOverflowModes.Overflow;
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
