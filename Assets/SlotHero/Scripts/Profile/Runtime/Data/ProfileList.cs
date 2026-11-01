using System;
using System.Collections.Generic;

namespace SlotHero.Profile
{
    /// <summary>
    /// 프로필 자리 전부.
    /// 저장 시스템 기획서의 "프로필 3개"에 맞춰 자리 수를 정해 둔다.
    ///
    /// 빈 자리도 자리로 센다. 화면이 자리마다 카드를 하나씩 놓기 때문이다.
    /// </summary>
    [Serializable]
    public class ProfileList
    {
        /// <summary>저장 시스템 기획서가 정한 프로필 수.</summary>
        public const int DefaultSlotCount = 3;

        /// <summary>자리마다의 요약. 빈 자리도 들어 있다.</summary>
        public List<ProfileSummary> Slots = new List<ProfileSummary>();

        /// <summary>지금 쓰고 있는 프로필. 고른 것이 없으면 -1.</summary>
        public int CurrentIndex = -1;

        /// <summary>자리 수.</summary>
        public int Count
        {
            get { return Slots.Count; }
        }

        /// <summary>지금 쓰는 프로필의 이름. 고른 것이 없으면 빈 글.</summary>
        public string CurrentName
        {
            get
            {
                if (CurrentIndex < 0 || CurrentIndex >= Slots.Count)
                {
                    return string.Empty;
                }

                return Slots[CurrentIndex].Name;
            }
        }

        /// <summary>자리 수를 맞춘다. 모자라면 빈 자리로 채우고 넘치면 버린다.</summary>
        public void EnsureSlots(int count)
        {
            if (count < 0)
            {
                count = 0;
            }

            while (Slots.Count < count)
            {
                Slots.Add(ProfileSummary.Empty());
            }

            while (Slots.Count > count)
            {
                Slots.RemoveAt(Slots.Count - 1);
            }

            if (CurrentIndex >= Slots.Count)
            {
                CurrentIndex = -1;
            }
        }

        /// <summary>그 자리의 요약. 자리를 벗어나면 빈 자리를 돌려준다.</summary>
        public ProfileSummary Get(int index)
        {
            if (index < 0 || index >= Slots.Count)
            {
                return ProfileSummary.Empty();
            }

            return Slots[index];
        }

        /// <summary>그 자리를 바꾼다.</summary>
        public void Set(int index, ProfileSummary summary)
        {
            if (index < 0 || index >= Slots.Count)
            {
                return;
            }

            Slots[index] = summary;
        }

        /// <summary>
        /// 그 자리를 빈 자리로 되돌린다.
        /// 기획서의 "프로필을 삭제하고 해당 자리를 빈 자리로 되돌린다"에 해당한다.
        /// 지우는 자리가 쓰고 있던 프로필이면 고른 것을 없앤다.
        /// </summary>
        public void Clear(int index)
        {
            if (index < 0 || index >= Slots.Count)
            {
                return;
            }

            Slots[index] = ProfileSummary.Empty();

            if (CurrentIndex == index)
            {
                CurrentIndex = -1;
            }
        }

        /// <summary>
        /// 게임을 켤 때 들어갈 자리를 고른다. 없으면 -1 이다.
        ///
        /// **마지막으로 논 자리**를 고른다. 날짜 꼴이 `yyyy-MM-dd` 라 글자끼리 견주면 된다.
        /// 날짜가 같거나 아직 논 적이 없으면 번호가 작은 쪽이다.
        /// 빈 자리와 읽을 수 없는 자리는 고르지 않는다.
        /// 그 자리를 골라 봐야 이름을 받거나 오류 팝업을 띄워야 하기 때문이다.
        ///
        /// 05 타이틀 화면 에 현재 프로필 버튼이 있어
        /// 알아서 고른 뒤에도 프로필을 바꿀 길은 남아 있다.
        /// </summary>
        public int PickStarting()
        {
            int best = -1;
            string bestDate = string.Empty;

            for (int i = 0; i < Slots.Count; i++)
            {
                if (!Slots[i].CanEnter)
                {
                    continue;
                }

                string date = Slots[i].LastPlayedDate ?? string.Empty;

                if (best < 0 || string.CompareOrdinal(date, bestDate) > 0)
                {
                    best = i;
                    bestDate = date;
                }
            }

            return best;
        }

        /// <summary>이름이 붙은 자리가 하나라도 있는지.</summary>
        public bool HasAny()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (!Slots[i].IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>자리 수만큼 빈 자리로 채운 목록을 만든다.</summary>
        public static ProfileList CreateEmpty(int count)
        {
            ProfileList list = new ProfileList();
            list.EnsureSlots(count);
            return list;
        }
    }
}
