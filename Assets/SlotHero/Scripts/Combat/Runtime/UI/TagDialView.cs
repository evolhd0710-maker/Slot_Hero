using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.CurrentBuild;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 태그 UI. 전투 UI 및 연출 초안 / 04 태그 UI 세부.
    ///
    /// 바깥 원은 열 칸이다. 맨 위가 소용돌이고 시계 방향으로 금성, 지구, 화성, 목성, 토성, 천왕성, 해왕성, 명왕성, 수성이다.
    /// 초안 그림의 차례를 그대로 옮겼다. 가운데는 고른 코인이고 누르면 코인 UI 로 돌아간다.
    ///
    /// **소용돌이 칸은 평소에 효과가 없다.** 블랙홀은 아직 기획 중이라 칸만 두었다(2026년 10월 25일).
    /// 유물 사건의 지평선이 있을 때만 켜지고, 누르면 가장 많은 태그로 공격한다.
    /// </summary>
    public class TagDialView : MonoBehaviour
    {
        /// <summary>소용돌이 칸의 자리.</summary>
        public const int VortexIndex = 0;

        /// <summary>바깥 원 칸 수. 태그 아홉과 소용돌이 하나.</summary>
        public const int SectorCount = 10;

        /// <summary>칸 차례. 소용돌이 다음부터 시계 방향이다.</summary>
        public static readonly SymbolTagType[] TagOrder =
        {
            SymbolTagType.Venus,
            SymbolTagType.Earth,
            SymbolTagType.Mars,
            SymbolTagType.Jupiter,
            SymbolTagType.Saturn,
            SymbolTagType.Uranus,
            SymbolTagType.Neptune,
            SymbolTagType.Pluto,
            SymbolTagType.Mercury,
        };

        [SerializeField] private TagSectorView[] _sectors = new TagSectorView[0];

        [Header("가운데")]
        [SerializeField] private Button _centerButton;
        [SerializeField] private Image _centerBack;
        [SerializeField] private Image _centerCoin;
        [SerializeField] private TMPro.TMP_Text _centerLabel;

        [Header("공격 예정")]
        [Tooltip("태그를 고르면 원판 전체가 그 태그 색으로 찬다. 초안 06 의 \"공격 예정(우측 UI 변경)\".")]
        [SerializeField] private Image _charge;

        private readonly List<Action> _clickHandlers = new List<Action>();
        private readonly List<Action<bool>> _hoverHandlers = new List<Action<bool>>();

        /// <summary>태그 칸을 눌렀다. 소용돌이는 알리지 않는다.</summary>
        public event Action<SymbolTagType> TagClicked;

        /// <summary>태그 칸에 마우스가 올라가거나 벗어났다.</summary>
        public event Action<SymbolTagType, bool> TagHoverChanged;

        /// <summary>소용돌이 칸에 마우스가 올라가거나 벗어났다.</summary>
        public event Action<bool> VortexHoverChanged;

        /// <summary>가운데 코인을 눌렀다.</summary>
        public event Action CenterClicked;

        /// <summary>소용돌이 칸을 눌렀다. 사건의 지평선이 있어 켜져 있을 때만 온다.</summary>
        public event Action VortexClicked;

        public IReadOnlyList<TagSectorView> Sectors
        {
            get { return _sectors; }
        }

        private void Awake()
        {
            for (int i = 0; i < _sectors.Length; i++)
            {
                int index = i;
                Action click = () => HandleSectorClicked(index);
                Action<bool> hover = hovering => HandleSectorHover(index, hovering);
                _clickHandlers.Add(click);
                _hoverHandlers.Add(hover);

                if (_sectors[i] != null)
                {
                    _sectors[i].Clicked += click;
                    _sectors[i].HoverChanged += hover;
                }
            }

            if (_centerButton != null)
            {
                _centerButton.onClick.AddListener(HandleCenterClicked);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _sectors.Length && i < _clickHandlers.Count; i++)
            {
                if (_sectors[i] != null)
                {
                    _sectors[i].Clicked -= _clickHandlers[i];
                    _sectors[i].HoverChanged -= _hoverHandlers[i];
                }
            }

            if (_centerButton != null)
            {
                _centerButton.onClick.RemoveListener(HandleCenterClicked);
            }
        }

        /// <summary>그 칸의 태그. 소용돌이면 false 다.</summary>
        public static bool TryGetTag(int sectorIndex, out SymbolTagType tag)
        {
            int at = sectorIndex - 1;
            if (sectorIndex == VortexIndex || at < 0 || at >= TagOrder.Length)
            {
                tag = SymbolTagType.Mercury;
                return false;
            }

            tag = TagOrder[at];
            return true;
        }

        /// <summary>그 태그의 칸 자리.</summary>
        public static int GetSectorIndex(SymbolTagType tag)
        {
            for (int i = 0; i < TagOrder.Length; i++)
            {
                if (TagOrder[i] == tag)
                {
                    return i + 1;
                }
            }

            return VortexIndex;
        }

        /// <summary>칸 각도와 색과 아이콘을 맞춘다.</summary>
        public void Configure(CombatLayoutConfig layout, CombatVisualConfig visual, CurrentBuildVisualConfig tags)
        {
            if (layout == null || visual == null)
            {
                return;
            }

            float sweep = 360f / SectorCount;
            float innerRatio = layout.TagCenterSize / layout.DialSize;

            for (int i = 0; i < _sectors.Length; i++)
            {
                if (_sectors[i] == null)
                {
                    continue;
                }

                // 맨 위 칸의 가운데가 12시에 오게 반 칸 앞에서 시작한다.
                float start = i * sweep - sweep * 0.5f;

                SymbolTagType tag;
                Color color = visual.VortexColor;
                Sprite icon = null;
                string symbol = "@";

                if (TryGetTag(i, out tag) && tags != null)
                {
                    BuildTagVisual tagVisual = tags.GetTagVisual(tag);
                    color = tagVisual.BarColor;
                    icon = tagVisual.Icon;
                    symbol = tagVisual.Symbol;
                }

                _sectors[i].Setup(
                    start, sweep, layout.TagSectorGapDegrees, innerRatio,
                    layout.TagIconDistance, layout.TagIconSize, color, icon, symbol);
                _sectors[i].SetStyle(visual.HoverBrightness, visual.DisabledBrightness, visual.HoverScale);
            }

            if (_centerButton != null)
            {
                RectTransform rect = (RectTransform)_centerButton.transform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(layout.TagCenterSize, layout.TagCenterSize);
                rect.anchoredPosition = Vector2.zero;
            }

            if (_centerBack != null)
            {
                _centerBack.color = visual.DialCenterColor;
            }

            SetCharge(false, Color.clear);
        }

        /// <summary>가운데에 고른 코인을 놓는다. 코인 없이 골랐으면 글자만 둔다.</summary>
        public void SetCenterCoin(Sprite icon, string label)
        {
            if (_centerCoin != null)
            {
                _centerCoin.sprite = icon;
                _centerCoin.enabled = icon != null;
                _centerCoin.preserveAspect = true;
            }

            if (_centerLabel != null)
            {
                _centerLabel.text = icon == null ? label : string.Empty;
            }
        }

        /// <summary>
        /// 태그마다 누를 수 있는지 정한다. 그 태그 문양이 하나도 없으면 끈다.
        /// 소용돌이는 사건의 지평선이 있을 때만 켠다.
        /// </summary>
        public void SetAvailable(Func<SymbolTagType, bool> canUse, bool vortexUsable)
        {
            for (int i = 0; i < _sectors.Length; i++)
            {
                if (_sectors[i] == null)
                {
                    continue;
                }

                SymbolTagType tag;
                bool usable = TryGetTag(i, out tag)
                    ? canUse != null && canUse(tag)
                    : vortexUsable;
                _sectors[i].SetInteractable(usable);
                _sectors[i].ClearHover();
            }
        }

        /// <summary>원판 전체를 그 색으로 채우거나 걷는다.</summary>
        public void SetCharge(bool on, Color color)
        {
            if (_charge == null)
            {
                return;
            }

            _charge.gameObject.SetActive(on);
            _charge.color = color;
            _charge.transform.SetAsLastSibling();
        }

        private void HandleSectorClicked(int index)
        {
            if (_sectors[index] == null || !_sectors[index].Interactable)
            {
                return;
            }

            SymbolTagType tag;
            if (!TryGetTag(index, out tag))
            {
                if (VortexClicked != null)
                {
                    VortexClicked();
                }

                return;
            }

            if (TagClicked != null)
            {
                TagClicked(tag);
            }
        }

        private void HandleSectorHover(int index, bool hovering)
        {
            SymbolTagType tag;
            if (!TryGetTag(index, out tag))
            {
                if (VortexHoverChanged != null)
                {
                    VortexHoverChanged(hovering);
                }

                return;
            }

            if (TagHoverChanged != null)
            {
                TagHoverChanged(tag, hovering);
            }
        }

        private void HandleCenterClicked()
        {
            if (CenterClicked != null)
            {
                CenterClicked();
            }
        }
    }
}
