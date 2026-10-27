using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Ui;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 회전 연출용 큰 슬롯. 2026년 10월 27일 원재가 정했다.
    ///
    ///   - 회전이 시작되면 화면 가운데에 6열 × 5행 슬롯을 크게 띄운다. 뒤는 어둡게 깐다
    ///   - 다섯 행이 함께 위에서 아래로 굴러간다. 처음에 빠르다가 점점 느려져 왼쪽 열부터 차례로 멈춘다
    ///   - 가운데 행이 결과 줄이다. 멈추면 그 줄에 실제 결과가 온다. 위아래 두 행씩은 보여 주기용이다
    ///   - 재굴림 때는 여섯 열을 다 보이고 다시 도는 열만 굴린다. 나머지 열은 처음부터 서 있다
    ///   - 다 멈추면 잠깐 보인 뒤 사라지면서 결과가 원래 릴 칸에 나타난다
    ///   - 도는 동안 마우스를 누르거나 스페이스바를 누르면 바로 결과로 건너뛴다
    ///
    /// 결과는 이미 정해져 있다(`CombatBattle`). 여기서는 보여 주기만 한다.
    /// 위아래 칸에 채우는 문양은 연출용이라 게임 결과와 상관이 없다.
    /// </summary>
    public class SlotSpinOverlay : MonoBehaviour
    {
        /// <summary>보이는 행 수. 가운데가 결과 줄이다.</summary>
        public const int Rows = 5;

        /// <summary>열 하나에 둔 칸 수. 보이는 다섯에 굴러 들어오는 하나를 더했다.</summary>
        public const int CellsPerColumn = Rows + 1;

        /// <summary>결과 줄의 행. 위에서 0 이다.</summary>
        public const int ResultRow = 2;

        [Header("설정")]
        [SerializeField] private CombatLayoutConfig _layout;
        [SerializeField] private CombatVisualConfig _visual;

        [Header("화면")]
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _dim;
        [SerializeField] private Image _machine;

        [Tooltip("열마다 잘라 보이는 칸. RectMask2D 가 붙어 있다.")]
        [SerializeField] private RectTransform[] _columns = new RectTransform[0];

        [Tooltip("열마다 바탕.")]
        [SerializeField] private Image[] _columnBacks = new Image[0];

        [Tooltip("칸 그림. 열 × CellsPerColumn 차례로 늘어놓는다.")]
        [SerializeField] private Image[] _cells = new Image[0];

        [Tooltip("위 두 행과 아래 두 행을 어둡게 덮는다.")]
        [SerializeField] private Image _topShade;
        [SerializeField] private Image _bottomShade;

        [Header("결과 줄 테두리")]
        [SerializeField] private Image _lineGlow;
        [SerializeField] private Image _lineFrame;
        [SerializeField] private Image _lineInner;
        [SerializeField] private TMP_Text _leftMarker;
        [SerializeField] private TMP_Text _rightMarker;

        [SerializeField] private TMP_Text _skipHint;

        private List<ReelSymbol>[] _columnStrips = new List<ReelSymbol>[0];
        private float[] _positions = new float[0];
        private ICombatIconSource _icons;
        private CombatRandom _random;

        /// <summary>도는 중인지.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>열 수.</summary>
        public int ColumnCount
        {
            get { return _columns.Length; }
        }

        /// <summary>
        /// 그 열 · 행 칸에 지금 보이는 문양. 열과 행은 0 부터, 행은 위에서부터다.
        /// 멈췄을 때만 정확하다. 검사가 결과 줄을 확인하는 데 쓴다.
        /// </summary>
        public ReelSymbol GetShown(int column, int row)
        {
            if (column < 0 || column >= _columnStrips.Length || _columnStrips[column] == null)
            {
                return new ReelSymbol();
            }

            int at = Mathf.FloorToInt(_positions[column]) + (Rows - 1 - row);
            List<ReelSymbol> strip = _columnStrips[column];
            return at >= 0 && at < strip.Count ? strip[at] : new ReelSymbol();
        }

        /// <summary>
        /// 큰 슬롯을 띄워 돌리고, 다 멈추면 `land` 를 부른 뒤 사라진다.
        /// `land` 에서 원래 릴 칸에 결과를 놓는다. 사라지면서 원래 칸에 나타나는 것이다.
        /// </summary>
        public IEnumerator Play(
            IReadOnlyList<ReelSymbol> result, IList<int> spinning, IReadOnlyList<ReelSymbol> deck,
            ICombatIconSource icons, Action land)
        {
            Prepare(result, spinning, deck, icons);

            float fade = _visual != null ? _visual.SpinOverlayFade : 0.18f;
            yield return Fade(0f, 1f, fade);

            // 열마다 멈추는 때와 굴러갈 거리. 다시 도는 열 가운데 왼쪽부터 차례로 늦게, 멀리 돈다.
            List<int> order = new List<int>(spinning);
            order.Sort();

            float baseTime = _visual != null ? _visual.SpinRollDuration : 1.6f;
            float stagger = _visual != null ? _visual.SpinStopStagger : 0.25f;
            float power = _visual != null ? Mathf.Max(1.5f, _visual.SpinEasePower) : 3f;

            float[] times = new float[_columns.Length];
            float[] travels = new float[_columns.Length];
            for (int k = 0; k < order.Count; k++)
            {
                int column = order[k];
                if (column < 0 || column >= _columns.Length)
                {
                    continue;
                }

                times[column] = baseTime + k * stagger;
                travels[column] = _columnStrips[column].Count - CellsPerColumn;
            }

            float elapsed = 0f;
            bool skipped = false;
            bool allStopped = order.Count == 0;

            while (!allStopped)
            {
                if (WantsSkip())
                {
                    skipped = true;
                }

                allStopped = true;
                for (int k = 0; k < order.Count; k++)
                {
                    int column = order[k];
                    if (column < 0 || column >= _columns.Length)
                    {
                        continue;
                    }

                    float t = skipped || times[column] <= 0f ? 1f : Mathf.Clamp01(elapsed / times[column]);

                    // 처음에 빠르고 점점 느려진다. 1 - (1 - t)^n 은 시작 속도가 가장 크고 끝에서 0 이 된다.
                    float eased = 1f - Mathf.Pow(1f - t, power);
                    _positions[column] = travels[column] * eased;
                    DrawColumn(column);

                    if (t < 1f)
                    {
                        allStopped = false;
                    }
                }

                if (!allStopped)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }

            // 결과 줄을 잠깐 보인다. 건너뛰었으면 짧게 끝낸다.
            float hold = skipped ? 0.12f : (_visual != null ? _visual.SpinResultHold : 0.45f);
            yield return Pulse(hold);

            if (land != null)
            {
                land();
            }

            yield return Fade(1f, 0f, fade);
            Hide();
        }

        /// <summary>모든 열을 멈춘 자리로 바로 보낸다. 건너뛰기와 같은 끝 모습이다. 검사도 쓴다.</summary>
        public void SnapToEnd()
        {
            for (int column = 0; column < _columnStrips.Length; column++)
            {
                if (_columnStrips[column] == null)
                {
                    continue;
                }

                _positions[column] = _columnStrips[column].Count - CellsPerColumn;
                DrawColumn(column);
            }
        }

        /// <summary>연출 없이 바로 걷는다.</summary>
        public void Hide()
        {
            IsPlaying = false;
            if (_group != null)
            {
                _group.alpha = 0f;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 열마다 굴러갈 문양 띠를 만든다.
        /// 띠의 [이동 거리 + 2] 자리가 결과다. 멈추면 그 자리가 가운데 행에 온다.
        /// 서 있는 열은 이동 거리 0 이라 처음부터 결과가 가운데에 있다.
        /// </summary>
        public void Prepare(
            IReadOnlyList<ReelSymbol> result, IList<int> spinning, IReadOnlyList<ReelSymbol> deck, ICombatIconSource icons)
        {
            _icons = icons;
            _random = new CombatRandom(Environment.TickCount);

            int count = _columns.Length;
            _columnStrips = new List<ReelSymbol>[count];
            _positions = new float[count];

            int travel = _visual != null ? _visual.SpinTravelCells : 18;
            int extra = _visual != null ? _visual.SpinTravelPerColumn : 3;

            List<int> order = new List<int>(spinning ?? new List<int>());
            order.Sort();

            for (int column = 0; column < count; column++)
            {
                int rank = order.IndexOf(column);
                int distance = rank >= 0 ? travel + rank * extra : 0;

                List<ReelSymbol> strip = new List<ReelSymbol>();
                for (int i = 0; i < distance + CellsPerColumn; i++)
                {
                    strip.Add(Filler(deck));
                }

                if (result != null && column < result.Count)
                {
                    strip[distance + ResultRow] = result[column];
                }

                _columnStrips[column] = strip;
                _positions[column] = 0f;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            IsPlaying = true;

            ApplyLayout();
            ApplyStyle();

            for (int column = 0; column < count; column++)
            {
                DrawColumn(column);
            }

            if (_group != null)
            {
                _group.alpha = 0f;
                _group.blocksRaycasts = true;
            }
        }

        /// <summary>그 열을 지금 위치대로 그린다. 칸 하나는 아래로 흘러 나가고 하나는 위에서 들어온다.</summary>
        private void DrawColumn(int column)
        {
            if (_layout == null || column >= _columnStrips.Length || _columnStrips[column] == null)
            {
                return;
            }

            float cell = _layout.SpinCellSize;
            float p = _positions[column];
            int baseIndex = Mathf.FloorToInt(p);
            float frac = p - baseIndex;
            List<ReelSymbol> strip = _columnStrips[column];

            for (int j = 0; j < CellsPerColumn; j++)
            {
                int at = column * CellsPerColumn + j;
                if (at >= _cells.Length || _cells[at] == null)
                {
                    continue;
                }

                Image image = _cells[at];
                int stripIndex = baseIndex + j;
                ReelSymbol symbol = stripIndex < strip.Count ? strip[stripIndex] : new ReelSymbol();
                Sprite icon = _icons != null && !string.IsNullOrEmpty(symbol.Id) ? _icons.GetIcon(symbol.Id) : null;

                image.sprite = icon;
                image.enabled = icon != null;
                image.preserveAspect = true;

                // 열 아래 끝에서 j 칸 위. 위치가 오를수록(frac) 아래로 내려간다.
                RectTransform rect = image.rectTransform;
                rect.anchoredPosition = new Vector2(0f, (j - frac) * cell + cell * 0.5f);
            }
        }

        private ReelSymbol Filler(IReadOnlyList<ReelSymbol> deck)
        {
            if (deck == null || deck.Count == 0)
            {
                return new ReelSymbol();
            }

            return deck[_random.Range(0, deck.Count)];
        }

        /// <summary>마우스나 스페이스바로 건너뛰는지. 같은 프레임의 다른 쪽과 겹치지 않게 키를 가져간다.</summary>
        private static bool WantsSkip()
        {
            return KeyGate.TryUse(KeyCode.Space) || KeyGate.TryUse(KeyCode.Mouse0);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (_group == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                _group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
                elapsed += Time.deltaTime;
            }

            _group.alpha = to;
        }

        /// <summary>결과 줄 테두리의 빛을 한 번 부풀렸다 가라앉힌다.</summary>
        private IEnumerator Pulse(float duration)
        {
            Color baseGlow = _visual != null ? _visual.SpinLineGlowColor : new Color(1f, 0.8f, 0.3f, 0.35f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float wave = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                if (_lineGlow != null)
                {
                    Color glow = baseGlow;
                    glow.a = Mathf.Clamp01(baseGlow.a + wave * 0.45f);
                    _lineGlow.color = glow;
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            if (_lineGlow != null)
            {
                _lineGlow.color = baseGlow;
            }
        }

        // ---- 자리와 색 ----

        /// <summary>설정의 크기대로 기계, 열, 칸, 결과 줄 테두리 자리를 잡는다.</summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            float cell = _layout.SpinCellSize;
            float gap = _layout.SpinColumnGap;
            float pad = _layout.SpinMachinePadding;
            int count = _columns.Length;

            float columnsWidth = count * cell + (count - 1) * gap;
            float height = Rows * cell;

            if (_dim != null)
            {
                Stretch(_dim.rectTransform);
            }

            Center(_machine != null ? _machine.rectTransform : null,
                new Vector2(columnsWidth + pad * 2f, height + pad * 2f), Vector2.zero);

            for (int i = 0; i < count; i++)
            {
                float x = -columnsWidth * 0.5f + cell * 0.5f + i * (cell + gap);

                if (i < _columnBacks.Length && _columnBacks[i] != null)
                {
                    Center(_columnBacks[i].rectTransform, new Vector2(cell, height), new Vector2(x, 0f));
                }

                if (_columns[i] == null)
                {
                    continue;
                }

                Center(_columns[i], new Vector2(cell, height), new Vector2(x, 0f));

                for (int j = 0; j < CellsPerColumn; j++)
                {
                    int at = i * CellsPerColumn + j;
                    if (at >= _cells.Length || _cells[at] == null)
                    {
                        continue;
                    }

                    // 열의 아래 끝 가운데를 기준으로 놓는다. DrawColumn 이 높이만 바꾼다.
                    RectTransform rect = _cells[at].rectTransform;
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    float inner = cell * _layout.SpinIconRatio;
                    rect.sizeDelta = new Vector2(inner, inner);
                    rect.anchoredPosition = new Vector2(0f, j * cell + cell * 0.5f);
                }
            }

            // 위 두 행과 아래 두 행 위에 어두운 막. 결과 줄만 밝게 남는다.
            float band = cell * ResultRow;
            Center(_topShade != null ? _topShade.rectTransform : null,
                new Vector2(columnsWidth, band), new Vector2(0f, height * 0.5f - band * 0.5f));
            Center(_bottomShade != null ? _bottomShade.rectTransform : null,
                new Vector2(columnsWidth, band), new Vector2(0f, -height * 0.5f + band * 0.5f));

            // 결과 줄 테두리. 바깥 빛, 금테, 안쪽 어두운 선을 겹쳐 두른다.
            float lineWidth = columnsWidth + _layout.SpinLineOverhang * 2f;
            float frame = _layout.SpinLineThickness;
            Center(_lineGlow != null ? _lineGlow.rectTransform : null,
                new Vector2(lineWidth + frame * 6f, cell + frame * 6f), Vector2.zero);
            Center(_lineFrame != null ? _lineFrame.rectTransform : null,
                new Vector2(lineWidth, cell + frame * 2f), Vector2.zero);
            Center(_lineInner != null ? _lineInner.rectTransform : null,
                new Vector2(lineWidth - frame * 2f, cell), Vector2.zero);

            float marker = _layout.SpinMarkerSize;
            Center(_leftMarker != null ? _leftMarker.rectTransform : null,
                new Vector2(marker, marker), new Vector2(-lineWidth * 0.5f - marker * 0.5f, 0f));
            Center(_rightMarker != null ? _rightMarker.rectTransform : null,
                new Vector2(marker, marker), new Vector2(lineWidth * 0.5f + marker * 0.5f, 0f));

            Center(_skipHint != null ? _skipHint.rectTransform : null,
                new Vector2(columnsWidth, UiScale.Px(40f)),
                new Vector2(0f, -height * 0.5f - pad - UiScale.Px(30f)));
        }

        /// <summary>색을 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            Paint(_dim, _visual.SpinOverlayDim);
            Paint(_machine, _visual.SpinMachineColor);
            for (int i = 0; i < _columnBacks.Length; i++)
            {
                Paint(_columnBacks[i], _visual.SpinColumnColor);
            }

            Paint(_topShade, _visual.SpinShadeColor);
            Paint(_bottomShade, _visual.SpinShadeColor);
            Paint(_lineGlow, _visual.SpinLineGlowColor);
            Paint(_lineFrame, _visual.SpinLineColor);
            Paint(_lineInner, _visual.SpinLineInnerColor);

            if (_leftMarker != null)
            {
                _leftMarker.color = _visual.SpinLineColor;
                _leftMarker.text = "▶";
            }

            if (_rightMarker != null)
            {
                _rightMarker.color = _visual.SpinLineColor;
                _rightMarker.text = "◀";
            }

            if (_skipHint != null)
            {
                _skipHint.text = _visual.SpinSkipHint;
                _skipHint.color = _visual.SubTextColor;
            }
        }

        private static void Paint(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
        }

        private static void Center(RectTransform rect, Vector2 size, Vector2 position)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
