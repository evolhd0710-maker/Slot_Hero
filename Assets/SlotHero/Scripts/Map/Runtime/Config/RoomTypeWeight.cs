using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SlotHero.Map
{
    /// <summary>
    /// 방 타입 하나의 등장 가중치. 합계가 10000이 되도록 맞춘다.
    /// 보스는 고정 배치이므로 이 목록에 넣지 않는다.
    /// </summary>
    [Serializable]
    public struct RoomTypeWeight
    {
        [FormerlySerializedAs("RoomType")]
        [SerializeField] private RoomType _roomType;
        public RoomType RoomType { get { return _roomType; } set { _roomType = value; } }

        [FormerlySerializedAs("Weight")]
        [SerializeField] private int _weight;
        public int Weight { get { return _weight; } set { _weight = value; } }

        public RoomTypeWeight(RoomType roomType, int weight)
        {
            _roomType = roomType;
            _weight = weight;
        }
    }
}
