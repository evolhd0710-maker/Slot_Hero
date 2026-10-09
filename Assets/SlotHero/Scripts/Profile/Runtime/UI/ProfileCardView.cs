using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Profile.UI
{
    /// <summary>
    /// 프로필 카드 하나.
    /// 인게임 화면 기획서 v0.2 / 06 프로필 선택 화면 의 프로필 카드와 빈 자리를 함께 맡는다.
    ///
    /// 채워진 자리에는 프로필명과 플레이 정보, 수정과 삭제 버튼이 나오고
    /// 빈 자리에는 가운데에 "비어 있음"만 나온다.
    ///
    /// 프리팹 구성: 루트에 이 스크립트와 Button,
    /// 자식 Content 아래에 배경 Image 와 글, 버튼을 둔다.
    /// 마우스를 올렸을 때 커지는 것은 Content 뿐이라 카드의 자리 계산과 어긋나지 않는다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ProfileCardView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("참조")]
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _background;

        [Header("채워진 자리")]
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _playTimeLabel;
        [SerializeField] private TMP_Text _lastPlayedLabel;

        [Header("빈 자리")]
        [SerializeField] private TMP_Text _emptyLabel;

        [Header("수정과 삭제")]
        [SerializeField] private Button _editButton;
        [SerializeField] private Image _editPanel;
        [SerializeField] private TMP_Text _editLabel;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Image _deletePanel;
        [SerializeField] private TMP_Text _deleteLabel;

        [Header("이름 고치기")]
        [Tooltip("프로필명 입력 상태에서 쓰는 입력칸. 기획서에 화면이 없어 카드 안에 두었다.")]
        [SerializeField] private TMP_InputField _nameInput;

        private ProfileLayoutConfig _layout;
        private ProfileVisualConfig _visual;
        private ProfileSummary _summary;
        private int _index = -1;
        private bool _hovering;
        private bool _pressed;
        private bool _editing;

        /// <summary>카드를 눌렀을 때. 몇 번째 자리인지 넘긴다.</summary>
        public event Action<int> Clicked;

        /// <summary>수정을 눌렀을 때.</summary>
        public event Action<int> EditClicked;

        /// <summary>삭제를 눌렀을 때.</summary>
        public event Action<int> DeleteClicked;

        /// <summary>이름 입력을 마쳤을 때. 자리와 새 이름을 넘긴다.</summary>
        public event Action<int, string> NameSubmitted;

        /// <summary>몇 번째 자리인지.</summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>지금 담긴 요약.</summary>
        public ProfileSummary Summary
        {
            get { return _summary; }
        }

        /// <summary>지금 이름을 고치는 중인지.</summary>
        public bool IsEditing
        {
            get { return _editing; }
        }

        public RectTransform RectTransform
        {
            get { return (RectTransform)transform; }
        }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                // 상태 표현을 이 스크립트가 직접 하므로 Button 의 색 전환은 쓰지 않는다.
                _button.transition = Selectable.Transition.None;
                _button.onClick.AddListener(HandleClicked);
            }

            if (_editButton != null)
            {
                _editButton.transition = Selectable.Transition.None;
                _editButton.onClick.AddListener(HandleEditClicked);
            }

            if (_deleteButton != null)
            {
                _deleteButton.transition = Selectable.Transition.None;
                _deleteButton.onClick.AddListener(HandleDeleteClicked);
            }

            if (_nameInput != null)
            {
                _nameInput.onSubmit.AddListener(HandleNameSubmitted);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }

            if (_editButton != null)
            {
                _editButton.onClick.RemoveListener(HandleEditClicked);
            }

            if (_deleteButton != null)
            {
                _deleteButton.onClick.RemoveListener(HandleDeleteClicked);
            }

            if (_nameInput != null)
            {
                _nameInput.onSubmit.RemoveListener(HandleNameSubmitted);
            }
        }

        /// <summary>자리 하나를 카드에 담는다.</summary>
        public void Bind(
            int index,
            ProfileSummary summary,
            ProfileLayoutConfig layout,
            ProfileVisualConfig visual)
        {
            _index = index;
            _summary = summary;
            _layout = layout;
            _visual = visual;
            _editing = false;

            PlaceInside();
            Refresh();
        }

        /// <summary>카드 안쪽 자리를 기획서대로 잡는다.</summary>
        public void PlaceInside()
        {
            if (_layout == null)
            {
                return;
            }

            float width = _layout.GetCardTextWidth();
            float left = _layout.CardPadding;

            PlaceInsideRect(_nameLabel, left, _layout.NameTop, width, _layout.NameHeight);
            PlaceInsideRect(_playTimeLabel, left, _layout.InfoTop, width, _layout.InfoHeight);
            PlaceInsideRect(
                _lastPlayedLabel, left, _layout.InfoTop + _layout.InfoSpacing, width, _layout.InfoHeight);

            if (_nameInput != null)
            {
                PlaceInsideRect(
                    _nameInput.GetComponent<RectTransform>(),
                    left, _layout.NameTop, width, _layout.NameHeight);
            }

            // 삭제가 맨 오른쪽, 수정이 그 왼쪽이다.
            Vector2 deleteAt = _layout.GetActionButtonPosition(Vector2.zero, 0);
            Vector2 editAt = _layout.GetActionButtonPosition(Vector2.zero, 1);

            PlaceInsideRect(_deletePanel, deleteAt.x, deleteAt.y,
                _layout.ActionButtonSize.x, _layout.ActionButtonSize.y);
            PlaceInsideRect(_editPanel, editAt.x, editAt.y,
                _layout.ActionButtonSize.x, _layout.ActionButtonSize.y);

            if (_emptyLabel != null)
            {
                // 빈 자리 글은 카드 한가운데에 놓는다.
                RectTransform rect = _emptyLabel.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>지금 상태에 맞춰 다시 그린다.</summary>
        public void Refresh()
        {
            if (_visual == null)
            {
                return;
            }

            bool empty = _summary.IsEmpty;
            bool broken = _summary.IsBroken;

            if (_background != null)
            {
                _background.color = _visual.GetCardColor(_index, _hovering, _pressed, broken);
            }

            if (_content != null)
            {
                float scale = _visual.GetCardScale(_hovering);
                _content.localScale = new Vector3(scale, scale, 1f);
            }

            SetActive(_emptyLabel, empty && !_editing);
            SetActive(_nameLabel, !empty && !_editing);
            SetActive(_playTimeLabel, !empty && !_editing);
            SetActive(_lastPlayedLabel, !empty && !_editing);

            // 빈 자리에는 수정과 삭제를 두지 않는다. 고칠 것도 지울 것도 없다.
            SetActive(_editPanel, !empty && !_editing);
            SetActive(_deletePanel, !empty && !_editing);

            if (_nameInput != null)
            {
                _nameInput.gameObject.SetActive(_editing);
            }

            if (_emptyLabel != null)
            {
                _emptyLabel.text = _visual.EmptyText;
                _emptyLabel.fontSize = _visual.EmptyFontSize;
                _emptyLabel.color = _visual.EmptyTextColor;
                _emptyLabel.alignment = TextAlignmentOptions.Center;
            }

            if (_nameLabel != null)
            {
                _nameLabel.text = _summary.Name;
                _nameLabel.fontSize = _visual.NameFontSize;
                _nameLabel.color = _visual.NameColor;
                _nameLabel.alignment = TextAlignmentOptions.Left;
            }

            if (_playTimeLabel != null)
            {
                // 읽을 수 없는 자리에는 플레이 정보 대신 그 사실을 적는다.
                _playTimeLabel.text = broken
                    ? _visual.BrokenText
                    : ProfileText.PlayTime(_visual, _summary);
                _playTimeLabel.fontSize = _visual.InfoFontSize;
                _playTimeLabel.color = _visual.InfoColor;
                _playTimeLabel.alignment = TextAlignmentOptions.Left;
            }

            if (_lastPlayedLabel != null)
            {
                _lastPlayedLabel.text = broken
                    ? string.Empty
                    : ProfileText.LastPlayed(_visual, _summary);
                _lastPlayedLabel.fontSize = _visual.InfoFontSize;
                _lastPlayedLabel.color = _visual.InfoColor;
                _lastPlayedLabel.alignment = TextAlignmentOptions.Left;
            }

            ApplyActionStyle(_editPanel, _editLabel, _visual.EditText, _visual.EditTextColor);
            ApplyActionStyle(_deletePanel, _deleteLabel, _visual.DeleteText, _visual.DeleteTextColor);
        }

        /// <summary>이름을 고치는 상태로 바꾼다. 기획서의 "프로필명 입력 상태"다.</summary>
        public void BeginEditName(string startingName)
        {
            _editing = true;

            if (_nameInput != null)
            {
                _nameInput.text = startingName == null ? string.Empty : startingName;
                _nameInput.Select();
                _nameInput.ActivateInputField();
            }

            Refresh();
        }

        /// <summary>이름 고치기를 끝낸다.</summary>
        public void EndEditName()
        {
            _editing = false;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            Refresh();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            Refresh();
        }

        private void ApplyActionStyle(Image panel, TMP_Text label, string text, Color textColor)
        {
            if (panel != null)
            {
                panel.color = _visual.GetActionButtonColor(false, false);
            }

            if (label != null)
            {
                label.text = text;
                label.fontSize = _visual.ActionFontSize;
                label.color = textColor;
                label.alignment = TextAlignmentOptions.Center;
            }
        }

        private void PlaceInsideRect(TMP_Text label, float x, float y, float width, float height)
        {
            if (label != null)
            {
                PlaceInsideRect(label.rectTransform, x, y, width, height);
            }
        }

        private void PlaceInsideRect(Image image, float x, float y, float width, float height)
        {
            if (image != null)
            {
                PlaceInsideRect(image.rectTransform, x, y, width, height);
            }
        }

        /// <summary>카드 왼쪽 위를 기준으로 자리를 잡는다.</summary>
        private static void PlaceInsideRect(RectTransform rect, float x, float y, float width, float height)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, -y);
        }

        private static void SetActive(TMP_Text label, bool value)
        {
            if (label != null)
            {
                label.gameObject.SetActive(value);
            }
        }

        private static void SetActive(Image image, bool value)
        {
            if (image != null)
            {
                image.gameObject.SetActive(value);
            }
        }

        private void HandleClicked()
        {
            if (!_editing && Clicked != null)
            {
                Clicked(_index);
            }
        }

        private void HandleEditClicked()
        {
            if (EditClicked != null)
            {
                EditClicked(_index);
            }
        }

        private void HandleDeleteClicked()
        {
            if (DeleteClicked != null)
            {
                DeleteClicked(_index);
            }
        }

        private void HandleNameSubmitted(string value)
        {
            if (NameSubmitted != null)
            {
                NameSubmitted(_index, value);
            }
        }
    }
}
