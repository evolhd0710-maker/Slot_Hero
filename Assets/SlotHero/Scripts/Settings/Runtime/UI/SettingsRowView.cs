using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Settings.UI
{
    /// <summary>
    /// 설정 항목 한 줄.
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 의
    /// "항목 이름을 왼쪽, 현재 값은 오른쪽에 배치한다"를 맡는다.
    ///
    /// 값을 고르는 방법은 기획서에 없어 항목 종류마다 따로 정했다.
    ///   고르는 항목    칸을 누르면 다음 값으로 넘어간다. 오른쪽 클릭은 앞 값이다
    ///   드롭다운 항목  칸을 누르면 선택지 목록이 펼쳐진다. 언어가 이것이다
    ///   수치 항목      칸 가운데의 막대를 끈다. 음소거가 있으면 오른쪽 끝에 버튼이 붙는다
    /// 드롭다운과 막대와 음소거는 원재가 2026년 10월 5일에 정했다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button, 배경 Image,
    /// 자식에 이름 TMP_Text 와 값 TMP_Text, 막대 Slider, 음소거 Button.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SettingsRowView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _valueLabel;

        [Header("수치 항목")]
        [Tooltip("끄는 막대. 수치 항목에서만 켠다.")]
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _sliderTrack;
        [SerializeField] private Image _sliderFill;
        [SerializeField] private Image _sliderHandle;

        [Tooltip("음소거 버튼. 음소거가 있는 수치 항목에서만 켠다.")]
        [SerializeField] private Button _muteButton;
        [SerializeField] private Image _muteBackground;
        [SerializeField] private TMP_Text _muteLabel;

        private RectTransform _rectTransform;
        private SettingsVisualConfig _visual;
        private Action<SettingsRowView, bool> _onChanged;
        private Action<SettingDefinition, float> _onSet;
        private Action<SettingsRowView> _onDropdown;
        private bool _hovering;
        private bool _pressed;
        private bool _muted;

        /// <summary>이 줄이 맡은 항목.</summary>
        public SettingDefinition Definition { get; private set; }

        /// <summary>이 줄의 음소거 값 정의. 음소거가 없으면 null 이다.</summary>
        public SettingDefinition MuteDefinition
        {
            get { return Definition != null ? Definition.GetMuteDefinition() : null; }
        }

        /// <summary>음소거가 켜져 있는지.</summary>
        public bool IsMuted
        {
            get { return _muted; }
        }

        /// <summary>막대. 수치 항목이 아니면 꺼져 있다.</summary>
        public Slider Slider
        {
            get { return _slider; }
        }

        /// <summary>음소거 버튼. 음소거가 없으면 꺼져 있다.</summary>
        public Button MuteButton
        {
            get { return _muteButton; }
        }

        /// <summary>지금 보여 주는 값.</summary>
        public float Value { get; private set; }

        /// <summary>값 칸에 적힌 글.</summary>
        public string ValueText
        {
            get { return _valueLabel != null ? _valueLabel.text : string.Empty; }
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

            if (_slider != null)
            {
                _slider.onValueChanged.AddListener(HandleSliderChanged);
            }

            if (_muteButton != null)
            {
                _muteButton.onClick.AddListener(HandleMuteClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }

            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(HandleSliderChanged);
            }

            if (_muteButton != null)
            {
                _muteButton.onClick.RemoveListener(HandleMuteClicked);
            }
        }

        /// <summary>항목과 지금 값을 반영한다.</summary>
        public void Bind(
            SettingDefinition definition,
            float value,
            bool muted,
            Vector2 size,
            SettingsLayoutConfig layout,
            SettingsVisualConfig visual,
            Action<SettingsRowView, bool> onChanged,
            Action<SettingDefinition, float> onSet,
            Action<SettingsRowView> onDropdown)
        {
            Definition = definition;
            _visual = visual;
            _onChanged = onChanged;
            _onSet = onSet;
            _onDropdown = onDropdown;
            _hovering = false;
            _pressed = false;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RectTransform.sizeDelta = size;

            bool isSlider = definition != null && definition.Kind == SettingKind.Slider;
            bool hasMute = isSlider && definition.HasMute;

            SetActive(_slider != null ? _slider.gameObject : null, isSlider);
            SetActive(_muteButton != null ? _muteButton.gameObject : null, hasMute);

            if (layout != null)
            {
                PlaceParts(size, layout, isSlider, hasMute);
            }

            if (_nameLabel != null && visual != null)
            {
                _nameLabel.text = definition != null ? definition.DisplayName : string.Empty;
                _nameLabel.fontSize = visual.ItemFontSize;
                _nameLabel.color = visual.ItemTextColor;
            }

            if (isSlider && _slider != null)
            {
                _slider.minValue = definition.Min;
                _slider.maxValue = definition.Max;
                _slider.wholeNumbers = definition.Step >= 1f;
                _slider.direction = Slider.Direction.LeftToRight;
            }

            ApplySliderColors();
            SetValue(value);
            SetMuted(muted);
            Refresh();
            gameObject.name = "Row_" + (definition != null ? definition.Id : "빈칸");
        }

        /// <summary>값만 다시 적는다. 막대도 그 자리로 옮긴다. 막대를 옮겨도 다시 알리지 않는다.</summary>
        public void SetValue(float value)
        {
            if (Definition == null)
            {
                return;
            }

            Value = Definition.Clamp(value);

            if (_valueLabel != null)
            {
                string text = Definition.FormatValue(value);

                // 드롭다운은 펼칠 수 있다는 것을 값 뒤에 표시한다.
                if (Definition.Kind == SettingKind.Dropdown && _visual != null)
                {
                    text += _visual.DropdownMark;
                }

                _valueLabel.text = text;

                if (_visual != null)
                {
                    _valueLabel.fontSize = _visual.ItemFontSize;
                    _valueLabel.color = _visual.ItemTextColor;
                }
            }

            if (Definition.Kind == SettingKind.Slider && _slider != null)
            {
                _slider.SetValueWithoutNotify(Definition.Clamp(value));
            }
        }

        /// <summary>음소거 버튼을 지금 값으로 칠한다. 켜져 있으면 막대가 흐려진다.</summary>
        public void SetMuted(bool muted)
        {
            _muted = muted;

            if (_visual == null)
            {
                return;
            }

            if (_muteBackground != null)
            {
                _muteBackground.color = muted ? _visual.MuteOnColor : _visual.MuteOffColor;
            }

            if (_muteLabel != null)
            {
                _muteLabel.text = _visual.MuteText;
                _muteLabel.fontSize = _visual.ItemFontSize;
                _muteLabel.color = muted ? _visual.MuteOnTextColor : _visual.ItemTextColor;
            }

            ApplySliderColors();
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

            // 오른쪽 버튼으로는 앞 값으로 되돌린다. 넘기며 고르는 항목만 그렇다.
            if (eventData != null && eventData.button == PointerEventData.InputButton.Right
                && Definition != null && Definition.Kind == SettingKind.Choice)
            {
                Change(false);
            }
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
            if (_background != null && _visual != null)
            {
                _background.color = _visual.GetItemColor(_hovering, _pressed);
            }
        }

        /// <summary>값 칸의 오른쪽 아래 모서리. 드롭다운 목록을 그 아래로 펼친다.</summary>
        public Vector3 GetValueCornerWorld()
        {
            Vector3[] corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);
            return corners[3];
        }

        private void ApplySliderColors()
        {
            if (_visual == null)
            {
                return;
            }

            float alpha = _muted ? _visual.MutedSliderAlpha : 1f;
            Tint(_sliderTrack, _visual.SliderTrackColor, alpha);
            Tint(_sliderFill, _visual.SliderFillColor, alpha);
            Tint(_sliderHandle, _visual.SliderHandleColor, alpha);
        }

        private static void Tint(Image image, Color color, float alpha)
        {
            if (image != null)
            {
                image.color = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        /// <summary>
        /// 줄 안의 자리를 잡는다. 이름은 왼쪽 끝에 붙는다.
        /// 고르는 항목은 값이 오른쪽 끝에 붙고,
        /// 수치 항목은 막대, 숫자, 음소거 버튼이 차례로 오른쪽 끝까지 늘어선다.
        /// </summary>
        private void PlaceParts(Vector2 size, SettingsLayoutConfig layout, bool isSlider, bool hasMute)
        {
            if (_nameLabel != null)
            {
                RectTransform rect = _nameLabel.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2((isSlider ? layout.SliderLeft : size.x * 0.5f) - layout.ItemPadding, 0f);
                rect.anchoredPosition = new Vector2(layout.ItemPadding, 0f);
            }

            if (!isSlider)
            {
                if (_valueLabel != null)
                {
                    RectTransform rect = _valueLabel.rectTransform;
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = new Vector2(-layout.ItemPadding, 0f);
                    _valueLabel.alignment = TextAlignmentOptions.Right;
                }

                return;
            }

            // 오른쪽 끝부터 왼쪽으로 채운다. 음소거 버튼, 숫자, 막대 차례다.
            float right = size.x - layout.ItemPadding;

            if (hasMute && _muteButton != null)
            {
                RectTransform rect = (RectTransform)_muteButton.transform;
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.sizeDelta = layout.MuteButtonSize;
                rect.anchoredPosition = new Vector2(right, 0f);
                right -= layout.MuteButtonSize.x + layout.SliderGap;
            }

            if (_valueLabel != null)
            {
                RectTransform rect = _valueLabel.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.sizeDelta = new Vector2(layout.SliderValueWidth, 0f);
                rect.anchoredPosition = new Vector2(right, 0f);
                _valueLabel.alignment = TextAlignmentOptions.Right;
                right -= layout.SliderValueWidth + layout.SliderGap;
            }

            if (_slider != null)
            {
                RectTransform rect = (RectTransform)_slider.transform;
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(Mathf.Max(1f, right - layout.SliderLeft), layout.SliderHandleSize);
                rect.anchoredPosition = new Vector2(layout.SliderLeft, 0f);

                // 막대 바탕과 찬 부분은 손잡이보다 얇게 가운데에 둔다.
                FitBar(_sliderTrack, layout.SliderHeight);

                if (_sliderFill != null)
                {
                    FitBar((RectTransform)_sliderFill.transform.parent, layout.SliderHeight);
                }

                if (_sliderHandle != null)
                {
                    RectTransform handle = _sliderHandle.rectTransform;
                    handle.sizeDelta = new Vector2(layout.SliderHandleSize, 0f);
                }
            }
        }

        private static void FitBar(Image image, float height)
        {
            if (image != null)
            {
                FitBar(image.rectTransform, height);
            }
        }

        private static void FitBar(RectTransform rect, float height)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null && target.activeSelf != value)
            {
                target.SetActive(value);
            }
        }

        private void HandleClick()
        {
            if (Definition == null)
            {
                return;
            }

            switch (Definition.Kind)
            {
                case SettingKind.Dropdown:
                    if (_onDropdown != null)
                    {
                        _onDropdown(this);
                    }

                    break;

                case SettingKind.Choice:
                    Change(true);
                    break;

                // 수치 항목은 칸을 눌러도 값이 바뀌지 않는다. 막대를 끈다.
            }
        }

        private void HandleSliderChanged(float value)
        {
            if (Definition != null && _onSet != null)
            {
                _onSet(Definition, value);
            }
        }

        private void HandleMuteClicked()
        {
            SettingDefinition mute = MuteDefinition;
            if (mute != null && _onSet != null)
            {
                _onSet(mute, _muted ? 0f : 1f);
            }
        }

        private void Change(bool forward)
        {
            if (Definition == null || _onChanged == null)
            {
                return;
            }

            _onChanged(this, forward);
        }
    }
}
