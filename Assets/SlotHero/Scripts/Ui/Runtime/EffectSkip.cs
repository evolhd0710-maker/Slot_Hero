using System;
using System.Collections.Generic;

namespace SlotHero.Ui
{
    /// <summary>
    /// 연출 중 화면을 누르면 연출을 끝 모습으로 넘긴다(2026년 11월 1일 원재).
    /// 전투 밖 연출(보상 화면, 지도 두루마리와 노드 반짝임, 이벤트 선택지, 야영 회복, 행상 유물 새로고침)이 쓴다.
    ///
    /// 연출이 시작하면 `Begin` 으로 "끝 모습으로 넘기는 일" 을 맡기고, 끝나면 `End` 로 거둔다.
    /// 화면 맨 위의 투명한 칸(`EffectSkipCatcher`)이 맡긴 일이 있는 동안만 누르기를 받아 `SkipAll` 을 부른다.
    /// 누르기는 그 칸이 먹으므로 아래의 버튼이나 카드가 함께 눌리지 않는다.
    /// </summary>
    public static class EffectSkip
    {
        private struct Entry
        {
            public object Owner;
            public Action Skip;
        }

        private static readonly List<Entry> Entries = new List<Entry>();

        /// <summary>건너뛸 수 있는 연출이 돌고 있는지.</summary>
        public static bool Waiting
        {
            get { return Entries.Count > 0; }
        }

        /// <summary>owner 의 연출이 건너뛰기를 기다리는지.</summary>
        public static bool IsWaiting(object owner)
        {
            return IndexOf(owner) >= 0;
        }

        /// <summary>
        /// owner 의 연출을 건너뛸 수 있게 맡긴다. 화면을 누르면 skip 을 부른다.
        /// 같은 owner 가 다시 맡기면 새 일로 바꾼다.
        /// </summary>
        public static void Begin(object owner, Action skip)
        {
            if (owner == null || skip == null)
            {
                return;
            }

            End(owner);
            Entries.Add(new Entry { Owner = owner, Skip = skip });
        }

        /// <summary>owner 의 연출이 끝났다. 맡긴 일을 거둔다.</summary>
        public static void End(object owner)
        {
            int index = IndexOf(owner);
            if (index >= 0)
            {
                Entries.RemoveAt(index);
            }
        }

        /// <summary>
        /// 화면을 눌렀다. 맡긴 연출을 모두 끝 모습으로 넘긴다.
        /// 넘기는 동안 다음 연출이 새로 맡기면 그것은 남는다. 다음 누르기에 넘긴다.
        /// </summary>
        public static void SkipAll()
        {
            if (Entries.Count == 0)
            {
                return;
            }

            Entry[] skipping = Entries.ToArray();
            Entries.Clear();
            for (int i = 0; i < skipping.Length; i++)
            {
                skipping[i].Skip();
            }
        }

        /// <summary>맡긴 일을 모두 버린다. 씬이 바뀌거나 검사를 새로 시작할 때 쓴다.</summary>
        public static void Clear()
        {
            Entries.Clear();
        }

        private static int IndexOf(object owner)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (ReferenceEquals(Entries[i].Owner, owner))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
