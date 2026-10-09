using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>선택지 칸이 지금 어떤 상태인지.</summary>
    public enum EventChoiceState
    {
        /// <summary>고를 수 있다.</summary>
        Selectable = 0,

        /// <summary>조건을 채우지 못해 고를 수 없다.</summary>
        Blocked = 1,

        /// <summary>이미 고른 것. 선택 후 화면에 흐리게 남는다.</summary>
        Chosen = 2,
    }

    /// <summary>
    /// 이벤트 선택지 하나.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 선택지 표시 세 가지를 모두 담는다.
    ///
    /// 결과가 공개된 선택지는 <see cref="Lines"/>에 획득과 소모를 담고,
    /// 결과를 알 수 없는 선택지는 <see cref="ResultHidden"/>을 켜 물음표만 나오게 하며,
    /// 조건을 채우지 못한 선택지는 <see cref="EventEffectKind.Missing"/> 줄을 넣는다.
    ///
    /// 어떤 조건을 보고 무엇을 주는지는 이벤트 기획서 소관이라
    /// 그 판정을 마친 결과만 이 자료로 넘겨받는다.
    /// </summary>
    [Serializable]
    public class EventChoice
    {
        /// <summary>선택지 식별자. 어느 것을 골랐는지 알리는 데 쓴다.</summary>
        public string Id;

        /// <summary>선택지 문구. "산책할까?" 처럼 행동만 적는다.</summary>
        public string Label;

        /// <summary>결과를 알 수 없는 선택지. 켜면 물음표만 나온다.</summary>
        public bool ResultHidden;

        /// <summary>칸 안에 적을 획득, 소모, 부족 줄.</summary>
        public List<EventEffectLine> Lines = new List<EventEffectLine>();

        /// <summary>칸의 상태.</summary>
        public EventChoiceState State = EventChoiceState.Selectable;

        public EventChoice()
        {
        }

        public EventChoice(string id, string label)
        {
            Id = id;
            Label = label;
        }

        /// <summary>
        /// 필요한데 갖추지 못한 것이 하나라도 있는지.
        /// 11 이벤트 화면 은 조건을 채우지 못한 선택지를 고를 수 없게 한다.
        /// </summary>
        public bool HasUnmetRequirement
        {
            get
            {
                if (Lines == null)
                {
                    return false;
                }

                for (int i = 0; i < Lines.Count; i++)
                {
                    if (Lines[i].Kind == EventEffectKind.Requirement && !Lines[i].Met)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>지금 누를 수 있는지. 이미 고른 것과 조건을 못 채운 것은 누를 수 없다.</summary>
        public bool IsSelectable
        {
            get { return State == EventChoiceState.Selectable; }
        }

        /// <summary>갖추지 못한 것이 있으면 고를 수 없는 상태로 맞춘다.</summary>
        public void ResolveState()
        {
            if (State == EventChoiceState.Chosen)
            {
                return;
            }

            State = HasUnmetRequirement ? EventChoiceState.Blocked : EventChoiceState.Selectable;
        }

        /// <summary>
        /// 똑같은 내용의 새 선택지를 만든다.
        ///
        /// **설정 에셋이 들고 있는 선택지를 화면이 그대로 쓰면 안 된다.**
        /// 고른 선택지에 `State` 를 적어 넣기 때문에 에셋 자체가 바뀌어
        /// 다음에 같은 이벤트가 나올 때 이미 고른 상태로 열린다.
        /// <see cref="EventPage.Clone"/> 가 이것을 쓴다.
        /// </summary>
        public EventChoice Clone()
        {
            EventChoice copy = new EventChoice(Id, Label);
            copy.ResultHidden = ResultHidden;
            copy.State = State;

            // EventEffectLine 은 구조체라 목록만 새로 만들면 값까지 따라 복사된다.
            if (Lines != null)
            {
                copy.Lines.AddRange(Lines);
            }

            return copy;
        }

        /// <summary>결과가 공개된 선택지를 만든다.</summary>
        public static EventChoice Known(string id, string label, params EventEffectLine[] lines)
        {
            EventChoice choice = new EventChoice(id, label);

            if (lines != null)
            {
                choice.Lines.AddRange(lines);
            }

            choice.ResolveState();
            return choice;
        }

        /// <summary>결과를 알 수 없는 선택지를 만든다.</summary>
        public static EventChoice Hidden(string id, string label)
        {
            EventChoice choice = new EventChoice(id, label);
            choice.ResultHidden = true;
            return choice;
        }
    }
}
