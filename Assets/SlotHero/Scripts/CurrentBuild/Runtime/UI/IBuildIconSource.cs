using UnityEngine;

namespace SlotHero.CurrentBuild.UI
{
    /// <summary>
    /// 유물, 코인, 문양의 그림을 내주는 쪽.
    /// <see cref="BuildEntry"/>는 식별자만 들고 있으므로 그림은 여기서 받아 온다.
    ///
    /// 무엇을 가지고 있고 어떤 그림을 쓰는지는 유물, 코인, 문양 기획서 소관이라
    /// 그 코드가 생기면 이 창구를 구현해 물려 주면 된다.
    /// </summary>
    public interface IBuildIconSource
    {
        /// <summary>그 항목의 그림. 없으면 null 을 돌려주면 된다.</summary>
        Sprite GetIcon(BuildEntry entry);
    }
}
