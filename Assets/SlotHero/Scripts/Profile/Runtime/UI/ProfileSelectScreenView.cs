using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Profile.UI
{
    /// <summary>
    /// 프로필 선택 화면 표시.
    /// 인게임 화면 기획서 v0.2 / 06 프로필 선택 화면 의 배치를 맡는다.
    ///
    /// 제목이 왼쪽 위, 카드가 가로로 늘어서고, 뒤로가 우측 하단이다.
    /// 카드 수는 저장 시스템 기획서의 프로필 수를 따르므로 프리팹을 복제해 만든다.
    /// </summary>
    public class ProfileSelectScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private ProfileLayoutConfig _layout;
        [SerializeField] private ProfileVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _titlePanel;
        [SerializeField] private TMP_Text _titleLabel;

        [Header("카드")]
        [SerializeField] private RectTransform _cardLayer;
        [SerializeField] private ProfileCardView _cardPrefab;

        [Header("뒤로")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Image _backPanel;
        [SerializeField] private TMP_Text _backLabel;

        private readonly List<ProfileCardView> _cards = new List<ProfileCardView>();

        /// <summary>카드를 눌렀을 때. 몇 번째 자리인지 넘긴다.</summary>
        public event Action<int> CardClicked;

        /// <summary>수정을 눌렀을 때.</summary>
        public event Action<int> EditClicked;

        /// <summary>삭제를 눌렀을 때.</summary>
        public event Action<int> DeleteClicked;

        /// <summary>이름 입력을 마쳤을 때.</summary>
        public event Action<int, string> NameSubmitted;

        /// <summary>뒤로를 눌렀을 때.</summary>
        public event Action BackClicked;

        /// <summary>지금 화면에 놓인 카드.</summary>
        public IList<ProfileCardView> Cards
        {
            get { return _cards; }
        }

        private void Awake()
        {
            if (_backButton != null)
            {
                _backButton.transition = Selectable.Transition.None;
                _backButton.onClick.AddListener(HandleBackClicked);
            }

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        /// <summary>
        /// 기획서 06장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            PlaceTopLeft(_titlePanel, _layout.TitlePosition, _layout.TitleSize);
            PlaceTopLeft(_backPanel, _layout.BackPosition, _layout.BackSize);

            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null)
                {
                    PlaceTopLeftRect(_cards[i].RectTransform, _layout.GetCardPosition(i), _layout.CardSize);
                }
            }

            ApplyStyle();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_background != null)
            {
                _background.color = _visual.BackgroundColor;
            }

            if (_titlePanel != null)
            {
                _titlePanel.color = _visual.PanelColor;
            }

            if (_backPanel != null)
            {
                _backPanel.color = _visual.PanelColor;
            }

            if (_titleLabel != null)
            {
                _titleLabel.text = _visual.TitleText;
                _titleLabel.fontSize = _visual.TitleFontSize;
                _titleLabel.color = _visual.NameColor;
                _titleLabel.alignment = TextAlignmentOptions.Left;
            }

            if (_backLabel != null)
            {
                _backLabel.text = _visual.BackText;
                _backLabel.fontSize = _visual.BackFontSize;
                _backLabel.color = _visual.NameColor;
                _backLabel.alignment = TextAlignmentOptions.Center;
            }
        }

        /// <summary>프로필 목록을 받아 카드를 늘어놓는다.</summary>
        public void Show(ProfileList list)
        {
            if (list == null || _cardLayer == null || _cardPrefab == null || _layout == null)
            {
                return;
            }

            int count = list.Count;

            while (_cards.Count < count)
            {
                ProfileCardView card = Instantiate(_cardPrefab, _cardLayer);
                card.Clicked += HandleCardClicked;
                card.EditClicked += HandleEditClicked;
                card.DeleteClicked += HandleDeleteClicked;
                card.NameSubmitted += HandleNameSubmitted;
                _cards.Add(card);
            }

            for (int i = 0; i < _cards.Count; i++)
            {
                ProfileCardView card = _cards[i];
                if (card == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                card.gameObject.SetActive(true);
                card.Bind(i, list.Get(i), _layout, _visual);
                PlaceTopLeftRect(card.RectTransform, _layout.GetCardPosition(i), _layout.CardSize);
            }
        }

        /// <summary>그 자리의 카드. 없으면 null.</summary>
        public ProfileCardView GetCard(int index)
        {
            if (index < 0 || index >= _cards.Count)
            {
                return null;
            }

            return _cards[index];
        }

        /// <summary>화면을 보이거나 감춘다.</summary>
        public void SetVisible(bool value)
        {
            gameObject.SetActive(value);
        }

        private static void PlaceTopLeft(Image image, Vector2 position, Vector2 size)
        {
            if (image != null)
            {
                PlaceTopLeftRect(image.rectTransform, position, size);
            }
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다. 기획서 좌표를 그대로 넣을 수 있다.</summary>
        private static void PlaceTopLeftRect(RectTransform rect, Vector2 position, Vector2 size)
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

        private void HandleCardClicked(int index)
        {
            if (CardClicked != null)
            {
                CardClicked(index);
            }
        }

        private void HandleEditClicked(int index)
        {
            if (EditClicked != null)
            {
                EditClicked(index);
            }
        }

        private void HandleDeleteClicked(int index)
        {
            if (DeleteClicked != null)
            {
                DeleteClicked(index);
            }
        }

        private void HandleNameSubmitted(int index, string value)
        {
            if (NameSubmitted != null)
            {
                NameSubmitted(index, value);
            }
        }

        private void HandleBackClicked()
        {
            if (BackClicked != null)
            {
                BackClicked();
            }
        }
    }
}
