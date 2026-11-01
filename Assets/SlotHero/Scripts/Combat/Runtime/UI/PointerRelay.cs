using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SlotHero.Combat.UI
{
    /// <summary>
    /// 마우스가 올라가고 벗어난 것을 알린다.
    /// 코인처럼 눌러도 되고 올려만 봐도 정보가 떠야 하는 칸에 붙인다.
    /// </summary>
    public class PointerRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>올라가면 true, 벗어나면 false.</summary>
        public event Action<bool> HoverChanged;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HoverChanged != null)
            {
                HoverChanged(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoverChanged != null)
            {
                HoverChanged(false);
            }
        }

        private void OnDisable()
        {
            // 화면이 바뀌며 꺼지면 벗어난 것으로 친다. 팝업이 남지 않게 한다.
            if (HoverChanged != null)
            {
                HoverChanged(false);
            }
        }
    }
}
