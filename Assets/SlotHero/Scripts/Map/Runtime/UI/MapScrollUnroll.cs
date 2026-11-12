using System;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Ui;

namespace SlotHero.Map.UI
{
    /// <summary>
    /// 효과음과 이펙트 목록 E05 두루마리 펼치기와 그 반대인 말기.
    /// 맵 화면을 열 때 가운데에 나란히 말려 있던 두루마리 둘이 양쪽으로 벌어지며 종이가 펼쳐지고,
    /// 닫을 때는 그것을 거꾸로 돌려 다시 만다.
    ///
    /// 조각 그림은 모두 지도_양피지와 같은 판에 제자리로 놓여 있어 양피지와 같은 크기로 겹쳐 그린다.
    ///   1. 두루마리 둘이 말린 자리(`MapVisualConfig.RolledLeftArt`, `RolledRightArt`)에서 펼친 자리(`OpenLeftArt`, `OpenRightArt`)로 간다
    ///   2. 가는 동안 말린 두루마리 그림이 펼친 두루마리 크기로 가늘어지고, 끝 무렵 투명해지며 아래의 펼친 두루마리 그림이 드러난다
    ///   3. 종이(`펼침_가운데`)는 제자리에 두고 두 두루마리 사이만 보이게 가림판을 넓힌다
    ///   4. 다 펼치면 조각을 끄고 다 펼친 양피지(`Parchment`)를 켠다. 마지막 모습은 연출이 없을 때와 같다
    ///   5. 노드와 간선이 짧게 나타난다. 다 나타날 때까지 노드를 누를 수 없다
    ///
    /// 말 때는 노드와 간선을 바로 감추고 1 ~ 4 를 거꾸로 돌린다(`MapVisualConfig.RollUpSeconds`).
    /// 다 말면 맵 화면을 끄라고 알린다.
    ///
    /// 연출은 게임이 돌 때만 움직인다. 편집 모드 검사에서는 바로 다 펼친 모습이 되고, 말지 않고 바로 닫힌다.
    /// </summary>
    public class MapScrollUnroll : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private MapVisualConfig _visual;

        [Tooltip("다 펼친 양피지. 펼치거나 마는 동안 끄고 다 펼치면 켠다.")]
        [SerializeField] private GameObject _parchment;

        [Tooltip("펼치거나 마는 동안만 켜는 조각 묶음.")]
        [SerializeField] private GameObject _pieces;

        [Tooltip("종이를 두 두루마리 사이만큼만 보이게 하는 가림판. RectMask2D 가 붙어 있다.")]
        [SerializeField] private RectTransform _paperMask;

        [Tooltip("가림판 안의 종이(`펼침_가운데`). 가림판이 움직여도 제자리에 있게 거꾸로 옮긴다.")]
        [SerializeField] private RectTransform _paper;

        [SerializeField] private Image _leftOpen;
        [SerializeField] private Image _rightOpen;
        [SerializeField] private Image _leftRolled;
        [SerializeField] private Image _rightRolled;

        [Tooltip("노드와 간선을 담은 뷰포트의 CanvasGroup. 다 펼친 뒤 나타나게 하고 말 때 감춘다.")]
        [SerializeField] private CanvasGroup _mapContent;

        private bool _playing;
        private bool _rollingUp;
        private float _time;
        private Action _rolledUp;

        /// <summary>펼치는 중이거나 노드가 나타나는 중인지. 이 동안에는 노드를 누를 수 없다.</summary>
        public bool IsPlaying
        {
            get { return _playing; }
        }

        /// <summary>닫으려고 마는 중인지.</summary>
        public bool IsRollingUp
        {
            get { return _rollingUp; }
        }

        /// <summary>처음부터 펼친다. 게임이 돌지 않거나 그림이 없으면 바로 다 펼친 모습이 된다.</summary>
        public void Play()
        {
            CancelRollUp();

            if (!Application.isPlaying || !isActiveAndEnabled || !HasPieces() || _visual.UnrollSeconds <= 0f)
            {
                Finish();
                return;
            }

            ShowPieces();

            _playing = true;
            _time = 0f;
            Apply(0f);

            // 펼치는 중 화면을 누르면 바로 다 펼친 모습이 된다(2026년 11월 1일 원재).
            EffectSkip.Begin(this, Finish);
        }

