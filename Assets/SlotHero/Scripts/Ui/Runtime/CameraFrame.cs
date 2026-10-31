using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 창 비율이 달라져도 화면을 16:9 로 지킨다.
    ///
    /// 기획서가 정한 것은 1920 × 1080, 16:9 다.
    /// 창이 더 넓으면 좌우에, 더 높으면 위아래에 검은 띠를 두고
    /// 가운데 16:9 자리에만 그린다. 그래야 와이어프레임에서 잰 자리가 어느 창에서든 그대로다.
    ///
    /// 카메라의 `rect` 를 좁히는 방식이라 띠 자리는 이 카메라가 지우지 않는다.
    /// 그래서 뒤에 화면 전체를 검게 지우는 카메라를 하나 더 둔다.
    /// 그것이 없으면 띠 자리에 지난 프레임이 번진다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraFrame : MonoBehaviour
    {
        [Header("지킬 비율")]
        [Tooltip("기준 해상도. 이 비율을 지킨다.")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

        [Tooltip("끄면 창을 가득 채운다. 띠가 생기지 않는 대신 자리가 늘어난다.")]
        [SerializeField] private bool _keepAspect = true;

        private Camera _camera;

        /// <summary>지키는 가로세로 비율.</summary>
        public float TargetAspect
        {
            get { return _referenceResolution.x / _referenceResolution.y; }
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        /// <summary>지금 창 크기에 맞춰 그리는 자리를 잡는다.</summary>
        public void Apply()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            if (_camera == null)
            {
                return;
            }

            if (!_keepAspect)
            {
                _camera.rect = new Rect(0f, 0f, 1f, 1f);
                return;
            }

            _camera.rect = GetFrame(GetTargetWidth(), GetTargetHeight(), TargetAspect);
        }

        /// <summary>
        /// 창 안에서 그 비율을 지키는 가운데 자리.
        /// 돌려주는 값은 0 에서 1 사이의 비율이다.
        /// </summary>
        public static Rect GetFrame(float width, float height, float targetAspect)
        {
            if (width <= 0f || height <= 0f || targetAspect <= 0f)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            float windowAspect = width / height;
            float scale = windowAspect / targetAspect;

            if (scale < 1f)
            {
                // 창이 더 홀쭉하다. 위아래에 띠를 둔다.
                return new Rect(0f, (1f - scale) * 0.5f, 1f, scale);
            }

            // 창이 더 넓다. 좌우에 띠를 둔다.
            float inverse = 1f / scale;
            return new Rect((1f - inverse) * 0.5f, 0f, inverse, 1f);
        }

        /// <summary>
        /// 그릴 자리의 가로.
        /// 그림으로 뽑을 때는 창이 아니라 그 그림 크기를 봐야 한다.
        /// </summary>
        private float GetTargetWidth()
        {
            return _camera.targetTexture != null ? _camera.targetTexture.width : Screen.width;
        }

        private float GetTargetHeight()
        {
            return _camera.targetTexture != null ? _camera.targetTexture.height : Screen.height;
        }

        /// <summary>기준 해상도와 비율을 정한다. 씬을 만드는 쪽이 쓴다.</summary>
        public void SetReference(Vector2 referenceResolution, bool keepAspect)
        {
            _referenceResolution = referenceResolution;
            _keepAspect = keepAspect;
            Apply();
        }
    }
}
