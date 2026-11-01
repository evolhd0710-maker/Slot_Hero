namespace SlotHero.Combat
{
    /// <summary>
    /// 태그의 한국어 이름. 유물 소식처럼 화면 설정 없이 글을 만들 때 쓴다.
    /// 화면에 그리는 이름과 색은 현재 빌드 설정(`CurrentBuildVisualConfig`)이 정본이다.
    /// </summary>
    public static class TagNames
    {
        private static readonly string[] Names = { "수성", "금성", "지구", "화성", "목성", "토성", "천왕성", "해왕성", "명왕성" };

        public static string Get(SymbolTagType tag)
        {
            int at = (int)tag;
            return at >= 0 && at < Names.Length ? Names[at] : tag.ToString();
        }
    }
}
