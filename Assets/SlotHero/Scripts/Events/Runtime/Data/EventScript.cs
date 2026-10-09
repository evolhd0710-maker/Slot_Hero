using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>이벤트의 성격. 이벤트 기획서 초안 2장 이벤트 성격 분류.</summary>
    public enum EventNature
    {
        /// <summary>이익을 준다. 선택지가 손해가 되지 않는다.</summary>
        Good = 0,

        /// <summary>손해를 준다. 손해는 피할 수 없다.</summary>
        Bad = 1,

        /// <summary>결과가 확률로 정해진다.</summary>
        Random = 2,

        /// <summary>선택과 빌드에 따라 이익도 손해도 된다.</summary>
        Neutral = 3,
    }

    /// <summary>
    /// 이벤트 하나의 각본.
    /// 이벤트 기획서 초안 1장 구성 방식 의 "이벤트는 여러 화면으로 이루어진다"를 그대로 담는다.
    ///
    /// **쪽 하나가 자라는 꼴이 아니라 화면 여럿을 오가는 꼴이다.**
    /// 화면마다 일러스트와 스토리와 선택지가 있고,
    /// 선택지를 고르면 다음 화면으로 가거나 작업을 하거나 이벤트를 끝낸다.
    ///
    /// 화면에 무엇이 적히는지와 무슨 작업을 하는지만 담는다.
    /// 그것을 실제로 굴리는 것은 <see cref="EventRunner"/> 이고,
    /// 작업을 런에 적용하는 것은 <see cref="IEventWorld"/> 를 구현한 쪽이다.
    /// </summary>
    [Serializable]
    public class EventScript
    {
        /// <summary>이벤트 식별자.</summary>
        public string EventId = string.Empty;

        /// <summary>이벤트 이름. 5장 이벤트 목록 의 이름이다.</summary>
        public string Title = string.Empty;

        /// <summary>이벤트 성격.</summary>
        public EventNature Nature = EventNature.Good;

        /// <summary>
        /// 나오기 시작하는 스테이지. **0 이하면 첫 스테이지부터다.**
        /// 2026년 10월 9일 외부 검토가 스테이지별 등장 조건을 권해 칸만 만들었다. 수치는 기획서가 오면 에디터에서 넣는다.
        /// 칸이 없던 예전 에셋은 0 으로 읽혀 모든 스테이지에 나온다.
        /// </summary>
        public int MinStage;

        /// <summary>마지막으로 나오는 스테이지. 0 이하면 끝까지다.</summary>
        public int MaxStage;

        /// <summary>
        /// 출현 비중. 나올 수 있는 이벤트끼리 이 비중대로 뽑는다. **0 이하는 1 로 본다.**
        /// 칸이 없던 예전 에셋이 0 으로 읽혀도 고르게 나오게 하려는 것이다. 빼려면 스테이지 범위를 좁힌다.
        /// </summary>
        public int Weight = 1;

        /// <summary>그 스테이지에 나올 수 있는지.</summary>
        public bool AppearsIn(int stage)
        {
            return (MinStage <= 0 || stage >= MinStage) && (MaxStage <= 0 || stage <= MaxStage);
        }

        /// <summary>뽑을 때 쓰는 비중. 0 이하는 1 이다.</summary>
        public int PickWeight
        {
            get { return Weight > 0 ? Weight : 1; }
        }

        /// <summary>화면들. 첫 화면이 시작 화면이다.</summary>
        public List<EventScreen> Screens = new List<EventScreen>();

        /// <summary>시작 화면. 없으면 null 이다.</summary>
        public EventScreen StartScreen
        {
            get { return Screens.Count > 0 ? Screens[0] : null; }
        }

        /// <summary>식별자로 화면을 찾는다. 없으면 null 이다.</summary>
        public EventScreen Find(string screenId)
        {
            if (string.IsNullOrEmpty(screenId))
            {
                return null;
            }

            for (int i = 0; i < Screens.Count; i++)
            {
                if (Screens[i] != null && Screens[i].ScreenId == screenId)
                {
                    return Screens[i];
                }
            }

            return null;
        }

        /// <summary>화면을 하나 더한다. 짓는 쪽이 줄줄이 부를 수 있게 자기를 돌려준다.</summary>
        public EventScript Add(EventScreen screen)
        {
            if (screen != null)
            {
                Screens.Add(screen);
            }

            return this;
        }

        /// <summary>
        /// 어느 화면에서도 갈 수 없는 화면과, 없는 화면을 가리키는 선택지를 찾는다.
        ///
        /// 빠뜨리면 이벤트가 그 자리에서 멈춰 방을 나갈 수가 없다.
        /// 검사가 이것으로 각본을 훑는다.
        /// </summary>
        public void FindBrokenLinks(List<string> problems)
        {
            if (problems == null)
            {
                return;
            }

            HashSet<string> reached = new HashSet<string>();

            if (StartScreen != null)
            {
                reached.Add(StartScreen.ScreenId);
            }

            for (int i = 0; i < Screens.Count; i++)
            {
                EventScreen screen = Screens[i];
                if (screen == null)
                {
                    continue;
                }

                if (screen.Choices.Count == 0)
                {
                    problems.Add(EventId + "/" + screen.ScreenId + " 에 선택지가 없다");
                }

                for (int j = 0; j < screen.Choices.Count; j++)
                {
                    CheckChoice(screen, screen.Choices[j], reached, problems);
                }
            }

            for (int i = 0; i < Screens.Count; i++)
            {
                if (Screens[i] != null && !reached.Contains(Screens[i].ScreenId))
                {
                    problems.Add(EventId + "/" + Screens[i].ScreenId + " 으로 가는 길이 없다");
                }
            }
        }

        /// <summary>
        /// 화면 하나에 나올 수 있는 선택지가 max 를 넘는 화면을 찾는다.
        /// 채우는 선택지는 채울 수만큼 센다(`EventScreenChoice.FillCount`). 무작위 태그 5개면 5개다.
        /// 고를 것이 막혀 저절로 끼우는 떠나는 선택지는 여기서 세지 않는다. 화면을 굴리는 쪽이 칸을 넘기지 않게 끼운다(`EventRunner.MaxChoices`).
        /// 2026년 10월 9일 원재가 화면당 5개로 정했다. 이벤트 기획서 초안 1장 의 "선택지 1~5개" 다.
        /// </summary>
        public void FindTooManyChoices(int max, List<string> problems)
        {
            if (problems == null || max <= 0)
            {
                return;
            }

            for (int i = 0; i < Screens.Count; i++)
            {
                EventScreen screen = Screens[i];
                if (screen == null)
                {
                    continue;
                }

                int count = CountChoices(screen);
                if (count > max)
                {
                    problems.Add(EventId + "/" + screen.ScreenId + " 의 선택지가 " + count + "개로 " + max + "개를 넘는다");
                }
            }
        }

        /// <summary>화면 하나에 나올 수 있는 가장 많은 선택지 수. 채우는 선택지는 채울 수만큼 센다.</summary>
        public static int CountChoices(EventScreen screen)
        {
            int count = 0;
            for (int i = 0; i < screen.Choices.Count; i++)
            {
                EventScreenChoice choice = screen.Choices[i];
                if (choice == null)
                {
                    continue;
                }

                count += choice.Fill == EventChoiceFill.None ? 1 : (choice.FillCount > 0 ? choice.FillCount : 0);
            }

            return count;
        }

        private void CheckChoice(
            EventScreen screen, EventScreenChoice choice, HashSet<string> reached, List<string> problems)
        {
            if (choice == null)
            {
                problems.Add(EventId + "/" + screen.ScreenId + " 에 빈 선택지가 있다");
                return;
            }

            string where = EventId + "/" + screen.ScreenId + "/" + choice.Id;

            if (choice.Kind == EventChoiceKind.Finish)
            {
                return;
            }

            if (choice.Branches.Count > 0)
            {
                for (int i = 0; i < choice.Branches.Count; i++)
                {
                    CheckLink(where, choice.Branches[i].ScreenId, reached, problems);
                }

                return;
            }

            CheckLink(where, choice.NextScreenId, reached, problems);
        }

        private void CheckLink(
            string where, string screenId, HashSet<string> reached, List<string> problems)
        {
            if (string.IsNullOrEmpty(screenId))
            {
                problems.Add(where + " 가 갈 화면을 적지 않았다");
                return;
            }

            if (Find(screenId) == null)
            {
                problems.Add(where + " 가 없는 화면 " + screenId + " 을 가리킨다");
                return;
            }

            reached.Add(screenId);
        }
    }
}
