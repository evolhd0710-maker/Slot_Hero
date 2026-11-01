using System;
using SlotHero.Settings;

namespace SlotHero.Save
{
    /// <summary>
    /// 기기에 저장하는 설정 값.
    /// 저장 시스템 기획서 v0.1 / 07 로컬 환경 설정 이다.
    ///
    /// 프로필과 따로 둔다. 기기당 하나다.
    /// 09 클라우드 저장 표가 설정 데이터는 올리지 않는다고 정했다. 기기마다 환경이 다르기 때문이다.
    ///
    /// 항목 자체는 설정 화면이 가진 `SettingsValues` 를 그대로 싣는다.
    /// 사운드, 그래픽, 언어, 조작, 기타, 접근성 여섯 갈래가 거기 들어 있다.
    /// </summary>
    [Serializable]
    public class SettingsSaveData
    {
        /// <summary>설정 항목들.</summary>
        public SettingsValues Values = new SettingsValues();

        /// <summary>
        /// 저장 형식의 판 번호.
        /// 자료 구조가 바뀌면 이 값으로 예전 파일을 가려내 올린다.
        /// </summary>
        public int FormatVersion = SaveFormat.Current;

        /// <summary>올바르게 저장되었는지 검사하는 값.</summary>
        public string Checksum = string.Empty;

        /// <summary>빈 설정을 만든다. 값이 하나도 없으면 각 항목의 기본값이 쓰인다.</summary>
        public static SettingsSaveData CreateDefault()
        {
            return new SettingsSaveData();
        }

        /// <summary>
        /// 값이 앞뒤가 맞는지.
        /// 설정은 10 예외 처리 표에서 오류가 나면 통째로 기본값으로 되돌리므로
        /// 담긴 항목이 있는지만 본다.
        /// </summary>
        public bool IsConsistent()
        {
            return Values != null && Values.Entries != null;
        }
    }
}
