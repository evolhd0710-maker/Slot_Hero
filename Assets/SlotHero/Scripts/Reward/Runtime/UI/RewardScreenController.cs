using System;
using UnityEngine;

namespace SlotHero.Reward.UI
{
    /// <summary>
    /// 보상 화면의 흐름을 맡는다. 인게임 화면 기획서 v0.2 / 10 보상 화면 을 따른다.
    ///
    /// **모든 보상은 받을 때 한 번에 받는다.** 2026년 10월 6일에 원재가 정했다.
    /// 골드, 오버킬 골드, 고른 카드를 아래 버튼을 누르는 순간 함께 준다. 예전에는 골드를 화면을 열 때 먼저 줬다.
    /// 골드만 있고 고를 카드가 없어도 화면을 띄운다. 아래 버튼은 "보상 받기" 다(2026년 10월 6일 원재).
    ///
    /// **카드는 골라 두었다가 아래 버튼으로 받는다.** 2026년 10월 4일에 원재가 정했다.
    /// 기획서 10장은 "카드 입력 시 즉시 획득하고 화면을 종료한다" 였는데 바꿨다.
    ///   - 고르지 않은 카드를 누르면 그 카드로 고른 것이 바뀌고 테두리가 생긴다
    ///   - 이미 고른 카드를 다시 누르면 고른 것이 풀린다
    ///   - 아래 버튼은 고른 것이 없으면 "아이템 선택 건너뛰기", 있으면 "보상 받기" 다
    ///   - 아래 버튼을 눌러야 받기가 끝나고 화면이 닫힌다. 고른 카드가 있으면 그것을 받는다
    ///
    /// 실제로 물건을 넣어 주는 일은 듣는 쪽이 한다. 여기서는 무엇을 받았는지만 알린다.
    /// </summary>
    public class RewardScreenController : MonoBehaviour
    {
        [Header("붙일 것")]
        [SerializeField] private RewardScreenView _view;

        [Header("동작")]
        [Tooltip("켜면 Awake 에서 화면을 닫아 둔다.")]
        // 기본은 끈다. 이 조종기는 제가 숨기는 화면 위에 붙어 있고, 게임 씬은 화면을 꺼 둔 채 시작한다.
        // 꺼진 오브젝트는 처음 켜질 때 Awake 가 돌므로, 켜 두면 흐름이 처음 열 때
        // 켜지는 순간 도로 숨겨 화면이 안 나온다. 켜진 채 시작하는 씬에서만 켠다.
        [SerializeField] private bool _hideOnAwake;

        private RewardOffer _offer;
        private bool _closed;

        /// 지금 골라 둔 카드. 고른 것이 없으면 null 이다.
        private string _selectedCardId;

        /// <summary>
        /// 보상을 받았다. 아래 버튼을 눌렀을 때 한 번 온다.
        /// 골드는 기본과 오버킬을 더한 값이고, 카드는 고른 것이 없으면 null 이다.
        /// </summary>
        public event Action<int, RewardCard> Received;

        /// <summary>카드 위에 올라가거나 벗어났다. 아이템 상세를 띄우는 쪽이 듣는다.</summary>
        public event Action<RewardCard, bool> CardHoverChanged;

        /// <summary>화면이 닫혔다. 카드를 골랐든 건너뛰었든 한 번만 온다.</summary>
        public event Action Closed;

        /// <summary>
        /// 받기 전에 고른 카드를 받아도 되는지 묻는다. false 를 돌려주면 받지 않고 화면을 그대로 둔다.
        /// 코인이나 유물이 소지 한도까지 찼을 때 흐름이 버릴 것을 고르게 하는 데 쓴다(2026년 10월 9일 원재).
        /// 버리고 나면 흐름이 `Confirm` 을 다시 부르고, 포기하면 이 화면에서 다른 카드나 건너뛰기를 고를 수 있다.
        /// 비워 두면 묻지 않고 받는다.
        /// </summary>
        public Func<RewardCard, bool> CanReceive;

        /// <summary>지금 보여 주는 보상.</summary>
        public RewardOffer Offer
        {
            get { return _offer; }
        }

        /// <summary>화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _view != null && _view.gameObject.activeInHierarchy; }
        }

