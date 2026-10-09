using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.RunResult.UI
{
    /// <summary>
    /// 줄을 쌓아 보여 주는 칸 하나.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의
    /// 최종 구성, 이번 런 기록, 해금 및 도전과제 세 칸이 모두 이것을 쓴다.
    ///
    /// 세 칸의 생김새가 같아 하나로 만들었다.
    /// 줄이 칸보다 많으면 그 칸 안에서만 세로로 스크롤한다.
    /// </summary>
    public class RunResultPanelView : MonoBehaviour
    {
        [Header("칸")]
        [SerializeField] private Image _panel;

        [Header("내용")]
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TMP_Text _label;

        private int _lineCount;

        /// <summary>칸 배경. 자리를 잡는 쪽이 쓴다.</summary>
        public Image Panel
        {
            get { return _panel; }
        }

        /// <summary>내용이 담기는 자리. 자리를 잡는 쪽이 쓴다.</summary>
        public RectTransform Content
        {
            get { return _content; }
        }

        /// <summary>지금 담긴 줄 수. 빈 줄도 센다.</summary>
        public int LineCount
        {
            get { return _lineCount; }
        }

        /// <summary>칸에 줄을 채운다.</summary>
        public void SetLines(
            IList<ResultLine> lines,
            RunResultLayoutConfig layout,
            RunResultVisualConfig visual,
            Vector2 panelSize)
        {
            _lineCount = lines != null ? lines.Count : 0;

            if (_label != null)
            {
                _label.text = RunResultText.ToRichText(lines, visual);

                if (visual != null)
                {
                    _label.fontSize = visual.BodyFontSize;
                    _label.color = visual.TextColor;
                }
            }

            if (_panel != null && visual != null)
            {
                _panel.color = visual.PanelColor;
            }

            ResizeContent(layout, panelSize);
        }

        /// <summary>줄 수에 맞춰 내용 높이를 잡고 스크롤이 필요한지 정한다.</summary>
        public void ResizeContent(RunResultLayoutConfig layout, Vector2 panelSize)
        {
            if (layout == null)
            {
                return;
            }

            // 글이 저절로 접혀 줄이 늘 수 있으므로 실제로 그려진 높이를 함께 본다.
            float byLines = layout.GetContentHeight(panelSize, _lineCount);
            float byText = byLines;

            if (_label != null)
            {
                float textHeight = _label.preferredHeight + layout.PanelTopPadding * 2f;
                if (textHeight > byText)
                {
                    byText = textHeight;
                }
            }

            // 보이는 칸은 늘 칸 크기 그대로 둔다. 여기를 늘리면 글이 옆 칸까지 넘어간다.
            // 늘어나는 것은 그 안에서 굴러가는 글 쪽이다.
            //
            // byText 에는 위아래 여백이 들어 있고 보이는 칸은 그 여백만큼 이미 줄어 있으므로
            // 굴러가는 높이에서 여백을 뺀다. 그래야 GetLinesInPanel 이 센 줄 수와 맞는다.
            if (_scroll != null)
            {
                if (_scroll.content != null)
                {
                    float scrolled = byText - layout.PanelTopPadding * 2f;
                    _scroll.content.sizeDelta = new Vector2(_scroll.content.sizeDelta.x, scrolled);
                }

                _scroll.vertical = byText > panelSize.y + 0.5f;
                _scroll.horizontal = false;
                _scroll.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>줄 수만 보고 스크롤이 필요한지. 글이 접히는 것은 세지 않는다.</summary>
        public bool NeedsScroll(RunResultLayoutConfig layout, Vector2 panelSize)
        {
            return layout != null && layout.NeedsScroll(panelSize, _lineCount);
        }
    }
}
