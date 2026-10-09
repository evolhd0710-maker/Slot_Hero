using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Settings.UI
{
    /// <summary>
    /// 분류 탭 하나.
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 의
    /// "선택된 탭만 진하게 표시한다"를 맡는다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 배경 Image, 자식에 TMP_Text.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SettingsTabView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;

        private RectTransform _rectTransform;
        private SettingsVisualConfig _visual;
        private Action<SettingsTabView> _onClicked;
        private bool _selected;
        private bool _hovering;
        private bool _pressed;

        /// <summary>이 탭이 맡은 분류.</summary>
        public SettingsTab Tab { get; private set; }

        /// <summary>몇 번째 탭인지.</summary>
        public int Index { get; private set; }

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

        /// <summary>탭 내용과 크기를 반영한다.</summary>
        public void Bind(
            SettingsTab tab,
            int index,
            Vector2 size,
            SettingsVisualConfig visual,
            Action<SettingsTabView> onClicked)
        {
            Tab = tab;
            Index = index;
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
                _label.text = tab != null ? tab.DisplayName : string.Empty;
                _label.fontSize = visual.ButtonFontSize;
            }

            Refresh();
            gameObject.name = "Tab_" + (tab != null ? tab.Id : "빈칸");
        }

        /// <summary>고른 탭인지 정한다.</summary>
        public void SetSelected(bool value)
        {
            _selected = value;
            Refresh();
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
                _background.color = _visual.GetTabColor(_selected, _hovering, _pressed);
            }

            if (_label != null)
            {
                _label.color = _visual.GetTabTextColor(_selected);
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
