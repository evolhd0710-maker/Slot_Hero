using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.TopBar.UI
{
    /// <summary>
    /// 상단 표시줄의 마우스 호버 팝업.
    /// 상단 UI 바 기획서 v0.2 / 06 상호작용 의 "지도" 텍스트 팝업과
    /// 위치 칸의 현재 스테이지 이름, 현재 방 종류를 띄운다.
    ///
    /// 기획서가 팝업의 생김새를 정하지 않았으므로 글자와 배경만 둔 최소 구성이다.
    /// 화면 가장자리에서 벗어나지 않게 하는 규칙은
    /// 인게임 화면 기획서 13장 아이템 상세 팝업 의 처리를 가져왔다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트, 자식 Root 아래에 배경 Image 와 TMP_Text.
    /// Root 의 피벗은 위쪽 가운데 (0.5, 1) 로 두고 크기는 Content Size Fitter 에 맡긴다.
    /// </summary>
    public class TopBarTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TopBarVisualConfig _visual;

        private void Awake()
        {
            ApplyStyle();
            Hide();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            // **풍선은 커서를 먹으면 안 된다.**
            // 버튼 바로 아래에 뜨기 때문에 커서를 받으면 버튼이 벗어난 것으로 읽혀
            // 풍선이 사라지고, 사라지면 다시 버튼에 올라간 것이 되어 또 뜬다.
            // 그렇게 점멸하고, 그 사이에 누르면 클릭이 풍선으로 들어가 버튼이 안 먹는다.
            // 설정이 없어도 이것만은 해 둔다.
            if (_background != null)
            {
                _background.raycastTarget = false;
            }

            if (_label != null)
            {
                _label.raycastTarget = false;
            }

            if (_visual == null)
            {
                return;
            }

            if (_background != null)
            {
                _background.color = _visual.TooltipBackgroundColor;
            }

            if (_label != null)
            {
                _label.color = _visual.TooltipTextColor;
                _label.fontSize = _visual.TooltipFontSize;
            }
        }

        /// <summary>대상 아래에 문구를 띄운다. 문구가 비어 있으면 띄우지 않는다.</summary>
        public void Show(RectTransform target, string text)
        {
            if (_root == null || _label == null || string.IsNullOrEmpty(text) || target == null)
            {
                Hide();
                return;
            }

            _label.text = text;

            if (!_root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(true);
            }

            // 크기가 글자에 맞춰진 뒤라야 자리를 제대로 잡을 수 있다.
            Resize(text);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
            Place(target);
        }

        /// <summary>
        /// 글에 맞춰 칸 크기를 잡는다.
        ///
        /// **크기를 안 잡으면 유니티가 새 칸에 넣어 주는 100 × 100 이 그대로 남는다.**
        /// 글과 상관없는 그 칸이 버튼 셋의 클릭을 가로챌 수 있다.
        /// 좌표 배율이 12 였을 때 실제로 표시줄 전체를 덮어 클릭을 통째로 가로챘다.
        /// </summary>
        private void Resize(string text)
        {
            float padding = _visual != null ? _visual.TooltipPadding : 0f;
            float maxWidth = _visual != null ? _visual.TooltipMaxWidth : 0f;

            float inner = maxWidth - padding * 2f;
            if (inner < 1f)
            {
                inner = 1f;
            }

            Vector2 wanted = _label.GetPreferredValues(text, inner, 0f);

            float width = wanted.x + padding * 2f;
            if (maxWidth > 0f && width > maxWidth)
            {
                width = maxWidth;
            }

            // **기준점은 부모 한가운데여야 한다.**
            // `Place` 가 `InverseTransformPoint` 로 자리를 구하는데 그 값은 부모 한가운데가 0 이다.
            // 기준점을 모서리에 두면 그 값을 모서리에서부터 잰 것으로 읽어
            // 풍선이 화면 반쪽만큼 어긋난 자리에 뜬다.
            _root.anchorMin = new Vector2(0.5f, 0.5f);
            _root.anchorMax = new Vector2(0.5f, 0.5f);

            // 피벗은 위쪽 가운데다. 버튼 아래에 매달리게 하려는 것이다.
            _root.pivot = new Vector2(0.5f, 1f);
            _root.sizeDelta = new Vector2(width, wanted.y + padding * 2f);
        }

        /// <summary>팝업을 지운다.</summary>
        public void Hide()
        {
            if (_root != null && _root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(false);
            }
        }

        /// <summary>대상의 아래 가운데에 붙이고 화면 밖으로 나가지 않게 민다.</summary>
        private void Place(RectTransform target)
        {
            RectTransform parent = _root.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            Rect targetRect = target.rect;
            Vector3 world = target.TransformPoint(new Vector3(targetRect.center.x, targetRect.yMin, 0f));
            Vector3 local = parent.InverseTransformPoint(world);

            float offsetY = _visual != null ? _visual.TooltipOffsetY : 0f;
            Vector2 position = new Vector2(local.x, local.y + offsetY);

            Rect parentRect = parent.rect;
            Rect selfRect = _root.rect;

            float halfWidth = selfRect.width * _root.pivot.x;
            float rightWidth = selfRect.width * (1f - _root.pivot.x);
            position.x = Mathf.Clamp(
                position.x,
                parentRect.xMin + halfWidth,
                parentRect.xMax - rightWidth);

            float below = selfRect.height * _root.pivot.y;
            position.y = Mathf.Max(position.y, parentRect.yMin + below);

            _root.anchoredPosition = position;
        }
    }
}
