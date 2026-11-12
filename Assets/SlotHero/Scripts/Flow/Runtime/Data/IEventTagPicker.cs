using System;
using SlotHero.Combat;

namespace SlotHero.Flow
{
    /// <summary>
    /// 이벤트의 태그 선택 창 창구. 07 기록 말살 의 "태그 선택 창에서 전체 태그 중 1개를 고른다" 에 쓴다.
    ///
    /// 지금 창(`Flow.UI.EventTagPicker`)은 전투의 태그 원판을 빌려 쓴다(2026년 11월 11일 원재).
    /// 흐름은 이 창구만 알아 전투 화면 코드가 없어도 순수 검사가 돈다. 전투 창구(`ICombatEntry`)와 같은 방식이다.
    /// </summary>
    public interface IEventTagPicker
    {
        /// <summary>창이 열려 있는지.</summary>
        bool IsOpen { get; }

        /// <summary>창을 연다. 태그를 누르면 onPicked, 취소하면 onCancelled 를 부른다.</summary>
        void Open(Action<SymbolTagType> onPicked, Action onCancelled);

        /// <summary>창만 닫는다. 아무것도 알리지 않는다.</summary>
        void Close();
    }
}
