using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 스크롤 창이 함께 쓰는 휠 거리와 스크롤 바의 생김새.
    /// 스크롤할 분량이 있으면 오른쪽에 바가 있어야 한다(2026년 10월 22일 원재).
    /// 값은 1920 × 1080 기준이다. 예전에는 씬을 만드는 도구의 상수였는데 설정 에셋으로 옮겼다(2026년 10월 25일 원재).
    /// </summary>
    [CreateAssetMenu(
        fileName = "ScrollVisualConfig",
        menuName = "Slot Hero/공용 UI/스크롤 설정",
        order = 0)]
    public class ScrollVisualConfig : ScriptableObject
    {
        [Header("휠")]
        [Tooltip("마우스 휠 한 칸에 스크롤이 움직이는 거리. 유니티 기본값 1 은 한 칸에 1 픽셀이라 스크롤이 안 되는 것처럼 보인다.")]
        [SerializeField] private float _wheelStep = UiScale.Px(60f);
        public float WheelStep { get { return _wheelStep; } set { _wheelStep = value; } }

        [Header("스크롤 바")]
        [Tooltip("바의 폭.")]
        [SerializeField] private float _barWidth = UiScale.Px(10f);
        public float BarWidth { get { return _barWidth; } set { _barWidth = value; } }

        [Tooltip("바를 창의 오른쪽 끝과 위아래 끝에서 들이는 거리. 칸 그림의 쇠 테에 걸치지 않게 한다.")]
        [SerializeField] private float _barInset = UiScale.Px(10f);
        public float BarInset { get { return _barInset; } set { _barInset = value; } }

        [Tooltip("바의 홈. 어두운 칸 위에서 길만 보이게 옅게 둔다.")]
        [SerializeField] private Color _trackColor = new Color(0f, 0f, 0f, 0.35f);
        public Color TrackColor { get { return _trackColor; } set { _trackColor = value; } }

        [Tooltip("바의 손잡이. 문양 화풍의 크림색이다.")]
        [SerializeField] private Color _handleColor = new Color(0.925f, 0.886f, 0.784f, 0.85f);
        public Color HandleColor { get { return _handleColor; } set { _handleColor = value; } }

        /// <summary>내용 쪽이 오른쪽에 비워 둘 바 자리. 바 폭에 양옆 들임을 더한 값이다.</summary>
        public float BarSpace
        {
            get { return _barWidth + _barInset * 2f; }
        }
    }
}
