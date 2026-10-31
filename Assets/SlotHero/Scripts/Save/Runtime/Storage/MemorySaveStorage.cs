using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 메모리에만 담는 창구.
    ///
    /// 유니티 없이 저장 규칙을 검사할 때 쓴다.
    /// 쓰기를 일부러 실패시키거나 임시 파일을 남겨 두어
    /// 10 예외 처리 표의 줄들을 실제로 재현할 수 있다.
    /// </summary>
    public class MemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();

        /// <summary>참이면 모든 쓰기가 실패한다. 저장 공간이 모자란 상황을 흉내 낸다.</summary>
        public bool FailWrites;

        /// <summary>참이면 임시 파일까지만 쓰고 본 이름으로 바꾸지 않는다. 쓰다 꺼진 상황이다.</summary>
        public bool StopBeforeRename;

        /// <summary>지금까지 쓰기를 시도한 횟수.</summary>
        public int WriteCount;

        /// <summary>여기 든 이름은 쓰기가 실패한다. 메타만 쓰지 못하는 것처럼 한 파일만 막을 때 쓴다.</summary>
        public readonly HashSet<string> FailingWrites = new HashSet<string>();

        /// <summary>여기 든 이름은 있어도 읽지 못한다. 다른 쪽이 파일을 잠시 붙잡은 상황을 흉내 낸다.</summary>
        public readonly HashSet<string> Unreadable = new HashSet<string>();

        /// <inheritdoc />
        public bool Exists(string fileName)
        {
            return fileName != null && _files.ContainsKey(fileName);
        }

        /// <inheritdoc />
        public string Read(string fileName)
        {
            string text;
            if (fileName != null && !Unreadable.Contains(fileName) && _files.TryGetValue(fileName, out text))
            {
                return text;
            }

            return null;
        }

        /// <inheritdoc />
        public bool Write(string fileName, string text, string tempSuffix)
        {
            if (fileName == null)
            {
                return false;
            }

            WriteCount++;

            if (FailWrites || FailingWrites.Contains(fileName))
            {
                return false;
            }

            string temp = fileName + (string.IsNullOrEmpty(tempSuffix) ? ".tmp" : tempSuffix);
            _files[temp] = text ?? string.Empty;

            if (StopBeforeRename)
            {
                return false;
            }

            _files[fileName] = _files[temp];
            _files.Remove(temp);
            return true;
        }

        /// <inheritdoc />
        public bool Promote(string tempFileName, string fileName)
        {
            string text;
            if (tempFileName == null || fileName == null || !_files.TryGetValue(tempFileName, out text))
            {
                return false;
            }

            // 쓰기가 막힌 상황이면 바꿔 달기도 실패한다. 임시 파일은 그대로 둔다.
            if (FailWrites)
            {
                return false;
            }

            _files[fileName] = text;
            _files.Remove(tempFileName);
            return true;
        }

        /// <inheritdoc />
        public void Delete(string fileName)
        {
            if (fileName != null)
            {
                _files.Remove(fileName);
            }
        }

        /// <inheritdoc />
        public void GetFileNames(List<string> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            foreach (KeyValuePair<string, string> pair in _files)
            {
                result.Add(pair.Key);
            }

            result.Sort();
        }

        /// <summary>글을 손으로 넣는다. 망가진 파일을 만들어 검사할 때 쓴다.</summary>
        public void Put(string fileName, string text)
        {
            if (fileName != null)
            {
                _files[fileName] = text ?? string.Empty;
            }
        }

        /// <summary>전부 지운다.</summary>
        public void Clear()
        {
            _files.Clear();
            Unreadable.Clear();
            FailingWrites.Clear();
            WriteCount = 0;
        }
    }
}
