using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 몬스터가 맞을 때 뜨는 피해 숫자. 피그마 `Jackpot 아트웍 (복사)` Frame 5 의 피해 숫자 12 를 옮긴 것이다.
    ///
    /// 숫자는 글꼴이 아니라 0 ~ 9 한 장씩 그린 그림을 이어 붙인다(2026년 10월 26일 원재가 0 ~ 9 를 같은 디자인으로 모두 만들라고 했다).
    /// 뒤에 번개 그림을 깔고, 방어도가 막은 몫은 그 아래에 작은 글로 적는다.
    /// 떠오르고 사라지는 연출은 `CombatScreenView` 가 이 칸을 통째로 움직여 한다.
    /// </summary>
    public class DamageNumberView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _spark;

        [Tooltip("숫자 자리. 앞에서부터 채우고 남는 자리는 끈다. 자리 수보다 긴 숫자는 앞자리부터 자른다.")]
        [SerializeField] private Image[] _digits = new Image[0];

        [Tooltip("0 ~ 9 그림. 차례대로 0 부터다.")]
        [SerializeField] private Sprite[] _digitSprites = new Sprite[0];

        [Tooltip("방어도가 막은 몫. 숫자 아래에 적는다.")]
        [SerializeField] private TMP_Text _note;

        [Tooltip("숫자 사이 간격. `Configure` 가 자리 설정의 `DamageDigitAdvance` 를 넣는다. 씬에 남겨 두려고 직렬화한다.")]
        [SerializeField] private float _advance;

        public RectTransform RectTransform
        {
            get { return (RectTransform)transform; }
        }

        public CanvasGroup Group
        {
            get { return _group; }
        }

        /// <summary>자리와 크기를 받는다.</summary>
        public void Configure(CombatLayoutConfig layout, CombatVisualConfig visual)
        {
            if (layout == null)
            {
                return;
            }

            RectTransform.sizeDelta = layout.DamageSparkSize;
            _advance = layout.DamageDigitAdvance;

            if (_spark != null)
            {
                PlaceCentered(_spark.rectTransform, Vector2.zero, layout.DamageSparkSize);
            }

            for (int i = 0; i < _digits.Length; i++)
            {
                if (_digits[i] != null)
                {
                    PlaceCentered(_digits[i].rectTransform, Vector2.zero, layout.DamageDigitSize);
                }
            }

            if (_note != null)
            {
                Vector2 noteSize = new Vector2(layout.DamageSparkSize.x, layout.DamageNoteHeight);
                float below = -(layout.DamageDigitSize.y + noteSize.y) * 0.5f;
                PlaceCentered(_note.rectTransform, new Vector2(0f, below), noteSize);

                if (visual != null)
                {
                    _note.fontSize = visual.DamageNoteFontSize;
                }
            }
        }

        /// <summary>
        /// 그 값을 숫자 그림으로 적을 수 있는지. 0 ~ 9 그림이 모두 있고 자리 수가 모자라지 않아야 한다.
        /// 아니면 부르는 쪽이 글자 피해 숫자로 적는다. 숫자를 잘라 다른 값을 보이지 않으려는 것이다(2026년 10월 26일 코덱스 점검).
        /// </summary>
        public bool CanShow(int value)
        {
            if (_digitSprites == null || _digitSprites.Length < 10)
            {
                return false;
            }

            for (int i = 0; i < 10; i++)
            {
                if (_digitSprites[i] == null)
                {
                    return false;
                }
            }

            return Mathf.Max(0, value).ToString().Length <= _digits.Length;
        }

        /// <summary>그 값을 숫자 그림으로 놓는다. 0 아래는 0 으로 적는다. note 가 비면 아래 글을 끈다. 먼저 `CanShow` 로 묻는다.</summary>
        public void Show(int value, string note)
        {
            string text = Mathf.Max(0, value).ToString();
            if (text.Length > _digits.Length)
            {
                text = text.Substring(text.Length - _digits.Length);
            }

            // 숫자 묶음 전체가 가운데에 오게 첫 숫자를 왼쪽으로 민다.
            float first = -(text.Length - 1) * _advance * 0.5f;

            for (int i = 0; i < _digits.Length; i++)
            {
                Image digit = _digits[i];
                if (digit == null)
                {
                    continue;
                }

                bool used = i < text.Length;
                digit.gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                int number = text[i] - '0';
                digit.sprite = number >= 0 && number < _digitSprites.Length ? _digitSprites[number] : null;
                digit.enabled = digit.sprite != null;
                digit.preserveAspect = true;
                digit.rectTransform.anchoredPosition = new Vector2(first + i * _advance, 0f);
            }

            if (_note != null)
            {
                _note.gameObject.SetActive(!string.IsNullOrEmpty(note));
                _note.text = note ?? string.Empty;
            }
        }

        private static void PlaceCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
