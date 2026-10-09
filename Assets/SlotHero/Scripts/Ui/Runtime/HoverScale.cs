using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Ui
{
    /// <summary>
    /// 마우스를 올리면 칸을 조금 키운다.
    ///
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 "마우스 호버, 밝기 한 단계 상승" 을
    /// 밝기만으로 하면 **그림이 밝은 버튼은 올렸는지 안 올렸는지 티가 안 난다.**
    /// 성소의 모닥불과 행상인, 행상의 버튼들이 그랬다.
    /// 그래서 밝기와 별개로 크기를 조금 키워 어느 버튼에나 같은 표시가 나게 한다.
    ///
    /// 크기만 바꾸므로 밝기를 스스로 다루는 버튼에 함께 붙여도 서로 방해하지 않는다.
    ///
    /// **칸의 가운데를 기준으로 커진다.** 유니티는 피벗을 기준으로 키우는데
    /// 버튼 대부분이 피벗을 왼쪽 위에 두어 오른쪽 아래로만 커졌다.
    /// 그래서 커지는 동안 `anchoredPosition` 을 그만큼 옮겨 가운데를 제자리에 둔다.
    /// 커진 동안 다른 코드가 자리를 바꾸면 그 자리를 새 기준으로 삼는다.
    ///
    /// 크기를 스스로 바꾸는 칸에 붙어도 된다. 마우스가 올라갈 때의 크기에 곱하고
    /// 벗어날 때 그 크기로 되돌린다. 맵의 고를 수 있는 노드가 그렇다.
    ///
    /// 성소 기획서의 행상 진열이 이미 5퍼센트 확대를 쓰고 있어 기본값을 그것에 맞췄다.
    ///
    /// **긴 칸은 덜 커진다.** 5퍼센트를 그대로 곱하면 설정 화면의 항목 칸처럼 폭이 1160 인 칸은
    /// 양옆으로 30 가까이 튀어나와 너무 커 보였다. 원재가 2026년 10월 5일에 짚었다.
    /// 그래서 긴 변이 늘어나는 길이를 <see cref="DefaultMaxGrow"/> 로 막는다.
    /// 폭 480 보다 작은 칸은 그대로 5퍼센트다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>기본 확대 비율. 행상 진열의 커서 확대와 같은 값이다.</summary>
        public const float DefaultRatio = 1.05f;

        /// <summary>긴 변이 늘어날 수 있는 가장 긴 길이. 1920 × 1080 기준 24 픽셀이다.</summary>
        public static readonly float DefaultMaxGrow = UiScale.Px(24f);

        [Tooltip("마우스를 올렸을 때 곱하는 크기. 1 이면 커지지 않는다.")]
        [Range(1f, 1.5f)]
        [SerializeField] private float _ratio = DefaultRatio;

        [Tooltip("긴 변이 늘어날 수 있는 가장 긴 길이. 0 이면 막지 않는다. 기준 해상도 픽셀의 12 배 단위다.")]
        [Min(0f)]
        [SerializeField] private float _maxGrow = DefaultMaxGrow;

        [Tooltip("키울 칸. 비워 두면 자기 자신이다.")]
        [SerializeField] private RectTransform _target;

        [Tooltip("누를 수 없는 동안에는 키우지 않는다. 비워 두면 같은 칸의 버튼을 찾는다.")]
        [SerializeField] private Selectable _selectable;

        private Vector3 _baseScale = Vector3.one;
        private bool _hovering;

        // 가운데를 제자리에 두려고 옮긴 거리와, 옮긴 직후의 자리.
        private Vector2 _shift = Vector2.zero;
        private Vector2 _shiftedPosition = Vector2.zero;

        /// <summary>마우스를 올렸을 때 곱하는 크기.</summary>
        public float Ratio
        {
            get { return _ratio; }
            set { _ratio = value < 1f ? 1f : value; }
        }

        /// <summary>긴 변이 늘어날 수 있는 가장 긴 길이. 0 이면 막지 않는다.</summary>
        public float MaxGrow
        {
            get { return _maxGrow; }
            set { _maxGrow = value < 0f ? 0f : value; }
        }

        /// <summary>지금 마우스가 올라가 있는지.</summary>
        public bool Hovering
        {
            get { return _hovering; }
        }

        private void Awake()
        {
            if (_target == null)
            {
                _target = (RectTransform)transform;
            }

            if (_selectable == null)
            {
                _selectable = GetComponent<Selectable>();
            }

            _baseScale = _target.localScale;
        }

        /// <summary>
        /// 칸이 꺼질 때 원래 크기로 되돌린다.
        ///
        /// 커진 채로 화면이 닫히면 마우스가 벗어난 것을 받지 못해 커진 채로 남는다.
        /// 다음에 그 화면을 열면 처음부터 커 보인다.
        /// </summary>
        private void OnDisable()
        {
            _hovering = false;
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // **올라갈 때의 크기를 기준으로 삼는다.** 처음 크기로 삼으면 안 된다.
            // 스스로 크기를 바꾸는 칸이 있다. 맵의 고를 수 있는 노드는 5퍼센트 커져 있는데,
            // 처음 크기를 기준으로 두면 마우스가 지나간 뒤 그 강조가 사라져 버린다.
            if (!_hovering && _target != null)
            {
                _baseScale = _target.localScale;
            }

            _hovering = true;
            Apply();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            Apply();
        }

        /// <summary>지금 상태에 맞는 크기로 맞춘다.</summary>
        public void Apply()
        {
            if (_target == null)
            {
                return;
            }

            // 옮겨 둔 사이에 다른 코드가 자리를 바꿨으면 옮긴 것은 이미 사라졌다. 지금 자리를 기준으로 삼는다.
            if (_shift != Vector2.zero && _target.anchoredPosition != _shiftedPosition)
            {
                _shift = Vector2.zero;
            }

            Vector2 basePosition = _target.anchoredPosition - _shift;
            bool grow = ShouldGrow();
            Vector2 size = new Vector2(_target.rect.width, _target.rect.height);
            float ratio = CapRatio(_ratio, size, _baseScale, _maxGrow);
            _target.localScale = grow ? _baseScale * ratio : _baseScale;

            Vector2 shift = Vector2.zero;
            if (grow)
            {
                Vector2 flat = CenterShift(_target.pivot, size, _baseScale, ratio);
                Vector3 turned = _target.localRotation * new Vector3(flat.x, flat.y, 0f);
                shift = new Vector2(turned.x, turned.y);
            }

            _shift = shift;
            _target.anchoredPosition = basePosition + shift;
            _shiftedPosition = _target.anchoredPosition;
        }

        /// <summary>
        /// 가운데를 제자리에 두려면 자리를 얼마나 옮겨야 하는지. 회전은 빼고 잰다.
        ///
        /// 피벗에서 가운데까지는 (0.5 - 피벗) × 크기 다. 크기가 원래 크기에서 비율만큼 커지면
        /// 가운데가 그 거리 × 원래 크기 × (비율 - 1) 만큼 밀려나므로 그만큼 반대로 옮긴다.
        /// 피벗이 가운데면 0 이다.
        /// </summary>
        public static Vector2 CenterShift(Vector2 pivot, Vector2 size, Vector3 baseScale, float ratio)
        {
            float grow = ratio - 1f;
            float toCenterX = (0.5f - pivot.x) * size.x;
            float toCenterY = (0.5f - pivot.y) * size.y;
            return new Vector2(-toCenterX * baseScale.x * grow, -toCenterY * baseScale.y * grow);
        }

        /// <summary>
        /// 긴 변이 `maxGrow` 보다 더 늘어나지 않게 줄인 비율.
        /// `maxGrow` 가 0 이하이거나 칸 크기를 모르면 비율을 그대로 돌려준다.
        /// </summary>
        public static float CapRatio(float ratio, Vector2 size, Vector3 baseScale, float maxGrow)
        {
            float longest = Mathf.Max(Mathf.Abs(size.x * baseScale.x), Mathf.Abs(size.y * baseScale.y));
            if (maxGrow <= 0f || longest <= 0f)
            {
                return ratio;
            }

            return Mathf.Min(ratio, 1f + maxGrow / longest);
        }

        /// <summary>지금 커져야 하는지. 누를 수 없는 버튼은 커지지 않는다.</summary>
        public bool ShouldGrow()
        {
            if (!_hovering)
            {
                return false;
            }

            return _selectable == null || _selectable.interactable;
        }

        /// <summary>
        /// 원래 크기를 다시 재 둔다.
        /// 자리를 잡는 쪽이 크기를 바꾼 뒤에 부르면 그 크기를 기준으로 삼는다.
        /// </summary>
        public void RememberScale()
        {
            if (_target != null)
            {
                Vector2 size = new Vector2(_target.rect.width, _target.rect.height);
                float ratio = CapRatio(_ratio, size, _target.localScale, _maxGrow);
                _baseScale = ShouldGrow() ? _target.localScale / ratio : _target.localScale;
            }
        }
    }
}
