using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Save
{
    /// <summary>
    /// 저장 데이터를 읽고 쓰는 곳.
    /// 저장 시스템 기획서 v0.1 / 04 저장 원칙 과 10 예외 처리 를 담는다.
    ///
    /// 읽을 때마다 체크섬을 다시 매겨 비교한다.
    /// 어긋나면 10 예외 처리 표가 정한 대로 백업을 쓰거나 지우거나 기본값으로 되돌린다.
    /// 어떻게 됐는지는 `SaveOutcome` 으로 알려 주고, 팝업이 필요한 줄은 그 안에서 가린다.
    ///
    /// 화면을 모른다. 파일과 자료만 다룬다.
    /// </summary>
    public class SaveRepository
    {
        private readonly SaveConfig _config;
        private readonly ISaveStorage _storage;
        private readonly List<string> _nameBuffer = new List<string>();

        /// <summary>설정과 창구를 물려 만든다.</summary>
        public SaveRepository(SaveConfig config, ISaveStorage storage)
        {
            _config = config;
            _storage = storage;
        }

        /// <summary>
        /// 쓰는 중이던 임시 파일이 남아 있으면 치운다. 게임을 켤 때 한 번 부른다.
        ///
        /// 10 예외 처리 의 "저장하던 중 종료 · 임시 파일 삭제 후 기존 저장 데이터 사용" 대로
        /// 기존 파일이 온전하면 임시 파일을 지우고 기존 것을 쓴다.
        ///
        /// **기존 파일이 없거나 망가졌고 임시 파일이 온전하면 임시 파일을 되살린다.**
        /// 바꿔 다는 도중에 꺼지면 그렇게 남을 수 있다. 그때 임시 파일까지 지우면 진행을 통째로 잃는다.
        /// 온전한지는 체크섬으로 본다. 2026년 10월 8일에 더했다.
        /// </summary>
        public SaveOutcome CleanTempFiles()
        {
            if (_storage == null || _config == null)
            {
                return SaveOutcome.Ok;
            }

            string suffix = _config.TempSuffix;
            if (string.IsNullOrEmpty(suffix))
            {
                return SaveOutcome.Ok;
            }

            _storage.GetFileNames(_nameBuffer);

            // 지우거나 되살리면 목록이 바뀌므로 먼저 옮겨 담는다.
            List<string> temps = new List<string>();
            for (int i = 0; i < _nameBuffer.Count; i++)
            {
                if (_nameBuffer[i].EndsWith(suffix, StringComparison.Ordinal))
                {
                    temps.Add(_nameBuffer[i]);
                }
            }

            for (int i = 0; i < temps.Count; i++)
            {
                string mainName = temps[i].Substring(0, temps[i].Length - suffix.Length);
                string tempText = _storage.Read(temps[i]);
                string mainText = _storage.Read(mainName);

                // **읽지 못한 것과 망가진 것은 다르다.** 파일이 있는데 글이 오지 않았으면 잠시 막힌 것으로 보고 손대지 않는다.
                // 예전에는 읽지 못한 임시 파일을 망가진 것으로 보고 지워, 본 파일이 없을 때 유일한 진행이 사라졌다.
                // 본 파일을 읽지 못했을 때도 그 위에 임시 파일을 덮지 않는다. 본 파일이 멀쩡할 수 있다.
                // 2026년 10월 9일 외부 검토가 실제 파일로 재현했다. 다음에 켤 때 다시 본다.
                if (IsUnreadable(temps[i], tempText) || IsUnreadable(mainName, mainText))
                {
                    continue;
                }

                bool mainIntact = mainText != null && SaveChecksum.VerifyJson(mainText);
                bool tempIntact = tempText != null && SaveChecksum.VerifyJson(tempText);

                if (!mainIntact && tempIntact && mainName.Length > 0)
                {
                    // 되살린다. 글을 다시 쓰지 않고 임시 파일의 이름만 바꿔 단다.
                    // 예전에는 같은 글을 `Write` 로 다시 써서, 그 임시 이름이 지금 되살리려는 파일과 같았다.
                    // 쓰기가 실패하면 반쯤 쓴 임시 파일로 보고 지워 본 파일도 임시 파일도 없는 상태가 됐다.
                    // 2026년 10월 9일 외부 검토가 실제 파일로 재현해 고쳤다. 실패하면 임시 파일을 그대로 두고 다음에 다시 해 본다.
                    _storage.Promote(temps[i], mainName);
                    continue;
                }

                _storage.Delete(temps[i]);
            }

            return temps.Count > 0 ? SaveOutcome.TempFileDiscarded : SaveOutcome.Ok;
        }

        // ---- 설정 데이터 · 07 로컬 환경 설정 ----

        /// <summary>
        /// 설정을 읽는다.
        /// 10 예외 처리 의 "설정 데이터 오류 · 모든 설정 값을 기본값으로 초기화"를 따른다.
        /// </summary>
        public SettingsSaveData LoadSettings(out SaveOutcome outcome)
        {
            if (_storage == null)
            {
                outcome = SaveOutcome.CreatedNew;
                return SettingsSaveData.CreateDefault();
            }

            // 읽지 못했으면 기본값으로 쓰되 손상으로 보지 않는다. 파일은 그대로 둔다.
            if (CannotRead(_config.SettingsFileName))
            {
                outcome = SaveOutcome.ReadFailed;
                return SettingsSaveData.CreateDefault();
            }

            string json = ReadSaved(_config.SettingsFileName);

            if (json == null)
            {
                outcome = SaveOutcome.CreatedNew;
                return SettingsSaveData.CreateDefault();
            }

            SettingsSaveData data = Parse<SettingsSaveData>(json);

            // 판 번호는 보지 않는다. `SaveFormat` 참고.
            if (data == null || !data.IsConsistent() || !SaveChecksum.VerifyJson(json))
            {
                outcome = SaveOutcome.SettingsReset;
                return SettingsSaveData.CreateDefault();
            }

            outcome = SaveOutcome.Ok;
            return data;
        }

        /// <summary>설정을 쓴다.</summary>
        public SaveOutcome SaveSettings(SettingsSaveData data)
        {
            if (data == null)
            {
                return SaveOutcome.WriteFailed;
            }

            StampSettings(data);
            return WriteJson(_config.SettingsFileName, JsonUtility.ToJson(data));
        }

        // ---- 메타 데이터 · 06 메타 데이터 ----

        /// <summary>
        /// 메타를 읽는다.
        /// 본 파일이 검사를 통과하지 못하면 백업을 쓰고, 백업도 안 되면 프로필을 초기화한다.
        /// </summary>
        public MetaSaveData LoadMeta(int profileIndex, out SaveOutcome outcome)
        {
            string mainName = _config.GetMetaFileName(profileIndex);
            string backupName = _config.GetMetaBackupFileName(profileIndex);

            // 판 번호는 보지 않는다. `SaveFormat` 참고.
            MetaSaveData main = ReadMeta(mainName);
            if (main != null)
            {
                outcome = SaveOutcome.Ok;
                return main;
            }

            // 본 파일이나 백업을 읽지 못했으면 손상으로 보지 않는다. 백업으로 되돌리지도, 새로 만들지도 않고 null 을 돌려준다.
            // 본 파일을 잠시 못 읽는데 백업을 쓰면 더 오래된 값으로 본 파일을 덮는다. 둘 다 못 읽는데 새로 만들면 프로필을 잃는다.
            // 예전에는 잠시 못 읽은 프로필이 "프로필1 · 0초" 로 덮였고 잠금을 풀어도 돌아오지 않았다. 2026년 10월 9일 외부 검토.
            if (CannotRead(mainName) || CannotRead(backupName))
            {
                outcome = SaveOutcome.ReadFailed;
                return null;
            }

            // 런을 읽지 못해도 되살리거나 새로 만들지 않는다. 런을 이어 붙일 수 없는데 확정하면
            // "런 없음" 이 적힌 메타가 본 자리에 남고, 런이 다시 읽힐 때 `ReconcileRunFlag` 가 멀쩡한 런을 지운다.
            // 망가진 본 파일은 그대로 두고 런이 읽힐 때 다시 본다. 2026년 10월 9일 외부 검토.
            if (CannotRead(_config.GetRunFileName(profileIndex)))
            {
                outcome = SaveOutcome.ReadFailed;
                return null;
            }

            MetaSaveData backup = ReadMeta(backupName);
            if (backup != null)
            {
                // 백업은 한 번 앞의 메타라 지금 런을 모를 수 있다. 온전한 런이 있으면 먼저 이어 붙인다.
                // 그러지 않으면 "런 없음" 이 적힌 백업이 되살아나 `ReconcileRunFlag` 가 멀쩡한 런을 지운다.
                LinkIntactRun(backup, profileIndex);

                // 백업을 본 자리로 되돌려 둔다. 다음에 읽을 때 또 백업을 뒤지지 않게 하려는 것이다.
                backup.ProfileIndex = profileIndex;
                StampMeta(backup);
                WriteJson(mainName, JsonUtility.ToJson(backup));

                outcome = SaveOutcome.MetaRestoredFromBackup;
                return backup;
            }

            // 본 파일도 백업도 없었을 뿐이면 그냥 새로 만든 것이다.
            bool hadAnything = _storage.Exists(mainName) || _storage.Exists(backupName);

            MetaSaveData fresh = MetaSaveData.CreateEmpty(
                profileIndex, GetDefaultProfileName(profileIndex));
            StampMeta(fresh);
            WriteJson(mainName, JsonUtility.ToJson(fresh));

            // 새 프로필을 만들 때는 백업도 함께 만든다. 10 예외 처리 의 "파일 없음" 줄이다.
            WriteJson(backupName, JsonUtility.ToJson(fresh));

            outcome = hadAnything ? SaveOutcome.MetaResetProfile : SaveOutcome.CreatedNew;
            return fresh;
        }

        /// <summary>
        /// 본 메타를 기다려도 읽지 못할 때 백업으로 되살린다. **백업이 온전하고 런이 온전할 때만 한다.**
        /// 되살린 메타에 그 런을 이어 붙이고(`LinkIntactRun`) 본 자리에 써 본다. 본 파일이 아직 잠겨 쓰지 못해도
        /// 되살린 메타를 돌려준다. 다음 메타 저장이 다시 쓴다.
        ///
        /// 런이 없으면 되살리지 않는다. 백업은 한 번 앞의 값이라 마지막 런 기록이 빠져 있을 수 있는데,
        /// 이어 갈 런도 없이 그것으로 본 파일을 덮으면 기록을 잃는다. 그때는 null 과 `ReadFailed` 다.
        /// 본 메타를 이제 읽을 수 있으면 `LoadMeta` 와 같다.
        /// 2026년 10월 9일 원재가 "백업이 있고 런이 정상이면 메타를 복원하고 런과 연결해 검증한다" 고 정했다.
        /// </summary>
        public MetaSaveData RestoreUnreadableMeta(int profileIndex, out SaveOutcome outcome)
        {
            string mainName = _config.GetMetaFileName(profileIndex);
            string backupName = _config.GetMetaBackupFileName(profileIndex);

            if (!CannotRead(mainName))
            {
                return LoadMeta(profileIndex, out outcome);
            }

            outcome = SaveOutcome.ReadFailed;

            if (CannotRead(backupName))
            {
                return null;
            }

            MetaSaveData backup = ReadMeta(backupName);
            if (backup == null || !LinkIntactRun(backup, profileIndex))
            {
                return null;
            }

            backup.ProfileIndex = profileIndex;
            StampMeta(backup);
            WriteJson(mainName, JsonUtility.ToJson(backup));

            outcome = SaveOutcome.MetaRestoredFromBackup;
            return backup;
        }

        /// <summary>
        /// 되살린 메타에 그 프로필의 온전한 런을 이어 붙인다. 붙였으면 true 다.
        /// "런 있음" 을 켜고 런 횟수를 그 런 번호까지 올린다.
        ///
        /// **이미 끝난 런이면 붙이지 않는다.** 메타의 지난 런 기록에 그 번호가 있으면 기록을 쓰고 런 파일을 지우기 전에
        /// 꺼져 남은 것이다. 붙이면 그 런을 다시 하고 기록이 두 번 쌓인다. 그 파일은 `ReconcileRunFlag` 가 지운다.
        /// 런을 읽지 못하거나 망가졌으면 붙이지 않는다.
        /// </summary>
        private bool LinkIntactRun(MetaSaveData meta, int profileIndex)
        {
            RunSaveData run = ReadIntactRun(profileIndex);
            if (meta == null || run == null)
            {
                return false;
            }

            for (int i = 0; i < meta.RunRecords.Count; i++)
            {
                if (meta.RunRecords[i].RunNumber == run.RunNumber)
                {
                    return false;
                }
            }

            meta.HasSavedRun = true;
            if (meta.RunCount < run.RunNumber)
            {
                meta.RunCount = run.RunNumber;
            }

            return true;
        }

        /// <summary>그 프로필의 런이 읽히고 검사를 통과하면 그 런. 아니면 null 이다. 망가져도 지우지 않는다.</summary>
        private RunSaveData ReadIntactRun(int profileIndex)
        {
            string fileName = _config.GetRunFileName(profileIndex);
            if (CannotRead(fileName))
            {
                return null;
            }

            string json = ReadSaved(fileName);
            if (json == null || !SaveChecksum.VerifyJson(json))
            {
                return null;
            }

            RunSaveData data = Parse<RunSaveData>(json);
            return data != null && data.IsConsistent() ? data : null;
        }

        /// <summary>
        /// 메타를 쓴다.
        /// 06 이 "갱신 시 직전 값을 삭제하지 않고 백업 데이터로 사용한다"로 정했으므로
        /// 덮어쓰기 전에 지금 파일을 백업 자리로 옮긴다. 백업은 최신 하나만 둔다.
        /// </summary>
        public SaveOutcome SaveMeta(MetaSaveData data)
        {
            if (data == null || !_config.IsValidProfileIndex(data.ProfileIndex))
            {
                return SaveOutcome.WriteFailed;
            }

            string mainName = _config.GetMetaFileName(data.ProfileIndex);
            string backupName = _config.GetMetaBackupFileName(data.ProfileIndex);

            // 온전한 지금 파일만 백업으로 옮긴다. 망가졌으면 기존 백업을 그대로 둔다.
            // 예전에는 읽히기만 하면 옮겨, 실행 중 본 파일이 망가진 채로 다시 저장하면 멀쩡한 백업을 망가진 글로 덮었다.
            // 2026년 10월 9일 외부 검토가 짚었다.
            string current = ReadSaved(mainName);
            if (current != null && ParseMeta(current) != null)
            {
                WriteJson(backupName, current);
            }

            StampMeta(data);
            return WriteJson(mainName, JsonUtility.ToJson(data));
        }

        // ---- 런 데이터 · 05 런 데이터 ----

        /// <summary>
        /// 런을 읽는다. 없으면 null 이다.
        /// 검사를 통과하지 못하면 지우고 `RunDiscarded` 를 알린다.
        /// 그 런은 진행하지 않은 것으로 친다.
        /// </summary>
        public RunSaveData LoadRun(int profileIndex, out SaveOutcome outcome)
        {
            string fileName = _config.GetRunFileName(profileIndex);

            // 읽지 못했으면 손상으로 보아 지우지 않는다. 런은 그대로 두고 다음에 다시 읽는다.
            if (CannotRead(fileName))
            {
                outcome = SaveOutcome.ReadFailed;
                return null;
            }

            string json = ReadSaved(fileName);

            if (json == null)
            {
                outcome = SaveOutcome.Ok;
                return null;
            }

            RunSaveData data = Parse<RunSaveData>(json);

            // 판 번호는 보지 않는다. 체크섬은 파일 글 그대로 확인한다.
            // 그래서 업데이트로 칸이 늘거나 판 번호가 달라도 멀쩡한 런은 그대로 읽힌다. `SaveFormat` 참고.
            if (data == null || !data.IsConsistent() || !SaveChecksum.VerifyJson(json))
            {
                _storage.Delete(fileName);
                outcome = SaveOutcome.RunDiscarded;
                return null;
            }

            outcome = SaveOutcome.Ok;
            return data;
        }

        /// <summary>런을 쓴다.</summary>
        public SaveOutcome SaveRun(int profileIndex, RunSaveData data)
        {
            if (data == null || !_config.IsValidProfileIndex(profileIndex))
            {
                return SaveOutcome.WriteFailed;
            }

            StampRun(data);
            return WriteJson(_config.GetRunFileName(profileIndex), JsonUtility.ToJson(data));
        }

        /// <summary>런을 지운다. 런 종료 시점에 부른다.</summary>
        public void DeleteRun(int profileIndex)
        {
            // 남은 임시 파일도 지운다. 남기면 본 파일 대신 읽혀 끝난 런이 되살아난다(`ReadSaved`).
            DeleteWithTemp(_config.GetRunFileName(profileIndex));
        }

        /// <summary>
        /// 그 프로필에 런 파일이 있는지. 되살리지 못하고 남은 온전한 임시 파일도 런 파일로 본다.
        /// 읽지 못하는 임시 파일도 있는 것으로 본다. 없다고 하면 메타의 "런 있음" 이 내려가고,
        /// 다음에 그 임시 파일을 되살려도 메타와 어긋났다며 지운다. 2026년 10월 9일 외부 검토.
        /// </summary>
        public bool HasRunFile(int profileIndex)
        {
            string fileName = _config.GetRunFileName(profileIndex);
            return _storage.Exists(fileName) || ReadKeptTemp(fileName) != null || CannotRead(fileName);
        }

        /// <summary>그 프로필의 런을 지금 읽을 수 없는지. 손상과 다르다. 다음에 다시 읽는다.</summary>
        public bool CannotReadRun(int profileIndex)
        {
            return CannotRead(_config.GetRunFileName(profileIndex));
        }

        /// <summary>
        /// 그 프로필의 메타를 지금 읽을 수 없는지. `LoadMeta` 가 읽기 실패를 돌려줄 때와 같다.
        /// 본 메타를 못 읽거나, 본 메타가 온전하지 않아 백업으로 되살려야 하는데 백업이나 런을 못 읽을 때다.
        /// **본 메타가 온전하면 백업이 잠겨 있어도 읽을 수 있는 것으로 본다.** 백업은 쓰지 않기 때문이다.
        /// 예전에는 백업만 잠겨도 못 읽는 것으로 보아, 게임을 켤 때와 타이틀에서 30초를 기다리고 실패 팝업까지 띄웠다.
        /// 메타를 저장할 때마다 본 파일과 백업이 함께 바뀌어 동기화 프로그램이 백업만 붙잡는 일이 생긴다. 2026년 10월 9일 전체 검토.
        /// </summary>
        public bool CannotReadMeta(int profileIndex)
        {
            string mainName = _config.GetMetaFileName(profileIndex);
            if (CannotRead(mainName))
            {
                return true;
            }

            if (ReadMeta(mainName) != null)
            {
                return false;
            }

            return CannotRead(_config.GetMetaBackupFileName(profileIndex)) || CannotRead(_config.GetRunFileName(profileIndex));
        }

        /// <summary>
        /// 메타가 말하는 런 존재 여부와 실제 런 파일을 맞춘다.
        /// 10 예외 처리 의 "메타와 런 데이터가 어긋남" 줄이다.
        ///
        /// 런 파일이 없는데 있다고 적혀 있으면 적힌 쪽을 고치고,
        /// 런 파일이 있는데 없다고 적혀 있으면 그 파일을 지운다.
        /// 어느 쪽이든 팝업을 띄운다.
        /// </summary>
        public SaveOutcome ReconcileRunFlag(MetaSaveData meta)
        {
            if (meta == null)
            {
                return SaveOutcome.Ok;
            }

            // 런을 읽지 못하는 동안에는 맞추지 않는다. 있는지 없는지 모르는 채로 메타를 고치거나 파일을 지우면 런을 잃는다.
            if (CannotRead(_config.GetRunFileName(meta.ProfileIndex)))
            {
                return SaveOutcome.Ok;
            }

            // 본 메타를 읽지 못하는 동안에도 맞추지 않는다. 손에 든 메타가 본 파일과 같은지 알 수 없다.
            // 백업으로 되살린 메타는 런을 이미 이어 붙였다(`RestoreUnreadableMeta`). 2026년 10월 9일 외부 검토.
            if (CannotRead(_config.GetMetaFileName(meta.ProfileIndex)))
            {
                return SaveOutcome.Ok;
            }

            bool hasFile = HasRunFile(meta.ProfileIndex);

            if (meta.HasSavedRun == hasFile)
            {
                return SaveOutcome.Ok;
            }

            if (hasFile)
            {
                // 메타가 모르는 온전한 런이면 지우지 않고 메타에 이어 붙인다. 런 번호가 메타의 런 횟수보다 크고
                // 지난 런 기록에도 없으면, 그 런을 시작한 뒤 메타 쓰기가 실패해 메타만 뒤처진 것이다.
                // 저장 실패 중에 프로필을 바꾸면 그렇게 남는다. 2026년 10월 9일 외부 검토.
                // 끝난 런(기록에 있다)이나 버린 런(번호를 이미 셌다)이면 예전처럼 지운다.
                RunSaveData run = ReadIntactRun(meta.ProfileIndex);
                if (run != null && run.RunNumber > meta.RunCount && LinkIntactRun(meta, meta.ProfileIndex))
                {
                    SaveMeta(meta);
                    return SaveOutcome.Ok;
                }

                DeleteRun(meta.ProfileIndex);
            }
            else
            {
                meta.HasSavedRun = false;
                SaveMeta(meta);
            }

            return SaveOutcome.RunFlagRepaired;
        }

        /// <summary>프로필 하나를 통째로 지운다. 프로필 삭제에 쓴다.</summary>
        public void DeleteProfile(int profileIndex)
        {
            DeleteWithTemp(_config.GetRunFileName(profileIndex));
            DeleteWithTemp(_config.GetMetaFileName(profileIndex));
            DeleteWithTemp(_config.GetMetaBackupFileName(profileIndex));
        }

        /// <summary>
        /// 그 프로필에 저장 데이터가 하나라도 있는지. **백업만 남아 있어도 있는 것으로 본다.**
        ///
        /// 예전에는 본 파일만 보아, 본 파일이 사라지고 온전한 백업만 남은 프로필이 빈 칸으로 보였다.
        /// 그러면 `LoadMeta` 의 백업 복구까지 가지 못하고, 첫 프로필을 만드는 단계가 그 자리를 새 이름으로 덮을 수도 있었다.
        /// 2026년 10월 8일에 고쳤다. 백업이 온전한지는 `LoadMeta` 가 읽으면서 가린다.
        /// </summary>
        public bool ProfileExists(int profileIndex)
        {
            string mainName = _config.GetMetaFileName(profileIndex);
            string backupName = _config.GetMetaBackupFileName(profileIndex);

            // 읽지 못하는 임시 파일만 남은 프로필도 있는 것으로 본다. 빈 자리로 보이면 읽기 실패 안내와
            // 다시 읽기로 이어지지 않고, 그 위에 새 프로필을 만들 수 있다. 2026년 10월 9일 외부 검토.
            return _storage.Exists(mainName)
                || _storage.Exists(backupName)
                || ReadKeptTemp(mainName) != null
                || ReadKeptTemp(backupName) != null
                || CannotRead(mainName)
                || CannotRead(backupName);
        }

        /// <summary>새 프로필의 기본 이름. 뒤에 번호가 붙는다.</summary>
        public string GetDefaultProfileName(int profileIndex)
        {
            return _config.DefaultProfileName + (profileIndex + 1);
        }

        // ---- 체크섬 ----

        /// <summary>런에 체크섬을 매긴다.</summary>
        public static void StampRun(RunSaveData data)
        {
            data.FormatVersion = SaveFormat.Current;
            data.Checksum = string.Empty;
            data.Checksum = SaveChecksum.Compute(JsonUtility.ToJson(data));
        }

        /// <summary>메타에 체크섬을 매긴다.</summary>
        public static void StampMeta(MetaSaveData data)
        {
            data.FormatVersion = SaveFormat.Current;
            data.Checksum = string.Empty;
            data.Checksum = SaveChecksum.Compute(JsonUtility.ToJson(data));
        }

        /// <summary>설정에 체크섬을 매긴다.</summary>
        public static void StampSettings(SettingsSaveData data)
        {
            data.FormatVersion = SaveFormat.Current;
            data.Checksum = string.Empty;
            data.Checksum = SaveChecksum.Compute(JsonUtility.ToJson(data));
        }

        // ---- 안쪽 ----

        /// <summary>
        /// 그 이름의 저장 글. 본 파일이 온전하지 않으면 되살리지 못하고 남은 온전한 임시 파일을 대신 읽는다.
        ///
        /// `CleanTempFiles` 가 임시 파일을 되살리다 실패하면 본 파일 없이 임시 파일만 남는다.
        /// 그때 본 파일만 보면 런이 없는 것으로 보여 메타의 "런 있음" 을 내리고,
        /// 다음 실행에서 되살리기가 성공해도 메타와 어긋난 런 파일로 보고 지운다. 2026년 10월 9일에 막았다.
        /// 본 파일이 온전하면 늘 본 파일을 쓴다. 10 예외 처리 의 "기존 저장 데이터 사용" 이다.
        /// </summary>
        private string ReadSaved(string fileName)
        {
            string main = _storage.Read(fileName);
            if (main != null && SaveChecksum.VerifyJson(main))
            {
                return main;
            }

            string kept = ReadKeptTemp(fileName);
            return kept ?? main;
        }

        /// <summary>본 파일이 온전하지 않을 때 남은 온전한 임시 파일의 글. 없으면 null 이다.</summary>
        private string ReadKeptTemp(string fileName)
        {
            string suffix = _config.TempSuffix;
            if (string.IsNullOrEmpty(suffix) || !_storage.Exists(fileName + suffix))
            {
                return null;
            }

            string main = _storage.Read(fileName);
            if (main != null && SaveChecksum.VerifyJson(main))
            {
                return null;
            }

            // **본 파일이 있는데 읽지 못하면 임시 파일로 대신하지 않는다.** 본 파일이 멀쩡한지 알 수 없다.
            // 본 파일이 잠긴 채 쓰기가 실패하면 다 쓴 임시 파일이 남는데, 그것은 실패한 쓰기라 본 파일이 정본이다.
            // 예전에는 그 임시 파일을 읽어 런 종료에 실패한 메타의 "런 없음, 기록 있음" 을 받아들였고,
            // 메타와 런을 맞추는 단계가 끝난 런으로 보고 런을 지웠다. 잠금이 풀리면 기록 없는 본 메타가 돌아와 런도 기록도 잃었다.
            // 2026년 10월 9일 외부 검토가 재현했다. 읽기 실패는 `CannotRead` 가 따로 알린다.
            if (IsUnreadable(fileName, main))
            {
                return null;
            }

            string temp = _storage.Read(fileName + suffix);
            return temp != null && SaveChecksum.VerifyJson(temp) ? temp : null;
        }

        /// <summary>그 이름의 메타를 읽되 검사를 통과한 것만 돌려준다.</summary>
        private MetaSaveData ReadMeta(string fileName)
        {
            return ParseMeta(ReadSaved(fileName));
        }

        /// <summary>메타 글을 자료로 되돌리되 검사를 통과한 것만 돌려준다.</summary>
        private static MetaSaveData ParseMeta(string json)
        {
            if (json == null)
            {
                return null;
            }

            MetaSaveData data = Parse<MetaSaveData>(json);
            if (data == null || !data.IsConsistent() || !SaveChecksum.VerifyJson(json))
            {
                return null;
            }

            return data;
        }

        /// <summary>
        /// 그 이름의 저장을 지금 읽을 수 없는지. **읽지 못한 것은 손상과 다르다.**
        /// 본 파일이 있는데 글이 오지 않거나, 본 파일이 온전하지 않은데 남은 임시 파일을 읽지 못하면 그렇다.
        /// 이때는 백업으로 되돌리거나, 새로 만들거나, 지우거나, 메타와 맞추지 않는다. 다음에 다시 읽는다.
        /// 2026년 10월 9일 외부 검토가 잠시 읽지 못한 프로필이 새 프로필로 덮이는 것을 재현해 더했다.
        /// </summary>
        public bool CannotRead(string fileName)
        {
            string main = _storage.Read(fileName);
            if (IsUnreadable(fileName, main))
            {
                return true;
            }

            if (main != null && SaveChecksum.VerifyJson(main))
            {
                return false;
            }

            string suffix = _config.TempSuffix;
            if (string.IsNullOrEmpty(suffix))
            {
                return false;
            }

            return IsUnreadable(fileName + suffix, _storage.Read(fileName + suffix));
        }

        /// <summary>파일은 있는데 글을 읽지 못했는지. 없는 파일이나 망가진 글과 구분한다.</summary>
        private bool IsUnreadable(string fileName, string text)
        {
            return text == null && _storage.Exists(fileName);
        }

        /// <summary>본 파일과 그 임시 파일을 함께 지운다.</summary>
        private void DeleteWithTemp(string fileName)
        {
            _storage.Delete(fileName);

            if (!string.IsNullOrEmpty(_config.TempSuffix))
            {
                _storage.Delete(fileName + _config.TempSuffix);
            }
        }

        private SaveOutcome WriteJson(string fileName, string json)
        {
            // 되살리지 못한 온전한 임시 파일이 있으면 먼저 되살린다. 그래도 안 되면 쓰지 않는다.
            // 쓰기는 같은 임시 이름에 글을 새로 쓰므로, 도중에 실패하면 유일한 진행이 사라진다.
            if (ReadKeptTemp(fileName) != null && !_storage.Promote(fileName + _config.TempSuffix, fileName))
            {
                return SaveOutcome.WriteFailed;
            }

            bool ok = _storage.Write(fileName, json, _config.TempSuffix);
            return ok ? SaveOutcome.Ok : SaveOutcome.WriteFailed;
        }

        /// <summary>글을 자료로 되돌린다. 글이 망가졌으면 null 이다.</summary>
        private static T Parse<T>(string json) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
