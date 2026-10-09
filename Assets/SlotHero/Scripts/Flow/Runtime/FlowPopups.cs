using System;
using System.Collections.Generic;
using SlotHero.Popup;
using SlotHero.Popup.UI;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 흐름이 띄우는 팝업. 글과 버튼을 한 곳에 모았다.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 과 14 진행 중 팝업 의 공통 틀(`PopupSpec`)로 띄운다.
    ///
    /// 무엇을 할지는 부르는 쪽이 정한다. 여기서는 팝업을 띄우고, 확인을 받으면 넘겨받은 일을 부른다.
    /// 그래서 확인을 기다리는 동안 무엇을 하려던 것인지 따로 적어 둘 필요가 없다.
    ///
    /// 2026년 10월 8일에 `GameFlowController` 에서 떼어 냈다. 원재가 "지도 버튼과 팝업 처리도 나눠 줘" 라고 했다.
    /// </summary>
    public class FlowPopups
    {
        /// <summary>확인 버튼의 식별자. `PopupSpec.Confirm` 과 `PopupSpec.Notice` 가 이것을 쓴다.</summary>
        public const string ConfirmId = "confirm";

        /// <summary>런 종료 팝업의 버튼 식별자.</summary>
        public const string EndRunCancelId = "cancel";
        public const string EndRunAbandonId = "abandon";
        public const string EndRunSaveId = "save";

        private readonly PopupPresenter _popup;

        /// <summary>팝업 창구를 물려 만든다.</summary>
        public FlowPopups(PopupPresenter popup)
        {
            _popup = popup;
        }

        /// <summary>팝업을 띄울 수 있는지. 창구가 붙어 있어야 한다.</summary>
        public bool Available
        {
            get { return _popup != null; }
        }

        /// <summary>팝업이 떠 있는지.</summary>
        public bool IsOpen
        {
            get { return _popup != null && _popup.IsOpen; }
        }

        // ---- 프로필 ----

        /// <summary>06 프로필 선택 화면 의 "프로필 삭제 확인 팝업". 삭제를 누르면 onConfirm 을 부른다.</summary>
        public void ConfirmProfileDelete(string profileName, Action onConfirm)
        {
            Show(
                PopupSpec.Confirm(
                    "프로필을 삭제하시겠습니까?",
                    profileName + " 의 기록이 모두 사라집니다. 되돌릴 수 없습니다.",
                    "돌아가기",
                    "삭제",
                    true),
                onConfirm);
        }

        /// <summary>읽을 수 없는 프로필을 눌렀을 때. 06 프로필 선택 화면 의 저장 데이터 오류 팝업이다.</summary>
        public void NotifyProfileBroken()
        {
            Show(PopupSpec.Notice(
                "저장 데이터 오류",
                "이 프로필의 저장 파일을 읽지 못했습니다.\n삭제한 뒤 새로 만들 수 있습니다.",
                "확인"));
        }

        // ---- 저장 파일 읽기 ----

        /// <summary>기다리는 팝업의 그만두기 버튼 식별자.</summary>
        public const string ReadWaitStopId = "stop";

        /// 지금 띄운 기다리는 팝업. 다시 읽기가 끝나면 이것만 거둔다.
        private PopupSpec _readWaiting;

        /// <summary>
        /// 저장 파일을 잠시 읽지 못해 기다렸다 다시 읽는 동안 띄운다. 그만두기를 누르면 onStop 을 부른다.
        /// 다시 읽기가 끝나면 흐름이 `HideReadWaiting` 으로 거둔다.
        /// 2026년 10월 9일 원재가 "팝업을 띄운 후 여유를 두고 재시도" 하라고 정했다.
        /// </summary>
        public void ShowReadWaiting(Action onStop)
        {
            HideReadWaiting();

            if (_popup == null)
            {
                return;
            }

            PopupSpec spec = new PopupSpec(
                "저장 파일을 기다리는 중",
                "다른 프로그램이 저장 파일을 쓰고 있어 지금 읽지 못했습니다.\n"
                + "백신 검사나 동기화가 끝나기를 기다렸다가 자동으로 다시 읽습니다.");
            spec.Add(PopupButtonSpec.Normal(ReadWaitStopId, "그만두기"));
            _readWaiting = spec;

            _popup.Show(spec, buttonId =>
            {
                if (_readWaiting == spec)
                {
                    _readWaiting = null;
                }

                // 흐름이 거둔 것이면 버튼 식별자가 비어 온다. 그때는 부르지 않는다.
                if (buttonId == ReadWaitStopId && onStop != null)
                {
                    onStop();
                }
            });
        }

        /// <summary>기다리는 팝업을 거둔다. 다른 팝업은 건드리지 않는다.</summary>
        public void HideReadWaiting()
        {
            PopupSpec spec = _readWaiting;
            _readWaiting = null;

            if (spec != null && _popup != null)
            {
                _popup.Withdraw(spec);
            }
        }

        /// <summary>
        /// 기다려도 저장 파일을 읽지 못했다. 손상이 아니므로 삭제를 권하지 않는다.
        /// 프로필을 다시 누르거나 타이틀을 다시 열면 다시 읽는다. 2026년 10월 9일 외부 검토가 읽기 실패와 손상을 나누라고 짚었다.
        /// </summary>
        public void NotifyReadFailed()
        {
            Show(PopupSpec.Notice(
                "저장 파일을 읽지 못함",
                "저장 파일을 지금 읽지 못했습니다.\n다른 프로그램이 파일을 쓰고 있는지 확인한 뒤 다시 시도해 주세요.",
                "확인"));
        }

        // ---- 타이틀 ----

        /// <summary>
        /// 저장된 런이 있는데 새 게임을 누르면 묻는다. savedRunSummary 는 지금 런의 요약이다.
        /// 새 게임 시작을 누르면 onConfirm 을 부른다.
        /// </summary>
        public void ConfirmNewGame(string savedRunSummary, Action onConfirm)
        {
            Show(
                PopupSpec.Confirm(
                    "새 게임을 시작하시겠습니까?",
                    "진행 중인 런이 사라집니다. 되돌릴 수 없습니다.\n\n" + (savedRunSummary ?? string.Empty),
                    "돌아가기",
                    "새 게임 시작",
                    true),
                onConfirm);
        }

        /// <summary>게임 종료를 누르면 묻는다. 종료를 누르면 onConfirm 을 부른다.</summary>
        public void ConfirmQuit(Action onConfirm)
        {
            Show(PopupSpec.Confirm("게임을 종료하시겠습니까?", string.Empty, "돌아가기", "종료", false), onConfirm);
        }

        /// <summary>저장된 런을 읽지 못해 이어할 수 없을 때.</summary>
        public void NotifyRunBroken()
        {
            Show(PopupSpec.Notice(
                "런 데이터 손상",
                "진행 중이던 런을 불러오지 못했습니다.\n새 게임으로 시작할 수 있습니다.",
                "확인"));
        }

        // ---- 런 ----

        /// <summary>
        /// 상단 표시줄의 런 종료 버튼. 취소, 런 포기, 저장하고 나가기 셋 중 무엇을 눌렀는지 onClosed 로 넘긴다.
        /// 버튼 식별자는 `EndRunCancelId`, `EndRunAbandonId`, `EndRunSaveId` 다.
        /// </summary>
        public void AskEndRun(Action<string> onClosed)
        {
            if (_popup == null)
            {
                return;
            }

            PopupSpec spec = new PopupSpec("런을 끝내시겠습니까?", "지금까지의 진행이 기록됩니다.");
            spec.Add(PopupButtonSpec.Normal(EndRunCancelId, "취소"));
            spec.Add(PopupButtonSpec.Danger(EndRunAbandonId, "런 포기"));
            spec.Add(PopupButtonSpec.Normal(EndRunSaveId, "저장하고 나가기"));

            _popup.Show(spec, onClosed);
        }

        /// <summary>
        /// 고르기 화면이 없을 때만 쓰는 문양 변경 확인. 시드 난수로 고른 문양을 보여 준다.
        /// 바꾸기를 누르면 onConfirm, 그 밖이면 onCancel 을 부른다.
        /// </summary>
        public void ConfirmSymbolChangeFallback(string symbolName, Action onConfirm, Action onCancel)
        {
            if (_popup == null)
            {
                if (onCancel != null)
                {
                    onCancel();
                }

                return;
            }

            _popup.Show(
                PopupSpec.Confirm(
                    "문양을 바꾸시겠습니까?",
                    symbolName + " 을 다른 문양으로 바꿉니다.\n무엇으로 바뀔지는 바꾸고 나서 알 수 있습니다.\n\n"
                    + "바꿀 문양을 고르는 화면은 아직 없습니다. 지금은 무작위로 하나 고릅니다.",
                    "돌아가기",
                    "바꾸기",
                    false),
                buttonId =>
                {
                    Action next = buttonId == ConfirmId ? onConfirm : onCancel;
                    if (next != null)
                    {
                        next();
                    }
                });
        }

        /// <summary>
        /// 문양이 바뀌었다고 알린다.
        /// **임시다.** 바뀐 문양은 팝업이 아니라 연출로 보여 줘야 한다. 연출이 정해질 때까지 팝업으로 둔다. 2026년 10월 6일 원재가 정했다.
        /// </summary>
        public void NotifySymbolChanged(string newSymbolName)
        {
            Show(PopupSpec.Notice("문양을 바꿨습니다", newSymbolName + " 을 받았습니다.", "확인"));
        }

        // ---- 설정과 저장 ----

        /// <summary>
        /// 07 설정 화면 의 "기본값 복원 확인 팝업". 되돌리기를 누르면 onConfirm 을 부른다.
        /// 해상도와 창 모드처럼 되돌리지 않는 항목은 본문 끝에 이름을 적는다(kept).
        /// 버튼 글은 08 화면 팝업 의 "기본값 복원 확인 · 취소 / 되돌리기" 다.
        /// </summary>
        public void ConfirmSettingsRestore(bool currentTabOnly, List<string> kept, Action onConfirm)
        {
            string body = currentTabOnly
                ? "지금 탭의 설정이 처음 값으로 돌아갑니다."
                : "모든 설정이 처음 값으로 돌아갑니다.";

            if (kept != null && kept.Count > 0)
            {
                body += "\n그대로 두는 항목: " + string.Join(", ", kept.ToArray());
            }

            Show(PopupSpec.Confirm("기본값으로 되돌리시겠습니까?", body, "취소", "되돌리기", true), onConfirm);
        }

        /// <summary>
        /// 저장 쪽이 알릴 일이 생겼다. 버튼은 확인 하나다.
        /// 깨진 런을 지웠을 때는 타이틀의 손상 알림과 같은 제목을 쓴다.
        /// 2026년 10월 7일 원재가 "깨진 런은 팝업을 띄우고 삭제한다" 고 정했다.
        /// </summary>
        public void NotifySave(SaveOutcome outcome, string text)
        {
            string title = outcome == SaveOutcome.RunDiscarded ? "런 데이터 손상" : "저장";
            Show(PopupSpec.Notice(title, text, "확인"));
        }

        // ---- 안쪽 ----

        private void Show(PopupSpec spec)
        {
            if (_popup != null)
            {
                _popup.Show(spec);
            }
        }

        /// <summary>확인 버튼을 누르면 onConfirm 을 부른다. 다른 버튼이나 그냥 닫히면 부르지 않는다.</summary>
        private void Show(PopupSpec spec, Action onConfirm)
        {
            if (_popup == null)
            {
                return;
            }

            _popup.Show(spec, buttonId =>
            {
                if (buttonId == ConfirmId && onConfirm != null)
                {
                    onConfirm();
                }
            });
        }
    }
}
