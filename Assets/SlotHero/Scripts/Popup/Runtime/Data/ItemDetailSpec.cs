using System;

namespace SlotHero.Popup
{
    /// <summary>
    /// 아이템 상세 오버레이에 적을 것.
    /// 인게임 화면 기획서 v0.2 / 13 현재 빌드 화면 의 아이템 상세 팝업 구성을 따른다.
    /// 이름은 좌상단, 가치 값은 우상단, 내용은 그 아래, 플레이버 텍스트는 하단이다.
    ///
    /// 문양은 현재 가치를, 유물과 코인은 희귀도를 <see cref="ValueText"/>에 적는다.
    /// 아이템마다의 내용은 문양, 코인, 유물 기획서 소관이라 적을 글만 받는다.
    /// </summary>
    [Serializable]
    public struct ItemDetailSpec
    {
        /// <summary>좌상단에 적는 이름.</summary>
        public string Name;

        /// <summary>우상단에 적는 가치 값이나 희귀도.</summary>
        public string ValueText;

        /// <summary>내용. 수치를 그대로 적고 조건이 있으면 함께 적는다.</summary>
        public string Body;

        /// <summary>플레이버 텍스트. 따옴표와 기울임은 표시 쪽이 입힌다.</summary>
        public string Flavor;

        public ItemDetailSpec(string name, string valueText, string body, string flavor)
        {
            Name = name;
            ValueText = valueText;
            Body = body;
            Flavor = flavor;
        }

        /// <summary>띄울 내용이 들어 있는지.</summary>
        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(Name); }
        }
    }
}