        /// <summary>
        /// 두루마리를 다시 만다. 다 말면 rolledUp 을 부른다. 맵 화면은 그쪽에서 끈다.
        /// 펼치는 도중이면 지금 펼친 만큼에서부터 만다.
        /// 말 수 없으면(편집 모드, 그림 없음, 0초) 아무것도 하지 않고 거짓을 돌려준다. 그때는 부른 쪽이 바로 닫는다.
        /// </summary>
        public bool PlayRollUp(Action rolledUp)
        {
            if (!Application.isPlaying || !isActiveAndEnabled || !HasPieces() || _visual.RollUpSeconds <= 0f)
            {
                return false;
            }

            // 펼치는 도중이면 그 자리에서 거꾸로 간다. 노드가 나타나는 중이면 다 펼친 것으로 본다.
            float opened = 1f;
            if (_playing && _visual.UnrollSeconds > 0f)
            {
                opened = Mathf.Clamp01(_time / _visual.UnrollSeconds);
            }

            _playing = false;
            ShowPieces();

            _rollingUp = true;
            _rolledUp = rolledUp;
            _time = (1f - opened) * _visual.RollUpSeconds;
            Apply(opened);

            // 마는 중 화면을 누르면 바로 다 만 것으로 친다(2026년 11월 1일 원재).
            EffectSkip.Begin(this, CompleteRollUp);
            return true;
        }

        /// <summary>마는 것을 멈춘다. 다 말았다고 알리지 않는다. 마는 도중에 다시 열 때 쓴다.</summary>
        public void CancelRollUp()
        {
            _rollingUp = false;
            _rolledUp = null;
            EffectSkip.End(this);
        }

        /// <summary>
        /// 펼치거나 마는 중이면 바로 끝 모습으로 넘긴다. 마는 중이면 다 말았다고 알린다.
        /// 노드 반짝임을 건너뛸 때 이어지는 말기까지 함께 넘기려고 맵 화면이 부른다.
        /// </summary>
        public void SkipToEnd()
        {
            if (_rollingUp)
            {
                CompleteRollUp();
                return;
            }

            if (_playing)
            {
                Finish();
            }
        }

        /// <summary>바로 다 펼친 모습으로 둔다.</summary>
        public void Finish()
        {
            _playing = false;
            CancelRollUp();

            if (_pieces != null)
            {
                _pieces.SetActive(false);
            }

            if (_parchment != null)
            {
                _parchment.SetActive(true);
            }

            SetMapContent(1f, true);
        }

        private bool HasPieces()
        {
            return _visual != null
                && _pieces != null && _paperMask != null && _paper != null
                && _leftOpen != null && _rightOpen != null && _leftRolled != null && _rightRolled != null
                && _leftRolled.sprite != null;
        }

        /// <summary>다 펼친 양피지를 끄고 조각을 켠다. 노드와 간선은 감추고 누르지 못하게 한다.</summary>
        private void ShowPieces()
        {
            if (_parchment != null)
            {
                _parchment.SetActive(false);
            }

            _pieces.SetActive(true);
            SetMapContent(0f, false);
        }

        private void Update()
        {
            if (_rollingUp)
            {
                UpdateRollUp();
                return;
            }

            if (!_playing)
            {
                return;
            }

            _time += EffectClock.Delta;

            float unrollSeconds = _visual.UnrollSeconds;
            if (_time < unrollSeconds)
            {
                Apply(_time / unrollSeconds);
                return;
            }

            // 다 펼쳤다. 조각을 끄고 다 펼친 양피지를 켠 뒤 노드를 나타낸다.
            if (_pieces.activeSelf)
            {
                Apply(1f);
                _pieces.SetActive(false);

                if (_parchment != null)
                {
                    _parchment.SetActive(true);
                }
            }

            float fadeSeconds = _visual.UnrollNodeFadeSeconds;
            float fade = fadeSeconds <= 0f ? 1f : Mathf.Clamp01((_time - unrollSeconds) / fadeSeconds);

            if (fade < 1f)
            {
                SetMapContent(fade, false);
                return;
            }

            Finish();
        }

        /// <summary>펼치기를 거꾸로 돌린다. 펼친 정도가 1 에서 0 으로 간다.</summary>
        private void UpdateRollUp()
        {
            _time += EffectClock.Delta;

            float seconds = _visual.RollUpSeconds;
            float done = seconds <= 0f ? 1f : Mathf.Clamp01(_time / seconds);
            Apply(1f - done);

            if (done < 1f)
            {
                return;
            }

            CompleteRollUp();
        }

