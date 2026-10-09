using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Title.UI
{
    /// <summary>
    /// 타이틀 메뉴 항목 하나.
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 버튼 상태를 따른다.
    /// 마우스를 올리면 밝아지고, 누르면 어두워지면서 2퍼센트 내려가고,
    /// 누를 수 없으면 투명도가 40퍼센트가 된다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button,
    /// 자식 Content 아래에 글자를 둔다.
    /// 누를 때 내려가는 것은 Content 뿐이라 항목의 자리 계산과 어긋나지 않는다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TitleMenuItemView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("참조")]
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TMP_Text _label;

        private TitleVisualConfig _visual;
        private TitleMenuKind _kind;
        private int _index = -1;
        private bool _interactable = true;
        private bool _hovering;
        private bool _pressed;
        private string _disabledReason = string.Empty;
        private float _pressedOffset;

        /// <summary>항목을 눌렀을 때. 어떤 종류인지 넘긴다.</summary>
        public event Action<TitleMenuKind> Clicked;

        /// <summary>
        /// 마우스가 올라가거나 벗어났을 때. 올라간 상태와 누를 수 없는 까닭을 함께 넘긴다.
        /// 04 화면 공통 규칙 이 비활성 이유를 적게 하므로 그 문구를 띄우는 데 쓴다.
        /// </summary>
        public event Action<TitleMenuKind, bool, string> HoverChanged;

        /// <summary>이 항목의 종류.</summary>
        public TitleMenuKind Kind
        {
            get { return _kind; }
        }

        /// <summary>몇 번째 항목인지.</summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>지금 누를 수 있는지.</summary>
        public bool Interactable
        {
            get { return _interactable; }
        }

        public RectTransform RectTransform
        {
            get { return (RectTransform)transform; }
        }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                // 상태 표현을 이 스크립트가 직접 하므로 Button 의 색 전환은 쓰지 않는다.
                _button.transition = Selectable.Transition.None;
                _button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }
        }

        /// <summary>항목 하나를 담는다.</summary>
        public void Bind(TitleMenuEntry entry, int index, Vector2 itemSize, TitleVisualConfig visual)
        {
            _kind = entry.Kind;
            _index = index;
            _visual = visual;
            _pressedOffset = visual != null ? itemSize.y * visual.PressedOffsetRatio : 0f;

            if (_label != null)
            {
                _label.text = entry.DisplayName;
                _label.alignment = TextAlignmentOptions.Left;

                if (visual != null)
                {
                    _label.fontSize = visual.MenuFontSize;
                }
            }

            Refresh();
        }

        /// <summary>누를 수 있는지와 누를 수 없는 까닭을 정한다.</summary>
        public void SetInteractable(bool value, string disabledReason)
        {
            _interactable = value;
            _disabledReason = disabledReason == null ? string.Empty : disabledReason;

            if (_button != null)
            {
                _button.interactable = value;
            }

            if (!value)
            {
                _hovering = false;
                _pressed = false;
            }

            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            Refresh();

            // 누를 수 없는 항목에 올라갔을 때도 알린다. 까닭을 띄워야 하기 때문이다.
            if (HoverChanged != null)
            {
                HoverChanged(_kind, true, _disabledReason);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            Refresh();

            if (HoverChanged != null)
            {
                HoverChanged(_kind, false, _disabledReason);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
            {
                return;
            }

            _pressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            Refresh();
        }

        /// <summary>지금 상태에 맞춰 다시 그린다.</summary>
        public void Refresh()
        {
            if (_visual == null)
            {
                return;
            }

            if (_label != null)
            {
                _label.color = _visual.GetMenuTextColor(_interactable, _hovering, _pressed);
            }

            if (_content != null)
            {
                // 04 화면 공통 규칙 · 버튼 클릭 중에는 2퍼센트 아래로 내려간다
                float drop = _pressed && _interactable ? -_pressedOffset : 0f;
                _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, drop);
            }
        }

        private void HandleClicked()
        {
            if (_interactable && Clicked != null)
            {
                Clicked(_kind);
            }
        }
    }
}
