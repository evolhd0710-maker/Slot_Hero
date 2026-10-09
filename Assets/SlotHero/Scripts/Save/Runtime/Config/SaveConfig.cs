using UnityEngine;

namespace SlotHero.Save
{
    /// <summary>
    /// 저장에 쓰는 수치와 파일 이름.
    /// 저장 시스템 기획서 v0.1 / 03 저장 구조 의 개수 표와 06 의 백업 규칙에서 가져왔다.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveConfig", menuName = "Slot Hero/저장/저장 설정")]
    public class SaveConfig : ScriptableObject
    {
        [Header("프로필")]
        [Tooltip("프로필 개수. 03 저장 구조 표가 3개를 권장한다.")]
        public int ProfileCount = 3;

        [Tooltip("프로필을 새로 만들 때 쓰는 이름. 뒤에 번호가 붙는다.")]
        public string DefaultProfileName = "프로필";

        [Tooltip(
            "게임을 처음 켰을 때 첫 프로필을 미리 만들어 둘지.\n" +
            "켜면 프로필 선택 화면에 첫 칸이 이미 이름을 달고 나와 바로 시작할 수 있다.")]
        public bool CreateFirstProfileOnStart = true;

        [Tooltip(
            "미리 만드는 첫 프로필의 이름.\n" +
            "**원래는 스팀이나 스토브 닉네임을 넣을 자리다.** 아직 연동이 없어 이 값을 쓴다.\n" +
            "연동이 붙으면 플랫폼 닉네임을 받아 이 자리를 채운다.")]
        public string FirstProfileName = "모험가";

        [Header("파일 이름")]
        [Tooltip("저장 파일이 들어가는 폴더 이름. Application.persistentDataPath 아래에 만든다.")]
        public string FolderName = "SlotHero";

        [Tooltip("런 데이터 파일 이름. {0} 에 프로필 번호가 들어간다.")]
        public string RunFileFormat = "profile{0}_run.json";

        [Tooltip("메타 데이터 파일 이름. {0} 에 프로필 번호가 들어간다.")]
        public string MetaFileFormat = "profile{0}_meta.json";

        [Tooltip("메타 백업 파일 이름. {0} 에 프로필 번호가 들어간다. 백업은 최신 하나만 둔다.")]
        public string MetaBackupFileFormat = "profile{0}_meta.backup.json";

        [Tooltip("설정 파일 이름. 기기당 하나라 번호가 없다.")]
        public string SettingsFileName = "settings.json";

        [Tooltip("쓰는 중인 파일에 붙이는 꼬리. 다 쓰면 본 이름으로 바꿔 단다.")]
        public string TempSuffix = ".tmp";

        [Header("읽기 재시도")]
        [Tooltip(
            "저장 파일을 잠시 읽지 못할 때 다시 읽기까지 기다리는 초. 실제로 흐른 시간으로 센다.\n" +
            "백신 검사나 동기화 프로그램이 파일을 붙잡는 일은 오래 걸릴 수 있어 넉넉히 둔다. 2026년 10월 9일 원재.")]
        public float ReadRetryDelaySeconds = 5f;

        [Tooltip(
            "다시 읽는 횟수. 다 써도 못 읽으면 백업과 런이 온전할 때 메타를 백업으로 되살리고,\n" +
            "아니면 읽지 못했다고 알린다.")]
        public int ReadRetryCount = 6;

        [Header("기록")]
        [Tooltip("메타에 남기는 지난 런 기록 수. 넘치면 오래된 것부터 지운다.")]
        public int RunRecordLimit = 20;

        [Tooltip("런을 시작할 때 주는 최대 체력.")]
        public int StartingMaxHealth = 99;

        /// <summary>그 프로필의 런 데이터 파일 이름.</summary>
        public string GetRunFileName(int profileIndex)
        {
            return string.Format(RunFileFormat, profileIndex);
        }

        /// <summary>그 프로필의 메타 데이터 파일 이름.</summary>
        public string GetMetaFileName(int profileIndex)
        {
            return string.Format(MetaFileFormat, profileIndex);
        }

        /// <summary>그 프로필의 메타 백업 파일 이름.</summary>
        public string GetMetaBackupFileName(int profileIndex)
        {
            return string.Format(MetaBackupFileFormat, profileIndex);
        }

        /// <summary>프로필 번호가 있는 범위인지.</summary>
        public bool IsValidProfileIndex(int profileIndex)
        {
            return profileIndex >= 0 && profileIndex < ProfileCount;
        }

        /// <summary>
        /// 파일 이름들이 서로 겹치지 않는지.
        /// 런과 메타와 백업이 같은 이름이면 서로 덮어써 버린다.
        /// </summary>
        public bool FileNamesDoNotCollide()
        {
            for (int i = 0; i < ProfileCount; i++)
            {
                string run = GetRunFileName(i);
                string meta = GetMetaFileName(i);
                string backup = GetMetaBackupFileName(i);

                if (run == meta || run == backup || meta == backup)
                {
                    return false;
                }

                if (run == SettingsFileName || meta == SettingsFileName)
                {
                    return false;
                }

                // 다른 프로필과도 겹치면 안 된다.
                for (int j = i + 1; j < ProfileCount; j++)
                {
                    if (run == GetRunFileName(j) || meta == GetMetaFileName(j))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void OnValidate()
        {
            if (ProfileCount < 1)
            {
                ProfileCount = 1;
            }

            if (RunRecordLimit < 0)
            {
                RunRecordLimit = 0;
            }

            if (StartingMaxHealth < 1)
            {
                StartingMaxHealth = 1;
            }

            if (ReadRetryDelaySeconds < 0f)
            {
                ReadRetryDelaySeconds = 0f;
            }

            if (ReadRetryCount < 0)
            {
                ReadRetryCount = 0;
            }
        }
    }
}
