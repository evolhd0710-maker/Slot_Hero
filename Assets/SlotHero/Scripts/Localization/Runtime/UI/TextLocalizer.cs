using TMPro;
using UnityEngine;

namespace SlotHero.Localization.UI
{
    /// <summary>
    /// 글상자 하나에 붙어 그리기 직전에 글자를 지금 언어로 바꾼다(2026년 11월 8일 원재).
    ///
    /// TextMeshPro 의 글자 전처리(`ITextPreprocessor`)를 쓴다. 글상자의 `text` 는 한국어 그대로 두고
    /// 화면에 그리는 글자만 바뀐다. 그래서 글자를 읽어 무엇을 가리는 코드는 언어와 상관없이 같은 글자를 본다.
    ///
    /// 씬을 만드는 도구(`UiSceneBuilder.NewText`)가 모든 글상자에 붙인다. 프리팹으로 새로 만드는 글상자도 따라온다.
    /// 언어가 바뀌면 꺼져 있는 글상자까지 다시 그리게 표시해 둔다.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextLocalizer : MonoBehaviour, ITextPreprocessor
    {
        [SerializeField] private TMP_Text _text;
        public TMP_Text Text { get { return _text; } set { _text = value; } }

        [Tooltip("같은 한국어가 화면마다 뜻이 다를 때의 맥락. 번역표에서 '맥락|원문' 줄을 먼저 찾는다. 비우면 원문만 찾는다.")]
        [SerializeField] private string _context;
        public string Context { get { return _context; } set { _context = value; } }

        private void Awake()
        {
            Hook();
            Loc.Changed += Refresh;
        }

        private void OnDestroy()
        {
            Loc.Changed -= Refresh;
        }

        /// <summary>글상자에 전처리를 건다. 이미 걸려 있으면 그대로다.</summary>
        public void Hook()
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            if (_text != null && !ReferenceEquals(_text.textPreprocessor, this))
            {
                _text.textPreprocessor = this;
                _text.havePropertiesChanged = true;
            }
        }

        /// <summary>지금 언어로 다시 그리게 한다.</summary>
        public void Refresh()
        {
            if (_text != null)
            {
                _text.havePropertiesChanged = true;
            }
        }

        public string PreprocessText(string text)
        {
            return Loc.T(text, _context);
        }
    }
}
