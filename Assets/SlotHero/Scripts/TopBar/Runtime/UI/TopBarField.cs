using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.TopBar.UI
{
    /// <summary>
    /// 아이콘 하나와 숫자 하나로 이뤄진 런 요약 한 칸.
    /// 상단 UI 바 기획서 v0.2 / 04 런 요약 표기 상세 의 체력과 골드가 이 구성이다.
    ///
    /// 05 정보 갱신 시점 의 "짧은 시간 동안 색상을 변경한다"를 여기서 맡는다.
    /// 프리팹 구성: 루트에 이 스크립트와 가로 배치, 자식에 아이콘 Image 와 숫자 TMP_Text.
    /// </summary>
    public class TopBarField : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;

        private RectTransform _rectTransform;
        private Color _baseColor = Color.white;
        private float _flashRemaining;

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

        /// <summary>색을 바꿔 두는 연출이 끝난 뒤 돌아갈 색.</summary>
        public Color BaseColor
        {
            get { return _baseColor; }
        }

        /// <summary>
        /// 꺼질 때 색 연출을 끝내 평소 색으로 돌린다.
        ///
        /// 연출 시간은 `Update` 로 재는데 꺼진 오브젝트는 `Update` 가 돌지 않는다.
        /// 안 돌리면 야영 직후 런을 끝냈을 때 체력 글자가 초록인 채 얼었다가
        /// 다음 런에 상단 표시줄이 켜지면서 이어서 번쩍인다.
        /// </summary>
        private void OnDisable()
        {
            if (_flashRemaining > 0f)
            {
                _flashRemaining = 0f;
                ApplyLabelColor(_baseColor);
            }
        }

        private void Update()
        {
            if (_flashRemaining <= 0f)
            {
                return;
            }

            // 연출 속도 설정이나 일시 정지와 무관하게 흘러야 하므로 실제 시간을 쓴다.
            _flashRemaining -= Time.unscaledDeltaTime;

            if (_flashRemaining <= 0f)
            {
                _flashRemaining = 0f;
                ApplyLabelColor(_baseColor);
            }
        }

        /// <summary>글자 크기와 평소 색을 맞춘다.</summary>
        public void SetStyle(float fontSize, Color color)
        {
            _baseColor = color;

            if (_label != null)
            {
                _label.fontSize = fontSize;

                if (_flashRemaining <= 0f)
                {
                    ApplyLabelColor(color);
                }
            }
        }

        /// <summary>아이콘을 넣는다. 크기는 표시줄 자리 설정이 맞춘다.</summary>
        public void SetIcon(Sprite sprite, float iconSize)
        {
            if (_icon == null)
            {
                return;
            }

            if (sprite != null)
            {
                _icon.sprite = sprite;
            }

            _icon.preserveAspect = true;
            _icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
            _icon.enabled = _icon.sprite != null;
        }

        /// <summary>숫자를 적는다.</summary>
        public void SetText(string text)
        {
            if (_label != null)
            {
                _label.text = text;
            }
        }

        /// <summary>짧은 시간 동안 다른 색으로 보여 준다. 시간이 지나면 평소 색으로 돌아간다.</summary>
        public void Flash(Color color, float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _flashRemaining = seconds;
            ApplyLabelColor(color);
        }

        private void ApplyLabelColor(Color color)
        {
            if (_label != null)
            {
                _label.color = color;
            }
        }
    }
}
