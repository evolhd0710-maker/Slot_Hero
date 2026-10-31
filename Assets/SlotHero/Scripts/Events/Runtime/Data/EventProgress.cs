using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 이벤트 한 번의 진행 기록.
    /// 어느 이벤트에서 무엇을 골랐는지만 담는다.
    ///
    /// 저장 시스템 기획서의 자동 저장 시점은 방 진입과 방 완료라
    /// 이벤트 도중에 저장되는 일은 없지만,
    /// 고른 내용이 뒤의 진행에 남을 수 있어 런 데이터에 실을 수 있게 해 둔다.
    /// </summary>
    [Serializable]
    public class EventProgress
    {
        /// <summary>지금 진행 중인 이벤트의 식별자.</summary>
        public string EventId;

        /// <summary>고른 선택지를 고른 차례대로 담는다.</summary>
        public List<string> ChosenChoiceIds = new List<string>();

        /// <summary>이벤트가 끝났는지.</summary>
        public bool Finished;

        /// <summary>새 이벤트를 시작한다.</summary>
        public void Begin(string eventId)
        {
            EventId = eventId;
            ChosenChoiceIds.Clear();
            Finished = false;
        }

        /// <summary>고른 선택지를 적어 둔다.</summary>
        public void Record(string choiceId)
        {
            if (string.IsNullOrEmpty(choiceId))
            {
                return;
            }

            ChosenChoiceIds.Add(choiceId);
        }

        /// <summary>이벤트를 끝낸 것으로 적는다.</summary>
        public void Finish()
        {
            Finished = true;
        }

        /// <summary>몇 번 골랐는지.</summary>
        public int ChoiceCount
        {
            get { return ChosenChoiceIds != null ? ChosenChoiceIds.Count : 0; }
        }

        /// <summary>그 선택지를 고른 적이 있는지.</summary>
        public bool HasChosen(string choiceId)
        {
            if (ChosenChoiceIds == null || string.IsNullOrEmpty(choiceId))
            {
                return false;
            }

            for (int i = 0; i < ChosenChoiceIds.Count; i++)
            {
                if (ChosenChoiceIds[i] == choiceId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
