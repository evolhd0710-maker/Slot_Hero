using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.CurrentBuild;
using SlotHero.Ui;

namespace SlotHero.Reward.UI
{
    /// <summary>
    /// 물건 그림을 내주는 쪽.
    /// 현재 빌드 화면과 행상처럼 보상 화면도 목록을 직접 갖지 않는다.
    /// </summary>
    public interface IRewardIconSource
    {
        /// <summary>그 카드에 놓을 그림. 없으면 null 이다.</summary>
        Sprite GetIcon(RewardCard card);
    }

    /// <summary>
    /// 보상 화면을 그린다.
    /// 인게임 화면 기획서 v0.2 / 10 보상 화면 을 옮긴 것이다.
    ///
    /// 표시만 맡는다. 무엇을 받는지는 조종기가 정한다.
    /// </summary>
    public class RewardScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private RewardLayoutConfig _layout;
        [SerializeField] private RewardVisualConfig _visual;

        [Tooltip("태그 이름을 가져오는 곳. 현재 빌드 화면과 같은 이름을 쓴다.")]
        [SerializeField] private CurrentBuildVisualConfig _buildVisual;

        [Header("화면")]
        [SerializeField] private Image _dim;
        [SerializeField] private Image _box;
        [SerializeField] private TMP_Text _titleLabel;

        [Header("골드")]
        [SerializeField] private Image _baseGoldRow;
        [SerializeField] private TMP_Text _baseGoldLabel;
        [SerializeField] private TMP_Text _baseGoldValue;
        [SerializeField] private Image _baseGoldIcon;

        [SerializeField] private Image _overkillGoldRow;
        [SerializeField] private TMP_Text _overkillGoldLabel;
        [SerializeField] private TMP_Text _overkillGoldValue;
        [SerializeField] private Image _overkillGoldIcon;

        [Tooltip("오버킬 줄의 회복. 골드 오른쪽에 하트와 회복량을 적는다. 회복이 없으면 끈다.")]
        [SerializeField] private Image _overkillHealIcon;
        [SerializeField] private TMP_Text _overkillHealValue;

        [Header("카드")]
        [SerializeField] private RectTransform _cardLayer;
        [SerializeField] private RewardCardView _cardPrefab;

        [Header("아래 버튼")]
        [Tooltip("보상 받기를 끝내는 버튼. 고른 카드가 없으면 \"아이템 선택 건너뛰기\", 있으면 \"보상 받기\" 다. " +
                 "예전 이름 그대로 건너뛰기 칸이다.")]
        [SerializeField] private Image _skipPanel;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TMP_Text _skipLabel;

        private readonly List<RewardCardView> _cards = new List<RewardCardView>();
        private RewardOffer _offer;
        private IRewardIconSource _icons;

        // 열릴 때의 연출. 골드 오르기, 오버킬 회복 등장, 카드 반짝임이 차례로 돈다.
        private bool _revealing;
        private float _revealTime;
        private RewardRevealTimeline _timeline;

        /// <summary>카드를 눌렀다.</summary>
        public event Action<string> CardClicked;

        /// <summary>아래 버튼을 눌렀다. 건너뛰기든 보상 받기든 같다. 무엇을 받는지는 조종기가 안다.</summary>
        public event Action ConfirmClicked;

        /// <summary>아래 버튼에 지금 적혀 있는 글.</summary>
        public string ConfirmText
        {
            get { return _skipLabel != null ? _skipLabel.text : string.Empty; }
        }

        /// <summary>카드 위에 올라가거나 벗어났다.</summary>
        public event Action<RewardCard, bool> CardHoverChanged;

        /// <summary>지금 만들어 둔 카드 칸.</summary>
        public IReadOnlyList<RewardCardView> Cards
        {
            get { return _cards; }
        }

