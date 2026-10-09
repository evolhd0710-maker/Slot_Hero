using System;

namespace SlotHero.Map
{
    /// <summary>
    /// 방 타입 하나의 등장 가중치. 합계가 10000이 되도록 맞춘다.
    /// 보스는 고정 배치이므로 이 목록에 넣지 않는다.
    /// </summary>
    [Serializable]
    public struct RoomTypeWeight
    {
        public RoomType RoomType;
        public int Weight;

        public RoomTypeWeight(RoomType roomType, int weight)
        {
            RoomType = roomType;
            Weight = weight;
        }
    }
}
