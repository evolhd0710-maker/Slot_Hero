using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 선택지에 지목해 둘 대상 하나.
    /// 문양이든 코인이든 유물이든 화면에는 이름만 나오므로 식별자와 이름만 담는다.
    /// </summary>
    [Serializable]
    public struct EventItem
    {
        public string Id;
        public string DisplayName;

        public EventItem(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(Id); }
        }

        /// <summary>화면에 적을 이름. 이름이 없으면 식별자를 쓴다.</summary>
        public string Label
        {
            get { return string.IsNullOrEmpty(DisplayName) ? Id : DisplayName; }
        }
    }

    /// <summary>
    /// 선택지를 무엇으로 채울지 묻는 것.
    /// 앞 화면에서 기억해 둔 것은 러너가 미리 풀어 식별자로 넣어 준다.
    /// </summary>
    public struct EventFillRequest
    {
        public EventChoiceFill Fill;

        /// <summary>몇 개를 채울지.</summary>
        public int Count;

        /// <summary><see cref="EventChoiceFill.TagsOfRemembered"/> 가 볼 문양.</summary>
        public string SourceId;

        /// <summary>뺄 것. 태그면 그 태그를, 문양이면 그 문양의 태그 둘을 뺀다. null 이면 빼지 않는다.</summary>
        public List<string> ExcludeIds;

        /// <summary>후보의 가장 낮은 등급. -1 이면 따지지 않는다. 코인과 유물에만 쓴다.</summary>
        public int MinRarity;

        /// <summary>후보의 가장 높은 등급. -1 이면 따지지 않는다. 코인과 유물에만 쓴다.</summary>
        public int MaxRarity;

        /// <summary>등급마다 뽑힐 비중. 차례가 등급이다. null 이거나 비어 있으면 고르게 뽑는다.</summary>
        public List<int> RarityWeights;

        public EventFillRequest(EventChoiceFill fill, int count)
        {
            Fill = fill;
            Count = count;
            SourceId = string.Empty;
            ExcludeIds = null;
            MinRarity = -1;
            MaxRarity = -1;
            RarityWeights = null;
        }
    }

    /// <summary>
    /// 작업을 걸 대상. 선택지가 지목한 것과 앞 화면에서 기억해 둔 것을 함께 담는다.
    /// </summary>
    public struct EventTarget
    {
        /// <summary>선택지가 지목한 것. 없으면 빈 글이다.</summary>
        public string Id;

        /// <summary>작업의 <see cref="EventAction.RecallKeys"/> 차례대로 풀어 둔 기억. null 일 수 있다.</summary>
        public List<string> Recalled;

        /// <summary>
        /// 선택지를 내놓을 때 미리 정해 둔 결과. 공개하는 선택지만 쓴다. null 일 수 있다.
        /// 칸에 적은 이름과 실제로 받는 것이 같아야 하므로 작업은 이것을 먼저 쓴다.
        /// </summary>
        public List<string> Decided;

        public EventTarget(string id)
        {
            Id = id ?? string.Empty;
            Recalled = null;
            Decided = null;
        }

        /// <summary>그 차례의 미리 정한 결과. 없으면 빈 글이다.</summary>
        public string GetDecided(int index)
        {
            if (Decided == null || index < 0 || index >= Decided.Count)
            {
                return string.Empty;
            }

            return Decided[index] ?? string.Empty;
        }

        /// <summary>그 차례의 기억. 없으면 빈 글이다.</summary>
        public string GetRecalled(int index)
        {
            if (Recalled == null || index < 0 || index >= Recalled.Count)
            {
                return string.Empty;
            }

            return Recalled[index] ?? string.Empty;
        }
    }

    /// <summary>
    /// 이벤트가 바깥 세상에 묻고 시키는 창구.
    ///
    /// 이벤트 코드는 런 데이터도 저장도 모른다.
    /// 무엇을 보여 줄지와 무슨 작업을 할지만 정하고,
    /// 실제로 가진 것을 세고 바꾸는 일은 이 창구를 구현한 쪽이 한다.
    /// `Flow` 의 `RunEventWorld` 가 그것이다.
    ///
    /// 이렇게 나눈 것은 이벤트 기획서가 문양과 코인과 유물의 실제 목록을
    /// 다른 문서로 넘겼기 때문이다. 그쪽이 바뀌어도 이벤트 각본은 그대로 돈다.
    /// </summary>
    public interface IEventWorld
    {
        /// <summary>지금 골드.</summary>
        int Gold { get; }

        /// <summary>지금 체력.</summary>
        int Health { get; }

        /// <summary>최대 체력.</summary>
        int MaxHealth { get; }

        /// <summary>
        /// 선택지를 채울 대상을 모은다.
        /// 모자라면 모은 만큼만 넣는다. 하나도 없으면 비워 둔다.
        /// </summary>
        void Collect(EventFillRequest request, EventRandom random, List<EventItem> into);

        /// <summary>
        /// 그 작업을 지금 할 수 있는지.
        /// 못 하면 까닭을 짧은 글로 돌려준다.
        /// 4장 공통 규칙 의 "조건을 채우지 못한 선택지는 숨기지 않고 비활성으로 표시하며,
        /// 부족한 조건을 함께 보여 준다"에 쓴다.
        /// </summary>
        bool CanDo(EventAction action, EventTarget target, out string reason);

        /// <summary>
        /// 그 작업이 받을 코인이나 유물이 소지 한도를 넘는지. 넘으면 흐름이 고르기 전에 버릴 것을 고르게 한다.
        /// 각본을 굴리는 쪽은 이것만으로 이루어진 화면에 떠나는 선택지를 끼운다. 버리지 않고 포기할 길이다.
        /// 2026년 10월 9일 원재가 "기존에 가지고 있던 것을 버리고 획득하거나 획득을 포기해야 한다" 고 정했다.
        /// </summary>
        bool NeedsRoom(EventAction action);

        /// <summary>
        /// 그 작업의 결과를 미리 정한다. 받거나 잃을 물건을 차례대로 넣는다.
        /// 결과를 공개하는 선택지(<see cref="EventScreenChoice.RevealOutcome"/>)에만 부른다.
        /// 정한 것은 <see cref="EventTarget.Decided"/> 로 <see cref="Apply"/> 에 돌아온다.
        /// 미리 정할 수 없는 작업이면 비워 둔다.
        /// </summary>
        void Decide(EventAction action, EventTarget target, EventRandom random, List<EventItem> into);

        /// <summary>
        /// 작업을 실제로 한다. 해낸 것을 짧은 글로 돌려준다.
        /// 그 글이 다음 화면 본문 아래에 붙는다.
        /// </summary>
        string Apply(EventAction action, EventTarget target, EventRandom random);
    }
}
