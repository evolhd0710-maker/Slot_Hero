namespace SlotHero.CurrentBuild
{
    /// <summary>
    /// 문양을 늘어놓는 차례.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의
    /// "개수 순, 태그 순, 이름 순 등의 정렬을 지원한다"를 따른다.
    /// </summary>
    public enum BuildSortOrder
    {
        /// <summary>개수가 많은 것부터.</summary>
        Count = 0,

        /// <summary>태그 차례대로. 같은 태그면 개수가 많은 것부터.</summary>
        Tag = 1,

        /// <summary>이름 차례대로.</summary>
        Name = 2,
    }
}