        /// <summary>지금 골라 둔 카드. 고른 것이 없으면 null 이다.</summary>
        public RewardCard SelectedCard
        {
            get { return _offer != null && _selectedCardId != null ? _offer.Find(_selectedCardId) : null; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.CardClicked += HandleCardClicked;
                _view.ConfirmClicked += HandleConfirmClicked;
                _view.CardHoverChanged += HandleCardHover;
            }

            if (_hideOnAwake)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.CardClicked -= HandleCardClicked;
                _view.ConfirmClicked -= HandleConfirmClicked;
                _view.CardHoverChanged -= HandleCardHover;
            }
        }

        /// <summary>
        /// 보상 화면을 연다. 여는 것만으로는 아무것도 주지 않는다.
        ///
        /// 받을 것이 하나도 없으면 열지 않고 바로 닫힌 것으로 친다.
        /// 골드만 있고 고를 카드가 없어도 화면을 띄우고 아래 버튼으로 받는다.
        /// </summary>
        public void Open(RewardOffer offer, IRewardIconSource icons)
        {
            _offer = offer;
            _closed = false;
            _selectedCardId = null;

            if (offer == null || offer.IsEmpty)
            {
                Finish();
                return;
            }

            if (_view != null)
            {
                _view.Show(offer, icons);
                ShowSelection();
            }
        }

        /// <summary>화면을 닫는다. 알리지 않는다.</summary>
        public void Close()
        {
            if (_view != null)
            {
                _view.Hide();
            }
        }

        /// <summary>카드를 누른다. 눌렀을 때와 같은 길이다. 고르거나, 바꾸거나, 풀린다.</summary>
        public void Select(string cardId)
        {
            HandleCardClicked(cardId);
        }

        /// <summary>아래 버튼을 누른다. 눌렀을 때와 같은 길이다. 고른 카드가 있으면 받고 닫는다.</summary>
        public void Confirm()
        {
            HandleConfirmClicked();
        }

        /// <summary>그 카드를 골라 바로 받는다. 카드를 누르고 아래 버튼을 누른 것과 같다.</summary>
        public void Choose(string cardId)
        {
            if (_closed || _offer == null || _offer.Find(cardId) == null)
            {
                return;
            }

            _selectedCardId = cardId;
            ShowSelection();
            HandleConfirmClicked();
        }

        /// <summary>고른 것 없이 받기를 끝낸다. 고른 것을 풀고 아래 버튼을 누른 것과 같다.</summary>
        public void Skip()
        {
            _selectedCardId = null;
            ShowSelection();
            HandleConfirmClicked();
        }

        /// <summary>지금 값으로 다시 그린다.</summary>
        public void Refresh()
        {
            if (_view != null)
            {
                _view.Refresh();
            }
        }

        private void HandleCardClicked(string cardId)
        {
            if (_closed || _offer == null)
            {
                return;
            }

            if (_offer.Find(cardId) == null)
            {
                return;
            }

            // 이미 고른 카드면 고른 것을 푼다. 아니면 그 카드로 바꾼다.
            _selectedCardId = _selectedCardId == cardId ? null : cardId;
            ShowSelection();
        }

        /// <summary>
        /// 아래 버튼. 골드 전부와 고른 카드를 한 번에 받고 끝낸다.
        /// 고른 카드가 없으면 카드만 건너뛰고 골드는 받는다.
        /// </summary>
        private void HandleConfirmClicked()
        {
            if (_closed)
            {
                return;
            }

            if (CanReceive != null && _offer != null && !CanReceive(SelectedCard))
            {
                return;
            }

            if (Received != null && _offer != null)
            {
                Received(_offer.TotalGold, SelectedCard);
            }

            Finish();
        }

        /// <summary>고른 카드의 테두리와 아래 버튼 글을 지금 고른 것에 맞춘다.</summary>
        private void ShowSelection()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetSelected(_selectedCardId);
            // 고를 카드가 없으면 건너뛸 것도 없으므로 "보상 받기" 다.
            bool noCards = _offer == null || !_offer.HasCards;
            _view.SetConfirmText(_selectedCardId != null || noCards);
        }

        private void HandleCardHover(RewardCard card, bool hovering)
        {
            if (CardHoverChanged != null)
            {
                CardHoverChanged(card, hovering);
            }
        }

        /// <summary>한 번만 닫는다. 카드와 건너뛰기가 겹쳐 두 번 불릴 수 있기 때문이다.</summary>
        private void Finish()
        {
            if (_closed)
            {
                return;
            }

            _closed = true;
            Close();

            if (Closed != null)
            {
                Closed();
            }
        }
    }
}