        private void Awake()
        {
            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(HandleSkipClicked);
            }
        }

        private void OnDisable()
        {
            // 연출 도중 꺼지면 끝 모습으로 두고 건너뛰기 맡긴 것도 거둔다. 남겨 두면 화면 누르기를 계속 가로챈다.
            StopReveal();
        }

        private void OnDestroy()
        {
            if (_skipButton != null)
            {
                _skipButton.onClick.RemoveListener(HandleSkipClicked);
            }
        }

        /// <summary>보상 한 벌을 그린다.</summary>
        public void Show(RewardOffer offer, IRewardIconSource icons)
        {
            _offer = offer;
            _icons = icons;

            gameObject.SetActive(true);

            BuildCards();
            ApplyLayout();
            ApplyStyle();
            Refresh();
            StartReveal();
        }

        /// <summary>화면을 닫는다.</summary>
        public void Hide()
        {
            StopReveal();
            gameObject.SetActive(false);
        }

        /// <summary>열릴 때의 연출이 도는 중인지.</summary>
        public bool IsRevealing
        {
            get { return _revealing; }
        }

        /// <summary>
        /// 열릴 때의 연출을 시작한다. 2026년 11월 1일 원재가 넷을 차례로 보이게 했다.
        ///   1. 기본 보상 골드가 0 에서 받는 골드까지 오른다
        ///   2. 오버킬 골드가 0 에서 받는 골드까지 오른다
        ///   3. 오버킬 회복(하트와 회복량)이 숫자가 오르지 않고 팝업처럼 튀어나온다
        ///   4. 세 카드가 함께 반짝인다. 반짝임은 예전의 세 배인 1.2초다
        /// 없는 것(오버킬 골드 0, 회복 0)은 건너뛴다. 연출 중 화면을 누르면 끝 모습으로 넘긴다(`EffectSkip`).
        /// 게임이 돌 때만 움직인다. 편집 모드 검사와 그림 뽑기는 바로 끝 모습이다.
        /// </summary>
        private void StartReveal()
        {
            if (!Application.isPlaying || _visual == null || !isActiveAndEnabled || _offer == null)
            {
                StopReveal();
                return;
            }

            BeginReveal();
        }

        private void BeginReveal()
        {
            _revealing = true;
            _revealTime = 0f;
            _timeline = _visual.GetRevealTimeline(_offer.OverkillGold > 0, _offer.HasOverkillHeal);
            EffectSkip.Begin(this, SkipReveal);
            UpdateReveal();
        }

        /// <summary>화면을 눌러 연출을 건너뛰었다. 바로 끝 모습이 된다.</summary>
        private void SkipReveal()
        {
            StopReveal();
        }

        private void StopReveal()
        {
            EffectSkip.End(this);
            if (!_revealing)
            {
                return;
            }

            _revealing = false;
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null)
                {
                    _cards[i].SetFlash(0f);
                }
            }

            SetHealScale(1f);
            Refresh();
        }

        private void Update()
        {
            if (!_revealing)
            {
                return;
            }

            _revealTime += EffectClock.Delta;
            UpdateReveal();
        }

        private void UpdateReveal()
        {
            float flashSeconds = _visual.CardFlashSeconds;
            float goldSeconds = _visual.GoldCountSeconds;

            float flashTime = _revealTime - _timeline.CardFlashStart;
            float flash = flashSeconds <= 0f || flashTime <= 0f
                ? 0f
                : Mathf.Sin(Mathf.Clamp01(flashTime / flashSeconds) * Mathf.PI);
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null)
                {
                    _cards[i].SetFlash(flash);
                }
            }

            if (_baseGoldValue != null)
            {
                _baseGoldValue.text = RewardText.Gold(RewardVisualConfig.GetCountedGold(
                    _offer.BaseGold, _revealTime - _timeline.BaseGoldStart, goldSeconds));
            }

            if (_overkillGoldValue != null)
            {
                _overkillGoldValue.text = RewardText.Gold(RewardVisualConfig.GetCountedGold(
                    _offer.OverkillGold, _revealTime - _timeline.OverkillGoldStart, goldSeconds));
            }

            if (_offer.HasOverkillHeal)
            {
                SetHealScale(RewardVisualConfig.GetPopScale(
                    _revealTime - _timeline.HealPopStart, _visual.HealPopSeconds, _visual.HealPopPeakScale));
            }

            if (_revealTime >= _timeline.End)
            {
                StopReveal();
            }
        }

        /// <summary>오버킬 회복(하트와 회복량)의 배율. 0 이면 아직 튀어나오지 않아 보이지 않는다.</summary>
        private void SetHealScale(float scale)
        {
            Vector3 size = new Vector3(scale, scale, 1f);
            if (_overkillHealIcon != null)
            {
                _overkillHealIcon.rectTransform.localScale = size;
            }

            if (_overkillHealValue != null)
            {
                _overkillHealValue.rectTransform.localScale = size;
            }
        }

        /// <summary>오버킬 회복의 지금 배율. 튀어나오기 전은 0, 다 나오면 1 이다.</summary>
        public float HealScale
        {
            get { return _overkillHealValue != null ? _overkillHealValue.rectTransform.localScale.x : 1f; }
        }

        /// <summary>연출 시각을 옮긴다. 검사가 연출 차례를 확인할 때 쓴다.</summary>
        public void AdvanceReveal(float seconds)
        {
            if (!_revealing)
            {
                return;
            }

            _revealTime += seconds;
            UpdateReveal();
        }

        /// <summary>보상 화면의 연출 차례. 검사가 쓴다.</summary>
        public RewardRevealTimeline RevealTimeline
        {
            get { return _timeline; }
        }

        /// <summary>
        /// 검사용. 편집 모드에서도 연출을 처음부터 시작한다. 게임에서는 열릴 때 저절로 시작한다.
        /// </summary>
        public void BeginRevealForCheck()
        {
            if (_visual == null || _offer == null)
            {
                return;
            }

            BeginReveal();
        }

        /// <summary>지금 값으로 글자를 다시 채운다.</summary>
        public void Refresh()
        {
            if (_offer == null)
            {
                return;
            }

            if (_baseGoldValue != null)
            {
                _baseGoldValue.text = RewardText.Gold(_offer.BaseGold);
            }

            if (_overkillGoldValue != null)
            {
                _overkillGoldValue.text = RewardText.Gold(_offer.OverkillGold);
            }

            if (_overkillHealValue != null)
            {
                _overkillHealValue.text = RewardText.Heal(_offer.OverkillHeal);
            }

            // 오버킬 골드도 회복도 없으면 그 줄은 비워 둔다.
            if (_overkillGoldRow != null)
            {
                _overkillGoldRow.gameObject.SetActive(_offer.HasOverkill);
            }

            SetActive(_overkillHealIcon, _offer.HasOverkillHeal);
            SetActive(_overkillHealValue, _offer.HasOverkillHeal);

            // 회복이 있으면 골드와 회복이 줄을 나눠 쓴다. 자리가 달라지니 다시 놓는다.
            PlaceOverkillRow();

            // 연출 도중이면 최종 값 대신 지금 연출 시각의 모습으로 되돌린다.
            // 그러지 않으면 골드가 잠깐 최종 값으로 튀었다 돌아간다(2026년 11월 1일 코덱스 검토).
            if (_revealing)
            {
                UpdateReveal();
            }
        }

        /// <summary>
        /// 기획서 10장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            int cardCount = _offer != null ? _offer.Cards.Count : 0;

            if (!_layout.SkipIsBelowBox())
            {
                Debug.LogWarning("건너뛰기가 보상 박스 아래에 있지 않다. 자리를 다시 본다.", this);
            }

            if (_dim != null)
            {
                Stretch(_dim.rectTransform);
            }

            PlaceTopLeft(_box, _layout.GetBoxPosition(cardCount), _layout.GetBoxSize(cardCount));
            PlaceTopLeftText(_titleLabel, _layout.TitlePosition, _layout.TitleSize);

            PlaceTopLeft(_baseGoldRow, _layout.BaseGoldPosition, _layout.GoldRowSize);
            PlaceTopLeft(_overkillGoldRow, _layout.OverkillGoldPosition, _layout.GoldRowSize);

            PlaceGoldRow(_baseGoldLabel, _baseGoldValue, _baseGoldIcon);
            PlaceOverkillRow();

            for (int i = 0; i < _cards.Count; i++)
            {
                PlaceTopLeftRect(
                    _cards[i].RectTransform,
                    _layout.GetCardPosition(i, _cards.Count),
                    _layout.CardSize);
            }

            PlaceTopLeft(_skipPanel, _layout.SkipPosition, _layout.SkipSize);
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

            if (_box != null)
            {
                _box.color = _visual.BoxColor;
            }

            ApplyLabel(_titleLabel, _visual.TitleText, _visual.TitleFontSize, _visual.TextColor);

            if (_baseGoldRow != null)
            {
                _baseGoldRow.color = _visual.GoldRowColor;
            }

            if (_overkillGoldRow != null)
            {
                _overkillGoldRow.color = _visual.GoldRowColor;
            }

            ApplyLabel(
                _baseGoldLabel, _visual.BaseGoldText, _visual.GoldLabelFontSize, _visual.TextColor);
            ApplyLabel(
                _overkillGoldLabel, _visual.OverkillGoldText, _visual.GoldLabelFontSize, _visual.TextColor);

            ApplyLabel(_baseGoldValue, null, _visual.GoldValueFontSize, _visual.TextColor);
            ApplyLabel(_overkillGoldValue, null, _visual.GoldValueFontSize, _visual.TextColor);
            ApplyLabel(_overkillHealValue, null, _visual.GoldValueFontSize, _visual.TextColor);

            if (_skipPanel != null)
            {
                _skipPanel.color = _visual.SkipColor;
            }

            ApplyLabel(_skipLabel, _visual.SkipText, _visual.SkipFontSize, _visual.SkipTextColor);

            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].ApplyStyle();
            }
        }

        /// <summary>
        /// 아래 버튼 글을 바꾼다. 고른 카드가 있으면 "보상 받기", 없으면 "아이템 선택 건너뛰기" 다.
        /// </summary>
        public void SetConfirmText(bool hasSelection)
        {
            if (_skipLabel == null || _visual == null)
            {
                return;
            }

            _skipLabel.text = hasSelection ? _visual.ConfirmText : _visual.SkipText;
        }

        /// <summary>고른 카드 하나만 밝게 만들고 테두리를 두른다. 비우면 고른 것이 없다.</summary>
        public void SetSelected(string cardId)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                RewardCard card = _cards[i].Card;
                _cards[i].SetSelected(card != null && card.Id == cardId);
            }
        }

        private void BuildCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null)
                {
                    _cards[i].Clicked -= HandleCardClicked;
                    _cards[i].HoverChanged -= HandleCardHover;

                    // 편집 모드 검사는 프레임이 돌지 않아 Destroy 로는 옛 카드가 새 카드와 겹쳐 남는다.
                    // 지도와 같이 편집 모드에서는 바로 지운다(2026년 10월 22일 코덱스 검토).
                    if (Application.isPlaying)
                    {
                        Destroy(_cards[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(_cards[i].gameObject);
                    }
                }
            }

            _cards.Clear();

            if (_offer == null || _cardPrefab == null || _cardLayer == null)
            {
                return;
            }

            for (int i = 0; i < _offer.Cards.Count; i++)
            {
                RewardCard card = _offer.Cards[i];

                RewardCardView view = Instantiate(_cardPrefab, _cardLayer);
                view.gameObject.SetActive(true);
                view.Bind(
                    card, _layout, _visual,
                    RewardText.Tags(card, _buildVisual),
                    _icons != null ? _icons.GetIcon(card) : null,
                    _buildVisual);

                view.Clicked += HandleCardClicked;
                view.HoverChanged += HandleCardHover;

                _cards.Add(view);
            }
        }

        private void PlaceGoldRow(TMP_Text label, TMP_Text value, Image icon)
        {
            if (_layout == null)
            {
                return;
            }

            float padding = _layout.GoldRowPadding;
            float width = _layout.GoldRowSize.x;
            float height = _layout.GoldRowSize.y;

            // 줄 안에서 이름은 왼쪽, 아이콘과 액수는 오른쪽에 붙는다.
            if (label != null)
            {
                RectTransform rect = label.rectTransform;
                InsideRow(rect, padding, width * 0.5f, height);
                label.alignment = TextAlignmentOptions.Left;
            }

            if (icon != null)
            {
                InsideRow(icon.rectTransform, width * 0.55f, height * 0.5f, height * 0.5f);
            }

            if (value != null)
            {
                RectTransform rect = value.rectTransform;
                InsideRow(rect, width * 0.55f + height * 0.6f, width * 0.45f - height * 0.6f - padding, height);
                value.alignment = TextAlignmentOptions.Left;
            }
        }

        /// <summary>
        /// 오버킬 줄. 회복이 없으면 기본 보상 줄과 같다.
        /// 회복이 있으면 오른쪽 절반을 골드와 하트 회복 둘로 나눈다(2026년 10월 30일).
        /// </summary>
        private void PlaceOverkillRow()
        {
            PlaceGoldRow(_overkillGoldLabel, _overkillGoldValue, _overkillGoldIcon);

            if (_layout == null || _offer == null || !_offer.HasOverkillHeal)
            {
                return;
            }

            float padding = _layout.GoldRowPadding;
            float width = _layout.GoldRowSize.x;
            float height = _layout.GoldRowSize.y;
            float icon = height * 0.45f;
            float start = width * 0.5f;
            float pair = (width - padding - start) * 0.5f;

            if (_overkillGoldIcon != null)
            {
                InsideRow(_overkillGoldIcon.rectTransform, start, icon, icon);
            }

            if (_overkillGoldValue != null)
            {
                InsideRow(_overkillGoldValue.rectTransform, start + icon + UiScale.Px(6f), pair - icon - UiScale.Px(6f), height);
            }

            if (_overkillHealIcon != null)
            {
                InsideRow(_overkillHealIcon.rectTransform, start + pair, icon, icon);
            }

            if (_overkillHealValue != null)
            {
                InsideRow(_overkillHealValue.rectTransform, start + pair + icon + UiScale.Px(6f), pair - icon - UiScale.Px(6f), height);
                _overkillHealValue.alignment = TextAlignmentOptions.Left;
                _overkillHealValue.textWrappingMode = TextWrappingModes.NoWrap;
            }

            if (_overkillGoldValue != null)
            {
                _overkillGoldValue.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        private static void SetActive(Component component, bool on)
        {
            if (component != null && component.gameObject.activeSelf != on)
            {
                component.gameObject.SetActive(on);
            }
        }

        private static void InsideRow(RectTransform rect, float x, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, 0f);
        }

        private void HandleCardClicked(string cardId)
        {
            if (CardClicked != null)
            {
                CardClicked(cardId);
            }
        }

        private void HandleCardHover(RewardCard card, bool hovering)
        {
            if (CardHoverChanged != null)
            {
                CardHoverChanged(card, hovering);
            }
        }

        private void HandleSkipClicked()
        {
            if (ConfirmClicked != null)
            {
                ConfirmClicked();
            }
        }

        private static void ApplyLabel(TMP_Text label, string text, float fontSize, Color color)
        {
            if (label == null)
            {
                return;
            }

            if (text != null)
            {
                label.text = text;
            }

            label.fontSize = fontSize;
            label.color = color;
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다. 기획서 좌표를 그대로 넣을 수 있다.</summary>
        private static void PlaceTopLeft(Image image, Vector2 position, Vector2 size)
        {
            if (image != null)
            {
                PlaceTopLeftRect(image.rectTransform, position, size);
            }
        }

        private static void PlaceTopLeftText(TMP_Text label, Vector2 position, Vector2 size)
        {
            if (label != null)
            {
                PlaceTopLeftRect(label.rectTransform, position, size);
            }
        }

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

        private static void Stretch(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
