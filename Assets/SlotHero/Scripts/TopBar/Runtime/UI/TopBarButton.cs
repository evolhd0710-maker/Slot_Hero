using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.TopBar.UI
{
    /// <summary>
    /// 상단 표시줄 우측의 버튼 하나.
    /// 상단 UI 바 기획서 v0.2 / 04 버튼 표기 상세 의 아이콘 60 × 60, 누르는 영역 64 × 64 를 따르고,
    /// 06 상호작용 의 클릭 동작과 마우스 호버 팝업, 비활성 조건을 맡는다.
    ///
    /// 상태 표현은 인게임 화면 기획서 04 화면 공통 규칙 의 버튼 상태를 따른다.
    /// 아이콘이 작아 눌렀을 때 내려가는 연출은 넣지 않고 밝기만 바꾼다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 자식에 아이콘 Image.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TopBarButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("무엇을 여는 버튼인지")]
        [SerializeField] private TopBarButtonKind _kind = TopBarButtonKind.Map;

        [Tooltip("마우스를 올렸을 때 띄울 문구. 06 상호작용 의 텍스트 팝업.")]
        [SerializeField] private string _tooltipText = "지도";

        [Header("참조")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TopBarVisualConfig _visual;

        private RectTransform _rectTransform;
        private bool _interactable = true;
        private bool _hovering;
        private bool _pressed;

        /// <summary>버튼을 눌렀을 때.</summary>
        public event Action<TopBarButton> Clicked;

        /// <summary>마우스가 올라가거나 벗어났을 때.</summary>
        public event Action<TopBarButton, bool> HoverChanged;

        /// <summary>이 버튼이 맡은 항목.</summary>
        public TopBarButtonKind Kind
        {
            get { return _kind; }
        }

        /// <summary>마우스를 올렸을 때 띄울 문구.</summary>
        public string TooltipText
        {
            get { return _tooltipText; }
            set { _tooltipText = value; }
        }

        /// <summary>지금 누를 수 있는지.</summary>
        public bool Interactable
        {
            get { return _interactable; }
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

        /// <summary>누르는 영역과 아이콘 크기를 맞춘다. 표시줄 자리 설정이 부른다.</summary>
        public void ApplySize(float hitSize, float iconSize)
        {
            RectTransform.sizeDelta = new Vector2(hitSize, hitSize);

            if (_icon != null)
            {
                RectTransform iconRect = _icon.rectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);

                // 지도 아이콘처럼 정사각형이 아닌 그림도 60 안에 들어오게 한다.
                _icon.preserveAspect = true;
            }

            Refresh();
        }

        /// <summary>누를 수 있는지를 정한다. 06 상호작용 의 비활성 조건에 맞춰 바깥에서 부른다.</summary>
        public void SetInteractable(bool value)
        {
            _interactable = value;

            if (!value)
            {
                _pressed = false;
            }

            if (_button != null)
            {
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
                HoverChanged(this, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            Refresh();

            if (HoverChanged != null)
            {
                HoverChanged(this, false);
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
            if (_icon == null)
            {
                return;
            }

            _icon.color = _visual == null
                ? Color.white
                : _visual.GetButtonTint(_interactable, _hovering, _pressed);
        }

        private void HandleClick()
        {
            if (!_interactable)
            {
                return;
            }

            if (Clicked != null)
            {
                Clicked(this);
            }
        }
    }
}
