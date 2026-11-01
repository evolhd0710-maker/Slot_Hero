using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotHero.Ui
{
    /// <summary>
    /// 마우스 휠과 스크롤 바로만 움직이는 스크롤 창. 내용을 마우스로 끌어도 움직이지 않는다.
    /// 설정 화면 목록이 쓴다. 막대를 끌다가 목록이 함께 밀리지 않게 2026년 10월 31일 원재가 정했다.
    ///
    /// 유니티의 `ScrollRect` 는 끌기를 받으면 내용을 옮긴다. 여기서는 끌기 네 단계를 받아서 버린다.
    /// 받아서 버려야 끌기가 더 위의 창으로 넘어가지 않는다.
    /// </summary>
    public class WheelOnlyScrollRect : ScrollRect
    {
        /// <inheritdoc />
        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
        }

        /// <inheritdoc />
        public override void OnBeginDrag(PointerEventData eventData)
        {
        }

        /// <inheritdoc />
        public override void OnDrag(PointerEventData eventData)
        {
        }

        /// <inheritdoc />
        public override void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}
