using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 이벤트 화면 하나.
    /// 이벤트 기획서 초안 1장 구성 방식 의 "일러스트 1개, 스토리, 선택지 1~5개"다.
    /// </summary>
    [Serializable]
    public class EventScreen
    {
        /// <summary>화면 식별자. 기획서의 "화면 2A" 같은 이름을 그대로 쓴다.</summary>
        public string ScreenId = string.Empty;

        /// <summary>삽화 식별자. 그림 자체는 표시 쪽이 이 식별자로 찾아온다.</summary>
        public string IllustrationId = string.Empty;

        /// <summary>
        /// 분위기. 기획서가 "연출 참고용 방향만 짧게 적는다"로 정한 그것이다.
        /// 화면에 나오지 않는다. 삽화를 그릴 때 보는 메모다.
        /// </summary>
        public string Mood = string.Empty;

        /// <summary>본문. 화면에 나오는 스토리다.</summary>
        public string BodyText = string.Empty;

        /// <summary>선택지. 위에서 아래로 그대로 놓인다.</summary>
        public List<EventScreenChoice> Choices = new List<EventScreenChoice>();

        public EventScreen()
        {
        }

        public EventScreen(string screenId, string mood, string bodyText)
        {
            ScreenId = screenId;
            Mood = mood;
            BodyText = bodyText;
        }

        /// <summary>식별자로 선택지를 찾는다.</summary>
        public EventScreenChoice Find(string choiceId)
        {
            if (string.IsNullOrEmpty(choiceId))
            {
                return null;
            }

            for (int i = 0; i < Choices.Count; i++)
            {
                if (Choices[i] != null && Choices[i].Id == choiceId)
                {
                    return Choices[i];
                }
            }

            return null;
        }

        /// <summary>선택지를 하나 더한다. 짓는 쪽이 줄줄이 부를 수 있게 자기를 돌려준다.</summary>
        public EventScreen Add(EventScreenChoice choice)
        {
            if (choice != null)
            {
                Choices.Add(choice);
            }

            return this;
        }

        /// <summary>
        /// 마무리 화면인지. 끝내는 선택지만 있는 화면이다.
        /// 4장 공통 규칙 이 "마무리 화면의 종료 선택지로 이벤트를 마친다"로 정했다.
        /// </summary>
        public bool IsEnding
        {
            get
            {
                if (Choices.Count == 0)
                {
                    return false;
                }

                for (int i = 0; i < Choices.Count; i++)
                {
                    if (Choices[i] == null || Choices[i].Kind != EventChoiceKind.Finish)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
