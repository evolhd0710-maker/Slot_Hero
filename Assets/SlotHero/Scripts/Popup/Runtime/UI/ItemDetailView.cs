using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Popup.UI
{
    /// <summary>
    /// 아이템 상세 오버레이.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 아이템 상세 팝업이다.
    /// 500 × 380 크기로 커서 옆에 뜨고 커서가 아이템 위를 벗어나면 사라진다.
    /// 화면 가장자리에 닿으면 가장자리를 벗어나지 않게 민다.
    ///
    /// 04 화면 공통 규칙 의 레이어 순서로는 오버레이라 아래 레이어를 막지 않는다.
    /// 그래서 이 오브젝트에는 Raycast Target 을 켜지 않는다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트, 자식 Root 아래에 배경 Image 와
    /// 이름, 가치, 내용, 플레이버 TMP_Text 를 둔다. 안쪽 배치는 프리팹에서 맞춘다.
    /// 이 스크립트가 붙은 칸은 캔버스를 꽉 채우게 펼쳐 둔다. 그 칸의 왼쪽 위가 자리의 기준이다.
    ///
    /// 떠 있는 동안에는 매 프레임 커서를 따라간다. 기획서의 "커서 옆에 출력" 이다.
    /// </summary>
    public class ItemDetailView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private PopupLayoutConfig _layout;
        [SerializeField] private PopupVisualConfig _visual;

        [Header("참조")]
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _valueLabel;
        [SerializeField] private TMP_Text _bodyLabel;
        [SerializeField] private TMP_Text _flavorLabel;

        /// <summary>지금 띄우고 있는 내용.</summary>
        public ItemDetailSpec Spec { get; private set; }

        /// <summary>떠 있는지.</summary>
        public bool IsShowing
        {
            get { return _root != null && _root.gameObject.activeInHierarchy; }
        }

        private void Awake()
        {
            ApplyStyle();
            Hide();
        }

        private void Update()
        {
            if (IsShowing)
            {
                Move(ScreenToLayout(CursorPoint()));
            }
        }

        /// <summary>지금 커서 옆에 띄운다. 커서가 물건 위에 올라갔을 때 부른다.</summary>
        public void ShowAtCursor(ItemDetailSpec spec)
        {
            Show(spec, ScreenToLayout(CursorPoint()));
        }

        /// <summary>지금 커서의 화면 픽셀 자리.</summary>
        private static Vector2 CursorPoint()
        {
            Vector3 mouse = Input.mousePosition;
            return new Vector2(mouse.x, mouse.y);
        }

        /// <summary>
        /// 화면 픽셀 자리를 이 칸의 왼쪽 위 기준 자리로 바꾼다. 기획서 좌표에 배율을 먹인 값이다.
        /// 캔버스가 카메라에 물려 있으면 그 카메라로 잰다.
        /// </summary>
        public Vector2 ScreenToLayout(Vector2 screenPoint)
        {
            RectTransform area = (RectTransform)transform;
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, camera, out local))
            {
                return Vector2.zero;
            }

            Rect rect = area.rect;
            return new Vector2(local.x - rect.xMin, rect.yMax - local.y);
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_background != null)
            {
                _background.color = _visual.ItemDetailColor;
            }

            ApplyLabel(_nameLabel, _visual.ItemNameFontSize, _visual.TextColor);
            ApplyLabel(_valueLabel, _visual.ItemValueFontSize, _visual.TextColor);
            ApplyLabel(_bodyLabel, _visual.ItemBodyFontSize, _visual.TextColor);
            ApplyLabel(_flavorLabel, _visual.ItemFlavorFontSize, _visual.ItemFlavorColor);

            if (_flavorLabel != null)
            {
                // 기획서는 플레이버 텍스트를 기울여 적게 한다.
                _flavorLabel.fontStyle = FontStyles.Italic;
            }

            if (_root != null && _layout != null)
            {
                _root.sizeDelta = _layout.ItemDetailSize;
            }
        }

        /// <summary>
        /// 커서 옆에 내용을 띄운다.
        /// cursor 는 화면 왼쪽 위를 기준으로 한 커서 자리다.
        /// </summary>
        public void Show(ItemDetailSpec spec, Vector2 cursor)
        {
            if (!spec.IsValid || _root == null || _layout == null)
            {
                Hide();
                return;
            }

            Spec = spec;

            if (_nameLabel != null)
            {
                _nameLabel.text = spec.Name;
            }

            if (_valueLabel != null)
            {
                _valueLabel.text = spec.ValueText;
                _valueLabel.enabled = !string.IsNullOrEmpty(spec.ValueText);
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.text = spec.Body;
            }

            if (_flavorLabel != null)
            {
                bool hasFlavor = !string.IsNullOrEmpty(spec.Flavor);
                _flavorLabel.enabled = hasFlavor;

                if (hasFlavor)
                {
                    string format = _visual != null && !string.IsNullOrEmpty(_visual.ItemFlavorFormat)
                        ? _visual.ItemFlavorFormat
                        : "{0}";
                    _flavorLabel.text = string.Format(CultureInfo.InvariantCulture, format, spec.Flavor);
                }
            }

            if (!_root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(true);
            }

            Move(cursor);
        }

        /// <summary>커서를 따라 자리를 옮긴다.</summary>
        public void Move(Vector2 cursor)
        {
            if (_root == null || _layout == null)
            {
                return;
            }

            Vector2 position = _layout.GetItemDetailPosition(cursor);

            _root.anchorMin = new Vector2(0f, 1f);
            _root.anchorMax = new Vector2(0f, 1f);
            _root.pivot = new Vector2(0f, 1f);
            _root.sizeDelta = _layout.ItemDetailSize;
            _root.anchoredPosition = new Vector2(position.x, -position.y);
        }

        /// <summary>오버레이를 지운다. 커서가 아이템 위를 벗어나면 부른다.</summary>
        public void Hide()
        {
            Spec = new ItemDetailSpec();

            if (_root != null && _root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(false);
            }
        }

        private static void ApplyLabel(TMP_Text label, float fontSize, Color color)
        {
            if (label == null)
            {
                return;
            }

            label.fontSize = fontSize;
            label.color = color;
        }
    }
}
