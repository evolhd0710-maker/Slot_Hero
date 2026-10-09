using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SlotHero.TopBar.UI
{
    /// <summary>
    /// 마우스가 올라갔는지만 알려 주는 영역.
    /// 상단 UI 바 기획서 v0.2 / 06 상호작용 에서 위치 칸은 누르는 동작이 없고
    /// 마우스 호버에만 현재 스테이지 이름과 방 종류를 띄우므로 버튼을 쓰지 않는다.
    ///
    /// 루트에 Raycast Target 을 켠 투명 Image 를 함께 둬야 마우스를 받는다.
    /// </summary>
    public class TopBarHoverArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform _rectTransform;

        /// <summary>마우스가 올라가거나 벗어났을 때.</summary>
        public event Action<TopBarHoverArea, bool> HoverChanged;

        /// <summary>띄울 문구. 없으면 아무것도 띄우지 않는다.</summary>
        public string TooltipText { get; set; }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = (RectTransform)transform;
                }

                return _rectTransform;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HoverChanged != null)
            {
                HoverChanged(this, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoverChanged != null)
            {
                HoverChanged(this, false);
            }
        }
    }
}
