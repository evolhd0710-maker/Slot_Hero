using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Ui
{
    /// <summary>
    /// 연출 중 화면 아무 곳이나 누르는 것을 받는 투명한 칸(2026년 11월 1일 원재).
    /// 화면 맨 위에 펼쳐 두고, 건너뛸 연출이 있는 동안만 누르기를 받는다(`EffectSkip.Waiting`).
    /// 누르는 순간(손을 떼기 전) 넘긴다. 누르기는 이 칸이 먹어 아래의 버튼이 함께 눌리지 않는다.
    /// </summary>
    public class EffectSkipCatcher : MonoBehaviour, IPointerDownHandler
    {
        [Tooltip("화면을 덮는 투명한 그림. 이 그림이 누르기를 받는다.")]
        [SerializeField] private Image _image;
        public Image Image { get { return _image; } }

        /// <summary>씬을 만들 때 그림을 물린다.</summary>
        public void Setup(Image image)
        {
            _image = image;
            Apply(false);
        }

        /// <summary>지금 누르기를 받는지.</summary>
        public bool Catching
        {
            get { return _image != null && _image.raycastTarget; }
        }

        private void Update()
        {
            Apply(EffectSkip.Waiting);
        }

        private void OnDestroy()
        {
            // 씬이 바뀌면 그 씬의 연출이 맡긴 일도 함께 버린다.
            EffectSkip.Clear();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EffectSkip.SkipAll();
            Apply(EffectSkip.Waiting);
        }

        private void Apply(bool catching)
        {
            if (_image == null || _image.raycastTarget == catching)
            {
                return;
            }

            if (catching)
            {
                // 다른 화면이 나중에 맨 위로 올라와도 이 칸이 가장 위에 있게 한다.
                transform.SetAsLastSibling();
            }

            _image.raycastTarget = catching;
        }
    }
}
