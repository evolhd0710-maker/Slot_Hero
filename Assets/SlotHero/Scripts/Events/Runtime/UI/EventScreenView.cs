using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Ui;

namespace SlotHero.Events.UI
{
    /// <summary>
    /// 이벤트 화면 표시.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 배치를 맡는다.
    /// 삽화는 화면 위쪽 1200 × 500, 본문은 삽화 아래 왼쪽,
    /// 선택지는 삽화 아래 오른쪽에 세로로 쌓이며,
    /// 본문과 선택지는 화면 가운데를 기준으로 좌우 대칭이다.
    ///
    /// 선택지 수가 이벤트마다 다르므로 칸은 프리팹을 복제해 만든다.
    /// </summary>
    public class EventScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private EventLayoutConfig _layout;
        [SerializeField] private EventVisualConfig _visual;

        [Header("삽화")]
        [SerializeField] private Image _illustration;

        [Tooltip("삽화 그림이 없는 화면에 까는 바탕 그림. 칸 스킨이다.")]
        [SerializeField] private Sprite _illustrationPlaceholder;

        [Header("본문")]
        [SerializeField] private Image _bodyPanel;
        [SerializeField] private TMP_Text _bodyLabel;

        [Header("선택지")]
        [Tooltip("선택지 칸을 담는 빈 RectTransform. 칸은 여기 아래에 만들어진다.")]
        [SerializeField] private RectTransform _choiceLayer;

        [SerializeField] private EventChoiceView _choicePrefab;

        private readonly List<EventChoiceView> _choiceViews = new List<EventChoiceView>();

        // 자리를 잡을 때만 쓰는 임시 목록이다. 매번 새로 만들지 않으려고 들고 있는다.
        private readonly List<EventChoiceView> _shown = new List<EventChoiceView>();
        private readonly List<float> _heights = new List<float>();

        /// <summary>선택지를 눌렀을 때. 선택지 식별자를 넘긴다.</summary>
        public event Action<string> ChoiceClicked;

        /// <summary>한 화면에 놓을 수 있는 선택지 칸의 최대 수.</summary>
        public int MaxChoiceCount
        {
            get { return _layout != null ? _layout.MaxChoiceCount : 0; }
        }

        private void Awake()
        {
            ApplyLayout();
        }

        /// <summary>
        /// 기획서 11장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.IsSymmetric())
            {
                Debug.LogWarning(
                    "이벤트 화면 자리 설정에서 본문과 선택지의 너비가 달라 좌우 대칭이 되지 않는다. " +
                    "기획서는 둘을 화면 가운데 기준으로 대칭으로 두게 한다.",
                    this);
            }

            if (!_layout.MaxChoiceCountFitsBody())
            {
                Debug.LogWarning(
                    "선택지 최대 수가 본문 칸 높이보다 많아 아래로 삐져나간다. " +
                    "지금 설정으로 들어가는 칸은 " + _layout.GetChoiceCountInBody() + "개다.",
                    this);
            }

            if (_illustration != null)
            {
                RectTransform rect = _illustration.rectTransform;
                AnchorToTopCenter(rect, new Vector2(0.5f, 1f));
                rect.sizeDelta = _layout.IllustrationSize;
                rect.anchoredPosition = _layout.GetIllustrationPosition();
            }

            if (_bodyPanel != null)
            {
                // 본문은 화면 가운데에서 왼쪽으로 뻗으므로 피벗을 오른쪽 위에 둔다.
                RectTransform rect = _bodyPanel.rectTransform;
                AnchorToTopCenter(rect, new Vector2(1f, 1f));
                rect.sizeDelta = _layout.BodySize;
                rect.anchoredPosition = _layout.GetBodyPosition();
            }

            if (_bodyLabel != null && _visual != null)
            {
                RectTransform rect = _bodyLabel.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.offsetMin = new Vector2(_visual.BodyPadding.x, _visual.BodyPadding.y);
                rect.offsetMax = new Vector2(-_visual.BodyPadding.x, -_visual.BodyPadding.y);
            }

