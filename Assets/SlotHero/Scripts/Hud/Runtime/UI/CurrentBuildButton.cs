using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Hud.UI
{
    /// <summary>
    /// 현재 빌드 버튼.
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 고정 자리와 버튼 상태를 따른다.
    /// 화면 우측 하단 200 × 200 원형이고, 누르면 현재 빌드 화면을 여는 신호를 보낸다.
    ///
    /// 이 스크립트는 버튼의 표시와 입력까지만 맡는다.
    /// 눌러서 열리는 현재 빌드 화면은 같은 기획서 13장을 따르는 쪽이 만든다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button,
    /// 자식 Content 아래에 배경 Image 와 아이콘 Image 를 둔다.
    /// 누를 때 내려가는 것은 Content 뿐이라 버튼의 자리 계산과 어긋나지 않는다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CurrentBuildButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("참조")]
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;

        [Header("스프라이트")]
        [Tooltip("평소의 버튼 배경. 우주 바탕에 푸른 테두리를 두른 원이다.")]
        [SerializeField] private Sprite _backgroundSprite;

        [Tooltip("채도를 뺀 버튼 배경. 04 화면 공통 규칙 의 비활성 표현에 쓴다. 비워 두면 투명도만 낮춘다.")]
        [SerializeField] private Sprite _backgroundDisabledSprite;

        [Tooltip("평소의 아이콘. 혼천의.")]
        [SerializeField] private Sprite _iconSprite;

        [Tooltip("채도를 뺀 아이콘. 비워 두면 투명도만 낮춘다.")]
        [SerializeField] private Sprite _iconDisabledSprite;

        [Header("설정")]
        [SerializeField] private HudVisualConfig _visual;

        private RectTransform _rectTransform;
        private bool _interactable = true;
        private bool _hovering;
        private bool _pressed;
        private string _disabledReason;
        private float _pressedOffset;

        /// <summary>버튼을 눌렀을 때. 현재 빌드 화면을 여는 쪽이 받는다.</summary>
        public event Action Clicked;

        /// <summary>
        /// 마우스가 올라가거나 벗어났을 때. 올라간 상태와 비활성 이유를 함께 넘긴다.
        /// 04 화면 공통 규칙 은 비활성 버튼에 이유를 적게 하므로 그 문구를 띄우는 데 쓴다.
        /// </summary>
        public event Action<bool, string> HoverChanged;

        /// <summary>지금 누를 수 있는지.</summary>
        public bool Interactable
        {
            get { return _interactable; }
        }

        /// <summary>누를 수 없을 때 보여 줄 이유.</summary>
        public string DisabledReason
        {
            get { return _disabledReason; }
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

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>버튼과 아이콘의 크기를 맞춘다. 고정 자리 설정이 부른다.</summary>
        public void ApplySize(float buttonSize, float iconSize)
        {
            RectTransform.sizeDelta = new Vector2(buttonSize, buttonSize);

            if (_content != null)
            {
                _content.anchorMin = new Vector2(0.5f, 0.5f);
                _content.anchorMax = new Vector2(0.5f, 0.5f);
                _content.pivot = new Vector2(0.5f, 0.5f);
                _content.sizeDelta = new Vector2(buttonSize, buttonSize);
            }

            if (_background != null)
            {
                RectTransform backgroundRect = _background.rectTransform;
                backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
                backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
                backgroundRect.pivot = new Vector2(0.5f, 0.5f);
                backgroundRect.anchoredPosition = Vector2.zero;
                backgroundRect.sizeDelta = new Vector2(buttonSize, buttonSize);
            }

            if (_icon != null)
            {
                RectTransform iconRect = _icon.rectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            _pressedOffset = buttonSize * (_visual != null ? _visual.PressedOffsetRatio : 0f);
            Refresh();
        }

        /// <summary>
        /// 버튼을 보이거나 감춘다.
        /// 현재 빌드 화면이 열려 있는 동안에는 아예 보이지 않게 한다.
        /// </summary>
        public void SetVisible(bool value)
        {
            if (gameObject.activeSelf == value)
            {
                return;
            }

            if (!value)
            {
                _hovering = false;
                _pressed = false;
            }

            gameObject.SetActive(value);
        }

        /// <summary>
        /// 누를 수 있는지를 정한다.
        /// 04 화면 공통 규칙 이 비활성 버튼에 이유를 적게 하므로 함께 받는다.
        /// </summary>
        public void SetInteractable(bool value, string disabledReason)
        {
            _interactable = value;
            _disabledReason = value ? null : disabledReason;

            if (!value)
            {
                _pressed = false;
            }

            if (_button != null)
            {
                // 비활성이어도 이유를 띄워야 하므로 눌리는 것만 막고 입력 자체는 받는다.
                _button.interactable = value;
            }

            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            Refresh();

            if (HoverChanged != null)
            {
                HoverChanged(true, _disabledReason);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            Refresh();

            if (HoverChanged != null)
            {
                HoverChanged(false, _disabledReason);
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
            bool useDisabledLook = !_interactable;

            if (_background != null)
            {
                Sprite sprite = useDisabledLook && _backgroundDisabledSprite != null
                    ? _backgroundDisabledSprite
                    : _backgroundSprite;

                if (sprite != null)
                {
                    _background.sprite = sprite;
                }

                _background.color = GetTint();
            }

            if (_icon != null)
            {
                Sprite sprite = useDisabledLook && _iconDisabledSprite != null
                    ? _iconDisabledSprite
                    : _iconSprite;

                if (sprite != null)
                {
                    _icon.sprite = sprite;
                }

                _icon.color = GetTint();
            }

            if (_content != null)
            {
                // 누르고 있는 동안만 버튼 높이의 일정 비율만큼 내려간다.
                float y = _pressed ? -_pressedOffset : 0f;
                _content.anchoredPosition = new Vector2(0f, y);
            }
        }

        private Color GetTint()
        {
            if (_visual == null)
            {
                return Color.white;
            }

            return _visual.GetTint(_interactable, _hovering, _pressed);
        }

        private void HandleClick()
        {
            if (!_interactable)
            {
                return;
            }

            if (Clicked != null)
            {
                Clicked();
            }
        }
    }
}