        /// <summary>다 말았다. 맵 화면을 끄라고 알린 뒤 다음에 열 때를 위해 다 펼친 모습으로 돌려 둔다.</summary>
        private void CompleteRollUp()
        {
            Action rolledUp = _rolledUp;
            _rollingUp = false;
            _rolledUp = null;
            EffectSkip.End(this);

            if (rolledUp != null)
            {
                rolledUp();
            }

            Finish();
        }

        /// <summary>
        /// 맵 화면이 꺼지면 다 펼친 모습으로 돌려 둔다. 다음에 열 때 처음부터 다시 펼친다.
        /// 마는 도중에 꺼져도 다 말았다고 알리지 않는다. 꺼지는 도중에 그 오브젝트를 다시 끄면 유니티가 오류를 낸다.
        /// </summary>
        private void OnDisable()
        {
            if (_playing || _rollingUp)
            {
                Finish();
            }

            EffectSkip.End(this);
        }

        /// <summary>펼친 정도 t(0 ~ 1)의 모습으로 조각을 놓는다.</summary>
        private void Apply(float t)
        {
            float progress = MapVisualConfig.GetUnrollEase(t);
            Vector2 artSize = _leftRolled.sprite.rect.size;
            Vector2 drawSize = _visual.ParchmentSize;
            float rolledAlpha = MapVisualConfig.GetRolledAlpha(t, _visual.UnrollRolledFadeStart);

            float left = PlaceRoll(_leftRolled, _leftOpen, _visual.RolledLeftArt, _visual.OpenLeftArt,
                progress, rolledAlpha, artSize, drawSize);
            float right = PlaceRoll(_rightRolled, _rightOpen, _visual.RolledRightArt, _visual.OpenRightArt,
                progress, rolledAlpha, artSize, drawSize);

            // 종이는 두 두루마리 가운데 사이만 보인다. 가림판이 움직인 만큼 종이를 거꾸로 옮겨 제자리에 둔다.
            float middle = (left + right) * 0.5f;
            _paperMask.anchoredPosition = new Vector2(middle, 0f);
            _paperMask.sizeDelta = new Vector2(Mathf.Max(0f, right - left), drawSize.y);
            _paper.anchoredPosition = new Vector2(-middle, 0f);
            _paper.sizeDelta = drawSize;
        }

        /// <summary>
        /// 한쪽 두루마리를 놓는다. 말린 그림과 펼친 그림이 늘 같은 가운데에 같은 크기로 겹친다.
        /// 돌려주는 값은 그 두루마리 가운데의 화면 가로 자리다.
        /// </summary>
        private static float PlaceRoll(Image rolled, Image open, Rect rolledArt, Rect openArt,
            float progress, float rolledAlpha, Vector2 artSize, Vector2 drawSize)
        {
            Vector2 center = Vector2.Lerp(rolledArt.center, openArt.center, progress);
            Vector2 size = Vector2.Lerp(rolledArt.size, openArt.size, progress);
            Vector2 local = MapVisualConfig.ArtToLocal(center, artSize, drawSize);

            PlacePiece(rolled, rolledArt, size, local, artSize, drawSize);
            PlacePiece(open, openArt, size, local, artSize, drawSize);

            Color color = rolled.color;
            color.a = rolledAlpha;
            rolled.color = color;

            return local.x;
        }

        /// <summary>
        /// 판 하나를 그 안 그림의 가운데를 축으로 놓는다. 그림이 size 크기가 되게 늘이고 줄인다.
        /// </summary>
        private static void PlacePiece(Image image, Rect art, Vector2 size, Vector2 local, Vector2 artSize, Vector2 drawSize)
        {
            RectTransform rect = image.rectTransform;
            Vector2 artCenter = art.center;

            rect.sizeDelta = drawSize;
            rect.pivot = new Vector2(artCenter.x / artSize.x, 1f - artCenter.y / artSize.y);
            rect.anchoredPosition = local;

            float scaleX = art.width > 0f ? size.x / art.width : 1f;
            float scaleY = art.height > 0f ? size.y / art.height : 1f;
            rect.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private void SetMapContent(float alpha, bool interactive)
        {
            if (_mapContent == null)
            {
                return;
            }

            _mapContent.alpha = alpha;
            _mapContent.interactable = interactive;
            _mapContent.blocksRaycasts = interactive;
        }
    }
}
