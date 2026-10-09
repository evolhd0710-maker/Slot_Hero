using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Events.UI;

namespace SlotHero.Events
{
    /// <summary>삽화 하나. 식별자와 그림이다.</summary>
    [Serializable]
    public struct EventIllustration
    {
        /// <summary>`이벤트식별자_화면식별자`. 그림 파일 이름과 같다.</summary>
        public string Id;

        public Sprite Sprite;
    }

    /// <summary>
    /// 이벤트 삽화를 모아 둔다.
    ///
    /// 인게임 화면 기획서 v0.2 / 11 이벤트 화면 의 삽화 1200 × 500 칸에 들어갈 그림이다.
    /// 어떤 화면에 어떤 그림이 붙는지는 이벤트 기획서 소관이라 식별자로만 잇는다.
    /// 그림을 넣는 일은 `그림 가져오기` 가 `Events/Art` 의 파일 이름으로 한다.
    ///
    /// **지금 그림은 임시다.** 원재가 2026년 10월 3일에 02 ~ 06 다섯 이벤트에 쓸 임시 그림을 주었다.
    /// </summary>
    [CreateAssetMenu(fileName = "EventIllustrationConfig", menuName = "Slot Hero/이벤트/삽화 목록")]
    public class EventIllustrationConfig : ScriptableObject, IEventIllustrationSource
    {
        public List<EventIllustration> Illustrations = new List<EventIllustration>();

        /// <inheritdoc />
        public Sprite GetIllustration(string illustrationId)
        {
            if (string.IsNullOrEmpty(illustrationId))
            {
                return null;
            }

            for (int i = 0; i < Illustrations.Count; i++)
            {
                if (Illustrations[i].Id == illustrationId)
                {
                    return Illustrations[i].Sprite;
                }
            }

            return null;
        }

        /// <summary>그림을 넣는다. 같은 식별자가 있으면 갈아 끼운다.</summary>
        public void Set(string illustrationId, Sprite sprite)
        {
            EventIllustration entry = new EventIllustration();
            entry.Id = illustrationId;
            entry.Sprite = sprite;

            for (int i = 0; i < Illustrations.Count; i++)
            {
                if (Illustrations[i].Id == illustrationId)
                {
                    Illustrations[i] = entry;
                    return;
                }
            }

            Illustrations.Add(entry);
        }
    }
}
