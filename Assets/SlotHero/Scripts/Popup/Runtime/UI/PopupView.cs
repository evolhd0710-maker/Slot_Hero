using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Popup.UI
{
    /// <summary>
    /// 팝업 하나의 표시.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 의 공통 틀을 맡는다.
    /// 뒤 화면을 어둡게 깔고 본체 안에 제목, 본문, 버튼을 놓는다.
    /// 14 진행 중 팝업 도 같은 틀을 쓴다.
    ///
    /// 본체 세로는 본문 길이에 따라 줄어들고,
    /// 본문이 최대 높이를 넘으면 본문 칸 안에서만 스크롤한다.
    /// </summary>
    public class PopupView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private PopupLayoutConfig _layout;
        [SerializeField] private PopupVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _dim;
        [SerializeField] private Image _popupPanel;

        [Header("제목")]
        [SerializeField] private Image _titlePanel;
        [SerializeField] private TMP_Text _titleLabel;

        [Header("본문")]
        [SerializeField] private Image _bodyPanel;
        [SerializeField] private TMP_Text _bodyLabel;

        [Tooltip("본문이 길 때 그 안에서만 훑는 스크롤. 없으면 본문이 잘린다.")]
        [SerializeField] private ScrollRect _bodyScroll;

        [SerializeField] private RectTransform _bodyContent;

        [Header("버튼")]
        [SerializeField] private RectTransform _buttonLayer;
        [SerializeField] private PopupButtonView _buttonPrefab;

        private readonly List<PopupButtonView> _buttons = new List<PopupButtonView>();

        /// <summary>버튼을 눌렀을 때. 버튼 식별자를 넘긴다.</summary>
        public event Action<string> ButtonClicked;

        /// <summary>지금 보여 주고 있는 내용.</summary>
        public PopupSpec Spec { get; private set; }

        /// <summary>팝업 하나를 그린다.</summary>
        public void Show(PopupSpec spec)
        {
            if (spec == null || _layout == null)
            {
                return;
            }

            Spec = spec;

            ApplyStyle();
            BuildButtons(spec);

            if (_titleLabel != null)
            {
                _titleLabel.text = spec.Title;
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.text = spec.Body;
            }

            ApplySize();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_dim != null)
            {
                _dim.color = _visual.DimColor;
            }

            if (_popupPanel != null)
            {
                _popupPanel.color = _visual.PopupColor;
            }

            if (_titlePanel != null)
            {
                _titlePanel.color = _visual.TitlePanelColor;
            }

            if (_bodyPanel != null)
            {
                _bodyPanel.color = _visual.BodyPanelColor;
            }

            if (_titleLabel != null)
            {
                _titleLabel.fontSize = _visual.TitleFontSize;
                _titleLabel.color = _visual.TextColor;
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.fontSize = _visual.BodyFontSize;
                _bodyLabel.color = _visual.TextColor;
            }
        }

        /// <summary>
        /// 본문 길이에 맞춰 본체 크기를 잡는다.
        /// 기획서대로 내용이 적으면 세로를 줄이고 길면 본문 안에서만 스크롤한다.
        /// </summary>
        public void ApplySize()
        {
            if (_layout == null || _popupPanel == null)
            {
                return;
            }

            float desired = GetDesiredBodyHeight();
            float bodyHeight = _layout.GetBodyHeight(desired);
            Vector2 popupSize = _layout.GetPopupSize(desired);

            RectTransform popupRect = _popupPanel.rectTransform;
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.sizeDelta = popupSize;

            float contentWidth = _layout.GetContentWidth();

            if (_titlePanel != null)
            {
                PlaceInPopup(
                    _titlePanel.rectTransform,
                    _layout.GetTitlePosition(),
                    new Vector2(contentWidth, _layout.TitleHeight));
            }

            if (_bodyPanel != null)
            {
                PlaceInPopup(
                    _bodyPanel.rectTransform,
                    _layout.GetBodyPosition(),
                    new Vector2(contentWidth, bodyHeight));
            }

            if (_bodyScroll != null)
            {
                _bodyScroll.horizontal = false;
                _bodyScroll.vertical = true;
            }

            if (_bodyContent != null)
            {
                float inner = contentWidth - _layout.BodyPadding * 2f;
                float height = desired > bodyHeight ? desired : bodyHeight;
                _bodyContent.anchorMin = new Vector2(0f, 1f);
                _bodyContent.anchorMax = new Vector2(0f, 1f);
                _bodyContent.pivot = new Vector2(0f, 1f);
                _bodyContent.anchoredPosition = Vector2.zero;
                _bodyContent.sizeDelta = new Vector2(inner, height);
            }

            PlaceButtons(popupSize);
        }

        /// <summary>본문이 담고 싶어 하는 높이. 글자 길이에서 재고 안쪽 여백을 더한다.</summary>
        private float GetDesiredBodyHeight()
        {
            if (_bodyLabel == null || _layout == null)
            {
                return 0f;
            }

            float inner = _layout.GetContentWidth() - _layout.BodyPadding * 2f;
            float text = _bodyLabel.GetPreferredValues(_bodyLabel.text, inner, 0f).y;
            return text + _layout.BodyPadding * 2f;
        }

        /// <summary>버튼을 수에 맞게 만들고 기획서 배치대로 늘어놓는다.</summary>
        private void BuildButtons(PopupSpec spec)
        {
            if (_buttonLayer == null || _buttonPrefab == null)
            {
                return;
            }

            int count = spec.ButtonCount;

            while (_buttons.Count < count)
            {
                _buttons.Add(Instantiate(_buttonPrefab, _buttonLayer));
            }

            Vector2 size = new Vector2(PopupLayoutMath.GetButtonWidth(count), _layout.ButtonHeight);

            for (int i = 0; i < _buttons.Count; i++)
            {
                PopupButtonView view = _buttons[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                // **다시 켜는 것을 빠뜨리면 안 된다.**
                // 버튼 칸은 한 번 만들면 다음 팝업에서도 그대로 쓴다.
                // 버튼이 적은 팝업이 한 번 뜨면 남는 칸을 꺼 두는데,
                // 그 뒤에 버튼이 많은 팝업이 떠도 꺼진 칸이 그대로 꺼져 있어
                // 버튼 셋 가운데 첫 하나만 보인다. 런 종료 확인 팝업이 그랬다.
                view.gameObject.SetActive(true);
                view.Bind(spec.Buttons[i], size, _visual, HandleButtonClicked);
            }
        }

        /// <summary>버튼 줄을 본체 아래쪽 가운데에 놓는다.</summary>
        private void PlaceButtons(Vector2 popupSize)
        {
            if (_layout == null || Spec == null)
            {
                return;
            }

            int count = Spec.ButtonCount;
            float top = _layout.GetButtonTop(popupSize.y);

            for (int i = 0; i < _buttons.Count; i++)
            {
                PopupButtonView view = _buttons[i];
                if (view == null || i >= count)
                {
                    continue;
                }

                // 가로는 본체 가운데를 0으로 본 값이고 세로는 본체 위에서부터 잰다.
                float offsetX = PopupLayoutMath.GetButtonCenterOffset(count, i);

                RectTransform rect = view.RectTransform;
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(offsetX, -top);
            }
        }

        /// <summary>본체 왼쪽 위를 기준으로 자리를 잡는다.</summary>
        private static void PlaceInPopup(RectTransform rect, Vector2 position, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(position.x, -position.y);
        }

        private void HandleButtonClicked(PopupButtonView view)
        {
            if (view == null)
            {
                return;
            }

            if (ButtonClicked != null)
            {
                ButtonClicked(view.Spec.Id);
            }
        }
    }
}
