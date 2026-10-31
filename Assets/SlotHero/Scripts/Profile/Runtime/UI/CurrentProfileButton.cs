using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Profile.UI
{
    /// <summary>
    /// 타이틀 화면 오른쪽 위의 현재 프로필 버튼.
    /// 인게임 화면 기획서 v0.2 / 05 타이틀 화면 의
    /// "현재 프로필명을 표시한다. 입력 시 프로필 선택 화면으로 이동한다"를 맡는다.
    ///
    /// 이 스크립트는 버튼의 표시와 입력까지만 맡는다.
    /// 눌러서 열리는 프로필 선택 화면은 06장을 따르는 쪽이 만든다.
    ///
    /// 와이어프레임에서 이 칸은 테두리만 있고 배경이 그대로 비친다.
    /// 그래서 배경 Image 와 테두리 Image 를 따로 두었다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CurrentProfileButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("참조")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _fill;
        [SerializeField] private Image _border;
        [SerializeField] private TMP_Text _label;

        [Header("설정")]
        [SerializeField] private ProfileLayoutConfig _layout;
        [SerializeField] private ProfileVisualConfig _visual;

        private RectTransform _rectTransform;
        private bool _hovering;
        private bool _pressed;

        /// <summary>버튼을 눌렀을 때. 프로필 선택 화면을 여는 쪽이 받는다.</summary>
        public event Action Clicked;

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

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>기획서 05장의 자리를 실제 RectTransform 에 적용한다.</summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            RectTransform rect = RectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = _layout.CurrentButtonSize;
            rect.anchoredPosition = new Vector2(
                _layout.CurrentButtonPosition.x,
                -_layout.CurrentButtonPosition.y);

            if (_label != null)
            {
                // 글은 왼쪽 정렬이고 세로로는 가운데에 놓인다.
                RectTransform labelRect = _label.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.offsetMin = new Vector2(_layout.CurrentButtonPadding, 0f);
                labelRect.offsetMax = new Vector2(-_layout.CurrentButtonPadding, 0f);
            }

            ApplyStyle();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_fill != null)
            {
                _fill.color = _visual.CurrentButtonFillColor;
            }

            if (_label != null)
            {
                _label.fontSize = _visual.CurrentButtonFontSize;
                _label.color = _visual.CurrentButtonTextColor;
                _label.alignment = TextAlignmentOptions.Left;
            }

            Refresh();
        }

        /// <summary>프로필 목록을 받아 지금 쓰는 프로필명을 적는다.</summary>
        public void Show(ProfileList list)
        {
            if (_label != null)
            {
                _label.text = ProfileText.CurrentProfile(_visual, list);
            }
        }

        /// <summary>프로필명을 바로 적는다.</summary>
        public void SetName(string name)
        {
            if (_label == null)
            {
                return;
            }

            _label.text = string.IsNullOrEmpty(name) && _visual != null
                ? _visual.NoProfileText
                : name;
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
            _pressed = false;
            Refresh();
        }

        /// <summary>테두리 밝기를 지금 상태에 맞춘다.</summary>
        private void Refresh()
        {
            if (_border == null || _visual == null)
            {
                return;
            }

            Color baseColor = _visual.CurrentButtonBorderColor;
            float brightness = 1f;

            if (_pressed)
            {
                brightness = _visual.PressedBrightness;
            }
            else if (_hovering)
            {
                brightness = _visual.HoverBrightness;
            }

            _border.color = new Color(
                baseColor.r * brightness,
                baseColor.g * brightness,
                baseColor.b * brightness,
                baseColor.a);
        }

        private void HandleClick()
        {
            if (Clicked != null)
            {
                Clicked();
            }
        }
    }
}
