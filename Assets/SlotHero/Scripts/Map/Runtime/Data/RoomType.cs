namespace SlotHero.Map
{
    /// <summary>
    /// 맵 노드에 배치되는 방의 종류.
    /// 맵 생성 규칙 기획서 v0.2 / 04 방 목록 의 코드와 1:1로 맞춘다.
    /// </summary>
    public enum RoomType
    {
        /// <summary>아직 타입이 정해지지 않은 상태.</summary>
        None = 0,

        /// <summary>일반 몬스터 (NORMAL). 가중치 5000.</summary>
        Normal = 1,

        /// <summary>엘리트 몬스터 (ELITE). 가중치 2000. 연속 배치 불가.</summary>
        Elite = 2,

        /// <summary>성소 (SANCTUM). 가중치 500. 연속 배치 불가.</summary>
        Sanctum = 3,

        /// <summary>이벤트 (EVENT). 가중치 2500.</summary>
        Event = 4,

        /// <summary>기타 (ETC). 현재 가중치 0. 방 타입 추가 시 사용한다.</summary>
        Etc = 5,

        /// <summary>보스 (BOSS). 12단계 고정. 가중치 배분 대상이 아니다.</summary>
        Boss = 6,
    }

    /// <summary>방 종류를 다루는 데 쓰는 값.</summary>
    public static class RoomTypes
    {
        /// <summary>
        /// 화면에 적는 이름.
        /// 상단 표시줄의 위치 풍선과 방에 들어갈 때 쓴다.
        /// </summary>
        public static string GetDisplayName(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Normal:
                    return "일반";

                case RoomType.Elite:
                    return "엘리트";

                case RoomType.Sanctum:
                    return "성소";

                case RoomType.Event:
                    return "이벤트";

                case RoomType.Etc:
                    return "기타";

                case RoomType.Boss:
                    return "보스";

                default:
                    return string.Empty;
            }
        }

        /// <summary>몬스터와 싸우는 방인지. 일반, 엘리트, 보스 셋이다.</summary>
        public static bool IsCombat(RoomType roomType)
        {
            return roomType == RoomType.Normal
                || roomType == RoomType.Elite
                || roomType == RoomType.Boss;
        }
    }
}
