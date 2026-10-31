using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("ProfileCount")]
        [SerializeField] private int _profileCount = 3;
        public int ProfileCount { get { return _profileCount; } set { _profileCount = value; } }

        [Tooltip("프로필을 새로 만들 때 쓰는 이름. 뒤에 번호가 붙는다.")]
        [FormerlySerializedAs("DefaultProfileName")]
        [SerializeField] private string _defaultProfileName = "프로필";
        public string DefaultProfileName { get { return _defaultProfileName; } set { _defaultProfileName = value; } }

        [Tooltip(
            "게임을 처음 켰을 때 첫 프로필을 미리 만들어 둘지.\n" +
            "켜면 프로필 선택 화면에 첫 칸이 이미 이름을 달고 나와 바로 시작할 수 있다.")]
        [FormerlySerializedAs("CreateFirstProfileOnStart")]
        [SerializeField] private bool _createFirstProfileOnStart = true;
        public bool CreateFirstProfileOnStart { get { return _createFirstProfileOnStart; } set { _createFirstProfileOnStart = value; } }

        [Tooltip(
            "미리 만드는 첫 프로필의 이름.\n" +
            "**원래는 스팀이나 스토브 닉네임을 넣을 자리다.** 아직 연동이 없어 이 값을 쓴다.\n" +
            "연동이 붙으면 플랫폼 닉네임을 받아 이 자리를 채운다.")]
        [FormerlySerializedAs("FirstProfileName")]
        [SerializeField] private string _firstProfileName = "모험가";
        public string FirstProfileName { get { return _firstProfileName; } set { _firstProfileName = value; } }

        [Header("파일 이름")]
        [Tooltip("저장 파일이 들어가는 폴더 이름. Application.persistentDataPath 아래에 만든다.")]
        [FormerlySerializedAs("FolderName")]
        [SerializeField] private string _folderName = "SlotHero";
        public string FolderName { get { return _folderName; } set { _folderName = value; } }

        [Tooltip("런 데이터 파일 이름. {0} 에 프로필 번호가 들어간다.")]
        [FormerlySerializedAs("RunFileFormat")]
        [SerializeField] private string _runFileFormat = "profile{0}_run.json";
        public string RunFileFormat { get { return _runFileFormat; } set { _runFileFormat = value; } }

        [Tooltip("메타 데이터 파일 이름. {0} 에 프로필 번호가 들어간다.")]
        [FormerlySerializedAs("MetaFileFormat")]
        [SerializeField] private string _metaFileFormat = "profile{0}_meta.json";
        public string MetaFileFormat { get { return _metaFileFormat; } set { _metaFileFormat = value; } }

        [Tooltip("메타 백업 파일 이름. {0} 에 프로필 번호가 들어간다. 백업은 최신 하나만 둔다.")]
        [FormerlySerializedAs("MetaBackupFileFormat")]
        [SerializeField] private string _metaBackupFileFormat = "profile{0}_meta.backup.json";
        public string MetaBackupFileFormat { get { return _metaBackupFileFormat; } set { _metaBackupFileFormat = value; } }

        [Tooltip("설정 파일 이름. 기기당 하나라 번호가 없다.")]
        [FormerlySerializedAs("SettingsFileName")]
        [SerializeField] private string _settingsFileName = "settings.json";
        public string SettingsFileName { get { return _settingsFileName; } set { _settingsFileName = value; } }

        [Tooltip("쓰는 중인 파일에 붙이는 꼬리. 다 쓰면 본 이름으로 바꿔 단다.")]
        [FormerlySerializedAs("TempSuffix")]
        [SerializeField] private string _tempSuffix = ".tmp";
        public string TempSuffix { get { return _tempSuffix; } set { _tempSuffix = value; } }

        [Header("읽기 재시도")]
        [Tooltip(
            "저장 파일을 잠시 읽지 못할 때 다시 읽기까지 기다리는 초. 실제로 흐른 시간으로 센다.\n" +
            "백신 검사나 동기화 프로그램이 파일을 붙잡는 일은 오래 걸릴 수 있어 넉넉히 둔다. 2026년 10월 9일 원재.")]
        [FormerlySerializedAs("ReadRetryDelaySeconds")]
        [SerializeField] private float _readRetryDelaySeconds = 5f;
        public float ReadRetryDelaySeconds { get { return _readRetryDelaySeconds; } set { _readRetryDelaySeconds = value; } }

        [Tooltip(
            "다시 읽는 횟수. 다 써도 못 읽으면 백업과 런이 온전할 때 메타를 백업으로 되살리고,\n" +
            "아니면 읽지 못했다고 알린다.")]
        [FormerlySerializedAs("ReadRetryCount")]
        [SerializeField] private int _readRetryCount = 6;
        public int ReadRetryCount { get { return _readRetryCount; } set { _readRetryCount = value; } }

        [Header("기록")]
        [Tooltip("메타에 남기는 지난 런 기록 수. 넘치면 오래된 것부터 지운다.")]
        [FormerlySerializedAs("RunRecordLimit")]
        [SerializeField] private int _runRecordLimit = 20;
        public int RunRecordLimit { get { return _runRecordLimit; } set { _runRecordLimit = value; } }

        [Tooltip("런을 시작할 때 주는 최대 체력.")]
        [FormerlySerializedAs("StartingMaxHealth")]
        [SerializeField] private int _startingMaxHealth = 99;
        public int StartingMaxHealth { get { return _startingMaxHealth; } set { _startingMaxHealth = value; } }

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
        ///
        /// 모든 프로필의 런, 메타, 백업과 설정 파일을 한 목록에 모아 한꺼번에 견준다.
        /// 예전에는 백업과 설정 파일, 프로필 사이의 백업끼리를 보지 않아 백업 이름에서 번호를 빼도 통과했다
        /// (2026년 10월 22일 코덱스 검토).
        /// </summary>
        public bool FileNamesDoNotCollide()
        {
            List<string> names = new List<string>();
            names.Add(SettingsFileName);

            for (int i = 0; i < ProfileCount; i++)
            {
                names.Add(GetRunFileName(i));
                names.Add(GetMetaFileName(i));
                names.Add(GetMetaBackupFileName(i));
            }

            for (int i = 0; i < names.Count; i++)
            {
                for (int j = i + 1; j < names.Count; j++)
                {
                    if (string.Equals(names[i], names[j], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void OnValidate()
        {
            if (_profileCount < 1)
            {
                _profileCount = 1;
            }

            if (_runRecordLimit < 0)
            {
                _runRecordLimit = 0;
            }

            if (_startingMaxHealth < 1)
            {
                _startingMaxHealth = 1;
            }

            if (_readRetryDelaySeconds < 0f)
            {
                _readRetryDelaySeconds = 0f;
            }

            if (_readRetryCount < 0)
            {
                _readRetryCount = 0;
            }
        }
    }
}
