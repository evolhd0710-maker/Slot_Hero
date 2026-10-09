using System;
using UnityEngine;

namespace SlotHero.Events.UI
{
    /// <summary>
    /// 이벤트 방의 진행 처리.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 만 따른다.
    ///
    /// 어떤 이벤트가 나오고 무엇을 주는지는 이벤트 기획서 소관이라
    /// 이 스크립트는 받은 페이지를 그리고 무엇을 골랐는지 알리는 데까지만 맡는다.
    /// 고른 뒤의 결과 문장과 이어지는 선택지도 바깥에서 <see cref="ApplyResult"/>로 넘겨 준다.
    ///
    /// 보상 지급도 전투 보상과 같게 처리하므로 여기서 하지 않는다.
    /// 기획서대로 본문에 글로만 적히고 실제 지급은 보상 쪽이 맡는다.
    /// </summary>
    public class EventScreenController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private EventScreenView _view;

        private EventPage _page;
        private EventProgress _progress = new EventProgress();
        private IEventIllustrationSource _illustrations;

        /// <summary>선택지를 골랐을 때. 이벤트 식별자와 선택지 식별자를 넘긴다.</summary>
        public event Action<string, string> ChoiceSelected;

        /// <summary>이어지는 선택지가 없어 이벤트가 끝났을 때. 이벤트 식별자를 넘긴다.</summary>
        public event Action<string> Finished;

        /// <summary>지금 보고 있는 페이지.</summary>
        public EventPage Page
        {
            get { return _page; }
        }

        /// <summary>이벤트 진행 기록. 저장 시스템이 런 데이터에 실을 수 있다.</summary>
        public EventProgress Progress
        {
            get { return _progress; }
        }

        /// <summary>이벤트 화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _screenRoot != null && _screenRoot.activeInHierarchy; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.ChoiceClicked += HandleChoiceClicked;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.ChoiceClicked -= HandleChoiceClicked;
            }
        }

        /// <summary>
        /// 이벤트 방에 들어와 첫 페이지를 연다.
        ///
        /// **받은 쪽을 그대로 쓰지 않고 복사해 쓴다.**
        /// 넘겨주는 쪽이 보통 설정 에셋이 들고 있는 쪽을 주는데,
        /// 화면이 그것을 제자리에서 고쳐 쓰면 에셋이 영구히 바뀌어
        /// 다음에 같은 이벤트가 나올 때 앞의 진행이 그대로 이어진다.
        /// 복사를 부르는 쪽에 맡기지 않고 여기서 하는 것은 한 군데만 보면 되게 하려는 것이다.
        /// </summary>
        public void Open(string eventId, EventPage page, IEventIllustrationSource illustrations)
        {
            _illustrations = illustrations;
            _page = page != null ? page.Clone() : null;
            _progress.Begin(eventId);

            if (_page != null && _page.Choices != null)
            {
                for (int i = 0; i < _page.Choices.Count; i++)
                {
                    if (_page.Choices[i] != null)
                    {
                        _page.Choices[i].ResolveState();
                    }
                }
            }

            SetActive(true);
            Refresh();
        }

        /// <summary>
        /// 고른 선택지의 결과를 화면에 반영한다.
        /// 11 이벤트 화면 · 선택 후 대로 본문이 결과 문장으로 바뀌고,
        /// 고른 선택지는 위에 남으며, 고르지 않은 선택지는 사라지고,
        /// 이어지는 선택지가 있으면 그 아래에 쌓인다.
        /// 이어지는 선택지가 없으면 이벤트가 끝난다.
        /// </summary>
        public void ApplyResult(EventResult result)
        {
            if (_page == null || result == null)
            {
                return;
            }

            // Open 과 같은 까닭으로 복사해 쓴다.
            // 이어지는 선택지에 고른 표시를 적어 넣기 때문에 원본을 쓰면 설정 에셋이 바뀐다.
            EventResult copy = result.Clone();

            int maxChoices = _view != null ? _view.MaxChoiceCount : 0;
            if (!_page.ApplyResult(copy, maxChoices))
            {
                return;
            }

            Refresh();

            if (copy.HasNextChoices)
            {
                return;
            }

            _progress.Finish();

            if (Finished != null)
            {
                Finished(_progress.EventId);
            }
        }

        /// <summary>이벤트 화면을 닫는다. 방을 끝내는 처리는 맵 쪽이 맡는다.</summary>
        public void Close()
        {
            SetActive(false);
        }

        /// <summary>지금 페이지를 다시 그린다.</summary>
        public void Refresh()
        {
            if (_view != null)
            {
                _view.Show(_page, _illustrations);
            }
        }

        private void HandleChoiceClicked(string choiceId)
        {
            if (_page == null)
            {
                return;
            }

            EventChoice choice = _page.Find(choiceId);
            if (choice == null || !choice.IsSelectable)
            {
                return;
            }

            // 결과를 알 수 없는 선택지도 확인 팝업을 거치지 않고 바로 고른다.
            _progress.Record(choiceId);

            if (ChoiceSelected != null)
            {
                ChoiceSelected(_progress.EventId, choiceId);
            }
        }

        private void SetActive(bool value)
        {
            if (_screenRoot != null && _screenRoot.activeSelf != value)
            {
                _screenRoot.SetActive(value);
            }
        }
    }
}
