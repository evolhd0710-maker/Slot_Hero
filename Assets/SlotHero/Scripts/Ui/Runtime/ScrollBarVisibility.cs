using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Ui
{
    /// <summary>
    /// 스크롤 창의 오른쪽 스크롤 바를 내용이 창보다 길 때만 보이게 한다.
    /// 스크롤할 만큼 분량이 있으면 바가 있어야 한다(2026년 10월 22일 원재).
    ///
    /// 유니티의 `AutoHide` 를 쓰지 않는다. 그것은 편집 모드에서 늘 "스크롤이 필요하다" 로 보아
    /// 씬 검사와 화면 캡처에 바가 늘 보인다. 여기서는 내용과 창의 높이를 직접 비교한다.
    ///
    /// 구성: 스크롤 창(뷰포트)과 같은 오브젝트에 붙는다. 바는 그 창의 자식으로 오른쪽 끝에 세로로 붙는다.
    /// </summary>
    [ExecuteAlways]
    public class ScrollBarVisibility : MonoBehaviour
    {
        [Tooltip("바를 붙인 스크롤.")]
        [SerializeField] private ScrollRect _scroll;

        [Tooltip("내용이 창보다 이만큼 넘게 길어야 바를 보인다. 반올림 오차로 바가 깜박이지 않게 둔다.")]
        [SerializeField] private float _tolerance = 0.5f;

        /// <summary>바를 붙인 스크롤.</summary>
        public ScrollRect Scroll
        {
            get { return _scroll; }
        }

        /// <summary>지금 바가 보이는지.</summary>
        public bool IsShowing
        {
            get
            {
                Scrollbar bar = _scroll != null ? _scroll.verticalScrollbar : null;
                return bar != null && bar.gameObject.activeSelf;
            }
        }

        private void LateUpdate()
        {
            Refresh();
        }

        /// <summary>내용과 창의 높이를 견주어 바를 켜거나 끈다. 내용 높이를 바꾼 직후에 불러도 된다.</summary>
        public void Refresh()
        {
            if (_scroll == null || _scroll.verticalScrollbar == null)
            {
                return;
            }

            RectTransform viewport = _scroll.viewport != null ? _scroll.viewport : (RectTransform)_scroll.transform;
            RectTransform content = _scroll.content;
            bool show = _scroll.vertical && content != null
                && content.rect.height > viewport.rect.height + _tolerance;

            GameObject bar = _scroll.verticalScrollbar.gameObject;
            if (bar.activeSelf != show)
            {
                bar.SetActive(show);
            }
        }
    }
}
