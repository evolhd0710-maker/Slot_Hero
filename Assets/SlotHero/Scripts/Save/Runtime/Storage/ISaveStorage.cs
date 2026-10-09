using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 글 하나를 이름 하나에 넣고 빼는 창구.
    ///
    /// 실제 파일과 메모리 둘 다 이것을 따른다.
    /// 유니티 없이 저장 규칙을 검사하려면 메모리 쪽이 필요하기 때문이다.
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>그 이름의 글이 있는지.</summary>
        bool Exists(string fileName);

        /// <summary>글을 읽는다. 없거나 읽지 못하면 null 을 돌려준다.</summary>
        string Read(string fileName);

        /// <summary>
        /// 글을 쓴다. 성공하면 true 다.
        /// 쓰는 도중에 꺼져도 기존 파일이 남도록 임시 이름에 먼저 쓰고 바꿔 단다.
        /// </summary>
        bool Write(string fileName, string text, string tempSuffix);

        /// <summary>
        /// 남은 임시 파일을 본 이름으로 바꿔 단다. 성공하면 true 다.
        /// 글을 다시 쓰지 않고 이름만 바꾼다. **실패해도 임시 파일을 건드리지 않는다.**
        /// 되살릴 수 있는 유일한 글일 수 있기 때문이다. `SaveRepository.CleanTempFiles` 가 쓴다.
        /// </summary>
        bool Promote(string tempFileName, string fileName);

        /// <summary>글을 지운다.</summary>
        void Delete(string fileName);

        /// <summary>지금 있는 이름을 모두 돌려준다. 남은 임시 파일을 찾는 데 쓴다.</summary>
        void GetFileNames(List<string> result);
    }
}
