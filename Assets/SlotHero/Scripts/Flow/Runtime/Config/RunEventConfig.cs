using System.Collections.Generic;
using UnityEngine;
using SlotHero.Events;
using SlotHero.Map;

namespace SlotHero.Flow
{
    /// <summary>
    /// 이벤트 방에 나올 이야기들.
    /// 각본은 `이벤트 기획서 초안` 6장 이벤트 상세 를 옮긴 것이고
    /// 실제 글은 `EventScripts` 가 짓는다.
    ///
    /// 어느 이벤트가 나올지는 런 시드와 방 번호로 정한다.
    /// 같은 시드로 같은 방에 들어가면 같은 이벤트가 나온다.
    /// 저장 시스템 기획서 10쪽의 "시드에서 다시 만드는 값"이 그렇게 정했다.
    /// </summary>
    [CreateAssetMenu(fileName = "RunEventConfig", menuName = "Slot Hero/흐름/이벤트 목록")]
    public class RunEventConfig : ScriptableObject
    {
        [Tooltip("이벤트 방에 나올 이야기들.")]
        public List<EventScript> Events = new List<EventScript>();

        [Tooltip("도전 이벤트가 전투에 거는 부정 효과. 이벤트 기획서 3장 도전 이벤트 공통 규칙 의 부정 효과 목록(D1 ~ D6)이다. " +
            "35 도전자 는 여기서 둘을 뽑아 고르게 하고, 38 매복 은 하나를 무작위로 건다. " +
            "얼마나 깎고 늘리는지는 전투가 정한다.")]
        public List<EventCombatPenalty> CombatPenalties = EventCombatPenalty.SpecDefaults();

        [Tooltip("화면 하나에 나오는 선택지의 최대 수. 이벤트 기획서 초안 1장 의 \"선택지 1~5개\" 로 2026년 10월 9일 원재가 5개로 정했다. " +
            "채우는 선택지와 저절로 끼우는 떠나는 선택지까지 센다. 넘는 화면은 검사가 알린다. " +
            "고른 이력과 함께 놓는 칸 수(EventLayoutConfig.MaxChoiceCount)보다 크면 안 된다.")]
        [Min(1)]
        public int MaxChoicesPerScreen = 5;

        /// <summary>선택지가 화면당 최대 수를 넘는 화면을 찾는다(`EventScript.FindTooManyChoices`).</summary>
        public void FindTooManyChoices(List<string> problems)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i] != null)
                {
                    Events[i].FindTooManyChoices(MaxChoicesPerScreen, problems);
                }
            }
        }

        /// <summary>
        /// 그 방에 나올 이벤트를 고른다. 스테이지를 모르면 첫 스테이지로 본다.
        /// 런 시드와 방 번호를 섞어 뽑으므로 같은 자리에서는 늘 같은 것이 나온다.
        /// </summary>
        public EventScript Pick(int runSeed, int nodeId)
        {
            return Pick(runSeed, nodeId, 1);
        }

        /// <summary>
        /// 그 스테이지의 방에 나올 이벤트를 고른다. 그 스테이지에 나올 수 있는 이벤트(`EventScript.AppearsIn`)만
        /// 출현 비중(`EventScript.PickWeight`)대로 뽑는다. 나올 수 있는 것이 없으면 null 이고, 그때 흐름은 방을 마친다.
        ///
        /// 모든 이벤트가 모든 스테이지에 비중 1 이면 예전과 같은 이벤트가 나온다. 굴리는 수가 이벤트 수와 같기 때문이다.
        /// 그래서 이 칸을 더하기 전에 저장한 런도 같은 방에서 같은 이벤트를 만난다.
        /// 2026년 10월 9일 외부 검토가 권했고 원재가 틀만 만들기로 정했다.
        /// </summary>
        public EventScript Pick(int runSeed, int nodeId, int stage)
        {
            return Pick(runSeed, nodeId, stage, null);
        }

        /// <summary>
        /// 그 스테이지의 방에 나올 이벤트를 고른다. seenIds 에 든 이벤트는 뺀다. 같은 런에서 이미 만난 이벤트다.
        /// 2026년 10월 9일 원재가 "모든 이벤트는 같은 런에서 두 번 다시 볼 일이 없도록" 하라고 정했다.
        /// 다 만나 나올 것이 없으면 null 이고, 그때 흐름은 방을 마친다.
        /// 뺄 것이 없으면 `Pick(runSeed, nodeId, stage)` 와 같다.
        /// </summary>
        public EventScript Pick(int runSeed, int nodeId, int stage, ICollection<string> seenIds)
        {
            int total = 0;
            for (int i = 0; i < Events.Count; i++)
            {
                if (CanPick(Events[i], stage, seenIds))
                {
                    total += Events[i].PickWeight;
                }
            }

            if (total <= 0)
            {
                return null;
            }

            MapRandom random = new MapRandom(runSeed ^ (nodeId * 92821));
            int roll = random.Range(0, total);

            for (int i = 0; i < Events.Count; i++)
            {
                if (!CanPick(Events[i], stage, seenIds))
                {
                    continue;
                }

                roll -= Events[i].PickWeight;
                if (roll < 0)
                {
                    return Events[i];
                }
            }

            return null;
        }

        /// <summary>그 스테이지에 나올 수 있고 아직 만나지 않은 이벤트인지.</summary>
        private static bool CanPick(EventScript script, int stage, ICollection<string> seenIds)
        {
            return script != null
                && script.AppearsIn(stage)
                && (seenIds == null || !seenIds.Contains(script.EventId));
        }

        /// <summary>식별자로 찾는다.</summary>
        public EventScript Find(string eventId)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i] != null && Events[i].EventId == eventId)
                {
                    return Events[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 각본이 끊긴 데가 없는지 훑는다.
        /// 없는 화면을 가리키거나 어디서도 갈 수 없는 화면이 있으면 적어 준다.
        /// 빠뜨리면 그 이벤트에서 런이 막힌다.
        /// </summary>
        public void FindBrokenLinks(List<string> problems)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i] != null)
                {
                    Events[i].FindBrokenLinks(problems);
                }
            }
        }

        /// <summary>식별자가 겹치는 이벤트가 없는지.</summary>
        public bool AllIdsAreUnique()
        {
            for (int i = 0; i < Events.Count; i++)
            {
                for (int j = i + 1; j < Events.Count; j++)
                {
                    if (Events[i] != null && Events[j] != null
                        && Events[i].EventId == Events[j].EventId)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
