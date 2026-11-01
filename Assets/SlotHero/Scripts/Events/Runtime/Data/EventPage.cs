using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 이벤트 화면에 한 번에 보여 줄 것.
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 삽화, 본문, 선택지를 담는다.
    ///
    /// 어떤 이벤트가 나오고 무엇이 적히는지는 이벤트 기획서 소관이라
    /// 그쪽이 만들어 넘겨 주는 자료다.
    /// </summary>
    [Serializable]
    public class EventPage
    {
        /// <summary>삽화 식별자. 그림 자체는 표시 쪽이 이 식별자로 찾아온다.</summary>
        public string IllustrationId;

        /// <summary>본문. 스토리나 상황 설명이 들어간다.</summary>
        public string BodyText;

        /// <summary>선택지 목록. 위에서 아래로 그대로 놓인다.</summary>
        public List<EventChoice> Choices = new List<EventChoice>();

        /// <summary>고를 수 있는 선택지가 하나라도 남아 있는지.</summary>
        public bool HasSelectableChoice
        {
            get
            {
                if (Choices == null)
                {
                    return false;
                }

                for (int i = 0; i < Choices.Count; i++)
                {
                    if (Choices[i] != null && Choices[i].IsSelectable)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 똑같은 내용의 새 쪽을 만든다.
        ///
        /// <see cref="ApplyResult"/> 가 본문과 선택지 목록을 제자리에서 갈아 끼우므로
        /// 설정 에셋이 들고 있는 쪽을 그대로 넘기면 **에셋이 영구히 바뀐다.**
        /// 그러면 다음에 같은 이벤트가 나올 때 앞의 진행이 그대로 이어진다.
        /// 이벤트 방에 들어갈 때마다 이것으로 복사해 쓴다.
        /// </summary>
        public EventPage Clone()
        {
            EventPage copy = new EventPage();
            copy.IllustrationId = IllustrationId;
            copy.BodyText = BodyText;

            if (Choices != null)
            {
                for (int i = 0; i < Choices.Count; i++)
                {
                    copy.Choices.Add(Choices[i] != null ? Choices[i].Clone() : null);
                }
            }

            return copy;
        }

        /// <summary>
        /// 식별자로 선택지를 찾는다. 같은 식별자가 여럿이면 가장 나중에 쌓인 것을 돌려준다.
        ///
        /// **뒤에서부터 찾는다.** 고른 칸은 위에 남고 새 칸은 그 아래에 쌓이는데,
        /// 각본 화면이 바뀌어도 선택지 식별자는 같을 수 있다. 02 힘을 새기는 것 의 카드는 화면 2와 3이 모두
        /// `card_0` ~ `card_4` 이고, 28 회복 은 "살펴본다" 로 갔다가 돌아오면 같은 화면이 다시 나온다.
        /// 앞에서부터 찾으면 이미 고른 옛 칸이 잡혀 새 칸을 눌러도 아무 일이 없었다.
        /// </summary>
        public EventChoice Find(string choiceId)
        {
            if (Choices == null || string.IsNullOrEmpty(choiceId))
            {
                return null;
            }

            for (int i = Choices.Count - 1; i >= 0; i--)
            {
                if (Choices[i] != null && Choices[i].Id == choiceId)
                {
                    return Choices[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 선택지를 고른 뒤의 화면으로 바꾼다.
        /// 11 이벤트 화면 · 선택 후 의 규칙을 그대로 따른다.
        /// 본문을 결과 문장으로 갈아 끼우고, 고른 선택지는 위에 흐리게 남기며,
        /// 고르지 않은 선택지는 지우고, 이어지는 선택지를 그 아래에 쌓는다.
        /// 삽화는 새로 받은 것이 있을 때만 바꾼다.
        ///
        /// 칸이 maxChoiceCount 를 넘으면 고른 이력만 가장 오래 전 것부터 지운다. 새 선택지는 지우지 않는다.
        /// 지난 이력보다 지금 고를 수 있는 선택지가 보이는 쪽이 중요하기 때문이다.
        /// 0 이하를 넘기면 수를 제한하지 않는다.
        /// </summary>
        public bool ApplyResult(EventResult result, int maxChoiceCount)
        {
            if (result == null)
            {
                return false;
            }

            EventChoice chosen = Find(result.ChosenChoiceId);
            if (chosen == null)
            {
                return false;
            }

            chosen.State = EventChoiceState.Chosen;

            // 지금까지 고른 것만 순서대로 남기고 고르지 않은 것은 버린다.
            List<EventChoice> history = new List<EventChoice>();
            for (int i = 0; i < Choices.Count; i++)
            {
                if (Choices[i] != null && Choices[i].State == EventChoiceState.Chosen)
                {
                    history.Add(Choices[i]);
                }
            }

            List<EventChoice> next = new List<EventChoice>();
            if (result.NextChoices != null)
            {
                for (int i = 0; i < result.NextChoices.Count; i++)
                {
                    EventChoice choice = result.NextChoices[i];
                    if (choice == null)
                    {
                        continue;
                    }

                    choice.ResolveState();
                    next.Add(choice);
                }
            }

            // **칸이 모자라면 고른 이력만 오래된 것부터 지운다. 새 선택지는 지우지 않는다.**
            // 예전에는 이력과 새 선택지를 한 줄로 세워 앞에서부터 잘라, 새 선택지가 많으면 아직 고르지 않은 것까지 지워
            // 유일한 떠난다 가 사라질 수 있었다. 2026년 10월 9일 외부 검토가 짚었고 원재가 "이력만 줄인다" 로 정했다.
            // 새 선택지만으로 칸을 넘는 일은 화면당 선택지 수 검사(`EventScript.FindTooManyChoices`)가 막는다.
            if (maxChoiceCount > 0)
            {
                int room = maxChoiceCount - next.Count;
                if (room < 0)
                {
                    room = 0;
                }

                if (history.Count > room)
                {
                    history.RemoveRange(0, history.Count - room);
                }
            }

            List<EventChoice> kept = new List<EventChoice>(history);
            kept.AddRange(next);
            Choices = kept;
            BodyText = result.BodyText;

            if (!string.IsNullOrEmpty(result.IllustrationId))
            {
                IllustrationId = result.IllustrationId;
            }

            return true;
        }
    }

    /// <summary>
    /// 선택지를 고른 뒤 화면을 어떻게 바꿀지.
    /// 11 이벤트 화면 · 선택 후 의 규칙을 그대로 담는다.
    /// 본문은 결과 문장으로 갈아 끼우고, 고른 선택지는 맨 위에 남기며,
    /// 이어지는 선택지가 있으면 그 아래에 쌓고, 고르지 않은 선택지는 지운다.
    /// </summary>
    [Serializable]
    public class EventResult
    {
        /// <summary>어느 선택지를 고른 결과인지.</summary>
        public string ChosenChoiceId;

        /// <summary>본문에 새로 적을 결과 문장. 보상도 여기에 글로만 적는다.</summary>
        public string BodyText;

        /// <summary>삽화를 바꿀 때만 채운다. 비워 두면 앞의 삽화를 그대로 둔다.</summary>
        public string IllustrationId;

        /// <summary>이어지는 선택지. 비어 있으면 이벤트가 끝난다.</summary>
        public List<EventChoice> NextChoices = new List<EventChoice>();

        public EventResult()
        {
        }

        public EventResult(string chosenChoiceId, string bodyText)
        {
            ChosenChoiceId = chosenChoiceId;
            BodyText = bodyText;
        }

        /// <summary>이어지는 선택지가 있는지.</summary>
        public bool HasNextChoices
        {
            get { return NextChoices != null && NextChoices.Count > 0; }
        }

        /// <summary>
        /// 똑같은 내용의 새 결과를 만든다.
        ///
        /// <see cref="EventPage.ApplyResult"/> 가 이어지는 선택지에 `State` 를 적어 넣으므로
        /// 설정 에셋이 들고 있는 결과를 그대로 넘기면 에셋이 바뀐다.
        /// <see cref="EventPage.Clone"/> 와 같은 까닭이다.
        /// </summary>
        public EventResult Clone()
        {
            EventResult copy = new EventResult(ChosenChoiceId, BodyText);
            copy.IllustrationId = IllustrationId;

            if (NextChoices != null)
            {
                for (int i = 0; i < NextChoices.Count; i++)
                {
                    copy.NextChoices.Add(NextChoices[i] != null ? NextChoices[i].Clone() : null);
                }
            }

            return copy;
        }
    }
}
