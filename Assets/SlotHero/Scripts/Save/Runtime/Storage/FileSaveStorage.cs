using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SlotHero.Save
{
    /// <summary>
    /// 진짜 파일에 쓰는 창구.
    /// `Application.persistentDataPath` 아래 폴더 하나를 쓴다.
    ///
    /// 저장 시스템 기획서 v0.1 / 10 예외 처리 의
    /// "저장하던 중 종료 · 임시 파일 삭제 후 기존 저장 데이터 사용"을 지키려고
    /// 임시 이름에 먼저 쓰고 다 쓴 뒤에 본 이름으로 바꿔 단다.
    /// 바꿔 달 때는 기존 파일을 지우지 않고 `File.Replace` 로 한 번에 갈아 끼운다.
    /// 도중에 꺼져도 기존 파일이나 임시 파일 중 온전한 하나는 남는다.
    /// </summary>
    public class FileSaveStorage : ISaveStorage
    {
        private readonly string _root;

        /// <summary>저장 폴더를 정해 만든다.</summary>
        public FileSaveStorage(string root)
        {
            _root = root;
            EnsureFolder();
        }

        /// <summary>기획서가 정한 자리에 만든다.</summary>
        public static FileSaveStorage CreateDefault(SaveConfig config)
        {
            string folder = config != null ? config.FolderName : "SlotHero";
            return new FileSaveStorage(Path.Combine(Application.persistentDataPath, folder));
        }

        /// <summary>저장 폴더 자리. 어디에 저장되는지 보여 줄 때 쓴다.</summary>
        public string Root
        {
            get { return _root; }
        }

        /// <inheritdoc />
        public bool Exists(string fileName)
        {
            return File.Exists(GetPath(fileName));
        }

        /// <inheritdoc />
        public string Read(string fileName)
        {
            string path = GetPath(fileName);

            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogWarning("저장 파일을 읽지 못했다: " + path + " · " + e.Message);
                return null;
            }
        }

        /// <inheritdoc />
        public bool Write(string fileName, string text, string tempSuffix)
        {
            string path = GetPath(fileName);
            string temp = path + (string.IsNullOrEmpty(tempSuffix) ? ".tmp" : tempSuffix);
            bool tempWritten = false;

            try
            {
                EnsureFolder();
                File.WriteAllText(temp, text ?? string.Empty, Encoding.UTF8);
                tempWritten = true;

                // 바꿔 달기. **기존 파일을 먼저 지우지 않는다.**
                // 예전에는 지우고 나서 옮겨, 그 사이에 꺼지면 기존 파일이 사라지고
                // 남은 임시 파일마저 다음에 켤 때 치워져 진행을 통째로 잃을 수 있었다. 2026년 10월 8일에 고쳤다.
                if (File.Exists(path))
                {
                    ReplaceExisting(temp, path);
                }
                else
                {
                    File.Move(temp, path);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("저장 파일을 쓰지 못했다: " + path + " · " + e.Message);

                // 반쯤 쓴 임시 파일은 남기지 않는다.
                // 다 쓴 임시 파일은 남긴다. 바꿔 달다 실패했으면 그것이 가장 새 온전한 글이라
                // 다음에 켤 때 `SaveRepository.CleanTempFiles` 가 되살린다.
                if (!tempWritten)
                {
                    TryDelete(temp);
                }

                return false;
            }
        }

        /// <inheritdoc />
        public bool Promote(string tempFileName, string fileName)
        {
            string temp = GetPath(tempFileName);
            string path = GetPath(fileName);

            try
            {
                if (!File.Exists(temp))
                {
                    return false;
                }

                if (File.Exists(path))
                {
                    ReplaceExisting(temp, path);
                }
                else
                {
                    File.Move(temp, path);
                }

                return true;
            }
            catch (Exception e)
            {
                // 임시 파일은 지우지 않는다. 본 파일이 없으면 이것이 유일한 온전한 글이다.
                // 다음에 켤 때 다시 되살려 본다.
                Debug.LogWarning("임시 파일을 되살리지 못했다: " + temp + " · " + e.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public void Delete(string fileName)
        {
            TryDelete(GetPath(fileName));
        }

        /// <inheritdoc />
        public void GetFileNames(List<string> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();

            try
            {
                if (!Directory.Exists(_root))
                {
                    return;
                }

                string[] paths = Directory.GetFiles(_root);
                for (int i = 0; i < paths.Length; i++)
                {
                    result.Add(Path.GetFileName(paths[i]));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("저장 폴더를 읽지 못했다: " + _root + " · " + e.Message);
            }
        }

        /// <summary>
        /// 임시 파일로 기존 파일을 바꿔 단다.
        ///
        /// `File.Replace` 는 운영체제가 한 번에 바꿔 달아, 도중에 꺼져도 기존 파일이나 새 파일 중 하나는 온전히 남는다.
        /// 그것을 지원하지 않는 곳에서는 기존 파일 위에 덮어 복사한다.
        /// 덮어쓰는 동안에도 임시 파일은 그대로 남아, 다음에 켤 때 `SaveRepository.CleanTempFiles` 가 되살릴 수 있다.
        /// </summary>
        private static void ReplaceExisting(string temp, string path)
        {
            try
            {
                File.Replace(temp, path, null);
                return;
            }
            catch (PlatformNotSupportedException)
            {
            }
            catch (IOException e)
            {
                Debug.LogWarning("한 번에 바꿔 달지 못해 덮어쓴다: " + path + " · " + e.Message);
            }

            File.Copy(temp, path, true);
            File.Delete(temp);
        }

        private string GetPath(string fileName)
        {
            return Path.Combine(_root, fileName ?? string.Empty);
        }

        private void EnsureFolder()
        {
            try
            {
                if (!Directory.Exists(_root))
                {
                    Directory.CreateDirectory(_root);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("저장 폴더를 만들지 못했다: " + _root + " · " + e.Message);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("저장 파일을 지우지 못했다: " + path + " · " + e.Message);
            }
        }
    }
}