            if (_choiceLayer != null)
            {
                // 선택지는 화면 가운데에서 오른쪽으로 뻗으므로 피벗을 왼쪽 위에 둔다.
                AnchorToTopCenter(_choiceLayer, new Vector2(0f, 1f));
                _choiceLayer.sizeDelta = Vector2.zero;
                _choiceLayer.anchoredPosition = Vector2.zero;
            }

            ApplyStyle();
            PlaceChoices();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_bodyPanel != null)
            {
                _bodyPanel.color = _visual.PanelColor;
            }

            if (_bodyLabel != null)
            {
                // 기획서 스토리는 길어 본문 칸을 넘칠 수 있다. 넘칠 때만 가장 작은 크기까지 줄인다.
                _bodyLabel.fontSize = _visual.BodyFontSize;
                _bodyLabel.enableAutoSizing = true;
                _bodyLabel.fontSizeMax = _visual.BodyFontSize;
                _bodyLabel.fontSizeMin = Mathf.Min(_visual.BodyMinFontSize, _visual.BodyFontSize);
                _bodyLabel.color = _visual.TextColor;
            }
        }

        /// <summary>페이지 하나를 그대로 그린다.</summary>
        public void Show(EventPage page, IEventIllustrationSource illustrations)
        {
            if (page == null)
            {
                return;
            }

            ShowIllustration(illustrations != null
                ? illustrations.GetIllustration(page.IllustrationId)
                : null);

            if (_bodyLabel != null)
            {
                // 결과 문장의 얻은 것은 초록, 잃은 것은 빨강으로 칠한다. 선택지 칸과 같은 색이다.
                _bodyLabel.text = _visual != null
                    ? EventChoiceText.ColorMarks(page.BodyText, _visual.GetTextFormat())
                    : EventChoiceText.StripMarks(page.BodyText);
            }

            BuildChoices(page);
        }

        /// <summary>
        /// 삽화를 놓는다. 그림이 없으면 옅은 바탕 칸으로 돌린다.
        ///
        /// **그림이 없을 때 앞의 그림을 남겨 두면 안 된다.**
        /// 예전에는 새 그림이 있을 때만 갈아 끼워, 그림 없는 이벤트에 앞 이벤트의 그림이 그대로 나올 수 있었다.
        /// 삽화는 1200 × 500 칸을 그대로 채운다. 그림 쪽을 미리 같은 비율로 잘라 둔다.
        /// </summary>
        public void ShowIllustration(Sprite sprite)
        {
            if (_illustration == null)
            {
                return;
            }

            if (sprite != null)
            {
                _illustration.sprite = sprite;
                _illustration.color = Color.white;
                _illustration.type = Image.Type.Simple;
                _illustration.enabled = true;
                return;
            }

            _illustration.sprite = _illustrationPlaceholder;
            _illustration.type = _illustrationPlaceholder != null ? Image.Type.Sliced : Image.Type.Simple;
            _illustration.color = _visual != null
                ? _visual.IllustrationPlaceholderColor
                : new Color(1f, 1f, 1f, 0.12f);
            _illustration.enabled = true;
        }

        /// <summary>지금 놓인 삽화 그림. 바탕 칸이면 바탕 그림이다.</summary>
        public Sprite CurrentIllustration
        {
            get { return _illustration != null ? _illustration.sprite : null; }
        }

        /// <summary>선택지 칸을 페이지에 맞춰 만들고 자리를 잡는다.</summary>
        private void BuildChoices(EventPage page)
        {
            if (_choiceLayer == null || _choicePrefab == null)
            {
                return;
            }

            int count = page.Choices != null ? page.Choices.Count : 0;

            // 칸 수를 넘기는 선택지는 그리지 않는다. 보통은 페이지 쪽에서 이미 잘려 온다.
            int limit = MaxChoiceCount;
            if (limit > 0 && count > limit)
            {
                Debug.LogWarning(
                    "선택지가 " + count + "개라 놓을 수 있는 " + limit + "개를 넘어 뒤쪽을 그리지 않는다.",
                    this);
                count = limit;
            }

            while (_choiceViews.Count < count)
            {
                EventChoiceView view = Instantiate(_choicePrefab, _choiceLayer);
                _choiceViews.Add(view);
            }

            for (int i = 0; i < _choiceViews.Count; i++)
            {
                EventChoiceView view = _choiceViews[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                view.Bind(page.Choices[i], _layout, _visual, HandleChoiceClicked);
            }

            FitChoiceFont();
            PlaceChoices();
        }

        /// <summary>
        /// 선택지가 쌓여 칸을 넘치면 모든 선택지의 글자를 1픽셀씩 함께 줄인다. 가장 작게는 20픽셀이다.
        /// 글이 긴 선택지가 여럿이면 화면 아래로 넘쳐 2026년 10월 5일 원재의 요청으로 넣었다.
        /// 칸마다 크기가 다르면 어색하므로 다 같이 줄인다.
        /// </summary>
        private void FitChoiceFont()
        {
            if (_layout == null || _visual == null)
            {
                return;
            }

            float size = _visual.ChoiceFontSize;
            float min = Mathf.Min(_visual.ChoiceMinFontSize, size);
            float step = UiScale.Px(1f);

            while (true)
            {
                _heights.Clear();
                for (int i = 0; i < _choiceViews.Count; i++)
                {
                    EventChoiceView view = _choiceViews[i];
                    if (view != null && view.gameObject.activeSelf)
                    {
                        _heights.Add(view.Height > 0f ? view.Height : _layout.ChoiceSize.y);
                    }
                }

                if (_heights.Count == 0 || _layout.ChoicesFitBody(_heights) || size - step < min - 0.01f)
                {
                    return;
                }

                size -= step;
                for (int i = 0; i < _choiceViews.Count; i++)
                {
                    EventChoiceView view = _choiceViews[i];
                    if (view != null && view.gameObject.activeSelf)
                    {
                        view.SetFontSize(size, _layout, _visual);
                    }
                }
            }
        }

        /// <summary>
        /// 선택지 칸을 위에서 아래로 늘어놓는다.
        ///
        /// 칸 높이가 저마다 다를 수 있으므로 먼저 전부 재고 그다음에 자리를 잡는다.
        /// 글이 길어 두 줄이 된 칸 뒤로는 그만큼 더 내려가야 한다.
        /// </summary>
        private void PlaceChoices()
        {
            if (_layout == null)
            {
                return;
            }

            _shown.Clear();
            _heights.Clear();

            for (int i = 0; i < _choiceViews.Count; i++)
            {
                EventChoiceView view = _choiceViews[i];
                if (view == null || !view.gameObject.activeSelf)
                {
                    continue;
                }

                _shown.Add(view);

                // 아직 재지 않은 칸은 한 줄로 본다. Bind 를 거치지 않은 칸이 그렇다.
                float height = view.Height > 0f ? view.Height : _layout.ChoiceSize.y;
                _heights.Add(height);
            }

            for (int i = 0; i < _shown.Count; i++)
            {
                RectTransform rect = _shown[i].RectTransform;
                AnchorToTopCenter(rect, new Vector2(0f, 1f));
                rect.sizeDelta = new Vector2(_layout.ChoiceSize.x, _heights[i]);
                rect.anchoredPosition = _layout.GetChoicePosition(_heights, i);
            }

            if (_shown.Count > 0 && !_layout.ChoicesFitBody(_heights))
            {
                Debug.LogWarning(
                    "선택지 " + _shown.Count + "개가 " + _layout.GetChoicesTotalHeight(_heights) +
                    " 만큼 쌓여 본문 칸 높이 " + _layout.BodySize.y + " 를 넘는다. " +
                    "글이 길어 칸이 여러 줄로 늘어난 것이다.",
                    this);
            }
        }

        /// <summary>기준점을 화면 가운데 위에 두고 피벗만 따로 잡는다.</summary>
        private static void AnchorToTopCenter(RectTransform rect, Vector2 pivot)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = pivot;
        }

        private void HandleChoiceClicked(EventChoiceView view)
        {
            if (view == null || view.Choice == null)
            {
                return;
            }

            if (ChoiceClicked != null)
            {
                ChoiceClicked(view.Choice.Id);
            }
        }
    }
}
