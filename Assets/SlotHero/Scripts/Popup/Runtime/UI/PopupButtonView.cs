using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Popup.UI
{
    /// <summary>
    /// 팝업 버튼 하나.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 의 보조 버튼과 주 버튼을 모두 맡는다.
    /// 되돌릴 수 없는 조작이면 04 화면 공통 규칙 의 강조 버튼 색을 쓴다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 배경 Image, 자식에 TMP_Text.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PopupButtonView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;

        private RectTransform _rectTransform;
        private PopupVisualConfig _visual;
        private Action<PopupButtonView> _onClicked;
        private bool _hovering;
        private bool _pressed;

        /// <summary>이 버튼이 맡은 내용.</summary>
        public PopupButtonSpec Spec { get; private set; }

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

        /// <summary>버튼 내용과 크기를 반영한다.</summary>
        public void Bind(
            PopupButtonSpec spec,
            Vector2 size,
            PopupVisualConfig visual,
            Action<PopupButtonView> onClicked)
        {
            Spec = spec;
            _visual = visual;
            _onClicked = onClicked;
            _hovering = false;
            _pressed = false;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RectTransform.sizeDelta = size;

            if (_label != null && visual != null)
            {
                _label.text = spec.Label;
                _label.fontSize = visual.ButtonFontSize;
                _label.color = visual.GetButtonTextColor(spec.Emphasized);
            }

            Refresh();
            gameObject.name = "PopupButton_" + spec.Id;
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
            Refresh();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
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
            if (_background == null || _visual == null)
            {
                return;
            }

            _background.color = _visual.GetButtonColor(Spec.Emphasized, true, _hovering, _pressed);
        }

        private void HandleClick()
        {
            if (_onClicked != null)
            {
                _onClicked(this);
            }
        }
    }
}
