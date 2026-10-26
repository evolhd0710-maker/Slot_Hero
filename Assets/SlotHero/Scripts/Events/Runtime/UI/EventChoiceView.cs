using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Events.UI
{
    /// <summary>
    /// 이벤트 선택지 칸 하나.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 선택지 표시 세 가지를 모두 맡는다.
    ///
    /// 결과가 공개된 선택지는 획득을 초록색으로 적고,
    /// 조건을 채우지 못한 선택지는 부족한 항목을 빨간색으로 적으며 누를 수 없고,
    /// 결과를 알 수 없는 선택지는 물음표만 적는다.
    /// 색은 글자 안에 태그로 들어가므로 글자 하나로 칸을 다 채운다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 배경 Image, 자식에 TMP_Text.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class EventChoiceView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;

        private RectTransform _rectTransform;
        private EventVisualConfig _visual;
        private Action<EventChoiceView> _onClicked;
        private Action<string, bool> _onItemHover;

        /// 커서가 올라가 있는 물건 이름의 식별자. 없으면 null 이다.
        private string _hoveredItem;
        private bool _hovering;
        private bool _pressed;
        private float _height;

        /// <summary>이 칸이 맡은 선택지.</summary>
        public EventChoice Choice { get; private set; }

        /// <summary>
        /// 글을 다 담는 데 필요한 칸 높이.
        /// 한 줄이면 설정의 `ChoiceSize.y` 그대로이고, 글이 길면 그보다 크다.
        /// <see cref="Bind"/> 가 글을 넣으면서 재 둔다.
        /// </summary>
        public float Height
        {
            get { return _height; }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = (RectTransform)transform;
                }

                return _rectTransform;
            }
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
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>선택지 내용과 자리를 반영한다. onItemHover 는 글 속 물건 이름에 커서가 들고 날 때 그 식별자를 받는다.</summary>
        public void Bind(
            EventChoice choice,
            EventLayoutConfig layout,
            EventVisualConfig visual,
            Action<EventChoiceView> onClicked,
            Action<string, bool> onItemHover)
        {
            EndItemHover();
            Choice = choice;
            _visual = visual;
            _onClicked = onClicked;
            _onItemHover = onItemHover;
            _hovering = false;
            _pressed = false;

            if (choice == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_label != null && visual != null)
            {
                // 글은 줄바꿈을 켜 둔다. 끄면 긴 선택지가 칸 오른쪽으로 삐져나간다.
                _label.textWrappingMode = TextWrappingModes.Normal;
                _label.text = EventChoiceText.Build(choice, visual.GetTextFormat());
                _label.fontSize = visual.ChoiceFontSize;
                _label.color = visual.GetChoiceTextColor(choice.State);
            }

            _height = Measure(layout, visual);

            if (layout != null)
            {
                RectTransform.sizeDelta = new Vector2(layout.ChoiceSize.x, _height);
            }

            if (_button != null)
            {
                _button.interactable = choice.IsSelectable;
            }

            Refresh();
            gameObject.name = "Choice_" + choice.Id;
        }

        /// <summary>
        /// 글자 크기를 바꾸고 칸 높이를 다시 잰다.
        /// 선택지가 쌓여 칸을 넘칠 때 화면 쪽이 줄여 부른다.
        /// </summary>
        public void SetFontSize(float fontSize, EventLayoutConfig layout, EventVisualConfig visual)
        {
            if (_label == null)
            {
                return;
            }

            _label.fontSize = fontSize;
            _height = Measure(layout, visual);

            if (layout != null)
            {
                RectTransform.sizeDelta = new Vector2(layout.ChoiceSize.x, _height);
            }
        }

        /// <summary>지금 글자 크기.</summary>
        public float FontSize
        {
            get { return _label != null ? _label.fontSize : 0f; }
        }

        /// <summary>
        /// 지금 글이 몇 줄이 되는지 물어 칸 높이를 구한다.
        ///
        /// 칸 폭은 설정이 정한 값을 지키고 세로로만 늘린다.
        /// 기획서 11장이 선택지를 본문과 좌우 대칭으로 두므로 폭은 건드리면 안 된다.
        /// </summary>
        private float Measure(EventLayoutConfig layout, EventVisualConfig visual)
        {
            if (layout == null)
            {
                return _height;
            }

            if (_label == null || visual == null)
            {
                return layout.ChoiceSize.y;
            }

            float width = layout.GetChoiceTextWidth(visual.ChoicePadding);

            // 높이를 0 으로 주면 TMP 가 이 폭에 맞춘 높이를 스스로 재서 돌려준다.
            float textHeight = _label.GetPreferredValues(_label.text, width, 0f).y;
            return layout.GetChoiceHeight(textHeight);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            EndItemHover();
            Refresh();
        }

        /// <summary>
        /// 커서가 글 속 물건 이름(`EventRunner.LinkItem` 의 고리) 위에 오면 그 물건의 아이템 상세를 띄우게 알린다.
        /// 얻거나 잃는 물건이 무엇인지 선택지에서 바로 볼 수 있게 한다(2026년 10월 25일 원재).
        /// </summary>
        public void OnPointerMove(PointerEventData eventData)
        {
            SetHoveredItem(FindItemAt(eventData.position, eventData.enterEventCamera));
        }

        /// <summary>그 화면 자리에 있는 물건 이름의 식별자. 없으면 null 이다.</summary>
        public string FindItemAt(Vector2 screenPoint, Camera camera)
        {
            if (_label == null)
            {
                return null;
            }

            int index = TMP_TextUtilities.FindIntersectingLink(_label, new Vector3(screenPoint.x, screenPoint.y, 0f), camera);
            if (index < 0 || index >= _label.textInfo.linkCount)
            {
                return null;
            }

            return _label.textInfo.linkInfo[index].GetLinkID();
        }

        /// <summary>커서가 올라간 물건을 바꾼다. 바뀌었을 때만 알린다.</summary>
        public void SetHoveredItem(string id)
        {
            if (id == _hoveredItem)
            {
                return;
            }

            EndItemHover();

            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            _hoveredItem = id;
            if (_onItemHover != null)
            {
                _onItemHover(id, true);
            }
        }

        private void OnDisable()
        {
            EndItemHover();
        }

        private void EndItemHover()
        {
            if (_hoveredItem == null)
            {
                return;
            }

            string id = _hoveredItem;
            _hoveredItem = null;
            if (_onItemHover != null)
            {
                _onItemHover(id, false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Choice == null || !Choice.IsSelectable)
            {
                return;
            }

            _pressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pressed)
            {
                return;
            }

            _pressed = false;
            Refresh();
        }

        /// <summary>지금 상태를 화면에 반영한다.</summary>
        public void Refresh()
        {
            if (_background == null || _visual == null || Choice == null)
            {
                return;
            }

            _background.color = _visual.GetPanelColor(Choice.State, _hovering, _pressed);

            // 고른 칸은 어둡게 가라앉으므로 글자를 크림색으로 바꾼다(2026년 10월 21일 코덱스 검토).
            if (_label != null)
            {
                _label.color = _visual.GetChoiceTextColor(Choice.State);
            }
        }

        private void HandleClick()
        {
            if (Choice == null || !Choice.IsSelectable)
            {
                return;
            }

            if (_onClicked != null)
            {
                _onClicked(this);
            }
        }
    }
}
