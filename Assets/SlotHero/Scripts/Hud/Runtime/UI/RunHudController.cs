using System;
using UnityEngine;

namespace SlotHero.Hud.UI
{
    /// <summary>
    /// 런 진행 중 화면의 고정 자리를 맡는다.
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의
    /// 현재 빌드 버튼과 그 아래 자동 저장 표시 자리를 한 덩어리로 다룬다.
    ///
    /// 상단 표시줄은 상단 UI 바 기획서가 따로 맡으므로 여기서 건드리지 않는다.
    /// 버튼을 눌러 열리는 현재 빌드 화면도 같은 기획서 13장 소관이라 신호만 보낸다.
    /// </summary>
    public class RunHudController : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private HudLayoutConfig _layout;

        [Header("참조")]
        [SerializeField] private CurrentBuildButton _currentBuildButton;
        [SerializeField] private AutoSaveNotice _autoSaveNotice;

        [Header("문구")]
        [Tooltip("현재 빌드 화면을 보고 있어 버튼을 누를 수 없을 때의 이유. " +
                 "04 화면 공통 규칙 은 비활성 버튼에 이유를 적게 한다.")]
        [SerializeField] private string _openReason = "현재 빌드 화면을 보고 있다";

        /// <summary>현재 빌드 버튼을 눌렀을 때. 현재 빌드 화면을 여는 쪽이 받는다.</summary>
        public event Action CurrentBuildRequested;

        /// <summary>현재 빌드 버튼에 마우스가 올라가거나 벗어났을 때. 비활성 이유를 함께 넘긴다.</summary>
        public event Action<bool, string> CurrentBuildHoverChanged;

        /// <summary>현재 빌드 버튼.</summary>
        public CurrentBuildButton CurrentBuildButton
        {
            get { return _currentBuildButton; }
        }

        /// <summary>자동 저장 표시.</summary>
        public AutoSaveNotice AutoSaveNotice
        {
            get { return _autoSaveNotice; }
        }

        private void Awake()
        {
            if (_currentBuildButton != null)
            {
                _currentBuildButton.Clicked += HandleCurrentBuildClicked;
                _currentBuildButton.HoverChanged += HandleCurrentBuildHover;
            }

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_currentBuildButton != null)
            {
                _currentBuildButton.Clicked -= HandleCurrentBuildClicked;
                _currentBuildButton.HoverChanged -= HandleCurrentBuildHover;
            }
        }

        /// <summary>
        /// 기획서 04장의 고정 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.AutoSaveNoticeFitsUnderButton())
            {
                Debug.LogWarning(
                    "고정 자리 설정에서 자동 저장 표시가 현재 빌드 버튼 아래에 들어가지 않는다. " +
                    "기획서 기준은 모서리 여백 70 = 간격 15 + 표시 높이 40 + 간격 15 이다.",
                    this);
            }

            if (_currentBuildButton != null)
            {
                RectTransform buttonRect = _currentBuildButton.RectTransform;
                AnchorToBottomCorner(buttonRect, _layout);
                buttonRect.anchoredPosition = _layout.GetButtonPosition();
                _currentBuildButton.ApplySize(_layout.ButtonSize, _layout.IconSize);
            }

            if (_autoSaveNotice != null)
            {
                RectTransform noticeRect = (RectTransform)_autoSaveNotice.transform;
                AnchorToBottomCorner(noticeRect, _layout);
                noticeRect.anchoredPosition = _layout.GetAutoSaveNoticePosition();
                _autoSaveNotice.ApplySize(_layout.AutoSaveNoticeSize);
                _autoSaveNotice.ApplyStyle();
            }
        }

        /// <summary>
        /// 현재 빌드 화면이 열리고 닫힐 때 부른다.
        /// 열려 있는 동안에는 버튼을 누를 수 없게 하고 화면에서도 감춘다.
        /// </summary>
        public void SetCurrentBuildOpen(bool open)
        {
            if (_currentBuildButton == null)
            {
                return;
            }

            _currentBuildButton.SetInteractable(!open, _openReason);
            _currentBuildButton.SetVisible(!open);
        }

        /// <summary>
        /// 다른 이유로 버튼을 막을 때 부른다.
        /// 이쪽은 감추지 않고 비활성 표시만 하므로 04 화면 공통 규칙 대로 이유를 적어 보여 준다.
        /// </summary>
        public void SetCurrentBuildAvailable(bool available, string reason)
        {
            if (_currentBuildButton != null)
            {
                _currentBuildButton.SetInteractable(available, reason);
            }
        }

        /// <summary>자동 저장이 끝났을 때 부른다. 저장 시점은 저장 시스템 기획서가 정한다.</summary>
        public void NotifyAutoSaved()
        {
            if (_autoSaveNotice != null)
            {
                _autoSaveNotice.Show();
            }
        }

        /// <summary>설정 화면의 자동 저장 알림 항목을 반영한다.</summary>
        public void SetAutoSaveNoticeEnabled(bool value)
        {
            if (_autoSaveNotice == null)
            {
                return;
            }

            _autoSaveNotice.NoticeEnabled = value;

            if (!value)
            {
                _autoSaveNotice.HideNow();
            }
        }

        /// <summary>기준점과 피벗을 화면 우측 하단에 맞춘다. 04 화면 공통 규칙 의 자리는 모두 그 모서리 기준이다.</summary>
        /// <summary>
        /// 설정이 고른 아래쪽 구석에 기준점과 피벗을 둔다.
        /// 자리 값은 `HudLayoutConfig` 가 같은 쪽 부호로 내준다.
        /// </summary>
        private static void AnchorToBottomCorner(RectTransform rect, HudLayoutConfig layout)
        {
            float side = layout != null && layout.OnLeftCorner ? 0f : 1f;
            rect.anchorMin = new Vector2(side, 0f);
            rect.anchorMax = new Vector2(side, 0f);
            rect.pivot = new Vector2(side, 0f);
        }

        private void HandleCurrentBuildClicked()
        {
            if (CurrentBuildRequested != null)
            {
                CurrentBuildRequested();
            }
        }

        private void HandleCurrentBuildHover(bool hovering, string disabledReason)
        {
            if (CurrentBuildHoverChanged != null)
            {
                CurrentBuildHoverChanged(hovering, disabledReason);
            }
        }
    }
}
