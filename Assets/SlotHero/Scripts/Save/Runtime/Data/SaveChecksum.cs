namespace SlotHero.Save
{
    /// <summary>
    /// 저장 데이터가 온전한지 보는 값.
    /// 저장 시스템 기획서 v0.1 / 05 · 06 의 "체크섬" 칸과
    /// 10 예외 처리 의 "유효성 검사 실패" 세 줄이 이것을 쓴다.
    ///
    /// 암호가 아니다. 파일이 잘리거나 손으로 고쳐진 것을 잡아내는 용도다.
    /// FNV-1a 32비트를 쓴다. 곱셈과 xor 뿐이라 어느 기기에서든 같은 값이 나온다.
    /// </summary>
    public static class SaveChecksum
    {
        private const uint OffsetBasis = 2166136261u;
        private const uint Prime = 16777619u;

        /// <summary>글을 8자리 열여섯 진수로 줄인다.</summary>
        public static string Compute(string text)
        {
            if (text == null)
            {
                text = string.Empty;
            }

            uint hash = OffsetBasis;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // 글자를 바이트 둘로 나눠 넣는다.
                // 그래야 한글처럼 두 바이트를 쓰는 글자도 온전히 섞인다.
                hash = (hash ^ (uint)(c & 0xFF)) * Prime;
                hash = (hash ^ (uint)((c >> 8) & 0xFF)) * Prime;
            }

            return hash.ToString("x8");
        }

        /// <summary>적힌 값과 다시 매긴 값이 같은지.</summary>
        public static bool Matches(string text, string stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return false;
            }

            return Compute(text) == stored;
        }

        /// <summary>파일에서 체크섬 칸이 시작하는 글. `JsonUtility` 는 칸 사이에 빈칸을 넣지 않는다.</summary>
        private const string FieldStart = "\"Checksum\":\"";

        /// <summary>
        /// 파일에 적힌 글 그대로 체크섬이 맞는지 본다.
        ///
        /// 저장할 때는 체크섬 칸을 비운 글로 값을 매기고 그 값을 칸에 넣어 쓴다.
        /// 그래서 파일 글에서 체크섬 칸만 다시 비우면 값을 매긴 글이 그대로 나온다.
        ///
        /// **읽은 자료를 다시 글로 바꿔 매기지 않는다.**
        /// 예전에는 그렇게 했는데, 업데이트로 저장 자료에 칸이 하나만 늘어도 다시 바꾼 글이 달라져
        /// 멀쩡한 런이 손상으로 처리되어 지워졌다. 2026년 10월 8일에 고쳤다.
        /// 파일 글만 보므로 칸이 늘거나 줄어도, 판 번호가 달라도 결과가 같다.
        /// </summary>
        public static bool VerifyJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            int start = json.IndexOf(FieldStart, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return false;
            }

            int valueStart = start + FieldStart.Length;
            int valueEnd = json.IndexOf('"', valueStart);
            if (valueEnd < 0)
            {
                return false;
            }

            string stored = json.Substring(valueStart, valueEnd - valueStart);
            string blanked = json.Substring(0, valueStart) + json.Substring(valueEnd);
            return Matches(blanked, stored);
        }
    }
}
