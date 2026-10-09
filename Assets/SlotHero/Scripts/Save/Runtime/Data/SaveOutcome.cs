namespace SlotHero.Save
{
    /// <summary>
    /// 읽거나 쓴 결과.
    /// 저장 시스템 기획서 v0.1 / 10 예외 처리 의 표를 그대로 옮긴 것이다.
    ///
    /// 팝업을 띄우는 줄은 여섯이다. `NeedsNotice` 가 그것을 가린다. 읽기 실패(`ReadFailed`)는 2026년 10월 9일에 더했다.
    /// 클라우드 두 줄은 클라우드 저장을 만들 때 더한다.
    /// </summary>
    public enum SaveOutcome
    {
        /// <summary>아무 일 없었다.</summary>
        Ok = 0,

        /// <summary>파일이 없어 새로 만들었다. 프로필이 없거나 저장 데이터가 없을 때다.</summary>
        CreatedNew = 1,

        /// <summary>
        /// 메타 데이터가 검사를 통과하지 못해 백업으로 읽었다.
        /// 팝업을 띄운다.
        /// </summary>
        MetaRestoredFromBackup = 2,

        /// <summary>
        /// 메타 데이터와 백업이 모두 검사를 통과하지 못해 프로필을 초기화했다.
        /// 팝업을 띄운다.
        /// </summary>
        MetaResetProfile = 3,

        /// <summary>
        /// 런 데이터가 검사를 통과하지 못해 지웠다.
        /// 그 런은 진행하지 않은 것으로 친다.
        /// 팝업을 띄운다. 플레이어가 고를 것 없이 지우므로 버튼은 확인 하나다(2026년 10월 7일 원재).
        /// </summary>
        RunDiscarded = 4,

        /// <summary>
        /// 메타가 말하는 런 존재 여부와 실제 런 파일이 어긋났다.
        /// 한쪽에 맞춰 고쳤다. 팝업을 띄운다.
        /// </summary>
        RunFlagRepaired = 5,

        /// <summary>설정 데이터에 문제가 있어 전부 기본값으로 되돌렸다.</summary>
        SettingsReset = 6,

        /// <summary>
        /// 쓰기에 실패했다. 저장 공간이나 권한이 모자랄 때다.
        /// 기존 저장 데이터를 그대로 쓴다. 팝업을 띄운다.
        /// </summary>
        WriteFailed = 7,

        /// <summary>
        /// 저장하던 중에 꺼져 임시 파일이 남아 있었다. 지우고 기존 저장 데이터를 썼다.
        /// 사용자에게 알릴 일이 아니라 팝업은 띄우지 않는다.
        /// </summary>
        TempFileDiscarded = 8,

        // 9 와 10 은 비워 둔다. 판 번호로 갈리던 "올려 읽음" 과 "더 새 판이라 못 읽음" 자리였다.
        // 2026년 10월 8일에 판 번호가 읽기에 영향을 주지 않게 바꾸면서 뺐다. `SaveFormat` 참고.

        /// <summary>
        /// 파일은 있는데 읽지 못했다. 다른 프로그램이 잠시 붙잡은 경우다. **손상과 다르다.**
        /// 백업으로 되돌리지도, 초기화하지도, 지우지도 않고 그대로 둔다. 다음에 다시 읽는다. 팝업을 띄운다.
        /// 예전에는 손상으로 보아 멀쩡한 프로필을 새 프로필로 덮고, 런 있음 표시를 내렸다.
        /// 2026년 10월 9일 외부 검토가 실제 파일로 재현했다.
        /// </summary>
        ReadFailed = 11,
    }

    /// <summary>결과를 다루는 데 쓰는 값.</summary>
    public static class SaveOutcomes
    {
        /// <summary>팝업을 띄워야 하는 결과인지.</summary>
        public static bool NeedsNotice(SaveOutcome outcome)
        {
            return outcome == SaveOutcome.MetaRestoredFromBackup
                || outcome == SaveOutcome.MetaResetProfile
                || outcome == SaveOutcome.RunDiscarded
                || outcome == SaveOutcome.RunFlagRepaired
                || outcome == SaveOutcome.WriteFailed
                || outcome == SaveOutcome.ReadFailed;
        }

        /// <summary>일이 잘 풀린 결과인지. 새로 만든 것도 잘 풀린 것으로 친다.</summary>
        public static bool IsSuccess(SaveOutcome outcome)
        {
            return outcome != SaveOutcome.WriteFailed && outcome != SaveOutcome.ReadFailed;
        }

        /// <summary>팝업에 띄울 글. 팝업이 필요 없는 결과면 빈 글을 돌려준다.</summary>
        public static string GetNoticeText(SaveOutcome outcome)
        {
            switch (outcome)
            {
                case SaveOutcome.MetaRestoredFromBackup:
                    return "저장 데이터가 손상되어 백업으로 되돌렸습니다.";

                case SaveOutcome.MetaResetProfile:
                    return "저장 데이터와 백업이 모두 손상되어 프로필을 초기화했습니다.";

                case SaveOutcome.RunDiscarded:
                    return "진행 중이던 런 데이터가 손상되어 삭제했습니다.\n새 게임으로 시작할 수 있습니다.";

                case SaveOutcome.RunFlagRepaired:
                    return "진행 중이던 런 데이터를 찾을 수 없어 기록을 정리했습니다.";

                case SaveOutcome.WriteFailed:
                    return "저장에 실패했습니다. 저장 공간과 권한을 확인해 주세요.";

                case SaveOutcome.ReadFailed:
                    return "저장 파일을 읽지 못했습니다.\n다른 프로그램이 파일을 쓰고 있는지 확인한 뒤 다시 시도해 주세요.";

                default:
                    return string.Empty;
            }
        }
    }
}
