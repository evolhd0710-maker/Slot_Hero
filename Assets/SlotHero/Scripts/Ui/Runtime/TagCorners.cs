using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Ui
{
    /// <summary>
    /// 문양 칸의 우상단과 좌하단에 태그 테두리를 얹고, 문양 뒤에 흑요석 판을 깐다.
    ///
    /// 문양은 태그를 둘 가진다. 첫 태그의 테두리를 우상단에, 둘째 태그의 테두리를 좌하단에 둔다(2026년 10월 19일 원재).
    /// 그림은 칸을 꽉 덮는 정사각 그림이고 무늬는 모서리에만 있다. 꾸밈이라 커서를 받지 않는다.
    ///
    /// 문양 뒤에는 행상 진열과 같은 흑요석 판(`문양_바닥`)을 깐다. 문양이 나오는 화면은 모두 같다(2026년 10월 20일 원재).
    /// 테두리는 판 가장자리에 걸치지 않고 판 안쪽으로 들어가 있고, 문양 그림은 테두리와 같은 넓이로 꼭 맞춘다.
    /// 그래서 테두리와 문양 사이가 좁다(2026년 10월 20일 원재).
    /// 판에서 얼마나 들어갈지는 부르는 쪽이 설정 에셋에서 읽어 넘긴다.
    ///
    /// 문양이 아닌 칸(코인, 유물, 빈 칸)에서는 테두리와 판을 감추고 물건 그림을 원래 자리로 되돌린다.
    ///
    /// 현재 빌드와 고르기 칸, 행상 진열, 보상 카드가 함께 쓴다. 어느 그림을 얹을지는 그 칸을 그리는 쪽이 정해 넘긴다.
    /// 행상 진열은 칸 그림이 이미 흑요석 판이라 판 자리를 비워 둔다.
    ///
    /// 테두리 장식이 감기는 모서리 끝에는 그 태그의 행성 기호를 얹는다(2026년 10월 26일 원재).
    /// 우상단 장식에는 첫 태그 기호, 좌하단 장식에는 둘째 태그 기호다. 크기와 자리는 부르는 쪽이 설정 에셋에서 읽어 넘긴다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트, 자식에 칸을 꽉 채운 Image 둘(우상단, 좌하단)과 기호 Image 둘.
    /// 판은 문양 그림보다 먼저 그려져야 하므로 문양 그림 앞자리에 따로 둔다.
    /// 이 루트와 문양 그림은 같은 부모 아래 있어 같은 비율로 안쪽에 맞춰진다.
    /// </summary>
    public class TagCorners : MonoBehaviour
    {
        [Tooltip("첫 태그의 테두리. 무늬가 우상단에 있는 그림이 들어간다.")]
        [SerializeField] private Image _topRight;

        [Tooltip("둘째 태그의 테두리. 무늬가 좌하단에 있는 그림이 들어간다.")]
        [SerializeField] private Image _bottomLeft;

        [Tooltip("우상단 장식 모서리에 얹는 첫 태그의 행성 기호.")]
        [SerializeField] private Image _topRightGlyph;

        [Tooltip("좌하단 장식 모서리에 얹는 둘째 태그의 행성 기호.")]
        [SerializeField] private Image _bottomLeftGlyph;

        [Tooltip("문양 뒤에 까는 흑요석 판. 문양 그림보다 앞자리다. 칸 그림이 이미 판인 행상 진열에서는 비워 둔다.")]
        [SerializeField] private Image _plate;

        [Tooltip("문양 그림. 문양을 보일 때 테두리와 같은 넓이로 맞추고, 감출 때 원래 자리로 되돌린다.")]
        [SerializeField] private RectTransform _symbol;

        // 문양 그림의 원래 자리. 처음 Show 할 때 적는다.
        // 확인용 씬은 문양을 보인 채 저장되므로 씬에 함께 남겨야 다시 열었을 때 원래 자리로 되돌릴 수 있다(2026년 10월 26일 코덱스 점검).
        [SerializeField, HideInInspector] private bool _symbolRectSaved;
        [SerializeField, HideInInspector] private Vector2 _symbolAnchorMin;
        [SerializeField, HideInInspector] private Vector2 _symbolAnchorMax;
        [SerializeField, HideInInspector] private Vector2 _symbolOffsetMin;
        [SerializeField, HideInInspector] private Vector2 _symbolOffsetMax;

        /// <summary>지금 우상단에 놓인 그림. 감췄으면 null 이다.</summary>
        public Sprite TopRight
        {
            get { return _topRight != null && _topRight.enabled ? _topRight.sprite : null; }
        }

        /// <summary>지금 좌하단에 놓인 그림. 감췄으면 null 이다.</summary>
        public Sprite BottomLeft
        {
            get { return _bottomLeft != null && _bottomLeft.enabled ? _bottomLeft.sprite : null; }
        }

        /// <summary>지금 문양 뒤에 깔린 판. 감췄거나 판 자리가 없으면 null 이다.</summary>
        public Sprite Plate
        {
            get { return _plate != null && _plate.enabled ? _plate.sprite : null; }
        }

        /// <summary>
        /// 두 모서리에 그림을 놓고 문양 뒤에 판을 깐다. 그림이 없는 쪽은 감춘다.
        /// 테두리와 문양 그림은 칸의 각 변에서 inset 비율만큼 안쪽에 맞춘다.
        /// </summary>
        public void Show(Sprite topRight, Sprite bottomLeft, Sprite plate, float inset)
        {
            Place(_topRight, topRight);
            Place(_bottomLeft, bottomLeft);
            Place(_plate, plate);

            FitInside((RectTransform)transform, inset);

            if (_symbol != null)
            {
                SaveSymbolRect();
                FitInside(_symbol, inset);
            }
        }

        /// <summary>지금 우상단 장식에 얹은 기호. 감췄으면 null 이다.</summary>
        public Sprite TopRightGlyph
        {
            get { return _topRightGlyph != null && _topRightGlyph.enabled ? _topRightGlyph.sprite : null; }
        }

        /// <summary>지금 좌하단 장식에 얹은 기호. 감췄으면 null 이다.</summary>
        public Sprite BottomLeftGlyph
        {
            get { return _bottomLeftGlyph != null && _bottomLeftGlyph.enabled ? _bottomLeftGlyph.sprite : null; }
        }

        /// <summary>
        /// 장식 모서리 끝에 태그 기호를 얹는다. 그림이 없는 쪽은 감춘다. `Show` 다음에 부른다.
        /// size 는 테두리 한 변에 대한 기호 크기 비율, fromCorner 는 모서리에서 기호 가운데까지의 비율이다.
        /// </summary>
        public void ShowGlyphs(Sprite topRight, Sprite bottomLeft, float size, float fromCorner)
        {
            Place(_topRightGlyph, topRight);
            Place(_bottomLeftGlyph, bottomLeft);
            PlaceGlyph(_topRightGlyph, new Vector2(1f - fromCorner, 1f - fromCorner), size);
            PlaceGlyph(_bottomLeftGlyph, new Vector2(fromCorner, fromCorner), size);
        }

        /// <summary>두 모서리와 기호와 판을 모두 감추고 물건 그림을 원래 자리로 되돌린다. 문양이 아닌 칸에서 부른다.</summary>
        public void Hide()
        {
            Place(_topRight, null);
            Place(_bottomLeft, null);
            Place(_topRightGlyph, null);
            Place(_bottomLeftGlyph, null);
            Place(_plate, null);
            RestoreSymbolRect();
        }

        /// <summary>기호를 테두리 안의 비율 자리(center)에 비율 크기(size)로 놓는다. 칸 크기를 따라 커지고 줄어든다.</summary>
        private static void PlaceGlyph(Image image, Vector2 center, float size)
        {
            if (image == null)
            {
                return;
            }

            float half = Mathf.Clamp(size, 0f, 1f) * 0.5f;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = center - new Vector2(half, half);
            rect.anchorMax = center + new Vector2(half, half);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.preserveAspect = true;
        }

        /// <summary>부모를 꽉 채우되 각 변에서 inset 비율만큼 안쪽으로 들인다.</summary>
        private static void FitInside(RectTransform rect, float inset)
        {
            float clamped = Mathf.Clamp(inset, 0f, 0.45f);
            rect.anchorMin = new Vector2(clamped, clamped);
            rect.anchorMax = new Vector2(1f - clamped, 1f - clamped);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>물건 그림의 원래 자리를 처음 한 번만 적어 둔다.</summary>
        private void SaveSymbolRect()
        {
            if (_symbolRectSaved || _symbol == null)
            {
                return;
            }

            _symbolAnchorMin = _symbol.anchorMin;
            _symbolAnchorMax = _symbol.anchorMax;
            _symbolOffsetMin = _symbol.offsetMin;
            _symbolOffsetMax = _symbol.offsetMax;
            _symbolRectSaved = true;
        }

        private void RestoreSymbolRect()
        {
            if (!_symbolRectSaved || _symbol == null)
            {
                return;
            }

            _symbol.anchorMin = _symbolAnchorMin;
            _symbol.anchorMax = _symbolAnchorMax;
            _symbol.offsetMin = _symbolOffsetMin;
            _symbol.offsetMax = _symbolOffsetMax;
        }

        private static void Place(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
