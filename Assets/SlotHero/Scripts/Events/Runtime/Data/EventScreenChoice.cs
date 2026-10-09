using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 선택지 종류. 이벤트 기획서 초안 1장 선택지 종류.
    /// </summary>
    public enum EventChoiceKind
    {
        /// <summary>진행. 스토리를 이어 다음 화면으로 간다.</summary>
        Advance = 0,

        /// <summary>실행. 작업 하나를 하고 다음 화면으로 간다.</summary>
        Act = 1,

        /// <summary>종료. 이벤트를 마친다.</summary>
        Finish = 2,
    }

    /// <summary>
    /// 화면에 들어갈 때 선택지를 무엇으로 채울지.
    /// 기획서의 "보유 문양 중 무작위 3종" 같은 줄이 이것이다.
    ///
    /// 4장 공통 규칙 이 "대상은 보유 중에서 무작위로 지목하고
    /// 여러 개를 제시할 때는 최대 3개까지 대상마다 선택지를 하나씩 둔다"로 정했다.
    /// </summary>
    public enum EventChoiceFill
    {
        /// <summary>적어 둔 그대로 쓴다.</summary>
        None = 0,

        /// <summary>아무 문양에서나 뽑는다.</summary>
        RandomSymbols = 1,

        /// <summary>가지고 있는 문양에서 뽑는다.</summary>
        OwnedSymbols = 2,

        /// <summary>아무 코인에서나 뽑는다.</summary>
        RandomCoins = 3,

        /// <summary>가지고 있는 코인에서 뽑는다.</summary>
        OwnedCoins = 4,

        /// <summary>아무 유물에서나 뽑는다. 이미 가진 것은 뺀다.</summary>
        RandomRelics = 5,

        /// <summary>
        /// 아홉 태그에서 뽑는다. 02 힘을 새기는 것 의 "무작위 태그 5개",
        /// 04 닮은 모습 화면 3 의 "무작위 태그 3개" 가 이것이다.
        /// </summary>
        RandomTags = 6,

        /// <summary>가지고 있는 문양에 붙은 태그에서 뽑는다. 05 정화 가 쓴다.</summary>
        OwnedTags = 7,

        /// <summary>
        /// 앞 화면에서 기억해 둔 문양의 태그 둘. 04 닮은 모습 화면 2 가 쓴다.
        /// 어느 문양인지는 <see cref="EventScreenChoice.FillSourceKey"/> 가 가리킨다.
        /// </summary>
        TagsOfRemembered = 8,

        /// <summary>
        /// 가장 많이 가진 태그를 가진 보유 문양에서 뽑는다. 10 정신 붕괴 가 쓴다.
        /// 태그는 문양 장수대로 센다. 문양 하나가 태그 둘을 가지므로 한 장이 두 태그에 하나씩 더해진다.
        /// 가장 많은 태그가 여럿이면 그 태그들을 모두 본다.
        /// 기획서 비고대로 같은 문양을 여러 장 가지면 같은 문양이 여러 선택지에 나올 수 있다.
        /// </summary>
        MostCommonTagSymbols = 9,

        /// <summary>
        /// 3장 도전 이벤트 공통 규칙 의 부정 효과 목록에서 서로 다른 것을 뽑는다.
        /// 35 도전자 의 "부정 효과 A와 B는 3장의 부정 효과 목록에서 무작위로 정한다" 가 쓴다.
        /// </summary>
        CombatPenalties = 10,
    }

    /// <summary>
    /// 고르기 화면에서 무엇을 고르게 할지.
    /// 선택지 하나를 누르면 현재 빌드 화면과 비슷한 고르기 화면이 열리고 거기서 고른 것이 대상이 된다.
    /// 2026년 10월 5일 원재가 정했다. "보유 중 무작위 3개" 처럼 칸이 정해진 것은 쓰지 않는다.
    /// </summary>
    public enum EventPickSource
    {
        /// <summary>고르기 화면을 열지 않는다.</summary>
        None = 0,

        /// <summary>게임의 모든 문양. 08 피의 거래 의 "전체 문양 목록에서 원하는 문양 1개" 다.</summary>
        AllSymbols = 1,

        /// <summary>가진 문양 전부.</summary>
        OwnedSymbols = 2,

        /// <summary>가진 코인 전부.</summary>
        OwnedCoins = 3,

        /// <summary>가진 유물 전부.</summary>
        OwnedRelics = 4,
    }

    /// <summary>
    /// 확률로 갈리는 길. 기획서의 "50% 확률로 화면 3A로 넘어간다"가 이것이다.
    /// 무게의 합을 분모로 본다. 50 과 50 이면 반반이다.
    /// </summary>
    [Serializable]
    public class EventBranch
    {
        public int Weight;
        public string ScreenId;

        public EventBranch()
        {
        }

        public EventBranch(int weight, string screenId)
        {
            Weight = weight;
            ScreenId = screenId;
        }
    }

    /// <summary>
    /// 화면의 선택지 하나.
    ///
    /// 4장 공통 규칙 의 "선택지 하나는 작업 하나만 실행한다"를 그대로 따라
    /// <see cref="Action"/> 을 하나만 둔다.
    /// </summary>
    [Serializable]
    public class EventScreenChoice
    {
        /// <summary>선택지 식별자. 한 화면 안에서 겹치지 않으면 된다.</summary>
        public string Id = string.Empty;

        /// <summary>선택지 문구. 대상이 붙는 선택지는 채울 때 이름이 끼워진다.</summary>
        public string Label = string.Empty;

        /// <summary>진행 · 실행 · 종료.</summary>
        public EventChoiceKind Kind = EventChoiceKind.Advance;

        /// <summary>갈 화면. <see cref="Branches"/> 가 있으면 그쪽이 먼저다.</summary>
        public string NextScreenId = string.Empty;

        /// <summary>확률로 갈리는 길. 비어 있으면 <see cref="NextScreenId"/> 로 간다.</summary>
        public List<EventBranch> Branches = new List<EventBranch>();

        /// <summary>이 선택지가 하는 작업. 진행과 종료는 비워 둔다.</summary>
        public EventAction Action = new EventAction();

        /// <summary>칸 안에 적을 획득과 소모 줄.</summary>
        public List<EventEffectLine> Lines = new List<EventEffectLine>();

        /// <summary>결과를 알 수 없는 선택지. 켜면 물음표만 나온다.</summary>
        public bool ResultHidden;

        /// <summary>화면에 들어갈 때 무엇으로 채울지.</summary>
        public EventChoiceFill Fill = EventChoiceFill.None;

        /// <summary>몇 개로 채울지. 4장 공통 규칙 대로 최대 3개다.</summary>
        public int FillCount = 3;

        /// <summary>
        /// 채울 때 `{0}` 자리에 대상 이름이 들어간다.
        /// 비워 두면 <see cref="Label"/> 을 그대로 쓴다.
        /// </summary>
        public string FillLabelFormat = "{0}";

        /// <summary>
        /// 채운 선택지마다 따로 쓰는 문구 꼴. 몇 번째 선택지인지에 맞춰 쓴다.
        /// 모자라면 <see cref="FillLabelFormat"/> 으로 돌아간다.
        /// 01 수상한 기운 이 세 통로를 각각 다른 문구로 적고 문양 이름을 붙이는 데 쓴다.
        /// </summary>
        public List<string> FillLabelFormats = new List<string>();

        /// <summary>
        /// 받거나 잃을 물건을 선택지를 내놓을 때 미리 정해 칸에 이름을 적는다.
        /// 2026년 10월 5일 원재가 의도적으로 숨기는 경우만 "무작위" 로 적고 나머지는 정확한 이름을 적으라고 했다.
        /// 숨기기로 한 것은 03 판도라의 상자, 05 정화, 25 미치광이 대장장이, 27 축복 이다.
        /// </summary>
        public bool RevealOutcome;

        /// <summary>
        /// 누르면 고르기 화면을 열어 고른 것을 대상으로 삼는다. 고르기 화면은 흐름이 연다.
        /// 고르지 않고 닫으면 선택지를 고르지 않은 것으로 친다.
        /// </summary>
        public EventPickSource PickSource = EventPickSource.None;

        /// <summary>
        /// 결과를 정하지 않고 같은 화면의 다른 선택지가 지목한 것을 모두 받는다. 그 선택지의 식별자다.
        /// 12 잠든 보물 의 "둘 다 가진다" 가 유물 A 와 B 를 그대로 받는 데 쓴다.
        /// </summary>
        public string TakeAllFromId = string.Empty;

        /// <summary>
        /// 고르면 지목된 대상을 이 이름으로 기억해 둔다. 비워 두면 기억하지 않는다.
        /// 02 힘을 새기는 것 이 화면 2 에서 고른 태그를 화면 3 에서 쓰는 것처럼
        /// 뒤 화면이 앞 화면의 선택을 알아야 할 때 쓴다.
        /// </summary>
        public string RememberAs = string.Empty;

        /// <summary><see cref="EventChoiceFill.TagsOfRemembered"/> 가 볼 기억 이름.</summary>
        public string FillSourceKey = string.Empty;

        /// <summary>
        /// 채울 때 뺄 것의 기억 이름들. 태그면 그 태그를, 문양이면 그 문양의 태그 둘을 뺀다.
        /// 04 닮은 모습 이 원래 문양으로 되돌아가는 선택지를 빼는 데 쓴다.
        /// </summary>
        public List<string> FillExcludeKeys = new List<string>();

        /// <summary>
        /// 채울 후보의 가장 낮은 등급. -1 이면 따지지 않는다.
        /// 등급은 0 일반, 1 고급, 2 희귀, 3 특급, 4 전설이다(`ItemRarity` 차례).
        /// 18 주인 없는 유물 이 "고급 이상" 으로 쓴다.
        /// </summary>
        public int FillMinRarity = -1;

        /// <summary>채울 후보의 가장 높은 등급. -1 이면 따지지 않는다. 27 축복 이 "일반 또는 고급" 으로 쓴다.</summary>
        public int FillMaxRarity = -1;

        /// <summary>
        /// 채울 때 등급마다 뽑힐 비중. 차례가 등급이다. 비어 있으면 후보를 고르게 뽑는다.
        /// 26 반짝이는 것 이 "일반 70%, 고급 30%" 로 쓴다. 비중이 없는 등급은 뽑히지 않는다.
        /// </summary>
        public List<int> FillRarityWeights = new List<int>();

        public EventScreenChoice()
        {
        }

        public EventScreenChoice(string id, string label, EventChoiceKind kind, string nextScreenId)
        {
            Id = id;
            Label = label;
            Kind = kind;
            NextScreenId = nextScreenId;
        }

        /// <summary>진행 선택지. 작업 없이 다음 화면으로만 간다.</summary>
        public static EventScreenChoice Advance(string id, string label, string nextScreenId)
        {
            return new EventScreenChoice(id, label, EventChoiceKind.Advance, nextScreenId);
        }

        /// <summary>종료 선택지. 4장 공통 규칙 대로 늘 비용 없이 고를 수 있다.</summary>
        public static EventScreenChoice Finish(string id, string label)
        {
            return new EventScreenChoice(id, label, EventChoiceKind.Finish, string.Empty);
        }

        /// <summary>실행 선택지. 작업 하나를 하고 다음 화면으로 간다.</summary>
        public static EventScreenChoice Act(
            string id, string label, EventAction action, string nextScreenId)
        {
            EventScreenChoice choice = new EventScreenChoice(
                id, label, EventChoiceKind.Act, nextScreenId);
            choice.Action = action ?? new EventAction();
            return choice;
        }

        /// <summary>칸 안에 적을 줄을 더한다.</summary>
        public EventScreenChoice With(EventEffectLine line)
        {
            Lines.Add(line);
            return this;
        }

        /// <summary>화면에 들어갈 때 대상으로 채우게 한다.</summary>
        public EventScreenChoice FilledWith(EventChoiceFill fill, int count, string labelFormat)
        {
            Fill = fill;
            FillCount = count;
            FillLabelFormat = labelFormat;
            return this;
        }

        /// <summary>채운 선택지마다 다른 문구를 쓰게 한다. `{0}` 자리에 대상 이름이 들어간다.</summary>
        public EventScreenChoice EachLabeled(params string[] formats)
        {
            if (formats != null)
            {
                FillLabelFormats.AddRange(formats);
            }

            return this;
        }

        /// <summary>몇 번째 채운 선택지의 문구 꼴.</summary>
        public string GetFillLabelFormat(int index)
        {
            if (FillLabelFormats != null && index >= 0 && index < FillLabelFormats.Count
                && !string.IsNullOrEmpty(FillLabelFormats[index]))
            {
                return FillLabelFormats[index];
            }

            return string.IsNullOrEmpty(FillLabelFormat) ? "{0}" : FillLabelFormat;
        }

        /// <summary>확률로 갈리게 한다.</summary>
        public EventScreenChoice Branch(int weight, string screenId)
        {
            Branches.Add(new EventBranch(weight, screenId));
            return this;
        }

        /// <summary>누르면 고르기 화면에서 대상을 고르게 한다.</summary>
        public EventScreenChoice PickingFrom(EventPickSource source)
        {
            PickSource = source;
            return this;
        }

        /// <summary>받거나 잃을 물건을 미리 정해 칸에 이름을 적게 한다.</summary>
        public EventScreenChoice Revealing()
        {
            RevealOutcome = true;
            return this;
        }

        /// <summary>같은 화면의 그 선택지가 지목한 것을 모두 받게 한다. 이름도 칸에 적는다.</summary>
        public EventScreenChoice TakingAllFrom(string choiceId)
        {
            RevealOutcome = true;
            TakeAllFromId = choiceId ?? string.Empty;
            return this;
        }

        /// <summary>결과를 알 수 없는 선택지로 둔다.</summary>
        public EventScreenChoice Hidden()
        {
            ResultHidden = true;
            return this;
        }

        /// <summary>고른 대상을 이 이름으로 기억하게 한다.</summary>
        public EventScreenChoice Remember(string key)
        {
            RememberAs = key;
            return this;
        }

        /// <summary>기억해 둔 문양의 태그로 채우게 한다.</summary>
        public EventScreenChoice FilledFrom(string key)
        {
            FillSourceKey = key;
            return this;
        }

        /// <summary>기억해 둔 것을 채울 후보에서 빼게 한다.</summary>
        public EventScreenChoice Excluding(string key)
        {
            FillExcludeKeys.Add(key);
            return this;
        }

        /// <summary>채울 후보를 그 등급 사이로 줄인다. 등급은 0 일반 ~ 4 전설이고 -1 은 따지지 않는다.</summary>
        public EventScreenChoice RarityBetween(int min, int max)
        {
            FillMinRarity = min;
            FillMaxRarity = max;
            return this;
        }

        /// <summary>채울 때 등급마다 뽑힐 비중을 정한다. 차례가 등급이다. 0 일반부터 적는다.</summary>
        public EventScreenChoice RarityWeights(params int[] weights)
        {
            FillRarityWeights.Clear();

            if (weights != null)
            {
                FillRarityWeights.AddRange(weights);
            }

            return this;
        }
    }
}
