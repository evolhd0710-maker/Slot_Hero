using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 태그 UI 바깥 원의 한 칸. 전투 UI 및 연출 초안 / 04 태그 UI 세부.
    ///
    /// 칸 그림은 원판 전체 크기의 원을 부채꼴로 채운 것(`Image.Type.Filled`, Radial360)이라
    /// 사각 영역으로 누르기를 받으면 열 칸이 한 자리에서 겹친다.
    /// 그래서 이 칸의 각도와 반지름 안을 누를 때만 받는다(`ICanvasRaycastFilter`).
    /// 유니티는 그림의 부모에 붙은 거름도 묻는다. 칸 그림은 이 오브젝트의 자식이다.
    /// </summary>
    public class TagSectorView : MonoBehaviour,
        ICanvasRaycastFilter, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _fill;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;

        [Tooltip("이 칸의 시작 각도. 위에서 시계 방향으로 잰다.")]
        [SerializeField] private float _startDegrees;

        [Tooltip("이 칸이 차지하는 각도.")]
        [SerializeField] private float _sweepDegrees = 36f;

        [Tooltip("안쪽 반지름. 원판 반지름에 대한 비율이다. 그 안은 가운데 코인 자리다.")]
        [SerializeField] private float _innerRatio = 0.37f;

        private Color _baseColor = Color.white;
        private bool _interactable = true;
        private bool _hovering;
        private float _hoverBrightness = 1.15f;
        private float _disabledBrightness = 0.35f;
        private float _hoverScale = 1.06f;

        /// <summary>눌렸다.</summary>
        public event Action Clicked;

        /// <summary>마우스가 올라가거나 벗어났다.</summary>
        public event Action<bool> HoverChanged;

        /// <summary>눌러 쓸 수 있는지. 끄면 어둡게 보이지만 호버는 알린다.</summary>
        public bool Interactable
        {
            get { return _interactable; }
        }

        /// <summary>칸의 각도와 색과 그림을 정한다.</summary>
        public void Setup(
            float startDegrees, float sweepDegrees, float gapDegrees, float innerRatio,
            float iconDistance, float iconSize,
            Color color, Sprite icon, string symbol)
        {
            _startDegrees = startDegrees;
            _sweepDegrees = sweepDegrees;
            _innerRatio = innerRatio;
            _baseColor = color;

            if (_fill != null)
            {
                _fill.type = Image.Type.Filled;
                _fill.fillMethod = Image.FillMethod.Radial360;
                _fill.fillOrigin = (int)Image.Origin360.Top;
                _fill.fillClockwise = true;
                _fill.fillAmount = Mathf.Clamp01((sweepDegrees - gapDegrees) / 360f);

                // 위에서 시작해 시계 방향으로 채우므로 시작 각도만큼 시계 방향으로 돌린다.
                _fill.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -(startDegrees + gapDegrees * 0.5f));
            }

            // 아이콘과 기호 글자는 칸 가운데 방향, 반지름 사이에 놓는다.
            float middle = (startDegrees + sweepDegrees * 0.5f) * Mathf.Deg2Rad;
            Vector2 at = new Vector2(Mathf.Sin(middle), Mathf.Cos(middle)) * iconDistance;

            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.enabled = icon != null;
                _icon.preserveAspect = true;
                PlaceAt(_icon.rectTransform, at, iconSize);
            }

            if (_label != null)
            {
                _label.text = icon == null ? symbol : string.Empty;
                PlaceAt(_label.rectTransform, at, iconSize);
            }

            Paint();
        }

        private static void PlaceAt(RectTransform rect, Vector2 center, float size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = center;
        }

        /// <summary>연출 밝기 값을 받는다.</summary>
        public void SetStyle(float hoverBrightness, float disabledBrightness, float hoverScale)
        {
            _hoverBrightness = hoverBrightness;
            _disabledBrightness = disabledBrightness;
            _hoverScale = hoverScale;
            Paint();
        }

        /// <summary>눌러 쓸 수 있는지 정한다.</summary>
        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            Paint();
        }

        /// <summary>호버 표시를 걷는다. 원판이 바뀔 때 부른다.</summary>
        public void ClearHover()
        {
            _hovering = false;
            Paint();
        }

        /// <summary>칸 가운데 방향. 위에서 시계 방향으로 잰 각도다.</summary>
        public float MiddleDegrees
        {
            get { return _startDegrees + _sweepDegrees * 0.5f; }
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransform rect = (RectTransform)transform;
            Vector2 local;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out local))
            {
                return false;
            }

            // 피벗이 가운데가 아닐 수 있어 사각형 가운데에서 잰다.
            local -= rect.rect.center;
            return Contains(local, rect.rect.width * 0.5f);
        }

        /// <summary>
        /// 원판 가운데 기준 점이 이 칸 안인지. y 는 위가 + 다.
        /// 반지름은 원판 반지름이다. 검사에서도 부른다.
        /// </summary>
        public bool Contains(Vector2 local, float radius)
        {
            float distance = local.magnitude;
            if (distance > radius || distance < radius * _innerRatio)
            {
                return false;
            }

            // 위에서 시계 방향으로 잰 각도.
            float degrees = Mathf.Atan2(local.x, local.y) * Mathf.Rad2Deg;
            float from = Normalize(degrees - _startDegrees);
            return from < _sweepDegrees;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 커진 칸이 이웃 칸 아래에 깔리지 않게 맨 위로 올린다. 가운데 원은 칸 묶음 밖이라 그대로 위에 있다.
            transform.SetAsLastSibling();
            _hovering = true;
            Paint();
            if (HoverChanged != null)
            {
                HoverChanged(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            Paint();
            if (HoverChanged != null)
            {
                HoverChanged(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Clicked != null)
            {
                Clicked();
            }
        }

        private void Paint()
        {
            if (_fill == null)
            {
                return;
            }

            float brightness = !_interactable ? _disabledBrightness : (_hovering ? _hoverBrightness : 1f);
            _fill.color = CombatVisualConfig.Scale(_baseColor, brightness);

            float scale = _hovering && _interactable ? _hoverScale : 1f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static float Normalize(float degrees)
        {
            degrees %= 360f;
            return degrees < 0f ? degrees + 360f : degrees;
        }
    }
}
