using UnityEngine;

namespace SlotHero.Events.UI
{
    /// <summary>
    /// 이벤트 삽화를 내주는 쪽.
    /// <see cref="EventPage"/>는 식별자만 들고 있으므로 그림은 여기서 받아 온다.
    /// 어떤 이벤트에 어떤 삽화가 붙는지는 이벤트 기획서 소관이다.
    /// </summary>
    public interface IEventIllustrationSource
    {
        /// <summary>삽화. 없으면 null 을 돌려주면 된다.</summary>
        Sprite GetIllustration(string illustrationId);
    }
}
