using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Popup.UI
{
    /// <summary>
    /// 팝업을 띄우고 닫는다.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 과 14 진행 중 팝업 이 모두 이 창구를 쓴다.
    ///
    /// 어떤 팝업이 언제 뜨는지는 각 화면이 정하고
    /// 이 스크립트는 받은 내용을 띄우고 무엇을 눌렀는지 알리는 데까지만 한다.
    /// 04 화면 공통 규칙 의 레이어 순서대로 팝업은 아래 레이어를 조작할 수 없게 막는다.
    /// </summary>
    public class PopupPresenter : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private PopupView _view;

        private Action<string> _onClosed;

        /// 앞 팝업이 떠 있는 동안 들어온 팝업. 앞 팝업이 닫히면 차례로 띄운다.
        private readonly List<PendingPopup> _pending = new List<PendingPopup>();

        private struct PendingPopup
        {
            public PopupSpec Spec;
            public Action<string> OnClosed;
        }

        /// <summary>팝업이 닫힐 때. 어느 버튼을 눌렀는지 넘긴다.</summary>
        public event Action<string> Closed;

        /// <summary>팝업이 떠 있는지.</summary>
        public bool IsOpen
        {
            // activeSelf 가 아니라 activeInHierarchy 로 본다.
            // 팝업 뿌리가 화면 오브젝트의 자식이라 화면이 통째로 꺼져도 뿌리는 켜진 채로 남는다.
            // 그때 activeSelf 로 보면 팝업이 늘 떠 있는 것으로 읽혀
            // 팝업 때문에 멈추는 것들이 영영 멈춰 있는다.
            get { return _popupRoot != null && _popupRoot.activeInHierarchy; }
        }

        /// <summary>지금 떠 있는 팝업의 내용.</summary>
        public PopupSpec Current { get; private set; }

        private void Awake()
        {
            if (_view != null)
            {
                _view.ButtonClicked += HandleButtonClicked;
            }

            SetActive(false);
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.ButtonClicked -= HandleButtonClicked;
            }
        }

        /// <summary>뒤에 기다리는 팝업 수.</summary>
        public int PendingCount
        {
            get { return _pending.Count; }
        }

        /// <summary>
        /// 팝업을 띄운다.
        /// onClosed 는 어느 버튼을 눌러 닫혔는지 받는다. 필요 없으면 비워 둔다.
        ///
        /// **이미 떠 있으면 덮어쓰지 않고 뒤에 세운다.** 앞 팝업이 닫히면 이어서 뜬다.
        /// 예전에는 덮어써서 앞 팝업이 기다리던 대답이 영영 오지 않았다.
        /// 전투 대역이 팝업으로 이기기와 지기를 기다리는 동안 저장 실패 알림이 뜨면
        /// 전투가 끝나지 않아 런이 멈췄다.
        /// </summary>
        public void Show(PopupSpec spec, Action<string> onClosed)
        {
            if (spec == null || _view == null)
            {
                return;
            }

            if (IsOpen)
            {
                PendingPopup waiting = new PendingPopup();
                waiting.Spec = spec;
                waiting.OnClosed = onClosed;
                _pending.Add(waiting);
                return;
            }

            Current = spec;
            _onClosed = onClosed;

            SetActive(true);
            _view.Show(spec);
        }

        /// <summary>팝업을 띄운다. 결과를 받지 않는다.</summary>
        public void Show(PopupSpec spec)
        {
            Show(spec, null);
        }

        /// <summary>버튼을 누르지 않고 닫는다.</summary>
        public void Dismiss()
        {
            CloseWith(null);
        }

        /// <summary>
        /// 그 팝업을 거둔다. 떠 있으면 버튼을 누르지 않고 닫고, 뒤에 서 있으면 줄에서 뺀다.
        /// 다른 팝업은 건드리지 않는다. 저장 파일을 기다리는 팝업처럼 띄운 쪽이 스스로 걷을 때 쓴다.
        /// </summary>
        public void Withdraw(PopupSpec spec)
        {
            if (spec == null)
            {
                return;
            }

            if (Current == spec)
            {
                Dismiss();
                return;
            }

            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (_pending[i].Spec == spec)
                {
                    _pending.RemoveAt(i);
                }
            }
        }

        private void HandleButtonClicked(string buttonId)
        {
            CloseWith(buttonId);
        }

        private void CloseWith(string buttonId)
        {
            if (!IsOpen)
            {
                return;
            }

            Action<string> callback = _onClosed;

            Current = null;
            _onClosed = null;
            SetActive(false);

            if (callback != null)
            {
                callback(buttonId);
            }

            if (Closed != null)
            {
                Closed(buttonId);
            }

            // 기다리던 팝업을 띄운다. 닫힌 팝업의 대답을 받은 쪽이 이미 새 팝업을 띄웠으면 그 뒤로 미룬다.
            if (!IsOpen && _pending.Count > 0)
            {
                PendingPopup next = _pending[0];
                _pending.RemoveAt(0);
                Show(next.Spec, next.OnClosed);
            }
        }

        private void SetActive(bool value)
        {
            if (_popupRoot != null && _popupRoot.activeSelf != value)
            {
                _popupRoot.SetActive(value);
            }
        }
    }
}
