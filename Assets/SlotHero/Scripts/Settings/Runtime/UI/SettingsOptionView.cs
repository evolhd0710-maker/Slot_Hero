using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Settings.UI
{
    /// <summary>
    /// 드롭다운 목록의 선택지 한 칸.
    /// 기획서에 드롭다운 규격이 없어 항목 칸과 같은 색과 글자로 맞췄다.
    /// 지금 고른 선택지만 고른 탭처럼 진하게 칠한다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 배경 Image, 자식에 TMP_Text.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SettingsOptionView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;

        private RectTransform _rectTransform;
        private SettingsVisualConfig _visual;
        private Action<SettingsOptionView> _onClicked;
        private bool _selected;
        private bool _hovering;
        private bool _pressed;

        /// <summary>몇 번째 선택지인지. 설정 값이 이것이다.</summary>
        public int Index { get; private set; }

        /// <summary>지금 고른 선택지인지.</summary>
        public bool IsSelected
        {
            get { return _selected; }
        }

        /// <summary>칸에 적힌 글.</summary>
        public string Text
        {
            get { return _label != null ? _label.text : string.Empty; }
        }

        public Button Button
        {
            get { return _button; }
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

        /// <summary>선택지 글과 고른 상태를 반영한다.</summary>
        public void Bind(
            string text,
            int index,
            bool selected,
            Vector2 size,
            SettingsVisualConfig visual,
            Action<SettingsOptionView> onClicked)
        {
            Index = index;
            _selected = selected;
            _visual = visual;
            _onClicked = onClicked;
            _hovering = false;
            _pressed = false;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RectTransform.sizeDelta = size;

            if (_label != null)
            {
                _label.text = text ?? string.Empty;

                if (visual != null)
                {
                    _label.fontSize = visual.ItemFontSize;
                }
            }

            Refresh();
            gameObject.name = "Option_" + index;
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
            if (_visual == null)
            {
                return;
            }

            if (_background != null)
            {
                Color baseColor = _selected ? _visual.SelectedOptionColor : _visual.ItemColor;
                _background.color = _visual.ApplyBrightness(baseColor, _hovering, _pressed);
            }

            if (_label != null)
            {
                _label.color = _selected ? _visual.SelectedOptionTextColor : _visual.ItemTextColor;
            }
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
