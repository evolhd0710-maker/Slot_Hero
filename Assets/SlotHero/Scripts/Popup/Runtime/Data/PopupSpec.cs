using System;
using System.Collections.Generic;

namespace SlotHero.Popup
{
    /// <summary>
    /// 팝업 버튼 하나.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 의 공통 틀을 따른다.
    /// 목록에 넣은 차례가 곧 왼쪽부터의 자리다.
    /// 보조 버튼인 닫기와 취소와 아니오를 맨 앞에,
    /// 주 버튼인 확인과 예를 맨 뒤에 넣는다.
    ///
    /// 강조색은 자리와 따로 논다.
    /// 새 게임 확인은 오른쪽 끝 버튼이 강조색이고,
    /// 런 종료 확인은 가운데 런 포기가 강조색이다.
    /// </summary>
    [Serializable]
    public struct PopupButtonSpec
    {
        /// <summary>어느 버튼을 눌렀는지 알리는 식별자.</summary>
        public string Id;

        /// <summary>버튼에 적는 글.</summary>
        public string Label;

        /// <summary>되돌릴 수 없는 조작이라 강조색을 쓰는지.</summary>
        public bool Emphasized;

        public PopupButtonSpec(string id, string label, bool emphasized)
        {
            Id = id;
            Label = label;
            Emphasized = emphasized;
        }

        /// <summary>평범한 버튼.</summary>
        public static PopupButtonSpec Normal(string id, string label)
        {
            return new PopupButtonSpec(id, label, false);
        }

        /// <summary>되돌릴 수 없는 조작의 버튼. 강조색을 쓴다.</summary>
        public static PopupButtonSpec Danger(string id, string label)
        {
            return new PopupButtonSpec(id, label, true);
        }
    }

    /// <summary>
    /// 팝업 하나에 담을 것.
    /// 08 화면 팝업 의 공통 틀인 제목, 본문, 버튼을 담는다.
    /// 14 진행 중 팝업 도 같은 틀을 쓴다.
    ///
    /// 어떤 팝업이 언제 뜨는지는 각 화면이 정하고
    /// 이 자료는 화면에 무엇을 적을지만 담는다.
    /// </summary>
    [Serializable]
    public class PopupSpec
    {
        /// <summary>무엇에 대한 팝업인지 한 줄로 적는다.</summary>
        public string Title;

        /// <summary>본문. 길면 본문 칸 안에서만 스크롤한다.</summary>
        public string Body;

        /// <summary>버튼. 왼쪽부터의 차례로 담는다. 하나에서 셋까지다.</summary>
        public List<PopupButtonSpec> Buttons = new List<PopupButtonSpec>();

        public PopupSpec()
        {
        }

        public PopupSpec(string title, string body)
        {
            Title = title;
            Body = body;
        }

        /// <summary>버튼 수.</summary>
        public int ButtonCount
        {
            get { return Buttons != null ? Buttons.Count : 0; }
        }

        /// <summary>버튼을 하나 붙인다.</summary>
        public PopupSpec Add(PopupButtonSpec button)
        {
            Buttons.Add(button);
            return this;
        }

        /// <summary>확인 하나만 있는 알림 팝업을 만든다.</summary>
        public static PopupSpec Notice(string title, string body, string confirmLabel)
        {
            PopupSpec spec = new PopupSpec(title, body);
            spec.Add(PopupButtonSpec.Normal("confirm", confirmLabel));
            return spec;
        }

        /// <summary>취소와 실행 둘로 이뤄진 확인 팝업을 만든다.</summary>
        public static PopupSpec Confirm(
            string title,
            string body,
            string cancelLabel,
            string confirmLabel,
            bool confirmIsDangerous)
        {
            PopupSpec spec = new PopupSpec(title, body);
            spec.Add(PopupButtonSpec.Normal("cancel", cancelLabel));
            spec.Add(new PopupButtonSpec("confirm", confirmLabel, confirmIsDangerous));
            return spec;
        }
    }
}
